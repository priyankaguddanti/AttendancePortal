namespace AttendancePortal.Areas.AttendanceApi.Models
{
    public class ClassViewModel
    {
        public string CourseTitle { get; set; }

        public string CourseAttendedDate { get; set; }

        public string Status { get; set; }

        public bool CanBeDisputed { get; set; }

        public int CourseAttendanceId { get; set; }
    }
}