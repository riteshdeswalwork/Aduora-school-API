-- Replaces the inline LINQ in InventoryItemController.GetPaged
-- (GET /api/inventory-items/paged).
CREATE OR ALTER PROCEDURE dbo.sp_InventoryItems_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @Category NVARCHAR(100) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @CategoryLike NVARCHAR(104) = '%' + NULLIF(LTRIM(RTRIM(@Category)), '') + '%';

    SELECT
        x.Id, x.Name, x.Category, x.Sku, x.Quantity, x.CriticalThreshold, x.Unit,
        x.ReorderLevel, x.Location, x.BinLocation, x.CurrentSupplier, x.UnitCost,
        x.DepletionRate, x.BatchNumber, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM InventoryItems x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Category IS NULL OR x.Category LIKE @CategoryLike)
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.Category LIKE @Like OR x.Sku LIKE @Like
             OR x.Location LIKE @Like OR x.CurrentSupplier LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM InventoryItems x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Category IS NULL OR x.Category LIKE @CategoryLike)
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.Category LIKE @Like OR x.Sku LIKE @Like
             OR x.Location LIKE @Like OR x.CurrentSupplier LIKE @Like);
END
