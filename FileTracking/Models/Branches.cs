using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class Branches
    {
        public byte Id { get; set; }
        public string Branch { get; set; }

        public Districts Districts { get; set; }
        public byte DistrictsId { get; set; }
    }
}