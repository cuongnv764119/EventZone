using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.DynamicData;
using Amazon.CloudWatchLogs;
using EventZone.Helpers;
using EventZone.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Channel = EventZone.Models.Channel;
using Comment = EventZone.Models.Comment;
using Video = EventZone.Models.Video;
using Amazon.S3;
using Quartz.Util;

namespace EventZone.Helpers
{

    /// <summary>
    ///     All functions related to User
    /// </summary>
    public class UserDatabaseHelper : SingletonBase<UserDatabaseHelper>
    {
        private readonly EventZoneEntities db;

        private UserDatabaseHelper()
        {
            db = new EventZoneEntities();
        }

        /// <summary>
        /// Check is user exists in database or not. If yes return true, else return false.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateUser(string userName, string password)
        {
            try
            {
                List<long> id = (from a in db.Users where a.UserName == userName && a.UserPassword == password select a.UserID).ToList();
                if (id != null&&id.Count>0) {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public void AddNewUser(SignUpViewModel model, User user)
        {
            user.UserEmail = model.Email;
            user.UserName = model.UserName;
            user.UserPassword = model.Password;
            user.UserDOB = model.UserDOB;
            user.UserFirstName = model.UserFirstName;
            user.DataJoin = DateTime.Today;
            if (model.UserLastName != null && model.UserLastName != "")
            {
                user.UserLastName = model.UserLastName;
            }
            user.AccountStatus = EventZoneConstants.ActiveUser; //set Active account
            if (user.Avartar == null) //set default avatar
            {
                user.Avartar = 10032;
            }
            user.UserRoles = EventZoneConstants.User; //set UserRole
            // insert user to Database
            db.Users.Add(user);
            db.SaveChanges();
        }

        /// <summary>
        ///     Check is user's status locked or not. If user's status is locked, return true else return false
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool isLookedUser(string userName)
        {
            try {
                var user = (from a in db.Users where a.UserName == userName && a.AccountStatus == EventZoneConstants.Lock select a.UserID).ToList();
                if (user != null && user.Count > 0) {
                    return true;
                }
                return false;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        ///     Create a default channel for user when they signup first time. return true if success, else return false
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool CreateUserChannel(User user)
        {
            try
            {
                var channel = new Channel();
                channel.UserID = user.UserID;
                channel.ChannelName = user.UserFirstName +
                                      (user.UserLastName == "" || user.UserLastName == null
                                          ? ""
                                          : " " + user.UserLastName);
                channel.ChannelDescription = "";
                db.Channels.Add(channel);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// user change avatar
        /// </summary>
        /// <param name="imageId"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool UpdateAvatar(User user, Image image)
        {
            try {
                db.Images.Add(image);
                db.SaveChanges();
                user.Avartar = image.ImageID;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
                return true;
            }catch{
                return false;
            }
        }

        /// <summary>
        ///     get Channel by userID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public Channel GetUserChannel(long? userID)
        {
            try {
                Channel channel = (from a in db.Channels where a.UserID == userID select a).ToList()[0];
                if (channel != null)
                {
                    return channel;
                }
                
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// get list following categoryID by user
        /// </summary>
        public List<long> GetListFollowingCategoryByUser (long userID){
            try
            {
                List<long> result = (from a in db.CategoryFollows where a.FollowerID == userID select a.CategoryID).ToList();
                return result;
            }
            catch {
                return null;
            }
        }
        /// <summary>
        ///     count numbers event of user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public int CountOwnedEvent(long userID, bool owner=false)
        {
            try
            {
                var k = GetUserEvent(userID, -1, owner).Count;
                return k;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        ///     get thumb list user from list user for viewing
        /// </summary>
        /// <param name="listUser"></param>
        /// <returns></returns>
        public List<ViewThumbUserModel> GetUserThumbByList(List<User> listUser)
        {
            var listView = new List<ViewThumbUserModel>();
            if (listUser == null)
            {
                return null;
            }
            foreach (var item in listUser)
            {
                var view = new ViewThumbUserModel();
                view.UserID = item.UserID;
                view.Avatar = item.Avartar;
                view.Name = item.UserFirstName +
                            (item.UserLastName == null || item.UserLastName == "" ? "" : item.UserLastName);
                view.NumberFollower = NumberFollower(item.UserID);
                view.NumberOwnedEvent = Instance.CountOwnedEvent(item.UserID);
                listView.Add(view);
            }
            return listView;
        }

        public Report IsReportedEvent(long userID, long eventID)
        {
            try {
                Report result = (from a in db.Reports where a.SenderID == userID && a.EventID == eventID select a).ToList()[0];
                if (result != null) {
                    return result;
                }
                   
            }
            catch { 
            }
            return null;
        }
       
        /// <summary>
        ///     Check is user following another user
        /// </summary>
        /// <param name="FollowerID"></param>
        /// <param name="FollowingID"></param>
        /// <returns></returns>
        public bool IsFollowingUser(long FollowerID, long FollowingID)
        {
            var people =
                (from a in db.PeopleFollows
                 where a.FollowerUserID == FollowerID && a.FollowingUserID == FollowingID
                 select a).ToList();
            if (people != null&&people.Count>0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     User Follows another user
        /// </summary>
        /// <param name="FollowerID"></param>
        /// <param name="FollowingID"></param>
        /// <returns></returns>
        public bool FollowPeople(long FollowerID, long FollowingID)
        {
            try
            {
                var ppfollow = new PeopleFollow();
                ppfollow.FollowerUserID = FollowerID;
                ppfollow.FollowingUserID = FollowingID;
                db.PeopleFollows.Add(ppfollow);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public Report ReportEvent(long userID, long eventID, int reportID, string reportContent) {

            Report newReport = new Report
            {
                EventID = eventID,
                SenderID = userID,
                ReportType= reportID,
                ReportContent=reportContent,
                ReportStatus=EventZoneConstants.Pending,
                ReportDate=DateTime.Now,
            };
            try
            {
                db.Reports.Add(newReport);
                db.SaveChanges();
                return newReport;
            }
            catch {
                return null;
            }
        }
        /// <summary>
        ///     Check is user following a event
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public bool IsFollowingEvent(long userID, long eventID)
        {
            try
            {
                var evtFollow =
                    (from a in db.EventFollows where a.FollowerID == userID && a.EventID == eventID select a.EventFollowID).ToList();
                if (evtFollow != null&&evtFollow.Count>0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     User follows event
        /// </summary>
        /// <param name="useID"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public bool FollowEvent(long useID, long eventID)
        {
            try
            {
                if (IsFollowingEvent(useID, eventID))
                {
                    UnFollowEvent(useID, eventID);
                    return true;
                }
                var evtFollow = new EventFollow();
                evtFollow.EventID = eventID;
                evtFollow.FollowerID = useID;
                db.EventFollows.Add(evtFollow);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// user unfollow event
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public bool UnFollowEvent(long userId, long eventId)
        {
            try
            {
                var follow =
                    (from a in db.EventFollows where a.FollowerID == userId && a.EventID == eventId select a).ToList()[0];
                db.EventFollows.Remove(follow);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// get all event is created by a given user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<Event> GetUserEvent(long userID, int numberEvent=-1,bool isOwner=true) {
            List<Event> myEvent = new List<Event>();
            try {
                myEvent = (from a in db.Channels join b in db.Events on a.ChannelID equals b.ChannelID where a.UserID == userID select b).ToList();
                if (!isOwner) {
                    myEvent.RemoveAll(o => (o.Privacy != EventZoneConstants.publicEvent) || (o.Status != EventZoneConstants.Active));
                }
                 if (numberEvent != -1) {
                    myEvent = myEvent.Take(numberEvent).ToList();
                }
            }
            catch { }
            return myEvent;
        }
        public List<Event> GetFollowingEvent(long userID)
        {
            try
            {
                List<Event> result= (from a in db.Events join b in db.EventFollows on a.EventID equals b.EventID where b.FollowerID==userID select a).ToList();    
                return result;
            }
            catch {
                return null;
            }
        }
        /// <summary>
        ///     Check xem user like or dislike event. Nếu chưa like hoặc dislike trả lại 0;
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public LikeDislike FindLike(long userId, long eventID)
        {
            try
            {
                var like =
                    (from a in db.LikeDislikes where a.UserID == userId && a.EventID == eventID select a).ToList()[0];
                if (like != null)
                {
                    return like;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Like event
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public bool InsertLike(long userID, long eventID)
        {
            try
            {
                //Check user liked this event or not		
                var findLike = FindLike(userID, eventID);
                //Check user liked this event or not		
                if (findLike != null)
                {
                    findLike.Type = EventZoneConstants.Like;
                    db.Entry(findLike).State = EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(findLike).Reload();
                    return true;
                }
                //If user dont like or dislike this event before		
                var like = new LikeDislike();
                like.Type = EventZoneConstants.Like;
                like.UserID = userID;
                like.EventID = eventID;
                db.LikeDislikes.Add(like);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     dislike event
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public bool InsertDislike(long userID, long eventID)
        {
            try
            {
                //Check user liked this event or not		
                var findLike = FindLike(userID, eventID);
                //Check user liked this event or not		
                if (findLike != null)
                {
                    findLike.Type = EventZoneConstants.Dislike;
                    db.Entry(findLike).State = EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(findLike).Reload();
                    return true;
                }
                //If user dont like or dislike this event before		
                var like = new LikeDislike();
                like.Type = EventZoneConstants.Dislike;
                like.UserID = userID;
                like.EventID = eventID;
                db.LikeDislikes.Add(like);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     count number follower of user
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public int NumberFollower(long UserID)
        {
            try
            {
                var k = (from a in db.PeopleFollows where a.FollowingUserID == UserID select a.PeopleFollowID).Count();
                return k;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        ///     Check is user following a category
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public bool IsFollowingCategory(long userID, long categoryID)
        {
            try
            {
                var carFollow =
                    (from a in db.CategoryFollows where a.FollowerID == userID && a.CategoryID == categoryID select a.CategoryFollowID).ToList()[0];
                if (carFollow != null)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        /// <summary>
        ///    Follow category if user doest now follow it, unfollow category if user is following this category
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public bool FollowCategory(long userID, long categoryID)
        {
            try
            {
                var catFollow = new CategoryFollow();
                if (IsFollowingCategory(userID, categoryID))
                {
                    catFollow = (from a in db.CategoryFollows where a.CategoryID == categoryID && a.FollowerID == userID select a).ToList()[0];
                    db.CategoryFollows.Remove(catFollow);
                    db.SaveChanges();
                    return true;
                }
                catFollow.FollowerID = userID;
                catFollow.CategoryID = categoryID;
                db.CategoryFollows.Add(catFollow);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     find user by email, return null if not found
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public User GetUserByEmail(string email)
        {
            var user = (from a in db.Users where a.UserEmail == email select a).ToList();
            if (user != null && user.Count > 0)
            {
                return user[0];
            }
            return null;
        }

        /// <summary>
        ///     get user by user name, return null if not found
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public User GetUserByUserName(string userName)
        {
            var user = (from a in db.Users where a.UserName == userName select a).ToList();
            if (user!=null&&user.Count > 0)
            {
                return user[0];
            }
            return null;
        }
        /// <summary>
        /// get user by account information
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User GetUserByAccount(string userName, string password) {
            User result = null;
            try
            {
                result = (from a in db.Users where a.UserName == userName && a.UserPassword == password select a).ToList()[0];  
            }
            catch { }
            return result;
        }
        /// <summary>
        ///     Get user by userId, return null if not found
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public User GetUserByID(long? userID)
        {
            User user = db.Users.Find(userID);
            return user;
        }
        /// <summary>
        /// Get All user in database
        /// </summary>
        /// <returns></returns>
        public List<User> GetAllUser(){
            try
            {
                List<User> result = db.Users.ToList();
                return result;
            }
            catch { }
            return null;
        }
        /// <summary>
        ///     Update User to database
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UpdateUser(User user)
        {
            try
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        ///     Change user password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ResetPassword(string email, string password)
        {
            var user = GetUserByEmail(email);
            if (user != null)
            {
                user.UserPassword = password;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Search user By keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<User> SearchUserByKeyword(string keyword)
        {
            var listResult = new List<User>();
            if (keyword == "" || keyword == null)
            {
                listResult = db.Users.ToList();
                return listResult;
            }
            keyword = keyword.ToLower();
            var retrievedResult = (from x in db.Users
                                   where x.UserFirstName.ToLower().Contains(keyword) || x.UserLastName.ToLower().Contains(keyword)||x.UserName.ToLower().Contains(keyword)
                                   select x).ToList();
            return retrievedResult;
        }
        public bool ChangePassword(User user, string password) {
            try
            {
                user.UserPassword = password;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
                return true;
            }
            catch {
                return false;
            }
        }
        public string GetUserDisplayName(long? UserID) {
            string result = "";
            try {
                User user = GetUserByID(UserID);
                result = user.UserFirstName + " "+(string.IsNullOrEmpty(user.UserLastName) ? "" : user.UserLastName);
            }
            catch
            {
            }
            return result;
        }

        /// <summary>
        /// get all following of an user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<User> GetListFollowingOfUser(long userID)
        {
            try{
            List<User> result= (from a in db.Users join b in db.PeopleFollows on a.UserID equals b.FollowingUserID where b.FollowerUserID==userID select a).ToList();
            return result;
            }catch{}
            return null;
        }
        /// <summary>
        /// get list follower of an user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<User> GetListFollowerOfUser(long userID)
        {
            try
            {
                List<User> result = (from a in db.Users join b in db.PeopleFollows on a.UserID equals b.FollowerUserID where b.FollowingUserID == userID select a).ToList();
                return result;
            }
            catch { }
            return null;
        }
        /// <summary>
        /// Get all pending Reported event
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<Event> GetPendingReportedEvent(long userID)
        {
            try { 
                Channel channel= GetUserChannel(userID);
                List<Event> result = (from a in db.Events 
                                      join report in db.Reports 
                                      on a.EventID equals report.EventID 
                                      where a.ChannelID == channel.ChannelID && report.ReportStatus == EventZoneConstants.Pending 
                                      select a).ToList();
                return result;
            }
            catch { }
            return new List<Event>();
        }
        /// <summary>
        /// Get All Event Has Report
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<Event> GetAllEventHasReports(long userID)
        {
            Channel channel = GetUserChannel(userID);
            List<Event> result = new List<Event>();
            try {
                result = (from a in db.Events
                          join report in db.Reports
                          on a.EventID equals report.EventID
                          where a.ChannelID == channel.ChannelID
                          select a).Distinct().ToList();
                foreach (var item in result)
                {
                    db.Entry(item).Reload();
                }
            }
            catch { }
            return result;
        }
        /// <summary>
        /// get pending appeal of event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public Appeal GetPendingAppeal(long eventID) {
            try {
                Appeal appeal = (from a in db.Appeals where a.EventID == eventID && a.AppealStatus == EventZoneConstants.Pending select a).ToList()[0];
                return appeal;
            }
            catch { }
            return null;
        }
        /// <summary>
        /// check is success appeal or not
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool AppealSuccess(long eventID)
        {
            try{
                Appeal appeal = (from a in db.Appeals where a.EventID==eventID select a).OrderByDescending(o => o.ResultDate).ToList()[0];
                if (appeal.AppealStatus == EventZoneConstants.Approved) { return true; }
            }catch{
            }
            return false;
        }
        public bool AppealFailed(long eventID) {
            try
            {
                Appeal appeal = (from a in db.Appeals where a.EventID == eventID select a).OrderByDescending(o => o.ResultDate).ToList()[0];
                if (appeal.AppealStatus == EventZoneConstants.Rejected) { return true; }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// UnFollow user
        /// </summary>
        /// <param name="p"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool UnFollowPeople(long followerID, long followingID)
        {
            try
            {
                PeopleFollow followingUser = (from a in db.PeopleFollows where a.FollowerUserID == followerID && a.FollowingUserID == followingID select a).ToList()[0];
                db.PeopleFollows.Remove(followingUser);
                db.SaveChanges();
                return true;
            }
            catch { } 
            return false;
        }
    }

    /// <summary>
    ///     All function related to Event
    /// </summary>
    public class EventDatabaseHelper : SingletonBase<EventDatabaseHelper>
    {
        private readonly EventZoneEntities db;

        private EventDatabaseHelper()
        {
            db = new EventZoneEntities();
        }

        public List<Event> GetAllEvent() {
            List<Event> result = new List<Event>();
            try {
                result = db.Events.ToList();
            }
            catch { }
            return result;
        }

        /// <summary>
        ///     get all locations of an event
        /// </summary>
        public List<Location> GetEventLocation(long? EventID)
        {
            try
            {
                var result = (from a in db.Locations join b in db.EventPlaces on a.LocationID equals b.LocationID where b.EventID == EventID select a).ToList();
                return result;
            }
            catch { }
            return null;
            
        }
        /// <summary>
        ///     Get event by eventID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Event GetEventByID(long? id)
        {
            try
            {
                Event evt = db.Events.Find(id);
                db.Entry(evt).Reload();
                return evt;
            }
            catch
            {
                return null;
            }
        }
        
        public bool UpdateEvent(EditViewModel model)
        {
            try
            {
                Event editedEvent = EventDatabaseHelper.Instance.GetEventByID(model.eventID);
                editedEvent.EventEndDate = model.EndTime;
                editedEvent.EventName = model.Title;
                editedEvent.Privacy = model.Privacy;
                editedEvent.EventStartDate = model.StartTime;
                editedEvent.EventDescription = model.Description;
                editedEvent.EditTime = DateTime.Now;
                List<Location> currentLocations = EventDatabaseHelper.Instance.GetEventLocation(editedEvent.EventID);
                List<Location> newLocations = new List<Location>();
                for (int i = 0; i < currentLocations.Count; i++)
                {
                    bool checkExisted = false;
                    for (int j = 0; j < model.Location.Count; j++)
                    {
                        if (currentLocations[i].LocationName == model.Location[j].LocationName
                            && Equals(currentLocations[i].Latitude, model.Location[j].Latitude)
                            && Equals(currentLocations[i].Longitude, model.Location[j].Longitude))
                        {
                            checkExisted = true;
                        }
                    }
                    if (!checkExisted)
                    {
                        LocationHelpers.Instance.RemoveLocationByEventLocationID(editedEvent.EventID,currentLocations[i].LocationID);
                    }
                }
                for (int j = 0; j < model.Location.Count; j++)
                {
                    bool checkExisted = false;
                    for (int i = 0; i < currentLocations.Count; i++)
                     {
                         if (currentLocations[i].LocationName == model.Location[j].LocationName
                            && Equals(currentLocations[i].Latitude, model.Location[j].Latitude)
                            && Equals(currentLocations[i].Longitude, model.Location[j].Longitude))
                         {
                             checkExisted = true;
                         }
                     }
                    if (!checkExisted)
                    {
                        newLocations.Add(model.Location[j]);
                    }
                }
                EventDatabaseHelper.Instance.AddEventPlace(LocationHelpers.Instance.AddNewLocation(newLocations), editedEvent);
                db.Entry(editedEvent).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(editedEvent).Reload();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        ///     Inscrease number view of event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public bool AddViewEvent(long eventID)
        {
            var evt = Instance.GetEventByID(eventID);
            if (evt != null)
            {
                evt.View += 1;
                db.Entry(evt).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(evt).Reload();                  
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Get all image of an event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Image> GetEventImage(long? id)
        {
            try
            {
                List<Image> listImage = (from a in db.EventImages join b in db.Images on a.ImageID equals b.ImageID where a.EventID==id select b).ToList();   
                return listImage;
                
            }
            catch {
                return null;
            }

        }

        public List<Image> GetEventApprovedImage(long? id)
        {
            try
            {
                List<Image> listImage = (from a in db.EventImages join b in db.Images on a.ImageID equals b.ImageID where a.EventID == id  && a.Approve==true select b).ToList();
                return listImage;

            }
            catch
            {
                return null;
            }
        }

        public List<Image> GetPendingImageInEvent(long? id) {
            try
            {
                List<Image> listImage = (from a in db.EventImages join b in db.Images on a.ImageID equals b.ImageID where a.EventID == id && a.Approve == false select b).ToList();
                return listImage;

            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        ///     Get all video of an event
        /// </summary>
        /// <param name="id"></param>
        /// <returns> </returns>
        public List<Video> GetEventVideo(long? id)
        {
            var eventVideo = new List<Video>();

            List<long> listEventPlaceID = (from a in db.EventPlaces where a.EventID == id select a.EventPlaceID).ToList();

            if (listEventPlaceID.Count != 0)
            {
                foreach (var item in listEventPlaceID)
                {
                    List<Video> video = (from a in db.Videos where a.EventPlaceID == item select a).ToList();
                    eventVideo.AddRange(video);
                }
            }
            eventVideo = eventVideo.Distinct().ToList();
            return eventVideo;
        }
        /// <summary>
        ///     Get all comment of an event
        /// </summary>
        /// <returns></returns>
        public List<Comment> GetEventComment(long? eventID)
        {
            var result = (from a in db.Comments where a.EventID == eventID select a).ToList();
            return result;
        }

        ///
        /// 
        public String GetEventCategory(long? eventID)
        {
             try
            {
                var result = (from a in db.Events join b in db.Categories on a.CategoryID equals b.CategoryID where a.EventID == eventID select b).ToList();
                return result[0].CategoryName;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        ///     Lay thong tin nguoi dang event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public User GetAuthorEvent(long? eventId)
        {
            Event evt = db.Events.Find(eventId);
            try
            {
                User user = (from a in db.Users join b in db.Channels on a.UserID equals b.UserID where b.ChannelID == evt.ChannelID select a).ToList()[0];
                return user;
            }
            catch { }
            return null;
        }
        /// <summary>
        /// get event place by eventPlaceID
        /// </summary>
        /// <param name="eventPlaceID"></param>
        /// <returns></returns>
        public EventPlace GetEventPlaceByID(long eventPlaceID)
        {
            EventPlace result = new EventPlace();
            try {
                result = db.EventPlaces.Find(eventPlaceID);
                
            }
            catch { }
            return result;

        }
        //Check is event owned by user or not
        public bool IsEventOwnedByUser(long? eventID, long? UserID)
        {
            try
            {
                var channel = UserDatabaseHelper.Instance.GetUserChannel(UserID);
                if (channel != null)
                {
                    var evt = db.Events.Find(eventID);
                    if (channel.ChannelID == evt.ChannelID) return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Get all user comment to event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public List<User> GetUniqueUserComment(long? eventID) {
            List<User> result = new List<User>();
            try
            {
                result = (from a in db.Comments join b in db.Users on a.UserID equals b.UserID where a.EventID == eventID select b).Distinct().ToList();
            }
            catch
            {
            }
            return result;
        }
        /// <summary>
        /// count number of unique user comment on event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public int CountUniqueUserComment(long eventID) {
            int count = 0;
            try
            {
                count = (from a in db.Comments where a.EventID == eventID select a.UserID).Distinct().ToList().Count();
            }
            catch { 
            }
            return count;
        }
        /// <summary>
        /// calculate event score to rank it, each view will be calculate as 1 point, like(or dislike) = 2 , unique user comment = 3 point, follow = 4 point.
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public long CalculateEventScore(long eventId) {
            long score = 0;
            try {
                Event evt= GetEventByID(eventId);
                double point= (evt.View * 0.001746201
            + (CountDisLike(eventId) + CountLike(eventId)) * 0.29578499
            + CountUniqueUserComment(eventId) ) *0.7+  CountFollowerOfEvent(eventId) * 0.3;
                score = long.Parse((point * 1000).ToString());
                if (evt.IsVerified) {
                    score = score + 100000;
                }
            }
            catch
            {
            }
            return score;
        }

        /// <summary>
        ///     trả lại event có tên, địa điểm hoặc description trùng với keyword, keyword = null thi tra lai toan bo event
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<Event> SearchEventByKeyword(string keyword)
        {
            if (keyword == "" || keyword == null)
            {
                return db.Events.ToList();
            }
            keyword = keyword.ToLower();
            var listResult = new List<Event>();
            var retrievedResult = (from x in db.Events
                                   where x.EventName.ToLower().Contains(keyword) || x.EventDescription.ToLower().Contains(keyword)
                                   select x).ToList();
            return retrievedResult;
        }
        /// <summary>
        /// check is live streaming video or not?
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        public bool IsLiveVideo(Video video) {
            try
            {
                DateTime currentTime = DateTime.Now;
                var start = video.StartTime;
                var end = video.EndTime;
                if (end != null && start.CompareTo(currentTime) <= 0 && currentTime.CompareTo(end) < 0)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }
        /// <summary>
        /// get list event Place by event ID
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public List<EventPlace> GetEventPlaceByEvent(long eventID){
            List<EventPlace> result = new List<EventPlace>();
            try
            {
                result = (from a in db.EventPlaces where a.EventID == eventID select a).ToList();
            }
            catch { 

            }
            return result;
        }
        /// <summary>
        /// get all reported event
        /// </summary>
        /// <returns></returns>
        public List<Event> GetAllReportedEvent() {
            try {
                List<Event> result = (from evt in db.Events join report in db.Reports on evt.EventID equals report.EventID select evt).Distinct().ToList();
                return result;
            }
            catch { 
            }
            return null;
        }

        /// <summary>
        /// get list report of event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public List<Report> GetListReportOfEvent(long eventID) {

            try {
                List<Report> result = (from a in db.Reports where a.EventID == eventID select a).OrderByDescending(m => m.ReportDate).ToList();
                return result;
            }
            catch { 
            
            }
            return null;
        }
        /// <summary>
        /// add Video to DB
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        public bool AddVideo(Video video) {
            try
            {
                db.Videos.Add(video);
                db.SaveChanges();
                return true;
            }
            catch { 
            
            }
            return false; 
        }
        
        /// <summary>
        /// Get List Live streaming video in list video
        /// </summary>
        /// <returns></returns>
        public List<Video> GetListLiveVideo(List<Video> listVideo) {
            List<Video> result = new List<Video>();
            try
            {
                foreach (var video in listVideo)
                {
                    if (IsLiveVideo(video)) result.Add(video);
                }
            }
            catch { }
            result = result.Distinct().ToList();
            return result;

        }
        /// <summary>
        ///     trả lại live event
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<Event> SearchLiveStreamByKeyword(string keyword)
        {
            var listEvent = SearchEventByKeyword(keyword);
            var currentStream = new List<Event>();
            foreach (var item in listEvent)
            {
                if (isLive(item.EventID))
                {
                    currentStream.Add(item);
                }
            }
            return currentStream;
        }

        /// <summary>
        ///     Check does event Live or not
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool isLive(long? eventID)
        {
            var listVideo = GetEventVideo(eventID);
            foreach (var item in listVideo) {
                if (IsLiveVideo(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get Thumb Event List By List Event
        /// </summary>
        /// <param name="listEvent"></param>
        /// <returns></returns>
        public List<ViewThumbEventModel> GetThumbEventListByListEvent(List<Event> listEvent)
        {
            var listThumbEvent = new List<ViewThumbEventModel>();
            if (listEvent == null) return listThumbEvent;
            try
            {
                foreach (var item in listEvent)
                {
                    ViewThumbEventModel thumbEventModel = new ViewThumbEventModel();
                    thumbEventModel.evt = item;
                    thumbEventModel.numberLike = EventDatabaseHelper.Instance.CountLike(item.EventID);
                    thumbEventModel.numberDislike = EventDatabaseHelper.Instance.CountDisLike(item.EventID);
                    thumbEventModel.numberFollow = EventDatabaseHelper.Instance.CountFollowerOfEvent(item.EventID);
                    thumbEventModel.location = EventDatabaseHelper.Instance.GetEventLocation(item.EventID);
                    listThumbEvent.Add(thumbEventModel);
                }
            }
            catch { 
            
            }
            return listThumbEvent;
        }
        /// <summary>
        /// get all reports of an event
        /// </summary>
        /// <param name="userID"><param>
        /// <returns></returns>
        public List<Report> GetEventReport(long? eventID, int type=-1,long senderID=-1) {
            List<Report> result = new List<Report>();
            try {
                result = (from a in db.Reports where a.EventID == eventID select a).ToList();
                if (type != -1)
                {
                    result.RemoveAll(o => o.ReportStatus != type);

                }
                if (senderID != -1)
                {
                    result = result.FindAll(o => o.SenderID == senderID);
                }
                foreach (var item in result) {
                    db.Entry(item).Reload();
                }
            }
            catch { }
            return result;
        }

        /// <summary>
        ///     get Image by ID
        /// </summary>
        /// <param name="imageID"></param>
        /// <returns></returns>
        public Image GetImageByID(long? imageID)
        {   
            return db.Images.Find(imageID);
        }

        public Image GetAvatarEvent(string url, long? eventID)
        {
            Image result = (from a in db.Images
                where a.ImageLink == url
                join b in db.EventImages on a.ImageID equals b.ImageID
                where b.EventID == eventID
                select a).ToList()[0];
            return result;
        }
        /// <summary>
        /// Search all event by CategoryID
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public List<Event> SearchEventByCategoryID(long categoryID)
        {
            List<Event> listEvent = new List<Event>();
            try {
                listEvent = (from a in db.Events where a.CategoryID == categoryID select a).ToList();
            }
            catch { 
            }
            return listEvent;

        }


        /// <summary>
        /// add image to event
        /// </summary>
        /// <param name="image"></param>
        /// <param name="evtID"></param>
        /// <returns></returns>
        public bool AddImageToEvent(Image image, long evtID) {
            try
            {
                db.Images.Add(image);
                db.SaveChanges();
                EventImage evtImage = new EventImage { EventID=evtID,ImageID=image.ImageID};
                if (!IsEventOwnedByUser(evtID, image.UserID))
                {
                    evtImage.Approve = false;
                }
                else {
                    evtImage.Approve = true;
                }
                db.EventImages.Add(evtImage);
                db.SaveChanges();
                return true;
                
            }
            catch {
                return false;
            }
        }


        /// <summary>
        ///     Lọc event theo nhóm category từ list event
        /// </summary>
        /// <param name="listEvent"></param>
        /// <param name="listCategory"></param>
        /// <returns></returns>
        public List<Event> SearchByListCategory(List<Event> listEvent, long[] listCategory)
        {
            if (listCategory.Length == 0)
            {
                return listEvent;
            }
            if (listEvent.Count == 0)
            {
                return null;
            }

            var result = new List<Event>();
            for (var i = 0; i < listCategory.Length; i++)
            {
                var item = listEvent.FindAll(m => m.CategoryID == listCategory[i]);
                foreach (var evt in item)
                {
                    result.Add(evt);
                }
            }
            return result;
        }

        /// <summary>
        /// Get list new event
        /// </summary>
        /// <returns></returns>
        public List<Event> GetListNewEvent() { 
            List<Event> result= new List<Event>();
            try
            {
                 DateTime floorDateTime = DateTime.Today.Date - TimeSpan.FromDays(7);
                 result = (from a in db.Events where a.EventRegisterDate >= floorDateTime select a).OrderByDescending(o => o.EventRegisterDate).ToList();
                 
            }
            catch { }
            return result;
        }
        /// <summary>
        /// get list new event by User (result is all new event which that user is following by category)
        /// </summary>
        /// <returns></returns>
        public List<Event> GetListNewEventByUser (long userID){
            List<Event> result = new List<Event>();
            List<Event> newEvent = GetListNewEvent();
            try
            {
                long[] listCategoryID = UserDatabaseHelper.Instance.GetListFollowingCategoryByUser(userID).ToArray();
                result = SearchByListCategory(newEvent, listCategoryID);
            }
            catch { 
            }
            result = result.OrderByDescending(o => o.EventRegisterDate).ToList();
            if (result.Count < 5)
            {
                result.AddRange(newEvent.Take(5).ToList());
            }
            result=result.Distinct().ToList();
            return result;
        }
        /// <summary>
        /// select all approved image in list
        /// </summary>
        /// <param name="listImage"></param>
        /// <returns></returns>
       
        public List<ThumbEventHomePage> GetThumbEventHomepage(List<Event> ListEvent)
        {
            List<ThumbEventHomePage> listThumb = new List<ThumbEventHomePage>();
            if (ListEvent == null) {
                return null;
            }
            foreach (var item in ListEvent) {
                ThumbEventHomePage thumbevt = new ThumbEventHomePage();
                thumbevt.EventID = item.EventID;
                thumbevt.EventName = item.EventName; 
                thumbevt.avatar = GetImageByID(item.Avatar).ImageLink;
                thumbevt.StartDate = item.EventStartDate;
                thumbevt.IsVeried = item.IsVerified;
                if (item.EventEndDate != null) {
                    thumbevt.EndDate = item.EventEndDate;
                }
                thumbevt.listLocation = EventDatabaseHelper.Instance.GetEventLocation(item.EventID);
                listThumb.Add(thumbevt);
            }
            return listThumb;

        }
        /// <summary>
        /// Select public and avaiable event(avaiable is event not locked)
        /// </summary>
        /// <param name="listEvent"></param>
        /// <returns></returns>
        /// 
        public List<Event> RemoveLockedEventInList(List<Event> listEvent)
        {
            try
            {
                List<Event> result = new List<Event>();
                listEvent.RemoveAll(o => (o.Privacy != EventZoneConstants.publicEvent) || (o.Status != EventZoneConstants.Active));
                result = listEvent;
                return result;
            }
            catch {
                return listEvent;
            }
        }
        /// <summary>
        /// Take 50 hotest event
        /// </summary>
        /// <returns></returns>
        public List<Event> GetHotEvent() {
            List<Event> result = new List<Event>();
            try {
      
                result=(from a in db.Events join b in db.EventRanks on a.EventID equals b.EventId orderby b.Score descending select a).Take(5).ToList();
            }
            catch {
                result = (from a in db.Events select a).Take(5).ToList();
            }
            return result;
            
        }
        /// <summary>
        ///     dem so like cua event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public int CountLike(long eventID)
        {
            try
            {
                var countLike =
                    (from a in db.LikeDislikes where a.EventID == eventID && a.Type == EventZoneConstants.Like select a.LikeDislikeID)
                        .Count();
                return countLike;
            }
            catch { return 0;}
        }

        /// <summary>
        ///     dem so dislike cua event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public int CountDisLike(long eventID)
        {
            try
            {
                var disLike =
                    (from a in db.LikeDislikes where a.EventID == eventID && a.Type == EventZoneConstants.Dislike select a.LikeDislikeID)
                        .Count();
                return disLike;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        ///     Count number followers of an event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public int CountFollowerOfEvent(long eventID)
        {
            try
            {
                var NumberFollower = (from a in db.EventFollows where a.EventID == eventID select a.EventFollowID).ToList().Count;
                return NumberFollower;
            }
            catch {
                return 0;
            }
           
        }

        /// <summary>
        ///     Tim kiem Su kien xung quanh 1 dia diem, default ban kinh = 20km
        /// </summary>
        /// <param name="location"></param>
        /// <param name="distance"></param>
        /// <param name="listEvent"></param>
        /// <returns></returns>
        public List<Event> GetEventAroundLocation(Location seachlocation, double distance = 20,
            List<Event> listEvent = null)
        {
            if (listEvent == null)
            {
                return null;
            }
            var result = new List<Event>();
            foreach (var item in listEvent)
            {
                var eventLocation = Instance.GetEventLocation(item.EventID);
                foreach (var location in eventLocation)
                {
                    if (LocationHelpers.Instance.CalculateDistance(location, seachlocation) <= distance)
                    {
                        if (!result.Contains(item))
                            result.Add(item);
                    }
                }
            }
            return result;
        }
        public List<Event> GetLiveEventByListEvent(List<Event> listEvent) {
            List<Event> result= new List<Event>();
            if (listEvent != null) { 
                foreach(var item in listEvent){
                    if (isLive(item.EventID)) {
                        result.Add(item);
                    }  
                }
            }
            return result;
        
        }
        /// <summary>
        /// add comment to event
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="userID"></param>
        /// <param name="commentContent"></param>
        /// <returns></returns>
        public Comment AddCommentToEvent(long eventID, long userID, string commentContent)
        {

            Comment comment = new Comment();
            comment.EventID = eventID;
            comment.UserID = userID;
            comment.CommentContent = commentContent;
            comment.DateIssue = DateTime.Now;
            try
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return comment;
            }
            catch { return null; }
        }

        /// <summary>
        ///     get list comment of event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public List<Comment> GetListComment(long eventID)
        {
            try
            {
                var listComment = (from a in db.Comments where a.EventID == eventID select a).ToList();
                return listComment;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     get event in date range from event list
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="listEvent"></param>
        /// <returns></returns>
        public List<Event> GetEventInDateRange(DateTime startDate, DateTime endDate, List<Event> listEvent = null)
        {
            if (listEvent == null)
            {
                return null;
            }
            var result = new List<Event>();
            DateTime today = DateTime.Now;
            foreach (var item in listEvent)
            {
                if (item.EventStartDate.CompareTo(startDate) >= 0 && item.EventStartDate.CompareTo(endDate) <= 0)
                    result.Add(item);
            }

            return result;
        }
        /// <summary>
        /// Upload image to amazone
        /// </summary>
        /// <param name="file"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public Image UserAddImage(HttpPostedFileBase file, long userID)
        {
            User user = db.Users.Find(userID);
            if (user == null)
            {
                return null;
            }
            else {
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
                                if (!EventZoneUtility.FileUploadToS3("eventzone", pic, stream, true, s3Client)) { return null; };
                            Image image = new Image();
                            image.ImageLink = "https://s3-us-west-2.amazonaws.com/eventzone/" + pic;
                            image.UserID = user.UserID;
                            image.UploadDate = DateTime.Today;
                            try
                            {
                                db.Images.Add(image);
                                db.SaveChanges();
                                return image;
                            }
                            catch {
                            }
                        } 
                    }
                }
                catch
                {
                }
            }
            return null;
        }
        /// <summary>
        /// add new event to database
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public Event AddNewEvent(CreateEventModel model, HttpPostedFileBase file,long userid)
        {
            var newEvent = new Event();
            newEvent.EventName = model.Title;
            User user= UserDatabaseHelper.Instance.GetUserByID(userid);
            if (user != null && user.UserRoles == EventZoneConstants.Mod || user.UserRoles==EventZoneConstants.Admin) {
                newEvent.IsVerified = true;
            }
            newEvent.ChannelID = UserDatabaseHelper.Instance.GetUserChannel(userid).ChannelID;
            newEvent.EventStartDate = model.StartTime;
            newEvent.EventEndDate = model.EndTime;
            newEvent.EventDescription = model.Description;
            newEvent.EventRegisterDate = DateTime.Now;
            newEvent.View = 0;
            newEvent.CategoryID = model.CategoryID;
            newEvent.Privacy = model.Privacy;
            newEvent.Avatar = CommonDataHelpers.Instance.GetCategoryById(model.CategoryID).CategoryAvatar;
            Image newImage = UserAddImage(file,userid);
            if (newImage != null) newEvent.Avatar = newImage.ImageID;
            newEvent.EditBy = userid;
            newEvent.EditTime = DateTime.Now;
            newEvent.EditContent = null;

            newEvent.Status = EventZoneConstants.Active; 
            // insert Event to Database
            try
            {
                db.Events.Add(newEvent);
                db.SaveChanges();
                EventRank eventRank = new EventRank { EventId = newEvent.EventID, Score = 0 };
                db.EventRanks.Add(eventRank);
                db.SaveChanges();
            }
            catch {
                return null;
            }
            return newEvent;
        }

        public List<EventPlace> AddEventPlace(List<Location> listLocation, Event newEvent)
        {
            List<EventPlace> listEventPlaces = new List<EventPlace>();
            for (var i = 0; i < listLocation.Count; i++)
            {
                var newEventPlace = new EventPlace();
                newEventPlace.LocationID = listLocation[i].LocationID;
                newEventPlace.EventID = newEvent.EventID;
                db.EventPlaces.Add(newEventPlace);
                db.SaveChanges();
                listEventPlaces.Add(newEventPlace);
            }
            return listEventPlaces;
        }
        /// <summary>
        /// get Event appeal
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="type"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<Appeal> GetEventAppeal(long eventID, int type=-1)
        {
            List<Appeal> result = new List<Appeal>();
            try
            {
                result = (from a in db.Appeals where a.EventID == eventID select a).ToList();
                if (type != -1)
                {
                    result.RemoveAll(o => o.AppealStatus != type);

                }
                foreach (var item in result)
                {
                    db.Entry(item).Reload();
                }
            }
            catch { }
            return result;
            
        }
        /// <summary>
        /// get all appeal of event
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public List<Appeal> GetListAppealOfEvent(long eventID)
        {
            try
            {
                List<Appeal> result = (from a in db.Appeals where a.EventID == eventID select a).OrderByDescending(m => m.SendDate).ToList();
                return result;
            }
            catch
            {

            }
            return null;
        }

        public bool AddNewAppeal(Appeal newAppeal)
        {
            
            try {
                db.Appeals.Add(newAppeal);
                db.SaveChanges();
                return true;
            }
            catch { }
            return false;
            
        }
        /// <summary>
        /// get Live streaming from list
        /// </summary>
        /// <param name="listEvent"></param>
        /// <returns></returns>
        internal List<Event> GetLiveStreamingFromList(List<Event> listEvent)
        {
            if (listEvent == null||listEvent.Count==0)
            {
                return null;
            }
            List<Event> result = new List<Event>();
            try
            {
                for (int i = 0; i < listEvent.Count - 1; i++) { 
                    if(isLive(listEvent[i].EventID)){
                        result.Add(listEvent[i]);
                    }
                }
            }
            catch { }
            return result;
        }
        /// <summary>
        /// change event avatar
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool ChangeEventAvatar( long eventID, Image image)
        {
            try {
                Event evt = db.Events.Find(eventID);
                evt.Avatar = image.ImageID;
                db.Entry(evt).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch { }
            return false;
        }
    }

    /// <summary>
    ///     All function related to Location
    /// </summary>
    public class LocationHelpers : SingletonBase<LocationHelpers>
    {
        private readonly EventZoneEntities db;

        private LocationHelpers()
        {
            db = new EventZoneEntities();
        }
        /// <summary>
        ///     get all location in db
        /// </summary>
        /// <returns></returns>
        public List<Location> GetAllLocation()
        {
         
            return db.Locations.ToList();
        }

        public Location GetLocationByEventPlaceID(long eventPlaceID) {
            try
            {
                Location result = new Location();
                EventPlace evtPlace = db.EventPlaces.Find(eventPlaceID);
                if (evtPlace != null)
                {
                    result = db.Locations.Find(evtPlace.LocationID);
                    
                }
                return result;
            }
            catch {
                return null;
            }
        }

        public bool RemoveVideoByEventPlaceID(long? eventPlacesID)
        {
            try
            {
                var removedVideo = (from a in db.Videos
                    where a.EventPlaceID== eventPlacesID
                    select a).ToList();
                foreach (var item in removedVideo)
                {
                    db.Videos.Remove(item);
                }
                db.SaveChanges();
                return true;
            }
            catch 
            {
                return false;
            }
            }
        public bool RemoveLocationByEventLocationID(long? eventID, long? locationID)
        {
            try
            {

                EventPlace removedEventPlace =
                    (from a in db.EventPlaces where a.EventID == eventID && a.LocationID == locationID select a).ToList()
                        [0];
                Instance.RemoveVideoByEventPlaceID(removedEventPlace.EventPlaceID);
                db.EventPlaces.Remove(removedEventPlace);

                var checkLocationExisted = (from a in db.EventPlaces where a.LocationID == locationID select a).ToList();

                if (checkLocationExisted.Count == 0)
                {
                    Location removedLocation =
                        (from a in db.Locations where a.LocationID == locationID select a).ToList()[0];
                    db.Locations.Remove(removedLocation);
                }
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// add new Location from location list. If there is location in database dont add it
        /// </summary>
        /// <param name="locationList"></param>
        /// <returns></returns>
        /// 
        public List<Location> AddNewLocation(List<Location> locationList )
        {
            var Location = new List<Location>();
            //Search for duplicated location before adding new location to database
            for (var i = 0; i < locationList.Count; i++)
            {
                Location tmpLocation = locationList[i];
                long locationIdIndex =LocationHelpers.Instance.FindLocationByAllData(tmpLocation.Longitude,
                                        tmpLocation.Latitude,
                                        tmpLocation.LocationName);
                if (locationIdIndex == -1)
                {
                    Location newLocation = new Location();
                    newLocation.LocationName = tmpLocation.LocationName;
                    newLocation.Latitude = tmpLocation.Latitude;
                    newLocation.Longitude = tmpLocation.Longitude;
                    db.Locations.Add(newLocation);
                    db.SaveChanges();
                    locationIdIndex = LocationHelpers.Instance.FindLocationByAllData(tmpLocation.Longitude,
                        tmpLocation.Latitude,
                        tmpLocation.LocationName);
                }
                Location.Add(GetLocationById(locationIdIndex));
            }
            return Location;
        }
    
        /// <summary>
        ///     get Location by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Location GetLocationById(long id)
        {
            try
            {
                Location loc = db.Locations.Find(id);
                return loc;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Find Location by longtitude, latitude and Location name
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <param name="locationName"></param>
        /// <returns></returns>
        public long FindLocationByAllData(double longitude, double latitude, string locationName)
        {
            var listLocationID = (from a in db.Locations
                                where
                                    a.Latitude.Equals(latitude) && a.Longitude.Equals(longitude) && a.LocationName.Equals(locationName)
                                select a.LocationID).ToList();
            if (listLocationID.Count == 0)
                return -1;
            return listLocationID[0];
        }

        /// <summary>
        ///     Tinh khoang cach giua 2 location
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public double CalculateDistance(Location p1, Location p2)
        {
            // using formula in http://www.movable-type.co.uk/scripts/latlong.html
            double R = 6371; // distance of the earth in km
            var dLatitude = Radians(p1.Latitude - p2.Latitude); // different in Rad of latitude
            var dLongitude = Radians(p1.Longitude - p2.Longitude); // different in Rad of longitude

            var a = Math.Sin(dLatitude / 2) * Math.Sin(dLatitude / 2) +
                    Math.Cos(Radians(p1.Latitude)) * Math.Cos(Radians(p2.Latitude))
                    * Math.Sin(dLongitude / 2) * Math.Sin(dLongitude / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return R * c;
        }

        private double Radians(double x)
        {
            return x * Math.PI / 180.0;
        }

        public List<EventPlace> GetAllEventPlace()
        {
            List<EventPlace> listeventPlace = db.EventPlaces.ToList();
            return listeventPlace;
        }

        public long GetEventPlacesID(long EventID, long LocationID)
        {
            EventPlace eventPlaces = (from a in db.EventPlaces
                                      where a.LocationID == LocationID && a.EventID == EventID
                                      select a).ToList()[0];
            return eventPlaces.EventPlaceID;
        }

        internal List<Location> RemovePlaceByDistance(List<Location> listPlace, double distance)
        {

            for (int i = listPlace.Count-1; i > 1; i--) {
             
                for (int j = i-1; j >=0 ; j--)
                {
                    if (j == -1) {
                        continue;
                    }
                    if (CalculateDistance(listPlace[i], listPlace[j]) < distance) {
                        listPlace.Remove(listPlace[j]);
                        i--;
                    }
                }
            }
            return listPlace;
        }
    }

    public class CommonDataHelpers : SingletonBase<CommonDataHelpers>
    {
        private readonly EventZoneEntities db;

        private CommonDataHelpers()
        {
            db = new EventZoneEntities();
        }
        /// <summary>
        /// get all category in database
        /// </summary>
        /// <returns></returns>
        public List<Category> GetAllCategory()
        {
            try
            {
                List<Category> listCate = db.Categories.ToList();
                return listCate;
            }
            catch { }
            return null;
        }
        public List<ReportDefine> GetAllReportType() {
            try
            {
                List<ReportDefine> listReport = db.ReportDefines.ToList();
                return listReport;
            }
            catch {
                return null;
            }

        }

        /// <summary>
        /// get category by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Category GetCategoryById(long id) {
            try
            {
                Category cate = db.Categories.Find(id);
                return cate;
            }
            catch { }
            return null;
        }
        /// <summary>
        /// count new event by category(new event is event that be defined as event is created in recent 7 days)
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public int CountNewEventByCategory(long categoryID)
        {
            int count = 0;
            DateTime floorDateTime = DateTime.Today.Date - TimeSpan.FromDays(7);
            count = (from a in db.Events where a.CategoryID == categoryID && (floorDateTime <= a.EventRegisterDate) select a.EventID).Count();
            return count;
        }
        /// <summary>
        ///  count live event by category(which event has streaming)
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public int CountLiveEventByCategory(long categoryID)
        {
            int count = 0;
            var listEventID = (from a in db.Events where a.CategoryID == categoryID select a.EventID).ToList();
            try
            {
                foreach (var item in listEventID)
                {
                    if (EventDatabaseHelper.Instance.isLive(item))
                    {
                        count = count + 1;
                    }
                }
            }
            catch (Exception)
            {
                return count;
            }
            return count;
        }

        public int CountFollowerByCategory(long categoryID)
        {
            var numberFollower = (from a in db.CategoryFollows where a.CategoryID == categoryID select a.CategoryFollowID).ToList().Count;
            return numberFollower;
        }

        public List<Video> GetVideoInLocation(List<Video> listVideo, Location location) {
            List<Video> result = new List<Video>();
            try {
                List<EventPlace> listEventPlace = new List<EventPlace>();
                foreach (var video in listVideo) {
                    var eventPlace = (from a in db.EventPlaces where a.EventPlaceID == video.EventPlaceID select a).ToList()[0];
                    if (eventPlace != null) {
                        listEventPlace.Add(eventPlace);
                    }
                }
                foreach (var item in listEventPlace) {
                    if (item.LocationID == location.LocationID) {
                        foreach (var video in listVideo) {
                            if (video.EventPlaceID == item.EventPlaceID) {
                                result.Add(video);
                            }
                        } 
                    }
                }
            }
            catch { }
            result=result.Distinct().ToList();
            return result;
        }
        public int CountNumberReport() {
            int result = 0;
            try {
                result = (from a in db.Reports select a.ReportID).ToList().Count;
            }
            catch {
            } 
            return result;
        }
        public int CountUnHandleReport()
        {
            int result = 0;
            try
            {
                result = (from a in db.Reports where a.ReportStatus == EventZoneConstants.Pending select a.ReportID).ToList().Count();
            }
            catch { 
            }
            return result;
        }

        public List<Report> GetAllReport()
        {
            try {
                List<Report> result = db.Reports.OrderByDescending(model=>model.ReportDate).ToList();
                return result;
            }
            catch { }
            return null;
        }
    }
    public class AdminDataHelpers : SingletonBase<AdminDataHelpers> { 
        private static EventZoneEntities db;
        /// <summary>
        /// constructor
        /// </summary>
        private AdminDataHelpers()
        {
            db = new EventZoneEntities();
        }
        public User FindAdmin(string userName, string password) {
            User admin = UserDatabaseHelper.Instance.GetUserByAccount(userName,password);
            if (admin != null) {
                if (admin.UserRoles == EventZoneConstants.Admin||admin.UserRoles==EventZoneConstants.RootAdmin)
                {
                    return admin;
                }
            }
            return null;
        }
        /// <summary>
        /// Admin lock an event
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="EventID"></param>
        /// <returns></returns>
        public bool LockEvent(long adminID, long eventID, string reason) {
            try {
                User user = EventDatabaseHelper.Instance.GetAuthorEvent(eventID);
                TrackingAction newAction = new TrackingAction
                {
                    SenderID = adminID,
                    SenderType = UserDatabaseHelper.Instance.GetUserByID(adminID).UserRoles,
                    ReceiverID = user.UserID,
                    ReceiverType = user.UserRoles,
                    ActionID = EventZoneConstants.LockEvent,
                    ActionTime = DateTime.Now
                };
                Event evt = db.Events.Find(eventID);
                evt.Status = EventZoneConstants.Lock;
                evt.EditBy = adminID;
                evt.EditContent = reason;
                evt.EditTime = DateTime.Now;
                db.Entry(evt).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(evt).Reload();
                
                db.TrackingActions.Add(newAction);
                db.SaveChanges();
                return true;
            }
            catch {
            }
            return false;
        }
        /// <summary>
        /// admin unlock Event
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public bool UnlockEvent(long adminID, long eventID)
        {
            try
            {
                User user = EventDatabaseHelper.Instance.GetAuthorEvent(eventID);
                TrackingAction newAction = new TrackingAction
                {
                    SenderID = adminID,
                    SenderType = UserDatabaseHelper.Instance.GetUserByID(adminID).UserRoles,
                    ReceiverID = user.UserID,
                    ReceiverType = user.UserRoles,
                    ActionID = EventZoneConstants.UnlockEvent,
                    ActionTime = DateTime.Now
                };
                Event evt = db.Events.Find(eventID);
                evt.Status = EventZoneConstants.Active;
                evt.EditBy = adminID;
                evt.EditContent = "";
                evt.EditTime = DateTime.Now;
                db.Entry(evt).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(evt).Reload();
                
                db.TrackingActions.Add(newAction);
                db.SaveChanges();
                return true;
            }
            catch
            {
            }
            return false;
        }
        /// <summary>
        /// Admin Lock User
        /// </summary>
        /// <param name="adminID"></param>
        /// <returns></returns>
        public bool LockUser(long adminID,long userID) {
            try
            {
                TrackingAction newAction = new TrackingAction
                {
                    SenderID = adminID,
                    SenderType = UserDatabaseHelper.Instance.GetUserByID(adminID).UserRoles,
                    ReceiverID = userID,
                    ReceiverType = UserDatabaseHelper.Instance.GetUserByID(userID).UserRoles,
                    ActionID = EventZoneConstants.LockEvent,
                    ActionTime = DateTime.Now
                };
                User user = db.Users.Find(userID);
                user.AccountStatus = EventZoneConstants.Lock;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
                
                db.TrackingActions.Add(newAction);
                db.SaveChanges();
                return true;
            }
            catch
            {
            }
            return false;
        }
        public bool UnlockUser(long adminID, long userID)
        {
            try
            {
                TrackingAction newAction = new TrackingAction
                {
                    SenderID = adminID,
                    SenderType = UserDatabaseHelper.Instance.GetUserByID(adminID).UserRoles,
                    ReceiverID = userID,
                    ReceiverType = UserDatabaseHelper.Instance.GetUserByID(userID).UserRoles,
                    ActionID = EventZoneConstants.UnLockUser,
                    ActionTime = DateTime.Now
                };
                User user = db.Users.Find(userID);
                user.AccountStatus = EventZoneConstants.Active;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
                db.TrackingActions.Add(newAction);
                db.SaveChanges();
                return true;
            }
            catch
            {
            }
            return false;
        }

        public bool ChangeUserEmail(long adminID, long userID, string email)
        {
            try
            {
                TrackingAction newAction = new TrackingAction
                {
                    SenderID = adminID,
                    SenderType = UserDatabaseHelper.Instance.GetUserByID(adminID).UserRoles,
                    ReceiverID = userID,
                    ReceiverType = UserDatabaseHelper.Instance.GetUserByID(userID).UserRoles,
                    ActionID = EventZoneConstants.ChangeUserEmail,
                    ActionTime = DateTime.Now
                };
                User user = db.Users.Find(userID);
                user.UserEmail = email;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
                db.TrackingActions.Add(newAction);
                db.SaveChanges();
                return true;
            }
            catch
            {
            }
            return false;
        }
        /// <summary>
        /// set user to mod
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool SetMod(long adminID, long userID)
        {
            try
            {
                TrackingAction newAction = new TrackingAction
                {
                    SenderID = adminID,
                    SenderType = UserDatabaseHelper.Instance.GetUserByID(adminID).UserRoles,
                    ReceiverID = userID,
                    ReceiverType = UserDatabaseHelper.Instance.GetUserByID(userID).UserRoles,
                    ActionID = EventZoneConstants.SetMod,
                    ActionTime = DateTime.Now
                };
                User user = db.Users.Find(userID);
                user.UserRoles = EventZoneConstants.Mod;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
               
                db.TrackingActions.Add(newAction);
                db.SaveChanges();
                return true;
            }
            catch
            {
            }
            return false;
        }
        /// <summary>
        /// set mod to user
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool UnSetMod(long adminID, long userID)
        {
            try
            {
                TrackingAction newAction = new TrackingAction
                {
                    SenderID = adminID,
                    SenderType = UserDatabaseHelper.Instance.GetUserByID(adminID).UserRoles,
                    ReceiverID = userID,
                    ReceiverType = UserDatabaseHelper.Instance.GetUserByID(userID).UserRoles,
                    ActionID = EventZoneConstants.UnSetMod,
                    ActionTime = DateTime.Now
                };
                User user = db.Users.Find(userID);
                user.UserRoles = EventZoneConstants.User;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
               
                db.TrackingActions.Add(newAction);
                db.SaveChanges();
                return true;
            }
            catch
            {
            }
            return false;
        }
        /// <summary>
        /// set user to admin, only root admin can use this feature
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool SetAdmin(long adminID, long userID)
        {
            try
            {
                TrackingAction newAction = new TrackingAction
                {
                    SenderID = adminID,
                    SenderType = EventZoneConstants.RootAdmin,
                    ReceiverID = userID,
                    ReceiverType = UserDatabaseHelper.Instance.GetUserByID(userID).UserRoles,
                    ActionID = EventZoneConstants.SetAdmin,
                    ActionTime = DateTime.Now
                };
                User user = db.Users.Find(userID);
                user.UserRoles = EventZoneConstants.Admin;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
                
                db.TrackingActions.Add(newAction);
                db.SaveChanges();
                return true;
            }
            catch
            {
            }
            return false;
        }
        /// <summary>
        /// set admin to user, only root admin can use this feature
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool UnSetAdmin(long adminID, long userID)
        {
            try
            {
                TrackingAction newAction = new TrackingAction
                {
                    SenderID = adminID,
                    SenderType = EventZoneConstants.RootAdmin,
                    ReceiverID = userID,
                    ReceiverType = UserDatabaseHelper.Instance.GetUserByID(userID).UserRoles,
                    ActionID = EventZoneConstants.UnSetAdmin,
                    ActionTime = DateTime.Now
                };
                User user = db.Users.Find(userID);
                user.UserRoles = EventZoneConstants.User;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Entry(user).Reload();
                
                db.TrackingActions.Add(newAction);
                db.SaveChanges();
                return true;
            }
            catch
            {
            }
            return false;
        }
        /// <summary>
        /// admin approve report
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="reportID"></param>
        /// <returns></returns>
        public Report ApproveReport(long adminID, long reportID)
        {
            try { 
               Report currReport= db.Reports.Find(reportID);
               Event evt = (from a in db.Events where a.EventID==currReport.EventID select a).ToList()[0];
               var some = db.Reports.Where(x => x.EventID == evt.EventID).ToList();
               some.ForEach(a => a.ReportStatus = EventZoneConstants.Closed);
               db.SaveChanges();
               currReport.ReportStatus = EventZoneConstants.Approved;
               currReport.HandleDate = DateTime.Now;
               currReport.HandleBy = adminID;
               db.Entry(currReport).State = EntityState.Modified;
               db.SaveChanges();
               evt.Status = EventZoneConstants.Lock;
               db.Entry(evt).State = EntityState.Modified;
               db.SaveChanges();

               return currReport;
            }   
            catch { }
            return null;
        }

        public Report RejectReport(long adminID, long reportID)
        {
            try
            {
                Report currReport = db.Reports.Find(reportID);
                currReport.ReportStatus = EventZoneConstants.Rejected;
                currReport.HandleDate = DateTime.Now;
                currReport.HandleBy = adminID;
                db.Entry(currReport).State = EntityState.Modified;
                db.SaveChanges();
                return currReport;
            }
            catch { }
            return null;
        }
        public Appeal ApproveAppeal(long adminID, long appealID)
        {
            try
            {
                Appeal appeal = db.Appeals.Find(appealID);
                Event evt = (from a in db.Events where a.EventID == appeal.EventID select a).ToList()[0];
                appeal.AppealStatus = EventZoneConstants.Approved;
                appeal.SendDate = DateTime.Today;
                appeal.HandleBy = adminID;
                db.Entry(appeal).State = EntityState.Modified;
                db.SaveChanges();
                evt.Status = EventZoneConstants.Active;
                db.Entry(evt).State = EntityState.Modified;
                db.SaveChanges();

                return appeal;
            }
            catch { }
            return null;
        }


        public Appeal RejectAppeal(long adminID, long appealID)
        {
            try
            {
                Appeal appeal = db.Appeals.Find(appealID);
                Event evt = (from a in db.Events where a.EventID == appeal.EventID select a).ToList()[0];
                appeal.AppealStatus = EventZoneConstants.Rejected;
                appeal.SendDate = DateTime.Today;
                appeal.HandleBy = adminID;
                db.Entry(appeal).State = EntityState.Modified;
                db.SaveChanges();
                evt.Status = EventZoneConstants.Lock;
                db.Entry(evt).State = EntityState.Modified;
                db.SaveChanges();
                return appeal;
            }
            catch { }
            return null;
        }

        public bool AddUser(User user)
        {
            try {
                db.Users.Add(user);
                db.SaveChanges();
                return true;
            }catch{
            }
            return false;
        }
    }
    /// <summary>
    /// All function related to statistic
    /// </summary>
    public class StatisticDataHelpers : SingletonBase<StatisticDataHelpers>
    {
        private readonly EventZoneEntities db;

        private StatisticDataHelpers()
        {
            db = new EventZoneEntities();
        }
        public Dictionary<string,int> GetEventCount()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            //result.Add("Total",db.Events.Count());
            var eachCategoryCount = (from b in db.Events
                group b by b.CategoryID
                into g
                select new {CategoryID = g.Key, Count = g.Count()}).ToList();
            
            foreach (var item in eachCategoryCount)
            {
                result.Add(CommonDataHelpers.Instance.GetCategoryById(item.CategoryID).CategoryName,item.Count);
            }
            return result;
        }

        public Dictionary<string, int> GetEventByStatus()
        {
            Dictionary<string, int> result = new Dictionary<string, int>() { };
            var eachEventCount = (from b in db.Events
                                   group b by b.Status
                                       into g
                                       select new { EventStatus = g.Key, Count = g.Count() }).ToList();
            foreach (var item in eachEventCount)
            {
                if (item.EventStatus)
                {
                    result.Add("Active", item.Count);
                }
                if (!item.EventStatus)
                {
                    result.Add("Locked", item.Count);
                }
            }
            return result;
        } 

        public Dictionary<string, List<int>> GetEventCreatedEachMonth()
        {
            Dictionary<string, List<int>> result = new Dictionary<string, List<int>>();
            List<Category> categories = CommonDataHelpers.Instance.GetAllCategory();
            foreach (var item in categories)
            {
                List<int> data = new List<int> {};
                var EventCreatedEachMonth = (from b in db.Events
                    where b.CategoryID == item.CategoryID
                    group b by b.EventStartDate.Month
                    into g
                    orderby g.Key ascending
                    select new {Month = g.Key, Count = g.Count()}).ToList();
                for (int i = 1; i < 13; i++)
                {
                    bool checkValidMonth = false;
                    int getData=0;
                    for (int j = 0; j < EventCreatedEachMonth.Count; j++)
                    {
                        if (i == EventCreatedEachMonth[j].Month)
                        {
                            checkValidMonth = true;
                            getData = EventCreatedEachMonth[j].Count;
                        }
                    }
                    if (checkValidMonth)
                    {
                        data.Add(getData);
                    }
                    else
                    {
                        data.Add(0);
                    }
                        
                }
                result.Add(item.CategoryName,data);
            }
            return result;
        }

        public Dictionary<Event, long> GetTopTenEvent()
        {
            Dictionary<Event, long> result = new Dictionary<Event, long>() { };
            db.EventRanks.Load();
            var listEvent = (from a in db.EventRanks orderby a.Score descending select new{id=a.EventId,score=a.Score}).Take(10).ToList();
            foreach (var item in listEvent)
            {
                Event evt = db.Events.Find(item.id);
                if (evt != null)
                {
                    db.Entry(evt).Reload();
                    result.Add(evt,item.score);
                }
            }
            return result;
        }

        public Dictionary<Location, int> GetTopLocation()
        {
            Dictionary<Location, int> result = new Dictionary<Location, int>() { };
            var listLocation = (from a in db.EventPlaces
                group a by a.LocationID
                into g
                orderby g.Count() descending
                select new {LocationID = g.Key, Count = g.Count()}).ToList().Take(10);
            foreach (var item in listLocation)
            {
                result.Add(db.Locations.ToList().FindAll(i => i.LocationID == item.LocationID)[0], item.Count);
            }
            return result;
        }

        public Dictionary<User, int> GetTopTenCreatedUser()
        {
            Dictionary<User, int> result = new Dictionary<User, int>() {};
            var listUser = db.Users.ToList();
            foreach (var item in listUser)
            {
                result.Add(item,UserDatabaseHelper.Instance.CountOwnedEvent(item.UserID,false));
            }
            var tmp = (from p in result orderby p.Value descending select p).ToList().Take(10);
            result.Clear();
            foreach (var item in tmp)
            {
                result.Add(item.Key, item.Value);
            }
            return result;
        }

        public Dictionary<string,int> GenderRate()
        {
            var listUser = (from a in db.Users
                group a by a.Gender
                into g
                orderby g.Count() descending
                select new {Gender = g.Key, Count = g.Count()}).ToList();
            Dictionary<string,int> result = new Dictionary<string,int>(){};
            foreach (var item in listUser)
            {
                if (item.Gender == 0)
                {
                    result.Add("Male", item.Count);
                }
                else
                {
                    result.Add("Female", item.Count);
                }
            }
            return result;
        }

        public Dictionary<string, int> GetGroupAgeReport()
        {
            Dictionary<string, int> result = new Dictionary<string, int>() {};
            var rangeYear = new List<int>{0,15,30,45,60};
            var groupAge = db.Users.Where(p => p.UserDOB != null).ToList()
                .Select(p => new { UserId = p.UserID, Age = DateTime.Now.Year - p.UserDOB.Year })
                .GroupBy(p => (int)((p.Age - 16) / 10))
                .Select(g => new { AgeGroup = string.Format("{0} - {1}", g.Key * 10 + 16, g.Key * 10 + 25), Count = g.Count() }).ToList();
            foreach (var item in groupAge)
            {
                result.Add(item.AgeGroup,item.Count);
            }
            return result;

        }

        public string getReportTypeNameById(int reportID)
        {
            try
            {
                ReportDefine report = db.ReportDefines.Find(reportID);
                return report.ReportTypeName;
            }
            catch { }
            return null;
        }
        public Dictionary<string, int> GetReportByType()
        {
            Dictionary<string, int> result = new Dictionary<string, int>() {};
            var eachReportCount = (from b in db.Reports
                                     group b by b.ReportType
                                         into g
                                       select new { ReportTypeID = g.Key, Count = g.Count() }).ToList();
            foreach (var item in eachReportCount)
            {
                result.Add(getReportTypeNameById(item.ReportTypeID), item.Count);
            }
            return result;
        }

        public Dictionary<string, int> GetReportByStatus()
        {
            Dictionary<string, int> result = new Dictionary<string, int>() { };
            var eachReportCount = (from b in db.Reports
                                   group b by b.ReportStatus
                                       into g
                                       select new { ReportStatus = g.Key, Count = g.Count() }).ToList();
            foreach (var item in eachReportCount)
            {
                if (item.ReportStatus == 0)
                {
                    result.Add("Pending", item.Count);
                }
                if (item.ReportStatus == 1)
                {
                    result.Add("Approved", item.Count);
                }
                if (item.ReportStatus == 2)
                {
                    result.Add("Rejected", item.Count);
                }
                if (item.ReportStatus == 3)
                {
                    result.Add("Closed", item.Count);
                }
            }
            return result;
        }

        public Dictionary<string, int> GetAppealByStatus()
        {
            Dictionary<string, int> result = new Dictionary<string, int>() { };
            var eachReportCount = (from b in db.Appeals
                                   group b by b.AppealStatus
                                       into g
                                       select new { AppealStatus = g.Key, Count = g.Count() }).ToList();
            foreach (var item in eachReportCount)
            {
                if (item.AppealStatus == 0)
                {
                    result.Add("Pending", item.Count);
                }
                if (item.AppealStatus == 1)
                {
                    result.Add("Approved", item.Count);
                }
                if (item.AppealStatus == 2)
                {
                    result.Add("Rejected", item.Count);
                }
            }
            return result;
        }
    }

    public class NotificationDataHelpers : SingletonBase<NotificationDataHelpers> {
        private static EventZoneEntities db;
        private NotificationDataHelpers() {
            db = new EventZoneEntities();
        }
        /// <summary>
        /// Get User notification and descending sort by created date
        /// </summary>
        /// <param name="isRead"> isRead= true meaning select all notification, esle select only unread notification</param>
        /// <returns></returns>
        public List<NotificationChange> GetUserNotification(long userID, bool isRead= true) {
            List<NotificationChange> listNotification = new List<NotificationChange>();
            try {
               List<NotificationObject> listNotiObject= (from a in db.Notifications join b in db.NotificationObjects on a.ID equals b.NotificationID where a.UserID==userID select b).ToList();

               foreach (var item in listNotiObject)
               {

                   List<NotificationChange> list = (from a in db.NotificationChanges join b in db.NotificationObjects on a.NotificationObjectID equals b.ID where b.ID==item.ID select a).ToList();
                    listNotification.AddRange(list);
                }
               listNotification=listNotification.Distinct().ToList();

                if (!isRead) {
                    listNotification.RemoveAll(r => r.IsRead == true);
                }
                foreach (var item in listNotification) {
                    db.Entry(item).Reload();
                }
                return listNotification;
            }
            catch { }
            return listNotification;
        }
        /// <summary>
        /// Get notification type
        /// </summary>
        /// <returns></returns>
        public NotificationObject GetNotificationObjectByID(long? notificationObjectID)
        {
            NotificationObject result = new NotificationObject();
            try {
                result = db.NotificationObjects.Find(notificationObjectID);
            }
            catch { }
            return result;
        }
        public Notification GetNotifycationByUser(long userID)
        {
            try {
                Notification result = (from a in db.Notifications where a.UserID == userID select a).ToList()[0];
                return result;
            }
            catch { }
            return null;
        }
        public NotificationObject GetNotificationObjectByType(int type,long userID) {
            NotificationObject result = new NotificationObject();
            try {
                Notification noti= GetNotifycationByUser(userID);
                if(noti!=null){
                result = (from a in db.NotificationObjects where a.Type == type &&  a.NotificationID==noti.ID select a).ToList()[0];
                }
            }
            catch { }
            return result;
        }
        /// <summary>
        /// count total notification of user
        /// </summary>
        /// <returns></returns>
        public int CountTotalNewNotification(long userID) {

            int result = 0;
            try {
                List<NotificationChange> listNewNotification = GetUserNotification(userID, false);
                List<NotificationChange> listNotificationNewEvent = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.FollowingUserAddNewEvent,userID);
                List<NotificationChange> listNotificationNewFollower = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.NewFollower,userID);
                List<NotificationChange> listNotificationLockEvent = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.EventHasBeenLocked,userID);
                List<NotificationChange> listNotificationUnLockEvent = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.EventHasBeenUnLocked,userID);
                List<NotificationChange> listNotificationRequestUpImage = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.RequestUploadImage,userID);
                List<NotificationChange> listNotificationNewReport = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.ReportNotification,userID);
                List<NotificationChange> listNotificationNewComment = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.CommentNotification,userID);
                result = listNotificationNewEvent.Count + listNotificationLockEvent.Count + listNotificationUnLockEvent.Count;
                if (listNotificationNewFollower != null && listNotificationNewFollower.Count > 0)
                {
                    result = result + 1;
                }
                ///count number request upload image to event group by event and user
                result = result + listNotificationRequestUpImage.Distinct(new GroupByActorIdAndEventID()).ToList().Count;
                // count number comment group by event and user
                result = result + listNotificationNewComment.Distinct(new GroupByActorIdAndEventID()).ToList().Count;
                // count number report group by event
                result = result + listNotificationNewReport.Distinct(new GroupByEventIDNotification()).ToList().Count;

            }
            catch { }
            return result;
        }
        /// <summary>
        /// get notification by type from list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<NotificationChange> GetNotifyByUserAndType(List<NotificationChange> list, int type, long userID)
        {
            List<NotificationChange> result = list;
            
            try
            {
                long notificationObjectID = GetNotificationObjectByType(type,userID).ID;
                result = result.FindAll(o => o.NotificationObjectID == notificationObjectID);
            }
            catch { }
            return result;
        }
        //public static int FollowingUserAddNewEvent = 1;
        //public static int CommentNotification = 2;
        //public static int ReportNotification = 3;
        //public static int NewFollower = 4;
        //public static int RequestUploadImaeg = 5;
        //public static int EventHasBeenLocked = 6;
        //public static int EventHasBeenUnLocked = 7;

        public bool AddNotification(long receiverID, int type, long? actorID, long? eventID) {
            try
            {
                List<Notification> listNoti= (from a in db.Notifications where a.UserID == receiverID select a).ToList();
                Notification noti = new Notification();
                if (listNoti == null ||listNoti.Count==0)
                {
                    noti = new Notification { UserID = receiverID };
                    db.Notifications.Add(noti);
                    db.SaveChanges();
                }
                else
                {
                    noti = listNoti[0];
                }
                List<NotificationObject> listNotiObject = (from a in db.NotificationObjects where a.NotificationID == noti.ID && a.Type == type select a).ToList();
                NotificationObject notiobject = new NotificationObject();
                if (listNotiObject == null || listNotiObject.Count == 0)
                {
                    notiobject = new NotificationObject { NotificationID = noti.ID, Type = type };
                    db.NotificationObjects.Add(notiobject);
                    db.SaveChanges();
                }
                else {
                    notiobject = listNotiObject[0];
                }
                NotificationChange newNotify = new NotificationChange();
                newNotify.CreatedDate = DateTime.Now;
                newNotify.IsRead = false;
                newNotify.NotificationObjectID = notiobject.ID;
                if (actorID != null)
                {
                    newNotify.ActorID = actorID;
                }
                if (eventID != null)
                {
                    newNotify.EventID = eventID;
                }
                db.NotificationChanges.Add(newNotify);
                db.SaveChanges();
                return true;
            }
            catch { }
            return false;               

        }
        public List<NotificationChange> RemoveDupilicateNotify (List<NotificationChange> listNewNotification,long userID){
            List<NotificationChange> result = listNewNotification;

            try {
                List<NotificationChange> listNotificationNewEvent = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.FollowingUserAddNewEvent, userID);
                List<NotificationChange> listNotificationNewFollower = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.NewFollower, userID);
                List<NotificationChange> listNotificationLockEvent = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.EventHasBeenLocked, userID);
                List<NotificationChange> listNotificationUnLockEvent = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.EventHasBeenUnLocked, userID);
                List<NotificationChange> listNotificationRequestUpImage = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.RequestUploadImage, userID);
                List<NotificationChange> listNotificationNewReport = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.ReportNotification, userID);
                List<NotificationChange> listNotificationNewComment = GetNotifyByUserAndType(listNewNotification, EventZoneConstants.CommentNotification, userID);
                result = listNotificationNewEvent;

                if (listNotificationNewFollower != null && listNotificationNewFollower.Count > 0)
                {
                    result.Add(listNotificationNewFollower[0]);
                }
                result.AddRange(listNotificationLockEvent);
                result.AddRange(listNotificationUnLockEvent);
                listNotificationRequestUpImage=listNotificationRequestUpImage.Distinct(new GroupByActorIdAndEventID()).ToList();
                result.AddRange(listNotificationRequestUpImage);
                listNotificationNewReport = listNotificationNewReport.Distinct(new GroupByEventIDNotification()).ToList();
                result.AddRange(listNotificationNewReport);
                listNotificationNewComment = listNotificationNewComment.Distinct(new GroupByActorIdAndEventID()).ToList();
                result.AddRange(listNotificationNewComment);
                result= result.OrderBy(o => o.CreatedDate).ToList();
            }
            catch { }
            return result;  
        }

        /// <summary>
        ///  get new user following notification
        /// </summary>
        /// <param name="followerID"></param>
        /// <param name="followingID"></param>
        /// <param name="isRead"></param>
        /// <returns></returns>
        public List<User> GetNewUserFollowNotify(long? followerID, long followingID, bool isRead) {
            List<NotificationChange> listNoti = GetUserNotification(followingID, isRead);
            List<NotificationChange> listNewFollowerNotificaton= GetNotifyByUserAndType(listNoti, EventZoneConstants.NewFollower, followingID);
            List<User> result = new List<User>();
            try {
                if (listNewFollowerNotificaton != null && listNewFollowerNotificaton.Count > 0)
                {
                    foreach (var item in listNewFollowerNotificaton)
                    {
                        User user = db.Users.Find(item.ActorID);
                        result.Add(user);
                    }
                }
                result= result.Distinct().ToList();
                return result;
            }
            catch { }
            return null;
    

        }

        /// <summary>
        /// Get new user report to event
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="receiverID"></param>
        /// <param name="actorID"></param>
        /// <returns></returns>
        public List<User> GetNewUserReportToEvent(long eventID,long receiverID, bool isRead){

            List<NotificationChange> listNoti = GetUserNotification(receiverID,isRead);
            List<NotificationChange> listNotificationNewReport = GetNotifyByUserAndType(listNoti, EventZoneConstants.ReportNotification, receiverID);
            List<User> result=new List<User>();
            listNotificationNewReport = listNotificationNewReport.Distinct(new GroupByEventIDNotification()).ToList();
            foreach (var item in listNotificationNewReport)
            {
                User user = db.Users.Find(item.ActorID);
                result.Add(user);
            }
            result=result.Distinct().ToList();
            return result;
        } 
        /// <summary>
        /// send notify new event is created to all follower
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="EventID"></param>
        public void SendNotifyNewEventToFollower(long actorID,long eventID)
        {
            List<User> listFollowingUser = UserDatabaseHelper.Instance.GetListFollowerOfUser(actorID);
            foreach (var item in listFollowingUser) {
                AddNotification(item.UserID, EventZoneConstants.FollowingUserAddNewEvent, actorID, eventID);
            }
        }
        /// <summary>
        /// send notify that recever has new follower
        /// </summary>
        public void SendNotiNewFollower(long receiverID, long actorID)
        {
            List<NotificationChange> listNotiFollow = (from a in db.NotificationChanges join b in db.NotificationObjects on a.NotificationObjectID equals b.ID where a.ActorID == actorID &&  b.Type== EventZoneConstants.NewFollower select a).ToList();
            if (listNotiFollow != null && listNotiFollow.Count > 0)
            {
                try
                {
                    NotificationChange notiChange = listNotiFollow[0];
                    notiChange.CreatedDate = DateTime.Now;
                    notiChange.IsRead = false;
                    db.Entry(notiChange).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch { }
               
            }
            else {
                AddNotification(receiverID, EventZoneConstants.NewFollower, actorID, null);
            }
           
        }
        public void SendNotiNewReport(long receiverID, long actorID, long eventID) {
            AddNotification(receiverID, EventZoneConstants.ReportNotification, actorID, eventID);
        }
        public void SendNotiLockEvent(long receiverID, long actorID, long eventID) {
            AddNotification(receiverID, EventZoneConstants.EventHasBeenLocked, actorID, eventID);
        }

        public void SendNotiUnLockEvent(long receiverID, long actorID, long eventID)
        {
            
            AddNotification(receiverID, EventZoneConstants.EventHasBeenUnLocked, actorID, eventID);
        }

        public void SendNotyNewComment(long actorID, long eventID)
        {
            List<User> listUser = EventDatabaseHelper.Instance.GetUniqueUserComment(eventID);
            User user = UserDatabaseHelper.Instance.GetUserByID(actorID);
            if (user != null) {
                listUser.RemoveAll(o=>o.UserID==actorID);
            }
            foreach (var item in listUser) {
                AddNotification(item.UserID, EventZoneConstants.CommentNotification, actorID, eventID);
            }

        }
    }
}
