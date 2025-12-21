using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Sprache;
using YumiStudio.Domain.Exceptions;
using YumiStudio.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.Common.Helpers;

public class HasSystemAdminPermissionAttribute : Attribute, IAuthorizationFilter
{
  public HasSystemAdminPermissionAttribute()
  {

  }

  public void OnAuthorization(AuthorizationFilterContext context)
  {
    var userIdClaim = context.HttpContext.User?.FindFirst(ClaimTypes.Name)?.Value ?? "0";

    Guid userId = Guid.Parse(userIdClaim);

    var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

    var isSystemAdmin = db.Users.Where(u => u.Id == userId).FirstOrDefault()?.IsSystemAdmin;

    if (isSystemAdmin == null || isSystemAdmin == false)
    {
      throw new UnauthorizedException("User is not authorized");
    }
  }
}
