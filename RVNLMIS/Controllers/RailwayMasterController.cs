using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
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
    public class RailwayMasterController : Controller
    {
        [PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }

        #region --- List Railway Values ---
        public ActionResult Railway_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.tblMasterRailways.Where(s=>s.isDeleted!=true)
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new RailwayMasterModel
                               {
                                   RailwayId = s.x.RailwayId,
                                   RailwayCode = s.x.RailwayCode,
                                   RailwayName = s.x.RailwayName

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Add Railway Details --
        [HttpPost]
        public ActionResult AddRailwayDetails(RailwayMasterModel oModel)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.RailwayId == 0)
                        {
                            var exist = db.tblMasterRailways.Where(u => u.RailwayCode == oModel.RailwayCode ).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblMasterRailway objRailway = new tblMasterRailway();
                                objRailway.RailwayId = oModel.RailwayId;
                                objRailway.RailwayCode = oModel.RailwayCode;
                                objRailway.RailwayName = oModel.RailwayName;
                                db.tblMasterRailways.Add(objRailway);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                        else
                        {
                            var exist = db.tblMasterRailways.Where(u => (u.RailwayName == oModel.RailwayName) && (u.RailwayId != oModel.RailwayId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblMasterRailway objRailwaymodel = db.tblMasterRailways.Where(u => u.RailwayId == oModel.RailwayId).SingleOrDefault();
                                objRailwaymodel.RailwayCode = oModel.RailwayCode;
                                objRailwaymodel.RailwayName = oModel.RailwayName;
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }
                        }

                    }
                    ModelState.Clear();
                    // return View("Index", oModel);
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_AddEditRailway", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- EDIT Railway Details --
        public ActionResult EditbyRailwayID(int id)
        {
            int RailwayId = id;
            RailwayMasterModel objModel = new RailwayMasterModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oRailwayDetails = db.tblMasterRailways.Where(o => o.RailwayId == id).SingleOrDefault();
                        if (oRailwayDetails != null)
                        {
                            objModel.RailwayId = oRailwayDetails.RailwayId;
                            objModel.RailwayCode = oRailwayDetails.RailwayCode;
                            objModel.RailwayName = oRailwayDetails.RailwayName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditRailway", objModel);
        }
        #endregion

        #region -- Delete Railway Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblMasterRailway obj = db.tblMasterRailways.SingleOrDefault(o => o.RailwayId == id);
                    obj.isDeleted = true;
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