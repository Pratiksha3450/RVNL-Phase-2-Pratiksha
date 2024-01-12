using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    //[Compress]
    [SessionAuthorize]
    public class AssignDataIssueController : Controller
    {
        [PageAccessFilter]
        // GET: AssignDataIssue
        public ActionResult Index()
        {
            GetViewData();
            return View();
        }

        /// <summary>
        /// Gets the view data.
        /// </summary>     
        private void GetViewData()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var packages = Functions.GetRoleAccessiblePackageList();
                ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");
                var objstatus = dbContext.tblDataIssueStatus.ToList();
                ViewBag.StatusList = new SelectList(objstatus, "StatusId", "Status");
            }
        }

        [HttpPost]
        public ActionResult SubmitIssue(DataIssueModel obj)
        {
            try
            {
                string newTicket = string.Empty;
                if (!ModelState.IsValid)
                {
                    //GetViewData();
                    return View("Index");
                }
                using (var db = new dbRVNLMISEntities())
                {
                    var objPackage = db.tblPackages.Where(a => a.PackageId == obj.PackageId).SingleOrDefault();
                    if (objPackage != null)
                    {
                        var LastTicket = db.tblDataIssues.Where(a => a.PackageId == obj.PackageId).OrderByDescending(o => o.CreatedOn).FirstOrDefault();
                        if (LastTicket == null)
                        {
                            newTicket = "TKT_" + objPackage.PackageCode + "-1001";
                        }
                        else
                        {
                            newTicket = LastTicket.DataTicket.Split('-')[0] + "-" + (Convert.ToInt32(LastTicket.DataTicket.Split('-').Last()) + 1).ToString();
                        }


                        byte[] uploadedFile = new byte[obj.File.InputStream.Length];
                        obj.File.InputStream.Read(uploadedFile, 0, uploadedFile.Length);
                        System.IO.FileStream file = System.IO.File.Create(Server.MapPath("~/Uploads/DataIssues/" + newTicket + "." + obj.File.FileName.Split('.').Last()));

                        file.Write(uploadedFile, 0, uploadedFile.Length);
                        file.Flush();
                        file.Close();
                        file.Dispose();

                        tblDataIssue tblObj = new tblDataIssue();
                        tblObj.PackageId = obj.PackageId;
                        tblObj.DataTicket = newTicket;
                        tblObj.ShortDescription = obj.ShortDescription;
                        tblObj.Description = obj.Description;
                        tblObj.StatusId = 1;
                        tblObj.Remark = "";
                        tblObj.Attachment = newTicket + "." + obj.File.FileName.Split('.').Last();
                        tblObj.CreatedOn = DateTime.Now;
                        tblObj.ModifiedOn = DateTime.Now;
                        tblObj.CreatedBy = Functions.ParseInteger(Convert.ToString(((UserModel)Session["UserData"]).UserId));
                        db.tblDataIssues.Add(tblObj);
                        db.SaveChanges();
                        db.Entry(tblObj).GetDatabaseValues();
                        tblDataIssueStatusLog objLog = new tblDataIssueStatusLog();
                        objLog.IssueId = tblObj.IssueId;
                        objLog.StatusId = 1;
                        objLog.Remark = "";
                        objLog.UpdatedOn = tblObj.CreatedOn;
                        objLog.UpdatedBy = tblObj.CreatedBy;
                        db.tblDataIssueStatusLogs.Add(objLog);
                        db.SaveChanges();
                    }
                }


                return RedirectToAction("Index", "AssignDataIssue");
            }
            catch (Exception)
            {

                throw;
            }

        }

        public ActionResult GetIssueList([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var dataRes = (from a in db.tblDataIssues
                                   join b in db.tblDataIssueStatus on a.StatusId equals b.StatusId
                                   join c in db.tblPackages on a.PackageId equals c.PackageId
                                   select new
                                   {
                                       a.IssueId,
                                       a.ShortDescription,
                                       a.DataTicket,
                                       a.Description,
                                       a.Remark,
                                       a.CreatedBy,
                                       a.CreatedOn,
                                       a.Attachment,
                                       a.StatusId,
                                       a.PackageId,
                                       b.Status,
                                       c.PackageCode
                                   }).AsEnumerable()
                                   .Select(a => new DataIssueModel
                                   {
                                       IssueId = a.IssueId,
                                       PackageId = (int)a.PackageId,
                                       StatusId = (int)a.StatusId,
                                       Attachment = a.Attachment,
                                       CreatedOn = Convert.ToDateTime(a.CreatedOn),
                                       CreatedBy = (int)a.CreatedBy,
                                       Description = a.Description,
                                       DataTicket = a.DataTicket,
                                       PackageCode = a.PackageCode,
                                       Remark = a.Remark,
                                       ShortDescription = a.ShortDescription,
                                       Status = a.Status
                                   }).OrderBy(a => a.StatusId).ToList();

                    return Json(dataRes.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        #region ---- Partial view ----
        [HttpGet]
        public ActionResult GetDataIssueById(int id)
        {
            try
            {
                GetViewData();
                DataIssueWrapper obj = new DataIssueWrapper();

                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var dataRes = (from a in db.tblDataIssues
                                       join b in db.tblDataIssueStatus on a.StatusId equals b.StatusId
                                       join c in db.tblPackages on a.PackageId equals c.PackageId
                                       where a.IssueId == id
                                       select new
                                       {
                                           a.IssueId,
                                           a.ShortDescription,
                                           a.DataTicket,
                                           a.Description,
                                           a.Remark,
                                           a.CreatedBy,
                                           a.CreatedOn,
                                           a.Attachment,
                                           a.StatusId,
                                           a.PackageId,
                                           b.Status,
                                           c.PackageCode
                                       }).AsEnumerable()
                                  .Select(a => new DataIssueModel
                                  {
                                      IssueId = a.IssueId,
                                      PackageId = (int)a.PackageId,
                                      StatusId = (int)a.StatusId,
                                      Attachment = a.Attachment,
                                      CreatedOn = Convert.ToDateTime(a.CreatedOn),
                                      CreatedBy = (int)a.CreatedBy,
                                      Description = a.Description,
                                      DataTicket = a.DataTicket,
                                      PackageCode = a.PackageCode,
                                      Remark = a.Remark,
                                      ShortDescription = a.ShortDescription,
                                      Status = a.Status
                                  }).SingleOrDefault();

                        obj.objModel = dataRes;

                        var logList = (from a in db.tblDataIssueStatusLogs
                                       join b in db.tblUserMasters on a.UpdatedBy equals b.UserId
                                       join c in db.tblDataIssueStatus on a.StatusId equals c.StatusId
                                       where a.IssueId == id
                                       select new
                                       {
                                           a.IssueId,
                                           a.LogId,
                                           a.Remark,
                                           a.StatusId,
                                           a.UpdatedBy,
                                           a.UpdatedOn,
                                           b.Name,
                                           c.Status
                                       }).AsEnumerable()
                                       .Select(a => new DataIssueStatusLogModel
                                       {
                                           IssueId = (int)a.IssueId,
                                           LogId = a.LogId,
                                           Remark = a.Remark,
                                           StatusId = (int)a.StatusId,
                                           UpdatedBy = (int)a.UpdatedBy,
                                           UpdatedOn = (DateTime)a.UpdatedOn,
                                           Name = a.Name,
                                           Status = a.Status
                                       }).OrderByDescending(a => a.UpdatedOn).ToList();

                        obj.objLogList = logList;
                    }
                }
                return View("_PartialIssueLogView", obj);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [SessionAuthorize]
        [HttpPost]
        public JsonResult UpdateIssueStatus(DataIssueWrapper obj)
        {
            try
            {
                if (!string.IsNullOrEmpty(Convert.ToString(obj.objModel.NewRemark)))
                {
                    if (obj.objModel.IssueId != null && obj.objModel.IssueId != 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var objDataIsue = db.tblDataIssues.Where(o => o.IssueId == obj.objModel.IssueId).SingleOrDefault();
                            if (objDataIsue != null)
                            {

                                objDataIsue.ModifiedOn = DateTime.Now;
                                objDataIsue.StatusId = obj.objModel.StatusId;
                                objDataIsue.Remark = obj.objModel.NewRemark;
                                db.SaveChanges();
                                tblDataIssueStatusLog oLog = new tblDataIssueStatusLog();
                                oLog.StatusId = obj.objModel.StatusId;
                                oLog.UpdatedOn = DateTime.Now;
                                oLog.IssueId = obj.objModel.IssueId;
                                oLog.Remark = obj.objModel.NewRemark.Trim();
                                oLog.UpdatedBy = Functions.ParseInteger(Convert.ToString(((UserModel)Session["UserData"]).UserId));
                                db.tblDataIssueStatusLogs.Add(oLog);
                                db.SaveChanges();

                            }
                            else
                            {

                            }
                        }
                    }
                    return Json(1);
                }
                return Json(2);
            }
            catch (Exception ex)
            {

                return Json(0);
            }

        }
        #endregion
    }
}