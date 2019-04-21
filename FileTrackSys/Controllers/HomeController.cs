using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileTracking.Models;

namespace FileTracking.Controllers
{
    
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;

        public HomeController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }
        public ActionResult Index()
        {
            var userName = new AdUser(User.Identity.Name);
            var user = _context.AdUsers.Include(u=>u.Branches).Single(u => u.Username == userName.Username);

            if (user == null)
            {
                return Content("You are not registered as part of this domain and therefore cannot proceed to this application. Please" +
                               "Visit DFC's IT department for further clarification or help.");
            }

            return View(user);
        }
        [HttpGet]
        public JsonResult GetUserBranch()
        {
            var adUser = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Include(u=>u.Branches).Single(u => u.Username == adUser.Username);

            var branch = user.Branches.Branch;

            return Json(new { data = branch }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "File tracking system";

            return View();
        }
        
        public ActionResult Contact()
        {
            ViewBag.Message = "IT department";

            return View();
        }
    }
}