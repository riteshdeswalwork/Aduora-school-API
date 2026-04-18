using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sms.api.Migrations
{
    /// <inheritdoc />
    public partial class frist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AchievementTalents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Level = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AchievementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AwardedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    EventName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CertificateIssued = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTalents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetMaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Vendor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    VendorContact = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ServiceCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PostServiceHealthGrade = table.Column<int>(type: "int", nullable: false),
                    MaintenanceChecklist = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMaintenanceRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TagId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurchaseCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DepreciatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierContact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LifespanEstimate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BiometricDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Port = table.Column<int>(type: "int", nullable: false),
                    MacAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirmwareVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SyncIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiometricDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BiometricLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoggedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiometricLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatRolePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AllowedRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRolePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoCurricularActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Organizer = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Venue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TotalParticipants = table.Column<int>(type: "int", nullable: true),
                    BudgetAllocated = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoCurricularActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoCurricularParticipations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoCurricularActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PointsAwarded = table.Column<int>(type: "int", nullable: true),
                    AchievementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CertificateIssued = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoCurricularParticipations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HeadOfDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DisciplineIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncidentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IncidentLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    WitnessJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ActionTaken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ActionPlan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Punishment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisciplineIncidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentPrintJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TargetReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JobPriority = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaperType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ColorMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsDuplex = table.Column<bool>(type: "bit", nullable: false),
                    PrintQuality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrinterNodeIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchSize = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrintedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedCompletion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentPrintJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enquiries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ParentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CurrentSchool = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SeekingClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CourseInterested = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnquiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FollowUpLog = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enquiries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GalleryMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AlbumCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MediaType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VisibilityTier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhotographerMetadata = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MediaDensity = table.Column<int>(type: "int", nullable: false),
                    TotalAssetSize = table.Column<long>(type: "bigint", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryMedia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AssignedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxMarks = table.Column<int>(type: "int", nullable: true),
                    InstructionsForParents = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HostelAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostelRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SecurityDeposit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostelAllocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HostelRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    IsAc = table.Column<bool>(type: "bit", nullable: false),
                    MonthlyRent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FurnitureJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RoomType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostelRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hostels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BuildingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WardenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecurityLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AmenityTags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HostelType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hostels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Sku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CriticalThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReorderLevel = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BinLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentSupplier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepletionRate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invitees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RsvpStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LectureTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LectureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ChapterOrUnit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    NotesUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OnlinePaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConvenienceFees = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxBreakdown = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentFor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gateway = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionReference = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MerchantRefId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    RefundReference = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SettlementStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LatencyMs = table.Column<int>(type: "int", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LedgerCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlinePaymentTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollComponentDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    CalculationMode = table.Column<int>(type: "int", nullable: false),
                    AmountOrPercent = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MaxAmountCap = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinGrossEligible = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxGrossEligible = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollComponentDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PtmFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeedbackDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ParentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MeetingTopic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SpecificStrengths = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AreasForImprovement = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PtmFeedbacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssignedToStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScholarshipApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScholarshipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AppliedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisbursedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DisbursedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScholarshipApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scholarships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GrantType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisbursementMode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RenewalFrequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinimumGpaRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EligibilityCriteria = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DocHashes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scholarships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Principal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VicePrincipal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstablishedYear = table.Column<int>(type: "int", nullable: false),
                    AffiliationBoard = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Mission = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Vision = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentIdCardRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardTech = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChipUid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MedicalData = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrintedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IssueReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BloodGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentIdCardRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePhotoData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ProfilePhotoContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProfilePhotoFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProfilePicture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitorEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    HostStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryGate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumberOfPersons = table.Column<int>(type: "int", nullable: false),
                    IdType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PersonToMeet = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    VehicleNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BadgeNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TimeIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CheckInById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CheckOutById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitorEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedToStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewerStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionPercentage = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Complexity = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReviewerComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkAllocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Designations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Designations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AcademicYears",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    YearName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCurrentYear = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumberOfTerms = table.Column<int>(type: "int", nullable: false),
                    TermNames = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdmissionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdmissionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultDeclarationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinancialYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicYears_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Biography = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfDeath = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TwitterHandle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FacebookProfile = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LinkedInProfile = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Education = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Awards = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NotableWorks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Authors_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxIssueDays = table.Column<int>(type: "int", nullable: false),
                    MaxRenewalCount = table.Column<int>(type: "int", nullable: false),
                    IsReferenceOnly = table.Column<bool>(type: "bit", nullable: false),
                    FinePerDay = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookCategories_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Buses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    DriverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DriverPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ConductorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConductorPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    FuelType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FuelCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Mileage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CurrentLatitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentLongitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastLocationUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaintenanceCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InsuranceExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InsurancePremium = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FitnessExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buses_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Buses_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FcmTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FcmTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FcmTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemPermission = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentPermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Permissions_ParentPermissionId",
                        column: x => x.ParentPermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Permissions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Permissions_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publishers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Roles_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfJoining = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staff_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Staff_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Staff_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PublishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Announcements_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VerificationNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Users_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSchoolWide = table.Column<bool>(type: "bit", nullable: false),
                    ClassIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequiresAttendance = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Events_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HolidayType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSchoolWide = table.Column<bool>(type: "bit", nullable: false),
                    ClassIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Holidays_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Holidays_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LibraryMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MemberType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MembershipStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MembershipEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    SuspensionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SuspensionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuspensionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxBooksAllowed = table.Column<int>(type: "int", nullable: false),
                    MaxIssueDays = table.Column<int>(type: "int", nullable: false),
                    MaxRenewalCount = table.Column<int>(type: "int", nullable: false),
                    CurrentBooksIssued = table.Column<int>(type: "int", nullable: false),
                    TotalBooksIssued = table.Column<int>(type: "int", nullable: false),
                    TotalFineAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaidFineAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PendingFineAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyContact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PreferredCategories = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreferredAuthors = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    SMSNotifications = table.Column<bool>(type: "bit", nullable: false),
                    SecurityDeposit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DepositAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DepositDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepositRefundStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdProofUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdProofType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdProofNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryMembers_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryMembers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    WorkWeekMode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AssumeFullAttendanceIfNoRecords = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPeriods_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PayrollPeriods_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsSchoolWide = table.Column<bool>(type: "bit", nullable: false),
                    ClassIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Periods_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Periods_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WeekOffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsSchoolWide = table.Column<bool>(type: "bit", nullable: false),
                    ClassIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekOffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeekOffs_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WeekOffs_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BusExpenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpenseType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VendorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusExpenses_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusExpenses_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BusFuelRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RatePerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FuelStation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusFuelRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusFuelRecords_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusFuelRecords_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BusMaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaintenanceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ServiceProviderPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Odometer = table.Column<int>(type: "int", nullable: true),
                    NextServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PartsReplaced = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusMaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusMaintenanceRecords_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusMaintenanceRecords_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusMaintenanceRecords_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BusRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RouteCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedDuration = table.Column<TimeSpan>(type: "time", nullable: true),
                    MorningStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    MorningEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EveningStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EveningEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    RouteWaypoints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusRoutes_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusRoutes_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusRoutes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ISBN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ISBN13 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublisherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Edition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PublicationYear = table.Column<int>(type: "int", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Pages = table.Column<int>(type: "int", nullable: true),
                    Format = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Dimensions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TableOfContents = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AccessionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CallNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CurrentValue = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcquisitionMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Vendor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConditionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LastConditionCheck = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsLost = table.Column<bool>(type: "bit", nullable: false),
                    IsDamaged = table.Column<bool>(type: "bit", nullable: false),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false),
                    StatusNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EBookUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasEBook = table.Column<bool>(type: "bit", nullable: false),
                    HasAudioBook = table.Column<bool>(type: "bit", nullable: false),
                    AudioBookUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AverageRating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    TotalIssues = table.Column<int>(type: "int", nullable: false),
                    LastIssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReturnedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentReservations = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublisherId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_BookCategories_BookCategoryId",
                        column: x => x.BookCategoryId,
                        principalTable: "BookCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Books_BookCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "BookCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Books_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Books_Publishers_PublisherId1",
                        column: x => x.PublisherId1,
                        principalTable: "Publishers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Books_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolePermissions_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_RevokedByUserId",
                        column: x => x.RevokedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CurrentEnrollment = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MonthlyFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    YearlyFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FeeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MinimumSubjects = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Staff_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Credits = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Courses_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StaffAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffAttendances_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffAttendances_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffLeaveApprovers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffLeaveApprovers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffLeaveApprovers_Staff_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffLeaveApprovers_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffLeaves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffLeaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffLeaves_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffLeaves_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffLeaves_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subjects_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PayrollSlips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayrollPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkingDays = table.Column<int>(type: "int", nullable: false),
                    PresentDays = table.Column<int>(type: "int", nullable: false),
                    PaidLeaveDays = table.Column<int>(type: "int", nullable: false),
                    LopDays = table.Column<int>(type: "int", nullable: false),
                    ProrationFactor = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    MonthlyBasicFromProfile = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeductionTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarningsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeductionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CalculationTrace = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollSlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollSlips_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollSlips_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BusStops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StopName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StopCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StopOrder = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Longitude = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Landmark = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DistanceFromSchool = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MorningPickupTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EveningDropTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    MonthlyFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusStops_BusRoutes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "BusRoutes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusStops_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusStops_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookAuthors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimaryAuthor = table.Column<bool>(type: "bit", nullable: false),
                    AuthorOrder = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAuthors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookAuthors_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookAuthors_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReturned = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RenewalCount = table.Column<int>(type: "int", nullable: false),
                    LastRenewalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRenewalDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RenewalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReturnCondition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReturnNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReturnedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HasFine = table.Column<bool>(type: "bit", nullable: false),
                    FineAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaidFineAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PendingFineAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FineReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FineCalculatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssueCondition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDigitalIssue = table.Column<bool>(type: "bit", nullable: false),
                    DigitalAccessUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DigitalAccessExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstReminderSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecondReminderSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalNoticeSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalRemindersSent = table.Column<int>(type: "int", nullable: false),
                    IsLost = table.Column<bool>(type: "bit", nullable: false),
                    IsDamaged = table.Column<bool>(type: "bit", nullable: false),
                    LostReportedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DamageReportedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LostDamageDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReplacementCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsReplacementPaid = table.Column<bool>(type: "bit", nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LibraryMemberId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookIssues_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookIssues_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookIssues_Books_BookId1",
                        column: x => x.BookId1,
                        principalTable: "Books",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookIssues_LibraryMembers_LibraryMemberId",
                        column: x => x.LibraryMemberId,
                        principalTable: "LibraryMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookIssues_LibraryMembers_LibraryMemberId1",
                        column: x => x.LibraryMemberId1,
                        principalTable: "LibraryMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookIssues_Users_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookIssues_Users_ReturnedByUserId",
                        column: x => x.ReturnedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReviewText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Pros = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cons = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecommendedFor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsFlagged = table.Column<bool>(type: "bit", nullable: false),
                    ModerationNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HelpfulVotes = table.Column<int>(type: "int", nullable: false),
                    NotHelpfulVotes = table.Column<int>(type: "int", nullable: false),
                    TotalVotes = table.Column<int>(type: "int", nullable: false),
                    HasReadBook = table.Column<bool>(type: "bit", nullable: false),
                    ReadingStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadingEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadingFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FavoriteQuote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SimilarBooks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WouldRecommend = table.Column<bool>(type: "bit", nullable: false),
                    ReadingLevel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Genre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Mood = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContainsSpoilers = table.Column<bool>(type: "bit", nullable: false),
                    SpoilerWarning = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewSource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceUsed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookReviews_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookReviews_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookReviews_Books_BookId1",
                        column: x => x.BookId1,
                        principalTable: "Books",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookReviews_LibraryMembers_LibraryMemberId",
                        column: x => x.LibraryMemberId,
                        principalTable: "LibraryMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookReviews_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassAttendances_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassAttendances_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassAttendances_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeeStructures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeStructures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeStructures_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeStructures_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnlineClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MeetingId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MeetingPassword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRecordingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsChatEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsNotesEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineClasses_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OnlineClasses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnlineClasses_Staff_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCompulsory = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    TotalMarks = table.Column<int>(type: "int", nullable: false),
                    PassingMarks = table.Column<int>(type: "int", nullable: false),
                    ExamType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Term = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exams_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exams_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exams_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TimetableEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubstituteTeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubstituteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimetableEntries_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Staff_SubstituteTeacherId",
                        column: x => x.SubstituteTeacherId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Staff_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RollNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BloodGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AadhaarNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Religion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianContact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnnualIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdmissionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousSchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BusStopId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MonthlyFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentGrade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusChangeReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_BusStops_BusStopId",
                        column: x => x.BusStopId,
                        principalTable: "BusStops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Students_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Students_Users_StatusChangedBy",
                        column: x => x.StatusChangedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Students_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    NotificationSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsNotificationSent = table.Column<bool>(type: "bit", nullable: false),
                    NotificationMethod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FulfilledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FulfilledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BookIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FulfillmentNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancellationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReservationNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestedEdition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpecialRequirements = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AutoExtend = table.Column<bool>(type: "bit", nullable: false),
                    ExtensionCount = table.Column<int>(type: "int", nullable: false),
                    MaxExtensions = table.Column<int>(type: "int", nullable: false),
                    PickupDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPickupReminderSent = table.Column<bool>(type: "bit", nullable: false),
                    PickupReminderSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDigitalReservation = table.Column<bool>(type: "bit", nullable: false),
                    DigitalAccessGrantedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LibraryMemberId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookReservations_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookReservations_BookIssues_BookIssueId",
                        column: x => x.BookIssueId,
                        principalTable: "BookIssues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookReservations_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookReservations_Books_BookId1",
                        column: x => x.BookId1,
                        principalTable: "Books",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookReservations_LibraryMembers_LibraryMemberId",
                        column: x => x.LibraryMemberId,
                        principalTable: "LibraryMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookReservations_LibraryMembers_LibraryMemberId1",
                        column: x => x.LibraryMemberId1,
                        principalTable: "LibraryMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookReservations_Users_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookReservations_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookReservations_Users_FulfilledByUserId",
                        column: x => x.FulfilledByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LibraryFines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FineNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FineType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PendingAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FineDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OverdueDays = table.Column<int>(type: "int", nullable: true),
                    DailyFineRate = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MaxFineAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    BookReplacementCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DamageDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DamageAssessment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentReceivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsWaived = table.Column<bool>(type: "bit", nullable: false),
                    WaivedAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    WaivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WaivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WaiverReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WaiverApprovalLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AllowInstallments = table.Column<bool>(type: "bit", nullable: false),
                    TotalInstallments = table.Column<int>(type: "int", nullable: true),
                    PaidInstallments = table.Column<int>(type: "int", nullable: true),
                    InstallmentAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    NextInstallmentDue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstNotificationSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastNotificationSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalNotificationsSent = table.Column<int>(type: "int", nullable: false),
                    NotificationMethods = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsAppealed = table.Column<bool>(type: "bit", nullable: false),
                    AppealDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppealReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AppealStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AppealDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppealDecidedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AppealDecisionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookIssueId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LibraryMemberId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryFines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryFines_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryFines_BookIssues_BookIssueId",
                        column: x => x.BookIssueId,
                        principalTable: "BookIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryFines_BookIssues_BookIssueId1",
                        column: x => x.BookIssueId1,
                        principalTable: "BookIssues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryFines_LibraryMembers_LibraryMemberId",
                        column: x => x.LibraryMemberId,
                        principalTable: "LibraryMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryFines_LibraryMembers_LibraryMemberId1",
                        column: x => x.LibraryMemberId1,
                        principalTable: "LibraryMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryFines_Users_AppealDecidedByUserId",
                        column: x => x.AppealDecidedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryFines_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryFines_Users_PaymentReceivedByUserId",
                        column: x => x.PaymentReceivedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryFines_Users_WaivedByUserId",
                        column: x => x.WaivedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OnlineClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AttachmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsSystemMessage = table.Column<bool>(type: "bit", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassMessages_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassMessages_OnlineClasses_OnlineClassId",
                        column: x => x.OnlineClassId,
                        principalTable: "OnlineClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassMessages_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OnlineClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AttachmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttachmentSize = table.Column<long>(type: "bigint", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassNotes_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassNotes_OnlineClasses_OnlineClassId",
                        column: x => x.OnlineClassId,
                        principalTable: "OnlineClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassNotes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OnlineClassParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OnlineClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsMicEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsCameraEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsHandRaised = table.Column<bool>(type: "bit", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    ConnectionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineClassParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineClassParticipants_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OnlineClassParticipants_OnlineClasses_OnlineClassId",
                        column: x => x.OnlineClassId,
                        principalTable: "OnlineClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnlineClassParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attendances_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attendances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassAttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassAttendanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassAttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassAttendanceRecords_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassAttendanceRecords_ClassAttendances_ClassAttendanceId",
                        column: x => x.ClassAttendanceId,
                        principalTable: "ClassAttendances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassAttendanceRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassPromotionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromGrade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToGrade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FromSection = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ToSection = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PromotionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PromotionCriteria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FinalPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FinalGrade = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ClassRank = table.Column<int>(type: "int", nullable: true),
                    AttendancePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    PromotionDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionMadeByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionMadeByRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PromotionRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TeacherRecommendation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsGracePromotion = table.Column<bool>(type: "bit", nullable: false),
                    GracePromotionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresSupplementaryExam = table.Column<bool>(type: "bit", nullable: false),
                    SupplementaryExamSubjects = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsFeeCleared = table.Column<bool>(type: "bit", nullable: false),
                    PendingFeeAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    AreDocumentsComplete = table.Column<bool>(type: "bit", nullable: false),
                    MissingDocuments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransferToSchool = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransferReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransferCertificateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcademicYearId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassPromotionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassPromotionHistories_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassPromotionHistories_AcademicYears_AcademicYearId1",
                        column: x => x.AcademicYearId1,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassPromotionHistories_Classes_FromClassId",
                        column: x => x.FromClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassPromotionHistories_Classes_ToClassId",
                        column: x => x.ToClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassPromotionHistories_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassPromotionHistories_Users_DecisionMadeByUserId",
                        column: x => x.DecisionMadeByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarksObtained = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsPass = table.Column<bool>(type: "bit", nullable: false),
                    IsAbsent = table.Column<bool>(type: "bit", nullable: false),
                    AbsentReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnteredByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResults_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamResults_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamResults_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamResults_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamResults_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentAcademicHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RollNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OverallPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    OverallGrade = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TotalMarksObtained = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    TotalMaxMarks = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    ClassRank = table.Column<int>(type: "int", nullable: true),
                    TotalStudentsInClass = table.Column<int>(type: "int", nullable: true),
                    TotalWorkingDays = table.Column<int>(type: "int", nullable: true),
                    TotalPresentDays = table.Column<int>(type: "int", nullable: true),
                    TotalAbsentDays = table.Column<int>(type: "int", nullable: true),
                    AttendancePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    TotalFeeAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FeePaid = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FeeBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PromotionStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NextClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PromotionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalWarnings = table.Column<int>(type: "int", nullable: true),
                    TotalSuspensions = table.Column<int>(type: "int", nullable: true),
                    DisciplinaryRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtracurricularActivities = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AchievementsAndAwards = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HealthRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AcademicYearId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAcademicHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAcademicHistories_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAcademicHistories_AcademicYears_AcademicYearId1",
                        column: x => x.AcademicYearId1,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAcademicHistories_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAcademicHistories_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAcademicHistories_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentBehaviorIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BehaviorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Archetype = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SeverityIndex = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    InterventionPlan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PsychReferral = table.Column<bool>(type: "bit", nullable: false),
                    ParentFeedbackCycle = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LocationOfIncident = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Witnesses = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActionTaken = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConsequenceType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentBehaviorIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentBehaviorIncidents_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentBehaviorIncidents_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentFees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeStructureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentFees_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentFees_FeeStructures_FeeStructureId",
                        column: x => x.FeeStructureId,
                        principalTable: "FeeStructures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentFees_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentLeaves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLeaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentLeaves_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentLeaves_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StudentLeaves_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentLeaves_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentSubjects_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentSubjects_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferCertificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TcNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClearanceStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GeneratedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalChain = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HashVerification = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeavingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastClassStudied = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PromotedToClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubjectsStudied = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CharacterConduct = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AcademicPerformance = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AnyDuesPending = table.Column<bool>(type: "bit", nullable: false),
                    LibraryClearance = table.Column<bool>(type: "bit", nullable: false),
                    LabClearance = table.Column<bool>(type: "bit", nullable: false),
                    TransportClearance = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferCertificates_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AcademicPerformanceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentAcademicHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Term = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExamType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MarksObtained = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    MaxMarks = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GradePoint = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    PassingMarks = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    SubjectRank = table.Column<int>(type: "int", nullable: true),
                    TotalStudentsAppeared = table.Column<int>(type: "int", nullable: true),
                    WrittenTestMarks = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    PracticalMarks = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ProjectMarks = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    AssignmentMarks = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    VivaMarks = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    TeacherRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StrengthAreas = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImprovementAreas = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClassParticipation = table.Column<int>(type: "int", nullable: true),
                    HomeworkCompletion = table.Column<int>(type: "int", nullable: true),
                    Punctuality = table.Column<int>(type: "int", nullable: true),
                    Discipline = table.Column<int>(type: "int", nullable: true),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResultStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResultPublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultPublishedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequiresRemedialClasses = table.Column<bool>(type: "bit", nullable: false),
                    RemedialRecommendations = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsReExamRequired = table.Column<bool>(type: "bit", nullable: false),
                    ReExamReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AcademicYearId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPerformanceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicPerformanceHistories_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcademicPerformanceHistories_AcademicYears_AcademicYearId1",
                        column: x => x.AcademicYearId1,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcademicPerformanceHistories_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcademicPerformanceHistories_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcademicPerformanceHistories_StudentAcademicHistories_StudentAcademicHistoryId",
                        column: x => x.StudentAcademicHistoryId,
                        principalTable: "StudentAcademicHistories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcademicPerformanceHistories_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcademicPerformanceHistories_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcademicPerformanceHistories_Users_ResultPublishedByUserId",
                        column: x => x.ResultPublishedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcademicPerformanceHistories_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeeHistoriesByYear",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentAcademicHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnnualTuitionFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AdmissionFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DevelopmentFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ExaminationFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LibraryFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LaboratoryFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SportsFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TransportFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    HostelFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MiscellaneousFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalAnnualFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalAmountPaid = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalPendingAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalDiscountAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalLateFeeCharged = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalRefundAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalInstallments = table.Column<int>(type: "int", nullable: false),
                    CompletedInstallments = table.Column<int>(type: "int", nullable: false),
                    PendingInstallments = table.Column<int>(type: "int", nullable: false),
                    OverdueInstallments = table.Column<int>(type: "int", nullable: false),
                    FirstPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiscountType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DiscountReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiscountApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DiscountApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FeeStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FeeCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FeeRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasScholarship = table.Column<bool>(type: "bit", nullable: false),
                    ScholarshipName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScholarshipAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ScholarshipProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HasFinancialAid = table.Column<bool>(type: "bit", nullable: false),
                    FinancialAidAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FinancialAidReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreferredPaymentMethods = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TotalCashPayments = table.Column<int>(type: "int", nullable: false),
                    TotalChequePayments = table.Column<int>(type: "int", nullable: false),
                    TotalOnlinePayments = table.Column<int>(type: "int", nullable: false),
                    TotalCardPayments = table.Column<int>(type: "int", nullable: false),
                    TotalRemindersSet = table.Column<int>(type: "int", nullable: false),
                    LastReminderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalNoticesSent = table.Column<int>(type: "int", nullable: false),
                    LastNoticeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFeeConcessionCase = table.Column<bool>(type: "bit", nullable: false),
                    IsHardshipCase = table.Column<bool>(type: "bit", nullable: false),
                    SpecialCircumstances = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AcademicYearId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeHistoriesByYear", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeHistoriesByYear_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeHistoriesByYear_AcademicYears_AcademicYearId1",
                        column: x => x.AcademicYearId1,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeHistoriesByYear_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeHistoriesByYear_StudentAcademicHistories_StudentAcademicHistoryId",
                        column: x => x.StudentAcademicHistoryId,
                        principalTable: "StudentAcademicHistories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeHistoriesByYear_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeHistoriesByYear_Users_DiscountApprovedByUserId",
                        column: x => x.DiscountApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeHistoriesByYear_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeeDiscounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentFeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscountType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeDiscounts_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeDiscounts_StudentFees_StudentFeeId",
                        column: x => x.StudentFeeId,
                        principalTable: "StudentFees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeDiscounts_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeePayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentFeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReceivedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FeeHistoryByYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeePayments_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeePayments_FeeHistoriesByYear_FeeHistoryByYearId",
                        column: x => x.FeeHistoryByYearId,
                        principalTable: "FeeHistoriesByYear",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeePayments_StudentFees_StudentFeeId",
                        column: x => x.StudentFeeId,
                        principalTable: "StudentFees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeePayments_Users_ReceivedById",
                        column: x => x.ReceivedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeeReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IssuedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeReceipts_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeReceipts_FeePayments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "FeePayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeReceipts_Users_IssuedById",
                        column: x => x.IssuedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_AcademicYearId",
                table: "AcademicPerformanceHistories",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_AcademicYearId1",
                table: "AcademicPerformanceHistories",
                column: "AcademicYearId1");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_ClassId",
                table: "AcademicPerformanceHistories",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_ExamId",
                table: "AcademicPerformanceHistories",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_ResultPublishedByUserId",
                table: "AcademicPerformanceHistories",
                column: "ResultPublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_StudentAcademicHistoryId",
                table: "AcademicPerformanceHistories",
                column: "StudentAcademicHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_StudentId",
                table: "AcademicPerformanceHistories",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_SubjectId",
                table: "AcademicPerformanceHistories",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPerformanceHistories_UpdatedByUserId",
                table: "AcademicPerformanceHistories",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_CreatedByUserId",
                table: "AcademicYears",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_UserId",
                table: "Admins",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_AcademicYearId",
                table: "Announcements",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedBy",
                table: "Announcements",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AcademicYearId",
                table: "Attendances",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_CourseId",
                table: "Attendances",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId",
                table: "Attendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_CreatedByUserId",
                table: "Authors",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_AuthorId",
                table: "BookAuthors",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_BookId",
                table: "BookAuthors",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCategories_CreatedByUserId",
                table: "BookCategories",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_AcademicYearId",
                table: "BookIssues",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_BookId",
                table: "BookIssues",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_BookId1",
                table: "BookIssues",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_IssuedByUserId",
                table: "BookIssues",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_LibraryMemberId",
                table: "BookIssues",
                column: "LibraryMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_LibraryMemberId1",
                table: "BookIssues",
                column: "LibraryMemberId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_ReturnedByUserId",
                table: "BookIssues",
                column: "ReturnedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_AcademicYearId",
                table: "BookReservations",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_BookId",
                table: "BookReservations",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_BookId1",
                table: "BookReservations",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_BookIssueId",
                table: "BookReservations",
                column: "BookIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_CancelledByUserId",
                table: "BookReservations",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_CreatedByUserId",
                table: "BookReservations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_FulfilledByUserId",
                table: "BookReservations",
                column: "FulfilledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_LibraryMemberId",
                table: "BookReservations",
                column: "LibraryMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReservations_LibraryMemberId1",
                table: "BookReservations",
                column: "LibraryMemberId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_AcademicYearId",
                table: "BookReviews",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_ApprovedByUserId",
                table: "BookReviews",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_BookId",
                table: "BookReviews",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_BookId1",
                table: "BookReviews",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_LibraryMemberId",
                table: "BookReviews",
                column: "LibraryMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_BookCategoryId",
                table: "Books",
                column: "BookCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CategoryId",
                table: "Books",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CreatedByUserId",
                table: "Books",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_PublisherId",
                table: "Books",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_PublisherId1",
                table: "Books",
                column: "PublisherId1");

            migrationBuilder.CreateIndex(
                name: "IX_Buses_BusNumber",
                table: "Buses",
                column: "BusNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buses_CreatedByUserId",
                table: "Buses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Buses_RegistrationNumber",
                table: "Buses",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buses_UpdatedById",
                table: "Buses",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BusExpenses_BusId",
                table: "BusExpenses",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_BusExpenses_CreatedByUserId",
                table: "BusExpenses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusFuelRecords_BusId",
                table: "BusFuelRecords",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_BusFuelRecords_CreatedByUserId",
                table: "BusFuelRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusMaintenanceRecords_BusId",
                table: "BusMaintenanceRecords",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_BusMaintenanceRecords_CreatedByUserId",
                table: "BusMaintenanceRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusMaintenanceRecords_UpdatedById",
                table: "BusMaintenanceRecords",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BusRoutes_AcademicYearId",
                table: "BusRoutes",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_BusRoutes_BusId",
                table: "BusRoutes",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_BusRoutes_CreatedByUserId",
                table: "BusRoutes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusRoutes_RouteCode",
                table: "BusRoutes",
                column: "RouteCode",
                unique: true,
                filter: "[RouteCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BusStops_CreatedByUserId",
                table: "BusStops",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusStops_RouteId",
                table: "BusStops",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_BusStops_StopCode",
                table: "BusStops",
                column: "StopCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusStops_UpdatedById",
                table: "BusStops",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_FromUserId_ToUserId_SentAt",
                table: "ChatMessages",
                columns: new[] { "FromUserId", "ToUserId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ToUserId",
                table: "ChatMessages",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRolePolicies_Role_AllowedRole",
                table: "ChatRolePolicies",
                columns: new[] { "Role", "AllowedRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassAttendanceRecords_AcademicYearId",
                table: "ClassAttendanceRecords",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAttendanceRecords_ClassAttendanceId",
                table: "ClassAttendanceRecords",
                column: "ClassAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAttendanceRecords_StudentId",
                table: "ClassAttendanceRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAttendances_AcademicYearId",
                table: "ClassAttendances",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAttendances_ClassId",
                table: "ClassAttendances",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAttendances_CreatedById",
                table: "ClassAttendances",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_AcademicYearId",
                table: "ClassEnrollments",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_ClassId",
                table: "ClassEnrollments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_StudentId",
                table: "ClassEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherId",
                table: "Classes",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMessages_AcademicYearId",
                table: "ClassMessages",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMessages_OnlineClassId",
                table: "ClassMessages",
                column: "OnlineClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMessages_RecipientId",
                table: "ClassMessages",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMessages_SenderId",
                table: "ClassMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassNotes_AcademicYearId",
                table: "ClassNotes",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassNotes_CreatedByUserId",
                table: "ClassNotes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassNotes_OnlineClassId",
                table: "ClassNotes",
                column: "OnlineClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassPromotionHistories_AcademicYearId",
                table: "ClassPromotionHistories",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassPromotionHistories_AcademicYearId1",
                table: "ClassPromotionHistories",
                column: "AcademicYearId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClassPromotionHistories_DecisionMadeByUserId",
                table: "ClassPromotionHistories",
                column: "DecisionMadeByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassPromotionHistories_FromClassId",
                table: "ClassPromotionHistories",
                column: "FromClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassPromotionHistories_StudentId",
                table: "ClassPromotionHistories",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassPromotionHistories_ToClassId",
                table: "ClassPromotionHistories",
                column: "ToClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_AcademicYearId",
                table: "ClassSubjects",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_ClassId_SubjectId_AcademicYearId",
                table: "ClassSubjects",
                columns: new[] { "ClassId", "SubjectId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_SubjectId",
                table: "ClassSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_DepartmentId",
                table: "Courses",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_StaffId",
                table: "Courses",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Designations_DepartmentId",
                table: "Designations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AcademicYearId",
                table: "Documents",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UserId_DocumentType",
                table: "Documents",
                columns: new[] { "UserId", "DocumentType" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VerifiedBy",
                table: "Documents",
                column: "VerifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Events_AcademicYearId",
                table: "Events",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatedByUserId",
                table: "Events",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_AcademicYearId",
                table: "ExamResults",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_EnteredByUserId",
                table: "ExamResults",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ExamId_StudentId",
                table: "ExamResults",
                columns: new[] { "ExamId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_UpdatedByUserId",
                table: "ExamResults",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_AcademicYearId",
                table: "Exams",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ClassId",
                table: "Exams",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CreatedByUserId",
                table: "Exams",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_SubjectId",
                table: "Exams",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FcmTokens_UserId",
                table: "FcmTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDiscounts_AcademicYearId",
                table: "FeeDiscounts",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDiscounts_ApprovedById",
                table: "FeeDiscounts",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDiscounts_StudentFeeId",
                table: "FeeDiscounts",
                column: "StudentFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeHistoriesByYear_AcademicYearId",
                table: "FeeHistoriesByYear",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeHistoriesByYear_AcademicYearId1",
                table: "FeeHistoriesByYear",
                column: "AcademicYearId1");

            migrationBuilder.CreateIndex(
                name: "IX_FeeHistoriesByYear_ClassId",
                table: "FeeHistoriesByYear",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeHistoriesByYear_DiscountApprovedByUserId",
                table: "FeeHistoriesByYear",
                column: "DiscountApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeHistoriesByYear_StudentAcademicHistoryId",
                table: "FeeHistoriesByYear",
                column: "StudentAcademicHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeHistoriesByYear_StudentId",
                table: "FeeHistoriesByYear",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeHistoriesByYear_UpdatedByUserId",
                table: "FeeHistoriesByYear",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_AcademicYearId",
                table: "FeePayments",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_FeeHistoryByYearId",
                table: "FeePayments",
                column: "FeeHistoryByYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_ReceivedById",
                table: "FeePayments",
                column: "ReceivedById");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_StudentFeeId",
                table: "FeePayments",
                column: "StudentFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceipts_AcademicYearId",
                table: "FeeReceipts",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceipts_IssuedById",
                table: "FeeReceipts",
                column: "IssuedById");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceipts_PaymentId",
                table: "FeeReceipts",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_AcademicYearId",
                table: "FeeStructures",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_ClassId",
                table: "FeeStructures",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_AcademicYearId",
                table: "Holidays",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_CreatedByUserId",
                table: "Holidays",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_AcademicYearId",
                table: "LibraryFines",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_AppealDecidedByUserId",
                table: "LibraryFines",
                column: "AppealDecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_BookIssueId",
                table: "LibraryFines",
                column: "BookIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_BookIssueId1",
                table: "LibraryFines",
                column: "BookIssueId1");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_CreatedByUserId",
                table: "LibraryFines",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_LibraryMemberId",
                table: "LibraryFines",
                column: "LibraryMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_LibraryMemberId1",
                table: "LibraryFines",
                column: "LibraryMemberId1");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_PaymentReceivedByUserId",
                table: "LibraryFines",
                column: "PaymentReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryFines_WaivedByUserId",
                table: "LibraryFines",
                column: "WaivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMembers_AcademicYearId",
                table: "LibraryMembers",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMembers_CreatedByUserId",
                table: "LibraryMembers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMembers_UserId",
                table: "LibraryMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineClasses_AcademicYearId",
                table: "OnlineClasses",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineClasses_ClassId",
                table: "OnlineClasses",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineClasses_TeacherId",
                table: "OnlineClasses",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineClassParticipants_AcademicYearId",
                table: "OnlineClassParticipants",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineClassParticipants_OnlineClassId",
                table: "OnlineClassParticipants",
                column: "OnlineClassId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineClassParticipants_UserId",
                table: "OnlineClassParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollComponentDefinitions_Code",
                table: "PayrollComponentDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_AcademicYearId_Year_Month",
                table: "PayrollPeriods",
                columns: new[] { "AcademicYearId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_CreatedByUserId",
                table: "PayrollPeriods",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSlips_PayrollPeriodId_StaffId",
                table: "PayrollSlips",
                columns: new[] { "PayrollPeriodId", "StaffId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSlips_StaffId",
                table: "PayrollSlips",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Periods_AcademicYearId",
                table: "Periods",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Periods_CreatedByUserId",
                table: "Periods",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_CreatedByUserId",
                table: "Permissions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ParentPermissionId",
                table: "Permissions",
                column: "ParentPermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UpdatedByUserId",
                table: "Permissions",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Publishers_CreatedByUserId",
                table: "Publishers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CreatedByUserId",
                table: "RolePermissions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_UpdatedByUserId",
                table: "RolePermissions",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreatedByUserId",
                table: "Roles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UpdatedByUserId",
                table: "Roles",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_DepartmentId",
                table: "Staff",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_DesignationId",
                table: "Staff",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_UserId",
                table: "Staff",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAttendances_AcademicYearId",
                table: "StaffAttendances",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAttendances_StaffId_Date",
                table: "StaffAttendances",
                columns: new[] { "StaffId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffLeaveApprovers_ApproverId",
                table: "StaffLeaveApprovers",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLeaveApprovers_StaffId",
                table: "StaffLeaveApprovers",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLeaves_AcademicYearId",
                table: "StaffLeaves",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLeaves_ApprovedById",
                table: "StaffLeaves",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLeaves_StaffId",
                table: "StaffLeaves",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAcademicHistories_AcademicYearId",
                table: "StudentAcademicHistories",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAcademicHistories_AcademicYearId1",
                table: "StudentAcademicHistories",
                column: "AcademicYearId1");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAcademicHistories_ClassId",
                table: "StudentAcademicHistories",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAcademicHistories_StudentId",
                table: "StudentAcademicHistories",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAcademicHistories_UpdatedByUserId",
                table: "StudentAcademicHistories",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBehaviorIncidents_ClassId",
                table: "StudentBehaviorIncidents",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBehaviorIncidents_StudentId",
                table: "StudentBehaviorIncidents",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_AcademicYearId",
                table: "StudentFees",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_FeeStructureId",
                table: "StudentFees",
                column: "FeeStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_StudentId",
                table: "StudentFees",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLeaves_AcademicYearId",
                table: "StudentLeaves",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLeaves_ApprovedById",
                table: "StudentLeaves",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLeaves_ClassId",
                table: "StudentLeaves",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLeaves_StudentId",
                table: "StudentLeaves",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_BusStopId",
                table: "Students",
                column: "BusStopId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassId",
                table: "Students",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StatusChangedBy",
                table: "Students",
                column: "StatusChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_AcademicYearId",
                table: "StudentSubjects",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_StudentId_SubjectId_AcademicYearId",
                table: "StudentSubjects",
                columns: new[] { "StudentId", "SubjectId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_SubjectId",
                table: "StudentSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_DepartmentId",
                table: "Subjects",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_StaffId",
                table: "Subjects",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_AcademicYearId",
                table: "TimetableEntries",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_ClassId",
                table: "TimetableEntries",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_CreatedByUserId",
                table: "TimetableEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_PeriodId",
                table: "TimetableEntries",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_SubjectId",
                table: "TimetableEntries",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_SubstituteTeacherId",
                table: "TimetableEntries",
                column: "SubstituteTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_TeacherId",
                table: "TimetableEntries",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferCertificates_StudentId",
                table: "TransferCertificates",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_AssignedByUserId",
                table: "UserRoles",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RevokedByUserId",
                table: "UserRoles",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeekOffs_AcademicYearId",
                table: "WeekOffs",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_WeekOffs_CreatedByUserId",
                table: "WeekOffs",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicPerformanceHistories");

            migrationBuilder.DropTable(
                name: "AchievementTalents");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "AssetMaintenanceRecords");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "BiometricDevices");

            migrationBuilder.DropTable(
                name: "BiometricLogs");

            migrationBuilder.DropTable(
                name: "BookAuthors");

            migrationBuilder.DropTable(
                name: "BookReservations");

            migrationBuilder.DropTable(
                name: "BookReviews");

            migrationBuilder.DropTable(
                name: "BusExpenses");

            migrationBuilder.DropTable(
                name: "BusFuelRecords");

            migrationBuilder.DropTable(
                name: "BusMaintenanceRecords");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatRolePolicies");

            migrationBuilder.DropTable(
                name: "ClassAttendanceRecords");

            migrationBuilder.DropTable(
                name: "ClassEnrollments");

            migrationBuilder.DropTable(
                name: "ClassMessages");

            migrationBuilder.DropTable(
                name: "ClassNotes");

            migrationBuilder.DropTable(
                name: "ClassPromotionHistories");

            migrationBuilder.DropTable(
                name: "ClassSubjects");

            migrationBuilder.DropTable(
                name: "CoCurricularActivities");

            migrationBuilder.DropTable(
                name: "CoCurricularParticipations");

            migrationBuilder.DropTable(
                name: "DisciplineIncidents");

            migrationBuilder.DropTable(
                name: "DocumentPrintJobs");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Enquiries");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "FcmTokens");

            migrationBuilder.DropTable(
                name: "FeeDiscounts");

            migrationBuilder.DropTable(
                name: "FeeReceipts");

            migrationBuilder.DropTable(
                name: "GalleryMedia");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "HomeworkAssignments");

            migrationBuilder.DropTable(
                name: "HostelAllocations");

            migrationBuilder.DropTable(
                name: "HostelRooms");

            migrationBuilder.DropTable(
                name: "Hostels");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "Invitees");

            migrationBuilder.DropTable(
                name: "LectureTopics");

            migrationBuilder.DropTable(
                name: "LibraryFines");

            migrationBuilder.DropTable(
                name: "OnlineClassParticipants");

            migrationBuilder.DropTable(
                name: "OnlinePaymentTransactions");

            migrationBuilder.DropTable(
                name: "PayrollComponentDefinitions");

            migrationBuilder.DropTable(
                name: "PayrollSlips");

            migrationBuilder.DropTable(
                name: "PtmFeedbacks");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "ScheduledServices");

            migrationBuilder.DropTable(
                name: "ScholarshipApplications");

            migrationBuilder.DropTable(
                name: "Scholarships");

            migrationBuilder.DropTable(
                name: "SchoolInfos");

            migrationBuilder.DropTable(
                name: "StaffAttendances");

            migrationBuilder.DropTable(
                name: "StaffLeaveApprovers");

            migrationBuilder.DropTable(
                name: "StaffLeaves");

            migrationBuilder.DropTable(
                name: "StudentBehaviorIncidents");

            migrationBuilder.DropTable(
                name: "StudentIdCardRequests");

            migrationBuilder.DropTable(
                name: "StudentLeaves");

            migrationBuilder.DropTable(
                name: "StudentSubjects");

            migrationBuilder.DropTable(
                name: "TimetableEntries");

            migrationBuilder.DropTable(
                name: "TransferCertificates");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "VisitorEntries");

            migrationBuilder.DropTable(
                name: "WeekOffs");

            migrationBuilder.DropTable(
                name: "WorkAllocations");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "ClassAttendances");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "FeePayments");

            migrationBuilder.DropTable(
                name: "BookIssues");

            migrationBuilder.DropTable(
                name: "OnlineClasses");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "FeeHistoriesByYear");

            migrationBuilder.DropTable(
                name: "StudentFees");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "LibraryMembers");

            migrationBuilder.DropTable(
                name: "StudentAcademicHistories");

            migrationBuilder.DropTable(
                name: "FeeStructures");

            migrationBuilder.DropTable(
                name: "BookCategories");

            migrationBuilder.DropTable(
                name: "Publishers");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "BusStops");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "BusRoutes");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "AcademicYears");

            migrationBuilder.DropTable(
                name: "Buses");

            migrationBuilder.DropTable(
                name: "Designations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
