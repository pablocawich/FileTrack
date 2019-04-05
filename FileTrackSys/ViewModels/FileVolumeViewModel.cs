using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using FileTracking.Models;

namespace FileTracking.ViewModels
{
    public class FileVolumeViewModel
    {
        public File File { get; set; }
        public FileVolumes FileVolumes { get; set; }
    }
}