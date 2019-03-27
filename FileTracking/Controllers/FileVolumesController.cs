﻿using System;
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
                Include(fv=>fv.Branches).Where(fv => fv.FileId == id).ToList();

            var volumes = _context.Files.Include(f => f.FileVolumes).SingleOrDefault(f => f.Id == id );

            var user = _context.AdUsers.Single(u=>u.Username == uName);

            var viewModel = new VolumesViewModel()
            {
                File = volumes,
                FileVolumes = volFileId,
                AdUser = user
            };

            return View("FileVolume",viewModel);
        }

        [Authorize(Roles = Role.RegularUser)]
        public ActionResult UserVolumes()
        {
            string username = ParseUsername(User.Identity.Name);
            var user = _context.AdUsers.Single(u => u.Username == username);

            var request = _context.Requests.Include(r=>r.FileVolumes).Where(r => r.UserId == user.Id).Where(r => r.IsConfirmed == true).Where(r=>r.ReturnStateId == 1).ToList();

           // var reqList = new List<Request>();

            //reqList = request;
            //run query that shows volume currently assigned to the signed in user
                        //get those from requests with user id,
            return View("UserVolumes", request);
        }

        public void ReturnVolume(int id)
        {
            const byte isReturning = 2;
            var request = _context.Requests.Single(r => r.Id == id);
            request.ReturnStateId = isReturning;

            _context.SaveChanges();

        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult ReturnApproval()
        {
            return View("ReturnApproval");
        }
        [Authorize(Roles = Role.Registry)]
        public ActionResult GetReturnedRequests()
        {
            var returnedReq = _context.Requests.Include(r => r.FileVolumes).
                Include(r => r.User.Branches).Where(r=>r.ReturnStateId== 2).ToList();


            return Json(new { data = returnedReq }, JsonRequestBehavior.AllowGet);
        }

        public void AcceptReturn(int id)
        {
            var req = _context.Requests.Single(r => r.Id == id);
            req.ReturnStateId = 3;

            _context.SaveChanges();
            ChangeStateToStored(req.FileVolumesId);
        }

        public void ChangeStateToStored(int id)
        {
            const byte storedState = 1;
            var vol = _context.FileVolumes.Single(v => v.Id == id);

            vol.StatesId = storedState;

            _context.SaveChanges();
        }
    }
}