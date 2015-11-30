using EventZone.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventZone.Models;
using System.Threading.Tasks;

namespace EventZone.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            if (Request.Cookies["Admin_userName"] != null && Request.Cookies["Admin_password"] != null)
            {
                string userName = Request.Cookies["Admin_userName"].Value;
                string password = Request.Cookies["Admin_password"].Value;
                var admin = AdminDataHelpers.Instance.FindAdmin(userName, password);
                if (admin != null)
                {
                    if (admin.AccountStatus != EventZoneConstants.LockedUser)
                    {
                        UserHelpers.SetCurrentAdmin(Session, admin);
                    }
                }
            }
            return View();
        }
        public ActionResult SignIn()
        {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin != null)
            {
                TempData["errorTittle"] = "Bad request";
                TempData["errorMessage"] = "You are already signed in the system";
                return RedirectToAction("Index", "Admin");
            }
            TempData["errorTitle"] = null;
            TempData["errorMessage"] = null;
            return PartialView();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SignInPost(SignInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["errorTitle"] = "Invalid Input";
                TempData["errorMessage"] = "Invalid Input";
            }
            if (UserDatabaseHelper.Instance.isLookedUser(model.UserName))
            {
                ModelState.AddModelError("", "Your account is locked! Please contact with our support");
            }
            var admin = AdminDataHelpers.Instance.FindAdmin(model.UserName, model.Password);
            if (admin!=null&&(admin.UserRoles==EventZoneConstants.Admin||admin.UserRoles==EventZoneConstants.RootAdmin))
            {
                if (model.Remember)
                {
                    HttpCookie userName = new HttpCookie("Admin_userName");
                    userName.Expires = DateTime.Now.AddDays(7);
                    userName.Value = model.UserName;
                    Response.Cookies.Add(userName);

                    HttpCookie password = new HttpCookie("Admin_password");
                    password.Expires = DateTime.Now.AddDays(7);
                    password.Value = model.Password;
                    Response.Cookies.Add(password);
                }
                UserHelpers.SetCurrentAdmin(Session, admin);
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                ModelState.AddModelError("", "UserName or password is invalid.");
            }

            return RedirectToAction("Index", "Admin");

            // If we got this far, something failed, redisplay form
            
            
        }

        public ActionResult SignOut()
        {

            if (Request.Cookies["Admin_userName"] != null)
            {
                HttpCookie userName = new HttpCookie("Admin_userName");
                userName.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(userName);
            }
            //remove cookie password
            if (Request.Cookies["Admin_password"] != null)
            {
                HttpCookie password = new HttpCookie("Admin_password");
                password.Expires = DateTime.Now.AddDays(-1);
                Request.Cookies.Add(password);
            }
            UserHelpers.SetCurrentAdmin(Session, null);
            TempData["errorTitle"] = null;
            TempData["errorMessage"] = null;
            return RedirectToAction("Index", "Admin");
        }
        public ActionResult CheckCookie() {
            if (Request.Cookies["Admin_userName"] != null && Request.Cookies["Admin_password"] != null)
            {
                string userName = Request.Cookies["Admin_userName"].Value;
                string password = Request.Cookies["Admin_password"].Value;
                var admin = AdminDataHelpers.Instance.FindAdmin(userName,password);
                if(admin!=null){
                if (admin.AccountStatus==EventZoneConstants.LockedUser)
                {
                    return Json(new
                    {
                        success = 0,
                        message = "Your account have been locked! Please contact with admin to active it!"
                    });
                }
                UserHelpers.SetCurrentAdmin(Session, admin);
                return Json(new
                {
                    success = 1,
                    message = ""
                });
                }
                else
                {
                    return Json(new
                    {
                        success = 0,
                        message = "Your account have been changed password! Please try to sign in with a new password!"
                    });
                }
            }
            return Json(new
            {
                success = 0,
                message = "Cookie is empty!"
            });
        }
        public ActionResult ManageEvent()
        {
            if (Request.Cookies["Admin_userName"] != null && Request.Cookies["Admin_password"] != null)
            {
                string userName = Request.Cookies["Admin_userName"].Value;
                string password = Request.Cookies["Admin_password"].Value;
                var admin = AdminDataHelpers.Instance.FindAdmin(userName, password);
                if (admin != null)
                {
                    if (admin.AccountStatus != EventZoneConstants.LockedUser)
                    {
                        UserHelpers.SetCurrentAdmin(Session, admin);
                    }
                }
            }
            List<Event> listEvent = EventDatabaseHelper.Instance.GetAllEvent();
            return View(listEvent);
        }
        public JsonResult LockEvent(long eventID, string reason) {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require Signin",
                    message = "You are not signed in..."
                });
            }
            else {
                if (admin.AccountStatus != EventZoneConstants.LockedUser)
                {

                    if (AdminDataHelpers.Instance.LockEvent(admin.UserID, eventID, reason)) {
                        return Json(new
                        {
                            state = 1,
                            error = "",
                            message = ""
                        });
                    }
                }
            }
            return Json(new
            {
                state = 0,
                error = "Error",
                message = "Ops... Somthing went wrong! Please try again!",
            });
        }
        public JsonResult UnlockEvent(long eventID)
        {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require Signin",
                    message = "You are not signed in..."
                });
            }
            else
            {
                if (admin.AccountStatus != EventZoneConstants.LockedUser)
                {

                    if (AdminDataHelpers.Instance.UnlockEvent(admin.UserID, eventID))
                    {
                        return Json(new
                        {
                            state = 1,
                            error = "",
                            message = ""
                        });
                    }
                }
            }
            return Json(new
            {
                state = 0,
                error = "Error",
                message = "Ops... Somthing went wrong! Please try again!",
            });
        }
        public ActionResult ManageUser()
        {
            if (Request.Cookies["Admin_userName"] != null && Request.Cookies["Admin_password"] != null)
            {
                string userName = Request.Cookies["Admin_userName"].Value;
                string password = Request.Cookies["Admin_password"].Value;
                var admin = AdminDataHelpers.Instance.FindAdmin(userName, password);
                if (admin != null)
                {
                    if (admin.AccountStatus != EventZoneConstants.LockedUser)
                    {
                        UserHelpers.SetCurrentAdmin(Session, admin);
                    }
                }
            }
            List<User> listUser = UserDatabaseHelper.Instance.GetAllUser();
            return View(listUser);
        }
        public JsonResult LockUser(long userID)
        {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require Signin",
                    message = "You are not signed in..."
                });
            }
            else
            {
                if (admin.AccountStatus != EventZoneConstants.LockedUser)
                {
                    if (AdminDataHelpers.Instance.LockUser(admin.UserID, userID))
                    {
                        return Json(new
                        {
                            state = 1,
                            error = "",
                            message = ""
                        });
                    }
                }
            }
            return Json(new
            {
                state = 0,
                error = "Error",
                message = "Ops... Somthing went wrong! Please try again!",
            });
        }
        public JsonResult UnlockUser(long userID)
        {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require Signin",
                    message = "You are not signed in..."
                });
            }
            else
            {
                if (admin.AccountStatus != EventZoneConstants.LockedUser)
                {
                    if (AdminDataHelpers.Instance.UnlockUser(admin.UserID, userID))
                    {
                        return Json(new
                        {
                            state = 1,
                            error = "",
                            message = ""
                        });
                    }
                }
            }
            return Json(new
            {
                state = 0,
                error = "Error",
                message = "Ops... Somthing went wrong! Please try again!",
            });
        }
        public JsonResult ChangeUserEmail(long userID, string newEmail)
        {

            if (ModelState.IsValid)
            {
                User admin = UserHelpers.GetCurrentAdmin(Session);
                if (admin == null)
                {
                    return Json(new
                    {
                        state = 0,
                        message = "You are not signed in..."
                    });
                }
                if (admin.AccountStatus != EventZoneConstants.LockedUser)
                {
                    if (UserDatabaseHelper.Instance.GetUserByEmail(newEmail) != null)
                    {
                        return Json(new
                        {
                            state = 0,
                            error = "Email is exists",
                            message="This email already used in system! Please choose another!"
                        });
                    }
                    if (AdminDataHelpers.Instance.ChangeUserEmail(admin.UserID, userID, newEmail))
                    {
                        return Json(new
                        {
                            state = 1,
                            userID = userID,
                            newEmail = newEmail
                        });
                    }
                }
                return Json(new
                {
                    state = 0,
                    error= "Error",
                    message = "somthing wrong! Please try again..."
                });
            }
            else {
                return Json(new
                {
                    state=0,
                    erorr= " Wrong format",
                    message="Wrong email format! Please try again..."
                });
            }
        }
        public ActionResult SetMod(long userID) {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error= "Require signin!",
                    message = "You are not signed in..."
                });
            }
            else if (admin.AccountStatus == EventZoneConstants.LockedUser) {
                return Json(new
                {
                    state = 0,
                    error="Locked account",
                    message = "Your account is locked. You cant use this feature!"
                });
            }
            if (admin.AccountStatus != EventZoneConstants.LockedUser)
            {
                User user = UserDatabaseHelper.Instance.GetUserByID(userID);
                if (user != null)
                {
                    if (AdminDataHelpers.Instance.SetMod(admin.UserID, user.UserID))
                    {
                        return Json(new
                        {
                            state = 1,
                            userID = userID
                        });
                    }
                }
            }
            return Json(new
            {
                state = 0,
                error = "Erorr",
                message = "Something wrong! Please try again!"
            });
        }
        public ActionResult UnSetMod(long userID)
        {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require signin!",
                    message = "You are not signed in..."
                });
            }
            else if (admin.AccountStatus == EventZoneConstants.LockedUser)
            {
                return Json(new
                {
                    state = 0,
                    error = "Locked account",
                    message = "Your account is locked. You cant use this feature!"
                });
            }
            if (admin.AccountStatus != EventZoneConstants.LockedUser)
            {
                User user = UserDatabaseHelper.Instance.GetUserByID(userID);
                if (user != null)
                {
                    if (AdminDataHelpers.Instance.UnSetMod(admin.UserID, user.UserID))
                    {
                        return Json(new
                        {
                            state = 1,
                            userID = userID
                        });
                    }
                }
            }
            return Json(new
            {
                state = 0,
                error = "Erorr",
                message = "Something wrong! Please try again!"
            });
        }
        public ActionResult SetAdmin(long userID)
        {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require signin!",
                    message = "You are not signed in..."
                });
            }
            else if (admin.AccountStatus == EventZoneConstants.LockedUser)
            {
                return Json(new
                {
                    state = 0,
                    error = "Locked account",
                    message = "Your account is locked. You cant use this feature!"
                });
            }
            else if (admin.UserRoles != EventZoneConstants.RootAdmin) {
                return Json(new
                {
                    state = 0,
                    error = "Permission denied",
                    message = "Only root admin can use this feature!"
                });
            }
            if (admin.AccountStatus != EventZoneConstants.LockedUser)
            {
                User user = UserDatabaseHelper.Instance.GetUserByID(userID);
                if (user != null)
                {
                    if (AdminDataHelpers.Instance.SetAdmin(admin.UserID, user.UserID))
                    {
                        return Json(new
                        {
                            state = 1,
                            userID = userID
                        });
                    }
                }
            }
            return Json(new
            {
                state = 0,
                error = "Erorr",
                message = "Something wrong! Please try again!"
            });
        }
        public ActionResult UnSetAdmin(long userID)
        {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require signin!",
                    message = "You are not signed in..."
                });
            }
            else if (admin.AccountStatus == EventZoneConstants.LockedUser)
            {
                return Json(new
                {
                    state = 0,
                    error = "Locked account",
                    message = "Your account is locked. You cant use this feature!"
                });
            }
            else if (admin.UserRoles != EventZoneConstants.RootAdmin)
            {
                return Json(new
                {
                    state = 0,
                    error = "Permission denied",
                    message = "Only root admin can use this feature!"
                });
            }
            if (admin.AccountStatus != EventZoneConstants.LockedUser)
            {
                User user = UserDatabaseHelper.Instance.GetUserByID(userID);
                if (user != null)
                {
                    if (AdminDataHelpers.Instance.UnSetAdmin(admin.UserID, user.UserID))
                    {
                        return Json(new
                        {
                            state = 1,
                            userID = userID
                        });
                    }
                }
            }
            return Json(new
            {
                state = 0,
                error = "Erorr",
                message = "Something wrong! Please try again!"
            });
        }
        public ActionResult ManageReport()
        {
            if (Request.Cookies["Admin_userName"] != null && Request.Cookies["Admin_password"] != null)
            {
                string userName = Request.Cookies["Admin_userName"].Value;
                string password = Request.Cookies["Admin_password"].Value;
                var admin = AdminDataHelpers.Instance.FindAdmin(userName, password);
                if (admin != null)
                {
                    if (admin.AccountStatus != EventZoneConstants.LockedUser)
                    {
                        UserHelpers.SetCurrentAdmin(Session, admin);
                    }
                }
            }
            return View();
        }
        public ActionResult ViewDetailReport(long eventID=-1)
        {
            List<Report> listReport = new List<Report>();
            listReport = EventDatabaseHelper.Instance.GetEventReport(eventID);
            return PartialView("ViewDetailReport", listReport);
        }
        public ActionResult ApproveReport(long reportID) {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require signin!",
                    message = "You are not signed in..."
                });
            }
            else if (admin.AccountStatus == EventZoneConstants.LockedUser)
            {
                return Json(new
                {
                    state = 0,
                    error = "Locked account",
                    message = "Your account is locked. You cant use this feature!"
                });
            }
            else if (admin.UserRoles != EventZoneConstants.RootAdmin&&admin.UserRoles!=EventZoneConstants.Admin)
            {
                return Json(new
                {
                    state = 0,
                    error = "Permission denied",
                    message = "This feature not avaiable for you!"
                });
            }
            if (admin.AccountStatus != EventZoneConstants.LockedUser)
            {
                Report newReport=AdminDataHelpers.Instance.ApproveReport(admin.UserID, reportID);
               
                    if (newReport!=null)
                    {
                     
                        return Json(new
                        {
                            state = 1,
                            handleDate= newReport.HandleDate.ToString(),
                            handleBy=admin.UserName
                        });
                    }
                
            }
            return Json(new
            {
                state = 0,
                error = "Erorr",
                message = "Something wrong! Please try again!"
            });
        }
        public ActionResult RejectReport(long reportID)
        {
            User admin = UserHelpers.GetCurrentAdmin(Session);
            if (admin == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require signin!",
                    message = "You are not signed in..."
                });
            }
            else if (admin.AccountStatus == EventZoneConstants.LockedUser)
            {
                return Json(new
                {
                    state = 0,
                    error = "Locked account",
                    message = "Your account is locked. You cant use this feature!"
                });
            }
            else if (admin.UserRoles != EventZoneConstants.RootAdmin && admin.UserRoles != EventZoneConstants.Admin)
            {
                return Json(new
                {
                    state = 0,
                    error = "Permission denied",
                    message = "This feature not avaiable for you!"
                });
            }
            if (admin.AccountStatus != EventZoneConstants.LockedUser)
            {
                Report newReport = AdminDataHelpers.Instance.RejectReport(admin.UserID, reportID);

                if (newReport != null)
                {

                    return Json(new
                    {
                        state = 1,
                        handleDate = newReport.HandleDate.ToString(),
                        handleBy = admin.UserName
                    });
                }

            }
            return Json(new
            {
                state = 0,
                error = "Erorr",
                message = "Something wrong! Please try again!"
            });
        }

        public ActionResult Statistic()
        {
            if (Request.Cookies["Admin_userName"] != null && Request.Cookies["Admin_password"] != null)
            {
                string userName = Request.Cookies["Admin_userName"].Value;
                string password = Request.Cookies["Admin_password"].Value;
                var admin = AdminDataHelpers.Instance.FindAdmin(userName, password);
                if (admin != null)
                {
                    if (admin.AccountStatus != EventZoneConstants.LockedUser)
                    {
                        UserHelpers.SetCurrentAdmin(Session, admin);
                    }
                }
            }
            StatisticViewModel model = new StatisticViewModel();
            model.EventCountStatistic = StatisticDataHelpers.Instance.GetEventCount();
            model.EventCreatedEachMonth = StatisticDataHelpers.Instance.GetEventCreatedEachMonth();
            model.TopTenEvents = StatisticDataHelpers.Instance.GetTopTenEvent();
            model.TopTenLocations = StatisticDataHelpers.Instance.GetTopLocation();
            model.TopTenUser = StatisticDataHelpers.Instance.GetTopTenCreatedUser();
            model.GenderRate = StatisticDataHelpers.Instance.GenderRate();
            model.GroupbyAge = StatisticDataHelpers.Instance.GetGroupAgeReport();
            model.ReportByStatus = StatisticDataHelpers.Instance.GetReportByStatus();
            model.ReportByType = StatisticDataHelpers.Instance.GetReportByType();
            model.AppealByStatus = StatisticDataHelpers.Instance.GetAppealByStatus();
            model.EventByStatus = StatisticDataHelpers.Instance.GetEventByStatus();
            return View(model);
        }
    }
}