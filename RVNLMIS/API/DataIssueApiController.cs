using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace RVNLMIS.API
{
    public class DataIssueApiController : ApiController
    {
        public HttpResponseMessage DataIssueList(int packageId)
        {
            DataIssueReportWrapper objResult = new DataIssueReportWrapper();
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    if (db.tblDataIssues.Any(a => a.StatusId != 4 && a.PackageId==packageId))
                    {



                        var groups = db.tblDataIssues.Where(a => a.PackageId == packageId).GroupBy(lr => lr.StatusId).Select(group => new { Id = group.Key, Count = group.Count() }).OrderBy(o => o.Id).ToList();

                        foreach (var group in groups)
                        {
                            switch (group.Id)
                            {
                                case 1:
                                    objResult.objModel.NewTicket = group.Count;
                                    break;
                                case 2:
                                    objResult.objModel.SubmitedForReview = group.Count;
                                    break;
                                case 3:
                                    objResult.objModel.ReOpened = group.Count;
                                    break;
                                default:
                                    objResult.objModel.Closed = group.Count;
                                    break;
                            }
                        }
                        //obj.objModel.ReportMsg = MvcHtmlString.Create( "There are total " + (obj.objModel.NewTicket + obj.objModel.SubmitedForReview + obj.objModel.ReOpened).ToString() + " active Tickets.");
                        var assignIssues = (from a in db.tblDataIssues
                                            join b in db.tblDataIssueStatus on a.StatusId equals b.StatusId
                                            join c in db.tblPackages on a.PackageId equals c.PackageId
                                            where a.PackageId == packageId && a.StatusId != 4
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

                        objResult.objList = assignIssues;
                        return ControllerContext.Request
                       .CreateResponse(HttpStatusCode.OK, new { objResult });
                    }
                    return ControllerContext.Request
                       .CreateResponse(HttpStatusCode.OK, "Data OK");
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, "Error occurred!");
            }
        }
        public HttpResponseMessage DataIssueDetailsById(int issueId)
        {
            try
            {
                DataIssueWrapper objResult = new DataIssueWrapper();

                if (issueId != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var dataRes = (from a in db.tblDataIssues
                                       join b in db.tblDataIssueStatus on a.StatusId equals b.StatusId
                                       join c in db.tblPackages on a.PackageId equals c.PackageId
                                       where a.IssueId == issueId
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

                        objResult.objModel = dataRes;

                        var logList = (from a in db.tblDataIssueStatusLogs
                                       join b in db.tblUserMasters on a.UpdatedBy equals b.UserId
                                       join c in db.tblDataIssueStatus on a.StatusId equals c.StatusId
                                       where a.IssueId == issueId
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

                        objResult.objLogList = logList;
                    }
                    return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.OK, new { objResult });
                }
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.BadRequest, "Issue identifier not found");
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, "Error occurred!");
            }
        }

        public HttpResponseMessage SubmitRemark(IssueResponse obj)
        {
            try
            {
                if (!string.IsNullOrEmpty(Convert.ToString(obj.Remark)))
                {
                    if (obj.IssueId != 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var objDataIsue = db.tblDataIssues.Where(o => o.IssueId == obj.IssueId).SingleOrDefault();
                            if (objDataIsue != null)
                            {
                                objDataIsue.StatusId = 2;
                                objDataIsue.Remark = obj.Remark;
                                objDataIsue.ModifiedOn = DateTime.Now;
                                db.SaveChanges();
                                tblDataIssueStatusLog oLog = new tblDataIssueStatusLog();
                                oLog.StatusId = 2;
                                oLog.UpdatedOn = DateTime.Now;
                                oLog.IssueId = obj.IssueId;
                                oLog.Remark = obj.Remark.Trim();
                                oLog.UpdatedBy = obj.UserId;
                                db.tblDataIssueStatusLogs.Add(oLog);
                                db.SaveChanges();
                                return ControllerContext.Request
                                .CreateResponse(HttpStatusCode.OK, "Remark added succesfully");
                            }
                            return ControllerContext.Request
                            .CreateResponse(HttpStatusCode.BadRequest, "No data found to update");
                        }
                    }
                    return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.BadRequest, "Issue identifier not found");
                }
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.BadRequest, "Remark field must be filled");
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, "Error occurred!");
            }
        }

    }

    public class IssueResponse
    {
        public int IssueId { get; set; }
        public int UserId { get; set; }
        public string Remark { get; set; }
    }
}