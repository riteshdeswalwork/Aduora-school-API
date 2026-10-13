-- Replaces the inline LINQ in StudentBehaviorIncidentController.GetPaged
-- (GET /api/student-behavior-management/paged). Joins Student+User and
-- Class (matching the original's ApplyIncludes), scoped to one academic year.
CREATE OR ALTER PROCEDURE dbo.sp_StudentBehaviorIncidents_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @StudentId UNIQUEIDENTIFIER = NULL,
    @ClassId UNIQUEIDENTIFIER = NULL,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @SeverityIndex INT = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @ToDateExclusive DATETIME2 = CASE WHEN @ToDate IS NULL THEN NULL ELSE DATEADD(DAY, 1, CAST(@ToDate AS DATE)) END;

    SELECT x.Id
    INTO #Filtered
    FROM StudentBehaviorIncidents x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@StudentId IS NULL OR x.StudentId = @StudentId)
        AND (@ClassId IS NULL OR x.ClassId = @ClassId)
        AND (@FromDate IS NULL OR x.IncidentDate >= @FromDate)
        AND (@ToDateExclusive IS NULL OR x.IncidentDate < @ToDateExclusive)
        AND (@SeverityIndex IS NULL OR x.SeverityIndex = @SeverityIndex)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@Term IS NULL
             OR x.BehaviorType LIKE @Like OR x.Description LIKE @Like OR x.InterventionPlan LIKE @Like
             OR x.Status LIKE @Like OR x.LocationOfIncident LIKE @Like OR x.ActionTaken LIKE @Like
             OR x.Notes LIKE @Like);

    SELECT COUNT(*) AS TotalCount FROM #Filtered;

    SELECT
        x.Id, x.AcademicYearId, x.StudentId, x.ClassId, x.IncidentDate, x.BehaviorType,
        x.Archetype, x.SeverityIndex, x.Description, x.InterventionPlan, x.PsychReferral,
        x.ParentFeedbackCycle, x.FollowUpDate, x.Status, x.ReportedByUserId,
        x.LocationOfIncident, x.Witnesses, x.ActionTaken, x.ConsequenceType, x.ResolvedAt,
        x.Notes, x.IsActive, x.CreatedAt, x.UpdatedAt,
        stu.Id AS Stu_Id, stu.RollNumber AS Stu_RollNumber, u.FirstName AS Stu_FirstName,
        u.LastName AS Stu_LastName,
        c.Id AS Class_Id, c.Name AS Class_Name
    FROM #Filtered f
    INNER JOIN StudentBehaviorIncidents x ON x.Id = f.Id
    LEFT JOIN Students stu ON stu.Id = x.StudentId
    LEFT JOIN Users u ON u.Id = stu.UserId
    LEFT JOIN Classes c ON c.Id = x.ClassId
    ORDER BY x.IncidentDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #Filtered;
END
