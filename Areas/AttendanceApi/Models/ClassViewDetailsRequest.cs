using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttendancePortal.Areas.AttendanceApi.Models
{
    public class ClassViewDetailsRequest
    {
        public string UserName { get; set; }

        public ClassViewRange Range { get; set; }
    }
}