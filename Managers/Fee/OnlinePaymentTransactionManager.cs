using Dapper;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Fee;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services;
using sms.api.Services.Data;

namespace sms.api.Managers.Fee
{
    public class OnlinePaymentTransactionManager : GenericManager<OnlinePaymentTransaction>, IOnlinePaymentTransactionManager
    {
        private readonly INotificationOrchestrator? _notify;
        private readonly ISqlDataAccess _sqlDataAccess;

        public OnlinePaymentTransactionManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess, INotificationOrchestrator? notify = null) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
            _notify = notify;
        }

        public async Task<PagedResult<OnlinePaymentTransaction>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, string? searchTerm,
            string? status, string? settlementStatus, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@AcademicYearId", academicYearId);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            parameters.Add("@SettlementStatus", string.IsNullOrWhiteSpace(settlementStatus) ? null : settlementStatus.Trim());
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<OnlinePaymentTransaction>(
                "dbo.sp_OnlinePaymentTransactions_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<OnlinePaymentTransaction>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public override async Task<OnlinePaymentTransaction> CreateAsync(OnlinePaymentTransaction entity)
        {
            var created = await base.CreateAsync(entity);
            await NotifyStatusChangeAsync(created, null);
            return created;
        }

        public override async Task<OnlinePaymentTransaction> UpdateAsync(OnlinePaymentTransaction entity)
        {
            var existing = await _context.OnlinePaymentTransactions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == entity.Id);
            var oldStatus = existing?.Status;
            var updated = await base.UpdateAsync(entity);
            await NotifyStatusChangeAsync(updated, oldStatus);
            return updated;
        }

        private async Task NotifyStatusChangeAsync(OnlinePaymentTransaction tx, string? previousStatus)
        {
            if (string.IsNullOrWhiteSpace(tx.Status)) return;
            if (previousStatus != null && string.Equals(previousStatus, tx.Status, StringComparison.OrdinalIgnoreCase))
                return;

            var isSuccess = IsSuccessStatus(tx.Status);
            var isFailure = IsFailureStatus(tx.Status);
            if (!isSuccess && !isFailure) return;

            var studentUserId = tx.StudentId.HasValue
                ? await NotificationEvents.GetStudentUserIdAsync(_context, tx.StudentId.Value)
                : null;

            if (isSuccess)
            {
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    studentUserId.HasValue ? new[] { studentUserId.Value } : Enumerable.Empty<Guid>(),
                    "Online payment successful",
                    $"Payment of {tx.Amount:N2} was successful. Ref: {tx.TransactionReference ?? tx.ReceiptNumber ?? tx.Id.ToString()}",
                    "payment.success",
                    "fees",
                    NotificationChannels.All,
                    tx.Id,
                    "OnlinePaymentTransaction",
                    "success",
                    idempotencyKey: $"payment.success:{tx.Id}",
                    includeAdmins: true);
            }
            else
            {
                await NotificationEvents.NotifyUsersAsync(
                    _notify,
                    studentUserId.HasValue ? new[] { studentUserId.Value } : Enumerable.Empty<Guid>(),
                    "Online payment failed",
                    $"Payment of {tx.Amount:N2} failed. Ref: {tx.TransactionReference ?? tx.MerchantRefId ?? tx.Id.ToString()}",
                    "payment.failed",
                    "fees",
                    NotificationChannels.All,
                    tx.Id,
                    "OnlinePaymentTransaction",
                    "warning",
                    idempotencyKey: $"payment.failed:{tx.Id}",
                    includeAdmins: true);
            }
        }

        private static bool IsSuccessStatus(string status) =>
            status.Equals("Success", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Paid", StringComparison.OrdinalIgnoreCase);

        private static bool IsFailureStatus(string status) =>
            status.Equals("Failed", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Failure", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Canceled", StringComparison.OrdinalIgnoreCase);
    }
}
