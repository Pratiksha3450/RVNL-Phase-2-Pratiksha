using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [Authorize]
    public class ChangePasswordController : Controller
    {
        // GET: ChangePassword
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult PasswordChange(string oldPassword, string newPassword)
        {
            string _oldPassword = oldPassword;
            string _newPassword = newPassword;
            try
            {
                string UserName = string.Empty;
                string OldPassword = Functions.Encrypt(oldPassword);
                using (var db = new dbRVNLMISEntities())
                {
                    UserModel objUser = new UserModel();
                    UserName = Convert.ToString(Session["UserName"]);
                    tblUserMaster objUserDetails = db.tblUserMasters.SingleOrDefault(o => o.Password == OldPassword && o.UserName == UserName);
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
    }
}