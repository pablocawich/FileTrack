using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class FileVolumes
    {
        public int Id { get; set; }

        public File File { get; set; }
        public int FileId { get; set; }

        public int FileNumber { get; set; }//just to add a secondary identification to this table. Not necessary

        public byte Volume { get; set; }

        [StringLength(255)]
        public string Comment { get; set; }

        public States States { get; set; }

        [Required]
        public byte StatesId { get; set; }
    }
}