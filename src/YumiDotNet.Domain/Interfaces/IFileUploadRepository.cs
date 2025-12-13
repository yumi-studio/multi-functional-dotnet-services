using System;
using YumiStudio.YumiDotNet.Domain.Entities;

namespace YumiStudio.YumiDotNet.Domain.Interfaces;

public interface IFileUploadRepository : IRepository<FileUpload, Guid>
{
  public Task<IEnumerable<FileUpload>> GetAllDrafts();
}
