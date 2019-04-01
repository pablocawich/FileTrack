using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class Location
    {
        [Key]
        public string LocationId { get; set; }
        [Required]
        public Districts Districts { get; set; }

        public byte DistrictsId { get; set; }

        [StringLength(64)]
        public string Name { get; set; }
    }
}