using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Diplom.Models.Users
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() { }
        public string Description { get; set; }
    }
}