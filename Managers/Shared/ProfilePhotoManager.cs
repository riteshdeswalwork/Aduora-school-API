using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Managers.Shared
{

    public class ProfilePhotoManager : IProfilePhotoManager
    {
        private readonly ApplicationDbContext _context;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly string[] _allowedMimeTypes = {
            "image/jpeg", "image/jpg", "image/png", "image/gif",
            "image/bmp", "image/webp"
        };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public ProfilePhotoManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UploadProfilePhotoAsync(Guid userId, IFormFile photoFile)
        {
            try
            {
                // Validate file
                if (!IsValidImageFile(photoFile))
                    return false;

                if (photoFile.Length > _maxFileSize)
                    return false;

                // Find user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                // Convert file to byte array
                using var memoryStream = new MemoryStream();
                await photoFile.CopyToAsync(memoryStream);
                var photoData = memoryStream.ToArray();

                // Update user with photo data
                user.ProfilePhotoData = photoData;
                user.ProfilePhotoContentType = photoFile.ContentType;
                user.ProfilePhotoFileName = photoFile.FileName;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ProfilePhotoDataDTO?> GetProfilePhotoAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.ProfilePhotoData != null)
                    .Select(u => new ProfilePhotoDataDTO
                    {
                        PhotoData = u.ProfilePhotoData!,
                        ContentType = u.ProfilePhotoContentType ?? "image/jpeg",
                        FileName = u.ProfilePhotoFileName ?? "profile.jpg"
                    })
                    .FirstOrDefaultAsync();

                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteProfilePhotoAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                user.ProfilePhotoData = null;
                user.ProfilePhotoContentType = null;
                user.ProfilePhotoFileName = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ProfilePhotoResponseDTO?> GetProfilePhotoInfoAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new ProfilePhotoResponseDTO
                    {
                        UserId = u.Id,
                        FileName = u.ProfilePhotoFileName,
                        ContentType = u.ProfilePhotoContentType,
                        FileSize = u.ProfilePhotoData != null ? u.ProfilePhotoData.Length : null,
                        UploadedAt = u.UpdatedAt,
                        PhotoUrl = u.ProfilePhotoData != null ? $"/api/profile/photo/{u.Id}" : "/images/default-avatar.png"
                    })
                    .FirstOrDefaultAsync();

                return user;
            }
            catch
            {
                return null;
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Check MIME type
            if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            // Check file size
            if (file.Length > _maxFileSize)
                return false;

            return true;
        }
    }
}
