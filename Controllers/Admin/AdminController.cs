using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : ApiControllerBase
    {
        private readonly IAdminManager _adminManager;

        public AdminController(IAdminManager adminManager)
        {
            _adminManager = adminManager;
        }

        [HttpGet("check-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAdminExists()
        {
            var admins = await _adminManager.GetAllAsync();
            return Ok(new { adminExists = admins.Any() });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var admins = await _adminManager.GetAllAsync();

            // Map the admin data to match the frontend model structure
            var mappedAdmins = admins.Select(a => new
            {
                a.Id,
                a.UserId,
                a.AdminLevel,
                a.Department,
                User = a.User != null ? new
                {
                    a.User.Id,
                    a.User.FirstName,
                    a.User.LastName,
                    a.User.Email,
                    a.User.Username,
                    a.User.PhoneNumber,
                    a.User.Address,
                    a.User.Role,
                    a.User.IsActive,
                    a.User.CreatedAt,
                    FullName = $"{a.User.FirstName} {a.User.LastName}",
                    ProfilePhotoData = a.User.ProfilePhotoData != null && a.User.ProfilePhotoData.Length > 0
                        ? Convert.ToBase64String(a.User.ProfilePhotoData) : null
                } : null
            });

            return Ok(mappedAdmins);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var admin = await _adminManager.GetByIdAsync(id);
            if (admin == null)
                return NotFound();

            // Map the admin data to match the frontend model structure
            var mappedAdmin = new
            {
                admin.Id,
                admin.UserId,
                admin.AdminLevel,
                admin.Department,
                User = admin.User != null ? new
                {
                    admin.User.Id,
                    admin.User.FirstName,
                    admin.User.LastName,
                    admin.User.Email,
                    admin.User.Username,
                    admin.User.PhoneNumber,
                    admin.User.Address,
                    admin.User.Role,
                    admin.User.IsActive,
                    admin.User.CreatedAt,
                    FullName = $"{admin.User.FirstName} {admin.User.LastName}",
                    ProfilePhotoData = admin.User.ProfilePhotoData != null && admin.User.ProfilePhotoData.Length > 0
                        ? Convert.ToBase64String(admin.User.ProfilePhotoData) : null
                } : null
            };

            return Ok(mappedAdmin);
        }

        [HttpPost]
        [AllowAnonymous] // Allow anonymous access to create the first admin
        public async Task<IActionResult> Create([FromBody] CreateAdminDTO adminDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if this is the first admin being created
            var admins = await _adminManager.GetAllAsync();
            bool isFirstAdmin = !admins.Any();

            // If it's not the first admin, only allow admin users to create other admins
            if (!isFirstAdmin && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Parse profile photo if provided
            byte[]? profilePhotoData = null;
            if (!string.IsNullOrEmpty(adminDto.ProfilePhotoData))
            {
                try
                {
                    var base64Data = adminDto.ProfilePhotoData;
                    if (base64Data.Contains(","))
                        base64Data = base64Data.Split(',')[1];
                    profilePhotoData = Convert.FromBase64String(base64Data);
                    if (profilePhotoData.Length > 5 * 1024 * 1024)
                        return BadRequest(new { message = "Profile photo size cannot exceed 5MB" });
                }
                catch
                {
                    return BadRequest(new { message = "Invalid profile photo data" });
                }
            }

            // Create user object
            var user = new User
            {
                FirstName = adminDto.FirstName,
                LastName = adminDto.LastName,
                Email = adminDto.Email,
                Username = adminDto.Username,
                PasswordHash = adminDto.Password, // Will be hashed in the manager
                PhoneNumber = adminDto.PhoneNumber,
                Address = adminDto.Address,
                ProfilePhotoData = profilePhotoData,
                ProfilePhotoContentType = profilePhotoData != null ? "image/jpeg" : null,
                ProfilePhotoFileName = profilePhotoData != null ? "profile.jpg" : null
            };

            // Map DTO to entity
            var admin = new Models.Admin
            {
                Id = Guid.NewGuid(),
                AdminLevel = !string.IsNullOrEmpty(adminDto.AdminLevel) ? adminDto.AdminLevel : "Administrator",
                Department = !string.IsNullOrEmpty(adminDto.Department) ? adminDto.Department : "Administration",
                User = user,
                RoleId = adminDto.RoleId
            };

            try
            {
                var createdAdmin = await _adminManager.CreateAsync(admin);

                // Don't return the password hash in the response
                if (createdAdmin.User != null)
                {
                    createdAdmin.User.PasswordHash = "[HIDDEN]";
                }

                return CreatedAtAction(nameof(GetById), new { id = createdAdmin.Id }, createdAdmin);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdminDTO adminDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingAdmin = await _adminManager.GetByIdAsync(id);
            if (existingAdmin == null)
                return NotFound();

            // Update the existing admin with DTO data
            existingAdmin.AdminLevel = adminDto.AdminLevel;
            existingAdmin.Department = adminDto.Department;
            existingAdmin.RoleId = adminDto.RoleId;

            // Update user properties
            if (existingAdmin.User != null)
            {
                existingAdmin.User.FirstName = adminDto.FirstName;
                existingAdmin.User.LastName = adminDto.LastName;
                existingAdmin.User.Email = adminDto.Email;
                existingAdmin.User.PhoneNumber = adminDto.PhoneNumber;
                existingAdmin.User.Address = adminDto.Address;
                existingAdmin.User.IsActive = adminDto.IsActive;

                // Profile photo: update or remove
                if (adminDto.ProfilePhotoData != null)
                {
                    if (string.IsNullOrEmpty(adminDto.ProfilePhotoData))
                    {
                        existingAdmin.User.ProfilePhotoData = null;
                        existingAdmin.User.ProfilePhotoContentType = null;
                        existingAdmin.User.ProfilePhotoFileName = null;
                    }
                    else
                    {
                        try
                        {
                            var base64Data = adminDto.ProfilePhotoData;
                            if (base64Data.Contains(","))
                                base64Data = base64Data.Split(',')[1];
                            var bytes = Convert.FromBase64String(base64Data);
                            if (bytes.Length > 5 * 1024 * 1024)
                                return BadRequest(new { message = "Profile photo size cannot exceed 5MB" });
                            existingAdmin.User.ProfilePhotoData = bytes;
                            existingAdmin.User.ProfilePhotoContentType = "image/jpeg";
                            existingAdmin.User.ProfilePhotoFileName = "profile.jpg";
                        }
                        catch
                        {
                            return BadRequest(new { message = "Invalid profile photo data" });
                        }
                    }
                }
            }

            try
            {
                var updatedAdmin = await _adminManager.UpdateAsync(existingAdmin);
                return Ok(updatedAdmin);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var admin = await _adminManager.GetByIdAsync(id);
                if (admin == null)
                    return NotFound(new { message = "Administrator not found" });

                // Check if trying to delete own account
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == admin.UserId.ToString())
                    return BadRequest(new { message = "Cannot delete your own account" });

                // Delete admin - cascade delete will handle all related data
                var result = await _adminManager.DeleteAsync(id);
                if (!result)
                    return BadRequest(new { message = "Failed to delete administrator" });

                return Ok(new { message = $"Administrator '{admin.User?.FullName}' and all associated data deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting administrator", error = ex.Message });
            }
        }
    }
}
