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
using System.Text.RegularExpressions;
using System.Web.WebPages;
using Microsoft.Ajax.Utilities;

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

        public string CreateFullName(string fName, string mName, string lName)
        {
            //in the event a middle is not provided. Avoids possible null exception
            if (mName.IsEmpty())
                return $"{fName} {lName}";

            return $"{fName} {mName} {lName}";
        }

        //allow users to view file tables and its linked tables
        public ActionResult SearchFiles()
        {
            var userObj = new AdUser(User.Identity.Name);
            var userInDb = _context.AdUsers.Single(u => u.Username == userObj.Username);

            //var file = _context.Files.Include(f => f.Districts).Include(f=>f.FileVolumes).ToList();
            if (User.IsInRole(Role.Registry) && userInDb.IsDisabled == false)
                return View("RegistryView");
            else if (User.IsInRole(Role.RegularUser) && userInDb.IsDisabled == false)
                return View("UserView");

            return View("Locked");

        }

        //Will show details for a specific file
        public ActionResult FileDetails(int id)
        {
            var file = _context.Files.Include(f=>f.Districts).Include(f => f.FileType).
                Include(f => f.IdentificationOption).Include(f=>f.FileStatus).Include(f=>f.Location).Single(f => f.Id == id);
            return PartialView(file);
        }
        
        //similar to the above function, displays file info. Allows no edits
        public ActionResult FileDetailsForConfirm(int id)
        {
            var file = _context.Files.Include(f => f.Districts).Include(f => f.FileType).
                Include(f => f.IdentificationOption).Include(f=>f.FileStatus).Include(f => f.Location).Single(f => f.Id == id);
            return PartialView(file);

        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult New()
        {
            var userObj = new AdUser(User.Identity.Name);
            var userInDB = _context.AdUsers.Single(u => u.Username == userObj.Username);

            //checks if a user is not disabled
            if (userInDB.IsDisabled == true)
            {
                return View("Locked");
            }
            else
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
                //we remove once string contains whitespace
                file.FirstName = Regex.Replace(file.FirstName, @"\s", "");
                file.MiddleName = Regex.Replace(file.MiddleName ?? "", @"\s", "");
                file.LastName = Regex.Replace(file.LastName, @"\s", "");

                /*if(file.LoanNumber == null)
                    file.LoanNumber = "";*/
                file.FullName = CreateFullName(file.FirstName, file.MiddleName, file.LastName);

                _context.Files.Add(file);
                _context.SaveChanges();
                
                //Upon that record being created, we immediately create volume one for that file based on the file number
                AddVolumeOnCreate(file.Id, file.VolumeOneDescription);
                
            }
            else
            {
                //else block is run whenever we are editing an existing file record
                var fileInDb = _context.Files.Single(f => f.Id == file.Id);
                  
                fileInDb.FirstName = Regex.Replace(file.FirstName, @"\s", "");
                if (file.MiddleName.IsEmpty())
                    fileInDb.MiddleName = file.MiddleName;
                else
                    fileInDb.MiddleName = file.MiddleName.Replace(" ", String.Empty);

                fileInDb.LastName = Regex.Replace(file.LastName, @"\s", "");
                fileInDb.Street = file.Street;
                fileInDb.DistrictsId = file.DistrictsId;
                fileInDb.LocationId = file.LocationId;
                fileInDb.Comments = file.Comments;
                fileInDb.FileTypeId = file.FileTypeId;
                fileInDb.FileStatusId = file.FileStatusId;
                fileInDb.IdentificationOptionId = file.IdentificationOptionId;
                fileInDb.IdentificationNumber = file.IdentificationNumber;
                fileInDb.LoanNumber = file.LoanNumber;
                fileInDb.PreviousFileNumber = file.PreviousFileNumber;
                fileInDb.FullName = CreateFullName(file.FirstName, file.MiddleName, file.LastName);
                fileInDb.FileAccess = file.FileAccess;
                _context.SaveChanges();
            }           

            return RedirectToAction("SearchFiles", "Files");
        }

        //update implementation
        public ActionResult Update(int id)
        {
            var currUser = new AdUser(User.Identity.Name);
            var userInDb = _context.AdUsers.Single(u=>u.Username == currUser.Username);

            if (userInDb.IsDisabled == false)
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

            return View("Locked");

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
               CurrentLocationId = thisUserBranchId,
               StatesId = 1,
               AdUserId = null
           };
           _context.FileVolumes.Add(fileVolumeRecord);
           _context.SaveChanges();
        }
        
        public byte GetAdUserBranch()
        {
            var user = new AdUser(User.Identity.Name);

            var userRecord = _context.AdUsers.Single(a => a.Username == user.Username);
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
        [ValidateAntiForgeryToken]
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
                fileVolumes.CurrentLocationId = thisUserBranchId;//Similarly, given the file is being created, the current location will have to match the above
                fileVolumes.StatesId = 1; //default for every new file vol's state is 1 which indicates a stored state
                fileVolumes.FileNumber = file.FileNumber;//not necessary but relevant, i think. 

                _context.FileVolumes.Add(fileVolumes);
                
                _context.SaveChanges();

            }
            //redirect to table with record info
            return RedirectToAction("SearchFiles", "Files");
        }

        //view volumes as it pertains to the chosen file
        [Authorize(Roles = Role.Registry)]
        public ActionResult FileVolumes(int id)
        {
            var uname  = new AdUser(User.Identity.Name);
            var user = _context.AdUsers.Single(u => u.Username == uname.Username);

            if (user.IsDisabled == false)
            {
                var volFileId = _context.FileVolumes.Include(fv => fv.File).Include(fv => fv.States).
                    Include(fv => fv.Branches).Include(fv=>fv.CurrentLocation).Include(fv => fv.AdUser)
                    .Where(fv => fv.FileId == id).ToList();

                var file = _context.Files.Include(f => f.FileVolumes).SingleOrDefault(f => f.Id == id);



                var viewModel = new VolumesViewModel()
                {
                    File = file,
                    FileVolumes = volFileId,
                    AdUser = user
                };

                return View(viewModel);
            }

            return View("Locked");
        }

        //[HttpPost]. Sends our file objects as a set of JSON objects. Enables the possibility of server side processing on our dataTable.
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
                FileList = FileList.Where(x => (x.FileNumber != 0 && x.FileNumber.ToString().Contains(Request["columns[0][search][value]"]))).ToList<File>();

            if (!string.IsNullOrEmpty(Request["columns[1][search][value]"]))
                FileList = FileList.Where(x => (x.FullName != null && x.FullName.ToLower().Contains(Request["columns[1][search][value]"].ToLower()))).ToList<File>();

            if (!string.IsNullOrEmpty(Request["columns[2][search][value]"]))
                FileList = FileList.Where(x => x.Districts.District.ToLower().Contains(Request["columns[2][search][value]"].ToLower())).ToList<File>();

            if (!string.IsNullOrEmpty(Request["columns[3][search][value]"]))
                FileList = FileList.Where(x => (x.LoanNumber != null && x.LoanNumber.ToString().Contains(Request["columns[3][search][value]"]))).ToList<File>();

            if (!string.IsNullOrEmpty(Request["columns[4][search][value]"]))
                FileList = FileList.Where(x => (x.PreviousFileNumber != null && x.PreviousFileNumber.ToString().ToLower().Contains(Request["columns[4][search][value]"]))).ToList<File>();

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

        //change file volume branch origins
        [Authorize(Roles = Role.Registry)]
        public ActionResult ChangeVolumeBranch(int id)
        {
            var volumeInDb = _context.FileVolumes.Include(v=>v.Branches).Single(v => v.Id == id);
            var branchesInDb = _context.Branches.ToList();

            var viewModel = new VolumeBranchesViewModel()
            {
                Volume = volumeInDb,
                Branches = branchesInDb
            };

            return PartialView("_ChangeVolumeBranch",viewModel);
        }
             
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.Registry)]
        public ActionResult SaveNewVolumeBranch(VolumeBranchesViewModel model)
        {
            
            if (!ModelState.IsValid)
            {
                return PartialView("_ChangeVolumeBranch", model);
            }

            if (model.Volume.Id != 0)
            {
                var volumeInDb = _context.FileVolumes.Single(v => v.Id == model.Volume.Id);
                //we ensure file volume is store to carry out procedure
                if (volumeInDb.StatesId == 1)
                {
                    volumeInDb.BranchesId = model.Volume.BranchesId;
                    volumeInDb.CurrentLocationId = model.Volume.BranchesId;

                    _context.SaveChanges();
                }
                return RedirectToRoute(new
                {
                    controller = "Files",
                    action = "FileVolumes",
                    id = model.Volume.FileId
                });
            }

            return HttpNotFound("Volume not found!. Try again or contact IT Department");
        }

        [Authorize(Roles = Role.Registry)]
        public JsonResult CheckForVolumeRequestActivity(int id)
        {
            var requestsInDb = _context.Requests.Where(r => r.FileVolumesId == id).ToList();

            if (requestsInDb.Any())
            {
                return this.Json(new { success = false, message = "There currently exists ongoing activities that feature this volume. Ensure clearances of these activities are made before proceeding." }, JsonRequestBehavior.AllowGet);
            }

            return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

    }
}