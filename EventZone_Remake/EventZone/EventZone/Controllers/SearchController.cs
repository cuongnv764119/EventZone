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

        public ActionResult CategorySearch(long categoryid, long tab=-1)
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
            TempData["TabSearch"] = tab;
            Session["listLiveStream"] = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(liveEvent);
            Session["listEvent"] = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(listEvent);
            return View("SearchResult");
        }
        [HttpPost]
        public ActionResult BasicSearch(BasicSearch model, int tab = -1)
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
            TempData["TabSearch"] = tab;
            Session["listEvent"] =
                EventDatabaseHelper.Instance.GetThumbEventListByListEvent(
                    EventDatabaseHelper.Instance.SearchEventByKeyword(keyword));
            Session["listLiveStream"] =
                EventDatabaseHelper.Instance.GetThumbEventListByListEvent(
                    EventDatabaseHelper.Instance.SearchLiveStreamByKeyword(keyword));
            Session["listUser"] = UserDatabaseHelper.Instance.GetUserThumbByList(UserDatabaseHelper.Instance.SearchUserByKeyword(keyword));
            TempData["task"] = "Search";
            return View("SearchResult");
        }

        public ActionResult AdvanceSearch(AdvanceSearch model, int datepick, int tab = -1)
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
            if (model.SelectedCategory != null && model.SelectedCategory.Length != 0)
            {
                listEvent = EventDatabaseHelper.Instance.SearchByListCategory(listEvent, model.SelectedCategory);

            }

            if (model.Location != null && model.Location.LocationName != null &&
                !(model.Location.Longitude == 0 && model.Location.Latitude == 0))
            {
                listEvent = EventDatabaseHelper.Instance.GetEventAroundLocation(model.Location, 20, listEvent);

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
            listLiveStream = EventDatabaseHelper.Instance.GetLiveStreamingFromList(listEvent);
            Session["listEvent"] = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(listEvent);
            Session["listLiveStream"] = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(listLiveStream);
            Session["listUser"] = new List<ViewThumbUserModel>();
            TempData["task"] = "Search";

            if (model.IsLive)
            {
                    tab = 3;
                  
            }
            TempData["TabSearch"] = tab;

            if (model.Keyword == null||model.Keyword.Trim().IsEmpty())
            {
                TempData["task"] = "AdvanceSearch";
            }
            return View("SearchResult");
        }
        public ActionResult LoadEventInPage(int page, int type){

            var listEvent = Session["listEvent"] as List<ViewThumbEventModel>;
            var listLiveStream = Session["listLiveStream"] as List<ViewThumbEventModel>;

            int listEventstartAt = (page-1) * 10;
            int ListEventendAt = (listEvent.Count) < (page * 10) ? (listEvent.Count - 1) : (page * 10) - 1;
            
            int listLiveEventstartAt = (page-1) * 10;
            int listLiveEventendtAt = (listLiveStream.Count) < (page * 10) ? (listLiveStream.Count) : (page * 10) - 1;
            
            if (type == 1)
            {
                if (listEvent == null) {
                    TempData["LoadMore"] = false;
                    return PartialView("_EventThumbPage", null);
                }
                if (listEvent.Count > (page * 10))
                {
                    TempData["LoadMore"] = true;
                }
                else
                {
                    TempData["LoadMore"] = false;
                }
                List<ViewThumbEventModel> listView = new List<ViewThumbEventModel>();
                for(int i=listEventstartAt;i<ListEventendAt;i++){
                    listView.Add(listEvent[i]);
                }
               
                return PartialView("_EventThumbPage", listView);
            }   
            else {
                if (listLiveStream == null)
                {
                    TempData["LoadMore"] = false;
                    return PartialView("_EventThumbPage", null);
                }
                if (listLiveStream.Count > (page * 10))
                {
                    TempData["LoadMore"] = true;
                }
                else
                {
                    TempData["LoadMore"] = false;
                }
                List<ViewThumbEventModel> listView = new List<ViewThumbEventModel>();
                for (int i = listLiveEventstartAt; i < listLiveEventendtAt; i++)
                {
                    listView.Add(listLiveStream[i]);
                }

                return PartialView("_EventThumbPage", listView);
            }
            
        }
        public ActionResult LoadUserInPage(int page)
        {
            var listUser = Session["listUser"] as List<ViewThumbUserModel>;

            int startIndex = (page - 1) * 10;
            int endIndex = (listUser.Count) < (page * 10) ? (listUser.Count - 1) : (page * 10) - 1;
            if (listUser == null)
            {
                TempData["LoadMore"] = false;
                return PartialView("_UserThumbnail", null);
            }
            if (listUser.Count > (page * 10))
            {
                TempData["LoadMore"] = true;
            }
            else
            {
                TempData["LoadMore"] = false;
            }
            List<ViewThumbUserModel> listView = new List<ViewThumbUserModel>();
                for (int i = startIndex; i < endIndex; i++) {
                    listView.Add(listUser[i]);
                }
            return PartialView("_UserThumbnail", listView);
        }

            

        
    }
}