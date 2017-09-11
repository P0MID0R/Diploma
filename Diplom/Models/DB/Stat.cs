using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Diplom.Models.DB
{
    public class teachersStat
    {
        public string Teacher { get; set; }
        public int TeacherId { get; set; }
        public int Themes { get; set; }
        public teachersStat(string name, int themes, int id)
        {
            this.Teacher = name;
            this.Themes = themes;
            this.TeacherId = id;
        }
    }

    public class studentsStat
    {
        public string Group { get; set; }
        public int All { get; set; }
        public int Taken { get; set; }
        public studentsStat(string group)
        {
            this.Group = group;
            this.All = 0;
            this.Taken = 0;
        }

        public void addStud()
        {
            this.All += 1;
        }

        public void addTaken()
        {
            this.Taken += 1;
        }
    }

    public class themeStat
    {
        public string Group { get; set; }
        public int All { get; set; }
        public int Completed { get; set; }
        public themeStat(string group)
        {
            this.Group = group;
            this.All = 0;
            this.Completed = 0;
        }

        public void addTheme()
        {
            this.All += 1;
        }

        public void addCompleted()
        {
            this.Completed += 1;
        }
    }

    public class markStat
    {
        public int Mark { get; set; }
        public int Count { get; set; }

        public markStat(int mark)
        {
            this.Mark = mark;
            this.Count = 0;
        }
    }
}