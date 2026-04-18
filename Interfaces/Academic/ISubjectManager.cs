using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Interfaces.Academic
{
    public interface ISubjectManager : IGenericManager<Subject>
    {
        Task<IEnumerable<Subject>> GetSubjectsByStaffIdAsync(Guid staffId);
    }
}
