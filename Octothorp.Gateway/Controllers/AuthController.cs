using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Octothorp.Gateway.DTOs.V1.Auth;
using Octothorp.Gateway.Shared.Exceptions.Auth;

namespace Octothorp.Gateway.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        [HttpGet("signin/providers")]
        [AllowAnonymous]
        public async Task<IActionResult> SignInProviders()
        {
            AuthenticationScheme[] schemes = await GetProvidersAsync();

            AuthProvider[] providers = schemes
                .Where(s => !string.IsNullOrWhiteSpace(s.DisplayName))
                .Select(s => new AuthProvider(s.Name, s.DisplayName)).ToArray();

            return Ok(providers);
        }

        [HttpGet("signin/{provider}")]
        [HttpPost("signin/{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> Signin(string provider)
        {
            ClaimsPrincipal httpContextUser = HttpContext.User;

            if (httpContextUser.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            if (string.IsNullOrEmpty(provider))
                return BadRequest();

            AuthenticationScheme[] schemes = await GetProvidersAsync();
            AuthenticationScheme? scheme = schemes.SingleOrDefault(s => s.Name.Equals(provider, StringComparison.CurrentCultureIgnoreCase));
            if (scheme == null)
                return BadRequest();

            return Challenge(new AuthenticationProperties {RedirectUri = "/"}, scheme.Name);
        }

        [HttpGet("signout")]
        [HttpPost("signout")]
        public async Task<IActionResult> Signout()
        {
            return SignOut(new AuthenticationProperties {RedirectUri = "/"}, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("user/current")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurrentUser()
        {
            ClaimsPrincipal httpContextUser = HttpContext.User;

            if(!httpContextUser.Identity.IsAuthenticated)
                throw new NotSignedInException();

            string? username = httpContextUser.Identity.Name;

            ClaimsIdentity identity = (ClaimsIdentity) httpContextUser.Identity;

            string issuer = identity.AuthenticationType;
            string identifier = identity.FindFirst(ClaimTypes.NameIdentifier).Value;

            return Ok(new CurrentUser(username, issuer, identifier));
        }

        private async Task<AuthenticationScheme[]> GetProvidersAsync()
        {
            var schemeProvider = HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

            IEnumerable<AuthenticationScheme> schemes = await schemeProvider.GetAllSchemesAsync();
            AuthenticationScheme[] schemesArray = schemes.Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToArray();

            return schemesArray;
        }
    }
}