using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;


[assembly: OwinStartup(typeof(Proyecto_Estadias.App_Start.Startup))]

namespace Proyecto_Estadias.App_Start
{
   
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(
                new Microsoft.Owin.Security.Cookies.CookieAuthenticationOptions()
                {

                    LoginPath = new PathString("/Auth/Login"),
                    ExpireTimeSpan = TimeSpan.FromMinutes(30),
                    SlidingExpiration = true,
                    CookieHttpOnly = true,
                    CookieSecure = CookieSecureOption.SameAsRequest,
                    CookieSameSite = Microsoft.Owin.SameSiteMode.Strict

                });
        }
    }
}