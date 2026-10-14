-- Paged replacement for LibraryManager.GetAllBookReservationsAsync (EF LINQ, unpaged)
-- for GET /api/library/reservations/paged.
-- Returns raw BookReservations columns plus the joined bits MapBookReservationToDTO
-- needs (Book Title/IsAvailable, member User First/LastName, fulfilled-by User
-- First/LastName). All computed properties (IsExpired, StatusColor, QueuePosition,
-- CanBeFulfilled, ...) stay in C# on the reconstructed BookReservation entity.
-- Default filter mirrors the original (Status <> 'Deleted'); ReservationDate DESC sort.
-- @AcademicYearId mirrors the EF global query filter on BookReservation/LibraryMember
-- (NULL = no year filter, same as CurrentAcademicYearId == null in EF).
-- Join types mirror EF's Include semantics: Book / LibraryMember / member User are
-- required navigations (INNER JOIN), FulfilledBy is optional (LEFT JOIN).
CREATE OR ALTER PROCEDURE dbo.sp_BookReservations_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @LibraryMemberId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @AcademicYearId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @StatusTrim NVARCHAR(50) = NULLIF(LTRIM(RTRIM(@Status)), '');

    -- Result set 1: the page of rows (matches ISqlDataAccess.QueryPagedStoredProcAsync's
    -- expected order - items first, then the count).
    SELECT
        r.Id, r.BookId, r.LibraryMemberId, r.AcademicYearId, r.ReservationNumber,
        r.ReservationDate, r.ExpiryDate, r.Status, r.Priority,
        r.NotificationSentDate, r.IsNotificationSent, r.NotificationMethod,
        r.FulfilledDate, r.FulfilledByUserId, r.BookIssueId, r.FulfillmentNotes,
        r.CancelledDate, r.CancelledByUserId, r.CancellationReason, r.CancellationType,
        r.ReservationNotes, r.RequestedEdition, r.SpecialRequirements,
        r.AutoExtend, r.ExtensionCount, r.MaxExtensions,
        r.PickupDeadline, r.IsPickupReminderSent, r.PickupReminderSentDate,
        r.IsDigitalReservation, r.DigitalAccessGrantedDate,
        r.CreatedByUserId, r.CreatedAt, r.UpdatedAt,
        b.Title AS JoinedBookTitle, b.IsAvailable AS BookIsAvailable,
        mu.Id AS MemberUserId, mu.FirstName AS MemberFirstName, mu.LastName AS MemberLastName,
        fu.FirstName AS FulfilledByFirstName, fu.LastName AS FulfilledByLastName
    FROM BookReservations r
    INNER JOIN Books b ON b.Id = r.BookId
    INNER JOIN LibraryMembers m ON m.Id = r.LibraryMemberId
        AND (@AcademicYearId IS NULL OR m.AcademicYearId = @AcademicYearId)
    INNER JOIN Users mu ON mu.Id = m.UserId
    LEFT JOIN Users fu ON fu.Id = r.FulfilledByUserId
    WHERE
        (@AcademicYearId IS NULL OR r.AcademicYearId = @AcademicYearId)
        AND ((@StatusTrim IS NULL AND r.Status <> 'Deleted') OR r.Status = @StatusTrim)
        AND (@LibraryMemberId IS NULL OR r.LibraryMemberId = @LibraryMemberId)
        AND (@Term IS NULL
             OR r.ReservationNumber LIKE @Like
             OR b.Title LIKE @Like
             OR mu.FirstName LIKE @Like OR mu.LastName LIKE @Like
             OR (mu.FirstName + ' ' + mu.LastName) LIKE @Like)
    ORDER BY r.ReservationDate DESC, r.Id
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Result set 2: total count (same filters, unpaged).
    SELECT COUNT(*) AS TotalCount
    FROM BookReservations r
    INNER JOIN Books b ON b.Id = r.BookId
    INNER JOIN LibraryMembers m ON m.Id = r.LibraryMemberId
        AND (@AcademicYearId IS NULL OR m.AcademicYearId = @AcademicYearId)
    INNER JOIN Users mu ON mu.Id = m.UserId
    WHERE
        (@AcademicYearId IS NULL OR r.AcademicYearId = @AcademicYearId)
        AND ((@StatusTrim IS NULL AND r.Status <> 'Deleted') OR r.Status = @StatusTrim)
        AND (@LibraryMemberId IS NULL OR r.LibraryMemberId = @LibraryMemberId)
        AND (@Term IS NULL
             OR r.ReservationNumber LIKE @Like
             OR b.Title LIKE @Like
             OR mu.FirstName LIKE @Like OR mu.LastName LIKE @Like
             OR (mu.FirstName + ' ' + mu.LastName) LIKE @Like);
END
