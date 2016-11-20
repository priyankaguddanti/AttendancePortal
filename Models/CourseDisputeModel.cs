namespace AttendancePortal.Models
{
    public class CourseDisputeModel
    {
        public int CourseAttendanceId { get; set; }

        public string StudentName { get; set; }

        public string DisputedDate { get; set; }

        public bool IsDisputed { get; set; }

        public string Reason { get; set; }
    }
}