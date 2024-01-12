using Kendo.Mvc.UI;
using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.Models;
using Kendo.Mvc.Extensions;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.Areas.RFI.Models;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [AreaSessionExpire]
    [HandleError]
    public class RFINotificationLogController : Controller
    {
        // GET: Enclouser
        public ActionResult Index()
        {
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            List<PushNotifyModel> objNotify = Read_Notification(userId);
            return View(objNotify);
        }

        public List<PushNotifyModel> Read_Notification(int userId)
        {
            List<PushNotifyModel> notifyObj = new List<PushNotifyModel>();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                notifyObj = (from n in dbContext.tblNotificationMsgs
                             join r in dbContext.tblNotificationReadStatus
                             on n.NotificationId equals r.NotificationId
                             where n.ReceiverId == userId
                             select new { n, r }).AsEnumerable()
                             .Select(s => new PushNotifyModel
                             {
                                 NotificationId = s.n.NotificationId,
                                 Title = s.n.Title,
                                 Message = s.n.Message,
                                 IsRead = s.r.IsRead,
                                 ReadOn = s.r.ReadOn,
                                 SentOn = s.n.SentOn
                             }).ToList();
            }

            return notifyObj;
        }

        public ActionResult MarkAllRead()
        {
            try
            {
                int userId = ((UserModel)Session["RFIUserSession"]).UserId;
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var notObj = dbContext.tblNotificationReadStatus.Where(w => w.ReceiverId == userId).ToList();
                    foreach (var item in notObj)
                    {
                        item.IsRead = true;
                        item.ReadOn = DateTime.Now;
                    }
                    dbContext.SaveChanges();
                }
                return Json("1", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("0", JsonRequestBehavior.AllowGet);
            }
        }
    }
}