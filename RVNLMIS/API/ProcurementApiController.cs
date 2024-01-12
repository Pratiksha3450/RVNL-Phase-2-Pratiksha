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

namespace RVNLMIS.API
{
    public class ProcurementApiController : ApiController
    {
        public HttpResponseMessage GetDiscipline(string roleCode, int dispId)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var discipline = (from d in dbContext.tblDisciplines
                                      where d.IsDeleted == false
                                      select new
                                      {
                                          d.DispId,
                                          d.DispCode,
                                          d.DispName
                                      }).ToList();

                    if (roleCode == "DISP")
                    {
                        discipline = discipline.Where(d => d.DispId == dispId).ToList();
                    }
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { discipline });
                }
                catch (Exception ex)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message });
                }
            }
        }

        public HttpResponseMessage GetProcMatList(string package, string disp)
        {
            List<ProcurementViewModel> lstProc = new List<ProcurementViewModel>();
            string whereCond = string.Empty;

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
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
                        OriginalQty = r.OriginalQty,
                        CumDeliveredQty = r.CumDeliveredQty,
                        CumOrderdQty = r.CumOrderedQty,
                        RatePerUnit = Convert.ToString(r.RatePerUnit),
                        RevisedQty = (decimal)r.RevisedQty

                    }).ToList();
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { lstProc });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, new { lstProc });
            }
        }

        [HttpPost]
        public HttpResponseMessage EditProcLoadTransaction(int PackMatId)
        {
            ProcurementController objProc = new ProcurementController();
            EditProcurementModel getMaterialInfo = new EditProcurementModel();
            List<ExistingProcList> gridLstProc = new List<ExistingProcList>();

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    getMaterialInfo = objProc.GetMaterialInfoBlock(dbContext, PackMatId);

                    gridLstProc = dbContext.GetExistingProcForPckgMaterial(PackMatId).Select(r => new ExistingProcList
                    {
                        PackMatId = PackMatId,
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
                        AttachFileName = r.FileName,
                        AttachFilePath = r.Path

                    }).ToList();
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { getMaterialInfo, gridLstProc });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, new { getMaterialInfo });
            }
        }

        [HttpPost]
        public HttpResponseMessage AddEditProcTransaction(FormDataCollection obj)
        {
            ExistingProcList objModel = new ExistingProcList();
            int transactionId = Convert.ToInt32(obj.Get("TransactionId"));
            int pkgMatId = Convert.ToInt32(obj.Get("pkgMatId"));

            objModel.OperationType = "Add";
            objModel.PackMatId = pkgMatId;

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    tblPkgMatTransaction present = dbContext.tblPkgMatTransactions.Where(t => t.PkgMatTranId == transactionId).FirstOrDefault();
                    if (present != null)
                    {
                        objModel.PackMatTransId = present.PkgMatTranId;
                        objModel.PackMatId = (int)present.PkgMatId;
                        objModel.StrTransDate = Convert.ToDateTime(present.TransDate).ToString("yyyy-MM-dd");
                        objModel.StrTargetDate = Convert.ToDateTime(present.TargetDate).ToString("yyyy-MM-dd");
                        objModel.TargetDate = present.TargetDate;
                        objModel.RevisedQty = present.RevisedQty ?? 0;
                        objModel.SupplierName = present.SupplierName;
                        objModel.PORef = present.PORef;
                        objModel.DeliveredQty = present.DeliveredQty ?? 0;
                        objModel.Remark = present.Remark;
                        objModel.OrderedQty = present.OrderedQty ?? 0;
                        objModel.OperationType = "Update";
                    }
                    return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.OK, new { objModel });
                }
                catch (Exception ex)
                {
                    return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.OK, new { objModel });
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage SubmitProcTransaction(ExistingProcList objList)
        {
            string message = string.Empty;
            DateTime? dt = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    DateTime transactionDate = Convert.ToDateTime(objList.StrTransDate);
                    tblPkgMatTransaction present = dbContext.tblPkgMatTransactions.Where(t => t.PkgMatTranId == objList.PackMatTransId).FirstOrDefault();
                    if (present != null)   //update values
                    {
                        present.TransDate = transactionDate;
                        present.DeliveredQty = objList.DeliveredQty;
                        present.OrderedQty = objList.OrderedQty;
                        present.PORef = objList.PORef;
                        present.Remark = objList.Remark;
                        present.RevisedQty = objList.RevisedQty;
                        present.SupplierName = objList.SupplierName;
                        present.TargetDate = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);
                        message = "Data updated successfully.";
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
                        objAdd.TargetDate = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);
                        dbContext.tblPkgMatTransactions.Add(objAdd);
                        message = "Data added successfully.";
                    }
                    dbContext.SaveChanges();
                }
                return ControllerContext.Request
               .CreateResponse(HttpStatusCode.OK, new { message });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
               .CreateResponse(HttpStatusCode.OK, new { ex.Message });
            }
        }

        public HttpResponseMessage SearchMaterial(string materialName, string disp, string package)
        {
            List<ProcurementViewModel> lstProc = new List<ProcurementViewModel>();
            string whereCond = string.Empty;

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    if (!string.IsNullOrEmpty(package) && !string.IsNullOrEmpty(disp))
                    {
                        whereCond = " WHERE PackageId=" + package + " AND DispId=" + disp;
                    }

                    lstProc = dbContext.GetProcurementIndexList(whereCond).Select(r => new ProcurementViewModel
                    {
                        PackMatId = r.PkgMatId,
                        PackageId = r.PackageId,
                        DispId = r.DispId,
                        MaterialName = r.MaterialName,
                        OriginalQty = r.OriginalQty,
                        CumDeliveredQty = r.CumDeliveredQty,
                        CumOrderdQty = r.CumOrderedQty,
                        RatePerUnit = Convert.ToString(r.RatePerUnit)
                    }).ToList();

                    lstProc = lstProc.Where(oh => oh.MaterialName.Contains(materialName)).ToList();
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { lstProc });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, new { lstProc });
            }
        }

        [HttpGet]
        public HttpResponseMessage DeleteTransaction(int id)
        {
            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    var objTodelete = dbContext.tblPkgMatTransactions.Where(d => d.PkgMatTranId == id).SingleOrDefault();

                    #region --Delete Attachment --

                    if (objTodelete.AttachmentId != null)
                    {
                        dbContext.tblAttachments.Remove(dbContext.tblAttachments.Where(a => a.AttachmentID == objTodelete.AttachmentId).FirstOrDefault());
                        // dbContext.SaveChanges();
                    }
                    #endregion

                    dbContext.tblPkgMatTransactions.Remove(objTodelete);
                    dbContext.SaveChanges();
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, "Data deleted successfully.");
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
    }
}
