using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System.Threading.Tasks;

namespace Diplom.Models.Users
{
    //___ Управление пользователями ___

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
                : base(store)
        {
        }
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
                                                IOwinContext context)
        {
            ApplicationContext db = context.Get<ApplicationContext>();
            ApplicationUserManager manager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            return manager;
        }

        public ApplicationUser FindbyLogin(string login)
        {
            ApplicationContext users = new ApplicationContext();
            foreach (var user in users.Users)
                if (user.UserName == login) return user;
            return null;
        }

        public async Task<IdentityResult> ChangePasswordNoOld(ApplicationUser user, string newPassword)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var store = this.Store as IUserPasswordStore<ApplicationUser>;
            if (store == null)
            {
                var errors = new string[] { "Current UserStore doesn't implement IUserPasswordStore" };
                return IdentityResult.Failed(errors);
            }

            var newPasswordHash = this.PasswordHasher.HashPassword(newPassword);
            await store.SetPasswordHashAsync(user, newPasswordHash);
            await store.UpdateAsync(user);
            return IdentityResult.Success;
        }
    }

    //___ Управление ролями ___

    public class ApplicationRoleManager : RoleManager<ApplicationRole>
    {
        public ApplicationRoleManager(RoleStore<ApplicationRole> store)
                    : base(store)
        { }
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options,
                                                IOwinContext context)
        {
            return new ApplicationRoleManager(new
                    RoleStore<ApplicationRole>(context.Get<ApplicationContext>()));
        }
    }
}