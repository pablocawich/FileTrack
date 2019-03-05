using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileTracking.Models;

namespace FileTracking.ViewModels
{
    public class FileViewModel
    {
        public File File { get; set; }
        public IEnumerable<Districts> Districts { get; set; }
        public IEnumerable<FileType> FileTypes { get; set; }
        public IEnumerable<FileStatus> FileStatuses { get; set; }
        public IEnumerable<IdentificationOption> IdentificationOptions { get; set; }
       
    }
}