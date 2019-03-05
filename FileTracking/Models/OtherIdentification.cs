using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class OtherIdentification
    {
        public int Id { get; set; }

        public IdentificationOption IdentificationOption { get; set; }
        public byte IdentificationOptionId { get; set; }

        public string IdNumber { get; set; }

    }
}