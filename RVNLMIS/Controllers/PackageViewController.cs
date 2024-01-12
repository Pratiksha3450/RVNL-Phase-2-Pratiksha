using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    //[HandleError]
    //[Authorize]
    [ApplicationAuthorize]
    [SessionAuthorize]
    public class PackageViewController : Controller
    {
        public string IpAddress = "";
        #region -------PAGE LOAD------

       // [PageAccessFilter]
        public ActionResult Index(int? id)
        {
            if (id != null)
            {
                ((UserModel)Session["UserData"]).RoleTableID = id ?? 0;
            }
            GetAllDataList();
            PackageModel objPkgList = Package_Details();
            return View(objPkgList);
        }

        #endregion

        #region -------LIST PACKAGE DETAILS-------

        public PackageModel Package_Details()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                int packageId = ((UserModel)Session["UserData"]).RoleTableID;
                int userId = ((UserModel)Session["UserData"]).UserId;
                string userName = ((UserModel)Session["UserData"]).Name;

                var lst = (from x in dbContext.PackageUserDetailsViews
                           where x.PackageId == packageId
                           select new { x }).AsEnumerable().Select(s => new PackageModel
                           {
                               ProjectId = s.x.ProjectId,
                               ProjectName = s.x.ProjectName,
                               PackageId = s.x.PackageId,
                               PackageName = s.x.PackageName,
                               PackageCode = s.x.PackageCode,
                               PackageShortName = s.x.PackageShortName,
                               BalanceValue = (float)s.x.BalanceValue,
                               BalanceValues = s.x.BalanceValues,
                               Description = s.x.Description,
                               PCPM = s.x.PCPM,
                               Client = s.x.Client,
                               PMC = s.x.PMC,
                               PackageValue = (float)s.x.PackageValue,
                               PackageValues = s.x.PackageValues,
                               CompletedValue = (float)s.x.CompletedValue,
                               CompletedValues = s.x.CompletedValues,
                               TotalKmLength = (float)s.x.TotalKmLength,
                               TotalKmLengths = s.x.TotalKmLengths,
                               PackageStart = s.x.PackageStart,
                               PackageFinish = s.x.PackageFinish,
                               ForecastComplDate = s.x.ForecastComplDate,
                               EndChainage = s.x.EndChainage,
                               Duration = s.x.Duration,
                               Durations = s.x.Durations,
                               Contractor = s.x.Contractor,
                               EoTgranted = s.x.EoTgranted,
                               StartChainage = s.x.StartChainage,
                               RevisedPackageValue = (float)s.x.RevisedPackageValue,
                               RevisedPackageValues = s.x.RevisedPackageValues,
                               SectionCount = s.x.SectionCount,
                               EntityCount = s.x.EntityCount,
                               EnggDrawingCount = s.x.EnggDrawingCount,
                               ConsActivityCount = s.x.ConsActivityCount,
                               ConsActDataUpdateCount = s.x.ConsActDataUpdateCount,
                               InvoiceCount = s.x.InvoiceCount,
                               PackageCPM = s.x.PackageCPM,
                               PackageED = s.x.PackageED,
                               ProcMatAssignCount = s.x.ProcMatAssignCount,
                               ProcMatDataCount = s.x.ProcMatDataCount,
                               UserName = userName,
                               RFICount = s.x.RFICount,
                               UpdateScore = s.x.UpdateScore,
                               UserId = userId,
                               RFIUsersCount = s.x.RFIUsersCount,
                               DispUsersCount = s.x.DispUsersCount
                           }).OrderByDescending(o => o.PackageId).FirstOrDefault();

                if (lst != null)
                {
                    PkgExpiryInfoModel oExpModel = new PkgExpiryInfoModel();
                    oExpModel = GetPkgExpiryDate(userId);
                    if (oExpModel.UserDetails != null)
                    {
                        lst.ExpiryDate = oExpModel.UserDetails.EndDate.ToString("dd MMM yyyy");
                        lst.DaysRemain = oExpModel.UserDetails.DaysRemain;
                    }
                    else
                    {
                        lst.ExpiryDate = "Subscription Expired";
                        lst.DaysRemain = 0;
                    }
                }

                return lst;
            }
        }

        public PkgExpiryInfoModel GetPkgExpiryDate(int userId)
        {
            string instanceUrl = ConfigurationManager.AppSettings["ServerPath"];
            PkgExpiryInfoModel obj = null;

            using (var client = new HttpClient())
            {
                if (instanceUrl != "https://dev.primabi.com" || instanceUrl != "http://localhost:62555")
                {
                    client.BaseAddress = new Uri("https://admin.primabi.com/");
                    var responseTask = client.GetAsync("api/SubscriptionStatus?id=" + userId);
                    responseTask.Wait();

                    var response = responseTask.Result;

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync().Result;
                        readTask = Functions.DecodeUrlString(readTask);
                        obj = JsonConvert.DeserializeObject<PkgExpiryInfoModel>(readTask);

                    }
                    else //web api sent error response 
                    {
                        //log response status here..

                        obj = new PkgExpiryInfoModel();
                    }
                }
            }
            return obj;
        }

        public class PkgExpiryInfoModel
        {
            public SubPkgModel UserDetails { get; set; }
        }

        public class SubPkgModel
        {
            public DateTime EndDate { get; set; }

            public int DaysRemain { get; set; }
        }

        #endregion

        #region -------EDIT PACKAGE-------

        [HttpGet]
        [Audit]
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
                        objModelView.PackageValues = Convert.ToString(oPackages.PackageValue);
                        objModelView.RevisedPackageValues = Convert.ToString(oPackages.RevisedPackageValue);
                        ///objModelView.CompletedValue = oPackages.CompletedValue;
                        objModelView.ReadOnly = db.InvoiceSumViews.Where(o => (o.ProjectId == oPackages.ProjectId && o.PackageId == oPackages.PackageId) && o.IsPaidPayment == false).Count();
                        objModelView.CompletedValue = GetTotelCompletedValue(oPackages.PackageId, oPackages.ProjectId, oPackages.CompletedValue);
                        objModelView.CompletedValues = Convert.ToString(objModelView.CompletedValue);
                        Session["CompletedValue"] = objModelView.CompletedValue;
                        objModelView.BalanceValues = Convert.ToString((oPackages.RevisedPackageValue == 0) ? (oPackages.PackageValue - objModelView.CompletedValue) : (oPackages.RevisedPackageValue - objModelView.CompletedValue));
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
            return View("_PartialEditPackage", objModelView);
        }

        [HttpPost]
        [Audit]
        public ActionResult AddPackagesDetails(PackageModel oModel)
        {
            GetAllDataList();
            int PackageId = oModel.PackageId;
            string Message = string.Empty;
            try
            {
                if (!ModelState.IsValid)
                {
                    oModel = new PackageModel();
                    return View("_PartialEditPackage", oModel);

                }
                else
                {
                    double? packageValue = GetDecimalValueWithoutComma(oModel.PackageValues);
                    double? revisedPackageValue = GetDecimalValueWithoutComma(oModel.RevisedPackageValues);
                    double? completedValue = GetDecimalValueWithoutComma(oModel.CompletedValues);

                    using (var db = new dbRVNLMISEntities())
                    {
                        #region --- Balance Value Validation ---

                        if (revisedPackageValue != 0)
                        {
                            if (revisedPackageValue < completedValue)
                            {
                                return Json(new { message = "3", viewHtml = "" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            if (packageValue < completedValue)
                            {
                                return Json(new { message = "4", viewHtml = "" }, JsonRequestBehavior.AllowGet); //success
                            }
                        }


                        #endregion

                        var isNameEnteredExist = db.tblPackages.Where(e => e.PackageName == oModel.PackageName && e.IsDeleted == false).FirstOrDefault();

                        tblPackage objPackage = new tblPackage();
                        objPackage = db.tblPackages.Where(o => o.PackageId == oModel.PackageId).SingleOrDefault();
                        if (objPackage != null)
                        {
                            if (objPackage.PackageName != oModel.PackageName)
                            {
                                if (isNameEnteredExist != null)
                                {
                                    return Json(new { message = "0", viewHtml = "" }, JsonRequestBehavior.AllowGet); //success
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
                            objPackage.PackageStart = (oModel.PackageStarts == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.PackageStarts);
                            objPackage.PackageFinish = (oModel.PackageFinishs == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.PackageFinishs);
                            objPackage.ForecastComplDate = (oModel.ForecastComplDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.ForecastComplDates);
                            objPackage.PackageValue = packageValue;
                            objPackage.RevisedPackageValue = revisedPackageValue;
                            objPackage.CompletedValue = completedValue;
                            if (Convert.ToString(Session["BalanceValue"]) != oModel.BalanceValues)
                            {
                                objPackage.BalanceValue = GetDecimalValueWithoutComma(oModel.BalanceValues);
                            }
                            else
                            {
                                objPackage.BalanceValue = (oModel.RevisedPackageValues == null || oModel.RevisedPackageValues == "0") ? (packageValue- completedValue) : (revisedPackageValue - completedValue);
                            }
                            objPackage.StartChainage = oModel.StartChainage;
                            objPackage.EndChainage = oModel.EndChainage;
                            objPackage.Duration = Convert.ToInt32((Convert.ToDateTime(oModel.PackageFinishs) - Convert.ToDateTime(oModel.PackageStarts)).TotalDays + 1);
                            db.SaveChanges();
                            if (Convert.ToString(Session["CompletedValue"]) != Convert.ToString(completedValue))
                            {
                                AddCompletedValueInTblInvoice(completedValue, oModel.ProjectId, oModel.PackageId);
                            }
                            Message = "2";
                            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(IpAddress))
                            {
                                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                            }
                            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                            int UserID = ((UserModel)Session["UserData"]).UserId;                         
                          
                             var UpdatedValue = (from ul in db.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();

                            if (UpdatedValue == null)
                            {
                                int k = Functions.SaveUserLog(pkgId, "Package View", "Update", UserID, IpAddress, "NA");
                            }
                            else
                            {
                                int k = Functions.SaveUserLog(pkgId, "Package View", "Update", UserID, IpAddress, Convert.ToString(UpdatedValue));
                            }                           
                           
                        }
                    }
                    PackageModel objModel = Package_Details();
                    string __PkgListView = RenderRazorViewToString("_PartialPkgInfo", objModel);
                    return Json(new { message = Message, viewHtml = __PkgListView }, JsonRequestBehavior.AllowGet); //success
                }

            }
            catch (Exception ex)
            {
                return View("_PartialEditPackage", oModel);
            }
        }

        #endregion

        #region --------SUPPORTIVE METHODS-------

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public double? GetTotelCompletedValue(int packageId, int? projectId, double? completedValue)
        {

            using (var db = new dbRVNLMISEntities())
            {
                var totelCompletedValue = db.InvoiceSumViews.Where(o => o.PackageId == packageId).Sum(o => o.PaidAmount);
                double totelSum = Convert.ToDouble(totelCompletedValue);
                return totelSum > 0 ? totelSum : completedValue;

            }
        }

        private double? GetDecimalValueWithoutComma(string packageValues)
        {
            string WithoutComma = (!string.IsNullOrEmpty(packageValues)) ? packageValues.Replace(",", "") : "0";
            return Functions.ParseDouble(WithoutComma);
        }

        public double? AddCompletedValueInTblInvoice(double? completedValue, int? projectId, int? packageId)
        {
            try
            {

                using (var db = new dbRVNLMISEntities())
                {
                    string invoiceNo = GetInvoiceNo();
                    db.AddCompletedAmount(projectId, packageId, Convert.ToDecimal(completedValue), invoiceNo);

                }

                return 00;
            }
            catch (Exception ex)
            {

                return 00;
            }
        }

        private string GetInvoiceNo()
        {
            string ou = string.Empty;
            using (var db = new dbRVNLMISEntities())
            {

                var maxPackage = db.GetInvoiceNo().FirstOrDefault();
                // var lastPackageCode = db.tblPackages.Where(o => o.IsDeleted == false).OrderByDescending(o => o.PackageCode).FirstOrDefault();
                if (maxPackage == null)
                {
                    //objPackageModel.PackageCode = "PKG1001";
                    ou = "INV-PB-0001";
                }
                else
                {
                    string get = maxPackage.Split('-')[2]; //label1.text=ATHCUS-100
                    string s = (Convert.ToInt32(get) + 1).ToString();
                    ou = "INV-PB-000" + s;
                }
                return ou;
            }
        }

        public double CalculateSectionLength(int packageId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                //double? res = dbContext.tblSections.Where(s => s.PackageId == packageId && s.IsDeleted == false).ToList().Sum(s => s.Length);
                //return res ?? 0;
                return 0;
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

        #endregion

        #region ---------BIND DROPDOWN--------

        private void GetAllDataList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<GetRoleAssignedProjectList_Result> sessionProjects = Functions.GetroleAccessibleProjectsList();
                // var projects = (List<GetRoleAssignedProjectList_Result>)Session["RoleAccessedProjects"];
                ViewBag.ProjectList = new SelectList(sessionProjects, "ProjectId", "ProjectName");
            }

        }

        #endregion
    }
}