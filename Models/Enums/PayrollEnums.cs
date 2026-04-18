namespace sms.api.Models.Enums
{
    public enum PayrollComponentKind
    {
        Earning = 0,
        Deduction = 1
    }

    public enum PayrollCalculationMode
    {
        /// <summary>Uses AmountOrPercent as fixed currency for the month (prorated by paid-days factor).</summary>
        FixedAmount = 0,
        /// <summary>AmountOrPercent is percentage of prorated basic (from Staff.Salary).</summary>
        PercentOfProratedBasic = 1,
        /// <summary>AmountOrPercent is percentage of gross earnings (sum of earnings incl. basic).</summary>
        PercentOfGrossEarnings = 2
    }
}
