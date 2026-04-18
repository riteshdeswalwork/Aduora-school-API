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
    public class FeeDiscountManager : GenericManager<FeeDiscount>, IFeeDiscountManager
    {

        public FeeDiscountManager(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<FeeDiscount>> GetAllAsync()
        {
            return await _dbSet
                .Include(fd => fd.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fd => fd.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                        .ThenInclude(fs => fs.Class)
                .Include(fd => fd.ApprovedBy)
                .OrderByDescending(fd => fd.ApprovedDate)
                .ToListAsync();
        }

        public override async Task<FeeDiscount> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(fd => fd.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fd => fd.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                        .ThenInclude(fs => fs.Class)
                .Include(fd => fd.ApprovedBy)
                .FirstOrDefaultAsync(fd => fd.Id == id);
        }

        public async Task<IEnumerable<FeeDiscount>> GetDiscountsByStudentFeeIdAsync(Guid studentFeeId)
        {
            return await _dbSet
                .Include(fd => fd.ApprovedBy)
                .Where(fd => fd.StudentFeeId == studentFeeId)
                .OrderByDescending(fd => fd.ApprovedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeeDiscount>> GetDiscountsByStudentIdAsync(Guid studentId)
        {
            return await _dbSet
                .Include(fd => fd.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                .Include(fd => fd.ApprovedBy)
                .Where(fd => fd.StudentFee.StudentId == studentId)
                .OrderByDescending(fd => fd.ApprovedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeeDiscount>> GetDiscountsByTypeAsync(string discountType)
        {
            return await _dbSet
                .Include(fd => fd.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fd => fd.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                        .ThenInclude(fs => fs.Class)
                .Include(fd => fd.ApprovedBy)
                .Where(fd => fd.DiscountType == discountType)
                .OrderByDescending(fd => fd.ApprovedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeeDiscount>> GetDiscountsByApproverIdAsync(Guid approverId)
        {
            return await _dbSet
                .Include(fd => fd.StudentFee)
                    .ThenInclude(sf => sf.Student)
                        .ThenInclude(s => s.User)
                .Include(fd => fd.StudentFee)
                    .ThenInclude(sf => sf.FeeStructure)
                        .ThenInclude(fs => fs.Class)
                .Where(fd => fd.ApprovedById == approverId)
                .OrderByDescending(fd => fd.ApprovedDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalDiscountByStudentAsync(Guid studentId)
        {
            return await _dbSet
                .Where(fd => fd.StudentFee.StudentId == studentId)
                .SumAsync(fd => fd.Amount);
        }

        public async Task<decimal> GetTotalDiscountByClassAsync(Guid classId)
        {
            return await _dbSet
                .Where(fd => fd.StudentFee.FeeStructure.ClassId == classId)
                .SumAsync(fd => fd.Amount);
        }

        public override async Task<FeeDiscount> CreateAsync(FeeDiscount entity)
        {
            var discount = await base.CreateAsync(entity);

            // Update the student fee outstanding amount after adding discount
            await UpdateStudentFeeOutstandingAsync(entity.StudentFeeId);

            return discount;
        }

        public override async Task<FeeDiscount> UpdateAsync(FeeDiscount entity)
        {
            var discount = await base.UpdateAsync(entity);

            // Update the student fee outstanding amount after updating discount
            await UpdateStudentFeeOutstandingAsync(entity.StudentFeeId);

            return discount;
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            var discount = await GetByIdAsync(id);
            if (discount == null) return false;

            var studentFeeId = discount.StudentFeeId;
            var result = await base.DeleteAsync(id);

            if (result)
            {
                // Update the student fee outstanding amount after deleting discount
                await UpdateStudentFeeOutstandingAsync(studentFeeId);
            }

            return result;
        }

        private async Task UpdateStudentFeeOutstandingAsync(Guid studentFeeId)
        {
            var studentFee = await _context.StudentFees
                .Include(sf => sf.Payments)
                .Include(sf => sf.Discounts)
                .FirstOrDefaultAsync(sf => sf.Id == studentFeeId);

            if (studentFee == null) return;

            var totalPaid = studentFee.Payments?.Where(p => !p.IsVoided).Sum(p => p.Amount) ?? 0;
            var totalDiscount = studentFee.Discounts?.Sum(d => d.Amount) ?? 0;

            studentFee.PaidAmount = totalPaid;
            studentFee.DiscountAmount = totalDiscount;
            studentFee.OutstandingAmount = studentFee.TotalAmount - totalPaid - totalDiscount;

            // Update status
            if (studentFee.OutstandingAmount <= 0)
            {
                studentFee.Status = "Paid";
            }
            else if (studentFee.PaidAmount > 0)
            {
                studentFee.Status = "Partial";
            }
            else if (studentFee.DueDate < DateTime.UtcNow)
            {
                studentFee.Status = "Overdue";
            }
            else
            {
                studentFee.Status = "Pending";
            }

            studentFee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
