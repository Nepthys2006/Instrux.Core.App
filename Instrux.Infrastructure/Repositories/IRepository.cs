using System.Linq.Expressions;

namespace Instrux.Infrastructure.Repositories;

public interface IRepository
{
    Task<T?> GetByIdAsync<T>(int id) where T : class;
    void Add<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Delete<T>(T entity) where T : class;
    void DeleteRange<T>(IEnumerable<T> entities) where T : class;
    Task<int> SaveChangesAsync();

    Task<List<T>> ListAsync<T>() where T : class;
    Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
    Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
    Task<bool> AnyAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
    Task<int> CountAsync<T>(Expression<Func<T, bool>>? predicate = null) where T : class;

    IQueryable<T> Query<T>() where T : class;
}
