using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileTracking.Models;

namespace FileTracking.ViewModels
{
    public class UserBranchViewModel
    {
        public AdUser User { get; set; }

        public IEnumerable<Branches> Branches { get; set; }
    }
}