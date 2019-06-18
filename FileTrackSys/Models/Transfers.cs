using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class Transfers
    {
        //
        public int Id { get; set; }

        public FileVolumes Volume { get; set; }
        public int VolumeId { get; set; }

        public AdUser RequesterUser { get; set; }
        [ForeignKey("RequesterUser")]
        public int AdUserId { get; set; }

        public AdUser RegistryUser { get; set; }
    }
}