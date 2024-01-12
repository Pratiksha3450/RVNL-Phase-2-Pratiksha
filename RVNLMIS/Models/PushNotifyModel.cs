using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PushNotifyModel
    {
        public int NotificationId{ get; set; }

        public int SenderId { get; set; }

        public int RecieverId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? ReadOn { get; set; }

        public DateTime? SentOn { get; set; }
    }
}