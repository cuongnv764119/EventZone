using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ASPSnippets.GoogleAPI;
using EventZone.Helpers;
using EventZone.Models;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Security.Cryptography;

namespace EventZone.Controllers
{
    public class AccountController : Controller
    {
        private readonly EventZoneEntities db = new EventZoneEntities();
       
        // GET: Account
        public ActionResult SignIn()
        {
          
                User user = UserHelpers.GetCurrentUser(Session);
                if (user != null)
                {
                    TempData["errorTittle"] = "Bad request";
                    TempData["errorMessage"] = "You are already signed in the system";
                    return RedirectToAction("Index", "Home");
                }
                TempData["errorTitle"] = null;
                TempData["errorMessage"] = null;
                return PartialView();
           
           
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public JsonResult SigninPost(SignInViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    state = 0,
                    message = "Invalid model"
                });

            if (UserDatabaseHelper.Instance.ValidateUser(model.UserName, model.Password))
            {
                if (UserDatabaseHelper.Instance.isLookedUser(model.UserName))
                {
                    UserHelpers.SetCurrentUser(Session, null);
                    ModelState.AddModelError("", "Your account has been locked! Please contact with our support");
                    return Json(new
                    {
                        state = 0,
                        message = "Your account has been locked! Please contact with our support"
                    });
                }
                if (model.Remember)
                {
                    HttpCookie userName = new HttpCookie("userName");
                    userName.Expires = DateTime.Now.AddDays(7);
                    userName.Value = model.UserName;
                    Response.Cookies.Add(userName);

                    HttpCookie password = new HttpCookie("password");
                    password.Expires = DateTime.Now.AddDays(7);
                    password.Value = model.Password;
                    Response.Cookies.Add(password);
                }
                else {
                    HttpCookie userName = new HttpCookie("userName");
                    userName.Expires = DateTime.Now.AddHours(1);
                    userName.Value = model.UserName;
                    Response.Cookies.Add(userName);

                    HttpCookie password = new HttpCookie("password");
                    password.Expires = DateTime.Now.AddHours(1);
                    password.Value = model.Password;
                    Response.Cookies.Add(password);
                }
                var user = UserDatabaseHelper.Instance.GetUserByUserName(model.UserName);
                UserHelpers.SetCurrentUser(Session, user);
            }
            else
            {
                ModelState.AddModelError("", "UserName or password is invalid.");
                return Json(new
                {
                    state = 0,
                    message = "Invalid account, password"
                });
            }

            return Json(new
            {
                state = 1,
                message = "Signin Successfully"
            });
            
        }

        // GET: Account/Details/5
        public ActionResult SignUp()
        {
           
                User user = UserHelpers.GetCurrentUser(Session);
                if (user != null)
                {
                    TempData["errorTittle"] = "Bad request";
                    TempData["errorMessage"] = "You are already signed in the system";
                    return RedirectToAction("Index", "Home");
                }
                TempData["errorTitle"] = null;
                TempData["errorMessage"] = null;
                return PartialView();
  
            
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignUpPost(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User();
                var listUser = new List<User>();
                listUser = db.Users.ToList();
                var newUser = listUser.FindAll(a => a.UserName.Equals(model.UserName));
                if (newUser.Count != 0)
                {
                    //ModelState.AddModelError("", "UserName is already exist. Please choose another.");
                    return Json(new
                    {
                        state = 0,
                        message = "UserName is already exist. Please choose another."
                    });
                }
                newUser = listUser.FindAll(a => a.UserEmail.Equals(model.Email));
                if (newUser.Count != 0)
                {
                    //ModelState.AddModelError("", "Email is already registered. Please choose another.");
                    return Json(new
                    {
                        state = 0,
                        message = "Email is already registered. Please choose another."
                    });
                }
                UserDatabaseHelper.Instance.AddNewUser(model, user);

                UserHelpers.SetCurrentUser(Session, user);

                //Create Channel
                UserDatabaseHelper.Instance.CreateUserChannel(user);
                //Send email confirm
                MailHelpers.Instance.SendMailWelcome(user.UserEmail,user.UserFirstName,user.UserLastName);
                //return RedirectToAction("RegisterSuccess", "Account");
                return Json(new
                {
                    state = 1,
                    message = "Registered Successfully"
                });
            }

            // If we got this far, something failed, redisplay form
            return Json(new
            {
                state = 0,
                message = "Something Wrong"
            });
        }


        private Uri RedirectUriGoogle
        {

            get
            {
                var uriBuilder = new UriBuilder(Request.Url)
                {
                    Query = null,
                    Fragment = null,
                    Path = Url.Action("GoogleCallback")
                };
                return uriBuilder.Uri;
            }
        }
        public ActionResult AuthenGoogle()
        {
            GoogleConnect.ClientId = "753316382181-58p94cof0aum06tigijhq3e1vlkqlgi8.apps.googleusercontent.com";
            GoogleConnect.ClientSecret = "1WmJi7FEw7rxs71B5EH2aH1f";
            GoogleConnect.RedirectUri = RedirectUriGoogle.AbsoluteUri.Split('?')[0];
            GoogleConnect.Authorize("profile", "email");
            return null;
        }

        public ActionResult GoogleCallback()
        {
            try
            {
                if (!string.IsNullOrEmpty(Request.QueryString["code"]))
                {
                    var code = Request.QueryString["code"];
                    dynamic google = JObject.Parse(GoogleConnect.Fetch("me", code));
                    var emailList = new JArray(google.emails);
                    var email = "";
                    foreach (var x in emailList)
                    {
                        var e = x.ToObject<Email>();
                        if (e.Type.Equals("account"))
                        {
                            email = e.Value;
                        }
                    }
                    
                    // select from DB
                    var newUser = UserDatabaseHelper.Instance.GetUserByEmail(email);
                    //if this is first time login
                    if (newUser == null)
                    {
                        var addressList = new JArray();
                        if (google.placesLived != null)
                        {
                            addressList = new JArray(google.placesLived);
                        }
                        var address = "";
                        foreach (var x in addressList)
                        {
                            var a = x.ToObject<Address>();
                            if (a.Primary)
                            {
                                address = a.Value;
                            }
                        }
                        var ggModel = new GoogleAccountModel
                        {
                            Email = email,
                            Place = address,
                            UserFirstName = google.name.familyName.Value,
                            UserLastName = google.name.givenName.Value
                            //Gender = google.gender == null ? 0 : google.gender.Value   
                        };
                        return View("ConfirmRegisterGoogle", ggModel);
                    }
                    if (UserDatabaseHelper.Instance.isLookedUser(newUser.UserName))
                    {
                        // user is Locked
                        GoogleConnect.Clear();
                        TempData["errorTitle"] = "Locked user";
                        TempData["errorMessage"] = "Ops...Your account has been locked! Please contact with our support!";
                        return RedirectToAction("Index", "Home");
                    }
                    TempData["errorTitle"] = null;
                    TempData["errorMessage"] = null;
                    UserHelpers.SetCurrentUser(Session, newUser);
                    return RedirectToAction("Index", "Home");
                }
                else if (!string.IsNullOrEmpty(Request.QueryString["error"]))
                {
                    TempData["errorTitle"] = "Signin Error";
                    TempData["errorMessage"] = Request.QueryString["error"].ToString();
                    return RedirectToAction("Index", "Home");
                }
                else {
                    UserHelpers.SetCurrentUser(Session, null);
                    return RedirectToAction("Index", "Home");
                }
               
            }
            catch
            {
                if (Request.Cookies["userName"] != null)
                {
                    HttpCookie userName = Request.Cookies["userName"];
                    userName.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(userName);
                }
                //remove cookie password
                if (Request.Cookies["password"] != null)
                {
                    HttpCookie password = Request.Cookies["password"];
                    password.Expires = DateTime.Now.AddDays(-1);
                    Request.Cookies.Add(password);
                }
                TempData["errorTitle"] = "Social Signin Error";
                TempData["errorMessage"] = "Failed to connect with Google! Check your connection please...";
                UserHelpers.SetCurrentUser(Session, null);
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult ConfirmRegisterGoogle(GoogleAccountModel model)
        {
            
            User user = UserHelpers.GetCurrentUser(Session);
            if (user != null)
            {
                TempData["errorTittle"] = "Bad request";
                TempData["errorMessage"] = "You are already signed in the system";
                return RedirectToAction("Index", "Home");
            }
            TempData["errorTitle"] = null;
            TempData["errorMessage"] = null;
            return View(model);
        }

        public ActionResult ExternalLoginConfirmation(GoogleAccountModel model)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            if (user != null) {
                TempData["errorTittle"] = "Bad request";
                TempData["errorMessage"] = "You are already signed in the system";
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                User newUser = UserDatabaseHelper.Instance.GetUserByUserName(model.UserName);
                if (newUser!= null)
                {
                    ModelState.AddModelError("", "UserName is already exist. Please choose another.");
                    TempData["errorTitle"] = null;
                    TempData["errorMessage"] = null;
                    return View("ConfirmRegisterGoogle", model);
                }
                newUser = new User();
                newUser.UserEmail = model.Email;
                newUser.UserName = model.UserName;
                newUser.UserPassword = model.Password;
                newUser.UserDOB = model.UserDOB;
                newUser.Place = model.Place;
                newUser.UserFirstName = model.UserFirstName;
                newUser.DataJoin = DateTime.Today;
                if (model.UserLastName != null && model.UserLastName != "")
                {
                    newUser.UserLastName = model.UserLastName;
                }
                newUser.AccountStatus = EventZoneConstants.ActiveUser; //set Active account
                newUser.Avartar = 10032;
                newUser.UserRoles = EventZoneConstants.User; //set UserRole
                // insert user to Database
                db.Users.Add(newUser);
                db.SaveChanges();
                UserHelpers.SetCurrentUser(Session, newUser);
                //Send email confirm
                MailHelpers.Instance.SendMailWelcome(newUser.UserEmail, newUser.UserFirstName, newUser.UserLastName);
                TempData["errorTitle"] = "Sucessfull SignUp";
                TempData["errorMessage"] = "Thank you for signing up in EventZone! We sent you a welcome message! Hope you have more fun and comfortable by joining with us";
                return RedirectToAction("Index", "Home");
            }
            TempData["errorTitle"] = "Invald input";
            TempData["errorMessage"] = "Invalid input! Please try again";
            // If we got this far, something failed, redisplay form
            return RedirectToAction("ExternalLoginConfirmation", "Account",model);
        }

        public ActionResult Signout()
        {
            var user = UserHelpers.GetCurrentUser(Session);
            try
            {
                GoogleConnect.Clear();
            }
            catch (Exception e)
            {

            }
            //remove cookie userName
            if (Request.Cookies["userName"] != null)
            {
                HttpCookie userName = Request.Cookies["userName"];
                userName.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(userName);
            }
            //remove cookie password
            if (Request.Cookies["password"] != null)
            {
                HttpCookie password = Request.Cookies["password"];
                password.Expires = DateTime.Now.AddDays(-1);
                Request.Cookies.Add(password);
            }
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now.AddHours(-1));
            Response.Cache.SetNoStore();
            UserHelpers.SetCurrentUser(Session, null);
            TempData["errorTitle"] = null;
            TempData["errorMessage"] = null;
           
         
            return RedirectToAction("Index", "Home");
        }
        private class Email
        {
            public string Value { get; set; }
            public string Type { get; set; }
        }

        private class Address
        {
            public string Value { get; set; }
            public bool Primary { get; set; }
        }

        public ActionResult ForgotPassword()
        {
            
                TempData["errorTitle"] = null;
                TempData["errorMessage"] = null;
                return PartialView();
           
        }

        public ActionResult HandleForgotPass(ForgotViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserDatabaseHelper.Instance.GetUserByEmail(model.Email);
                if (user != null)
                {
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    var newPassword = new string(
                        Enumerable.Repeat(chars, 8)
                            .Select(s => s[random.Next(s.Length)])
                            .ToArray());
                    string passHash = EventZoneUtility.Instance.HashPassword(newPassword);
                    var isUpdated = UserDatabaseHelper.Instance.ResetPassword(model.Email, passHash);
                    if (isUpdated)
                    {
                        MailHelpers.Instance.SendMailResetPassword(model.Email, newPassword);
                        return Json(new
                        {
                            state = 1,
                            message = "Reset Password Successfully"
                        });
                    }
                    return Json(new
                    {
                        state = 0,
                        message = "Something Wrong! Please Try Again Later"
                    });
                }
                return Json(new
                {
                    state = 0,
                    message = "We Couldn't Find Your Email in Our Database"
                });
            }
            return Json(new
            {
                state = 0,
                message = "The E-mail format is wrong !"
            });
        }
        public ActionResult CheckCookie(string url)
        {
            
            if (Request.Cookies["userName"] != null && Request.Cookies["password"] != null)
            {
                string userName = Request.Cookies["userName"].Value;
                string password = Request.Cookies["password"].Value;
                if (UserDatabaseHelper.Instance.ValidateUser(userName, password))
                {
                    var user = UserDatabaseHelper.Instance.GetUserByUserName(userName);
                    if (UserDatabaseHelper.Instance.isLookedUser(user.UserName))
                    {
                        return Json(new
                        {
                            success = 0,
                            message = "Your account has been locked! Please contact with admin to active it!"
                        });
                    }
                    UserHelpers.SetCurrentUser(Session, user);
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
                        message = "Your account has been changed password! Please try to sign in with a new password!"
                    });
                }
            }
            return Json(new
            {
                success = 0,
                message = "Cookie is empty!"
            });
        }
        

    }
}