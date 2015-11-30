﻿using EventZone.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventZone.Models;
using System.Net;
using System.Data.Entity;
using System.Web.Helpers;
using System.IO;
using Amazon.S3;

namespace EventZone.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        public ActionResult ManageProfile()
        {
            User user = UserHelpers.GetCurrentUser(Session);
            if (user == null)
            {
                if (Request.Cookies["userName"] != null && Request.Cookies["password"] != null)
                {
                    string userName = Request.Cookies["userName"].Value;
                    string password = Request.Cookies["password"].Value;
                    if (UserDatabaseHelper.Instance.ValidateUser(userName, password))
                    {
                        user = UserDatabaseHelper.Instance.GetUserByUserName(userName);
                        if (UserDatabaseHelper.Instance.isLookedUser(user.UserName))
                        {
                            TempData["errorTitle"] = "Locked User";
                            TempData["errorMessage"] = "Your account is locked! Please contact with our support";
                        }
                        else {
                            UserHelpers.SetCurrentUser(Session, user);
                        }
                        
                    }
                }
            }
            if (UserHelpers.GetCurrentUser(Session) == null)
            {
                TempData["errorTitle"] = "Require Signin";
                TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                return View();
            }
            else if (TempData["errorTitle"] != null)
            {
                TempData["errorTitle"] = TempData["errorTitle"];
                TempData["errorMessage"] = TempData["errorMessage"];
            }
            else {
                TempData["errorTitle"] = null;
                TempData["errorMessage"] = null;
            }
            return View();
        }
        public ActionResult Index(long userID=-1) {
            User user = UserHelpers.GetCurrentUser(Session);
            if (userID == -1)
            {
                if (user == null)
                {
                    if (Request.Cookies["userName"] != null && Request.Cookies["password"] != null)
                    {
                        string userName = Request.Cookies["userName"].Value;
                        string password = Request.Cookies["password"].Value;
                        if (UserDatabaseHelper.Instance.ValidateUser(userName, password))
                        {
                            user = UserDatabaseHelper.Instance.GetUserByUserName(userName);
                            if (UserDatabaseHelper.Instance.isLookedUser(user.UserName))
                            {
                                TempData["errorTitle"] = "Locked User";
                                TempData["errorMessage"] = "Your account is locked! Please contact with our support";
                            }
                            else
                            {
                                UserHelpers.SetCurrentUser(Session, user);
                            }

                        }
                    }
                }
                if (user == null)
                {
                    TempData["errorTitle"] = "Require Signin";
                    TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                    return RedirectToAction("Index", "Home");
                }
            }
            else {
                user = UserDatabaseHelper.Instance.GetUserByID(userID);
                if (user == null)
                {
                    TempData["errorTitle"] = "Not Avaiable";
                    TempData["errorMessage"] = "Ops.. This user does not exists in the system!";
                    return RedirectToAction("Index", "Home");
                }
                else if (user.AccountStatus == EventZoneConstants.Lock) {
                    TempData["errorTitle"] = "Locked User";
                    TempData["errorMessage"] = "Ops.. This user has been locked!";
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(user);
       }
        

        /// <summary>
        /// view detail user info
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UserInfo(long? id = 0)
        {
            User userSession = UserHelpers.GetCurrentUser(Session);
            User user;

            if (userSession.UserID == id || id == 0)
            {
                user = userSession;
            }
            else
            {
                user = UserDatabaseHelper.Instance.GetUserByID(id);
                if (user == null)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewData["UserSession"] = user;
            TempData["ManageProfileTask"] = "UserInfo";
            return RedirectToAction("ManageProfile");
        }

        /// <summary>
        /// manage event
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageEvent()
        {
            if (UserHelpers.GetCurrentUser(Session) == null)
            {
                return RedirectToAction("RequireSignin", "Account");
            }
            User currentUser = UserHelpers.GetCurrentUser(Session);
            List<Event> myEvent = EventDatabaseHelper.Instance.GetEventsByUser(currentUser.UserID);
            if (myEvent == null)
            {
                return View("SuggestCreateEvent");
            }

            List<ViewThumbEventModel> listThumbEvent = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(myEvent);

            if (ViewData["ManageEventTask"] == null)
            {
                ViewData["ManageEventTask"] = "MyEvent";
            }
            ViewData["ListThumbEvent"] = listThumbEvent;//chứa thông tin cần thiết để show 1 event
            ViewData["MyEvent"] = myEvent;//event cua người dùng
            return View();

        }
        /// <summary>
        /// view all my event thumb
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MyEvent(long? id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("ManageEvent");
            }
            User user = UserDatabaseHelper.Instance.GetUserByID(id);
            List<Event> myEvent = EventDatabaseHelper.Instance.GetEventsByUser(id);
            if (myEvent == null)
            {
                return RedirectToAction("ManageEvent");
            }
            List<ViewThumbEventModel> listThumbEvent = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(myEvent);
            ViewData["ListThumbEvent"] = listThumbEvent;
            ViewData["MyEvent"] = myEvent;
            return RedirectToAction("ManageEvent");
        }

        public JsonResult Like(long eventId)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Boolean success = false;
            int state = EventZoneConstants.NotRate;

            if (user != null)
            {
                LikeDislike findlike = UserDatabaseHelper.Instance.FindLike(user.UserID, eventId);
                if (findlike != null)
                {
                    state = findlike.Type;
                }
                success = UserDatabaseHelper.Instance.InsertLike(user.UserID, eventId);
            }

            return Json(new
            {
                success = success,
                state = state
            });
        }

        public JsonResult DisLike(long eventId)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Boolean success = false;
            int state = EventZoneConstants.NotRate;
            if (user != null)
            {
                LikeDislike findlike = UserDatabaseHelper.Instance.FindLike(user.UserID, eventId);
                if (findlike != null)
                {
                    state = findlike.Type;
                }
                success = UserDatabaseHelper.Instance.InsertDislike(user.UserID, eventId);
            }
            return Json(new
            {
                success = success,
                state = state
            });
        }
        public JsonResult FollowEvent(long eventId)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Boolean success = false;
            int followState = 0;// trang thai khong follow
            if (user != null)
            {
                success = UserDatabaseHelper.Instance.FollowEvent(user.UserID, eventId);
                if (UserDatabaseHelper.Instance.IsFollowingEvent(user.UserID, eventId))
                {
                    //trang thai tu unfollow sang follow
                    followState = 1;
                }
            }
            return Json(new
            {
                success = success,
                state = followState
            }

           );
        }

        public JsonResult FollowCategory(long categoryId)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Boolean success = false;
            int followState = 0;
            if (user != null)
            {
                success = UserDatabaseHelper.Instance.FollowCategory(user.UserID, categoryId);
                if (UserDatabaseHelper.Instance.IsFollowingCategory(user.UserID, categoryId))
                {
                    followState = 1;
                }
            }
            return Json(new
            {
                success = success,
                state = followState
            }

           );
        }

        public ActionResult UploadAvatar(HttpPostedFileBase file)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            if (user == null)
            {
                TempData["errorTitle"]="Require Signin";
                TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Image photo = new Image();
                try
                {
                    if (file != null)
                    {
                        string[] whiteListedExt = { ".jpg", ".gif", ".png", ".tiff" };
                        Stream stream = file.InputStream;
                        string extension = Path.GetExtension(file.FileName);
                        if (whiteListedExt.Contains(extension))
                        {
                            string pic = Guid.NewGuid() + user.UserID.ToString() + extension;
                            using (AmazonS3Client s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
                                EventZoneUtility.FileUploadToS3("eventzone", pic, stream, true, s3Client);
                            Image image = new Image();
                            image.ImageLink = "https://s3-us-west-2.amazonaws.com/eventzone/" + pic;
                            image.UserID = user.UserID;
                            image.UploadDate = DateTime.Today;
                            if (UserDatabaseHelper.Instance.UpdateAvatar(user, image))
                            {
                                TempData["errorTitle"] = null;
                                TempData["errorMessage"] = null;
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["errorTitle"] = "Database Error";
                                TempData["errorMessage"] = "Ops... Some error is ocurred while we save to database! Please try again later!";
                                return RedirectToAction("Index");
                            }

                        }
                        else
                        {
                            TempData["errorTitle"] = "Database Error";
                            TempData["errorMessage"] = "Ops... Some error is ocurred while we save to database! Please try again later!";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        TempData["errorTitle"] = "Not select file";
                        TempData["errorMessage"] = "It's look like you forgot select an image! Are you getting old?";
                        return RedirectToAction("Index");
                    }
                }
                catch
                {
                    TempData["errorTitle"] = "Unknow Error";
                    TempData["errorMessage"] = "Oops..Something wrong is happened! Please try again later...";
                    return RedirectToAction("Index");
                }
            }
        }

        public ActionResult ChangePassword()
        {
            return PartialView();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePasswordPost(ChangePasswordView chgpwd) {
            if (!ModelState.IsValid)
            return Json(new
            {
                state = 0,
                message = "Invalid model"
            });
            User user = UserHelpers.GetCurrentUser(Session);
            if (user == null)
            {
              
                return Json(new
                {
                    state = 0,
                    message = "require signin"
                });
            }
            else {
                if (!UserDatabaseHelper.Instance.ValidateUser(user.UserName, chgpwd.OldPassword))
                {
                   
                    return Json(new
                    {
                        state = 0,
                        message = "wrong old passsword"
                    });

                }
                else if (UserDatabaseHelper.Instance.ChangePassword(user, chgpwd.NewPassword))
                {
                    return Json(new
                    {
                        state = 1,
                        message = ""
                    });
                }
                else {
                    return Json(new
                    {
                        state = 0,
                        message = "something wrong!"
                    });
                }
            }
            
        }
        public ActionResult UpdateInfo() {
            User userSession = UserHelpers.GetCurrentUser(Session);

            if (userSession == null)
            {
                TempData["errorTitle"] = "Require Signin";
                TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                return RedirectToAction("Index", "Home");
            }
            EditUserModel editUserModel = new EditUserModel();
            editUserModel.UserID = userSession.UserID;
            editUserModel.UserDOB = userSession.UserDOB;
            editUserModel.UserFirstName = userSession.UserFirstName;
            editUserModel.UserLastName = userSession.UserLastName;
            editUserModel.Phone = userSession.Phone;
            editUserModel.Place = userSession.Place;
            editUserModel.IDCard = userSession.IDCard;
            return PartialView(editUserModel);
        
        }
        public ActionResult UpdateInfoPost(EditUserModel editUserModel)
        {
            try
            {
                // TODO: Add update logic here
                if (ModelState.IsValid)
                {
                    User user = UserHelpers.GetCurrentUser(Session);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "You are signed out!Please signin to do this!");
                        return RedirectToAction("EditProfile");
                    }
                    user.UserFirstName = editUserModel.UserFirstName;
                    user.UserLastName = editUserModel.UserLastName;
                    user.UserDOB = editUserModel.UserDOB;
                    user.IDCard = editUserModel.IDCard;
                    user.Gender = editUserModel.Gender;
                    user.Phone = editUserModel.Phone;
                    user.Place = editUserModel.Place;

                    if (UserDatabaseHelper.Instance.UpdateUser(user))
                    {
                        UserHelpers.SetCurrentUser(Session, user);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Something went wrong! Please try again!");
                        return RedirectToAction("EditProfile");
                    }
                }
                ModelState.AddModelError("", "Something went wrong! Please try again!");
                return RedirectToAction("EditProfile");
            }
            catch (Exception ex)
            {
                TempData["ManageProfileTask"] = "EditProfile";
                ModelState.AddModelError("", "Something went wrong! Please try again!");
                return RedirectToAction("EditProfile");
            }
        }
        public ActionResult ManageFollow(long userID = -1)
        {
            User user= UserHelpers.GetCurrentUser(Session);
            if (userID == -1) {
                if (user == null)
                {
                    TempData["errorTitle"] = "Require Signin";
                    TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                    return PartialView("_ManageFollow");
                }
                else userID = user.UserID;
            }
            List<ViewThumbEventModel> listviewThumb = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(UserDatabaseHelper.Instance.GetFollowingEvent(userID));
            List<User> listFollowingUser = UserDatabaseHelper.Instance.GetListFollowingOfUser(userID);
            List<User> listFollower = UserDatabaseHelper.Instance.GetListFollowerOfUser(userID);
            TempData["ListEventThumb"] = listviewThumb;
            TempData["ListFollowing"] = listFollowingUser;
            TempData["ListFollower"] = listFollower;
            return PartialView("_ManageFollow");
        }
        public ActionResult ReportEvent(int typeReport=-1, string reportContent="", long eventId=-1 )
        {
            if (eventId == -1 || typeReport == -1)
            {
                return Json(new
                {
                    state = 0,
                    error = "Error",
                    message = "Somthing wrong..."
                });
            }

            User user = UserHelpers.GetCurrentUser(Session);
            if (user == null)
            {
                return Json(new
                {
                    state = 0,
                    error = "Require Signin",
                    message = "Ops.. It's look like you are current is not signed in system! Please sign in first!",
                });
            }
            else if (user.UserID == EventDatabaseHelper.Instance.GetAuthorEvent(eventId).UserID)
            {
                return Json(new
                {
                    state = 0,
                    error = "Error",
                    message = "You cant report your event!",
                });
            }
            else {

                Report newReport = UserDatabaseHelper.Instance.ReportEvent(user.UserID, eventId, typeReport, reportContent);
                if (newReport != null)
                {
                    return Json(new
                    {
                        state = 1,
                        newReportType= newReport.ReportType,
                        newReportDate= newReport.ReportDate.ToString()
                    });
                }
                return Json(new
                {
                    state = 0,
                    error="Error",
                    message="Somthing wrong..."
                });
                
            }
        
        }
        public ActionResult Event(long userID=-1)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            if (userID == -1)
            {
                if (user == null)
                {
                    if (Request.Cookies["userName"] != null && Request.Cookies["password"] != null)
                    {
                        string userName = Request.Cookies["userName"].Value;
                        string password = Request.Cookies["password"].Value;
                        if (UserDatabaseHelper.Instance.ValidateUser(userName, password))
                        {
                            user = UserDatabaseHelper.Instance.GetUserByUserName(userName);
                            if (UserDatabaseHelper.Instance.isLookedUser(user.UserName))
                            {
                                TempData["errorTitle"] = "Locked User";
                                TempData["errorMessage"] = "Your account is locked! Please contact with our support";
                            }
                            else
                            {
                                UserHelpers.SetCurrentUser(Session, user);
                            }

                        }
                    }
                }
                if (user == null)
                {
                    TempData["errorTitle"] = "Require Signin";
                    TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                user = UserDatabaseHelper.Instance.GetUserByID(userID);
                if (user == null)
                {
                    TempData["errorTitle"] = "Not Avaiable";
                    TempData["errorMessage"] = "Ops.. This user does not exists in the system!";
                    return RedirectToAction("Index", "Home");
                }
                else if (user.AccountStatus == EventZoneConstants.Lock)
                {
                    TempData["errorTitle"] = "Locked User";
                    TempData["errorMessage"] = "Ops.. This user has been locked!";
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(user);
        }

    }
   
}
