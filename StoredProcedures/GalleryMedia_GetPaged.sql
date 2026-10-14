-- Replaces the inline LINQ in GalleryMediaController.GetPaged
-- (GET /api/photo-gallery/paged).
CREATE OR ALTER PROCEDURE dbo.sp_GalleryMedia_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @MediaType NVARCHAR(50) = NULL,
    @AlbumCategory NVARCHAR(100) = NULL,
    @IsPublic BIT = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @AlbumLike NVARCHAR(104) = '%' + NULLIF(LTRIM(RTRIM(@AlbumCategory)), '') + '%';

    SELECT
        x.Id, x.Title, x.AlbumCategory, x.MediaType, x.Url, x.VisibilityTier,
        x.EventDate, x.UploadedByUserId, x.PhotographerMetadata, x.MediaDensity,
        x.TotalAssetSize, x.IsPublic, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM GalleryMedia x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@MediaType IS NULL OR LOWER(x.MediaType) = LOWER(@MediaType))
        AND (@AlbumCategory IS NULL OR x.AlbumCategory LIKE @AlbumLike)
        AND (@IsPublic IS NULL OR x.IsPublic = @IsPublic)
        AND (@Term IS NULL
             OR x.Title LIKE @Like OR x.Url LIKE @Like OR x.AlbumCategory LIKE @Like
             OR x.MediaType LIKE @Like OR x.PhotographerMetadata LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM GalleryMedia x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@MediaType IS NULL OR LOWER(x.MediaType) = LOWER(@MediaType))
        AND (@AlbumCategory IS NULL OR x.AlbumCategory LIKE @AlbumLike)
        AND (@IsPublic IS NULL OR x.IsPublic = @IsPublic)
        AND (@Term IS NULL
             OR x.Title LIKE @Like OR x.Url LIKE @Like OR x.AlbumCategory LIKE @Like
             OR x.MediaType LIKE @Like OR x.PhotographerMetadata LIKE @Like);
END
