using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace RVNLMIS.Controllers
{
    //[Audit]
    [HandleError]
    //[Compress]
    public class LoginController : Controller
    {
        public readonly int d;

        public string _playerId = ""; //onesignal playerid
        // GET: Login
        [AllowAnonymous]
        public ActionResult Index(string returnUrl, string Session)
        {
            LoginModel objlogin = new LoginModel();
            objlogin.returnUrl = returnUrl;

            string cookiename = FormsAuthentication.FormsCookieName;
            HttpCookie nameFormAuth = HttpContext.Request.Cookies[cookiename];

            //objlogin.cookie = FormsAuthentication.FormsCookieName;

            //objlogin.nameFormAuth = nameFormAuth;

            if (nameFormAuth != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(nameFormAuth.Value);

                if (ticket.IsPersistent)
                {
                    if (ticket != null && !string.IsNullOrEmpty(Convert.ToString(HttpContext.Session["UserData"])))
                    {
                        if (ticket.Expiration > DateTime.Now)
                        {
                            // return RedirectToAction("LockScreen", "Login", new { returnUrl = ReturnUrl });

                            string[] paramStr = ticket.Name.Split('|');
                            int userId = Functions.ParseInteger(paramStr[1]);

                            SetSession(userId);
                            return RedirectToAction("LockScreen", "Login", new { returnUrl });
                            //return RedirectToAction("Index", "DashboardReports", new { id = HttpUtility.UrlEncode("?id=1") });
                        }
                    }
                    else if (ticket != null && string.IsNullOrEmpty(Convert.ToString(HttpContext.Session["UserData"])))
                    {
                        if (ticket.Expiration > DateTime.Now)
                        {
                            return RedirectToAction("LockScreen", "Login", new { returnUrl });
                        }
                    }
                }
                else
                {
                    HttpCookie LoginCookie = Request.Cookies["RVNLMIS_Login"];
                    // string token = (LoginCookie != null) ? Convert.ToString(Request.Cookies["RVNLMIS_Login"]["UserName"]) : "NA";

                    if (LoginCookie != null && LoginCookie.Expires > DateTime.Now)
                    {
                        //objlogin.rloginCookie = LoginCookie;
                        return RedirectToAction("LockScreen", "Login", new { returnUrl = returnUrl });
                    }
                    else
                    {
                        //objlogin.r1 = 1;
                        DeleteAuthCookies();
                        return View(objlogin);
                    }
                }
            }
            else
            {
                HttpCookie LoginCookie = Request.Cookies["RVNLMIS_Login"];
                // string token = (LoginCookie != null) ? Convert.ToString(Request.Cookies["RVNLMIS_Login"]["UserName"]) : "NA";

                if (LoginCookie != null && LoginCookie.Expires > DateTime.Now)
                {
                    //objlogin.rloginCookie = LoginCookie;
                    return RedirectToAction("LockScreen", "Login", new { returnUrl = returnUrl });
                }
                else
                {
                    // objlogin.rloginCookie1 = token;
                    //objlogin.r2 = 2;
                    DeleteAuthCookies();
                    return View(objlogin);
                }
            }
            // objlogin.r3 = 3;
            return View(objlogin);
            // return View();
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
                        var obj = db.tblUserPushNotifyIds.Where(x => x.UserId == userId && x.PlayerId == deskPushId && x.PushType == "DESK" && x.UserType == "pbi").SingleOrDefault();
                        if (obj == null)
                        {
                            var tbl = new tblUserPushNotifyId()
                            {
                                PushType = "DESK",
                                PlayerId = deskPushId,
                                UserId = userId,
                                CreatedOn = DateTime.Now,
                                UserType = "pbi"

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
                    var rs = Functions.SendWebPush(userId, "RVNL-PrimaBi", "Welcome to PrimaBi " + username, "pbi");
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
            }
        }

        private void SetSession(int userId)
        {
            using (var db = new dbRVNLMISEntities())
            {
                var objUser = db.UserDetailsWithRoles.Where(o => o.UserId == userId).SingleOrDefault();

                if (objUser != null)
                {
                    UserModel objUserM = new UserModel();
                    objUserM.RoleTableID = (int)objUser.RoleTableId;
                    objUserM.UserName = objUser.UserName;
                    objUserM.UserId = objUser.UserId;
                    objUserM.RoleId = Convert.ToInt32(objUser.RoleId);
                    objUserM.RoleCode = objUser.RoleCode;
                    objUserM.EmailId = objUser.EmailId;
                    objUserM.Name = objUser.Name;
                    objUserM.TableDataName = objUser.TableDataName;
                    objUserM.TableDataCode = objUser.TableDataCode;
                    objUserM.Discipline = (int)objUser.Discipline;
                    Session["UserData"] = (UserModel)objUserM;
                    Session["UserName"] = objUser.UserName;
                    Session["PKGID"] = "0";
                    Session["ENTID"] = "0";
                }
            }
        }

        [Compress]
        [HttpPost]
        public ActionResult Login(LoginModel oModel)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return View("Index");
                }
                oModel.isActiveIssue = false;
                using (var db = new dbRVNLMISEntities())
                {
                    string Encryptpass = Functions.Encrypt(oModel.Password.Trim());
                    var objUser = db.UserDetailsWithRoles.Where(o => o.UserName == oModel.Username && o.Password == Encryptpass).SingleOrDefault();

                    if (objUser != null)
                    {
                        // FormsAuthentication.SetAuthCookie(objUser.UserName + "|" + objUser.UserId + "|" + objUser.Name + "|" + objUser.EmailId + "|" + objUser.RoleId + "|" + objUser.RoleTableId + "|" + objUser.TableDataName + "|" + objUser.TableName, false);
                        //one signal push notification, playerId registration
                        var playerId = Session["PlayerId"];
                        RegisterPushId(objUser.UserId, Convert.ToString(playerId), objUser.Name);
                        if (objUser.RoleCode == "PKG")
                        {
                            int subStatus = CheckIsSubscription(objUser.UserId);
                            if (subStatus == 200) // success
                            {
                                // check if user have active issues
                                var isActiveIssue = db.tblDataIssues.Any(a => a.PackageId == objUser.RoleTableId && a.StatusId != 4);
                                oModel.isActiveIssue = isActiveIssue;

                                SetFormAuthCookies(oModel, objUser);

                                SetSession(objUser.UserId);

                                CreateLog(objUser);

                                BreadCrumbModel obj = new BreadCrumbModel()
                                {
                                    MenuName = "Overall Dashboard",
                                    MainParent = string.Empty,
                                    SubParent = string.Empty
                                };
                                Session["SessBreadCrumb"] = obj;

                                var objContact = db.tblPackageUserContacts.Where(o => o.UserId == objUser.UserId).FirstOrDefault();
                                if (objContact != null)
                                {
                                    if (isActiveIssue)
                                    {
                                        TempData["AfterLogin"] = "1";
                                        SaveUserLog();
                                        return Json("6");
                                    }
                                    TempData["AfterLogin"] = "1";
                                    SaveUserLog();
                                    string returnUrls = !string.IsNullOrEmpty(oModel.returnUrl) ? oModel.returnUrl : "2";
                                    return Json(returnUrls);
                                    //return Json("2");
                                }
                                TempData["AfterLogin"] = "1";
                                SaveUserLog();
                                string returnUrl = !string.IsNullOrEmpty(oModel.returnUrl) ? oModel.returnUrl : "5";
                                return Json(returnUrl);
                                //return Json("5");


                            }
                            else if (subStatus == 406) //expired/inactive
                            {
                                return Json("3");
                            }
                            else
                            {
                                return Json("-1");
                            }
                        }
                        else
                        {
                            SetFormAuthCookies(oModel, objUser);

                            SetSession(objUser.UserId);

                            CreateLog(objUser);

                            BreadCrumbModel obj = new BreadCrumbModel()
                            {
                                MenuName = "Overall Dashboard",
                                MainParent = string.Empty,
                                SubParent = string.Empty
                            };
                            Session["SessBreadCrumb"] = obj;
                            TempData["AfterLogin"] = "1";
                            SaveUserLog();
                            string returnUrl = !string.IsNullOrEmpty(oModel.returnUrl) ? oModel.returnUrl : "0";
                            return Json(returnUrl);
                        }

                    }
                    else
                    {
                        return Json("1");   //username incorrect
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return Json("-1"); //Error
            }

        }

        public void SaveUserLog()
        {
            string IpAddress = "";
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Login Form", "Login", UserID, IpAddress, "NA");

        }
        private void CreateLog(UserDetailsWithRole objUser)
        {
            tblAudit audit = new tblAudit()
            {
                // Your Audit Identifier     
                // AuditID = Guid.NewGuid(),
                // Our Username (if available)
                UserId = objUser.UserId,
                UserName = objUser.UserName,
                // The IP Address of the Request
                IPAddress = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Request.UserHostAddress,
                // The URL that was accessed
                AreaAccessed = "/Login/Index",
                // Creates our Timestamp
                TimeAccessed = DateTime.Now
            };

            // Stores the Audit in the Database
            dbRVNLMISEntities context = new dbRVNLMISEntities();
            context.tblAudits.Add(audit);
            context.SaveChanges();
        }

        public HttpCookie CreateLoginCookie(string UserName, int UserId, string Name, DateTime validupto)
        {
            HttpCookie LoginCookies = new HttpCookie("RVNLMIS_Login");
            LoginCookies.Values.Add("UserName", Functions.Encrypt(UserName));
            //LoginCookies.Values.Add("UserId", UserId.ToString());
            //LoginCookies.Values.Add("Name", Name);

            if (!HttpContext.Request.Url.Host.Contains("localhost"))  //this check to make ur life easier on development
            {
                LoginCookies.Domain = Request.Url.Host;
            }
            LoginCookies.Expires = Convert.ToDateTime(validupto);
            return LoginCookies;
        }

        private void SetFormAuthCookies(LoginModel oModel, UserDetailsWithRole objUser)
        {
            DateTime utcNow = DateTime.UtcNow;

            DateTime utcExpires = oModel.RememberMe ? utcNow.AddDays(7) : utcNow.AddHours(24);

            // DateTime utcExpires = utcNow.AddDays(1);

            var authTicket = new FormsAuthenticationTicket(
                2,
                objUser.UserName + "|" + objUser.UserId + "|" + objUser.Name + "|" + oModel.isActiveIssue + "|" + objUser.RoleCode,
                utcNow,
                utcExpires,
                oModel.RememberMe,
                //true,
                string.Empty,
                "/"
            );

            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(authTicket));
            // cookie.Value = oModel.nameFormAuth.ToString();
            cookie.HttpOnly = true;

            if (oModel.RememberMe)
            {
                cookie.Expires = authTicket.Expiration;
            }

            Response.Cookies.Add(cookie);

            Response.Cookies.Add(CreateLoginCookie(objUser.UserName, objUser.UserId, objUser.Name, utcExpires));
        }

        public ActionResult LogOut()
        {

            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public JsonResult VerifyEmail(string email)
        {
            string _Emails = email;
            try
            {

                using (var db = new dbRVNLMISEntities())
                {

                    tblUserMaster objUser = db.tblUserMasters.SingleOrDefault(o => o.EmailId == _Emails);
                    string EmailBody = "Your User Name " + objUser.UserName + " & Password is " + Functions.Decrypt(objUser.Password);


                    if (!string.IsNullOrEmpty(objUser.EmailId))
                    {
                        Email obj = new Email();

                        List<Attachment> attachment = null;
                        List<string> cc = null;
                        List<string> bcc = null;
                        int rs = obj.SendMail(_Emails, cc, bcc, attachment, "Account information ", EmailBody, ConfigurationManager.AppSettings["FROM"]);
                    }
                }
                return Json("1");
            }
            catch (Exception ex)
            {
                return Json("-1");
            }
        }

        //[AllowAnonymous]
        //[SessionAuthorize]
        public ActionResult Lockscreen(string returnUrl)
        {

            LoginModel obj = new LoginModel();
            HttpCookie nameFormAuth = Request.Cookies[FormsAuthentication.FormsCookieName];
            HttpCookie LoginCookie = Request.Cookies["RVNLMIS_Login"];

            if (nameFormAuth != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(nameFormAuth.Value);
                if (ticket != null)
                {
                    if (ticket.Expiration > DateTime.Now)
                    {
                        string[] paramStr = ticket.Name.Split('|');
                        //int userId = Functions.ParseInteger(paramStr[1]);
                        string userName = paramStr[0];

                        obj.Username = userName;
                        obj.returnUrl = returnUrl;
                        obj.RememberMe = true;
                    }
                }
            }
            else if (LoginCookie != null)
            {
                string userName = Convert.ToString(Request.Cookies["RVNLMIS_Login"]["UserName"]);

                //if (LoginCookie.Expires > DateTime.Now)
                //{

                //if (LoginCookie != null)
                //{
                obj.Username = Functions.Decrypt(userName);
                obj.returnUrl = returnUrl;
                obj.RememberMe = true;
                // }
                // return RedirectToAction("LockScreen", "Login", new { returnUrl = returnUrl });
                // }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View(obj);
        }

        public ActionResult DeleteAuthCookies()
        {
            DeleteLoginCookies();

            HttpCookie nameFormAuth = Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthentication.SignOut();
            Session.Abandon();

            // clear authentication cookie
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            // clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
            SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
            HttpCookie cookie2 = new HttpCookie(sessionStateSection.CookieName, "");
            cookie2.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie2);
            return Json("1", JsonRequestBehavior.AllowGet);
        }

        private void DeleteLoginCookies()
        {
            if (Request.Cookies["RVNLMIS_Login"] != null)
            {
                HttpCookie aCookie = Request.Cookies["RVNLMIS_Login"];
                aCookie.Expires = DateTime.Now.AddDays(-10);
                aCookie.Value = "";
                Response.Cookies.Add(aCookie);
            }
        }

        [Compress]
        [HttpPost]
        public ActionResult UnlockScreen(LoginModel oModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Lockscreen");
                }
                using (var db = new dbRVNLMISEntities())
                {
                    string Encryptpass = Functions.Encrypt(oModel.Password.Trim());
                    var objUser = db.UserDetailsWithRoles.Where(o => o.UserName == oModel.Username && o.Password == Encryptpass).SingleOrDefault();

                    if (objUser != null)
                    {
                        if (objUser.RoleCode == "PKG")
                        {
                            int subStatus = CheckIsSubscription(objUser.UserId);
                            if (subStatus == 200) // success
                            {
                                // check if user have active issues
                                var isActiveIssue = db.tblDataIssues.Any(a => a.PackageId == objUser.RoleTableId && a.StatusId != 4);
                                oModel.isActiveIssue = isActiveIssue;
                                SetFormAuthCookies(oModel, objUser);

                                UserModel objUserM = new UserModel();
                                objUserM.UserName = objUser.UserName;
                                objUserM.RoleTableID = (int)objUser.RoleTableId;
                                objUserM.UserId = objUser.UserId;
                                objUserM.RoleId = Convert.ToInt32(objUser.RoleId);
                                objUserM.RoleCode = objUser.RoleCode;
                                objUserM.TableDataName = objUser.TableDataName;
                                Session["UserData"] = (UserModel)objUserM;
                                Session["UserName"] = objUser.UserName;

                                var objContact = db.tblPackageUserContacts.Where(o => o.UserId == objUser.UserId).FirstOrDefault();
                                if (objContact != null)
                                {
                                    if (isActiveIssue)
                                    {
                                        return Json("6");
                                    }
                                    string returnUrl = !string.IsNullOrEmpty(oModel.returnUrl) ? oModel.returnUrl : "3";
                                    return Json(returnUrl);
                                    //return Json("3");
                                }
                                return Json("5");

                            }
                            else if (subStatus == 406) //expired/inactive
                            {
                                return Json("4");
                            }
                            else
                            {
                                return Json("-1");
                            }
                        }
                        else
                        {
                            SetFormAuthCookies(oModel, objUser);

                            UserModel objUserM = new UserModel();
                            objUserM.UserName = objUser.UserName;
                            objUserM.UserId = objUser.UserId;
                            objUserM.RoleId = Convert.ToInt32(objUser.RoleId);
                            objUserM.RoleCode = objUser.RoleCode;
                            objUserM.TableDataName = objUser.TableDataName;
                            Session["UserData"] = (UserModel)objUserM;
                            Session["UserName"] = objUser.UserName;

                            if (!string.IsNullOrEmpty(oModel.returnUrl))
                            {
                                return Json("2");
                            }

                            string returnUrl = !string.IsNullOrEmpty(oModel.returnUrl) ? oModel.returnUrl : "0";
                            return Json(returnUrl);
                            //return Json("0");
                        }
                    }
                    else
                    {
                        return Json("1");   //username incorrect
                    }
                }
            }
            catch (Exception ex)
            {
                return Json("-1"); //Error
            }


        }

        #region -- Menu List --

        [Authorize]
        public ActionResult GetMenuList()
        {
            try
            {
                int roleId = string.IsNullOrEmpty(Session["UserData"].ToString()) ? 0 : ((UserModel)Session["UserData"]).RoleId;

                if (roleId == 0)
                {
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var result = (from m in db.tblAppMenus
                                      join rm in db.tblRoleMenuAccesses on m.MenuId equals rm.MenuId
                                      where m.IsDeleted == false && rm.RoleId == roleId
                                      select new { m, rm }).AsEnumerable().Select(s => new MenuModel
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
                        return PartialView("_MenuList", result);
                    }
                }
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

        public int CheckIsSubscription(int userId)
        {
            int statusId = 0;
            string instanceUrl = ConfigurationManager.AppSettings["ServerPath"];

            using (var client = new HttpClient())
            {
                if (instanceUrl == "https://dev.primabi.com" || instanceUrl == "http://localhost:62555")
                {
                    statusId = 200;
                }
                else
                {
                    client.BaseAddress = new Uri("https://admin.primabi.com/");
                    var responseTask = client.GetAsync("api/SubscriptionStatus?id=" + userId);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    statusId = (int)result.StatusCode;
                }
            }
            return statusId;
        }

        [HttpPost]
        public ActionResult KeepSessionAlive()
        {
            return Json(new { Data = "Beat Generated" });
        }
    }
}