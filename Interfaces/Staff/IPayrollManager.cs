using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Staff
{
    public interface IPayrollManager
    {
        Task<IReadOnlyList<PayrollComponentDefinitionDto>> GetComponentsAsync();
        Task<PayrollComponentDefinitionDto?> GetComponentByIdAsync(Guid id);
        Task<PayrollComponentDefinitionDto> CreateComponentAsync(PayrollComponentWriteDto dto);
        Task<PayrollComponentDefinitionDto?> UpdateComponentAsync(Guid id, PayrollComponentWriteDto dto);
        Task<bool> DeleteComponentAsync(Guid id);
        Task<int> SeedDefaultComponentsAsync();

        Task<IReadOnlyList<PayrollPeriodDto>> GetPeriodsAsync(Guid academicYearId);
        Task<PayrollPeriodDto?> GetPeriodByIdAsync(Guid id);
        Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriodCreateDto dto, Guid? createdByUserId);
        Task<PayrollRecalculateResultDto> RecalculatePeriodAsync(Guid periodId);
        Task<bool> FinalizePeriodAsync(Guid periodId);
        Task<bool> MarkPeriodPaidAsync(Guid periodId);
        Task<bool> LockPeriodAsync(Guid periodId);
        Task<bool> ReopenPeriodAsync(Guid periodId);

        Task<IReadOnlyList<PayrollSlipDto>> GetSlipsForPeriodAsync(Guid periodId);
        Task<PayrollSlipDto?> GetSlipByIdAsync(Guid slipId);
    }
}
