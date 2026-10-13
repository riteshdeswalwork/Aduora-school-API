using sms.api.DTOs;
using sms.api.Models;

namespace sms.api.Interfaces.Shared
{
    public interface IDocumentPrintJobManager : IGenericManager<DocumentPrintJob>
    {
        Task<PagedResult<DocumentPrintJob>> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm, string? status, bool includeInactive);
    }
}
