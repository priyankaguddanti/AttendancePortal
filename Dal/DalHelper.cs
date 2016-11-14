using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI.WebControls;
using AttendancePortal.Areas.AttendanceApi.Models;
using AttendancePortal.Code;
using AttendancePortal.Models;

namespace AttendancePortal.Dal
{
    public class DalHelper
    {
        public Result<User> GetUserByUserName(string userName)
        {
            try
            {
                using (var context = new AttendanceDataContext())
                {
                    var user = context.Users.FirstOrDefault(c => c.UserName == userName);
                    if (user == null)
                        throw new InvalidOperationException("user not found");

                    return new Result<User>(user);
                }
            }
            catch (Exception ex)
            {
                return new Result<User>(HttpStatusCode.InternalServerError, "User not found");
            }
        }

        public Result<List<Course>> GetProfessorCoursesByUser(string userName)
        {
            return new Result<List<Course>>(HttpStatusCode.OK);
        }

        public Result<List<StudentCourseDetails>> GetStudentCoursesByUser(string userName)
        {
            var result = GetUserByUserName(userName);

            if (result.HasError)
                return new Result<List<StudentCourseDetails>>(HttpStatusCode.InternalServerError);

            using (var context = new AttendanceDataContext())
            {
                var studentCourseDetails = from uc in context.UserCourses
                    join c in context.Courses on uc.CourseId equals c.Id
                    join ca in context.CourseAttendances on uc.CourseId equals ca.CourseId into ss
                    from ca in ss.DefaultIfEmpty()
                    where uc.UserId == result.Value.Id
                    select new StudentCourseDetails
                    {
                        CourseStartTime = c.CourseStartTime,
                        CheckInStartTime = c.CheckInStartTime,
                        CheckInEndTime = c.CheckInEndTime,
                        CourseId = c.Id,
                        CourseEndTime = c.CourseEndTime,
                        Status = ca.Status,
                        CourseName = c.CourseName
                    };

                return new Result<List<StudentCourseDetails>>(studentCourseDetails.ToList());
            }
        }

        public Result<List<ClassViewModel>> GetClassViewDetails(ClassViewDetailsRequest request)
        {
            var result = GetUserByUserName(request.UserName);

            if (result.HasError)
                return new Result<List<ClassViewModel>>(HttpStatusCode.InternalServerError);

            var backDateRange = GetClassViewBackDate(request.Range);

            var classViewDetailsList = new List<ClassViewModel>();

            using (var context = new AttendanceDataContext())
            {
                var courseAttendances =
                    context.CourseAttendances.Where(
                        c =>
                            c.UserId == result.Value.Id &&
                            (c.Created < DateTime.Now) && (c.Created > DateTime.Now.AddDays(-backDateRange)));

                courseAttendances = courseAttendances.OrderByDescending(c => c.Created);

                foreach (var attendance in courseAttendances)
                {
                    var status = attendance.Status;
                    if (attendance.Disputed && !attendance.DisputeRespondedBy.HasValue)
                    {
                        status = AttendanceStatus.Pending.ToString();
                    }

                    classViewDetailsList.Add(new ClassViewModel
                    {
                        CourseAttendanceId = attendance.Id,
                        Status = status,
                        CourseTitle = attendance.Course.CourseName,
                        CourseAttendedDate = attendance.Created.ToString("MM/dd/yy"),
                        CanBeDisputed =
                        (attendance.Status == AttendanceStatus.Tardy.ToString() ||
                         attendance.Status == AttendanceStatus.Absent.ToString()) && !attendance.Disputed
                    });
                }
            }

            return new Result<List<ClassViewModel>>(classViewDetailsList);
        }

        private int GetClassViewBackDate(ClassViewRange range)
        {
            switch (range)
            {
                case ClassViewRange.OneMonth:
                    return 30;
                case ClassViewRange.TwoWeeks:
                    return 15;
                case ClassViewRange.SevenDays:
                    return 7;
                default:
                    return 3;
            }
        }

        public Result<Course> GetCourseById(int courseId)
        {
            using (var context = new AttendanceDataContext())
            {
                var course = context.Courses.FirstOrDefault(c => c.Id == courseId);
                if (course == null)
                    return new Result<Course>(HttpStatusCode.InternalServerError);

                return new Result<Course>(course);
            }
        }

        public CoursesDetailsModel GetCourseDetails(int courseId)
        {
            using (var context = new AttendanceDataContext())
            {
                var course = context.Courses.FirstOrDefault(c => c.Id == courseId);

                var users = from uc in context.UserCourses
                    join u in context.Users on uc.UserId equals u.Id
                    where uc.CourseId == courseId && u.RoleId == 3
                    select u;

                //var users = context.UserCourses.Where(c => c.CourseId == courseId && c.User.RoleId == 3).Select(j=> j.User).ToList();
                var totalUsers = context.Users.Where(c => c.RoleId == 3).ToList();

                return new CoursesDetailsModel
                {
                    Course = course,
                    CourseUsers = users.ToList(),
                    TotalUsers = totalUsers
                };
            }
        }

        public Result InsertCourseAttendance(CourseAttendance model)
        {
            try
            {
                using (var context = new AttendanceDataContext())
                {
                    context.CourseAttendances.InsertOnSubmit(model);
                    context.SubmitChanges();
                }

                return new Result();
            }
            catch (Exception)
            {
                return new Result(HttpStatusCode.InternalServerError, "Unable to check in");
            }
        }

        public Result DisputeCourse(DisputeCourseRequest request)
        {
            try
            {
                using (var context = new AttendanceDataContext())
                {
                    var courseAttendance = context.CourseAttendances.FirstOrDefault(c => c.Id == request.CourseAttendanceId);

                    if (courseAttendance == null)
                        return new Result(HttpStatusCode.InternalServerError, "Unable to find the course attended");

                    courseAttendance.Disputed = true;
                    courseAttendance.DisputedReason = request.DisputeReason;
                    courseAttendance.DisputedDate = DateTime.Now;

                    context.SubmitChanges();
                    return new Result();
                }
            }
            catch (Exception)
            {
                return new Result(HttpStatusCode.InternalServerError, "Unable to update dispute reason");
            }
            
        }
    }
}