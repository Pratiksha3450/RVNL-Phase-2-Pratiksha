
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
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

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class InvoicePaymentController : Controller
    {
        public string IpAddress = "";
        // GET: Invoice  InvoicePayment
        [Audit]
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
            int k = Functions.SaveUserLog(pkgId, "Invoice", "View", UserID, IpAddress, "NA");
            GetPackageList();
            string _TotalPaid = "";
            using (var db = new dbRVNLMISEntities())
            {
                if (((UserModel)Session["UserData"]).RoleId == 600)
                {

                    var pay = (from a in db.tblInvoices
                               join b in db.tblInvoicePayments on a.InvoiceId equals b.InvoiceId
                               where a.PackageId == pkgId && a.IsDeleted == false && b.IsDeleted == false
                               select new { b }).ToList();
                    _TotalPaid = pay.Count > 0 ? pay.Sum(x => x.b.PaidAmount).ToString() : "";
                }
            }
            ViewBag.TotalPaid = _TotalPaid;
            return View();
        }
        [HttpPost]
        public JsonResult GetTotalInvoice()
        {
            string _TotalPaid = "";
            using (var db = new dbRVNLMISEntities())
            {
                if (((UserModel)Session["UserData"]).RoleId == 600)
                {
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    var pay = (from a in db.tblInvoices
                               join b in db.tblInvoicePayments on a.InvoiceId equals b.InvoiceId
                               where a.PackageId == pkgId
                               select new { b }).ToList();
                    _TotalPaid = pay.Count > 0 ? pay.Sum(x => x.b.PaidAmount).ToString() : "";
                }
            }
            ViewBag.TotalPaid = _TotalPaid;
            return Json(_TotalPaid, JsonRequestBehavior.AllowGet);
        }

        [Audit]
        public ActionResult Create()
        {
            InvoicePaymentDetail objModelView = new InvoicePaymentDetail();
            GetPackageList();
            return View("_PartialAddEditInvoiceDetails", objModelView);
        }
        public JsonResult GetPackageUnderProject(int? id)
        {
            try
            {
                var _PackageList = GetPackages(id);

                return Json(_PackageList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        #region -- Load Package List --
        public void GetPackageList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var projects = Functions.GetroleAccessibleProjectsList();
                ViewBag.ProjectList = new SelectList(projects, "ProjectId", "ProjectName");

                // var pkgs = Functions.GetRoleAccessiblePackageList();
                ViewBag.PackageList = new SelectList(new List<tblPackage>(), "PackageId", "PackageName");
            }
        }
        #endregion

        public List<PackageModel> GetPackages(int? projectId)
        {
            List<PackageModel> _PackageList = new List<PackageModel>();
            string roleCode = ((UserModel)Session["UserData"]).RoleCode;
            int userId = ((UserModel)HttpContext.Session["UserData"]).UserId;
            int roleId = ((UserModel)HttpContext.Session["UserData"]).RoleId;


            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                if (roleCode == "PKG")
                {
                    _PackageList = dbContext.GetRoleAssignedPackageList(userId, roleId).Select(s => new PackageModel
                    {
                        PackageId = s.PackageId,
                        PackageName = s.PackageName
                    }).ToList();
                }
                else
                {
                    _PackageList = (from e in dbContext.tblPackages
                                    where (e.ProjectId == projectId && e.IsDeleted == false)
                                    select new PackageModel
                                    {
                                        PackageId = e.PackageId,
                                        PackageName = e.PackageCode + " - " + e.PackageName
                                    }).ToList();
                }
                return _PackageList;
            }
        }

        public JsonResult ServerFiltering_GetProducts(string text)
        {
            List<GetRoleAssignedProjectList_Result> sessionProjects = Functions.GetroleAccessibleProjectsList();
            if (!string.IsNullOrEmpty(text))
            {
                // sessionProjects = sessionProjects.Where(p => p.ProjectName.Contains(text, StringComparer.InvariantCultureIgnoreCase)).ToList();
                sessionProjects = sessionProjects.Where(p =>
             CultureInfo.CurrentCulture.CompareInfo.IndexOf
             (p.ProjectName, text, CompareOptions.IgnoreCase) >= 0).ToList(); ;
            }
            return Json(sessionProjects, JsonRequestBehavior.AllowGet);
        }


        #region --- List Invoice Details Values ---
        public ActionResult InvoiceDetails_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                var pkgs = Functions.GetRoleAccessiblePackageList();
                var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();

                var lst = (from x in db.InvoicePaymentViewDetails
                           select new InvoicePaymentDetail
                           {
                               ProjectId = (int)x.ProjectId,
                               ProjectName = x.ProjectName,
                               InvoiceId = x.InvoiceId,
                               PackageId = (int)x.PackageId,
                               PackageName = x.PackageName,
                               InvoiceNo = x.InvoiceNo,
                               InvoiceAmount = x.InvoiceAmount,
                               InvoiceDate = x.InvoiceDate,
                               CertifiedAmount = x.CertifiedAmount,
                               CreatedOn = (DateTime)x.CreatedOn,
                               IsDeleted = x.IsDeleted,
                           }).OrderByDescending(o => o.InvoiceId).ToList();

                lst = lst.Where(w => accessiblePackageList.Contains(w.PackageId)).ToList();



                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region --- List Resource Values ---
        public ActionResult HierarchyBinding_Sub(int InvoiceId, [DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    _ = new List<InvoicePayment>();
                    List<InvoicePayment> lst = dbContext.tblInvoicePayments.Where(o => o.IsDeleted == false && o.InvoiceId == InvoiceId).Select(
                    x => new Models.InvoicePayment
                    {
                        PaymentId = x.PaymentId,
                        InvoiceId = (int)x.InvoiceId,
                        InvoiceNo = dbContext.tblInvoices.Where(o => o.InvoiceId == (int)x.InvoiceId).Select(o => o.InvoiceNo).FirstOrDefault(),
                        PaidAmount = x.PaidAmount,
                        PaymentDate = x.PaymentDate,
                        Remark=x.Remark,
                        IsDeleted = x.IsDeleted,
                        IsPaid = (bool)x.IsPaid,
                        AttachFilePath = dbContext.tblAttachments.Where(o => o.AttachmentID == (int)x.AttachmentId).Select(o => o.Path).FirstOrDefault(),
                        AttachFileName = dbContext.tblAttachments.Where(o => o.AttachmentID == (int)x.AttachmentId).Select(o => o.FileName).FirstOrDefault(),
                        CreatedOn = (DateTime)x.CreatedOn,

                    }).OrderByDescending(c => c.CreatedOn).ToList();
                    //lst = lst.ToList();
                    return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);


                    // return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                throw;
            }

        }
        #endregion

        #region -- Add and Update Invoice Details --
        [Audit]
        [HttpPost]
        public ActionResult AddInvoiceDetails(InvoicePaymentDetail oModel)
        {
            GetPackageList();
            int InvoiceId = oModel.InvoiceId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    oModel.InvoiceAmount = Convert.ToDecimal(oModel.InvoiceAmountString.Replace(",", ""));
                    oModel.CertifiedAmount = Convert.ToDecimal(oModel.CertifiedAmountString.Replace(",", ""));
                    if (InvoiceId == 0)
                    {
                        DateTime dt = Convert.ToDateTime(oModel.InvoiceDates);
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblInvoices.Where(u => u.InvoiceNo == oModel.InvoiceNo && u.IsDeleted == false && u.InvoiceDate == dt && u.InvoiceAmount == oModel.InvoiceAmount && u.CertifiedAmount == oModel.CertifiedAmount).SingleOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                                return Json(message, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                tblInvoice objInvoice = new tblInvoice();
                                objInvoice.ProjectId = oModel.ProjectId;
                                objInvoice.PackageId = oModel.PackageId;      // Add Package Dropdown
                                objInvoice.InvoiceNo = oModel.InvoiceNo;
                                objInvoice.InvoiceDate = (oModel.InvoiceDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.InvoiceDates);
                                objInvoice.InvoiceAmount = (oModel.InvoiceAmount == null) ? 00 : oModel.InvoiceAmount;
                                objInvoice.CertifiedAmount = (oModel.CertifiedAmount == null) ? 00 : oModel.CertifiedAmount;
                                objInvoice.IsDeleted = false;
                                objInvoice.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                objInvoice.IsPaid = false;
                                objInvoice.Remark = "-";
                                db.tblInvoices.Add(objInvoice);
                                db.SaveChanges();
                                message = "Added Successfully";
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                int k = Functions.SaveUserLog(pkgId, "Invoice", "Save", UserID, IpAddress, "Invoice No.:" + oModel.InvoiceNo);
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            DateTime dt = Convert.ToDateTime(oModel.InvoiceDates);
                            var exist = db.tblInvoices.Where(u => u.InvoiceNo == oModel.InvoiceNo && u.IsDeleted == false && u.InvoiceDate == dt && u.InvoiceId != oModel.InvoiceId && u.InvoiceAmount == oModel.InvoiceAmount && u.CertifiedAmount == oModel.CertifiedAmount).SingleOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblInvoice objInvoice = db.tblInvoices.Where(u => u.InvoiceId == oModel.InvoiceId).SingleOrDefault();
                                objInvoice.ProjectId = oModel.ProjectId;
                                objInvoice.PackageId = oModel.PackageId;      // Add Package Dropdown
                                objInvoice.InvoiceNo = oModel.InvoiceNo;
                                objInvoice.InvoiceAmount = oModel.InvoiceAmount;
                                objInvoice.CertifiedAmount = oModel.CertifiedAmount;
                                objInvoice.InvoiceDate = (oModel.InvoiceDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.InvoiceDates);
                                objInvoice.IsDeleted = false;
                                objInvoice.CreatedOn = DateTime.UtcNow.AddHours(5.5);

                                db.SaveChanges();
                                message = "Updated Successfully";
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
                                    var UpdatedValue = (from ul in db.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                                    if (UpdatedValue != null)
                                    {
                                        str = UpdatedValue.Replace("CreatedOn, ", "");
                                        str = str.Replace("CreatedOn", "");
                                        str = str.TrimStart(',');
                                        str = str.Replace(", ,", ",");
                                        str = str.TrimEnd(',');
                                        if (String.IsNullOrEmpty(str))
                                        {
                                            str = "Invoice No.: " + oModel.InvoiceNo + "= " + "NA";
                                        }
                                        else
                                        {
                                            str = "Invoice No.: " + oModel.InvoiceNo + "= " + str;
                                        }


                                    }
                                    else
                                    {
                                        str = "NA";
                                    }

                                    int k = Functions.SaveUserLog(pkgId, "Invoice", "Update", UserID, IpAddress, str);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                    ModelState.Clear();
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    #region -- Bind Dropdowns --
                    using (var db = new dbRVNLMISEntities())
                    {
                        var projects = Functions.GetroleAccessibleProjectsList();
                        ViewBag.ProjectList = new SelectList(projects, "ProjectId", "ProjectName");

                        var pkgs = GetPackages(oModel.ProjectId);
                        ViewBag.PackageList = new SelectList(pkgs, "PackageId", "PackageName");
                        #endregion
                    }
                    oModel = new InvoicePaymentDetail();
                    return View("_PartialAddEditInvoiceDetails", oModel);
                }
            }
            catch (Exception ex)
            {
                oModel = new InvoicePaymentDetail();
                return View("_PartialAddEditInvoiceDetails", oModel);
            }
        }
        #endregion

        #region -- Edit Invoice Details --
        [Audit]
        public ActionResult EditInvoiceByInvoiceId(int id)
        {
            GetPackageList();
            int resourceId = id;
            InvoicePaymentDetail objModel = new InvoicePaymentDetail();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var oInvoiceDetails = db.tblInvoices.Where(o => o.InvoiceId == id).SingleOrDefault();
                        if (oInvoiceDetails != null)
                        {
                            objModel.ProjectId = (oInvoiceDetails.ProjectId == null) ? 0 : (int)oInvoiceDetails.ProjectId; ;
                            objModel.InvoiceId = oInvoiceDetails.InvoiceId;
                            objModel.PackageId = (int)oInvoiceDetails.PackageId;
                            objModel.InvoiceNo = oInvoiceDetails.InvoiceNo;
                            objModel.InvoiceDate = Convert.ToDateTime(oInvoiceDetails.InvoiceDate);
                            objModel.InvoiceDates = string.IsNullOrEmpty(Convert.ToString(oInvoiceDetails.InvoiceDate)) ? "" : Convert.ToDateTime(Convert.ToString(oInvoiceDetails.InvoiceDate)).ToString("yyyy-MM-dd");
                            objModel.InvoiceAmount = oInvoiceDetails.InvoiceAmount;
                            objModel.CertifiedAmount = (oInvoiceDetails.CertifiedAmount);
                            objModel.InvoiceAmountString = string.Format("{0:0.00}", oInvoiceDetails.InvoiceAmount); // Math.Round(Convert.ToDouble(oInvoiceDetails.InvoiceAmount)).ToString();//
                            objModel.CertifiedAmountString = string.Format("{0:0.00}", oInvoiceDetails.CertifiedAmount);// Math.Round(Convert.ToDouble(oInvoiceDetails.CertifiedAmount)).ToString(); // 
                        }

                        var packagesForProject = GetPackages(objModel.ProjectId);
                        ViewBag.PackageList = new SelectList(packagesForProject, "PackageId", "PackageName");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialAddEditInvoiceDetails", objModel);
        }
        #endregion

        public ActionResult GetPayDetailsByPaymentId(int id)
        {
            InvoicePayment objModel = new InvoicePayment();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var present = (from t in db.tblInvoicePayments
                                       join a in db.tblAttachments on t.AttachmentId equals a.AttachmentID into tran
                                       from anull in tran.DefaultIfEmpty()
                                       where t.PaymentId == id
                                       select new { t, anull }).FirstOrDefault();
                        var oInvoiceDetails = db.tblInvoicePayments.Where(o => o.PaymentId == id && o.IsDeleted == false).FirstOrDefault();
                        if (oInvoiceDetails != null)
                        {
                            objModel.PaymentDates = string.IsNullOrEmpty(Convert.ToString(oInvoiceDetails.PaymentDate)) ? DateTime.Now.ToString("yyyy-MM-dd") : Convert.ToDateTime(Convert.ToString(oInvoiceDetails.PaymentDate)).ToString("yyyy-MM-dd");
                            objModel.PaymentId = oInvoiceDetails.PaymentId;
                            objModel.InvoiceId = (int)oInvoiceDetails.InvoiceId;
                            objModel.PaymentDate = Convert.ToDateTime(oInvoiceDetails.PaymentDate);
                            objModel.PaidAmount = oInvoiceDetails.PaidAmount;
                            objModel.IsPaid = (bool)oInvoiceDetails.IsPaid;
                            objModel.Remark = oInvoiceDetails.Remark;

                            objModel.AttachFilePath = present.anull == null ? "" : present.anull.Path;
                            objModel.AttachFileName = present.anull == null ? "" : present.anull.FileName;
                            objModel.PaidAmountString = string.Format("{0:0.00}", oInvoiceDetails.PaidAmount); //Math.Round(Convert.ToDouble(oInvoiceDetails.PaidAmount)).ToString();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialAddEditPaymentDetails", objModel);
        }

        public ActionResult AddInvoicePayByInvoiceId(int id)
        {
            InvoicePayment objModel = new InvoicePayment();
            objModel.InvoiceId = id;
            return View("_PartialAddEditPaymentDetails", objModel);
        }

        [Audit]
        [HttpPost]
        public ActionResult AddInvoicePaymentDetails(InvoicePayment objModel)
        {
            try
            {
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                string message = string.Empty;
                int attachmentId = 0;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        DateTime PaymentDates = Convert.ToDateTime(objModel.PaymentDates);

                        tblInvoicePayment present = db.tblInvoicePayments.Where(t => t.PaymentDate == PaymentDates && t.PaymentId == objModel.PaymentId).FirstOrDefault();

                        if (objModel.AttachmentFile != null)
                        {
                            FileInfo fi = new FileInfo(objModel.AttachmentFile.FileName);
                            var strings = new List<string> { ".pdf" };
                            bool contains = strings.Contains(fi.Extension, StringComparer.OrdinalIgnoreCase);

                            if (!contains)
                            {
                                return Json("0", JsonRequestBehavior.AllowGet);
                            }

                            var oInvoiceDetails = db.tblInvoices.Where(t => t.InvoiceId == objModel.InvoiceId).Select(o => o.InvoiceNo).FirstOrDefault();
                            objModel.InvoiceNo = oInvoiceDetails;
                            attachmentId = Functions.AttachmentCommonFun(objModel.AttachmentFile, objModel.InvoiceNo, "PaymentInvoice", "Invoice Payment", present);
                        }
                        objModel.PaidAmount = Convert.ToDecimal(objModel.PaidAmountString.Replace(",", ""));
                        var PackageId = db.tblInvoices.Where(o => o.InvoiceId == objModel.InvoiceId).Select(o => o.PackageId).FirstOrDefault();
                        var BalanceValue = db.tblPackages.Where(o => o.PackageId == PackageId).Select(o => o.BalanceValue).FirstOrDefault();
                        //if (Functions.ParseInteger(Convert.ToString(BalanceValue)) >= objModel.PaidAmount)
                           if (Convert.ToDecimal(BalanceValue) >= objModel.PaidAmount) // adde by shyam
                            {
                            if (objModel.PaymentId == 0)
                            {
                                DateTime dt = Convert.ToDateTime(objModel.PaymentDates);
                                var exist = db.tblInvoicePayments.Where(u => u.InvoiceId == objModel.InvoiceId && u.IsDeleted == false && u.PaymentDate == dt && u.PaidAmount == objModel.PaidAmount).ToList();
                                if (exist.Count != 0)
                                {
                                    message = "Already Exists";
                                }
                                else
                                {
                                    tblInvoicePayment objInvoice = new tblInvoicePayment();
                                    objInvoice.InvoiceId = objModel.InvoiceId; // Add Package Dropdown
                                    objInvoice.AttachmentId = attachmentId == 0 ? (Nullable<int>)null : attachmentId;
                                    objInvoice.PaymentDate = (objModel.PaymentDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(objModel.PaymentDates);
                                    objInvoice.PaidAmount = (objModel.PaidAmount == null) ? 00 : objModel.PaidAmount;
                                    objInvoice.IsDeleted = false;
                                    objInvoice.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                    objInvoice.IsPaid = false;
                                    objInvoice.Remark = objModel.Remark;
                                    db.tblInvoicePayments.Add(objInvoice);
                                    db.SaveChanges();
                                    message = "Added Successfully";
                                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                    if (string.IsNullOrEmpty(IpAddress))
                                    {
                                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                    }
                                    int UserID = ((UserModel)Session["UserData"]).UserId;
                                    int k = Functions.SaveUserLog(pkgId, "Invoice Payment", "Save", UserID, IpAddress, "Paid Amount: " + objModel.PaidAmount);
                                }

                            }

                            else
                            {
                                DateTime dt = Convert.ToDateTime(objModel.PaymentDates);
                                var exist = db.tblInvoicePayments.Where(u => u.InvoiceId == objModel.InvoiceId && u.IsDeleted == false && u.PaymentDate == dt && u.PaidAmount == objModel.PaidAmount && u.PaymentId != objModel.PaymentId).ToList();
                                if (exist.Count != 0)
                                {
                                    message = "Already Exists";
                                }
                                else
                                {
                                    tblInvoicePayment objInvoice = db.tblInvoicePayments.Where(u => u.PaymentId == objModel.PaymentId).SingleOrDefault();
                                    objInvoice.PaymentDate = (objModel.PaymentDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(objModel.PaymentDates);
                                    objInvoice.PaidAmount = (objModel.PaidAmount == null) ? 00 : objModel.PaidAmount;
                                    objInvoice.AttachmentId = attachmentId == 0 ? present == null ? (Nullable<int>)null : present.AttachmentId : attachmentId;
                                    objInvoice.Remark = objModel.Remark;
                                    db.SaveChanges();
                                    message = "Updated Successfully";
                                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                    if (string.IsNullOrEmpty(IpAddress))
                                    {
                                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                    }
                                    int UserID = ((UserModel)Session["UserData"]).UserId;
                                    try
                                    {
                                        string str = ""; ;
                                        var UpdatedValue = (from ul in db.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                                        if (UpdatedValue != null)
                                        {
                                            DateTime TrDate = (DateTime)objInvoice.PaymentDate;
                                            string TrzDate = TrDate.ToString("dd-MMM-yyyy");
                                            str = UpdatedValue;
                                            str = str.Replace(", ,", ",");
                                            str = str.TrimEnd(',');
                                            if (String.IsNullOrEmpty(str))
                                            {
                                                str = "Payment Date: " + TrzDate + "= " + "NA";
                                            }
                                            else
                                            {
                                                str = "Payment Date: " + TrzDate + "= " + str;
                                            }


                                        }
                                        else
                                        {
                                            str = "NA";
                                        }

                                        int k = Functions.SaveUserLog(pkgId, "Invoice Payment", "Update", UserID, IpAddress, str);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }



                            }
                            UpdatePackageValue(pkgId);
                        }
                        else
                        {
                            message = "Paid Value Not Valid";

                        }

                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return View("_PartialAddEditPaymentDetails", objModel);
                }
            }
            catch (Exception ex)
            {
                return View("_PartialAddEditPaymentDetails", objModel);
            }
        }

        #region -- Delete Invoice Details --
        [Audit]
        [HttpPost]
        public JsonResult InvoiceDelete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblInvoice objInvoice = db.tblInvoices.SingleOrDefault(o => o.InvoiceId == id);
                    int pkgId = (int)objInvoice.PackageId;
                    var InvoicePaymentList = db.tblInvoicePayments.Where(o => o.InvoiceId == id && o.IsDeleted == false).ToList();
                    if (InvoicePaymentList.Count != 0)
                    {
                        foreach (var item in InvoicePaymentList)
                        {
                            if (item.AttachmentId != null)
                            {
                                db.tblAttachments.Remove(db.tblAttachments.Where(a => a.AttachmentID == item.AttachmentId).FirstOrDefault());
                            }
                        }
                        db.tblInvoicePayments.RemoveRange(db.tblInvoicePayments.Where(a => a.InvoiceId == id));
                    }
                    db.tblInvoices.Remove(db.tblInvoices.Where(a => a.InvoiceId == id).FirstOrDefault());
                    db.SaveChanges();
                    UpdatePackageValue(pkgId);
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkggId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    int k = Functions.SaveUserLog(pkgId, "Invoice", "Delete", UserID, IpAddress, "Invoice No.:" + objInvoice.InvoiceNo);
                }
                return Json("1");
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion

        #region -- Delete InvoicePayment Details --
        [Audit]
        [HttpPost]
        public JsonResult PaymentDelete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    int packageId = (int)db.tblInvoices.Where(o => o.InvoiceId == (db.tblInvoicePayments.Where(x => x.PaymentId == id).FirstOrDefault().InvoiceId)).FirstOrDefault().PackageId;
                    var PaymentList = db.tblInvoicePayments.Where(o => o.PaymentId == id && o.IsDeleted == false).FirstOrDefault();
                    if (PaymentList != null)
                    {
                        if (PaymentList.AttachmentId != null)
                        {
                            db.tblAttachments.Remove(db.tblAttachments.Where(a => a.AttachmentID == PaymentList.AttachmentId).FirstOrDefault());
                        }
                        db.tblInvoicePayments.Remove(db.tblInvoicePayments.Where(a => a.PaymentId == id).FirstOrDefault());

                    }

                    db.SaveChanges();
                    if (packageId != null)
                    {
                        UpdatePackageValue(packageId);
                    }
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    int k = Functions.SaveUserLog(pkgId, "Invoice Payment", "Delete", UserID, IpAddress, "Paid Amount.:" + PaymentList.PaidAmount);
                }
                return Json("1");
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion

        #region ---- Updates the package value in tblPackage ----
        /// <summary>
        /// Updates the package value in tblPackage
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        internal void UpdatePackageValue(int packageId)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var oModel = db.tblPackages.Where(o => o.PackageId == packageId).FirstOrDefault();
                    if (oModel != null)
                    {
                        var totelCompletedValue = db.InvoiceSumViews.Where(o => o.PackageId == packageId).Sum(o => o.PaidAmount); // add all paymentamout from tblInvoicePayment table of that package
                        double totelSum = Convert.ToDouble(totelCompletedValue);
                        //if (totelSum>0)
                        //{
                        oModel.CompletedValue = totelSum;
                        oModel.BalanceValue = (oModel.RevisedPackageValue == null || oModel.RevisedPackageValue == 0) ?
                                                                           (oModel.PackageValue - totelSum) :
                                                                           (oModel.RevisedPackageValue - totelSum);
                        //}
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion ---- Updates the package value in tblPackage ----

    }
}