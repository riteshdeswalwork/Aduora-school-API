-- Replaces the inline LINQ in PtmFeedbackController.GetPaged
-- (GET /api/ptm-feedback/paged). Flat single-table query, scoped to one
-- academic year.
CREATE OR ALTER PROCEDURE dbo.sp_PtmFeedbacks_GetPaged
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
        x.Id, x.AcademicYearId, x.TeacherStaffId, x.StudentId, x.FeedbackDate,
        x.Rating, x.Comments, x.ParentName, x.MeetingTopic, x.SpecificStrengths,
        x.AreasForImprovement, x.IsActive, x.CreatedAt
    FROM PtmFeedbacks x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Term IS NULL
             OR x.ParentName LIKE @Like OR x.Comments LIKE @Like OR x.MeetingTopic LIKE @Like
             OR x.SpecificStrengths LIKE @Like OR x.AreasForImprovement LIKE @Like)
    ORDER BY x.FeedbackDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM PtmFeedbacks x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Term IS NULL
             OR x.ParentName LIKE @Like OR x.Comments LIKE @Like OR x.MeetingTopic LIKE @Like
             OR x.SpecificStrengths LIKE @Like OR x.AreasForImprovement LIKE @Like);
END
