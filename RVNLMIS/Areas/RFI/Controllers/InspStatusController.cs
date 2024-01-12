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
    [Authorize]
   // [Compress]
    [SessionAuthorize]
    public class InspStatusController : Controller
    {
        // GET: InspectionStatus
        [PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }

        #region --- List InspectionStatus Values ---
        public ActionResult InspectionStatus_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.tblInspectionStatus
                           where x.IsDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new InspStatusModel
                               {
                                   InspId = s.x.InspId,                                  
                                   InspDesc = s.x.InspDesc,
                                   StatusType=s.x.StatusType,
                                   CreatedOn = s.x.CreatedOn,
                                   //IsDeleted=s.x.IsDeleted

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Add and Update InspectionStatus Details --
        [HttpPost]
        public ActionResult AddInspectionStatusDetails(InspStatusModel oModel)
        {
            int InspId = oModel.InspId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    if (InspId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblInspectionStatus.Where(u => u.StatusType == oModel.StatusType && u.IsDeleted==false).SingleOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblInspectionStatu objInspectionStatus = new tblInspectionStatu();
                                objInspectionStatus.StatusType = oModel.StatusType;
                                objInspectionStatus.InspDesc = oModel.InspDesc;
                                objInspectionStatus.IsDeleted = false;
                                objInspectionStatus.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblInspectionStatus.Add(objInspectionStatus);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblInspectionStatus.Where(u => (u.StatusType == oModel.StatusType && u.IsDeleted == false) && (u.InspId != oModel.InspId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblInspectionStatu objInspectionStatus = db.tblInspectionStatus.Where(o => o.InspId == oModel.InspId).SingleOrDefault();
                                objInspectionStatus.StatusType = oModel.StatusType;
                                objInspectionStatus.InspDesc = oModel.InspDesc;
                                objInspectionStatus.IsDeleted = false;
                                objInspectionStatus.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }
                        }
                    }
                    ModelState.Clear();
                    //return View("Index", oModel);
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_PartialEditInspectionStatus", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- Edit InspectionStatus Details --
        public ActionResult EditInspectionStatusByInspId(int id)
        {
            int InspId = id;
            InspStatusModel objModel = new InspStatusModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oInspectionStatusDetails = db.tblInspectionStatus.Where(o => o.InspId == id).SingleOrDefault();
                        if (oInspectionStatusDetails != null)
                        {
                            objModel.InspId = oInspectionStatusDetails.InspId;                         
                            objModel.InspDesc = oInspectionStatusDetails.InspDesc;
                            objModel.StatusType = oInspectionStatusDetails.StatusType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditInspectionStatus", objModel);
        }
        #endregion

        #region -- Delete InspectionStatus Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblInspectionStatu objDisp = db.tblInspectionStatus.SingleOrDefault(o => o.InspId == id);
                    objDisp.IsDeleted = true;
                    db.SaveChanges();
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