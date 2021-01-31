using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Octothorp.Gateway.Shared.Models.Auth;
using Octothorp.Gateway.Shared.Repositories.Interfaces;

namespace Octothorp.Gateway.Events.Auth
{
    public class CookieAuthEvents : CookieAuthenticationEvents
    {
        private readonly IAuthRepository _authRepository;

        public CookieAuthEvents(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                return Task.CompletedTask;
            }

            return base.RedirectToAccessDenied(context);
        }

        public override async Task SigningIn(CookieSigningInContext context)
        {
            ClaimsPrincipal principal = context.Principal;
            ClaimsIdentity identity = (ClaimsIdentity) principal.Identity;
            string provider = identity.AuthenticationType;
            string id = identity.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            string name = identity.Name;

            OAuthUserInfo oAuthUserInfo = new OAuthUserInfo(provider, id, name);
            UserInfo userInfo = await _authRepository.TrySigninAsync(oAuthUserInfo);

            await base.SigningIn(context);
        }

        public override Task SigningOut(CookieSigningOutContext context)
        {
            context.Response.Redirect("/User");
            return Task.CompletedTask;
        }
    }
}