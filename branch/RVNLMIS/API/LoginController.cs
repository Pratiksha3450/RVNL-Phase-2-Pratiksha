using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using RVNLMIS.DAC;
using RVNLMIS.Common;
using RVNLMIS.Models;
using System.Net.Http.Formatting;
using System.Text.RegularExpressions;

namespace RVNLMIS.API
{
    public class LoginController : ApiController
    {
        [HttpPost]
        // POST: api/Login
        public HttpResponseMessage Post(FormDataCollection obj)
        {
            Controllers.LoginController objContrLogin = new Controllers.LoginController();

            using (var dbContext = new dbRVNLMISEntities())
            {
                string Encryptpass = Functions.Encrypt(obj.Get("password").Trim());
                string username = obj.Get("username");
                var objUser = dbContext.UserDetailsWithRoles.Where(o => o.UserName == username && o.Password == Encryptpass).SingleOrDefault();

                ResponseModelView objResponse = new ResponseModelView();
                ResponseData objResponseData = new ResponseData();

                if (objUser != null)
                {
                    //Create Response Object
                    if (objUser.RoleCode == "PKG")
                    {
                        int subStatus = objContrLogin.CheckIsSubscription(objUser.UserId);
                        if (subStatus == 200) // success
                        {
                            objResponse.Type = "Response";
                            objResponse.StatusCode = "200";
                            objResponse.Message = "User Exist & Active!";

                            objResponseData.userid = objUser.UserId.ToString();
                            objResponseData.name = objUser.Name.Trim().ToString();
                            objResponseData.username = objUser.UserName.Trim().ToString();
                            objResponseData.RoleId = objUser.RoleId.ToString();
                            objResponseData.RoleCode = objUser.RoleCode.ToString();
                            objResponseData.TableDataName = Regex.Replace(objUser.TableDataName.ToString(), @"\s+", "");
                            objResponseData.ContactNo = objUser.MobileNo;
                            objResponseData.EmailId = objUser.EmailId;
                            objResponse.Data = objResponseData;
                        }
                        else if (subStatus == 406) //expired/inactive
                        {
                            objResponse.Type = "Response";
                            objResponse.StatusCode = "406";
                            objResponse.Message = "Your subscription is inactive or expired.";
                        }
                        else      //error
                        {
                            objResponse.Type = "Response";
                            objResponse.StatusCode = "200";
                            objResponse.Message = "Technical Error Please Try Again Later.";
                        }
                    }
                    else
                    {
                        objResponse.Type = "Response";
                        objResponse.StatusCode = "200";
                        objResponse.Message = "User Exist & Active!";

                        objResponseData.userid = objUser.UserId.ToString();
                        objResponseData.name = objUser.Name.Trim().ToString();
                        objResponseData.username = objUser.UserName.Trim().ToString();
                        objResponseData.RoleId = objUser.RoleId.ToString();
                        objResponseData.RoleCode = objUser.RoleCode.ToString();
                        objResponseData.TableDataName = Regex.Replace(objUser.TableDataName.ToString(), @"\s+", "");
                        objResponseData.ContactNo = objUser.MobileNo;
                        objResponseData.EmailId = objUser.EmailId;
                        objResponse.Data = objResponseData;
                    }
                }
                else
                {
                    //ResponseModelView objResponse = new ResponseModelView();
                    objResponse.Type = "Response";
                    objResponse.StatusCode = "101";
                    objResponse.Message = "User Not Exist Or Inactive!";

                    objResponse.Data = objResponseData;
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(objResponse), Encoding.UTF8, "application/json");
                return response;
            }
        }
    }
}
