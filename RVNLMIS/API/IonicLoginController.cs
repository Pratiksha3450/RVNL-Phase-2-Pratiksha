using Newtonsoft.Json;
using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace RVNLMIS.API
{
    public class IonicLoginController : ApiController
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
                string fcmToken = obj.Get("FCM");
                string iosUserId = obj.Get("iosUserId");

                var objUser = dbContext.UserDetailsWithRoles.Where(o => o.UserName == username && o.Password == Encryptpass).SingleOrDefault();

                ResponseModelView objResponse = new ResponseModelView();
                ResponseData objResponseData = new ResponseData();

                if (objUser != null)
                {
                    #region add/ update FCM Token

                    var objUserUpdate = dbContext.tblUserMasters.Where(u => u.UserId == objUser.UserId).SingleOrDefault();

                    if (!string.IsNullOrEmpty(fcmToken))  //update token
                    {
                        objUserUpdate.FCMToken = fcmToken;
                    }

                    if (!string.IsNullOrEmpty(iosUserId))  //update token
                    {
                        objUserUpdate.IosUserId = iosUserId;
                        // dbContext.SaveChanges();
                    }

                    dbContext.SaveChanges();
                    #endregion

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
                            objResponseData.Discipline = objUser.Discipline;
                            objResponseData.RoleCode = objUser.RoleCode.ToString();
                            objResponseData.TableDataName = Regex.Replace(objUser.TableDataName.ToString(), @"\s+", "");
                            objResponseData.ContactNo = objUser.MobileNo;
                            objResponseData.EmailId = objUser.EmailId;
                            var isActiveIssue = dbContext.tblDataIssues.Any(a => a.PackageId == objUser.RoleTableId && a.StatusId != 4);
                            objResponseData.AnyActiveIssue = isActiveIssue;
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
                        objResponseData.Discipline = objUser.Discipline;
                        objResponseData.RoleId = objUser.RoleId.ToString();
                        objResponseData.RoleCode = objUser.RoleCode.ToString();
                        objResponseData.TableDataName = Regex.Replace(objUser.TableDataName.ToString(), @"\s+", "");
                        objResponseData.ContactNo = objUser.MobileNo;
                        objResponseData.EmailId = objUser.EmailId;
                        objResponseData.AnyActiveIssue = false; ;
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

        [HttpPost]
        public HttpResponseMessage IonicLogin(FormDataCollection obj)
        {
            Controllers.LoginController objContrLogin = new Controllers.LoginController();

            using (var dbContext = new dbRVNLMISEntities())
            {
                string Encryptpass = Functions.Encrypt(obj.Get("password").Trim());
                string username = obj.Get("username");
                string fcmToken = obj.Get("FCM");
                string iosUserId = obj.Get("iosUserId");

                var objUser = dbContext.UserDetailsWithRoles.Where(o => o.UserName == username && o.Password == Encryptpass).SingleOrDefault();

                ResponseModelView objResponse = new ResponseModelView();
                ResponseData objResponseData = new ResponseData();

                if (objUser != null)
                {
                    #region add/ update FCM Token

                    var objUserUpdate = dbContext.tblUserMasters.Where(u => u.UserId == objUser.UserId).SingleOrDefault();

                    if (!string.IsNullOrEmpty(fcmToken))  //update token
                    {
                        objUserUpdate.FCMToken = fcmToken;
                    }

                    if (!string.IsNullOrEmpty(iosUserId))  //update token
                    {
                        objUserUpdate.IosUserId = iosUserId;
                        // dbContext.SaveChanges();
                    }

                    dbContext.SaveChanges();
                    #endregion

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
                            objResponseData.Discipline = objUser.Discipline;
                            objResponseData.RoleCode = objUser.RoleCode.ToString();
                            objResponseData.TableDataName = Regex.Replace(objUser.TableDataName.ToString(), @"\s+", "");
                            objResponseData.ContactNo = objUser.MobileNo;
                            objResponseData.EmailId = objUser.EmailId;
                            var isActiveIssue = dbContext.tblDataIssues.Any(a => a.PackageId == objUser.RoleTableId && a.StatusId != 4);
                            objResponseData.AnyActiveIssue = isActiveIssue;
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
                        objResponseData.Discipline = objUser.Discipline;
                        objResponseData.RoleId = objUser.RoleId.ToString();
                        objResponseData.RoleCode = objUser.RoleCode.ToString();
                        objResponseData.TableDataName = Regex.Replace(objUser.TableDataName.ToString(), @"\s+", "");
                        objResponseData.ContactNo = objUser.MobileNo;
                        objResponseData.EmailId = objUser.EmailId;
                        objResponseData.AnyActiveIssue = false; ;
                        objResponse.Data = objResponseData;
                    }
                }
                else
                {
                    var checkIsRFI = dbContext.tblRFIUsers.Where(o => (o.Email == username && o.Password == Encryptpass) || (o.Mobile == username && o.Password == Encryptpass))
                        .FirstOrDefault();
                    if (checkIsRFI != null)
                    {
                        objResponse.Type = "Response";
                        objResponse.StatusCode = "200";
                        objResponse.Message = "User Exist & Active!";

                        objResponseData.userid = checkIsRFI.RFIUserId.ToString();
                        objResponseData.name = checkIsRFI.FullName;
                        objResponseData.PackageId = checkIsRFI.PackgeId;
                        objResponseData.DesignationId = checkIsRFI.DesignationId;
                        objResponseData.Orgnisation = checkIsRFI.Organisation.ToString();
                        objResponseData.Designation = dbContext.tblRFIDesignations.Where(d => d.RFIDesignId == checkIsRFI.DesignationId).Select(s => s.Designation).FirstOrDefault();
                        objResponseData.ContactNo = checkIsRFI.Mobile;
                        objResponseData.EmailId = checkIsRFI.Email;
                        objResponse.Data = objResponseData;
                    }
                    else
                    {
                        objResponse.Type = "Response";
                        objResponse.StatusCode = "101";
                        objResponse.Message = "User Not Exist Or Inactive!";
                        objResponse.Data = objResponseData;
                    }
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(objResponse), Encoding.UTF8, "application/json");
                return response;
            }
        }
    }
}
