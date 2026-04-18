namespace sms.api.Constants
{
    /// <summary>
    /// Permission codes for role-based page and feature access.
    /// Frontend uses same codes to show/hide nav and buttons.
    /// </summary>
    public static class PagePermissions
    {
        public const string PagePrefix = "page.";

        // Core
        public const string Dashboard = "page.dashboard";
        public const string Settings = "page.settings";
        public const string AdminManagement = "page.admin-management";
        public const string Students = "page.students";
        public const string Staff = "page.staff";
        public const string Classes = "page.classes";
        public const string Subjects = "page.subjects";
        public const string Departments = "page.departments";
        public const string Designations = "page.designations";
        public const string SchoolInfo = "page.school-info";

        // Academics
        public const string AcademicYears = "page.academic-years";
        public const string Periods = "page.periods";
        public const string OnlineClasses = "page.online-classes";
        public const string Attendance = "page.attendance";
        public const string ManageAttendance = "page.manage-attendance";
        public const string Exams = "page.exams";
        public const string Results = "page.results";
        public const string Timetable = "page.timetable";

        // Finance
        public const string Fees = "page.fees";
        public const string FeeCollection = "page.fee-collection";
        public const string FeeReports = "page.fee-reports";
        public const string FeeStructures = "page.fee-structures";
        public const string Payroll = "page.payroll";

        // Operations
        public const string Library = "page.library";
        public const string BusManagement = "page.bus-management";
        public const string LeaveManagement = "page.leave-management";
        public const string CalendarEvents = "page.calendar-events";
        public const string Holidays = "page.holidays";
        public const string Events = "page.events";
        public const string WeekOffs = "page.weekoffs";
        public const string Announcements = "page.announcements";
        public const string VisitorManagement = "page.visitor-management";
        public const string DisciplineManagement = "page.discipline-management";
        public const string TransferCertificates = "page.transfer-certificates";
        public const string CoCurricularActivities = "page.co-curricular-activities";
        public const string Enquiries = "page.enquiries";
        public const string WorkAllocation = "page.work-allocation";
        public const string DocumentPrinting = "page.document-printing";
        public const string HomeworkAssignments = "page.homework-assignments";
        public const string LectureTopics = "page.lecture-topics";
        public const string AchievementsTalents = "page.achievements-talents";
        public const string StudentIdCards = "page.student-id-cards";
        public const string PtmFeedback = "page.ptm-feedback";
        public const string BiometricIntegration = "page.biometric-integration";
        public const string InventoryManagement = "page.inventory-management";
        public const string ScheduledServices = "page.scheduled-services";
        public const string PhotoGallery = "page.photo-gallery";
        public const string InviteesManagement = "page.invitees-management";
        public const string StudentBehaviorManagement = "page.student-behavior-management";
        public const string HostelManagement = "page.hostel-management";
        public const string ScholarshipManagement = "page.scholarship-management";
        public const string AssetMaintenance = "page.asset-maintenance";

        // Analytics & Chat
        public const string Analytics = "page.analytics";
        public const string Chat = "page.chat";

        // Finance (Extensions)
        public const string OnlinePayments = "page.online-payments";

        /// <summary>Void wrongly posted fee payments (counter); backend enforces Admin role.</summary>
        public const string FeePaymentVoid = "page.fee-payment-void";

        /// <summary>Bulk monthly demand generation for the whole school.</summary>
        public const string FeeBulkGenerateDemands = "page.fee-bulk-generate-demands";

        /// <summary>Combined fee UI route (<c>/fee-management</c>).</summary>
        public const string FeeManagement = "page.fee-management";

        /// <summary>Calendar hub route (<c>/manage-calendar</c>) — same tabs as calendar-events.</summary>
        public const string ManageCalendar = "page.manage-calendar";

        public const string SendNotifications = "page.send-notifications";

        // Role & Permission management
        public const string Roles = "page.roles";
        public const string Permissions = "page.permissions";

        /// <summary>All page permission codes for Admin fallback when no Role/Permission data.</summary>
        public static readonly string[] AllPages =
        {
            Dashboard, Settings, AdminManagement, Roles, Permissions, Students, Staff, Classes, Subjects,
            Departments, Designations, SchoolInfo, AcademicYears, Periods, OnlineClasses,
            Attendance, ManageAttendance, Exams, Results, Timetable, Fees, FeeCollection,
            FeeReports, FeeStructures, Payroll, Library, BusManagement, LeaveManagement, CalendarEvents,
            Holidays, Events, WeekOffs, Announcements, VisitorManagement, DisciplineManagement,
            TransferCertificates, CoCurricularActivities, Enquiries, WorkAllocation, DocumentPrinting,
            HomeworkAssignments, LectureTopics, AchievementsTalents, StudentIdCards, PtmFeedback,
            BiometricIntegration, InventoryManagement, ScheduledServices, PhotoGallery, InviteesManagement,
            StudentBehaviorManagement, HostelManagement, ScholarshipManagement, AssetMaintenance,
            OnlinePayments, Analytics, Chat
        };

        /// <summary>
        /// Extra portal permission roots seeded by <c>DataInitialization/step-5-permissions</c> (routes not listed in <see cref="AllPages"/>).
        /// Keep in sync with frontend paths under <c>/send-notifications</c>, <c>/fee-management</c>, <c>/manage-calendar</c>.
        /// </summary>
        public static readonly string[] AdditionalSeedPageCodes =
        {
            FeeManagement,
            ManageCalendar,
            SendNotifications,
            FeePaymentVoid,
            FeeBulkGenerateDemands
        };

        /// <summary>All page roots used by step-5-permissions (no duplicates).</summary>
        public static string[] GetAllPageCodesForSeed() =>
            AllPages.Concat(AdditionalSeedPageCodes).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        /// <summary>Default page codes for Staff when no Role/Permission data.</summary>
        public static readonly string[] StaffDefaultPages =
        {
            Dashboard, Settings, Students, Staff, Classes, Subjects, AcademicYears, Periods,
            OnlineClasses, Attendance, ManageAttendance, Exams, Results, Timetable,
            CalendarEvents, Holidays, Events, Announcements, Chat, LeaveManagement, Library,
            VisitorManagement, DisciplineManagement, CoCurricularActivities, Enquiries, WorkAllocation,
            DocumentPrinting, HomeworkAssignments, LectureTopics, AchievementsTalents, PtmFeedback,
            BiometricIntegration, InventoryManagement, ScheduledServices, PhotoGallery, InviteesManagement,
            StudentBehaviorManagement, HostelManagement, ScholarshipManagement, AssetMaintenance,
            OnlinePayments, Payroll, FeeManagement, ManageCalendar, SendNotifications,
            TransferCertificates, StudentIdCards
        };
    }
}
