using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.WebPages;
using EventZone.Helpers;
using EventZone.Models;
using Microsoft.Ajax.Utilities;

namespace EventZone.Controllers
{
    public class SearchController : Controller
    {
        public ActionResult Search()
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
            return PartialView();
        }

        public ActionResult SearchAdvance()
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
            return PartialView(new AdvanceSearch());
        }

        public ActionResult CategorySearch(long categoryid)
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
            List<Event> listEvent = new List<Event>();
            List<Event> liveEvent= new List<Event>();
            listEvent = EventDatabaseHelper.Instance.SearchEventByCategoryID(categoryid);
            liveEvent = EventDatabaseHelper.Instance.GetLiveEventByListEvent(listEvent);
            try
            {
                TempData["task"] = "Category " + CommonDataHelpers.Instance.GetCategoryById(categoryid).CategoryName;
            }
            catch { }
            TempData["listLiveStream"] = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(liveEvent);
            TempData["listEvent"] = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(listEvent);
            return View("SearchResult");
        }
        [HttpPost]
        public ActionResult BasicSearch(BasicSearch model)
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
            string keyword;
            if (model.Keyword.IsNullOrWhiteSpace())
            {
                keyword = "";
            }
            else
            {
                keyword = model.Keyword;
            }
            keyword = keyword.Trim();
            TempData["listEvent"] =
                EventDatabaseHelper.Instance.GetThumbEventListByListEvent(
                    EventDatabaseHelper.Instance.SearchEventByKeyword(keyword));
            TempData["listLiveStream"] =
                EventDatabaseHelper.Instance.GetThumbEventListByListEvent(
                    EventDatabaseHelper.Instance.SearchLiveStreamByKeyword(keyword));
            TempData["listUser"] = UserDatabaseHelper.Instance.GetUserThumbByList(UserDatabaseHelper.Instance.SearchUserByKeyword(keyword));
            TempData["task"] = "Search";
            return View("SearchResult");
        }

        public ActionResult AdvanceSearch(AdvanceSearch model, int datepick)
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
            List<Event> listEvent = new List<Event>();
            AdvanceSearch myModel = model;
            List<Event> listLiveStream = new List<Event>();
            if (model.Keyword != null)
            {
                model.Keyword = model.Keyword.Trim();
            }
            listEvent = EventDatabaseHelper.Instance.SearchEventByKeyword(model.Keyword);
            listLiveStream = EventDatabaseHelper.Instance.SearchLiveStreamByKeyword(model.Keyword);
            if (model.SelectedCategory != null && model.SelectedCategory.Length != 0)
            {
                listEvent = EventDatabaseHelper.Instance.SearchByListCategory(listEvent, model.SelectedCategory);
                listLiveStream = EventDatabaseHelper.Instance.SearchByListCategory(listLiveStream, model.SelectedCategory);
            }

            if (model.Location != null && model.Location.LocationName != null &&
                !(model.Location.Longitude == 0 && model.Location.Latitude == 0))
            {
                listEvent = EventDatabaseHelper.Instance.GetEventAroundLocation(model.Location, 20, listEvent);
                listLiveStream = EventDatabaseHelper.Instance.GetEventAroundLocation(model.Location, 20, listLiveStream);
            }
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            if (datepick == 0)
            {
                startTime = new DateTime(0001, 1, 1, 0, 0, 0);
                endTime = new DateTime(5000, 12, 30, 0, 0, 0);
            }
            else if (datepick == 1)
            {
                startTime = DateTime.Today;
                endTime = DateTime.Today.AddDays(1);
            }
            else if (datepick == 2)
            {
                startTime = DateTime.Today;
                endTime = startTime.AddDays(4);
            }
            else if (datepick == 3)
            {
                startTime = DateTime.Today;
                endTime = startTime.AddDays(8);
            }
            else if (datepick == 4)
            {
                startTime = model.StartDateRange;
                endTime = model.FinishDateRange.AddDays(1);
            }

            listEvent = EventDatabaseHelper.Instance.GetEventInDateRange(startTime, endTime, listEvent);
            listLiveStream = EventDatabaseHelper.Instance.GetEventInDateRange(startTime, endTime, listLiveStream);
            TempData["listEvent"] = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(listEvent);
            TempData["listLiveStream"] = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(listLiveStream);
            TempData["listUser"] = new List<ViewThumbUserModel>();
            TempData["task"] = "Search";
            if (model.Keyword == null||model.Keyword.Trim().IsEmpty())
            {
                TempData["task"] = "AdvanceSearch";
            }
           
            return View("SearchResult");
        }
    }
}