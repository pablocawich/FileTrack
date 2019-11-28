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
    public class NotificationsController : Controller
    {
        private ApplicationDbContext _context;

        public NotificationsController()
        {
            _context = new ApplicationDbContext();

        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }
        // GET: Notifications
        [Authorize(Roles = Role.RegularUser)]
        public ActionResult Index()
        {
            //for regular user role
            var AdUsername = new AdUser(User.Identity.Name);
            
            try
            {
                var userInDb = _context.AdUsers.Single(u => u.Username == AdUsername.Username);

                var notifications = _context.Notifications.Include(n => n.RecipientUser).Include(n => n.Message).Include(n=>n.FileVolume).
                    Where(n => n.RecipientUserId == userInDb.Id).Where(n => n.Read == false).
                    Where(n => n.MessageId == Message.InAccept || n.MessageId == Message.InReject || n.MessageId == Message.TransferRequest
                    ||n.MessageId == Message.TransferAccept || n.MessageId == Message.TransferDenied || n.MessageId == Message.DirectTransferReq
                    || n.MessageId == Message.ExAccept || n.MessageId == Message.ExReject).ToList();

                return PartialView("Notifications", notifications);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               //throw;
            }

            return HttpNotFound("User not yet registered to database");

        }

        //registry purposes
        [Authorize(Roles = Role.Registry)]
        public ActionResult NotificationRegistry()
        {
            var AdUsername = new AdUser(User.Identity.Name);

            var userInDb = _context.AdUsers.Single(u => u.Username == AdUsername.Username);

            var notifications = _context.Notifications.Include(n => n.RecipientUser).Include(n => n.Message)
                .Where(n=>n.RecipientBranchId == userInDb.BranchesId).
                Where(n=>n.MessageId == Message.Return || n.MessageId == Message.ExReturn || n.MessageId == Message.PendingFile)
                .Where(n => n.Read == false).ToList();
            //for internal returns 

            var viewModel = new RegistryNotificationViewModel()
            {
                RegistryInReturns = notifications,
                RegistryExReturns = _context.Notifications.Include(n => n.RecipientUser).Include(n => n.Message)
                .Where(n => n.SenderBranchId == userInDb.BranchesId || n.RecipientBranchId == userInDb.BranchesId).
                Where(n => n.MessageId == Message.ExReturnApproval || n.MessageId == Message.ExternalPending || n.MessageId == Message.ExternalRoute)
                .Where(n => n.Read == false).ToList(),//for external returns
            };
            return PartialView("RegistryNotification",viewModel);

        }

        public void ChangeToRead(int id)
        {
            
            var notifInDb = _context.Notifications.Single(n => n.Id == id);

           //notifInDb.Read = true;

            _context.Notifications.Remove(notifInDb);
            _context.SaveChanges();
        }
        //------------------------------------------NOTIFICATIONS------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------
        //for regular request notification
        public void CreateNotification(Request req, string messageId)
        {
            var notif = new Notification()
            {
                RecipientUserId = req.UserId,//user 
                MessageId = messageId,
                Read = false,
                //RequestId = req.Id,
                FileVolumeId = req.FileVolumesId,
                DateTriggered = DateTime.Now,
                SenderUserId = req.UserRequestedFromId
            };

            _context.Notifications.Add(notif);
            _context.SaveChanges();
        }
        //the notification that will be sent to local registry for external request
        public void CreateInitialExternalRequest(Request req, string messageId)
        {
            var notif = new Notification()
            {
                RecipientUserId = req.UserId,//user 
                MessageId = messageId,
                Read = false,
                RecipientBranchId = req.RequesterBranchId,
                //SenderBranchId = req.RecipientBranchId,
                //RequestId = req.Id,
                FileVolumeId = req.FileVolumesId,
                DateTriggered = DateTime.Now,
                SenderUserId = req.UserRequestedFromId
            };

            _context.Notifications.Add(notif);
            _context.SaveChanges();
        }

        //for initial transfer request.
        public void NotificationForInitialTransfer(Request req, string messageId)
        {

            var notif = new Notification()
            {
                RecipientUserId = req.UserRequestedFromId,//user 
                MessageId = messageId,
                Read = false,
                //RequestId = req.Id,
                FileVolumeId = req.FileVolumesId,
                DateTriggered = DateTime.Now,
                SenderUserId = req.UserId
            };
            _context.Notifications.Add(notif);
            _context.SaveChanges();
        }

        //creates and sends a request to registry
        public void NotificationForInitialRequest(Request req, string messageId)
        {

            var notif = new Notification()
            {
                RecipientUserId = null,//user 
                MessageId = messageId,
                Read = false,
                //RequestId = req.Id,
                RecipientBranchId = req.RecipientBranchId, //imperative, since all branch registry should receive the notif
                FileVolumeId = req.FileVolumesId,
                DateTriggered = DateTime.Now,
                SenderUserId = req.UserId
            };
            _context.Notifications.Add(notif);
            _context.SaveChanges();
        }
    }
}