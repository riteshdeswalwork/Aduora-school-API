-- Replaces StudentManager.GetPagedAsync (EF LINQ) for the Students "paged" list
-- (GET /api/students/paged) - the single highest-traffic query in the app.
-- Mirrors the exact filtering/sort/paging behavior of the original C# method:
--   * optional @AcademicYearId: students enrolled that year, OR students with
--     no enrollment row at all (legacy/imported) - but ONLY applied when at
--     least one student is actually enrolled that year (matches original
--     "if enrolledStudentIds.Count > 0" guard).
--   * optional @AllowedStudentIds (data-scope restriction): when
--     @HasAllowedFilter = 1 and the TVP is empty, returns zero rows (matches
--     the original "idList.Count == 0 => return empty" short-circuit).
--   * optional @SearchTerm across name/email/username/registration/roll/admission no.
-- Result set 1: the page of rows. Result set 2: a single row with the total count.
CREATE OR ALTER PROCEDURE dbo.sp_Students_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @AcademicYearId UNIQUEIDENTIFIER = NULL,
    @HasAllowedFilter BIT = 0,
    @AllowedStudentIds dbo.GuidListType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Data-scope restriction with an empty allow-list means "no access to anyone".
    IF @HasAllowedFilter = 1 AND NOT EXISTS (SELECT 1 FROM @AllowedStudentIds)
    BEGIN
        SELECT TOP (0)
            CAST(NULL AS UNIQUEIDENTIFIER) AS Id, CAST(NULL AS NVARCHAR(200)) AS FirstName,
            CAST(NULL AS NVARCHAR(200)) AS LastName,
            CAST(NULL AS NVARCHAR(100)) AS RollNumber, CAST(NULL AS NVARCHAR(100)) AS RegistrationNumber,
            CAST(NULL AS NVARCHAR(200)) AS ClassName, CAST(NULL AS INT) AS Status,
            CAST(NULL AS VARBINARY(MAX)) AS ProfilePhotoData;
        SELECT 0 AS TotalCount;
        RETURN;
    END

    DECLARE @EnrolledCount INT = 0;
    IF @AcademicYearId IS NOT NULL
        SELECT @EnrolledCount = COUNT(DISTINCT StudentId)
        FROM ClassEnrollments
        WHERE AcademicYearId = @AcademicYearId AND IsActive = 1;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    -- Materialized (not a CTE) so it can be queried twice: once for the page,
    -- once for the total count.
    SELECT s.Id, s.RegistrationNumber, s.RollNumber, s.AdmissionNumber, s.CurrentGrade,
           s.Status, s.ClassId, s.UserId,
           u.FirstName, u.LastName, u.Email, u.Username, u.ProfilePhotoData,
           c.Name AS ClassNameRaw
    INTO #Eligible
    FROM Students s
    LEFT JOIN Users u ON u.Id = s.UserId
    LEFT JOIN Classes c ON c.Id = s.ClassId
    WHERE
        -- Academic-year filter only kicks in when someone is actually enrolled that year,
        -- matching the original method's fallback-to-unfiltered behavior otherwise.
        (@AcademicYearId IS NULL OR @EnrolledCount = 0 OR EXISTS (
            SELECT 1 FROM ClassEnrollments ce
            WHERE ce.StudentId = s.Id AND ce.AcademicYearId = @AcademicYearId AND ce.IsActive = 1
        ) OR NOT EXISTS (
            SELECT 1 FROM ClassEnrollments ce2 WHERE ce2.StudentId = s.Id AND ce2.IsActive = 1
        ))
        AND (@HasAllowedFilter = 0 OR EXISTS (SELECT 1 FROM @AllowedStudentIds a WHERE a.Id = s.Id))
        AND (@Term IS NULL
             OR u.FirstName LIKE @Like OR u.LastName LIKE @Like
             OR u.Email LIKE @Like OR u.Username LIKE @Like
             OR s.RegistrationNumber LIKE @Like OR s.RollNumber LIKE @Like
             OR s.AdmissionNumber LIKE @Like);

    SELECT
        Id,
        FirstName,
        LastName,
        RollNumber,
        RegistrationNumber,
        COALESCE(ClassNameRaw, CurrentGrade, 'No Class') AS ClassName,
        Status,
        ProfilePhotoData
    FROM #Eligible
    ORDER BY FirstName, LastName
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount FROM #Eligible;

    DROP TABLE #Eligible;
END
