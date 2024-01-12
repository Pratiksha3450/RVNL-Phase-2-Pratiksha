 using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    public class MilestoneController : Controller
    {
       
        string milecode;
        // GET: Milestone
        public ActionResult Index()
        {
            
            GetPackages();
            GetProjects();
            return View();
        }

        public ActionResult _AddEditMileStone()
        {
            MilestoneModel milestone = new MilestoneModel();
            GetProjects();
            GetPackages();
            return View();
        }

        public ActionResult GetProjects()
        {
            dbRVNLMISEntities db = new dbRVNLMISEntities();
            var projects = Functions.GetroleAccessibleProjectsList();
            SelectList list = new SelectList(projects, "ProjectId", "ProjectName");
            ViewBag.ProjectList = list;
            return View();
        }

        public ActionResult GetPackages()
        {
            dbRVNLMISEntities db = new dbRVNLMISEntities();
            var pkgs = db.tblPackages.ToList();
            SelectList list = new SelectList(pkgs, "PackageId", "PackageName");
            ViewBag.PackageList = list;
            return View();
        }

        public string GenerateMileCode()
        {
            dbRVNLMISEntities db = new dbRVNLMISEntities();
            int id;
            int maxId = db.tblMilestones.Max(p => p.MilestoneId);
            if (maxId <= 0)
            {
                milecode = "MA-001";
            }
            else
            {
                id = maxId + 1;
                milecode = "MA-" + id;
            }
            return milecode;
        }

        #region -- Add and Update Mile Details --
        [HttpPost]
    
        public ActionResult AddEditMileDetails(MilestoneModel oModel)
        { 
            try
            {
                    string message = string.Empty;
                    if (oModel.MilestoneId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblMilestones.Where(u => u.MileName == oModel.MileName && u.ProjectId == oModel.ProjectId && u.IsDeleted == false).FirstOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                                return Json(message, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                GenerateMileCode();
                                tblMilestone objmile = new tblMilestone();
                                objmile.ProjectId = oModel.ProjectId;
                                objmile.PackageId = oModel.PackageId;      // Add Package Dropdown
                                objmile.PrimaMileCode = milecode;
                                objmile.MileName = oModel.MileName;
                                objmile.MilePlanDate = Convert.ToDateTime(oModel.MilePlanDate);
                                objmile.ContractMonitor = oModel.ContractMonitor;
                                objmile.Revision = oModel.Revision;
                                objmile.IsDeleted = false;
                                objmile.ProjCompFlag = Convert.ToBoolean(oModel.ProjCompFlag);
                                objmile.isActive = Convert.ToBoolean(oModel.isActive);
                            db.tblMilestones.Add(objmile);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            tblMilestone objmile = db.tblMilestones.Where(u => u.MilestoneId == oModel.MilestoneId).SingleOrDefault();
                            objmile.ProjectId = oModel.ProjectId;
                            objmile.PackageId = oModel.PackageId;      // Add Package Dropdown
                            objmile.PrimaMileCode = oModel.PrimaMileCode;
                            objmile.MileName = oModel.MileName;
                            objmile.MilePlanDate = Convert.ToDateTime(oModel.MilePlanDate);
                            objmile.ContractMonitor = oModel.ContractMonitor;
                            objmile.Revision = oModel.Revision;
                            objmile.IsDeleted = false;
                            objmile.ProjCompFlag = Convert.ToBoolean(oModel.ProjCompFlag);
                            objmile.isActive = Convert.ToBoolean(oModel.isActive);
                            db.SaveChanges();
                            message = "Updated Successfully";

                        }
                    }
                    ModelState.Clear();
                    return Json(message, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }

        }
        #endregion

        #region -- Edit Resource Details --
        [Audit]
        public ActionResult EditMileByMileId(int id)
        {
            GetPackages();
            GetProjects();
            int resourceId = id;
            MilestoneModel objModel = new MilestoneModel();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var oMileDetails = db.tblMilestones.Where(o => o.MilestoneId == id && o.IsDeleted == false).SingleOrDefault();
                        if (oMileDetails != null)
                        {
                            objModel.ProjectId = (oMileDetails.ProjectId == null) ? 0 : (int)oMileDetails.ProjectId; ;
                            objModel.MilestoneId = oMileDetails.MilestoneId;
                            objModel.PackageId = (int)oMileDetails.PackageId;
                            objModel.PrimaMileCode = oMileDetails.PrimaMileCode;
                            objModel.MileName = oMileDetails.MileName;
                            objModel.MilePlanDate = string.IsNullOrEmpty(Convert.ToString(oMileDetails.MilePlanDate)) ? "" : Convert.ToDateTime(Convert.ToString(oMileDetails.MilePlanDate)).ToString("yyyy-MM-dd");
                            objModel.ContractMonitor = oMileDetails.ContractMonitor;
                            objModel.Revision = oMileDetails.Revision;
                            objModel.ProjCompFlag = Convert.ToBoolean(oMileDetails.ProjCompFlag);
                            objModel.isActive = Convert.ToBoolean (oMileDetails.isActive);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditMileStone", objModel);
        }
        #endregion

        public JsonResult Get_PackageByProject(int? id)
        {
            List<PackageModel> _PackageList = new List<PackageModel>();

            try
            {
                string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                int userId = ((UserModel)HttpContext.Session["UserData"]).UserId;
                int roleId = ((UserModel)HttpContext.Session["UserData"]).RoleId;

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    if (roleCode == "PKG")
                    {
                        _PackageList = dbContext.GetRoleAssignedPackageList(userId, roleId).Select(s => new PackageModel
                        {
                            PackageId = s.PackageId,
                            PackageName = s.PackageName
                        }).ToList();
                    }
                    else
                    {
                        _PackageList = (from e in dbContext.tblPackages
                                        where (e.ProjectId == id && e.IsDeleted == false)
                                        select new PackageModel
                                        {
                                            PackageId = e.PackageId,
                                            PackageName = e.PackageCode + " - " + e.PackageName
                                        }).ToList();
                    }
                    return Json(_PackageList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(_PackageList, JsonRequestBehavior.AllowGet);
            }
        }

        #region -- Delete Mile Details --
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblMilestone obj = db.tblMilestones.FirstOrDefault(o => o.MilestoneId == id);
                    obj.IsDeleted = true;
                    db.SaveChanges();
                    return Json("success");
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }
        #endregion



        #region --- List Resource Values ---
        public ActionResult MileStone_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var pkgs = Functions.GetRoleAccessiblePackageList();
                var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();

                var lst = (from x in dbContext.MilestoneViews

                           select new MilestoneModel
                           {
                               ProjectId = (int)x.ProjectId,
                               ProjectName = x.ProjectName,
                               MilestoneId = x.MilestoneId,
                               PackageId = (int)x.PackageId,
                               PackageName = x.PackageName,
                               PackageCode = x.PackageCode,
                               MileName = x.MileName,
                               PrimaMileCode = x.PrimaMileCode,
                               MilePlanDate = (x.MilePlanDate).ToString(),
                               ContractMonitor = x.ContractMonitor,
                               Revision = x.Revision,
                             
                           }).OrderByDescending(o => o.MilestoneId).ToList();

                lst = lst.Where(w => accessiblePackageList.Contains((int)w.PackageId)).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion 

    }
}