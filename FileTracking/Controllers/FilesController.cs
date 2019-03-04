using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileTracking.Models;

namespace FileTracking.Controllers
{
    public class FilesController : Controller
    {
        // GET: Files
        public ActionResult Index()
        {
           /* var fileView = new File
            {
                FirstName = "John's file",
                FileType = new FileType
                {
                    Id = 1,
                    Type = "Client"
                }
            };*/
            return View();
        }

        public ActionResult CreateFile()
        {
            return View("FileForm");
        }
        //below function should accepts a file objects with its binded values from a form as its parameter
        //and ultimately save that value unto the database.
        [HttpPost]
        public ActionResult NewFile(File file)
        {
            return View();
        }
    }
}