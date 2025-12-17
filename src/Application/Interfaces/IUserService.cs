using YumiStudio.Application.DTOs;
using YumiStudio.Application.Features.Auth.Login;
using YumiStudio.Application.Features.Auth.Register;

namespace YumiStudio.Application.Interfaces;

public interface IUserService
{
  public Task<IEnumerable<UserDto>> GetAllUsers();
  public Task<UserDto?> GetUserById(Guid id);
  public Task<UserDto?> GetUserByEmail(string id);
  public Task<UserDto> CreateUser(UserDto user);
  public Task<UserDto> UpdateUser(UserDto user);
  public Task DeleteUser(UserDto user);
  public Task<UserDto> RegisterUser(RegisterRequest registerRequest);
  public Task<UserDto> AuthenticateUser(LoginRequest loginRequest);
}

