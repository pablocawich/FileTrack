using System;
using System.Collections.Generic;
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
        
    }
}