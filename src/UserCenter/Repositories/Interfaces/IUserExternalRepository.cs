using System;
using UserApi.Models;

namespace UserApi.Repositories.Interfaces;

public interface IUserExternalRepository : IRepository<UserExternal, Guid>
{
}
