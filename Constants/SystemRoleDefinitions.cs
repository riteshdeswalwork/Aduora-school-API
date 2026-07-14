namespace sms.api.Constants
{
    /// <summary>
    /// System roles seeded by DataInitialization step-5-roles, with page-level permission templates.
    /// Full = page root + all descendants; ViewOnly = page root + permissions whose code ends with ".view".
    /// </summary>
    public static class SystemRoleDefinitions
    {
        /// <summary>Grant permissions for specific tabs on a page (when full-page access is too broad).</summary>
        public sealed class TabAccessSpec
        {
            public required string PageCode { get; init; }
            public required string TabKey { get; init; }
            /// <summary>True = all tab actions; false = tab view actions only.</summary>
            public bool FullAccess { get; init; } = true;
        }

        public sealed class Definition
        {
            public required string Name { get; init; }
            public required string Description { get; init; }
            /// <summary>When true, role receives every active permission (Admin).</summary>
            public bool GrantAllPermissions { get; init; }
            public DataScopeType DataScope { get; init; } = DataScopeType.AssignedClasses;
            public string[] FullAccessPageCodes { get; init; } = [];
            public string[] ViewOnlyPageCodes { get; init; } = [];
            public TabAccessSpec[] TabAccessSpecs { get; init; } = [];
        }

        public static DataScopeType GetDefaultDataScopeForRoleName(string roleName)
        {
            return roleName.ToLowerInvariant() switch
            {
                "admin" or "principal" or "accountant" or "librarian" or "receptionist" or "transport manager" or "hr" => DataScopeType.Global,
                "teacher" or "staff" => DataScopeType.AssignedClasses,
                "student" or "parent" or "driver" => DataScopeType.OwnOnly,
                _ => DataScopeType.AssignedClasses
            };
        }

        private static readonly string[] PrincipalExcludedPages =
        [
            PagePermissions.AdminManagement,
            PagePermissions.Roles,
            PagePermissions.Permissions
        ];

        private static readonly string[] PrincipalViewPages = PagePermissions.GetAllPageCodesForSeed()
            .Where(p => !PrincipalExcludedPages.Contains(p))
            .ToArray();

        public static Definition[] All { get; } =
        [
            new Definition
            {
                Name = "Admin",
                Description = "Full control — users, roles, settings, and all modules.",
                GrantAllPermissions = true,
                DataScope = DataScopeType.Global
            },
            new Definition
            {
                Name = "Principal",
                Description = "Reports and overall school monitoring (broad read access; no role/user admin).",
                DataScope = DataScopeType.Global,
                ViewOnlyPageCodes = PrincipalViewPages
            },
            new Definition
            {
                Name = "Teacher",
                Description = "Mark attendance, upload marks, assign homework, manage class academic workflow.",
                DataScope = DataScopeType.AssignedClasses,
                FullAccessPageCodes =
                [
                    PagePermissions.Attendance,
                    PagePermissions.HomeworkAssignments,
                    PagePermissions.LectureTopics,
                    PagePermissions.Exams,
                    PagePermissions.Results,
                    PagePermissions.Timetable,
                    PagePermissions.OnlineClasses
                ],
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Settings,
                    PagePermissions.Students,
                    PagePermissions.Classes,
                    PagePermissions.Subjects,
                    PagePermissions.AcademicYears,
                    PagePermissions.Periods,
                    PagePermissions.Chat,
                    PagePermissions.Announcements,
                    PagePermissions.ManageCalendar,
                    PagePermissions.PtmFeedback,
                    PagePermissions.CoCurricularActivities,
                    PagePermissions.AchievementsTalents
                ],
                TabAccessSpecs =
                [
                    new TabAccessSpec { PageCode = PagePermissions.ManageAttendance, TabKey = "students", FullAccess = true },
                    new TabAccessSpec { PageCode = PagePermissions.LeaveManagement, TabKey = "requests", FullAccess = true },
                    new TabAccessSpec { PageCode = PagePermissions.DisciplineManagement, TabKey = "incidents", FullAccess = true },
                    new TabAccessSpec { PageCode = PagePermissions.DisciplineManagement, TabKey = "resolved", FullAccess = false },
                    new TabAccessSpec { PageCode = PagePermissions.DisciplineManagement, TabKey = "conduct-rules", FullAccess = false },
                    new TabAccessSpec { PageCode = PagePermissions.StudentBehaviorManagement, TabKey = "incidents", FullAccess = true },
                    new TabAccessSpec { PageCode = PagePermissions.StudentBehaviorManagement, TabKey = "interventions", FullAccess = false },
                    new TabAccessSpec { PageCode = PagePermissions.PtmFeedback, TabKey = "feedback", FullAccess = true }
                ]
            },
            new Definition
            {
                Name = "Student",
                Description = "View attendance, marks, timetable, homework; announcements and own fee/payment info.",
                DataScope = DataScopeType.OwnOnly,
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Settings,
                    PagePermissions.Attendance,
                    PagePermissions.Results,
                    PagePermissions.Timetable,
                    PagePermissions.HomeworkAssignments,
                    PagePermissions.Exams,
                    PagePermissions.Chat,
                    PagePermissions.Announcements,
                    PagePermissions.ManageCalendar,
                    PagePermissions.Fees,
                    PagePermissions.FeeCollection,
                    PagePermissions.OnlinePayments
                ]
            },
            new Definition
            {
                Name = "Parent",
                Description = "Track child progress; fees, attendance, and results (read-only).",
                DataScope = DataScopeType.OwnOnly,
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Settings,
                    PagePermissions.Students,
                    PagePermissions.Attendance,
                    PagePermissions.Results,
                    PagePermissions.Fees,
                    PagePermissions.FeeCollection,
                    PagePermissions.FeeReports,
                    PagePermissions.HomeworkAssignments,
                    PagePermissions.PtmFeedback,
                    PagePermissions.Timetable,
                    PagePermissions.Chat,
                    PagePermissions.Announcements,
                    PagePermissions.ManageCalendar,
                    PagePermissions.OnlinePayments
                ]
            },
            new Definition
            {
                Name = "Accountant",
                Description = "Fees collection, salary-related staff visibility, financial reports.",
                DataScope = DataScopeType.Global,
                FullAccessPageCodes =
                [
                    PagePermissions.Fees,
                    PagePermissions.FeeCollection,
                    PagePermissions.FeeReports,
                    PagePermissions.FeeStructures,
                    PagePermissions.FeeBulkGenerateDemands,
                    PagePermissions.FeeManagement,
                    PagePermissions.FeePaymentVoid,
                    PagePermissions.OnlinePayments,
                    PagePermissions.Payroll
                ],
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Settings,
                    PagePermissions.Analytics,
                    PagePermissions.Staff
                ]
            },
            new Definition
            {
                Name = "Librarian",
                Description = "Book issue/return and library records.",
                DataScope = DataScopeType.Global,
                FullAccessPageCodes = [PagePermissions.Library],
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Settings,
                    PagePermissions.Students
                ]
            },
            new Definition
            {
                Name = "Receptionist",
                Description = "Inquiries, visitor management, student admissions and related documents.",
                DataScope = DataScopeType.Global,
                FullAccessPageCodes =
                [
                    PagePermissions.Enquiries,
                    PagePermissions.VisitorManagement,
                    PagePermissions.Students,
                    PagePermissions.DocumentPrinting,
                    PagePermissions.TransferCertificates
                ],
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Settings,
                    PagePermissions.Classes,
                    PagePermissions.AcademicYears,
                    PagePermissions.ManageCalendar,
                    PagePermissions.StudentIdCards
                ]
            },
            new Definition
            {
                Name = "Transport Manager",
                Description = "Bus routes, vehicles, drivers, and tracking.",
                DataScope = DataScopeType.Global,
                FullAccessPageCodes = [PagePermissions.BusManagement],
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Settings,
                    PagePermissions.Staff
                ]
            },
            new Definition
            {
                Name = "Driver",
                Description = "Assigned routes, stops, buses, and tracking — operational view (no master data changes).",
                DataScope = DataScopeType.OwnOnly,
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Settings,
                    PagePermissions.Announcements,
                    PagePermissions.ManageCalendar
                ],
                TabAccessSpecs =
                [
                    new TabAccessSpec { PageCode = PagePermissions.BusManagement, TabKey = "track", FullAccess = false }
                ]
            },
            new Definition
            {
                Name = "HR",
                Description = "Staff hiring, staff attendance, leave, work allocation, payroll-related visibility.",
                DataScope = DataScopeType.Global,
                FullAccessPageCodes =
                [
                    PagePermissions.Staff,
                    PagePermissions.LeaveManagement,
                    PagePermissions.WorkAllocation,
                    PagePermissions.Departments,
                    PagePermissions.Designations,
                    PagePermissions.BiometricIntegration,
                    PagePermissions.Payroll
                ],
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Settings,
                    PagePermissions.SchoolInfo,
                    PagePermissions.ManageCalendar,
                    PagePermissions.Analytics
                ],
                TabAccessSpecs =
                [
                    new TabAccessSpec { PageCode = PagePermissions.ManageAttendance, TabKey = "staff", FullAccess = true },
                    new TabAccessSpec { PageCode = PagePermissions.ManageAttendance, TabKey = "students", FullAccess = false }
                ]
            }
        ];
    }
}
