using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web.Mvc;


namespace Diplom.Models.Users
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationContext() : base("DiplomDb")
        {
            Database.SetInitializer<DbContext>(new DbInitializer());
            Database.SetInitializer<ApplicationContext>(new UsersInitializer());
        }

        static ApplicationContext() { Database.SetInitializer<ApplicationContext>(new UsersInitializer()); }

        public static ApplicationContext Create()
        {
            return new ApplicationContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>().Ignore(c => c.PhoneNumber)
                                               .Ignore(c => c.PhoneNumberConfirmed)
                                               .Ignore(c => c.TwoFactorEnabled)
                                               .Ignore(c => c.LockoutEnabled)
                                               .Ignore(c => c.LockoutEndDateUtc)
                                               .Ignore(c => c.AccessFailedCount)
                                               .Ignore(c => c.EmailConfirmed)
                                               .ToTable("AspNetUsers"); ;
        }
    }
}