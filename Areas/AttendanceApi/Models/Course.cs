namespace AttendancePortal.Areas.AttendanceApi
{
    public class Course
    {
        public int CourseId { get; set; }
        
        public string CourseTitle { get; set; }

        public string CheckInTime { get; set; }

        public bool IsEligibleForCheckIn { get; set; }

        public bool AlreadyCheckedIn { get; set; }
    }
}