using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eMedLis.Domain.Shared.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Execute stored procedures
        Task<T> ExecuteSprocAsync(string sprocName, params object[] parameters);
        Task<List<T>> ExecuteSprocListAsync(string sprocName, params object[] parameters);
        Task<int> ExecuteSprocNonQueryAsync(string sprocName, params object[] parameters);
        Task<bool> ExecuteSprocBoolAsync(string sprocName, params object[] parameters);

        // Direct entity operations (for simple CRUD if needed)
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();

        // Create
        Task AddAsync(T entity);

        // Update
        Task UpdateAsync(T entity);

        // Delete
        Task DeleteAsync(int id);
        Task DeleteAsync(T entity);

        // Save
        Task<bool> SaveChangesAsync();
    }
}
