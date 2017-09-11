using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Diplom.Models;
using Diplom.Models.Users;
using PagedList.Mvc;
using PagedList;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Office.Interop.Word;

namespace Diplom.Controllers
{
    public class HomeController : Controller
    {
        DbContext db = new DbContext();
        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        [Authorize]
        public ActionResult Main(int? page)
        {
            try
            {
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                // получаем из бд все объекты News
                IEnumerable<News> news = db.News;
                ViewBag.listCount = news.Count();
                // возвращаем представление
                return View(news.OrderByDescending(i => i.Created).ToPagedList(pageNumber, pageSize));
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult AddNews()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Teacher")]
        [HttpPost]
        public ActionResult AddNews(News news)
        {
            try
            {
                bool check = true;
                db.News.Add(news);
                db.SaveChanges();
                if (news.FilesUpload.Count() > 0)
                {
                    foreach (var file in news.FilesUpload)
                    {
                        if (file == null) continue;
                        var fileName = Path.GetFileName(file.FileName);
                        string path = AppDomain.CurrentDomain.BaseDirectory + "UploadedFiles/";
                        string fullpath = Path.Combine(path, fileName);
                        while (check)
                        {
                            if (System.IO.File.Exists(fullpath))
                            {
                                fullpath = Path.Combine(path, fileName.Split('.')[0] + "1" + "." + fileName.Split('.')[1]);
                                fileName = fileName.Split('.')[0] + "1" + "." + fileName.Split('.')[1];
                                continue;
                            }
                            check = false;
                        }
                        check = true;
                        Diplom.Models.DB.Files FileToAdd = new Models.DB.Files();
                        FileToAdd.FileName = Path.GetFileName(file.FileName);
                        FileToAdd.FilePath = fullpath;
                        FileToAdd.IdNews = news.Id;
                        db.Files.Add(FileToAdd);
                        file.SaveAs(fullpath);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("News", new { page = news.Id });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult EditNews(int id)
        {
            Diplom.Models.News news = db.News.Where(s => s.Id == id).FirstOrDefault();
            return View(news);
        }

        [Authorize(Roles = "Admin, Teacher")]
        [HttpPost]
        public ActionResult EditNews(News news)
        {
            news.Created = DateTime.Now;
            using (var newContext = new DbContext())
            {
                newContext.Entry(news).State = System.Data.Entity.EntityState.Modified;
                System.Threading.Thread.Sleep(1000);
                newContext.SaveChanges();
            }
            return RedirectToAction("News", new { page = news.Id });
        }

        [Authorize]
        public ActionResult News(int page)
        {
            try
            {
                var news = db.News.Find(page);
                ViewBag.Files = db.Files.Where(s => s.IdNews == news.Id);
                return View(news);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        [HttpGet]
        public ActionResult RemoveNews(int id)
        {
            try
            {
                News NewstoDelete;
                IEnumerable<News> posts = db.News;
                using (var ctx = new DbContext())
                {
                    NewstoDelete = ctx.News.Where(s => s.Id == id).FirstOrDefault<News>();
                }

                using (var newContext = new DbContext())
                {
                    newContext.Entry(NewstoDelete).State = System.Data.Entity.EntityState.Deleted;
                    System.Threading.Thread.Sleep(1000);
                    newContext.SaveChanges();
                }
                // возвращаем представление
                return RedirectToAction("Main", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        [HttpGet]
        public ActionResult StudentsList(int? page, string rule)
        {
            try
            {
                if ((rule == null) || (rule == "Все")) rule = "All";
                ViewBag.Rule = rule;
                int pageSize = 15;
                int pageNumber = (page ?? 1);
                IEnumerable<Student> students = db.Students;
                List<Student> studentsTemp = new List<Student>();
                List<string> GroupList = new List<string>();
                // получаем из бд все объекты Students              
                IEnumerable<Theme> themes = db.Themes;
                foreach (var i in students)
                {
                    if (themes.Where(s => s.StudentID == i.Id).FirstOrDefault() != null) i.Theme = themes.Where(s => s.StudentID == i.Id).FirstOrDefault().Topic;
                }

                GroupList.Add("Все");
                foreach (var i in db.Students)
                {
                    if (!GroupList.Contains(i.Group)) GroupList.Add(i.Group);
                }
                SelectList temp = new SelectList(GroupList);
                ViewBag.Groups = temp;

                if (User.IsInRole("Teacher"))
                {
                    ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();

                    Teacher teacher = db.Teachers.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault();
                    IEnumerable<Theme> theme = db.Themes.Where(s => s.TeacherID == teacher.Id);
                    foreach (var i in students)
                    {
                        if (!i.Got)
                        {
                            studentsTemp.Add(i);
                            continue;
                        }
                        foreach (var k in theme)
                        {
                            if (k.StudentID == i.Id) studentsTemp.Add(i);
                        }
                    }
                    IEnumerable<Student> tempStud = studentsTemp;
                    if (rule != "All") tempStud = tempStud.Where(s => s.Group == rule);

                    ViewBag.listCount = tempStud.Count();
                    return View(tempStud.OrderBy(i => i.Surname).ToPagedList(pageNumber, pageSize));
                }
                if (rule != "All") students = students.Where(s => s.Group == rule);
                ViewBag.listCount = students.Count();
                // возвращаем представление
                return View(students.OrderBy(i => i.Surname).ToPagedList(pageNumber, pageSize));
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteStudent(int id)
        {
            try
            {
                Student StudenttoDelete;
                IEnumerable<Student> students = db.Students;
                using (var ctx = new DbContext())
                {
                    StudenttoDelete = ctx.Students.Where(s => s.Id == id).FirstOrDefault<Student>();
                }

                using (var newContext = new DbContext())
                {
                    newContext.Entry(StudenttoDelete).State = System.Data.Entity.EntityState.Deleted;
                    System.Threading.Thread.Sleep(1000);
                    newContext.SaveChanges();
                }
                return RedirectToAction("StudentsList", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult TeachersList(int? page)
        {
            try
            {
                int pageSize = 15;
                int pageNumber = (page ?? 1);
                // получаем из бд все объекты News
                IEnumerable<Teacher> teachers = db.Teachers;
                ViewBag.listCount = teachers.Count();
                // возвращаем представление
                return View(teachers.OrderBy(i => i.Surname).ToPagedList(pageNumber, pageSize));
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet]
        public ActionResult ThemeList(int? page)
        {
            try
            {
                int pageSize = 20;
                int pageNumber = (page ?? 1);
                // получаем из бд все объекты News
                IEnumerable<Theme> themes = db.Themes;
                ViewBag.listCount = themes.Count();
                List<string> subjects = new List<string>();
                List<string> teachers = new List<string>();
                foreach (var i in db.Teachers)
                {
                    if (!subjects.Contains(i.Subject)) subjects.Add(i.Subject);
                }
                foreach (var i in db.Teachers)
                {
                    if (!teachers.Contains(i.Surname + " " + i.Name + " " + i.Middlename)) teachers.Add(i.Surname + " " + i.Name + " " + i.Middlename);
                }
                foreach (var i in themes)
                {
                    Teacher temp = db.Teachers.Where(c => c.Id == i.TeacherID).FirstOrDefault();
                    if
                        (i.TeacherID == -1) i.FullName = "Нет";
                    else
                        i.FullName = temp.Surname + " " + temp.Name + " " + temp.Middlename;
                }
                ViewBag.Subjects = subjects;
                ViewBag.Teachers = teachers;
                // возвращаем представление
                return View(themes.OrderBy(i => i.Topic).ToPagedList(pageNumber, pageSize));
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        [HttpPost]
        public ActionResult ThemeList(Theme theme)
        {
            try
            {
                db.Themes.Add(theme);
                Teacher teacher = null;
                foreach (var i in db.Teachers)
                {
                    if (i.Surname == theme.FullName.Split(' ')[0] && i.Name == theme.FullName.Split(' ')[1] && i.Middlename == theme.FullName.Split(' ')[2]) teacher = i;
                }
                theme.StudentID = -1;
                theme.TeacherID = teacher.Id;
                db.SaveChanges();
                return RedirectToAction("ThemeList", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult ThemeControl(int? page)
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    int pageSize = 20;
                    int pageNumber = (page ?? 1);
                    // получаем из бд все объекты News
                    IEnumerable<Theme> themes = db.Themes;
                    ViewBag.listCount = themes.Count();
                    return View(themes.OrderBy(i => i.Topic).ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();
                    Teacher teacher = db.Teachers.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault();
                    int pageSize = 20;
                    int pageNumber = (page ?? 1);
                    // получаем из бд все объекты News
                    IEnumerable<Theme> themes = db.Themes.Where(s => s.TeacherID == teacher.Id);
                    ViewBag.listCount = themes.Count();
                    return View(themes.OrderBy(i => i.Topic).ToPagedList(pageNumber, pageSize));
                }
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult CloseTheme(int id, int mark)
        {
            try
            {
                Theme theme = db.Themes.Where(s => s.Id == id).FirstOrDefault();
                theme.Mark = mark;
                Theme newTheme = theme;
                newTheme.Completed = true;

                using (var newContext = new DbContext())
                {
                    newContext.Entry(newTheme).State = System.Data.Entity.EntityState.Modified;
                    newContext.SaveChanges();
                }
                return RedirectToAction("ThemeControl", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        public ActionResult OpenTheme(int id)
        {
            try
            {
                Theme theme = db.Themes.Where(s => s.Id == id).FirstOrDefault();
                theme.Mark = 0;
                Theme newTheme = theme;
                newTheme.Completed = false;

                using (var newContext = new DbContext())
                {
                    newContext.Entry(newTheme).State = System.Data.Entity.EntityState.Modified;
                    newContext.SaveChanges();
                }
                return RedirectToAction("ThemeControl", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult Theme(int page)
        {
            try
            {
                var theme = db.Themes.Find(page);
                return View(theme);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        [HttpGet]
        public ActionResult AddTheme(Theme theme)
        {
            try
            {
                theme.StudentID = -1;
                theme.TeacherID = -1;
                db.Themes.Add(theme);
                db.SaveChanges();
                return RedirectToAction("ThemeList", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult EditTheme(int id)
        {
            try
            {
                Theme theme = db.Themes.Where(s => s.Id == id).FirstOrDefault();
                Teacher teacher = db.Teachers.Where(s => s.Id == theme.TeacherID).FirstOrDefault();
                if (teacher != null) theme.FullName = teacher.Surname + " " + teacher.Name + " " + teacher.Middlename;
                List<string> subjects = new List<string>();
                List<string> teachers = new List<string>();
                foreach (var i in db.Teachers)
                {
                    if (!subjects.Contains(i.Subject)) subjects.Add(i.Subject);
                }
                teachers.Add("Нет");
                foreach (var i in db.Teachers)
                {
                    if (!teachers.Contains(i.Surname + " " + i.Name + " " + i.Middlename)) teachers.Add(i.Surname + " " + i.Name + " " + i.Middlename);
                }
                ViewBag.Subjects = subjects;
                ViewBag.Teachers = teachers;

                return View(theme);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        [HttpPost]
        public ActionResult EditTheme(Theme theme)
        {
            try
            {
                Theme newTheme;
                IEnumerable<Teacher> teacherList = db.Teachers;
                Teacher teacher = new Teacher();
                if (theme.FullName != "Нет")
                {
                    teacher = teacherList.Where(s => s.Surname == theme.FullName.Split(' ')[0] &&
                    s.Name == theme.FullName.Split(' ')[1] &&
                    s.Middlename == theme.FullName.Split(' ')[2]).FirstOrDefault();
                }
                else
                {
                    teacher.Id = -1;
                }
                Student student = new Student();

                using (var ctx = new DbContext())
                {
                    newTheme = ctx.Themes.Where(s => s.Id == theme.Id).FirstOrDefault<Theme>();
                }

                newTheme.Topic = theme.Topic;
                newTheme.Type = theme.Type;
                newTheme.Description = theme.Description;
                newTheme.TeacherID = teacher.Id;

                using (var newContext = new DbContext())
                {
                    newContext.Entry(newTheme).State = System.Data.Entity.EntityState.Modified;
                    newContext.SaveChanges();
                }
                return RedirectToAction("ThemeList", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult RemoveStudent(int id)
        {
            try
            {
                Theme theme = db.Themes.Where(s => s.Id == id).FirstOrDefault();
                Student student = db.Students.Where(s => s.Id == theme.StudentID).FirstOrDefault();
                student.Got = false;
                theme.Completed = false;
                theme.StudentID = -1;

                using (var newContext = new DbContext())
                {
                    newContext.Entry(student).State = System.Data.Entity.EntityState.Modified;
                    newContext.Entry(theme).State = System.Data.Entity.EntityState.Modified;
                    newContext.SaveChanges();
                }
                return RedirectToAction("ThemeList", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }


        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult AddToUser()
        {
            try
            {
                List<string> Students = new List<string>();
                List<string> Themes = new List<string>();
                List<string> Teachers = new List<string>();

                if (User.IsInRole("Teacher"))
                {
                    ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();

                    foreach (var i in db.Students)
                    {
                        if (!i.Got) Students.Add(i.Surname + " " + i.Name);
                    }
                    foreach (var i in db.Themes)
                    {
                        if (i.StudentID < 0 && i.Subject == user.Group) Themes.Add(i.Topic);
                    }
                    foreach (var i in db.Teachers)
                    {
                        if (i.Name == user.Name && i.Surname == user.Surname && i.Middlename == user.Middlename) Teachers.Add(i.Surname + " " + i.Name + " " + i.Middlename);
                    }
                }
                else
                {
                    foreach (var i in db.Students)
                    {
                        if (!i.Got) Students.Add(i.Surname + " " + i.Name);
                    }
                    foreach (var i in db.Themes)
                    {
                        if (i.StudentID < 0) Themes.Add(i.Topic);
                    }
                    foreach (var i in db.Teachers)
                    {
                        Teachers.Add(i.Surname + " " + i.Name + " " + i.Middlename);
                    }
                }

                SelectList students = new SelectList(Students);
                SelectList themes = new SelectList(Themes);
                SelectList teachers = new SelectList(Teachers);

                ViewBag.Students = students;
                ViewBag.Themes = themes;
                ViewBag.Teachers = teachers;
                return View();
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "Admin, Teacher")]
        [HttpPost]
        public ActionResult AddToUser(string Student, string Teacher, string Theme)
        {
            try
            {
                IEnumerable<Student> Students = db.Students.Where(s => s.Name == Student.Split(' ')[1] && s.Surname == Student.Split(' ')[0]);

                Student studenttoAdd = null;
                Teacher teachertoAdd = null;
                Theme themetoAdd = null;


                foreach (var i in db.Students)
                {
                    if (i.Name == Student.Split(' ')[1] && i.Surname == Student.Split(' ')[0]) studenttoAdd = i;
                }

                foreach (var i in db.Teachers)
                {
                    if (i.Surname == Teacher.Split(' ')[0] && i.Name == Teacher.Split(' ')[1] && i.Middlename == Teacher.Split(' ')[2]) teachertoAdd = i;
                }

                foreach (var i in db.Themes)
                {
                    if (i.Topic == Theme) themetoAdd = i;
                }

                themetoAdd.StudentID = studenttoAdd.Id;
                themetoAdd.TeacherID = teachertoAdd.Id;
                studenttoAdd.Got = true;

                using (var newContext = new DbContext())
                {
                    newContext.Entry(themetoAdd).State = System.Data.Entity.EntityState.Modified;
                    newContext.Entry(studenttoAdd).State = System.Data.Entity.EntityState.Modified;
                    newContext.SaveChanges();
                }

                return RedirectToAction("AddToUser", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public ActionResult TakeTheme(int? page)
        {
            try
            {
                Models.DB.Settings DeadLine = db.SystemTable.FirstOrDefault();
                ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();
                ViewBag.tooLate = false;
                ViewBag.Geted = false;
                Student stud = db.Students.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault();
                if (stud.Got)
                {
                    ViewBag.Geted = true;
                    return View();
                }
                if (((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])) - 5 == 0)
                {
                    if ((DateTime.Compare(DeadLine.DiplTime, DateTime.Now) < 0) || (DateTime.Compare(DeadLine.DiplTimeDL, DateTime.Now) < 0)) ViewBag.tooLate = true;
                    int pageSize = 20;
                    int pageNumber = (page ?? 1);
                    // получаем из бд все объекты Themes
                    IEnumerable<Theme> themes = db.Themes.Where(c => c.Type == "Диплом");
                    ViewBag.listCount = themes.Count();
                    // возвращаем представление
                    return View(themes.OrderBy(i => i.Topic).ToPagedList(pageNumber, pageSize));
                }
                else
                if (((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])) - 5 < 0)
                {
                    if ((DateTime.Compare(DeadLine.CursTime, DateTime.Now) < 0) || (DateTime.Compare(DeadLine.CursTimeDL, DateTime.Now) < 0)) ViewBag.tooLate = true;
                    int pageSize = 20;
                    int pageNumber = (page ?? 1);
                    // получаем из бд все объекты Themes
                    IEnumerable<Theme> themes = db.Themes.Where(c => c.Type == "Курсовая");
                    ViewBag.listCount = themes.Count();
                    // возвращаем представление
                    return View(themes.OrderBy(i => i.Topic).ToPagedList(pageNumber, pageSize));
                }
                else
                    return RedirectToAction("ErrorPage", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }


        [Authorize(Roles = "User")]
        public ActionResult TakeThemeByUser(int id)
        {
            try
            {
                Models.DB.Settings DeadLine = db.SystemTable.FirstOrDefault();
                if (((DateTime.Compare(DeadLine.DiplTime, DateTime.Now) < 0) || (DateTime.Compare(DeadLine.DiplTimeDL, DateTime.Now) < 0)) ||
                    (DateTime.Compare(DeadLine.CursTime, DateTime.Now) < 0) || (DateTime.Compare(DeadLine.CursTimeDL, DateTime.Now) < 0))
                    return RedirectToAction("Main", "Home");

                ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();
                Student student = db.Students.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault();

                Theme themetoAdd = null;

                foreach (var i in db.Themes)
                {
                    if (i.Id == id) themetoAdd = i;
                }

                themetoAdd.StudentID = student.Id;
                student.Got = true;

                using (var newContext = new DbContext())
                {
                    newContext.Entry(themetoAdd).State = System.Data.Entity.EntityState.Modified;
                    newContext.Entry(student).State = System.Data.Entity.EntityState.Modified;
                    newContext.SaveChanges();
                }
                return RedirectToAction("Main", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult PersonalMail(int? page)
        {
            try
            {
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                // получаем из бд все объекты Messages
                List<Message> messages = new List<Message>();
                ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();
                Student student = db.Students.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == s.Middlename).FirstOrDefault();
                try
                {
                    Theme theme = db.Themes.Where(s => s.StudentID == student.Id).FirstOrDefault();
                    if (theme == null)
                        ViewBag.GetTheme = false;
                    else
                        ViewBag.GetTheme = true;
                }
                catch
                {
                    ViewBag.GetTheme = false;
                }
                //db.Messages.Where(s => s.ToUsers.Split(',').Contains(User.Identity.Name) || s.FromUser == User.Identity.Name);
                foreach (var message in db.Messages)
                {
                    if (message.ToUsers.Split(',').Contains(User.Identity.Name) || message.FromUser == User.Identity.Name || message.FromUser == "System")
                    {
                        if (message.FromUser == "System")
                        {
                            if (user.Type == "Student")
                            {
                                if ((((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])) == 5) && message.ToUsers == "_AllStudentD")
                                {
                                    messages.Add(message);
                                }
                                else if ((((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])) < 5) && message.ToUsers == "_AllStudentK")
                                {
                                    messages.Add(message);
                                }
                                else continue;
                            }
                            else
                                messages.Add(message);
                        }
                        else
                            messages.Add(message);
                    }
                }
                ViewBag.listCount = messages.Count();
                // возвращаем представление
                return View(messages.OrderByDescending(i => i.Created).ToPagedList(pageNumber, pageSize));
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize]
        public JsonResult MessageCount()
        {
            int data = 0;
            ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();
            IEnumerable<Message> mesList = db.Messages;
            foreach (var i in mesList)
            {
                if (i.FromUser == "System" && !(i.Read.Split(',').Contains(User.Identity.Name)))
                {
                    if (i.FromUser == "System")
                    {
                        if (user.Type == "Student")
                        {
                            if ((((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])) == 5) && i.ToUsers == "_AllStudentD")
                            {
                                data += 1;
                            }
                            else if ((((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])) < 5) && i.ToUsers == "_AllStudentK")
                            {
                                data += 1;
                            }
                            else continue;
                        }
                        else
                            data += 1;
                    }
                    else
                        data += 1;
                    continue;
                }
                if (i.ToUsers.Split(',').Contains(User.Identity.Name) && !(i.Read.Split(',').Contains(User.Identity.Name)))
                    data += 1;
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        public ActionResult Mail(int page)
        {
            try
            {
                var message = db.Messages.Find(page);

                if (!message.Read.Split(',').Contains(User.Identity.Name))
                {
                    message.Read += User.Identity.Name + ",";
                    using (var newContext = new DbContext())
                    {
                        newContext.Entry(message).State = System.Data.Entity.EntityState.Modified;
                        newContext.SaveChanges();
                    }
                }

                IEnumerable<Diplom.Models.DB.Files> files = db.Files.Where(s => s.IdMessage == message.Id);
                if (message.FromUser == "System")
                    ViewBag.FromUser = "Система";
                else
                {
                    ApplicationUser user = UserManager.Users.Where(s => s.UserName == message.FromUser).FirstOrDefault();
                    if (user.Type == "Teacher")
                        ViewBag.FromUser = "пр. " + user.Surname + " " + user.Name + " " + user.Middlename;
                    else if (user.Type == "Admin")
                        ViewBag.FromUser = "Администратор";
                    else if (user.Type == "Student")
                        ViewBag.FromUser = "ст. " + user.Surname + " " + user.Name + " " + user.Middlename;
                }
                ViewBag.Files = files;
                ViewBag.FilesCount = files.Count();
                return View(message);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize]
        public ActionResult AddMail()
        {
            try
            {
                List<string> users = new List<string>();
                foreach (var user in UserManager.Users)
                {
                    if (user.UserName != User.Identity.Name)
                    {
                        if (user.Type == "Teacher")
                            users.Add(user.UserName + " (пр. " + user.Name + " " + user.Surname + ")");
                        users.Add(user.UserName + " (" + user.Name + " " + user.Surname + ")");
                    }
                }
                ViewBag.Users = new MultiSelectList(users);
                return View();
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddMail(Message message)
        {
            try
            {
                message.FromUser = User.Identity.Name;
                string temp = "";
                foreach (var i in message.ToUsersList)
                {
                    temp += i.Split(' ')[0];
                    temp += ',';
                }
                message.ToUsers = temp.Substring(0, temp.Length - 1);
                message.Read = User.Identity.Name + ",";
                bool check = true;
                db.Messages.Add(message);
                db.SaveChanges();
                if (message.FilesUpload.Count() > 0)
                {
                    foreach (var file in message.FilesUpload)
                    {
                        if (file == null) continue;
                        var fileName = Path.GetFileName(file.FileName);
                        string path = AppDomain.CurrentDomain.BaseDirectory + "UploadedFiles/";
                        string fullpath = Path.Combine(path, fileName);
                        while (check)
                        {
                            if (System.IO.File.Exists(fullpath))
                            {
                                fullpath = Path.Combine(path, fileName.Split('.')[0] + "1" + "." + fileName.Split('.')[1]);
                                fileName = fileName.Split('.')[0] + "1" + "." + fileName.Split('.')[1];
                                continue;
                            }
                            check = false;
                        }
                        check = true;
                        Diplom.Models.DB.Files FileToAdd = new Models.DB.Files();
                        FileToAdd.FileName = Path.GetFileName(file.FileName);
                        FileToAdd.FilePath = fullpath;
                        FileToAdd.IdMessage = message.Id;
                        db.Files.Add(FileToAdd);
                        file.SaveAs(fullpath);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Mail", new { page = message.Id });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult Deletemail(int id)
        {
            try
            {
                Message MessagetoDelete;
                IEnumerable<Message> messages = db.Messages;
                using (var ctx = new DbContext())
                {
                    MessagetoDelete = ctx.Messages.Where(s => s.Id == id).FirstOrDefault<Message>();
                }

                var temp = MessagetoDelete.ToUsers.Split(',').ToList();

                temp.Remove(User.Identity.Name);

                string tempStr = String.Join(",", temp.ToArray());

                MessagetoDelete.ToUsers = tempStr;

                using (var newContext = new DbContext())
                {
                    if (MessagetoDelete.FromUser == User.Identity.Name)
                        newContext.Entry(MessagetoDelete).State = System.Data.Entity.EntityState.Deleted;
                    else
                        newContext.Entry(MessagetoDelete).State = System.Data.Entity.EntityState.Modified;
                    System.Threading.Thread.Sleep(1000);
                    newContext.SaveChanges();
                }
                // возвращаем представление
                return RedirectToAction("PersonalMail", "Home");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize(Roles = "User")]
        public ActionResult MesstoTeacher()
        {
            ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();
            Student student = db.Students.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault();
            Theme theme = db.Themes.Where(s => s.StudentID == student.Id).FirstOrDefault();
            Teacher teacher = db.Teachers.Where(s => s.Id == theme.TeacherID).FirstOrDefault();
            user = UserManager.Users.Where(s => s.Name == teacher.Name && s.Surname == teacher.Surname && s.Middlename == teacher.Middlename).FirstOrDefault();

            ViewBag.Teacher = user.UserName + "@" + teacher.Surname + " " + teacher.Name + " " + teacher.Middlename;
            return View();
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public ActionResult MesstoTeacher(Message message)
        {
            try
            {
                message.FromUser = User.Identity.Name;
                message.Read = User.Identity.Name + ",";
                bool check = true;
                db.Messages.Add(message);
                db.SaveChanges();
                if (message.FilesUpload.Count() > 0)
                {
                    foreach (var file in message.FilesUpload)
                    {
                        if (file == null) continue;
                        var fileName = Path.GetFileName(file.FileName);
                        string path = AppDomain.CurrentDomain.BaseDirectory + "UploadedFiles/";
                        string fullpath = Path.Combine(path, fileName);
                        while (check)
                        {
                            if (System.IO.File.Exists(fullpath))
                            {
                                fullpath = Path.Combine(path, fileName.Split('.')[0] + "1" + "." + fileName.Split('.')[1]);
                                fileName = fileName.Split('.')[0] + "1" + "." + fileName.Split('.')[1];
                                continue;
                            }
                            check = false;
                        }
                        check = true;
                        Diplom.Models.DB.Files FileToAdd = new Models.DB.Files();
                        FileToAdd.FileName = Path.GetFileName(file.FileName);
                        FileToAdd.FilePath = fullpath;
                        FileToAdd.IdMessage = message.Id;
                        db.Files.Add(FileToAdd);
                        file.SaveAs(fullpath);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Mail", new { page = message.Id });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize]
        public ActionResult Reply(string topic, string toUser)
        {
            ViewBag.ToMail = topic;
            ViewBag.Touser = toUser;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Reply(Message message)
        {
            try
            {
                message.FromUser = User.Identity.Name;
                message.Read = User.Identity.Name + ",";
                message.Topic = message.Topic + " (ответ)";
                bool check = true;
                db.Messages.Add(message);
                db.SaveChanges();
                if (message.FilesUpload.Count() > 0)
                {
                    foreach (var file in message.FilesUpload)
                    {
                        if (file == null) continue;
                        var fileName = Path.GetFileName(file.FileName);
                        string path = AppDomain.CurrentDomain.BaseDirectory + "UploadedFiles/";
                        string fullpath = Path.Combine(path, fileName);
                        while (check)
                        {
                            if (System.IO.File.Exists(fullpath))
                            {
                                fullpath = Path.Combine(path, fileName.Split('.')[0] + "1" + "." + fileName.Split('.')[1]);
                                fileName = fileName.Split('.')[0] + "1" + "." + fileName.Split('.')[1];
                                continue;
                            }
                            check = false;
                        }
                        check = true;
                        Diplom.Models.DB.Files FileToAdd = new Models.DB.Files();
                        FileToAdd.FileName = Path.GetFileName(file.FileName);
                        FileToAdd.FilePath = fullpath;
                        FileToAdd.IdMessage = message.Id;
                        db.Files.Add(FileToAdd);
                        file.SaveAs(fullpath);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Mail", new { page = message.Id });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        public FileResult Download(string FileName, string FilePath)
        {
            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "UploadedFiles" + "\\" + FilePath.Split('/')[1], FileMode.Open);
            string file_type = "application/" + FileName.Split('.')[1];
            string file_name = FileName;
            return File(fs, file_type, file_name);
            //return File(AppDomain.CurrentDomain.BaseDirectory + "UploadedFiles" + "\\" + FileName, System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult Settings()
        {
            try
            {
                Diplom.Models.DB.Settings settings = db.SystemTable.FirstOrDefault();
                return View(settings);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }


        [Authorize(Roles = "Admin, Teacher")]
        [HttpPost]
        public ActionResult Settings(Diplom.Models.DB.Settings settings)
        {
            try
            {
                Diplom.Models.DB.Settings settingsOld = db.SystemTable.FirstOrDefault();
                if (settingsOld == null)
                {
                    settingsOld = new Models.DB.Settings();
                    settingsOld.CursTime = settings.CursTime;
                    settingsOld.CursTimeDL = settings.CursTimeDL;
                    //settingsOld.CursTimeDL = settings.CursTime;

                    settingsOld.DiplTime = settings.DiplTime;
                    settingsOld.DiplTimeDL = settings.CursTimeDL;
                    //settingsOld.DiplTimeDL = settings.DiplTime;

                    db.SystemTable.Add(settingsOld);
                    db.SaveChanges();
                    return View(settings);
                }

                settingsOld.CursTime = settings.CursTime;
                settingsOld.CursTimeDL = settings.CursTimeDL;
                settingsOld.DiplTime = settings.DiplTime;
                settingsOld.DiplTimeDL = settings.DiplTimeDL;

                using (var newContext = new DbContext())
                {
                    newContext.Entry(settingsOld).State = System.Data.Entity.EntityState.Modified;
                    newContext.SaveChanges();
                }
                return View(settings);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }


        [Authorize]
        public FileResult PrintDoc(Diplom.Models.DB.PrintModel model)
        {
            try
            {
                ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();
                Student student = db.Students.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault();
                Theme theme = db.Themes.Where(s => s.StudentID == student.Id).FirstOrDefault();
                Teacher teacher = db.Teachers.Where(s => s.Id == theme.TeacherID).FirstOrDefault();

                model.FIOStudent = student.Surname + " " + student.Name + " " + student.Middlename;
                model.FIOTeacher = teacher.Surname + " " + teacher.Name + " " + teacher.Middlename;
                model.Group = student.Group;
                model.Kurs = ((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])).ToString();
                model.Subj = teacher.Subject;
                model.Theme = theme.Topic;
                model.Year = DateTime.Now.Year.ToString();

                if (((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])) < 5)
                {

                    if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx"))
                    {
                        System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx");
                    }
                    System.IO.File.Copy(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKurs.docx", AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx");

                    Microsoft.Office.Interop.Word.Application oWord = new Application();
                    Microsoft.Office.Interop.Word.Document oDoc = oWord.Documents.Add(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx");
                    oDoc.Bookmarks["Subj"].Range.Text = model.Subj;
                    oDoc.Bookmarks["Subj2"].Range.Text = model.Subj;
                    oDoc.Bookmarks["Theme"].Range.Text = model.Theme;
                    oDoc.Bookmarks["Theme2"].Range.Text = model.Theme;
                    oDoc.Bookmarks["Kurs"].Range.Text = model.Kurs;
                    oDoc.Bookmarks["Kurs2"].Range.Text = model.Kurs;
                    oDoc.Bookmarks["Group"].Range.Text = model.Group;
                    oDoc.Bookmarks["Group2"].Range.Text = model.Group;
                    oDoc.Bookmarks["FIOStudent"].Range.Text = model.FIOStudent;
                    oDoc.Bookmarks["FIOStudent2"].Range.Text = model.FIOStudent;
                    oDoc.Bookmarks["FIOTeacher"].Range.Text = model.FIOTeacher;
                    oDoc.Bookmarks["FIOTeacher2"].Range.Text = model.FIOTeacher;
                    oDoc.Bookmarks["Year"].Range.Text = model.Year;
                    oDoc.Bookmarks["Year2"].Range.Text = model.Year;

                    oDoc.SaveAs2(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx");
                    System.Threading.Thread.Sleep(1000);
                    oWord.ActiveDocument.Close();
                    System.Threading.Thread.Sleep(1000);
                    oWord.Application.Quit();
                    oWord = null;

                    FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx", FileMode.Open);
                    string file_type = "application/docx";
                    string file_name = "Титульник.docx";
                    return File(fs, file_type, file_name);
                }
                else
                {
                    model.FIOStudent = student.Surname + " " + student.Name[0] + ". " + student.Middlename[0] + ".";
                    model.FIOTeacher = teacher.Surname + " " + teacher.Name[0] + ". " + teacher.Middlename[0] + ".";

                    if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx"))
                    {
                        System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx");
                    }
                    System.IO.File.Copy(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDp.docx", AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx");

                    Microsoft.Office.Interop.Word.Application oWord = new Application();
                    Microsoft.Office.Interop.Word.Document oDoc = oWord.Documents.Add(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx");
                    oDoc.Bookmarks["Theme"].Range.Text = model.Theme;
                    oDoc.Bookmarks["Group"].Range.Text = model.Group;
                    oDoc.Bookmarks["FIOStudent"].Range.Text = model.FIOStudent;
                    oDoc.Bookmarks["FIOTeacher"].Range.Text = model.FIOTeacher;
                    oDoc.Bookmarks["FIOTeacher2"].Range.Text = "";
                    oDoc.Bookmarks["FIOTeacher3"].Range.Text = "";
                    oDoc.Bookmarks["FIOTeacher4"].Range.Text = "";
                    oDoc.Bookmarks["FIOTeacher5"].Range.Text = "";
                    oDoc.Bookmarks["Year"].Range.Text = model.Year;
                    oDoc.Bookmarks["Year2"].Range.Text = model.Year;

                    oDoc.SaveAs2(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx");
                    System.Threading.Thread.Sleep(1000);
                    oWord.ActiveDocument.Close();
                    System.Threading.Thread.Sleep(1000);
                    oWord.Application.Quit();
                    oWord = null;

                    FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx", FileMode.Open);
                    string file_type = "application/docx";
                    string file_name = "Титульник.docx";
                    return File(fs, file_type, file_name);
                }
            }
            catch
            {
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDp.docx", FileMode.Open);
                string file_type = "application/docx";
                string file_name = "Титульник.docx";
                return File(fs, file_type, file_name);
            }
        }

        [Authorize]
        public FileStreamResult ViewDoc(Diplom.Models.DB.PrintModel model)
        {
            try
            {
                ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();
                Student student = db.Students.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault();
                Theme theme = db.Themes.Where(s => s.StudentID == student.Id).FirstOrDefault();
                Teacher teacher = db.Teachers.Where(s => s.Id == theme.TeacherID).FirstOrDefault();

                model.FIOStudent = student.Surname + " " + student.Name + " " + student.Middlename;
                model.FIOTeacher = teacher.Surname + " " + teacher.Name + " " + teacher.Middlename;
                model.Group = student.Group;
                model.Kurs = ((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])).ToString();
                model.Subj = teacher.Subject;
                model.Theme = theme.Topic;
                model.Year = DateTime.Now.Year.ToString();

                if (((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])) < 5)
                {
                    if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx"))
                    {
                        System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx");
                    }
                    System.IO.File.Copy(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKurs.docx", AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx");

                    Microsoft.Office.Interop.Word.Application oWord = new Application();
                    Microsoft.Office.Interop.Word.Document oDoc = oWord.Documents.Add(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx");
                    oDoc.Bookmarks["Subj"].Range.Text = model.Subj;
                    oDoc.Bookmarks["Subj2"].Range.Text = model.Subj;
                    oDoc.Bookmarks["Theme"].Range.Text = model.Theme;
                    oDoc.Bookmarks["Theme2"].Range.Text = model.Theme;
                    oDoc.Bookmarks["Kurs"].Range.Text = model.Kurs;
                    oDoc.Bookmarks["Kurs2"].Range.Text = model.Kurs;
                    oDoc.Bookmarks["Group"].Range.Text = model.Group;
                    oDoc.Bookmarks["Group2"].Range.Text = model.Group;
                    oDoc.Bookmarks["FIOStudent"].Range.Text = model.FIOStudent;
                    oDoc.Bookmarks["FIOStudent2"].Range.Text = model.FIOStudent;
                    oDoc.Bookmarks["FIOTeacher"].Range.Text = model.FIOTeacher;
                    oDoc.Bookmarks["FIOTeacher2"].Range.Text = model.FIOTeacher;
                    oDoc.Bookmarks["Year"].Range.Text = model.Year;
                    oDoc.Bookmarks["Year2"].Range.Text = model.Year;

                    oDoc.SaveAs2(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.docx");
                    oDoc.SaveAs2(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.pdf", 17);
                    System.Threading.Thread.Sleep(1000);
                    oWord.ActiveDocument.Close();
                    System.Threading.Thread.Sleep(1000);
                    oWord.Application.Quit();
                    System.Threading.Thread.Sleep(1000);
                    oWord = null;

                    FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleKursTemp.pdf", FileMode.Open);
                    string file_type = "application/pdf";
                    return new FileStreamResult(fs, file_type);
                }
                else
                {
                    model.FIOStudent = student.Surname + " " + student.Name[0] + ". " + student.Middlename[0] + ".";
                    model.FIOTeacher = teacher.Surname + " " + teacher.Name[0] + ". " + teacher.Middlename[0] + ".";

                    if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx"))
                    {
                        System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx");
                    }
                    System.IO.File.Copy(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDp.docx", AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx");

                    Microsoft.Office.Interop.Word.Application oWord = new Application();
                    Microsoft.Office.Interop.Word.Document oDoc = oWord.Documents.Add(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx");
                    oDoc.Bookmarks["Theme"].Range.Text = model.Theme;
                    oDoc.Bookmarks["Group"].Range.Text = model.Group;
                    oDoc.Bookmarks["FIOStudent"].Range.Text = model.FIOStudent;
                    oDoc.Bookmarks["FIOTeacher"].Range.Text = model.FIOTeacher;
                    oDoc.Bookmarks["FIOTeacher2"].Range.Text = "";
                    oDoc.Bookmarks["FIOTeacher3"].Range.Text = "";
                    oDoc.Bookmarks["FIOTeacher4"].Range.Text = "";
                    oDoc.Bookmarks["FIOTeacher5"].Range.Text = "";
                    oDoc.Bookmarks["Year"].Range.Text = model.Year;
                    oDoc.Bookmarks["Year2"].Range.Text = model.Year;

                    oDoc.SaveAs2(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx");
                    oDoc.SaveAs2(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.pdf", 17);
                    System.Threading.Thread.Sleep(1000);
                    oWord.ActiveDocument.Close();
                    System.Threading.Thread.Sleep(1000);
                    oWord.Application.Quit();
                    System.Threading.Thread.Sleep(1000);
                    oWord = null;

                    FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.pdf", FileMode.Open);
                    string file_type = "application/pdf";
                    return new FileStreamResult(fs, file_type);
                }
            }
            catch
            {
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "App_Data" + "\\" + "ExampleDpTemp.docx", FileMode.Open);
                string file_type = "application/docx";
                string file_name = "Титульник.docx";
                return File(fs, file_type, file_name);
            }
        }


        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult Stats()
        {
            try
            {
                IEnumerable<Student> students = db.Students;

                List<Diplom.Models.DB.studentsStat> studentsStar = new List<Models.DB.studentsStat>();
                List<string> groupList = new List<string>();
                studentsStar.Add(new Models.DB.studentsStat("All"));

                foreach (var i in students)
                {
                    studentsStar[0].addStud();
                    if (i.Got)
                    {
                        studentsStar[0].addTaken();
                    }
                }

                foreach (var i in students)
                {
                    if (!(studentsStar.FindIndex(s => s.Group == i.Group) >= 0)) studentsStar.Add(new Models.DB.studentsStat(i.Group));
                }

                foreach (var i in students)
                {
                    foreach (var k in studentsStar)
                    {
                        if (k.Group == i.Group)
                        {
                            k.addStud();
                            if (i.Got) k.addTaken();
                        }
                    }
                }

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                foreach (var i in studentsStar)
                {
                    groupList.Add(i.Group);
                }

                ViewBag.groupList = groupList;
                ViewBag.studentsStar = serializer.Serialize(studentsStar);
                ViewBag.groupListJSON = serializer.Serialize(groupList);


                List<Diplom.Models.DB.teachersStat> teacherStat = new List<Diplom.Models.DB.teachersStat>();
                List<string> teachers = new List<string>();
                foreach (var i in db.Teachers)
                {
                    if (!teachers.Contains(i.Surname + " " + i.Name + " " + i.Middlename))
                    {
                        teachers.Add(i.Surname + " " + i.Name + " " + i.Middlename);
                        teacherStat.Add(new Diplom.Models.DB.teachersStat(i.Surname + " " + i.Name + " " + i.Middlename, 0, i.Id));
                    }
                }

                foreach (var i in teacherStat)
                {
                    foreach (var k in db.Themes)
                    {
                        if (k.TeacherID == i.TeacherId) i.Themes += 1;
                    }
                }

                ViewBag.teacherStat = serializer.Serialize(teacherStat);

                List<Diplom.Models.DB.themeStat> themeStat = new List<Models.DB.themeStat>();
                themeStat.Add(new Models.DB.themeStat("All"));

                foreach (var i in db.Themes)
                {
                    if (i.StudentID >= 0)
                    {
                        themeStat[0].addTheme();
                        if (i.Completed) themeStat[0].addCompleted();
                    }
                }

                foreach (var i in students)
                {
                    if (!(themeStat.FindIndex(s => s.Group == i.Group) >= 0)) themeStat.Add(new Models.DB.themeStat(i.Group));
                }

                foreach (var i in db.Themes)
                {
                    foreach (var k in db.Students)
                    {
                        if (i.StudentID == k.Id)
                        {
                            foreach (var t in themeStat)
                            {
                                if (k.Group == t.Group)
                                {
                                    t.addTheme();
                                    if (i.Completed) t.addCompleted();
                                    break;
                                }
                            }
                        }
                    }
                }

                ViewBag.themeStat = serializer.Serialize(themeStat);

                ViewBag.CompletedThemes = 0;
                foreach (var i in db.Themes)
                {
                    if (i.Completed && i.StudentID >= 0)
                        ViewBag.CompletedThemes += 1;
                }

                ViewBag.ThemeCount = db.Themes.Where(s => s.StudentID >= 0).Count();

                List<Diplom.Models.DB.markStat> markStat = new List<Models.DB.markStat>();
                for (int i = 0; i < 10; i++)
                    markStat.Add(new Models.DB.markStat(i + 1));


                foreach (var i in db.Themes)
                {
                    if (i.Mark > 0)
                    {
                        markStat.Where(s => s.Mark == i.Mark).FirstOrDefault().Count += 1;
                    }
                }

                ViewBag.markStat = serializer.Serialize(markStat);

                return View();
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home");
            }
        }

        [Authorize]
        public ActionResult ErrorPage()
        {
            return View();
        }

        [Authorize(Roles = "User")]
        public JsonResult AlertMessage()
        {
            Diplom.Models.DB.Settings settings = db.SystemTable.FirstOrDefault();
            ApplicationUser user = UserManager.Users.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();
            Student student = db.Students.Where(s => s.Name == user.Name && s.Surname == user.Surname && s.Middlename == user.Middlename).FirstOrDefault();
            Theme theme = db.Themes.Where(s => s.StudentID == student.Id).FirstOrDefault();
            if (theme == null) return Json(settings, JsonRequestBehavior.AllowGet);
            if (user.Type == "Student" && !theme.Completed)
            {
                if (((int)Char.GetNumericValue(DateTime.Now.Year.ToString()[3]) - (int)Char.GetNumericValue(user.Group[user.Group.Length - 1])) < 5)
                {
                    settings.DiplTime = DateTime.MinValue;
                    settings.DiplTimeDL = DateTime.MinValue;
                    if (student.Got) settings.CursTime = DateTime.MinValue;
                }
                else
                {
                    settings.CursTime = DateTime.MinValue;
                    settings.CursTimeDL = DateTime.MinValue;
                    if (student.Got) settings.DiplTime = DateTime.MinValue;
                }
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            return Json(settings, JsonRequestBehavior.AllowGet);
        }



        [Authorize]
        public ActionResult Search(string searchString, int? page)
        {
            if (searchString == "") searchString = null;
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            List<News> news = new List<Models.News>();
            if (searchString != null)
            {
                // получаем из бд все объекты News               
                foreach (var item in db.News)
                {
                    if (item.Topic.Contains(searchString) || item.Text.Contains(searchString))
                        news.Add(item);
                }
            }
            IEnumerable<News> newsOUT = news;
            ViewBag.searchString = searchString;
            ViewBag.listCount = newsOUT.Count();
            return View(newsOUT.OrderByDescending(i => i.Created).ToPagedList(pageNumber, pageSize));
        }
    }
}