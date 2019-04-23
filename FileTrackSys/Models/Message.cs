using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class Message
    {
        [Key]
        [StringLength(8)]
        public string Id { get; set; }

        [StringLength(24)]
        public string MessageType { get; set; }

        [StringLength(255)]
        public string MessageText { get; set; }

        [NotMapped]
        public const string InAccept = "ACC";
        [NotMapped]
        public const string InReject = "REJ";
        [NotMapped]
        public const string Return = "RET";
        [NotMapped]
        public const string ReturnAccept = "RET_ACC";
        [NotMapped]
        public const string ExAccept = "EX_ACC";
        [NotMapped]
        public const string ExReject = "EX_REJ";
        [NotMapped]
        public const string ExReturn = "EX_RET";
        [NotMapped]
        public const string ExReturnApproval = "ExRetAcc";

    }
}