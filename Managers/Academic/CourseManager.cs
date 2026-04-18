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
    public class CourseManager : GenericManager<Course>, ICourseManager
    {
        public CourseManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _dbSet.Include(c => c.Staff).ToListAsync();
        }

        public override async Task<Course> GetByIdAsync(Guid id)
        {
            return await _dbSet.Include(c => c.Staff).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(Guid departmentId)
        {
            return await _dbSet.Where(c => c.DepartmentId == departmentId).ToListAsync();
        }

        public async Task<IEnumerable<Models.Student>> GetStudentsByCourseIdAsync(Guid courseId)
        {
            // Since we removed Enrollments table, return empty list for now
            // This method may need to be reimplemented based on new requirements
            return new List<Models.Student>();
        }

        public async Task<bool> AssignStaffToCourseAsync(Guid courseId, Guid staffId)
        {
            var course = await GetByIdAsync(courseId);
            if (course == null)
                return false;

            var staff = await _context.Staff.FindAsync(staffId);
            if (staff == null)
                return false;

            course.StaffId = staffId;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public override async Task<Course> UpdateAsync(Course course)
        {
            course.UpdatedAt = DateTime.UtcNow;
            return await base.UpdateAsync(course);
        }
    }
}
