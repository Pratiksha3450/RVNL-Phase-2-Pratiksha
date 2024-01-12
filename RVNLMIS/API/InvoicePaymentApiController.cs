using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace RVNLMIS.API
{
    public class InvoicePaymentApiController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/InvoicePaymentApi/5
        public object Get(int id)
        {
            int _PaymentId = id;
            InvoicePaymentInfoById objInvoiceWrapper = new InvoicePaymentInfoById();
            List<InvoicePayment> ObjIpModel = new List<InvoicePayment>();

            try
            {

                //int _Proj = Functions.ParseInteger(form.Get("Project"));
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    ObjIpModel = dbContext.tblInvoicePayments.Where(o => o.IsDeleted == false && o.PaymentId == _PaymentId).Select(x => new Models.InvoicePayment
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
                    //return Json(lst.Where(o => o.IsDeleted == false && o.InvoiceId == InvoiceId).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);



                }
                if (ObjIpModel.Count > 0)
                {
                    objInvoiceWrapper.Status = true;
                    objInvoiceWrapper.Message = "Successfully";
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
                objInvoiceWrapper.InvoicePayment = ObjIpModel;

            }


            return objInvoiceWrapper;
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }
        [Route("InvoicePayment/AddInvoicePayment")]
        public object AddInvoicePayment(FormDataCollection form)
        {
            AddUpdateInvoiceWrapper objInvoiceWrapper = new AddUpdateInvoiceWrapper();
            InvoicePayment ObjIpdModel = new InvoicePayment();
            tblInvoicePayment objInvoice = new tblInvoicePayment();
            string message = string.Empty;
            try
            {

                ObjIpdModel.InvoiceId = Functions.ParseInteger(form.Get("InvoiceId"));
                ObjIpdModel.AttachmentId = Functions.ParseInteger(form.Get("AttachmentId"));
                ObjIpdModel.PaidAmount = Functions.ParseInteger(form.Get("PaidAmount"));
                ObjIpdModel.PaymentDates = Convert.ToString(form.Get("PaymentDates"));
                using (var db = new dbRVNLMISEntities())
                {
                    objInvoice.InvoiceId = ObjIpdModel.InvoiceId; // Add Package Dropdown
                    objInvoice.AttachmentId = ObjIpdModel.AttachmentId == 0 ? (Nullable<int>)null : ObjIpdModel.AttachmentId;
                    objInvoice.PaymentDate = (ObjIpdModel.PaymentDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(ObjIpdModel.PaymentDates);
                    objInvoice.PaidAmount = (ObjIpdModel.PaidAmount == null) ? 00 : ObjIpdModel.PaidAmount;
                    objInvoice.IsDeleted = false;
                    objInvoice.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                    objInvoice.IsPaid = false;
                    objInvoice.Remark = "-";
                    db.tblInvoicePayments.Add(objInvoice);
                    db.SaveChanges();

                    message = "Added Successfully";
                    objInvoiceWrapper.Data = objInvoice.PaymentId;
                    objInvoiceWrapper.Status = true;
                    objInvoiceWrapper.Message = message;

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


        [Route("InvoicePayment/UpdateInvoicePayment")]
        public object UpdateInvoicePayment(FormDataCollection form)
        {
            AddUpdateInvoiceWrapper objInvoiceWrapper = new AddUpdateInvoiceWrapper();
            InvoicePayment ObjIpdModel = new InvoicePayment();
            tblInvoicePayment objInvoice = new tblInvoicePayment();
            string message = string.Empty;
            try
            {

                ObjIpdModel.InvoiceId = Functions.ParseInteger(form.Get("InvoiceId"));
                ObjIpdModel.PaymentId = Functions.ParseInteger(form.Get("PaymentId"));
                ObjIpdModel.AttachmentId = Functions.ParseInteger(form.Get("AttachmentId"));
                ObjIpdModel.PaidAmount = Functions.ParseInteger(form.Get("PaidAmount"));
                ObjIpdModel.PaymentDates = Convert.ToString(form.Get("PaymentDates"));
                using (var db = new dbRVNLMISEntities())
                {
                    objInvoice = db.tblInvoicePayments.Where(u => u.PaymentId == ObjIpdModel.PaymentId).SingleOrDefault();
                    objInvoice.PaymentDate = (ObjIpdModel.PaymentDates == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(ObjIpdModel.PaymentDates);
                    objInvoice.PaidAmount = (ObjIpdModel.PaidAmount == null) ? 00 : ObjIpdModel.PaidAmount;
                    objInvoice.AttachmentId = ObjIpdModel.AttachmentId == 0 ? objInvoice.AttachmentId : ObjIpdModel.AttachmentId;
                    db.SaveChanges();
                    message = "Updated Successfully";
                    objInvoiceWrapper.Data = objInvoice.PaymentId;
                    objInvoiceWrapper.Status = true;
                    objInvoiceWrapper.Message = message;

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
        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}