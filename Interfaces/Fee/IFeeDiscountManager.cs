using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Fee
{
    public interface IFeeDiscountManager : IGenericManager<FeeDiscount>
    {
        Task<IEnumerable<FeeDiscount>> GetDiscountsByStudentFeeIdAsync(Guid studentFeeId);
        Task<IEnumerable<FeeDiscount>> GetDiscountsByStudentIdAsync(Guid studentId);
        Task<IEnumerable<FeeDiscount>> GetDiscountsByTypeAsync(string discountType);
        Task<IEnumerable<FeeDiscount>> GetDiscountsByApproverIdAsync(Guid approverId);
        Task<decimal> GetTotalDiscountByStudentAsync(Guid studentId);
        Task<decimal> GetTotalDiscountByClassAsync(Guid classId);
    }
}
