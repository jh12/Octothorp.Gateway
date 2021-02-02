using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Octothorp.Gateway.DTOs.V1.Auth;
using Octothorp.Gateway.Shared.Repositories.Interfaces;

namespace Octothorp.Gateway.Pages.Auth
{
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IAuthRepository _authRepository;

        public UserController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public IActionResult Index()
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;

            CurrentUser? currentUser = null;

            if (!isAuthenticated)
                return View("NotLoggedIn");

            IIdentity identity = HttpContext.User.Identity;

            currentUser = new CurrentUser(identity.Name, identity.AuthenticationType, "identifier");

            ViewBag.CurrentUser = currentUser;

            return View("LoggedIn");
        }

        [Route("signout")]
        public IActionResult Signout()
        {
            return SignOut(new AuthenticationProperties {RedirectUri = "user"}, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}