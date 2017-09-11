using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using Diplom.Models;
using Diplom.Models.Users;
using Diplom.Models.App;

namespace Diplom
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            Database.SetInitializer(new DbInitializer());
            Database.SetInitializer<ApplicationContext>(new UsersInitializer());
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            AlertSheduler.Start();
        }
    }
}
