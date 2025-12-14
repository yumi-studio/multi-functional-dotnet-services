using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YumiStudio.YumiDotNet.Application.DTOs;
using YumiStudio.YumiDotNet.Application.Features.Auth.Login;
using YumiStudio.YumiDotNet.Application.Features.Auth.Register;
using YumiStudio.YumiDotNet.Application.Interfaces;
using YumiStudio.YumiDotNet.Common.Configurations;
using YumiStudio.YumiDotNet.Common.Constants;
using YumiStudio.YumiDotNet.Common.Helpers;
using YumiStudio.YumiDotNet.Domain.Enums;
using YumiStudio.YumiDotNet.Domain.Interfaces;

namespace YumiStudio.YumiDotNet.API.Controllers;

[ApiController]
[Route("api/v1/auth", Name = "Authentication")]
public class AuthController(
  ILogger<AuthController> _logger,
  IOptions<JwtConfiguration> _jwtConfig,
  CookiesManager _cookiesManager,
  IUserService _userService,
  ITokenRepository _tokenRepository
) : GenericController
{

  private async Task<string> GenerateJwtToken(UserDto userDto)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Value.Secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var newJwtId = Guid.NewGuid();

    var claims = new[]
    {
      new Claim(CustomClaims.JwtId, newJwtId.ToString()),
      new Claim(ClaimTypes.Name, userDto.Id.ToString())
    };

    var expireDateTime = DateTimeOffset.UtcNow.AddMinutes(60);

    var jwtToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
      issuer: _jwtConfig.Value.Issuer,
      audience: _jwtConfig.Value.Audience,
      claims: claims,
      expires: expireDateTime.UtcDateTime,
      signingCredentials: creds
    ));

    await _tokenRepository.AddAsync(new Domain.Entities.Token
    {
      Id = Guid.Parse(newJwtId.ToString()),
      UserId = userDto.Id,
      ExpiredAt = expireDateTime,
    });
    await _tokenRepository.SaveChangesAsync();

    return jwtToken;
  }

  [HttpPost("register", Name = "UserRegister")]
  [AllowAnonymous]
  public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
  {
    UserDto newUser = await _userService.RegisterUser(registerRequest);

    return OkResponse(new RegisterResponse());
  }

  [HttpPost("login", Name = "UserLogin")]
  [AllowAnonymous]
  public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
  {
    UserDto? user = null;
    if (loginRequest.LoginType == LoginType.Username
      || loginRequest.LoginType == LoginType.Email
      || loginRequest.LoginType == LoginType.PhoneNumber)
    {
      user = await _userService.AuthenticateUser(loginRequest);
    }

    if (user == null)
    {
      return ErrorResponse(["Account not found or invalid credentials"]);
    }

    string token = await GenerateJwtToken(user);
    _cookiesManager.SetCookie(CookieKeys.JWT_TOKEN, token, DateTimeOffset.UtcNow.AddDays(1));

    return OkResponse(new LoginResponse { Token = token });
  }

  [HttpPost("logout", Name = "UserLogout")]
  [Authorize]
  public async Task<IActionResult> Logout()
  {
    var userId = Guid.Parse(HttpContext.User.Identity?.Name!);
    var jwtId = HttpContext.User.FindFirst(CustomClaims.JwtId)?.Value ?? throw new Exception("Token is not valid");

    if (jwtId != null)
    {
      var token = await _tokenRepository.GetByIdAsync(Guid.Parse(jwtId)) ?? throw new Exception("Token is not valid");
      _tokenRepository.Delete(token);
      await _tokenRepository.SaveChangesAsync();
    }

    return OkResponse(new { Message = "Logged out successfully" });
  }

  [HttpGet("login-types", Name = "UserAllLoginType")]
  public IActionResult AllLoginType()
  {
    return OkResponse<List<object>>([
      new { Type = LoginType.Username, Label = LoginType.Username.ToString() },
      new { Type = LoginType.Email, Label = LoginType.Email.ToString() },
      new { Type = LoginType.PhoneNumber, Label = LoginType.PhoneNumber.ToString() },
      new { Type = LoginType.Google, Label = LoginType.Google.ToString() },
      new { Type = LoginType.Facebook, Label = LoginType.Facebook.ToString() },
      new { Type = LoginType.Twitter, Label = LoginType.Twitter.ToString() },
      new { Type = LoginType.Microsoft, Label = LoginType.Microsoft.ToString() },
    ]);
  }
}
