-- Replaces the inline LINQ in StudentIdCardRequestController.GetPaged
-- (GET /api/student-id-card-requests/paged). Flat single-table query,
-- scoped to one academic year.
CREATE OR ALTER PROCEDURE dbo.sp_StudentIdCardRequests_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.AcademicYearId, x.StudentId, x.CardNumber, x.CardTech, x.CardExpiryDate,
        x.ChipUid, x.MedicalData, x.RequestDate, x.Status, x.RequestedByUserId,
        x.ApprovedByUserId, x.ApprovedAt, x.PrintedAt, x.Notes, x.IssueReason,
        x.PhotoUrl, x.BloodGroup, x.Allergies, x.IsActive
    FROM StudentIdCardRequests x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@Term IS NULL
             OR x.CardNumber LIKE @Like OR x.ChipUid LIKE @Like OR x.Status LIKE @Like
             OR x.Notes LIKE @Like OR x.IssueReason LIKE @Like)
    ORDER BY x.RequestDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM StudentIdCardRequests x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@Term IS NULL
             OR x.CardNumber LIKE @Like OR x.ChipUid LIKE @Like OR x.Status LIKE @Like
             OR x.Notes LIKE @Like OR x.IssueReason LIKE @Like);
END
