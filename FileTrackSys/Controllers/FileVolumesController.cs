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

        public void CreateNotification(Request req, string messageId)
        {
            var notif = new Notification()
            {
                RecipientUserId = req.UserId,
                MessageId = messageId,
                Read = false,
                RequestId = req.Id,
                DateTriggered = DateTime.Now,
                SenderUser = req.AcceptedBy
            };

            _context.Notifications.Add(notif);
            _context.SaveChanges();
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

            var volFileId = _context.FileVolumes.Include(fv=>fv.States).
                Include(fv=>fv.Branches).Include(fv=>fv.AdUser).Where(fv => fv.FileId == id).ToList();

            var file = _context.Files.Include(f => f.FileVolumes).SingleOrDefault(f => f.Id == id );

            var user = _context.AdUsers.Single(u=>u.Username == uName);

            var viewModel = new VolumesViewModel()
            {
                File = file,
                FileVolumes = volFileId,
                AdUser = user
            };

            return View("FileVolume",viewModel);
        }

        [Authorize(Roles = Role.RegularUser)]
        public ActionResult UserVolumes()//this interface gets files currently check out to user
        {
            string username = ParseUsername(User.Identity.Name);
            var user = _context.AdUsers.Single(u => u.Username == username);

            var request = _context.Requests.Include(r=>r.FileVolumes).Include(r=>r.Branches).Where(r => r.UserId == user.Id).Where(r=>r.IsRequestActive == true)
                .Where(r => r.IsConfirmed == true).
                Where(r=>r.ReturnStateId == 1).Where(r=>r.RequestTypeId == RequestType.InternalRequest).ToList();

           // var reqList = new List<Request>();

            //reqList = request;
            //run query that shows volume currently assigned to the signed in user
                      
            return View("UserVolumes", request);
        }

        public void ReturnVolume(int id)
        {
            const byte isReturning = 2;
            var request = _context.Requests.Single(r => r.Id == id);
            request.ReturnStateId = isReturning;

            _context.SaveChanges();

            CreateNotification(request, Message.Return);
            //since returning, state should transition to the transfer state. Just to give more detail. Not significant change
            ChangeStateToTransfer(request.FileVolumesId, request.UserId);

        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult ReturnApproval()
        {
            return View("ReturnApproval");
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult GetReturnedRequests()
        {
            var username = new AdUser(User.Identity.Name);
            var adUser = _context.AdUsers.Single(a => a.Username == username.Username);
            var returnedReq = _context.Requests.Include(r => r.FileVolumes).
                Include(r => r.User.Branches).Where(r=>r.RequesteeBranch == adUser.BranchesId).Where(r=>r.RequestTypeId == RequestType.InternalRequest)
                .Where(r=>r.IsRequestActive == true).Where(r=>r.ReturnStateId == 2).ToList();


            return Json(new { data = returnedReq }, JsonRequestBehavior.AllowGet);
        }

        //handles returns for both external and internal transfer 
        public void AcceptReturn(int id)
        {
            var user = new AdUser(User.Identity.Name);//we get the current registry user and initialize its username
            var req = _context.Requests.Single(r => r.Id == id);
            
            req.ReturnedDate = DateTime.Now;
            req.ReturnAcceptBy = user.Username;
            req.ReturnStateId = 3;
            req.IsRequestActive = false;
            
            //if a binder exists then that signifies our records is still not complete   
            if (req.RequestBinder != 0)
            {
                //if it has external binder we switch the req type to external   
                _context.SaveChanges();
                InitiateExternalReturn(req.RequestBinder);//we set the external (based on the bind value) request to active
                UpdateVolumeForExternalTransfer(req.FileVolumesId);
            }
            else
            {
                _context.SaveChanges();
                ChangeStateToStored(req.FileVolumesId);
            }
                       
        }

        //accepting external return
        public void AcceptExternalReturn(int id)
        {
           var user = new AdUser(User.Identity.Name);//we get the current registry user and initialize its username
            var req = _context.Requests.Single(r => r.Id == id);

            req.ReturnedDate = DateTime.Now;
            req.ReturnAcceptBy = user.Username;
            req.ReturnStateId = 3;
            req.IsRequestActive = false;

            _context.SaveChanges();

            ChangeStateToStored(req.FileVolumesId);
            CreateNotification(req,Message.ExReturnApproval);
        }

        public void ChangeStateToStored(int volId)
        {
            //stored state => 1
            var vol = _context.FileVolumes.Single(v => v.Id == volId);

            vol.StatesId = 1;
            vol.AdUserId = null;

            if(vol.CurrentLocation != vol.BranchesId)//we both locations are different, it means it was an external request and we should reset since the return process is complete
                vol.CurrentLocation = vol.BranchesId;
            _context.SaveChanges();
        }

        public void ChangeStateToTransfer(int volId, int requesterId)
        {
            //transfer = 4
            var vol = _context.FileVolumes.Single(v => v.Id == volId);

            vol.StatesId = 4;
            vol.AdUserId = requesterId;

            /*if (vol.CurrentLocation != vol.BranchesId)//we both locations are different, it means it was an external request and we should reset since the return process is complete
                vol.CurrentLocation = vol.BranchesId;*/
            _context.SaveChanges();
        }

        public void UpdateVolumeForExternalTransfer(int volId)
        {
            var vol = _context.FileVolumes.Single(v => v.Id == volId);

            vol.StatesId = 4;
            vol.AdUserId = null;

            _context.SaveChanges();
        }

        //after the binded internal request is returned to local registry, we must initiate registry to registry request to return the file to its initial location
        public void InitiateExternalReturn(int binderId)
        {
            var extReturnReq = _context.Requests.Single(r=>r.RequestBinder == binderId && r.RequestTypeId == RequestType.ExternalRequest &&
                                                           r.RequestStatusId == 2 && r.IsConfirmed == true);
            //if(extReturnReq == null)
                
            extReturnReq.IsRequestActive = true;
            _context.SaveChanges();
            return;
        }

        //load the return to branch view, reigstry users will be able to see external files currently at their disposal to further send a request back to the external registry
        [Authorize(Roles = Role.Registry)]
        public ActionResult ReturnToBranch()
        {
            return View();
        }

        //executs query and returns external files list in order for the local registry to send external return
        public ActionResult GetReturnToBranchFiles()
        {
            var userObj = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);

            var request = _context.Requests.Include(r => r.FileVolumes).Include(r => r.User.Branches).
                Where(r => r.BranchesId == user.BranchesId).Where(r => r.RequestTypeId == RequestType.ExternalRequest).
                Where(r => r.IsConfirmed == true).Where(r=>r.ReturnStateId == 1).Where(r=>r.IsRequestActive == true).ToList();

            return Json(new { data = request }, JsonRequestBehavior.AllowGet);
        }

        public void ReturnToBranchAction(int id)
        {
            var currUser = new AdUser(User.Identity.Name);
            //return sent => 2
            var extReq = _context.Requests.Single(r => r.Id == id);
            extReq.ReturnStateId = 2;
            extReq.ReturnedDate = DateTime.Now;
            extReq.ReturnAcceptBy = currUser.Username;

            _context.SaveChanges();
            CreateNotification(extReq, Message.ExReturn);
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult ApproveExternalReturns()
        {
            return View();
        }

        public ActionResult GetExternalApproveReturns()
        {
            //after internal returns, local registry will need to return that file to its original branch
            //this query will return all those returns to its original branch to await confirmation
            var userObj = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);

            var request = _context.Requests.Include(r => r.FileVolumes).Include(r => r.User.Branches).
                Where(r => r.RequesteeBranch == user.BranchesId).Where(r => r.RequestTypeId == RequestType.ExternalRequest).
                Where(r => r.IsConfirmed == true).Where(r => r.ReturnStateId == 2).Where(r => r.IsRequestActive == true).ToList();

            return Json(new { data = request }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult UpdateVolumeDescription(int id)
        {
            var volumeInDb = _context.FileVolumes.Single(fv => fv.Id == id);

            return PartialView(volumeInDb);
        }

        [HttpPost]
        public ActionResult SaveVolumeDescription(FileVolumes fileVolumes)
        {
            if (!ModelState.IsValid)
            {
                //redirect to form page if validations fails with the same file vol objects redirected
                var volumeInDb = _context.FileVolumes.Single(fv => fv.Id == fileVolumes.Id);
                return PartialView("UpdateVolumeDescription", volumeInDb);
            }

            //otherwise, create a new volume record with respect to its file parent
            if (fileVolumes.Id != 0)
            {
                var volumeInDb = _context.FileVolumes.Single(v=>v.Id == fileVolumes.Id);
                volumeInDb.Comment = fileVolumes.Comment;
                volumeInDb.Id = fileVolumes.Id;
                volumeInDb.FileId = fileVolumes.FileId;
                volumeInDb.Volume = fileVolumes.Volume;
                volumeInDb.StatesId = fileVolumes.StatesId;
                volumeInDb.FileNumber = fileVolumes.FileNumber;
                volumeInDb.BranchesId = fileVolumes.BranchesId;
                volumeInDb.CurrentLocation = fileVolumes.CurrentLocation;
                volumeInDb.AdUserId = fileVolumes.AdUserId;

                _context.SaveChanges();

            }

            return RedirectToRoute(new
            {
                controller = "Files",
                action = "FileVolumes",
                id = fileVolumes.FileId
            });

        }
    }
}