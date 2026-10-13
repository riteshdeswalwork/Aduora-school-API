-- Replaces the inline LINQ in DisciplineIncidentController.GetPaged
-- (GET /api/discipline-management/paged). Flat single-table query, scoped
-- to one academic year.
CREATE OR ALTER PROCEDURE dbo.sp_DisciplineIncidents_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.AcademicYearId, x.StudentId, x.IncidentDate, x.IncidentType,
        x.IncidentLocation, x.Severity, x.Description, x.WitnessJson, x.ActionTaken,
        x.ActionPlan, x.Punishment, x.FollowUpDate, x.Status, x.IsActive,
        x.CreatedAt, x.UpdatedAt
    FROM DisciplineIncidents x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@Term IS NULL
             OR x.IncidentType LIKE @Like OR x.Description LIKE @Like
             OR x.ActionTaken LIKE @Like OR x.Status LIKE @Like)
    ORDER BY x.IncidentDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM DisciplineIncidents x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@Term IS NULL
             OR x.IncidentType LIKE @Like OR x.Description LIKE @Like
             OR x.ActionTaken LIKE @Like OR x.Status LIKE @Like);
END
