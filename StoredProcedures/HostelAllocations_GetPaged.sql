-- Replaces the inline LINQ in HostelAllocationController.GetPaged
-- (GET /api/hostel-allocations/paged). Scoped to one academic year.
CREATE OR ALTER PROCEDURE dbo.sp_HostelAllocations_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @HostelRoomId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        x.Id, x.AcademicYearId, x.HostelRoomId, x.StudentId, x.FromDate, x.ToDate,
        x.Status, x.SecurityDeposit, x.Notes, x.IsActive
    FROM HostelAllocations x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@HostelRoomId IS NULL OR x.HostelRoomId = @HostelRoomId)
        AND (@Status IS NULL OR x.Status = @Status)
    ORDER BY x.FromDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM HostelAllocations x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@HostelRoomId IS NULL OR x.HostelRoomId = @HostelRoomId)
        AND (@Status IS NULL OR x.Status = @Status);
END
