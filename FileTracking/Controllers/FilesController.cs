using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileTracking.Models;
using FileTracking.ViewModels;
using System.Linq.Dynamic;

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

        //allow users to view file tables and its linked tables
        public ActionResult Index()
        {
            var file = _context.Files.Include(f => f.Districts).Include(f=>f.FileVolumes).ToList();//If you want to get other data from 
            //other tables remember to add it to the include as seen above
           

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
                File = new File(),//be sure to initialize this to not get that Id error
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
        [ValidateAntiForgeryToken]
        public ActionResult Save(File file)
        {
            //check if data pulled from form is valid
            if (!ModelState.IsValid)
            {
                var viewModel = new FileViewModel()
                {
                    File = file,
                    Districts = _context.Districts.ToList(),
                    FileTypes = _context.FileTypes.ToList(),
                    FileStatuses = _context.FileStatuses.ToList(),
                    IdentificationOptions = _context.IdentificationOptions.ToList()

                };
                return View("FileForm", viewModel);
            }

            //first if block is run in the event that we are creating a new file
            if (file.Id == 0)
            {
               
                file.DateCreated = DateTime.Now;
                file.Volume = 1;
                file.FileNumber = GetFileNumber();

                _context.Files.Add(file);

                _context.SaveChanges();
                int thisId = file.Id;
                //Upon that record being created, we immediately create volume one for that file based on the file number
                AddVolumeOnCreate(thisId);
                
            }
            else
            {
                //else block is run whenever we are editing an existing file record
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
                _context.SaveChanges();
            }

            

            return RedirectToAction("Index", "Files");
        }

        //update implementation
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

        //function retrieves currentFileNumber from ManageFileNumber tables in the db
        public int GetFileNumber()
        {
            var getRecord = _context.ManageFileNumbers.Single(mfn => mfn.Id == 1);
            var currentFileNum = getRecord.CurrentFileNumber;

            UpdateManageFileNumber();//update manageFileNumber to increment by 1 so as to not allow duplicate values #

            return currentFileNum;
        }

        //function updates the currentFileNumber whenever called
        public void UpdateManageFileNumber()
        {
            var getRecord = _context.ManageFileNumbers.Single(mfn => mfn.Id == 1);
            getRecord.CurrentFileNumber++;

            _context.SaveChanges();
        }

        //volume 1 for a file is made as soon as a new file is created.
        public void AddVolumeOnCreate(int thisId)
        {
           var fileInDb = _context.Files.Single(f => f.Id == thisId);

           var fileVolumeRecord = new FileVolumes
           {
               FileId = fileInDb.Id,
               FileNumber = fileInDb.FileNumber,
               Volume = 1,
               Comment = null,
               StatesId = 1
           };
           _context.FileVolumes.Add(fileVolumeRecord);
           _context.SaveChanges();
        }

        public ActionResult AddVolume(int id)
        {
            var fileInDb = _context.Files.SingleOrDefault(f => f.Id == id);
            var viewModel = new FileVolumeViewModel
            {
                File = fileInDb,
                FileVolumes = new FileVolumes()
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SaveVolume(FileVolumes fileVolumes,  File file)
        {
            //we get an existing record form file table, which in we only need the id
            int i = file.Id;
            var fileInDb = _context.Files.Single(f => f.Id == i);
            //Check of posted information is valid
            if (!ModelState.IsValid)
            {
                
                var viewModel = new FileVolumeViewModel
                {
                    File = fileInDb,
                    FileVolumes = new FileVolumes()
                    
                };
                //redirect to form page if validations fails
                return View("AddVolume", viewModel);
            }
            //otherwise, create a new volume record with respect to its file parent
            if (fileVolumes.Id == 0 && file.Id != 0)
            {
                fileInDb.Volume++;
                fileVolumes.FileId = file.Id;
                fileVolumes.Volume = fileInDb.Volume;
                fileVolumes.StatesId = 1;
                fileVolumes.FileNumber = file.FileNumber;

                _context.FileVolumes.Add(fileVolumes);
                
                _context.SaveChanges();


            }
            //redirect to table with record info
            return RedirectToAction("Index", "Files");
        }

        //[HttpPost]
        public ActionResult GetFiles()
        {
            //Server side parameters
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns["+Request["order[0][column]"]+"][name]"];
            string sortDirection = Request["order[0][dir]"];

            //We retrieve the data from the file database with respect to its table relationships
            List<File> FileList = new List<File>();
            FileList = _context.Files.Include(f => f.Districts).ToList<File>();

            int totalFiles = FileList.Count;
            //We check if search value if null or otherwise
            if (!string.IsNullOrEmpty(searchValue))//filter
            {
                FileList = FileList.Where(x => x.FileNumber.ToString().Contains(searchValue) ||
                                               x.FirstName.ToLower().Contains(searchValue.ToLower())||
                                               x.LastName.ToLower().Contains(searchValue.ToLower())||
                                               x.Volume.ToString().Contains(searchValue.ToLower())).ToList<File>();
            }

            int totalFileAfterFilter = FileList.Count;
            //sort Operation
            FileList = FileList.OrderBy(sortColumnName + " " + sortDirection).ToList<File>();

            //Paging Operation
            FileList = FileList.Skip(start).Take(length).ToList<File>();
            return Json(new
            {
                data = FileList, draw = Request["draw"], recordsTotal = totalFiles, recordsFiltered = totalFileAfterFilter

            }, JsonRequestBehavior.AllowGet);
        }

        
    }
}