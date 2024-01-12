using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class UserSettingModel
    {
        public UserModel objUser { get; set; }

        public string UserImagePath { get; set; }

        public NotifyModel objNotify { get; set; }
    }

    public class NotifyModel
    {
        public bool Email { get; set; }

        public bool SMS { get; set; }

        public bool Whatsapp { get; set; }

        public bool AppPushNotification { get; set; }

        public bool DesktopPushNotification { get; set; }

        public bool PopupNotification { get; set; }
    }
}