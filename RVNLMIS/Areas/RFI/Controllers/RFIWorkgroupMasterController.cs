using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Areas.RFI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [AreaSessionExpire]
    [HandleError]
    public class RFIWorkgroupMasterController : Controller
    {
        // GET: RFIBOQMaster
        // [PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }

        #region --- List RFI Workgroup Values ---
        public ActionResult WorkGroup_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<RFIWorkgroupModel> obj = new List<RFIWorkgroupModel>();

                obj = (from x in dbContext.tblWorkGroups
                       join d in dbContext.tblDisciplines
                       on x.DispId equals d.DispId
                       where d.IsDeleted == false
                       select new { x, d })
                                       .AsEnumerable().Select(s =>
                                          new RFIWorkgroupModel
                                          {
                                              WorkgroupId = s.x.WorkGrId,
                                              WorkgroupName = s.x.WorkGrName,
                                              WorkgroupPrefix = s.x.WorkGrPrefix,
                                              Discipline = s.d.DispName
                                          }).ToList();

                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region -- Add RFI Work group Details --
        [HttpPost]
        public ActionResult SubmitRFIWorkGrp(RFIWorkgroupModel oModel)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.WorkgroupId == 0)
                        {
                            var exist = db.tblWorkGroups.Where(u => u.WorkGrName == oModel.WorkgroupName).ToList();
                            if (exist.Count != 0)
                            {
                                message = "3";
                            }
                            else
                            {
                                tblWorkGroup objWG = new tblWorkGroup();
                                objWG.WorkGrName = oModel.WorkgroupName;
                                objWG.WorkGrPrefix = oModel.WorkgroupPrefix;
                                objWG.DispId = oModel.DispId;
                                db.tblWorkGroups.Add(objWG);
                                db.SaveChanges();
                                message = "1";
                            }
                        }
                        else
                        {
                            var exist = db.tblWorkGroups.Where(u => (u.WorkGrName == oModel.WorkgroupName) && (u.WorkGrId != oModel.WorkgroupId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "3";
                            }
                            else
                            {
                                tblWorkGroup objGroupModel = db.tblWorkGroups.Where(u => u.WorkGrId == oModel.WorkgroupId).SingleOrDefault();
                                objGroupModel.WorkGrName = oModel.WorkgroupName;
                                objGroupModel.DispId = oModel.DispId;
                                objGroupModel.WorkGrPrefix = oModel.WorkgroupPrefix;
                                db.SaveChanges();
                                message = "2";
                            }
                        }
                    }
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View("_AddEditRFIWGroup", oModel);
                }
            }
            catch (Exception ex)
            {
                return Json("0", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- EDIT Group Details --
        public ActionResult EditWorkGroup(int id)
        {
            RFIWorkgroupModel objModel = new RFIWorkgroupModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oGroupDetails = db.tblWorkGroups.Where(o => o.WorkGrId == id).SingleOrDefault();
                        if (oGroupDetails != null)
                        {
                            objModel.WorkgroupId = oGroupDetails.WorkGrId;
                            objModel.WorkgroupName = oGroupDetails.WorkGrName;
                            objModel.DispId = oGroupDetails.DispId;
                            objModel.WorkgroupPrefix = oGroupDetails.WorkGrPrefix;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditRFIWGroup", objModel);
        }
        #endregion

        #region -- Delete Group Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    if(db.tblRFIActivities.Where(w=>w.WorkGroupId==id && w.isDeleted == false).FirstOrDefault() != null)
                    {
                        return Json("2", JsonRequestBehavior.AllowGet);      //cannot delete
                    }

                    tblWorkGroup obj = db.tblWorkGroups.SingleOrDefault(o => o.WorkGrId == id);
                    db.tblWorkGroups.Remove(obj);
                    db.SaveChanges();
                }
                return Json("1",JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json("-1",JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region ----SUPPORTIVE METHODS 

        public JsonResult Get_Discipline()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var discipline = dbContext.tblDisciplines.Where(d => d.IsDeleted == false)
                    .Select(s => new { s.DispId, s.DispCode }).ToList();
                return Json(discipline, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}