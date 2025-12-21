using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YumiStudio.API.Controllers;

namespace YumiStudio.API.Controllers.Admin;

[Route("api/admin")]
[ApiController]
public abstract class BaseAdminController : GenericController
{
}
