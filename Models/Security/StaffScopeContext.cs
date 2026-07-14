using sms.api.Constants;

namespace sms.api.Models.Security
{
    public sealed class StaffScopeContext
    {
        public DataScopeType EffectiveScope { get; init; } = DataScopeType.OwnOnly;
        public Guid? StaffId { get; init; }
        /// <summary>null = unrestricted (Global scope)</summary>
        public IReadOnlyList<Guid>? AllowedClassIds { get; init; }
        /// <summary>null = unrestricted; empty list = no students visible</summary>
        public IReadOnlyList<Guid>? AllowedStudentIds { get; init; }

        public bool IsUnrestricted => EffectiveScope == DataScopeType.Global;

        public bool CanAccessClass(Guid classId)
        {
            if (IsUnrestricted) return true;
            if (EffectiveScope != DataScopeType.AssignedClasses) return false;
            return AllowedClassIds != null && AllowedClassIds.Contains(classId);
        }

        public bool CanAccessStudent(Guid studentId)
        {
            if (IsUnrestricted) return true;
            if (EffectiveScope != DataScopeType.AssignedClasses) return false;
            return AllowedStudentIds != null && AllowedStudentIds.Contains(studentId);
        }
    }
}
