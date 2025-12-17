using System;
using YumiStudio.Domain.Entities;

namespace YumiStudio.Domain.Interfaces;

public interface IFileUploadRepository : IRepository<FileUpload, Guid>
{
  public Task<IEnumerable<FileUpload>> GetAllDrafts();
}
