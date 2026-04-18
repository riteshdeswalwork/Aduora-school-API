using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Managers;
using sms.api.Models.DTOs;
using System.Security.Claims;

namespace sms.api.Controllers.Shared
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfilePhotoController : ControllerBase
    {
        private readonly IProfilePhotoManager _profilePhotoManager;

        public ProfilePhotoController(IProfilePhotoManager profilePhotoManager)
        {
            _profilePhotoManager = profilePhotoManager;
        }

        /// <summary>
        /// Upload profile photo for current user
        /// </summary>
        [HttpPost("photo")]
        public async Task<IActionResult> UploadProfilePhoto([FromForm] UploadProfilePhotoDTO request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return BadRequest("Invalid user ID");

                if (!_profilePhotoManager.IsValidImageFile(request.PhotoFile))
                    return BadRequest("Invalid image file. Allowed formats: JPG, PNG, GIF, BMP, WEBP. Max size: 5MB");

                var success = await _profilePhotoManager.UploadProfilePhotoAsync(userId, request.PhotoFile);
                if (!success)
                    return BadRequest("Failed to upload profile photo");

                var photoInfo = await _profilePhotoManager.GetProfilePhotoInfoAsync(userId);
                return Ok(new { message = "Profile photo uploaded successfully", data = photoInfo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Upload profile photo for specific user (Admin only)
        /// </summary>
        [HttpPost("photo/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadProfilePhotoForUser(Guid userId, [FromForm] UploadProfilePhotoDTO request)
        {
            try
            {
                if (!_profilePhotoManager.IsValidImageFile(request.PhotoFile))
                    return BadRequest("Invalid image file. Allowed formats: JPG, PNG, GIF, BMP, WEBP. Max size: 5MB");

                var success = await _profilePhotoManager.UploadProfilePhotoAsync(userId, request.PhotoFile);
                if (!success)
                    return BadRequest("Failed to upload profile photo");

                var photoInfo = await _profilePhotoManager.GetProfilePhotoInfoAsync(userId);
                return Ok(new { message = "Profile photo uploaded successfully", data = photoInfo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get profile photo for current user
        /// </summary>
        [HttpGet("photo")]
        public async Task<IActionResult> GetMyProfilePhoto()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return BadRequest("Invalid user ID");

                var photoData = await _profilePhotoManager.GetProfilePhotoAsync(userId);
                if (photoData == null)
                    return NotFound("Profile photo not found");

                return File(photoData.PhotoData, photoData.ContentType, photoData.FileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get profile photo for specific user
        /// </summary>
        [HttpGet("photo/{userId}")]
        [AllowAnonymous] // Allow public access to view profile photos
        public async Task<IActionResult> GetProfilePhoto(Guid userId)
        {
            try
            {
                var photoData = await _profilePhotoManager.GetProfilePhotoAsync(userId);
                if (photoData == null)
                    return NotFound("Profile photo not found");

                return File(photoData.PhotoData, photoData.ContentType, photoData.FileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get profile photo info for current user
        /// </summary>
        [HttpGet("photo/info")]
        public async Task<IActionResult> GetMyProfilePhotoInfo()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return BadRequest("Invalid user ID");

                var photoInfo = await _profilePhotoManager.GetProfilePhotoInfoAsync(userId);
                return Ok(photoInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get profile photo info for specific user
        /// </summary>
        [HttpGet("photo/info/{userId}")]
        public async Task<IActionResult> GetProfilePhotoInfo(Guid userId)
        {
            try
            {
                var photoInfo = await _profilePhotoManager.GetProfilePhotoInfoAsync(userId);
                return Ok(photoInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete profile photo for current user
        /// </summary>
        [HttpDelete("photo")]
        public async Task<IActionResult> DeleteMyProfilePhoto()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return BadRequest("Invalid user ID");

                var success = await _profilePhotoManager.DeleteProfilePhotoAsync(userId);
                if (!success)
                    return BadRequest("Failed to delete profile photo");

                return Ok(new { message = "Profile photo deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete profile photo for specific user (Admin only)
        /// </summary>
        [HttpDelete("photo/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProfilePhoto(Guid userId)
        {
            try
            {
                var success = await _profilePhotoManager.DeleteProfilePhotoAsync(userId);
                if (!success)
                    return BadRequest("Failed to delete profile photo");

                return Ok(new { message = "Profile photo deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
