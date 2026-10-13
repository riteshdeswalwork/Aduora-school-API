-- Replaces the inline LINQ in DocumentPrintJobController.GetPaged
-- (GET /api/document-printing/paged). Flat single-table query.
CREATE OR ALTER PROCEDURE dbo.sp_DocumentPrintJobs_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        x.Id, x.DocumentType, x.TemplateName, x.TargetReference, x.JobPriority,
        x.PaperType, x.ColorMode, x.IsDuplex, x.PrintQuality, x.PrinterNodeIp,
        x.BatchSize, x.Status, x.RequestedByUserId, x.RequestedAt, x.PrintedAt,
        x.EstimatedCompletion, x.Notes, x.IsActive
    FROM DocumentPrintJobs x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(LTRIM(RTRIM(x.Status))) = LOWER(LTRIM(RTRIM(@Status))))
        AND (@Term IS NULL
             OR x.DocumentType LIKE @Like OR x.TemplateName LIKE @Like OR x.TargetReference LIKE @Like
             OR x.Status LIKE @Like OR x.Notes LIKE @Like)
    ORDER BY x.RequestedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM DocumentPrintJobs x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR LOWER(LTRIM(RTRIM(x.Status))) = LOWER(LTRIM(RTRIM(@Status))))
        AND (@Term IS NULL
             OR x.DocumentType LIKE @Like OR x.TemplateName LIKE @Like OR x.TargetReference LIKE @Like
             OR x.Status LIKE @Like OR x.Notes LIKE @Like);
END
