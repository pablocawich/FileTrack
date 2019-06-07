using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;


namespace FileTracking.Models
{
    public class AdUser
    {

        public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        public Branches Branches { get; set; }
        public byte BranchesId { get; set; }

        [DefaultValue(false)]
        public bool IsDisabled { get; set; }//will store whether a user is enabled or disabled.

        public AdUser()
        {
            
        }

        //since asp identity provides a library which gives us the name of the active directory user currently logged on
        //we use that to identify the user in the database but before we do that we must parse the given username to that
        //which matches the string in the database field
        //the constructor initializes and ensures we work with the relevant username to the database
        public AdUser(string uname)
        {
            this.Username = ParseUsername(uname);
        }
        //we remove the devfinco from the string
        public string ParseUsername(string adName)
        {
            string newName = "";
            if (adName.Contains("DEVFINCO"))
                newName = adName.Remove(0, 9);
            return newName;
        }
    }
}