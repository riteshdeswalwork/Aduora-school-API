using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Admin;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Admin
{
    public class DepartmentManager : GenericManager<Department>, IDepartmentManager
    {
        public DepartmentManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _dbSet
                .Include(d => d.Designations)
                .Include(d => d.Staff)
                    .ThenInclude(s => s.User)
                .Include(d => d.Subjects)
                .Include(d => d.Courses)
                .ToListAsync();
        }

        public override async Task<Department> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(d => d.Designations)
                .Include(d => d.Staff)
                    .ThenInclude(s => s.User)
                .Include(d => d.Subjects)
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
        {
            return await _dbSet
                .Where(d => d.IsActive)
                .Include(d => d.Designations)
                .Include(d => d.Staff)
                    .ThenInclude(s => s.User)
                .Include(d => d.Subjects)
                .Include(d => d.Courses)
                .ToListAsync();
        }

        public async Task<Department?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Include(d => d.Designations)
                .Include(d => d.Staff)
                    .ThenInclude(s => s.User)
                .Include(d => d.Subjects)
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(d => d.Code == code);
        }

        public async Task<IEnumerable<Department>> SearchByNameAsync(string name)
        {
            return await _dbSet
                .Where(d => d.Name.Contains(name))
                .Include(d => d.Designations)
                .Include(d => d.Staff)
                    .ThenInclude(s => s.User)
                .Include(d => d.Subjects)
                .Include(d => d.Courses)
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
        {
            var query = _dbSet.Where(d => d.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<Designation>> GetDepartmentDesignationsAsync(Guid departmentId)
        {
            return await _context.Designations
                .Where(d => d.DepartmentId == departmentId && d.IsActive)
                .Include(d => d.Staff)
                    .ThenInclude(s => s.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.Staff>> GetDepartmentStaffAsync(Guid departmentId)
        {
            return await _context.Staff
                .Where(s => s.Designation != null && s.Designation.DepartmentId == departmentId)
                .Include(s => s.User)
                .Include(s => s.Designation)
                .ToListAsync();
        }



        public async Task<IEnumerable<Course>> GetDepartmentCoursesAsync(Guid departmentId)
        {
            return await _context.Courses
                .Where(c => c.DepartmentId == departmentId && c.IsActive)
                .Include(c => c.Staff)
                    .ThenInclude(staff => staff.User)
                .ToListAsync();
        }

        public override async Task<Department> CreateAsync(Department entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            return await base.CreateAsync(entity);
        }

        public override async Task<Department> UpdateAsync(Department entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            return await base.UpdateAsync(entity);
        }
    }
}
