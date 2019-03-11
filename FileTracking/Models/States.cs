using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class States
    {
        public byte Id { get; set; }

        [StringLength(32)]
        public string State { get; set; }
    }
}