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
    public class Theme
    {
        [ScaffoldColumn(false)]
        // ID темы
        public int Id { get; set; }

        [Display(Name = "Тип")]
        [Required]
        // Тип темы
        public string Type { get; set; }

        [Display(Name = "Предмет")]
        [Required]
        // Тип темы
        public string Subject { get; set; }

        [Display(Name = "Тема")]
        [Required]
        // Нозвание темы
        public string Topic { get; set; }

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        // Краткое описание (не обязательно)
        public string Description { get; set; }

        [Display(Name = "Оценка")]
        // Оценка
        public int Mark { get; set; }

        [ScaffoldColumn(false)]
        // ID студента
        public int StudentID { get; set; }

        [ScaffoldColumn(false)]
        // ID преподавателя
        public int TeacherID { get; set; }
        public bool Completed { get; set; }
        [NotMapped]
        // Ммя преподавателя
        [Display(Name = "Руководитель")]
        public string FullName { get; set; }
    }
}