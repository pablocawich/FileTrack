using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public RequestType RequestType { get; set; }
        public string RequestTypeId { get; set; }

        public Branches Branches { get; set; }
        [Required]
        public byte BranchesId { get; set; }

        public byte RequesteeBranch { get; set; }

        public RequestStatus RequestStatus { get; set; }
        [Required]
        public byte RequestStatusId { get; set; } //pending, accepted, rejected

        public DateTime RequestDate { get; set; }//date request was made by the user

        public DateTime? AcceptedDate { get; set; }//date accepted by registry

        public string AcceptedBy { get; set; }//specific registry member

        public DateTime? ReturnedDate { get; set; } //date file was returned to registry, triggered when registry accepts return

        public string ReturnAcceptBy { get; set; }//Specific reg member who did the accept

        [DefaultValue(false)]
        public bool IsConfirmed { get; set; }

        public ReturnState ReturnState { get; set; }
        [Required]
        [DefaultValue(1)]
        public byte ReturnStateId { get; set; }

        [DefaultValue(true)]
        public bool IsRequestActive { get; set; }

        public int RequestBinder { get; set; }

        //This will be used to track the using the file is being requested from
        public AdUser UserRequestedFrom{ get; set; }
        public int? UserRequestedFromId { get; set; }

    }
}