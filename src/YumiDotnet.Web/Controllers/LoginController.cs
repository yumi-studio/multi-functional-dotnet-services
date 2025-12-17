using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace YumiStudio.YumiDotnet.Web.Controllers
{
	public class LoginController : Controller
	{
		// GET: LoginController
		public ActionResult Index()
		{
			return View();
		}

		[AllowAnonymous]
		public ActionResult LoginGoogle()
		{
			var properties = new AuthenticationProperties
			{
				RedirectUri = "/Login/GoogleResponse"
			};
			return Challenge(properties, "Google");
		}

		[AllowAnonymous]
		public async Task<ActionResult> GoogleResponse()
		{
			var result = await HttpContext.AuthenticateAsync("Google");
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

			return RedirectToAction("Index", "Login");
		}
	}
}
