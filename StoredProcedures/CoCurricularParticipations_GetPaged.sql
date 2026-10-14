-- Replaces the inline LINQ in CoCurricularParticipationController.GetPaged
-- (GET /api/co-curricular-participation/paged). Flat table, scoped to one
-- academic year.
CREATE OR ALTER PROCEDURE dbo.sp_CoCurricularParticipations_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @ActivityId UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.AcademicYearId, x.CoCurricularActivityId, x.StudentId, x.Role,
        x.Outcome, x.Remarks, x.PointsAwarded, x.AchievementDate, x.CertificateIssued,
        x.IsActive, x.CreatedAt
    FROM CoCurricularParticipations x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@ActivityId IS NULL OR x.CoCurricularActivityId = @ActivityId)
        AND (@Term IS NULL OR x.Role LIKE @Like OR x.Outcome LIKE @Like OR x.Remarks LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM CoCurricularParticipations x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@ActivityId IS NULL OR x.CoCurricularActivityId = @ActivityId)
        AND (@Term IS NULL OR x.Role LIKE @Like OR x.Outcome LIKE @Like OR x.Remarks LIKE @Like);
END
