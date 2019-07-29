using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileTracking.Models;

namespace FileTracking.ViewModels
{
    public class VolumeBranchesViewModel
    {
        public FileVolumes Volume{ get; set; }

        public IEnumerable<Branches> Branches { get; set; }
    }
}