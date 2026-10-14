-- New paged counterpart to LibraryManager.GetAllLibraryMembersAsync (EF LINQ,
-- unpaginated) for GET /api/library/members/paged. Mirrors the original's
-- INNER join to Users (required .Include(m => m.User)), active-only filter
-- (overridable via @IncludeInactive) and sort (MembershipNumber ASC; Id is a
-- deterministic tie-breaker for stable paging). Adds optional MemberType and
-- search (MembershipNumber / user first, last or full name) filters.
-- @AcademicYearId reproduces EF's global query filter on LibraryMember and
-- BookIssue (CurrentAcademicYearId from the X-Academic-Year-Id header; NULL =
-- no filter), which the original route gets implicitly.
-- Three result sets, read in this order by GetLibraryMembersPagedAsync:
--   (1) total count of the filtered (unpaged) set,
--   (2) the page of member rows + joined user first/last name,
--   (3) the raw BookIssues fields (IsActive, IsReturned, DueDate) for just that
--       page's members (no IsActive/IsReturned filter - the entity's computed
--       properties do that). Grouped back onto each member in C# so the
--       entity's own computed HasOverdueBooks etc. do the logic.
--       NOTE: joined on the real FK BookIssues.LibraryMemberId. The EF model
--       maps LibraryMember.BookIssues to a separate shadow FK column
--       (LibraryMemberId1, never populated by app code) because the
--       BookIssue->LibraryMember relationship is configured .WithMany() with no
--       inverse, so the original route's HasOverdueBooks is effectively always
--       false. This proc intentionally uses the real FK.
CREATE OR ALTER PROCEDURE dbo.sp_LibraryMembers_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @MemberType NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0,
    @AcademicYearId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @Type NVARCHAR(50) = NULLIF(LTRIM(RTRIM(@MemberType)), '');

    SELECT lm.Id, lm.MembershipNumber
    INTO #Filtered
    FROM LibraryMembers lm
    INNER JOIN Users u ON u.Id = lm.UserId
    WHERE
        (@IncludeInactive = 1 OR lm.IsActive = 1)
        AND (@AcademicYearId IS NULL OR lm.AcademicYearId = @AcademicYearId)
        AND (@Type IS NULL OR lm.MemberType = @Type)
        AND (@Term IS NULL
             OR lm.MembershipNumber LIKE @Like
             OR u.FirstName LIKE @Like
             OR u.LastName LIKE @Like
             OR (COALESCE(u.FirstName, '') + ' ' + COALESCE(u.LastName, '')) LIKE @Like);

    SELECT COUNT(*) AS TotalCount FROM #Filtered;

    SELECT f.Id,
           ROW_NUMBER() OVER (ORDER BY f.MembershipNumber ASC, f.Id ASC) AS RowNum
    INTO #Page
    FROM #Filtered f
    ORDER BY f.MembershipNumber ASC, f.Id ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT
        lm.Id, lm.UserId, lm.AcademicYearId, lm.MembershipNumber, lm.MemberType,
        lm.MembershipStartDate, lm.MembershipEndDate, lm.IsActive, lm.IsSuspended,
        lm.SuspensionReason, lm.SuspensionStartDate, lm.SuspensionEndDate,
        lm.MaxBooksAllowed, lm.MaxIssueDays, lm.MaxRenewalCount,
        lm.CurrentBooksIssued, lm.TotalBooksIssued,
        lm.TotalFineAmount, lm.PaidFineAmount, lm.PendingFineAmount,
        lm.Address, lm.Phone, lm.Email, lm.EmergencyContact, lm.EmergencyPhone,
        lm.PreferredCategories, lm.PreferredAuthors, lm.EmailNotifications, lm.SMSNotifications,
        lm.SecurityDeposit, lm.DepositAmount, lm.DepositDate, lm.DepositRefundStatus,
        lm.PhotoUrl, lm.IdProofUrl, lm.IdProofType, lm.IdProofNumber,
        lm.CreatedByUserId, lm.CreatedAt, lm.UpdatedAt,
        u.FirstName AS User_FirstName, u.LastName AS User_LastName
    FROM #Page p
    INNER JOIN LibraryMembers lm ON lm.Id = p.Id
    INNER JOIN Users u ON u.Id = lm.UserId
    ORDER BY p.RowNum;

    -- Just enough of each BookIssue for the entity's computed properties.
    SELECT bi.LibraryMemberId, bi.IsActive, bi.IsReturned, bi.DueDate
    FROM BookIssues bi
    WHERE bi.LibraryMemberId IN (SELECT Id FROM #Page)
        AND (@AcademicYearId IS NULL OR bi.AcademicYearId = @AcademicYearId);

    DROP TABLE #Page;
    DROP TABLE #Filtered;
END
