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
            if (!(User.IsInRole(Role.Registry) || User.IsInRole(Role.RegularUser) || User.IsInRole(Role.AdminUser)))
                return View("InvalidGroup");

            var userObj = new AdUser(User.Identity.Name);
            
            
            try
            {
                
                var user = _context.AdUsers.Include(u => u.Branches)
                    .SingleOrDefault(u => u.Username == userObj.Username);

                if (user.IsDisabled == true)
                    return View("Locked");
                   
               
                return View(user);
            }
            catch (Exception e)
            {                  
                return HttpNotFound(e.Message);
            }

          
        }

        public ActionResult ContentArea()
        {
            return View();
        }

        //Labels the branch for the respective user on the navigation panel
        public ActionResult BranchNavigationBar()
        {
            var adUser = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Include(u => u.Branches).Single(u => u.Username == adUser.Username);

            ViewBag.Message = user.Branches.Branch;
            return PartialView("_BranchNav");
        }

        //directs to the about page
        public ActionResult About()
        {
            ViewBag.Message = "File Tracking System";

            return View();
        }

        //directs to contact page
        public ActionResult Contact()
        {
            ViewBag.Message = "IT Department";

            return View();
        }
    }
}