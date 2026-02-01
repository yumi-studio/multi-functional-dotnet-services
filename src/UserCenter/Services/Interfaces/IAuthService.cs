using System;
using UserApi.Features.AuthFeature;
using UserApi.Models;

namespace UserApi.Services.Interfaces;

public interface IAuthService
{
  public Task<User> RegisterUser(RegisterFeatureRequest request);
  public Task<User> AuthenticateUser(LoginFeatureRequest Request);
}
