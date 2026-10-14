-- Replaces the inline LINQ in ScholarshipController.GetPaged
-- (GET /api/scholarships/paged). Flat table, no academic-year scoping.
CREATE OR ALTER PROCEDURE dbo.sp_Scholarships_GetPaged
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
        x.Id, x.Name, x.Provider, x.Amount, x.GrantType, x.DisbursementMode,
        x.RenewalFrequency, x.MinimumGpaRequirement, x.EligibilityCriteria,
        x.DocHashes, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM Scholarships x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Term IS NULL OR x.Name LIKE @Like OR x.Provider LIKE @Like OR x.EligibilityCriteria LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM Scholarships x
    WHERE
        (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Term IS NULL OR x.Name LIKE @Like OR x.Provider LIKE @Like OR x.EligibilityCriteria LIKE @Like);
END
