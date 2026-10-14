-- Replaces the inline LINQ in ScheduledServiceController.GetPaged
-- (GET /api/scheduled-services/paged).
CREATE OR ALTER PROCEDURE dbo.sp_ScheduledServices_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @Category NVARCHAR(100) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @CategoryLike NVARCHAR(104) = '%' + NULLIF(LTRIM(RTRIM(@Category)), '') + '%';

    SELECT
        x.Id, x.Title, x.Category, x.AssignedToStaffId, x.ScheduledFor, x.Frequency,
        x.Status, x.Notes, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM ScheduledServices x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Category IS NULL OR x.Category LIKE @CategoryLike)
        AND (@Term IS NULL
             OR x.Title LIKE @Like OR x.Category LIKE @Like OR x.Notes LIKE @Like
             OR x.Frequency LIKE @Like OR x.Status LIKE @Like)
    ORDER BY x.ScheduledFor DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM ScheduledServices x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(x.Status) = LOWER(@Status))
        AND (@Category IS NULL OR x.Category LIKE @CategoryLike)
        AND (@Term IS NULL
             OR x.Title LIKE @Like OR x.Category LIKE @Like OR x.Notes LIKE @Like
             OR x.Frequency LIKE @Like OR x.Status LIKE @Like);
END
