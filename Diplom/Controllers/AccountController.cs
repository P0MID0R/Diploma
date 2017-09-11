using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Web.Mvc;
using Diplom.Models;
using Diplom.Models.Users;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;
using Microsoft.Owin.Security;
using PagedList.Mvc;
using PagedList;

namespace Diplom.Controllers
{
    public class AccountController : Controller
    {
        DbContext db = new DbContext();
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }
        private ApplicationRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
        }

        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> RegistrationStudents(RegistrationModelStudent model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                    Name = model.Name,
                    Surname = model.Surname,
                    Middlename = model.Middlename,
                    Group = model.Group,
                    Banned = false,
                    Type = "Student"
                };
                System.Threading.Thread.Sleep(4000);
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (user.UserName == "Admin")
                    {
                        UserManager.AddToRole(user.Id, "Admin");
                    }
                    else

                    {
                        UserManager.AddToRole(user.Id, "User");
                    }

                    db.Students.Add(new Student { Name = model.Name, Surname = model.Surname, Middlename = model.Middlename, Group = model.Group });
                    db.SaveChanges();

                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> RegistrationTeachers(RegistrationModelTeacher model)
        {
            if (model.Code == "1704003219001")
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser user = new ApplicationUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                        Name = model.Name,
                        Surname = model.Surname,
                        Middlename = model.Middlename,
                        Group = model.Subject,
                        Type = "Teacher"
                    };
                    IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        if (user.UserName == "Admin")
                        {
                            UserManager.AddToRole(user.Id, "Admin");
                        }
                        else
                        {
                            UserManager.AddToRole(user.Id, "Teacher");
                        }

                        db.Teachers.Add(new Teacher { Name = model.Name, Surname = model.Surname, Middlename = model.Middlename, Subject = model.Subject });
                        db.SaveChanges();

                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        foreach (string error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await UserManager.FindAsync(model.Login, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Неверный логин или пароль.");
                }
                else
                {
                    ClaimsIdentity claim = await UserManager.CreateIdentityAsync(user,
                                            DefaultAuthenticationTypes.ApplicationCookie);
                    AuthenticationManager.SignOut();
                    AuthenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = true
                    }, claim);
                    if (String.IsNullOrEmpty(returnUrl))
                        return RedirectToAction("Main", "Home");
                    return Redirect(returnUrl);
                }
            }
            ViewBag.returnUrl = returnUrl;
            return View(model);
        }
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Accounts(int? page)
        {
            try
            {
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                // получаем из бд все объекты News
                IEnumerable<ApplicationUser> users = UserManager.Users;
                ViewBag.listCount = users.Count();
                // возвращаем представление
                return View(users.OrderBy(i => i.UserName).ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAccount(string Login)
        {
            ApplicationUser user = await UserManager.FindByNameAsync(Login);
            if (user != null)
            {
                Teacher teachertoDelete;
                using (var ctx = new DbContext())
                {
                    teachertoDelete = ctx.Teachers.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault<Teacher>();
                }
                using (var newContext = new DbContext())
                {
                    newContext.Entry(teachertoDelete).State = System.Data.Entity.EntityState.Deleted;
                    System.Threading.Thread.Sleep(1000);
                    newContext.SaveChanges();
                }

                foreach (var i in db.Themes)
                {
                    if (i.TeacherID == teachertoDelete.Id)
                    {

                        Theme theme = i;
                        foreach (var k in db.Students)
                        {
                            if (i.StudentID == k.Id) theme.StudentID = -1;
                        }
                        theme.TeacherID = -1;
                        using (var newContext = new DbContext())
                        {
                            newContext.Entry(theme).State = System.Data.Entity.EntityState.Modified;
                            newContext.SaveChanges();
                        }
                    }
                }



                IdentityResult result = await UserManager.DeleteAsync(user);
                if (result.Succeeded && User.Identity.Name == Login)
                {
                    return RedirectToAction("Logout", "Account");
                }
            }
            return RedirectToAction("Accounts", "Account");
        }

        [Authorize]
        public ActionResult UserProfile(string userName)
        {
            try
            {
                ApplicationUser user = UserManager.Users.Where(s => s.UserName == userName).FirstOrDefault();
                Student student = db.Students.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault();
                Theme theme = db.Themes.Where(s => s.StudentID == student.Id).FirstOrDefault();
                Teacher teacher = db.Teachers.Where(s => s.Id == theme.TeacherID).FirstOrDefault();
                if (theme.Completed == null)
                {
                    ViewBag.Complited = false;
                }
                else
                {
                    ViewBag.Complited = theme.Completed;
                }
                ViewBag.Theme = theme.Topic;
                ViewBag.Teacher = teacher.Surname + " " + teacher.Name + " " + teacher.Middlename;
                return View(user);
            }
            catch
            {
                ApplicationUser user = UserManager.Users.Where(s => s.UserName == userName).FirstOrDefault();
                ViewBag.Theme = "Не выбрана";
                ViewBag.Teacher = "Нет";
                ViewBag.Complited = false;
                return View(user);
            }
        }
    }
}