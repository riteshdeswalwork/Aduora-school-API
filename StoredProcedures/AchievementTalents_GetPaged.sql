-- Replaces the inline LINQ in AchievementTalentController.GetPaged
-- (GET /api/achievements-talents/paged). Flat single-table query, scoped
-- to one academic year.
CREATE OR ALTER PROCEDURE dbo.sp_AchievementTalents_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(200) = NULL,
    @Type NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.AcademicYearId, x.StudentId, x.Title, x.Category, x.Level,
        x.AchievementDate, x.Description, x.AwardedBy, x.Type, x.EventName,
        x.Position, x.CertificateIssued, x.Remarks, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM AchievementTalents x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Type IS NULL OR LOWER(LTRIM(RTRIM(x.Type))) = LOWER(LTRIM(RTRIM(@Type))))
        AND (@Term IS NULL
             OR x.Title LIKE @Like OR x.Category LIKE @Like OR x.Level LIKE @Like
             OR x.Description LIKE @Like OR x.AwardedBy LIKE @Like OR x.EventName LIKE @Like
             OR x.Position LIKE @Like OR x.Remarks LIKE @Like)
    ORDER BY x.AchievementDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM AchievementTalents x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Type IS NULL OR LOWER(LTRIM(RTRIM(x.Type))) = LOWER(LTRIM(RTRIM(@Type))))
        AND (@Term IS NULL
             OR x.Title LIKE @Like OR x.Category LIKE @Like OR x.Level LIKE @Like
             OR x.Description LIKE @Like OR x.AwardedBy LIKE @Like OR x.EventName LIKE @Like
             OR x.Position LIKE @Like OR x.Remarks LIKE @Like);
END
