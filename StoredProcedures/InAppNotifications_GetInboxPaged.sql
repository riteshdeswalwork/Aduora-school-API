-- Replaces the inline LINQ in NotificationController.GetInbox
-- (GET /api/notification/inbox). Already had page/pageSize params in the
-- original - this moves the query itself onto the database side, same
-- contract. Three result sets: (1) total count (all, for this user),
-- (2) unread count, (3) the page of notifications.
CREATE OR ALTER PROCEDURE dbo.sp_InAppNotifications_GetInboxPaged
    @UserId UNIQUEIDENTIFIER,
    @UnreadOnly BIT = 0,
    @Page INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS TotalCount
    FROM InAppNotifications n
    WHERE n.UserId = @UserId AND (@UnreadOnly = 0 OR n.IsRead = 0);

    SELECT COUNT(*) AS UnreadCount
    FROM InAppNotifications n
    WHERE n.UserId = @UserId AND n.IsRead = 0;

    SELECT
        n.Id, n.Title, n.Body, n.Type, n.EntityType, n.EntityId, n.Severity,
        n.IsRead, n.CreatedAt, n.DataJson
    FROM InAppNotifications n
    WHERE n.UserId = @UserId AND (@UnreadOnly = 0 OR n.IsRead = 0)
    ORDER BY n.CreatedAt DESC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
