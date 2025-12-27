using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YumiStudio.Application.DTOs;
using YumiStudio.Application.Features.Auth.Login;
using YumiStudio.Application.Features.Auth.Register;
using YumiStudio.Application.Interfaces;
using YumiStudio.Common.Configurations;
using YumiStudio.Common.Constants;
using YumiStudio.Common.Helpers;
using YumiStudio.Domain.Entities;
using YumiStudio.Domain.Enums;
using YumiStudio.Domain.Interfaces;

namespace YumiStudio.API.Controllers;

[ApiController]
[Route("api/v1/auth", Name = "Authentication")]
public class AuthController(
  // ILogger<AuthController> _logger,
  IOptions<JwtConfiguration> _jwtConfig,
  CookiesManager _cookiesManager,
  IUserService _userService,
  ITokenRepository _tokenRepository,
  // IUserRepository _userRepository,
  IUserExternalRepository _userExternalRepository
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
    var user = await _userService.AuthenticateUser(loginRequest);

    string token = await GenerateJwtToken(user);
    _cookiesManager.SetCookie(CookieKeys.JWT_TOKEN, token, DateTimeOffset.UtcNow.AddDays(1));

    return OkResponse(new LoginResponse { Token = token });
  }

  [HttpPost("logout", Name = "UserLogout")]
  [Authorize]
  public async Task<IActionResult> Logout()
  {
    var jwtId = HttpContext.User.FindFirst(CustomClaims.JwtId)?.Value ?? throw new Exception("Token is not valid");

    if (jwtId != null)
    {
      var token = await _tokenRepository.GetByIdAsync(Guid.Parse(jwtId)) ?? throw new Exception("Token is not valid");
      _tokenRepository.Delete(token);
      await _tokenRepository.SaveChangesAsync();
    }
    else
    {
      throw new Exception("Token is not valid");
    }

    return OkResponse();
  }

  [AllowAnonymous]
  [HttpGet("external-providers", Name = "GetExternalProviders")]
  public IActionResult GetExternalProviders()
  {
    var providers = new Dictionary<int, string>
    {
      { ExternalProviders.Provider.Google.GetHashCode(), ExternalProviders.Provider.Google.ToString() },
      { ExternalProviders.Provider.Microsoft.GetHashCode(), ExternalProviders.Provider.Microsoft.ToString() },
      { ExternalProviders.Provider.Facebook.GetHashCode(), ExternalProviders.Provider.Facebook.ToString() },
      { ExternalProviders.Provider.Twitter.GetHashCode(), ExternalProviders.Provider.Twitter.ToString() },
      { ExternalProviders.Provider.GitHub.GetHashCode(), ExternalProviders.Provider.GitHub.ToString() },
      { ExternalProviders.Provider.LinkedIn.GetHashCode(), ExternalProviders.Provider.LinkedIn.ToString() },
    };

    return OkResponse(providers);
  }

  [AllowAnonymous]
  [HttpGet("login-external/{provider}", Name = "LoginExternal")]
  public IActionResult LoginExternal(ExternalProviders.Provider provider, [FromQuery] string? redirect)
  {
    var properties = new AuthenticationProperties
    {
      RedirectUri = $"/api/v1/auth/external-login-callback/{provider.GetHashCode()}?redirect={redirect}"
    };
    if (provider == ExternalProviders.Provider.Google)
      return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    else if (provider == ExternalProviders.Provider.Microsoft)
      return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
    else
      throw new Exception("Unsupported external authentication provider");
  }

  [AllowAnonymous]
  [HttpGet("external-login-callback/{provider}", Name = "ExternalLoginCallback")]
  public async Task<IActionResult> ExternalLoginCallback(ExternalProviders.Provider provider, [FromQuery] string? redirect)
  {
    AuthenticateResult? result = null;
    if (provider == ExternalProviders.Provider.Google)
    {
      result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
    }
    else if (provider == ExternalProviders.Provider.Microsoft)
    {
      result = await HttpContext.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);
    }
    else
    {
      throw new Exception("Unsupported external authentication provider");
    }

    if (!result.Succeeded)
      return Unauthorized();

    var claimsIdentity = new ClaimsIdentity(
      result.Principal.Claims,
      CookieAuthenticationDefaults.AuthenticationScheme
    );

    await HttpContext.SignInAsync(
      CookieAuthenticationDefaults.AuthenticationScheme,
      new ClaimsPrincipal(claimsIdentity)
    );

    var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
    if (email == null) return Unauthorized();

    var user = await _userService.GetUserByEmail(email);
    if (user == null)
    {
      // Auto-register the user
      var registerRequest = new RegisterRequest
      {
        Email = email,
        Username = email.Split('@')[0],
        Password = Guid.NewGuid().ToString(), // Random password
        FirstName = claimsIdentity.FindFirst(ClaimTypes.GivenName)?.Value ?? "Unknown",
        LastName = claimsIdentity.FindFirst(ClaimTypes.Surname)?.Value ?? "Unknown",
        Gender = Gender.Unknown,
        BirthDate = DateOnly.FromDateTime(DateTime.UtcNow),
      };
      user = await _userService.RegisterUser(registerRequest);
    }

    // Check if external auth is already linked
    var userExternal = await _userExternalRepository.GetDbSet().Where(ue => ue.UserId == user.Id && ue.Provider == provider)
      .FirstOrDefaultAsync();
    if (userExternal == null)
    {
      // Link external auth
      userExternal = new UserExternal
      {
        Id = Guid.NewGuid(),
        UserId = user.Id,
        Provider = provider,
        ProviderUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        LinkedAt = DateTimeOffset.UtcNow
      };
      await _userExternalRepository.AddAsync(userExternal);
      await _userExternalRepository.SaveChangesAsync();
    }

    string token = await GenerateJwtToken(user);
    _cookiesManager.SetCookie(CookieKeys.JWT_TOKEN, token, DateTimeOffset.UtcNow.AddDays(1));

    if (redirect != null)
    {
      return Redirect(redirect);
    }
    return OkResponse(new LoginResponse { Token = token });
  }
}
