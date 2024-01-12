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
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    //[Compress]
    [SessionAuthorize]
    public class DashboardReportsController : Controller
    {
        private static TimeZoneInfo INDIANTIME = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        #region ---  Show Power bi Report ---

        //[PageAccessFilter]
        public async Task<ActionResult> Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "UnderConstruction");
            }

            ReportEmbeddingData embeddingData;
            string menuId = HttpUtility.UrlDecode(id);

            menuId = id.Split('=')[1];

            string tableDataName = ((UserModel)Session["UserData"]).TableDataName;
            string roleCode = ((UserModel)Session["UserData"]).RoleCode;
            Hashtable values = new Hashtable();
            values = RowLevelSecurity.getUsernameAndRole(tableDataName, roleCode);

            //   ReportEmbeddingData dds = await PbiEmbeddedManager.ExportReport(menuId, values["Username"].ToString(), values["Role"].ToString()); // This is temparary commited


            embeddingData = await PbiEmbeddedManager.GetReportEmbeddingDataWithSecurity(menuId, values["Username"].ToString(), values["Role"].ToString()); // This is temparary commited



            // throw new Exception("Something went wrong!");
            return View(embeddingData);
        }


        #endregion

        #region --- Crud Operation for Power bi report Data ---

        [PageAccessFilter]
        public ActionResult ListPowerbiReport()
        {
            BindDropdown();

            return View();
        }

        private void BindDropdown()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var menuList = (from menu in dbContext.tblAppMenus
                                where !dbContext.tblPowerBIReports.Any(f => f.MenuId == menu.MenuId)
                                && menu.IsDeleted == false
                                select new { menu.MenuId, menu.MenuName }).ToList();

                ViewBag.MenuList = new SelectList(menuList, "MenuId", "MenuName");


                var groupList = (from Group in dbContext.tblMasterGroups
                                 where Group.IsDeleted == false
                                 select new { Group.GroupId, Group.GroupName }).ToList();

                ViewBag.groupList = new SelectList(groupList, "GroupId", "GroupName");
            }
        }

        public JsonResult Get_Workspace(string TenantId, string AppId, string AppSecret)
        {
            string accessToken = PbiEmbeddedManager.GetAccessToken(TenantId, AppId, AppSecret);
            List<ReportOptionModel> groups = new List<ReportOptionModel>();

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Authorization", String.Format("Bearer {0}", accessToken));

                try
                {
                    string json = webClient.DownloadString("https://api.powerbi.com/v1.0/myorg/groups");

                    dynamic response = JsonConvert.DeserializeObject(json);

                    foreach (var group in response.value)
                    {
                        ReportOptionModel obj = new ReportOptionModel();
                        obj.ID = group.id;
                        obj.Name = group.name;
                        //groups.Add(dataset.name.ToString(), dataset.id.ToString());
                        groups.Add(obj);
                    }

                }
                catch (Exception e)
                { // Log as you want }
                }
            }
            return Json(groups, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Get_ReportInGroup(string tenentID, string workSpaceId, string AppId, string AppSecret)
        {
            string accessToken = PbiEmbeddedManager.GetAccessToken(tenentID, AppId, AppSecret);
            List<ReportOptionModel> reports = new List<ReportOptionModel>();

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Authorization", String.Format("Bearer {0}", accessToken));

                try
                {
                    string json = webClient.DownloadString(string.Format("https://api.powerbi.com/v1.0/myorg/groups/{0}/reports", workSpaceId));

                    dynamic response = JsonConvert.DeserializeObject(json);

                    foreach (var report in response.value)
                    {
                        ReportOptionModel obj = new ReportOptionModel();
                        obj.ID = report.id;
                        obj.Name = report.name;
                        //groups.Add(dataset.name.ToString(), dataset.id.ToString());
                        reports.Add(obj);
                    }

                }
                catch (Exception e)
                { // Log as you want }
                }
            }
            return Json(reports, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Get_Dataset(string tenentID, string workSpaceId, string AppId, string AppSecret)
        {
            string accessToken = PbiEmbeddedManager.GetAccessToken(tenentID, AppId, AppSecret);
            List<ReportOptionModel> datasets = new List<ReportOptionModel>();

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Authorization", String.Format("Bearer {0}", accessToken));

                try
                {
                    string json = webClient.DownloadString(string.Format("https://api.powerbi.com/v1.0/myorg/groups/{0}/datasets", workSpaceId));

                    dynamic response = JsonConvert.DeserializeObject(json);

                    foreach (var dataset in response.value)
                    {
                        ReportOptionModel obj = new ReportOptionModel();
                        obj.ID = dataset.id;
                        obj.Name = dataset.name;
                        //groups.Add(dataset.name.ToString(), dataset.id.ToString());
                        datasets.Add(obj);
                    }
                }
                catch (Exception e)
                { // Log as you want }
                }
            }
            return Json(datasets, JsonRequestBehavior.AllowGet);
        }

        #region --- List Table Values ---

        public ActionResult PowerbiReport_Read([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var lstReport = (from r in dbContext.tblPowerBIReports
                                 join m in dbContext.tblAppMenus
                                 on r.MenuId equals m.MenuId
                                 where r.isDeleted == false
                                 select new PowerbiReportData
                                 {
                                     Id = r.Id,
                                     ReportId = r.ReportId,
                                     ReportName = r.ReportName,
                                     ReportImage = r.ReportImage,
                                     MenuName = m.MenuName,
                                     GroupName = dbContext.tblMasterGroups.Where(o => o.GroupId == r.GroupId && o.IsDeleted == false).Select(o => o.GroupName).FirstOrDefault(),
                                     WorkSpaceId = r.WorkSpaceId,
                                     ApplicationSecret = r.AppSecret,
                                     ApplicationId = r.ApplicationId,
                                     DatasetId = r.DatasetId,
                                     TenantId = r.TenantId,
                                     URL = r.URL,
                                     Description = r.Description
                                 }).ToList();

                return Json(lstReport.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region --- Add and Edit form ---

        public ActionResult AddEditPowerbiReportInfo(int id)
        {

            PowerbiReportData objModelView = new PowerbiReportData();
            objModelView.ApplicationId = ConfigurationManager.AppSettings["application-id"];
            objModelView.ApplicationSecret = ConfigurationManager.AppSettings["application-secret"];
            objModelView.TenantId = ConfigurationManager.AppSettings["tenant-id"];

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                BindDropdown();

                if (id != 0)   //Edit the form
                {
                    tblPowerBIReport objEdit = dbContext.tblPowerBIReports.Where(p => p.Id == id).FirstOrDefault();
                    if (objEdit != null)
                    {
                        objModelView.Id = objEdit.Id;
                        objModelView.ReportId = objEdit.ReportId;
                        objModelView.ReportName = objEdit.ReportName;
                        objModelView.WorkSpaceId = objEdit.WorkSpaceId;
                        objModelView.ApplicationId = objEdit.ApplicationId;
                        objModelView.ApplicationSecret = objEdit.AppSecret;
                        objModelView.TenantId = objEdit.TenantId;
                        objModelView.ReportImage = objEdit.ReportImage;
                        objModelView.Description = objEdit.Description;
                        objModelView.isDashboard = objEdit.isDashboard;
                        objModelView.URL = objEdit.URL;
                        objModelView.MenuId = objEdit.MenuId;
                        objModelView.GroupId = objEdit.GroupId;
                        objModelView.DatasetId = objEdit.DatasetId;
                    }

                    var menuList = dbContext.tblAppMenus.Where(m => m.IsDeleted == false).ToList();
                    ViewBag.MenuList = new SelectList(menuList, "MenuId", "MenuName");



                    var groupList = (from Group in dbContext.tblMasterGroups
                                     where Group.IsDeleted == false
                                     select new { Group.GroupId, Group.GroupName }).ToList();

                    ViewBag.groupList = new SelectList(groupList, "GroupId", "GroupName");
                }
            }
            return View("_PartialCreateEdit", objModelView);
        }

        [HttpPost]
        public ActionResult AddEditPowerbiReportInfo(PowerbiReportData objModel)
        {
            try
            {
                string filePath = string.Empty;
                string fileName = string.Empty;
                string message = string.Empty;

                if (!ModelState.IsValid)
                {
                    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                    {
                        var menuList = dbContext.tblAppMenus.Where(m => m.IsDeleted == false).ToList();
                        ViewBag.MenuList = new SelectList(menuList, "MenuId", "MenuName");

                        var groupList = (from Group in dbContext.tblMasterGroups
                                         where Group.IsDeleted == false
                                         select new { Group.GroupId, Group.GroupName }).ToList();

                        ViewBag.groupList = new SelectList(groupList, "GroupId", "GroupName");
                    }
                    return View("_PartialCreateEdit", objModel);
                }

                if (objModel.Image != null)
                {
                    fileName = string.Concat(Path.GetFileNameWithoutExtension(objModel.Image.FileName), DateTime.Now.ToString("yyyyMMddHHmmssfff"), Path.GetExtension(objModel.Image.FileName));

                    string localPath = "~/Uploads/PowerBiReportImages/";
                    Functions.CreateIfMissing(Server.MapPath(localPath));
                    filePath = localPath + "/" + fileName;
                    objModel.Image.SaveAs(Server.MapPath(filePath));
                }

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var isNameEnteredExist = dbContext.tblPowerBIReports.Where(e => e.ReportName == objModel.ReportName && e.isDeleted == false).FirstOrDefault();

                    if (objModel.Id == 0)       //add operation
                    {
                        if (isNameEnteredExist != null)
                        {
                            return Json("0", JsonRequestBehavior.AllowGet);
                        }

                        tblPowerBIReport objAdd = new tblPowerBIReport();
                        objAdd.ReportId = objModel.ReportId;
                        objAdd.ReportName = objModel.ReportName;
                        objAdd.WorkSpaceId = objModel.WorkSpaceId;
                        fileName = objModel.Image == null ? "powerbi_default.png" : fileName;
                        objAdd.ReportImage = fileName;
                        objAdd.Description = objModel.Description;
                        objAdd.isDashboard = objModel.isDashboard;
                        objAdd.ApplicationId = objModel.ApplicationId;
                        objAdd.AppSecret = objModel.ApplicationSecret;
                        objAdd.TenantId = objModel.TenantId;
                        objAdd.DatasetId = objModel.DatasetId;
                        objAdd.isDeleted = false;
                        objAdd.URL = objModel.URL;
                        objAdd.MenuId = objModel.MenuId;
                        objAdd.GroupId = objModel.GroupId;
                        dbContext.tblPowerBIReports.Add(objAdd);
                        message = "Data added successfully.";
                    }
                    else   //edit operation
                    {
                        tblPowerBIReport objEdit = dbContext.tblPowerBIReports.Where(p => p.Id == objModel.Id).FirstOrDefault();

                        if (objEdit.ReportName != objModel.ReportName)
                        {
                            if (isNameEnteredExist != null)
                            {
                                return Json("0", JsonRequestBehavior.AllowGet);
                            }
                        }

                        objEdit.ReportId = objModel.ReportId;
                        objEdit.ReportName = objModel.ReportName;
                        objEdit.WorkSpaceId = objModel.WorkSpaceId;
                        if (objModel.Image != null)
                        {
                            objEdit.ReportImage = fileName;
                        }
                        objEdit.Description = objModel.Description;
                        objEdit.isDashboard = objModel.isDashboard;
                        objEdit.URL = objModel.URL;
                        objEdit.AppSecret = objModel.ApplicationSecret;
                        objEdit.ApplicationId = objModel.ApplicationId;
                        objEdit.TenantId = objModel.TenantId;
                        objEdit.DatasetId = objModel.DatasetId;
                        objEdit.MenuId = objModel.MenuId;
                        objEdit.GroupId = objModel.GroupId;
                        message = "Data updated successfully.";
                    }
                    dbContext.SaveChanges();
                }

                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region -- Delete report data -- 


        // POST: DashboardReports/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var idToDelete = db.tblPowerBIReports.Where(o => o.Id == id).FirstOrDefault();
                    idToDelete.isDeleted = true;
                    db.SaveChanges();
                }
                return Json("1");
            }
            catch
            {
                return Json("-1");
            }
        }

        #endregion



        #endregion

        #region ---- Refresh datasets -----

        public ActionResult AjaxRefreshDataset(string groupId, string datasetId, string applicationSecret, string applicationId, string tenantId)
        {
            RefreshDataset(groupId, datasetId, applicationSecret, applicationId, tenantId);
            return Json("1", JsonRequestBehavior.AllowGet);
        }

        public static void RefreshDataset(string groupId, string datasetId, string applicationSecret, string applicationId, string tenentID)
        {
            string token = PbiEmbeddedManager.GetAccessToken(tenentID, applicationId, applicationSecret);
            HttpWebRequest request = System.Net.HttpWebRequest.CreateHttp(String.Format("https://api.powerbi.com/v1.0/myorg/groups/{0}/datasets/{1}/refreshes", groupId, datasetId));
            //POST web request to create a datasource.
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentLength = 0;

            //Add token to the request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                try
                {
                    var response = (HttpWebResponse)request.GetResponse();

                    // Console.WriteLine("Dataset refresh request {0}", response.StatusCode.ToString());
                }
                catch (WebException e)
                {
                    using (WebResponse response = e.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        //Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            //Console.WriteLine(text);
                        }
                    }
                }
            }


        }

        public ActionResult RefreshAllDatasets()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var obj = dbContext.tblPowerBIReports.Where(t => t.isDeleted == false).ToList();

                foreach (var item in obj)
                {
                    RefreshDataset(item.WorkSpaceId, item.DatasetId, item.AppSecret, item.ApplicationId, item.TenantId);
                }
            }
            return Json("1", JsonRequestBehavior.AllowGet);
        }

        #endregion

        public class ReportOptionModel
        {
            public string ID { get; set; }

            public string Name { get; set; }
        }

        #region ------LAST REFRESH DATE AND TIME--------

        public DateTime Get_DatasetLastRefreshDate(string workSpaceId, string datasetId, string AppId, string AppSecret, string tenantId)
        {
            string token = PbiEmbeddedManager.GetAccessToken(tenantId, AppId, AppSecret);
            DateTime refreshDateTime = new DateTime();

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Authorization", String.Format("Bearer {0}", token));

                try
                {
                    string json = webClient.DownloadString(string.Format("https://api.powerbi.com/v1.0/myorg/groups/{0}/datasets/{1}/refreshes", workSpaceId, datasetId));
                    dynamic response = JsonConvert.DeserializeObject(json);

                    foreach (var dataset in response.value)
                    {
                        if (dataset.status == "Completed")
                        {
                            refreshDateTime = dataset.endTime;
                            break;
                        }
                    }
                }
                catch (Exception e)
                { // Log as you want }
                }
            }
            return refreshDateTime;
        }

        public ActionResult ReloadLastRefreshDate(int menuId)
        {
            try
            {
                string strDate = string.Empty;
                using (var db = new dbRVNLMISEntities())
                {
                    var obj = db.tblPowerBIReports.Where(w => w.MenuId == menuId && w.isDeleted == false).FirstOrDefault();
                    DateTime getDate = Get_DatasetLastRefreshDate(obj.WorkSpaceId, obj.DatasetId, obj.ApplicationId, obj.AppSecret, obj.TenantId);

                    if (getDate != new DateTime())
                    {
                        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(getDate, INDIANTIME);
                        strDate = string.Format("{0:dd - MMM - yyyy hh:mm:ss tt}", indianTime);
                        return Json(strDate, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
            return Json("1", JsonRequestBehavior.AllowGet);
        }


        #endregion
    }
}