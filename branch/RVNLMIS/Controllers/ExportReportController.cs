using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
//using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [SessionAuthorize]
    public class ExportReportController : Controller
    {
        private static TimeZoneInfo INDIANTIME = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        [EncryptedActionParameter]
        public ActionResult Index(string id)
        {
            int menuid = Convert.ToInt32(id);
            ViewBag.MenuId = menuid;
            return View();
        }

        #region ---- Export Report ----

        public async Task<ActionResult> AjaxExportReport(int id)
        {
            ReportEmbeddingData objReport = new ReportEmbeddingData();

            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                objReport = db.tblPowerBIReports.Where(p => p.MenuId == id && p.isDeleted == false)
                    .Select(s => new ReportEmbeddingData
                    {
                        reportId = s.ReportId,
                        applicationId = s.ApplicationId,
                        applicationSecret = s.AppSecret,
                        DatasetId = s.DatasetId,
                        WorkspaceId = s.WorkSpaceId,
                        reportName = s.ReportName,
                        tenantId=s.TenantId
                    }).FirstOrDefault();
            }

            string token = PbiEmbeddedManager.GetAccessToken(objReport.tenantId, objReport.applicationId, objReport.applicationSecret);

            ExportClass myojb = new ExportClass();
            string apiRequest = string.Format("https://api.powerbi.com/v1.0/myorg/groups/{0}/reports/{1}/ExportTo", objReport.WorkspaceId, objReport.reportId);

            HttpWebRequest request = System.Net.HttpWebRequest.CreateHttp(apiRequest);
            //POST web request to create a datasource.
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json";

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));
            string tableDataName = ((UserModel)Session["UserData"]).TableDataName;
            string roleCode = ((UserModel)Session["UserData"]).RoleCode;
            Hashtable values = new Hashtable();
            values = RowLevelSecurity.getUsernameAndRole(tableDataName, roleCode);

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{\r\n\"format\": \"pdf\",\r\n\"powerBIReportConfiguration\": {\r\n\"identities\": [\r\n{\r\n\"username\": \"885922b0-b473-434c-9a3c-4fb365d25fbb\",\r\n\"roles\": [\r\n\"" + values["Role"].ToString() + "\"\r\n],\r\n\"datasets\": [\r\n\"" + objReport.DatasetId + "\"\r\n]\r\n}\r\n]\r\n}\r\n}";
                //string json = "{\r\n\"format\": \"pdf\"\r\n}";
                streamWriter.Write(json);
            }

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();
                    myojb = (ExportClass)js.Deserialize(objText, typeof(ExportClass));


                    string status = string.Empty;

                    do
                    {
                        await Task.Delay(10000);
                        status = GetExportStatus(myojb.id, objReport.applicationSecret, objReport.applicationId, objReport.WorkspaceId, objReport.reportId,objReport.tenantId);
                    } while (status == "Running");

                    if (status == "Succeeded")
                    {
                        string isFileName = GetActualFile(myojb.id, objReport.reportName, objReport.applicationSecret, objReport.applicationId, objReport.WorkspaceId, objReport.reportId,objReport.tenantId);
                        if (isFileName != "Empty")
                        {
                            return Json(new { code = "success", fileName = isFileName }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (status == "Failed" || status == "Undefined")
                    {
                        ViewBag.Resp = "Error";
                        return Json(new { code = "Error" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Logger.LogErrorToLogFile(text);
                    }
                }
                ViewBag.Resp = "Error";
                return Json(new { code = "Error" }, JsonRequestBehavior.AllowGet);
            }
            ViewBag.Resp = "Error";
            return Json(new { code = "Error" }, JsonRequestBehavior.AllowGet);
        }

        public string GetExportStatus(string exportId, string applicationSecret, string applicationId, string groupId, string reportId,string tenantId)
        {
            string token = PbiEmbeddedManager.GetAccessToken(tenantId, applicationId, applicationSecret);

            ExportClass myojb = new ExportClass();
            string apiRequest = string.Format
                ("https://api.powerbi.com/v1.0/myorg/groups/{0}/reports/{1}/exports/{2}", groupId, reportId, exportId);

            HttpWebRequest request = System.Net.HttpWebRequest.CreateHttp(apiRequest);
            //POST web request to create a datasource.
            request.KeepAlive = true;
            request.Method = "GET";
            request.ContentType = "application/json";

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();
                    myojb = (ExportClass)js.Deserialize(objText, typeof(ExportClass));
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Logger.LogErrorToLogFile(text);
                    }
                }
            }

            return myojb.status;
        }

        public string GetActualFile(string exportId, string reportName, string applicationSecret, string applicationId, string groupId, string reportId,string tenantId)
        {
            string token = PbiEmbeddedManager.GetAccessToken(tenantId, applicationId, applicationSecret);
            var exportRequestUri = string.Format("https://api.powerbi.com/v1.0/myorg/groups/{0}/reports/{1}/exports/{2}/file", groupId, reportId, exportId);

            try
            {
                HttpWebRequest request = System.Net.WebRequest.Create(exportRequestUri) as System.Net.HttpWebRequest;
                request.Method = "GET";
                request.Headers.Add("Authorization", String.Format("Bearer {0}", token));
                request.ContentType = "application/pdf";

                WebResponse response = request.GetResponse();

                using (Stream exportResponse = response.GetResponseStream())
                {
                    string fileName = string.Concat(reportName.Replace(" ", ""), DateTime.Now.ToString("ddMMyyyyhhmmssfff"), ".pdf");
                    string path = HostingEnvironment.MapPath("~/Uploads/TemporaryFiles/" + fileName);//get location of file
                    CopyStream(exportResponse, path);
                    return fileName;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Logger.LogErrorToLogFile(text);
                    }
                }
                return "Empty";
            }
        }

        public static void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }

        public class ExportClass
        {
            public string id { get; set; }
            public string reportId { get; set; }
            public string status { get; set; }
        }

        #endregion

        [HttpGet]
        public ActionResult Download(string file)
        {
            string path = HostingEnvironment.MapPath("~/Uploads/TemporaryFiles/" + file);//get location of file
            byte[] fileByteArray = System.IO.File.ReadAllBytes(path);
            Functions.DeleteFilesInFolder(path, false);
            return File(fileByteArray, "application/pdf", file);
        }
    }
}