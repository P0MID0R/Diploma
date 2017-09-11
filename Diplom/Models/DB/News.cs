using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PagedList;
using PagedList.Mvc;


namespace Diplom.Models
{
    public class News
    {
        [ScaffoldColumn(false)]
        // ID новости
        public int Id { get; set; }

        [Display(Name = "Заголовок")]
        [Required]
        // Тема
        public string Topic { get; set; }

        [ScaffoldColumn(false)]
        //[DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        // Дата создания
        public DateTime Created { get; set; }

        [Display(Name = "Текст")]
        [DataType(DataType.MultilineText)]
        [Required]
        // Текст
        public string Text { get; set; }

        [Display(Name = "Файлы")]
        // Прикрепленные файлы (не обязательно)
        [ScaffoldColumn(false)]
        public IEnumerable<HttpPostedFileBase> FilesUpload { get; set; }

        public News()
        {
            Created = DateTime.Now;
        }

    }
}