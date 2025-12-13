using Microsoft.EntityFrameworkCore;

namespace YumiStudio.YumiDotNet.Domain.Interfaces;

public interface IRepository<T, TKey>
where T : class
{
  public DbContext GetDbContext();
  public DbSet<T> GetDbSet();
  public Task<T?> GetByIdAsync(TKey id);
  public Task<IEnumerable<T>> GetAllAsync();
  public Task AddAsync(T entity);
  public void Update(T entity);
  public void Delete(T entity);
  public Task SaveChangesAsync();
}
