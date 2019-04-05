using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileTracking.Models;

namespace FileTracking.Controllers
{
    public class ProfilesController : Controller
    {
        private ApplicationDbContext _context;

        public ProfilesController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        // GET: Profiles
        public ActionResult Index()
        {
            string uName = ParseUsername(User.Identity.Name);
            var currentUserInDb = _context.AdUsers.Include(u=>u.Branches).Single(u => u.Username == uName);
            return View("Index", currentUserInDb);
        }
        public string ParseUsername(string adName)
        {
            string newName = "";
            if (adName.Contains("DEVFINCO"))
                newName = adName.Remove(0, 9);
            return newName;
        }
    }
}