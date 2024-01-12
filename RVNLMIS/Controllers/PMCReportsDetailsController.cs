using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    //[Compress]
    [SessionAuthorize]
    public class PMCReportsDetailsController : Controller
    {
        public string IpAddress = "";
        // GET: PMCReportsDetails
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
            int k = Functions.SaveUserLog(pkgId, "PMC Report", "View", UserID, IpAddress, "NA");
            return View();
        }
        #region --- Kendo Grid Data building------
        public JsonResult ServerFiltering_GetPMCReportsDetails(string text)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var PMCReportType = dbContext.tblPMCReportTypes.Where(d => d.IsDeleted == false)
                    .Select(s => new { s.PRId, s.PMCReportType }).ToList();
                return Json(PMCReportType, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        public ActionResult AddPMCReportForm(string packagesID, string pmcID)
        {
            PMCReportDetailsModel objModel = new PMCReportDetailsModel();
            objModel.PackageId = Functions.ParseInteger(packagesID);
            objModel.PRId = Functions.ParseInteger(pmcID);
            objModel.ReportingDates = DateTime.Now.ToString("yyyy-MM-dd");
            return View("_PartialPMCReportDetails", objModel);
        }

        public ActionResult PdfViewPath(int id)
        {

            if (id != 0)
            {
                PMCReportDetailsModel objModel = new PMCReportDetailsModel();
                using (var db = new dbRVNLMISEntities())
                {
                    int attID = (int)db.tblPMCReportDetails.Where(o => o.PMCReportId == id && o.IsDeleted == false).Select(o => o.AttachmentID).FirstOrDefault();
                    string Attpath = db.tblAttachments.Where(o => o.AttachmentID == attID && o.IsDeleted == false).Select(o => o.Path).FirstOrDefault();

                    if (Attpath != null)
                    {
                        objModel.AttachFilePath = ConfigurationManager.AppSettings["ServerPath"] + Attpath;

                    }
                    else
                    {
                        objModel.AttachFilePath = "../Content/images/defult.pdf";
                    }
                }
                return Json(objModel);
            }
            else
            {
                return Json("0");
            }
        }

        public ActionResult PMCReport_Details([DataSourceRequest]  DataSourceRequest request, int? packageId, int? typeId)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    List<GetRoleAssignedPackageList_Result> pkgs = Functions.GetRoleAccessiblePackageList();
                    var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();
                    var lst = (from x in dbContext.PMCReportDetailsViews
                               select new PMCReportDetailsModel
                               {
                                   PackageId = (int)x.PackageId,
                                   AttachFilePath = x.Path,
                                   PackageName = x.PackageName,
                                   PMCReportType = x.PMCReportType,
                                   ReportingDate = x.ReportingDate,
                                   ReportingDates = x.ReportingDates,
                                   Title = x.Title,
                                   Remark = x.Remark,
                                   Type = x.Type,
                                   PRId = x.PRId,
                                   PMCReportId = x.PMCReportId
                               }).ToList();
                    if (packageId == null && typeId == null)
                    {
                        lst = lst.Where(w => accessiblePackageList.Contains(w.PackageId)).ToList();
                    }
                    else if (packageId != null && typeId == null)

                    {
                        lst = lst.Where(w => w.PackageId == packageId).ToList();
                    }
                    else
                    {
                        lst = lst.Where(w => w.PackageId == packageId && w.PRId == typeId).ToList();
                    }


                    return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost]
        public ActionResult AddPMCReportDetailsInformation(PMCReportDetailsModel oModel)
        {
            string message = string.Empty;
            int attachmentId = 0;
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("_PartialPMCReportDetails", oModel);
                }
                else
                {
                    FileInfo fi = new FileInfo(oModel.AttachmentFile.FileName);
                    string extn = fi.Extension;
                    

                    if (!string.IsNullOrEmpty(oModel.AttachmentFile.FileName) && extn == ".pdf" || extn == ".xls" || extn == ".xlsx")
                    {

                        using (var db = new dbRVNLMISEntities())
                        {
                            DateTime PaymentDates = Convert.ToDateTime(oModel.ReportingDate);
                            //tblPMCReportDetail present = db.tblPMCReportDetails.Where(t => t.ReportingDate == PaymentDates && t.PRId == oModel.PRId).FirstOrDefault();

                            DateTime rpdate = Convert.ToDateTime(oModel.ReportingDates);
                            int allReady = db.tblPMCReportDetails.Where(t => t.Title == oModel.Title && t.ReportingDate == rpdate && t.IsDeleted == false && t.PackageId == oModel.PackageId).Count();
                            if (allReady == 0)
                            {
                                attachmentId = Functions.AttachmentCommonFun(oModel.AttachmentFile, DateTime.Now.ToString("yyyy_MMM_dd_HHmmss"), "PMCRepoting", "PMC Report", null);
                                tblPMCReportDetail objmodel = new tblPMCReportDetail();
                                objmodel.PRId = oModel.PRId;
                                objmodel.PMCReportId = oModel.PMCReportId;
                                objmodel.Title = oModel.Title;
                                objmodel.PackageId = oModel.PackageId;
                                objmodel.Remark = oModel.Remark;
                                objmodel.ReportingDate = Convert.ToDateTime(oModel.ReportingDates);
                                objmodel.AttachmentID = attachmentId;
                                objmodel.CreatedOn = DateTime.Now;
                                objmodel.IsDeleted = false;
                                db.tblPMCReportDetails.Add(objmodel);
                                db.SaveChanges();
                                message = "1";
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                              
                                int k = Functions.SaveUserLog(pkgId, "PMC Report", "Save", UserID, IpAddress, "Report: "+ oModel.Title);
                            }
                            else
                            {
                                message = "3";
                            }
                        }
                    }
                    else
                    {
                        message = "0";
                    }
                }

                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                message = "2";
                return Json(message, JsonRequestBehavior.AllowGet);
            }

        }

        #region -- Delete Section Details --
        [HttpPost]
        [Audit]
        public JsonResult PMCReportDetail(int id)
        {
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {
                    tblPMCReportDetail objToDelete = db.tblPMCReportDetails.FirstOrDefault(o => o.PMCReportId == id);

                    #region --Delete Attachment --

                    if (objToDelete.AttachmentID != null)
                    {
                        db.tblAttachments.Remove(db.tblAttachments.Where(a => a.AttachmentID == objToDelete.AttachmentID).FirstOrDefault());
                        // dbContext.SaveChanges();
                    }
                    #endregion

                    db.tblPMCReportDetails.Remove(objToDelete);
                    db.SaveChanges();
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                  
                    int k = Functions.SaveUserLog(pkgId, "PMC Report", "Delete", UserID, IpAddress, "Report: " + objToDelete.Title);

                }
                return Json("1");
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion

        #region -- Share PMC Report on email --

        public ActionResult PMCReportShareUrl(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var obj = (from p in db.tblPMCReportDetails
                               join a in db.tblAttachments on p.AttachmentID equals a.AttachmentID
                               where p.PMCReportId == id
                               select new
                               {
                                   p.PMCReportId,
                                   a.FileName
                               }).FirstOrDefault();

                    ViewBag.AutoId = obj.PMCReportId;
                    ViewBag.FileName = obj.FileName;

                }
                return View("_ShareUrlPartial");
            }
            catch
            {
                return View("_ShareUrlPartial");
            }
        }

        public ActionResult SubmitReportUrl(int hdnAutoId, string txtMail)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var lst = (from x in db.PMCReportDetailsViews
                               where x.PMCReportId == hdnAutoId && x.IsDeleted == false
                               select new PMCReportDetailsModel
                               {
                                   PackageId = (int)x.PackageId,
                                   AttachFilePath = x.Path,
                                   PackageName = x.PackageName,
                                   PMCReportType = x.PMCReportType,
                                   ReportingDate = x.ReportingDate,
                                   ReportingDates = x.ReportingDates,
                                   Title = x.Title,
                                   Remark = x.Remark,
                                   Type = x.Type,
                                   PRId = x.PRId,
                                   PMCReportId = x.PMCReportId
                               }).FirstOrDefault();

                    string instanceUrl = ConfigurationManager.AppSettings["ServerPath"];
                    int rs = SendEmailWithUrl(txtMail, string.Concat(instanceUrl, lst.AttachFilePath), lst.Title, lst.PMCReportType, lst.Remark);
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    //int k = Functions.SaveUserLogs(pkgId, "PMC Report", "Submit", UserID, IpAddress);
                    int k = Functions.SaveUserLog(pkgId, "PMC Report", "Submit", UserID, IpAddress, "Report URL: " + instanceUrl);

                    if (rs != 1)
                    {
                        return Json("2", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("1", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("2", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Sends the email to share url
        /// </summary>
        private int SendEmailWithUrl(string emailString, string url, string title, string type, string remark)
        {
            UserModel objuser = ((UserModel)Session["UserData"]);
            string txtFile = Server.MapPath("~/Common/EmailTemplate/PMCReport_invitation.txt");//get location of file
            string body = System.IO.File.ReadAllText(txtFile); //get all file textfile data in string
            OrderedDictionary EmailPlaceholderObject = new OrderedDictionary();
            EmailPlaceholderObject.Add("ActualUrl", url);
            EmailPlaceholderObject.Add("title", title);
            EmailPlaceholderObject.Add("reporttype", type);
            EmailPlaceholderObject.Add("remark", remark);
            EmailPlaceholderObject.Add("mailid", objuser.EmailId);
            EmailPlaceholderObject.Add("Fname", objuser.UserName);

            Email objEmail = new Email();
            string msgBody = objEmail.SetTemplate(body, EmailPlaceholderObject);
            string msgSubject = "Invitation to view PMC Report";
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "PMC Report", "Send Email", UserID, IpAddress, "url" + url);
            return objEmail.SendMail(emailString, null, null, null, msgSubject, msgBody, ConfigurationManager.AppSettings["SUPPORTFROM"]);
        }
        #endregion

    }
}