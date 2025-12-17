using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using YumiStudio.YumiDotNet.API.Middlewares;
using YumiStudio.YumiDotNet.Application.DTOs;
using YumiStudio.YumiDotNet.Common.Configurations;
using YumiStudio.YumiDotNet.Common.Helpers;
using YumiStudio.YumiDotNet.Application.Interfaces;
using YumiStudio.YumiDotNet.Infrastructure.Persistence.DbContexts;
using ApplicationInterfacesFakebook = YumiStudio.YumiDotNet.Application.Interfaces.Fakebook;
using YumiStudio.YumiDotNet.Application.Services;
using ApplicationServicesFakebook = YumiStudio.YumiDotNet.Application.Services.Fakebook;
using YumiStudio.YumiDotNet.Domain.Interfaces;
using DomainInterfacesFakebook = YumiStudio.YumiDotNet.Domain.Interfaces.Fakebook;
using YumiStudio.YumiDotNet.Infrastructure.Repositories;
using InfrastructureRepositoriesFakebook = YumiStudio.YumiDotNet.Infrastructure.Repositories.Fakebook;
using Microsoft.AspNetCore.Authorization;
using YumiStudio.YumiDotNet.Common.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Configure strongly typed settings objects
builder.Services.Configure<StorageConfig>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GoogleAuthConfiguration>(builder.Configuration.GetSection("Authentication:Google"));
builder.Services.Configure<CookiePolicyOptions>(options =>
{
  options.MinimumSameSitePolicy = SameSiteMode.None;
});

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
      var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfiguration>()!;
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
      var jwtConfig = builder.Configuration.GetSection("Authentication:Google").Get<GoogleAuthConfiguration>()!;
      options.ClientId = jwtConfig.ClientId;
      options.ClientSecret = jwtConfig.ClientSecret;
      options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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

// Configure Swagger to use the JWT Bearer token authentication
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = ".NET API", Version = "v1" });
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

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Config AWS Service
builder.Services.AddSingleton<AwsS3Helper>();

var defaultDatabase = builder.Configuration.GetSection("DefaultDatabase").Value;
switch (defaultDatabase?.ToLower())
{
  case "mysql":
    var mysqlConfig = builder.Configuration.GetSection("Mysql");
    // string connectionString =
    // "Server=" + mysqlConfig["Host"] +
    // ";Port=" + mysqlConfig["Port"] +
    // ";Database=" + mysqlConfig["Database"] +
    // ";User ID=" + mysqlConfig["User"] +
    // ";Password=" + mysqlConfig["Password"] +
    // ";Pooling=true" +
    // ";Min Pool Size=10" +
    // ";Max Pool Size=20";

    var connectionStringBuilder = new MySqlConnectionStringBuilder
    {
      Server = mysqlConfig["Host"],
      Port = uint.Parse(mysqlConfig["Port"] ?? "3306"),
      Database = mysqlConfig["Database"],
      UserID = mysqlConfig["User"],
      Password = mysqlConfig["Password"],
      Pooling = true,
      MinimumPoolSize = 10,
      MaximumPoolSize = 20
    };

    var serverVersion = ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString);
    builder.Services.AddDbContext<AppDbContext>(opt =>
    {
      opt.UseMySql(connectionStringBuilder.ConnectionString, serverVersion);
    });
    Console.WriteLine($"Connect MySQL: {connectionStringBuilder.ConnectionString}");
    break;
  default:
    builder.Services.AddDbContext<AppDbContext>(opt =>
    {
      opt.UseInMemoryDatabase("InMemoryDb")
        .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    });
    Console.WriteLine($"Connect H2 Database");
    break;
}

// Register application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<ApplicationInterfacesFakebook.IPostService, ApplicationServicesFakebook.PostService>();
builder.Services.AddScoped<ApplicationInterfacesFakebook.IProfileService, ApplicationServicesFakebook.ProfileService>();
builder.Services.AddScoped<ApplicationInterfacesFakebook.ICommentService, ApplicationServicesFakebook.CommentService>();

// Register application repositories
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFileUploadRepository, FileUploadRepository>();
builder.Services.AddScoped<DomainInterfacesFakebook.IProfileRepository, InfrastructureRepositoriesFakebook.ProfileRepository>();
builder.Services.AddScoped<DomainInterfacesFakebook.IPostRepository, InfrastructureRepositoriesFakebook.PostRepository>();
builder.Services.AddScoped<DomainInterfacesFakebook.IPostMediaRepository, InfrastructureRepositoriesFakebook.PostMediaRepository>();
builder.Services.AddScoped<DomainInterfacesFakebook.IPostCommentRepository, InfrastructureRepositoriesFakebook.PostCommentRepository>();
builder.Services.AddScoped<DomainInterfacesFakebook.IReactionRepository, InfrastructureRepositoriesFakebook.ReactionRepository>();

// Register application helpers
builder.Services.AddSingleton<ActiveTokenManager>();
builder.Services.AddSingleton<CookiesManager>();
builder.Services.AddSingleton<FakebookHelper>();

// Register background jobs
// builder.Services.AddHostedService<ClearUnusedUploadFile>();

// Configure custom error response for model validation errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
  options.InvalidModelStateResponseFactory = context =>
  {
    var result = new ResponseDto<string>
    {
      Success = false,
      Message = "Something went wrong.",
      Errors = [.. context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)]
    };

    return new BadRequestObjectResult(result);
  };
});

var storageConfig = builder.Configuration
    .GetSection("Storage")
    .Get<StorageConfig>();

var app = builder.Build();

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
app.MapOpenApi();

app.UseSwagger();
app.UseSwaggerUi(c =>
{
  c.DocumentPath = "/swagger/v1/swagger.json";
});

if (storageConfig?.Default == StorageConfig.LOCALDIR)
{
  var storageDirPath = storageConfig.LocalDir.DirPath;
  var staticFileOptions = new StaticFileOptions
  {
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storageDirPath),
    RequestPath = "/storage",
  };
  if (!Directory.Exists(storageDirPath))
  {
    Directory.CreateDirectory(storageDirPath);
  }
  app.UseStaticFiles(staticFileOptions);
  Console.WriteLine($"Use Local Storage: {storageDirPath}");
}
else if (storageConfig?.Default == StorageConfig.AWSS3)
{
  // Config use aws3s directory storage
}
else
{
  throw new Exception("Unable to config default storage");
}

app.UseMiddleware<RequestErrorMiddleware>();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CheckTokenBlacklistMiddleware>();
app.UseMiddleware<AuthorizeMiddleware>();

// app.UseHttpsRedirection();

app.MapControllers();
app.MapDefaultControllerRoute();
app.Run();
