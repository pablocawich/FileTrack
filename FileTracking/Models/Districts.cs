using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class Districts
    {
        public byte Id { get; set; }
        [Required]
        public string District { get; set; }
    }
}