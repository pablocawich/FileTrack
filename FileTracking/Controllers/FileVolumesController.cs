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
        // GET: FileVolumes for a specific file identified by the id parameter
        public ActionResult RequestFile(int id)
        {
            var volFileId = _context.FileVolumes.Include(fv=>fv.States).Where(fv => fv.FileId == id).ToList();
            var volumes = _context.Files.Include(f => f.FileVolumes).SingleOrDefault(f => f.Id == id );

            var viewModel = new VolumesViewModel()
            {
                File = volumes,
                FileVolumes = volFileId
            };
            return View("FileVolume",viewModel);
        }
    }
}