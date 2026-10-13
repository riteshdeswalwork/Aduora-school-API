-- Replaces the inline LINQ in VisitorEntryController.GetPaged
-- (GET /api/visitor-entries/paged). Flat single-table query. Mirrors the
-- original filters (active/inactive, "currently inside" active-only,
-- TimeIn date range, free-text search across 8 fields) and TimeIn DESC sort.
CREATE OR ALTER PROCEDURE dbo.sp_VisitorEntries_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0,
    @ActiveOnly BIT = 0,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @ToDateExclusive DATETIME2 = CASE WHEN @ToDate IS NULL THEN NULL ELSE DATEADD(DAY, 1, CAST(@ToDate AS DATE)) END;

    SELECT
        x.Id, x.VisitorName, x.ContactNumber, x.Address, x.Purpose, x.HostStaffId,
        x.DepartmentId, x.EntryGate, x.NumberOfPersons, x.IdType, x.IdNumber,
        x.PersonToMeet, x.VehicleNumber, x.BadgeNumber, x.TimeIn, x.TimeOut,
        x.Status, x.CheckInById, x.CheckOutById, x.Remarks, x.IsActive,
        x.CreatedAt, x.UpdatedAt
    FROM VisitorEntries x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@ActiveOnly = 0 OR (x.Status = 'In' AND x.TimeOut IS NULL))
        AND (@FromDate IS NULL OR x.TimeIn >= @FromDate)
        AND (@ToDateExclusive IS NULL OR x.TimeIn < @ToDateExclusive)
        AND (@Term IS NULL
             OR x.VisitorName LIKE @Like OR x.ContactNumber LIKE @Like OR x.Purpose LIKE @Like
             OR x.PersonToMeet LIKE @Like OR x.VehicleNumber LIKE @Like OR x.Address LIKE @Like
             OR x.IdNumber LIKE @Like OR x.BadgeNumber LIKE @Like)
    ORDER BY x.TimeIn DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM VisitorEntries x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@ActiveOnly = 0 OR (x.Status = 'In' AND x.TimeOut IS NULL))
        AND (@FromDate IS NULL OR x.TimeIn >= @FromDate)
        AND (@ToDateExclusive IS NULL OR x.TimeIn < @ToDateExclusive)
        AND (@Term IS NULL
             OR x.VisitorName LIKE @Like OR x.ContactNumber LIKE @Like OR x.Purpose LIKE @Like
             OR x.PersonToMeet LIKE @Like OR x.VehicleNumber LIKE @Like OR x.Address LIKE @Like
             OR x.IdNumber LIKE @Like OR x.BadgeNumber LIKE @Like);
END
