using Microsoft.AspNetCore.Mvc;
using YumiStudio.Application.DTOs;
using YumiStudio.Application.Interfaces;
using YumiStudio.Common.Helpers;

namespace YumiStudio.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/users", Name = "AdminUser")]
[HasSystemAdminPermission]
public class AdminUserController(IUserService userService) : BaseAdminController
{

  private IUserService _userService = userService;

  private NotFoundObjectResult UserNotFound()
  {
    return NotFound(new ResponseDto<string>()
    {
      Success = false,
      Message = "User not found."
    });
  }

  [HttpGet(Name = "GetUsers")]
  public async Task<IActionResult> GetAll()
  {
    var users = await _userService.GetAllUsers();
    return Ok(new ResponseDto<IEnumerable<UserDto>>()
    {
      Success = true,
      Message = "Users retrieved successfully.",
      Data = users
    });
  }

  [HttpGet("{id}", Name = "GetUserById")]
  public async Task<IActionResult> GetById(Guid id)
  {
    var user = await _userService.GetUserById(id);
    if (user == null)
    {
      return UserNotFound();
    }

    return Ok(new ResponseDto<UserDto>()
    {
      Success = true,
      Message = "User retrieved successfully.",
      Data = user
    });
  }

  [HttpPost(Name = "CreateUser")]
  public async Task<IActionResult> Create([FromBody] UserDto user)
  {
    UserDto createdUser = await _userService.CreateUser(user);
    return Ok(new ResponseDto<UserDto>()
    {
      Success = true,
      Message = "User created successfully.",
      Data = createdUser
    });
  }

  [HttpPut("{id}", Name = "UpdateUser")]
  public async Task<IActionResult> Update(Guid id, [FromBody] UserDto user)
  {
    if (user == null || id != user.Id)
    {
      return BadRequest("Invalid user data.");
    }

    var updatedUser = await _userService.UpdateUser(user);
    if (updatedUser == null)
    {
      return UserNotFound();
    }

    return Ok(new ResponseDto<UserDto>()
    {
      Success = true,
      Message = "User updated successfully.",
      Data = updatedUser
    });
  }

  [HttpDelete("{id}", Name = "DeleteUser")]
  public async Task<IActionResult> Delete(Guid id)
  {
    var user = await _userService.GetUserById(id);
    if (user == null)
    {
      return UserNotFound();
    }

    await _userService.DeleteUser(user);
    return Ok(new ResponseDto<string>()
    {
      Success = true,
      Message = "User deleted successfully."
    });
  }

}
