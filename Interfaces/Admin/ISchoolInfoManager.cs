using sms.api.Models;

namespace sms.api.Interfaces.Admin
{
    public interface ISchoolInfoManager
    {
        Task<SchoolInfo?> GetSchoolInfoAsync();
        Task<SchoolInfo> CreateOrUpdateSchoolInfoAsync(SchoolInfo schoolInfo);
        Task<bool> DeleteSchoolInfoAsync(Guid id);
    }
}
