using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class Transfers
    {
        //model of an almost duplicate version to the request table except that it instead acts as a permanent and more substantial role for
        //recording checks-ins and check-outs process and lifetime.

        //Not yet implemented to DB, controllers, or anywhere besides it's definition
        public int Id { get; set; }

        public FileVolumes Volume { get; set; }
        public int VolumeId { get; set; }

        public AdUser RequesterUser { get; set; }
        [ForeignKey("RequesterUser")]
        public int AdUserId { get; set; }

        public AdUser RegistryUser { get; set; }
    }
}