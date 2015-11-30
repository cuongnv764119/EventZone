using System.Linq;
using System.Web.Mvc;
using EventZone.Models;

namespace EventZone.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly EventZoneEntities db = new EventZoneEntities();
        // GET: Categories
        public ActionResult Index()
        {
            return PartialView(db.Categories.ToList());
        }
    }
}