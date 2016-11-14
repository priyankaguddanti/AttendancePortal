using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttendancePortal.Areas.AttendanceApi.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Tardy,
        Pending
    }
}