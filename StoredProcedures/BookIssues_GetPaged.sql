-- Paged replacement for LibraryManager.GetAllBookIssuesAsync (new route GET /api/library/issues/paged;
-- the original unpaged GET /api/library/issues is left untouched).
-- Returns RAW BookIssue columns plus the joined Book / LibraryMember / member User / issuer User
-- columns needed to rebuild the BookIssue entity graph in C#. All computed properties
-- (IsOverdue, Status, CanBeRenewed, NeedsAttention, ...) stay in C# on the entity - not replicated here.
--
-- Mirrors the EF query semantics:
--   * BookIssues.IsActive = 1
--   * academic-year global query filter on BookIssues AND LibraryMembers (EF inner-joins the
--     required LibraryMember navigation through its own filter) when @AcademicYearId is supplied
--   * Book / LibraryMember / IssuedBy are required navigations -> INNER JOIN
--   * ORDER BY IssueDate DESC
CREATE OR ALTER PROCEDURE dbo.sp_BookIssues_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @LibraryMemberId UNIQUEIDENTIFIER = NULL,
    @IsReturned BIT = NULL,
    @AcademicYearId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT bi.Id, bi.IssueDate
    INTO #Filtered
    FROM BookIssues bi
    INNER JOIN Books b ON b.Id = bi.BookId
    INNER JOIN LibraryMembers lm ON lm.Id = bi.LibraryMemberId
    INNER JOIN Users mu ON mu.Id = lm.UserId
    INNER JOIN Users iu ON iu.Id = bi.IssuedByUserId
    WHERE
        bi.IsActive = 1
        AND (@AcademicYearId IS NULL OR (bi.AcademicYearId = @AcademicYearId AND lm.AcademicYearId = @AcademicYearId))
        AND (@LibraryMemberId IS NULL OR bi.LibraryMemberId = @LibraryMemberId)
        AND (@IsReturned IS NULL OR bi.IsReturned = @IsReturned)
        AND (@Term IS NULL
             OR b.Title LIKE @Like
             OR b.ISBN LIKE @Like
             OR bi.IssueNumber LIKE @Like
             OR lm.MembershipNumber LIKE @Like
             OR (COALESCE(mu.FirstName, '') + ' ' + COALESCE(mu.LastName, '')) LIKE @Like);

    -- Result set 1: the page of rows (items first, then the count - matches
    -- ISqlDataAccess.QueryPagedStoredProcAsync's expected order).
    SELECT
        bi.Id, bi.BookId, bi.LibraryMemberId, bi.AcademicYearId, bi.IssueNumber,
        bi.IssueDate, bi.DueDate, bi.ReturnDate, bi.IsReturned, bi.IsActive,
        bi.RenewalCount, bi.HasFine, bi.FineAmount, bi.PaidFineAmount, bi.PendingFineAmount,
        bi.FineReason, bi.IssueCondition, bi.ReturnCondition, bi.IsLost, bi.IsDamaged,
        bi.IssuedByUserId, bi.CreatedAt, bi.UpdatedAt,
        b.Title AS BookTitle, b.ISBN AS BookISBN,
        lm.UserId AS MemberUserId, lm.MembershipNumber, lm.IsActive AS MemberIsActive,
        lm.IsSuspended AS MemberIsSuspended, lm.MaxBooksAllowed AS MemberMaxBooksAllowed,
        lm.MaxIssueDays AS MemberMaxIssueDays, lm.MaxRenewalCount AS MemberMaxRenewalCount,
        lm.CurrentBooksIssued AS MemberCurrentBooksIssued, lm.PendingFineAmount AS MemberPendingFineAmount,
        mu.FirstName AS MemberFirstName, mu.LastName AS MemberLastName,
        iu.FirstName AS IssuedByFirstName, iu.LastName AS IssuedByLastName
    FROM #Filtered f
    INNER JOIN BookIssues bi ON bi.Id = f.Id
    INNER JOIN Books b ON b.Id = bi.BookId
    INNER JOIN LibraryMembers lm ON lm.Id = bi.LibraryMemberId
    INNER JOIN Users mu ON mu.Id = lm.UserId
    INNER JOIN Users iu ON iu.Id = bi.IssuedByUserId
    ORDER BY f.IssueDate DESC, f.Id
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Result set 2: total count (same filters, unpaged).
    SELECT COUNT(*) AS TotalCount FROM #Filtered;

    DROP TABLE #Filtered;
END
