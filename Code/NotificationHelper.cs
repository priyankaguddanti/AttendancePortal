using AttendancePortal.Dal;

namespace AttendancePortal.Code
{
    public class NotificationHelper
    {
        private static readonly DalHelper DalHelper = new DalHelper();
        
        public static void SendCourseStartNotifications()
        {
            var result = DalHelper.GetCourseStartNotifications();

            if(result.HasError)
                return;

            //foreach (var courseStart in result.Value)
            //{
            //    var body = $"Your course check in is going to start at - {courseStart.CourseWillStartIn}";
            //    Email.SendEmail(courseStart.EmailAddress, courseStart.FirstName, "Course Start Notification", body);
            //}
        }
    }
}