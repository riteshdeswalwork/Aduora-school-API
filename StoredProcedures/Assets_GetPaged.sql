-- Replaces the inline LINQ in AssetController.GetPaged (GET /api/assets/paged).
CREATE OR ALTER PROCEDURE dbo.sp_Assets_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.Name, x.Category, x.SerialNumber, x.TagId, x.PurchaseDate,
        x.WarrantyExpiryDate, x.PurchaseCost, x.DepreciatedValue, x.Location,
        x.SupplierContact, x.LifespanEstimate, x.Status, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM Assets x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.Category LIKE @Like OR x.SerialNumber LIKE @Like
             OR x.Location LIKE @Like OR x.Status LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM Assets x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.Category LIKE @Like OR x.SerialNumber LIKE @Like
             OR x.Location LIKE @Like OR x.Status LIKE @Like);
END
