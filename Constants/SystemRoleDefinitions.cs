namespace sms.api.Constants
{
    /// <summary>
    /// System roles seeded by DataInitialization step-5-roles, with page-level permission templates.
    /// Full = page root + all descendants; ViewOnly = page root + permissions whose code ends with ".view".
    /// </summary>
    public static class SystemRoleDefinitions
    {
        public sealed class Definition
        {
            public required string Name { get; init; }
            public required string Description { get; init; }
            /// <summary>When true, role receives every active permission (Admin).</summary>
            public bool GrantAllPermissions { get; init; }
            public string[] FullAccessPageCodes { get; init; } = [];
            public string[] ViewOnlyPageCodes { get; init; } = [];
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
                GrantAllPermissions = true
            },
            new Definition
            {
                Name = "Principal",
                Description = "Reports and overall school monitoring (broad read access; no role/user admin).",
                ViewOnlyPageCodes = PrincipalViewPages
            },
            new Definition
            {
                Name = "Teacher",
                Description = "Mark attendance, upload marks, assign homework, manage class academic workflow.",
                FullAccessPageCodes =
                [
                    PagePermissions.ManageAttendance,
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
                    PagePermissions.Students,
                    PagePermissions.Classes,
                    PagePermissions.Subjects,
                    PagePermissions.AcademicYears,
                    PagePermissions.Periods,
                    PagePermissions.Chat,
                    PagePermissions.Announcements,
                    PagePermissions.CalendarEvents,
                    PagePermissions.Events,
                    PagePermissions.Holidays,
                    PagePermissions.WeekOffs,
                    PagePermissions.PtmFeedback,
                    PagePermissions.CoCurricularActivities,
                    PagePermissions.AchievementsTalents
                ]
            },
            new Definition
            {
                Name = "Student",
                Description = "View attendance, marks, timetable, homework; announcements and own fee/payment info.",
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Attendance,
                    PagePermissions.Results,
                    PagePermissions.Timetable,
                    PagePermissions.HomeworkAssignments,
                    PagePermissions.Chat,
                    PagePermissions.Announcements,
                    PagePermissions.Fees,
                    PagePermissions.FeeCollection,
                    PagePermissions.OnlinePayments
                ]
            },
            new Definition
            {
                Name = "Parent",
                Description = "Track child progress; fees, attendance, and results (read-only).",
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
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
                    PagePermissions.OnlinePayments
                ]
            },
            new Definition
            {
                Name = "Accountant",
                Description = "Fees collection, salary-related staff visibility, financial reports.",
                FullAccessPageCodes =
                [
                    PagePermissions.Fees,
                    PagePermissions.FeeCollection,
                    PagePermissions.FeeReports,
                    PagePermissions.FeeStructures,
                    PagePermissions.FeeBulkGenerateDemands,
                    PagePermissions.OnlinePayments,
                    PagePermissions.Payroll
                ],
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Analytics,
                    PagePermissions.Staff
                ]
            },
            new Definition
            {
                Name = "Librarian",
                Description = "Book issue/return and library records.",
                FullAccessPageCodes = [PagePermissions.Library],
                ViewOnlyPageCodes = [PagePermissions.Dashboard]
            },
            new Definition
            {
                Name = "Receptionist",
                Description = "Inquiries, visitor management, student admissions and related documents.",
                FullAccessPageCodes =
                [
                    PagePermissions.Enquiries,
                    PagePermissions.VisitorManagement,
                    PagePermissions.Students,
                    PagePermissions.DocumentPrinting
                ],
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.Classes,
                    PagePermissions.AcademicYears
                ]
            },
            new Definition
            {
                Name = "Transport Manager",
                Description = "Bus routes, vehicles, drivers, and tracking.",
                FullAccessPageCodes = [PagePermissions.BusManagement],
                ViewOnlyPageCodes = [PagePermissions.Dashboard]
            },
            new Definition
            {
                Name = "Driver",
                Description = "Assigned routes, stops, buses, and tracking — operational view (no master data changes).",
                ViewOnlyPageCodes =
                [
                    PagePermissions.Dashboard,
                    PagePermissions.BusManagement,
                    PagePermissions.Announcements,
                    PagePermissions.CalendarEvents
                ]
            },
            new Definition
            {
                Name = "HR",
                Description = "Staff hiring, staff attendance, leave, work allocation, payroll-related visibility.",
                FullAccessPageCodes =
                [
                    PagePermissions.Staff,
                    PagePermissions.ManageAttendance,
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
                    PagePermissions.SchoolInfo,
                    PagePermissions.CalendarEvents
                ]
            }
        ];
    }
}
