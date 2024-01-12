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
    public class ForgetPasswordController : ApiController
    {
        string xParentSignature = "ksbi8NwdXX";

        /// <summary>
        /// Verifies the email.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("VerifyEmail")]
        public object VerifyEmail(FormDataCollection form)
        {           
            ApiResult obj = new ApiResult();
            try
            {
                var req = Request;
                var headers = req.Headers;
                if (headers.Contains("X-signature"))
                {
                    string xValue = headers.GetValues("X-signature").First();                    
                    if (xValue == xParentSignature)
                    {
                        using (var dbContext = new dbRVNLMISEntities())
                        {
                            string emailId = form.Get("EmailId");
                            string token = form.Get("Token");
                            var objUser = dbContext.tblUserMasters.Where(o => o.EmailId == emailId).SingleOrDefault();
                            if (objUser != null)
                            {
                                objUser.OTP = token;
                                dbContext.SaveChanges();
                                obj.Code = 200;
                                obj.Data = "";
                                obj.Msg = "success";
                            }
                            else
                            {
                                obj.Code = 204;
                                obj.Data = "";
                                obj.Msg = "Invalid email address!";
                            }
                        }
                    }
                    else
                    {
                        obj.Code = 204;
                        obj.Data = "";
                        obj.Msg = "Invalid Signature";
                    }                    
                }
                else
                {
                    obj.Code = 204;
                    obj.Data = "";
                    obj.Msg = "Invalid request! Header missing";
                }
                return obj;
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                obj.Code = 204;
                obj.Msg = "error" + ex.Message;
                obj.Data = "";
                return obj;
            }
        }

        /// <summary>
        /// Updates the password.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("UpdatePassword")]
        public object UpdatePassword(FormDataCollection form)
        {
            ApiResult obj = new ApiResult();
            try
            {                
                var req = Request;
                var headers = req.Headers;
                if (headers.Contains("X-signature"))
                {
                    string xValue = headers.GetValues("X-signature").First();

                    if (xValue == xParentSignature)
                    {
                        using (var dbContext = new dbRVNLMISEntities())
                        {
                            string emailId = form.Get("EmailId");
                            string token = form.Get("Token");
                            string NewPass = form.Get("NewPass");
                            var objUser = dbContext.tblUserMasters.Where(o => o.EmailId == emailId && o.OTP == token).SingleOrDefault();
                            if (objUser != null)
                            {
                                objUser.Password = Functions.Encrypt(NewPass);
                                objUser.OTP = null;
                                dbContext.SaveChanges();
                                obj.Code = 200;
                                obj.Data = "";
                                obj.Msg = "success";
                            }
                            else
                            {
                                obj.Code = 204;
                                obj.Data = "";
                                obj.Msg = "Invalid token / email address!";
                            }
                        }
                    }
                    else
                    {
                        obj.Code = 204;
                        obj.Data = "";
                        obj.Msg = "Invalid Signature";
                    }
                }
                else
                {
                    obj.Code = 204;
                    obj.Data = "";
                    obj.Msg = "Invalid request! Header missing";
                }
                return obj;
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                obj.Code = 204;
                obj.Msg = "error" + ex.Message;
                obj.Data = "";
                return obj;
            }
        }
    }
}