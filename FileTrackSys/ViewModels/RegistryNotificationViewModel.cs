using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileTracking.Models;

namespace FileTracking.ViewModels
{
    public class RegistryNotificationViewModel
    {
        public IEnumerable<Notification> RegistryInReturns { get; set; }

        public IEnumerable<Notification> RegistryExReturns { get; set; }

        public IEnumerable<Notification> RegistryExRetApprove { get; set; }

        public string ExceptionMessage { get; set; }
    }
}