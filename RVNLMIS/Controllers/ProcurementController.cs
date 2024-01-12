using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
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
    public class ProcurementController : Controller
    {
        public string IpAddress = "";
        // GET: Procurement
        [PageAccessFilter]
        public ActionResult Index()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var packages = Functions.GetRoleAccessiblePackageList();
                ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");

                string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                int dispId = ((UserModel)Session["UserData"]).Discipline;

                if (roleCode == "DISP")
                {
                    var discipline = dbContext.tblDisciplines.Where(d => d.DispId == dispId && d.IsDeleted == false).ToList();
                    ViewBag.DisciplineList = new SelectList(discipline, "DispId", "DispCode", discipline.First().DispId);
                }
                else
                {
                    var discipline = dbContext.tblDisciplines.Where(d => d.IsDeleted == false).ToList();
                    ViewBag.DisciplineList = new SelectList(discipline, "DispId", "DispCode");
                }
            }
            Session["PkgCodeForAttachment"] = "";

            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Procurement- Update Data", "View", UserID, IpAddress, "NA");

            return View();
        }

        #region --- List Procurement Values ---

        public ActionResult Procurement_Read([DataSourceRequest]  DataSourceRequest request, string package, string disp)
        {
            List<ProcurementViewModel> lstProc = new List<ProcurementViewModel>();
            string whereCond = string.Empty;

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    if (!string.IsNullOrEmpty(package) && string.IsNullOrEmpty(disp))
                    {
                        whereCond = " WHERE PackageId=" + package;
                    }
                    else if (string.IsNullOrEmpty(package) && !string.IsNullOrEmpty(disp))
                    {
                        whereCond = " WHERE DispId=" + disp;
                    }
                    else if (!string.IsNullOrEmpty(package) && !string.IsNullOrEmpty(disp))
                    {
                        whereCond = " WHERE PackageId=" + package + " AND DispId=" + disp;
                    }

                    lstProc = dbContext.GetProcurementIndexList(whereCond).Select(r => new ProcurementViewModel
                    {
                        PackMatId = r.PkgMatId,
                        PackageId = r.PackageId,
                        DispId = r.DispId,
                        Discipline = r.Discipline,
                        MaterialName = r.MaterialName,
                        MaterialUnit = r.MaterialUnit,
                        OriginalQty = Convert.ToDecimal( r.OriginalQty),
                        CumDeliveredQty = r.CumDeliveredQty,
                        CumOrderdQty = r.CumOrderedQty,
                        RatePerUnit = Convert.ToString(r.RatePerUnit),
                        RevisedQty = (decimal)r.RevisedQty

                    }).ToList();

                    return Json(lstProc.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(lstProc.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion

        #region --- Edit Procurement form methods ---

        public ActionResult GetId(int id = 0)
        {
            Session["ProcId"] = id;
            return RedirectToAction("EditProcurement");
        }

        /// <summary>
        /// Get method to load view for material info, edit procurement and exististing procurement list for package material
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Audit]
        public ActionResult EditProcurement()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    int id = int.Parse(Session["ProcId"].ToString());
                    EditProcurementModel getMaterialInfo = GetMaterialInfoBlock(dbContext, id);

                    Session["PkgCodeForAttachment"] = getMaterialInfo.PackageCode;

                    return View(getMaterialInfo);
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Index", "Procurement");
                }
            }
        }

        public EditProcurementModel GetMaterialInfoBlock(dbRVNLMISEntities dbContext, int id)
        {
            //return (from m in dbContext.tblProcMaterials
            //        join pkm in dbContext.tblProcPkgMaterials on m.MaterialId equals pkm.MaterialId
            //        join p in dbContext.tblPackages on pkm.PackageId equals p.PackageId
            //        where pkm.PkgMatId == id
            //        select new { pkm, m, p }).AsEnumerable().Select(s => new EditProcurementModel
            //        {
            //            PckMatId = s.pkm.PkgMatId,
            //            PackageName = s.p.PackageName,
            //            MaterialName = s.m.MaterialName,
            //            RatePerUnit = Convert.ToString(s.pkm.RatePerUnit),
            //            OriginalQty = Convert.ToString(s.pkm.OriginalQty),
            //            PackageCode = s.p.PackageCode
            //        }).FirstOrDefault();

            var result = (from m in dbContext.C100ProcForm
                              // join pkm in dbContext.tblProcPkgMaterials on m.MaterialId equals pkm.MaterialId
                          join p in dbContext.tblPackages on m.PackageId equals p.PackageId
                          where m.PkgMatId == id
                          select new { m, p }).AsEnumerable().Select(s => new EditProcurementModel
                          {
                              PckMatId = s.m.PkgMatId,
                              PackageName = s.p.PackageName,
                              MaterialName = s.m.MaterialName,
                              RatePerUnit = Convert.ToString(s.m.RatePerUnit) ==""? "-": Convert.ToString(s.m.RatePerUnit),
                              OriginalQty = Convert.ToString(s.m.OriginalQty) == "" ? "0.0" : Convert.ToString(s.m.OriginalQty),
                              OrderedQty = Convert.ToString(s.m.CumOrderedQty) == "" ? "0.0" : Convert.ToString(s.m.CumOrderedQty),
                              DeliveredQty = Convert.ToString(s.m.CumDeliveredQty) == "" ? "0.0" : Convert.ToString(s.m.CumDeliveredQty),
                              RevisedQty = Convert.ToString(s.m.RevisedQty) == "" ? "0.0" : Convert.ToString(s.m.RevisedQty),
                              PackageCode = s.p.PackageCode
                          }).FirstOrDefault();
            return result;
        }

        [HttpPost]
        public ActionResult RefreshInfoSection(int pkgMatId)
        {
            EditProcurementModel getMaterialInfo = new EditProcurementModel();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    getMaterialInfo = GetMaterialInfoBlock(dbContext, pkgMatId);

                    return View("_MaterialInfo", getMaterialInfo);
                }

                catch (Exception ex)
                {
                    return View("_MaterialInfo", getMaterialInfo);
                }
            }
        }


        /// <summary>
        /// method to get existing records for package material
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PkgMatId"></param>
        /// <returns></returns>
        public ActionResult ReadExistingProcurementForMaterial([DataSourceRequest]  DataSourceRequest request, int PkgMatId)
        {
            List<ExistingProcList> lstProc = new List<ExistingProcList>();
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    lstProc = dbContext.GetExistingProcForPckgMaterial(PkgMatId).Select(r => new ExistingProcList
                    {
                        PackMatTransId = r.PkgMatTranId,
                        TransDate = r.TransDate,
                        MaterialCode = r.MaterialCode,
                        MaterialUnit = r.MaterialUnit,
                        RevisedQty = r.RevisedQty,
                        OrderedQty = r.OrderedQty,
                        DeliveredQty = r.DeliveredQty,
                        SupplierName = r.SupplierName,
                        PORef = r.PORef,
                        TargetDate = r.TargetDate,
                        Remark = r.Remark,
                        RatePerUnit = Convert.ToDecimal(r.RatePerUnit),
                        AttachFilePath = r.Path,
                        AttachFileName = r.FileName
                    }).ToList();
                    return Json(lstProc.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(lstProc.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// method to load edit info partial view
        /// </summary>
        /// <param name="transactionDate"></param>
        /// <param name="pkgMatId"></param>
        /// <returns></returns>

        [Audit]
        public ActionResult LoadEditProcTransView(int id)
        {
            ExistingProcList objModel = new ExistingProcList();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var present = (from t in dbContext.tblPkgMatTransactions
                                   join a in dbContext.tblAttachments on t.AttachmentId equals a.AttachmentID into tran
                                   from anull in tran.DefaultIfEmpty()
                                   where t.PkgMatTranId == id
                                   select new { t, anull }).FirstOrDefault();

                    //dbContext.tblPkgMatTransactions.Where(t => t.TransDate == transactionDate && t.PkgMatId == pkgMatId).FirstOrDefault();

                    if (present != null) //update
                    {
                        objModel.PackMatTransId = present.t.PkgMatTranId;
                        objModel.PackMatId = (int)present.t.PkgMatId;
                        objModel.StrTargetDate = present.t.TargetDate == null ? "" : Convert.ToDateTime(present.t.TargetDate).ToString("yyyy-MM-dd");
                        objModel.StrTransDate = Convert.ToDateTime(present.t.TransDate).ToString("yyyy-MM-dd");
                        objModel.TargetDate = present.t.TargetDate;
                        objModel.RevisedQty = present.t.RevisedQty;
                        objModel.SupplierName = present.t.SupplierName;
                        objModel.PORef = present.t.PORef;
                        objModel.DeliveredQty = present.t.DeliveredQty ?? 0;
                        objModel.Remark = present.t.Remark;
                        objModel.OrderedQty = present.t.OrderedQty;
                        objModel.AttachFilePath = present.anull == null ? "" : present.anull.Path;
                        objModel.AttachFileName = present.anull == null ? "" : present.anull.FileName;
                        objModel.OperationType = "Update";
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return View("_EditAddProcOnDateSelect", objModel);
        }

        /// <summary>
        /// method to add or edit values in tblPkgMatTransaction 
        /// </summary>
        /// <param name="objList"></param>
        /// <returns></returns>
        [HttpPost]
        [Audit]
        public ActionResult SubmitProc(ExistingProcList objList)
        {
            string message = string.Empty;
            DateTime? dt = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);
            string packageCode = Session["PkgCodeForAttachment"].ToString();
            // int pkgTransId = 0;
            int attachmentId = 0;

            try
            {
                //if (objList.OrderedQty > objList.RevisedQty)
                //{
                //    return Json("1", JsonRequestBehavior.AllowGet);   ///Ordered Qty should not be greater than Revised Qty
                //}

                //if (objList.DeliveredQty > objList.OrderedQty)
                //{
                //    return Json("2", JsonRequestBehavior.AllowGet);   /// Delivered Qty should not be greater than Ordered Qty
                //}

                if (!ModelState.IsValid)
                {
                    return View("_EditAddProcOnDateSelect", objList);
                }
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    #region -- ATTACHMENT --

                    DateTime transactionDate = Convert.ToDateTime(objList.StrTransDate);
                    tblPkgMatTransaction present = dbContext.tblPkgMatTransactions.Where(t => t.PkgMatTranId == objList.PackMatTransId).FirstOrDefault();

                    if (objList.AttachmentFile != null)
                    {
                        FileInfo fi = new FileInfo(objList.AttachmentFile.FileName);
                        var strings = new List<string> { ".pdf", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".zip" };
                        bool contains = strings.Contains(fi.Extension, StringComparer.OrdinalIgnoreCase);
                        if (!contains)
                        {
                            return Json("0", JsonRequestBehavior.AllowGet);
                        }

                        //var allowedExtensions = new[] { ".pdf, .xls, .xlsx, .png, .jpg, .jpeg" };
                        //var extension = Path.GetExtension(objList.AttachmentFile.FileName);
                        //if (!allowedExtensions.Contains(extension))
                        //{
                        //    // Not allowed
                        //    return Json("0", JsonRequestBehavior.AllowGet);
                        //}

                        attachmentId = Functions.AttachmentCommonFun(objList.AttachmentFile, packageCode, "Material", "Procurement Transaction", present);
                    }

                    #endregion

                    //DateTime transactionDate = Convert.ToDateTime(objList.StrTransDate);
                    //tblPkgMatTransaction present = dbContext.tblPkgMatTransactions.Where(t => t.TransDate == transactionDate && t.PkgMatId == objList.PackMatId).FirstOrDefault();

                    if (present != null)   //update values
                    {
                        present.TransDate = transactionDate;
                        present.DeliveredQty = objList.DeliveredQty;
                        present.OrderedQty = objList.OrderedQty;
                        present.PORef = objList.PORef;
                        present.Remark = objList.Remark;
                        present.RevisedQty = objList.RevisedQty;
                        present.SupplierName = objList.SupplierName;
                        present.AttachmentId = attachmentId == 0 ? present.AttachmentId : attachmentId;
                        present.TargetDate = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);
                        dbContext.SaveChanges();
                        message = "Data updated successfully.";


                        try
                        {
                            string str = ""; ;
                            var UpdatedValue = (from ul in dbContext.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                            if (UpdatedValue != null)
                            {
                                string TrDate = transactionDate.Date.ToString("dd-MMM-yyyy");
                                str = "Transaction Date: " + TrDate + "= " + UpdatedValue;
                            }
                            else
                            {
                                str = "NA";
                            }

                            int k = Functions.SaveUserLog(pkgId, "Procurement- Update Data", "Update", UserID, IpAddress, str);
                        }
                        catch (Exception ex)
                        {

                        }


                    }
                    else                   //add values
                    {
                        tblPkgMatTransaction objAdd = new tblPkgMatTransaction();
                        objAdd.TransDate = transactionDate;
                        objAdd.DeliveredQty = objList.DeliveredQty;
                        objAdd.OrderedQty = objList.OrderedQty;
                        objAdd.PkgMatId = objList.PackMatId;
                        objAdd.PORef = objList.PORef;
                        objAdd.Remark = objList.Remark;
                        objAdd.RevisedQty = objList.RevisedQty;
                        objAdd.SupplierName = objList.SupplierName;
                        objAdd.AttachmentId = attachmentId == 0 ? (Nullable<int>)null : attachmentId;
                        objAdd.TargetDate = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);
                        dbContext.tblPkgMatTransactions.Add(objAdd);
                        dbContext.SaveChanges();
                        message = "Data added successfully.";
                        string TrDate = transactionDate.Date.ToString("dd-MMM-yyyy");
                        int k = Functions.SaveUserLog(pkgId, "Procurement- Update Data", "Save", UserID, IpAddress, "Transaction Date:" + TrDate);
                        // pkgTransId = objAdd.PkgMatTranId;
                    }

                    return Json(message, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

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

        public JsonResult ServerFiltering_GetProducts(string text)
        {
            List<GetRoleAssignedPackageList_Result> sessionPackages = Functions.GetRoleAccessiblePackageList();
            if (!string.IsNullOrEmpty(text))
            {
                sessionPackages = sessionPackages.Where(p =>
               CultureInfo.CurrentCulture.CompareInfo.IndexOf
               (p.PackageName, text, CompareOptions.IgnoreCase) >= 0).ToList(); 
            }
            return Json(sessionPackages, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ServerFiltering_GetDiscipline(string text)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                int dispId = ((UserModel)Session["UserData"]).Discipline;

                if (roleCode == "DISP")
                {
                    var discipline = dbContext.tblDisciplines.Where(d => d.DispId == dispId && d.IsDeleted == false).Select(s => new { s.DispId, s.DispCode }).ToList();
                    return Json(discipline, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var discipline = dbContext.tblDisciplines.Where(d => d.IsDeleted == false)
                        .Select(s => new { s.DispId, s.DispCode }).ToList();
                    return Json(discipline, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult DeleteTransaction(int id)
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
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {
                    tblPkgMatTransaction objToDelete = db.tblPkgMatTransactions.FirstOrDefault(o => o.PkgMatTranId == id);

                    #region --Delete Attachment --

                    if (objToDelete.AttachmentId != null)
                    {
                        db.tblAttachments.Remove(db.tblAttachments.Where(a => a.AttachmentID == objToDelete.AttachmentId).FirstOrDefault());
                        // dbContext.SaveChanges();
                    }
                    #endregion

                    db.tblPkgMatTransactions.Remove(objToDelete);
                    db.SaveChanges();
                    DateTime trDT = (DateTime)objToDelete.TransDate;
                    string TrDate = trDT.Date.ToString("dd-MMM-yyyy");

                    int k = Functions.SaveUserLog(pkgId, "Procurement- Update Data", "Delete", UserID, IpAddress, "Transaction Date:" + TrDate);


                }
                return Json("1");
            }
            catch
            {
                return Json("-1");
            }
        }
    }
}