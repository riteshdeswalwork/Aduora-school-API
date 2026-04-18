using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Document;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Models.DTOs;
using DocumentEntity = sms.api.Models.Document;

namespace sms.api.Managers.Document
{
    public class DocumentManager : GenericManager<DocumentEntity>, IDocumentManager
    {
        private readonly ILogger<DocumentManager> _logger;

        public DocumentManager(ApplicationDbContext context, ILogger<DocumentManager> logger)
            : base(context)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<DocumentListDTO>> GetUserDocumentsAsync(Guid userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .Include(d => d.User)
                .Include(d => d.VerifiedByUser)
                .Select(d => new DocumentListDTO
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    DocumentType = d.DocumentType,
                    ContentType = d.ContentType,
                    FileSize = d.FileSize,
                    Description = d.Description,
                    IsVerified = d.IsVerified,
                    CreatedAt = d.CreatedAt,
                    UserName = d.User.FullName,
                    UserRole = d.User.Role ?? "Unknown"
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentListDTO>> GetAllDocumentsAsync()
        {
            return await _context.Documents
                .Include(d => d.User)
                .Include(d => d.VerifiedByUser)
                .Select(d => new DocumentListDTO
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    DocumentType = d.DocumentType,
                    ContentType = d.ContentType,
                    FileSize = d.FileSize,
                    Description = d.Description,
                    IsVerified = d.IsVerified,
                    CreatedAt = d.CreatedAt,
                    UserName = d.User.FullName,
                    UserRole = d.User.Role ?? "Unknown"
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentListDTO>> GetStudentDocumentsByStaffAsync(Guid staffUserId)
        {
            // Get staff record
            var staff = await _context.Staff
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == staffUserId);

            if (staff == null)
                return new List<DocumentListDTO>();

            // Get classes assigned to this staff
            var assignedClassIds = await _context.Classes
                .Where(c => c.TeacherId == staff.Id)
                .Select(c => c.Id)
                .ToListAsync();

            // Get students from those classes
            var studentIds = await _context.ClassEnrollments
                .Where(ce => assignedClassIds.Contains(ce.ClassId) && ce.IsActive)
                .Select(ce => ce.StudentId)
                .ToListAsync();

            // Get user IDs for those students
            var studentUserIds = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .Select(s => s.UserId)
                .ToListAsync();

            // Get documents for those students
            return await _context.Documents
                .Where(d => studentUserIds.Contains(d.UserId))
                .Include(d => d.User)
                .Include(d => d.VerifiedByUser)
                .Select(d => new DocumentListDTO
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    DocumentType = d.DocumentType,
                    ContentType = d.ContentType,
                    FileSize = d.FileSize,
                    Description = d.Description,
                    IsVerified = d.IsVerified,
                    CreatedAt = d.CreatedAt,
                    UserName = d.User.FullName,
                    UserRole = d.User.Role ?? "Unknown"
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<DocumentEntity?> GetDocumentWithDataAsync(Guid documentId)
        {
            return await _context.Documents
                .Include(d => d.User)
                .Include(d => d.VerifiedByUser)
                .FirstOrDefaultAsync(d => d.Id == documentId);
        }

        public async Task<bool> VerifyDocumentAsync(Guid documentId, bool isVerified, string? notes, Guid verifiedByUserId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                return false;

            document.IsVerified = isVerified;
            document.VerificationNotes = notes;
            document.VerifiedBy = verifiedByUserId;
            document.VerifiedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsValidDocumentType(string documentType)
        {
            var validTypes = new[]
            {
                "AadhaarCard", "PanCard", "BirthCertificate", "MarkSheet",
                "TransferCertificate", "CasteCertificate", "IncomeCertificate",
                "PassportPhoto", "Signature", "Other"
            };
           
            return validTypes.Contains(documentType);
        }

        public async Task<bool> IsValidFileType(string contentType)
        {
            var validTypes = new[]
            {
                "application/pdf",
                "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp",
                "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };
           
            return validTypes.Contains(contentType.ToLower());
        }

        public async Task<long> GetUserDocumentsSizeAsync(Guid userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .SumAsync(d => d.FileSize);
        }

        public async Task<int> GetUserDocumentsCountAsync(Guid userId)
        {
            return await _context.Documents
                .CountAsync(d => d.UserId == userId);
        }

        public async Task<bool> HasDocumentTypeAsync(Guid userId, string documentType)
        {
            return await _context.Documents
                .AnyAsync(d => d.UserId == userId && d.DocumentType == documentType);
        }

        public async Task<IEnumerable<DocumentListDTO>> GetDocumentsByTypeAsync(string documentType)
        {
            return await _context.Documents
                .Where(d => d.DocumentType == documentType)
                .Include(d => d.User)
                .Include(d => d.VerifiedByUser)
                .Select(d => new DocumentListDTO
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    DocumentType = d.DocumentType,
                    ContentType = d.ContentType,
                    FileSize = d.FileSize,
                    Description = d.Description,
                    IsVerified = d.IsVerified,
                    CreatedAt = d.CreatedAt,
                    UserName = d.User.FullName,
                    UserRole = d.User.Role ?? "Unknown"
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentListDTO>> GetUnverifiedDocumentsAsync()
        {
            return await _context.Documents
                .Where(d => !d.IsVerified)
                .Include(d => d.User)
                .Select(d => new DocumentListDTO
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    DocumentType = d.DocumentType,
                    ContentType = d.ContentType,
                    FileSize = d.FileSize,
                    Description = d.Description,
                    IsVerified = d.IsVerified,
                    CreatedAt = d.CreatedAt,
                    UserName = d.User.FullName,
                    UserRole = d.User.Role ?? "Unknown"
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
    }
}
