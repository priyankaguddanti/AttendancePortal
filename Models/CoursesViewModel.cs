using System.Collections.Generic;
using AttendancePortal.Dal;

namespace AttendancePortal.Models
{
    public class CoursesViewModel
    {
        public List<Course> Courses { get; set; } = new List<Course>();
    }
}