using System;
using System.Collections.Generic;
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
        // GET: AdUsers
        public ActionResult TestUsers()
        {
            var connection = InitializeAdConnection();
            var userList = AdUsers(Role.Registry, connection);

            var viewModel = new UserAdModel
            {
                UsersInRole = userList
            };
            return View("TestUsers",viewModel);
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

                        //var branch = DetermineBranch(user);

                        var AdUserObj = new AdUser()
                        {
                            Name = user.Name,
                            Email = user.EmailAddress,
                            Username = user.SamAccountName,
                            Role = groupName,
                            BranchId = 1

                        };
                        //user variable has the details about the user 

                        userList.Add(AdUserObj);
                    }
                }
            }
            
            return userList;
        }

        //returns user branch based on a string search
        public string DetermineBranch(UserPrincipal u)
        {
            if (u.DistinguishedName.Contains("Belmopan"))
                return "Belmopan";
            if (u.DistinguishedName.Contains("Dangriga"))
                return "Dangriga";
            if (u.DistinguishedName.Contains("Corozal"))
                return "Corozal";
            if (u.DistinguishedName.Contains("San Pedro"))
                return "San Pedro";
            if (u.DistinguishedName.Contains("Orange Walk"))
                return "Orange Walk";
            if (u.DistinguishedName.Contains("Belize City"))
                return "Belize City";

            return "Branch not declared";

        }

        //stores users objects into our db
        public void SaveUsersToList()
        {
            //write code that grabs the list of user objects and stores them in our db
        }
    }
}
