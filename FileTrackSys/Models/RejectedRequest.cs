using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class RejectedRequest
    {
        public int Id { get; set; }

        public FileVolumes FileVolume { get; set; }
        public int FileVolumeId { get; set; }

        public AdUser RequesterUser { get; set; }
        public int RequesterUserId { get; set; }

        public Branches RequesterBranch { get; set; }
        public byte RequesterBranchId { get; set; }

        public Branches FileBranch { get; set; }
        public byte FileBranchId { get; set; }

        public DateTime RequestDate { get; set; }

        public AdUser RegistryUserReject { get; set; }
        public int? RegistryUserRejectId { get; set; }

        public DateTime? RegRejectedDate { get; set; }

        public AdUser UserTransferFrom { get; set; }
        public int? UserTransferFromId { get; set; }

        public string TransferType { get; set; }
    }
}