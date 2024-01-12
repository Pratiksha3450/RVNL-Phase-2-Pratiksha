using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using PrimaBiWeb.Common;
using RVNLMIS.Areas.RFI.Common;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.Controllers;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [AreaSessionExpire]
    [HandleError]
    public class RFISummaryController : Controller
    {
        CommonRFIMethodsController _objCommon = new CommonRFIMethodsController();

        #region ---- Page Load ----

        public ActionResult Index()
        {
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            string orgnisation = ((UserModel)Session["RFIUserSession"]).RoleCode;
            int designId = ((UserModel)Session["RFIUserSession"]).RoleId;

            RFIcountSummary obj = _objCommon._GetRfiCountObj(0, userId,designId,orgnisation);
            return View(obj);
        }

        #endregion

        public ActionResult Get_RFICount(int? id)
        {
             id = id ?? 0;
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            string orgnisation = ((UserModel)Session["RFIUserSession"]).RoleCode;
            int designId = ((UserModel)Session["RFIUserSession"]).RoleId;

            RFIcountSummary obj = _objCommon._GetRfiCountObj(id,userId,designId,orgnisation);
            return View("_PartialRFICount", obj);
        }

        #region -----BIND DROPDOWN------

        public JsonResult Get_Userslist(string text)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            int packageId = ((UserModel)Session["RFIUserSession"]).RoleTableID;
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            string org = ((UserModel)Session["RFIUserSession"]).RoleCode;
            string designName = ((UserModel)Session["RFIUserSession"]).DesignationName;

            try
            {
                obj = _objCommon._GetUsersListDrp(packageId, userId, org, designName);

                if (!string.IsNullOrEmpty(text))
                {
                    obj = obj.Where(p => p.Name.ToLower().Contains(text)).ToList();
                }
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}