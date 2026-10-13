-- Replaces the inline LINQ in WorkAllocationController.GetPaged
-- (GET /api/work-allocation/paged). Flat single-table query. The original's
-- "assignedToUserId" filter does a sub-lookup (Staff.UserId -> Staff.Id);
-- replicated here as a scalar subquery.
CREATE OR ALTER PROCEDURE dbo.sp_WorkAllocations_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @AssignedToStaffId UNIQUEIDENTIFIER = NULL,
    @AssignedToUserId UNIQUEIDENTIFIER = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    -- Resolve assignedToUserId -> staff id (matches the original's lookup;
    -- "staffId == default" there meant "no such staff" -> zero rows).
    DECLARE @StaffIdFromUser UNIQUEIDENTIFIER = NULL;
    DECLARE @UserLookupFailed BIT = 0;
    IF @AssignedToUserId IS NOT NULL
    BEGIN
        SELECT @StaffIdFromUser = Id FROM Staff WHERE UserId = @AssignedToUserId;
        IF @StaffIdFromUser IS NULL SET @UserLookupFailed = 1;
    END

    SELECT
        x.Id, x.AssignedToStaffId, x.AssignedByUserId, x.ReviewerStaffId, x.Title,
        x.Description, x.EstimatedHours, x.ActualHours, x.StartTime, x.DueDate,
        x.CompletedAt, x.CompletionPercentage, x.Priority, x.Complexity, x.Status,
        x.ReviewerComments, x.AttachmentUrl, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM WorkAllocations x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(LTRIM(RTRIM(x.Status))) = LOWER(LTRIM(RTRIM(@Status))))
        AND (@AssignedToStaffId IS NULL OR x.AssignedToStaffId = @AssignedToStaffId)
        AND (@AssignedToUserId IS NULL OR (@UserLookupFailed = 0 AND x.AssignedToStaffId = @StaffIdFromUser))
        AND (@UserLookupFailed = 0)
        AND (@Term IS NULL
             OR x.Title LIKE @Like OR x.Description LIKE @Like OR x.Priority LIKE @Like
             OR x.Status LIKE @Like OR x.Complexity LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM WorkAllocations x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(LTRIM(RTRIM(x.Status))) = LOWER(LTRIM(RTRIM(@Status))))
        AND (@AssignedToStaffId IS NULL OR x.AssignedToStaffId = @AssignedToStaffId)
        AND (@AssignedToUserId IS NULL OR (@UserLookupFailed = 0 AND x.AssignedToStaffId = @StaffIdFromUser))
        AND (@UserLookupFailed = 0)
        AND (@Term IS NULL
             OR x.Title LIKE @Like OR x.Description LIKE @Like OR x.Priority LIKE @Like
             OR x.Status LIKE @Like OR x.Complexity LIKE @Like);
END
