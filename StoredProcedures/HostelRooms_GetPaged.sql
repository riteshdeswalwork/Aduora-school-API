-- Replaces the inline LINQ in HostelRoomController.GetPaged (GET /api/hostel-rooms/paged).
CREATE OR ALTER PROCEDURE dbo.sp_HostelRooms_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @HostelId UNIQUEIDENTIFIER = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.HostelId, x.RoomNumber, x.Floor, x.Capacity, x.IsAc, x.MonthlyRent,
        x.FurnitureJson, x.RoomType, x.Status, x.IsActive, x.CreatedAt
    FROM HostelRooms x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@HostelId IS NULL OR x.HostelId = @HostelId)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@Term IS NULL OR x.RoomNumber LIKE @Like OR x.RoomType LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM HostelRooms x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@HostelId IS NULL OR x.HostelId = @HostelId)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@Term IS NULL OR x.RoomNumber LIKE @Like OR x.RoomType LIKE @Like);
END
