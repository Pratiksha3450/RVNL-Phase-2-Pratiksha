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
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
   // [ApplicationAuthorize]
    [Compress]
    [SessionAuthorize]
    //[ApplicationAuthorize]
    public class PackagesController : Controller
    {
        public string IpAddress = "";
       [PageAccessFilter]
        public ActionResult Index()
        {
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Manage Package", "View", UserID, IpAddress, "NA");
            GetAllDataList();
            return View();
        }

        //[ApplicationAuthorize]
        [Audit]
        public ActionResult Create()
        {
            PackageModel objModelView = new PackageModel();
            GetAllDataList();
            objModelView.PackageCode = CreatePackageCode();
            return View("_PartialCreate", objModelView);
        }

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
                        objModelView.CompletedValue = GetTotelCompletedValue(oPackages.PackageId, oPackages.ProjectId);
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

        public double? GetTotelCompletedValue(int packageId, int? projectId)
        {

            using (var db = new dbRVNLMISEntities())
            {
                var totelCompletedValue = db.InvoiceSumViews.Where(o => o.ProjectId == projectId && o.PackageId == packageId).Sum(o => o.PaidAmount);
                double totelSum = Convert.ToDouble(totelCompletedValue);
                return totelSum;

            }
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
                    return oModel.PackageId != 0 ? View("_PartialEditPackage", oModel) : View("_PartialCreate", oModel);

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
                            if (revisedPackageValue< completedValue)
                            {
                                return Json("3", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            if (packageValue < completedValue)
                            {
                                return Json("4", JsonRequestBehavior.AllowGet);
                            }
                        }


                        #endregion

                        var isNameEnteredExist = db.tblPackages.Where(e => e.PackageName == oModel.PackageName && e.IsDeleted == false).FirstOrDefault();

                        if (PackageId == 0)
                        {
                            if (isNameEnteredExist != null)
                            {
                                return Json("0", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                tblPackage objMasterPackage = new tblPackage();
                                objMasterPackage.ProjectId = oModel.ProjectId;
                                objMasterPackage.PackageCode = oModel.PackageCode;
                                objMasterPackage.PackageName = oModel.PackageName;
                                objMasterPackage.PackageShortName = oModel.PackageShortName;
                                objMasterPackage.Description = oModel.Description;
                                objMasterPackage.Client = string.IsNullOrEmpty(oModel.Client) ? "RVNL" : oModel.Client;
                                objMasterPackage.Contractor = oModel.Contractor;
                                //objMasterPackage.PackageStart = Convert.ToDateTime(oModel.PackageStarts);
                                //objMasterPackage.PackageFinish = Convert.ToDateTime(oModel.PackageFinishs);
                                //objMasterPackage.ForecastComplDate = Convert.ToDateTime(oModel.ForecastComplDates);
                                objMasterPackage.PackageStart = (oModel.PackageStarts == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.PackageStarts);
                                objMasterPackage.PackageFinish = (oModel.PackageFinishs == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.PackageFinishs);
                                objMasterPackage.ForecastComplDate = (oModel.ForecastComplDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.ForecastComplDates);
                                objMasterPackage.PackageValue = packageValue;
                                objMasterPackage.RevisedPackageValue = (oModel.RevisedPackageValues == null) ? 0 : revisedPackageValue;
                                objMasterPackage.PMC = oModel.PMC;
                                objMasterPackage.PCPM = string.IsNullOrEmpty(oModel.PCPM) ? "Plansquare Procon Pvt Ltd" : oModel.PCPM;
                                objMasterPackage.EoTgranted = oModel.EoTgranted;
                                // objMasterPackage.CompletedValue = oModel.CompletedValueCompletedValue
                                objMasterPackage.CompletedValue = (oModel.CompletedValues == null) ? 0 : completedValue;
                                objMasterPackage.BalanceValue = (oModel.RevisedPackageValues == null || oModel.RevisedPackageValues == "0") ? (packageValue - completedValue) : (revisedPackageValue - completedValue);
                                objMasterPackage.StartChainage = oModel.StartChainage;
                                objMasterPackage.EndChainage = oModel.EndChainage;
                                objMasterPackage.TotalKmLength = 0;

                                //if (Convert.ToString(objMasterPackage.TotalKmLength).Contains('-'))        //End Chainage cannot be greater than Start Chainage.
                                //{
                                //    return Json("5", JsonRequestBehavior.AllowGet);
                                //}
                                objMasterPackage.Duration = Convert.ToInt32((Convert.ToDateTime(oModel.PackageFinishs) - Convert.ToDateTime(oModel.PackageStarts)).TotalDays + 1);
                                objMasterPackage.IsDeleted = false;
                                objMasterPackage.EoTgranted = 0;
                                db.tblPackages.Add(objMasterPackage);
                                db.SaveChanges();
                                if (completedValue != 0)
                                {
                                    AddCompletedValueInTblInvoice(completedValue, oModel.ProjectId, objMasterPackage.PackageId);
                                }

                                Message = "1";
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                int k= Functions.SaveUserLog(pkgId, "Manage Package", "Save", UserID, IpAddress, "NA");
                            }
                            //}
                        }
                        else
                        {
                            tblPackage objPackage = new tblPackage();
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
                                //objPackage.PackageStart = Convert.ToDateTime(oModel.PackageStarts);
                                //objPackage.PackageFinish = Convert.ToDateTime(oModel.PackageFinishs);
                                //objPackage.ForecastComplDate = Convert.ToDateTime(oModel.ForecastComplDates);
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
                            }
                        }
                    }
                    return Json(Message);
                }

            }
            catch (Exception ex)
            {
                return oModel.PackageId != 0 ? View("_PartialEditPackage", oModel) : View("_PartialCreate", oModel);
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

        private void GetAllDataList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<GetRoleAssignedProjectList_Result> sessionProjects = Functions.GetroleAccessibleProjectsList();
                // var projects = (List<GetRoleAssignedProjectList_Result>)Session["RoleAccessedProjects"];
                ViewBag.ProjectList = new SelectList(sessionProjects, "ProjectId", "ProjectName");
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
        public ActionResult Package_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<GetRoleAssignedPackageList_Result> pkgs = Functions.GetRoleAccessiblePackageList();

                // var pkgs = (List<GetRoleAssignedPackageList_Result>)Session["RoleAccessPackages"];
                var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();

                var lst = (from x in dbContext.PackageDetailsViews
                           select new PackageModel
                           {
                               ProjectId = x.ProjectId,
                               ProjectName = x.ProjectName,
                               PackageId = x.PackageId,
                               PackageName = x.PackageName,
                               PackageCode = x.PackageCode,
                               PackageShortName = x.PackageShortName,
                               BalanceValue = (float)x.BalanceValue,
                               Description = x.Description,
                               PCPM = x.PCPM,
                               Client = x.Client,
                               PMC = x.PMC,
                               PackageValue = (float)x.PackageValue,
                               PackageValues = x.PackageValues,
                               CompletedValue = (float)x.CompletedValue,
                               CompletedValues = x.CompletedValues,
                               TotalKmLength = (float)x.TotalKmLength,
                               TotalKmLengths = x.TotalKmLengths,
                               PackageStart = x.PackageStart,
                               PackageFinish = x.PackageFinish,
                               ForecastComplDate = x.ForecastComplDate,
                               EndChainage = x.EndChainage,
                               Duration = x.Duration,
                               Durations = x.Durations,
                               Contractor = x.Contractor,
                               EoTgranted = x.EoTgranted,
                               StartChainage = x.StartChainage,
                               RevisedPackageValue = (float)x.RevisedPackageValue,
                               RevisedPackageValues = x.RevisedPackageValues
                           }).OrderByDescending(o => o.PackageId).ToList();
                lst = lst.Where(w => accessiblePackageList.Contains(w.PackageId)).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Audit]
        public JsonResult Delete(int id)
        {
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {
                    //db.tblSections.RemoveRange(db.tblSections.Where(o => o.SectionID == id).ToList());
                    tblPackage objPackages = db.tblPackages.SingleOrDefault(o => o.PackageId == id);
                    objPackages.IsDeleted = true;
                    // _data = CreateData();
                    db.SaveChanges();

                }
                return Json("1");
            }
            catch
            {
                // _data = CreateData();
                return Json("-1");
            }
        }

        public string CreatePackageCode()
        {
            PackageModel objPackageModel = new PackageModel();
            string ou = string.Empty;
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {

                    var maxPackage = db.GetNextPackageCode("tblPackages").ToList();
                    // var lastPackageCode = db.tblPackages.Where(o => o.IsDeleted == false).OrderByDescending(o => o.PackageCode).FirstOrDefault();
                    if (maxPackage == null)
                    {
                        //objPackageModel.PackageCode = "PKG1001";
                        ou = "PKG1001";
                    }
                    else
                    {
                        string get = maxPackage[0].Code.Split('G')[1]; //label1.text=ATHCUS-100
                        string s = (Convert.ToInt32(get) + 1).ToString();
                        ou = "PKG" + s;
                    }
                    return ou;
                }
            }
            catch (Exception ex)
            {
                // _data = CreateData();
                return objPackageModel.PackageCode;
            }
        }

    }
}