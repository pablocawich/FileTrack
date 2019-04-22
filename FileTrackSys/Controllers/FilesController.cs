﻿using System;
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
            //var file = _context.Files.Include(f => f.Districts).Include(f=>f.FileVolumes).ToList();
            if (User.IsInRole(Role.Registry))
                return View("RegistryView");

            if (User.IsInRole(Role.RegularUser))
                return View("UserView");

            return Content("You do not have permissions to visit this page. Please see or contact admin for further information");

        }

        //Will show details for a specific file
        public ActionResult FileDetails(int id)
        {
            var file = _context.Files.Include(f=>f.Districts).Include(f => f.FileType).
                Include(f => f.IdentificationOption).Include(f=>f.Location).Single(f => f.Id == id);
            return PartialView(file);
        }
        
        public ActionResult FileDetailsForConfirm(int id)
        {
            var file = _context.Files.Include(f => f.Districts).Include(f => f.FileType).
                Include(f => f.IdentificationOption).Include(f => f.Location).Single(f => f.Id == id);
            return PartialView(file);

        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult New()
        {
            //retrieving table content
            var districts = _context.Districts.ToList();
            var fileTypes = _context.FileTypes.ToList();
            var fileStatuses = _context.FileStatuses.ToList();
            var identificationStatuses = _context.IdentificationOptions.ToList();
            var locations = _context.Locations.ToList();
            //instantiating viewModel objects to the db content
            var viewModel = new FileViewModel
            {
                File = new File(),//be sure to initialize this to not get that Id error
                Districts = districts,
                FileTypes = fileTypes,
                FileStatuses = fileStatuses,
                IdentificationOptions = identificationStatuses,
                Locations = locations
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
                    IdentificationOptions = _context.IdentificationOptions.ToList(),
                    Locations = _context.Locations.ToList()

                };
                return View("FileForm", viewModel);
            }

            //first if block is run in the event that we are creating a new file
            if (file.Id == 0)
            {
               
                file.DateCreated = DateTime.Now;
                file.Volume = 1;
                file.FileNumber = GetFileNumber();
                if(file.LoanNumber == null)
                    file.LoanNumber = "";

                _context.Files.Add(file);

                _context.SaveChanges();
                
                //Upon that record being created, we immediately create volume one for that file based on the file number
                AddVolumeOnCreate(file.Id, file.VolumeOneDescription);
                
            }
            else
            {
                //else block is run whenever we are editing an existing file record
                var fileInDb = _context.Files.Single(f => f.Id == file.Id);
                  
                fileInDb.FirstName = file.FirstName;
                fileInDb.MiddleName = file.MiddleName;
                fileInDb.LastName = file.LastName;
                fileInDb.Street = file.Street;
                fileInDb.DistrictsId = file.DistrictsId;
                fileInDb.LocationId = file.LocationId;
                fileInDb.Comments = file.Comments;
                fileInDb.FileTypeId = file.FileTypeId;
                fileInDb.FileStatusId = file.FileStatusId;
                fileInDb.IdentificationOptionId = file.IdentificationOptionId;
                fileInDb.IdentificationNumber = file.IdentificationNumber;
                fileInDb.LoanNumber = file.LoanNumber;
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
                IdentificationOptions = _context.IdentificationOptions.ToList(),
                Locations = _context.Locations.ToList()
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

        //to be used for jquery to handle and determine what to place in the dropdown list
        public ActionResult GetLocationsByDistrict(int id)
        {
            var locations = _context.Locations.Where(l => l.DistrictsId == id).ToList();           
            return Json(locations, JsonRequestBehavior.AllowGet);
        }

        //volume 1 for a file is made as soon as a new file is created.
        public void AddVolumeOnCreate(int thisId, string volumeOneDescription)
        {
           var fileInDb = _context.Files.Single(f => f.Id == thisId);
           byte thisUserBranchId = GetAdUserBranch();

           var fileVolumeRecord = new FileVolumes
           {
               FileId = fileInDb.Id,
               FileNumber = fileInDb.FileNumber,
               Volume = 1,
               Comment = volumeOneDescription,
               BranchesId = thisUserBranchId,
               CurrentLocation = thisUserBranchId,
               StatesId = 1,
               AdUserId = null
           };
           _context.FileVolumes.Add(fileVolumeRecord);
           _context.SaveChanges();
        }

        //we need this function
        public string ParseUsername(string adName)
        {
            string newName = "";
            if (adName.Contains("DEVFINCO"))
                newName = adName.Remove(0, 9);
            return newName;
        }

        //
        public byte GetAdUserBranch()
        {
            string user = ParseUsername(User.Identity.Name);
            var userRecord = _context.AdUsers.Single(a => a.Username == user);
            return userRecord.BranchesId;
        }

        //directs users to the volumes modal view for a specific file based on the id parameter
      [Authorize(Roles = Role.Registry)]
      public ActionResult AddNewVolume(int id)
      {
          var fileInDb = _context.Files.SingleOrDefault(f => f.Id == id);
          var viewModel = new FileVolumeViewModel
          {
              File = fileInDb,
              FileVolumes = new FileVolumes()
          };
        return PartialView(viewModel);
      }

      //saves a volume with its associated file infomation 
        [HttpPost]
        public ActionResult SaveVolume(FileVolumes fileVolumes,  File file)
        {
            //we get an existing record form file table, which in we only need the id
            //int i = file.Id;
            var fileInDb = _context.Files.Single(f => f.Id == file.Id);
            //Check of posted information is valid
            if (!ModelState.IsValid)
            {
                
                var viewModel = new FileVolumeViewModel
                {
                    File = fileInDb,
                    FileVolumes = new FileVolumes()
                    
                };
                //redirect to form page if validations fails with the same file vol objects redirected
                return PartialView("AddNewVolume", viewModel);
            }

            byte thisUserBranchId = GetAdUserBranch();
            //otherwise, create a new volume record with respect to its file parent
            if (fileVolumes.Id == 0 && file.Id != 0)
            {
                fileInDb.Volume++;
                fileVolumes.FileId = file.Id;
                fileVolumes.Volume = fileInDb.Volume;
                fileVolumes.BranchesId = thisUserBranchId;//based on where the user is situated we add the location of origin
                fileVolumes.CurrentLocation = thisUserBranchId;//Similarly, given the file is being created, the current location will have to match the above
                fileVolumes.StatesId = 1; //default for every new file vol's state is 1 which indicates a stored state
                fileVolumes.FileNumber = file.FileNumber;//not necessary but relevant, i think. 

                _context.FileVolumes.Add(fileVolumes);
                
                _context.SaveChanges();

            }
            //redirect to table with record info
            return RedirectToAction("Index", "Files");
        }

        //view volumes as it pertains to the chosen file
        public ActionResult FileVolumes(int id)
        {
            string uName = ParseUsername(User.Identity.Name);

            var volFileId = _context.FileVolumes.Include(fv=>fv.File).Include(fv => fv.States).
                Include(fv => fv.Branches).Include(fv=>fv.AdUser).Where(fv => fv.FileId == id).ToList();

            var file = _context.Files.Include(f => f.FileVolumes).SingleOrDefault(f => f.Id == id);

            var user = _context.AdUsers.Single(u => u.Username == uName);

            var viewModel = new VolumesViewModel()
            {
                File = file,
                FileVolumes = volFileId,
                AdUser = user
            };
            return View(viewModel);
        }

        //[HttpPost]. Sends our file objects as a set of JSON objects. Enables the possibility of server side processing on our datatable.
        public ActionResult GetFiles()
        {
            //Server side parameters
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
           // string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns["+Request["order[0][column]"]+"][name]"];
            string sortDirection = Request["order[0][dir]"];

            //We retrieve the data from the file database with respect to its table relationships
            List<File> FileList = new List<File>();
            FileList = _context.Files.Include(f=>f.Location).Include(f => f.Districts).ToList<File>();

            int totalFiles = FileList.Count;
            //We check if search value if null or otherwise
            /*if (!string.IsNullOrEmpty(searchValue) && !string.IsNullOrWhiteSpace(searchValue))//filter
            {
                FileList = FileList.Where(x => x.FileNumber.ToString().Contains(searchValue) ||
                                               x.FirstName.ToLower().Contains(searchValue.ToLower())||
                                               x.LastName.ToLower().Contains(searchValue.ToLower())||
                                              x.LoanNumber.ToString().Contains(searchValue)||
                                               x.Districts.District.ToLower().Contains(searchValue.ToLower())).ToList<File>();
            }*/
            // we no longer need the above since we will implement our custom filter
            if (!string.IsNullOrEmpty(Request["columns[0][search][value]"]))
                FileList = FileList.Where(x => x.FileNumber.ToString().Contains(Request["columns[0][search][value]"])).ToList<File>();

            if (!string.IsNullOrEmpty(Request["columns[1][search][value]"]))
                FileList = FileList.Where(x => x.FirstName.ToLower().Contains(Request["columns[1][search][value]"].ToLower()) ||
                                               x.LastName.ToLower().Contains(Request["columns[1][search][value]"].ToLower())).ToList<File>();

            if (!string.IsNullOrEmpty(Request["columns[2][search][value]"]))
                FileList = FileList.Where(x => x.Districts.District.ToLower().Contains(Request["columns[2][search][value]"].ToLower())).ToList<File>();

            if (!string.IsNullOrEmpty(Request["columns[3][search][value]"]))
                FileList = FileList.Where(x => x.LoanNumber.ToString().Contains(Request["columns[3][search][value]"])).ToList<File>();

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