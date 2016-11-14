using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttendancePortal.Areas.AttendanceApi.Models
{
    public class CheckInCourseModel
    {
        public int CourseId { get; set; }

        public string UserName { get; set; }
    }
}