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
    public class ClassEnrollmentManager : GenericManager<ClassEnrollment>, IClassEnrollmentManager
    {
        public ClassEnrollmentManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<ClassEnrollment>> GetAllAsync()
        {
            return await _dbSet.Include(e => e.Student).Include(e => e.Class).ToListAsync();
        }

        public override async Task<ClassEnrollment> GetByIdAsync(Guid id)
        {
            return await _dbSet.Include(e => e.Student).Include(e => e.Class).FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<ClassEnrollment>> GetEnrollmentsByClassIdAsync(Guid classId)
        {
            return await _dbSet.Include(e => e.Student).Where(e => e.ClassId == classId).ToListAsync();
        }

        public async Task<IEnumerable<ClassEnrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId)
        {
            return await _dbSet.Include(e => e.Class).Where(e => e.StudentId == studentId).ToListAsync();
        }

        public async Task<ClassEnrollment> GetEnrollmentByStudentAndClassAsync(Guid studentId, Guid classId)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Class)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.ClassId == classId);
        }

        public async Task<ClassEnrollment?> GetEnrollmentByStudentAndAcademicYearAsync(Guid studentId, Guid academicYearId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(e => e.Class)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.AcademicYearId == academicYearId && e.IsActive);
        }

        public async Task<bool> UpdateClassEnrollmentAsync(Guid enrollmentId, bool isActive)
        {
            var enrollment = await GetByIdAsync(enrollmentId);
            if (enrollment == null)
                return false;

            enrollment.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<(Guid ClassId, Guid StudentId, DateTime? EnrollmentDate)>> GetAllEnrollmentsAsync()
        {
            var items = await _dbSet
                .AsNoTracking()
                .Select(ce => new { ce.ClassId, ce.StudentId, ce.EnrollmentDate })
                .ToListAsync();
            return items.Select(x => (x.ClassId, x.StudentId, (DateTime?)x.EnrollmentDate));
        }

        public override async Task<ClassEnrollment> CreateAsync(ClassEnrollment enrollment)
        {
            // Update the class's current enrollment count
            var classEntity = await _context.Classes.FindAsync(enrollment.ClassId);
            if (classEntity != null)
            {
                classEntity.CurrentEnrollment += 1;
                _context.Classes.Update(classEntity);
            }

            return await base.CreateAsync(enrollment);
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            var enrollment = await GetByIdAsync(id);
            if (enrollment == null)
                return false;

            // Update the class's current enrollment count
            var classEntity = await _context.Classes.FindAsync(enrollment.ClassId);
            if (classEntity != null && classEntity.CurrentEnrollment > 0)
            {
                classEntity.CurrentEnrollment -= 1;
                _context.Classes.Update(classEntity);
            }

            return await base.DeleteAsync(id);
        }
    }
}
