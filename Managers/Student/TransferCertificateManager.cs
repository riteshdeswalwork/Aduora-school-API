using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Student;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services.Data;

namespace sms.api.Managers.Student
{
    public class TransferCertificateManager : GenericManager<TransferCertificate>, ITransferCertificateManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public TransferCertificateManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess) : base(context)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        // Flat row shape returned by dbo.sp_TransferCertificates_GetPaged -
        // mirrors what the controller's ApplyIncludes() (Student.User) needs.
        private sealed class TransferCertificateRow
        {
            public Guid Id { get; set; }
            public Guid AcademicYearId { get; set; }
            public Guid? StudentId { get; set; }
            public string? TcNumber { get; set; }
            public DateTime IssueDate { get; set; }
            public string? Reason { get; set; }
            public string? ClearanceStatus { get; set; }
            public string? Status { get; set; }
            public Guid? GeneratedByUserId { get; set; }
            public Guid? ApprovedByUserId { get; set; }
            public DateTime? ApprovedAt { get; set; }
            public string? ApprovalChain { get; set; }
            public string? HashVerification { get; set; }
            public string? Remarks { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public DateTime? AdmissionDate { get; set; }
            public DateTime? LeavingDate { get; set; }
            public string? LastClassStudied { get; set; }
            public string? PromotedToClass { get; set; }
            public string? SubjectsStudied { get; set; }
            public string? CharacterConduct { get; set; }
            public string? AcademicPerformance { get; set; }
            public bool AnyDuesPending { get; set; }
            public bool LibraryClearance { get; set; }
            public bool LabClearance { get; set; }
            public bool TransportClearance { get; set; }
            public Guid? Stu_Id { get; set; }
            public string? Stu_RollNumber { get; set; }
            public string? Stu_FirstName { get; set; }
            public string? Stu_LastName { get; set; }
        }

        public async Task<PagedResult<TransferCertificate>> GetPagedAsync(
            Guid academicYearId, int pageNumber, int pageSize, string? searchTerm, string? status, bool includeInactive)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@AcademicYearId", academicYearId);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            parameters.Add("@IncludeInactive", includeInactive);

            using var connection = await _sqlDataAccess.CreateOpenConnectionAsync();
            using var multi = await connection.QueryMultipleAsync(
                "dbo.sp_TransferCertificates_GetPaged", parameters, commandType: System.Data.CommandType.StoredProcedure);

            var totalCount = await multi.ReadSingleAsync<int>();
            var rows = (await multi.ReadAsync<TransferCertificateRow>()).ToList();

            var items = rows.Select(r => new TransferCertificate
            {
                Id = r.Id,
                AcademicYearId = r.AcademicYearId,
                StudentId = r.StudentId,
                TcNumber = r.TcNumber,
                IssueDate = r.IssueDate,
                Reason = r.Reason,
                ClearanceStatus = r.ClearanceStatus,
                Status = r.Status ?? string.Empty,
                GeneratedByUserId = r.GeneratedByUserId,
                ApprovedByUserId = r.ApprovedByUserId,
                ApprovedAt = r.ApprovedAt,
                ApprovalChain = r.ApprovalChain,
                HashVerification = r.HashVerification,
                Remarks = r.Remarks,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                AdmissionDate = r.AdmissionDate,
                LeavingDate = r.LeavingDate,
                LastClassStudied = r.LastClassStudied,
                PromotedToClass = r.PromotedToClass,
                SubjectsStudied = r.SubjectsStudied,
                CharacterConduct = r.CharacterConduct,
                AcademicPerformance = r.AcademicPerformance,
                AnyDuesPending = r.AnyDuesPending,
                LibraryClearance = r.LibraryClearance,
                LabClearance = r.LabClearance,
                TransportClearance = r.TransportClearance,
                Student = r.Stu_Id.HasValue ? new sms.api.Models.Student
                {
                    Id = r.Stu_Id.Value,
                    RollNumber = r.Stu_RollNumber ?? string.Empty,
                    User = new sms.api.Models.User { FirstName = r.Stu_FirstName ?? string.Empty, LastName = r.Stu_LastName ?? string.Empty },
                } : null,
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<TransferCertificate>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }
    }
}
