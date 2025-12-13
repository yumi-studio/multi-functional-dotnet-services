using YumiStudio.YumiDotNet.Application.DTOs;
using YumiStudio.YumiDotNet.Application.Features.Auth.Login;
using YumiStudio.YumiDotNet.Application.Features.Auth.Register;

namespace YumiStudio.YumiDotNet.Application.Interfaces;

public interface IUserService
{
  public Task<IEnumerable<UserDto>> GetAllUsers();
  public Task<UserDto?> GetUserById(Guid id);
  public Task<UserDto> CreateUser(UserDto user);
  public Task<UserDto> UpdateUser(UserDto user);
  public Task DeleteUser(UserDto user);
  public Task<UserDto> RegisterUser(RegisterRequest registerRequest);
  public Task<UserDto> AuthenticateUser(LoginRequest loginRequest);
}

