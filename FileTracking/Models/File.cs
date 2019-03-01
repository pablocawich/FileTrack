using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class File
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public string Address { get; set; }

        public DateTime DateCreated { get; set; }
    }
}