using sms.api.Data;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Managers.Shared
{
    public class DocumentPrintJobManager : GenericManager<DocumentPrintJob>, IDocumentPrintJobManager
    {
        public DocumentPrintJobManager(ApplicationDbContext context) : base(context) { }
    }
}

