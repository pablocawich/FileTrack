using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class RequestType
    {
        [Key]
        public string RequestTypeId { get; set; }
        [Required]
        [StringLength(32)]
        public string Type { get; set; }

        [NotMapped]
        public const string InternalRequest = "INREQ";
        [NotMapped]
        public const string ExternalRequest = "EXREQ";
    }
}