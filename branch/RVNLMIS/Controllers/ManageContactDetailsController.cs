using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    public class ManageContactDetailsController : Controller
    {
        // GET: ManageContactDetails
        public ActionResult Index()
        {
            return View();
        }
        #region --- List ManageContact_Details Values ---
        public ActionResult ManageContact_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                var pkgs = Functions.GetRoleAccessiblePackageList();
                var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();

                var lst = (from x in db.SpGetPMCManagerDetails()
                           select new PackageUserContact
                           {
                               AutoId = x.AutoId,
                               FullName = x.FullName,
                               Email = x.Email,
                               PackageId = x.PackageId,
                               PackageCode = x.PackageCode,
                               PackageName = x.PackageName,
                               UserName = x.UserName,
                               UserId = x.UserId,
                               NoOfUser = x.NoOfUser,
                               WhatsappNo = x.WhatsappNo,
                               IsAppUser = x.IsAppUser,
                           }).ToList();

          
                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region --- List SubManageContact_Details Values ---
        public ActionResult SubManageContact_Details(int AutoId, [DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                List<ResidentEnggDetails> lst = dbContext.SpGetSubPMCManagerDetails().Where(o=>o.AutoId== AutoId).Select(
                x => new Models.ResidentEnggDetails
                {
                    WId = x.WId,
                    AutoId = x.AutoId,
                    ResidentEngineerName = x.ResidentEngineerName,
                    ResidentEngineerName2 = x.ResidentEngineerName2,
                    ReWhatsAppNum = x.ReWhatsAppNum,
                    Re2WhatsAppNum = x.Re2WhatsAppNum

                }).ToList();
                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);


                // return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}