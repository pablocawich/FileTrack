using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileTracking.Models;

namespace FileTracking.ViewModels
{
    public class ConfirmRequestViewModel
    {
        public ICollection<Request> Requests { get; set; }
    }
}