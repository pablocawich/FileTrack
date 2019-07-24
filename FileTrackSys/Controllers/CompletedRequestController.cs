using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileTracking.Models;

namespace FileTracking.Controllers
{
    public class CompletedRequestController : Controller
    {
        private ApplicationDbContext _context;


        public CompletedRequestController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }
        // GET: CompletedRequest
        public ActionResult Index()
        {
            return Content("something");
        }      

        public void SaveToCompletedRequestTable(Request request)
        {

            var completedRequest = new CompletedRequest()
            {
                FileVolumeId = request.FileVolumesId,
                RequesterUserId = request.UserId,
                RequesterBranchId = request.RequesterBranchId,
                FileBranchId = request.RecipientBranchId,
                RequestDate = request.RequestDate,
                RegistryUserAcceptId = request.AcceptedById,
                RegAcceptedDate = request.AcceptedDate,
                ReturnAcceptById = request.ReturnAcceptById,
                ReturnDate = request.ReturnedDate,
                UserTransferFromId = request.UserRequestedFromId,
                TransferType = "Local Transfer"

            };

            _context.CompletedRequests.Add(completedRequest);
            _context.SaveChanges();
        }

        //will delete the request record after relevant information has been retrieved and stored to Completed Requests table
        //done in respective controller
        //not here because it throws an unhandled exception
    }
}