using System;

namespace sms.api.DTOs
{
    public class FeeReportRowDto
    {
        public Guid StudentFeeId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string FeeHeadName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
    }

    public class FeeDefaulterRowDto
    {
        public Guid StudentFeeId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string FeeHeadName { get; set; } = string.Empty;
        public decimal OutstandingAmount { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }

        /// <summary>0-30, 31-60, 61-90, 90+</summary>
        public string AgingBucket { get; set; } = string.Empty;
    }

    public class FeeDayBookRowDto
    {
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class FeeDayBookDetailDto
    {
        public DateTime PaymentDate { get; set; }
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public Guid ReceivedById { get; set; }
        public string? ReceivedByName { get; set; }
        public bool IsVoided { get; set; }
    }
}
