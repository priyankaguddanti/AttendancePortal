using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttendancePortal.Areas.AttendanceApi.Models
{
    public class StudentCourseDetails
    {
        public string CourseName { get; set; }

        public TimeSpan? CourseStartTime { get; set; }

        public TimeSpan? CourseEndTime { get; set; }

        public string CheckInStartTime { get; set; }

        public string CheckInEndTime { get; set; }

        public int CourseId { get; set; }

        public string Status { get; set; }
    }
}