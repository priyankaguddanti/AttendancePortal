using System;

namespace AttendancePortal.Models
{
    public class CourseStartNotification
    {
        public string EmailAddress { get; set; }

        public string FirstName { get; set; }

        public string CourseName { get; set; }

        public TimeSpan? CourseStartTime { get; set; }

        public string CourseBeforeTime { get; set; }

        public string CourseWillStartIn
        {
            get
            {
                if (!CourseStartTime.HasValue)
                    return string.Empty;

                var courseBeforeCheckIn = string.IsNullOrWhiteSpace(CourseBeforeTime)
                    ? 0
                    : Convert.ToInt32(CourseBeforeTime);
                return $"{CourseStartTime.Value.Add(new TimeSpan(0, 0, -courseBeforeCheckIn, 0))}";
            }
        }
    }
}