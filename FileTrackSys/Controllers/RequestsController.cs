﻿using System;
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

        //will check if user's branch matches the files current location
        public bool CheckBranchValidity(AdUser u, FileVolumes v)
        {
            if (u.BranchesId == v.CurrentLocation)
                return true;
            return false;
        }

        [Route("Requests/Index/{volId}")]
        public ActionResult Index(int volId)
        {
            //pulls records associated with a request such as volume and user
            var userObj = new AdUser(User.Identity.Name);

            var volume = _context.FileVolumes.Single(v => v.Id == volId);

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);

            //check if this volume number has not already been requested by this user
            if (volume.StatesId == 1)
            {
                if (HasBeenRequested(volume, user))
                    return View("AlreadyRequested");
            }

            if (CheckBranchValidity(user, volume))
            {
                if (PopulateRequest(volume, user))
                {
                    UpdateVolumeState(volume);
                    return View();
                }
                else
                    return Content("Saving to request database failed");
               
            }
            else
            {
                if (PopulateExternalRequests(volume, user))
                    return Content("The populate external requests was populated. Verify that 2 records are add to the database on your server sql");
                else
                {
                    return Content(
                        "Sorry it appears something occured with the methods of inserting into your database. Check your query or logic");
                }
            }

                                      
        }

        //checks that a volume is not requested more than once by the same user
        public bool HasBeenRequested(FileVolumes v, AdUser u)
        {
          //First we query based in current user, then we get the volume id, and finally if the request is active.
            var userReq = _context.Requests.Where(r => r.UserId == u.Id).Where(r => r.FileVolumesId == v.Id)
                .Where(r=>r.IsRequestActive == true).ToList();

            if (userReq.Any())
                return true;
            return false;
        }
        //populates request table with data
        public bool PopulateRequest(FileVolumes v, AdUser u)
        {
            var requestRecord = new Request()
            {
                //FileId = f.Id,
                UserId = u.Id,
                FileVolumesId = v.Id,
                RequesteeBranch = v.CurrentLocation,//the requesteeBranch is the brnacch where the request will be sent to, based in the volume's current location
                BranchesId = u.BranchesId,//we get the branch based on the signed on user, since user must match volume branch atm
                RequestStatusId = 1, //1 signifies pending
                ReturnStateId = 1,//1 signifies idle state, meaning the return process is not in order
                IsRequestActive = true,//meaning this request has been initiated, switched to false when file is returned and back to stored state
                RequestDate = DateTime.Now,//assigns immediate date as the request was made in this moment of time
            };
            _context.Requests.Add(requestRecord);

            if (_context.SaveChanges() > 0)
                return true;
            
            return false;
        }

        public bool PopulateExternalRequests(FileVolumes v, AdUser u)
        {
            //line below holds a value we will use to bind both requests and then increment thereafter assignment.
            var bindVal = _context.ExternalRequestsBinder.Single(b => b.Id == 1);

            List<Request> externalReqRecords = new List<Request>(2);
             externalReqRecords.Add(new Request()
             {
                 //here we will add the first record, which is the local registry request to external registry

                 UserId = u.Id,//0 doesn't work so we try must use another determining value
                 FileVolumesId = v.Id, 
                 RequesteeBranch = v.CurrentLocation,//In this case requestee branch is assigned to the external location that corresponds to the vol loc
                 BranchesId = u.BranchesId,//requesting user's location which will ofc differ from requestee branch
                 RequestStatusId = 1, //1 signifies pending
                 ReturnStateId = 1,//1 signifies idle state, meaning the return process is not in order
                 IsRequestActive = true,//meaning this request has been initiated, switched to false when file is returned and back to stored state
                 RequestDate = DateTime.Now,
                 RequestBinder = bindVal.CurrentNumberBinder
             });
             externalReqRecords.Add(new Request()
             {
                 UserId = u.Id,//the user's, making the request, id
                 FileVolumesId = v.Id,
                 RequesteeBranch = u.BranchesId,//In this case requestee branch is assigned to user making the request's branch, as he must request from his respective branch eventually
                 BranchesId = u.BranchesId,//requesting user's location which will ofc differ from requestee branch
                 RequestStatusId = 1, //1 signifies pending
                 ReturnStateId = 1,//1 signifies idle state, meaning the return process is not in order
                 IsRequestActive = false,//Since this vol is depending on the above request to be confrimed, it's default val is false, until the above get confirmed
                 RequestDate = DateTime.Now,
                 RequestBinder = bindVal.CurrentNumberBinder
             });
            //for the above 2 requests we must have a binding value, a unique identifier that only those 2 will have to know to move on to the next request after the first is executed
             foreach (var req in externalReqRecords)
             {
                _context.Requests.Add(req);
             }

             bindVal.CurrentNumberBinder++;//we increment the value in order to generate a unique value on another instance
             if (_context.SaveChanges() > 0)//confirms changes
                 return true;
             return false;
        }

        //updates the state of the volume being requested
        public void UpdateVolumeState(FileVolumes v)
        {
            if (v.StatesId != 1) //1 meaning state is at stored
            {
                v.StatesId = 1; //state should now be changed to 2 (requested state) since file request is made
                _context.SaveChanges();
            }
            //if it is not in STORED state, it should already be in request state as other user may have already requested thus changing 
            //the state then. In that case we should simply do nothing
        }

        // GET pendingFiles
        [Authorize(Roles = Role.Registry)]
        public ActionResult PendingFiles()
        {           
            return View();
        }

        //sends all request records with a Request status of pending to be approved by registry
        [Authorize(Roles = Role.Registry)]
        public ActionResult GetPendingFiles()
        {
            //we must ensure to take into account branches. registry is only to see request from user made within their respective branch
            var userObj = new AdUser(User.Identity.Name);
           
            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);
            byte Pending = 1;
            var pendingRequests = _context.Requests.Include(r => r.FileVolumes).
                Include(r => r.User.Branches).Where(r=>r.RequesteeBranch == user.BranchesId).Where(r => r.RequestStatusId == Pending).
                Where(r=>r.RequestBinder == 0).Where(r=>r.IsRequestActive == true).ToList();


            return Json(new { data = pendingRequests }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult ExternalBranchRequest()
        {
            return View();
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult GetExternalBranchPendingFiles()
        {
            var userObj = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);
      
            byte Pending = 1;
            var pendingRequests = _context.Requests.Include(r => r.FileVolumes).
                Include(r => r.User.Branches).Where(r=>r.RequesteeBranch == user.BranchesId).Where(r => r.RequestStatusId == Pending)
                .Where(r => r.IsRequestActive == true).Where(r=>r.RequestBinder != 0).ToList();


            return Json(new { data = pendingRequests }, JsonRequestBehavior.AllowGet);
        }

        //function is invoked when a registry user accepts a file, changing it to check out
        public void AcceptRequest(int id)
        {
            var userObj = new AdUser(User.Identity.Name);
            //recall that a person from registry accepts this request so get person name
          const byte acceptedState = 2;
           var request = _context.Requests.Single(r => r.Id == id);
         

            request.RequestStatusId = acceptedState;
            request.AcceptedBy = userObj.Username;
            request.AcceptedDate = DateTime.Now;
            //request.UserId = 
            //request.AcceptedBy
            int volId = request.FileVolumesId;

            _context.SaveChanges();

            UpdateVolumeState(volId);
            // Here's where you do stuff.

        }
        //note tha once a file vol is accepted by reg that vol should no longer be in a stored or requested state but
        //rather a transferred state
        public void UpdateVolumeState(int volumeId)
        {
            const byte transferState = 4;
            var volume = _context.FileVolumes.Single(v => v.Id == volumeId);
            volume.StatesId = transferState;

            _context.SaveChanges();
        }

        //here we accept the request id and change its status to Deny/reject
        public void DeclineRequest(int id)
        {
            const byte rejectedState = 3;
            var request = _context.Requests.Single(r => r.Id == id);

            request.RequestStatusId = rejectedState;
            request.IsRequestActive = false;

            _context.SaveChanges();
            //say for instance registry rejects a file, recall the request nonetheless changed the volume state to 
            //requested, when we deny a request we do not perform any changing of state so what operation resolves the issue.
            //after a volume's been rejected, better yet, let all request for that specific volume be denied.
            //What do we do then? since the state will remain at requested and never changed due to it never being accepted.
            //does this affect the flow of things
        }

        [Authorize(Roles = Role.RegularUser)]
        public ActionResult ConfirmCheckout()
        {
            return View();
        }

        [Authorize(Roles = Role.RegularUser)]
        public ActionResult GetConfirmCheckout()
        {
            byte reqStatus = 2;

           var userObj = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);

            var request = _context.Requests.Include(r=>r.FileVolumes).Where(r => r.UserId == user.Id).
                Where(r=>r.RequestStatusId == reqStatus).Where(r=>r.IsConfirmed == false).ToList();

            return Json(new { data = request }, JsonRequestBehavior.AllowGet);
            
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
            //perhaps here we create a function that performs notifications

        }

        public void CheckoutVolume(int volId)
        {
           
            var userObj = new AdUser(User.Identity.Name);
            var currUser = _context.AdUsers.Single(u => u.Username == userObj.Username);

            const byte checkoutState = 5;
            var volume = _context.FileVolumes.Single(v => v.Id == volId);
            volume.StatesId = checkoutState;
            volume.AdUserId = currUser.Id; 
            _context.SaveChanges();
        }

        public ActionResult UserNotification()
        {
            var userObj = new AdUser(User.Identity.Name);
            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);
            var Notifications = _context.Requests.Include(r => r.FileVolumes).Where(r => r.UserId == user.Id).Where(r => r.IsRequestActive == true)
                .Where(r => r.RequestStatusId == 2).Where(r => r.IsConfirmed == false).ToList();

            return View("UserNotification", Notifications);
        }
    }

}