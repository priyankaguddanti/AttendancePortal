using System.Web.Http;
using System.Web.Mvc;

namespace AttendancePortal.Areas.AttendanceApi
{
    public class AttendanceApiAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "AttendanceApi";

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.Routes.MapHttpRoute(
                "AttendanceApi_Login",
                "api/Attendance/Login",
                new {controller= "Attendance",  action = "Login" }
            );

            context.Routes.MapHttpRoute(
                "AttendanceApi_CheckInCourse",
                "api/Attendance/CheckInCourse",
                new { controller = "Attendance", action = "CheckInCourse" }
            );

            context.Routes.MapHttpRoute(
                "AttendanceApi_ClassViewDetails",
                "api/Attendance/GetClassViewDetails",
                new { controller = "Attendance", action = "GetClassViewDetails" }
            );

            context.Routes.MapHttpRoute(
               "AttendanceApi_DisputeCourse",
               "api/Attendance/DisputeCourse",
               new { controller = "Attendance", action = "DisputeCourse" }
           );
        }
    }
}