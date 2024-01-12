using RVNLMIS.DAC;
using RVNLMIS.Models;
using RVNLMIS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.Common.ActionFilters;

namespace RVNLMIS.Controllers
{
    [Authorize]
    public class PushNotificationController : Controller
    {
        // GET: PushNotification
        [PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SendNotification(PushNotifyModel objModel)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    tblNotification objAdd = new tblNotification()
                    {
                        Title = objModel.Title,
                        Message = objModel.Message,
                        Date = DateTime.Now,
                        SenderId = ((UserModel)Session["UserData"]).UserId,
                        ReceiverId = 0,
                        Payload = null,
                        Response = null
                    };

                    dbContext.tblNotifications.Add(objAdd);
                    dbContext.SaveChanges();

                    #region Send notification

                    //get users list with fcm token
                    var getUsers = dbContext.UserDetailsWithRoles.ToList();

                    for (int i = 0; i < getUsers.Count(); i++)
                    {
                        if (!string.IsNullOrEmpty(getUsers[i].FCMToken) && string.IsNullOrEmpty(getUsers[i].IosUserId))
                        {
                            PushNotification.PushNotify(objModel.Message, objModel.Title, getUsers[i].FCMToken, "FA");
                        }
                        if (!string.IsNullOrEmpty(getUsers[i].IosUserId) && string.IsNullOrEmpty(getUsers[i].FCMToken))
                        {
                            PushNotification.PushNotify(objModel.Message, objModel.Title, getUsers[i].IosUserId, "OA");
                        }
                        if (!string.IsNullOrEmpty(getUsers[i].FCMToken) && !string.IsNullOrEmpty(getUsers[i].IosUserId))
                        {
                            PushNotification.PushNotify(objModel.Message, objModel.Title, getUsers[i].FCMToken, "FA");
                            PushNotification.PushNotify(objModel.Message, objModel.Title, getUsers[i].IosUserId, "OA");
                        }
                    }

                    return Json("success!", JsonRequestBehavior.AllowGet);

                    #endregion
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }

            }
        }
    }
}