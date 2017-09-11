using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Diplom.Models.Users;
using System.Web.Routing;
using Diplom.Models.DB;

namespace Diplom.Models
{
    public class DbContext : System.Data.Entity.DbContext
    {
        public DbContext() : base("DiplomDb")
        {
            Database.SetInitializer(new DbInitializer());
            Database.SetInitializer<ApplicationContext>(new UsersInitializer());
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Files> Files { get; set; }
        public DbSet<Settings> SystemTable { get; set; }
        
    }
}