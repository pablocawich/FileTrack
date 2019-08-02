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

        public void CreateNotification(Request req, string messageId)
        {
            var notif = new Notification()
            {
                RecipientUserId = req.UserId,
                MessageId = messageId,
                Read = false,
                RequestId = req.Id,
                DateTriggered = DateTime.Now,
                SenderUserId = req.AcceptedById
            };

            _context.Notifications.Add(notif);
            _context.SaveChanges();
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
                return true;
            
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
        }

        // Direct to pendingFiles page (RegistryOnly)
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
            //for the most part, requesteeBranchId will always be the currentLocation Branch
            //we must ensure to take into account branches. registry is only to see request from user made within their respective branch
            var userObj = new AdUser(User.Identity.Name);
           
            var user = _context.AdUsers.Single(u => u.Username == userObj.Username);

            if (user.IsDisabled)
                return View("Locked");
            
            //PENDING = 1 (RequestStatusID)
            //where a User RequesteeFrom field is NULL, Registry users in general are represented
            var pendingRequests = _context.Requests.Include(r => r.FileVolumes).
                Include(r => r.User.Branches).Where(r=>r.RecipientBranchId == user.BranchesId).Where(r => r.RequestStatusId == 1).
                Where(r=>r.IsRequestActive == true).Where(r=>r.RequestTypeId == RequestType.InternalRequest)
                .Where(r => r.UserRequestedFromId == null).ToList();

            return Json(new { data = pendingRequests }, JsonRequestBehavior.AllowGet);
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
                    CreateNotification(req, Message.InReject);

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

                    CreateNotification(request, Message.InAccept);

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
                    //Since the 
                    CreateNotification(request, Message.ExAccept);

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
                if (request.RequestBinder == 0)
                    CreateNotification(request, Message.InReject);
                else if (request.RequestTypeId == RequestType.ExternalRequest)
                    CreateNotification(request, Message.ExReject);

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
        public void AcceptForeignRegistryTransfer(int id)
        {
            //request binded records that sees to registry to registry operations before initiating the local request it is bound by
            var requestInDb = _context.Requests.Single(r => r.Id == id);

            requestInDb.IsConfirmed = true;
            requestInDb.IsRequestActive = false;//mark record temporarily inactive
            _context.SaveChanges();

            //after the above is performed we let its binded request become active in order for the initiating user to go through the local request process
            //also be reminded to change to volume's current location, as being accepted now signifies the file is at a foreign branch       

           var volume = _context.FileVolumes.Single(v => v.Id == requestInDb.FileVolumesId);

           PopulateNewInternalRequest(requestInDb.UserId, requestInDb.RequesterBranchId, requestInDb.RequestDate,volume, requestInDb.RequestBinder);//both registry passed, now we create an internal request

           volume.CurrentLocationId = requestInDb.RequesterBranchId; //we must change the volume's current location to the current user's branch
           _context.SaveChanges();
           CreateNotification(requestInDb, Message.InAccept);//revise
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
            //if(volInDb.CurrentLocation = userrequestingtransfer.branchId){}
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
                CreateNotification(requestInDb,Message.InAccept);

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

            CreateNotification(requestInDb, Message.InReject);

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
                RequestId = req.Id,
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
                var requestInDb = _context.Requests.Single(r => r.Id == id);
                requestInDb.IsConfirmed = true;
                _context.SaveChanges();

                //volume record must be updated
                CheckoutVolume(requestInDb.FileVolumesId, userInSession.Id);

                //FROM obj in the return statement indicates from whom the confirmation is being made in response to.
                return Json(new { success = true, message = "Confirmation Successful", from = requestInDb.AcceptedBy}, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "This account is not active. Kindly exit" }, JsonRequestBehavior.AllowGet);
        }
    }

}