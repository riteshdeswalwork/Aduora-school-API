-- Replaces the inline LINQ in OnlinePaymentTransactionController.GetPaged
-- (GET /api/online-payments/paged). Scoped to one academic year. Mirrors
-- the "NotSettled" meta-filter (SettlementStatus != 'Settled') exactly.
CREATE OR ALTER PROCEDURE dbo.sp_OnlinePaymentTransactions_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @AcademicYearId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @SettlementStatus NVARCHAR(50) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';
    DECLARE @IsNotSettled BIT = CASE WHEN @SettlementStatus = 'NotSettled' THEN 1 ELSE 0 END;

    SELECT
        x.Id, x.AcademicYearId, x.StudentId, x.Amount, x.ConvenienceFees, x.TaxBreakdown,
        x.Currency, x.PaymentFor, x.Gateway, x.TransactionReference, x.MerchantRefId,
        x.RefundReference, x.Status, x.SettlementStatus, x.LatencyMs, x.PaidAt,
        x.ReceiptNumber, x.Notes, x.LedgerCategory, x.IsActive, x.CreatedAt, x.UpdatedAt
    FROM OnlinePaymentTransactions x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@SettlementStatus IS NULL
             OR (@IsNotSettled = 1 AND (x.SettlementStatus IS NULL OR x.SettlementStatus <> 'Settled'))
             OR (@IsNotSettled = 0 AND x.SettlementStatus = @SettlementStatus))
        AND (@Term IS NULL
             OR x.TransactionReference LIKE @Like OR x.ReceiptNumber LIKE @Like OR x.MerchantRefId LIKE @Like
             OR x.Gateway LIKE @Like OR x.PaymentFor LIKE @Like OR x.LedgerCategory LIKE @Like
             OR x.SettlementStatus LIKE @Like OR x.Status LIKE @Like)
    ORDER BY x.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM OnlinePaymentTransactions x
    WHERE
        x.AcademicYearId = @AcademicYearId
        AND (@IncludeInactive = 1 OR x.IsActive = 1)
        AND (@Status IS NULL OR x.Status = @Status)
        AND (@SettlementStatus IS NULL
             OR (@IsNotSettled = 1 AND (x.SettlementStatus IS NULL OR x.SettlementStatus <> 'Settled'))
             OR (@IsNotSettled = 0 AND x.SettlementStatus = @SettlementStatus))
        AND (@Term IS NULL
             OR x.TransactionReference LIKE @Like OR x.ReceiptNumber LIKE @Like OR x.MerchantRefId LIKE @Like
             OR x.Gateway LIKE @Like OR x.PaymentFor LIKE @Like OR x.LedgerCategory LIKE @Like
             OR x.SettlementStatus LIKE @Like OR x.Status LIKE @Like);
END
