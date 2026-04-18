using System;
using System.Text.Json.Serialization;

namespace sms.api.Models.DTOs
{
    /// <summary>Create/Update DTO for Transfer Certificate (camelCase from frontend).</summary>
    public class TransferCertificateCreateUpdateDto
    {
        [JsonPropertyName("studentId")]
        public Guid? StudentId { get; set; }

        [JsonPropertyName("tcNumber")]
        public string? TcNumber { get; set; }

        [JsonPropertyName("issueDate")]
        public DateTime IssueDate { get; set; }

        [JsonPropertyName("reasonForLeaving")]
        public string? ReasonForLeaving { get; set; }

        [JsonPropertyName("admissionDate")]
        public DateTime? AdmissionDate { get; set; }

        [JsonPropertyName("leavingDate")]
        public DateTime? LeavingDate { get; set; }

        [JsonPropertyName("lastClassStudied")]
        public string? LastClassStudied { get; set; }

        [JsonPropertyName("promotedToClass")]
        public string? PromotedToClass { get; set; }

        [JsonPropertyName("subjectsStudied")]
        public string? SubjectsStudied { get; set; }

        [JsonPropertyName("characterConduct")]
        public string? CharacterConduct { get; set; }

        [JsonPropertyName("academicPerformance")]
        public string? AcademicPerformance { get; set; }

        [JsonPropertyName("anyDuesPending")]
        public bool AnyDuesPending { get; set; }

        [JsonPropertyName("libraryClearance")]
        public bool LibraryClearance { get; set; } = true;

        [JsonPropertyName("labClearance")]
        public bool LabClearance { get; set; } = true;

        [JsonPropertyName("transportClearance")]
        public bool TransportClearance { get; set; } = true;

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }
    }
}
