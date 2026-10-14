using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    public class InventoryTransactionManager : GenericManager<InventoryTransaction>, IInventoryTransactionManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public InventoryTransactionManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<PagedResult<InventoryTransaction>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? inventoryItemId, string? transactionType, string? searchTerm, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@InventoryItemId", inventoryItemId);
            parameters.Add("@TransactionType", string.IsNullOrWhiteSpace(transactionType) ? null : transactionType.Trim());
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@IncludeInactive", includeInactive);

            var (items, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<InventoryTransaction>(
                "dbo.sp_InventoryTransactions_GetPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<InventoryTransaction>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }
    }
}
