using System;
using Fakebook.Enums;
using Fakebook.Infrastructure.UploadMethods;

namespace Fakebook.Infrastructure.Resolvers;

public interface IUploadMethodResolver
{
  IUploadMethod Resolve(UploadMethod method);
}
