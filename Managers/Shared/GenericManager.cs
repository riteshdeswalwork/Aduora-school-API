using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.Interfaces.Shared;

namespace sms.api.Managers.Shared
{
    public class GenericManager<T> : IGenericManager<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericManager(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity!;
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            var entityType = _context.Model.FindEntityType(typeof(T));
            if (entityType == null)
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }

            var key = entityType.FindPrimaryKey();
            if (key == null || key.Properties.Count == 0)
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }

            var clr = entityType.ClrType;
            var keyValues = key.Properties
                .Select(p => clr.GetProperty(p.Name)?.GetValue(entity))
                .ToArray();

            if (keyValues.Any(v => v == null))
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }

            var tracked = await _dbSet.FindAsync(keyValues);
            if (tracked != null)
            {
                _context.Entry(tracked).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
                return tracked;
            }

            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
