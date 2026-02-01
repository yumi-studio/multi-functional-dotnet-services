using System.Text;
using Fakebook.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using Fakebook.Configurations;
using Fakebook.Constants;
using Fakebook.Helpers;
using Fakebook.Models;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Repositories;
using Fakebook.Repositories.Interfaces;
using Fakebook.Services;
using Fakebook.Services.Interfaces;
using Fakebook.Infrastructure.UploadMethods;
using Fakebook.Infrastructure.Resolvers;

var builder = WebApplication.CreateBuilder(args);

// Configure strongly typed settings objects
builder.Services.Configure<StorageConfiguration>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<DatabaseConfiguration>(builder.Configuration.GetSection("Mysql"));
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<CookiePolicyOptions>(options =>
{
  options.MinimumSameSitePolicy = SameSiteMode.None;
});

var storageConfig = builder.Configuration.GetSection("Storage").Get<StorageConfiguration>()!;
var databaseConfig = builder.Configuration.GetSection("Mysql").Get<DatabaseConfiguration>()!;
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfiguration>()!;

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
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fakebook API", Version = "v1" });
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

Console.WriteLine($"Register Upload Methods");
builder.Services.AddScoped<IUploadMethod, LocalUploadMethod>();
builder.Services.AddScoped<IUploadMethodResolver, UploadMethodResolver>();

Console.WriteLine($"Register Services");
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

Console.WriteLine($"Register Repositories");
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostMediaRepository, PostMediaRepository>();
builder.Services.AddScoped<IPostCommentRepository, PostCommentRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();

Console.WriteLine("Register Helpers");
builder.Services.AddScoped<CookiesManager>();

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

Console.WriteLine("Configure Static File Middleware");
var storageDirPath = storageConfig.DirPath;
if (!Directory.Exists(storageDirPath))
{
  Directory.CreateDirectory(storageDirPath);
}
var staticFileOptions = new StaticFileOptions
{
  FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storageDirPath),
  RequestPath = storageConfig.BasePath,
};
app.UseStaticFiles(staticFileOptions);

app.UseHttpsRedirection();

app.UseMiddleware<RequestErrorMiddleware>();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();
app.Run();
