using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileTracking.Models;

namespace FileTracking.ViewModels
{
    public class RequestAndUserViewModel
    {
        public IEnumerable<AdUser> Users { get; set; }

        public Request Request { get; set; }

    }
}