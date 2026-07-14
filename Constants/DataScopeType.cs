namespace sms.api.Constants
{
    /// <summary>
    /// How far a role can see/modify data. Most permissive role wins when user has multiple roles.
    /// </summary>
    public enum DataScopeType
    {
        /// <summary>Full school data (Admin, Principal, Accountant, etc.)</summary>
        Global = 0,

        /// <summary>Only classes where Staff is class teacher (Class.TeacherId)</summary>
        AssignedClasses = 1,

        /// <summary>Only own user/staff record</summary>
        OwnOnly = 2
    }
}
