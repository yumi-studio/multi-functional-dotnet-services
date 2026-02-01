using UserApi.Features.AuthFeature;
using UserApi.Models;

namespace UserApi.Services.Interfaces;

public interface IUserService
{
  public Task<UserDTO> GetUserDTOFromUser(User user);
}
