-- Replaces the inline LINQ in CoCurricularActivityController.GetPaged
-- (GET /api/co-curricular-activities/paged). Flat table, scoped to one
-- academic year.
CREATE OR ALTER PROCEDURE dbo.sp_CoCurricularActivities_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.AcademicYearId, x.Name, x.Category, x.ActivityDate, x.Description,
        x.Organizer, x.Venue, x.TotalParticipants, x.BudgetAllocated, x.Status,
        x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM CoCurricularActivities x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Term IS NULL OR x.Name LIKE @Like OR x.Category LIKE @Like OR x.Description LIKE @Like OR x.Organizer LIKE @Like)
    ORDER BY x.ActivityDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM CoCurricularActivities x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Term IS NULL OR x.Name LIKE @Like OR x.Category LIKE @Like OR x.Description LIKE @Like OR x.Organizer LIKE @Like);
END
