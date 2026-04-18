using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sms.api.DTOs;

namespace sms.api.Interfaces.Admin
{
    public interface IDesignationManager
    {
        Task<IEnumerable<DesignationDTO>> GetAllDesignationsAsync();
        Task<DesignationDTO> GetDesignationByIdAsync(Guid id);
        Task<DesignationDTO> CreateDesignationAsync(CreateDesignationDTO designationDto);
        Task<DesignationDTO> UpdateDesignationAsync(Guid id, UpdateDesignationDTO designationDto);
        Task<bool> DeleteDesignationAsync(Guid id);
    }
}
