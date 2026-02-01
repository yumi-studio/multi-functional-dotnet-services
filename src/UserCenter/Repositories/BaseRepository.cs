using System;
using Microsoft.EntityFrameworkCore;
using UserApi.Models;
using UserApi.Repositories.Interfaces;

namespace UserApi.Repositories;

public abstract class BaseRepository<T, TKey>(DbContext dbContext) : IRepository<T, TKey> where T : class
{
  protected readonly DbContext _dbContext = dbContext;
  protected readonly DbSet<T> _dbSet = dbContext.Set<T>();

  public DbContext GetDbContext()
  {
    return _dbContext;
  }

  public DbSet<T> GetDbSet()
  {
    return _dbSet;
  }

  public async Task<T?> GetByIdAsync(TKey id)
  {
    return await _dbSet.FindAsync(id);
  }

  public async Task<IEnumerable<T>> GetAllAsync()
  {
    return await _dbSet.ToListAsync();
  }

  public async Task AddAsync(T entity)
  {
    await _dbSet.AddAsync(entity);
  }

  public void Delete(T entity)
  {
    _dbSet.Remove(entity);
  }

  public void Update(T entity)
  {
    _dbSet.Update(entity);
  }

  public async Task SaveChangesAsync()
  {
    await _dbContext.SaveChangesAsync();
  }
}
