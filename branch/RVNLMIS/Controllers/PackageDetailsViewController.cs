using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
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
    public class PackageDetailsViewController : Controller
    {
        // GET: PackageDetailsView
        //public ActionResult Index()
        //{
        //    return View();
        //}
        //[PageAccessFilter]
        public ActionResult Index(int id)
        {
            var PackageId = id;
            var builder = new StringBuilder();
            PackageModel objPackageModel = new PackageModel();
            try
            {
                if (PackageId != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {

                        var oPackageDetails = db.PackageDetailsViews.Where(o => o.PackageId == PackageId).SingleOrDefault();
                        if (oPackageDetails != null)
                        {

                            objPackageModel.PackageName = oPackageDetails.PackageCode + " - " + oPackageDetails.PackageName;
                            objPackageModel.PackageCode = oPackageDetails.PackageCode;
                            objPackageModel.PackageShortName = string.IsNullOrEmpty(Convert.ToString(oPackageDetails.PackageShortName)) ? "NA" : Convert.ToString(oPackageDetails.PackageShortName);
                            objPackageModel.Description = string.IsNullOrEmpty(Convert.ToString(oPackageDetails.Description)) ? "NA" : Convert.ToString(oPackageDetails.Description);
                            objPackageModel.Client = oPackageDetails.Client;
                            objPackageModel.Contractor = oPackageDetails.Contractor;
                            objPackageModel.PMC = oPackageDetails.PMC;
                            objPackageModel.PCPM = oPackageDetails.PCPM;
                            objPackageModel.PackageStart = (oPackageDetails.PackageStart);
                            objPackageModel.PackageStarts = string.IsNullOrEmpty(Convert.ToString(oPackageDetails.PackageStart)) ? "NA" : Convert.ToDateTime(Convert.ToString(oPackageDetails.PackageStart)).ToString("yyyy-MM-dd");
                            objPackageModel.PackageFinish = (oPackageDetails.PackageFinish);
                            objPackageModel.PackageFinishs = string.IsNullOrEmpty(Convert.ToString(oPackageDetails.PackageFinish)) ? "NA" : Convert.ToDateTime(Convert.ToString(oPackageDetails.PackageFinish)).ToString("yyyy-MM-dd");
                            objPackageModel.ForecastComplDate = oPackageDetails.ForecastComplDate;//string.IsNullOrEmpty(Convert.ToString(oPackageDetails.PackageFinish)) ? "" : Convert.ToDateTime(Convert.ToString(oPackageDetails.PackageFinish)).ToString("yyyy-MM-dd");
                            objPackageModel.ForecastComplDates = string.IsNullOrEmpty(Convert.ToString(oPackageDetails.ForecastComplDate)) ? "NA" : Convert.ToDateTime(Convert.ToString(oPackageDetails.ForecastComplDate)).ToString("yyyy-MM-dd");
                            objPackageModel.PackageValue = (oPackageDetails.PackageValue);
                            objPackageModel.RevisedPackageValue = oPackageDetails.RevisedPackageValue;
                            objPackageModel.CompletedValue = oPackageDetails.CompletedValue;
                            objPackageModel.BalanceValue = oPackageDetails.BalanceValue;
                            objPackageModel.StartChainage = string.IsNullOrEmpty(Convert.ToString(oPackageDetails.StartChainage)) ? "NA" : Convert.ToString(oPackageDetails.StartChainage);
                            // StartChainages = string.IsNullOrEmpty(Convert.ToString(oPackageDetails.StartChainage)) ? "" : Convert.ToDateTime(Convert.ToString(oPackageDetails.StartChainage)).ToString("yyyy-MM-dd");
                            objPackageModel.EndChainage = string.IsNullOrEmpty(Convert.ToString(oPackageDetails.EndChainage)) ? "NA" : Convert.ToString(oPackageDetails.EndChainage);
                            // EndChainages = string.IsNullOrEmpty(Convert.ToString(oPackageDetails.EndChainage)) ? "" : Convert.ToDateTime(Convert.ToString(oPackageDetails.EndChainage)).ToString("yyyy-MM-dd");
                            objPackageModel.TotalKmLengths = oPackageDetails.TotalKmLengths;
                            objPackageModel.Duration = Convert.ToInt32((Convert.ToDateTime(oPackageDetails.PackageFinish) - Convert.ToDateTime(oPackageDetails.PackageStart)).TotalDays + 1);

                        }

                    }
                }
                ViewBag.SectionDetails = SectionDetails(PackageId);
                //return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
                return View(objPackageModel);
            }
            catch (Exception ex)
            {
                return Json("1");
            }
        }

        public string SectionDetails(int PackageId)
        {
            var builder = new StringBuilder();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    ///int PackageCount = dbContext.GetNullPackageFields(PackageId).Where(a => a.ErrorMsg != null).Count();
                    var lst = dbContext.SectionViews.Where(a => a.PackageId == PackageId).ToList();
                    if (lst.Count() != 0)
                    {
                        foreach (var item in lst)
                        {
                            builder.AppendLine("<div class='align-middle m-b-10'>");
                            builder.AppendLine("<div class='d-inline-block'>");
                            builder.AppendLine("<a href='#!'><h6>" + item.SectionName + "</h6></a>");
                            builder.AppendLine("<p class='m-b-0 text-danger'>" + item.SectionCode + "</p>");
                            builder.AppendLine("<a class='status deactive btn btn-xs btn-warning'  href='/SectionDetailsView/Index/" + item.SectionID + "'><i class='fas fa-arrow-right'></i></a></div></div>");

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