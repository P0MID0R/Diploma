using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PagedList;
using PagedList.Mvc;

namespace Diplom.Models.DB
{
    public class Settings
    {
        [ScaffoldColumn(false)]
        // ID студента
        public int Id { get; set; }

        [Display(Name = "Срок выбора курсовой")]
        public DateTime CursTime { get; set; }

        [Display(Name = "Срок выбора диплома")]
        public DateTime DiplTime { get; set; }

        [Display(Name = "Срок сдачи курсовой")]
        public DateTime CursTimeDL { get; set; }

        [Display(Name = "Срок садчи диплома")]
        public DateTime DiplTimeDL { get; set; }
    }
}