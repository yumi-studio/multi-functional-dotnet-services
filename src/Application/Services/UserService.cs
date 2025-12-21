using Microsoft.EntityFrameworkCore;
using YumiStudio.Application.DTOs;
using YumiStudio.Application.Features.Auth.Login;
using YumiStudio.Application.Features.Auth.Register;
using YumiStudio.Application.Interfaces;
using YumiStudio.Domain.Entities;
using YumiStudio.Domain.Interfaces;

namespace YumiStudio.Application.Services;

public class UserService(
  IUserRepository _userRepository
) : IUserService
{
  private static UserDto GetUserDtoFromUser(User user)
  {
    return new UserDto
    {
      Id = user.Id,
      Username = user.Username,
      Email = user.Email,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Gender = user.Gender,
      BirthDate = user.BirthDate,
      Bio = user.Bio,
      Avatar = user.Avatar,
      JoinedAt = user.JoinedAt
    };
  }

  private static User GetUserByUserDto(UserDto user)
  {
    return new User
    {
      Id = user.Id,
      Username = user.Username,
      Email = user.Email,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Gender = user.Gender,
      BirthDate = user.BirthDate,
      Bio = user.Bio,
      Avatar = user.Avatar,
      JoinedAt = user.JoinedAt
    };
  }

  public async Task<IEnumerable<UserDto>> GetAllUsers()
  {
    List<UserDto> users = [];
    var userEntities = await _userRepository.GetAllAsync();
    foreach (User userEntity in userEntities)
    {
      users.Add(GetUserDtoFromUser(userEntity));
    }
    return users;
  }

  public async Task<UserDto?> GetUserById(Guid id)
  {
    User? userEntity = await _userRepository.GetByIdAsync(id);
    return userEntity != null ? GetUserDtoFromUser(userEntity) : null;
  }

  public async Task<UserDto?> GetUserByEmail(string email)
  {
    User? userEntity = await _userRepository.GetDbSet()
      .Where(u => u.Email == email)
      .FirstOrDefaultAsync();
    return userEntity != null ? GetUserDtoFromUser(userEntity) : null;
  }

  public async Task<UserDto> CreateUser(UserDto user)
  {
    User userEntity = GetUserByUserDto(user);
    await _userRepository.AddAsync(userEntity);
    return GetUserDtoFromUser(userEntity);
  }

  public async Task<UserDto> UpdateUser(UserDto user)
  {
    User userEntity = GetUserByUserDto(user);
    _userRepository.Update(userEntity);
    await _userRepository.SaveChangesAsync();
    return GetUserDtoFromUser(userEntity);
  }

  public async Task DeleteUser(UserDto user)
  {
    User userEntity = GetUserByUserDto(user);
    _userRepository.Delete(userEntity);
    await _userRepository.SaveChangesAsync();
    return;
  }

  public async Task<UserDto> RegisterUser(RegisterRequest registerRequest)
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
    return GetUserDtoFromUser(user);
  }

  public async Task<UserDto> AuthenticateUser(LoginRequest userLoginDto)
  {
    User? existUser = await _userRepository.GetDbSet().Where(u => u.Email == userLoginDto.Email).FirstOrDefaultAsync();

    return existUser == null ? throw new Exception("Account not exist.") : GetUserDtoFromUser(existUser);
  }
}
