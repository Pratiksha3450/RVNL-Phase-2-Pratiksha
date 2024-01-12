using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using RVNLMIS.Models.PowerBI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    public class InvoiceController : Controller
    {
        public string IpAddress = "";
        string invoicecode;
        // GET: Invoice
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
            int k = Functions.SaveUserLog(pkgId, "Invoice Status", "View", UserID, IpAddress, "NA");
            TempData["InvoiceCode"] = GenerateInvoiceCode();
            GetPackages();
            GetProjects();
            return View();
        }

        public ActionResult _AddEditInvoice()
        {
            InvoiceModel invoice = new InvoiceModel();
            GetProjects();
            GetPackages();
            GenerateInvoiceCode();
            invoice.InvoiceNo = invoicecode;
            return View();
        }

        public ActionResult GetProjects()
        {
            dbRVNLMISEntities db = new dbRVNLMISEntities();
            var projects = Functions.GetroleAccessibleProjectsList();
            SelectList list = new SelectList(projects, "ProjectId", "ProjectName");
            ViewBag.ProjectList = list;
            return View();
        }

        public ActionResult GetPackages()
        {
            dbRVNLMISEntities db = new dbRVNLMISEntities();
            var pkgs = db.tblPackages.ToList();
            SelectList list = new SelectList(pkgs, "PackageId", "PackageName");
            ViewBag.PackageList = list;
            return View();
        }

            #region -- Add and Update Invoice Details --
            [HttpPost]
            public ActionResult AddEditInvoice(InvoiceModel oModel)
            {
                try
                {
                    string message = string.Empty;
                    if (oModel.InvoiceId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblInvoices.Where(u => u.ProjectId == oModel.ProjectId && u.IsDeleted == false).FirstOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                                return Json(message, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                GenerateInvoiceCode();
                                tblInvoice objinvoice = new tblInvoice();
                                objinvoice.ProjectId = oModel.ProjectId;
                                objinvoice.PackageId = oModel.PackageId;
                                objinvoice.InvoiceNo = invoicecode;
                                objinvoice.InvoiceDate = Convert.ToDateTime(oModel.InvoiceDate);
                                objinvoice.InvoiceAmount = oModel.InvoiceAmount;
                                objinvoice.CertifiedAmount = oModel.CertifiedAmount;
                                objinvoice.IsPaid = false;
                                objinvoice.Remark = oModel.Remark;
                                objinvoice.CreatedOn = Convert.ToDateTime(oModel.CreatedOn);
                                objinvoice.IsDeleted = false;
                                db.tblInvoices.Add(objinvoice);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            tblInvoice objinvoice = db.tblInvoices.Where(u => u.InvoiceId == oModel.InvoiceId).SingleOrDefault();
                            objinvoice.ProjectId = oModel.ProjectId;
                            objinvoice.PackageId = oModel.PackageId;
                            objinvoice.InvoiceNo = oModel.InvoiceNo;
                            objinvoice.InvoiceDate = Convert.ToDateTime(oModel.InvoiceDate);
                            objinvoice.InvoiceAmount = oModel.InvoiceAmount;
                            objinvoice.CertifiedAmount = oModel.CertifiedAmount;
                            objinvoice.IsPaid = false;
                            objinvoice.Remark = oModel.Remark;
                            objinvoice.CreatedOn = Convert.ToDateTime(oModel.CreatedOn);
                            db.SaveChanges();
                            message = "Updated Successfully";

                        }
                    }
                    ModelState.Clear();
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return View("Index", oModel);
                }
            
            }
            #endregion


        #region -- Edit Resource Details --
        [Audit]
        public ActionResult EditInvoiceByInvoiceId(int id)
        {
            GetPackages();
            GetProjects();
            InvoiceModel objModel = new InvoiceModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oInvoiceDetails = db.tblInvoices.Where(o => o.InvoiceId == id && o.IsDeleted == false).SingleOrDefault();
                        if (oInvoiceDetails != null)
                        {
                            objModel.ProjectId = (oInvoiceDetails.ProjectId == null) ? 0 : (int)oInvoiceDetails.ProjectId; ;   
                            objModel.Remark = oInvoiceDetails.Remark;
                            objModel.PackageId = (int)oInvoiceDetails.PackageId;
                            objModel.InvoiceId = (int)oInvoiceDetails.InvoiceId;
                            objModel.InvoiceAmount = oInvoiceDetails.InvoiceAmount;
                            objModel.InvoiceNo = oInvoiceDetails.InvoiceNo;
                            objModel.CertifiedAmount = oInvoiceDetails.CertifiedAmount;
                            objModel.IsPaid = false;
                            objModel.InvoiceDate = Convert.ToDateTime(oInvoiceDetails.InvoiceDate).ToString("yyyy-MM-dd");
                            objModel.CreatedOn = Convert.ToDateTime(oInvoiceDetails.CreatedOn).ToString("yyyy-MM-dd");
                        }                       
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditInvoice", objModel);
        }
        #endregion

        public string GenerateInvoiceCode()
        {
            dbRVNLMISEntities db = new dbRVNLMISEntities();
            int id;
            int maxId = db.tblInvoices.Max(p => p.InvoiceId);
            if (maxId <= 0)
            {
                invoicecode = "IPA-001";
            }
            else
            {
                id = maxId + 1;
                invoicecode = "IPA-" + id;
            }
            return invoicecode;
        }


        #region --- List Resource Values ---
        public ActionResult Invoice_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var pkgs = Functions.GetRoleAccessiblePackageList();
                var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();

                var lst = (from x in dbContext.InvoiceViews

                           select new InvoiceModel
                           {
                               ProjectId = (int)x.ProjectId,
                               ProjectName = x.ProjectName,
                               InvoiceId = x.InvoiceId,
                               PackageId = (int)x.PackageId,
                               PackageName = x.PackageName,
                               PackageCode = x.PackageCode,
                               InvoiceNo = x.InvoiceNo,
                               InvoiceDate = (x.InvoiceDate).ToString(),
                               InvoiceAmount = x.InvoiceAmount,
                               CertifiedAmount = x.CertifiedAmount,
                               Remark = x.Remark,
                               IsPaid = x.IsPaid,
                               CreatedOn = (x.CreatedOn).ToString(),
                           }).OrderByDescending(o => o.InvoiceId).ToList();

                lst = lst.Where(w => accessiblePackageList.Contains((int)w.PackageId)).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Delete Invoice Details --
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblInvoice obj = db.tblInvoices.FirstOrDefault(o => o.InvoiceId == id);
                    obj.IsDeleted = true;
                    db.SaveChanges();
                    return Json("success");
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }
        #endregion

        public JsonResult Get_PackageByProject(int? id)
        {
            List<PackageModel> _PackageList = new List<PackageModel>();

            try
            {
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
                                        where (e.ProjectId == id && e.IsDeleted == false)
                                        select new PackageModel
                                        {
                                            PackageId = e.PackageId,
                                            PackageName = e.PackageCode + " - " + e.PackageName
                                        }).ToList();
                    }
                    return Json(_PackageList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(_PackageList, JsonRequestBehavior.AllowGet);
            }
        }


    }
}