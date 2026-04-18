using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Interfaces.Document
{
    public interface IDocumentManager : IGenericManager<Models.Document>
    {
        Task<IEnumerable<DocumentListDTO>> GetUserDocumentsAsync(Guid userId);
        Task<IEnumerable<DocumentListDTO>> GetAllDocumentsAsync();
        Task<IEnumerable<DocumentListDTO>> GetStudentDocumentsByStaffAsync(Guid staffUserId);
        Task<Models.Document?> GetDocumentWithDataAsync(Guid documentId);
        Task<bool> VerifyDocumentAsync(Guid documentId, bool isVerified, string? notes, Guid verifiedByUserId);
        Task<bool> IsValidDocumentType(string documentType);
        Task<bool> IsValidFileType(string contentType);
        Task<long> GetUserDocumentsSizeAsync(Guid userId);
        Task<int> GetUserDocumentsCountAsync(Guid userId);
        Task<bool> HasDocumentTypeAsync(Guid userId, string documentType);
        Task<IEnumerable<DocumentListDTO>> GetDocumentsByTypeAsync(string documentType);
        Task<IEnumerable<DocumentListDTO>> GetUnverifiedDocumentsAsync();
    }
}
