using System.Collections.Generic;

namespace sms.api.Constants
{
    /// <summary>
    /// Action (sub-permission) codes per page. Convention: page.&lt;page-slug&gt;.view, .add, .edit, .delete, .approve.
    /// Used for seeding and for frontend permission gating.
    /// </summary>
    public static class PageActionPermissions
    {
        public const string View = "view";
        public const string Add = "add";
        public const string Edit = "edit";
        public const string Delete = "delete";
        public const string Approve = "approve";

        /// <summary>Standard CRUD + Approve actions for pages that support them.</summary>
        public static readonly string[] StandardActions = { View, Add, Edit, Delete };

        /// <summary>Actions that include Approve (e.g. leave, fee approval).</summary>
        public static readonly string[] ActionsWithApprove = { View, Add, Edit, Delete, Approve };

        /// <summary>Get action suffix for a page code. E.g. "page.students" -> "students".</summary>
        public static string GetPageSlug(string pageCode)
        {
            if (string.IsNullOrEmpty(pageCode) || !pageCode.StartsWith(PagePermissions.PagePrefix, StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            return pageCode.Substring(PagePermissions.PagePrefix.Length).Trim();
        }

        /// <summary>Get sub-permission code. E.g. ("page.students", "add") -> "page.students.add".</summary>
        public static string GetActionCode(string pageCode, string action)
        {
            var slug = GetPageSlug(pageCode);
            return string.IsNullOrEmpty(slug) ? string.Empty : $"{pageCode}.{action.ToLowerInvariant()}";
        }

        /// <summary>For pages that have approve workflow (leave, fees, etc.).</summary>
        public static readonly string[] PagesWithApprove =
        {
            PagePermissions.LeaveManagement,
            PagePermissions.FeeCollection,
            PagePermissions.FeeReports,
            PagePermissions.TransferCertificates,
            PagePermissions.StudentIdCards,
            PagePermissions.ScholarshipManagement
        };

        /// <summary>Pages that are view-only or have minimal actions (no add/edit/delete).</summary>
        public static readonly string[] ViewOnlyPages =
        {
            PagePermissions.Dashboard,
            PagePermissions.Settings,
            PagePermissions.Analytics,
            PagePermissions.Chat
        };

        /// <summary>Get actions to seed for a given page code.</summary>
        public static IReadOnlyList<string> GetActionsForPage(string pageCode)
        {
            if (ViewOnlyPages.Contains(pageCode))
                return new[] { View };
            if (PagesWithApprove.Contains(pageCode))
                return ActionsWithApprove;
            return StandardActions;
        }
    }
}
