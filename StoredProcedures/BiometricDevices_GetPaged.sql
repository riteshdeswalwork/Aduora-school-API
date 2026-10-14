-- Replaces the inline LINQ in BiometricDeviceController.GetPaged
-- (GET /api/biometric-devices/paged).
CREATE OR ALTER PROCEDURE dbo.sp_BiometricDevices_GetPaged
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
        x.Id, x.DeviceIdentifier, x.IpAddress, x.Port, x.MacAddress, x.FirmwareVersion,
        x.SyncIntervalSeconds, x.ApiKey, x.Location, x.Model, x.Status, x.IsActive, x.CreatedAt
    FROM BiometricDevices x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(LTRIM(RTRIM(x.Status))) = LOWER(LTRIM(RTRIM(@Status))))
        AND (@Term IS NULL OR x.DeviceIdentifier LIKE @Like OR x.Location LIKE @Like OR x.Model LIKE @Like OR x.Status LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM BiometricDevices x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(LTRIM(RTRIM(x.Status))) = LOWER(LTRIM(RTRIM(@Status))))
        AND (@Term IS NULL OR x.DeviceIdentifier LIKE @Like OR x.Location LIKE @Like OR x.Model LIKE @Like OR x.Status LIKE @Like);
END
