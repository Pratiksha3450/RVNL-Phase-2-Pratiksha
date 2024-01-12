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
    public class ProjectsController : Controller
    {
        // GET: Projects
        [PageAccessFilter]
        public ActionResult Index()
        {
            CreateProjectCode();

            GetAllDataList();
            return View();
        }
        public ActionResult Create()
        {
            ProjectModel objModelView = new ProjectModel();
            GetAllDataList();
            objModelView.ProjectCode = CreateProjectCode();
            return View("_PartialCreate", objModelView);
        }
        private void GetAllDataList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var _EdList = (from q in dbContext.tblMasterEDs
                               where (q.IsDelete == false)
                               select new
                               {
                                   q.EDId,
                                   EdName = q.EDCode + " - " + q.EDName
                               }).ToList();
                ViewBag.EdList = new SelectList(_EdList, "EDId", "EdName");

                //var _CPMList = (from q in dbContext.tblMasterPIUs
                //                select new
                //                {
                //                    q.PIUId,
                //                    PIUName = q.PIUCode + " - " + q.PIUName
                //                }).ToList();
                //ViewBag.CPMList = new SelectList(_CPMList, "PIUId", "PIUName");


                ViewBag.CPMList = new SelectList(new List<ProjectModel>(), "PIUId", "PIUName");


                var _RailwayList = (from q in dbContext.tblMasterRailways
                                    where (q.isDeleted == false)
                                    select new
                                    {
                                        q.RailwayId,
                                        RailwayName = q.RailwayCode + " - " + q.RailwayName
                                    }).ToList();

                ViewBag.RailwayList = new SelectList(_RailwayList, "RailwayId", "RailwayName");

                var _ProjectTypeList = (from q in dbContext.tblProjectTypes
                                        where (q.IsDelete == false)
                                        select new
                                        {
                                            q.ProjectTypeId,
                                            ProjectTypeName = q.ProjectTypeCode + " - " + q.ProjectTypeName
                                        }).ToList();
                ViewBag.ProjectTypeList = new SelectList(_ProjectTypeList, "ProjectTypeId", "ProjectTypeName");


                //ViewBag.RoleTableList = new SelectList(new List<UserViewModel>(), "RoleTableId", "RoleTableName");
            }
        }
        [HttpPost]
        public ActionResult AddProjectDetails(ProjectModel oModel)
        {
            GetAllDataList();
            int projectId = oModel.ProjectId;
            // ViewBag.RoleTableList = new SelectList(new List<UserViewModel>(), "RoleTableId", "RoleTableName");
            try
            {
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var isNameEnteredExist = db.tblMasterProjects.Where(o => o.ProjectName == oModel.ProjectName && o.IsDeleted == false).FirstOrDefault();

                        if (projectId == 0)
                        {
                            if (isNameEnteredExist != null)
                            {
                                return Json("0", JsonRequestBehavior.AllowGet);
                            }

                            tblMasterProject objMasterProject = new tblMasterProject();
                            objMasterProject.EDId = oModel.EDId;
                            objMasterProject.PIUId = oModel.PIUId;
                            objMasterProject.ProjectName = oModel.ProjectName;
                            objMasterProject.ProjectFullName = oModel.ProjectFullName;
                            objMasterProject.ProjectCode = oModel.ProjectCode;
                            objMasterProject.ProjectLength = oModel.ProjectLength;
                            objMasterProject.RailwayId = oModel.RailwayId;
                            objMasterProject.ProjectTypeId = oModel.ProjectTypeId;
                            objMasterProject.AnticipatedValue = oModel.AnticipatedValue;
                            objMasterProject.ValueTillDate = oModel.ValueTillDate;
                            //objMasterProject.DateOfTransfer = Convert.ToDateTime(oModel.DateOfTransfers);
                            objMasterProject.DateOfTransfer = (oModel.DateOfTransfers == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(oModel.DateOfTransfers);
                            objMasterProject.IsMonitorFlag = true;
                            objMasterProject.IsDeleted = false;
                            db.tblMasterProjects.Add(objMasterProject);
                            db.SaveChanges();
                            ModelState.Clear();
                            return Json("1");

                        }
                        else
                        {
                            tblMasterProject objMasterProject = db.tblMasterProjects.Where(o => o.ProjectId == oModel.ProjectId).SingleOrDefault();
                            if (objMasterProject.ProjectName != oModel.ProjectName)
                            {
                                if (isNameEnteredExist != null)
                                {
                                    return Json("0", JsonRequestBehavior.AllowGet);
                                }
                            }

                            objMasterProject.EDId = oModel.EDId;
                            objMasterProject.PIUId = oModel.PIUId;
                            objMasterProject.ProjectName = oModel.ProjectName;
                            objMasterProject.ProjectFullName = oModel.ProjectFullName;
                            objMasterProject.ProjectCode = oModel.ProjectCode;
                            objMasterProject.ProjectLength = oModel.ProjectLength;
                            objMasterProject.RailwayId = oModel.RailwayId;
                            objMasterProject.ProjectTypeId = oModel.ProjectTypeId;
                            objMasterProject.DateOfTransfer = (oModel.DateOfTransfers == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(oModel.DateOfTransfers);
                            objMasterProject.AnticipatedValue = oModel.AnticipatedValue;
                            objMasterProject.ValueTillDate = oModel.ValueTillDate;
                            db.SaveChanges();
                            ModelState.Clear();
                            return Json("2");

                        }
                    }
                }
                else
                {
                    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                    {
                        var _CPMList = (from e in dbContext.tblMasterPIUs
                                        where (e.EDId == oModel.EDId && e.IsDelete == false)
                                        select new
                                        {
                                            e.PIUId,
                                            PIUName = e.PIUCode + " - " + e.PIUName
                                        }).ToList();
                        ViewBag.CPMList = new SelectList(_CPMList, "PIUId", "PIUName");
                    }
                    oModel = new ProjectModel();
                    return View("_PartialCreate", oModel);
                }

            }
            catch (Exception ex)
            {
                return Json("-1");
            }
        }
        public ActionResult Project_Details([DataSourceRequest]  DataSourceRequest request)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    List<GetRoleAssignedProjectList_Result> sessionProjects = Functions.GetroleAccessibleProjectsList();
                    //var sessionProjects = (List<GetRoleAssignedProjectList_Result>)Session["RoleAccessedProjects"];
                    var accessibleProjectsList = sessionProjects.Select(s => s.ProjectId).ToList();

                    var lst = (from x in dbContext.ProjectDetailsViews
                               select new ProjectModel
                               {
                                   ProjectId = x.ProjectId,
                                   ProjectCode = x.ProjectCode,
                                   ProjectName = x.ProjectName,
                                   ProjectLength = x.ProjectLength,
                                   ProjectLengths = x.ProjectLengths,
                                   EDName = x.EDName,
                                   PIUName = x.PIUName,
                                   AnticipatedValue = (decimal)x.AnticipatedValue,
                                   AnticipatedValues = x.AnticipatedValues,
                                   ValueTillDate = (decimal)x.ValueTillDate,
                                   ValueTillDates = x.ValueTillDates,
                                   RailwayCode = x.RailwayCode,
                                   PIUCode = x.PIUCode,
                                   ProjectTypeName = x.ProjectTypeName,
                               }).OrderByDescending(o => o.ProjectId).ToList().Where(O => O.IsDeleted == false);

                    lst = lst.Where(w => accessibleProjectsList.Contains(w.ProjectId)).ToList();

                    return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public JsonResult GetCPMDetails(int id)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var _CPMList = (from e in dbContext.tblMasterPIUs
                                    where (e.EDId == id && e.IsDelete == false)
                                    select new
                                    {
                                        e.PIUId,
                                        PIUName = e.PIUCode + " - " + e.PIUName
                                    }).ToList();
                    ViewBag.CPMList = new SelectList(_CPMList, "PIUId", "PIUName");


                    return Json(_CPMList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new tblMasterPIU(), JsonRequestBehavior.AllowGet);
                }

            }
        }
        public ActionResult EditProjectByProjectId(int id)
        {

            GetAllDataList();
            int userId = id;
            ProjectModel objModelView = new ProjectModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oProjectDetails = db.tblMasterProjects.Where(o => o.ProjectId == id).SingleOrDefault();
                        if (oProjectDetails != null)
                        {
                            objModelView.ProjectId = oProjectDetails.ProjectId;
                            objModelView.ProjectName = oProjectDetails.ProjectName;
                            objModelView.ProjectFullName = oProjectDetails.ProjectFullName;
                            objModelView.DateOfTransfer = oProjectDetails.DateOfTransfer;
                            objModelView.DateOfTransfers = string.IsNullOrEmpty(Convert.ToString(oProjectDetails.DateOfTransfer)) ? "" : Convert.ToDateTime(Convert.ToString(oProjectDetails.DateOfTransfer)).ToString("yyyy-MM-dd");
                            objModelView.ProjectLength = oProjectDetails.ProjectLength;
                            objModelView.ProjectCode = oProjectDetails.ProjectCode;
                            objModelView.RailwayId = oProjectDetails.RailwayId;
                            objModelView.ProjectTypeId = oProjectDetails.ProjectTypeId;
                            objModelView.EDId = oProjectDetails.EDId;
                            GetCPMDetails((int)oProjectDetails.EDId);
                            objModelView.PIUId = oProjectDetails.PIUId;
                            objModelView.AnticipatedValue = oProjectDetails.AnticipatedValue;
                            objModelView.ValueTillDate = oProjectDetails.ValueTillDate;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditProject", objModelView);

        }
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {
                    var objproject = db.tblMasterProjects.SingleOrDefault(o => o.ProjectId == id);
                    if (objproject != null)
                    {
                        objproject.IsDeleted = true;
                        db.SaveChanges();
                    }


                }
                return Json("1");
            }
            catch
            {
                // _data = CreateData();
                return Json("-1");
            }
        }
        public string CreateProjectCode()
        {
            ProjectModel objUser = new ProjectModel();
            string ou = string.Empty;
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {
                    var lastProjectCode = db.tblMasterProjects.Where(o => o.IsDeleted == false).OrderByDescending(o => o.ProjectCode).FirstOrDefault();
                    if (lastProjectCode == null)
                    {
                        objUser.ProjectCode = "RVNL0001";
                    }
                    else
                    {
                        string get = lastProjectCode.ProjectCode.Substring(5); //label1.text=ATHCUS-100
                        string s = (Convert.ToInt32(get) + 1).ToString();
                        ou = "RVNL0" + s;
                    }
                    return ou;
                }
            }
            catch
            {
                // _data = CreateData();
                return objUser.ProjectCode;
            }
        }

    }
}