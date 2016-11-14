using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script;
using AttendancePortal.Areas.AttendanceApi.Models;
using AttendancePortal.Code;
using AttendancePortal.Dal;

namespace AttendancePortal.Controllers
{
    [AllowAnonymous]
    public class AttendanceController : ApiController
    {
        private readonly DalHelper _dalHelper;

        public AttendanceController()
        {
            _dalHelper = new DalHelper();
        }

        [HttpPost]
        public HttpResponseMessage Login([FromBody] LoginModel model)
        {
            var result = _dalHelper.GetUserByUserName(model.UserName);
            if (result.HasError)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new Result(HttpStatusCode.InternalServerError, "Please enter a valid user name"));
            }

            if (!result.Value.UserName.Equals(model.UserName, StringComparison.CurrentCultureIgnoreCase))
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new Result(HttpStatusCode.InternalServerError, "UserName is invalid"));

            if (!result.Value.Password.Equals(model.Password, StringComparison.CurrentCultureIgnoreCase))
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new Result(HttpStatusCode.InternalServerError, "Password is invalid"));

            return Request.CreateResponse(HttpStatusCode.OK, new Result());
        }

        [HttpGet]
        public HttpResponseMessage GetCourses(string userName)
        {
            try
            {
                var result = _dalHelper.GetStudentCoursesByUser(userName);
                if (result.HasError)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }

                var courses = new List<Areas.AttendanceApi.Course>();
                if(result.Value == null || !result.Value.Any())
                    return Request.CreateResponse(HttpStatusCode.OK, new List<Areas.AttendanceApi.Course>(courses));

                foreach (var userCourse in result.Value)
                {
                    var checkInTime = string.Empty;
                    var isEligibleToCheckIn = false;
                    var alreadyCheckedIn = false;

                    if (userCourse.CourseStartTime.HasValue && userCourse.CourseEndTime.HasValue)
                    {
                        var courseCheckInStart = string.IsNullOrWhiteSpace(userCourse.CheckInStartTime)
                            ? 0
                            : Convert.ToInt32(userCourse.CheckInStartTime);
                        var courseStart = userCourse.CourseStartTime.Value.Add(new TimeSpan(0, -courseCheckInStart, 0));
                        var courseStartString = courseStart.ToString(@"hh\:mm");
                        var courseEnd = userCourse.CourseEndTime.Value;
                        var courseEndString = userCourse.CourseEndTime.Value.ToString(@"hh\:mm");

                        checkInTime = $"({courseStartString}-{courseEndString})";

                        var now = DateTime.Now.TimeOfDay;
                        if ((now > courseStart) && (now < courseEnd))
                        {
                            isEligibleToCheckIn = true;
                        }

                        if (!string.IsNullOrWhiteSpace(userCourse.Status))
                        {
                            alreadyCheckedIn = true;
                            isEligibleToCheckIn = false;
                        }
                    }
                    courses.Add(new Areas.AttendanceApi.Course
                    {
                        CourseTitle = userCourse.CourseName,
                        CheckInTime = checkInTime,
                        IsEligibleForCheckIn = isEligibleToCheckIn,
                        CourseId = userCourse.CourseId,
                        AlreadyCheckedIn = alreadyCheckedIn
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new List<Areas.AttendanceApi.Course>(courses));
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        private string GetStatus(Course course)
        {
            if (course.CourseStartTime.HasValue && course.CourseEndTime.HasValue)
            {
                var checkInStart = string.IsNullOrWhiteSpace(course.CheckInStartTime)
                    ? 0
                    : Convert.ToInt32(course.CheckInStartTime);
                var courseStart = course.CourseStartTime.Value.Add(new TimeSpan(0, -checkInStart, 0));
               
                var checkInEnd = string.IsNullOrWhiteSpace(course.CheckInEndTime)
                    ? 0
                    : Convert.ToInt32(course.CheckInEndTime);
                var courseEnd = course.CourseStartTime.Value.Add(new TimeSpan(0, checkInEnd, 0));
                var now = DateTime.Now.TimeOfDay;
                if ((now > courseStart) && (now < courseEnd))
                {
                    return "Present";
                }

                courseStart = course.CourseStartTime.Value;
                courseEnd = course.CourseEndTime.Value;
                if ((now > courseStart) && (now < courseEnd))
                {
                    return "Tardy";
                }
            }
            return "Absent"; 
        }

        [HttpPost]
        public HttpResponseMessage CheckInCourse([FromBody] CheckInCourseModel model)
        {
            var userResult = _dalHelper.GetUserByUserName(model.UserName);
            if (userResult.HasError)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new Result(HttpStatusCode.InternalServerError, "User name is invalid"));
            }

            var courseResult = _dalHelper.GetCourseById(model.CourseId);
            var status = GetStatus(courseResult.Value);
            var courseAttendance = new CourseAttendance()
            {
                CourseId = courseResult.Value.Id,
                UserId = userResult.Value.Id,
                Status = status,
                Created = DateTime.Now
            };

            var insertCourseResult = _dalHelper.InsertCourseAttendance(courseAttendance);
            if (insertCourseResult.HasError)
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new Result(HttpStatusCode.InternalServerError, "Unable to Check In"));

            return Request.CreateResponse(HttpStatusCode.OK, new Result());
        }
        
        [HttpPost]
        public HttpResponseMessage GetClassViewDetails([FromBody] ClassViewDetailsRequest request)
        {
            try
            {
                var result = _dalHelper.GetClassViewDetails(request);
                if (result.HasError)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
               
                return Request.CreateResponse(HttpStatusCode.OK, result.Value);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public HttpResponseMessage DisputeCourse([FromBody] DisputeCourseRequest request)
        {
            var result = _dalHelper.DisputeCourse(request);
            return result.HasError
                ? Request.CreateResponse(HttpStatusCode.InternalServerError, result)
                : Request.CreateResponse(HttpStatusCode.OK, new Result());
        }
    }
}