using Microsoft.EntityFrameworkCore;
using sms.api.DTOs;

namespace sms.api.Utils
{
    public static class PaginationExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
    }
}

