using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RVNLMIS.Controllers;
using System.Net.Http.Formatting;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Areas.RFI.Controllers;
using System.Web;
using System.Configuration;
using System.Web.Hosting;
using System.IO;
using RVNLMIS.Areas.RFI.Common;
using System.Threading.Tasks;

namespace RVNLMIS.Areas.RFI.API
{
    public class RFIMainApiController : ApiController
    {
        CommonRFIMethodsController _objCommon = new CommonRFIMethodsController();

        #region -----List RFI-------------

        public HttpResponseMessage ReadRFIs(FormDataCollection obj)
        {
            List<RFIMainModel> lstInfo = new List<RFIMainModel>();

            try
            {
                int packageId = Convert.ToInt32(obj.Get("packageId"));
                int userId = Convert.ToInt32(obj.Get("userId"));
                string orgnisation = obj.Get("orgnisation");
                string designName = obj.Get("designName");
                lstInfo = _objCommon._GetRFIListing(packageId, userId, orgnisation, designName);

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lstInfo });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lstInfo });
            }
        }

        #endregion

        #region -------Add/Edit RFI------

        public HttpResponseMessage EditRFI(int rfiId)
        {
            RFIMainModel objModel = new RFIMainModel();

            try
            {
                objModel = _objCommon._GetRFIToEdit(rfiId);
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { objModel });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { objModel });
            }
        }

        [HttpPost]
        public HttpResponseMessage SubmitRFI(RFIMainModel objRFIModel, string userName, int userid)
        {
            string message = string.Empty;
            try
            {
                if (objRFIModel.LocationType == "Chainage")
                {
                    SectionController objSecContr = new SectionController();
                    string msgChainage = objSecContr.ChainageValidation(objRFIModel.StartChainage, objRFIModel.EndChainage, objRFIModel.PackageId);

                    if (msgChainage != "Empty")
                    {
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { msgChainage });
                    }
                }
                Tuple<string, string> _outAddEdit = _objCommon._AddEditRFI(objRFIModel, userName, userid);

                // message = _objCommon._AddEditRFI(objRFIModel, userName, userid);
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { status = _outAddEdit.Item1, message = _outAddEdit.Item2 });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Error." });
            }
        }

        #endregion

        #region -------DELETE RFI-------

        [HttpPost]
        [Route("RFIMainApi/DeleteRFI")]
        public HttpResponseMessage DeleteRFI(int rfiId)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var obj = dbContext.tblRFIMains.Where(w => w.RFIId == rfiId).FirstOrDefault();
                    obj.IsDeleted = true;
                    dbContext.SaveChanges();
                }

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Data deleted successfully." });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Error." });
            }
        }

        #endregion

        #region ------DROPDOWN-----

        [Route("RFIMainApi/GetPackageDrp")]
        public IHttpActionResult GetPackageDrp(int packageId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                DropDownOptionModel obj = dbContext.tblPackages.Where(p => p.PackageId == packageId && p.IsDeleted == false)
                    .Select(s => new DropDownOptionModel
                    {
                        ID = s.PackageId,
                        Name = s.PackageCode + "-" + s.PackageName
                    }).FirstOrDefault();
                return Ok(obj);
            }
        }

        [Route("RFIMainApi/GetEntityDrp")]
        public IHttpActionResult GetEntityDrp(int packageId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<DropDownOptionModel> obj = dbContext.tblMasterEntities.Where(p => p.PackageId == packageId && p.IsDelete == false)
                     .Select(s => new DropDownOptionModel
                     {
                         ID = s.EntityID,
                         Name = s.EntityCode + "-" + s.EntityName
                     }).ToList();
                return Ok(obj);
            }
        }

        [Route("RFIMainApi/GetWorkgroupList")]
        public IHttpActionResult GetWorkgroupList()
        {
            List<DropDownOptionModel> obj = _objCommon._GetWorkgroupList();
            return Ok(obj);
        }

        [Route("RFIMainApi/GetRFICode")]
        public IHttpActionResult GetRFICode(int workGrpId)
        {
            string code = _objCommon.GetRFICode(workGrpId);

            return Ok(code);
        }

        [Route("RFIMainApi/Get_RFIActivity")]
        public List<DropDownOptionModel> Get_RFIActivity(int? wrkgrpId)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            obj = _objCommon._GetActivity(wrkgrpId);
            return obj;
        }

        [Route("RFIMainApi/GetAssignedToPMCNames")]
        public List<DropDownOptionModel> GetAssignedToPMCNames(int? pkgId)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            obj = _objCommon._GetAssignedToPMC(pkgId);
            return obj;
        }

        [Route("RFIMainApi/Get_RFIWorkside")]
        public List<DropDownOptionModel> Get_RFIWorkside()
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            obj = _objCommon._GetRFIWorkside();
            return obj;
        }

        [Route("RFIMainApi/GetEnclosureTypeList")]
        public List<DropDownOptionModel> GetEnclosureTypeList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<DropDownOptionModel> obj = (from w in dbContext.tblEnclosures
                                                 where w.IsDeleted == false
                                                 select new { w })
                              .AsEnumerable()
                              .Select(s => new DropDownOptionModel
                              {
                                  ID = s.w.EnclId,
                                  Name = s.w.EnclName
                              }).ToList();
                return obj;
            }
        }

        [Route("RFIMainApi/GetInspStatusDropdown")]
        public List<DropDownOptionModel> GetInspStatusDropdown()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<DropDownOptionModel> obj = dbContext.tblInspectionStatus.Where(p => p.IsDeleted == false)
                          .Select(s => new DropDownOptionModel
                          {
                              ID = s.InspId,
                              Name = s.StatusType
                          }).ToList();
                return obj;
            }
        }

        #endregion

        #region ----Enclosure-----

        [Route("RFIMainApi/GetExistingEnclosures")]
        public IHttpActionResult GetExistingEnclosures(int rfiid)
        {
            EnclosureAttachModel objModel = new EnclosureAttachModel();
            objModel.RFIId = rfiid;

            if (rfiid != 0)
            {
                objModel.objAttachDetail = _objCommon._GetEnclAttachModel(rfiid);
            }
            return Ok(objModel);
        }

        [HttpPost]
        [Route("RFIMainApi/UploadRFIEnclFile")]
        public HttpResponseMessage UploadRFIEnclFile(int rfiId, int enclId, string userName)
        {
            GenerateRFIController objRFICtr = new GenerateRFIController();
            var request = HttpContext.Current.Request;
            string fileName = string.Empty;
            string root = ConfigurationManager.AppSettings["ServerPath"].ToString();
            int encAutoId = 0;

            if (Request.Content.IsMimeMultipartContent())
            {
                if (request.Files.Count > 0)
                {
                    using (var dbContext = new dbRVNLMISEntities())
                    {
                        string packageCode = objRFICtr.GetPackageCode(rfiId);
                        var postedFile = request.Files.Get("file");
                        var strings = new List<string> { ".pdf", ".xls", ".xlsx", ".doc", ".docx", ".png", ".jpg", ".jpeg" };
                        bool contains = strings.Contains(Path.GetExtension(postedFile.FileName), StringComparer.OrdinalIgnoreCase);

                        if (!contains)
                        {
                            return Request.CreateResponse(HttpStatusCode.Found, new { status = "ext not allowed" });
                        }

                        fileName = string.Concat(rfiId, "-", postedFile.FileName.Replace(' ', '_'));
                        string localPath = string.Format("~/Uploads/RFIEnclosure/{0}/{1}", packageCode, rfiId);
                        Functions.CreateIfMissing(HostingEnvironment.MapPath(localPath));
                        string filePath = string.Format("/Uploads/RFIEnclosure/{0}/{1}/{2}", packageCode, rfiId, fileName);

                        try
                        {
                            postedFile.SaveAs(HostingEnvironment.MapPath(string.Concat(localPath, "/", fileName)));
                            //Save post to DB

                            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                            {
                                tblRFIEnclosure objAdd = new tblRFIEnclosure();
                                objAdd.EnclId = enclId;
                                objAdd.RFIId = rfiId;
                                objAdd.EnclAttachPath = string.Concat(localPath, "/", fileName);
                                objAdd.EnclAttach = postedFile.FileName;

                                db.tblRFIEnclosures.Add(objAdd);
                                db.SaveChanges();

                                encAutoId = objAdd.RFIEnclId;
                            }

                            #region -----ADD TIMELINE ACTIVITY-------

                            //string userName = ((UserModel)Session["RFIUserSession"]).Name;
                            string actText = "Enclosure attachment added by " + userName;
                            RFIFunctions.AddTimelineActivity(rfiId, actText, "bg-c-purple", "Encl", encAutoId.ToString());

                            #endregion

                            return Request.CreateResponse(HttpStatusCode.Found, new
                            {
                                status = "created",
                                path = string.Concat(root, filePath),
                                filename = fileName,
                            });
                        }
                        catch (Exception ex)
                        {
                            Functions.DeleteFilesInFolder(HostingEnvironment.MapPath(filePath), false);
                            return Request.CreateResponse(HttpStatusCode.Found, new { status = "error" });
                        }
                    }
                }
            }
            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { status = "Error while uploading file.", });
        }

        [HttpPost]
        [Route("RFIMainApi/DeleteEnclAttach")]
        public HttpResponseMessage DeleteEnclAttach(int enclId, string userName)
        {
            try
            {
                Tuple<string, string, int> _outEncl = _objCommon._DeleteEncl(enclId, userName);

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Data deleted successfully." });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Error." });
            }
        }

        #endregion

        #region ------DOWNLOAD RFI DOC
        [Route("RFIMainApi/DownloadRFIDoc")]
        public HttpResponseMessage DownloadRFIDoc(int id)
        {
            #region ---- RFI details ----
            RFIDocumentController objCtr = new RFIDocumentController();
            string filePath = string.Empty;
            RFIDocumentModel objModel = new RFIDocumentModel();

            List<AttachDetails> objAttachDetail = new List<AttachDetails>();
            List<CommentList> objCommList = new List<CommentList>();
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                if (id != 0)
                {
                    objModel = db.ViewRFIPDFDetails.Where(r => r.RFIId == id)
                        .Select(s => new RFIDocumentModel
                        {
                            RFIId = s.RFIId,
                            PackageId = s.PkgId,
                            EndChainage = s.EndChainage,
                            StartChainage = s.StartChainge,
                            EntityId = s.Entity,
                            LayerNo = s.LayerNo,
                            LocationType = s.LocationType,
                            OtherWorkLocation = s.OtherWorkLocation,
                            RFIActivityId = s.RFIActivityId,
                            RFICode = s.RFICode,
                            RFIStatus = s.RFIStatus,
                            WorkgroupId = s.WorkGroupId,
                            WorkSide = s.WorkSide,
                            AssignToPMC = s.AssignedTo,
                            RFIOpenDate = s.RFIOpenDate,
                            InspDate = s.InspDate,
                            AssignToPMCName = s.AssignedToName,
                            InspStatus = s.InspStatus,
                            InspId = s.InspId,
                            ProjectName = s.Description,// Convert.ToString( s.ProjectName)+"- "+ Convert.ToString(s.Description),
                            Contractor = s.Contractor,
                            ActivityName = s.RFIActName,
                            BillNo = "",
                            BOQItemNo = s.BOQCode,
                            ClientName = s.Client,
                            DateOfSubmission = (DateTime)s.DateOfSubmission,
                            EntityName = s.EntityName,
                            Layer = ((int)s.LayerNo).ToString(),
                            PackageName = s.PackageName,
                            PMC = s.PMC,
                            PrevRFICode = s.PrevRFICode,
                            RFINo = s.RFICode,
                            WorkDescription = s.WorkDescription,
                            Workgroup = s.WorkGrName,
                            Requestedby = s.Requestedby,
                            Revision = s.Revision,
                            RFIRevId = s.RFIRevId,
                            AcceptedDate = (DateTime)s.AcceptedDate

                        })
                        .FirstOrDefault();

                    if (objModel.Revision != "0" && objModel.Revision != string.Empty && objModel.Revision != null)
                    {
                        try
                        {
                            objCommList = (from a in db.tblRFIRevisions
                                           join b in db.tblRFIUsers on a.AssignedTo equals b.RFIUserId
                                           join c in db.tblRFIDesignations on b.DesignationId equals c.RFIDesignId
                                           where a.RFIId == objModel.RFIId //&& a.RFIRevId!=objModel.RFIRevId
                                           select new { a, b, c })
                                     .AsEnumerable()
                                     .Select(o =>
                                     new CommentList
                                     {
                                         Status = o.a.InspStatus,
                                         Comment = o.a.Note == null ? "" : o.a.Note,
                                         CommentedBy = o.b.FullName,
                                         Designation = o.c.Designation
                                        ,
                                         CommentDate = o.a.InspDate == null ? "" : ((DateTime)o.a.InspDate).ToString("dd-MMM-yyyy hh:mm tt")
                                     }).ToList();
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                    objAttachDetail = db.tblRFIEnclosures.Join(db.tblEnclosures, re => re.EnclId, e => e.EnclId, (re, e)
                           => new { re, e }).Where(r => r.re.RFIId == id).AsEnumerable()
                     .Select(s => new AttachDetails
                     {
                         RFIEnclId = s.re.RFIEnclId,
                         EnclType = s.e.EnclName + " (" + s.re.EnclAttach.Split('.')[0] + ")",
                         FileName = s.re.EnclAttach,
                         Path = s.re.EnclAttachPath
                     }).ToList();

                }
            }

            if (objModel != null)
            {
                objModel.WorkLocation = objModel.LocationType == "Other" ?
                                                objModel.OtherWorkLocation :
                                                objModel.LocationType == "Entity" ?
                                                objModel.EntityName :
                                                (objModel.StartChainage == null || objModel.EndChainage == null ? "" : "From " + IntToStringChainage(Convert.ToInt32((double)objModel.StartChainage)) + " To " + IntToStringChainage(Convert.ToInt32((double)objModel.EndChainage)));
                string fileName = objModel.RFICode != string.Empty ? objModel.RFICode.Replace("/", "-").Replace("--", "-") + ".pdf" : DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                #endregion

                // DownloadPDF(objModel, objAttachDetail, objCommList, fileName);
                filePath = objCtr.SaveRfiPDF(objModel, objAttachDetail, objCommList, fileName);

            }
            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { path = filePath });
        }

        public string IntToStringChainage(int chainage)
        {
            return chainage.ToString("D6").Insert(chainage.ToString("D6").Length - 3, "+");
        }
        #endregion

        #region -------Inspection Process related Apis------

        [Route("RFIMainApi/GetRFIViewModelData")]
        public HttpResponseMessage GetRFIViewModelData(int rfiId)
        {
            try
            {
                RFIInspStatusUpdateModel objModel = _objCommon._ViewRFIInspStatus(rfiId);
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { objModel });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Error." });
            }
        }

        [Route("RFIMainApi/UpdateRFIStatus")]
        public HttpResponseMessage UpdateRFIStatus(RFIInspStatusUpdateModel objModel, int loginUserId, string userName, int pkgId)
        {
            GenerateRFIController objRfiCtr = new GenerateRFIController();
            string picWithComma = string.Empty;

            try
            {
                Tuple<string, string> _outInspProcess = _objCommon._RFIStatusUpdateProcess(objModel, loginUserId, userName, pkgId);
                UpdateRFIPicRemarkAndStaus(objModel.Pic1Id, objModel.Remark1, objModel.InspStatus);
                UpdateRFIPicRemarkAndStaus(objModel.Pic2Id, objModel.Remark2, objModel.InspStatus);
                UpdateRFIPicRemarkAndStaus(objModel.Pic3Id, objModel.Remark3, objModel.InspStatus);

                if (objModel.Pic1Id != 0 || objModel.Pic2Id != 0 || objModel.Pic3Id != 0)
                {
                    objRfiCtr.AddPicturesInTimeline(objModel, userName, objModel.Pic1Id, objModel.Pic2Id, objModel.Pic3Id, picWithComma);
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = _outInspProcess.Item1, chainageError = _outInspProcess.Item2 });
            }
            catch (Exception ex)
            {
                DeletePictures(objModel.Pic1Id);
                DeletePictures(objModel.Pic2Id);
                DeletePictures(objModel.Pic3Id);
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Error." });
            }
        }

        public void UpdateRFIPicRemarkAndStaus(int picId, string remark, string inspStatus)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var getPicObj = dbContext.tblRFIPictures.Where(p => p.RFIPicId == picId).FirstOrDefault();
                if (getPicObj != null)
                {
                    getPicObj.Remarks = remark;
                    getPicObj.InspStatus = inspStatus;
                    dbContext.SaveChanges();
                }
            }
        }

        private void DeletePictures(int picId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var getPicObj = dbContext.tblRFIPictures.Where(p => p.RFIPicId == picId).FirstOrDefault();
                if (getPicObj != null)
                {
                    dbContext.tblRFIPictures.Remove(getPicObj);
                    dbContext.SaveChanges();
                }
            }
        }

        //[Route("RFIMainApi/UploadPictures")]
        //[HttpPost]
        //public async Task<IHttpActionResult> UploadPictures()
        //{
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        return StatusCode(HttpStatusCode.UnsupportedMediaType);
        //    }
        //    var filesReadToProvider = await Request.Content.ReadAsMultipartAsync();

        //    //We will use two content part one is used to store the json another is used to store the image binary.
        //    var json = await filesReadToProvider.Contents[0].ReadAsStringAsync();
        //    var fileBytes = await filesReadToProvider.Contents[0].ReadAsByteArrayAsync();
        //    return Ok();
        //}

        [Route("RFIMainApi/UploadPictures")]
        [HttpPost]
        public HttpResponseMessage UploadPictures(int rfiId, int revisionId, int userId)
        {
            GenerateRFIController objRFICtr = new GenerateRFIController();
            var request = HttpContext.Current.Request;
            string fileName = string.Empty;
            string root = ConfigurationManager.AppSettings["ServerPath"].ToString();
            int rfiPicId = 0;

            if (Request.Content.IsMimeMultipartContent())
            {
                if (request.Files.Count > 0)
                {
                    using (var dbContext = new dbRVNLMISEntities())
                    {
                        var postedFile = request.Files.Get("file");
                        var strings = new List<string> { ".png", ".jpg", ".jpeg" };
                        bool contains = strings.Contains(Path.GetExtension(postedFile.FileName), StringComparer.OrdinalIgnoreCase);

                        if (!contains)
                        {
                            return Request.CreateResponse(HttpStatusCode.Found, new { status = "ext not allowed" });
                        }

                        fileName = string.Concat(revisionId + "-" + postedFile.FileName.Replace(' ', '_'));
                        string filePath = string.Format("/Uploads/RFIPictures/{0}/{1}/{2}", rfiId, revisionId, fileName);

                        string localPath = string.Format("~/Uploads/RFIPictures/{0}/{1}", rfiId, revisionId);
                        Functions.CreateIfMissing(HostingEnvironment.MapPath(localPath));

                        try
                        {
                            postedFile.SaveAs(HostingEnvironment.MapPath(string.Concat(localPath, "/", fileName)));
                            //Save post to DB

                            #region -----SAVE PICTURE TO DB--------

                            using (dbRVNLMISEntities dbcontext = new dbRVNLMISEntities())
                            {
                                tblRFIPicture objAddNew = new tblRFIPicture();
                                objAddNew.RFIRevId = revisionId;
                                objAddNew.Picture = postedFile.FileName;
                                // objAddNew.Remarks = remark;
                                objAddNew.PicPath = string.Concat(localPath, "/", fileName);
                                // objAddNew.InspStatus = status;
                                objAddNew.AddedOn = DateTime.Now;
                                objAddNew.AddedLocation = "Web";
                                objAddNew.AddedById = userId;

                                dbcontext.tblRFIPictures.Add(objAddNew);
                                dbcontext.SaveChanges();
                                rfiPicId = objAddNew.RFIPicId;
                            }

                            #endregion

                            return Request.CreateResponse(HttpStatusCode.Found, new
                            {
                                status = "created",
                                path = string.Concat(root, filePath),
                                rfiPictureId = rfiPicId
                            });
                        }
                        catch (Exception ex)
                        {
                            Functions.DeleteFilesInFolder(HostingEnvironment.MapPath(filePath), false);
                            return Request.CreateResponse(HttpStatusCode.Found, new { status = "error" });
                        }
                    }
                }
            }
            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { status = "Error while uploading file.", });
        }
        #endregion

        #region --------SHOW RFI TIMELINE-----------

        [Route("RFIMainApi/GetRFITimelineList")]
        public IHttpActionResult GetRFITimelineList(int rfiId)
        {
            RFITimeLineModel objModel = new RFITimeLineModel();

            try
            {
                objModel = _objCommon._TimelineList(rfiId);
                return Ok(objModel);
            }
            catch (Exception ex)
            {
                return Ok("Error");
            }
        }

        #endregion

        #region ------Reschedule RFI------

        [Route("RFIMainApi/RFIReschedule")]
        public HttpResponseMessage RFIReschedule(FormDataCollection obj)
        {
            DateTime NewInspDate = Convert.ToDateTime(obj.Get("NewInspDate"));
            int RFIId = Convert.ToInt32(obj.Get("RFIId"));
            string userName = obj.Get("userName");
            int userId = Convert.ToInt32(obj.Get("userId"));
            string assignTo = obj.Get("assignedToId");

            try
            {
                _objCommon._CommonRescheduleRFI(NewInspDate, RFIId, userId, userName, assignTo);

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "1" });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "2" });
            }
        }

        #endregion

        #region ----------FILTER RFI LIST------

        [Route("RFIMainApi/FilterRFI")]
        public HttpResponseMessage FilterRFI(FormDataCollection obj)
        {
            List<RFIMainModel> lst = new List<RFIMainModel>();
            string[] statusArray = obj.Get("strStatus").Split(',');
            int packageId = Convert.ToInt32(obj.Get("packageId"));
            int userId = Convert.ToInt32(obj.Get("userId"));
            string orgnisation = obj.Get("orgnisation");
            string designName = obj.Get("designName");

            try
            {
                statusArray = statusArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                lst = statusArray.Count() == 0 ? _objCommon._GetRFIListing(packageId, userId, orgnisation, designName)
                   : _objCommon._GetRFIListing(packageId, userId, orgnisation, designName).Where(x => statusArray.Contains(x.InspStatus)).ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lst });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Error" });
            }
        }

        #endregion

        #region -----Approved by RVNL -----

        [Route("RFIMainApi/ApproveByRVNL")]
        public HttpResponseMessage ApproveByRVNL(FormDataCollection obj)
        {
            int rFIId = Convert.ToInt32(obj.Get("RFIId"));
            string userName = obj.Get("userName");

            try
            {
                _objCommon._CommonRVNLApproveRFI(rFIId, userName);

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "1" });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "2" });
            }
        }


        #endregion

        #region ------RFI Notification -----

        [Route("RFIMainApi/GetNotificationList")]
        public HttpResponseMessage GetNotificationList(int userId)
        {
            List<PushNotifyModel> notifyObj = new List<PushNotifyModel>();

            try
            {
                notifyObj = _objCommon._NotifyCommonList(userId);

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { notifyObj });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { notifyObj });
            }
        }

        [Route("RFIMainApi/NotificationLog")]
        public HttpResponseMessage NotificationLog(int userId)
        {
            List<PushNotifyModel> notifyObj = new List<PushNotifyModel>();
            NotificationLogController objCtr = new NotificationLogController();

            try
            {
                notifyObj = objCtr.Read_Notification(userId).OrderByDescending(o => o.SentOn).ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { notifyObj });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { notifyObj });
            }
        }

        [Route("RFIMainApi/ReadAllNotification")]
        public HttpResponseMessage ReadAllNotification(int userId)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var notObj = dbContext.tblNotificationReadStatus.Where(w => w.ReceiverId == userId).ToList();
                    foreach (var item in notObj)
                    {
                        item.IsRead = true;
                        item.ReadOn = DateTime.Now;
                    }
                    dbContext.SaveChanges();
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "success" });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "error" });
            }
        }

        [Route("RFIMainApi/MarkNotificationAsRead")]
        public HttpResponseMessage MarkNotificationAsRead(int userId, int notiId)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var notObj = dbContext.tblNotificationReadStatus.Where(w => w.NotificationId == notiId && w.ReceiverId == userId).FirstOrDefault();
                    notObj.IsRead = true;
                    notObj.ReadOn = DateTime.Now;
                    dbContext.SaveChanges();
                }

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "success" });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "error" });
            }
        }

        #endregion
    }
}
