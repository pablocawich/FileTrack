using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileTracking.Models;

namespace FileTracking.ViewModels
{
    public class VolumesViewModel
    {
        public File File { get; set; }
        public ICollection<FileVolumes> FileVolumes { get; set; }
    }
}