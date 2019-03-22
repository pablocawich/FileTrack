using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileTracking.Models;
using FileTracking.ViewModels;

namespace FileTracking.Controllers
{
    public class FileVolumesController : Controller
    {
        private ApplicationDbContext _context;

        public FileVolumesController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }
        public string ParseUsername(string adName)
        {
            string newName = "";
            if (adName.Contains("DEVFINCO"))
                newName = adName.Remove(0, 9);
            return newName;
        }
        // GET: FileVolumes for a specific file identified by the id parameter
        [Authorize(Roles = Role.RegularUser)]
        public ActionResult RequestFile(int id)
        {
            string uName = ParseUsername(User.Identity.Name);
            var volFileId = _context.FileVolumes.Include(fv=>fv.States).Include(fv=>fv.Branches).Where(fv => fv.FileId == id).ToList();
            var volumes = _context.Files.Include(f => f.FileVolumes).SingleOrDefault(f => f.Id == id );
            var user = _context.AdUsers.Single(u=>u.Username == uName);

            var viewModel = new VolumesViewModel()
            {
                File = volumes,
                FileVolumes = volFileId,
                AdUser = user
            };
            return View("FileVolume",viewModel);
        }

        public void CheckIfAlreadyRequested()
        {

        }
    }
}