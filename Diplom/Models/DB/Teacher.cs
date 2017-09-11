using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PagedList;
using PagedList.Mvc;

namespace Diplom.Models
{
    public class Teacher
    {

        [ScaffoldColumn(false)]
        // ID преподавателя
        public int Id { get; set; }

        [Display(Name = "Имя")]
        [Required]
        // Имя
        public string Name { get; set; }
        [Display(Name = "Фамилия")]
        [Required]
        // Фамилия
        public string Surname { get; set; }

        [Display(Name = "Отчество")]
        [Required]
        // Отчество
        public string Middlename { get; set; }

        [Display(Name = "Предмет")]
        [Required]
        // Предмет
        public string Subject { get; set; }
    }
}