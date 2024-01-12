using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [AreaSessionExpire]
    [HandleError]
    public class RFIActivityController : Controller
    {
        #region ---- Page Load ----
        // GET: RFIModule/RFIActivity
        public ActionResult Index()
        {            
            return View();
        }

        /// <summary>
        /// Gets the constant activity.
        /// </summary>
        /// <returns></returns>
        public JsonResult Get_ConstActivity(string text)
        {
            List<RFIConsActivityModel> obj = new List<RFIConsActivityModel>();
            try
            {

                using (var db = new dbRVNLMISEntities())
                {
                    obj = db.tblConsActivities.Where(o => o.IsDelete == false && o.IsRFI == true).Select(s => new RFIConsActivityModel
                    {
                        ActivityName = s.ActivityName,
                        ConsActID = s.ConsActId

                    }).OrderBy(x => x.ActivityName).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.ActivityName, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }

                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// Gets the work group.
        /// </summary>
        /// <returns></returns>
        public JsonResult Get_WorkGroup(string text)
        {
            List<RFIWorkgroupModel> obj = new List<RFIWorkgroupModel>();
            try
            {

                using (var db = new dbRVNLMISEntities())
                {
                    obj = db.tblWorkGroups.Select(s => new RFIWorkgroupModel
                    {
                        WorkgroupId = s.WorkGrId,
                        WorkgroupName = s.WorkGrName

                    }).OrderBy(x => x.WorkgroupName).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.WorkgroupName, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }

                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region ---- Activty Grid ----        
        /// <summary>
        /// Gets the rfi activity grid.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="grpId">The GRP identifier.</param>
        /// <returns></returns>
        public ActionResult GetRFIActivityGrid([DataSourceRequest]  DataSourceRequest request, int? grpId, int? actId)
        {
            List<RFIActivityModel> lstResult = new List<RFIActivityModel>();

            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                try
                {
                    
                    lstResult = (from a in db.viewRfiActivities
                                 where a.isDeleted==false
                                 select new { a }).AsEnumerable()
                                .Select(x => new RFIActivityModel
                                {
                                    ConsActId=(int)x.a.ConsActId,
                                    ActityName=x.a.ActivityName,
                                    RFIActId=x.a.RFIActId,
                                    RFIActName=x.a.RFIActName,
                                    Unit=x.a.Unit,
                                    WorkGrName=x.a.WorkGrName,
                                    WorkGroupId= (int)x.a.WorkGroupId                                    
                                }).ToList();
                    if (grpId != 0 && grpId != null && actId != 0 && actId != null)
                    {
                        lstResult = lstResult.Where(w => w.WorkGroupId == grpId && w.ConsActId == actId).ToList();
                    }
                    else
                    {
                        if (grpId != 0 && grpId != null)
                        {
                            lstResult = lstResult.Where(w => w.WorkGroupId == grpId).ToList();
                        }

                        if (actId != 0 && actId != null)
                        {
                            lstResult = lstResult.Where(w => w.ConsActId == actId).ToList();
                        }
                    }             
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion

        #region ---- Add/Update Activity ----
        [HttpPost]
        public ActionResult AddUpdateActivity(RFIActivityModel oModel)
        {
            if (!ModelState.IsValid)
            {                
                return PartialView("_PartialAddUpdateAct", oModel);                
            }
            else
            {
                if (oModel.WorkGroupId==0||oModel.WorkGroupId==null )
                {
                    return Json(4);
                }
                if (oModel.ConsActId==0||oModel.ConsActId==null )
                {
                    return Json(5);
                }
                using (var db = new dbRVNLMISEntities())
                {
                    try
                    {
                        bool checkIfExists = CheckIfNameExistsInSameGrp(oModel);
                        if (checkIfExists)
                        {
                            return Json(3); // Activty name already exist in Group
                        }
                        if (oModel.RFIActId == 0)
                        {

                            tblRFIActivity obj = new tblRFIActivity();
                            obj.ConsActId = oModel.ConsActId;
                            obj.WorkGroupId = oModel.WorkGroupId;
                            obj.Unit = oModel.Unit;
                            obj.RFIActName = oModel.RFIActName;
                            obj.isDeleted = false;
                            db.tblRFIActivities.Add(obj);
                            db.SaveChanges();
                            return Json(1); // add success
                        }
                        else
                        {
                            var objTbl = db.tblRFIActivities.Where(o => o.RFIActId == oModel.RFIActId).SingleOrDefault();
                            if (objTbl != null)
                            {
                                objTbl.ConsActId = oModel.ConsActId;
                                objTbl.WorkGroupId = oModel.WorkGroupId;
                                objTbl.Unit = oModel.Unit;
                                objTbl.RFIActName = oModel.RFIActName;                                
                                db.SaveChanges();
                                db.SaveChanges();
                                
                            }
                            return Json(2); // update sucess
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }            
        }

        /// <summary>
        /// Checks if name exists in same GRP.
        /// </summary>
        /// <param name="oModel">The o model.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool CheckIfNameExistsInSameGrp(RFIActivityModel oModel)
        {
            using (var db = new dbRVNLMISEntities())
            {
                bool _res = db.tblRFIActivities.Any(o => o.WorkGroupId == oModel.WorkGroupId 
                                                      && o.ConsActId == oModel.ConsActId 
                                                      && o.RFIActId != oModel.RFIActId 
                                                      && o.isDeleted==false
                                                      && o.RFIActName.Equals(oModel.RFIActName, StringComparison.InvariantCultureIgnoreCase));
                return _res;
            }
        }
        #endregion

        #region --- Edit Activty ----        
        /// <summary>
        /// Edits the data.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ActionResult EditData(int id)
        {
            try
            {
                RFIActivityModel objTbl = new RFIActivityModel();
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oModel = db.tblRFIActivities.Where(o => o.RFIActId == id).SingleOrDefault();
                        if (oModel != null)
                        {
                            objTbl.RFIActId = oModel.RFIActId;
                            objTbl.ConsActId =(int) oModel.ConsActId;
                            objTbl.WorkGroupId =(int) oModel.WorkGroupId;
                            objTbl.Unit = oModel.Unit;
                            objTbl.RFIActName = oModel.RFIActName;
                        }
                    }
                }
                return View("_PartialAddUpdateAct", objTbl);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion

        #region ---- Delete Activity ----        
        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var res = db.tblRFIActivities.Where(o => o.RFIActId == id).SingleOrDefault();
                        if (res != null)
                        {
                            res.isDeleted = true;
                            db.SaveChanges();                            
                        }
                    }
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