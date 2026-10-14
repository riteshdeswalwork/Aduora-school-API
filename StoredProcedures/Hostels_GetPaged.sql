-- Replaces the inline LINQ in HostelController.GetPaged (GET /api/hostels/paged).
CREATE OR ALTER PROCEDURE dbo.sp_Hostels_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.Name, x.BuildingName, x.WardenId, x.SecurityLevel, x.AmenityTags,
        x.Address, x.HostelType, x.ContactNumber, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM Hostels x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.BuildingName LIKE @Like OR x.Address LIKE @Like
             OR x.HostelType LIKE @Like OR x.ContactNumber LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM Hostels x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Term IS NULL
             OR x.Name LIKE @Like OR x.BuildingName LIKE @Like OR x.Address LIKE @Like
             OR x.HostelType LIKE @Like OR x.ContactNumber LIKE @Like);
END
