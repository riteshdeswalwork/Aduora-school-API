-- Replaces the inline LINQ in EnquiryController.GetPaged (GET /api/enquiries/paged).
-- Flat single-table query - no joins needed. Mirrors the original filters
-- (active/inactive, status, priority, source substring, has-follow-up,
-- free-text search across 10 fields) and EnquiryDate DESC sort exactly.
CREATE OR ALTER PROCEDURE dbo.sp_Enquiries_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @Priority NVARCHAR(50) = NULL,
    @Source NVARCHAR(100) = NULL,
    @HasFollowUp BIT = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @SourceLike NVARCHAR(104) = '%' + NULLIF(LTRIM(RTRIM(@Source)), '') + '%';

    -- Result set 1: the page of rows (matches ISqlDataAccess.QueryPagedStoredProcAsync's
    -- expected order - items first, then the count).
    SELECT
        x.Id, x.Name, x.ParentName, x.ContactNumber, x.Email, x.CurrentSchool,
        x.SeekingClass, x.CourseInterested, x.Source, x.Priority, x.Status,
        x.EnquiryDate, x.FollowUpDate, x.FollowUpLog, x.Notes, x.IsActive,
        x.CreatedAt, x.UpdatedAt
    FROM Enquiries x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(LTRIM(RTRIM(x.Status))) = LOWER(LTRIM(RTRIM(@Status))))
        AND (@Priority IS NULL OR LOWER(LTRIM(RTRIM(x.Priority))) = LOWER(LTRIM(RTRIM(@Priority))))
        AND (@Source IS NULL OR x.Source LIKE @SourceLike)
        AND (@HasFollowUp IS NULL OR @HasFollowUp = 0 OR x.FollowUpDate IS NOT NULL)
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.ParentName LIKE @Like OR x.ContactNumber LIKE @Like
             OR x.Email LIKE @Like OR x.CurrentSchool LIKE @Like OR x.SeekingClass LIKE @Like
             OR x.CourseInterested LIKE @Like OR x.Source LIKE @Like OR x.Status LIKE @Like
             OR x.Notes LIKE @Like)
    ORDER BY x.EnquiryDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Result set 2: total count (same filters, unpaged).
    SELECT COUNT(*) AS TotalCount
    FROM Enquiries x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(LTRIM(RTRIM(x.Status))) = LOWER(LTRIM(RTRIM(@Status))))
        AND (@Priority IS NULL OR LOWER(LTRIM(RTRIM(x.Priority))) = LOWER(LTRIM(RTRIM(@Priority))))
        AND (@Source IS NULL OR x.Source LIKE @SourceLike)
        AND (@HasFollowUp IS NULL OR @HasFollowUp = 0 OR x.FollowUpDate IS NOT NULL)
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.ParentName LIKE @Like OR x.ContactNumber LIKE @Like
             OR x.Email LIKE @Like OR x.CurrentSchool LIKE @Like OR x.SeekingClass LIKE @Like
             OR x.CourseInterested LIKE @Like OR x.Source LIKE @Like OR x.Status LIKE @Like
             OR x.Notes LIKE @Like);
END
