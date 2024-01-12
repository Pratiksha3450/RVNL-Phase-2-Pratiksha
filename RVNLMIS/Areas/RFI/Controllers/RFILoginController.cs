using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using PrimaBiWeb.Common;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace RVNLMIS.Areas.RFI.Controllers
{
    public class RFILoginController : Controller
    {
        public string _playerId = ""; //onesignal playerid
        // GET: RFIModule/RFILogins
        public ActionResult Index(string returnUrl)
        {
            LoginModel obj = new LoginModel();
            obj.returnUrl = returnUrl;
            return View(obj);
        }

        [HttpPost]
        public ActionResult Login(LoginModel oModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index");
                }

                using (var db = new dbRVNLMISEntities())
                {
                    string Encryptpass = Functions.Encrypt(oModel.Password.Trim());
                    var objUser = db.tblRFIUsers.Where(o => (o.Email == oModel.Username && o.Password == Encryptpass) || (o.Mobile == oModel.Username && o.Password == Encryptpass))
                        .SingleOrDefault();

                    if (objUser != null)
                    {
                        var playerId = Session["PlayerId"];
                        RegisterPushId(objUser.RFIUserId, Convert.ToString(playerId), objUser.FullName);
                        var authTicket = new FormsAuthenticationTicket(
                 2,
                objUser.FullName + "|" + objUser.Email + "|" + objUser.RFIUserId + "|" + objUser.Mobile + "|" + objUser.Organisation,
                 DateTime.Now,
                 DateTime.Now.AddDays(1),
                 false,
                 string.Empty,
                 "/RFIModule/RFILogin/Index"
             );
                        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(authTicket));
                        // cookie.Value = oModel.nameFormAuth.ToString();
                        cookie.HttpOnly = true;

                        if (oModel.RememberMe)
                        {
                            cookie.Expires = authTicket.Expiration;
                        }

                        Response.Cookies.Add(cookie);
                        SetSession(objUser.RFIUserId);
                        // CreateLog(objUser);

                        TempData["AfterLogin"] = 1;
                        string returnUrl = !string.IsNullOrEmpty(oModel.returnUrl) ? oModel.returnUrl : "0";
                        return Json(returnUrl);  /////SUCCESS
                    }
                    else
                    {
                        return Json("1", JsonRequestBehavior.AllowGet);   //////INCORRECT USERNAME
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return Json("-1", JsonRequestBehavior.AllowGet); //Error
            }
        }

        /// <summary>
        /// Sets the player identifier. (one signal)
        /// </summary>
        /// <param name="playerId">The player identifier.</param>
        /// <returns></returns>
        public JsonResult SetPlayerId(string playerId)
        {
            _playerId = playerId;
            System.Web.HttpContext.Current.Session["PlayerId"] = playerId;
            return Json("1");
        }

        private void RegisterPushId(int userId, string deskPushId, string username)
        {
            try
            {
                if (!string.IsNullOrEmpty(deskPushId))
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var obj = db.tblUserPushNotifyIds.Where(x => x.UserId == userId && x.PlayerId == deskPushId && x.PushType == "DESK" && x.UserType == "rfi").SingleOrDefault();
                        if (obj == null)
                        {
                            var tbl = new tblUserPushNotifyId()
                            {
                                PushType = "DESK",
                                PlayerId = deskPushId,
                                UserId = userId,
                                CreatedOn = DateTime.Now,
                                UserType = "rfi"

                            };
                            db.tblUserPushNotifyIds.Add(tbl);
                            db.SaveChanges();
                        }
                        else
                        {
                            obj.CreatedOn = DateTime.Now;
                            db.SaveChanges();
                        }
                    }
                    var rs = Functions.SendWebPush(userId, "RVNL-RFI", "Welcome to PrimaBi " + username, "rfi");
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
            }
        }


        #region ----SUPPORTIVE METHODS-----

        private void SetSession(int userId)
        {
            using (var db = new dbRVNLMISEntities())
            {
                UserModel objUserM = new UserModel();
                objUserM = (from u in db.tblRFIUsers
                            join d in db.tblRFIDesignations on u.DesignationId equals d.RFIDesignId
                            where u.RFIUserId == userId
                            select new { u, d }).AsEnumerable()
                           .Select(s => new UserModel
                           {
                               RoleTableID = (int)s.u.PackgeId,
                               UserId = s.u.RFIUserId,
                               UserName = s.u.FullName,
                               RoleId = Convert.ToInt32(s.u.DesignationId),
                               RoleCode = s.u.Organisation,
                               MobileNo = s.u.Mobile,
                               EmailId = s.u.Email,
                               DesignationName = s.d.Designation
                           }).FirstOrDefault();

                Session["RFIUserSession"] = objUserM;
             
            }
        }

        //private void CreateLog(tblrfiLogin objUser)
        //{
        //    tblAudit audit = new tblAudit()
        //    {
        //        UserId = objUser.RFIUserId,
        //        UserName = objUser.Email,
        //        // The IP Address of the Request
        //        IPAddress = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Request.UserHostAddress,
        //        // The URL that was accessed
        //        AreaAccessed = "/RFIModule/RFILogin/Index",
        //        // Creates our Timestamp
        //        TimeAccessed = DateTime.Now
        //    };
        //}

        #endregion

        #region -- Menu List --

        [Authorize]
        public ActionResult GetRFIMenuList()
        {
            try
            {
                //int roleId = string.IsNullOrEmpty(Session["UserData"].ToString()) ? 0 : ((UserModel)Session["UserData"]).RoleId;

                //if (roleId == 0)
                //{
                //    return RedirectToAction("Index", "Login");
                //}
                //else
                //{
                using (var db = new dbRVNLMISEntities())
                {
                    var result = (from m in db.tblAppMenus
                                      //join rm in db.tblRoleMenuAccesses on m.MenuId equals rm.MenuId
                                  where m.IsDeleted == false && m.IsRFI == true
                                  //&& rm.RoleId == roleId
                                  select new { m }).AsEnumerable().Select(s => new MenuModel
                                  {
                                      MenuID = s.m.MenuId,
                                      MenuParentID = s.m.ParrentId,
                                      MenuName = s.m.MenuName,
                                      Url = s.m.URL,
                                      Icon = s.m.Icon,
                                      ListId = s.m.LiTagId,
                                      MenuOrder = s.m.ParentOrder,
                                      IsReportMenu = CheckIsReport(s.m.MenuId)
                                  }).ToList().OrderBy(o => o.MenuOrder);
                    return PartialView("~/Areas/RFI/Views/Shared/_RFIMenuList.cshtml", result);
                }
                // }
            }
            catch (Exception ex)
            {
                var error = ex.Message.ToString();
                // return Content("Error");
                return RedirectToAction("Index", "Login");
            }
        }

        private int CheckIsReport(int menuId)
        {
            int yes = 0;
            using (var db = new dbRVNLMISEntities())
            {
                var checkMenu = db.tblPowerBIReports.Where(w => w.MenuId == menuId && w.isDeleted == false).FirstOrDefault();
                if (checkMenu != null)
                {
                    yes = 1;
                }
            }
            return yes;
        }
        #endregion

        public ActionResult LogOut()
        {
            Session.Abandon();
            return RedirectToAction("Index", "RFI/RFILogin");
        }

        [HttpPost]
        public JsonResult VerifyEmail(string email)
        {
            string _Emails = email;
            try
            {

                using (var db = new dbRVNLMISEntities())
                {

                    var objUser = db.ViewRFIUsersDetails.SingleOrDefault(o => o.Email == _Emails);

                    if (!string.IsNullOrEmpty(objUser.Email))
                    {
                        string EmailBody = "Your User email " + objUser.Email + " & Password is " + Functions.Decrypt(objUser.Password);
                        Email obj = new Email();
                        // _Emails = "csns.giri@gmail.com";
                        //List<Attachment> attachment = null;
                        List<string> cc = null;
                        List<string> bcc = null;
                        int rs = obj.SendMail(_Emails, cc, bcc, null, "Account information ", EmailBody, ConfigurationManager.AppSettings["SUPPORTFROM"]);
                        return Json("1");
                    }
                    else
                    {
                        return Json("2");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json("-1");
            }
        }

        #region -----CHANGE PASSWORD----

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SubmitPass(string oldPassword, string newPassword)
        {
            string _oldPassword = oldPassword;
            string _newPassword = newPassword;
            try
            {
                int loginUserId = Convert.ToInt32(((UserModel)Session["RFIUserSession"]).UserId);
                string OldPassword = Functions.Encrypt(oldPassword);
                using (var db = new dbRVNLMISEntities())
                {
                    UserModel objUser = new UserModel();
                    tblRFIUser objUserDetails = db.tblRFIUsers.FirstOrDefault(o => o.Password == OldPassword && o.RFIUserId == loginUserId);
                    if (objUserDetails != null)
                    {
                        objUserDetails.Password = Functions.Encrypt(newPassword);
                        db.SaveChanges();
                        return Json("1");
                    }
                    else
                    {
                        return Json("2");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json("-1");
            }
        }
        #endregion
    
        #region -----Notifications-----

        public ActionResult GetNotifications(int id)
        {
            RFINotificationLogController objCtr = new RFINotificationLogController();
            List<PushNotifyModel> notifyObj = objCtr.Read_Notification(id);

            return View("_PartialNotifyLayout", notifyObj.OrderByDescending(o => o.SentOn).Take(5).ToList()); //success
        }

        public ActionResult GetNotBadgeCount()
        {
            RFINotificationLogController objCtr = new RFINotificationLogController();
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            int count = objCtr.Read_Notification(userId).Where(n=>n.IsRead==false).Count();
            return Json(count, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}