using System.Linq;
using System.Web.Mvc;
using EventZone.Helpers;
using EventZone.Models;
using System.Collections.Generic;

namespace EventZone.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventZoneEntities db = new EventZoneEntities();
        public ActionResult Index()
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
                        UserHelpers.SetCurrentUser(Session, user);
                    }
                }
            }
            TempData["errorTitle"] = TempData["errorTitle"];
            TempData["errorMessage"] = TempData["errorMessage"];
            return View();
        }

        public ActionResult About()
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

                            return RedirectToAction("Index", "Home");
                        }
                        UserHelpers.SetCurrentUser(Session, user);
                    }
                }
            }
           
            return View();
        }
        public ActionResult Contact()
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

                            return RedirectToAction("Index", "Home");
                        }
                        UserHelpers.SetCurrentUser(Session, user);
                    }
                }
            }
            TempData["errorTitle"] = TempData["errorTitle"];
            TempData["errorMessage"] = TempData["errorMessage"];
            return View();
        }
        public ActionResult Policy()
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

                            return RedirectToAction("Index", "Home");
                        }
                        UserHelpers.SetCurrentUser(Session, user);
                    }
                }
            }
            TempData["errorTitle"] = TempData["errorTitle"];
            TempData["errorMessage"] = TempData["errorMessage"];
            return View();
        }
        public ActionResult Help()
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

                            return RedirectToAction("Index", "Home");
                        }
                        UserHelpers.SetCurrentUser(Session, user);
                    }
                }
            }
            TempData["errorTitle"] = TempData["errorTitle"];
            TempData["errorMessage"] = TempData["errorMessage"];
            return View();
        }
        /// <summary>
        /// return new event to new event column Home page
        /// </summary>
        /// <returns></returns>
        public ActionResult NewEvent() {
            List<ThumbEventHomePage> listThumb = new List<ThumbEventHomePage>();
            List<Event> listEvent = new List<Event>();
            listEvent= EventDatabaseHelper.Instance.GetListNewEvent();
            listEvent = EventDatabaseHelper.Instance.RemoveLockedEventInList(listEvent);
            User user= UserHelpers.GetCurrentUser(Session);
            if (user == null) {
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
                            listThumb = EventDatabaseHelper.Instance.GetThumbEventHomepage(listEvent);
                            return RedirectToAction("Index", "Home");
                        }
                        UserHelpers.SetCurrentUser(Session, user);
                    }
                }
            }
            if (user != null) {
                listEvent = EventDatabaseHelper.Instance.GetListNewEventByUser(user.UserID);
            }
            listThumb = EventDatabaseHelper.Instance.GetThumbEventHomepage(listEvent).Take(5).ToList();
            TempData["errorTitle"] = TempData["errorTitle"];
            TempData["errorMessage"] = TempData["errorMessage"];
            return PartialView("_ThumbEventHomepage", listThumb);
        }
        public ActionResult HotEvent()
        {
            List<ThumbEventHomePage> listThumb = new List<ThumbEventHomePage>();
            List<Event> listEvent = new List<Event>();
            listEvent = EventDatabaseHelper.Instance.GetHotEvent();
            listEvent = EventDatabaseHelper.Instance.RemoveLockedEventInList(listEvent);
            listThumb = EventDatabaseHelper.Instance.GetThumbEventHomepage(listEvent).Take(5).ToList();
            TempData["errorTitle"] = TempData["errorTitle"];
            TempData["errorMessage"] = TempData["errorMessage"];
            return PartialView("_ThumbEventHomepage",listThumb);
        }
        public ActionResult LiveEvent()
        {
            List<ThumbEventHomePage> listThumb = new List<ThumbEventHomePage>();
            List<Event> listEvent = new List<Event>();
            listEvent = EventDatabaseHelper.Instance.SearchLiveStreamByKeyword("");
            listEvent = EventDatabaseHelper.Instance.RemoveLockedEventInList(listEvent);
            listThumb = EventDatabaseHelper.Instance.GetThumbEventHomepage(listEvent).Take(5).ToList() ;
            TempData["errorTitle"] = TempData["errorTitle"];
            TempData["errorMessage"] = TempData["errorMessage"];
            return PartialView("_ThumbEventHomepage",listThumb);
        }
        public ActionResult LoadCateInfo(long categoryID)
        {
            Category cate = CommonDataHelpers.Instance.GetCategoryById(categoryID);
            return PartialView("_LoadCategoryInformation", cate);

        }
    }
}