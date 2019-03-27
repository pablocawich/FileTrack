using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FileTracking.Models;
using FileTracking.ViewModels;
using Microsoft.AspNet.Identity;

namespace FileTracking.Controllers
{
    public class RequestsController : Controller
    {
        private ApplicationDbContext _context;

        public RequestsController()
        {
            _context = new ApplicationDbContext();
       
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }
       

        [Route("Requests/Index/{volId}")]
        public ActionResult Index(int volId)
        {
            //pulls records associated with a request such as volume and user

            var volume = _context.FileVolumes.Single(v => v.Id == volId);
            string username = ParseUsername(User.Identity.Name);
            var user = _context.AdUsers.Single(u => u.Username == username);

            if (!CheckBranchValidity(user, volume))
                return Content("Cannot request file from situated outside your local branch. " +
                    " Maybe this page can direct to a special interface that enables this feature however, not" +
                    " to be our focus now. For a later time.");

            //check if this volume number has not already been requested by this user
            if (volume.StatesId != 1)
            {
                if (HasBeenRequested(volume, user))
                    return View("AlreadyRequested");
            }
             //code that populates requests table
            //if populate request was suucessful we should change volume state to requested, if it has not already been in that state
            if (PopulateRequest(volume, user))
            {
                UpdateVolumeState(volume);
                return View("Index");
            }
            else
            {
                return Content("Saving to request database failed");
            }            
                                      
        }

        public bool HasBeenRequested(FileVolumes v, AdUser u)
        {
          //inspect
            var userReq = _context.Requests.Where(r => r.FileVolumesId == v.Id).Where(r=>r.UserId == u.Id).ToList();

            if (userReq.Count >= 1)
                return true;
            return false;
        }

        // GET pendingFiles
        [Authorize(Roles = Role.Registry)]
        public ActionResult PendingFiles()
        {           
            return View();
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult GetPendingFiles()
        {
            byte Pending = 1;
            var pendingRequests = _context.Requests.Include(r => r.FileVolumes).
                Include(r => r.User.Branches).Where(r => r.RequestStatusId == Pending).ToList();


            return Json(new { data = pendingRequests }, JsonRequestBehavior.AllowGet);
        }

        //return username only and discards string 'DEVFINCO\\'. Reason being our database sets username without the devfinco literal
        public string ParseUsername(string adName)
        {
            string newName = "";
            if (adName.Contains("DEVFINCO"))
                newName = adName.Remove(0, 9);
            return newName;
        }

        //populates request table with data
        public bool PopulateRequest(FileVolumes v, AdUser u) {
            var requestRecord = new Request()
            {
                //FileId = f.Id,
                UserId = u.Id,
                FileVolumesId = v.Id,
                BranchesId = u.BranchesId,
                RequestStatusId = 1, //1 signifies pending
                ReturnStateId = 1,
                RequestDate = DateTime.Now,
            };
            _context.Requests.Add(requestRecord);

            if (_context.SaveChanges() > 0) {
                return true;
            }
            return false;
        }

        //updates the state of the volume being requested
        public void UpdateVolumeState(FileVolumes v)
        {
            if (v.StatesId == 1) //1 meaning state is at stored
            {
                v.StatesId = 2; //state should now be changed to 2 (requested state) since file request is made
                _context.SaveChanges();
            }
            //if it is not in STORED state, it should already be in request state as other user may have already requested thus changing 
            //the state then. In that case we should simply do nothing
        }

        public void AcceptRequest(int id)
        {
            
          const byte acceptedState = 2;
           var request = _context.Requests.Single(r => r.Id == id);

            request.RequestStatusId = acceptedState;
            //request.AcceptedBy
            int volId = request.FileVolumesId;

            _context.SaveChanges();

            UpdateVolumeState(volId);
            // Here's where you do stuff.

        }

        //here we accept the request id and change its status to Deny/reject
        public void DeclineRequest(int id)
        {
            const byte rejectedState = 3;
            var request = _context.Requests.Single(r => r.Id == id);

            request.RequestStatusId = rejectedState;

            _context.SaveChanges();
            //say for instance registry rejects a file, recall the request nonetheless changed the volume state to 
            //requested, when we deny a request we do not perform any changing of state so what operation resolves the issue.
            //after a volume's been rejected, better yet, let all request for that specific volume be denied.
            //What do we do then? since the state will remain at requested and never changed due to it never being accepted.
            //does this affect the flow of things
           /* if (CheckVolumesIfAllRejected(request.FileVolumesId))
            {
                ChangeVolStateToStored(request.FileVolumesId);
            }*/

        }

       /* public bool CheckVolumesIfAllRejected(int volumeId)
        {
            var requestsForVol = _context.Requests.Where(r => r.FileVolumesId == volumeId).ToList();

            var requestForVolWithRejected = _context.Requests.Where(r => r.FileVolumesId == volumeId)
                .Where(r => r.RequestStatusId == 3).ToList();
            if (requestsForVol.Count == requestForVolWithRejected.Count)
                return true;
            return false;
        }

        public void ChangeVolStateToStored(int volId)
        {
            const byte storedState = 1;
            var vol = _context.FileVolumes.Single(v => v.Id == volId);
            vol.StatesId = storedState;

            _context.SaveChanges();
        }
        */
        //note tha once a file vol is accepted by reg that vol should no longer be in a stored or requested state but
        //rather a transferred state
        public void UpdateVolumeState(int volumeId)
        {
           const byte transferState = 4;
           var volume =  _context.FileVolumes.Single(v => v.Id == volumeId);
           volume.StatesId = transferState;

           _context.SaveChanges();
        }

        //will check if user's branch matches the files current location
        public bool CheckBranchValidity(AdUser u, FileVolumes v) {
            if (u.BranchesId == v.CurrentLocation)
                return true;
            return false;
        }
        [Authorize(Roles = Role.RegularUser)]
        public ActionResult ConfirmCheckout()
        {
            byte reqStatus = 2;

            string uname = ParseUsername(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == uname);

            var request = _context.Requests.Include(r=>r.FileVolumes).Where(r => r.UserId == user.Id).
                Where(r=>r.RequestStatusId == reqStatus).Where(r=>r.IsConfirmed == false).ToList();
           
            return View("ConfirmCheckout", request);
        }

        public void AcceptCheckout(int id)
        {
            //search in request table based on the id parameter
            //change IsConfirmed field to true, meaning file is now checked out by user
            //call and implement a function that updates the file volumes table, specifically the state to the volume
            //from TRANSFERRED --> CHECKED OUT

            //END INTERFACE
            var requestRecord = _context.Requests.Single(r => r.Id == id);

            requestRecord.IsConfirmed = true;
            _context.SaveChanges();

            CheckoutVolume(requestRecord.FileVolumesId);

        }

        public void CheckoutVolume(int volId)
        {
            const byte checkoutState = 5;
            var volume = _context.FileVolumes.Single(v => v.Id == volId);
            volume.StatesId = checkoutState;

            _context.SaveChanges();
        }
    }
    
}