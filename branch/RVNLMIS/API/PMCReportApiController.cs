using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;

namespace RVNLMIS.API
{
    public class PMCReportApiController : ApiController
    {
        [Route("PMCReportApi/GetPMCRTypes")]
        public List<drpOptions> GetPMCRTypes()
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var list = (from d in dbContext.tblPMCReportTypes
                                where d.IsDeleted == false
                                select new drpOptions
                                {
                                    Id = d.PRId,
                                    Name = d.PMCReportType
                                }).ToList();
                    return list;
                }
                catch (Exception ex)
                {
                    return new List<drpOptions>();
                }
            }
        }

        [Route("PMCReportApi/PMCReport_Details")]
        public List<PMCReportDetailsModel> PMCReport_Details(FormDataCollection frm)
        {
            int packageId = Convert.ToInt32(frm.Get("packageId"));
            int typeId = Convert.ToInt32(frm.Get("typeId"));
            int userId = Convert.ToInt32(frm.Get("userId"));
            int roleId = Convert.ToInt32(frm.Get("roleId"));

            List<PMCReportDetailsModel> objlist = new List<PMCReportDetailsModel>();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    List<GetRoleAssignedPackageList_Result> pkgs = dbContext.GetRoleAssignedPackageList(userId, roleId).ToList();
                    var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();
                    objlist = (from x in dbContext.PMCReportDetailsViews
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
                    if (objlist != null)
                    {
                        if (packageId == 0 && typeId == 0)
                        {
                            objlist = objlist.Where(w => accessiblePackageList.Contains(w.PackageId)).ToList();
                        }
                        else if (packageId != 0 && typeId == 0)
                        {
                            objlist = objlist.Where(w => w.PackageId == packageId).ToList();
                        }
                        else
                        {
                            objlist = objlist.Where(w => w.PackageId == packageId && w.PRId == typeId).ToList();
                        }
                    }
                    return objlist;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [Route("PMCReportApi/DeletePMCReport")]
        [HttpPost]
        public HttpResponseMessage DeletePMCReport(int id)
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

                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Data deleted successfully." });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpPost]
        [Route("PMCReportApi/UploadPMCReportFile")]
        public HttpResponseMessage UploadPMCReportFile(int packageId, int userId)
        {
            var request = HttpContext.Current.Request;
            string fileName = string.Empty;
            string root = ConfigurationManager.AppSettings["ServerPath"].ToString();
            int attachmentId = 0;

            if (Request.Content.IsMimeMultipartContent())
            {
                if (request.Files.Count > 0)
                {
                    using (var dbContext = new dbRVNLMISEntities())
                    {
                        string packageCode = dbContext.tblPackages.Where(p => p.PackageId == packageId && p.IsDeleted == false).Select(s => s.PackageCode).FirstOrDefault();

                        var postedFile = request.Files.Get("file");
                        string localPath = "~/Uploads/Attachments/PMCRepoting";
                        Functions.CreateIfMissing(HostingEnvironment.MapPath(localPath));

                        fileName = string.Concat(packageCode, "-", postedFile.FileName.Replace(' ', '_'));
                        string filePath = "/Uploads/Attachments/PMCRepoting/" + fileName;

                        postedFile.SaveAs(HostingEnvironment.MapPath(string.Concat(localPath, "/", fileName)));
                        //Save post to DB

                        tblAttachment objAttach = new tblAttachment();

                        objAttach.FileName = fileName;
                        objAttach.Path = filePath;
                        objAttach.Type = "PMC Report";
                        objAttach.CreatedOn = DateTime.Now;
                        objAttach.IsDeleted = false;
                        objAttach.CreatedBy = userId;

                        dbContext.tblAttachments.Add(objAttach);
                        dbContext.SaveChanges();
                        attachmentId = objAttach.AttachmentID;

                        return Request.CreateResponse(HttpStatusCode.Found, new
                        {
                            status = "created",
                            path = root + filePath,
                            filename = fileName,
                            attachmentId = attachmentId
                        });
                    }
                }
            }
            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { status = "Error while uploading file.", });
        }

        [Route("PMCReportApi/AddPMCReportDetails")]
        [HttpPost]
        public HttpResponseMessage AddPMCReportDetails(PMCReportDetailsModel oModel)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    DateTime rpdate = Convert.ToDateTime(oModel.ReportingDates);
                    int allReady = db.tblPMCReportDetails.Where(t => t.Title == oModel.Title && t.ReportingDate == rpdate && t.IsDeleted == false && t.PackageId == oModel.PackageId).Count();
                    if (allReady == 0)
                    {
                        tblPMCReportDetail objmodel = new tblPMCReportDetail();
                        objmodel.PRId = oModel.PRId;
                        //objmodel.PMCReportId = oModel.PMCReportId;
                        objmodel.Title = oModel.Title;
                        objmodel.PackageId = oModel.PackageId;
                        objmodel.Remark = oModel.Remark;
                        objmodel.ReportingDate = Convert.ToDateTime(oModel.ReportingDates);
                        objmodel.AttachmentID = oModel.AttachmentID;
                        objmodel.CreatedOn = DateTime.Now;
                        objmodel.IsDeleted = false;
                        db.tblPMCReportDetails.Add(objmodel);
                        db.SaveChanges();
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Added successfully.", });
                    }
                    else
                    {
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Already exists.", });
                    }
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Error", });
            }

        }
    }
}
