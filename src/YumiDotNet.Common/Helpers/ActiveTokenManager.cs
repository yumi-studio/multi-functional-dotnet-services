using System;

namespace YumiStudio.YumiDotNet.Common.Helpers;

public class ActiveTokenManager
{
  private Dictionary<string, bool> Tokens { get; set; } = [];

  public void AddToken(string token)
  {
    Tokens.Add(token, true);
  }

  public bool IsTokenActive(string token)
  {
    return Tokens.ContainsKey(token) && Tokens[token];
  }

  public void DeactiveToken(string token)
  {
    Tokens[token] = false;
  }
}
