using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Diplom.Models.Users
{
    public class ApplicationUser : IdentityUser
    {
        // Имя
        public string Name { get; set; }
        // Фамилия
        public string Surname { get; set; }
        // Отчество
        public string Middlename { get; set; }
        // Номер группы/название предмета
        public string Group { get; set; }
        // Дата Регистрации
        public string RegistrationDate { get; set; }
        // Бан
        public bool Banned { get; set; }
        public string Type { get; set; }
        public ApplicationUser()
        {
        }
    }
}