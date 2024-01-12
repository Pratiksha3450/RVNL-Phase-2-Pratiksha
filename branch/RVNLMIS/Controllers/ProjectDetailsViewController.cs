using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class ProjectDetailsViewController : Controller
    {
        //[PageAccessFilter]
        public ActionResult Index(int id)
        {
            var ProjectId = id;
            var builder = new StringBuilder();
            ProjectModel obj = new ProjectModel();
            try
            {
                if (ProjectId != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oProjectDetails = db.ProjectDetailsViews.Where(o => o.ProjectId == ProjectId && o.IsDeleted == false).SingleOrDefault();
                        if (oProjectDetails != null)
                        {

                            obj.ProjectId = oProjectDetails.ProjectId;
                            obj.ProjectName = oProjectDetails.ProjectCode + " - " + oProjectDetails.ProjectName;
                            obj.ProjectFullName = string.IsNullOrEmpty(Convert.ToString(oProjectDetails.ProjectFullName)) ? "NA" : oProjectDetails.ProjectFullName;
                            obj.DateOfTransfer = oProjectDetails.DateOfTransfer;
                            obj.DateOfTransfers = string.IsNullOrEmpty(Convert.ToString(oProjectDetails.DateOfTransfer)) ? "NA" : Convert.ToDateTime(Convert.ToString(oProjectDetails.DateOfTransfer)).ToString("yyyy-MM-dd");
                            obj.ProjectLength = oProjectDetails.ProjectLength;
                            obj.ProjectCode = oProjectDetails.ProjectCode;
                            obj.RailwayCode = oProjectDetails.RailwayCode;
                            obj.ProjectTypeName = oProjectDetails.ProjectTypeName;
                            obj.EDName = Convert.ToString(oProjectDetails.EDName);
                            obj.PIUName = oProjectDetails.PIUCode + " - " + oProjectDetails.PIUName;
                            obj.AnticipatedValue = oProjectDetails.AnticipatedValue;
                            obj.AnticipatedValues = oProjectDetails.AnticipatedValues;
                            obj.ValueTillDate = oProjectDetails.ValueTillDate;
                            obj.ValueTillDates = oProjectDetails.ValueTillDates;
                        }

                    }
                }

                ViewBag.PackageDetails = PackageDetails(ProjectId);
                //return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
                return View(obj);
            }
            catch (Exception ex)
            {
                return Json("1");
            }
        }

        public string PackageDetails(int projectId)
        {
            var builder = new StringBuilder();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    ///int PackageCount = dbContext.GetNullPackageFields(projectId).Where(a => a.ErrorMsg != null).Count();
                    var lst = dbContext.PackageDetailsViews.Where(a => a.ProjectId == projectId).ToList();
                    if (lst.Count() != 0)
                    {
                        foreach (var item in lst)
                        {
                            //builder.AppendLine("<div class='align-middle m-b-10'>");
                            //builder.AppendLine("<div class='d-inline-block'>");
                            //builder.AppendLine("<a href='#!'><h6>" + item.PackageName + "</h6></a>");
                            //builder.AppendLine("<p class='m-b-0 text-danger'>" + item.PackageCode + "</p>");
                            ////builder.AppendLine("<button id='btnPackageDetails' data-url= '/PackageDetailsView/GetPackageDetailsById/" + item.PackageId + "' class='btn btn-xs btn-warning'><i class='fas fa-arrow-right'></i></button></div></div>");
                            //builder.AppendLine("<a href=\"/PackageDetailsView/GetPackageDetailsById?id=" + item.PackageId + "\" class='btn btn-xs btn-warning'><i class='status deactive fas fa-arrow-right'></i></a></div></div>");
                            //builder.AppendLine("<hr class='my- 2'>");
                            builder.AppendLine("<div class='align-middle m-b-10'>");
                            builder.AppendLine("<div class='d-inline-block'>");
                            builder.AppendLine("<a href='#!'><h6>" + item.PackageName + "</h6></a>");
                            builder.AppendLine("<p class='m-b-0 text-danger'>" + item.PackageCode + "</p>");
                            // builder.AppendLine("<button id='btnPackageDetails' data-url= '/DataMissingReport/EditPackage/" + item.PackageId + "' class='status deactive btn btn-xs btn-warning'><i class='fas fa-arrow-right'></i></button></div></div>");
                            builder.AppendLine("<a class='status deactive btn btn-xs btn-warning'  href='/PackageDetailsView/Index/" + item.PackageId + "'><i class='fas fa-arrow-right'></i></a></div></div>");
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
                    return builder.ToString();
                    // return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return "1";
                //return Json("1");
            }
        }
    }
}