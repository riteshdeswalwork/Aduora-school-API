-- Replaces BusController's "maintenance/paged" endpoint, which previously
-- fetched up to 500 rows via BusManager.GetAllMaintenanceRecordsAsync and
-- then paged them IN MEMORY (.Skip().Take() in C#) - this is a genuine
-- database-side pagination fix, not just a LINQ->SP move. Returns the
-- same flat shape as BusMaintenanceDTO directly.
CREATE OR ALTER PROCEDURE dbo.sp_BusMaintenanceRecords_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @BusId UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @MaintenanceType NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.BusId, b.BusNumber, x.MaintenanceType, x.MaintenanceDate, x.Description,
        x.Cost, x.ServiceProvider, x.ServiceProviderPhone, x.Odometer, x.NextServiceDate,
        x.PartsReplaced, x.Remarks, x.IsCompleted,
        LTRIM(RTRIM(COALESCE(u.FirstName, '') + ' ' + COALESCE(u.LastName, ''))) AS CreatedByName,
        x.CreatedAt
    FROM BusMaintenanceRecords x
    LEFT JOIN Buses b ON b.Id = x.BusId
    LEFT JOIN Users u ON u.Id = x.CreatedByUserId
    WHERE
        (@BusId IS NULL OR x.BusId = @BusId)
        AND (@MaintenanceType IS NULL OR x.MaintenanceType = @MaintenanceType)
        AND (@Term IS NULL
             OR b.BusNumber LIKE @Like OR x.MaintenanceType LIKE @Like OR x.Description LIKE @Like
             OR x.ServiceProvider LIKE @Like OR x.PartsReplaced LIKE @Like OR x.Remarks LIKE @Like)
    ORDER BY x.MaintenanceDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM BusMaintenanceRecords x
    LEFT JOIN Buses b ON b.Id = x.BusId
    WHERE
        (@BusId IS NULL OR x.BusId = @BusId)
        AND (@MaintenanceType IS NULL OR x.MaintenanceType = @MaintenanceType)
        AND (@Term IS NULL
             OR b.BusNumber LIKE @Like OR x.MaintenanceType LIKE @Like OR x.Description LIKE @Like
             OR x.ServiceProvider LIKE @Like OR x.PartsReplaced LIKE @Like OR x.Remarks LIKE @Like);
END
