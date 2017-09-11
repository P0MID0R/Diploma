using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Diplom.Models.DB;
using System.Threading;
using Diplom.Controllers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Owin;
using System.Web.Mvc;
using Diplom.Models;
using Diplom.Models.Users;
using PagedList.Mvc;
using PagedList;
using System.Threading.Tasks;

namespace Diplom.Models.App
{
    public class AlertSender : IJob
    {
        static bool sentK = false;
        static bool sentD = false;
        DbContext db = new DbContext();
        public void Execute(IJobExecutionContext context)
        {
            DateTime dd = DateTime.Now;
            Settings settings = db.SystemTable.FirstOrDefault();
            TimeSpan daysLeftK = settings.CursTimeDL - dd;
            TimeSpan daysLeftD = settings.DiplTimeDL - dd;
            if (daysLeftK.Days < 7 && daysLeftK.Days > 0 && dd.Hour == 8 && sentK == false)
            {
                SendAlert("kurs", "До сдачи курсовой осталось " + daysLeftK.Days + " дней");
                sentK = true;
            }
            else if (daysLeftK.Days > 7)
            {
                sentK = false;
            }

            if (daysLeftD.Days < 7 && daysLeftD.Days > 0 && dd.Hour == 8 && sentD == false)
            {
                SendAlert("dipl", "До сдачи диплома осталось " + daysLeftD.Days + " дней");
                sentD = true;
            }
            else if (daysLeftD.Days > 7)
            {
                sentD = false;
            }
        }
        public bool SendAlert(string flag, string message)
        {
            try
            {
                Message temp = null;
                IEnumerable<Student> student = db.Students;
                IEnumerable<Theme> themes = db.Themes;
                foreach (var i in db.Messages)
                {
                    if (i.FromUser == "System" && (i.ToUsers == "_AllStudentK" || i.ToUsers == "_AllStudentD"))
                    {
                        using (var ctx = new DbContext())
                        {
                            temp = ctx.Messages.Where(s => s.Id == i.Id).FirstOrDefault<Message>();
                        }
                        using (var newContext = new DbContext())
                        {
                            newContext.Entry(temp).State = System.Data.Entity.EntityState.Deleted;
                            System.Threading.Thread.Sleep(1000);
                            newContext.SaveChanges();
                        }
                    }
                }
                temp = new Message();
                if (flag == "kurs")
                {
                    temp.Id = 0;
                    temp.Topic = "Внимание";
                    temp.Text = message;
                    temp.FromUser = "System";
                    temp.ToUsers = "_AllStudentK";
                    temp.Read = "";
                    db.Messages.Add(temp);
                    db.SaveChanges();
                }
                else if (flag == "dipl")
                {
                    temp.Id = 0;
                    temp.Topic = "Внимание";
                    temp.Text = message;
                    temp.FromUser = "System";
                    temp.ToUsers = "_AllStudentD";
                    temp.Read = "";
                    db.Messages.Add(temp);
                    db.SaveChanges();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}