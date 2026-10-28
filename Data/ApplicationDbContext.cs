using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using sms.api.Middleware;
using sms.api.Models;

namespace sms.api.Data
{
    public class ApplicationDbContext : DbContext
    {
        /// <summary>Set from X-Academic-Year-Id per request; used by global query filters. Null when header absent or in design-time (migrations).</summary>
        public Guid? CurrentAcademicYearId { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            if (httpContextAccessor?.HttpContext != null)
            {
                if (httpContextAccessor.HttpContext.Items.TryGetValue(AcademicYearHeaderMiddleware.HttpContextKey, out var obj) && obj is Guid id)
                    CurrentAcademicYearId = id;
                else if (httpContextAccessor.HttpContext.Request.Headers.TryGetValue(AcademicYearHeaderMiddleware.HeaderName, out var v) && Guid.TryParse(v.ToString().Trim(), out var parsed))
                    CurrentAcademicYearId = parsed;
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassEnrollment> ClassEnrollments { get; set; }
        public DbSet<ClassSubject> ClassSubjects { get; set; }
        public DbSet<StudentSubject> StudentSubjects { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<StudentLeave> StudentLeaves { get; set; }
        public DbSet<StaffLeave> StaffLeaves { get; set; }
        public DbSet<StaffLeaveApprover> StaffLeaveApprovers { get; set; }
        public DbSet<ClassAttendance> ClassAttendances { get; set; }
        public DbSet<ClassAttendanceRecord> ClassAttendanceRecords { get; set; }
        public DbSet<FeeStructure> FeeStructures { get; set; }
        public DbSet<FeeStructureLine> FeeStructureLines { get; set; }
        public DbSet<StudentFee> StudentFees { get; set; }
        public DbSet<FeePayment> FeePayments { get; set; }
        public DbSet<FeeDiscount> FeeDiscounts { get; set; }
        public DbSet<FeeReceipt> FeeReceipts { get; set; }
        public DbSet<SchoolInfo> SchoolInfos { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<OnlineClass> OnlineClasses { get; set; }
        public DbSet<OnlineClassParticipant> OnlineClassParticipants { get; set; }
        public DbSet<ClassNote> ClassNotes { get; set; }
        public DbSet<ClassMessage> ClassMessages { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<WeekOff> WeekOffs { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<TimetableEntry> TimetableEntries { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatRolePolicy> ChatRolePolicies { get; set; }
        public DbSet<StaffAttendance> StaffAttendances { get; set; }
        public DbSet<FcmToken> FcmTokens { get; set; }
        public DbSet<InAppNotification> InAppNotifications { get; set; }
        public DbSet<NotificationDeliveryLog> NotificationDeliveryLogs { get; set; }
        public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }

        public DbSet<VisitorEntry> VisitorEntries { get; set; }
        public DbSet<Enquiry> Enquiries { get; set; }
        public DbSet<WorkAllocation> WorkAllocations { get; set; }
        public DbSet<DocumentPrintJob> DocumentPrintJobs { get; set; }
        public DbSet<HomeworkAssignment> HomeworkAssignments { get; set; }
        public DbSet<OnlinePaymentTransaction> OnlinePaymentTransactions { get; set; }
        public DbSet<LectureTopic> LectureTopics { get; set; }
        public DbSet<ScheduledService> ScheduledServices { get; set; }
        public DbSet<GalleryMedia> GalleryMedia { get; set; }
        public DbSet<Invitee> Invitees { get; set; }

        public DbSet<DisciplineIncident> DisciplineIncidents { get; set; }
        public DbSet<StudentBehaviorIncident> StudentBehaviorIncidents { get; set; }
        public DbSet<AchievementTalent> AchievementTalents { get; set; }
        public DbSet<PtmFeedback> PtmFeedbacks { get; set; }
        public DbSet<Scholarship> Scholarships { get; set; }
        public DbSet<ScholarshipApplication> ScholarshipApplications { get; set; }

        public DbSet<TransferCertificate> TransferCertificates { get; set; }
        public DbSet<StudentIdCardRequest> StudentIdCardRequests { get; set; }

        public DbSet<BiometricDevice> BiometricDevices { get; set; }
        public DbSet<BiometricLog> BiometricLogs { get; set; }
        public DbSet<BiometricUserMap> BiometricUserMaps { get; set; }
        public DbSet<BiometricFingerTemplate> BiometricFingerTemplates { get; set; }
        public DbSet<BiometricFaceTemplate> BiometricFaceTemplates { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetMaintenanceRecord> AssetMaintenanceRecords { get; set; }

        public DbSet<Hostel> Hostels { get; set; }
        public DbSet<HostelRoom> HostelRooms { get; set; }
        public DbSet<HostelAllocation> HostelAllocations { get; set; }
        public DbSet<CoCurricularActivity> CoCurricularActivities { get; set; }
        public DbSet<CoCurricularParticipation> CoCurricularParticipations { get; set; }

        // Academic Year Management
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<StudentAcademicHistory> StudentAcademicHistories { get; set; }
        public DbSet<ClassPromotionHistory> ClassPromotionHistories { get; set; }
        public DbSet<AcademicPerformanceHistory> AcademicPerformanceHistories { get; set; }
        public DbSet<FeeHistoryByYear> FeeHistoriesByYear { get; set; }

        public DbSet<PayrollComponentDefinition> PayrollComponentDefinitions { get; set; }
        public DbSet<PayrollPeriod> PayrollPeriods { get; set; }
        public DbSet<PayrollSlip> PayrollSlips { get; set; }

        // Bus Management System
        public DbSet<Bus> Buses { get; set; }
        public DbSet<BusRoute> BusRoutes { get; set; }
        public DbSet<BusStop> BusStops { get; set; }


        public DbSet<BusMaintenanceRecord> BusMaintenanceRecords { get; set; }
        public DbSet<BusFuelRecord> BusFuelRecords { get; set; }
        public DbSet<BusExpense> BusExpenses { get; set; }

        // Library Management System
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<LibraryMember> LibraryMembers { get; set; }
        public DbSet<BookIssue> BookIssues { get; set; }
        public DbSet<BookReservation> BookReservations { get; set; }
        public DbSet<LibraryFine> LibraryFines { get; set; }
        public DbSet<BookReview> BookReviews { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var academicYearId = CurrentAcademicYearId;
            if (!academicYearId.HasValue || academicYearId.Value == Guid.Empty)
            {
                academicYearId = await ResolveCurrentAcademicYearIdAsync(cancellationToken);
            }

            if (academicYearId.HasValue && academicYearId.Value != Guid.Empty)
            {
                foreach (var entry in ChangeTracker.Entries())
                {
                    if (entry.State != EntityState.Added) continue;
                    var prop = entry.Metadata.FindProperty("AcademicYearId");
                    if (prop?.ClrType != typeof(Guid)) continue;
                    var current = entry.Property("AcademicYearId").CurrentValue;
                    if (current == null || (Guid)current == default)
                        entry.Property("AcademicYearId").CurrentValue = academicYearId.Value;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task<Guid?> ResolveCurrentAcademicYearIdAsync(CancellationToken cancellationToken)
        {
            var current = await AcademicYears.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IsCurrentYear, cancellationToken);
            if (current != null) return current.Id;

            var latest = await AcademicYears.AsNoTracking()
                .OrderByDescending(a => a.StartDate)
                .FirstOrDefaultAsync(cancellationToken);
            return latest?.Id;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure tables
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Staff>().ToTable("Staff");
            modelBuilder.Entity<Student>().ToTable("Students");

            // Configure User relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Staff)
                .WithOne(s => s.User)
                .HasForeignKey<Staff>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Admin)
                .WithOne(a => a.User)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationships
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Staff)
                .WithMany(s => s.Courses)
                .HasForeignKey(c => c.StaffId)
                .OnDelete(DeleteBehavior.SetNull);



            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // ClassEnrollment relationships - NO CASCADE to prevent conflicts
            modelBuilder.Entity<ClassEnrollment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(ce => ce.Student)
                    .WithMany(s => s.ClassEnrollments)
                    .HasForeignKey(ce => ce.StudentId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_ClassEnrollments_Students_StudentId");

                entity.HasOne(ce => ce.Class)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(ce => ce.ClassId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_ClassEnrollments_Classes_ClassId");

                entity.HasOne(ce => ce.AcademicYearNavigation)
                    .WithMany()
                    .HasForeignKey(ce => ce.AcademicYearId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_ClassEnrollments_AcademicYears_AcademicYearId");
            });

            modelBuilder.Entity<Subject>()
                .HasOne(s => s.Staff)
                .WithMany(st => st.Subjects)
                .HasForeignKey(s => s.StaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // ClassSubject relationships
            modelBuilder.Entity<ClassSubject>()
                .HasOne(cs => cs.Class)
                .WithMany(c => c.ClassSubjects)
                .HasForeignKey(cs => cs.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassSubject>()
                .HasOne(cs => cs.Subject)
                .WithMany()
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassSubject>()
                .HasOne(cs => cs.AcademicYear)
                .WithMany()
                .HasForeignKey(cs => cs.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            // Unique constraint for Class-Subject-AcademicYear combination
            modelBuilder.Entity<ClassSubject>()
                .HasIndex(cs => new { cs.ClassId, cs.SubjectId, cs.AcademicYearId })
                .IsUnique();

            // StudentSubject relationships
            modelBuilder.Entity<StudentSubject>()
                .HasOne(ss => ss.Student)
                .WithMany()
                .HasForeignKey(ss => ss.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentSubject>()
                .HasOne(ss => ss.Subject)
                .WithMany()
                .HasForeignKey(ss => ss.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentSubject>()
                .HasOne(ss => ss.AcademicYearNavigation)
                .WithMany()
                .HasForeignKey(ss => ss.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            // Unique constraint for Student-Subject-AcademicYear combination
            modelBuilder.Entity<StudentSubject>()
                .HasIndex(ss => new { ss.StudentId, ss.SubjectId, ss.AcademicYearId })
                .IsUnique();

            // Configure Leave relationships
            modelBuilder.Entity<StudentLeave>()
                .HasOne(sl => sl.Student)
                .WithMany()
                .HasForeignKey(sl => sl.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentLeave>()
                .HasOne(sl => sl.Class)
                .WithMany()
                .HasForeignKey(sl => sl.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StudentLeave>()
                .HasOne(sl => sl.ApprovedBy)
                .WithMany()
                .HasForeignKey(sl => sl.ApprovedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StaffLeave>()
                .HasOne(sl => sl.Staff)
                .WithMany()
                .HasForeignKey(sl => sl.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffLeave>()
                .HasOne(sl => sl.ApprovedBy)
                .WithMany()
                .HasForeignKey(sl => sl.ApprovedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StaffLeave>()
                .HasOne(sl => sl.AcademicYear)
                .WithMany()
                .HasForeignKey(sl => sl.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StaffLeaveApprover>()
                .HasOne(sla => sla.Staff)
                .WithMany()
                .HasForeignKey(sla => sla.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StaffLeaveApprover>()
                .HasOne(sla => sla.Approver)
                .WithMany()
                .HasForeignKey(sla => sla.ApproverId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure ClassAttendance relationships
            modelBuilder.Entity<ClassAttendance>()
                .HasOne(ca => ca.Class)
                .WithMany()
                .HasForeignKey(ca => ca.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassAttendance>()
                .HasOne(ca => ca.CreatedBy)
                .WithMany()
                .HasForeignKey(ca => ca.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClassAttendanceRecord>()
                .HasOne(ar => ar.ClassAttendance)
                .WithMany(ca => ca.AttendanceRecords)
                .HasForeignKey(ar => ar.ClassAttendanceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassAttendanceRecord>()
                .HasOne(ar => ar.Student)
                .WithMany()
                .HasForeignKey(ar => ar.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Fee relationships
            modelBuilder.Entity<FeeStructure>()
                .HasOne(fs => fs.Class)
                .WithMany()
                .HasForeignKey(fs => fs.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeeStructure>()
                .HasOne(fs => fs.AcademicYearNavigation)
                .WithMany()
                .HasForeignKey(fs => fs.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentFee>()
                .HasOne(sf => sf.Student)
                .WithMany()
                .HasForeignKey(sf => sf.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentFee>()
                .HasOne(sf => sf.FeeStructure)
                .WithMany(fs => fs.StudentFees)
                .HasForeignKey(sf => sf.FeeStructureId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeeStructureLine>()
                .HasOne(l => l.FeeStructure)
                .WithMany(fs => fs.Lines)
                .HasForeignKey(l => l.FeeStructureId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentFee>()
                .HasOne(sf => sf.FeeStructureLine)
                .WithMany(l => l.StudentFees)
                .HasForeignKey(sf => sf.FeeStructureLineId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeePayment>()
                .HasOne(fp => fp.StudentFee)
                .WithMany(sf => sf.Payments)
                .HasForeignKey(fp => fp.StudentFeeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeePayment>()
                .HasOne(fp => fp.ReceivedBy)
                .WithMany()
                .HasForeignKey(fp => fp.ReceivedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeeDiscount>()
                .HasOne(fd => fd.StudentFee)
                .WithMany(sf => sf.Discounts)
                .HasForeignKey(fd => fd.StudentFeeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeeDiscount>()
                .HasOne(fd => fd.ApprovedBy)
                .WithMany()
                .HasForeignKey(fd => fd.ApprovedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeeDiscount>()
                .HasOne(fd => fd.AcademicYear)
                .WithMany()
                .HasForeignKey(fd => fd.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeeReceipt>()
                .HasOne(fr => fr.Payment)
                .WithMany(fp => fp.Receipts)
                .HasForeignKey(fr => fr.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeeReceipt>()
                .HasOne(fr => fr.IssuedBy)
                .WithMany()
                .HasForeignKey(fr => fr.IssuedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeeReceipt>()
                .HasOne(fr => fr.AcademicYear)
                .WithMany()
                .HasForeignKey(fr => fr.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Announcement relationships
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Creator)
                .WithMany()
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.AcademicYear)
                .WithMany()
                .HasForeignKey(a => a.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Online Class relationships
            modelBuilder.Entity<OnlineClass>()
                .HasOne(oc => oc.Class)
                .WithMany()
                .HasForeignKey(oc => oc.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OnlineClass>()
                .HasOne(oc => oc.Teacher)
                .WithMany()
                .HasForeignKey(oc => oc.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OnlineClass>()
                .HasOne(oc => oc.AcademicYear)
                .WithMany()
                .HasForeignKey(oc => oc.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OnlineClassParticipant>()
                .HasOne(ocp => ocp.OnlineClass)
                .WithMany(oc => oc.Participants)
                .HasForeignKey(ocp => ocp.OnlineClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OnlineClassParticipant>()
                .HasOne(ocp => ocp.User)
                .WithMany()
                .HasForeignKey(ocp => ocp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OnlineClassParticipant>()
                .HasOne(ocp => ocp.AcademicYear)
                .WithMany()
                .HasForeignKey(ocp => ocp.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClassNote>()
                .HasOne(cn => cn.OnlineClass)
                .WithMany(oc => oc.Notes)
                .HasForeignKey(cn => cn.OnlineClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassNote>()
                .HasOne(cn => cn.CreatedBy)
                .WithMany()
                .HasForeignKey(cn => cn.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClassNote>()
                .HasOne(cn => cn.AcademicYear)
                .WithMany()
                .HasForeignKey(cn => cn.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClassMessage>()
                .HasOne(cm => cm.OnlineClass)
                .WithMany(oc => oc.Messages)
                .HasForeignKey(cm => cm.OnlineClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClassMessage>()
                .HasOne(cm => cm.Recipient)
                .WithMany()
                .HasForeignKey(cm => cm.RecipientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClassMessage>()
                .HasOne(cm => cm.AcademicYear)
                .WithMany()
                .HasForeignKey(cm => cm.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Calendar relationships
            modelBuilder.Entity<Holiday>()
                .HasOne(h => h.CreatedBy)
                .WithMany()
                .HasForeignKey(h => h.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Holiday>()
                .HasOne(h => h.AcademicYear)
                .WithMany()
                .HasForeignKey(h => h.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WeekOff>()
                .HasOne(w => w.CreatedBy)
                .WithMany()
                .HasForeignKey(w => w.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WeekOff>()
                .HasOne(w => w.AcademicYear)
                .WithMany()
                .HasForeignKey(w => w.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.AcademicYear)
                .WithMany()
                .HasForeignKey(e => e.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Timetable relationships
            modelBuilder.Entity<Period>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Period>()
                .HasOne(p => p.AcademicYear)
                .WithMany()
                .HasForeignKey(p => p.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(te => te.Period)
                .WithMany(p => p.TimetableEntries)
                .HasForeignKey(te => te.PeriodId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(te => te.Class)
                .WithMany()
                .HasForeignKey(te => te.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(te => te.Subject)
                .WithMany()
                .HasForeignKey(te => te.SubjectId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(te => te.Teacher)
                .WithMany()
                .HasForeignKey(te => te.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(te => te.SubstituteTeacher)
                .WithMany()
                .HasForeignKey(te => te.SubstituteTeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(te => te.CreatedBy)
                .WithMany()
                .HasForeignKey(te => te.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(te => te.AcademicYear)
                .WithMany()
                .HasForeignKey(te => te.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Exam relationships
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Class)
                .WithMany()
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure ExamResult relationships - NO ACTION to prevent cascade conflicts
            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.Exam)
                .WithMany(e => e.ExamResults)
                .HasForeignKey(er => er.ExamId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.Student)
                .WithMany()
                .HasForeignKey(er => er.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.EnteredBy)
                .WithMany()
                .HasForeignKey(er => er.EnteredByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.UpdatedBy)
                .WithMany()
                .HasForeignKey(er => er.UpdatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ExamResult>()
                .HasOne(er => er.AcademicYear)
                .WithMany()
                .HasForeignKey(er => er.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            // Add unique constraint for exam result (one result per student per exam)
            modelBuilder.Entity<ExamResult>()
                .HasIndex(er => new { er.ExamId, er.StudentId })
                .IsUnique();

            // Configure Department relationships
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Designation)
                .WithMany(d => d.Staff)
                .HasForeignKey(s => s.DesignationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Designation>()
                .HasOne(d => d.Department)
                .WithMany(dept => dept.Designations)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);



            modelBuilder.Entity<Course>()
                .HasOne(c => c.Department)
                .WithMany(d => d.Courses)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Document relationships
            modelBuilder.Entity<Document>()
                .HasOne(d => d.AcademicYear)
                .WithMany()
                .HasForeignKey(d => d.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.VerifiedByUser)
                .WithMany()
                .HasForeignKey(d => d.VerifiedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Add index for better performance
            modelBuilder.Entity<Document>()
                .HasIndex(d => new { d.UserId, d.DocumentType });

            // Configure Attendance relationships - CASCADE DELETE
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure StudentFee relationships - NO ACTION to prevent cascade conflicts
            modelBuilder.Entity<StudentFee>()
                .HasOne(sf => sf.Student)
                .WithMany()
                .HasForeignKey(sf => sf.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Leave relationships - CASCADE DELETE
            modelBuilder.Entity<StaffLeave>()
                .HasOne(sl => sl.Staff)
                .WithMany()
                .HasForeignKey(sl => sl.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentLeave>()
                .HasOne(sl => sl.Student)
                .WithMany()
                .HasForeignKey(sl => sl.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure ClassAttendance relationships
            modelBuilder.Entity<ClassAttendance>()
                .HasOne(ca => ca.CreatedBy)
                .WithMany()
                .HasForeignKey(ca => ca.CreatedById)
                .OnDelete(DeleteBehavior.NoAction); // Don't delete attendance if user is deleted

            // Configure ClassAttendanceRecord relationships - CASCADE DELETE
            modelBuilder.Entity<ClassAttendanceRecord>()
                .HasOne(car => car.Student)
                .WithMany()
                .HasForeignKey(car => car.StudentId)
                .OnDelete(DeleteBehavior.Cascade);





            // Configure FeePayment relationships - NO ACTION to prevent cascade conflicts
            modelBuilder.Entity<FeePayment>()
                .HasOne(fp => fp.StudentFee)
                .WithMany(sf => sf.Payments)
                .HasForeignKey(fp => fp.StudentFeeId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure FeeDiscount relationships - NO ACTION to prevent cascade conflicts
            modelBuilder.Entity<FeeDiscount>()
                .HasOne(fd => fd.StudentFee)
                .WithMany(sf => sf.Discounts)
                .HasForeignKey(fd => fd.StudentFeeId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Library relationships
            modelBuilder.Entity<LibraryMember>()
                .HasOne(lm => lm.User)
                .WithMany()
                .HasForeignKey(lm => lm.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<LibraryMember>()
                .HasOne(lm => lm.CreatedBy)
                .WithMany()
                .HasForeignKey(lm => lm.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<LibraryMember>()
                .HasOne(lm => lm.AcademicYear)
                .WithMany()
                .HasForeignKey(lm => lm.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookIssue>()
                .HasOne(bi => bi.LibraryMember)
                .WithMany(lm => lm.BookIssues)
                .HasForeignKey(bi => bi.LibraryMemberId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookIssue>()
                .HasOne(bi => bi.Book)
                .WithMany(b => b.BookIssues)
                .HasForeignKey(bi => bi.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookIssue>()
                .HasOne(bi => bi.IssuedBy)
                .WithMany()
                .HasForeignKey(bi => bi.IssuedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookIssue>()
                .HasOne(bi => bi.AcademicYear)
                .WithMany()
                .HasForeignKey(bi => bi.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookReservation>()
                .HasOne(br => br.LibraryMember)
                .WithMany(lm => lm.BookReservations)
                .HasForeignKey(br => br.LibraryMemberId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookReservation>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BookReservations)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookReservation>()
                .HasOne(br => br.AcademicYear)
                .WithMany()
                .HasForeignKey(br => br.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LibraryFine>()
                .HasOne(lf => lf.LibraryMember)
                .WithMany(lm => lm.LibraryFines)
                .HasForeignKey(lf => lf.LibraryMemberId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LibraryFine>()
                .HasOne(lf => lf.BookIssue)
                .WithMany(bi => bi.LibraryFines)
                .HasForeignKey(lf => lf.BookIssueId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LibraryFine>()
                .HasOne(lf => lf.AcademicYear)
                .WithMany()
                .HasForeignKey(lf => lf.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookReview>()
                .HasOne(br => br.LibraryMember)
                .WithMany()
                .HasForeignKey(br => br.LibraryMemberId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookReview>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BookReviews)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookReview>()
                .HasOne(br => br.AcademicYear)
                .WithMany()
                .HasForeignKey(br => br.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            // ChatMessage relationships - avoid multiple cascade paths
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.FromUser)
                .WithMany()
                .HasForeignKey(m => m.FromUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.ToUser)
                .WithMany()
                .HasForeignKey(m => m.ToUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Book relationships to prevent cascade conflicts
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Publisher)
                .WithMany(p => p.Books)
                .HasForeignKey(b => b.PublisherId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<Book>()
                .HasOne(b => b.CreatedBy)
                .WithMany()
                .HasForeignKey(b => b.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            // Configure Attendance relationships to prevent cascade conflicts
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.AcademicYear)
                .WithMany()
                .HasForeignKey(a => a.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            // Configure ClassAttendanceRecord relationships to prevent cascade conflicts
            modelBuilder.Entity<ClassAttendanceRecord>()
                .HasOne(car => car.ClassAttendance)
                .WithMany(ca => ca.AttendanceRecords)
                .HasForeignKey(car => car.ClassAttendanceId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<ClassAttendanceRecord>()
                .HasOne(car => car.Student)
                .WithMany()
                .HasForeignKey(car => car.StudentId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<ClassAttendanceRecord>()
                .HasOne(car => car.AcademicYear)
                .WithMany()
                .HasForeignKey(car => car.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            // Configure ClassPromotionHistory relationships to prevent cascade conflicts
            modelBuilder.Entity<ClassPromotionHistory>()
                .HasOne(cph => cph.Student)
                .WithMany()
                .HasForeignKey(cph => cph.StudentId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<ClassPromotionHistory>()
                .HasOne(cph => cph.AcademicYear)
                .WithMany(ay => ay.ClassPromotionHistories)
                .HasForeignKey(cph => cph.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<ClassPromotionHistory>()
                .HasOne(cph => cph.FromClass)
                .WithMany()
                .HasForeignKey(cph => cph.FromClassId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<ClassPromotionHistory>()
                .HasOne(cph => cph.ToClass)
                .WithMany()
                .HasForeignKey(cph => cph.ToClassId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<ClassPromotionHistory>()
                .HasOne(cph => cph.DecisionMadeBy)
                .WithMany()
                .HasForeignKey(cph => cph.DecisionMadeByUserId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            // Configure StudentAcademicHistory relationships to prevent cascade conflicts
            modelBuilder.Entity<StudentAcademicHistory>()
                .HasOne(sah => sah.Student)
                .WithMany()
                .HasForeignKey(sah => sah.StudentId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<StudentAcademicHistory>()
                .HasOne(sah => sah.AcademicYear)
                .WithMany(ay => ay.StudentAcademicHistories)
                .HasForeignKey(sah => sah.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<StudentAcademicHistory>()
                .HasOne(sah => sah.Class)
                .WithMany()
                .HasForeignKey(sah => sah.ClassId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            // Configure AcademicPerformanceHistory relationships to prevent cascade conflicts
            modelBuilder.Entity<AcademicPerformanceHistory>()
                .HasOne(aph => aph.Student)
                .WithMany()
                .HasForeignKey(aph => aph.StudentId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<AcademicPerformanceHistory>()
                .HasOne(aph => aph.AcademicYear)
                .WithMany(ay => ay.AcademicPerformanceHistories)
                .HasForeignKey(aph => aph.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<AcademicPerformanceHistory>()
                .HasOne(aph => aph.Class)
                .WithMany()
                .HasForeignKey(aph => aph.ClassId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<AcademicPerformanceHistory>()
                .HasOne(aph => aph.Subject)
                .WithMany()
                .HasForeignKey(aph => aph.SubjectId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            // Configure FeeHistoryByYear relationships to prevent cascade conflicts
            modelBuilder.Entity<FeeHistoryByYear>()
                .HasOne(fhy => fhy.Student)
                .WithMany()
                .HasForeignKey(fhy => fhy.StudentId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<FeeHistoryByYear>()
                .HasOne(fhy => fhy.AcademicYear)
                .WithMany(ay => ay.FeeHistoriesByYear)
                .HasForeignKey(fhy => fhy.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            modelBuilder.Entity<FeeHistoryByYear>()
                .HasOne(fhy => fhy.Class)
                .WithMany()
                .HasForeignKey(fhy => fhy.ClassId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade conflicts

            // Configure Bus Management relationships
            modelBuilder.Entity<Bus>()
                .HasIndex(b => b.BusNumber)
                .IsUnique();

            modelBuilder.Entity<Bus>()
                .HasIndex(b => b.RegistrationNumber)
                .IsUnique();

            modelBuilder.Entity<Bus>()
                .HasOne(b => b.CreatedBy)
                .WithMany()
                .HasForeignKey(b => b.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BusRoute>()
                .HasIndex(br => br.RouteCode)
                .IsUnique();

            modelBuilder.Entity<BusRoute>()
                .HasOne(br => br.Bus)
                .WithMany(b => b.BusRoutes)
                .HasForeignKey(br => br.BusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BusRoute>()
                .HasOne(br => br.CreatedBy)
                .WithMany()
                .HasForeignKey(br => br.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BusStop>()
                .HasIndex(bs => bs.StopCode)
                .IsUnique();

            modelBuilder.Entity<BusStop>()
                .HasOne(bs => bs.Route)
                .WithMany(br => br.BusStops)
                .HasForeignKey(bs => bs.RouteId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BusStop>()
                .HasOne(bs => bs.CreatedBy)
                .WithMany()
                .HasForeignKey(bs => bs.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);



            modelBuilder.Entity<BusMaintenanceRecord>()
                .HasOne(bmr => bmr.Bus)
                .WithMany(b => b.MaintenanceRecords)
                .HasForeignKey(bmr => bmr.BusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BusMaintenanceRecord>()
                .HasOne(bmr => bmr.CreatedBy)
                .WithMany()
                .HasForeignKey(bmr => bmr.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BusFuelRecord>()
                .HasOne(bfr => bfr.Bus)
                .WithMany()
                .HasForeignKey(bfr => bfr.BusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BusFuelRecord>()
                .HasOne(bfr => bfr.CreatedBy)
                .WithMany()
                .HasForeignKey(bfr => bfr.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BusExpense>()
                .HasOne(be => be.Bus)
                .WithMany()
                .HasForeignKey(be => be.BusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BusExpense>()
                .HasOne(be => be.CreatedBy)
                .WithMany()
                .HasForeignKey(be => be.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Student-BusStop relationship
            modelBuilder.Entity<Student>()
                .HasOne(s => s.BusStop)
                .WithMany()
                .HasForeignKey(s => s.BusStopId)
                .OnDelete(DeleteBehavior.SetNull);

            // Role configurations
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Description).HasMaxLength(500);
            });

            // Permission configurations
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasIndex(p => p.Code).IsUnique();
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Code).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Description).HasMaxLength(500);
                entity.Property(p => p.Module).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Action).IsRequired().HasMaxLength(50);

                entity.HasOne(p => p.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(p => p.ParentPermissionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RolePermission configurations
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserRole configurations
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

                entity.HasOne(ur => ur.User)
                    .WithMany()
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== ACADEMIC YEAR ARCHITECTURE ==========
            // MASTER / STATIC (no AcademicYearId, no filter): Users, Admins, Roles, Permissions, RolePermissions, UserRoles,
            //   SchoolInfos, Departments, Designations, Staff, Students, AcademicYears, Classes, Subjects, Courses, Periods,
            //   Buses, BusRoutes, BusStops, StaffLeaveApprovers, BookCategories, Authors, Publishers, Books, BookAuthors.
            // YEAR-DEPENDENT (have AcademicYearId + global filter below): All other tables listed in HasQueryFilter.
            // When X-Academic-Year-Id header is sent, only data for that year is returned for year-dependent tables.
            // Reports and lists are scoped by the selected academic year. New inserts get AcademicYearId set in SaveChangesAsync when header is present.
            // ========== Academic year global query filters (year-dependent tables only)
            modelBuilder.Entity<ClassEnrollment>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<ClassSubject>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<StudentSubject>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<TimetableEntry>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<ClassAttendance>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<ClassAttendanceRecord>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<Attendance>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<StudentAcademicHistory>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<ClassPromotionHistory>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<AcademicPerformanceHistory>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<Exam>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<ExamResult>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<StudentLeave>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<StaffLeave>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<FeeStructure>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<StudentFee>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<FeePayment>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<FeeDiscount>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<FeeReceipt>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<FeeHistoryByYear>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<Holiday>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<WeekOff>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<Event>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<OnlineClass>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<OnlineClassParticipant>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<ClassNote>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<ClassMessage>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<Document>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<Announcement>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<LibraryMember>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<BookIssue>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<BookReservation>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<LibraryFine>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<BookReview>().HasQueryFilter(e => CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<PayrollComponentDefinition>().ToTable("PayrollComponentDefinitions");
            modelBuilder.Entity<PayrollComponentDefinition>().HasIndex(c => c.Code).IsUnique();

            modelBuilder.Entity<PayrollPeriod>().ToTable("PayrollPeriods");
            modelBuilder.Entity<PayrollPeriod>().HasIndex(p => new { p.AcademicYearId, p.Year, p.Month }).IsUnique();
            modelBuilder.Entity<PayrollPeriod>()
                .HasOne(p => p.AcademicYear)
                .WithMany()
                .HasForeignKey(p => p.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PayrollSlip>().ToTable("PayrollSlips");
            modelBuilder.Entity<PayrollSlip>().HasIndex(s => new { s.PayrollPeriodId, s.StaffId }).IsUnique();
            modelBuilder.Entity<PayrollSlip>()
                .HasOne(s => s.PayrollPeriod)
                .WithMany(p => p.Slips)
                .HasForeignKey(s => s.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PayrollSlip>()
                .HasOne(s => s.Staff)
                .WithMany()
                .HasForeignKey(s => s.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PayrollPeriod>().HasQueryFilter(p => CurrentAcademicYearId == null || p.AcademicYearId == CurrentAcademicYearId);
            modelBuilder.Entity<PayrollSlip>().HasQueryFilter(s => CurrentAcademicYearId == null || s.AcademicYearId == CurrentAcademicYearId);

            modelBuilder.Entity<ChatMessage>().HasIndex(m => new { m.FromUserId, m.ToUserId, m.SentAt });
            modelBuilder.Entity<ChatRolePolicy>().HasIndex(p => new { p.Role, p.AllowedRole }).IsUnique();
            modelBuilder.Entity<StaffAttendance>().HasIndex(a => new { a.StaffId, a.Date }).IsUnique();
            modelBuilder.Entity<StaffAttendance>()
                .HasOne(a => a.Staff)
                .WithMany()
                .HasForeignKey(a => a.StaffId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<StaffAttendance>()
                .HasOne(a => a.AcademicYear)
                .WithMany()
                .HasForeignKey(a => a.AcademicYearId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
