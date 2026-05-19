using System.Linq.Expressions;
using Instrux.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Infrastructure.Repositories;

public sealed class Repository : IRepository
{
    private readonly InstruxDbContext _dbContext;

    public Repository(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T?> GetByIdAsync<T>(int id) where T : class
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public void Add<T>(T entity) where T : class
    {
        _dbContext.Set<T>().Add(entity);
    }

    public void Update<T>(T entity) where T : class
    {
        _dbContext.Set<T>().Update(entity);
    }

    public void Delete<T>(T entity) where T : class
    {
        _dbContext.Set<T>().Remove(entity);
    }

    public void DeleteRange<T>(IEnumerable<T> entities) where T : class
    {
        _dbContext.Set<T>().RemoveRange(entities);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public async Task<List<T>> ListAsync<T>() where T : class
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        return await _dbContext.Set<T>().FirstOrDefaultAsync(predicate);
    }

    public async Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        return await _dbContext.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task<bool> AnyAsync<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        return await _dbContext.Set<T>().AnyAsync(predicate);
    }

    public async Task<int> CountAsync<T>(Expression<Func<T, bool>>? predicate = null) where T : class
    {
        return predicate is null
            ? await _dbContext.Set<T>().CountAsync()
            : await _dbContext.Set<T>().CountAsync(predicate);
    }

    public IQueryable<T> Query<T>() where T : class
    {
        return _dbContext.Set<T>();
    }
}
