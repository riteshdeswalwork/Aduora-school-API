using Microsoft.AspNetCore.Http;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Shared
{
    public interface IProfilePhotoManager
    {
        Task<bool> UploadProfilePhotoAsync(Guid userId, IFormFile photoFile);
        Task<ProfilePhotoDataDTO?> GetProfilePhotoAsync(Guid userId);
        Task<bool> DeleteProfilePhotoAsync(Guid userId);
        Task<ProfilePhotoResponseDTO?> GetProfilePhotoInfoAsync(Guid userId);
        bool IsValidImageFile(IFormFile file);
    }
}
