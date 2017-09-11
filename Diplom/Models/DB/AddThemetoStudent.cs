using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Diplom.Models.DB
{
    public class AddThemetoStudent
    {
        [Required]
        public string Student { get; set; }

        [Required]
        public string Teacher { get; set; }

        [Required]
        public string Theme { get; set; }
    }
}