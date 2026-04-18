using System;
using System.Collections.Generic;

namespace sms.api.Constants
{
    public static class PageTabPermissions
    {
        private static readonly Dictionary<string, List<(string key, string[] actions)>> Tabs = new()
        {
            {
                PagePermissions.BusManagement,
                new List<(string, string[])>
                {
                    ("stops", PageActionPermissions.StandardActions),
                    ("routes", PageActionPermissions.StandardActions),
                    ("buses", PageActionPermissions.StandardActions),
                    ("track", new[] { PageActionPermissions.View }),
                    ("maintenance", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.LeaveManagement,
                new List<(string, string[])>
                {
                    ("requests", PageActionPermissions.StandardActions),
                    ("approvals", new[] { PageActionPermissions.View, PageActionPermissions.Approve }),
                    ("types", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.Library,
                new List<(string, string[])>
                {
                    ("dashboard", new[] { PageActionPermissions.View }),
                    ("members", PageActionPermissions.StandardActions),
                    ("issues", PageActionPermissions.StandardActions),
                    ("fines", PageActionPermissions.StandardActions),
                    ("reservations", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.OnlineClasses,
                new List<(string, string[])>
                {
                    ("live", new[] { PageActionPermissions.View }),
                    ("upcoming", new[] { PageActionPermissions.View }),
                    ("all", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.CalendarEvents,
                new List<(string, string[])>
                {
                    ("holidays", PageActionPermissions.StandardActions),
                    ("events", PageActionPermissions.StandardActions),
                    ("weekoffs", PageActionPermissions.StandardActions),
                    ("calendar", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.Exams,
                new List<(string, string[])>
                {
                    ("management", PageActionPermissions.StandardActions),
                    ("results", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.Results,
                new List<(string, string[])>
                {
                    ("view", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.ManageAttendance,
                new List<(string, string[])>
                {
                    ("students", PageActionPermissions.StandardActions),
                    ("staff", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.VisitorManagement,
                new List<(string, string[])>
                {
                    ("entries", PageActionPermissions.StandardActions),
                    ("logs", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.DisciplineManagement,
                new List<(string, string[])>
                {
                    ("incidents", PageActionPermissions.StandardActions),
                    ("reports", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.TransferCertificates,
                new List<(string, string[])>
                {
                    ("requests", new[] { PageActionPermissions.View, PageActionPermissions.Add, PageActionPermissions.Edit, PageActionPermissions.Delete, PageActionPermissions.Approve }),
                    ("generated", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.CoCurricularActivities,
                new List<(string, string[])>
                {
                    ("activities", PageActionPermissions.StandardActions),
                    ("participation", PageActionPermissions.StandardActions),
                    ("outcomes", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.Enquiries,
                new List<(string, string[])>
                {
                    ("enquiries", PageActionPermissions.StandardActions),
                    ("followups", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.WorkAllocation,
                new List<(string, string[])>
                {
                    ("tasks", PageActionPermissions.StandardActions),
                    ("my-tasks", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.DocumentPrinting,
                new List<(string, string[])>
                {
                    ("jobs", PageActionPermissions.StandardActions),
                    ("history", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.HomeworkAssignments,
                new List<(string, string[])>
                {
                    ("assignments", PageActionPermissions.StandardActions),
                    ("history", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.LectureTopics,
                new List<(string, string[])>
                {
                    ("topics", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.AchievementsTalents,
                new List<(string, string[])>
                {
                    ("achievements", PageActionPermissions.StandardActions),
                    ("talent", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.StudentIdCards,
                new List<(string, string[])>
                {
                    ("requests", new[] { PageActionPermissions.View, PageActionPermissions.Add, PageActionPermissions.Edit, PageActionPermissions.Delete, PageActionPermissions.Approve }),
                    ("print-history", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.PtmFeedback,
                new List<(string, string[])>
                {
                    ("feedback", new[] { PageActionPermissions.View }),
                    ("reports", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.BiometricIntegration,
                new List<(string, string[])>
                {
                    ("devices", PageActionPermissions.StandardActions),
                    ("logs", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.InventoryManagement,
                new List<(string, string[])>
                {
                    ("items", PageActionPermissions.StandardActions),
                    ("transactions", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.ScheduledServices,
                new List<(string, string[])>
                {
                    ("schedules", PageActionPermissions.StandardActions),
                    ("history", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.PhotoGallery,
                new List<(string, string[])>
                {
                    ("photos", PageActionPermissions.StandardActions),
                    ("videos", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.InviteesManagement,
                new List<(string, string[])>
                {
                    ("invitees", PageActionPermissions.StandardActions),
                    ("rsvps", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.StudentBehaviorManagement,
                new List<(string, string[])>
                {
                    ("incidents", PageActionPermissions.StandardActions),
                    ("interventions", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.HostelManagement,
                new List<(string, string[])>
                {
                    ("hostels", PageActionPermissions.StandardActions),
                    ("rooms", PageActionPermissions.StandardActions),
                    ("allocations", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.ScholarshipManagement,
                new List<(string, string[])>
                {
                    ("scholarships", PageActionPermissions.StandardActions),
                    ("applications", new[] { PageActionPermissions.View, PageActionPermissions.Add, PageActionPermissions.Edit, PageActionPermissions.Delete, PageActionPermissions.Approve })
                }
            },
            {
                PagePermissions.AssetMaintenance,
                new List<(string, string[])>
                {
                    ("assets", PageActionPermissions.StandardActions),
                    ("maintenance", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.OnlinePayments,
                new List<(string, string[])>
                {
                    ("transactions", new[] { PageActionPermissions.View })
                }
            },
            {
                PagePermissions.Payroll,
                new List<(string, string[])>
                {
                    ("components", PageActionPermissions.StandardActions),
                    ("periods", PageActionPermissions.StandardActions),
                    ("slips", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.FeeManagement,
                new List<(string, string[])>
                {
                    ("collection", PageActionPermissions.StandardActions),
                    ("structures", PageActionPermissions.StandardActions)
                }
            },
            {
                PagePermissions.ManageCalendar,
                new List<(string, string[])>
                {
                    ("holidays", PageActionPermissions.StandardActions),
                    ("events", PageActionPermissions.StandardActions),
                    ("weekoffs", PageActionPermissions.StandardActions),
                    ("calendar", new[] { PageActionPermissions.View })
                }
            }
        };

        public static IReadOnlyList<string> GetTabsForPage(string pageCode)
        {
            return Tabs.TryGetValue(pageCode, out var list)
                ? list.ConvertAll(t => t.key)
                : Array.Empty<string>();
        }

        public static IReadOnlyList<string> GetActionsForTab(string pageCode, string tabKey)
        {
            if (!Tabs.TryGetValue(pageCode, out var list)) return Array.Empty<string>();
            foreach (var t in list)
            {
                if (string.Equals(t.key, tabKey, StringComparison.OrdinalIgnoreCase))
                    return t.actions;
            }
            return Array.Empty<string>();
        }

        public static string GetTabCode(string pageCode, string tabKey)
        {
            if (string.IsNullOrWhiteSpace(pageCode) || string.IsNullOrWhiteSpace(tabKey)) return string.Empty;
            return $"{pageCode}.tab.{tabKey.ToLowerInvariant()}";
        }

        public static string GetTabActionCode(string pageCode, string tabKey, string action)
        {
            var baseCode = GetTabCode(pageCode, tabKey);
            return string.IsNullOrEmpty(baseCode) ? string.Empty : $"{baseCode}.{action.ToLowerInvariant()}";
        }
    }
}
