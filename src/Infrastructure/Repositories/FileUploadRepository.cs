using System;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Domain.Entities;
using YumiStudio.Domain.Interfaces;
using YumiStudio.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.Infrastructure.Repositories;

public class FileUploadRepository(AppDbContext appDbContext) : BaseRepository<FileUpload, Guid>(appDbContext), IFileUploadRepository
{
  public async Task<IEnumerable<FileUpload>> GetAllDrafts()
  {
    return await GetDbSet().Where(f => f.IsDraft == true).ToListAsync();
  }
}
