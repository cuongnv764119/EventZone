using EventZone.Helpers;
using EventZone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventZone.Controllers
{
    public class LookAroundController : Controller
    {
   
        public ActionResult LookAround(double longitude = 0, double latitude = 0, double distance = 20.0) {
         
                List<Location> listPlace = new List<Location>();
                Location cur = new Location();
                cur.LocationID = -1;
                cur.LocationName = "Current Location";
                cur.Longitude = longitude;
                cur.Latitude = latitude;
                //if (Request.Form["search"] != null)
                //{
                //    cur.Longitude = Double.Parse(Request.Form["latitude"]);
                //    cur.Latitude = Double.Parse(Request.Form["longitude"]);
                //}
                listPlace = LocationHelpers.Instance.GetAllLocation();
                listPlace.RemoveAll(item => (LocationHelpers.Instance.CalculateDistance(cur, item) - distance) > 0);// tat ca cac dia diem o gan
                List<EventPlace> listEventPlace = new List<EventPlace>();
                List<EventPlace> nearlyEventPlace = new List<EventPlace>();
                listEventPlace = LocationHelpers.Instance.GetAllEventPlace();
                foreach (var item in listPlace)
                {
                    var abc = listEventPlace.FindAll(a => a.LocationID == item.LocationID);
                    if (abc != null)
                        foreach (var x in abc)
                        {
                            nearlyEventPlace.Add(x);
                        }
                }
                List<Location> nearlyLocation = new List<Location>();
                List<Event> nearlyEvent = new List<Event>();
                nearlyEventPlace = nearlyEventPlace.Distinct().ToList();
                foreach (var item in nearlyEventPlace)
                {
                    var place = LocationHelpers.Instance.GetLocationById(item.LocationID);
                    var evt = EventDatabaseHelper.Instance.GetEventByID(item.EventID);
                    nearlyLocation.Add(place);
                    nearlyEvent.Add(evt);
                }
                listPlace = LocationHelpers.Instance.RemovePlaceByDistance(listPlace, 1);
                nearlyEvent = nearlyEvent.Distinct().ToList();
                nearlyLocation = nearlyLocation.Distinct().ToList();
                ViewData["currentLocation"] = cur;
                ViewData["nearlyEvent"] = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(nearlyEvent);
                ViewData["nearlyLocation"] = listPlace.Distinct().ToList();
                return View();           
        }
        public ActionResult showEventInLocation(long id)
        {

            if (Request.IsAjaxRequest())
            {
                Location  currLocation= LocationHelpers.Instance.GetLocationById(id);
                
                  List<Location> listPlace = LocationHelpers.Instance.GetAllLocation();
                  listPlace.RemoveAll(item => (LocationHelpers.Instance.CalculateDistance(currLocation, item) - 0.5) >= 0);
                  List<EventPlace> listEventPlace= new List<EventPlace>();
                  
                  foreach (var item in listPlace) {
                      try
                      {
                          var result = (from a in LocationHelpers.Instance.GetAllEventPlace() where a.LocationID == item.LocationID select a).ToList();
                          if (result != null) {
                              listEventPlace.AddRange(result);
                          } 
                      }
                      catch { }
                     
                  }
                List<Event> listEventSamePlace = new List<Event>();
                foreach (var item in listEventPlace)
                {
                    var evt = EventDatabaseHelper.Instance.GetEventByID(item.EventID);
                    listEventSamePlace.Add(evt);
                }
               listEventSamePlace = listEventSamePlace.Distinct().ToList();
                List<ViewThumbEventModel> listThumb = EventDatabaseHelper.Instance.GetThumbEventListByListEvent(listEventSamePlace);
                return PartialView("_ViewEventInPlace", listThumb);
            }
            return null;
            //return RedirectToAction("ShowMap", "Event", new {id = 1});
        }
       
    }
}
