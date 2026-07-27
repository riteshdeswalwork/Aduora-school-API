using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using sms.api.Data;
using sms.api.Hubs;
using sms.api.Interfaces.Admin;
using sms.api.Interfaces.Auth;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Attendance;
using sms.api.Interfaces.Bus;
using sms.api.Interfaces.Calendar;
using sms.api.Interfaces.Document;
using sms.api.Interfaces.Fee;
using sms.api.Interfaces.Library;
using sms.api.Interfaces.Shared;
using sms.api.Interfaces.Security;
using sms.api.Interfaces.Staff;
using sms.api.Interfaces.Student;
using sms.api.Managers.Security;
using sms.api.Managers.Admin;
using sms.api.Managers.Auth;
using sms.api.Managers.Academic;
using sms.api.Managers.Attendance;
using sms.api.Managers.Bus;
using sms.api.Managers.Calendar;
using sms.api.Managers.Document;
using sms.api.Managers.Fee;
using sms.api.Managers.Library;
using sms.api.Managers.Shared;
using sms.api.Managers.Staff;
using sms.api.Managers.Student;
using sms.api.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.SignalR;
using GoogleReCaptcha.V3.Interface;
using GoogleReCaptcha.V3;
using sms.api.Models;
using sms.api.Interfaces.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        // Configure JWT for SignalR - tokens come from query parameter
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our SignalR hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (
                    path.StartsWithSegments("/onlineclasshub")
                    || path.StartsWithSegments("/chathub")
                    || path.StartsWithSegments("/notificationhub")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.Configure<LiveKitOptions>(builder.Configuration.GetSection("LiveKit"));

// Configure SignalR to use JWT for user identification
// NOTE: This allows SignalR to get user ID from JWT token claims
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// Register Managers
builder.Services.AddScoped<IStaffDataScopeService, StaffDataScopeService>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IAdminManager, AdminManager>();
builder.Services.AddScoped<IStaffManager, StaffManager>();
builder.Services.AddScoped<IPayrollManager, PayrollManager>();
builder.Services.AddScoped<IStudentManager, StudentManager>();
// Course system removed - using ClassEnrollment instead
// builder.Services.AddScoped<ICourseManager, CourseManager>();
// builder.Services.AddScoped<IEnrollmentManager, EnrollmentManager>();
builder.Services.AddScoped<IAttendanceManager, AttendanceManager>();
builder.Services.AddScoped<IClassManager, ClassManager>();
builder.Services.AddScoped<IClassEnrollmentManager, ClassEnrollmentManager>();
builder.Services.AddScoped<ISubjectManager, SubjectManager>();
builder.Services.AddScoped<IDepartmentManager, DepartmentManager>();
builder.Services.AddScoped<IProfilePhotoManager, ProfilePhotoManager>();
builder.Services.AddScoped<IAnalyticsManager, AnalyticsManager>();
builder.Services.AddScoped<IDesignationManager, DesignationManager>();
builder.Services.AddScoped<IStudentLeaveManager, StudentLeaveManager>();
builder.Services.AddScoped<IStaffLeaveManager, StaffLeaveManager>();
builder.Services.AddScoped<IClassAttendanceManager, ClassAttendanceManager>();
builder.Services.AddScoped<IFeeStructureManager, FeeStructureManager>();
builder.Services.AddScoped<IStudentFeeManager, StudentFeeManager>();
builder.Services.AddScoped<IFeePaymentManager, FeePaymentManager>();
builder.Services.AddScoped<IFeeDiscountManager, FeeDiscountManager>();
builder.Services.AddScoped<IFeeReceiptManager, FeeReceiptManager>();
builder.Services.AddScoped<ISchoolInfoManager, SchoolInfoManager>();
builder.Services.AddScoped<IRoleManager, RoleManager>();
builder.Services.AddScoped<IPermissionManager, PermissionManager>();
builder.Services.AddScoped<IDataInitializationManager, DataInitializationManager>();
builder.Services.AddScoped<IAnnouncementManager, AnnouncementManager>();
builder.Services.AddScoped<IOnlineClassManager, OnlineClassManager>();
builder.Services.AddScoped<ILiveKitTokenService, LiveKitTokenService>();
builder.Services.AddScoped<IHolidayManager, HolidayManager>();
builder.Services.AddScoped<IWeekOffManager, WeekOffManager>();
builder.Services.AddScoped<IEventManager, EventManager>();
builder.Services.AddScoped<IPeriodManager, PeriodManager>();
builder.Services.AddScoped<ITimetableManager, TimetableManager>();
builder.Services.AddScoped<ICalendarManager, CalendarManager>();
builder.Services.AddScoped<IExamManager, ExamManager>();
builder.Services.AddScoped<IExamResultManager, ExamResultManager>();
builder.Services.AddScoped<IDocumentManager, DocumentManager>();
builder.Services.AddScoped<IVisitorEntryManager, VisitorEntryManager>();
builder.Services.AddScoped<IEnquiryManager, EnquiryManager>();
builder.Services.AddScoped<IWorkAllocationManager, WorkAllocationManager>();
builder.Services.AddScoped<IDocumentPrintJobManager, DocumentPrintJobManager>();
builder.Services.AddScoped<IHomeworkAssignmentManager, HomeworkAssignmentManager>();
builder.Services.AddScoped<ILectureTopicManager, LectureTopicManager>();
builder.Services.AddScoped<IScheduledServiceManager, ScheduledServiceManager>();
builder.Services.AddScoped<IGalleryMediaManager, GalleryMediaManager>();
builder.Services.AddScoped<IInviteeManager, InviteeManager>();
builder.Services.AddScoped<IInventoryItemManager, InventoryItemManager>();
builder.Services.AddScoped<IInventoryTransactionManager, InventoryTransactionManager>();
builder.Services.AddScoped<IAssetManager, AssetManager>();
builder.Services.AddScoped<IAssetMaintenanceManager, AssetMaintenanceManager>();
builder.Services.AddScoped<IBiometricDeviceManager, BiometricDeviceManager>();
builder.Services.AddScoped<IBiometricLogManager, BiometricLogManager>();
builder.Services.AddScoped<IBiometricAttendanceService, BiometricAttendanceService>();
builder.Services.AddScoped<IOnlinePaymentTransactionManager, OnlinePaymentTransactionManager>();
builder.Services.AddScoped<IDisciplineIncidentManager, DisciplineIncidentManager>();
builder.Services.AddScoped<IStudentBehaviorIncidentManager, StudentBehaviorIncidentManager>();
builder.Services.AddScoped<IAchievementTalentManager, AchievementTalentManager>();
builder.Services.AddScoped<IPtmFeedbackManager, PtmFeedbackManager>();
builder.Services.AddScoped<ITransferCertificateManager, TransferCertificateManager>();
builder.Services.AddScoped<IStudentIdCardRequestManager, StudentIdCardRequestManager>();
builder.Services.AddScoped<IHostelManager, HostelManager>();
builder.Services.AddScoped<IHostelRoomManager, HostelRoomManager>();
builder.Services.AddScoped<IHostelAllocationManager, HostelAllocationManager>();
builder.Services.AddScoped<ICoCurricularActivityManager, CoCurricularActivityManager>();
builder.Services.AddScoped<ICoCurricularParticipationManager, CoCurricularParticipationManager>();
builder.Services.AddScoped<IScholarshipManager, ScholarshipManager>();
builder.Services.AddScoped<IScholarshipApplicationManager, ScholarshipApplicationManager>();

// Academic Year Management
builder.Services.AddScoped<IAcademicYearManager, AcademicYearManager>();
builder.Services.AddScoped<IStudentAcademicHistoryManager, StudentAcademicHistoryManager>();

// Configure Google reCAPTCHA
builder.Services.AddHttpClient<ICaptchaValidator, GoogleReCaptchaValidator>();
builder.Services.Configure<GoogleReCaptchaConfig>(options =>
{
    options.SiteKey = builder.Configuration["googleReCaptcha:SiteKey"];
    options.SecretKey = builder.Configuration["googleReCaptcha:SecretKey"];
});
builder.Services.AddScoped<IRecaptchaService, RecaptchaService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));
builder.Services.Configure<NotificationSettings>(builder.Configuration.GetSection("Notifications"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<ITwilioSmsService, TwilioSmsService>();
builder.Services.AddSingleton<INotificationOrchestrator, NotificationOrchestrator>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddHostedService<MonthlyFeeDemandBackgroundService>();
builder.Services.AddSingleton<IHostResourceMetricsService, HostResourceMetricsService>();
builder.Services.AddHostedService<SystemHealthAlertBackgroundService>();
builder.Services.AddSingleton<ITrafficMetricsService, TrafficMetricsService>();

// Bus Management System
builder.Services.AddScoped<IBusManager, BusManager>();
builder.Services.AddScoped<IBusRouteManager, BusRouteManager>();
builder.Services.AddScoped<IBusStopManager, BusStopManager>();


// Library Management System
builder.Services.AddScoped<ILibraryManager, LibraryManager>();

// Firebase Notification Service
builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();

// Add SignalR with JWT authentication support
// NOTE: SignalR configured to use JWT tokens for user identification
builder.Services.AddSignalR();

// Configure CORS - Frontend URL: https://localhost:7017, API URL: https://localhost:7238
// NOTE: Update these URLs when deploying to production or changing ports
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://localhost:7017",  // Frontend Blazor app URL
                "http://localhost:7017",
                "https://localhost:7238",
                "http://localhost:7238",
                "http://localhost:5173",   // Vite dev server (React frontend)
                "http://localhost:5174",
                "http://localhost:3000",
                "http://localhost:5000",  // Frontend when served from same host
                "https://school.aduora.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR with authentication
    });
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "School Management System API", Version = "v1" });

    // Configure Swagger to use JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "School Management System API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the root
    });
}

app.UseHttpsRedirection();

// Use the default CORS policy
app.UseCors();

app.UseMiddleware<sms.api.Middleware.AcademicYearHeaderMiddleware>();
app.UseMiddleware<sms.api.Middleware.RequestTrafficMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<OnlineClassHub>("/onlineclasshub");
app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");

// Ensure database is created and updated
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Apply migrations if any
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        Console.WriteLine("Database has been created/updated successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating/updating the database.");
        Console.WriteLine($"Error: {ex.Message}");
    }
}

app.Run();
