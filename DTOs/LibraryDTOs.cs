using System.ComponentModel.DataAnnotations;

namespace sms.api.DTOs
{
    // Book Category DTOs
    public class BookCategoryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ColorCode { get; set; } = "#007bff";
        public bool IsActive { get; set; }
        public int MaxIssueDays { get; set; }
        public int MaxRenewalCount { get; set; }
        public bool IsReferenceOnly { get; set; }
        public decimal FinePerDay { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalBooks { get; set; }
        public int AvailableBooks { get; set; }
        public int IssuedBooks { get; set; }
    }

    public class CreateBookCategoryDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(7)]
        public string ColorCode { get; set; } = "#007bff";

        [Range(1, 365)]
        public int MaxIssueDays { get; set; } = 14;

        [Range(0, 10)]
        public int MaxRenewalCount { get; set; } = 2;

        public bool IsReferenceOnly { get; set; } = false;

        [Range(0, 1000)]
        public decimal FinePerDay { get; set; } = 1.00m;
    }

    public class UpdateBookCategoryDTO
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(7)]
        public string? ColorCode { get; set; }

        public bool? IsActive { get; set; }

        [Range(1, 365)]
        public int? MaxIssueDays { get; set; }

        [Range(0, 10)]
        public int? MaxRenewalCount { get; set; }

        public bool? IsReferenceOnly { get; set; }

        [Range(0, 1000)]
        public decimal? FinePerDay { get; set; }
    }

    // Author DTOs
    public class AuthorDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public string? Nationality { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Education { get; set; }
        public string? Awards { get; set; }
        public string? NotableWorks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAlive { get; set; }
        public int? Age { get; set; }
        public int TotalBooks { get; set; }
        public string ShortBio { get; set; } = string.Empty;
    }

    public class CreateAuthorDTO
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Biography { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [StringLength(500)]
        public string? Education { get; set; }

        [StringLength(1000)]
        public string? Awards { get; set; }

        [StringLength(1000)]
        public string? NotableWorks { get; set; }
    }

    // Publisher DTOs
    public class PublisherDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? ContactPerson { get; set; }
        public DateTime? EstablishedDate { get; set; }
        public string? Specialization { get; set; }
        public bool IsActive { get; set; }
        public decimal? Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalBooks { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public string PrimaryContact { get; set; } = string.Empty;
    }

    public class CreatePublisherDTO
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        public DateTime? EstablishedDate { get; set; }

        [StringLength(1000)]
        public string? Specialization { get; set; }
    }

    // Book DTOs
    public class BookDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string? ISBN13 { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid PublisherId { get; set; }
        public string PublisherName { get; set; } = string.Empty;
        public string? Edition { get; set; }
        public int? PublicationYear { get; set; }
        public string? Language { get; set; }
        public int? Pages { get; set; }
        public string? Format { get; set; }
        public string? Description { get; set; }
        public string AccessionNumber { get; set; } = string.Empty;
        public string? CallNumber { get; set; }
        public string? Location { get; set; }
        public string? Barcode { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime? AcquisitionDate { get; set; }
        public string Condition { get; set; } = "Good";
        public bool IsAvailable { get; set; }
        public bool IsActive { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int TotalIssues { get; set; }
        public DateTime? LastIssuedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DisplayTitle { get; set; } = string.Empty;
        public string PrimaryAuthor { get; set; } = string.Empty;
        public string AllAuthors { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public string ConditionColor { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
        public List<AuthorDTO> Authors { get; set; } = new();
    }

    public class CreateBookDTO
    {
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Subtitle { get; set; }

        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ISBN13 { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        public Guid PublisherId { get; set; }

        [StringLength(100)]
        public string? Edition { get; set; }

        [Range(1000, 9999)]
        public int? PublicationYear { get; set; }

        [StringLength(100)]
        public string Language { get; set; } = "English";

        [Range(1, 10000)]
        public int? Pages { get; set; }

        [StringLength(50)]
        public string? Format { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string AccessionNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CallNumber { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [Range(0, 100000)]
        public decimal? PurchasePrice { get; set; }

        public DateTime? AcquisitionDate { get; set; }

        [StringLength(50)]
        public string Condition { get; set; } = "Good";

        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        public List<Guid> AuthorIds { get; set; } = new();
    }

    // Library Member DTOs
    public class LibraryMemberDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string MembershipNumber { get; set; } = string.Empty;
        public string MemberType { get; set; } = string.Empty;
        public DateTime MembershipStartDate { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuspended { get; set; }
        public int MaxBooksAllowed { get; set; }
        public int CurrentBooksIssued { get; set; }
        public int TotalBooksIssued { get; set; }
        public decimal PendingFineAmount { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool CanIssueBooks { get; set; }
        public bool HasOverdueBooks { get; set; }
        public string MembershipStatus { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public int AvailableBookSlots { get; set; }
        public bool IsMembershipExpired { get; set; }
        public string MemberTypeDisplay { get; set; } = string.Empty;
    }

    public class CreateLibraryMemberDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string MembershipNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string MemberType { get; set; } = string.Empty;

        public DateTime? MembershipEndDate { get; set; }

        [Range(1, 20)]
        public int MaxBooksAllowed { get; set; } = 3;

        [Range(1, 365)]
        public int MaxIssueDays { get; set; } = 14;

        [Range(0, 10)]
        public int MaxRenewalCount { get; set; } = 2;

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? EmergencyContact { get; set; }

        [StringLength(20)]
        public string? EmergencyPhone { get; set; }
    }

    // Book Issue DTOs
    public class BookIssueDTO
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookISBN { get; set; } = string.Empty;
        public Guid LibraryMemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string MembershipNumber { get; set; } = string.Empty;
        public string IssueNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public int RenewalCount { get; set; }
        public decimal FineAmount { get; set; }
        public decimal PendingFineAmount { get; set; }
        public string? FineReason { get; set; }
        public string IssueCondition { get; set; } = string.Empty;
        public string? ReturnCondition { get; set; }
        public bool IsLost { get; set; }
        public bool IsDamaged { get; set; }
        public string IssuedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public int DaysUntilDue { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public bool CanBeRenewed { get; set; }
        public bool NeedsAttention { get; set; }
        public string AttentionReason { get; set; } = string.Empty;
    }

    public class CreateBookIssueDTO
    {
        [Required]
        public Guid BookId { get; set; }

        [Required]
        public Guid LibraryMemberId { get; set; }

        public DateTime? IssueDate { get; set; }

        public int? IssueDays { get; set; }

        [StringLength(1000)]
        public string? IssueNotes { get; set; }

        [StringLength(100)]
        public string? SpecialInstructions { get; set; }
    }

    // Book Reservation DTOs
    public class BookReservationDTO
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public Guid LibraryMemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string ReservationNumber { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime? FulfilledDate { get; set; }
        public string? FulfilledByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public string QueuePosition { get; set; } = string.Empty;
        public bool CanBeFulfilled { get; set; }
        public bool CanBeCancelled { get; set; }
        public string EstimatedWaitTime { get; set; } = string.Empty;
    }

    public class CreateBookReservationDTO
    {
        [Required]
        public Guid BookId { get; set; }

        [Required]
        public Guid LibraryMemberId { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(1000)]
        public string? ReservationNotes { get; set; }

        [StringLength(50)]
        public string? RequestedEdition { get; set; }

        [StringLength(100)]
        public string? SpecialRequirements { get; set; }
    }

    // Library Fine DTOs
    public class LibraryFineDTO
    {
        public Guid Id { get; set; }
        public Guid LibraryMemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public Guid? BookIssueId { get; set; }
        public string? BookTitle { get; set; }
        public string FineNumber { get; set; } = string.Empty;
        public string FineType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime FineDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Description { get; set; }
        public string? Reason { get; set; }
        public int? OverdueDays { get; set; }
        public bool IsWaived { get; set; }
        public string? WaiverReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPending { get; set; }
        public bool IsPaid { get; set; }
        public bool IsOverdue { get; set; }
        public string StatusColor { get; set; } = string.Empty;
        public string FineTypeDisplay { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public bool NeedsAttention { get; set; }
    }

    public class CreateLibraryFineDTO
    {
        [Required]
        public Guid LibraryMemberId { get; set; }

        public Guid? BookIssueId { get; set; }

        [Required]
        [StringLength(50)]
        public string FineType { get; set; } = string.Empty; // Overdue, Damage, Lost, Late Return, Other

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }

    // Library Dashboard DTOs
    public class LibraryDashboardDTO
    {
        public int TotalBooks { get; set; }
        public int AvailableBooks { get; set; }
        public int IssuedBooks { get; set; }
        public int ReservedBooks { get; set; }
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int SuspendedMembers { get; set; }
        public int TotalIssues { get; set; }
        public int OverdueIssues { get; set; }
        public int TodayIssues { get; set; }
        public int TodayReturns { get; set; }
        public decimal TotalFines { get; set; }
        public decimal PendingFines { get; set; }
        public int PendingReservations { get; set; }
        public List<BookDTO> PopularBooks { get; set; } = new();
        public List<BookIssueDTO> RecentIssues { get; set; } = new();
        public List<BookIssueDTO> OverdueBooks { get; set; } = new();
        public List<LibraryFineDTO> PendingFinesList { get; set; } = new();
    }
}
