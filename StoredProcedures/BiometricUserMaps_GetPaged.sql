-- Replaces the inline LINQ in BiometricUserMapController.GetPaged
-- (GET /api/biometric-user-maps/paged). The original query has no
-- .Include() - navigation props come back null, same as before.
CREATE OR ALTER PROCEDURE dbo.sp_BiometricUserMaps_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @DeviceId UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.DeviceId, x.ExternalUserId, x.PersonType, x.StaffId, x.StudentId,
        x.DisplayName, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM BiometricUserMaps x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@DeviceId IS NULL OR x.DeviceId = @DeviceId)
        AND (@Term IS NULL OR x.ExternalUserId LIKE @Like OR x.DisplayName LIKE @Like)
    ORDER BY x.ExternalUserId
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM BiometricUserMaps x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@DeviceId IS NULL OR x.DeviceId = @DeviceId)
        AND (@Term IS NULL OR x.ExternalUserId LIKE @Like OR x.DisplayName LIKE @Like);
END
