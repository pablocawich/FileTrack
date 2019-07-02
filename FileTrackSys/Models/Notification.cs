using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public AdUser SenderUser { get; set; }
        public int? SenderUserId { get; set; }

        public AdUser RecipientUser { get; set; }
        public int? RecipientUserId { get; set; }

        public Message Message { get; set; }
        public string MessageId { get; set; }

        public Request Request { get; set; }
        public int? RequestId { get; set; }

        public DateTime? DateTriggered { get; set; }

        public bool Read { get; set; }

    }
}