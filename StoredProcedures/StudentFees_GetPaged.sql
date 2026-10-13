-- Replaces StudentFeeManager.GetStudentFeesPagedAsync (EF LINQ) for
-- GET /api/student-fees/paged. Mirrors the original filters (academic year,
-- class, data-scope allowed classes, status incl. the computed "Overdue",
-- search across student name/roll/registration/email), sort
-- (DueDate DESC, Student.User.LastName ASC) and paging exactly.
-- Four result sets: (1) total count, (2) the page of student-fee rows joined
-- with student/fee-structure/class, (3) fee payments for that page's rows,
-- (4) fee discounts for that page's rows (both grouped back onto each row in C#).
CREATE OR ALTER PROCEDURE dbo.sp_StudentFees_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @ClassId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50) = NULL,
    @AcademicYearId UNIQUEIDENTIFIER = NULL,
    @HasAllowedClassFilter BIT = 0,
    @AllowedClassIds dbo.GuidListType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    IF @HasAllowedClassFilter = 1 AND NOT EXISTS (SELECT 1 FROM @AllowedClassIds)
    BEGIN
        SELECT 0 AS TotalCount;
        SELECT TOP (0)
            CAST(NULL AS UNIQUEIDENTIFIER) AS Id, CAST(NULL AS UNIQUEIDENTIFIER) AS StudentId,
            CAST(NULL AS UNIQUEIDENTIFIER) AS FeeStructureId, CAST(NULL AS UNIQUEIDENTIFIER) AS FeeStructureLineId,
            CAST(NULL AS UNIQUEIDENTIFIER) AS AcademicYearId, CAST(NULL AS DECIMAL(18,2)) AS TotalAmount,
            CAST(NULL AS DECIMAL(18,2)) AS Amount, CAST(NULL AS DECIMAL(18,2)) AS PaidAmount,
            CAST(NULL AS DECIMAL(18,2)) AS DiscountAmount, CAST(NULL AS DECIMAL(18,2)) AS OutstandingAmount,
            CAST(NULL AS DATETIME2) AS DueDate, CAST(NULL AS NVARCHAR(50)) AS Status,
            CAST(NULL AS NVARCHAR(500)) AS Remarks, CAST(NULL AS INT) AS BillingPeriodYear,
            CAST(NULL AS INT) AS BillingPeriodMonth, CAST(NULL AS INT) AS InstallmentSequence,
            CAST(NULL AS NVARCHAR(50)) AS Source, CAST(NULL AS DATETIME2) AS CreatedAt, CAST(NULL AS DATETIME2) AS UpdatedAt,
            CAST(NULL AS NVARCHAR(100)) AS Stu_RollNumber, CAST(NULL AS NVARCHAR(100)) AS Stu_RegistrationNumber,
            CAST(NULL AS NVARCHAR(200)) AS Stu_FirstName, CAST(NULL AS NVARCHAR(200)) AS Stu_LastName,
            CAST(NULL AS NVARCHAR(200)) AS Stu_Email,
            CAST(NULL AS NVARCHAR(200)) AS FS_Name, CAST(NULL AS DECIMAL(18,2)) AS FS_Amount,
            CAST(NULL AS UNIQUEIDENTIFIER) AS FS_ClassId, CAST(NULL AS NVARCHAR(200)) AS Class_Name;
        SELECT TOP (0)
            CAST(NULL AS UNIQUEIDENTIFIER) AS Id, CAST(NULL AS UNIQUEIDENTIFIER) AS StudentFeeId,
            CAST(NULL AS UNIQUEIDENTIFIER) AS AcademicYearId, CAST(NULL AS DECIMAL(18,2)) AS Amount,
            CAST(NULL AS DATETIME2) AS PaymentDate, CAST(NULL AS NVARCHAR(50)) AS PaymentMethod,
            CAST(NULL AS NVARCHAR(100)) AS TransactionReference, CAST(NULL AS NVARCHAR(100)) AS ReceiptNumber,
            CAST(NULL AS NVARCHAR(500)) AS Remarks, CAST(NULL AS UNIQUEIDENTIFIER) AS ReceivedById,
            CAST(NULL AS DATETIME2) AS CreatedAt, CAST(NULL AS DATETIME2) AS UpdatedAt,
            CAST(NULL AS BIT) AS IsVoided, CAST(NULL AS DATETIME2) AS VoidedAtUtc,
            CAST(NULL AS UNIQUEIDENTIFIER) AS VoidedById, CAST(NULL AS NVARCHAR(500)) AS VoidReason;
        SELECT TOP (0)
            CAST(NULL AS UNIQUEIDENTIFIER) AS Id, CAST(NULL AS UNIQUEIDENTIFIER) AS StudentFeeId,
            CAST(NULL AS UNIQUEIDENTIFIER) AS AcademicYearId, CAST(NULL AS NVARCHAR(50)) AS DiscountType,
            CAST(NULL AS DECIMAL(18,2)) AS Amount, CAST(NULL AS DECIMAL(18,2)) AS Percentage,
            CAST(NULL AS NVARCHAR(500)) AS Reason, CAST(NULL AS UNIQUEIDENTIFIER) AS ApprovedById,
            CAST(NULL AS DATETIME2) AS ApprovedDate, CAST(NULL AS NVARCHAR(500)) AS Remarks,
            CAST(NULL AS DATETIME2) AS CreatedAt, CAST(NULL AS DATETIME2) AS UpdatedAt;
        RETURN;
    END

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @IsOverdue BIT = CASE WHEN @Status = 'Overdue' THEN 1 ELSE 0 END;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

    SELECT sf.Id
    INTO #Filtered
    FROM StudentFees sf
    LEFT JOIN FeeStructures fs ON fs.Id = sf.FeeStructureId
    LEFT JOIN Students stu ON stu.Id = sf.StudentId
    LEFT JOIN Users u ON u.Id = stu.UserId
    WHERE
        (@AcademicYearId IS NULL OR sf.AcademicYearId = @AcademicYearId)
        AND (@ClassId IS NULL OR fs.ClassId = @ClassId)
        AND (@HasAllowedClassFilter = 0 OR EXISTS (SELECT 1 FROM @AllowedClassIds a WHERE a.Id = fs.ClassId))
        AND (
            @Status IS NULL
            OR (@IsOverdue = 1 AND sf.OutstandingAmount > 0 AND CAST(sf.DueDate AS DATE) < @Today)
            OR (@IsOverdue = 0 AND sf.Status = @Status)
        )
        AND (@Term IS NULL
             OR (COALESCE(u.FirstName, '') + ' ' + COALESCE(u.LastName, '')) LIKE @Like
             OR stu.RollNumber LIKE @Like
             OR stu.RegistrationNumber LIKE @Like
             OR u.Email LIKE @Like);

    SELECT COUNT(*) AS TotalCount FROM #Filtered;

    SELECT
        sf.Id, sf.StudentId, sf.FeeStructureId, sf.FeeStructureLineId, sf.AcademicYearId,
        sf.TotalAmount, sf.Amount, sf.PaidAmount, sf.DiscountAmount, sf.OutstandingAmount,
        sf.DueDate, sf.Status, sf.Remarks, sf.BillingPeriodYear, sf.BillingPeriodMonth,
        sf.InstallmentSequence, sf.Source, sf.CreatedAt, sf.UpdatedAt,
        stu.RollNumber AS Stu_RollNumber, stu.RegistrationNumber AS Stu_RegistrationNumber,
        u.FirstName AS Stu_FirstName, u.LastName AS Stu_LastName, u.Email AS Stu_Email,
        fs.Name AS FS_Name, fs.Amount AS FS_Amount, fs.ClassId AS FS_ClassId, c.Name AS Class_Name
    INTO #Page
    FROM #Filtered f
    INNER JOIN StudentFees sf ON sf.Id = f.Id
    LEFT JOIN FeeStructures fs ON fs.Id = sf.FeeStructureId
    LEFT JOIN Classes c ON c.Id = fs.ClassId
    LEFT JOIN Students stu ON stu.Id = sf.StudentId
    LEFT JOIN Users u ON u.Id = stu.UserId
    ORDER BY sf.DueDate DESC, u.LastName ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT * FROM #Page;

    -- Payments for just this page's rows (no IsVoided filter - matches the
    -- original's unfiltered .Include(sf => sf.Payments)).
    SELECT fp.Id, fp.StudentFeeId, fp.AcademicYearId, fp.Amount, fp.PaymentDate, fp.PaymentMethod,
           fp.TransactionReference, fp.ReceiptNumber, fp.Remarks, fp.ReceivedById,
           fp.CreatedAt, fp.UpdatedAt, fp.IsVoided, fp.VoidedAtUtc, fp.VoidedById, fp.VoidReason
    FROM FeePayments fp
    WHERE fp.StudentFeeId IN (SELECT Id FROM #Page);

    -- Discounts for just this page's rows.
    SELECT d.Id, d.StudentFeeId, d.AcademicYearId, d.DiscountType, d.Amount, d.Percentage,
           d.Reason, d.ApprovedById, d.ApprovedDate, d.Remarks, d.CreatedAt, d.UpdatedAt
    FROM FeeDiscounts d
    WHERE d.StudentFeeId IN (SELECT Id FROM #Page);

    DROP TABLE #Page;
    DROP TABLE #Filtered;
END
