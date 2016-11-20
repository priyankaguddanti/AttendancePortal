using System.Collections.Generic;
using AttendancePortal.Dal;

namespace AttendancePortal.Models
{
    public class DisputesViewModel
    {
        public List<Course> AvailableCourses { get; set; } = new List<Course>();

        public List<CourseDisputeModel> AvailableDisputes { get; set; } = new List<CourseDisputeModel>();
    }
}