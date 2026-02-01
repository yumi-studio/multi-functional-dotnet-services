using System;
using UserApi.Models;
using UserApi.Repositories.Interfaces;

namespace UserApi.Repositories;

public class UserExternalRepository(AppDbContext appDbContext) : BaseRepository<UserExternal, Guid>(appDbContext), IUserExternalRepository
{
}
