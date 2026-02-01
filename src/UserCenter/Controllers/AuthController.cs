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
using UserApi.Configurations;
using UserApi.Constants;
using UserApi.Enums;
using UserApi.Features.AuthFeature;
using UserApi.Helpers;
using UserApi.Models;
using UserApi.Repositories.Interfaces;
using UserApi.Services.Interfaces;

namespace UserApi.Controllers;

[ApiController]
[Route("api/v1/auth", Name = "Authentication")]
public class AuthController(
  // ILogger<AuthController> _logger,
  IOptions<JwtConfiguration> _jwtConfig,
  CookiesManager _cookiesManager,
  IAuthService _authService,
  IUserRepository _userRepository,
  ITokenRepository _tokenRepository,
  IUserExternalRepository _userExternalRepository
) : GenericController
{
  private async Task<string> GenerateJwtToken(User user)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Value.Secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var newJwtId = Guid.NewGuid();

    var claims = new[]
    {
      new Claim(CustomClaims.JwtId, newJwtId.ToString()),
      new Claim(ClaimTypes.Name, user.Id.ToString())
    };

    var expireDateTime = DateTimeOffset.UtcNow.AddMinutes(60);

    var jwtToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
      issuer: _jwtConfig.Value.Issuer,
      audience: _jwtConfig.Value.Audience,
      claims: claims,
      expires: expireDateTime.UtcDateTime,
      signingCredentials: creds
    ));

    await _tokenRepository.AddAsync(new Token
    {
      Id = Guid.Parse(newJwtId.ToString()),
      UserId = user.Id,
      ExpiredAt = expireDateTime,
    });
    await _tokenRepository.SaveChangesAsync();

    return jwtToken;
  }

  [HttpPost("register", Name = "UserRegister")]
  [AllowAnonymous]
  public async Task<IActionResult> Register([FromBody] RegisterFeatureRequest request)
  {
    var newUser = await _authService.RegisterUser(request);

    return OkResponse(new RegisterFeatureResponse() { UserId = newUser.Id });
  }

  [HttpPost("login", Name = "UserLogin")]
  [AllowAnonymous]
  public async Task<IActionResult> Login([FromBody] LoginFeatureRequest request)
  {
    var user = await _authService.AuthenticateUser(request);

    string token = await GenerateJwtToken(user);
    _cookiesManager.SetCookie(CookieKeys.JWT_TOKEN, token, DateTimeOffset.UtcNow.AddDays(1));

    return OkResponse(new LoginFeatureResponse { Token = token });
  }

  [HttpPost("logout", Name = "UserLogout")]
  [Authorize]
  public async Task<IActionResult> Logout()
  {
    var jwtId = HttpContext.User.FindFirst(CustomClaims.JwtId)?.Value ?? throw new Exception("Token is not valid");
    var token = await _tokenRepository.GetByIdAsync(Guid.Parse(jwtId)) ?? throw new Exception("Token is not valid");

    _tokenRepository.Delete(token);
    await _tokenRepository.SaveChangesAsync();

    return OkResponse();
  }

  [AllowAnonymous]
  [HttpGet("external-providers", Name = "GetExternalProviders")]
  public IActionResult GetExternalProviders()
  {
    var providers = new Dictionary<int, string>
    {
      { AuthProvider.Google.GetHashCode(), AuthProvider.Google.ToString() },
      { AuthProvider.Microsoft.GetHashCode(), AuthProvider.Microsoft.ToString() },
      { AuthProvider.Facebook.GetHashCode(), AuthProvider.Facebook.ToString() },
      { AuthProvider.Twitter.GetHashCode(), AuthProvider.Twitter.ToString() },
      { AuthProvider.GitHub.GetHashCode(), AuthProvider.GitHub.ToString() },
      { AuthProvider.LinkedIn.GetHashCode(), AuthProvider.LinkedIn.ToString() },
    };

    return OkResponse(providers);
  }

  [AllowAnonymous]
  [HttpGet("login-external/{provider}", Name = "LoginExternal")]
  public IActionResult LoginExternal(AuthProvider provider, [FromQuery] string? redirect)
  {
    var properties = new AuthenticationProperties
    {
      RedirectUri = $"/api/v1/auth/external-login-callback/{provider.GetHashCode()}?redirect={redirect}"
    };
    if (provider == AuthProvider.Google)
      return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    else if (provider == AuthProvider.Microsoft)
      return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
    else
      throw new Exception("Unsupported external authentication provider");
  }

  [AllowAnonymous]
  [HttpGet("external-login-callback/{provider}", Name = "ExternalLoginCallback")]
  public async Task<IActionResult> ExternalLoginCallback(AuthProvider provider, [FromQuery] string? redirect)
  {
    AuthenticateResult? result = null;
    if (provider == AuthProvider.Google)
    {
      result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
    }
    else if (provider == AuthProvider.Microsoft)
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

    var user = await _userRepository.GetUserByEmail(email);
    if (user == null)
    {
      // Auto-register the user
      var registerRequest = new RegisterFeatureRequest
      {
        Email = email,
        Username = email.Split('@')[0],
        Password = Guid.NewGuid().ToString(), // Random password
        FirstName = claimsIdentity.FindFirst(ClaimTypes.GivenName)?.Value ?? "Unknown",
        LastName = claimsIdentity.FindFirst(ClaimTypes.Surname)?.Value ?? "Unknown",
        Gender = Gender.Unknown,
        BirthDate = DateOnly.FromDateTime(DateTime.UtcNow),
      };
      user = await _authService.RegisterUser(registerRequest);
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
    return OkResponse(new LoginFeatureResponse { Token = token });
  }
}
