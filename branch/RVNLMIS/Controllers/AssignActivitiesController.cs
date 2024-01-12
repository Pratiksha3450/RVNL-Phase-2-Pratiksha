using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class AssignActivitiesController : Controller
    {
        public string IpAddress = "";
        [PageAccessFilter]
        public ActionResult Index()
        {
            BindDropdown();
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Construction- Assign Activities", "View", UserID, IpAddress, "View");
            return View();
        }
        #region -- Bind Dropdown list --
        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                {
                    var packages = Functions.GetRoleAccessiblePackageList();
                    ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");
          
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
        public JsonResult ServerFiltering_GetActivityGroupsList(string text)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var activityGroups = dbContext.tblActivityGroups.Where(d => d.IsDeleted == false)
                    .Select(s => new { s.ActGId, s.ActivityGroupName }).ToList();
                return Json(activityGroups, JsonRequestBehavior.AllowGet);
            }
        }

        #region -- Add Entity Activity- --

        [HttpPost]
        [Audit]
        public ActionResult Create(List<ConsActivityModel> selectedItems, int? EntityID, int activityGroupId)// CREATE
        {
            string Message = string.Empty;
            try
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;
               
                using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                {
                   
                    if (!string.IsNullOrEmpty(Convert.ToString(selectedItems)))
                    {
                        string joined = string.Join(",", selectedItems.Select(x => x.ConsActID.ToString()).ToArray());
                        db.INSERT_EntityActivity(Convert.ToInt32(EntityID), joined,activityGroupId);
                        Message = "Successfully assigned activities to selected entity.";
                        int k = Functions.SaveUserLog(pkgId, "Construction- Assign Activities", "Assign", UserID, IpAddress, "NA");
                    }
                    else
                    {
                        db.UnAssignAllActivitiesWithEntity(EntityID, activityGroupId);
                        Message = "Successfully unassigned all activities to selected entity.";
                        int k = Functions.SaveUserLog(pkgId, "Construction- Assign Activities", "Unassigned", UserID, IpAddress, "NA");
                    }
                    return Json(Message, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Warning: For delete Activity you should delete the entry from Update data first.", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region -- Bind GRID  --
        public ActionResult Activity_Details([DataSourceRequest]  DataSourceRequest request, int? entityID, int? activityGroup) // Bind Grid
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var lst = (from x in dbContext.GetEntityActivityIds(entityID, activityGroup)
                           select new ConsActivityModel
                           {
                               EntActId = Convert.ToInt32(x.EntActId),
                               ConsActID = Convert.ToInt32(x.ConsActId),
                               ActivityCode = x.ActivityCode,
                               ActivityName = x.ActivityName,
                               ActivityGroup = x.ActivityGroup,
                               ActUnit = x.ActUnit,
                               Unit = x.Unit,
                               OriginalQty = x.BudgQty
                           }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


       

        public JsonResult GetEntityUnderSection(int? sectionId)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var _EntityList = (from e in dbContext.tblMasterEntities
                                       where (e.SectionID == sectionId && e.IsDelete == false)
                                       select new drpEntityModel
                                       {
                                           EntityID = e.EntityID,
                                           EntityName = e.EntityCode + " - " + e.EntityName
                                       }).ToList();

                    return Json(_EntityList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        #region Bind Listbox of Activities
        public ActionResult Activity_DetailsList1([DataSourceRequest]  DataSourceRequest request, int? entityID, int? activityGroup)// Listbox 1
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                if (activityGroup == null)
                {
                    activityGroup = dbContext.tblActivityGroups.Where(d => d.IsDeleted == false).Select(a => a.ActGId).FirstOrDefault();
                }

                var lst = (from x in dbContext.GetNotExistActivityListByEntity(entityID, activityGroup)
                           select new ConsActivityModel
                           {
                               ConsActID = (int)x.ConsActId,
                               ActivityCode = x.ActivityCode,
                               ActivityName = x.ActivityCode + "-" + x.ActivityName,
                               ActGId = (int)x.ActGId
                           }).ToList();

                return Json(lst, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Activity_DetailsList2([DataSourceRequest]  DataSourceRequest request, int? entityID, int? activityGroup) // second list
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    //if (activityGroup == null)
                    //{
                    //    activityGroup = dbContext.tblActivityGroups.Where(d => d.IsDeleted == false).Select(a => a.ActGId).FirstOrDefault();
                    //}

                    var lst1 = (from x in dbContext.GetExistActivityListByEntity(entityID,activityGroup)
                                select new ConsActivityModel
                                {
                                    ConsActID = (int)x.ConsActId,
                                    ActivityCode = x.ActivityCode,
                                    ActivityName = x.ActivityCode + "-" + x.ActivityName
                                }).ToList();

                    return Json(lst1, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion


        #region UPDATE Activity Quantity
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditingCustom_Update([DataSourceRequest] DataSourceRequest request,
         [Bind(Prefix = "models")]IEnumerable<ConsActivityModel> objEntAct) // EDIT QTY, Rate 
        {
            if (objEntAct != null)
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;

                foreach (var item in objEntAct)
                {
                    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                    {
                        var target = dbContext.tblConsEntActs.Where(o => o.EntActId == item.EntActId).FirstOrDefault();
                        if (target != null)
                        {
                            tblConsEntAct obj = dbContext.tblConsEntActs.Where(o => o.EntActId == target.EntActId).FirstOrDefault();
                            obj.BudgQty = item.OriginalQty;
                            obj.Unit = item.Unit;
                            dbContext.SaveChanges();
                            int k = Functions.SaveUserLog(pkgId, "Construction- Assign Activities", "Update", UserID, IpAddress, "Qty & Unit");
                        }
                    }
                }
            }
            return Json(objEntAct.ToDataSourceResult(request, ModelState));
        }
        #endregion
    }
}