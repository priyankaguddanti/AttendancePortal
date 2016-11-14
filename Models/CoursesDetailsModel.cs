using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AttendancePortal.Dal;

namespace AttendancePortal.Models
{
    public class CoursesDetailsModel
    {
        public Course Course { get; set; }
        public List<User> TotalUsers { get; set; }

        public List<User> CourseUsers { get; set; }
    }
}