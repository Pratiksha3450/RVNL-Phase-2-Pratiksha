using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class DataMissingReportController : Controller
    {
        // GET: DataMissingReport
        [PageAccessFilter]
        public ActionResult Index()
        {
            BindDropdown();
            return View();
        }
        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                {
                    var projects = Functions.GetroleAccessibleProjectsList();
                    ViewBag.ProjectPackageList = new SelectList(projects, "ProjectId", "ProjectName");

                    //List<GetRoleAssignedProjectList_Result> sessionProjects = Functions.GetroleAccessibleProjectsList();
                    //var pkgList = (from p in sessionProjects
                    //               select new GetRoleAssignedProjectList_Result
                    //               {
                    //                   ProjectName = p.ProjectName,
                    //                   ProjectId = p.ProjectId,

                    //               }).ToList().OrderBy(N => N.ProjectId);
                    //ViewBag.ProjectPackageList = new SelectList(pkgList, "ProjectId", "ProjectName", 0);


                    ViewBag.firstPackage = projects.FirstOrDefault().ProjectId;

                    ViewBag.EntityType = new SelectList(db.tblEntityTypes.Where(e => e.IsDeleted == false).ToList(), "Id", "EntityType");
                }
            }
            catch (Exception ex)
            {

            }
        }
        public JsonResult ServerFiltering_GetProducts(string text)
        {
            List<GetRoleAssignedProjectList_Result> sessionProjects = Functions.GetroleAccessibleProjectsList();
            if (!string.IsNullOrEmpty(text))
            {
                // sessionProjects = sessionProjects.Where(p => p.ProjectName.Contains(text, StringComparer.InvariantCultureIgnoreCase)).ToList();
                sessionProjects = sessionProjects.Where(p =>
             CultureInfo.CurrentCulture.CompareInfo.IndexOf
             (p.ProjectName, text, CompareOptions.IgnoreCase) >= 0).ToList(); ;
            }
            return Json(sessionProjects, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Project_Details(int id)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var lst = dbContext.GetNullProjectFieldsString(id).Where(a => a.ErrorMsg != null).ToList();
                    var builder = new StringBuilder();
                    int RoleId = ((UserModel)Session["UserData"]).RoleId;
                    string RoleCode = ((UserModel)Session["UserData"]).RoleCode;

                    if (lst.Count() != 0)
                    {
                        if (RoleId != 600 && RoleCode != "PKG")
                        {
                            builder.AppendLine("<div class='row'><div class='col-9 text-left pt-1'>");
                            builder.AppendLine(lst[0].ErrorMsg);
                            builder.AppendLine("</div>");
                            builder.AppendLine(" <div class='col-2'>");
                            builder.AppendLine("<button data-url= '/DataMissingReport/EditProject/" + lst[0].ProjectId + "' class='btnProjectDetails status deactive btn btn-xs btn-warning'><i class='fas fa-edit'></i></button></div></div></div>");


                        }
                    }
                    else
                    {
                        builder.AppendLine("<div>");
                        builder.AppendLine("<a href='#!'><h6> No Data Found </h6></a>");
                        builder.AppendLine("</div>");
                        builder.AppendLine("<p class='m-b-0'> - </p>");
                        builder.AppendLine("<hr class='my- 2'>");


                    }
                    return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json("1");
            }
        }
        public JsonResult PackageDetails(int projectId)
        {
            var builder = new StringBuilder();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    ///int PackageCount = dbContext.GetNullPackageFields(projectId).Where(a => a.ErrorMsg != null).Count();
                    var lst = dbContext.GetNullPackageFields(projectId).Where(a => a.ErrorMsg != null).ToList();
                    if (lst.Count() != 0)
                    {
                        foreach (var item in lst)
                        {
                            builder.AppendLine("<div class='align-middle m-b-10'>");
                            builder.AppendLine("<div class='d-inline-block'>");
                            builder.AppendLine("<a href='#!'><h6>" + item.PackageName + "</h6></a>");
                            builder.AppendLine("<p class='m-b-0 text-danger'>" + item.ErrorMsg + "</p>");
                            builder.AppendLine("<button id='btnPackageDetails' data-url= '/DataMissingReport/EditPackage/" + item.PackageId + "' class='status deactive btn btn-xs btn-warning'><i class='fas fa-edit'></i></button></div></div>");
                            builder.AppendLine("<hr class='my- 2'>");
                        }
                    }
                    else
                    {
                        builder.AppendLine("<div class='align-middle m-b-10'>");
                        builder.AppendLine("<div class='d-inline-block'>");
                        builder.AppendLine("<a href='#!'><h6> No Data Found </h6></a>");
                        builder.AppendLine("<p class='m-b-0'> - </p>");


                    }
                    return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json("1");
            }
        }



        public JsonResult SectionDetails(int projectId)
        {
            var builder = new StringBuilder();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    ///int PackageCount = dbContext.GetNullPackageFields(projectId).Where(a => a.ErrorMsg != null).Count();
                    var lst = dbContext.GetNullSectionFields(projectId).Where(a => a.ErrorMsg != null).ToList();
                    if (lst.Count() != 0)
                    {
                        foreach (var item in lst)
                        {

                            builder.AppendLine("<div class='align-middle m-b-10'>");
                            builder.AppendLine("<div class='d-inline-block'>");
                            builder.AppendLine("<a href='#!'><h6>" + item.SectionName + "</h6></a>");
                            builder.AppendLine("<p class='m-b-0 text-danger'>" + item.ErrorMsg + "</p>");
                            builder.AppendLine("<button id='btnSectionDetails' data-url= '/DataMissingReport/EditSection/" + item.SectionID + "' class='status deactive btn btn-xs btn-warning '><i class='fas fa-edit'></i></button></div></div>");
                            builder.AppendLine("<hr class='my- 2'>");
                        }
                    }
                    else
                    {
                        builder.AppendLine("<div class='align-middle m-b-10'>");
                        builder.AppendLine("<div class='d-inline-block'>");
                        builder.AppendLine("<a href='#!'><h6> No Data Found </h6></a>");
                        builder.AppendLine("<p class='m-b-0'> - </p>");



                    }
                    return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json("1");
            }
        }


        public JsonResult EntityDetails(int projectId)
        {
            var builder = new StringBuilder();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    ///int PackageCount = dbContext.GetNullPackageFields(projectId).Where(a => a.ErrorMsg != null).Count();
                    var lst = dbContext.GetNullEntityFields(projectId).Where(a => a.ErrorMsg != null).ToList();
                    if (lst.Count() != 0)
                    {
                        foreach (var item in lst)
                        {

                            builder.AppendLine("<div class='align-middle m-b-10'>");
                            builder.AppendLine("<div class='d-inline-block'>");
                            builder.AppendLine("<a href='#!'><h6>" + item.EntityName + "</h6></a>");
                            builder.AppendLine("<p class='m-b-0 text-danger'>" + item.ErrorMsg + "</p>");
                            builder.AppendLine("<button id='btnEntityDetails' data-url= '/DataMissingReport/EditEntity/" + item.EntityID + "' class='status deactive btn btn-xs btn-warning'><i class='fas fa-edit'></i></button></div></div>");
                            builder.AppendLine("<hr class='my- 2'>");

                        }
                    }
                    else
                    {
                        builder.AppendLine("<div class='align-middle m-b-10'>");
                        builder.AppendLine("<div class='d-inline-block'>");
                        builder.AppendLine("<a href='#!'><h6> No Data Found </h6></a>");
                        builder.AppendLine("<p class='m-b-0'> - </p>");

                    }
                    return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json("1");
            }
        }

        [HttpGet]
        public ActionResult EditPackage(int id)
        {
            PackageModel objModelView = new PackageModel();
            GetAllDataList();
            if (id != 0)
            {
                using (var db = new dbRVNLMISEntities())
                {

                    var oPackages = db.tblPackages.Where(o => o.PackageId == id).SingleOrDefault();
                    if (oPackages != null)
                    {
                        objModelView.PackageId = oPackages.PackageId;
                        objModelView.ProjectId = oPackages.ProjectId;
                        objModelView.PackageCode = oPackages.PackageCode;
                        objModelView.PackageName = oPackages.PackageName;
                        objModelView.PackageShortName = oPackages.PackageShortName;
                        objModelView.Description = oPackages.Description;
                        objModelView.Client = oPackages.Client;
                        objModelView.Contractor = oPackages.Contractor;
                        objModelView.PMC = oPackages.PMC;
                        objModelView.PCPM = oPackages.PCPM;
                        objModelView.PackageStart = (oPackages.PackageStart);
                        objModelView.PackageStarts = string.IsNullOrEmpty(Convert.ToString(oPackages.PackageStart)) ? "" : Convert.ToDateTime(Convert.ToString(oPackages.PackageStart)).ToString("yyyy-MM-dd");
                        objModelView.PackageFinish = (oPackages.PackageFinish);
                        objModelView.PackageFinishs = string.IsNullOrEmpty(Convert.ToString(oPackages.PackageFinish)) ? "" : Convert.ToDateTime(Convert.ToString(oPackages.PackageFinish)).ToString("yyyy-MM-dd");
                        objModelView.ForecastComplDate = oPackages.ForecastComplDate;//string.IsNullOrEmpty(Convert.ToString(oPackages.PackageFinish)) ? "" : Convert.ToDateTime(Convert.ToString(oPackages.PackageFinish)).ToString("yyyy-MM-dd");
                        objModelView.ForecastComplDates = string.IsNullOrEmpty(Convert.ToString(oPackages.ForecastComplDate)) ? "" : Convert.ToDateTime(Convert.ToString(oPackages.ForecastComplDate)).ToString("yyyy-MM-dd");
                        objModelView.PackageValue = (oPackages.PackageValue);
                        objModelView.RevisedPackageValue = oPackages.RevisedPackageValue;
                        objModelView.CompletedValue = GetTotelCompletedValue(oPackages.PackageId, oPackages.ProjectId);
                        Session["CompletedValue"] = objModelView.CompletedValue;
                        objModelView.BalanceValue = (oPackages.RevisedPackageValue == 0) ? (oPackages.PackageValue - objModelView.CompletedValue) : (oPackages.RevisedPackageValue - objModelView.CompletedValue);
                        //objModelView.BalanceValue = oPackages.BalanceValue;
                        Session["BalanceValue"] = oPackages.BalanceValue;
                        objModelView.StartChainage = (oPackages.StartChainage);
                        // objModelView.StartChainages = string.IsNullOrEmpty(Convert.ToString(oPackages.StartChainage)) ? "" : Convert.ToDateTime(Convert.ToString(oPackages.StartChainage)).ToString("yyyy-MM-dd");
                        objModelView.EndChainage = (oPackages.EndChainage);
                        // objModelView.EndChainages = string.IsNullOrEmpty(Convert.ToString(oPackages.EndChainage)) ? "" : Convert.ToDateTime(Convert.ToString(oPackages.EndChainage)).ToString("yyyy-MM-dd");
                        objModelView.TotalKmLength = oPackages.TotalKmLength;
                        objModelView.Duration = Convert.ToInt32((Convert.ToDateTime(oPackages.PackageFinish) - Convert.ToDateTime(oPackages.PackageStart)).TotalDays + 1);

                    }
                }
            }
            return View("_PartialMissingPackeageEdit", objModelView);


        }
        private double? GetTotelCompletedValue(int packageId, int? projectId)
        {

            using (var db = new dbRVNLMISEntities())
            {
                var totelCompletedValue = db.InvoiceSumViews.Where(o => o.ProjectId == projectId && o.PackageId == packageId).Sum(o => o.PaidAmount);
                double totelSum = Convert.ToDouble(totelCompletedValue);
                return totelSum;

            }
        }
        [HttpPost]
        public ActionResult AddPackagesDetails(PackageModel oModel)
        {
            string Message = string.Empty;
            tblPackage objPackage = new tblPackage();
            using (var db = new dbRVNLMISEntities())
            {
                var isNameEnteredExist = db.tblPackages.Where(e => e.PackageName == oModel.PackageName && e.IsDeleted == false).FirstOrDefault();
                objPackage = db.tblPackages.Where(o => o.PackageId == oModel.PackageId).SingleOrDefault();
                if (objPackage != null)
                {
                    if (objPackage.PackageName != oModel.PackageName)
                    {
                        if (isNameEnteredExist != null)
                        {
                            return Json("0", JsonRequestBehavior.AllowGet);
                        }
                    }
                    objPackage.PackageCode = oModel.PackageCode;
                    objPackage.ProjectId = oModel.ProjectId;
                    objPackage.PackageName = oModel.PackageName;
                    objPackage.PackageShortName = oModel.PackageShortName;
                    objPackage.Description = oModel.Description;
                    objPackage.Client = string.IsNullOrEmpty(oModel.Client) ? "RVNL" : oModel.Client;
                    objPackage.Contractor = oModel.Contractor;
                    objPackage.PMC = oModel.PMC;
                    objPackage.PCPM = string.IsNullOrEmpty(oModel.PCPM) ? "Plansquare Procon Pvt Ltd" : oModel.PCPM;
                    objPackage.EoTgranted = oModel.EoTgranted;
                    //objPackage.PackageStart = Convert.ToDateTime(oModel.PackageStarts);
                    //objPackage.PackageFinish = Convert.ToDateTime(oModel.PackageFinishs);
                    //objPackage.ForecastComplDate = Convert.ToDateTime(oModel.ForecastComplDates);
                    objPackage.PackageStart = (oModel.PackageStarts == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.PackageStarts);
                    objPackage.PackageFinish = (oModel.PackageFinishs == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.PackageFinishs);
                    objPackage.ForecastComplDate = (oModel.ForecastComplDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.ForecastComplDates);
                    objPackage.PackageValue = (oModel.PackageValue);
                    objPackage.RevisedPackageValue = oModel.RevisedPackageValue;
                    objPackage.CompletedValue = (oModel.CompletedValue == null) ? 0 : oModel.CompletedValue;
                    objPackage.BalanceValue = (objPackage.RevisedPackageValue == 0) ? (oModel.PackageValue - objPackage.CompletedValue) : (objPackage.RevisedPackageValue - objPackage.CompletedValue);
                    objPackage.StartChainage = oModel.StartChainage;
                    objPackage.EndChainage = oModel.EndChainage;
                   // objPackage.TotalKmLength = CalculatePackagesLength(objPackage);
                    objPackage.Duration = Convert.ToInt32((Convert.ToDateTime(oModel.PackageFinishs) - Convert.ToDateTime(oModel.PackageStarts)).TotalDays + 1);
                    db.SaveChanges();
                    Message = "2";
                }
            }
            return Json(Message);
        }

        private double? CalculatePackagesLength(tblPackage objPackage)
        {
            int EC = objPackage.EndChainage == null ? 0 : Convert.ToInt32(objPackage.EndChainage.Replace("+", ""));
            int SC = objPackage.StartChainage == null ? 0 : Convert.ToInt32(objPackage.StartChainage.Replace("+", ""));
            double Km = Math.Abs(EC - SC);
            return objPackage.TotalKmLength = Km / 1000;
        }

        private void GetAllDataList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<GetRoleAssignedProjectList_Result> sessionProjects = Functions.GetroleAccessibleProjectsList();

                // var projects = (List<GetRoleAssignedProjectList_Result>)Session["RoleAccessedProjects"];
                ViewBag.ProjectList = new SelectList(sessionProjects, "ProjectId", "ProjectName");
            }

        }


        public ActionResult EditSection(int id)
        {
            GetPackageList();
            int resourceId = id;
            SectionModel objModel = new SectionModel();

            if (id != 0)
            {

                using (var db = new dbRVNLMISEntities())
                {
                    var oSectionDetails = db.tblSections.Where(o => o.SectionID == id).SingleOrDefault();
                    if (oSectionDetails != null)
                    {
                        objModel.ProjectId = (oSectionDetails.ProjectId == null) ? 0 : (int)oSectionDetails.ProjectId; ;

                        objModel.SectionId = oSectionDetails.SectionID;
                        objModel.PackageId = (int)oSectionDetails.PackageId;
                        //objModel.PackageName = oResourceDetails.PackageName;
                        objModel.SectionName = oSectionDetails.SectionName;
                        objModel.SectionCode = oSectionDetails.SectionCode;
                        //objModel.SectionStart = oSectionDetails.SectionStart;
                        //objModel.SectionFinish =oSectionDetails.SectionFinish;
                        objModel.SectionStart = Convert.ToDateTime(oSectionDetails.SectionStart);
                        objModel.SectionStarts = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.SectionStart)) ? "" : Convert.ToDateTime(Convert.ToString(oSectionDetails.SectionStart)).ToString("yyyy-MM-dd");
                        objModel.SectionFinish = Convert.ToDateTime(oSectionDetails.SectionFinish);
                        objModel.SectionFinishs = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.SectionFinish)) ? "" : Convert.ToDateTime(Convert.ToString(oSectionDetails.SectionFinish)).ToString("yyyy-MM-dd");
                        objModel.StartChaining = oSectionDetails.StartChainage;
                        objModel.EndChaining = oSectionDetails.EndChainage;
                    }

                    var packagesForProject = GetPackages(objModel.ProjectId);
                    ViewBag.PackageList = new SelectList(packagesForProject, "PackageId", "PackageName");
                }
            }
            return View("_PartialMissingSectionEdit", objModel);
        }

        public void GetPackageList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var projects = Functions.GetroleAccessibleProjectsList();
                ViewBag.ProjectList = new SelectList(projects, "ProjectId", "ProjectName");

                // var pkgs = Functions.GetRoleAccessiblePackageList();
                ViewBag.PackageList = new SelectList(new List<tblPackage>(), "PackageId", "PackageName");
            }
        }
        public List<PackageModel> GetPackages(int? projectId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var _PackageList = (from e in dbContext.tblPackages
                                    where (e.ProjectId == projectId && e.IsDeleted == false)
                                    select new PackageModel
                                    {
                                        PackageId = e.PackageId,
                                        PackageName = e.PackageCode + " - " + e.PackageName
                                    }).ToList();
                return _PackageList;


            }
        }
        [HttpPost]
        public ActionResult UpdateSectionDetails(SectionModel oModel)
        {
            string message = string.Empty;
            using (var db = new dbRVNLMISEntities())
            {
                var exist = db.tblSections.Where(u => u.SectionName == oModel.SectionName && u.IsDeleted == false && u.SectionID != oModel.SectionId).ToList();
                if (exist.Count != 0)
                {
                    message = "Already Exists";
                }
                else
                {
                    tblSection objSection = db.tblSections.Where(u => u.SectionID == oModel.SectionId).SingleOrDefault();
                    objSection.ProjectId = oModel.ProjectId;
                    objSection.PackageId = oModel.PackageId;      // Add Package Dropdown
                    objSection.SectionName = oModel.SectionName;
                    objSection.SectionCode = oModel.SectionCode;
                    //objSection.SectionStart = Convert.ToDateTime(oModel.SectionStarts);
                    objSection.SectionStart = (oModel.SectionStarts == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionStarts);
                    objSection.SectionFinish = (oModel.SectionFinishs == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionFinishs);
                    //objSection.SectionFinish = Convert.ToDateTime(oModel.SectionFinishs);
                    objSection.StartChainage = oModel.StartChaining;
                    objSection.EndChainage = oModel.EndChaining;
                    objSection.IsDeleted = false;
                    objSection.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                    CalculateSectionLength(objSection);
                    db.SaveChanges();
                    message = "Updated Successfully";
                }

                //Update Package TotalKM length
                using (var dbContext = new dbRVNLMISEntities())
                {
                    dbContext.UpdatePackageLength(oModel.PackageId);
                }
            }
            ModelState.Clear();
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        public double? CalculateSectionLength(tblSection objSection)
        {
            int EC = Convert.ToInt32(objSection.EndChainage.Replace("+", ""));
            int SC = Convert.ToInt32(objSection.StartChainage.Replace("+", ""));
            return objSection.Length = Math.Abs(EC - SC);
        }

        public ActionResult EditEntity(int id)
        {
            EntityMasterModel objModel = new EntityMasterModel();

            BindDropdown();

            if (id != 0)
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {

                    objModel = (from e in dbContext.tblMasterEntities
                                join et in dbContext.tblEntityTypes on e.EntityType equals et.EntityType
                                where e.EntityID == id && et.IsDeleted == false
                                select new { e, et }).AsEnumerable().Select(s => new EntityMasterModel
                                {
                                    EntityID = s.e.EntityID,
                                    EntityCode = s.e.EntityCode,
                                    EntityName = s.e.EntityName,
                                    PackageId = (int)s.e.PackageId,
                                    ProjectId = s.e.ProjectId,
                                    SectionID = s.e.SectionID,
                                    EntityTypeId = s.et.Id.ToString(),
                                    EntityTypeName = s.et.EntityType,
                                    Lat = s.e.Lat,
                                    Long = s.e.Long,
                                    StartChainage = s.e.StartChainage,
                                    EndChainage = s.e.EndChainage,
                                    ModalHeader = "Update Entity Details"
                                }).FirstOrDefault();

                    var packagesForProject = GetPackages(objModel.ProjectId);
                    ViewBag.PackageList = new SelectList(packagesForProject, "PackageId", "PackageName");

                    var sectionsForPackage = GetSections(objModel.PackageId);
                    ViewBag.SectionList = new SelectList(sectionsForPackage, "SectionID", "SectionName");



                }
            }
            return View("_PartialMissingEntityEdit", objModel);
        }

        public List<SectionModel> GetSections(int? packageId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var _SectionList = (from e in dbContext.tblSections
                                    where (e.PackageId == packageId && e.IsDeleted == false)
                                    select new SectionModel
                                    {
                                        SectionId = e.SectionID,
                                        SectionName = e.SectionCode + " - " + e.SectionName
                                    }).ToList();
                return _SectionList;
            }
        }



        public ActionResult SubmitEntity(EntityMasterModel objModel)
        {
            try
            {
                string message = string.Empty;
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var objEdit = dbContext.tblMasterEntities.Where(e => e.EntityID == objModel.EntityID && e.IsDelete == false).SingleOrDefault();
                    objEdit.EntityName = objModel.EntityName;
                    objEdit.PackageId = objModel.PackageId;
                    objEdit.ProjectId = objModel.ProjectId;
                    objEdit.SectionID = objModel.SectionID;
                    objEdit.EntityType = objModel.EntityTypeName;
                    objEdit.Lat = objModel.Lat;
                    objEdit.Long = objModel.Long;
                    objEdit.StartChainage = objModel.StartChainage;
                    objEdit.EndChainage = objModel.EndChainage;
                    objEdit.ModifiedOn = DateTime.Now;
                    objEdit.ModifiedBy = ((UserModel)Session["UserData"]).UserId;
                    message = "Data updated successfully.";

                    dbContext.SaveChanges();
                }
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult EditProject(int id)
        {
            GetAllDataLists();
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
            return View("_PartilaMissingProjectEdit", objModelView);

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
                    return Json("1");
                }

            }
        }


        private void GetAllDataLists()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var _EdList = (from q in dbContext.tblMasterEDs
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
                                    select new
                                    {
                                        q.RailwayId,
                                        RailwayName = q.RailwayCode + " - " + q.RailwayName
                                    }).ToList();

                ViewBag.RailwayList = new SelectList(_RailwayList, "RailwayId", "RailwayName");

                var _ProjectTypeList = (from q in dbContext.tblProjectTypes
                                        select new
                                        {
                                            q.ProjectTypeId,
                                            ProjectTypeName = q.ProjectTypeCode + " - " + q.ProjectTypeName
                                        }).ToList();
                ViewBag.ProjectTypeList = new SelectList(_ProjectTypeList, "ProjectTypeId", "ProjectTypeName");


                //ViewBag.RoleTableList = new SelectList(new List<UserViewModel>(), "RoleTableId", "RoleTableName");
            }
        }

        public ActionResult AddProjectDetails(ProjectModel oModel)
        {
            using (var db = new dbRVNLMISEntities())
            {
                tblMasterProject objMasterProject = db.tblMasterProjects.Where(o => o.ProjectId == oModel.ProjectId).SingleOrDefault();


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
}