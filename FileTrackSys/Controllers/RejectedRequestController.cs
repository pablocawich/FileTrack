using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileTracking.Models;

namespace FileTracking.Controllers
{
    public class RejectedRequestController : Controller
    {
        private ApplicationDbContext _context;


        public RejectedRequestController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }
        // GET: RejectedRequest
        public ActionResult Index()
        {
            return Content("Something here");
        }

        public void SaveToRejectedRequestTable(Request request)
        {

            var rejectedRequest = new RejectedRequest()
            {
                FileVolumeId = request.FileVolumesId,
                RequesterUserId = request.UserId,
                RequesterBranchId = request.RequesterBranchId,
                FileBranchId = request.RecipientBranchId,
                RequestDate = request.RequestDate,
                RegistryUserRejectId = request.AcceptedById,  
                RegRejectedDate = request.AcceptedDate,
                UserTransferFromId = request.UserRequestedFromId,
                TransferType = "Local Reject"

            };

            _context.RejectedRequests.Add(rejectedRequest);
            _context.SaveChanges();
        }
    }
}