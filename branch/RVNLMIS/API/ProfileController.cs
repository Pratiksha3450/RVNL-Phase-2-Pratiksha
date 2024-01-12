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
    public class ProfileController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage ProfileUpdate(FormDataCollection obj)
        {
            //ResponseModelView objResponse = new ResponseModelView();
            //ResponseData objResponseData = new ResponseData();
            string message = string.Empty;

            try
            {
                int userId = Convert.ToInt32(obj.Get("userid"));
                string mobNo = obj.Get("mobileNo");

                using (var dbContext = new dbRVNLMISEntities())
                {
                    var objUser = dbContext.tblUserMasters.Where(u => u.UserId == userId && u.IsDeleted == false).SingleOrDefault();

                    if (objUser != null)
                    {
                        objUser.MobileNo = mobNo;
                        dbContext.SaveChanges();

                        //objResponse.Type = "Response";
                        //objResponse.StatusCode = "200";
                        //objResponse.Message = "Profile updated successfully.";

                        //objResponseData.username = objUser.UserName.Trim().ToString();
                        //objResponseData.ContactNo = mobNo;
                        //objResponseData.EmailId = objUser.EmailId;
                        //objResponse.Data = objResponseData;
                    }
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK,  "Profile updated successfully.");
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public HttpResponseMessage GetProfileDetails(int userId)
        {
            ResponseModelView objResponse = new ResponseModelView();
            ResponseData objResponseData = new ResponseData();

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    var objUser = dbContext.tblUserMasters.Where(u => u.UserId == userId && u.IsDeleted == false).SingleOrDefault();

                    if (objUser != null)
                    {
                        objResponse.Type = "Response";
                        objResponse.StatusCode = "200";
                        objResponse.Message = "User Exist & Active!";

                        objResponseData.username = objUser.UserName.Trim().ToString();
                        objResponseData.ContactNo = objUser.MobileNo;
                        objResponseData.EmailId = objUser.EmailId;
                        objResponse.Data = objResponseData;
                    }
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { objResponse });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage ChangePassword(FormDataCollection obj)
        {
            try
            {
                string message = string.Empty;
                string oldPass = Functions.Encrypt(obj.Get("oldpassword"));
                string newPass = Functions.Encrypt(obj.Get("newpassword"));
                int userId = Convert.ToInt32(obj.Get("userid"));

                using (var dbContext = new dbRVNLMISEntities())
                {
                    var objUser = dbContext.tblUserMasters.Where(u => u.UserId == userId && u.Password == oldPass && u.IsDeleted == false).SingleOrDefault();

                    if (objUser == null)
                    {
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK,  "Old password is incorrect.");
                    }
                    else
                    {
                        objUser.Password = newPass;
                        dbContext.SaveChanges();
                    }
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, "password updated successfully.");
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public HttpResponseMessage GetNotifications(int userId)
        {

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    var objList = dbContext.tblNotifications.ToList().Select(s=>new { s.Date,s.Message,s.Title}).OrderByDescending(o=>o.Date);
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { objList });
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
    }
}
