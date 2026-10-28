using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Fee;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;

namespace sms.api.Managers.Fee
{
    public class FeeStructureManager : GenericManager<FeeStructure>, IFeeStructureManager
    {

        public FeeStructureManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<FeeStructure>> GetAllAsync()
        {
            return await _dbSet
                .Include(fs => fs.Class)
                .Include(fs => fs.Lines)
                .Include(fs => fs.StudentFees)
                .OrderByDescending(fs => fs.CreatedAt)
                .ToListAsync();
        }

        public override async Task<FeeStructure> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(fs => fs.Class)
                .Include(fs => fs.Lines)
                .Include(fs => fs.StudentFees)
                .FirstOrDefaultAsync(fs => fs.Id == id);
        }

        public async Task<IEnumerable<FeeStructure>> GetFeeStructuresByClassIdAsync(Guid classId)
        {
            return await _dbSet
                .Include(fs => fs.Class)
                .Include(fs => fs.Lines)
                .Where(fs => fs.ClassId == classId || fs.ClassId == null)
                .OrderBy(fs => fs.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeeStructure>> GetFeeStructuresByAcademicYearIdAsync(Guid academicYearId)
        {
            return await _dbSet
                .Include(fs => fs.Class)
                .Include(fs => fs.Lines)
                .Where(fs => fs.AcademicYearId == academicYearId)
                .OrderBy(fs => fs.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeeStructure>> GetActiveFeeStructuresAsync()
        {
            return await _dbSet
                .Include(fs => fs.Class)
                .Include(fs => fs.Lines)
                .Where(fs => fs.IsActive)
                .OrderBy(fs => fs.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeeStructure>> GetFeeStructuresByAcademicYearAsync(string academicYear)
        {
            return await _dbSet
                .Include(fs => fs.Class)
                .Include(fs => fs.Lines)
                .Where(fs => fs.AcademicYear == academicYear)
                .OrderBy(fs => fs.Name)
                .ToListAsync();
        }

        public async Task<FeeStructure?> GetFeeStructureByClassAndTypeAsync(Guid classId, string feeType)
        {
            return await _dbSet
                .Include(fs => fs.Class)
                .Include(fs => fs.Lines)
                .FirstOrDefaultAsync(fs => (fs.ClassId == classId || fs.ClassId == null) && fs.FeeType == feeType && fs.IsActive);
        }

        public async Task<FeeStructureLine> AddLineAsync(Guid feeStructureId, FeeStructureLine line)
        {
            var fs = await _dbSet.FirstOrDefaultAsync(f => f.Id == feeStructureId);
            if (fs == null)
                throw new InvalidOperationException("Fee structure not found.");

            line.Id = Guid.NewGuid();
            line.FeeStructureId = feeStructureId;
            line.CreatedAt = DateTime.UtcNow;
            _context.FeeStructureLines.Add(line);
            await _context.SaveChangesAsync();
            return line;
        }

        public async Task<FeeStructureLine?> UpdateLineAsync(Guid feeStructureId, Guid lineId, FeeStructureLine line)
        {
            var existing = await _context.FeeStructureLines
                .FirstOrDefaultAsync(l => l.Id == lineId && l.FeeStructureId == feeStructureId);
            if (existing == null)
                return null;

            existing.LineCode = line.LineCode;
            existing.Name = line.Name;
            existing.Amount = line.Amount;
            existing.SortOrder = line.SortOrder;
            existing.DefaultConcessionPercent = line.DefaultConcessionPercent;
            existing.IsActive = line.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteLineAsync(Guid feeStructureId, Guid lineId)
        {
            var existing = await _context.FeeStructureLines
                .FirstOrDefaultAsync(l => l.Id == lineId && l.FeeStructureId == feeStructureId);
            if (existing == null)
                return false;

            var hasLedger = await _context.StudentFees.AnyAsync(sf => sf.FeeStructureLineId == lineId);
            if (hasLedger)
                throw new InvalidOperationException("Cannot delete a fee line that already has student ledger rows.");

            _context.FeeStructureLines.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
