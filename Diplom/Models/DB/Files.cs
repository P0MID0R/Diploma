using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Diplom.Models.DB
{
    public class Files
    {
        [ScaffoldColumn(false)]
        // ID Файла
        public int Id { get; set; }

        [ScaffoldColumn(false)]
        // ID Новости
        public int IdNews { get; set; }

        [ScaffoldColumn(false)]
        // ID сообщения
        public int IdMessage { get; set; }

        [ScaffoldColumn(false)]
        // Заглавный пост
        public string FileName { get; set; }

        [ScaffoldColumn(false)]
        // Заглавный пост
        public string FilePath { get; set; }
    }
}