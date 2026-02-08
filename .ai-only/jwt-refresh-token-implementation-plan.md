# JWT Refresh Token Implementation Plan - UserCenter Service

**Last Updated:** February 8, 2026  
**Service:** UserCenter  
**Feature:** JWT Token Refresh Mechanism  
**Status:** Planning

---

## Table of Contents

1. [Overview](#overview)
2. [Current State Analysis](#current-state-analysis)
3. [Architecture Design](#architecture-design)
4. [Implementation Changes](#implementation-changes)
5. [Database Schema Changes](#database-schema-changes)
6. [Code Changes Summary](#code-changes-summary)
7. [Step-by-Step Implementation Guide](#step-by-step-implementation-guide)
8. [Testing Strategy](#testing-strategy)
9. [Security Considerations](#security-considerations)

---

## Overview

### Objective
Implement JWT token refresh mechanism using refresh tokens to enable long-lived sessions without exposing long-lived access tokens.

### Benefits
- **Enhanced Security**: Short-lived access tokens reduce exposure window
- **Better UX**: Users don't need to re-login when access token expires
- **Token Rotation**: Ability to rotate tokens without user intervention
- **Logout Control**: Force logout across all devices by invalidating refresh tokens
- **Fine-grained Control**: Different expiration times for access and refresh tokens

### Implementation Approach
- **Access Token**: 15-30 minutes validity (short-lived)
- **Refresh Token**: 7-30 days validity (long-lived)
- **Database Storage**: Store refresh tokens with user association
- **Secure Rotation**: Optional token rotation on refresh

---

## Current State Analysis

### Existing JWT Implementation

**Current Token Model (Limited)**
```csharp
[Table("tokens")]
public class Token
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public DateTimeOffset ExpiredAt { get; set; }
  public User User { get; set; }
}
```

**Issues with Current Implementation**
- Single token model used for both access token tracking and blacklist
- No distinction between access and refresh tokens
- No support for token rotation
- Token expiration is 60 minutes (too long for access token)

### Current JWT Configuration
```csharp
public class JwtConfiguration
{
  public required string Issuer { get; set; }
  public required string Audience { get; set; }
  public required string Secret { get; set; }
}
```

**Missing Configuration**
- No separate refresh token expiration time
- No refresh token secret (can use same as access token)
- No token rotation configuration

### Current Authentication Flow
1. User registers
2. User logs in with credentials
3. Access token generated and stored in token blacklist table
4. Token returned in response and cookie
5. Token validated on each request via middleware
6. On logout, token added to blacklist and deleted

---

## Architecture Design

### Token Type Separation

Instead of using one Token model, we'll separate concerns:

```
Access Token (JWT)
├─ Short lived (15-30 minutes)
├─ Contains user claims
├─ Verified on each request
└─ Stored in Token table for blacklist

Refresh Token (Long-lived secret)
├─ Long lived (7-30 days)
├─ Not verified as JWT
├─ Stored securely in database
└─ Stored in RefreshToken table
```

### Token Management Database Schema

#### Token Table (Modified/Enhanced)
Purpose: Blacklist management for logout
- `id` (Primary Key)
- `user_id` (Foreign Key)
- `jwt_id` (Claims identifier)
- `access_token_hash` (Hash of access token)
- `expired_at` (Expiration time)
- `revoked_at` (When explicitly revoked)
- `created_at` (Creation timestamp)

#### RefreshToken Table (New)
Purpose: Manage refresh token lifecycle
- `id` (Primary Key)
- `user_id` (Foreign Key)
- `token_hash` (Hashed refresh token)
- `expires_at` (Expiration time)
- `created_at` (Creation timestamp)
- `revoked_at` (When revoked/rotated)
- `ip_address` (Optional: for tracking)
- `user_agent` (Optional: for tracking)
- `is_active` (Flag for quick lookup)

### Authentication Flow (New)

```
┌─────────────────────────────────────────────────────────────┐
│           LOGIN ENDPOINT                                    │
└─────────────────────────────────────────────────────────────┘
  ↓
1. Validate credentials
  ↓
2. Generate Access Token (15-30 min expiry)
  ↓
3. Generate Refresh Token (7-30 days expiry)
  ↓
4. Store Refresh Token in database
  ↓
5. Store Access Token in blacklist (for logout tracking)
  ↓
6. Return both tokens to client
  ├── Access Token in response body / cookie
  └── Refresh Token in HTTP-only secure cookie (or storage based on policy)

┌─────────────────────────────────────────────────────────────┐
│           PROTECTED ENDPOINT (with Access Token)            │
└─────────────────────────────────────────────────────────────┘
  ↓
1. Validate Access Token JWT signature & claims
  ↓
2. Check if token in blacklist (revoked)
  ↓
3. If valid → Allow request
  ↓
4. If expired/invalid → Return 401 Unauthorized
  └── Client should request new Access Token using Refresh Token

┌─────────────────────────────────────────────────────────────┐
│           REFRESH TOKEN ENDPOINT                            │
└─────────────────────────────────────────────────────────────┘
  ↓
1. Receive Refresh Token from client
  ↓
2. Validate Refresh Token exists, not expired, not revoked
  ↓
3. Verify token hash matches database
  ↓
4. Generate new Access Token
  ↓
5. Optionally: Rotate Refresh Token (revoke old, create new)
  ↓
6. Return new Access Token (and new Refresh Token if rotated)

┌─────────────────────────────────────────────────────────────┐
│           LOGOUT ENDPOINT                                   │
└─────────────────────────────────────────────────────────────┘
  ↓
1. Receive Access Token from request
  ↓
2. Add Access Token to blacklist
  ↓
3. Revoke associated Refresh Token
  ↓
4. Clear cookies on client
  ↓
5. Return success
```

---

## Implementation Changes

### 1. Model Changes

#### New: RefreshToken Entity
**File**: `Models/RefreshToken.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserApi.Models;

[Table("refresh_tokens")]
public class RefreshToken
{
  [Key]
  [Column("id")]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  [Column("user_id")]
  public Guid UserId { get; set; }

  [Required]
  [MaxLength(500)]
  [Column("token_hash")]
  public string TokenHash { get; set; } = string.Empty;

  [Required]
  [Column("expires_at")]
  public DateTimeOffset ExpiresAt { get; set; }

  [Column("created_at")]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  [Column("revoked_at")]
  public DateTimeOffset? RevokedAt { get; set; }

  [Column("ip_address")]
  public string? IpAddress { get; set; }

  [Column("user_agent")]
  public string? UserAgent { get; set; }

  [Column("is_active")]
  public bool IsActive { get; set; } = true;

  [ForeignKey(nameof(UserId))]
  public User User { get; set; } = null!;
}
```

#### Updated: Token Entity
**File**: `Models/Token.cs` (Enhanced)

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserApi.Models;

[Table("tokens")]
public class Token
{
  [Key]
  [Column("id")]
  public Guid Id { get; set; }

  [Required]
  [Column("user_id")]
  public Guid UserId { get; set; }

  [Required]
  [MaxLength(500)]
  [Column("jwt_id")]
  public string JwtId { get; set; } = string.Empty;

  [Column("expired_at")]
  public DateTimeOffset ExpiredAt { get; set; }

  [Column("revoked_at")]
  public DateTimeOffset? RevokedAt { get; set; }

  [Column("created_at")]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  public User User { get; set; } = null!;
}
```

#### New: RefreshTokenRequest DTO
**File**: `Features/AuthFeature/RefreshTokenFeature.cs`

```csharp
using System;

namespace UserApi.Features.AuthFeature;

public class RefreshTokenRequest
{
  public required string RefreshToken { get; set; }
}

public class RefreshTokenResponse
{
  public required string AccessToken { get; set; }
  public required string RefreshToken { get; set; }
  public int ExpiresIn { get; set; } = 900; // 15 minutes in seconds
}
```

#### Updated: LoginFeatureResponse DTO
**File**: `Features/AuthFeature/LoginFeature.cs` (Updated)

```csharp
public class LoginFeatureResponse
{
  public required string AccessToken { get; set; }
  public required string RefreshToken { get; set; }
  public int ExpiresIn { get; set; } = 900; // 15 minutes in seconds
  public int RefreshExpiresIn { get; set; } = 2592000; // 30 days in seconds
}
```

### 2. Configuration Changes

#### Updated: JwtConfiguration
**File**: `Configurations/JwtConfiguration.cs`

```csharp
using System;

namespace UserApi.Configurations;

public class JwtConfiguration
{
  public required string Issuer { get; set; }
  public required string Audience { get; set; }
  public required string Secret { get; set; }
  
  // New properties for refresh token support
  public int AccessTokenExiryMinutes { get; set; } = 15; // Default 15 minutes
  public int RefreshTokenExpiryDays { get; set; } = 30;  // Default 30 days
  public string? RefreshTokenSecret { get; set; } // Optional: separate secret for refresh tokens
  public bool EnableTokenRotation { get; set; } = true; // Rotate refresh token on each use
}
```

#### Updated: appsettings.json

```json
{
  "Jwt": {
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "Secret": "your-super-secret-key-with-at-least-32-characters",
    "AccessTokenExiryMinutes": 15,
    "RefreshTokenExpiryDays": 30,
    "RefreshTokenSecret": "your-refresh-token-secret-or-null-to-use-main-secret",
    "EnableTokenRotation": true
  }
}
```

### 3. Repository Changes

#### New: IRefreshTokenRepository Interface
**File**: `Repositories/Interfaces/IRefreshTokenRepository.cs`

```csharp
using System;
using UserApi.Models;

namespace UserApi.Repositories.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
  Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
  Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId);
  Task<IEnumerable<RefreshToken>> GetAllActiveByUserIdAsync(Guid userId);
  Task RevokeByUserIdAsync(Guid userId);
  Task RevokeOlderTokensAsync(Guid userId, DateTimeOffset beforeDate);
  Task<bool> IsTokenValidAsync(string tokenHash);
}
```

#### New: RefreshTokenRepository Implementation
**File**: `Repositories/RefreshTokenRepository.cs`

```csharp
using System;
using Microsoft.EntityFrameworkCore;
using UserApi.Models;
using UserApi.Repositories.Interfaces;

namespace UserApi.Repositories;

public class RefreshTokenRepository(AppDbContext _context) 
  : BaseRepository<RefreshToken>(_context), IRefreshTokenRepository
{
  public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
  {
    return await GetDbSet()
      .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.IsActive);
  }

  public async Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId)
  {
    return await GetDbSet()
      .Where(rt => rt.UserId == userId && rt.IsActive && rt.ExpiresAt > DateTimeOffset.UtcNow)
      .OrderByDescending(rt => rt.CreatedAt)
      .FirstOrDefaultAsync();
  }

  public async Task<IEnumerable<RefreshToken>> GetAllActiveByUserIdAsync(Guid userId)
  {
    return await GetDbSet()
      .Where(rt => rt.UserId == userId && rt.IsActive && rt.ExpiresAt > DateTimeOffset.UtcNow)
      .ToListAsync();
  }

  public async Task RevokeByUserIdAsync(Guid userId)
  {
    var tokens = await GetDbSet()
      .Where(rt => rt.UserId == userId && rt.IsActive)
      .ToListAsync();

    foreach (var token in tokens)
    {
      token.IsActive = false;
      token.RevokedAt = DateTimeOffset.UtcNow;
    }

    await SaveChangesAsync();
  }

  public async Task RevokeOlderTokensAsync(Guid userId, DateTimeOffset beforeDate)
  {
    var tokens = await GetDbSet()
      .Where(rt => rt.UserId == userId && rt.CreatedAt < beforeDate && rt.IsActive)
      .ToListAsync();

    foreach (var token in tokens)
    {
      token.IsActive = false;
      token.RevokedAt = DateTimeOffset.UtcNow;
    }

    await SaveChangesAsync();
  }

  public async Task<bool> IsTokenValidAsync(string tokenHash)
  {
    return await GetDbSet()
      .AnyAsync(rt => rt.TokenHash == tokenHash 
        && rt.IsActive 
        && rt.ExpiresAt > DateTimeOffset.UtcNow);
  }
}
```

#### Updated: ITokenRepository Interface
**File**: `Repositories/Interfaces/ITokenRepository.cs`

```csharp
using System;
using UserApi.Models;

namespace UserApi.Repositories.Interfaces;

public interface ITokenRepository : IRepository<Token>
{
  Task<Token?> GetByJwtIdAsync(string jwtId);
  Task RevokeByUserIdAsync(Guid userId);
  Task<bool> IsTokenBlacklistedAsync(string jwtId);
}
```

#### Updated: TokenRepository Implementation
**File**: `Repositories/TokenRepository.cs`

```csharp
using System;
using Microsoft.EntityFrameworkCore;
using UserApi.Models;
using UserApi.Repositories.Interfaces;

namespace UserApi.Repositories;

public class TokenRepository(AppDbContext _context) 
  : BaseRepository<Token>(_context), ITokenRepository
{
  public async Task<Token?> GetByJwtIdAsync(string jwtId)
  {
    return await GetDbSet()
      .FirstOrDefaultAsync(t => t.JwtId == jwtId);
  }

  public async Task RevokeByUserIdAsync(Guid userId)
  {
    var tokens = await GetDbSet()
      .Where(t => t.UserId == userId && t.RevokedAt == null)
      .ToListAsync();

    foreach (var token in tokens)
    {
      token.RevokedAt = DateTimeOffset.UtcNow;
    }

    await SaveChangesAsync();
  }

  public async Task<bool> IsTokenBlacklistedAsync(string jwtId)
  {
    return await GetDbSet()
      .AnyAsync(t => t.JwtId == jwtId && t.RevokedAt != null);
  }
}
```

### 4. Service Changes

#### Updated: IAuthService Interface
**File**: `Services/Interfaces/IAuthService.cs`

```csharp
using System;
using UserApi.Features.AuthFeature;
using UserApi.Models;

namespace UserApi.Services.Interfaces;

public interface IAuthService
{
  Task<User> RegisterUser(RegisterFeatureRequest request);
  Task<User> AuthenticateUser(LoginFeatureRequest request);
  Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user);
  Task<string> RefreshAccessTokenAsync(string refreshToken);
  Task<(string accessToken, string refreshToken)> RefreshTokensAsync(string refreshToken);
  Task RevokeRefreshTokenAsync(string refreshToken);
  Task RevokeUserTokensAsync(Guid userId);
}
```

#### Updated: AuthService Implementation
**File**: `Services/AuthService.cs`

```csharp
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserApi.Configurations;
using UserApi.Exceptions;
using UserApi.Features.AuthFeature;
using UserApi.Models;
using UserApi.Repositories.Interfaces;
using UserApi.Services.Interfaces;

namespace UserApi.Services;

public class AuthService(
  IUserRepository _userRepository,
  ITokenRepository _tokenRepository,
  IRefreshTokenRepository _refreshTokenRepository,
  IOptions<JwtConfiguration> _jwtConfig
) : IAuthService
{
  public async Task<User> RegisterUser(RegisterFeatureRequest registerRequest)
  {
    var user = new User
    {
      Email = registerRequest.Email,
      Username = registerRequest.Username,
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
      FirstName = registerRequest.FirstName ?? "Unknown",
      LastName = registerRequest.LastName ?? "Unknown",
      Gender = registerRequest.Gender,
      BirthDate = registerRequest.BirthDate,
    };

    var existUser = await _userRepository.GetDbSet()
      .Where(u => u.Email == user.Email || u.Username == user.Username)
      .FirstOrDefaultAsync();

    if (existUser != null && existUser.Email == registerRequest.Email)
      throw new Exception("User with the same email already exists.");
    if (existUser != null && existUser.Username == registerRequest.Username)
      throw new Exception("User with the same username already exists.");

    await _userRepository.AddAsync(user);
    await _userRepository.SaveChangesAsync();
    return user;
  }

  public async Task<User> AuthenticateUser(LoginFeatureRequest loginRequest)
  {
    var existUser = await _userRepository.GetUserByEmail(loginRequest.Email)
      ?? throw new UserNotExistException();
    
    // Verify password
    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, existUser.PasswordHash);
    if (!isPasswordValid)
      throw new Exception("Invalid password");
      
    return existUser;
  }

  public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user)
  {
    var accessToken = await GenerateAccessTokenAsync(user);
    var refreshToken = await GenerateRefreshTokenAsync(user);
    
    return (accessToken, refreshToken);
  }

  public async Task<string> RefreshAccessTokenAsync(string refreshToken)
  {
    var tokenHash = HashToken(refreshToken);
    var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash)
      ?? throw new Exception("Invalid refresh token");

    if (!storedToken.IsActive || storedToken.ExpiresAt <= DateTimeOffset.UtcNow)
      throw new Exception("Refresh token has expired or been revoked");

    var user = await _userRepository.GetByIdAsync(storedToken.UserId)
      ?? throw new UserNotExistException();

    return await GenerateAccessTokenAsync(user);
  }

  public async Task<(string accessToken, string refreshToken)> RefreshTokensAsync(string refreshToken)
  {
    var tokenHash = HashToken(refreshToken);
    var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash)
      ?? throw new Exception("Invalid refresh token");

    if (!storedToken.IsActive || storedToken.ExpiresAt <= DateTimeOffset.UtcNow)
      throw new Exception("Refresh token has expired or been revoked");

    var user = await _userRepository.GetByIdAsync(storedToken.UserId)
      ?? throw new UserNotExistException();

    var newAccessToken = await GenerateAccessTokenAsync(user);
    
    string newRefreshToken = refreshToken;
    if (_jwtConfig.Value.EnableTokenRotation)
    {
      // Revoke old token and create new one
      storedToken.IsActive = false;
      storedToken.RevokedAt = DateTimeOffset.UtcNow;
      await _refreshTokenRepository.UpdateAsync(storedToken);
      
      newRefreshToken = await GenerateRefreshTokenAsync(user);
    }

    return (newAccessToken, newRefreshToken);
  }

  public async Task RevokeRefreshTokenAsync(string refreshToken)
  {
    var tokenHash = HashToken(refreshToken);
    var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
    
    if (storedToken != null)
    {
      storedToken.IsActive = false;
      storedToken.RevokedAt = DateTimeOffset.UtcNow;
      await _refreshTokenRepository.UpdateAsync(storedToken);
    }
  }

  public async Task RevokeUserTokensAsync(Guid userId)
  {
    await _refreshTokenRepository.RevokeByUserIdAsync(userId);
    await _tokenRepository.RevokeByUserIdAsync(userId);
  }

  private async Task<string> GenerateAccessTokenAsync(User user)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Value.Secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var jwtId = Guid.NewGuid().ToString();

    var claims = new[]
    {
      new System.Security.Claims.Claim("sub", user.Id.ToString()),
      new System.Security.Claims.Claim("name", user.Username),
      new System.Security.Claims.Claim("email", user.Email),
      new System.Security.Claims.Claim("jti", jwtId)
    };

    var expiry = DateTimeOffset.UtcNow.AddMinutes(_jwtConfig.Value.AccessTokenExiryMinutes);

    var token = new JwtSecurityTokenHandler().WriteToken(
      new JwtSecurityToken(
        issuer: _jwtConfig.Value.Issuer,
        audience: _jwtConfig.Value.Audience,
        claims: claims,
        expires: expiry.UtcDateTime,
        signingCredentials: creds
      )
    );

    // Store in blacklist table for logout tracking
    await _tokenRepository.AddAsync(new Token
    {
      Id = Guid.Parse(jwtId),
      UserId = user.Id,
      JwtId = jwtId,
      ExpiredAt = expiry,
      CreatedAt = DateTimeOffset.UtcNow
    });
    await _tokenRepository.SaveChangesAsync();

    return token;
  }

  private async Task<string> GenerateRefreshTokenAsync(User user)
  {
    var tokenBytes = new byte[64];
    using (var rng = RandomNumberGenerator.Create())
    {
      rng.GetBytes(tokenBytes);
    }
    var refreshToken = Convert.ToBase64String(tokenBytes);
    var tokenHash = HashToken(refreshToken);

    var expiry = DateTimeOffset.UtcNow.AddDays(_jwtConfig.Value.RefreshTokenExpiryDays);

    await _refreshTokenRepository.AddAsync(new RefreshToken
    {
      UserId = user.Id,
      TokenHash = tokenHash,
      ExpiresAt = expiry,
      CreatedAt = DateTimeOffset.UtcNow,
      IsActive = true
    });
    await _refreshTokenRepository.SaveChangesAsync();

    return refreshToken;
  }

  private static string HashToken(string token)
  {
    using (var sha256 = System.Security.Cryptography.SHA256.Create())
    {
      var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
      return Convert.ToBase64String(hashedBytes);
    }
  }
}
```

### 5. Controller Changes

#### Updated: AuthController
**File**: `Controllers/AuthController.cs`

Key changes:
1. Update `GenerateJwtToken()` to use new token generation methods
2. Update `Login()` endpoint to return both access and refresh tokens
3. Add new `RefreshAccessToken()` endpoint
4. Add new `RefreshTokens()` endpoint
5. Update `Logout()` to revoke both tokens

```csharp
[HttpPost("login", Name = "UserLogin")]
[AllowAnonymous]
public async Task<IActionResult> Login([FromBody] LoginFeatureRequest request)
{
  var user = await _authService.AuthenticateUser(request);
  var (accessToken, refreshToken) = await _authService.GenerateTokensAsync(user);

  _cookiesManager.SetCookie(CookieKeys.JWT_TOKEN, accessToken, 
    DateTimeOffset.UtcNow.AddMinutes(_jwtConfig.Value.AccessTokenExiryMinutes));
  _cookiesManager.SetCookie("refresh_token", refreshToken, 
    DateTimeOffset.UtcNow.AddDays(_jwtConfig.Value.RefreshTokenExpiryDays));

  return OkResponse(new LoginFeatureResponse 
  { 
    AccessToken = accessToken,
    RefreshToken = refreshToken,
    ExpiresIn = _jwtConfig.Value.AccessTokenExiryMinutes * 60,
    RefreshExpiresIn = _jwtConfig.Value.RefreshTokenExpiryDays * 86400
  });
}

[HttpPost("refresh", Name = "RefreshToken")]
[AllowAnonymous]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
  try
  {
    var (newAccessToken, newRefreshToken) = await _authService.RefreshTokensAsync(request.RefreshToken);

    _cookiesManager.SetCookie(CookieKeys.JWT_TOKEN, newAccessToken, 
      DateTimeOffset.UtcNow.AddMinutes(_jwtConfig.Value.AccessTokenExiryMinutes));
    _cookiesManager.SetCookie("refresh_token", newRefreshToken, 
      DateTimeOffset.UtcNow.AddDays(_jwtConfig.Value.RefreshTokenExpiryDays));

    return OkResponse(new RefreshTokenResponse
    {
      AccessToken = newAccessToken,
      RefreshToken = newRefreshToken,
      ExpiresIn = _jwtConfig.Value.AccessTokenExiryMinutes * 60
    });
  }
  catch (Exception ex)
  {
    return UnauthorizedResponse(ex.Message);
  }
}

[HttpPost("logout", Name = "UserLogout")]
[Authorize]
public async Task<IActionResult> Logout()
{
  var userId = HttpContext.User.FindFirst("sub")?.Value ?? throw new Exception("Token is not valid");
  
  if (Guid.TryParse(userId, out var userIdGuid))
  {
    await _authService.RevokeUserTokensAsync(userIdGuid);
  }

  _cookiesManager.DeleteCookie(CookieKeys.JWT_TOKEN);
  _cookiesManager.DeleteCookie("refresh_token");

  return OkResponse();
}
```

### 6. Middleware Updates

#### Updated: CheckTokenBlacklistMiddleware
**File**: `Middlewares/CheckTokenBlacklistMiddleware.cs`

```csharp
public class CheckTokenBlacklistMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
{
  public async Task InvokeAsync(HttpContext context)
  {
    var token = context.Request.Cookies["JWT_TOKEN"];

    if (!string.IsNullOrEmpty(token))
    {
      var handler = new JwtSecurityTokenHandler();
      var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

      if (jwtToken != null)
      {
        var jwtId = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

        if (!string.IsNullOrEmpty(jwtId))
        {
          using (var scope = serviceProvider.CreateScope())
          {
            var tokenRepository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
            var isBlacklisted = await tokenRepository.IsTokenBlacklistedAsync(jwtId);

            if (isBlacklisted)
            {
              context.Response.StatusCode = StatusCodes.Status401Unauthorized;
              await context.Response.WriteAsJsonAsync(new { error = "Token has been revoked" });
              return;
            }
          }
        }
      }
    }

    await next(context);
  }
}
```

### 7. Program.cs Updates

```csharp
// Add RefreshToken repository registration
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Update TokenRepository if using new interface
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
```

---

## Database Schema Changes

### Migration: Add RefreshToken Table and Update Token Table

**Migration Name**: `AddRefreshTokenSupport`

#### Changes:
1. Create new `refresh_tokens` table
2. Add columns to `tokens` table
3. Create indexes for performance

**SQL Script Overview**:
```sql
-- New refresh_tokens table
CREATE TABLE refresh_tokens (
  id CHAR(36) NOT NULL PRIMARY KEY,
  user_id CHAR(36) NOT NULL,
  token_hash VARCHAR(500) NOT NULL,
  expires_at DATETIME(6) NOT NULL,
  created_at DATETIME(6) NOT NULL,
  revoked_at DATETIME(6) NULL,
  ip_address VARCHAR(45) NULL,
  user_agent VARCHAR(500) NULL,
  is_active TINYINT NOT NULL DEFAULT 1,
  FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
  INDEX idx_user_active (user_id, is_active),
  INDEX idx_expires (expires_at)
);

-- Update tokens table
ALTER TABLE tokens 
ADD COLUMN jwt_id VARCHAR(500) NOT NULL AFTER id,
ADD COLUMN revoked_at DATETIME(6) NULL AFTER expired_at,
ADD COLUMN created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER revoked_at,
ADD UNIQUE INDEX idx_jwt_id (jwt_id),
ADD INDEX idx_user_id (user_id),
ADD INDEX idx_revoked (revoked_at);
```

Create migration using EF Core:
```bash
dotnet ef migrations add AddRefreshTokenSupport
```

---

## Code Changes Summary

### Files to Create
1. `Models/RefreshToken.cs` - ✅ New
2. `Features/AuthFeature/RefreshTokenFeature.cs` - ✅ New
3. `Repositories/Interfaces/IRefreshTokenRepository.cs` - ✅ New
4. `Repositories/RefreshTokenRepository.cs` - ✅ New

### Files to Update
1. `Configurations/JwtConfiguration.cs` - ✅ Enhanced
2. `Models/Token.cs` - ✅ Enhanced
3. `Models/AppDbContext.cs` - ✅ Add DbSet for RefreshToken
4. `Services/Interfaces/IAuthService.cs` - ✅ New methods
5. `Services/AuthService.cs` - ✅ Complete rewrite
6. `Repositories/Interfaces/ITokenRepository.cs` - ✅ New methods
7. `Repositories/TokenRepository.cs` - ✅ New methods
8. `Controllers/AuthController.cs` - ✅ New endpoints
9. `Middlewares/CheckTokenBlacklistMiddleware.cs` - ✅ Updated
10. `Program.cs` - ✅ Register RefreshTokenRepository
11. `appsettings.json` - ✅ Add JWT config
12. `Features/AuthFeature/LoginFeature.cs` - ✅ Update response

### Database Changes
1. Create migration for RefreshToken table
2. Add columns to Token table

---

## Step-by-Step Implementation Guide

### Phase 1: Database Setup (30 minutes)

1. **Create RefreshToken Model**
   - Create `Models/RefreshToken.cs`
   - Define entity with all properties
   - Add table and column attributes

2. **Update Token Model**
   - Add `JwtId`, `RevokedAt`, `CreatedAt` columns
   - Update property mapping

3. **Update AppDbContext**
   - Add `DbSet<RefreshToken> RefreshTokens` property
   - Add model configuration in `OnModelCreating()`

4. **Create Database Migration**
   ```bash
   dotnet ef migrations add AddRefreshTokenSupport
   dotnet ef database update
   ```

### Phase 2: Service Layer (45 minutes)

1. **Create RefreshToken Repository**
   - Create `IRefreshTokenRepository` interface
   - Implement `RefreshTokenRepository` class
   - Add query methods

2. **Update Auth Service Interface**
   - Add new method signatures for token operations

3. **Implement New Auth Service**
   - Implement token generation methods
   - Implement refresh token validation
   - Implement token revocation logic

4. **Update Token Repository**
   - Add blacklist checking methods
   - Add revocation methods

### Phase 3: API Layer (45 minutes)

1. **Create DTOs**
   - Create `RefreshTokenFeature.cs` (request/response)
   - Update `LoginFeature.cs` response

2. **Update JwtConfiguration**
   - Add new config properties

3. **Update AuthController**
   - Update `/login` endpoint
   - Add `/refresh` endpoint
   - Update `/logout` endpoint
   - Update error handling

4. **Update Middleware**
   - Update `CheckTokenBlacklistMiddleware`
   - Implement blacklist checking logic

### Phase 4: Dependency Injection (15 minutes)

1. **Update Program.cs**
   - Register `IRefreshTokenRepository`
   - Update JWT configuration

2. **Update appsettings.json**
   - Add token expiry times
   - Configure token rotation

### Phase 5: Testing (60+ minutes)

1. **Unit Tests**
   - Test token generation
   - Test token validation
   - Test token revocation

2. **Integration Tests**
   - Test login flow
   - Test refresh token endpoint
   - Test logout flow
   - Test expired token handling

3. **Manual Testing**
   - Test with Postman
   - Test cookie handling
   - Test token rotation

---

## Testing Strategy

### Unit Tests

```csharp
[TestFixture]
public class AuthServiceTests
{
  private Mock<IUserRepository> _mockUserRepository;
  private Mock<ITokenRepository> _mockTokenRepository;
  private Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
  private IOptions<JwtConfiguration> _jwtConfig;
  private AuthService _authService;

  [SetUp]
  public void Setup()
  {
    _mockUserRepository = new Mock<IUserRepository>();
    _mockTokenRepository = new Mock<ITokenRepository>();
    _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
    
    var config = new JwtConfiguration
    {
      Issuer = "test",
      Audience = "test",
      Secret = "your-super-secret-key-with-at-least-32-characters",
      AccessTokenExiryMinutes = 15,
      RefreshTokenExpiryDays = 30,
      EnableTokenRotation = true
    };
    _jwtConfig = Options.Create(config);

    _authService = new AuthService(
      _mockUserRepository.Object,
      _mockTokenRepository.Object,
      _mockRefreshTokenRepository.Object,
      _jwtConfig
    );
  }

  [Test]
  public async Task GenerateTokensAsync_ValidUser_ReturnsAccessAndRefreshTokens()
  {
    // Arrange
    var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com" };

    // Act
    var (accessToken, refreshToken) = await _authService.GenerateTokensAsync(user);

    // Assert
    Assert.IsNotNull(accessToken);
    Assert.IsNotNull(refreshToken);
    Assert.That(accessToken, Is.Not.EqualTo(refreshToken));
  }

  [Test]
  public async Task RefreshAccessTokenAsync_ValidRefreshToken_ReturnsNewAccessToken()
  {
    // Arrange
    var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com" };
    var (_, refreshToken) = await _authService.GenerateTokensAsync(user);
    _mockUserRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

    // Act
    var newAccessToken = await _authService.RefreshAccessTokenAsync(refreshToken);

    // Assert
    Assert.IsNotNull(newAccessToken);
  }

  [Test]
  public async Task RevokeRefreshTokenAsync_ValidToken_MarksTokenAsInactive()
  {
    // Arrange
    var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com" };
    var (_, refreshToken) = await _authService.GenerateTokensAsync(user);

    // Act
    await _authService.RevokeRefreshTokenAsync(refreshToken);

    // Assert
    var isValid = await _refreshTokenRepository.IsTokenValidAsync(HashToken(refreshToken));
    Assert.IsFalse(isValid);
  }
}
```

### Integration Tests

```csharp
[TestFixture]
public class AuthControllerIntegrationTests
{
  private WebApplicationFactory<Program> _factory;
  private HttpClient _client;

  [OneTimeSetUp]
  public void OneTimeSetUp()
  {
    _factory = new WebApplicationFactory<Program>();
    _client = _factory.CreateClient();
  }

  [Test]
  public async Task Login_ValidCredentials_ReturnsAccessAndRefreshTokens()
  {
    // Arrange
    var request = new LoginFeatureRequest { Email = "test@example.com", Password = "password" };

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    var content = await response.Content.ReadAsAsync<LoginFeatureResponse>();
    Assert.IsNotNull(content.AccessToken);
    Assert.IsNotNull(content.RefreshToken);
  }

  [Test]
  public async Task RefreshToken_ValidRefreshToken_ReturnsNewAccessToken()
  {
    // Arrange (assume login was successful)
    var refreshToken = "valid-refresh-token";
    var request = new RefreshTokenRequest { RefreshToken = refreshToken };

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", request);

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    var content = await response.Content.ReadAsAsync<RefreshTokenResponse>();
    Assert.IsNotNull(content.AccessToken);
  }

  [Test]
  public async Task RefreshToken_InvalidToken_Returns401()
  {
    // Arrange
    var request = new RefreshTokenRequest { RefreshToken = "invalid-token" };

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", request);

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
  }

  [Test]
  public async Task Logout_ValidToken_InvalidatesBothTokens()
  {
    // Arrange (assume login was successful and we have a token)
    var token = "valid-access-token";
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await _client.PostAsync("/api/v1/auth/logout", null);

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

    // Try to use the same token again
    var protectedResponse = await _client.GetAsync("/api/v1/users/me");
    Assert.That(protectedResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
  }
}
```

---

## Security Considerations

### 1. Refresh Token Storage
- **Option 1 (Recommended)**: HTTP-only secure cookie
  - Not accessible via JavaScript
  - Immune to XSS attacks
  - Requires HTTPS in production
  
- **Option 2**: Secure local storage (less secure)
  - Requires careful handling
  - Vulnerable to XSS

```csharp
// Set HTTP-only cookie (recommended)
_cookiesManager.SetCookie("refresh_token", refreshToken, expiry, httpOnly: true, secure: true);
```

### 2. Token Signing & Validation
- Use strong secret keys (at least 32 characters)
- Store secrets in secure configuration (User Secrets during dev, Key Vault in production)
- Validate token signature on every request
- Validate token expiration

### 3. Token Rotation Strategy
- Enable token rotation for enhanced security
- Each refresh invalidates the previous refresh token
- Reduces window of exposure if token is compromised

### 4. Rate Limiting
- Implement rate limiting on `/refresh` endpoint
- Prevent brute force attacks
- Limit refresh attempts per user

```csharp
[HttpPost("refresh", Name = "RefreshToken")]
[AllowAnonymous]
[Throttle(interval: 60, maxRequests: 5)] // 5 requests per 60 seconds
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
  // Implementation
}
```

### 5. CORS & CSRF Protection
- Configure CORS properly for refresh endpoint
- Implement CSRF tokens for state-changing operations
- Validate origin headers

### 6. Token Endpoint Security
- Refresh endpoint should be HTTPS only
- Require valid access token for critical operations
- Log token refresh attempts for audit trail

### 7. Cleanup Old Tokens
- Periodically clean up expired refresh tokens
- Remove revoked tokens after grace period
- Implement scheduled cleanup job

```csharp
// Add to Program.cs or separate service
public class TokenCleanupService : IHostedService
{
  public async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      await RemoveExpiredTokensAsync();
      await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
    }
  }

  private async Task RemoveExpiredTokensAsync()
  {
    // Remove tokens expired more than 7 days ago
    var cutoffDate = DateTimeOffset.UtcNow.AddDays(-7);
    await _tokenRepository.DeleteAsync(t => t.RevokedAt < cutoffDate);
  }
}
```

### 8. Audit Logging
- Log all token issuance events
- Log token refresh events
- Log token revocation events
- Include user ID, IP address, user agent

---

## Implementation Checklist

**Phase 1: Database**
- [ ] Create RefreshToken model class
- [ ] Update Token model class
- [ ] Update AppDbContext
- [ ] Create and apply migration

**Phase 2: Repositories**
- [ ] Create IRefreshTokenRepository interface
- [ ] Create RefreshTokenRepository implementation
- [ ] Update ITokenRepository interface
- [ ] Update TokenRepository implementation

**Phase 3: Services**
- [ ] Update IAuthService interface
- [ ] Rewrite AuthService with new methods
- [ ] Implement token generation logic
- [ ] Implement token validation logic
- [ ] Implement token revocation logic

**Phase 4: Data Transfer Objects**
- [ ] Create RefreshTokenFeature (request/response)
- [ ] Update LoginFeatureResponse

**Phase 5: Configuration**
- [ ] Update JwtConfiguration class
- [ ] Update appsettings.json
- [ ] Update appsettings.Development.json

**Phase 6: API Layer**
- [ ] Update AuthController login endpoint
- [ ] Add refresh token endpoint
- [ ] Update logout endpoint
- [ ] Update error handling

**Phase 7: Middleware & Dependency Injection**
- [ ] Update CheckTokenBlacklistMiddleware
- [ ] Update Program.cs DI registration
- [ ] Configure JWT options

**Phase 8: Testing**
- [ ] Create unit tests for AuthService
- [ ] Create integration tests for AuthController
- [ ] Test login flow
- [ ] Test refresh flow
- [ ] Test logout flow
- [ ] Test error scenarios
- [ ] Test with Postman

**Phase 9: Documentation & Deployment**
- [ ] Update API documentation
- [ ] Update client integration guide
- [ ] Update deployment procedures
- [ ] Code review and testing

---

## Timeline Estimate

| Phase | Task | Duration |
|-------|------|----------|
| 1 | Database Setup | 30 min |
| 2 | Service Layer | 45 min |
| 3 | API Layer | 45 min |
| 4 | Dependency Injection | 15 min |
| 5 | Testing | 120+ min |
| 6 | Code Review & Fixes | 30+ min |
| **Total** | **Full Implementation** | **≈5-6 hours** |

---

## Client Integration Guide

### For Frontend Developers

#### Login Flow
```javascript
async function login(email, password) {
  const response = await fetch('/api/v1/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
    credentials: 'include' // Include cookies
  });
  
  const data = await response.json();
  // Store access token in memory or secure storage
  localStorage.setItem('accessToken', data.accessToken);
  // Refresh token is stored in HTTP-only cookie automatically
}
```

#### Protected API Call
```javascript
async function apiCall(endpoint, options = {}) {
  const response = await fetch(endpoint, {
    ...options,
    headers: {
      ...options.headers,
      'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
    },
    credentials: 'include' // Send cookies for refresh token
  });

  if (response.status === 401) {
    // Token expired, try refresh
    return await refreshAndRetry(endpoint, options);
  }

  return response;
}
```

#### Refresh Token Flow
```javascript
async function refreshAndRetry(endpoint, options) {
  const response = await fetch('/api/v1/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: localStorage.getItem('refreshToken') }),
    credentials: 'include'
  });

  if (!response.ok) {
    // Refresh failed, redirect to login
    redirectToLogin();
    return;
  }

  const data = await response.json();
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);

  // Retry original request with new token
  return apiCall(endpoint, options);
}
```

---

**Document Version**: 1.0  
**Created**: February 8, 2026  
**Last Updated**: February 8, 2026
