using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileTracking.Models;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.CompilerServices;
using System.Web.Security;
using FileTracking.ViewModels;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace FileTracking.Controllers
{
    public class AdUsersController : Controller
    {
        private ApplicationDbContext _context;

        public AdUsersController()
        {
            _context = new ApplicationDbContext();
        }
        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        [Authorize(Roles = Role.Registry)]
        public ActionResult SignInPrompt()
        {
            return View();
        }

        // GET: AdUsers
        [Authorize(Roles = "WEB_IT")]
        [Route("adUsers/TestUsers/{role}")]
        public ActionResult TestUsers(string role)
        {
            
            var connection = InitializeAdConnection();
            var userList = new List<AdUser>();

            if (role == "registry")
            {
                userList = AdUsers(Role.Registry, connection);
                ViewBag.Message = "Registry Users";
            }else if (role == "web")
            {
                userList = AdUsers("WEB_IT", connection);
                ViewBag.Message = "Regular Users";
            }
            else if (role == "admin")
            {
                userList = AdUsers("FMS_Admin", connection);
                ViewBag.Message = "Admin USers";
            }

            var viewModel = new UserAdModel
            {
                UsersInRole = userList
            };

            return View("TestUsers",viewModel);
        }

        [Authorize(Roles = Role.AdminUser)]
        public ActionResult GetAdUsers()
        {
            var connection = InitializeAdConnection();
            var userList = new List<AdUser>();

            //retrieving users from AD, those not yet listed in the DB
            userList = AdUsers(Role.Registry, connection);

            //now adding regular users found in the active directory
            var regularList = AdUsers(Role.RegularUser, connection);
            //must be done in a loops as i cannot directly add a list of objects.
            foreach (var rec in regularList)
            {
                userList.Add(rec);
            }           
            
            /*else if (role == null || role != "registry" || role != "regular")
            {
                return HttpNotFound("Role cannot be anything other than regular or registry. Please provide valid parameters");
            }*/

            return Json(new { data = userList }, JsonRequestBehavior.AllowGet);

        }

        [Authorize(Roles = Role.AdminUser)]
        public ActionResult GetDbUsers()
        {
            var usersInDb = _context.AdUsers.Include(u=>u.Branches).ToList();
            return Json(new { data = usersInDb }, JsonRequestBehavior.AllowGet);

        }
        //Establishes connection with active directory domain
        public PrincipalContext InitializeAdConnection()
        {
            return new PrincipalContext(ContextType.Domain, "DEVFINCO", "DC=DEVFINCO,DC=net");
        }

        //retrieves users and their appropriate information based on the role specified
        [Authorize(Roles = Role.AdminUser)]
        public List<AdUser> AdUsers(string groupName, PrincipalContext context)
        {
            var userList = new List<AdUser>();
            var adUsersinDb = _context.AdUsers.ToList();

            using (var group = GroupPrincipal.FindByIdentity(context, groupName))
            {
                if (group == null)
                {
                    //Group does not exist. returning an empty list
                    return userList;
                }
                else
                {
                    var users = group.GetMembers(true);

                    foreach (UserPrincipal user in users)
                    {                       
                        //this if block ensures a check is done to verify that our db records are not duplicated by the instances in the active directory
                        if (!(adUsersinDb.Exists(adUser => adUser.Username == user.SamAccountName)))
                        {
                            var branchId = DetermineBranch(user);
                            var AdUserObj = new AdUser()
                            {
                                Name = user.Name,
                                Email = user.EmailAddress,
                                Username = user.SamAccountName,
                                Role = groupName,
                                BranchesId = branchId

                            };
                            //user variable has the details about the user 

                            userList.Add(AdUserObj);
                        }
                            
                    }
                }
            }
            
            return userList;
        }

        //returns user branch based on a string search
        public byte DetermineBranch(UserPrincipal u)
        {
            if (u.DistinguishedName.Contains("Belmopan"))
                return 5;
            if (u.DistinguishedName.Contains("Dangriga"))
                return 6;
            if (u.DistinguishedName.Contains("Corozal"))
                return 1;
            if (u.DistinguishedName.Contains("San Pedro"))
                return 4;
            if (u.DistinguishedName.Contains("Orange Walk"))
                return 2;
            if (u.DistinguishedName.Contains("Belize City"))
                return 3;

            return 0;

        }

        //stores user objects into db
        [Authorize(Roles = Role.AdminUser)]
        public JsonResult SaveAdUsers()
        {
            var connection = InitializeAdConnection();
            var userListInRegistry = AdUsers(Role.Registry, connection);
            var userListInRegular = AdUsers(Role.RegularUser, connection);
   
            //Prepares insert queries for registry users
            foreach (var u in userListInRegistry)
            {
                     _context.AdUsers.Add(u);
            }

            //Prepares insert queries for regular users
            foreach (var ru in userListInRegular)
            {
                _context.AdUsers.Add(ru);
            }

            //save all insert queries generated by both loops and ultimately stores information
            _context.SaveChanges();

            if (_context.SaveChanges() > 0)
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);

        }

        //directs user to admin page
        [Authorize(Roles = Role.AdminUser)]
        public ActionResult AdminManagement()
        {
            var currentUser = new AdUser(User.Identity.Name);

            var userInDb = _context.AdUsers.Single(u => u.Username == currentUser.Username);

            if (userInDb.IsDisabled == false)
            {
                return View();
            }

            return View("Locked");
        }

        //directs to the change branch modal
        [Authorize(Roles = Role.AdminUser)]
        public ActionResult ChangeBranch(int id)
        {
            var userInDb = _context.AdUsers.Include(u=>u.Branches).Single(u => u.Id == id);
            var branchesInDb = _context.Branches.ToList();

            var viewModel = new UserBranchViewModel()
            {
                User = userInDb,
                Branches = branchesInDb
            };

            return PartialView(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = Role.AdminUser)]
        [ValidateAntiForgeryToken]
        public ActionResult SaveNewBranch(AdUser user)
        {
            var userParsed = new AdUser(User.Identity.Name);
            
            if (!ModelState.IsValid)
            {
                return Content("Option selected not valid. Please return and try again.");
            }
            

            if (user.Id != 0)
            {
                var userInDb = _context.AdUsers.Single(u => u.Id == user.Id);
                //checks that user is not changing his/her own branch
                if (userInDb.Username != userParsed.Username)
                {
                    //we only change the user's branch
                    userInDb.BranchesId = user.BranchesId;
                    _context.SaveChanges();
                }
                else
                {
                    return HttpNotFound("Unfortunately cannot change your own district. Seek another admin to carry this out.");
                }
                
            }

            return RedirectToAction("AdminManagement", "AdUsers");
        }

        public JsonResult CheckRequestActivity(int id)
        {
            var requestsInDb = _context.Requests.Where(r => r.UserId == id).ToList();

            if (requestsInDb.Any())
            {
                return this.Json(new { success = false, message = "User seems to have active file request/transfer activities. User needs clear all activities before a change can be made." }, JsonRequestBehavior.AllowGet);
            }

            return this.Json(new { success = true}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = Role.AdminUser)]
        public JsonResult DeleteUser(string function_param)
        {
            var adUser = new AdUser(User.Identity.Name);
            
            dynamic func_param = JsonConvert.DeserializeObject(function_param);

            foreach (var user in func_param)
            {
                int result = Int32.Parse(user.ToString());
                var userinDb = _context.AdUsers.Single(u => u.Id == result);
                if(userinDb.Username != adUser.Username)
                    userinDb.IsDisabled = true;

            }
            if (_context.SaveChanges() > 0)
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = Role.AdminUser)]
        public JsonResult ReEnableUser(string function_param)
        {
            dynamic func_param = JsonConvert.DeserializeObject(function_param);

            foreach (var user in func_param)
            {
                int result = Int32.Parse(user.ToString());
                var userinDb = _context.AdUsers.Single(u => u.Id == result);
                userinDb.IsDisabled = false;

            }

            if (_context.SaveChanges() > 0)
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

    }
}
