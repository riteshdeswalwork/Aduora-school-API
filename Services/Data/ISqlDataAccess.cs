using System.Data;

namespace sms.api.Services.Data
{
    /// <summary>
    /// Thin Dapper-based data access helper for calling stored procedures.
    /// Used for the high-traffic, large-dataset GET endpoints that have been
    /// moved off EF Core LINQ and onto SQL Server stored procedures for
    /// performance (fewer round trips, server-side filtering/paging, no
    /// change-tracking overhead).
    ///
    /// Regular CRUD/writes continue to go through EF Core (ApplicationDbContext)
    /// as before - this is additive, not a replacement for EF.
    /// </summary>
    public interface ISqlDataAccess
    {
        /// <summary>Executes a stored procedure and maps each row to T.</summary>
        Task<IReadOnlyList<T>> QueryStoredProcAsync<T>(string procedureName, object? parameters = null);

        /// <summary>Executes a stored procedure expected to return exactly one row (or none).</summary>
        Task<T?> QuerySingleStoredProcAsync<T>(string procedureName, object? parameters = null);

        /// <summary>
        /// Executes a stored procedure that returns two result sets: the page of rows (T)
        /// followed by a single total-count row. Used for paginated list endpoints.
        /// </summary>
        Task<(IReadOnlyList<T> Items, int TotalCount)> QueryPagedStoredProcAsync<T>(string procedureName, object? parameters = null);

        /// <summary>Executes a stored procedure that doesn't return rows (e.g. a write proc).</summary>
        Task<int> ExecuteStoredProcAsync(string procedureName, object? parameters = null);
    }
}
