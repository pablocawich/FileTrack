using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class CompletedRequest
    {
        //model of an almost duplicate version to the request table except that it instead acts as a permanent and more substantial role for
        //recording checks-ins and check-outs process and lifetime.

        //Not yet implemented to DB, controllers, or anywhere besides it's declaration
        public int Id { get; set; }

        public FileVolumes Volume { get; set; }
        public int VolumeId { get; set; }

        public AdUser RequesterUser { get; set; }
        public int RequesterUserId { get; set; }

        public AdUser RegistryUser { get; set; }
        public int RegistryUserId { get; set; }

        public Branches RequesterBranch { get; set; }
        public byte RequesterBranchId { get; set; }

        public Branches FileBranch { get; set; }
        public byte FileBranchId { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime RegAcceptedDate { get; set; }

        public DateTime ReturnDate { get; set; }

        //ATM to identify the registry user doing the accept we only track the username which is not a FK, so we don't get association
        //conaider fixing in the request TABLE (Tedious asf) or not at all
        public string RegistryUserAccept { get; set; }

        public string RegistryUserReturn { get; set; }

        public AdUser UserTransferFrom { get; set; }
        public int? UserTransferFromId { get; set; }

        public string TransferType { get; set; }

    }
}