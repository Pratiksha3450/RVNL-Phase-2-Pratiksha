using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.Models;
using RVNLMIS.DAC;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.Common;
using System.Configuration;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
   // [SessionAuthorize]
    public class DataIssueReportController : Controller
    {
        // GET: DataIssueReport
        public ActionResult Index()
        {
            DataIssueReportWrapper obj = new DataIssueReportWrapper();
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var objUserM = (UserModel)Session["UserData"];

                    var groups = db.tblDataIssues.Where(a => a.PackageId == objUserM.RoleTableID).GroupBy(lr => lr.StatusId).Select(group => new { Id = group.Key, Count = group.Count() }).OrderBy(o => o.Id).ToList();

                    foreach (var group in groups)
                    {
                        switch (group.Id)
                        {
                            case 1:
                                obj.objModel.NewTicket = group.Count;
                                break;
                            case 2:
                                obj.objModel.SubmitedForReview = group.Count;
                                break;
                            case 3:
                                obj.objModel.ReOpened = group.Count;
                                break;
                            default:
                                obj.objModel.Closed = group.Count;
                                break;
                        }
                    }
                    obj.objModel.ReportMsg = MvcHtmlString.Create(ConfigurationManager.AppSettings["IssueMsg"].ToString().Replace("#CNT#", (obj.objModel.NewTicket + obj.objModel.SubmitedForReview + obj.objModel.ReOpened).ToString()).Replace(@"\n", @"<br />"));


                    var assignIssues = (from a in db.tblDataIssues
                                        join b in db.tblDataIssueStatus on a.StatusId equals b.StatusId
                                        join c in db.tblPackages on a.PackageId equals c.PackageId
                                        where a.PackageId == objUserM.RoleTableID && a.StatusId != 4
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

                    obj.objList = assignIssues;
                    return View(obj);
                }
            }
            catch (Exception ex)
            {
                return View(obj);
            }

        }
        public ActionResult GetDataIssueById(int id)
        {
            try
            {
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
                return View("_PartialIssueLogReportView", obj);

            }
            catch (Exception)
            {

                throw;
            }
        }


        [SessionAuthorize]
        [HttpPost]
        public JsonResult UpdateIssueByUser(DataIssueWrapper obj)
        {
            try
            {
                if (!string.IsNullOrEmpty(Convert.ToString(obj.objModel.NewRemark)))
                {
                    if ( obj.objModel.IssueId != 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var objDataIsue = db.tblDataIssues.Where(o => o.IssueId == obj.objModel.IssueId).SingleOrDefault();
                            if (objDataIsue != null)
                            {
                                if (objDataIsue.StatusId != obj.objModel.StatusId)
                                {
                                    objDataIsue.StatusId = 2;
                                    objDataIsue.Remark = obj.objModel.NewRemark;
                                    objDataIsue.ModifiedOn = DateTime.Now;
                                    db.SaveChanges();
                                    tblDataIssueStatusLog oLog = new tblDataIssueStatusLog();
                                    oLog.StatusId = 2;
                                    oLog.UpdatedOn = DateTime.Now;
                                    oLog.IssueId = obj.objModel.IssueId;
                                    oLog.Remark = obj.objModel.NewRemark.Trim();
                                    oLog.UpdatedBy = Functions.ParseInteger(Convert.ToString(((UserModel)Session["UserData"]).UserId));
                                    db.tblDataIssueStatusLogs.Add(oLog);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    objDataIsue.Remark = obj.objModel.NewRemark;
                                    objDataIsue.ModifiedOn = DateTime.Now;
                                    db.SaveChanges();
                                }
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

    }
}