using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;
using Owin;
using Diplom.Models.Users;
using System.Data.Entity;
using Diplom.Models;

[assembly: OwinStartup(typeof(Diplom.App_Start.Startup))]

namespace Diplom.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext<ApplicationContext>(ApplicationContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            Database.SetInitializer(new DbInitializer());
            Database.SetInitializer<ApplicationContext>(new UsersInitializer());

            // регистрация менеджера ролей
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
            });
            Database.SetInitializer(new DbInitializer());
            Database.SetInitializer<ApplicationContext>(new UsersInitializer());           
        }
    }
}