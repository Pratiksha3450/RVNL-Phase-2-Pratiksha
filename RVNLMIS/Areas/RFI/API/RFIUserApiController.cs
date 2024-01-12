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
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;

namespace RVNLMIS.Areas.RFI.API
{
    public class RFIUserApiController : ApiController
    {
        public HttpResponseMessage ReadRfiUsers(int packageId)
        {
            List<RFIUserModel> lstInfo = new List<RFIUserModel>();

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    lstInfo = (from s in dbContext.tblRFIUsers
                               where s.PackgeId == packageId
                               select new RFIUserModel
                               {
                                   RFIUserId = s.RFIUserId,
                                   FullName = s.FullName,
                                   DesignationId = s.DesignationId,
                                   Email = s.Email,
                                   Mobile = s.Mobile,
                                   Organisation = s.Organisation,
                                   ReportingTo = s.ReportingTo,
                               }).ToList();
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lstInfo });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new { lstInfo });
            }
        }

       // [Route("RFIModule/RFIUserApi/GetInvoicePaymentDetails")]
        [HttpPost]
        public HttpResponseMessage AddEditUser(RFIUserModel objModel)
        {
            string message = string.Empty;
            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    if (objModel.RFIUserId == 0)
                    {
                        tblRFIUser objAdd = new tblRFIUser();
                        objAdd.PackgeId = objModel.PackgeId;
                        objAdd.FullName = objModel.FullName;
                        objAdd.Email = objModel.Email;
                        objAdd.DesignationId = objModel.DesignationId;
                        objAdd.Organisation = objModel.Organisation;
                        objAdd.Mobile = objModel.Mobile;
                        objAdd.ReportingTo = objModel.ReportingTo;
                        objAdd.Password = Functions.Encrypt(objModel.Password);
                        dbContext.tblRFIUsers.Add(objAdd);
                        dbContext.SaveChanges();
                        message = "User added successfully.";
                    }
                    else
                    {
                        var objEdit = dbContext.tblRFIUsers.Where(u => u.RFIUserId == objModel.RFIUserId).FirstOrDefault();
                       // objEdit.PackgeId = objModel.PackgeId;
                        objEdit.FullName = objModel.FullName;
                        objEdit.Email = objModel.Email;
                        objEdit.DesignationId = objModel.DesignationId;
                        objEdit.Organisation = objModel.Organisation;
                        objEdit.Mobile = objModel.Mobile;
                        objEdit.ReportingTo = objModel.ReportingTo;
                        //objEdit.Password = Functions.Encrypt(objModel.Password);
                        dbContext.SaveChanges();
                        message = "User updated successfully.";
                    }
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
            }
            catch (Exception ex)
            {
                message = "Error!";
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new { message });
            }
        }
    }
}
