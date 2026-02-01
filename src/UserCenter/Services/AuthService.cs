using System;
using Microsoft.EntityFrameworkCore;
using UserApi.Exceptions;
using UserApi.Features.AuthFeature;
using UserApi.Models;
using UserApi.Repositories.Interfaces;
using UserApi.Services.Interfaces;

namespace UserApi.Services;

public class AuthService(
  IUserRepository _userRepository
) : IAuthService
{
  public async Task<User> RegisterUser(RegisterFeatureRequest registerRequest)
  {
    User user = new()
    {
      Email = registerRequest.Email,
      Username = registerRequest.Username,
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
      FirstName = registerRequest.FirstName ?? "Unknown",
      LastName = registerRequest.LastName ?? "Unknown",
      Gender = registerRequest.Gender,
      BirthDate = registerRequest.BirthDate,
    };
    User? existUser = await _userRepository.GetDbSet()
      .Where(u => u.Email == user.Email || u.Username == user.Username)
      .FirstOrDefaultAsync();

    if (existUser != null && existUser.Email == registerRequest.Email)
      throw new Exception("User with the same email already exists.");
    if (existUser != null && existUser.Username == registerRequest.Username)
      throw new Exception("User with the same username already exists.");

    await _userRepository.AddAsync(user);
    await _userRepository.SaveChangesAsync();
    return user;
  }

  public async Task<User> AuthenticateUser(LoginFeatureRequest loginRequest)
  {
    User existUser = await _userRepository.GetUserByEmail(loginRequest.Email) ?? throw new UserNotExistException();
    return existUser;
  }
}
