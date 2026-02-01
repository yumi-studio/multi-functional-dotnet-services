using System;
using UserApi.Infrastructure.Clients;
using UserApi.Models;
using UserApi.Repositories.Interfaces;
using UserApi.Services.Interfaces;

namespace UserApi.Services;

public class UserService(
  // IUserRepository _userRepository
) : IUserService
{
  public async Task<UserDTO> GetUserDTOFromUser(User user)
  {
    return new UserDTO
    {
      Id = user.Id,
      Username = user.Username,
      Email = user.Email,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Gender = user.Gender,
      BirthDate = user.BirthDate,
      Bio = user.Bio,
      Avatar = null,
      JoinedAt = user.JoinedAt
    };
  }
}
