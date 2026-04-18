using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace sms.api.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadProfilePhotoAsync(IFormFile file, Guid userId);
        Task<bool> DeleteProfilePhotoAsync(string filePath);
        string GetProfilePhotoUrl(string? filePath);
        bool IsValidImageFile(IFormFile file);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public FileUploadService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<string> UploadProfilePhotoAsync(IFormFile file, Guid userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            if (!IsValidImageFile(file))
                throw new ArgumentException("Invalid file type. Only image files are allowed.");

            if (file.Length > _maxFileSize)
                throw new ArgumentException($"File size exceeds maximum limit of {_maxFileSize / (1024 * 1024)}MB");

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Delete existing profile photo if exists
            var existingFiles = Directory.GetFiles(uploadsPath, $"{userId}_*");
            foreach (var existingFile in existingFiles)
            {
                try
                {
                    File.Delete(existingFile);
                }
                catch
                {
                    // Ignore errors when deleting old files
                }
            }

            // Save the new file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            return $"uploads/profiles/{fileName}";
        }

        public async Task<bool> DeleteProfilePhotoAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetProfilePhotoUrl(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "/images/default-avatar.png"; // Default avatar

            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7001";
            return $"{baseUrl}/{filePath}";
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Check MIME type
            var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }
    }
}
