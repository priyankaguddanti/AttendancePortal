using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

        public ActionResult Profile()
        {
            return View();
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
            return View();
        }

        public ActionResult Reports()
        {
            return View();
        }

        public PartialViewResult GetCourseDetails(int courseId)
        {
            var stopwatch = DateTime.Now;
            Debug.WriteLine(stopwatch);
            Debug.WriteLine($"started - {(DateTime.Now -stopwatch).Seconds}");

            var dalHelper = new DalHelper();
            var courseDetails = dalHelper.GetCourseDetails(courseId);

            Debug.WriteLine($"after database call - {(DateTime.Now - stopwatch).Seconds}");

            var courseDetailsViewModel = new CoursesDetailsViewModel
            {
                CourseTitle = courseDetails.Course.CourseName,
                StartTime = courseDetails.Course.CourseStartTime.ToString(),
                EndTime = courseDetails.Course.CourseEndTime.ToString(),
                BeforeCheckIn = courseDetails.Course.CheckInStartTime,
                AfterCheckIn = courseDetails.Course.CheckInEndTime
            };

            Debug.WriteLine($"after modal initialize - {(DateTime.Now - stopwatch).Seconds}");

            var students = new List<Student>();

            courseDetails.TotalUsers.ForEach(c => students.Add(new Student
            {
                Id = c.Id,
                Name = $"{c.FirstName} {c.LastName}",
                Selected = false
            }));

            Debug.WriteLine($"after total users call - {(DateTime.Now - stopwatch).Seconds}");

            foreach (var user in courseDetails.CourseUsers)
            {
                var firstOrDefault = students.FirstOrDefault(c => c.Id == user.Id);
                if (firstOrDefault != null)
                    firstOrDefault.Selected = true;
            }

            Debug.WriteLine($"after student selection call - {(DateTime.Now - stopwatch).Seconds}");

            courseDetailsViewModel.Students = students;

            Debug.WriteLine(stopwatch);

            return PartialView("CourseDetails", courseDetailsViewModel);
        }

        [HttpPost]
        public JsonResult SaveCourseDetails(CoursesDetailsViewModel model)
        {
            return Json(new {}, JsonRequestBehavior.AllowGet);
        }
    }
}