using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Interfaces.Auth;
using sms.api.Models;
using sms.api.Models.DTOs;
using System.Security.Claims;

namespace sms.api.Controllers.Document
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentManager _documentManager;
        private readonly IUserManager _userManager;
        private readonly ILogger<DocumentController> _logger;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private const long MaxUserStorageSize = 100 * 1024 * 1024; // 100MB per user

        public DocumentController(IDocumentManager documentManager, IUserManager userManager, ILogger<DocumentController> logger)
        {
            _documentManager = documentManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's documents
        /// </summary>
        [HttpGet("my-documents")]
        public async Task<IActionResult> GetMyDocuments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return BadRequest("Invalid user ID");

                var documents = await _documentManager.GetUserDocumentsAsync(userId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user documents");
                return StatusCode(500, new { message = "Error retrieving documents", error = ex.Message });
            }
        }

        /// <summary>
        /// Get documents for a specific user (Admin only)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserDocuments(Guid userId)
        {
            try
            {
                var documents = await _documentManager.GetUserDocumentsAsync(userId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user documents for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving documents", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all documents (Admin only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var documents = await _documentManager.GetAllDocumentsAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all documents");
                return StatusCode(500, new { message = "Error retrieving documents", error = ex.Message });
            }
        }

        /// <summary>
        /// Get student documents for staff (Staff can see only their assigned students' documents)
        /// </summary>
        [HttpGet("students")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetStudentDocuments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return BadRequest("Invalid user ID");

                var documents = await _documentManager.GetStudentDocumentsByStaffAsync(userId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student documents for staff");
                return StatusCode(500, new { message = "Error retrieving student documents", error = ex.Message });
            }
        }

        /// <summary>
        /// Upload a document
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentDTO uploadDto)
        {
            try
            {
                _logger.LogInformation("Document upload request received");
                _logger.LogInformation("Upload DTO - DocumentType: {DocumentType}, UserId: {UserId}, File: {FileName}",
                    uploadDto.DocumentType, uploadDto.UserId, uploadDto.DocumentFile?.FileName);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state: {ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                if (uploadDto.DocumentFile == null)
                {
                    _logger.LogWarning("No file provided in upload request");
                    return BadRequest(new { message = "No file provided" });
                }

                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                {
                    _logger.LogWarning("Invalid user ID claim: {UserIdClaim}", userIdClaim);
                    return BadRequest(new { message = "Invalid user ID" });
                }

                _logger.LogInformation("Upload request from user: {UserId}", currentUserId);

                // Determine target user - if no UserId specified, use current user
                var targetUserId = uploadDto.UserId ?? currentUserId;

                // Check if user is trying to upload for someone else
                if (uploadDto.UserId.HasValue && uploadDto.UserId.Value != currentUserId)
                {
                    if (!User.IsInRole("Admin"))
                    {
                        _logger.LogWarning("Non-admin user {UserId} trying to upload for another user {TargetUserId}", currentUserId, uploadDto.UserId.Value);
                        return Forbid("Only admins can upload documents for other users");
                    }
                    _logger.LogInformation("Admin uploading for user: {TargetUserId}", targetUserId);
                }

                // Validate file
                if (uploadDto.DocumentFile == null || uploadDto.DocumentFile.Length == 0)
                {
                    _logger.LogWarning("No file uploaded");
                    return BadRequest(new { message = "No file uploaded" });
                }

                _logger.LogInformation("File details - Name: {FileName}, Size: {FileSize}, Type: {ContentType}",
                    uploadDto.DocumentFile.FileName, uploadDto.DocumentFile.Length, uploadDto.DocumentFile.ContentType);

                if (uploadDto.DocumentFile.Length > MaxFileSize)
                {
                    _logger.LogWarning("File size {FileSize} exceeds limit {MaxFileSize}", uploadDto.DocumentFile.Length, MaxFileSize);
                    return BadRequest(new { message = $"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB" });
                }

                if (!await _documentManager.IsValidFileType(uploadDto.DocumentFile.ContentType))
                {
                    _logger.LogWarning("Invalid file type: {ContentType}", uploadDto.DocumentFile.ContentType);
                    return BadRequest(new { message = "Invalid file type. Allowed: PDF, Images (JPG, PNG, GIF), Word documents" });
                }

                if (!await _documentManager.IsValidDocumentType(uploadDto.DocumentType))
                {
                    _logger.LogWarning("Invalid document type: {DocumentType}", uploadDto.DocumentType);
                    return BadRequest(new { message = "Invalid document type" });
                }

                // Verify that the target user exists
                var targetUser = await _userManager.GetByIdAsync(targetUserId);
                if (targetUser == null)
                {
                    _logger.LogWarning("Target user {UserId} not found", targetUserId);
                    return BadRequest(new { message = $"Target user with ID {targetUserId} not found" });
                }

                _logger.LogInformation("Target user found: {UserName} ({UserId})", targetUser.FullName, targetUserId);

                // Validate file size
                if (uploadDto.DocumentFile.Length > MaxFileSize)
                {
                    _logger.LogWarning("File size {FileSize} exceeds maximum {MaxSize}", uploadDto.DocumentFile.Length, MaxFileSize);
                    return BadRequest(new { message = $"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB" });
                }

                // Check user storage limit
                var currentStorageSize = await _documentManager.GetUserDocumentsSizeAsync(targetUserId);
                _logger.LogInformation("Current storage size for user {UserId}: {CurrentSize}/{MaxSize}",
                    targetUserId, currentStorageSize, MaxUserStorageSize);

                if (currentStorageSize + uploadDto.DocumentFile.Length > MaxUserStorageSize)
                {
                    _logger.LogWarning("Storage limit exceeded for user {UserId}. Current: {CurrentSize}, Adding: {FileSize}, Limit: {MaxSize}",
                        targetUserId, currentStorageSize, uploadDto.DocumentFile.Length, MaxUserStorageSize);
                    return BadRequest(new { message = $"Storage limit exceeded. Maximum {MaxUserStorageSize / (1024 * 1024)}MB per user" });
                }

                // Validate document type
                if (!await _documentManager.IsValidDocumentType(uploadDto.DocumentType))
                {
                    _logger.LogWarning("Invalid document type: {DocumentType}", uploadDto.DocumentType);
                    return BadRequest(new { message = "Invalid document type" });
                }

                // Validate file type
                if (!await _documentManager.IsValidFileType(uploadDto.DocumentFile.ContentType))
                {
                    _logger.LogWarning("Invalid file type: {ContentType}", uploadDto.DocumentFile.ContentType);
                    return BadRequest(new { message = "Invalid file type. Only PDF, images, and Word documents are allowed" });
                }

                _logger.LogInformation("All validations passed. Converting file to byte array...");

                // Convert file to byte array
                byte[] documentData;
                using (var memoryStream = new MemoryStream())
                {
                    await uploadDto.DocumentFile.CopyToAsync(memoryStream);
                    documentData = memoryStream.ToArray();
                }

                _logger.LogInformation("File converted to byte array. Size: {DataSize}", documentData.Length);

                // Create document
                var document = new Models.Document
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    FileName = uploadDto.DocumentFile.FileName,
                    DocumentType = uploadDto.DocumentType,
                    ContentType = uploadDto.DocumentFile.ContentType,
                    DocumentData = documentData,
                    FileSize = uploadDto.DocumentFile.Length,
                    Description = uploadDto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Creating document with ID: {DocumentId}", document.Id);

                try
                {
                    var createdDocument = await _documentManager.CreateAsync(document);
                    _logger.LogInformation("Document created successfully with ID: {DocumentId}", createdDocument.Id);

                    return Ok(new {
                        message = "Document uploaded successfully",
                        documentId = createdDocument.Id,
                        fileName = createdDocument.FileName,
                        documentType = createdDocument.DocumentType
                    });
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error while saving document");

                    // Check for specific constraint violations
                    if (dbEx.InnerException?.Message.Contains("FOREIGN KEY constraint") == true)
                    {
                        return BadRequest(new { message = "Invalid user reference. Please ensure the user exists." });
                    }

                    if (dbEx.InnerException?.Message.Contains("duplicate key") == true)
                    {
                        return BadRequest(new { message = "A document with similar properties already exists." });
                    }

                    return StatusCode(500, new { message = "Database error while saving document", error = dbEx.InnerException?.Message ?? dbEx.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, new { message = "Error uploading document", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a user exists (debug endpoint)
        /// </summary>
        [HttpGet("debug/user/{userId}")]
        public async Task<IActionResult> CheckUser(Guid userId)
        {
            try
            {
                var user = await _userManager.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found", userId = userId });
                }

                return Ok(new {
                    message = "User found",
                    userId = user.Id,
                    userName = user.FullName,
                    role = user.Role,
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user {UserId}", userId);
                return StatusCode(500, new { message = "Error checking user", error = ex.Message });
            }
        }

        /// <summary>
        /// Download a document
        /// </summary>
        [HttpGet("{documentId}/download")]
        public async Task<IActionResult> DownloadDocument(Guid documentId)
        {
            try
            {
                var document = await _documentManager.GetDocumentWithDataAsync(documentId);
                if (document == null)
                    return NotFound("Document not found");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                    return BadRequest("Invalid user ID");

                // Check authorization
                if (!User.IsInRole("Admin") && document.UserId != currentUserId)
                {
                    // Staff can view documents of their assigned students
                    if (User.IsInRole("Staff"))
                    {
                        var staffDocuments = await _documentManager.GetStudentDocumentsByStaffAsync(currentUserId);
                        if (!staffDocuments.Any(d => d.Id == documentId))
                            return Forbid("Access denied");
                    }
                    else
                    {
                        return Forbid("Access denied");
                    }
                }

                // Set proper headers for download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{document.FileName}\"");
                return File(document.DocumentData, document.ContentType, document.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Error downloading document", error = ex.Message });
            }
        }

        /// <summary>
        /// View a document (inline in browser)
        /// </summary>
        [HttpGet("{documentId}/view")]
        public async Task<IActionResult> ViewDocument(Guid documentId)
        {
            try
            {
                var document = await _documentManager.GetDocumentWithDataAsync(documentId);
                if (document == null)
                    return NotFound("Document not found");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                    return BadRequest("Invalid user ID");

                // Check authorization (same as download)
                if (!User.IsInRole("Admin") && document.UserId != currentUserId)
                {
                    // Staff can view documents of their assigned students
                    if (User.IsInRole("Staff"))
                    {
                        var staffDocuments = await _documentManager.GetStudentDocumentsByStaffAsync(currentUserId);
                        if (!staffDocuments.Any(d => d.Id == documentId))
                            return Forbid("Access denied");
                    }
                    else
                    {
                        return Forbid("Access denied");
                    }
                }

                // Set headers for inline viewing
                Response.Headers.Add("Content-Disposition", $"inline; filename=\"{document.FileName}\"");
                return File(document.DocumentData, document.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Error viewing document", error = ex.Message });
            }
        }

        /// <summary>
        /// Update document metadata
        /// </summary>
        [HttpPut("{documentId}")]
        public async Task<IActionResult> UpdateDocument(Guid documentId, [FromBody] UpdateDocumentDTO updateDto)
        {
            try
            {
                if (documentId != updateDto.Id)
                    return BadRequest("Document ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var document = await _documentManager.GetByIdAsync(documentId);
                if (document == null)
                    return NotFound("Document not found");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                    return BadRequest("Invalid user ID");

                // Check authorization
                if (!User.IsInRole("Admin") && document.UserId != currentUserId)
                    return Forbid("Access denied");

                if (!await _documentManager.IsValidDocumentType(updateDto.DocumentType))
                    return BadRequest("Invalid document type");

                // Update document
                document.DocumentType = updateDto.DocumentType;
                document.Description = updateDto.Description;
                document.UpdatedAt = DateTime.UtcNow;

                await _documentManager.UpdateAsync(document);
                
                return Ok(new { message = "Document updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Error updating document", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteDocument(Guid documentId)
        {
            try
            {
                var document = await _documentManager.GetByIdAsync(documentId);
                if (document == null)
                    return NotFound("Document not found");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                    return BadRequest("Invalid user ID");

                // Check authorization
                if (!User.IsInRole("Admin") && document.UserId != currentUserId)
                    return Forbid("Access denied");

                await _documentManager.DeleteAsync(documentId);

                return Ok(new { message = "Document deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Error deleting document", error = ex.Message });
            }
        }

        /// <summary>
        /// Verify a document (Admin only)
        /// </summary>
        [HttpPost("{documentId}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyDocument(Guid documentId, [FromBody] VerifyDocumentDTO verifyDto)
        {
            try
            {
                if (documentId != verifyDto.DocumentId)
                    return BadRequest("Document ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                    return BadRequest("Invalid user ID");

                var success = await _documentManager.VerifyDocumentAsync(
                    documentId,
                    verifyDto.IsVerified,
                    verifyDto.VerificationNotes,
                    currentUserId);

                if (!success)
                    return NotFound("Document not found");

                return Ok(new { message = "Document verification updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Error verifying document", error = ex.Message });
            }
        }

        /// <summary>
        /// Get unverified documents (Admin only)
        /// </summary>
        [HttpGet("unverified")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUnverifiedDocuments()
        {
            try
            {
                var documents = await _documentManager.GetUnverifiedDocumentsAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unverified documents");
                return StatusCode(500, new { message = "Error retrieving unverified documents", error = ex.Message });
            }
        }

        /// <summary>
        /// Get documents by type (Admin only)
        /// </summary>
        [HttpGet("type/{documentType}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDocumentsByType(string documentType)
        {
            try
            {
                if (!await _documentManager.IsValidDocumentType(documentType))
                    return BadRequest("Invalid document type");

                var documents = await _documentManager.GetDocumentsByTypeAsync(documentType);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents by type {DocumentType}", documentType);
                return StatusCode(500, new { message = "Error retrieving documents", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user storage statistics
        /// </summary>
        [HttpGet("storage-stats")]
        public async Task<IActionResult> GetStorageStats()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return BadRequest("Invalid user ID");

                var totalSize = await _documentManager.GetUserDocumentsSizeAsync(userId);
                var documentCount = await _documentManager.GetUserDocumentsCountAsync(userId);

                return Ok(new
                {
                    totalSize,
                    documentCount,
                    maxStorageSize = MaxUserStorageSize,
                    remainingStorage = MaxUserStorageSize - totalSize,
                    usagePercentage = (double)totalSize / MaxUserStorageSize * 100
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage stats");
                return StatusCode(500, new { message = "Error retrieving storage statistics", error = ex.Message });
            }
        }
    }
}
