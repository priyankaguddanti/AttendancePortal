using System;
using System.Web.Mvc;
using System.Web.Security;
using AttendancePortal.Dal;
using AttendancePortal.Models;

namespace AttendancePortal.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var dalHelper = new DalHelper();
            var userResult = dalHelper.GetUserByUserName(model.UserName);

            if(userResult.HasError)
                throw new InvalidOperationException(userResult.Message);
            
            FormsAuthentication.SetAuthCookie(model.UserName,false);

            return RedirectToAction("Index", "Home");
        }
    }
}