using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class PMCReportTypeController : Controller
    {
        public string IpAddress = "";
        
        [PageAccessFilter]
        public ActionResult Index()
        {
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "PMC Report Type", "View", UserID, IpAddress, "View");
            return View();
        }
        #region --- List PMCReport Type Values ---
        public ActionResult PMCReportType_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.tblPMCReportTypes
                           where x.IsDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new PMCReportType
                               {
                                   PRId = s.x.PRId,
                                   PMCReportTypeName = s.x.PMCReportType,
                                   //CreatedOn = s.x.CreatedOn
                                   IsDeleted = s.x.IsDeleted

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit PMCReport Type Details --
        public ActionResult EditPMCReportTypeByPRId(int id)
        {
            int entiytTypeId = id;
            PMCReportType objModel = new PMCReportType();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oPMCReportTypeDetails = db.tblPMCReportTypes.Where(o => o.PRId == id).SingleOrDefault();
                        if (oPMCReportTypeDetails != null)
                        {
                            objModel.PRId = oPMCReportTypeDetails.PRId;
                            objModel.PMCReportTypeName = oPMCReportTypeDetails.PMCReportType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditPMCReportType", objModel);
        }
        #endregion

        #region -- Add and Update PMC Report Types Details --
        [HttpPost]
        public ActionResult AddPMCReportTypes(PMCReportType oModel)
        {
            int PMCReportTypeId = oModel.PRId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblPMCReportTypes.Where(u => (u.PMCReportType == oModel.PMCReportTypeName) && u.IsDeleted == false).FirstOrDefault();

                        if (PMCReportTypeId == 0)
                        {
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblPMCReportType objPMCReportType = new tblPMCReportType();
                                //objDiscipline.DispId = oModel.DisciplineId;
                                objPMCReportType.PMCReportType = oModel.PMCReportTypeName;
                                objPMCReportType.IsDeleted = false;
                                db.tblPMCReportTypes.Add(objPMCReportType);
                                db.SaveChanges();
                                message = "Added Successfully";
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                int k = Functions.SaveUserLog(pkgId, "PMC Report Type", "Save", UserID, IpAddress, "Report Type: " + oModel.PMCReportTypeName);
                            }
                        }
                        else
                        {

                            var exist1 = db.tblPMCReportTypes.Where(u => u.PMCReportType == oModel.PMCReportTypeName && u.IsDeleted == false && u.PRId != oModel.PRId).ToList();
                            if (exist1.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblPMCReportType objPMCReportType = db.tblPMCReportTypes.Where(o => o.PRId == oModel.PRId).SingleOrDefault();
                                objPMCReportType.PMCReportType = oModel.PMCReportTypeName;
                                objPMCReportType.IsDeleted = false;
                                db.SaveChanges();
                                message = "Updated Successfully";
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                int k = Functions.SaveUserLog(pkgId, "PMC Report Type", "Update", UserID, IpAddress, "Report Type: " + oModel.PMCReportTypeName);
                            }


                        }
                        ModelState.Clear();
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ModelState.Clear();
                    return View("_PartialEditPMCReportType", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- Delete PMC Report Types Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblPMCReportType objPMCReportType = db.tblPMCReportTypes.SingleOrDefault(o => o.PRId == id);
                    objPMCReportType.IsDeleted = true;
                    db.SaveChanges();
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    int k = Functions.SaveUserLog(pkgId, "PMC Report Type", "Delete", UserID, IpAddress, "PMC Report Type" + objPMCReportType.PMCReportType);
                }
                return Json("1");
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion
    }
}