using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class FileStatus
    {
        public byte Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Status { get; set; }
    }
}