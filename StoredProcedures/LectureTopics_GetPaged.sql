-- Replaces the inline LINQ in LectureTopicController.GetPaged
-- (GET /api/lecture-topics/paged). Same pattern as HomeworkAssignments -
-- academic-year scoped, data-scope allowed-class filter (TVP), a "tab"
-- grouping (Planned = Planned/Postponed, Covered = Covered).
CREATE OR ALTER PROCEDURE dbo.sp_LectureTopics_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @Tab NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0,
    @HasAllowedClassFilter BIT = 0,
    @AllowedClassIds dbo.GuidListType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    IF @HasAllowedClassFilter = 1 AND NOT EXISTS (SELECT 1 FROM @AllowedClassIds)
    BEGIN
        SELECT TOP (0)
            CAST(NULL AS UNIQUEIDENTIFIER) AS Id, CAST(NULL AS UNIQUEIDENTIFIER) AS AcademicYearId,
            CAST(NULL AS UNIQUEIDENTIFIER) AS ClassId, CAST(NULL AS UNIQUEIDENTIFIER) AS SubjectId,
            CAST(NULL AS DATETIME2) AS LectureDate, CAST(NULL AS NVARCHAR(200)) AS Topic,
            CAST(NULL AS NVARCHAR(100)) AS ChapterOrUnit, CAST(NULL AS NVARCHAR(MAX)) AS Details,
            CAST(NULL AS NVARCHAR(500)) AS NotesUrl, CAST(NULL AS NVARCHAR(50)) AS Status,
            CAST(NULL AS BIT) AS IsActive, CAST(NULL AS DATETIME2) AS CreatedAt, CAST(NULL AS DATETIME2) AS UpdatedAt;
        SELECT 0 AS TotalCount;
        RETURN;
    END

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @IsPlannedTab BIT = CASE WHEN @Tab = 'Planned' THEN 1 ELSE 0 END;
    DECLARE @IsCoveredTab BIT = CASE WHEN @Tab = 'Covered' THEN 1 ELSE 0 END;

    SELECT
        x.Id, x.AcademicYearId, x.ClassId, x.SubjectId, x.LectureDate, x.Topic,
        x.ChapterOrUnit, x.Details, x.NotesUrl, x.Status, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM LectureTopics x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@HasAllowedClassFilter = 0 OR (x.ClassId IS NOT NULL AND EXISTS (SELECT 1 FROM @AllowedClassIds a WHERE a.Id = x.ClassId)))
        AND (@Tab IS NULL
             OR (@IsPlannedTab = 1 AND (x.Status = 'Planned' OR x.Status = 'Postponed'))
             OR (@IsCoveredTab = 1 AND x.Status = 'Covered')
             OR (@IsPlannedTab = 0 AND @IsCoveredTab = 0))
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Term IS NULL OR x.Topic LIKE @Like OR x.Details LIKE @Like OR x.ChapterOrUnit LIKE @Like)
    ORDER BY x.LectureDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM LectureTopics x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@HasAllowedClassFilter = 0 OR (x.ClassId IS NOT NULL AND EXISTS (SELECT 1 FROM @AllowedClassIds a WHERE a.Id = x.ClassId)))
        AND (@Tab IS NULL
             OR (@IsPlannedTab = 1 AND (x.Status = 'Planned' OR x.Status = 'Postponed'))
             OR (@IsCoveredTab = 1 AND x.Status = 'Covered')
             OR (@IsPlannedTab = 0 AND @IsCoveredTab = 0))
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Term IS NULL OR x.Topic LIKE @Like OR x.Details LIKE @Like OR x.ChapterOrUnit LIKE @Like);
END
