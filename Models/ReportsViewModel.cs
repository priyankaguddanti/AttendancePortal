using System.Collections.Generic;
using AttendancePortal.Dal;

namespace AttendancePortal.Models
{
    public class ReportsViewModel
    {
        public List<Course> AvailableCourses { get; set; } = new List<Course>();

        public List<StudentReport> StudentsReports { get; set; } = new List<StudentReport>();
    }
}