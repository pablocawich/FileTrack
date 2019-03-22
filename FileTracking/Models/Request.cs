using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class Request
    {
        public int Id { get; set; }

        public FileVolumes FileVolumes { get; set; }
        [Required]
        public int FileVolumesId { get; set; }

        public AdUser User { get; set; }
        [Required]
        public int UserId { get; set; }

        /*public File File { get; set; }
        [Required]
        public int FileId { get; set; }*/

        public Branches Branches { get; set; }
        [Required]
        public byte BranchesId { get; set; }

        public RequestStatus RequestStatus { get; set; }
        [Required]
        public byte RequestStatusId { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime? AcceptedDate { get; set; }

        public string AcceptedBy { get; set; }
    }
}