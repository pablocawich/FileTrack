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
    public class FilesController : Controller
    {
        // GET: Files
        private ApplicationDbContext _context;

        public FilesController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public ActionResult Index()
        {
            var file = _context.Files.Include(f => f.Districts).ToList();
            
            return View(file);
        }

        public ActionResult CreateFile()
        {
            //retrieving table content
            var districts = _context.Districts.ToList();
            var fileTypes = _context.FileTypes.ToList();
            var fileStatuses = _context.FileStatuses.ToList();
            var identificationStatuses = _context.IdentificationOptions.ToList();
            //instantiating viewModel objects to the db content
            var viewModel = new FileViewModel
            {
                Districts = districts,
                FileTypes = fileTypes,
                FileStatuses = fileStatuses,
                IdentificationOptions = identificationStatuses
            };
            //passing db (viewModel) content to FileForm 
            return View("FileForm", viewModel);
        }
        //below function should accepts a file objects with its binded values from a form as its parameter
        //and ultimately save that value unto the database.
        [HttpPost]
        public ActionResult NewFile(File file)
        {
            file.DateCreated = DateTime.Now;
            
            return Content("Getting to this page signifies you have filled the form fields with data and is now being" +
                           " fetched in an attempt to store them into the database" + file.DateCreated);
        }
    }
}