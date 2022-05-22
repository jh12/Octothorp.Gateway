﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Octothorp.Gateway.Auth.Cookie
{
    public class OctoCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return Task.CompletedTask;

            // TODO: Use when login pages are implemented
            //if (context.Request.Path.StartsWithSegments("/api"))
            //{
            //    context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            //}
            //else
            //{
            //    context.Response.Redirect(context.RedirectUri);
            //}

            //return Task.CompletedTask;
        }
    }
}
