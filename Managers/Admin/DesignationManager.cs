using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Admin;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Admin
{
    public class DesignationManager : IDesignationManager
    {
        private readonly ApplicationDbContext _context;

        public DesignationManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DesignationDTO>> GetAllDesignationsAsync()
        {
            var designations = await _context.Designations
                .Include(d => d.Department)
                .Include(d => d.Staff)
                    .ThenInclude(s => s.User)
                .ToListAsync();

            return designations.Select(d => new DesignationDTO
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                DepartmentId = d.DepartmentId ?? Guid.Empty,
                DepartmentName = d.Department?.Name ?? "Not assigned",
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                TotalStaff = d.Staff?.Count ?? 0,
                ActiveStaff = d.Staff?.Count(s => s.User.IsActive) ?? 0,
                InactiveStaff = d.Staff?.Count(s => !s.User.IsActive) ?? 0
                // Staff collection removed for GetAll - only counts needed
            });
        }

        public async Task<DesignationDTO> GetDesignationByIdAsync(Guid id)
        {
            var designation = await _context.Designations
                .Include(d => d.Department)
                .Include(d => d.Staff)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (designation == null)
            {
                return null;
            }

            return new DesignationDTO
            {
                Id = designation.Id,
                Name = designation.Name,
                Description = designation.Description,
                DepartmentId = designation.DepartmentId ?? Guid.Empty,
                DepartmentName = designation.Department?.Name ?? "Not assigned",
                IsActive = designation.IsActive,
                CreatedAt = designation.CreatedAt,
                UpdatedAt = designation.UpdatedAt,
                TotalStaff = designation.Staff?.Count ?? 0,
                ActiveStaff = designation.Staff?.Count(s => s.User.IsActive) ?? 0,
                InactiveStaff = designation.Staff?.Count(s => !s.User.IsActive) ?? 0,
                Staff = designation.Staff?.Select(s => new StaffDTO
                {
                    Id = s.Id,
                    EmployeeId = s.EmployeeId,
                    UserId = s.UserId,
                    DesignationId = s.DesignationId,
                    DateOfJoining = s.DateOfJoining,
                    Qualification = s.Qualification,
                    Salary = s.Salary,
                    Address = s.Address,
                    User = s.User != null ? new UserDTO
                    {
                        Id = s.User.Id,
                        FirstName = s.User.FirstName,
                        LastName = s.User.LastName,
                        Email = s.User.Email,
                        Username = s.User.Username,
                        Role = s.User.Role,
                        PhoneNumber = s.User.PhoneNumber,
                        Address = s.User.Address,
                        ProfilePicture = s.User.ProfilePicture,
                        IsActive = s.User.IsActive,
                        CreatedAt = s.User.CreatedAt,
                        UpdatedAt = s.User.UpdatedAt
                    } : null
                }).ToList() ?? new List<StaffDTO>()
            };
        }

        public async Task<DesignationDTO> CreateDesignationAsync(CreateDesignationDTO designationDto)
        {
            var designation = new Designation
            {
                Id = Guid.NewGuid(),
                Name = designationDto.Name,
                Description = designationDto.Description,
                DepartmentId = designationDto.DepartmentId,
                IsActive = designationDto.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Designations.Add(designation);
            await _context.SaveChangesAsync();

            return await GetDesignationByIdAsync(designation.Id);
        }

        public async Task<DesignationDTO> UpdateDesignationAsync(Guid id, UpdateDesignationDTO designationDto)
        {
            var designation = await _context.Designations.FindAsync(id);

            if (designation == null)
            {
                return null;
            }

            designation.Name = designationDto.Name;
            designation.Description = designationDto.Description;
            designation.DepartmentId = designationDto.DepartmentId;
            designation.IsActive = designationDto.IsActive;
            designation.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return await GetDesignationByIdAsync(id);
        }

        public async Task<bool> DeleteDesignationAsync(Guid id)
        {
            var designation = await _context.Designations.FindAsync(id);

            if (designation == null)
            {
                return false;
            }

            _context.Designations.Remove(designation);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
