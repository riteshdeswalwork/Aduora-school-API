using sms.api.Models.Security;

namespace sms.api.Utils
{
    public static class DataScopeHelper
    {
        public static IEnumerable<Guid>? AllowedClassIdsForQuery(StaffScopeContext scope)
        {
            if (scope.IsUnrestricted) return null;
            return scope.AllowedClassIds ?? Array.Empty<Guid>();
        }
    }
}
