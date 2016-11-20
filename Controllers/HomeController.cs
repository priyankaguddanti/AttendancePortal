using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using AttendancePortal.Code;
using AttendancePortal.Dal;
using AttendancePortal.Models;

namespace AttendancePortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
           return View();
        }

        public new ActionResult Profile()
        {
            var dalHelper = new DalHelper();
            var userResult = dalHelper.GetUserByUserName(HttpContext.User.Identity.Name);

            if(userResult.HasError)
                throw new InvalidOperationException(userResult.Message);

            return View(userResult.Value);
        }

        public ActionResult Courses()
        {
            var dalHelper = new DalHelper();
            var result = dalHelper.GetProfessorCoursesByUser(HttpContext.User.Identity.Name);
            var model = new CoursesViewModel
            {
                Courses = result.Value
            };
            return View(model);
        }

        public ActionResult Disputes()
        {
            var dalHelper = new DalHelper();
            var courseResult = dalHelper.GetProfessorCoursesByUser(HttpContext.User.Identity.Name);
            if (courseResult.HasError)
                throw new InvalidOperationException(courseResult.Message);

            if (courseResult.Value == null || !courseResult.Value.Any())
            {
                return View(new DisputesViewModel());
            }

            var disputeResult = dalHelper.GetDisputesByCourseId(courseResult.Value.FirstOrDefault().Id);
            if(disputeResult.HasError)
                throw new InvalidOperationException(disputeResult.Message);

            var disputes = disputeResult.Value;
            var model = new DisputesViewModel
            {
                AvailableCourses = courseResult.Value,
                AvailableDisputes = disputes
            };

            return View(model);
        }

        public ActionResult Reports()
        {
            var dalHelper = new DalHelper();

            var courseResult = dalHelper.GetProfessorCoursesByUser(HttpContext.User.Identity.Name);
            if (courseResult.HasError)
                throw new InvalidOperationException(courseResult.Message);

            if (courseResult.Value == null || !courseResult.Value.Any())
            {
                return View(new ReportsViewModel());
            }
            
            var studentReportsResult = dalHelper.GetStudentReportByCourse(courseResult.Value.FirstOrDefault().Id);
            if(studentReportsResult.HasError)
                throw new InvalidOperationException(studentReportsResult.Message);

            var reportModel = new ReportsViewModel
            {
                AvailableCourses = courseResult.Value,
                StudentsReports = studentReportsResult.Value
            };
            return View(reportModel);
        }

        public PartialViewResult GetReportsByCourse(int courseId)
        {
            var dalHelper = new DalHelper();
            var studentReportsResult = dalHelper.GetStudentReportByCourse(courseId);
            if (studentReportsResult.HasError)
                throw new InvalidOperationException(studentReportsResult.Message);

            return PartialView("ReportsByCourse", new ReportsViewModel
            {
                StudentsReports = studentReportsResult.Value
            });
        }

        public PartialViewResult GetCourseDetails(int courseId)
        {
            var dalHelper = new DalHelper();
            var courseDetails = dalHelper.GetCourseDetails(courseId);
            var courseDetailsViewModel = new CoursesDetailsViewModel
            {
                CourseTitle = courseDetails.Course.CourseName,
                StartTime = courseDetails.Course.CourseStartTime.ToString(),
                EndTime = courseDetails.Course.CourseEndTime.ToString(),
                BeforeCheckIn = courseDetails.Course.CheckInStartTime,
                AfterCheckIn = courseDetails.Course.CheckInEndTime
            };

            var students = new List<Student>();
            courseDetails.TotalUsers.ForEach(c => students.Add(new Student
            {
                Id = c.Id,
                Name = $"{c.FirstName} {c.LastName}",
                Selected = false
            }));

            foreach (var user in courseDetails.CourseUsers)
            {
                var firstOrDefault = students.FirstOrDefault(c => c.Id == user.Id);
                if (firstOrDefault != null)
                    firstOrDefault.Selected = true;
            }

            courseDetailsViewModel.Students = students;

            return PartialView("CourseDetails", courseDetailsViewModel);
        }

        public PartialViewResult GetDisputeCourses(int courseId)
        {
            var dalHelper = new DalHelper();
            var disputeResult = dalHelper.GetDisputesByCourseId(courseId);
            
            if (disputeResult.HasError)
                throw new InvalidOperationException(disputeResult.Message);

            var model = new DisputesViewModel {AvailableDisputes = disputeResult.Value};
            
            return PartialView("DisputeCourse", model);
        }

        [HttpPost]
        public JsonResult SaveProfile(User user)
        {
            var dalHelper = new DalHelper();
            var result = dalHelper.SaveUser(user);
            return result.HasError
                ? Json(new { Message = "Save Failed" }, JsonRequestBehavior.AllowGet)
                : Json(new { Message = "Success" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveCourseDetails(CoursesDetailsViewModel model)
        {
            var dalHelper = new DalHelper();
            var result = dalHelper.UpdateCourseDetals(model);

            return result.HasError
                ? Json(new { Message = "Save Failed" }, JsonRequestBehavior.AllowGet)
                : Json(new { Message = "Saved" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveDisputeResponse(int courseAttendanceId, bool disputeAccepted)
        {
            var dalHelper = new DalHelper();
            var result = dalHelper.SaveDisputeResponse(courseAttendanceId, disputeAccepted);

            return result.HasError
                ? Json(new { Message = "Failed" }, JsonRequestBehavior.AllowGet)
                : Json(new { Message = "Success" }, JsonRequestBehavior.AllowGet);
        }
    }
}