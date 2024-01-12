using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Areas.RFI.Controllers;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]

    public class LayoutFunctionsController : Controller
    {
        // GET: ActivitesGroups
        [PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }
        #region -----NOTIFICATIONS----

        public ActionResult CheckUnReadNotification()
        {
            CommonRFIMethodsController objCtr = new CommonRFIMethodsController();
            int userId = ((UserModel)Session["UserData"]).UserId;
            List<PushNotifyModel> notifyObj =objCtr._NotifyCommonList(userId);

            if (notifyObj.Count() != 0)
            {
                string _NotifyListView = RenderRazorViewToString("_PartialNotifyList", notifyObj);
                return Json(new { message = "1", viewHtml = _NotifyListView }, JsonRequestBehavior.AllowGet); //success
            }
            else
            {
                return Json(new { message = "0", viewHtml = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MarkNotiAsRead(int notiId)
        {
            int userId = ((UserModel)Session["UserData"]).UserId;
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var notObj = dbContext.tblNotificationReadStatus.Where(w => w.NotificationId == notiId && w.ReceiverId == userId).FirstOrDefault();
                notObj.IsRead = true;
                notObj.ReadOn = DateTime.Now;
                dbContext.SaveChanges();
            }
            return Json("1", JsonRequestBehavior.AllowGet);
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult GetNotifications(int id)
        {
            NotificationLogController objCtr = new NotificationLogController();
            List<PushNotifyModel> notifyObj = objCtr.Read_Notification(id);

            return View("_PartialNotifyLayout", notifyObj.OrderByDescending(o => o.SentOn).Take(5).ToList()); //success
        }

        public ActionResult GetNotBadgeCount()
        {
            NotificationLogController objCtr = new NotificationLogController();
            int userId = ((UserModel)Session["UserData"]).UserId;
            int count = objCtr.Read_Notification(userId).Where(n => n.IsRead == false).Count();
            return Json(count, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -------BREADCRUMBS------

        public ActionResult GetBreadCrumbsModel(string menuName, string mainParent, string subParent)
        {
            BreadCrumbModel obj = new BreadCrumbModel()
            {
                MenuName = menuName,
                MainParent = mainParent,
                SubParent = subParent
            };
            Session["SessBreadCrumb"] = obj;
            return View("_PartialBC", obj);
        }

        #endregion
    }
}