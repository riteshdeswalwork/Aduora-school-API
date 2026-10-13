-- Replaces FeePaymentManager.GetPaymentsPagedAsync (EF LINQ) for GET /api/fee-payments/paged.
-- Mirrors the original filters (academic year, date range, class), the
-- IsVoided=0 constraint, and the PaymentDate DESC sort exactly.
-- Three result sets: (1) the page of payments joined with student/fee-structure/
-- class/received-by, (2) the total count, (3) the fee receipts for just the
-- payments in that page (grouped back onto each payment in C#).
CREATE OR ALTER PROCEDURE dbo.sp_FeePayments_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER = NULL,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @ClassId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT fp.Id
    INTO #Filtered
    FROM FeePayments fp
    LEFT JOIN StudentFees sf ON sf.Id = fp.StudentFeeId
    LEFT JOIN FeeStructures fs ON fs.Id = sf.FeeStructureId
    WHERE fp.IsVoided = 0
        AND (@AcademicYearId IS NULL OR fp.AcademicYearId = @AcademicYearId)
        AND (@FromDate IS NULL OR CAST(fp.PaymentDate AS DATE) >= CAST(@FromDate AS DATE))
        AND (@ToDate IS NULL OR CAST(fp.PaymentDate AS DATE) <= CAST(@ToDate AS DATE))
        AND (@ClassId IS NULL OR fs.ClassId = @ClassId);

    SELECT COUNT(*) AS TotalCount FROM #Filtered;

    SELECT
        fp.Id, fp.StudentFeeId, fp.AcademicYearId, fp.Amount, fp.PaymentDate, fp.PaymentMethod,
        fp.TransactionReference, fp.ReceiptNumber, fp.Remarks, fp.ReceivedById,
        fp.CreatedAt, fp.UpdatedAt, fp.IsVoided, fp.VoidedAtUtc, fp.VoidedById, fp.VoidReason,
        sf.Id AS SF_Id, sf.StudentId AS SF_StudentId, sf.TotalAmount AS SF_TotalAmount,
        sf.PaidAmount AS SF_PaidAmount, sf.OutstandingAmount AS SF_OutstandingAmount,
        sf.DueDate AS SF_DueDate, sf.Status AS SF_Status,
        stu.Id AS Stu_Id, u.FirstName AS Stu_FirstName, u.LastName AS Stu_LastName,
        fs.Id AS FS_Id, fs.Name AS FS_Name, fs.Amount AS FS_Amount, fs.ClassId AS FS_ClassId,
        c.Id AS Class_Id, c.Name AS Class_Name,
        rb.Id AS RB_Id, rb.FirstName AS RB_FirstName, rb.LastName AS RB_LastName
    FROM #Filtered f
    INNER JOIN FeePayments fp ON fp.Id = f.Id
    LEFT JOIN StudentFees sf ON sf.Id = fp.StudentFeeId
    LEFT JOIN Students stu ON stu.Id = sf.StudentId
    LEFT JOIN Users u ON u.Id = stu.UserId
    LEFT JOIN FeeStructures fs ON fs.Id = sf.FeeStructureId
    LEFT JOIN Classes c ON c.Id = fs.ClassId
    LEFT JOIN Users rb ON rb.Id = fp.ReceivedById
    ORDER BY fp.PaymentDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Receipts for just the payments on this page (joined back in C#).
    SELECT r.Id, r.PaymentId, r.AcademicYearId, r.ReceiptNumber, r.IssueDate, r.Amount,
           r.Description, r.IssuedById, r.CreatedAt, r.UpdatedAt, r.ReprintCount, r.LastReprintAtUtc
    FROM FeeReceipts r
    WHERE r.PaymentId IN (
        SELECT fp.Id
        FROM #Filtered f
        INNER JOIN FeePayments fp ON fp.Id = f.Id
        ORDER BY fp.PaymentDate DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY
    );

    DROP TABLE #Filtered;
END
