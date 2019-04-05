using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class FileType
    {
        public byte Id { get; set; }

        [Required]
        [StringLength(24)]
        public string Type { get; set; }

    }
}