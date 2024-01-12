using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static RVNLMIS.Controllers.EnggDrawingController;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class ConstructionController : Controller
    {
        public string IpAddress = "";
        // GET: Construction
        [PageAccessFilter]
        public ActionResult Index()
        {
            BindDropdown();
            //ViewBag.EntityList = new SelectList(new List<tblMasterEntity>(), "EntityID", "EntityName");
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Construction- Update data", "View", UserID, IpAddress, "NA");

            if (((UserModel)Session["UserData"]).RoleCode == "PKG")
            {
                Session["PKGID"] = pkgId;
            }
            ContrustionDropModel ovj = new ContrustionDropModel();

            ovj.drpPackages = Functions.ParseInteger(Convert.ToString(Session["PKGID"])).ToString();
            ovj.drpEntities = Functions.ParseInteger(Convert.ToString(Session["ENTID"])).ToString();
            return View(ovj);
        }

        public ActionResult ConstructionActivity_Read([DataSourceRequest] DataSourceRequest request, string packageId, string entityId)
        {
            string sessPkg = Session["PKGID"].ToString();
            string sessEnt = Session["ENTID"].ToString();
            packageId = sessPkg == "0" ? packageId : sessPkg;
            entityId = sessEnt == "0" ? entityId : sessEnt;

            List<ConstructionViewModel> lstInfo = new List<ConstructionViewModel>();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    if (!string.IsNullOrEmpty(packageId) && string.IsNullOrEmpty(entityId))
                    {
                        int pkgId = Convert.ToInt32(packageId);
                        lstInfo = dbContext.ConstructionActivityViews.Where(e => e.PackageId == pkgId).Select(s => new ConstructionViewModel
                        {
                            AutoId = s.EntActId,
                            EntityName = s.EntityName,
                            EntityCode = s.EntityCode,
                            ActivityName = s.ActivityName,
                            BudgetedQty = s.BudgQty,
                            ActivityUnit = s.ActUnit,
                            RevisedQty = s.RevisedQty,
                            CompleteQtyToDate = s.CompletedQtyToDate
                        }).ToList();
                    }
                    else if (!string.IsNullOrEmpty(packageId) && !string.IsNullOrEmpty(entityId))
                    {
                        int pkgId = Convert.ToInt32(packageId);
                        int entId = Convert.ToInt32(entityId);
                        lstInfo = dbContext.ConstructionActivityViews.Where(e => e.PackageId == pkgId && e.EntityID == entId).Select(s => new ConstructionViewModel
                        {
                            AutoId = s.EntActId,
                            EntityName = s.EntityName,
                            EntityCode = s.EntityCode,
                            ActivityName = s.ActivityName,
                            BudgetedQty = s.BudgQty,
                            ActivityUnit = s.ActUnit,
                            RevisedQty = s.RevisedQty,
                            CompleteQtyToDate = s.CompletedQtyToDate
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(lstInfo.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            return Json(lstInfo.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEntityDrpValues(int? pkgId, string text)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    if (((UserModel)Session["UserData"]).RoleCode == "PKG")
                    {
                        pkgId = Convert.ToInt32(Session["PKGID"].ToString());
                    }
                    var entityDrpList = (from e in dbContext.tblMasterEntities
                                         where e.PackageId == pkgId && e.SectionID != 0 && e.IsDelete == false
                                         select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                                         {
                                             Id = s.e.EntityID,
                                             Name = s.e.EntityCode + " " + s.e.EntityName
                                         }).ToList();
                    if (!string.IsNullOrEmpty(text))
                    {
                        entityDrpList = entityDrpList.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.Name, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(entityDrpList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [Audit]
        public ActionResult LoadEditActivityView(int id)
        {
            ConstructionViewModel objModel = new ConstructionViewModel();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                BindDropdown();

                try
                {
                    var present = dbContext.ConstructionActivityViews.Where(t => t.EntActId == id).FirstOrDefault();

                    var entityDrpList = (from e in dbContext.tblMasterEntities
                                         where e.PackageId == present.PackageId && e.IsDelete == false
                                         select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                                         {
                                             Id = s.e.EntityID,
                                             //Name = s.e.EntityName + " " + s.e.StartChainage + " " + s.e.EndChainage
                                             Name = s.e.EntityCode + " " + s.e.EntityName
                                         }).ToList();
                    ViewBag.EntityList = new SelectList(entityDrpList, "Id", "Name");

                    if (present != null)
                    {
                        objModel.PackageId = present.PackageId;
                        objModel.AutoId = present.EntActId;
                        objModel.EntityId = present.EntityID;
                        objModel.ActivityId = present.ConsActId;
                        objModel.BudgetedQty = present.BudgQty;
                        objModel.OperationType = "Update";
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return View("_ConstructionActivityAddEdit", objModel);
        }

        [HttpPost]
        [Audit]
        public ActionResult SubmitActivityConst(ConstructionViewModel objModel)
        {
            string message = string.Empty;
            try
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    if (objModel.AutoId != 0)   //Update operation
                    {
                        if (!ModelState.IsValid)
                        {
                            BindDropdown();
                            var entityDrpList = (from e in dbContext.tblMasterEntities
                                                 where e.PackageId == objModel.PackageId && e.IsDelete == false
                                                 select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                                                 {
                                                     Id = s.e.EntityID,
                                                     // Name = s.e.EntityName + " " + s.e.StartChainage + " " + s.e.EndChainage
                                                     Name = s.e.EntityCode + " " + s.e.EntityName
                                                 }).ToList();
                            ViewBag.EntityList = new SelectList(entityDrpList, "Id", "Name");
                            return View("_ConstructionActivityAddEdit", objModel);
                        }

                        var getExisting = dbContext.tblConsEntActs.Where(d => d.EntActId == objModel.AutoId && d.IsDeleted == false).FirstOrDefault();
                        getExisting.EntityId = objModel.EntityId;
                        getExisting.ConsActId = objModel.ActivityId;
                        getExisting.BudgQty = Convert.ToDouble(objModel.BudgetedQty);
                        message = "Data updated successfully.";

                        try
                        {
                            string str = ""; ;
                            var UpdatedValue = (from ul in dbContext.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                            if (UpdatedValue != null)
                            {
                                str = UpdatedValue.Replace("CreatedOn, ", "");
                                str = str.Replace("CreatedOn", "");
                                str = str.TrimStart(',');
                            }
                            else
                            {
                                str = "NA";
                            }

                            int k = Functions.SaveUserLog(pkgId, "Construction- Update data", "Update", UserID, IpAddress, str);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else                       //Add operation
                    {
                        if (!ModelState.IsValid)
                        {
                            BindDropdown();
                            ViewBag.EntityList = new SelectList(new List<tblMasterEntity>(), "EntityID", "EntityName");
                            return View("_ConstructionActivityAddEdit", objModel);
                        }

                        double bQty = Convert.ToDouble(objModel.BudgetedQty);
                        //check is exist
                        var isexists = dbContext.tblConsEntActs.Where(a => a.EntityId == objModel.EntityId && a.ConsActId == objModel.ActivityId && a.BudgQty == bQty).FirstOrDefault();
                        if (isexists != null)
                        {
                            return Json("1", JsonRequestBehavior.AllowGet);
                        }

                        tblConsEntAct objAdd = new tblConsEntAct();
                        objAdd.EntityId = objModel.EntityId;
                        objAdd.ConsActId = objModel.ActivityId;
                        objAdd.BudgQty = Convert.ToDouble(objModel.BudgetedQty);
                        objAdd.IsDeleted = false;
                        objAdd.CreatedOn = DateTime.Now;

                        dbContext.tblConsEntActs.Add(objAdd);
                        message = "Data added successfully.";
                        int k = Functions.SaveUserLog(pkgId, "Construction- Update data", "Save", UserID, IpAddress, "Entity Name:" + objModel.EntityName);
                    }
                    dbContext.SaveChanges();

                    return Json(message, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Error occured", JsonRequestBehavior.AllowGet);
            }
        }

        [Audit]
        public JsonResult Delete(int? id)
        {
            try
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    //check transactions added
                    var checkActTrans = dbContext.tblConsEntActTrans.Where(t => t.EntActId == id).FirstOrDefault();
                    if (checkActTrans != null)
                    {
                        return Json("1", JsonRequestBehavior.AllowGet);
                    }
                    var objTodelete = dbContext.tblConsEntActs.Where(d => d.EntActId == id).SingleOrDefault();
                    objTodelete.IsDeleted = true;
                    dbContext.SaveChanges();
                    try
                    {
                        int k = Functions.SaveUserLog(pkgId, "Construction- Update data", "Delete", UserID, IpAddress, "Entity Name:" + objTodelete.tblMasterEntity.EntityName);
                    }
                    catch (Exception e)
                    {

                    }


                }
                return Json("2", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error occurred.", JsonRequestBehavior.AllowGet);
            }
        }

        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var packages = Functions.GetRoleAccessiblePackageList();
                    ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");

                    //if (packages.Count == 1)
                    //{
                    //    int pkgId = packages[0].PackageId;
                    //    var entity = dbContext.tblMasterEntities.Where(e => e.PackageId == pkgId && e.SectionID != 0 && e.IsDelete == false).Select(s => new
                    //    {
                    //        EntityId = s.EntityID,
                    //        EntityName = s.EntityCode + " " + s.EntityName
                    //    }).ToList();
                    //    ViewBag.EntityList = new SelectList(entity, "EntityID", "EntityName");
                    //}
                    //else
                    //{
                    //    ViewBag.EntityList = new SelectList(new List<tblMasterEntity>(), "EntityID", "EntityName");
                    //}

                    var activity = dbContext.tblConsActivities.ToList();
                    ViewBag.ActivityList = new SelectList(activity, "ConsActId", "ActivityName");
                }
            }
            catch (Exception ex)
            {

            }
        }


        public JsonResult BindEntityDrpValues(int? pkgId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var entityDrpList = (from e in dbContext.tblMasterEntities
                                         where e.PackageId == pkgId && e.SectionID != 0 && e.IsDelete == false
                                         select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                                         {
                                             Id = s.e.EntityID,
                                             Name = s.e.EntityCode + " " + s.e.EntityName
                                         }).ToList();
                    return Json(entityDrpList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //public JsonResult BindEntityDrpValues(string text)
        //{
        //    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
        //    {
        //        try
        //        {

        //            List<EntityMasterModel> obj = new List<EntityMasterModel>();
        //            if (!string.IsNullOrEmpty(text))
        //            {
        //                obj = obj.Where(p =>
        //               CultureInfo.CurrentCulture.CompareInfo.IndexOf
        //               (p.EntityName, text, CompareOptions.IgnoreCase) >= 0).ToList();
        //            }

        //            return Json(obj, JsonRequestBehavior.AllowGet);
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        [HttpPost]
        public ActionResult SetPkgSession(string key, string value)
        {
            if (key== "PKGID")
            {
                Session["PKGID"] = value;
            }
            else
            {
                Session["ENTID"] = value;
            }            
            return this.Json(new { success = true });
        }
    }
}