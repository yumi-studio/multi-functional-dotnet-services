using System;

namespace YumiStudio.YumiDotNet.Domain.Exceptions;

public class UnauthorizedException : Exception
{
  public UnauthorizedException() { }
  public UnauthorizedException(string message) : base(message) { }
  public UnauthorizedException(string message, Exception inner) : base(message, inner) { }
}
