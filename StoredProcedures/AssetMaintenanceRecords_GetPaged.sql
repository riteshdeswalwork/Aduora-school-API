-- Replaces the inline LINQ in AssetMaintenanceRecordController.GetPaged
-- (GET /api/asset-maintenance/paged).
CREATE OR ALTER PROCEDURE dbo.sp_AssetMaintenanceRecords_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AssetId UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.AssetId, x.ServiceDate, x.NextDueDate, x.Vendor, x.VendorContact,
        x.ServiceCost, x.PostServiceHealthGrade, x.MaintenanceChecklist, x.Notes,
        x.Status, x.IsActive
    FROM AssetMaintenanceRecords x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@AssetId IS NULL OR x.AssetId = @AssetId)
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Term IS NULL OR x.Vendor LIKE @Like OR x.Status LIKE @Like OR x.Notes LIKE @Like OR x.MaintenanceChecklist LIKE @Like)
    ORDER BY x.ServiceDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM AssetMaintenanceRecords x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@AssetId IS NULL OR x.AssetId = @AssetId)
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Term IS NULL OR x.Vendor LIKE @Like OR x.Status LIKE @Like OR x.Notes LIKE @Like OR x.MaintenanceChecklist LIKE @Like);
END
