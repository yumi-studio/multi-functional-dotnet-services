using System;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace YumiStudio.Domain.Exceptions;

public class UserNotExistException : Exception
{
  public UserNotExistException() { }
  public UserNotExistException(string message) : base(message) { }
  public UserNotExistException(string message, Exception inner) : base(message, inner) { }
}
