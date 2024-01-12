using RVNLMIS.Common;
using RVNLMIS.Controllers;
using RVNLMIS.DAC;
using RVNLMIS.Models;
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

namespace RVNLMIS.API
{
    public class InvoiceApiController : ApiController
    {
        // GET: api/InvoiceApi
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/InvoiceApi/5 GetInvoice Info By Invoice Id
        public object Get(int id)
        {
            InvoiceInfoWrapper objInvoiceWrapper = new InvoiceInfoWrapper();
            List<InvoicePaymentDetail> ObjIpdModel = new List<InvoicePaymentDetail>();
            int _InvoiceId = id;
            try
            {

                //int _Proj = Functions.ParseInteger(form.Get("Project"));
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    ObjIpdModel = (from x in dbContext.InvoicePaymentViewDetails
                                   where x.InvoiceId == _InvoiceId && x.IsDeleted == false
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

                }
                if (ObjIpdModel.Count > 0)
                {
                    objInvoiceWrapper.Status = true;
                    objInvoiceWrapper.Message = "Successfully";
                    objInvoiceWrapper.InvoiceDetails = ObjIpdModel;

                }
                else
                {
                    objInvoiceWrapper.Status = false;
                    objInvoiceWrapper.Message = "Data Not Found";

                }
            }
            catch (Exception ex)
            {
                objInvoiceWrapper.Status = false;
                objInvoiceWrapper.Message = "Error";

            }


            return objInvoiceWrapper;
        }
        // POST: api/InvoiceApi
        [Route("Invoice/GetInvoicePaymentDetails")]
        public object Post(FormDataCollection form)
        {
            InvoiceWrapper objInvoiceWrapper = new InvoiceWrapper();
            List<InvoicePayment> ObjIpModel = new List<InvoicePayment>();
            List<InvoicePaymentDetail> ObjIpdModel = new List<InvoicePaymentDetail>();
            try
            {
                int _Pkg = Functions.ParseInteger(form.Get("Pkg"));
                //int _Proj = Functions.ParseInteger(form.Get("Project"));
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    ObjIpdModel = (from x in dbContext.InvoicePaymentViewDetails
                                   where x.PackageId == _Pkg && x.IsDeleted == false
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




                    foreach (var item in ObjIpdModel)
                    {

                        ObjIpModel = dbContext.tblInvoicePayments.Where(o => o.IsDeleted == false && o.InvoiceId == item.InvoiceId).Select(x => new Models.InvoicePayment
                        {
                            PaymentId = x.PaymentId,
                            InvoiceId = (int)x.InvoiceId,
                            InvoiceNo = dbContext.tblInvoices.Where(o => o.InvoiceId == (int)x.InvoiceId).Select(o => o.InvoiceNo).FirstOrDefault(),
                            PaidAmount = x.PaidAmount,
                            PaymentDate = x.PaymentDate,
                            IsDeleted = x.IsDeleted,
                            AttachFilePath = dbContext.tblAttachments.Where(o => o.AttachmentID == (int)x.AttachmentId).Select(o => o.Path).FirstOrDefault(),
                            AttachFileName = dbContext.tblAttachments.Where(o => o.AttachmentID == (int)x.AttachmentId).Select(o => o.FileName).FirstOrDefault(),
                            CreatedOn = (DateTime)x.CreatedOn,

                        }).OrderByDescending(c => c.CreatedOn).ToList();
                    }


                    //return Json(lst.Where(o => o.IsDeleted == false && o.InvoiceId == InvoiceId).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);



                }
                if (ObjIpdModel.Count > 0 || ObjIpModel.Count > 0)
                {
                    objInvoiceWrapper.Status = true;
                    objInvoiceWrapper.Message = "Successfully";
                    objInvoiceWrapper.InvoiceDetails = ObjIpdModel;
                    objInvoiceWrapper.InvoicePayment = ObjIpModel;

                }
                else
                {
                    objInvoiceWrapper.Status = false;
                    objInvoiceWrapper.Message = "Data Not Found";

                }
            }
            catch (Exception ex)
            {
                objInvoiceWrapper.Status = false;
                objInvoiceWrapper.Message = "Error";
                objInvoiceWrapper.InvoiceDetails = ObjIpdModel;
                objInvoiceWrapper.InvoicePayment = ObjIpModel;

            }


            return objInvoiceWrapper;
        }

        [HttpPost]
        [Route("Invoice/UploadFile")]
        public HttpResponseMessage UploadFile(int packageId,int userId)
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
                        string localPath = "~/Uploads/Attachments/PaymentInvoice";
                        Functions.CreateIfMissing(HostingEnvironment.MapPath(localPath));

                        fileName = string.Concat(packageCode, "-", postedFile.FileName.Replace(' ', '_'));
                        string filePath = "/Uploads/Attachments/PaymentInvoice/" + fileName;

                        postedFile.SaveAs(HostingEnvironment.MapPath(string.Concat(localPath,"/",fileName)));
                        //Save post to DB
                        
                        tblAttachment objAttach = new tblAttachment();

                        objAttach.FileName = fileName;
                        objAttach.Path = filePath;
                        objAttach.Type = "Invoice Payment";
                        objAttach.CreatedOn = DateTime.Now;
                        objAttach.IsDeleted = false;
                        objAttach.CreatedBy =userId;

                        dbContext.tblAttachments.Add(objAttach);
                        dbContext.SaveChanges();
                        attachmentId = objAttach.AttachmentID;

                        return Request.CreateResponse(HttpStatusCode.Found, new
                        {
                            status = "created",
                            path = root + filePath,
                            filename = fileName,
                            attachmentId= attachmentId
                        });
                    }
                }
            }
            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { status = "Error while uploading file.", });
        }

        [Route("Invoice/AddInvoice")]
        public object AddInvoice(FormDataCollection form)
        {
            AddUpdateInvoiceWrapper objInvoiceWrapper = new AddUpdateInvoiceWrapper();
            InvoicePaymentDetail ObjIpdModel = new InvoicePaymentDetail();
            tblInvoice objInvoice = new tblInvoice();
            string message = string.Empty;
            try
            {

                ObjIpdModel.ProjectId = Functions.ParseInteger(form.Get("ProjId"));
                ObjIpdModel.PackageId = Functions.ParseInteger(form.Get("PkgId"));
                ObjIpdModel.InvoiceNo = Convert.ToString(form.Get("InvoiceNo"));
                ObjIpdModel.CertifiedAmount = Convert.ToDecimal(form.Get("CertifiedAmount"));
                ObjIpdModel.InvoiceAmount = Convert.ToDecimal(form.Get("InvoiceAmount"));
                ObjIpdModel.InvoiceDates = Convert.ToString(form.Get("InvoiceDate"));


                using (var db = new dbRVNLMISEntities())
                {
                    var exist = db.tblInvoices.Where(u => u.InvoiceNo == ObjIpdModel.InvoiceNo && u.IsDeleted == false).SingleOrDefault();
                    if (exist != null)
                    {
                        message = "Already Exists";
                        objInvoiceWrapper.Data = 201;
                        objInvoiceWrapper.Status = true;
                        objInvoiceWrapper.Message = message;

                    }
                    else
                    {

                        objInvoice.ProjectId = ObjIpdModel.ProjectId;
                        objInvoice.PackageId = ObjIpdModel.PackageId;      // Add Package Dropdown
                        objInvoice.InvoiceNo = ObjIpdModel.InvoiceNo;
                        objInvoice.InvoiceDate = (ObjIpdModel.InvoiceDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(ObjIpdModel.InvoiceDates);
                        objInvoice.InvoiceAmount = (ObjIpdModel.InvoiceAmount == null) ? 00 : ObjIpdModel.InvoiceAmount;
                        objInvoice.CertifiedAmount = (ObjIpdModel.CertifiedAmount == null) ? 00 : ObjIpdModel.CertifiedAmount;
                        objInvoice.IsDeleted = false;
                        objInvoice.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                        objInvoice.IsPaid = false;
                        objInvoice.Remark = "-";
                        db.tblInvoices.Add(objInvoice);
                        db.SaveChanges();

                        message = "Added Successfully";
                        objInvoiceWrapper.Data = objInvoice.InvoiceId;
                        objInvoiceWrapper.Status = true;
                        objInvoiceWrapper.Message = message;
                    }
                }
            }
            catch (Exception ex)
            {
                message = "Error";
                objInvoiceWrapper.Data = 200;
                objInvoiceWrapper.Status = false;
                objInvoiceWrapper.Message = message;
            }


            return objInvoiceWrapper;
        }
        [Route("Invoice/UpdateInvoice")]
        public object UpdateInvoice(FormDataCollection form)
        {
            AddUpdateInvoiceWrapper objInvoiceWrapper = new AddUpdateInvoiceWrapper();
            InvoicePaymentDetail ObjIpdModel = new InvoicePaymentDetail();
            tblInvoice objInvoice = new tblInvoice();
            string message = string.Empty;
            try
            {

                ObjIpdModel.ProjectId = Functions.ParseInteger(form.Get("ProjId"));
                ObjIpdModel.InvoiceId = Functions.ParseInteger(form.Get("InvoiceId"));
                ObjIpdModel.PackageId = Functions.ParseInteger(form.Get("PkgId"));
                ObjIpdModel.InvoiceNo = Convert.ToString(form.Get("InvoiceNo"));
                ObjIpdModel.CertifiedAmount = Convert.ToDecimal(form.Get("CertifiedAmount"));
                ObjIpdModel.InvoiceAmount = Convert.ToDecimal(form.Get("InvoiceAmount"));
                ObjIpdModel.InvoiceDates = Convert.ToString(form.Get("InvoiceDate"));


                using (var db = new dbRVNLMISEntities())
                {
                    var exist = db.tblInvoices.Where(u => u.InvoiceNo == ObjIpdModel.InvoiceNo && u.IsDeleted == false && u.InvoiceId != ObjIpdModel.InvoiceId).ToList();
                    if (exist.Count != 0)
                    {
                        message = "Already Exists";
                        objInvoiceWrapper.Data = 201;
                        objInvoiceWrapper.Status = true;
                        objInvoiceWrapper.Message = message;
                    }
                    else
                    {
                        objInvoice = db.tblInvoices.Where(u => u.InvoiceId == ObjIpdModel.InvoiceId).SingleOrDefault();
                        objInvoice.ProjectId = ObjIpdModel.ProjectId;
                        objInvoice.PackageId = ObjIpdModel.PackageId;      // Add Package Dropdown
                        objInvoice.InvoiceNo = ObjIpdModel.InvoiceNo;
                        objInvoice.InvoiceAmount = ObjIpdModel.InvoiceAmount;
                        objInvoice.CertifiedAmount = ObjIpdModel.CertifiedAmount;
                        objInvoice.InvoiceDate = (ObjIpdModel.InvoiceDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(ObjIpdModel.InvoiceDates);
                        objInvoice.IsDeleted = false;
                        objInvoice.CreatedOn = DateTime.UtcNow.AddHours(5.5);

                        db.SaveChanges();
                        message = "Updated Successfully";
                        objInvoiceWrapper.Data = objInvoice.InvoiceId;
                        objInvoiceWrapper.Status = true;
                        objInvoiceWrapper.Message = message;
                    }
                }
            }
            catch (Exception ex)
            {
                message = "Error";
                objInvoiceWrapper.Data = 200;
                objInvoiceWrapper.Status = false;
                objInvoiceWrapper.Message = message;
            }


            return objInvoiceWrapper;
        }

        // PUT: api/InvoiceApi/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/InvoiceApi/5
        public void Delete(int id)
        {
        }
    }
}
