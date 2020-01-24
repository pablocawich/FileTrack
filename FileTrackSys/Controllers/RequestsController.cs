using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FileTracking.Models;
using FileTracking.ViewModels;
using Microsoft.Ajax.Utilities;
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
       

        public ActionResult VolumeStateNotValid()
        {
            return View();
        }

        //this is invoked whenever a user makes a request from the 'FileVolume' page where the parameters with the exact volume is provided.
        [Route("Requests/Index/{volId}")]
        [Authorize(Roles = Role.RegularUser)]
        public ActionResult Index(int volId)
        {
            //pulls records associated with a request such as volume and user
            var userObj = new AdUser(User.Identity.Name);          

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);
            
            if (!user.IsDisabled)//user not disabled
            {
                var volume = _context.FileVolumes.Single(v => v.Id == volId);

                //check if this volume number has not already been requested by this user
                if (volume.StatesId == 1 && HasBeenRequested(volume, user))
                {
                   return View("AlreadyRequested");
                }

                if (user.BranchesId == volume.CurrentLocationId && volume.CurrentLocationId == volume.BranchesId)//will check if user branch matches the current file branch, makes internal requests
                {
                    //local request 
                    if (PopulateRequest(volume, user))
                    {
                        UpdateVolumeState(volume);
                        return View();
                    }
          
                    
                }
                else if(user.BranchesId != volume.BranchesId && volume.BranchesId == volume.CurrentLocationId)
                {
                    //external request
                    if (PopulateExternalRequests(volume, user))
                        return View("ExternalRequestMade");
                    else
                        return Content("Sorry something went wrong with the methods of inserting into your database. Check your query or logic");
                }
            }

            return View("Locked");
        }

        //checks that a volume is not requested more than once by the same user
        public bool HasBeenRequested(FileVolumes v, AdUser u)
        {
          //First we query based in current user, then we get the volume id, and finally if the request is active.
            var userReq = _context.Requests.AsNoTracking().Where(r => r.UserId == u.Id).Where(r => r.FileVolumesId == v.Id)
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
                RecipientBranchId = v.CurrentLocationId,//the recipient Branch is the branch where the request will be sent to, based in the volume's current location
                RequesterBranchId = u.BranchesId,//we get the branch based on the signed on user, since user must match volume branch atm
                RequestStatusId = 1, //1 signifies pending
                ReturnStateId = 1,//1 signifies idle state, meaning the return process is not in order
                IsRequestActive = true,//meaning this request has been initiated, switched to false when file is returned and back to stored state
                RequestDate = DateTime.Now,//assigns immediate date as the request was made in this moment of time
                RequestTypeId = RequestType.InternalRequest
            };
            _context.Requests.Add(requestRecord);

            if (_context.SaveChanges() > 0)
            {
                //sending a notification to corresponding registry
                var notify = new NotificationsController();
                notify.NotificationForInitialRequest(requestRecord, Message.PendingFile);
                return true;
            }
           
            return false;
        }

        [Authorize(Roles = Role.RegularUser)]
        public bool PopulateExternalRequests(FileVolumes v, AdUser u)
        {
            //line below holds a value we will use to bind both requests and then increment thereafter the assignment.
            var bindVal = _context.ExternalRequestsBinder.Single(b => b.Id == 1);

            var externalRequestRec = new Request()
            {
                UserId = u.Id,//0 doesn't work so we try must use another determining value
                FileVolumesId = v.Id,
                RecipientBranchId = v.BranchesId,//we have a bad naming convention here, current file branch should actually be the origins
                RequesterBranchId = u.BranchesId,//requesting user's location which will ofc differ from file branch
                RequestStatusId = 1, //1 signifies pending
                ReturnStateId = 1,//1 signifies idle state, meaning the return process is not in order
                IsRequestActive = true,//meaning this request has been initiated, switched to false when file is returned and back to stored state
                RequestDate = DateTime.Now,
                RequestBinder = bindVal.CurrentNumberBinder,//This identifier will bind these two newly created requests
                RequestTypeId = RequestType.ExternalRequest
            };          
             
            //for the above 2 requests we must have a binding value, a unique identifier that only those 2 will have to know to move on to the next request after the first is executed            
             _context.Requests.Add(externalRequestRec);
             
             bindVal.CurrentNumberBinder++;//we increment the value in order to generate a unique value on another instance

             if (_context.SaveChanges() > 0)
             {
                 var notify = new NotificationsController();
                 notify.NotificationForInitialRequest(externalRequestRec, Message.ExternalPending);
                return true;
             } //confirms changes

             
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
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult PendingFiles()
        {           
            var currentUser = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == currentUser.Username);

            if (user.IsDisabled)
                return View("Locked");

            return View();
        }

        //sends all request records with a Request status of pending to be approved by registry
        [Authorize(Roles = Role.Registry)]
        public ActionResult GetPendingFiles()
        {          
            //we must ensure to take into account branches. registry is only to see request from user made within their respective branch
            var userObj = new AdUser(User.Identity.Name);
           
            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);

            if (user.IsDisabled)
                return View("Locked");
            
            //PENDING = 1 (RequestStatusID)
            //where a User RequesteeFrom field is NULL
            var pendingRequests = _context.Requests.Include(r => r.FileVolumes).
                Include(r => r.User.Branches).Where(r => r.RecipientBranchId == user.BranchesId).Where(r => r.RequestStatusId == 1).
                Where(r => r.IsRequestActive == true).Where(r => r.RequestTypeId == RequestType.InternalRequest)
                .Where(r => r.UserRequestedFromId == null).ToList();

            return Json(new { data = pendingRequests}, JsonRequestBehavior.AllowGet);
           
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult ExternalBranchRequest()
        {
            var userObj = new AdUser(User.Identity.Name);
            var userInSession = _context.AdUsers.Single(u => u.Username == userObj.Username);

            if (userInSession.IsDisabled)
                return View("Locked");
            return View();
        }

        //Gets files that are awaiting external approval
        [Authorize(Roles = Role.Registry)]
        public ActionResult GetExternalBranchPendingFiles()
        {
            var userObj = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);
      
            byte Pending = 1;
            var pendingRequests = _context.Requests.Include(r => r.FileVolumes).Include(r => r.User.Branches).
                Where(r=>r.RecipientBranchId == user.BranchesId).Where(r => r.IsRequestActive == true).
                Where(r => r.RequestStatusId == Pending).Where(r => r.RequestTypeId == RequestType.ExternalRequest).ToList();
            //4 => never received, was deprecated
            
            return Json(new { data = pendingRequests }, JsonRequestBehavior.AllowGet);
        }

        //check if a requested state has not been already checked out or in the transfer state
        public bool IsVolumeStateValid(int volId)
        {
            var volumeInDb = _context.FileVolumes.Single(v=>v.Id == volId);
            //1 = Stored state
            if (volumeInDb.StatesId == 1) {
                return true;
            }
            return false;
        }

        //if in fact other requests were made for the same volume and one gets accepted we cancel the other
        public bool CancelIfOtherRequests(int volumeId, int excludeRequest)
        {
            var requestsInDb = _context.Requests.Where(r => r.FileVolumesId == volumeId).Where(r => r.Id != excludeRequest)
                .Where(r => r.IsRequestActive == true).Where(r => r.RequestStatusId == 1).ToList();

            if (requestsInDb.Any())
            {
                foreach (var req in requestsInDb)
                {
                    req.IsRequestActive = false;//here we also need to update the notification system when its implemented (for a later time)
                    req.RequestStatusId = 3; //we set the request status to rejected
                    req.AcceptedDate = DateTime.Now;//get the date denial was made

                    //here we need to notify other user, so we call the createNotification() function 
                    var notify = new NotificationsController();
                    notify.CreateNotification(req, Message.InReject);

                    var rejectedRequestOperation = new RejectedRequestController();//saving to new table
                    rejectedRequestOperation.SaveToRejectedRequestTable(req);//deleting from this table

                    _context.Requests.Remove(req);
                    //.....
                }

                _context.SaveChanges();
                return true;
            }

           return false;
        }

        //function is invoked when local registry user accepts a file, changing it to check out
        public JsonResult AcceptRequest(int id)
        {            
            var userObj = new AdUser(User.Identity.Name);
            var userInSessionInDb = _context.AdUsers.Single(u => u.Username == userObj.Username);

            if (!userInSessionInDb.IsDisabled)//user not disabled. proceed
            {
                var request = _context.Requests.Include(r=>r.User).Single(r => r.Id == id);
                if (IsVolumeStateValid(request.FileVolumesId))
                {
                    request.RequestStatusId = 2;//2 ---> Accepted state
                    request.AcceptedById = userInSessionInDb.Id;
                    request.AcceptedDate = DateTime.Now;
                    //request.UserId =                   
                       
                    _context.SaveChanges();
                    UpdateVolumeState(request.FileVolumesId);

                    var notify = new NotificationsController();
                    notify.CreateNotification(request, Message.InAccept);

                    if (CancelIfOtherRequests(request.FileVolumesId, request.Id))//if true, we have other requests for the same file thus canceling them
                        return this.Json(new { success = false, message = $"File Checked out to {request.User.Name}. Due to requests being made to this same file a reload is necessary." }, JsonRequestBehavior.AllowGet);
                    //not other requests 
                    return this.Json(new { success = true, message = $"Checked Out to {request.User.Username}. Kindly await confirmation from user." }, JsonRequestBehavior.AllowGet);
                }
                return this.Json(new { success = false,message = "File already checked out or is being transferred", opt=2 }, JsonRequestBehavior.AllowGet);

            }
           return this.Json(new { success = false, message = "You are not allowed to be in this system. Kindly Exit!" }, JsonRequestBehavior.AllowGet);
        }

        //for external requests, as opposed to the above's local request
        public ActionResult AcceptExternalUserRequest(int id)
        {
            var userObj = new AdUser(User.Identity.Name);
            var userInSessionInDb = _context.AdUsers.Single(u => u.Username == userObj.Username);
            //recall that a person from registry accepts this request so get person name

            var request = _context.Requests.Include(r=>r.User).Single(r => r.Id == id);

            if (request.RequestStatusId == 1) //ensure the file is still pending
            {
                if (IsVolumeStateValid(request.FileVolumesId))
                {
                    //we proceed with processing the request since no other request to the same volume was found
                    request.RequestStatusId = 2;//accepted state
                    request.AcceptedById = userInSessionInDb.Id;
                    request.AcceptedDate = DateTime.Now;
                    //request.UserId = 

                    _context.SaveChanges();
                    UpdateVolumeState(request.FileVolumesId);

                    var notify = new NotificationsController();
                    notify.CreateNotification(request, Message.ExAccept);//this notification only informs the acting user

                    //since it's an external request, the registry also needs to be informed, function below does this.
                    notify.CreateInitialExternalRequest(request, Message.ExternalRoute);

                    //note: this does not cancel this requests, only those who have requested the same file
                    if (CancelIfOtherRequests(request.FileVolumesId, request.Id))
                        return this.Json(new { success = true, message = $"File Checked-Out to {request.User.Name}. Due to requests being made to this same file a reload is necessary.", opt = 1 }, JsonRequestBehavior.AllowGet);
                    return this.Json(new { success = true, message = $"Checked-Out to {request.User.Name}.", opt = 2 }, JsonRequestBehavior.AllowGet);
                }
            }
            return this.Json(new { success = false, message = "Something went wrong. File already checked out or is being transferred" }, JsonRequestBehavior.AllowGet);
        }

        //note tha once a file vol is accepted by reg that vol should no longer be in a stored or requested state but
        //rather a transferred state
        public void UpdateVolumeState(int volumeId)
        {
            //Request State 4 => transfer state
            var volume = _context.FileVolumes.Single(v => v.Id == volumeId);
            if (volume.StatesId == 4)
                return;           
            volume.StatesId = 4;

            _context.SaveChanges();
        }

        //here we get the request id and change its status to Deny/reject
        public JsonResult DeclineRequest(int id)
        {
            var adUser = new AdUser(User.Identity.Name);
            var userInSessionInDb = _context.AdUsers.Single(u => u.Username == adUser.Username);

            var request = _context.Requests.Single(r => r.Id == id);

            if (request.RequestStatusId == 1)//still pending
            {
                request.RequestStatusId = 3;// 3 rejected state
                request.IsRequestActive = false;
                request.AcceptedById = userInSessionInDb.Id;//Identifies the user who rejected the requests rather than who accept. Terminology mishap
                request.AcceptedDate = DateTime.Now;
                _context.SaveChanges();


                //creates the notifications based on binder which distinguishes between internal and external requests
                var notify = new NotificationsController();
                
                if (request.RequestBinder == 0)
                    notify.CreateNotification(request, Message.InReject);
                else if (request.RequestTypeId == RequestType.ExternalRequest)
                    notify.CreateNotification(request, Message.ExReject);

                //deleting this record and adding to our specially made RejectedRequest Table
                var rejectedRequestOperation = new RejectedRequestController();
                rejectedRequestOperation.SaveToRejectedRequestTable(request);

                _context.Requests.Remove(request);
                _context.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult ExternalTransferApproval()
        {
            return View();
        }

        //After getting external approval a file must then make its way back to internal registry to gain further confirmation
        [Authorize(Roles = Role.Registry)]
        public ActionResult GetExternalTransferRecords()
        {

            var userObj = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);

            var request = _context.Requests.Include(r => r.FileVolumes).Include(r => r.User.Branches).Include(r=>r.AcceptedBy)
                .Include(r=>r.RecipientBranch).Where(r=>r.RequesterBranchId == user.BranchesId).Where(r=>r.RequestTypeId == RequestType.ExternalRequest).
                Where(r => r.RequestStatusId == 2).Where(r => r.IsConfirmed == false).ToList();

            return Json(new { data = request }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = Role.RegularUser)]
        public ActionResult ConfirmCheckout()
        {
            var currentUser = new AdUser(User.Identity.Name);
            try
            {
                var userInDb = _context.AdUsers.Single(u => u.Username == currentUser.Username);

                if (userInDb.IsDisabled == false)
                {
                    return View();
                }

                return View("Locked");
            }
            catch (Exception e)
            {
                return HttpNotFound(e.Message);
            }

        }

        //where REGULAR USERS will be able to see those requests accepted by registry
        [Authorize(Roles = Role.RegularUser)]
        public ActionResult GetConfirmCheckout()
        {
           var userObj = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);

            var request = _context.Requests.Include(r=>r.FileVolumes.Branches).Include(r=>r.RequesterBranch).Include(r=>r.AcceptedBy)
                .Where(r => r.UserId == user.Id).Where(r=>r.IsRequestActive == true)//2 --> Accepted State
                .Where(r=>r.RequestTypeId == RequestType.InternalRequest).Where(r=>r.RequestStatusId == 2).Where(r=>r.IsConfirmed == false).
                Where(r=>r.UserRequestedFromId == null).ToList();
            //UserRequestedFromId = = NULL signifies we only get accepted files from REGISTRY.
            return Json(new { data = request }, JsonRequestBehavior.AllowGet);
            
        }

        //CONFIRM checkout interface
        public void AcceptCheckout(int id)
        {
            var requestRecord = _context.Requests.Single(r => r.Id == id);

            requestRecord.IsConfirmed = true;
            _context.SaveChanges();
            //Go to volumes table and update appropriate fields
            CheckoutVolume(requestRecord.FileVolumesId, requestRecord.UserId);

        }

        //the external registry interaction confirmation for file transfer
        [Authorize(Roles = Role.Registry)]
        public JsonResult AcceptForeignRegistryTransfer(int id)
        {
            //request binded records that sees to registry to registry operations before initiating the local request it is bound by
            var requestInDb = _context.Requests.Include(r=>r.User).SingleOrDefault(r => r.Id == id);
            //check if we don't already have a record for this file created so we don't tolerate duplicates
            
            if (requestInDb != null && !(HasFileRouteRequestBeenCreated(id, requestInDb.FileVolumesId,requestInDb.UserId)))
            {
                requestInDb.IsConfirmed = true;
                requestInDb.IsRequestActive = false;//mark record temporarily inactive
                _context.SaveChanges();

                //after the above is performed we let its binded request become active in order for the initiating user to go through the local request process   

                var volume = _context.FileVolumes.Single(v => v.Id == requestInDb.FileVolumesId);

                //creating a new record to track the local processes of the file when in an external branch
                PopulateNewInternalRequest(requestInDb.UserId, requestInDb.RequesterBranchId, requestInDb.RequestDate, volume, requestInDb.RequestBinder);//both registry passed, now we create an internal request

                volume.CurrentLocationId = requestInDb.RequesterBranchId; //we must change the volume's current location to the current user's branch
                _context.SaveChanges();

                var notify = new NotificationsController();
                notify.CreateNotification(requestInDb, Message.InAccept);//revise
                return this.Json(new { success = true, message = $"{requestInDb.User.Name}'s external file transfer has been approved. Kindly await user retrieval" }, JsonRequestBehavior.AllowGet);
            }
            return this.Json(new { success = false, message = "Something wrong seems to have occured. try reloading the page and try again." }, JsonRequestBehavior.AllowGet);

        }

        //ensures a internal file record for an associated record is not yet created
        public bool HasFileRouteRequestBeenCreated(int reqId, int volId, int userId)
        {
            var requests = _context.Requests.AsNoTracking().Where(r=>r.Id != reqId && r.FileVolumesId == volId && r.UserId == userId && 
                                                      r.RequestTypeId != RequestType.ExternalRequest && r.IsRequestActive == true).ToList();
            if(requests.Any())
                return true;
            return false;
        }

        //now that user's local branch has accepted 
        public void PopulateNewInternalRequest(int userId, byte userBranchId, DateTime reqDate,FileVolumes v, int binder)
        {
            var user = new AdUser(User.Identity.Name);
            var userInSessionInDb = _context.AdUsers.Single(u => u.Username == user.Username);

            var internalRequest = new Request()
            {
                UserId = userId,//the user's, making the request, id
                FileVolumesId = v.Id,
                RecipientBranchId = userBranchId,//the branch that should be receiving this is the user's branch registry, so we just assign the user's branch
                RequesterBranchId = userBranchId,//requesting user's location
                RequestStatusId = 2, //2 signifies accepted
                ReturnStateId = 1,//1 signifies idle state, meaning the return process is not in order
                IsRequestActive = true,//Since this vol is depending on the above request to be confrimed, it's default val is false, until the above get confirmed
                RequestDate = reqDate,
                RequestBinder = binder,
                RequestTypeId = RequestType.InternalRequest,
                AcceptedById = userInSessionInDb.Id,
                AcceptedDate = DateTime.Now
            };
            _context.Requests.Add(internalRequest);
            _context.SaveChanges();
        }        

        public void CheckoutVolume(int volId, int requester)
        {          
            var volume = _context.FileVolumes.Single(v => v.Id == volId);
            volume.StatesId = 5;//5 => Checked out state
            volume.AdUserId = requester; 
            _context.SaveChanges();
        }

        public void UpdateVolumeStateStored(int volumeId)
        {
            const byte stored = 1;
            var volume = _context.FileVolumes.Single(v => v.Id == volumeId);
            volume.StatesId = stored;

            _context.SaveChanges();
        }

        /*----------------------------------------------------------------------------------------------------------------------------------
        ----------------------------------Initiating User Transfer functionality ----------------------------------------------------------- 
        ----------------------------------------------------------------------------------------------------------------------------------*/
        
        //When a user makes attempts to have a file transferred to them this function is invoked to create a new record
        [Authorize(Roles = Role.RegularUser)]
        [Route("Requests/OnUserTransferRequest/{volId}/{userId}/{currentLocation}")]
        public ActionResult OnUserTransferRequest(int volId, int userId, byte currentLocation)
        {           
            var userObj = new AdUser(User.Identity.Name); //user in session

            var thisUser = _context.AdUsers.Single(u => u.Username == userObj.Username);//user in session dataBase information

            if (thisUser.IsDisabled)
                return View("Locked");

            if (thisUser.Id == userId)//checks that user in session is not requesting from his/her own self
                return HttpNotFound("It appears your request of transfer is being processed to yourself. Cannot proceed.");

            var volInDb = _context.FileVolumes.Single(v => v.Id == volId);

            if (volInDb.StatesId != 5 || volInDb.AdUserId == null) //checks is file volumes is eligible for a transfer
                return View("VolumeStateNotValid");

            if (HasBeenRequested(volInDb, thisUser))//if request has not been already made
                return View("AlreadyRequested");

           //creating new request
            var newRequest = new Request() //creating that new request record
            {
                UserId = thisUser.Id,//this is the new user making the request
                FileVolumesId = volId,
                RecipientBranchId = currentLocation,//the requesteeBranch is the brnacch where the request will be sent to, based in the volume's current location
                RequesterBranchId = thisUser.BranchesId,//we get the branch based on the signed on user, since user must match volume branch atm
                RequestStatusId = 1, //1 signifies pending
                ReturnStateId = 1,//1 signifies idle state, meaning the return process is not in order
                IsRequestActive = true,
                RequestDate = DateTime.Now,
                RequestTypeId = RequestType.InternalRequest,
                UserRequestedFromId = userId //THIS is the user from who the transfer is being requested
            };
            
            _context.Requests.Add(newRequest);
            _context.SaveChanges();

            //creating notification for the transfer request
            var notify = new NotificationsController();
            notify.NotificationForInitialTransfer(newRequest,Message.TransferRequest);

            return View("RequestSuccessful"); 
        }

        //the view page function
        [Authorize(Roles = Role.RegularUser)]
        public ActionResult UserPendingTransfer()
        {
            return View();//Navigational link to page
        }

        //GET: a user's transfers that are pending so the user currently with the file may choose to accept or deny
        [Authorize(Roles = Role.RegularUser)]
        public ActionResult GetUserPendingTransfers()
        {
            var userObj = new AdUser(User.Identity.Name);

            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);

            if (user.IsDisabled)
                return View("Locked");
            
            var request = _context.Requests.Include(r => r.FileVolumes).Include(r=>r.RequesterBranch).Include(r => r.UserRequestedFrom).Include(r=>r.User)
                .Where(r=>r.UserRequestedFromId == user.Id).Where(r=>r.IsRequestActive == true).Where(r=>r.RequestStatusId == 1).ToList();

            return Json(new { data = request }, JsonRequestBehavior.AllowGet);
        }

        //user accepts the pending transfer
        [Authorize(Roles = Role.RegularUser)]
        public JsonResult AcceptTransfer(int id)
        {
            var userInSession = new AdUser(User.Identity.Name);
            var userInSessionInDb = _context.AdUsers.Single(u => u.Username == userInSession.Username);
            //the main request to be worked with
            var requestInDb = _context.Requests.Include(r=>r.User).Single(r => r.Id == id);

            var volumeInDb = _context.FileVolumes.Single(v => v.Id == requestInDb.FileVolumesId);

            //CHECKED OUT state = 5.
            //Recall a file must be checked out to a regular user (Will specify Id, so not NULL is the criteria) in order for transfer to be possible
            if (volumeInDb.AdUserId != null && volumeInDb.StatesId == 5)
            {
                //we are getting the request for the holding user to transfer some content 
                var currentHoldingUserReq = _context.Requests.Single(r => r.UserId == requestInDb.UserRequestedFromId 
                                     && r.IsRequestActive == true &&  r.FileVolumesId == requestInDb.FileVolumesId && r.IsConfirmed == true);

                var binder = currentHoldingUserReq.RequestBinder;//in case the file is acting as an external process

                currentHoldingUserReq.IsRequestActive = false;
                currentHoldingUserReq.ReturnedDate = DateTime.Now;
                currentHoldingUserReq.ReturnAcceptById = requestInDb.User.Id; //the user requesting the transfer will now as the return stage entity, rather than the indicated actor which is a registry user
                currentHoldingUserReq.ReturnStateId = 3; //completing the request cycle and marking the record as returned
                
                //Copying request to COMPLETED-TABLE table and deleting from REQUEST table
                var completedRequestOperation = new CompletedRequestController();
                completedRequestOperation.SaveToCompletedRequestTable(currentHoldingUserReq);
                _context.Requests.Remove(currentHoldingUserReq);

                //Set new record Accept state criteria for the user the file is to be transferred to. 
                requestInDb.RequestStatusId = 2;//2 => accepted state
                requestInDb.AcceptedDate = DateTime.Now;
                requestInDb.AcceptedById = userInSessionInDb.Id;
                requestInDb.RequestBinder = binder;
                //create ACCEPT NOTIFICATION
                var notify = new NotificationsController();
                notify.CreateNotification(requestInDb,Message.TransferAccept);

                //now we change the file volume to TRANSFER state (4) since transfer was granted
                volumeInDb.StatesId = 4;
                _context.SaveChanges();
                //canceling other request to the same file. Except the req id just granted
                if (CancelOtherTransferRequestsIfMoreThanOne(volumeInDb, requestInDb.Id))
                    return this.Json(new { success = true, message = "Transfer Confirmed. A reload of page is required due to multiple request being sent to the same file.", option = 1 }, JsonRequestBehavior.AllowGet);
                return this.Json(new { success = true, message = "Transfer successfully accepted", option = 2 }, JsonRequestBehavior.AllowGet);
            }

            return this.Json(new { success = false, message = "It appears the file has been sent back to registry and cannot commit to transfer." }, JsonRequestBehavior.AllowGet);
        }

        //can reuse the function stated a bit above but for scrolling sake, i'll create another one here where it more fits 
        [Authorize(Roles =  Role.RegularUser)]
        public JsonResult DeclineTransfer(int id)
        {
            var requestInDb = _context.Requests.Single(r=>r.Id == id);
            requestInDb.IsRequestActive = false;
            requestInDb.RequestStatusId = 3;
            requestInDb.AcceptedDate = DateTime.Now;

            _context.SaveChanges();
            //--------copying req data to new table and deleting it from this table--------
            var rejectedRequestOperation = new RejectedRequestController();
            rejectedRequestOperation.SaveToRejectedRequestTable(requestInDb);

            var notify = new NotificationsController();
            notify.CreateNotification(requestInDb, Message.TransferDenied);

            _context.Requests.Remove(requestInDb);
            _context.SaveChanges();

            return this.Json(new { success = true, message = "This transfer request has been declined." }, JsonRequestBehavior.AllowGet);
        }

        //will cancel all other requests to the same file once a request is accepted
        [Authorize(Roles = Role.RegularUser)]
        public bool CancelOtherTransferRequestsIfMoreThanOne(FileVolumes fv, int reqId)
        {
            //cancel every other request record except for the provided in the parameter
            var reqsInDb = _context.Requests.Where(r => r.UserRequestedFromId == fv.AdUserId).Where(r => r.Id != reqId)
                .Where(r => r.FileVolumesId == fv.Id).ToList();
            if (reqsInDb.Any())
            {
                foreach (var req in reqsInDb)
                {
                    req.IsRequestActive = false;
                    //request record will be set to rejected = 3
                    req.RequestStatusId = 3;
                    req.AcceptedDate = DateTime.Now;
                    //we must also make a function that creates a new record in the notifications table, informing th user their file got canceled.
                    NotifyUserOfTransferDenials(req, Message.InReject);
                    var rejectedRequestsOperation = new RejectedRequestController();
                    rejectedRequestsOperation.SaveToRejectedRequestTable(req);

                    _context.Requests.Remove(req);
                    _context.SaveChanges();
                }
                return true;
            }
            return false;
        }

        //Notification of requests being discarded.
        public void NotifyUserOfTransferDenials(Request req, string messageId)
        {
            //REJ - that a file's been rejected
            var notif = new Notification()
            {
                RecipientUserId = req.UserId,
                MessageId = messageId,
                Read = false,
                FileVolumeId = req.FileVolumesId,
                RecipientBranchId = req.RecipientBranchId,
                SenderBranchId = req.RequesterBranchId,
                DateTriggered = DateTime.Now,
                SenderUserId = req.AcceptedById
            };
           _context.Notifications.Add(notif);
        }

        //Creating view navigation Page ---------------------------------------------------
        [Authorize(Roles = Role.RegularUser)]
        public ActionResult UserConfirmTransfer()
        {
            return View();
        }

        //Json request function that returns a serialized list of request objects
        [Authorize(Roles = Role.RegularUser)]
        public JsonResult GetConfirmationTransfers()
        {
            var userInSession = new AdUser(User.Identity.Name);

            var adUserInDb = _context.AdUsers.Single(u => u.Username == userInSession.Username);

            var requestsInDb = _context.Requests.Include(r=>r.User).Include(r=>r.UserRequestedFrom).Include(r=>r.FileVolumes).Include(r=>r.RequesterBranch).
                Where(r => r.UserId == adUserInDb.Id).Where(r=>r.UserRequestedFromId != null).Where(r => r.IsConfirmed == false).
                Where(r=>r.RequestStatusId == 2).Where(r => r.IsRequestActive == true).ToList();

            return Json(new { data = requestsInDb }, JsonRequestBehavior.AllowGet);
        }
        
        //Function that handles confirmation button functionality
        [Authorize(Roles = Role.RegularUser)]
        public JsonResult OnConfirmAccept(int id)
        {
            var adUserObj = new AdUser(User.Identity.Name);
            var userInSession = _context.AdUsers.Single(u => u.Username == adUserObj.Username);
            //user account not disable carry on
            if (!userInSession.IsDisabled)
            {
                var requestInDb = _context.Requests.Include(r=>r.AcceptedBy).Single(r => r.Id == id);
                requestInDb.IsConfirmed = true;
                requestInDb.AcceptedDate = DateTime.Now;
                if (requestInDb.RequestTypeId ==  RequestType.DirectTransfer)
                {
                    //look for bounded request to relieve the previous req to file off it's responsibility.
                    DeleteBoundedRequest(requestInDb.Id, requestInDb.FileVolumesId, userInSession.Id);
                }

                _context.SaveChanges();

                //volume record must be updated
                CheckoutVolume(requestInDb.FileVolumesId, userInSession.Id);

                //FROM obj in the return statement indicates from whom the confirmation is being made in response to.
                return Json(new { success = true, message = "Confirmation Successful", from = requestInDb.AcceptedBy.Name}, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "This account is not active. Kindly exit" }, JsonRequestBehavior.AllowGet);
        }

        //Initiating direct transfer ----------------------------------------------------------------------------

        [Authorize(Roles = Role.RegularUser)]
        public ActionResult DirectTransferModal(int id)
        {
            var request = _context.Requests.Single(r => r.Id == id);
            var userList = _context.AdUsers.Where(u => u.Role == Role.RegularUser && u.Id != request.UserId && 
                  u.BranchesId == request.RequesterBranchId && u.IsDisabled == false).ToList();

            var viewModel = new RequestAndUserViewModel()
            {
                Users = userList,
                Request = request,
                FileVolumes = null
            };

            return PartialView("_DirectTransferUsers", viewModel);
        }

        [Authorize(Roles = Role.RegularUser)]
        [Route("Requests/TransferToUser/{userId}/{reqId}")]
        public JsonResult TransferToUser(int userId, int reqId)
        {
            var userInDb = _context.AdUsers.Single(u => u.Id == userId);
            var request = _context.Requests.Single(r => r.Id == reqId);//the acting request
            if (userInDb.BranchesId == request.RequesterBranchId)
            {
                //int bindVal = RetrieveBindValue();
                request.IsRequestActive = false;//temporarily inactivate
                //request.RequestBinder = bindVal;
                //invoke the binder function and add an increment so as to link the 2 records.

                //create a new request record
                var newReq = new Request()
                {
                    FileVolumesId = request.FileVolumesId,
                    UserId = userId,
                    RecipientBranchId = request.RecipientBranchId,
                    RequestStatusId = 2,
                    RequestDate = DateTime.Now,
                    //AcceptedDate = DateTime.Now,
                    IsConfirmed = false,
                    ReturnStateId = 1,
                    IsRequestActive = true,
                    RequestBinder = request.RequestBinder, 
                    RequestTypeId = RequestType.DirectTransfer,
                    AcceptedById = request.UserId,
                    UserRequestedFromId = request.UserId,
                    RequesterBranchId = request.RequesterBranchId
                };

                _context.Requests.Add(newReq);

                var volumeInDb = _context.FileVolumes.Single(v=>v.Id == request.FileVolumesId);
                volumeInDb.StatesId = 4;//indicates that the volumes is being transferred

                _context.SaveChanges();

                //creating notification for a direct transfer
                var notify = new NotificationsController();
                notify.CreateNotification(newReq,Message.DirectTransferReq);
                //update volume to transfer
                
                return Json(new { success = true, message = $"Transferring to {userInDb.Name}. He/She must further confirm to complete transfer." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Something went wrong. invalid User or Incorrect Request Id." }, JsonRequestBehavior.AllowGet);
            
        }

      //reject on  reject direcct transfer
        public JsonResult OnRejectDirectTransfer(int id)
        {
            var rejReq = _context.Requests.Single(r => r.Id == id);

            //reactivate previous record, since the transfer was rejected.
            //recall we are searching for a record that is not active
            var originalReq = FindInitialRequestAndActivate(rejReq.Id, rejReq.FileVolumesId);
            originalReq.IsRequestActive = true;

            _context.SaveChanges();

            var rejectRecOperation = new RejectedRequestController();
            rejectRecOperation.SaveToRejectedRequestTable(rejReq);

            _context.Requests.Remove(rejReq);
            _context.SaveChanges();

            CheckoutVolume(originalReq.FileVolumesId, originalReq.UserId);//since parent req was on transfer prior, we need to change back to checkout since it was never accepted

            return Json(new { success = true, message = $"File should be still be the responsibility of the holding user." +
                                                        $" {originalReq.User.Name}"}, JsonRequestBehavior.AllowGet);
        }

        public Request FindInitialRequestAndActivate(int exId, int volId)
        {
            var reqObj = _context.Requests.Include(u=>u.User).SingleOrDefault(r=>r.Id != exId && 
                         r.FileVolumesId == volId && r.RequestTypeId != RequestType.ExternalRequest && r.IsRequestActive == false);

            return reqObj;
        }

        public int RetrieveBindValue()
        {
            int value = 0;
            var extBinder = _context.ExternalRequestsBinder.Single(b => b.Id == 1);

            value = extBinder.CurrentNumberBinder;

            extBinder.CurrentNumberBinder++;
            _context.SaveChanges();

            return value;
        }

        public void DeleteBoundedRequest(int reqId,  int volId, int userId)
        {
            var oldRequest = _context.Requests.Single(r =>
                r.Id != reqId && r.FileVolumesId == volId && r.RequestTypeId != RequestType.ExternalRequest);

            oldRequest.ReturnStateId = 3;
            oldRequest.ReturnedDate = DateTime.Now;
            oldRequest.ReturnAcceptById = userId;
            _context.SaveChanges();

            var completedReqOperation = new CompletedRequestController();
            completedReqOperation.SaveToCompletedRequestTable(oldRequest);

            _context.Requests.Remove(oldRequest);
            _context.SaveChanges();
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult BranchRequestActivity()
        {
            return View();
        }

        //retrieves active request for users based on branch
        [Authorize(Roles = Role.Registry)]
        public JsonResult GetActiveRequestsForBranch()
        {
            var userInSession = new AdUser(User.Identity.Name);

            try
            {

                var userInDb = _context.AdUsers.Single(u => u.Username == userInSession.Username);

                if (!userInDb.IsDisabled)
                {
                    var requests = _context.Requests.Include(u=>u.User).Include(r=>r.FileVolumes).Include(f=>f.FileVolumes.States).
                        Include(r=>r.AcceptedBy).Include(r=>r.UserRequestedFrom).
                        Where(r => r.RecipientBranchId == userInDb.BranchesId && r.RequestStatusId != 1).ToList();
                    
                    return Json(new { data = requests.OrderByDescending(r=>r.RequestDate), success = true ,message = "Requests for this branch retrieved." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "This account cannot perform this action. Kindly logout." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)//DbEntityValidation e
            {
                return Json(new { success = false, message = $"Something occured with the database. {e.Message}" }, JsonRequestBehavior.AllowGet);
            }

        }

        //initiating request for a registry direcct transfer
        [Authorize(Roles = Role.Registry)]
        [Route("Requests/CreateRegistryTransferRequest/{userId}/{volId}")]
        public JsonResult CreateRegistryTransferRequest(int userId, int volId)
        {
            var adUser = new AdUser(User.Identity.Name);

            try
            {
                var userInSession = _context.AdUsers.Single(u => u.Username == adUser.Username);//registry user
                var regularUser = _context.AdUsers.Single(u => u.Id == userId);
                var volume = _context.FileVolumes.Single(v => v.Id == volId);

                //Ensuring validity
                if (userInSession.BranchesId == regularUser.BranchesId &&
                    userInSession.BranchesId == volume.BranchesId && volume.StatesId == 1)
                {
                    var requestTransfer = new Request()
                    {
                        UserId = userId,
                        FileVolumesId = volId,
                        RecipientBranchId = userInSession.BranchesId,
                        RequestStatusId = 2,
                        RequestDate = DateTime.Now,
                        AcceptedDate = DateTime.Now,
                        IsConfirmed = false,
                        ReturnStateId = 1,
                        IsRequestActive = true,
                        ReturnedDate = null,
                        RequestBinder = 0,
                        RequestTypeId = RequestType.InternalRequest,
                        UserRequestedFromId = null,
                        RequesterBranchId = userInSession.BranchesId,
                        AcceptedById = userInSession.Id,
                        ReturnAcceptById = null

                    };
                    _context.Requests.Add(requestTransfer);
                    _context.SaveChanges();

                    volume.StatesId = 4;//set to transfer

                    _context.SaveChanges();

                    //create notification function here ...
                    var notify = new NotificationsController();
                    notify.NotifyOfRegistryTransfer(userId, userInSession.Id, userInSession.BranchesId,volId, Message.RegistryTransfer);

                    return Json(new { success = true, message = $"Transfer was successfully made. Await confirmation from {regularUser.Name}." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Cannot perform action. It seems there might be conflict in branches. Or File is in not in a valid state." }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception exception)
            {
                return Json(new { success = false, message = $"Exception Occured: {exception.Message}" }, JsonRequestBehavior.AllowGet);

            }

        }
    }

}