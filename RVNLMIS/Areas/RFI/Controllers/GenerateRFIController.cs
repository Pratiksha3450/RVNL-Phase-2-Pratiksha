using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using PrimaBiWeb.Common;
using RVNLMIS.Areas.RFI.Common;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.Controllers;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [AreaSessionExpire]
    [HandleError]
    public class GenerateRFIController : Controller
    {
        CommonRFIMethodsController _objCommon = new CommonRFIMethodsController();

        #region ---- Page Load ----

        public ActionResult Index(int? id, string status)
        {
            List<RFIMainModel> objRFIList = new List<RFIMainModel>();

            if (id != null)
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var oUser = dbContext.UserDetailsWithRoles.Where(u => u.UserId == id)
                        .Select(s => new UserModel
                        {
                            RoleTableID = (int)s.RoleTableId,
                            UserId = s.UserId,
                            UserName = s.UserName,
                            RoleId = 0,
                            RoleCode = "PMC",
                            MobileNo = s.MobileNo,
                            EmailId = s.EmailId,
                            DesignationName = "Project Manager"
                        })
                        .FirstOrDefault();

                    if (oUser.UserName == "Admin")
                    {
                        oUser.RoleTableID = 0;
                    }
                    Session["RFIUserSession"] = oUser;
                }
            }

            objRFIList = !string.IsNullOrEmpty(status) ? GetRFIList().Where(w => w.InspStatus == status).ToList() : GetRFIList();
            return View(objRFIList);
        }

        public ActionResult CheckUnReadNotification()
        {
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            List<PushNotifyModel> notifyObj = _objCommon._NotifyCommonList(userId);

            if (notifyObj.Count() != 0)
            {
                string _NotifyListView = RenderRazorViewToString("_PartialNotifyList", notifyObj);
                return Json(new { message = "1", viewHtml = _NotifyListView }, JsonRequestBehavior.AllowGet); //success
            }
            else
            {
                return Json(new { message = "0", viewHtml = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MarkNotiAsRead(int notiId)
        {
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var notObj = dbContext.tblNotificationReadStatus.Where(w => w.NotificationId == notiId && w.ReceiverId == userId).FirstOrDefault();
                notObj.IsRead = true;
                notObj.ReadOn = DateTime.Now;
                dbContext.SaveChanges();
            }
            return Json("1", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ----REFRESH RFI LIST-------

        public ActionResult RefreshRFIList()
        {
            List<RFIMainModel> objRFIListModel = GetRFIList();
            return View("_PartialListRFIWithOrdering", objRFIListModel);
        }
        #endregion

        #region --- LIST RFI DETAILS -----

        public List<RFIMainModel> GetRFIList()
        {
            int packageId = ((UserModel)Session["RFIUserSession"]).RoleTableID;
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            string orgnisation = ((UserModel)Session["RFIUserSession"]).RoleCode;
            string designName = ((UserModel)Session["RFIUserSession"]).DesignationName;
            List<RFIMainModel> lst = _objCommon._GetRFIListing(packageId, userId, orgnisation, designName);
            return lst;
        }

        #endregion

        #region ----ADD EDIT RFI ------

        //[AreaSessionExpire]
        public ActionResult AddEditRFI(int id)
        {
            BindLayerDrp();
            RFIMainModel objModel = _objCommon._GetRFIToEdit(id);
            return View("_PartialAddEditRFIMain", objModel);
        }

        //[AreaSessionExpire]
        public ActionResult SubmitRFI(RFIMainModel objRFIModel)
        {
            int _startC = Functions.RepalceCharacter(objRFIModel.StartChainage);
            int _endC = Functions.RepalceCharacter(objRFIModel.EndChainage);
            string userName = ((UserModel)Session["RFIUserSession"]).UserName;
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;

            string message = string.Empty;

            if (!ModelState.IsValid)
            {
                BindLayerDrp();
                return View("_PartialAddEditRFIMain", objRFIModel);
            }


            Tuple<string, string> _outAddEdit = _objCommon._AddEditRFI(objRFIModel, userName, userId);



            List<RFIMainModel> objModel = GetRFIList();
            string _RfiListView = !string.IsNullOrEmpty(_outAddEdit.Item2) ? _outAddEdit.Item2 : RenderRazorViewToString("_PartialListRFIWithOrdering", objModel);
            return Json(new { message = _outAddEdit.Item1, viewHtml = _RfiListView }, JsonRequestBehavior.AllowGet); //success
            //return Json("1", JsonRequestBehavior.AllowGet);
        }

        public void SMSWhenNewRFI(int? userId, string message)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var userObj = dbContext.tblRFIUsers.Where(u => u.RFIUserId == userId).FirstOrDefault();
                //string message = "New RFI has been submitted and assigned to you.";
                SMSSend.SendSMS(userObj.Mobile, message);
            }
        }

        #endregion

        #region -------Re-Schedule RFI-------

        public JsonResult Get_PMCNUserList(string text)
        {
            int pkgId = ((UserModel)Session["RFIUserSession"]).RoleTableID;
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();

            try
            {
                obj = _objCommon._GetAssignedToPMC(pkgId);

                if (!string.IsNullOrEmpty(text))
                {
                    obj = obj.Where(p => p.Name.ToLower().Contains(text)).ToList();
                }

                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetRFIToReschedule(int id)
        {
            RFIInspStatusUpdateModel objModel = _objCommon._ViewRFIInspStatus(id);
            return View("_PartialRescheduleRFI", objModel);
        }

        [HttpPost]
        public ActionResult SubmitRescheduledInspDate(DateTime NewInspDate, int RFIId, string AssignToPMC)
        {
            string userName = ((UserModel)Session["RFIUserSession"]).UserName;
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;

            try
            {
                _objCommon._CommonRescheduleRFI(NewInspDate, RFIId, userId, userName, AssignToPMC);

                List<RFIMainModel> objRFIListModel = GetRFIList();
                string _RfiCtrListView = RenderRazorViewToString("_PartialListRFIWithOrdering", objRFIListModel);
                return Json(new { message = "1", viewHtml = _RfiCtrListView }, JsonRequestBehavior.AllowGet); //success
            }
            catch (Exception ex)
            {
                return Json(new { message = "2", viewHtml = "" }, JsonRequestBehavior.AllowGet); //success
            }
        }

        #endregion

        #region ----APPROVED BY RVNL-----

        public ActionResult ApproveRFIByRVNL(int id)
        {
            string userName = ((UserModel)Session["RFIUserSession"]).UserName;

            try
            {
                _objCommon._CommonRVNLApproveRFI(id, userName);

                List<RFIMainModel> objRFIListModel = GetRFIList();
                string _RfiCtrListView = RenderRazorViewToString("_PartialListRFIWithOrdering", objRFIListModel);
                return Json(new { message = "1", viewHtml = _RfiCtrListView }, JsonRequestBehavior.AllowGet); //success
            }
            catch (Exception ex)
            {
                return Json(new { message = "2", viewHtml = "" }, JsonRequestBehavior.AllowGet); //success
            }
        }

        #endregion

        #region ------DELETE RFI-------

        [HttpPost]
        public ActionResult DeleteRFI(int id)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var obj = dbContext.tblRFIMains.Where(w => w.RFIId == id).FirstOrDefault();
                    obj.IsDeleted = true;
                    dbContext.SaveChanges();
                }
                List<RFIMainModel> objModel = GetRFIList();
                string _RfiListView = RenderRazorViewToString("_PartialListRFIWithOrdering", objModel);
                return Json(_RfiListView, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region -------SUPPORTIVE METHOD-------

        public ActionResult GenerateRFICode(int workGrpId)
        {
            string code = _objCommon.GetRFICode(workGrpId);
            return Json(code, JsonRequestBehavior.AllowGet);
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult DownloadDoc(string path, string name)
        {
            try
            {
                path = string.Concat(Server.MapPath(path));
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                string fileName = name;
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                return Json("1");
            }
        }
        #endregion

        #region ------Bind Dropdown------

        private void BindLayerDrp()
        {
            var list = new List<SelectListItem>();
            for (var i = 1; i <= 100; i++)
                list.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            list.Insert(0, new SelectListItem()
            {
                Value = "-1",
                Text = "NA"
            });
            list.Insert(1, new SelectListItem()
            {
                Value = "0",
                Text = "Top"
            });
            ViewBag.LayerList = list;
        }

        public JsonResult Get_Packages(string text)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            try
            {
                int pkgId = ((UserModel)Session["RFIUserSession"]).RoleTableID;
                using (var db = new dbRVNLMISEntities())
                {
                    obj = db.tblPackages.Where(p => p.IsDeleted == false)
                          .AsEnumerable()
                          .Select(s => new DropDownOptionModel
                          {
                              ID = s.PackageId,
                              Name = s.PackageCode + "-" + s.PackageName
                          }).ToList();

                    if (pkgId != 0)
                    {
                        obj = obj.Where(w => w.ID == pkgId).ToList();
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p => p.Name.ToLower().Contains(text)).ToList();
                    }
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_Workgroup(string text)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            try
            {
                obj = _objCommon._GetWorkgroupList();

                if (!string.IsNullOrEmpty(text))
                {
                    obj = obj.Where(p => p.Name.ToLower().Contains(text)).ToList();
                }
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_EnclosureType(string text)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    obj = (from w in db.tblEnclosures
                           where w.IsDeleted == false
                           select new { w })
                          .AsEnumerable()
                          .Select(s => new DropDownOptionModel
                          {
                              ID = s.w.EnclId,
                              Name = s.w.EnclName
                          }).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p => p.Name.ToLower().Contains(text)).ToList();
                    }
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_RFIActivity(int? wrkgrpId)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            try
            {
                obj = _objCommon._GetActivity(wrkgrpId);
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_AssignedToPMCNames(int? pkgId)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            try
            {
                obj = _objCommon._GetAssignedToPMC(pkgId);
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_RFIWorkSideNames()
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            try
            {
                obj = _objCommon._GetRFIWorkside();
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_PackageEntities(int? pkgId)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    obj = (from e in db.tblMasterEntities
                           where e.PackageId == pkgId && e.IsDelete == false
                           select new { e })
                          .AsEnumerable()
                          .Select(s => new DropDownOptionModel
                          {
                              ID = s.e.EntityID,
                              Name = s.e.EntityCode + " " + s.e.EntityName
                          }).ToList();
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region ------ ENCLOSSURE ATTACH LIST--------

        //[AreaSessionExpire]
        public ActionResult GetEnclAttachModel(int id)
        {
            EnclosureAttachModel objModel = new EnclosureAttachModel();
            objModel.RFIId = id;

            if (id != 0)
            {
                objModel.objAttachDetail = _objCommon._GetEnclAttachModel(id);
            }
            return View("_PartialEnclouser", objModel);
        }

        #endregion

        #region -----ENCLOSURE ATTACH DELETE-----

        public ActionResult DeleteEnclAttach(int id)
        {
            string userName = ((UserModel)Session["RFIUserSession"]).UserName;

            try
            {
                Tuple<string, string, int> _outEncl = _objCommon._DeleteEncl(id, userName);

                EnclosureAttachModel objModel = new EnclosureAttachModel();
                objModel.RFIId = _outEncl.Item3;
                objModel.objAttachDetail = _objCommon._GetEnclAttachModel(_outEncl.Item3);
                string _EnclView = RenderRazorViewToString("_PartialEnclouser", objModel);

                return Json(new { message = "Data deleted successfully.", viewHtml = _EnclView }, JsonRequestBehavior.AllowGet); //success
            }
            catch (Exception ex)
            {
                return Json(new { message = "Error!", viewHtml = "" }, JsonRequestBehavior.AllowGet); //success
            }
        }

        #endregion

        #region ------ ENCLOSURE ATTACH ADD--------

        public ActionResult SubmitEnclAttach(EnclosureAttachModel objEncl)
        {
            string userName = ((UserModel)Session["RFIUserSession"]).UserName;
            int encAutoId = 0;

            if (objEncl.AttachmentFile != null)
            {
                string packageCode = GetPackageCode(objEncl.RFIId);

                FileInfo fi = new FileInfo(objEncl.AttachmentFile.FileName);
                var strings = new List<string> { ".pdf", ".xls", ".xlsx", ".doc", ".docx", ".png", ".jpg", ".jpeg" };
                bool contains = strings.Contains(fi.Extension, StringComparer.OrdinalIgnoreCase);

                if (!contains)
                {
                    return Json(new { message = "0", viewHtml = "" }, JsonRequestBehavior.AllowGet);
                }

                string localPath = string.Format("~/Uploads/RFIEnclosure/{0}/{1}", packageCode, objEncl.RFIId);
                Functions.CreateIfMissing(Server.MapPath(localPath));

                string getFileName = Path.GetFileName(objEncl.AttachmentFile.FileName);
                string fileName = string.Concat(objEncl.RFIId + "-" + getFileName.Replace(' ', '_'));
                string filePath = string.Format("/Uploads/RFIEnclosure/{0}/{1}/{2}", packageCode, objEncl.RFIId, fileName);

                try
                {
                    objEncl.AttachmentFile.SaveAs(Server.MapPath(filePath));
                    //Save Attachment to DB

                    using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                    {
                        tblRFIEnclosure objAdd = new tblRFIEnclosure();
                        objAdd.EnclId = objEncl.EnclId;
                        objAdd.RFIId = objEncl.RFIId;
                        objAdd.EnclAttachPath = string.Concat(localPath, "/", fileName);
                        objAdd.EnclAttach = getFileName;

                        db.tblRFIEnclosures.Add(objAdd);
                        db.SaveChanges();

                        encAutoId = objAdd.RFIEnclId;
                    }

                    EnclosureAttachModel objModel = new EnclosureAttachModel();
                    objModel.RFIId = objEncl.RFIId;
                    objModel.objAttachDetail = _objCommon._GetEnclAttachModel(objEncl.RFIId);
                    string _EnclView = RenderRazorViewToString("_PartialEnclouser", objModel);

                    #region -----ADD TIMELINE ACTIVITY-------

                    //string userName = ((UserModel)Session["RFIUserSession"]).Name;
                    string actText = "Enclosure attachment added by " + userName;
                    RFIFunctions.AddTimelineActivity(objEncl.RFIId, actText, "bg-c-purple", "Encl", encAutoId.ToString());

                    #endregion

                    return Json(new { message = "1", viewHtml = _EnclView }, JsonRequestBehavior.AllowGet); //success
                }
                catch (Exception ex)
                {
                    Functions.DeleteFilesInFolder(Server.MapPath(filePath), false);
                    return Json(new { message = "error", viewHtml = "" }, JsonRequestBehavior.AllowGet);

                }
            }
            return Json(new { message = "2", viewHtml = "" }, JsonRequestBehavior.AllowGet); //please select a file
        }

        public string GetPackageCode(int rFIId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var pkgInfo = (from p in dbContext.tblPackages
                               join r in dbContext.tblRFIMains
                               on p.PackageId equals r.PkgId
                               where r.RFIId == rFIId
                               select p.PackageCode).FirstOrDefault();

                return pkgInfo;
            }

        }

        #endregion

        #region -------RFI INSPECTION PROCESS------

        #region ------BIND DROPDOWN--------

        public JsonResult Get_InspectionStatus(string text)
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    obj = db.tblInspectionStatus.Where(p => p.IsDeleted == false)
                          .Select(s => new DropDownOptionModel
                          {
                              ID = s.InspId,
                              Name = s.StatusType
                          }).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p =>
                      CultureInfo.CurrentCulture.CompareInfo.IndexOf
                      (p.Name, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_CanBeInspector(string text)
        {
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            int pkgId = ((UserModel)Session["RFIUserSession"]).RoleTableID;
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    obj = (from a in db.tblRFIUsers
                           where a.PackgeId == pkgId && a.Organisation == "PMC"
                           select new { a })
                          .AsEnumerable()
                          .Select(s => new DropDownOptionModel
                          {
                              ID = s.a.RFIUserId,
                              Name = s.a.FullName
                          }).ToList();
                    obj.Remove(obj.Where(w => w.ID == userId).FirstOrDefault());

                    obj.Insert(0, new DropDownOptionModel { ID = userId, Name = "Me" });

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p =>
                      CultureInfo.CurrentCulture.CompareInfo.IndexOf
                      (p.Name, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        public ActionResult GetRFIWithStatus(int rfiId)
        {
            RFIInspStatusUpdateModel objModel = _objCommon._ViewRFIInspStatus(rfiId);
            return View("_PartialUpdateInspStatus", objModel);
        }

        public ActionResult UpdateRFIDateAndStatus(RFIInspStatusUpdateModel objModel)
        {
            int loginUserId = Convert.ToInt32(((UserModel)Session["RFIUserSession"]).UserId);
            objModel.UserOrgnisation = Convert.ToString(((UserModel)Session["RFIUserSession"]).RoleCode);
            string userName = ((UserModel)Session["RFIUserSession"]).UserName;
            int pkgId = ((UserModel)Session["RFIUserSession"]).RoleTableID;

            Tuple<string, string> _outInspProcess = _objCommon._RFIStatusUpdateProcess(objModel, loginUserId, userName, pkgId);

            if (string.IsNullOrEmpty(_outInspProcess.Item2))
            {
                #region ------SUBMIT PICTURES-------
                int picId1 = 0, picId2 = 0, picId3 = 0;
                string picWithComma = string.Empty;

                if (objModel.Picture1 != null)
                {
                    picId1 = AddPictureToDBAndFolder(objModel.Picture1, objModel.Remark1, objModel.RevisionId, objModel.RFIId, objModel.InspStatus);
                }
                if (objModel.Picture2 != null)
                {
                    picId2 = AddPictureToDBAndFolder(objModel.Picture2, objModel.Remark2, objModel.RevisionId, objModel.RFIId, objModel.InspStatus);
                }
                if (objModel.Picture3 != null)
                {
                    picId3 = AddPictureToDBAndFolder(objModel.Picture3, objModel.Remark3, objModel.RevisionId, objModel.RFIId, objModel.InspStatus);
                }

                #endregion

                if (objModel.Picture1 != null || objModel.Picture2 != null || objModel.Picture3 != null)
                {
                    picWithComma = AddPicturesInTimeline(objModel, userName, picId1, picId2, picId3, picWithComma);
                }
            }

            List<RFIMainModel> objListModel = GetRFIList();
            string _RfiListView = string.IsNullOrEmpty(_outInspProcess.Item2) ? RenderRazorViewToString("_PartialListRFIWithOrdering", objListModel) : _outInspProcess.Item2;

            return Json(new { message = _outInspProcess.Item1, viewHtml = _RfiListView }, JsonRequestBehavior.AllowGet); //success
        }

        public string AddPicturesInTimeline(RFIInspStatusUpdateModel objModel, string userName, int picId1, int picId2, int picId3, string picWithComma)
        {
            if (picId1 != 0)
            {
                picWithComma = string.Concat(picId1.ToString(), ",");
            }
            if (picId2 != 0)
            {
                picWithComma = string.Concat(picWithComma, picId2.ToString(), ",");
            }
            if (picId3 != 0)
            {
                picWithComma = string.Concat(picWithComma, picId3.ToString());
            }

            string actText = "following Pictures with remark submitted by " + userName;
            RFIFunctions.AddTimelineActivity(objModel.RFIId, actText, "bg-c-purple", "InspPic", picWithComma.TrimEnd(','));
            return picWithComma;
        }

        public int AddPictureToDBAndFolder(HttpPostedFileBase picture, string remark, int revisionId, int rfiId, string status)
        {
            string localPath = string.Format("~/Uploads/RFIPictures/{0}/{1}", rfiId, revisionId);
            Functions.CreateIfMissing(Server.MapPath(localPath));

            string getFileName = Path.GetFileName(picture.FileName);
            string fileName = string.Concat(revisionId + "-" + getFileName.Replace(' ', '_'));
            string filePath = string.Format("/Uploads/RFIPictures/{0}/{1}/{2}", rfiId, revisionId, fileName);
            int autoId = 0;

            try
            {
                //save to Folder
                picture.SaveAs(Server.MapPath(filePath));

                #region -----SAVE PICTURE TO DB--------

                using (dbRVNLMISEntities dbcontext = new dbRVNLMISEntities())
                {
                    tblRFIPicture objAddNew = new tblRFIPicture();
                    objAddNew.RFIRevId = revisionId;
                    objAddNew.Picture = getFileName;
                    objAddNew.Remarks = remark;
                    objAddNew.PicPath = string.Concat(localPath, "/", fileName);
                    objAddNew.InspStatus = status;
                    objAddNew.AddedOn = DateTime.Now;
                    objAddNew.AddedLocation = "Web";
                    objAddNew.AddedById = ((UserModel)Session["RFIUserSession"]).UserId;

                    dbcontext.tblRFIPictures.Add(objAddNew);
                    dbcontext.SaveChanges();
                    autoId = objAddNew.RFIPicId;
                }

                #endregion
            }
            catch (Exception ex)
            {

            }
            return autoId;
        }

        [HttpPost]
        public ActionResult UpdateSingleImage(HttpPostedFileBase picture, int picId, string remark, int rfiId, int revisionId)
        {
            string inspStatus = string.Empty;
            string localPath = string.Format("~/Uploads/RFIPictures/{0}/{1}", rfiId, revisionId);
            Functions.CreateIfMissing(Server.MapPath(localPath));

            string getFileName = Path.GetFileName(picture.FileName);
            string fileName = string.Concat(revisionId + "-" + getFileName.Replace(' ', '_'));
            string filePath = string.Format("/Uploads/RFIPictures/{0}/{1}/{2}", rfiId, revisionId, fileName);

            try
            {
                //save to Folder
                picture.SaveAs(Server.MapPath(filePath));

                #region -----SAVE PICTURE TO DB--------

                using (dbRVNLMISEntities dbcontext = new dbRVNLMISEntities())
                {
                    var getRFIPic = dbcontext.tblRFIPictures.Where(r => r.RFIPicId == picId).FirstOrDefault();
                    getRFIPic.Picture = getFileName;
                    getRFIPic.Remarks = remark;
                    getRFIPic.PicPath = string.Concat(localPath, "/", fileName);
                    inspStatus = getRFIPic.InspStatus;

                    dbcontext.SaveChanges();
                }

                #endregion
            }
            catch (Exception ex)
            {

            }
                RFIInspStatusUpdateModel objModel = new RFIInspStatusUpdateModel();
            objModel.objPicture = _objCommon.GetRFIPictureModel(revisionId, rfiId, inspStatus);
            return View("_PartialRFIPictures", objModel);
        }

        #region ------DELETE RFI-------

        [HttpPost]
        public ActionResult DeleteRFIPicture(int hdnrfiId, int hdnPicIdToDelete)
        {
            int? revisionId;
            string inspStatus = string.Empty;

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var obj = dbContext.tblRFIPictures.Where(w => w.RFIPicId == hdnPicIdToDelete).FirstOrDefault();
                    revisionId = obj.RFIRevId;
                    inspStatus = obj.InspStatus;
                    dbContext.tblRFIPictures.Remove(obj);
                    dbContext.SaveChanges();
                }

                RFIInspStatusUpdateModel objModel = new RFIInspStatusUpdateModel();
                objModel.objPicture = _objCommon.GetRFIPictureModel(revisionId ?? 0, hdnrfiId, inspStatus);
                return View("_PartialRFIPictures", objModel);
            }
            catch (Exception ex)
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        #endregion

        #region --------SHOW RFI TIMELINE-----------

        public ActionResult GetRFITimeline(int rfiId)
        {
            RFITimeLineModel objModel = new RFITimeLineModel();

            try
            {
                objModel = _objCommon._TimelineList(rfiId);
                return View("_PartialRFITimeline", objModel);
            }
            catch (Exception ex)
            {
                return View("_PartialRFITimeline", objModel);
            }
        }

        #endregion

        #region -------FILTER LIST-----
        public ActionResult FilterRFIList(FormCollection fc)
        {
            List<RFIMainModel> lst = new List<RFIMainModel>();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                int i = 0;
                string[] value = new string[fc.Count];
                foreach (var key in fc.AllKeys)
                {
                    if (fc[key] != "XMLHttpRequest")
                    {
                        value[i] = fc[key];
                        i++;
                    }
                }

                value = value.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                lst = value.Count() == 0 ? GetRFIList() : GetRFIList().Where(x => value.Contains(x.InspStatus)).ToList();

                return View("_PartialListRFIWithOrdering", lst);
            }
        }
        #endregion


        #region Import Excel

        public ActionResult GetImortModal()
        {          
            int pkgId = ((UserModel)Session["RFIUserSession"]).RoleTableID;
            using (var db = new dbRVNLMISEntities())
            {
                List<DropDownOptionModel> obj = new List<DropDownOptionModel>();
                obj = db.tblPackages.Where(p => p.IsDeleted == false)
                      .AsEnumerable()
                      .Select(s => new DropDownOptionModel
                      {
                          ID = s.PackageId,
                          Name = s.PackageCode + "-" + s.PackageName
                      }).ToList();
                if (pkgId != 0)
                {
                    obj = obj.Where(w => w.ID == pkgId).ToList();
                }
                ViewBag.ProjectPackageList = new SelectList(obj, "Id", "Name", 0);
            }           
            return View("_ImportExcel");
        }

        //[HttpPost]
        //public ActionResult ImportExcel(int packageId, HttpPostedFileBase AttachmentFile)
        //{
        //    int updateCnt = 0, addCnt = 0;
        //    string pathToExcelFile = string.Empty;
        //    List<string> data = new List<string>();           
        //    if (AttachmentFile.ContentType == "application/vnd.ms-excel" || AttachmentFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        //    {
        //        try
        //        {
        //            string filename = AttachmentFile.FileName;
        //            string targetpath = Server.MapPath("~/Uploads/ExcelImport");
        //            AttachmentFile.SaveAs(targetpath + filename);
        //            pathToExcelFile = targetpath + filename;
        //            var connectionString = "";
        //            if (filename.EndsWith(".xls"))
        //            {
        //                connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
        //            }
        //            else if (filename.EndsWith(".xlsx"))
        //            {
        //                connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
        //            }

        //            var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
        //            var ds = new DataSet();

        //            adapter.Fill(ds, "ExcelTable");

        //            DataTable dtable = ds.Tables["ExcelTable"];
        //            var outputPram = new ObjectParameter("OutputValue", 0);
        //            for (int i = 0; i < dtable.Rows.Count; i++)
        //            {                      

        //                using (var db = new dbRVNLMISEntities())
        //                {
        //                    DataRow row = dtable.Rows[i];
        //                    string WorkGroup = Convert.ToString(row["Work Group"]);
        //                    var WorkGroupID = (from k in db.tblWorkGroups where k.WorkGrName == WorkGroup select k.WorkGrId).SingleOrDefault();

        //                    var res = db.GenerateRFICode(WorkGroupID).FirstOrDefault();
        //                    string RFICOde = string.Concat(res.Code, "/", (Convert.ToInt32(res.Number) + 1));                        

        //                    int _startC = Functions.RepalceCharacter(Convert.ToString(row["Start Chainge"]));
        //                    int _endC = Functions.RepalceCharacter(Convert.ToString(row["End Chainage"]));

        //                    string LayerNo = Convert.ToString(row["LayerNo"]);
        //                    string RFIActivity = Convert.ToString(row["RFI Activity"]);
        //                    string LocationType = Convert.ToString(row["LocationType"]);
        //                    double StartChainge = _startC;
        //                    double EndChainage = _endC;
        //                    string Entity = Convert.ToString(row["Entity"]);
        //                    string OtherWorkLocation = Convert.ToString(row["Other Work Location"]);
        //                    string WorkSide = Convert.ToString(row["WorkSide"]);
        //                    string WorkDescription = Convert.ToString(row["WorkDescription"]);

        //                    db.RFIBulkImport(packageId, Convert.ToInt32(WorkGroupID), RFICOde, Convert.ToInt32(LayerNo), RFIActivity,
        //                    LocationType, StartChainge, EndChainage, Entity, OtherWorkLocation, WorkSide, WorkDescription, outputPram);
        //                }

        //                if (Convert.ToInt32(outputPram.Value) == 1)
        //                {
        //                    //update
        //                    updateCnt = updateCnt + 1;
        //                }
        //                else
        //                {
        //                    //add
        //                    addCnt = addCnt + 1;
        //                }
        //            }
        //            //deleting excel file from folder  
        //            if ((System.IO.File.Exists(pathToExcelFile)))
        //            {
        //                System.IO.File.Delete(pathToExcelFile);
        //            }
        //            return Json(addCnt + " new sections are added and " + updateCnt + " sections are updated. ", JsonRequestBehavior.AllowGet);
        //        }
        //        catch (Exception ex)
        //        {
        //            //deleting excel file from folder  
        //            if ((System.IO.File.Exists(pathToExcelFile)))
        //            {
        //                System.IO.File.Delete(pathToExcelFile);
        //            }
        //            return Json(ex.Message + " -- " + ex.InnerException, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        //alert message for invalid file format  
        //        return Json("Only Excel file format is allowed", JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPost]
        public ActionResult ImportExcel(int packageId, HttpPostedFileBase AttachmentFile)
        {
            int updateCnt = 0, addCnt = 0;
            string pathToExcelFile = string.Empty;
            List<string> data = new List<string>();
            string msg = "";
            if (AttachmentFile.ContentType == "application/vnd.ms-excel" || AttachmentFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                try
                {
                    string filename = AttachmentFile.FileName;
                    string targetpath = Server.MapPath("~/Uploads/ExcelImport");
                    AttachmentFile.SaveAs(targetpath + filename);
                    pathToExcelFile = targetpath + filename;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }

                    var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                    var ds = new DataSet();

                    adapter.Fill(ds, "ExcelTable");

                    DataTable dtable = ds.Tables["ExcelTable"];
                    var outputPram = new ObjectParameter("OutputValue", 0);
                    for (int i = 0; i < dtable.Rows.Count; i++)
                    {

                        using (var db = new dbRVNLMISEntities())
                        {
                            DataRow row = dtable.Rows[i];
                            double StartChainge = row["StartChainage"] as double? ?? 0;  
                            double EndChainage = row["EndChainage"] as double? ?? 0;
                            string OtherWorkLocation = "";
                            if (Convert.IsDBNull(row["Other Work Location"]) == true)
                            {
                                OtherWorkLocation = "";
                            }
                            else
                            {
                                OtherWorkLocation = Convert.ToString(row["Other Work Location"]);
                            }
                            string WorkDescription = Convert.ToString(row["WorkDescription"]);

                            string RFINo = Convert.ToString(row["RFINo"]);

                            DateTime? RFIOpenDate;
                            if (Convert.IsDBNull(row["DATE OF SUBMISSION"]) == true)
                            {
                                RFIOpenDate = null;
                            }
                            else
                            {
                                RFIOpenDate = Convert.ToDateTime(row["DATE OF SUBMISSION"]);
                            }

                           
                            DateTime? RFICloseDate;
                            if (Convert.IsDBNull(row["RECEIVED DATE"]) == true)
                            {
                                RFICloseDate = null;
                            }
                            else
                            {
                                RFICloseDate = Convert.ToDateTime(row["RECEIVED DATE"]);
                            }                         
                            DateTime? dateOfInspection;
                            if (Convert.IsDBNull(row["DATE OF INSPECTION"]) == true)
                            {
                                dateOfInspection = null;
                            }
                            else
                            {
                                dateOfInspection = Convert.ToDateTime(row["DATE OF INSPECTION"]);
                            }
                            //DateTime InspectionOn = Convert.ToDateTime(row["INSPECTED ON"]);
                            DateTime? InspectionOn;
                            if (Convert.IsDBNull(row["INSPECTED ON"]) == true)
                            {
                                InspectionOn = null;
                            }
                            else
                            {
                                InspectionOn = Convert.ToDateTime(row["INSPECTED ON"]);
                            }
                            string BOQItemNo = "";
                            if (Convert.IsDBNull(row["BOQ Item No"]) == true)
                            {
                                BOQItemNo = "";
                            }
                            else
                            {
                                BOQItemNo = Convert.ToString(row["BOQ Item No"]);
                            }
                            string ResultOfInspection = Convert.ToString(row["RESULT OF INSPECTION"]);
                            string RFIStatus = Convert.ToString(row["RFIStatus"]);

                            db.RFIBulkImportAhemedabadProject(packageId, StartChainge, EndChainage, OtherWorkLocation, WorkDescription, RFINo,
                                RFIOpenDate, RFICloseDate, dateOfInspection, InspectionOn, BOQItemNo, ResultOfInspection, RFIStatus, outputPram);
                        }

                        if (Convert.ToInt32(outputPram.Value) == 1)
                        {
                            //update
                            addCnt = addCnt + 1;
                            msg = addCnt + " RFI's are added Successfully";
                        }
                        else
                        {
                            msg = " RFI's are added exists";
                        }
                    }
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return Json(addCnt + " RFI's are added Successfully", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return Json(ex.Message + " -- " + ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //alert message for invalid file format  
                return Json("Only Excel file format is allowed", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

    }
}