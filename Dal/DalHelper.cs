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
            try
            {
                using (var context = new AttendanceDataContext())
                {
                    var user = context.Users.FirstOrDefault(c => c.UserName == userName);
                    if (user == null)
                        throw new InvalidOperationException("user not found");

                    var courses = context.UserCourses.Where(c => c.UserId == user.Id).Select(j => j.Course).ToList();

                    return new Result<List<Course>>(courses);
                }
            }
            catch (Exception ex)
            {
                return new Result<List<Course>>(HttpStatusCode.InternalServerError, "Unable to get courses");
            }
        }

        public Result SaveUser(User user)
        {
            try
            {
                using (var context = new AttendanceDataContext())
                {
                    var dalUser = context.Users.FirstOrDefault(c => c.UserName == user.UserName);

                    if (dalUser == null)
                        throw new InvalidOperationException("user is null");

                    dalUser.FirstName = user.FirstName;
                    dalUser.LastName = user.LastName;
                    dalUser.EmailAddress = user.EmailAddress;

                    context.SubmitChanges();
                    return new Result();
                }
            }
            catch (Exception)
            {
                return new Result(HttpStatusCode.InternalServerError);
            }
        }

        public Result<List<StudentReport>> GetStudentReportByCourse(int courseId)
        {
            try
            {
                var studentReports = new List<StudentReport>();
                using (var context = new AttendanceDataContext())
                {
                    var studentCourseAttendances = from uc in context.CourseAttendances
                        join u in context.Users on uc.UserId equals u.Id
                        where uc.CourseId == courseId
                        select new StudentCourseAttendance
                        {
                            Name = $"{u.FirstName} {u.LastName}",
                            Status = uc.Status
                        };

                    var studentCourseAttendancesList = studentCourseAttendances.ToList();

                    foreach (var courseAttendances in studentCourseAttendancesList.GroupBy(c => c.Name))
                    {
                        studentReports.Add(new StudentReport
                        {
                            Name = courseAttendances.Key,
                            TotalAbsents =
                                courseAttendances.Count(c => c.Status.Equals(AttendanceStatus.Absent.ToString())),
                            TotalTardy =
                                courseAttendances.Count(c => c.Status.Equals(AttendanceStatus.Tardy.ToString())),
                            TotalPresents =
                                courseAttendances.Count(c => c.Status.Equals(AttendanceStatus.Present.ToString())),
                        });
                    }
                }

                return new Result<List<StudentReport>>(studentReports);
            }
            catch (Exception)
            {
                return new Result<List<StudentReport>>(HttpStatusCode.InternalServerError);
            }
        }

        public Result<List<CourseDisputeModel>> GetDisputesByCourseId(int courseId)
        {
            using (var context = new AttendanceDataContext())
            {
                var studentDisputeDetails = from c in context.CourseAttendances
                    join ua in context.Users on c.UserId equals ua.Id
                    where (c.CourseId == courseId && c.Disputed && c.DisputeRespondedBy == null)
                    select new CourseDisputeModel
                    {
                        DisputedDate = c.DisputedDate.Value.ToShortDateString(),
                        StudentName = $"{ua.FirstName}{ua.LastName}",
                        IsDisputed = c.Disputed,
                        CourseAttendanceId = c.Id,
                        Reason = c.DisputedReason
                    };

                return new Result<List<CourseDisputeModel>>(studentDisputeDetails.ToList());
            }
        }

        public Result UpdateCourseDetals(CoursesDetailsViewModel model)
        {
            try
            {
                using (var context = new AttendanceDataContext())
                {
                    var course = context.Courses.FirstOrDefault(c => c.Id == model.CourseId);
                    if (course == null)
                        throw new InvalidOperationException("Course is not found");

                    course.CheckInStartTime = model.BeforeCheckIn;
                    course.CheckInEndTime = model.AfterCheckIn;

                    foreach (var student in model.Students)
                    {
                        var currentUserCourse =
                            context.UserCourses.FirstOrDefault(
                                c => c.CourseId == model.CourseId && c.UserId == student.Id);
                        if (student.Selected)
                        {
                            if (currentUserCourse == null)
                            {
                                context.UserCourses.InsertOnSubmit(new UserCourse
                                {
                                    CourseId = model.CourseId,
                                    UserId = student.Id,
                                    Created = DateTime.Now
                                });
                            }
                        }
                        else
                        {
                            if (currentUserCourse != null)
                            {
                                context.UserCourses.DeleteOnSubmit(currentUserCourse);
                            }
                        }
                    }

                    context.SubmitChanges();

                    return new Result();
                }
            }
            catch (Exception)
            {
                return new Result(HttpStatusCode.InternalServerError);
            }
        }

        public Result SaveDisputeResponse(int courseAttendanceId, bool disputeAccepted = false)
        {
            try
            {
                var userReult = GetUserByUserName(HttpContext.Current.User.Identity.Name);
                if (userReult.HasError)
                    return userReult;

                using (var context = new AttendanceDataContext())
                {
                    var courseAttendant = context.CourseAttendances.FirstOrDefault(c => c.Id == courseAttendanceId);

                    if (courseAttendant == null)
                    {
                        throw new InvalidOperationException("course attendant is null");
                    }

                    courseAttendant.Status = disputeAccepted
                        ? AttendanceStatus.Present.ToString()
                        : AttendanceStatus.Absent.ToString();
                    courseAttendant.DisputeRespondedBy = userReult.Value.Id;

                    context.SubmitChanges();

                    return new Result();
                }
            }
            catch (Exception)
            {
                return new Result(HttpStatusCode.InternalServerError);
            }
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
                    where uc.UserId == result.Value.Id
                    select new StudentCourseDetails
                    {
                        CourseStartTime = c.CourseStartTime,
                        CheckInStartTime = c.CheckInStartTime,
                        CheckInEndTime = c.CheckInEndTime,
                        CourseId = c.Id,
                        CourseEndTime = c.CourseEndTime,
                        CourseName = c.CourseName
                    };

                var studentCourseDetailsList = studentCourseDetails.ToList();

                var courseIds = studentCourseDetailsList.Select(c => c.CourseId);
                var courseAttendanceDetails =
                    context.CourseAttendances.Where(
                        c =>
                            c.UserId == result.Value.Id && courseIds.Contains(c.CourseId) &&
                            c.Created.Date == DateTime.Now.Date);

                foreach (var courseAttendance in courseAttendanceDetails)
                {
                    var studentCourse =
                        studentCourseDetailsList.FirstOrDefault(c => c.CourseId == courseAttendance.CourseId);
                    if (studentCourse != null)
                    {
                        studentCourse.Status = courseAttendance.Status;
                    }
                }
                return new Result<List<StudentCourseDetails>>(studentCourseDetailsList.ToList());
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
                    var courseAttendance =
                        context.CourseAttendances.FirstOrDefault(c => c.Id == request.CourseAttendanceId);

                    if (courseAttendance == null)
                        return new Result(HttpStatusCode.InternalServerError, "Unable to find the course attended");

                    courseAttendance.Disputed = true;
                    courseAttendance.DisputedReason = request.DisputeReason;
                    courseAttendance.DisputedDate = DateTime.Now;

                    //send notification email
                    var professorDetails = from uc in context.UserCourses
                        join u in context.Users on uc.UserId equals u.Id
                        where uc.CourseId == courseAttendance.CourseId && u.RoleId == 2
                        select new DisputeNotification
                        {
                            DisplayName = u.FirstName,
                            ToAddress = u.EmailAddress,
                            CourseName = courseAttendance.Course.CourseName
                        };

                    var courseProfessorDetails = professorDetails.FirstOrDefault();
                    if (courseProfessorDetails != null)
                    {
                        Email.SendEmail(courseProfessorDetails.ToAddress, courseProfessorDetails.DisplayName,
                            $"Dispute-{courseProfessorDetails.CourseName}", "Student disputed the course");
                    }

                    context.SubmitChanges();

                    return new Result();
                }
            }
            catch (Exception)
            {
                return new Result(HttpStatusCode.InternalServerError, "Unable to update dispute reason");
            }

        }

        public Result<List<CourseStartNotification>> GetCourseStartNotifications()
        { 
            try
            {
                using (var context = new AttendanceDataContext())
                {
                    var courseStart = from c in context.Courses
                        join uc in context.UserCourses on c.Id equals uc.CourseId
                        join u in context.Users on uc.UserId equals u.Id
                        where
                        (c.CourseStartTime > DateTime.Now.TimeOfDay &&
                         c.CourseStartTime < DateTime.Now.AddMinutes(30).TimeOfDay)
                        select new CourseStartNotification
                        {
                            CourseName = c.CourseName,
                            FirstName = u.FirstName,
                            EmailAddress = u.EmailAddress,
                            CourseStartTime = c.CourseStartTime.Value,
                            CourseBeforeTime = c.CheckInStartTime
                        };

                    var courseStartList = courseStart.ToList();
                    return new Result<List<CourseStartNotification>>(courseStartList);
                }
            }
            catch (Exception ex)
            {
                return new Result<List<CourseStartNotification>>(HttpStatusCode.InternalServerError);
            }

        }
    }
}
