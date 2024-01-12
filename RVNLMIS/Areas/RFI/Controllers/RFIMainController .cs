using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using PrimaBiWeb.Common;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [AreaSessionExpire]
    [HandleError]
    public class RFIMainController : Controller
    {
        #region ---- Page Load ----

        public ActionResult Index(int? id)
        {
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
                            RoleCode = "Contractor",
                            MobileNo = s.MobileNo,
                            EmailId = s.EmailId
                        })
                        .FirstOrDefault();

                    if (oUser.UserName == "Admin")
                    {
                        oUser.RoleTableID = 0;
                    }
                    Session["RFIUserSession"] = oUser;
                }
            }
            return View();
        }

        #endregion

        #region --- LIST RFI DETAILS -----
        public ActionResult Read_RFIMainDetails([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbcontext = new dbRVNLMISEntities())
            {
                int packageId = ((UserModel)Session["RFIUserSession"]).RoleTableID;
                int userId = ((UserModel)Session["RFIUserSession"]).UserId;
                string orgnisation = ((UserModel)Session["RFIUserSession"]).RoleCode;
                List<RFIMainModel> lst = new List<RFIMainModel>();

                lst = dbcontext.ViewRFIMainDetails
                    .AsEnumerable()
                    .Select(s => new RFIMainModel
                    {
                        RFIId = s.RFIId,
                        RFICode = s.RFICode,
                        StartChainage = s.StartChainge == null ? s.StartChainge.ToString() : IntToStringChainage(Convert.ToInt32((double)s.StartChainge)),
                        EndChainage = s.EndChainage == null ? s.EndChainage.ToString() : IntToStringChainage(Convert.ToInt32((double)s.EndChainage)),
                        PackageId = s.PkgId,
                        OtherWorkLocation = s.OtherWorkLocation,
                        LocationType = s.LocationType,
                        EntityName = s.EntityName,
                        Layer = GetLayer(s.LayerNo),
                        ActivityName = s.RFIActName,
                        PackageName = s.PackageName,
                        WorkSide = s.WorkSide,
                        InspStatus = s.InspStatus,
                        Workgroup = s.WorkGrName,
                        AssignToPMC = s.AssignedTo,
                        AssignToPMCName = s.AssignedToName,
                        RFIOpenDate = s.RFIOpenDate
                    }).ToList();

                if (packageId != 0 && orgnisation == "PMC")
                {
                    lst = lst.Where(p => p.AssignToPMC == userId).ToList();
                }
                else if (packageId != 0)
                {
                    lst = lst.Where(p => p.PackageId == packageId).ToList();
                }
                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        private string GetLayer(int? layerNo)
        {
            switch (layerNo)
            {
                case -1: return "NA";
                case 0: return "Top";
            }
            return layerNo.ToString();
        }
        #endregion

        #region ---- Chainage int to String ----
        /// <summary>
        /// Ints to string chainage.
        /// </summary>
        /// <param name="chainage">The chainage.</param>
        /// <returns></returns>
        public string IntToStringChainage(int chainage)
        {
            return chainage.ToString("D6").Insert(chainage.ToString("D6").Length - 3, "+");
        }
        #endregion

        #region ----ADD EDIT RFI ------

        //[AreaSessionExpire]
        public ActionResult AddEditRFI(int id)
        {
            RFIMainModel objModel = new RFIMainModel();
            objModel.LocationType = "Chainage";
            BindLayerDrp();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                if (id != 0)
                {
                    objModel = dbContext.ViewRFIMainDetails.Where(r => r.RFIId == id)
                        .AsEnumerable()
                        .Select(s => new RFIMainModel
                        {
                            RFIId = s.RFIId,
                            PackageId = s.PkgId,
                            EndChainage = s.EndChainage == null ? s.EndChainage.ToString() : IntToStringChainage(Convert.ToInt32((double)s.EndChainage)),
                            StartChainage = s.StartChainge == null ? s.StartChainge.ToString() : IntToStringChainage(Convert.ToInt32((double)s.StartChainge)),
                            EntityId = s.Entity,
                            LayerNo = s.LayerNo,
                            LocationType = s.LocationType,
                            WorkDescription = s.WorkDescription,
                            OtherWorkLocation = s.OtherWorkLocation,
                            RFIActivityId = s.RFIActivityId,
                            RFICode = s.RFICode,
                            RFIStatus = s.RFIStatus,
                            WorkgroupId = s.WorkGroupId,
                            WorkSide = s.WorkSide,
                            AssignToPMC = s.AssignedTo
                        })
                        .FirstOrDefault();
                }
            }
            return View("_PartialAddEditRFIMain", objModel);
        }

        //[AreaSessionExpire]
        public ActionResult SubmitRFI(RFIMainModel objRFIModel)
        {
            int _startC = Functions.RepalceCharacter(objRFIModel.StartChainage);
            int _endC = Functions.RepalceCharacter(objRFIModel.EndChainage);

            if (!ModelState.IsValid)
            {
                BindLayerDrp();
                return View("_PartialAddEditRFIMain", objRFIModel);
            }

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                if (objRFIModel.RFIId == 0)         //add operation
                {
                    tblRFIMain objAdd = new tblRFIMain();
                    objAdd.PkgId = objRFIModel.PackageId;
                    objAdd.RFIActivityId = objRFIModel.RFIActivityId;
                    objAdd.RFICode = objRFIModel.RFICode;
                    //objAdd.StartChainge = objRFIModel.StartChainage;
                    //objAdd.EndChainage = objRFIModel.EndChainage;
                    objAdd.StartChainge = _startC;
                    objAdd.EndChainage = _endC;
                    objAdd.Entity = Convert.ToString(objRFIModel.EntityId);
                    objAdd.LayerNo = objRFIModel.LayerNo;
                    objAdd.WorkGroupId = objRFIModel.WorkgroupId;
                    objAdd.WorkSide = objRFIModel.WorkSide;
                    objAdd.WorkDescription = objRFIModel.WorkDescription;
                    objAdd.OtherWorkLocation = objRFIModel.OtherWorkLocation;
                    objAdd.LocationType = objRFIModel.LocationType;
                    objAdd.RFIStatus = "Open";
                    objAdd.RFIOpenDate = DateTime.Now;
                    dbContext.tblRFIMains.Add(objAdd);
                    dbContext.SaveChanges();
                    try
                    {
                        AddRevision(objAdd.RFIId, objRFIModel.AssignToPMC);
                    }
                    catch (Exception ex)
                    {
                        return Json("0", JsonRequestBehavior.AllowGet); ///error
                    }

                    // if (objRFIModel.AssignToPMC != null)
                    // SMSWhenNewRFI(objRFIModel.AssignToPMC);

                    return Json("1", JsonRequestBehavior.AllowGet);  //add succes
                }
                else                               //edit operation
                {
                    tblRFIMain objEdit = new tblRFIMain();
                    objEdit.PkgId = objRFIModel.PackageId;
                    objEdit.RFIActivityId = objRFIModel.RFIActivityId;
                    objEdit.WorkDescription = objRFIModel.WorkDescription;
                    ///objEdit.RFICode = objRFIModel.RFICode;
                    objEdit.StartChainge = _startC;
                    objEdit.EndChainage = _endC;
                    objEdit.Entity = Convert.ToString(objRFIModel.EntityId);
                    objEdit.LayerNo = objRFIModel.LayerNo;
                    objEdit.WorkGroupId = objRFIModel.WorkgroupId;
                    objEdit.WorkSide = objRFIModel.WorkSide;
                    objEdit.OtherWorkLocation = objRFIModel.OtherWorkLocation;
                    objEdit.LocationType = objRFIModel.LocationType;
                    dbContext.SaveChanges();

                    if (objRFIModel.AssignToPMC != null)
                    {
                        //update assigned to revision
                        AddRevision(objRFIModel.RFIId, objRFIModel.AssignToPMC);
                    }

                    return Json("2", JsonRequestBehavior.AllowGet);  //update success
                }
            }
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

        private void AddRevision(int rfiId, int? assignedToId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                //var isExist = dbContext.tblRFIRevisions.Where(r => r.RFIId == rfiId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                //if (isExist == null)
                //{
                try
                {
                    tblRFIRevision objRev = new tblRFIRevision();
                    objRev.RFIId = rfiId;

                    var getTop1RevForRFI = dbContext.tblRFIRevisions.Where(r => r.RFIId == rfiId)
                        .OrderByDescending(r => r.RFIRevId).FirstOrDefault();

                    if (getTop1RevForRFI == null)
                    {
                        objRev.Revision = "0";
                    }
                    else
                    {
                        objRev.Revision = Convert.ToString(Convert.ToInt32(getTop1RevForRFI.Revision) + 1);
                    }

                    objRev.CreatedBy = ((UserModel)Session["RFIUserSession"]).UserId;
                    objRev.CreatedDate = DateTime.Now;
                    objRev.CreatedLocation = "Web";
                    objRev.AssignedTo = assignedToId;

                    dbContext.tblRFIRevisions.Add(objRev);
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {

                }
            }
            //}
        }

        #endregion

        #region -------SUPPORTIVE METHOD-------

        public ActionResult GenerateRFICode(int workGrpId)
        {
            string code = string.Empty;

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var res = dbContext.GenerateRFICode(workGrpId).FirstOrDefault();
                code = string.Concat(res.Code, "/", (Convert.ToInt32(res.Number) + 1));
            }
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
                using (var db = new dbRVNLMISEntities())
                {
                    obj = (from w in db.tblWorkGroups
                           select new { w })
                          .AsEnumerable()
                          .Select(s => new DropDownOptionModel
                          {
                              ID = s.w.WorkGrId,
                              Name = s.w.WorkGrName
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
                using (var db = new dbRVNLMISEntities())
                {
                    obj = (from a in db.tblRFIActivities
                           where a.WorkGroupId == wrkgrpId && a.isDeleted == false
                           select new { a })
                          .AsEnumerable()
                          .Select(s => new DropDownOptionModel
                          {
                              ID = s.a.RFIActId,
                              Name = s.a.RFIActName
                          }).ToList();
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_AssignedToPMCNames(int? pkgId, string text)
        {
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
                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.Name, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
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
                using (var db = new dbRVNLMISEntities())
                {
                    obj = (from a in db.tblWorkSides
                           where a.IsDeleted == false
                           select new { a })
                          .AsEnumerable()
                          .Select(s => new DropDownOptionModel
                          {
                              // ID = s.a.WorkSideId,
                              Name = s.a.WorkSideName
                          }).ToList();
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
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
                              Name = s.e.EntityName
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
                objModel.objAttachDetail = GetAttachModel(id);
            }
            return View("_PartialEnclouser", objModel);
        }

        private List<AttachDetails> GetAttachModel(int id)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<AttachDetails> objAttachDetail = dbContext.tblRFIEnclosures.Join(dbContext.tblEnclosures, re => re.EnclId, e => e.EnclId, (re, e)
                          => new { re, e }).Where(r => r.re.RFIId == id).AsEnumerable()
                    .Select(s => new AttachDetails
                    {
                        RFIEnclId = s.re.RFIEnclId,
                        EnclType = s.e.EnclName,
                        FileName = s.re.EnclAttach,
                        Path = s.re.EnclAttachPath,
                        Icon = GetFileIconFromExtension(s.re.EnclAttach)
                    }).ToList();
                return objAttachDetail;
            }
        }

        private string GetFileIconFromExtension(string enclAttachFileName)
        {
            var extension = enclAttachFileName.Substring(enclAttachFileName.LastIndexOf('.') + 1);
            string icon = string.Empty;

            switch (extension)
            {
                case "pdf":
                    icon = "fa fa-file-pdf";
                    break;
                case "xls":
                case "xlsx":
                    icon = "fa fa-file-excel";
                    break;
                case "doc":
                case "docx":
                    icon = "fa fa-file-word";
                    break;
            }
            return icon;
        }

        #endregion

        #region -----ENCLOSURE ATTACH DELETE-----

        //[AreaSessionExpire]
        public ActionResult DeleteEnclAttach(int id)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    #region ----DELETE FILE FROM DB------

                    var obj = dbContext.tblRFIEnclosures.Where(e => e.RFIEnclId == id).FirstOrDefault();
                    string path = string.Concat("~/Uploads/", obj.EnclAttach);
                    int rfiId = obj.RFIId ?? 0;

                    dbContext.tblRFIEnclosures.Remove(obj);
                    dbContext.SaveChanges();

                    #endregion

                    #region -------DELETE FILE FROM PATH------

                    Functions.DeleteFilesInFolder(path, false);

                    #endregion

                    EnclosureAttachModel objModel = new EnclosureAttachModel();
                    objModel.RFIId = rfiId;
                    objModel.objAttachDetail = GetAttachModel(rfiId);
                    string _EnclView = RenderRazorViewToString("_PartialEnclouser", objModel);
                    return Json(new { message = "Data deleted successfully.", viewHtml = _EnclView }, JsonRequestBehavior.AllowGet); //success
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = "Error!", viewHtml = "" }, JsonRequestBehavior.AllowGet); //success
            }
        }

        #endregion

        #region ------ ENCLOSURE ATTACH ADD--------

        //[AreaSessionExpire]
        public ActionResult SubmitEnclAttach(EnclosureAttachModel objEncl)
        {
            if (objEncl.AttachmentFile != null)
            {
                string packageCode = GetPackageCode(objEncl.RFIId);

                FileInfo fi = new FileInfo(objEncl.AttachmentFile.FileName);
                var strings = new List<string> { ".pdf", ".xls", ".xlsx", ".doc", ".docx" };
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
                    }

                    EnclosureAttachModel objModel = new EnclosureAttachModel();
                    objModel.RFIId = objEncl.RFIId;
                    objModel.objAttachDetail = GetAttachModel(objEncl.RFIId);
                    string _EnclView = RenderRazorViewToString("_PartialEnclouser", objModel);
                    return Json(new { message = "1", viewHtml = _EnclView }, JsonRequestBehavior.AllowGet); //success
                }
                catch (Exception ex)
                {
                    return Json(new { message = "error", viewHtml = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { message = "2", viewHtml = "" }, JsonRequestBehavior.AllowGet); //please select a file
        }

        private string GetPackageCode(int rFIId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var pkgInfo = (from p in dbContext.tblPackages
                               join r in dbContext.tblRFIMains
                               on p.PackageId equals r.PkgId
                               where r.RFIId == rFIId
                               select p.PackageCode).FirstOrDefault();

                return pkgInfo.ToString();
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
            RFIInspStatusUpdateModel objModel = new RFIInspStatusUpdateModel();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                objModel = dbContext.ViewRFIMainDetails.Where(r => r.RFIId == rfiId)
                    .AsEnumerable()
                    .Select(s => new RFIInspStatusUpdateModel
                    {
                        RFIId = s.RFIId,
                        RevisionId = s.RFIRevId,
                        PackageName = s.PackageName,
                        ActivityName = s.RFIActName,
                        Workgroup = s.WorkGrName,
                        AssignedToName = s.AssignedToName,
                        Enclosure = s.Enclosure,
                        RFICode = s.RFICode,
                        EndChainage = s.EndChainage == null ? "-" : IntToStringChainage(Convert.ToInt32((double)s.EndChainage)),
                        StartChainage = s.StartChainge == null ? "-" : IntToStringChainage(Convert.ToInt32((double)s.StartChainge)),
                        EntityName = s.EntityName,
                        InspStatus = s.InspStatus ?? "Open",
                        Layer = GetLayer(s.LayerNo),
                        LocationType = s.LocationType,
                        OtherWorkLocation = s.OtherWorkLocation,
                        WorkSide = s.WorkSide,
                        //InspDate = s.InspDate == null ? string.Empty : Convert.ToDateTime(s.InspDate).ToString("yyyy-MM-ddTHH:mm"),
                        InspDate = s.InspDate,
                        InspId = s.InspId,
                        InspName = s.InspStatus,
                        Note = s.note
                    }).FirstOrDefault();
            }
            return View("_PartialUpdateInspStatus", objModel);
        }

        public ActionResult UpdateRFIDateAndStatus(RFIInspStatusUpdateModel objModel)
        {
            int loginUserId = Convert.ToInt32(((UserModel)Session["RFIUserSession"]).UserId);
            string userOrgnisation = Convert.ToString(((UserModel)Session["RFIUserSession"]).RoleCode);

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    if (userOrgnisation == "Contractor")
                    {
                        ///// submit comment to rfi revision and add new rfi revision
                        var getRFI = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                        getRFI.ContrComment = objModel.ContrComment;
                        int? assigneePMC = getRFI.AssignedTo;
                        dbContext.SaveChanges();

                        AddRevision(objModel.RFIId, assigneePMC);

                        ///send notification pmc
                        string message = "RFI Revision submitted";
                        //SMSWhenNewRFI(assigneePMC,);
                        return Json("3", JsonRequestBehavior.AllowGet);
                    }

                    if (objModel.InspStatus == "Open" && objModel.InspDate != null)
                    {
                        ////assign inspection date and send notification to contractor
                        var getRFI = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                        getRFI.InspDate = Convert.ToDateTime(objModel.InspDate);
                        getRFI.AssignedTo = objModel.AssignedTo;
                        dbContext.SaveChanges();
                        //SMSWhenNewRFI(getRFI.AssignedTo,);
                        return Json("1", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        #region ------UPDATE INSPECTION STATUS ADD/UPDATE REVISION ENTRIES -----

                        switch (objModel.InspStatus)
                        {
                            case "Approved":
                                //Close RFI
                                var getRFI = dbContext.tblRFIMains.Where(r => r.RFIId == objModel.RFIId).FirstOrDefault();
                                getRFI.RFICloseDate = Convert.ToDateTime(objModel.InspDate);
                                //update in revision table 
                                //accepted by
                                //accepted date
                                //actual inspdate

                                var getRevision = dbContext.tblRFIRevisions.Where(rv => rv.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                                getRevision.InspStatus = objModel.InspStatus;
                                getRevision.InspId = objModel.InspId;
                                getRevision.InspDate = DateTime.Now;
                                getRevision.InspBy = Convert.ToString(loginUserId);
                                getRevision.AcceptedBy = loginUserId;
                                getRevision.AcceptedDate = DateTime.Now;
                                dbContext.SaveChanges();
                                break;
                            case "Revise & Resubmit":
                                //Update RFI Revision 
                                var getRFIRev = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();

                                getRFIRev.InspDate = Convert.ToDateTime(objModel.InspDate);
                                getRFIRev.Note = objModel.Note;
                                getRFIRev.InspId = objModel.InspId;
                                getRFIRev.InspStatus = objModel.InspStatus;
                                getRFIRev.InspBy = Convert.ToString(loginUserId);
                                dbContext.SaveChanges();

                                ///Notification send to Contractor




                                break;
                            case "Rejected":
                                var getRevToReject = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();

                                getRevToReject.InspDate = Convert.ToDateTime(objModel.InspDate);
                                getRevToReject.Note = objModel.Note;
                                getRevToReject.InspId = objModel.InspId;
                                getRevToReject.InspStatus = objModel.InspStatus;
                                getRevToReject.InspBy = Convert.ToString(loginUserId);
                                dbContext.SaveChanges();
                                break;
                            default:
                                var updateAssignedTo = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                                updateAssignedTo.AssignedTo = objModel.AssignedTo;
                                dbContext.SaveChanges();

                                // SMSWhenNewRFI(objModel.AssignedTo,);
                                break;
                        }

                        #endregion

                        #region ------SUBMIT PICTURES-------

                        if (objModel.Picture1 != null)
                        {
                            AddPictureToDBAndFolder(objModel.Picture1, objModel.RevisionId, objModel.RFIId);
                        }
                        if (objModel.Picture2 != null)
                        {
                            AddPictureToDBAndFolder(objModel.Picture1, objModel.RevisionId, objModel.RFIId);
                        }
                        if (objModel.Picture3 != null)
                        {
                            AddPictureToDBAndFolder(objModel.Picture1, objModel.RevisionId, objModel.RFIId);
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return Json("2", JsonRequestBehavior.AllowGet);
        }

        public void AddPictureToDBAndFolder(HttpPostedFileBase picture, int revisionId, int rfiId)
        {
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
                    tblRFIPicture objAddNew = new tblRFIPicture();
                    objAddNew.RFIRevId = revisionId;
                    objAddNew.Picture = getFileName;
                    objAddNew.AddedOn = DateTime.Now;
                    objAddNew.AddedLocation = "Web";
                    objAddNew.AddedById = ((UserModel)Session["RFIUserSession"]).UserId;
                    //objAddNew.ramrk

                    dbcontext.tblRFIPictures.Add(objAddNew);
                    dbcontext.SaveChanges();
                }

                #endregion
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

    }
}