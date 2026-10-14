-- Replaces the inline LINQ in BiometricLogController.GetPaged
-- (GET /api/biometric-logs/paged). Unbounded audit-log table (grows
-- forever) - a genuine candidate for pagination, not just a Tier-2
-- "move the LINQ into a proc" case.
CREATE OR ALTER PROCEDURE dbo.sp_BiometricLogs_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @DeviceId UNIQUEIDENTIFIER = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @StudentId UNIQUEIDENTIFIER = NULL,
    @StaffId UNIQUEIDENTIFIER = NULL,
    @LogType NVARCHAR(10) = NULL,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        x.Id, x.DeviceId, x.UserId, x.StudentId, x.StaffId, x.LoggedAt, x.LogType,
        x.RawData, x.ExternalUserId, x.AttendanceApplied, x.IsActive
    FROM BiometricLogs x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@DeviceId IS NULL OR x.DeviceId = @DeviceId)
        AND (@UserId IS NULL OR x.UserId = @UserId)
        AND (@StudentId IS NULL OR x.StudentId = @StudentId)
        AND (@StaffId IS NULL OR x.StaffId = @StaffId)
        AND (@LogType IS NULL OR x.LogType = @LogType)
        AND (@FromDate IS NULL OR x.LoggedAt >= @FromDate)
        AND (@ToDate IS NULL OR x.LoggedAt <= @ToDate)
    ORDER BY x.LoggedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM BiometricLogs x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@DeviceId IS NULL OR x.DeviceId = @DeviceId)
        AND (@UserId IS NULL OR x.UserId = @UserId)
        AND (@StudentId IS NULL OR x.StudentId = @StudentId)
        AND (@StaffId IS NULL OR x.StaffId = @StaffId)
        AND (@LogType IS NULL OR x.LogType = @LogType)
        AND (@FromDate IS NULL OR x.LoggedAt >= @FromDate)
        AND (@ToDate IS NULL OR x.LoggedAt <= @ToDate);
END
