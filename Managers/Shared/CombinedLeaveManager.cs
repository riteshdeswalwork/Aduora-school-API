using Dapper;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Shared;
using sms.api.Services.Data;

namespace sms.api.Managers.Shared
{
    /// <summary>
    /// SP-backed, paged replacement for the in-memory merge in LeaveController.GetAll.
    /// dbo.sp_Leaves_GetAllPaged UNIONs StudentLeaves and StaffLeaves into one flat row
    /// shape; each row is mapped here to the exact shape LeaveController.MapStudentLeave /
    /// MapStaffLeave produce.
    /// </summary>
    public class CombinedLeaveManager : ICombinedLeaveManager
    {
        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly ApplicationDbContext _context;

        public CombinedLeaveManager(ISqlDataAccess sqlDataAccess, ApplicationDbContext context)
        {
            _sqlDataAccess = sqlDataAccess;
            _context = context;
        }

        public async Task<PagedResult<object>> GetAllLeavesPagedAsync(int pageNumber, int pageSize, string? status = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            // Mirrors the EF global query filter on StudentLeave/StaffLeave
            // (X-Academic-Year-Id header -> ApplicationDbContext.CurrentAcademicYearId; null = all years).
            parameters.Add("@AcademicYearId", _context.CurrentAcademicYearId);

            var (rows, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<CombinedLeaveRow>(
                "dbo.sp_Leaves_GetAllPaged", parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResult<object>
            {
                Items = rows.Select(Map).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        private static object Map(CombinedLeaveRow r)
        {
            var isStudent = r.StudentId.HasValue;

            object? person = r.Person_Id.HasValue
                ? new
                {
                    Id = r.Person_Id.Value,
                    User = r.Person_UserId.HasValue
                        ? new
                        {
                            FirstName = r.Person_FirstName,
                            LastName = r.Person_LastName,
                            FullName = $"{r.Person_FirstName} {r.Person_LastName}"
                        }
                        : null
                }
                : null;

            return new
            {
                Id = r.Id,
                StudentId = r.StudentId,
                StaffId = r.StaffId,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Reason = r.Reason,
                Status = r.Status,
                ApprovedByStaffId = r.ApprovedByStaffId,
                ApprovedAt = r.ApprovedAt,
                ApprovalRemarks = r.ApprovalRemarks,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Student = isStudent ? person : null,
                Staff = isStudent ? null : person,
                ApprovedByStaff = r.ApprovedBy_Id.HasValue
                    ? new
                    {
                        Id = r.ApprovedBy_Id.Value,
                        FirstName = r.ApprovedBy_FirstName,
                        LastName = r.ApprovedBy_LastName,
                        FullName = $"{r.ApprovedBy_FirstName} {r.ApprovedBy_LastName}"
                    }
                    : null,
                TotalDays = (r.EndDate - r.StartDate).Days + 1,
                ApplicantType = isStudent ? "Student" : "Staff"
            };
        }

        private sealed class CombinedLeaveRow
        {
            public Guid Id { get; set; }
            public Guid? StudentId { get; set; }
            public Guid? StaffId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string? Reason { get; set; }
            public string? Status { get; set; }
            public Guid? ApprovedByStaffId { get; set; }
            public DateTime? ApprovedAt { get; set; }
            public string? ApprovalRemarks { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public string? ApplicantType { get; set; }
            public Guid? Person_Id { get; set; }        // Students.Id or Staff.Id, whichever applies
            public Guid? Person_UserId { get; set; }
            public string? Person_FirstName { get; set; }
            public string? Person_LastName { get; set; }
            public Guid? ApprovedBy_Id { get; set; }    // Users.Id of the approver
            public string? ApprovedBy_FirstName { get; set; }
            public string? ApprovedBy_LastName { get; set; }
        }
    }
}
