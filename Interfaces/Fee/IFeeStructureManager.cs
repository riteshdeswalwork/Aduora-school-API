using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Fee
{
    public interface IFeeStructureManager : IGenericManager<FeeStructure>
    {
        Task<IEnumerable<FeeStructure>> GetFeeStructuresByClassIdAsync(Guid classId);
        Task<IEnumerable<FeeStructure>> GetFeeStructuresByAcademicYearIdAsync(Guid academicYearId);
        Task<IEnumerable<FeeStructure>> GetActiveFeeStructuresAsync();
        Task<IEnumerable<FeeStructure>> GetFeeStructuresByAcademicYearAsync(string academicYear);
        Task<FeeStructure?> GetFeeStructureByClassAndTypeAsync(Guid classId, string feeType);
        Task<FeeStructureLine> AddLineAsync(Guid feeStructureId, FeeStructureLine line);
        Task<FeeStructureLine?> UpdateLineAsync(Guid feeStructureId, Guid lineId, FeeStructureLine line);
        Task<bool> DeleteLineAsync(Guid feeStructureId, Guid lineId);
    }
}
