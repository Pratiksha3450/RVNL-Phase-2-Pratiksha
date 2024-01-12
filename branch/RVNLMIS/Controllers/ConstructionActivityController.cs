using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
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
    public class ConstructionActivityController : Controller
    {
        public string IpAddress = "";

        public ActionResult GetId(int id = 0)
        {
            Session["EntActId"] = id;
            return RedirectToAction("Index");
        }

        // [PageAccessFilter]
        // GET: ConstructionActivity
        public ActionResult Index()
        {
            try
            {
                BindDropdown();

                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;
                int k = Functions.SaveUserLog(pkgId, "Construction Activity- Update data", "View", UserID, IpAddress, "NA");
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {

                    int id = int.Parse(Session["EntActId"].ToString());
                    ConstActivityViewModel getMaterialInfo = GetConstActInfoBlock(dbContext, id);

                    Session["PkgCodeForAttachment"] = getMaterialInfo.PackageCode;

                    return View(getMaterialInfo);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Construction");
            }

        }

        public ConstActivityViewModel GetConstActInfoBlock(dbRVNLMISEntities dbContext, int id)
        {
            //return (from ca in dbContext.tblConsActivities
            //        join cea in dbContext.tblConsEntActs on ca.ConsActId equals cea.ConsActId
            //        join ent in dbContext.tblMasterEntities on cea.EntityId equals ent.EntityID
            //        join pkg in dbContext.tblPackages on ent.PackageId equals pkg.PackageId
            //        where cea.EntActId == id
            //        select new { ca, cea, ent, pkg }).AsEnumerable().Select(s => new ConstActivityViewModel
            //        {
            //            EntityActId = s.cea.EntActId,
            //            PackageName = s.pkg.PackageName,
            //            EntityName = s.ent.EntityName,
            //            ActivityName = s.ca.ActivityName,
            //            ActivityUnit = s.ca.ActUnit,
            //            BudgetedQty = s.cea.BudgQty,
            //            PackageCode = s.pkg.PackageCode
            //        }).FirstOrDefault();

            var result = (from ca in dbContext.ConstructionActivityViews
                              // join cea in dbContext.tblConsEntActs on ca.ConsActId equals cea.ConsActId
                              // join ent in dbContext.tblMasterEntities on cea.EntityId equals ent.EntityID
                          join pkg in dbContext.tblPackages on ca.PackageId equals pkg.PackageId
                          where ca.EntActId == id
                          select new { ca, pkg }).AsEnumerable().Select(s => new ConstActivityViewModel
                          {
                              EntityActId = s.ca.EntActId,
                              PackageName = s.pkg.PackageName,
                              EntityName = s.ca.EntityName,
                              ActivityName = s.ca.ActivityName,
                              ActivityUnit = s.ca.ActUnit,
                              BudgetedQty = s.ca.BudgQty,
                              PackageCode = s.pkg.PackageCode,
                              RevisedQty = Convert.ToDecimal(s.ca.RevisedQty),
                              CompletedQty = Convert.ToDecimal(s.ca.CompletedQtyToDate)
                          }).FirstOrDefault();
            return result;
        }

        [HttpPost]
        public ActionResult RefreshInfoSection(int entActId)
        {
            ConstActivityViewModel getMaterialInfo = new ConstActivityViewModel();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    getMaterialInfo = GetConstActInfoBlock(dbContext, entActId);

                    return View("_ConstructionInfo", getMaterialInfo);
                }

                catch (Exception ex)
                {
                    return View("_ConstructionInfo", getMaterialInfo);
                }
            }
        }

        public ActionResult Read_ExistingConstAct([DataSourceRequest]  DataSourceRequest request, int? consActId)
        {
            List<ConstActivityViewModel> lstProc = new List<ConstActivityViewModel>();
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    if (consActId != null)
                    {
                        lstProc = dbContext.GetExistingConstrEntityAct(consActId).Select(r => new ConstActivityViewModel
                        {
                            ConsTranId = r.Id,
                            TransactionDate = r.TransDate,
                            RevisedQty = Convert.ToDecimal(r.RevisedQty),
                            CompletedQty = Convert.ToDecimal(r.CompletedQty),
                            TargetDate = r.TargetDate,
                            Remark = r.Remark,
                            AttachFileName = r.FileName,
                            AttachFilePath = r.Path
                        }).ToList();
                    }
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
        public ActionResult LoadEditConsTransView(int id)
        {
            ConstActivityViewModel objModel = new ConstActivityViewModel();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    //tblConsEntActTran present = dbContext.tblConsEntActTrans.Where(t => t.TransDate == transactionDate && t.EntActId == entActId).FirstOrDefault();

                    var present = (from t in dbContext.tblConsEntActTrans
                                   join a in dbContext.tblAttachments on t.AttachmentId equals a.AttachmentID into tran
                                   from anull in tran.DefaultIfEmpty()
                                   where t.Id == id
                                   select new { t, anull }).FirstOrDefault();

                    if (present != null)
                    {
                        objModel.ConsTranId = present.t.Id;
                        objModel.EntityActId = (int)present.t.EntActId;
                        objModel.StrTargetDate = present.t.TargetDate == null ? "" : Convert.ToDateTime(present.t.TargetDate).ToString("yyyy-MM-dd");
                        objModel.StrTransDate = Convert.ToDateTime(present.t.TransDate).ToString("yyyy-MM-dd");
                        objModel.TargetDate = present.t.TargetDate;
                        objModel.RevisedQty = Convert.ToDecimal(present.t.RevisedQty);
                        objModel.CompletedQty = Convert.ToDecimal(present.t.CompletedQty);
                        objModel.Remark = present.t.Remark;
                        objModel.AttachFilePath = present.anull == null ? "" : present.anull.Path;
                        objModel.AttachFileName = present.anull == null ? "" : present.anull.FileName;
                        objModel.OperationType = "Update";
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return View("_AddEditConstActTransaction", objModel);
        }

        [Audit]
        [HttpPost]
        public ActionResult SubmitConstAct(ConstActivityViewModel objList)
        {
            string message = string.Empty;
            DateTime? dt = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);
            string packageCode = Session["PkgCodeForAttachment"].ToString();
            int attachmentId = 0;

            try
            {
                if (!ModelState.IsValid)
                {
                    return View("_AddEditConstActTransaction", objList);
                }

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    #region -- ATTACHMENT --
                    DateTime transactionDate = Convert.ToDateTime(objList.StrTransDate);
                    tblConsEntActTran present = dbContext.tblConsEntActTrans.Where(t => t.Id == objList.ConsTranId).FirstOrDefault();

                    if (objList.AttachmentFile != null)
                    {
                        FileInfo fi = new FileInfo(objList.AttachmentFile.FileName);
                        var strings = new List<string> { ".pdf", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".zip" };
                        bool contains = strings.Contains(fi.Extension, StringComparer.OrdinalIgnoreCase);
                        if (!contains)
                        {
                            return Json("0", JsonRequestBehavior.AllowGet);
                        }

                        attachmentId = Functions.AttachmentCommonFun(objList.AttachmentFile, packageCode, "Construction", "Construction Transaction", present);
                    }

                    #endregion

                    if (present != null)   //update values
                    {
                        present.TransDate = transactionDate;
                        present.RevisedQty = Convert.ToDouble(objList.RevisedQty);
                        present.CompletedQty = Convert.ToDouble(objList.CompletedQty);
                        present.Remark = objList.Remark;
                        present.AttachmentId = attachmentId == 0 ? present.AttachmentId : attachmentId;
                        present.TargetDate = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);                       
                        dbContext.SaveChanges();
                        message = "Data updated successfully.";
                        IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (string.IsNullOrEmpty(IpAddress))
                        {
                            IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                        }
                        int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                        int UserID = ((UserModel)Session["UserData"]).UserId;
                       
                        try
                        {
                            string str = ""; ;
                            var UpdatedValue = (from ul in dbContext.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                            if (UpdatedValue != null)
                            {
                                string TrDate = transactionDate.Date.ToString("dd-MMM-yyyy");
                                str = "Transaction Date: "+ TrDate + "= " + UpdatedValue;                                
                            }
                            else
                            {
                                str = "NA";
                            }

                            int k = Functions.SaveUserLog(pkgId, "Construction Activity- Update Data", "Update", UserID, IpAddress, str);
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                    else                   //add values
                    {
                        tblConsEntActTran objAdd = new tblConsEntActTran();
                        objAdd.TransDate = transactionDate;
                        objAdd.CompletedQty = Convert.ToDouble(objList.CompletedQty);
                        objAdd.EntActId = objList.entityActvity;
                        objAdd.Remark = objList.Remark;
                        objAdd.RevisedQty = Convert.ToDouble(objList.RevisedQty);
                        objAdd.AttachmentId = attachmentId == 0 ? (Nullable<int>)null : attachmentId;
                        objAdd.TargetDate = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);
                        dbContext.tblConsEntActTrans.Add(objAdd);
                        dbContext.SaveChanges();
                        message = "Data added successfully.";

                        IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (string.IsNullOrEmpty(IpAddress))
                        {
                            IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                        }
                        int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                        int UserID = ((UserModel)Session["UserData"]).UserId;
                        string TrDate = transactionDate.Date.ToString("dd-MMM-yyyy");
                     
                        int k = Functions.SaveUserLog(pkgId, "Construction Activity- Update data", "Save", UserID, IpAddress, "Transaction Date:" + TrDate);
                    }
                    
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Error occured", JsonRequestBehavior.AllowGet);
            }
        }

        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var packages = Functions.GetRoleAccessiblePackageList();
                    ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");

                    if (packages.Count == 1)
                    {
                        int pkgId = packages[0].PackageId;
                        var entity = dbContext.tblMasterEntities.Where(e => e.PackageId == pkgId && e.IsDelete == false).Select(s => new
                        {
                            EntityId = s.EntityID,
                            EntityName = s.EntityCode + " " + s.EntityName
                        }).ToList();
                        ViewBag.EntityList = new SelectList(entity, "EntityID", "EntityName");
                    }
                    else
                    {
                        ViewBag.EntityList = new SelectList(new List<tblMasterEntity>(), "EntityID", "EntityName");
                    }

                    var activity = dbContext.tblConsActivities.ToList();
                    ViewBag.ActivityList = new SelectList(activity, "ConsActId", "ActivityName");
                }
            }
            catch (Exception ex)
            {

            }
        }

        [HttpPost]
        public JsonResult DeleteTransaction(int id)
        {
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {
                    tblConsEntActTran objToDelete = db.tblConsEntActTrans.FirstOrDefault(o => o.Id == id);

                    #region --Delete Attachment --

                    if (objToDelete.AttachmentId != null)
                    {
                        db.tblAttachments.Remove(db.tblAttachments.Where(a => a.AttachmentID == objToDelete.AttachmentId).FirstOrDefault());
                        // dbContext.SaveChanges();
                    }
                    #endregion

                    db.tblConsEntActTrans.Remove(objToDelete);
                    db.SaveChanges();

                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    DateTime trDT = (DateTime)objToDelete.TransDate;
                    string TrDate = trDT.Date.ToString("dd-MMM-yyyy");
                    int k = Functions.SaveUserLog(pkgId, "Construction Activity- Update data", "Delete", UserID, IpAddress, "Transaction Date:" + TrDate);
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