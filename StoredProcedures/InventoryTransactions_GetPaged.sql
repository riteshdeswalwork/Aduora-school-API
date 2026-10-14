-- Replaces the inline LINQ in InventoryTransactionController.GetPaged
-- (GET /api/inventory-transactions/paged). The original only honors
-- transactionType when it's exactly "IN" or "OUT" (case-insensitive) -
-- replicated here with the same guard.
CREATE OR ALTER PROCEDURE dbo.sp_InventoryTransactions_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @InventoryItemId UNIQUEIDENTIFIER = NULL,
    @TransactionType NVARCHAR(10) = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @TypeNorm NVARCHAR(10) = UPPER(LTRIM(RTRIM(@TransactionType)));
    IF @TypeNorm NOT IN ('IN', 'OUT') SET @TypeNorm = NULL;

    SELECT
        x.Id, x.InventoryItemId, x.TransactionType, x.Quantity, x.TransactionDate,
        x.Remarks, x.PerformedByUserId, x.IsActive
    FROM InventoryTransactions x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@InventoryItemId IS NULL OR x.InventoryItemId = @InventoryItemId)
        AND (@TypeNorm IS NULL OR x.TransactionType = @TypeNorm)
        AND (@Term IS NULL OR x.TransactionType LIKE @Like OR x.Remarks LIKE @Like)
    ORDER BY x.TransactionDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM InventoryTransactions x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@InventoryItemId IS NULL OR x.InventoryItemId = @InventoryItemId)
        AND (@TypeNorm IS NULL OR x.TransactionType = @TypeNorm)
        AND (@Term IS NULL OR x.TransactionType LIKE @Like OR x.Remarks LIKE @Like);
END
