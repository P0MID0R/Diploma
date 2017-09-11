using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PagedList;
using PagedList.Mvc;
using Diplom.Models.Users;

namespace Diplom.Models
{
    public class Message
    {
        [ScaffoldColumn(false)]
        // ID студента
        public int Id { get; set; }

        [Display(Name = "Отправитель")]
        // Отправитель
        public string FromUser { get; set; }

        [Display(Name = "Получатель")]
        // Получатель(и)
        public string ToUsers { get; set; }
        public  IEnumerable<string> ToUsersList { get; set; }

        [Display(Name = "Заголовок")]
        [Required]
        // Тема
        public string Topic { get; set; }

        [ScaffoldColumn(false)]
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

        [ScaffoldColumn(false)]
        // Прочитано?
        public string Read { get; set; }

        public Message()
        {
            Created = DateTime.Now;
        }
    }
}