using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
       

        [Route("Requests/Index/{volId}/{fileId}")]
        public ActionResult Index(int volId,int fileId)
        {
            //pulls records associated with a request such as file, volume and user
            var file = _context.Files.Single(f => f.Id == fileId);
            var volume = _context.FileVolumes.Single(v => v.Id == volId);
            string username = ParseUsername(User.Identity.Name);
            var user = _context.AdUsers.Single(u => u.Username == username);

            //code that populates requests table
            PopulateRequest(file, volume, user);

            //if populate request was suucessful we should change volume state to requested, if it has not already been in that state
            if (volId != 0)
                return View("Index");            

            return HttpNotFound("Invalid use of page, request parameters not provided or does not correspond to a file in db");
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
        public bool PopulateRequest(File f,FileVolumes v, AdUser u) {
            var requestRecord = new Request()
            {
                FileId = f.Id,
                UserId = u.Id,
                FileVolumesId = v.Id,
                BranchesId = u.BranchesId,
                RequestStatusId = 1,
                RequestDate = DateTime.Now,
            };
            _context.Requests.Add(requestRecord);

            if (_context.SaveChanges() > 0) {
                return true;
            }
            return false;
        }

        //updates the state of the volume being requested
        public void UpdateVolumeState(FileVolumes v) {

        }
    }
}