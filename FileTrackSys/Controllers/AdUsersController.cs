using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileTracking.Models;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using FileTracking.ViewModels;


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
            }else if (role == "regular")
            {
                userList = AdUsers(Role.RegularUser, connection);
                ViewBag.Message = "Regular Users";
            }
            else if (role == null || role != "registry"||role != "regular")
            {
                return HttpNotFound("Role cannot be anything other than regular or registry. Please provide valid parameters");
            }

            var viewModel = new UserAdModel
            {
                UsersInRole = userList
            };

            return View("TestUsers",viewModel);
        }

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

                        
                        //this if ensures a check is done to verify that our db records are not duplicated by the instances in the active directory
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

        public ActionResult AdminManagement()
        {
            return View();
        }

    }
}
