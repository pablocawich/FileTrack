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
            if (HasBeenRequested(volume, user))
                return View("AlreadyRequested");

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
            var userReq = _context.Requests.Where(r => r.FileVolumesId == v.Id).Where(r=>r.UserId == u.Id).ToList();

            if (userReq.Any())
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
        }

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
    }
    
}