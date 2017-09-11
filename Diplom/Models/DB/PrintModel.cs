using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Diplom.Models.DB
{
    public class PrintModel
    {
        [Display(Name = "ФИО Студента")]
        public string FIOStudent { get; set; }
        [Display(Name = "ФИО Преподавателя")]
        public string FIOTeacher { get; set; }

        [Display(Name = "Группа")]
        public string Group { get; set; }

        [Display(Name = "Курс")]
        public string Kurs { get; set; }

        [Display(Name = "Предмет")]
        public string Subj { get; set; }

        [Display(Name = "Тема")]
        public string Theme { get; set; }

        [Display(Name = "Год")]
        public string Year { get; set; }
    }
}