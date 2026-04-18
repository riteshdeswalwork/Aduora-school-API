using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Academic
{
    public class SubjectManager : GenericManager<Subject>, ISubjectManager
    {
        public SubjectManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Subject>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Staff)
                    .ThenInclude(staff => staff.User)
                .ToListAsync();
        }

        public override async Task<Subject> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(s => s.Staff)
                    .ThenInclude(staff => staff.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Subject>> GetSubjectsByStaffIdAsync(Guid staffId)
        {
            return await _dbSet
                .Include(s => s.Staff)
                    .ThenInclude(staff => staff.User)
                .Where(s => s.StaffId == staffId)
                .ToListAsync();
        }

        public override async Task<Subject> UpdateAsync(Subject subject)
        {
            subject.UpdatedAt = DateTime.UtcNow;
            return await base.UpdateAsync(subject);
        }
    }
}
