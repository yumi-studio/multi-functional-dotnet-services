using System;
using UserApi.Enums;
using UserApi.Models;

namespace UserApi.Repositories.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{
  public Task<UserExternal?> GetUserExternalAuthAsync(Guid userId, AuthProvider provider);

  public Task<User?> GetUserByEmail(string email);
}
