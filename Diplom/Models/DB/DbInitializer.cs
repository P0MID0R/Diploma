using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web.Mvc;
using Diplom.Models.Users;

namespace Diplom.Models
{
    public class DbInitializer : CreateDatabaseIfNotExists<DbContext> //DropCreateDatabaseAlways<BlogContext>  //CreateDatabaseIfNotExists<BlogContext>
    {
        protected override void Seed(DbContext db)
        {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();

            ApplicationUser user = new ApplicationUser
            {
                UserName = "Admin",
                Email = "support@bntu.by",
                RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                Name = "",
                Surname = "",
                Middlename = "",
                Group = "777",
                Type = "Admin"
            };

            userManager.Create(user, "1234567");


            roleManager.Create(new ApplicationRole { Name = "Admin", Description = "Администрирование БД" });
            roleManager.Create(new ApplicationRole { Name = "Teacher", Description = "Преподаватель" });
            roleManager.Create(new ApplicationRole { Name = "User", Description = "Студент" });
            userManager.AddToRole(user.Id, "Admin");
            System.Threading.Thread.Sleep(100);
            base.Seed(db);
        }
    }
}