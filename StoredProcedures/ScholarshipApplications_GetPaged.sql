-- Replaces the inline LINQ in ScholarshipApplicationController.GetPaged
-- (GET /api/scholarship-applications/paged). Flat table, scoped to one
-- academic year. (No search term in the original - just scholarship/status filters.)
CREATE OR ALTER PROCEDURE dbo.sp_ScholarshipApplications_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @ScholarshipId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        x.Id, x.AcademicYearId, x.ScholarshipId, x.StudentId, x.AppliedOn, x.Status,
        x.Remarks, x.ApprovedByUserId, x.ApprovedAt, x.DisbursedAmount, x.DisbursedAt,
        x.IsActive
    FROM ScholarshipApplications x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@ScholarshipId IS NULL OR x.ScholarshipId = @ScholarshipId)
        AND (@Status IS NULL OR x.Status = @Status)
    ORDER BY x.AppliedOn DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM ScholarshipApplications x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@ScholarshipId IS NULL OR x.ScholarshipId = @ScholarshipId)
        AND (@Status IS NULL OR x.Status = @Status);
END
