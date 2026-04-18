using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Admin;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Admin
{
    public class SchoolInfoManager : ISchoolInfoManager
    {
        private readonly ApplicationDbContext _context;

        public SchoolInfoManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SchoolInfo?> GetSchoolInfoAsync()
        {
            return await _context.SchoolInfos
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<SchoolInfo> CreateOrUpdateSchoolInfoAsync(SchoolInfo schoolInfo)
        {
            var existingSchoolInfo = await GetSchoolInfoAsync();

            if (existingSchoolInfo != null)
            {
                // Update existing school info
                existingSchoolInfo.SchoolName = schoolInfo.SchoolName;
                existingSchoolInfo.Description = schoolInfo.Description;
                existingSchoolInfo.Address = schoolInfo.Address;
                existingSchoolInfo.PhoneNumber = schoolInfo.PhoneNumber;
                existingSchoolInfo.Email = schoolInfo.Email;
                existingSchoolInfo.Website = schoolInfo.Website;
                existingSchoolInfo.Principal = schoolInfo.Principal;
                existingSchoolInfo.VicePrincipal = schoolInfo.VicePrincipal;
                existingSchoolInfo.EstablishedYear = schoolInfo.EstablishedYear;
                existingSchoolInfo.AffiliationBoard = schoolInfo.AffiliationBoard;
                existingSchoolInfo.Mission = schoolInfo.Mission;
                existingSchoolInfo.Vision = schoolInfo.Vision;
                existingSchoolInfo.LogoUrl = schoolInfo.LogoUrl;
                existingSchoolInfo.UpdatedAt = DateTime.UtcNow;

                _context.SchoolInfos.Update(existingSchoolInfo);
                await _context.SaveChangesAsync();
                return existingSchoolInfo;
            }
            else
            {
                // Create new school info
                schoolInfo.Id = Guid.NewGuid();
                schoolInfo.CreatedAt = DateTime.UtcNow;
                schoolInfo.IsActive = true;

                _context.SchoolInfos.Add(schoolInfo);
                await _context.SaveChangesAsync();
                return schoolInfo;
            }
        }

        public async Task<bool> DeleteSchoolInfoAsync(Guid id)
        {
            var schoolInfo = await _context.SchoolInfos.FindAsync(id);
            if (schoolInfo == null)
                return false;

            schoolInfo.IsActive = false;
            schoolInfo.UpdatedAt = DateTime.UtcNow;

            _context.SchoolInfos.Update(schoolInfo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
