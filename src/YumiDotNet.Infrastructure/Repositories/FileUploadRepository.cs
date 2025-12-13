using System;
using Microsoft.EntityFrameworkCore;
using YumiStudio.YumiDotNet.Domain.Entities;
using YumiStudio.YumiDotNet.Domain.Interfaces;
using YumiStudio.YumiDotNet.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.YumiDotNet.Infrastructure.Repositories;

public class FileUploadRepository(AppDbContext appDbContext) : BaseRepository<FileUpload, Guid>(appDbContext), IFileUploadRepository
{
  public async Task<IEnumerable<FileUpload>> GetAllDrafts()
  {
    return await GetDbSet().Where(f => f.IsDraft == true).ToListAsync();
  }
}
