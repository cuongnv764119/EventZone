using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EventZone.Helpers;
using EventZone.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Ajax.Utilities;
using Video = EventZone.Models.Video;
using Amazon.S3;
using Newtonsoft.Json;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Mvc;

namespace EventZone.Controllers
{
    public class EventController : Controller
    {
        private readonly EventZoneEntities db = new EventZoneEntities();

        //
        // GET: /Event
        // GET: Event
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateEvent(CreateEventModel model, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                List<Location> locationList = model.Location;
                locationList.RemoveAll(o => o.LocationName.Equals("Remove Location"));
                //add location to database
                var listLocation = LocationHelpers.Instance.AddNewLocation(locationList);

                //Adding new event with given infomation to database
                var newEvent = EventDatabaseHelper.Instance.AddNewEvent(model, file, UserHelpers.GetCurrentUser(Session).UserID);

                //Add event place to database
                var listEventPlaces = EventDatabaseHelper.Instance.AddEventPlace(listLocation, newEvent);

                LiveStreamingModel liveModel = new LiveStreamingModel { eventID = newEvent.EventID, Title = newEvent.EventName };
                TempData["LiveModel"] = liveModel;
                HttpCookie newEventID = new HttpCookie("CreateEventID");
                newEventID.Value = newEvent.EventID.ToString();
                newEventID.Expires=DateTime.Now.AddDays(1);
                Response.Cookies.Add(newEventID);
                NotificationDataHelpers.Instance.SendNotifyNewEventToFollower(UserHelpers.GetCurrentUser(Session).UserID, newEvent.EventID);
                return RedirectToAction("Details", "Event",new{id=newEvent.EventID});
            }
            // If we got this far, something failed, redisplay form
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryId", "CategoryName");
            TempData["errorTitle"] = "Error";
            TempData["errorMessage"] = "Please select location from suggestion!"; 
            return View("Create", model);
        }
        /// <summary>
        /// return form edit event 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditEvent(ViewDetailEventModel model)
        {
            EditViewModel editModel = new EditViewModel();
            if (model.eventDescription.IsNullOrWhiteSpace())
            {
                editModel.Description = "";
            }
            else
            {
                editModel.Description = model.eventDescription;
            }
            editModel.EndTime = model.EndTime;
            editModel.Privacy = model.Privacy;
            editModel.StartTime = model.StartTime;
            editModel.Title = model.eventName;
            editModel.eventID = model.eventId;
            editModel.Location = model.eventLocation;
            return PartialView(editModel);
        }
        /// <summary>
        /// Edit event post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditEventPost(EditViewModel model)
        {
            if (EventDatabaseHelper.Instance.UpdateEvent(model))
            {
                return RedirectToAction("Details", "Event", new {id = model.eventID});
            }
            else
            {
                TempData["EditError"] = "Something Wrong Happened... Try Again Later";
                return RedirectToAction("Details", "Event", new { id = model.eventID });
            }
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult AddLiveStream(LiveStreamingModel model)
        {
            List<EventPlace> listPlace= new List<EventPlace>();
            TempData["errorTitle"] = TempData["errorTitle"];
            TempData["errorMessage"] = TempData["errorMessage"];
            if (Request.Cookies["CreateEventID"] != null)
            {
                model.eventID = long.Parse(Request.Cookies["CreateEventID"].Value);
                model.Title = EventDatabaseHelper.Instance.GetEventByID(model.eventID).EventName;

            }
            listPlace = EventDatabaseHelper.Instance.GetEventPlaceByEvent(model.eventID);
            TempData["EventPlace"] = listPlace;           
            return PartialView(model);
        }

        public async Task<ActionResult> IndexAsync(LiveStreamingModel liveModel, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid) { 
                var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).
                AuthorizeAsync(cancellationToken);

                if (result.Credential != null)
                {
                    var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = result.Credential,
                        ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
                    });
                    LiveBroadcastSnippet broadcastSnippet = new LiveBroadcastSnippet();
                    broadcastSnippet.Title = liveModel.Title;
                    broadcastSnippet.ScheduledStartTime = liveModel.StartTimeYoutube.CompareTo(DateTime.Now) < 0 ? (DateTime.Now) : liveModel.StartTimeYoutube;
                    broadcastSnippet.ScheduledEndTime = liveModel.EndTimeYoutube;
                    // Set the broadcast's privacy status to "private". See:
                    // https://developers.google.com/youtube/v3/live/docs/liveBroadcasts#status.privacyStatus
                    LiveBroadcastStatus status = new LiveBroadcastStatus();
                    if (liveModel.PrivacyYoutube == EventZoneConstants.publicEvent)
                    {
                        status.PrivacyStatus = "public";
                    }
                    else if (liveModel.PrivacyYoutube == EventZoneConstants.unlistedEvent)
                    {
                        status.PrivacyStatus = "unlisted";
                    }
                    else { 
                        status.PrivacyStatus= "private";
                    }
                    
                    //Set LiveBroadcast
                    LiveBroadcast broadcast = new LiveBroadcast();
                    LiveBroadcast returnBroadcast = new LiveBroadcast();
                    broadcast.Kind = "youtube#liveBroadcast";
                    broadcast.Snippet = broadcastSnippet;
                    broadcast.Status = status;
                    LiveBroadcastsResource.InsertRequest liveBroadcastInsert = youtubeService.LiveBroadcasts.Insert(broadcast, "snippet,status");
                    try
                    {
                        returnBroadcast = liveBroadcastInsert.Execute();
                    }
                    catch (Exception ex){
                        TempData["errorTitle"] = "Error";
                        TempData["errorMessage"] = ex.Message;
                        result.Credential.RevokeTokenAsync(CancellationToken.None).Wait();
                        return RedirectToAction("AddLiveStream", "Event", new { liveModel = liveModel});
                    }
                    
                    //Set LiveStream Snippet
                    LiveStreamSnippet streamSnippet = new LiveStreamSnippet();
                    streamSnippet.Title = liveModel.Title + "Stream Title";
                    CdnSettings cdnSettings = new CdnSettings();
                    cdnSettings.Format = liveModel.Quality;
                    cdnSettings.IngestionType = "rtmp";

                    //Set LiveStream
                    LiveStream streamLive = new LiveStream();
                    streamLive.Kind = "youtube#liveStream";
                    streamLive.Snippet = streamSnippet;
                    streamLive.Cdn = cdnSettings;
                    LiveStream returnLiveStream = youtubeService.LiveStreams.Insert(streamLive, "snippet,cdn").Execute();
                    LiveBroadcastsResource.BindRequest liveBroadcastBind = youtubeService.LiveBroadcasts.Bind(returnBroadcast.Id, "id,contentDetails");
                    liveBroadcastBind.StreamId = returnLiveStream.Id;
                    try
                    {
                        returnBroadcast = liveBroadcastBind.Execute();
                    }
                    catch (Exception ex)
                    {
                        TempData["errorTitle"] = "Error";
                        TempData["errorMessage"] = ex.Message;
                        result.Credential.RevokeTokenAsync(CancellationToken.None).Wait();
                        return RedirectToAction("AddLiveStream", "Event", liveModel);
                    }
                    
                    //Return Value
                    String streamName = returnLiveStream.Cdn.IngestionInfo.StreamName;
                    String primaryServerUrl = returnLiveStream.Cdn.IngestionInfo.IngestionAddress;
                    String backupServerUrl = returnLiveStream.Cdn.IngestionInfo.BackupIngestionAddress;
                    String youtubeUrl = "https://www.youtube.com/watch?v=" + returnBroadcast.Id;
                    
                    //youtubeReturnModel model = new youtubeReturnModel { streamName = streamName, primaryServerUrl = primaryServerUrl,backupServerUrl=backupServerUrl,youtubeUrl=youtubeUrl };
                    Video video = new Video {  EventPlaceID = liveModel.EventPlaceID,
                                               VideoLink = youtubeUrl,
                                               PrimaryServer = primaryServerUrl,
                                               StartTime = liveModel.StartTimeYoutube,
                                               Privacy = liveModel.PrivacyYoutube,
                                               EndTime = liveModel.EndTimeYoutube,
                                               BackupServer = backupServerUrl,
                                               StreamName= streamName};
                    EventDatabaseHelper.Instance.AddVideo(video);
                    
                    HttpCookie newEventID = new HttpCookie("CreateEventID");
                    newEventID.Expires = DateTime.Now.AddDays(-1);
                    Request.Cookies.Add(newEventID);
                    return RedirectToAction("Details", "Event", new { id = EventDatabaseHelper.Instance.GetEventPlaceByID(liveModel.EventPlaceID).EventID });
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
            }
            TempData["errorTitle"] = "Error";
            TempData["errorMessage"] = "Invalid Input, please try again";
           
            return RedirectToAction("AddLiveStream", "Event", liveModel);
        }
        /// <summary>
        /// View detail of event 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(long? id)
        {
            if (id == null||
                EventDatabaseHelper.Instance.GetEventByID(id)==null||
                EventDatabaseHelper.Instance.GetEventByID(id).Status==EventZoneConstants.Lock||
                EventDatabaseHelper.Instance.GetEventByID(id).Privacy==EventZoneConstants.privateEvent)
            {
                TempData["errorTitle"] = "Failed to load event";
                TempData["errorMessage"] = "This event is not available! It may be locked or set private!";
                return RedirectToAction("Index", "Home");
            }
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

            Event evt = EventDatabaseHelper.Instance.GetEventByID(id);
            ViewDetailEventModel viewDetail = new ViewDetailEventModel();

            viewDetail.createdBy = EventDatabaseHelper.Instance.GetAuthorEvent(evt.EventID);
            viewDetail.eventId = evt.EventID;
            viewDetail.eventName = evt.EventName;

            viewDetail.eventAvatar = EventDatabaseHelper.Instance.GetImageByID(evt.Avatar).ImageLink;
            viewDetail.numberView = evt.View;
           
            viewDetail.eventDescription = evt.EventDescription;
            viewDetail.StartTime = evt.EventStartDate;
            viewDetail.EndTime = evt.EventEndDate;
            viewDetail.isOwningEvent = false;
            viewDetail.NumberLike = EventDatabaseHelper.Instance.CountLike(evt.EventID);
            viewDetail.NumberDisLike = EventDatabaseHelper.Instance.CountDisLike(evt.EventID);
            viewDetail.NumberFowllower = EventDatabaseHelper.Instance.CountFollowerOfEvent(evt.EventID);
            viewDetail.eventLocation = EventDatabaseHelper.Instance.GetEventLocation(evt.EventID);
            viewDetail.eventImage = EventDatabaseHelper.Instance.GetEventImage(evt.EventID);
            viewDetail.eventVideo = EventDatabaseHelper.Instance.GetEventVideo(evt.EventID);
            viewDetail.eventComment = EventDatabaseHelper.Instance.GetListComment(evt.EventID);
            viewDetail.Category = EventDatabaseHelper.Instance.GetEventCategory(evt.EventID);
            viewDetail.FindLike = new LikeDislike();
            viewDetail.FindLike.Type = EventZoneConstants.NotRate;
            viewDetail.FindLike.EventID = evt.EventID;
            LiveStreamingModel liveModel = new LiveStreamingModel { eventID = evt.EventID, Title = evt.EventName };
            TempData["LiveModel"] = liveModel;
            if (user != null)
            {
                viewDetail.isOwningEvent = EventDatabaseHelper.Instance.IsEventOwnedByUser(evt.EventID, user.UserID);
                viewDetail.FindLike = UserDatabaseHelper.Instance.FindLike(user.UserID, evt.EventID);
                if (viewDetail.FindLike == null)
                {
                    viewDetail.FindLike = new LikeDislike();
                    viewDetail.FindLike.Type = EventZoneConstants.NotRate;
                }
                viewDetail.isFollowing = UserDatabaseHelper.Instance.IsFollowingEvent(user.UserID, evt.EventID);
            }
            viewDetail.Privacy = evt.Privacy;
            if (TempData["EventDetailTask"] == null)
            {
                ViewData["EventDetailTask"] = "EventDetail";
                if (user == null || EventDatabaseHelper.Instance.GetAuthorEvent(evt.EventID).UserID != user.UserID)
                {
                    EventDatabaseHelper.Instance.AddViewEvent(evt.EventID);
                    viewDetail.numberView = evt.View;
                }
                return View(viewDetail);
            }
            else
            {
                ViewData["EventDetailTask"] = "EditEvent";
                return View(viewDetail);
            }
        }

        public ActionResult VideoAdd(String VideoURL, long LocationID, long EventID)
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
                    TempData["errorTitle"] = "Require Signin";
                    TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                    return RedirectToAction("Details", "Event", new { id = EventID });
                }
            }
            Video newVideo = new Video();
            newVideo.EventPlaceID = LocationHelpers.Instance.GetEventPlacesID(EventID, LocationID);
            newVideo.StartTime = DateTime.Now;
            newVideo.Privacy = 1;
            newVideo.VideoLink = VideoURL;
            if (EventDatabaseHelper.Instance.AddVideo(newVideo))
            {
                TempData["VideoAddError"] = null; // success
                
            }
            else
            {
                TempData["VideoAddError"] = "There is something wrong";
            }
            return RedirectToAction("Details", "Event",new {id=EventID});

        }
        public ActionResult ImageUpload(HttpPostedFileBase file, long eventID)
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
                    TempData["errorTitle"] = "Require Signin";
                    TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                    return RedirectToAction("Details", "Event", new { id = eventID });
                }
            }

            Image photo = new Image();
            if (file != null)
            {
                string[] whiteListedExt = { ".jpg", ".gif", ".png", ".tiff" };
                Stream stream = file.InputStream;
                string extension = Path.GetExtension(file.FileName);
                if (whiteListedExt.Contains(extension))
                {
                    string pic = Guid.NewGuid()+ eventID.ToString() + extension;
                    using (AmazonS3Client s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
                        EventZoneUtility.FileUploadToS3("eventzone", pic, stream, true, s3Client);
                    Image image = new Image();
                    image.ImageLink = "https://s3-us-west-2.amazonaws.com/eventzone/"+pic;
                    image.UserID = user.UserID;
                    image.UploadDate = DateTime.Today;
                  
                    if (EventDatabaseHelper.Instance.AddImageToEvent(image,eventID))
                    {
                        TempData["ImageUploadError"] = null; // success
                    }
                    TempData["ImageUploadError"] = "Something wrong.."; // success 
                }
                else
                {
                    TempData["ImageUploadError"] = "File is not supported! Please select an image file";
                }
            }
            else
            {
                TempData["ImageUploadError"] = "Your must select a file to upload";
            }
            return RedirectToAction("Details", "Event",new {id=eventID});
        }
        public ActionResult Comment(int eventID, string commentContent)
        {
            List<EventZone.Models.Comment> listComment = EventDatabaseHelper.Instance.GetListComment(eventID);
            CommentViewModel comment = new CommentViewModel { eventID = eventID, listComment = listComment };
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
                            return Json(new
                            {
                                state=0,
                                title="Locked User",
                                message = "Your account is locked! Please contact with our support",
                            });
                        }
                        UserHelpers.SetCurrentUser(Session, user);
                    }
                }
            }
            else if (user != null)
            {
               EventZone.Models.Comment newcmt= EventDatabaseHelper.Instance.AddCommentToEvent(eventID, user.UserID, commentContent);
                if (newcmt != null)
                {
                    NotificationDataHelpers.Instance.SendNotyNewComment(user.UserID,eventID);
                    string dataAppend = " <div class='d_each_event'>"
                        + "<div class='d_ee_ava_user'>"
                         + "   <div class='d_ee_ava'>"
                           + "    <img src=" + EventDatabaseHelper.Instance.GetImageByID(UserDatabaseHelper.Instance.GetUserByID(newcmt.UserID).Avartar).ImageLink + ">"
                            + "</div>"
                            + "<div class='d_ee_user'>"
                              + "<i>" + UserDatabaseHelper.Instance.GetUserDisplayName(newcmt.UserID) + "</i>"
                           + " </div>"
                           + " <div class='d_ee_time'>"
                           + " <i>" + newcmt.DateIssue + "</i>"
                           + " </div>"
                        + "</div>"
                        + "<div class='d_ee_content'>"
                        + newcmt.CommentContent
                        + "</div>"
                   + " </div>";

                    return Json(new
                    {
                        state = 1,
                        dataAppend = dataAppend
                    });
                };
            }
            return Json(new
            {
                state = 0,
                title = "Failed!",
                message = "Ops.. Somthing went wrong. Please try again later",
            });

        }
         public ActionResult EventImage(long? id) {
             List<Image> listImage = EventDatabaseHelper.Instance.GetEventImage(id);
             return PartialView("_EventImage", listImage);
         }
        [HttpPost]
         public ActionResult ImageDelete(long? imageID, long? eventID)
        {
            Image deletedImage = db.Images.Find(imageID);
            EventImage eventImage = (from a in db.EventImages where a.ImageID == imageID select a).ToList()[0];
            db.EventImages.Remove(eventImage);
            db.Images.Remove(deletedImage);
            db.SaveChanges();
            return RedirectToAction("Details", "Event", new { id = eventID });
        }

        [HttpPost]
        public ActionResult VideoDelete(long? videoID, long? eventID)
        {
            Video deletedVideo = db.Videos.Find(videoID);
            db.Videos.Remove(deletedVideo);
            db.SaveChanges();
            return RedirectToAction("Details", "Event", new { id = eventID });
        }
         public ActionResult UploadImageEvent(HttpPostedFileBase eventImage, string eventId) {
             long eventID = long.Parse(eventId);
             User user = UserHelpers.GetCurrentUser(Session);
             if (user == null)
             {
                 TempData["errorTitle"] = "Require Signin";
                 TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                 return RedirectToAction("Index", "Home");
             }
             else
             {
                 List<Image> listImage = EventDatabaseHelper.Instance.GetEventImage(eventID);
                 Image photo = new Image();
                 try
                 {
                     if (eventImage != null)
                     {
                         string[] whiteListedExt = { ".jpg", ".gif", ".png", ".tiff" };
                         Stream stream = eventImage.InputStream;
                         string extension = Path.GetExtension(eventImage.FileName);
                         if (whiteListedExt.Contains(extension))
                         {
                             string pic = Guid.NewGuid() + user.UserID.ToString() + extension;
                             using (AmazonS3Client s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
                                 EventZoneUtility.FileUploadToS3("eventzone", pic, stream, true, s3Client);
                             Image image = new Image();
                             image.ImageLink = "https://s3-us-west-2.amazonaws.com/eventzone/" + pic;
                             image.UserID = user.UserID;
                             image.UploadDate = DateTime.Today;
                             if (EventDatabaseHelper.Instance.AddImageToEvent(image,eventID))
                             {
                                 TempData["errorTitle"] = null;
                                 TempData["errorMessage"] = null;
                                 listImage = EventDatabaseHelper.Instance.GetEventImage(eventID);
                                 return PartialView("_ImagePartial",listImage);
                             }
                             else
                             {
                                 TempData["errorTitle"] = "Database Error";
                                 TempData["errorMessage"] = "Ops... Some error is ocurred while we save to database! Please try again later!";
                                 
                                 return PartialView("_ImagePartial", listImage);
                             }

                         }
                         else
                         {
                             TempData["errorTitle"] = "Database Error";
                             TempData["errorMessage"] = "Ops... Some error is ocurred while we save to database! Please try again later!";
                             
                             return PartialView("_ImagePartial", listImage);
                         }
                     }
                     else
                     {
                         TempData["errorTitle"] = "Not select file";
                         TempData["errorMessage"] = "It's look like you fogot select an image! Are you getting old?";
                  
                         return PartialView("_ImagePartial", listImage);
                     }
                 }
                 catch
                 {
                     TempData["errorTitle"] = "Unknow Error";
                     TempData["errorMessage"] = "Oops..Something wrong is happened! Please try again later...";
                     
                     return PartialView("_ImagePartial", listImage);
                 }
             }
         }
         public ActionResult ManageEvent() {
             User user = UserHelpers.GetCurrentUser(Session);
             if (user == null)
             {
                 TempData["errorTitle"] = "Require Signin";
                 TempData["errorMessage"] = "Ops.. It's look like you are current is not signed in system! Please sign in first!";
                 return RedirectToAction("Index", "Home");
             }
             else { 
                 List<Event> myevent= UserDatabaseHelper.Instance.GetUserEvent(user.UserID);
                 List<ViewThumbEventModel> listThumb= EventDatabaseHelper.Instance.GetThumbEventListByListEvent(myevent);
                 return View();
             }
         
         }
    }
}