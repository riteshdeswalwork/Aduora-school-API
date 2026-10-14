-- Paged replacement for LibraryManager.GetAllLibraryFinesAsync (GET /api/library/fines),
-- exposed as GET /api/library/fines/paged. Mirrors the original EF query:
--   LibraryFines
--     .Include(LibraryMember).ThenInclude(User)      -> INNER JOINs (required navs)
--     .Include(BookIssue).ThenInclude(Book)          -> LEFT JOINs (BookIssueId is nullable)
--     .OrderByDescending(FineDate)
-- Returns raw columns only; the C# side rebuilds a LibraryFine entity graph and reuses
-- MapLibraryFineToDTO so the entity's computed properties (IsOverdue, StatusColor,
-- PaymentStatus, NeedsAttention, ...) are not duplicated here.
-- Note: the table also has EF shadow columns BookIssueId1/LibraryMemberId1; the
-- [ForeignKey]-attributed navigations used by the original Include map to
-- LibraryMemberId/BookIssueId, so those are the join keys here.
CREATE OR ALTER PROCEDURE dbo.sp_LibraryFines_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @LibraryMemberId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @StatusNorm NVARCHAR(50) = LOWER(NULLIF(LTRIM(RTRIM(@Status)), ''));

    -- Result set 1: the page of rows (matches ISqlDataAccess.QueryPagedStoredProcAsync's
    -- expected order - items first, then the count).
    SELECT
        f.Id, f.LibraryMemberId, f.AcademicYearId, f.BookIssueId,
        f.FineNumber, f.FineType, f.Amount, f.PaidAmount, f.PendingAmount, f.Status,
        f.FineDate, f.DueDate, f.PaidDate, f.Description, f.Reason, f.OverdueDays,
        f.IsWaived, f.WaiverReason, f.IsAppealed, f.AppealStatus,
        f.CreatedByUserId, f.CreatedAt, f.UpdatedAt,
        m.UserId     AS MemberUserId,
        u.FirstName  AS MemberFirstName,
        u.LastName   AS MemberLastName,
        bi.BookId    AS BookIssueBookId,
        b.Title      AS BookTitle
    FROM LibraryFines f
    INNER JOIN LibraryMembers m ON m.Id = f.LibraryMemberId
    INNER JOIN Users u ON u.Id = m.UserId
    LEFT JOIN BookIssues bi ON bi.Id = f.BookIssueId
    LEFT JOIN Books b ON b.Id = bi.BookId
    WHERE
        (@LibraryMemberId IS NULL OR f.LibraryMemberId = @LibraryMemberId)
        AND (@StatusNorm IS NULL OR LOWER(LTRIM(RTRIM(f.Status))) = @StatusNorm)
        AND (@Term IS NULL
             OR f.FineNumber LIKE @Like OR f.FineType LIKE @Like
             OR f.Description LIKE @Like OR f.Reason LIKE @Like)
    ORDER BY f.FineDate DESC, f.Id
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Result set 2: total count (same joins + filters, unpaged).
    SELECT COUNT(*) AS TotalCount
    FROM LibraryFines f
    INNER JOIN LibraryMembers m ON m.Id = f.LibraryMemberId
    INNER JOIN Users u ON u.Id = m.UserId
    WHERE
        (@LibraryMemberId IS NULL OR f.LibraryMemberId = @LibraryMemberId)
        AND (@StatusNorm IS NULL OR LOWER(LTRIM(RTRIM(f.Status))) = @StatusNorm)
        AND (@Term IS NULL
             OR f.FineNumber LIKE @Like OR f.FineType LIKE @Like
             OR f.Description LIKE @Like OR f.Reason LIKE @Like);
END
