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
    public class PIUMasterController : Controller
    {
        [PageAccessFilter]
        public ActionResult Index()
        {
            BindDropdown();
            return View();
        }

        #region -- Bind Dropdown list --
        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var EDList = dbContext.Get_EDNameWithCode().ToList();
                    ViewBag.EDList = new SelectList(EDList, "EDId", "EDName");
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region --- List PIU Values ---
        public ActionResult PIU_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var lst = (from p in dbContext.tblMasterPIUs.Where(s => s.IsDelete != true)
                           join e in dbContext.tblMasterEDs on p.EDId equals e.EDId

                           select new { e, p }).AsEnumerable().Select(s => new PIUMasterModel
                           {
                               PIUId = s.p.PIUId,
                               PIUCode = s.p.PIUCode,
                               Latitude = Convert.ToDecimal(s.p.Latitute),
                               Logitude = Convert.ToDecimal(s.p.Longitude),
                               PIUName = s.p.PIUName,
                               EDName = s.e.EDName
                           }).ToList();


                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Add PIU Details --
        [HttpPost]
        public ActionResult AddPIUDetails(PIUMasterModel oModel)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.PIUId == 0)
                        {
                            var exist = db.tblMasterPIUs.Where(u => u.PIUCode == oModel.PIUCode).ToList();
                            if (exist.Count != 0)
                            {
                                message = "PIU Code already exists";
                            }
                            else if ((db.tblMasterPIUs.Where(u => u.PIUName == oModel.PIUName).ToList().Count != 0)){
                                message = "PIU Name already exists";
                            }
                            else
                            {
                                tblMasterPIU objPIU = new tblMasterPIU();
                                objPIU.PIUId = oModel.PIUId;
                                objPIU.PIUCode = oModel.PIUCode;
                                objPIU.PIUName = oModel.PIUName;
                                objPIU.Latitute = oModel.Latitude;
                                objPIU.Longitude = oModel.Logitude;
                                objPIU.EDId = oModel.EDId;
                                objPIU.IsDelete = false;

                                db.tblMasterPIUs.Add(objPIU);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }

                        }
                        else
                        {
                            var codexist = db.tblMasterPIUs.Where(u => (u.PIUCode == oModel.PIUCode) && (u.PIUId != oModel.PIUId)).ToList();
                            if (codexist.Count != 0)
                            {
                                message = "PIU Code already exists";
                            }
                            else if (db.tblMasterPIUs.Where(u => (u.PIUName == oModel.PIUName) && (u.PIUId != oModel.PIUId)).ToList().Count()!=0)
                            {
                                message = "PIU Name already exists";
                            }
                            else
                            {
                                tblMasterPIU objPIUmodel = db.tblMasterPIUs.Where(u => u.PIUId == oModel.PIUId).SingleOrDefault();
                                objPIUmodel.PIUCode = oModel.PIUCode;
                                objPIUmodel.PIUName = oModel.PIUName;
                                objPIUmodel.Latitute = oModel.Latitude;
                                objPIUmodel.Longitude = oModel.Logitude;
                                objPIUmodel.EDId = oModel.EDId;
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
                    return View("_AddEditPIU", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- EDIT PIU Details --
        public ActionResult EditbyPIUID(int id)
        {
            BindDropdown();
            int PIUId = id;
            PIUMasterModel objModel = new PIUMasterModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oPIUDetails = db.tblMasterPIUs.Where(o => o.PIUId == id).SingleOrDefault();
                        if (oPIUDetails != null)
                        {
                            objModel.PIUId = oPIUDetails.PIUId;
                            objModel.PIUCode = oPIUDetails.PIUCode;
                            objModel.PIUName = oPIUDetails.PIUName;
                            objModel.EDId = oPIUDetails.EDId;
                            objModel.Latitude = Convert.ToDecimal(oPIUDetails.Latitute);
                            objModel.Logitude = Convert.ToDecimal(oPIUDetails.Longitude);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditPIU", objModel);
        }
        #endregion

        #region -- Delete ED Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblMasterPIU obj = db.tblMasterPIUs.SingleOrDefault(o => o.PIUId == id);
                    obj.IsDelete = true;
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

