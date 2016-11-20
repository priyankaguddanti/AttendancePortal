namespace AttendancePortal.Models
{
    public class StudentReport
    {
        public string Name { get; set; }

        public int TotalAbsents { get; set; }

        public int TotalTardy { get; set; }

        public int TotalPresents { get; set; }

    }
}