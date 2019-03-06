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

        public ActionResult New()
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
        public ActionResult Save(File file)
        {
            if (file.Id == 0)
            {
                file.DateCreated = DateTime.Now;
                // fileVol.Id = file.Id;
                //"Getting to this page signifies you have filled the form fields with data and is now being" +
                //" fetched in an attempt to store them into the database"
                _context.Files.Add(file);
            }
            else
            {
                var fileInDb = _context.Files.Single(f => f.Id == file.Id);

                fileInDb.FirstName = file.FirstName;
                fileInDb.MiddleName = file.MiddleName;
                fileInDb.LastName = file.LastName;
                fileInDb.Street = file.Street;
                fileInDb.CityOrTown = file.CityOrTown;
                fileInDb.DistrictsId = file.DistrictsId;
                fileInDb.Comments = file.Comments;
                fileInDb.FileTypeId = file.FileTypeId;
                fileInDb.FileStatusId = file.FileStatusId;
                fileInDb.IdentificationOptionId = file.IdentificationOptionId;
                fileInDb.IdentificationNumber = file.IdentificationNumber;


            }

            _context.SaveChanges();

            return RedirectToAction("Index", "Files");
            /* return Content(file.FirstName+"" +file.MiddleName+" " +file.LastName + " " +file.DateCreated
                            + " \n" +file.Street + " " + file.CityOrTown + " " +file.DistrictsId + " " + file.Comments
                            + " " +file.FileTypeId + " " + file.FileStatusId + " " + file.IdentificationOptionId
                            + " " +file.IdentificationNumber);*/
        }

        public ActionResult Update(int id)
        {
            var fileInDb = _context.Files.SingleOrDefault(f => f.Id == id);
            if (fileInDb == null)
            {
                return HttpNotFound();
            }
            
            var viewModel = new FileViewModel
            {
                File = fileInDb,
                Districts = _context.Districts.ToList(),
                FileTypes = _context.FileTypes.ToList(),
                FileStatuses = _context.FileStatuses.ToList(),
                IdentificationOptions = _context.IdentificationOptions.ToList()
            };
           

            return View("FileForm", viewModel);
        }
    }
}