using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using UserApi.Configurations;
using UserApi.Constants;
using UserApi.Helpers;
using UserApi.Infrastructure.Clients;
using UserApi.Middlewares;
using UserApi.Models;
using UserApi.Repositories;
using UserApi.Repositories.Interfaces;
using UserApi.Services;
using UserApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure strongly typed settings objects
builder.Services.Configure<DatabaseConfiguration>(builder.Configuration.GetSection("Mysql"));
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GoogleAuthConfiguration>(builder.Configuration.GetSection("Authentication:Google"));
builder.Services.Configure<CookiePolicyOptions>(options =>
{
  options.MinimumSameSitePolicy = SameSiteMode.None;
});

var databaseConfig = builder.Configuration.GetSection("Mysql").Get<DatabaseConfiguration>()!;
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfiguration>()!;
var googleAuthConfig = builder.Configuration.GetSection("Authentication:Google").Get<GoogleAuthConfiguration>()!;
var microsoftAuthConfig = builder.Configuration.GetSection("Authentication:Microsoft").Get<MicrosoftAuthConfiguration>()!;

builder.Services.AddHttpContextAccessor();
builder.Services
    .AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
      options.Cookie.SameSite = SameSiteMode.None;
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddJwtBearer(jwtOptions =>
    {
      jwtOptions.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
      jwtOptions.SaveToken = true;
      jwtOptions.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidIssuer = jwtConfig.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtConfig.Audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
      };
      jwtOptions.Events = new JwtBearerEvents
      {
        OnMessageReceived = context =>
        {
          context.Token = context.Request.Cookies[CookieKeys.JWT_TOKEN];
          return Task.CompletedTask;
        }
      };
    })
    .AddGoogle(options =>
    {
      options.ClientId = googleAuthConfig.ClientId;
      options.ClientSecret = googleAuthConfig.ClientSecret;
      options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.SaveTokens = true;
    })
    .AddMicrosoftAccount(options =>
    {
      options.ClientId = microsoftAuthConfig.ClientId;
      options.ClientSecret = microsoftAuthConfig.ClientSecret;
      options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.AuthorizationEndpoint = microsoftAuthConfig.AuthorizationEndpoint;
      options.TokenEndpoint = microsoftAuthConfig.TokenEndpoint;
      options.SaveTokens = true;
    });

builder.Services.AddAuthorizationBuilder().AddFallbackPolicy(
  name: "custom-fallback-policy",
  policy: new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
);

builder.Services.AddCors(options =>
{
  var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>();

  options.AddPolicy("AllowFrontend", policy =>
    {
      if (allowedOrigins == null || allowedOrigins.Length == 0)
      {
        // No specific origins configured -> allow any origin (do NOT use credentials)
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
      }
      else
      {
        // Specific origins configured -> credentials are allowed
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
      }
    });
});

builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Service API", Version = "v1" });
  c.CustomSchemaIds(type => type.FullName);
  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Description = "Enter JWT token: Bearer {token}",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
  });
  c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var connectionStringBuilder = new MySqlConnectionStringBuilder
{
  Server = databaseConfig.Host,
  Port = databaseConfig.Port,
  Database = databaseConfig.Database,
  UserID = databaseConfig.User,
  Password = databaseConfig.Password,
  Pooling = true,
  MinimumPoolSize = 10,
  MaximumPoolSize = 20
};
builder.Services.AddDbContext<AppDbContext>(opt =>
{
  opt.UseMySQL(connectionStringBuilder.ConnectionString);
});
Console.WriteLine($"Connect MySQL: {connectionStringBuilder.ConnectionString}");

Console.WriteLine($"Register Services");
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

Console.WriteLine($"Register Repositories");
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserExternalRepository, UserExternalRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

Console.WriteLine("Register Helpers");
builder.Services.AddScoped<CookiesManager>();

Console.WriteLine("Register External Services");
builder.Services.AddHttpClient<StorageClient>();
builder.Services.AddHttpClient<FakebookClient>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure custom error response for model validation errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
  options.InvalidModelStateResponseFactory = context =>
  {
    var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
    var result = new
    {
      Success = false,
      Errors = errors
    };

    return new BadRequestObjectResult(result);
  };
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUi(c =>
{
  c.DocumentPath = "/swagger/v1/swagger.json";
});

app.UseHttpsRedirection();

app.UseMiddleware<RequestErrorMiddleware>();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CheckTokenBlacklistMiddleware>();

app.MapControllers();
app.MapDefaultControllerRoute();
app.Run();
