using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttendancePortal.Areas.AttendanceApi.Models
{
    public class DisputeCourseRequest
    {
        public int CourseAttendanceId { get; set; }

        public string DisputeReason { get; set; }
    }
}