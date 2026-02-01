using System;
using Fakebook.Enums;
using Fakebook.Infrastructure.UploadMethods;

namespace Fakebook.Infrastructure.Resolvers;

public class UploadMethodResolver(
  IEnumerable<IUploadMethod> uploaders
) : IUploadMethodResolver
{
  private readonly IEnumerable<IUploadMethod> _uploaders = uploaders;

  public IUploadMethod Resolve(UploadMethod method)
  {
    return _uploaders.First(u => u.CanHandle(method));
  }
}
