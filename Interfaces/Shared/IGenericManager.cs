using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sms.api.Interfaces.Shared
{
    public interface IGenericManager<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(Guid id);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(Guid id);
    }
}
