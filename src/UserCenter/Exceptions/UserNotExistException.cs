using System;

namespace UserApi.Exceptions;

public class UserNotExistException : Exception
{
  public UserNotExistException() { }
  public UserNotExistException(string message) : base(message) { }
  public UserNotExistException(string message, Exception inner) : base(message, inner) { }
}
