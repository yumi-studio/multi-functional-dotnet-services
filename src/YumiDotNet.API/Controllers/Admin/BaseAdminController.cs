using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YumiStudio.YumiDotNet.API.Controllers;

namespace YumiStudio.YumiDotNet.API.Controllers.Admin;

[Route("api/admin")]
[ApiController]
public abstract class BaseAdminController : GenericController
{
}
