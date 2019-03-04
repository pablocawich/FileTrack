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

        public string Street { get; set; }
        public string CityOrTown { get; set; }

        public Districts Districts { get; set; }
        public byte DistrictsId { get; set; }

        public string Comments { get; set; }

        //public FileType FileType { get; set; }
        //public int FileTypeId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}