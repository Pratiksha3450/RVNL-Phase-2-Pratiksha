using RVNLMIS.Common;
using RVNLMIS.Controllers;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;


namespace RVNLMIS.API
{
    public class PackageController : ApiController
    {
        // GET: api/Package
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Package/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Package

        public object AddPackage(FormDataCollection form)
        {
            ApiResult obj = new ApiResult();
            PackagesController objPack = new PackagesController();

            try
            {
                int ProjectId = Functions.ParseInteger(form.Get("ProjectId"));
                string PackageName = form.Get("PackageName");
                PackageModel objPackageModel = new PackageModel();

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {

                    //  var packageId = dbContext.tblPackages.Where(o => o.ProjectId == ProjectId && o.PackageName.Contains(PackageName) && o.IsDeleted == false);
                    var packageId = dbContext.tblPackages.Where(o => o.PackageName == PackageName && o.ProjectId == ProjectId && o.IsDeleted == false).SingleOrDefault();
                    if (packageId != null)
                    {
                        obj.Code = 200;
                        obj.Msg = "success";
                        obj.Data = packageId.PackageId;
                        return obj;
                    }
                    else
                    {
                        tblPackage objMasterPackage = new tblPackage();
                        objMasterPackage.ProjectId = ProjectId;
                        objMasterPackage.PackageName = PackageName;
                        objMasterPackage.PackageCode = objPack.CreatePackageCode();
                        objMasterPackage.IsDeleted = false;
                        dbContext.tblPackages.Add(objMasterPackage);
                        dbContext.SaveChanges();
                        obj.Code = 200;
                        obj.Msg = "success";
                        obj.Data = objMasterPackage.PackageId;
                        return obj;

                    }
                }
            }
            catch (Exception ex)
            {
                obj.Code = 204;
                obj.Msg = "error" + ex.Message;
                obj.Data = "";
                return obj;
            }

        }

        //private string CreatePackageCode()
        //{
        //    PackageModel objPackageModel = new PackageModel();
        //    string ou = string.Empty;
        //    try
        //    {
        //        // TODO: Add delete logic here
        //        using (var db = new dbRVNLMISEntities())
        //        {
        //            var lastPackageCode = db.tblPackages.Where(o => o.IsDeleted == false).OrderByDescending(o => o.PackageId).FirstOrDefault();
        //            if (lastPackageCode == null)
        //            {
        //                objPackageModel.PackageCode = "PKG01";
        //            }
        //            else
        //            {
        //                string get = lastPackageCode.PackageCode.Substring(4); //label1.text=ATHCUS-100
        //                string s = (Convert.ToInt32(get) + 1).ToString();
        //                ou = "PKG0" + s;
        //            }
        //            return ou;
        //        }
        //    }
        //    catch
        //    {
        //        // _data = CreateData();
        //        return objPackageModel.PackageCode;
        //    }
        //}

        public IEnumerable<DrpOptionsModel> GetProjectList(int userId, int roleId)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var projects = dbContext.GetRoleAssignedProjectList(userId, roleId).Select(s => new DrpOptionsModel
                {
                    ID = s.ProjectId,
                    Name = s.ProjectName
                }).ToList();
                return projects;
            }
        }

        public IEnumerable<DrpOptionsModel> GetPackageList(int userId, int roleId, int projectId)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var allPackages = dbContext.GetRoleAssignedPackageList(userId, roleId).Select(s => s.PackageId).ToList();

                var onlyUnderProject = (from p in dbContext.tblPackages
                                        where p.ProjectId == projectId && p.IsDeleted == false
                                        select new DrpOptionsModel
                                        {
                                            ID = p.PackageId,
                                            Name = p.PackageName
                                        }).ToList();

                onlyUnderProject = onlyUnderProject.Where(w => allPackages.Contains(w.ID)).ToList();

                return onlyUnderProject;
            }
        }

        public HttpResponseMessage PackageDetails_Read(int packageId)
        {
            List<PackageModel> lstInfo = new List<PackageModel>();
            PackagesController objConPackage = new PackagesController();

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    lstInfo = (from x in dbContext.PackageDetailsViews
                               where x.PackageId == packageId
                               select new { x}).AsEnumerable().Select(s=> new PackageModel
                               {
                                   ProjectId =s.x.ProjectId,
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
                                   PackageValue = s.x.PackageValue,
                                   PackageValues = s.x.PackageValues,
                                   CompletedValue = objConPackage.GetTotelCompletedValue(s.x.PackageId, s.x.ProjectId),
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
                                   RevisedPackageValues = s.x.RevisedPackageValues
                               }).OrderByDescending(o => o.PackageId).ToList();
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { lstInfo });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, new { lstInfo });
            }
        }

        public HttpResponseMessage EditPackage(int id)
        {
            PackageModel objModelView = new PackageModel();
            PackagesController objConPackage = new PackagesController();

            try
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
                        ///objModelView.CompletedValue = oPackages.CompletedValue;
                        objModelView.ReadOnly = db.InvoiceSumViews.Where(o => (o.ProjectId == oPackages.ProjectId && o.PackageId == oPackages.PackageId) && o.IsPaidPayment == false).Count();
                        objModelView.CompletedValue = objConPackage.GetTotelCompletedValue(oPackages.PackageId, oPackages.ProjectId);
                        objModelView.BalanceValue = (oPackages.RevisedPackageValue == 0) ? (oPackages.PackageValue - objModelView.CompletedValue) : (oPackages.RevisedPackageValue - objModelView.CompletedValue);
                        //objModelView.BalanceValue = oPackages.BalanceValue;
                        objModelView.StartChainage = (oPackages.StartChainage);
                        // objModelView.StartChainages = string.IsNullOrEmpty(Convert.ToString(oPackages.StartChainage)) ? "" : Convert.ToDateTime(Convert.ToString(oPackages.StartChainage)).ToString("yyyy-MM-dd");
                        objModelView.EndChainage = (oPackages.EndChainage);
                        // objModelView.EndChainages = string.IsNullOrEmpty(Convert.ToString(oPackages.EndChainage)) ? "" : Convert.ToDateTime(Convert.ToString(oPackages.EndChainage)).ToString("yyyy-MM-dd");
                        objModelView.TotalKmLength = oPackages.TotalKmLength;
                        objModelView.Duration = Convert.ToInt32((Convert.ToDateTime(oPackages.PackageFinish) - Convert.ToDateTime(oPackages.PackageStart)).TotalDays + 1);

                    }
                }
                return ControllerContext.Request
                        .CreateResponse(HttpStatusCode.OK, new { objModelView });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, new { objModelView });
            }
        }

        [HttpPost]
        public HttpResponseMessage AddEditPaackage(PackageModel oModel)
        {
            try
            {
                PackagesController objConPackage = new PackagesController();
                string message = string.Empty;

                using (var db = new dbRVNLMISEntities())
                {
                    #region --- Balance Value Validation ---

                    if (oModel.RevisedPackageValue != 0)
                    {
                        if (oModel.RevisedPackageValue < oModel.CompletedValue)
                        {
                            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Revised value cannot be less than Completed value." });
                        }
                    }
                    else
                    {
                        if (oModel.PackageValue < oModel.CompletedValue)
                        {
                            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Package value cannot be less than Completed value." });
                        }
                    }

                    #endregion

                    var isNameEnteredExist = db.tblPackages.Where(e => e.PackageName == oModel.PackageName && e.IsDeleted == false).FirstOrDefault();

                    if (oModel.PackageId == 0)
                    {
                        if (isNameEnteredExist != null)
                        {
                            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Package Name is already exists." });
                        }
                        else
                        {
                            tblPackage objMasterPackage = new tblPackage();
                            objMasterPackage.ProjectId = oModel.ProjectId;
                            objMasterPackage.PackageCode = objConPackage.CreatePackageCode();
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

                            objMasterPackage.PackageValue = oModel.PackageValue;
                            objMasterPackage.RevisedPackageValue = oModel.RevisedPackageValue;
                            objMasterPackage.PMC = oModel.PMC;
                            objMasterPackage.PCPM = string.IsNullOrEmpty(oModel.PCPM) ? "Plansquare Procon Pvt Ltd" : oModel.PCPM;
                            objMasterPackage.EoTgranted = oModel.EoTgranted;
                            objMasterPackage.CompletedValue = oModel.CompletedValue;
                            objMasterPackage.BalanceValue = (oModel.RevisedPackageValue == null) ? (oModel.PackageValue - oModel.CompletedValue) : (oModel.RevisedPackageValue - oModel.CompletedValue);
                            objMasterPackage.StartChainage = oModel.StartChainage;
                            objMasterPackage.EndChainage = oModel.EndChainage;
                            objMasterPackage.TotalKmLength = 0;
                            objMasterPackage.Duration = Convert.ToInt32((Convert.ToDateTime(oModel.PackageFinishs) - Convert.ToDateTime(oModel.PackageStarts)).TotalDays + 1);
                            objMasterPackage.IsDeleted = false;

                            objMasterPackage.EoTgranted = 0;
                            db.tblPackages.Add(objMasterPackage);
                            db.SaveChanges();
                            message = "Package details are added successfully.";
                        }
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
                                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Package Name is already exists." });
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
                            objPackage.PackageValue = (oModel.PackageValue);
                            objPackage.RevisedPackageValue = oModel.RevisedPackageValue;
                            objPackage.CompletedValue = oModel.CompletedValue;

                            if (objPackage.BalanceValue != oModel.BalanceValue)
                            {
                                objPackage.BalanceValue = oModel.BalanceValue;
                            }
                            else
                            {
                                objPackage.BalanceValue = (oModel.RevisedPackageValue == null || oModel.RevisedPackageValue == 0) ? (oModel.PackageValue - oModel.CompletedValue) : (oModel.RevisedPackageValue - oModel.CompletedValue);
                            }
                            objPackage.StartChainage = oModel.StartChainage;
                            objPackage.EndChainage = oModel.EndChainage;
                           // objPackage.TotalKmLength = oModel.TotalKmLength == null ? objConPackage.CalculateSectionLength(objPackage.PackageId) : oModel.TotalKmLength;

                            if (Convert.ToString(objPackage.TotalKmLength).Contains('-'))        //Start Chainage cannot be greater than End Chainage.
                            {
                                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Start Chainage cannot be greater than End Chainage." });
                            }
                            objPackage.Duration = Convert.ToInt32((Convert.ToDateTime(oModel.PackageFinishs) - Convert.ToDateTime(oModel.PackageStarts)).TotalDays + 1);
                            db.SaveChanges();


                            objConPackage.AddCompletedValueInTblInvoice(oModel.CompletedValue, oModel.ProjectId, oModel.PackageId);

                            message = "Package details are updated successfully.";
                        }
                    }
                }
                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { message });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message });
            }
        }

        public HttpResponseMessage GetPackageCode()
        {
            string code = string.Empty;
            PackagesController objContPack = new PackagesController();

            try
            {
                code = objContPack.CreatePackageCode();
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { code });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, "Error occurred!");
            }
        }

        [HttpGet]
        public HttpResponseMessage DeletePackage(int id)
        {
            string message = string.Empty;
            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    var objTodelete = dbContext.tblPackages.Where(d => d.PackageId == id).SingleOrDefault();
                    objTodelete.IsDeleted = true;
                    dbContext.SaveChanges();
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { message = "Data deleted successfully." });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public object UpdatePackageStatus(FormDataCollection form)
        {
            ApiResult obj = new ApiResult();

            try
            {
                int PackageId = Functions.ParseInteger(form.Get("PackageId"));
                bool isSubscribed = Convert.ToBoolean(form.Get("isSubscribed"));

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    //  var packageId = dbContext.tblPackages.Where(o => o.ProjectId == ProjectId && o.PackageName.Contains(PackageName) && o.IsDeleted == false);
                    var objPackage = dbContext.tblPackages.Where(o => o.PackageId == PackageId && o.IsDeleted == false).SingleOrDefault();
                    if (objPackage != null)
                    {
                        obj.Code = 200;
                        obj.Msg = "success";
                        objPackage.IsSubscribed = isSubscribed;
                        dbContext.SaveChanges();
                        return obj;
                    }
                    else
                    {
                        obj.Code = 202;
                        obj.Msg = "Package not found";
                        return obj;

                    }
                }
            }
            catch (Exception ex)
            {
                obj.Code = 204;
                obj.Msg = "error" + ex.Message;
                obj.Data = "";
                return obj;
            }
        }
    }
}

