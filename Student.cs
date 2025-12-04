using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp3
{
    public class Student
    {
        public string Name { get; set; }
        public string Faculty { get; set; }
        public string Course { get; set; }
        public string ResultDetails { get; set; } 

        public override string ToString()
        {
            return $"Студент: {Name}, Ф-т: {Faculty}, {ResultDetails}";
        }
    }

    public class SearchCriteria
    {
        public string Faculty { get; set; }
        public string Course { get; set; }
        public string SubjectName { get; set; }
    }
}
