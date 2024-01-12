using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class EnggDrawingController : Controller
    {
        public string IpAddress = "";
        public string FileName = "";
        // GET: EnggDrawing
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
            int k = Functions.SaveUserLog(pkgId, "Engineering Drawing", "View", UserID, IpAddress, "NA");
            BindDropdown();

            return View();
        }

        public ActionResult PkgDwgAppInfo_Read([DataSourceRequest] DataSourceRequest request, int? entityId, int? drawing)
        {
            List<PkgDwgApprovalModel> lstInfo = new List<PkgDwgApprovalModel>();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    if (drawing == null && entityId != null)
                    {
                        lstInfo = dbContext.EntityDrawingApprViews.Where(e => e.EntityId == entityId).Select(s => new PkgDwgApprovalModel
                        {
                            AutoId = s.Id,
                            EntityName = s.EntityName,
                            ApprovedBy = s.ApprGateName,
                            DrawingName = s.DrawingName,
                            Drawing = s.DwgName,
                            Date = s.ApprStartDate,
                            Revision = "Rev " + s.Revision,
                            ActionName = s.ActionName,
                            Remark = s.Remark,
                            AttachFileName = s.FileName,
                            AttachFilePath = s.Path
                        }).ToList();
                    }
                    else if (drawing != null && entityId != null)
                    {
                        lstInfo = dbContext.EntityDrawingApprViews.Where(e => e.DwgId == drawing && e.EntityId == entityId).Select(s => new PkgDwgApprovalModel
                        {
                            AutoId = s.Id,
                            EntityName = s.EntityName,
                            ApprovedBy = s.ApprGateName,
                            DrawingName = s.DrawingName,
                            Drawing = s.DwgName,
                            Date = s.ApprStartDate,
                            Revision = "Rev " + s.Revision,
                            ActionName = s.ActionName,
                            Remark = s.Remark,
                            AttachFileName = s.FileName,
                            AttachFilePath = s.Path
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(lstInfo.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            return Json(lstInfo.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var packages = Functions.GetRoleAccessiblePackageList();
                    ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");

                    //if (packages.Count == 1)
                    //{
                    //    int pkgId = packages[0].PackageId;
                    //    var entity = dbContext.tblMasterEntities.Where(e => e.PackageId == pkgId && e.SectionID!=0 && e.IsDelete == false).Select(s => new
                    //    {
                    //        EntityId = s.EntityID,
                    //        EntityName = s.EntityCode + " " + s.EntityName
                    //    }).ToList();
                    //    ViewBag.EntityList = new SelectList(entity, "EntityID", "EntityName");
                    //}
                    //else
                    //{
                    //    ViewBag.EntityList = new SelectList(new List<tblMasterEntity>(), "EntityID", "EntityName");
                    //}

                    //var approver = dbContext.tblEnggApprGates.Where(a => a.IsDeleted == false).ToList();
                    //ViewBag.ApproverList = new SelectList(approver, "ApprGateId", "ApprGateName");

                    //// var actions = dbContext.tblApprActions.Where(a => a.IsDeleted == false).ToList();
                    //ViewBag.ActionList = new SelectList(new List<tblApprAction>(), "ActionId", "ActionName");

                    //var drawing = dbContext.tblEnggDwgTypes.Where(d => d.IsDeleted == false).ToList();
                    //ViewBag.DrawingList = new SelectList(drawing, "DwgId", "DwgName");
                }
            }
            catch (Exception ex)
            {

            }
        }

        public ActionResult LoadEditDrawingView(int id)
        {
            DrawingUpdateModel objModel = new DrawingUpdateModel();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                BindDropdown();
                try
                {
                    var present = dbContext.EntityDrawingApprViews.Where(t => t.Id == id).FirstOrDefault();

                    var entityDrpList = (from e in dbContext.tblMasterEntities
                                         where e.PackageId == present.PackageId && e.SectionID != 0 && e.IsDelete == false
                                         select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                                         {
                                             Id = s.e.EntityID,
                                             //Name = s.e.EntityName + " " + s.e.StartChainage + " " + s.e.EndChainage
                                             Name = s.e.EntityCode + " " + s.e.EntityName
                                         }).ToList();
                    ViewBag.EntityList = new SelectList(entityDrpList, "Id", "Name");

                    var actions = dbContext.tblApprActions.Where(a => a.ApproverId == present.ApprGateId && a.IsDeleted == false).ToList();
                    ViewBag.ActionList = new SelectList(actions, "ActionId", "ActionName");

                    if (present != null)
                    {
                        objModel.PackageId = present.PackageId;
                        objModel.AutoId = present.Id;
                        objModel.EntityId = present.EntityId;
                        objModel.DrawingTypeId = present.DwgId;
                        objModel.ApprovedById = present.ApprGateId;
                        objModel.StrDwgDate = Convert.ToDateTime(present.ApprStartDate).ToString("yyyy-MM-dd");
                        objModel.Revision = Convert.ToInt32(present.Revision);
                        objModel.ActionId = present.ActionId;
                        objModel.Remark = present.Remark;
                        objModel.AttachFileName = present.FileName;
                        objModel.AttachFilePath = present.Path;
                        objModel.DrawingName = present.DrawingName == "-" ? string.Empty : present.DrawingName;
                        objModel.HdnDrawingName = present.DrawingName == "-" ? string.Empty : present.DrawingName;
                        objModel.OperationType = "Update";
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return View("_DrawingUpdate", objModel);
        }

        [HttpPost]

        public ActionResult SubmitDrawing(DrawingUpdateModel objModel)
        {
            string message = string.Empty;
            int attachmentId = 0;

            List<tblEnggDwgAttachment> attchList = new List<tblEnggDwgAttachment>();
            try
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {


                    #region  --- Check modelState error ---

                    if (!ModelState.IsValid)
                    {
                        //objModel.PackageId = packageInfo.PackageId;
                        BindDropdown();

                        #region commented
                        //var entityDrpList = (from e in dbContext.tblMasterEntities
                        //                     where e.PackageId == objModel.PackageId && e.SectionID != 0 && e.IsDelete == false
                        //                     select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                        //                     {
                        //                         Id = s.e.EntityID,
                        //                         //Name = s.e.EntityName + " " + s.e.StartChainage + " " + s.e.EndChainage
                        //                         Name = s.e.EntityCode + " " + s.e.EntityName
                        //                     }).ToList();
                        //ViewBag.EntityList = new SelectList(entityDrpList, "Id", "Name");

                        //var actions = dbContext.tblApprActions.Where(a => a.ApproverId == objModel.ApprovedById && a.IsDeleted == false).ToList();
                        //ViewBag.ActionList = new SelectList(actions, "ActionId", "ActionName");
                        #endregion

                        return View("_DrawingUpdate", objModel);
                    }

                    #endregion

                    tblEnggEntDwgAppr getExisting = new tblEnggEntDwgAppr();

                    #region -- ATTACHMENT --

                    try
                    {
                        var packageInfo = (from p in dbContext.tblPackages
                                           join e in dbContext.tblMasterEntities on p.PackageId equals e.PackageId
                                           where e.EntityID == objModel.EntityId && e.IsDelete == false && p.IsDeleted == false
                                           select new
                                           {
                                               p.PackageCode,
                                               p.PackageId
                                           }).Distinct().FirstOrDefault();

                        //dbContext.tblPackages.Where(p => p.PackageId == objModel.PackageId).SingleOrDefault().PackageCode;

                        getExisting = dbContext.tblEnggEntDwgApprs.Where(d => d.Id == objModel.AutoId).FirstOrDefault();

                        if (objModel.AttachmentFile != null)
                        {
                            #region ---- For single file ----
                            //FileInfo fi = new FileInfo(objModel.AttachmentFile.FileName);
                            //var strings = new List<string> { ".pdf", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".zip" };
                            //bool contains = strings.Contains(fi.Extension, StringComparer.OrdinalIgnoreCase);
                            //if (!contains)
                            //{
                            //    return Json("0", JsonRequestBehavior.AllowGet);
                            //}

                            //        //// creating a StringCollection named myCol 
                            //        //StringCollection myCol = new StringCollection();

                            //        //// creating a string array named myArr 
                            //        //String[] myArr = new String[] { ".pdf, .xls, .xlsx, .png, .jpg, .jpeg" };

                            //        //// Copying the elements of a string 
                            //        //// array to the end of the StringCollection. 
                            //        //myCol.AddRange(myArr);
                            //        //bool contains = myCol.Contains(fi.Extension);

                            //        //if (!contains)
                            //        //{
                            //        //    return Json("0", JsonRequestBehavior.AllowGet);
                            //        //}                            
                            //attachmentId = Functions.AttachmentCommonFun(objModel.AttachmentFile, packageCode, "Drawing", "Drawing", getExisting);
                            #endregion

                            #region -- added for multiple file upload
                            foreach (HttpPostedFileBase postedFile in objModel.AttachmentFile)
                            {
                                if (postedFile != null)
                                {
                                    FileInfo fi = new FileInfo(postedFile.FileName);
                                    FileName = Convert.ToString(fi.Name);
                                    var strings = new List<string> { ".pdf", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".zip" };
                                    bool contains = strings.Contains(fi.Extension, StringComparer.OrdinalIgnoreCase);
                                    if (!contains)
                                    {
                                        return Json("0", JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            foreach (HttpPostedFileBase postedFile in objModel.AttachmentFile)
                            {
                                if (postedFile != null)
                                {
                                    attchList.Add(new tblEnggDwgAttachment
                                    {
                                        EnggDwgId = 0,
                                        AttachmentID = Functions.AttachmentCommonFun(postedFile, packageInfo.PackageCode, "Drawing", "Drawing", getExisting)
                                    });

                                }
                            }
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    #endregion

                    int drwingId = 0;

                    string strRev = Convert.ToString(objModel.Revision);
                    DateTime SelectedStartDate = Convert.ToDateTime(objModel.StrDwgDate);


                    if (objModel.AutoId != 0)   //Update operation
                    {
                        var isexist = dbContext.tblEnggEntDwgApprs
                            .Where(e => e.EntityId == objModel.EntityId && e.DwgId == objModel.DrawingTypeId && e.DrawingName == objModel.DrawingName
                            && e.ApprStartDate == SelectedStartDate && e.Id != objModel.AutoId && e.AppGateId == objModel.ApprovedById
                            && e.ActionId == objModel.ActionId && e.Revision == strRev
                             ).FirstOrDefault();

                        if (isexist != null)
                        {
                            return Json("1", JsonRequestBehavior.AllowGet);
                        }

                        getExisting.EntityId = objModel.EntityId;
                        getExisting.DwgId = objModel.DrawingTypeId;
                        getExisting.DrawingName = objModel.DrawingName == string.Empty ? "-" : objModel.DrawingName;
                        getExisting.AppGateId = objModel.ApprovedById;
                        getExisting.ApprStartDate = (objModel.StrDwgDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objModel.StrDwgDate);
                        getExisting.ApprFinsihDate = SelectedStartDate;
                        getExisting.Revision = Convert.ToString(objModel.Revision);
                        getExisting.ActionId = objModel.ActionId;
                        getExisting.Remark = objModel.Remark;
                        getExisting.AttachmentId = attachmentId == 0 ? getExisting.AttachmentId : attachmentId;
                        message = "Data updated successfully.";
                        dbContext.SaveChanges();
                        drwingId = objModel.AutoId;


                        try
                        {
                            string str = ""; ;
                            var UpdatedValue = (from ul in dbContext.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                            if (UpdatedValue != null)
                            {
                                str = UpdatedValue;
                                str = str.Replace("ActionId,", "");
                                str = str.Replace("AppGateId,", "");
                                str = str.Replace("ApprFinsihDate,", "");
                                str = str.Replace("ApprStartDate,", "");


                                str = str.Replace("ActionId", "");
                                str = str.Replace("AppGateId", "");
                                str = str.Replace("ApprFinsihDate", "");
                                str = str.Replace("ApprFinsihDate", "");
                                str = str.Replace(", ,", ",");
                                str = str.TrimEnd(',');
                                if (String.IsNullOrEmpty(str))
                                {
                                    str = "Drawing Name: " + objModel.DrawingName + "= " + "NA";
                                }
                                else
                                {
                                    str = "Drawing Name: " + objModel.DrawingName + "= " + str;
                                }

                            }
                            else
                            {
                                str = "NA";
                            }

                            int k = Functions.SaveUserLog(pkgId, "Engineering Drawing", "Update", UserID, IpAddress, str);
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                    else                      //Add operation
                    {
                        #region --- comented ---
                        //if (!ModelState.IsValid)
                        //{
                        //    BindDropdown();
                        //    //ViewBag.EntityList = new SelectList(new List<tblMasterEntity>(), "EntityID", "EntityName");
                        //    //ViewBag.ActionList = new SelectList(new List<tblApprAction>(), "ActionId", "ActionName");
                        //    var entityDrpList = (from e in dbContext.tblMasterEntities
                        //                         where e.PackageId == objModel.PackageId && e.IsDelete == false
                        //                         select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                        //                         {
                        //                             Id = s.e.EntityID,
                        //                             //Name = s.e.EntityName + " " + s.e.StartChainage + " " + s.e.EndChainage
                        //                             Name = s.e.EntityCode + " " + s.e.EntityName
                        //                         }).ToList();
                        //    ViewBag.EntityList = new SelectList(entityDrpList, "Id", "Name");

                        //    var actions = dbContext.tblApprActions.Where(a => a.ApproverId == objModel.ApprovedById && a.IsDeleted == false).ToList();
                        //    ViewBag.ActionList = new SelectList(actions, "ActionId", "ActionName");
                        //    return View("_DrawingUpdate", objModel);
                        //}
                        #endregion

                        //check is exists
                        //var isexist = dbContext.tblEnggEntDwgApprs
                        //    .Where(e => e.EntityId == objModel.EntityId && e.DwgId == objModel.DrawingTypeId && e.DrawingName== objModel.DrawingName
                        //     && e.AppGateId == objModel.ApprovedById && e.ApprStartDate == SelectedStartDate).FirstOrDefault();

                        var isexist = dbContext.tblEnggEntDwgApprs
                            .Where(e => e.EntityId == objModel.EntityId && e.DwgId == objModel.DrawingTypeId && e.DrawingName == objModel.DrawingName && e.ApprStartDate == SelectedStartDate
                            && e.AppGateId == objModel.ApprovedById && e.ActionId == objModel.ActionId && e.Revision == strRev
                             ).FirstOrDefault();


                        if (isexist != null)
                        {
                            return Json("1", JsonRequestBehavior.AllowGet);
                        }

                        tblEnggEntDwgAppr objAdd = new tblEnggEntDwgAppr();
                        objAdd.EntityId = objModel.EntityId;
                        objAdd.DwgId = objModel.DrawingTypeId;
                        objAdd.DrawingName = objModel.DrawingName == "Select Drawing Name" ? "-" : objModel.DrawingName;
                        objAdd.AppGateId = objModel.ApprovedById;
                        objAdd.ApprStartDate = SelectedStartDate;
                        objAdd.ApprFinsihDate = SelectedStartDate;
                        objAdd.Revision = Convert.ToString(objModel.Revision);
                        objAdd.ActionId = objModel.ActionId;
                        objAdd.Remark = objModel.Remark;
                        objAdd.AttachmentId = attachmentId == 0 ? (Nullable<int>)null : attachmentId;
                        //objAdd.Revision = objModel.IsFinal ? "Rev Final" : GetNextRevision(objModel.EntityId, objModel.DrawingTypeId);

                        dbContext.tblEnggEntDwgApprs.Add(objAdd);
                        dbContext.SaveChanges();
                        dbContext.Entry(objAdd).GetDatabaseValues();
                        drwingId = objAdd.Id;
                        string heading = "";
                        if (objModel.DrawingName == null)
                        {
                            heading = "NA";
                        }
                        else
                        {
                            heading = objModel.DrawingName;
                        }
                        message = "Data added successfully.";
                        int k = Functions.SaveUserLog(pkgId, "Engineering Drawing", "Save", UserID, IpAddress, "Drawing Name:" + heading);

                    }

                    #region --- add multiple attachment link to db 

                    attchList.ForEach(o => o.EnggDwgId = drwingId);
                    dbContext.tblEnggDwgAttachments.AddRange(attchList);
                    dbContext.SaveChanges();
                    #endregion

                    //dbContext.SaveChanges();
                    dbContext.UpdateApprFinishDate(objModel.ApprovedById, SelectedStartDate, objModel.EntityId, objModel.DrawingTypeId);
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Error occured", JsonRequestBehavior.AllowGet);
            }
        }

        public bool CheckIsLastRevToUpdate(int? entityId, int? drawingTypeId, int autoId)
        {
            bool result = false;

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var obj = dbContext.tblEnggEntDwgApprs.Where(e => e.EntityId == entityId && e.DwgId == drawingTypeId).ToList().OrderByDescending(o => o.Id).FirstOrDefault();

                if (obj != null)
                {
                    result = obj.Id == autoId ? true : false;
                }
            }
            return result;
        }

        public bool CheckRevisionFinal(int? drawingId, int? entityId)
        {
            bool isFinal = false;
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                if (dbContext.EntityDrawingApprViews.Where(d => d.DwgId == drawingId && d.Revision == "Rev Final" && d.EntityId == entityId).FirstOrDefault() != null)
                {
                    isFinal = true;
                }
            }
            return isFinal;
        }

        public JsonResult Delete(int id)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var objTodelete = dbContext.tblEnggEntDwgApprs.Where(d => d.Id == id).SingleOrDefault();
                    dbContext.tblEnggEntDwgApprs.Remove(objTodelete);
                    dbContext.SaveChanges();

                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;

                    int k = Functions.SaveUserLog(pkgId, "Engineering Drawing", "Delete", UserID, IpAddress, "Drawing Name:" + Convert.ToString(objTodelete.DrawingName));
                }
                return Json("Data deleted successfully.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error occurred.", JsonRequestBehavior.AllowGet);
            }
        }

        public string GetNextRevision(int? entity, int? drawingId)
        {
            string nextRev = string.Empty;
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                if (entity != null)
                {
                    var obj = dbContext.tblEnggEntDwgApprs.Where(e => e.EntityId == entity && e.DwgId == drawingId).ToList().OrderByDescending(o => o.Id).FirstOrDefault();
                    if (obj != null)
                    {
                        string[] rev = obj.Revision.Split(' ');
                        nextRev = "Rev " + (int.Parse(rev[1]) + 1);
                    }
                    else
                    {
                        nextRev = "Rev 1";
                    }
                }
            }
            return nextRev;
        }

        public JsonResult BindEntityDrpValues(int? pkgId, string text)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {

                    var entityDrpList = (from e in dbContext.tblMasterEntities
                                         where e.PackageId == pkgId && e.SectionID != 0 && e.IsDelete == false
                                         select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                                         {
                                             Id = s.e.EntityID,
                                             Name = s.e.EntityCode + " " + s.e.EntityName
                                         }).ToList();
                    if (!string.IsNullOrEmpty(text))
                    {
                        entityDrpList = entityDrpList.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.Name, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(entityDrpList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //public JsonResult BindEntityDrpValues(string text)
        //{
        //    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
        //    {
        //        try
        //        {

        //            List<EntityMasterModel> obj = new List<EntityMasterModel>();
        //            if (!string.IsNullOrEmpty(text))
        //            {
        //                obj = obj.Where(p =>
        //               CultureInfo.CurrentCulture.CompareInfo.IndexOf
        //               (p.EntityName, text, CompareOptions.IgnoreCase) >= 0).ToList();
        //            }

        //            return Json(obj, JsonRequestBehavior.AllowGet);
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public JsonResult BindActionsDrp(int? id)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var actionDrpList = (from e in dbContext.tblApprActions
                                         where e.ApproverId == id && e.IsDeleted == false
                                         select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                                         {
                                             Id = s.e.ActionId,
                                             Name = s.e.ActionName
                                         }).ToList();
                    return Json(actionDrpList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult Get_DrawingList(string text)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var _DwgList = (from e in dbContext.tblEnggDwgTypes
                                    where e.IsDeleted == false
                                    select new DrpOptionsModel
                                    {
                                        ID = e.DwgId,
                                        Name = e.DwgName
                                    }).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        _DwgList = _DwgList.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.Name, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }

                    return Json(_DwgList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_DrawingNameList()
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var _DwgNameList = (from e in dbContext.tblEnggEntDwgApprs
                                        where e.DrawingName != "-"
                                        select new DrpOptionsModel
                                        {
                                            // ID = e.Id,
                                            Name = e.DrawingName
                                        }).Distinct().ToList();

                    return Json(_DwgNameList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_ApproverList(string text)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var _ApprList = (from e in dbContext.tblEnggApprGates
                                     where e.IsDeleted == false
                                     select new DrpOptionsModel
                                     {
                                         ID = e.ApprGateId,
                                         Name = e.ApprGateName
                                     }).ToList();
                    if (!string.IsNullOrEmpty(text))
                    {
                        _ApprList = _ApprList.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.Name, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(_ApprList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        #region  ---- New changes for multiple file upload ------
        public ActionResult HierarchyBinding_Attachment(int AutoId, [DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                List<DrwAttachment> lst = dbContext.EnggDwgAttachmentViews.Where(o => o.EnggDwgId == AutoId).Select(
                x => new Models.DrwAttachment
                {
                    ListId = (int)x.AutoId,
                    AutoId = (int)x.AutoId,
                    EnggDwgId = (int)x.EnggDwgId,
                    AttachmentID = (int)x.AttachmentID,
                    AttachFileName = x.FileName,
                    AttachFilePath = x.Path

                }).ToList();
                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult DeleteDrwaing(int id)
        {
            try
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var objTodelete = dbContext.tblEnggDwgAttachments.Where(d => d.AutoId == id).SingleOrDefault();
                    var objattach = dbContext.tblAttachments.Where(o => o.AttachmentID == objTodelete.AttachmentID).SingleOrDefault();
                    objattach.IsDeleted = true;
                    objattach.CreatedBy = ((UserModel)Session["UserData"]).UserId;
                    dbContext.tblEnggDwgAttachments.Remove(objTodelete);
                    dbContext.SaveChanges();
                    int k = Functions.SaveUserLog(pkgId, "Engineering Drawing", "Delete", UserID, IpAddress, "FileName" + objattach.FileName);
                }
                return Json("Data deleted successfully.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error occurred.", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        public class EntityDropdownOptions
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}