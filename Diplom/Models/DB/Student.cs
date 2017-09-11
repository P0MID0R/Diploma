using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PagedList;
using PagedList.Mvc;

namespace Diplom.Models
{
    public class Student
    {
        [ScaffoldColumn(false)]
        // ID студента
        public int Id { get; set; }

        [ScaffoldColumn(false)]
        // ID студента
        public bool Got { get; set; }

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

        [Display(Name = "Группа")]
        [Required]
        // Номер группы
        public string Group { get; set; }

        [NotMapped]
        // Ммя преподавателя
        [Display(Name = "Название темы")]
        public string Theme { get; set; }
    }
}