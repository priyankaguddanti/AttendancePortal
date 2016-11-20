using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AttendancePortal.Dal;

namespace AttendancePortal.Models
{
    public class CoursesDetailsViewModel
    {
        public int CourseId { get; set; }


        public string CourseTitle { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public List<Student> Students { get; set; }

        public string BeforeCheckIn { get; set; }

        public string AfterCheckIn { get; set; }
    }
}