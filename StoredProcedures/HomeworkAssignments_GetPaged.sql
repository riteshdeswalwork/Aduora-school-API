-- Replaces the inline LINQ in HomeworkAssignmentController.GetPaged
-- (GET /api/homework-assignments/paged). Flat single-table query, with an
-- allowed-class-ids data-scope restriction (GuidListType TVP) and a "tab"
-- grouping (Active = Draft/Published, History = Closed/Evaluated).
CREATE OR ALTER PROCEDURE dbo.sp_HomeworkAssignments_GetPaged
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
            CAST(NULL AS NVARCHAR(200)) AS Title, CAST(NULL AS NVARCHAR(MAX)) AS Description,
            CAST(NULL AS DATETIME2) AS IssuedDate, CAST(NULL AS DATETIME2) AS DueDate,
            CAST(NULL AS NVARCHAR(500)) AS AttachmentUrl, CAST(NULL AS UNIQUEIDENTIFIER) AS AssignedById,
            CAST(NULL AS INT) AS MaxMarks, CAST(NULL AS NVARCHAR(MAX)) AS InstructionsForParents,
            CAST(NULL AS NVARCHAR(50)) AS Status, CAST(NULL AS BIT) AS IsActive,
            CAST(NULL AS DATETIME2) AS CreatedAt, CAST(NULL AS DATETIME2) AS UpdatedAt;
        SELECT 0 AS TotalCount;
        RETURN;
    END

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @IsActiveTab BIT = CASE WHEN @Tab = 'Active' THEN 1 ELSE 0 END;
    DECLARE @IsHistoryTab BIT = CASE WHEN @Tab = 'History' THEN 1 ELSE 0 END;

    SELECT
        x.Id, x.AcademicYearId, x.ClassId, x.SubjectId, x.Title, x.Description,
        x.IssuedDate, x.DueDate, x.AttachmentUrl, x.AssignedById, x.MaxMarks,
        x.InstructionsForParents, x.Status, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM HomeworkAssignments x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@HasAllowedClassFilter = 0 OR (x.ClassId IS NOT NULL AND EXISTS (SELECT 1 FROM @AllowedClassIds a WHERE a.Id = x.ClassId)))
        AND (@Tab IS NULL
             OR (@IsActiveTab = 1 AND (x.Status = 'Draft' OR x.Status = 'Published'))
             OR (@IsHistoryTab = 1 AND (x.Status = 'Closed' OR x.Status = 'Evaluated'))
             OR (@IsActiveTab = 0 AND @IsHistoryTab = 0))
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Term IS NULL OR x.Title LIKE @Like OR x.Description LIKE @Like OR x.Status LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM HomeworkAssignments x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@HasAllowedClassFilter = 0 OR (x.ClassId IS NOT NULL AND EXISTS (SELECT 1 FROM @AllowedClassIds a WHERE a.Id = x.ClassId)))
        AND (@Tab IS NULL
             OR (@IsActiveTab = 1 AND (x.Status = 'Draft' OR x.Status = 'Published'))
             OR (@IsHistoryTab = 1 AND (x.Status = 'Closed' OR x.Status = 'Evaluated'))
             OR (@IsActiveTab = 0 AND @IsHistoryTab = 0))
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Term IS NULL OR x.Title LIKE @Like OR x.Description LIKE @Like OR x.Status LIKE @Like);
END
