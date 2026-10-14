-- Replaces the inline LINQ in TransferCertificateController.GetPaged
-- (GET /api/transfer-certificates/paged). Joins Student+User (matches
-- the controller's ApplyIncludes()), scoped to one academic year.
CREATE OR ALTER PROCEDURE dbo.sp_TransferCertificates_GetPaged
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

    SELECT x.Id
    INTO #Filtered
    FROM TransferCertificates x
    LEFT JOIN Students stu ON stu.Id = x.StudentId
    LEFT JOIN Users u ON u.Id = stu.UserId
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@Term IS NULL
             OR x.TcNumber LIKE @Like OR x.Reason LIKE @Like OR x.Status LIKE @Like OR x.Remarks LIKE @Like
             OR (COALESCE(u.FirstName, '') + ' ' + COALESCE(u.LastName, '')) LIKE @Like);

    SELECT COUNT(*) AS TotalCount FROM #Filtered;

    SELECT
        x.Id, x.AcademicYearId, x.StudentId, x.TcNumber, x.IssueDate, x.Reason,
        x.ClearanceStatus, x.Status, x.GeneratedByUserId, x.ApprovedByUserId, x.ApprovedAt,
        x.ApprovalChain, x.HashVerification, x.Remarks, x.IsActive, x.CreatedAt, x.UpdatedAt,
        x.AdmissionDate, x.LeavingDate, x.LastClassStudied, x.PromotedToClass, x.SubjectsStudied,
        x.CharacterConduct, x.AcademicPerformance, x.AnyDuesPending, x.LibraryClearance,
        x.LabClearance, x.TransportClearance,
        stu.Id AS Stu_Id, stu.RollNumber AS Stu_RollNumber, u.FirstName AS Stu_FirstName, u.LastName AS Stu_LastName
    FROM #Filtered f
    INNER JOIN TransferCertificates x ON x.Id = f.Id
    LEFT JOIN Students stu ON stu.Id = x.StudentId
    LEFT JOIN Users u ON u.Id = stu.UserId
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #Filtered;
END
