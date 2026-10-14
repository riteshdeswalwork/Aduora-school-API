-- Replaces the inline LINQ in InviteeController.GetPaged (GET /api/invitees/paged).
CREATE OR ALTER PROCEDURE dbo.sp_Invitees_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @EventId UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @RsvpStatus NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.EventId, x.Name, x.Email, x.ContactNumber, x.Role, x.RsvpStatus,
        x.Notes, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM Invitees x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@EventId IS NULL OR x.EventId = @EventId)
        AND (@RsvpStatus IS NULL OR LOWER(x.RsvpStatus) = LOWER(@RsvpStatus))
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.Email LIKE @Like OR x.ContactNumber LIKE @Like
             OR x.Role LIKE @Like OR x.RsvpStatus LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM Invitees x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@EventId IS NULL OR x.EventId = @EventId)
        AND (@RsvpStatus IS NULL OR LOWER(x.RsvpStatus) = LOWER(@RsvpStatus))
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.Email LIKE @Like OR x.ContactNumber LIKE @Like
             OR x.Role LIKE @Like OR x.RsvpStatus LIKE @Like);
END
