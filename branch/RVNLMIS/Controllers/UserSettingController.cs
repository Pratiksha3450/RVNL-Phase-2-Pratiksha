using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    public class UserSettingController : Controller
    {
        // GET: Setting
        public ActionResult Index()
        {
            int userId = ((UserModel)Session["UserData"]).UserId;
            string profilepic = string.Empty;
            UserSettingModel objModel = new UserSettingModel();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                objModel.objUser = dbContext.tblUserMasters.Where(u => u.UserId == userId && u.IsDeleted == false)
                    .AsEnumerable()
                    .Select(u => new UserModel
                    {
                        UserId = u.UserId,
                        EmailId = u.EmailId,
                        Name = u.Name,
                        MobileNo = u.MobileNo,
                        ProfilePic = u.ProfilePic
                    })
                    .FirstOrDefault();

                if (objModel.objUser.ProfilePic != null)
                {
                    objModel.UserImagePath = string.Format("/Uploads/ProfilePictures/{0}/{1}", objModel.objUser.UserId, objModel.objUser.ProfilePic);
                }

                var getNotObj = dbContext.tblNotificationSettings.Where(n => n.UserId == userId).FirstOrDefault();

                NotifyModel obj = new NotifyModel();
                if (getNotObj != null)
                {
                    obj.Email = getNotObj.IsEmail ?? false;
                    obj.SMS = getNotObj.IsSMS ?? false;
                    obj.Whatsapp = getNotObj.IsWhatsApp ?? false;
                    obj.AppPushNotification = getNotObj.IsAppPush ?? false;
                    obj.DesktopPushNotification = getNotObj.IsDesktopPush ?? false;
                    obj.PopupNotification = getNotObj.IsPopup ?? false;
                }
                objModel.objNotify = obj;
            }
            return View(objModel);
        }

        [HttpPost]
        public ActionResult EditProfileSubmit(UserSettingModel objModel)
        {
            //string filePath = string.Empty;
            //string fileName = string.Empty;

            try
            {
                //if (objModel.UserImage != null)
                //{
                //    //save image to folder
                //    FileInfo fi = new FileInfo(objModel.UserImage.FileName);
                //    var strings = new List<string> { ".png", ".jpg", ".jpeg" };
                //    bool contains = strings.Contains(fi.Extension, StringComparer.OrdinalIgnoreCase);

                //    if (!contains)
                //    {
                //        return Json("0", JsonRequestBehavior.AllowGet);
                //    }

                //    string localPath = string.Format("~/Uploads/ProfilePictures/{0}", objModel.objUser.UserId);
                //    Functions.CreateIfMissing(Server.MapPath(localPath));

                //    string getFileName = Path.GetFileName(objModel.UserImage.FileName);
                //    fileName = string.Concat(objModel.objUser.UserId + "-" + getFileName.Replace(' ', '_'));
                //    filePath = string.Format("/Uploads/ProfilePictures/{0}/{1}", objModel.objUser.UserId, fileName);

                //    objModel.UserImage.SaveAs(Server.MapPath(filePath));
                //}

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var getObj = dbContext.tblUserMasters.Where(u => u.UserId == objModel.objUser.UserId && u.IsDeleted == false).FirstOrDefault();
                    getObj.Name = objModel.objUser.Name;
                    getObj.EmailId = objModel.objUser.EmailId;
                    getObj.MobileNo = objModel.objUser.MobileNo;
                    // getObj.ProfilePic = fileName;

                    dbContext.SaveChanges();

                    return Json(new { message = "1", name = getObj.Name, mobile = getObj.MobileNo, email = getObj.EmailId }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("2", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Upload()
        {
            string filePath = string.Empty;
            string fileName = string.Empty;
            int userId = ((UserModel)Session["UserData"]).UserId;

            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase file = Request.Files[i]; //Uploaded file

                //save image to folder
                FileInfo fi = new FileInfo(file.FileName);
                var strings = new List<string> { ".png", ".jpg", ".jpeg" };
                bool contains = strings.Contains(fi.Extension, StringComparer.OrdinalIgnoreCase);

                if (!contains)
                {
                    return Json("0", JsonRequestBehavior.AllowGet);
                }

                string localPath = string.Format("~/Uploads/ProfilePictures/{0}", userId);
                Functions.CreateIfMissing(Server.MapPath(localPath));

                string getFileName = Path.GetFileName(file.FileName);
                fileName = string.Concat(userId + "-" + getFileName.Replace(' ', '_'));
                filePath = string.Format("/Uploads/ProfilePictures/{0}/{1}", userId, fileName);

                file.SaveAs(Server.MapPath(filePath));

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var getObj = dbContext.tblUserMasters.Where(u => u.UserId == userId && u.IsDeleted == false).FirstOrDefault();
                    getObj.ProfilePic = fileName;

                    dbContext.SaveChanges();
                }
            }
            return Json("1", JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemovePhoto()
        {
            int userId = ((UserModel)Session["UserData"]).UserId;
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var getObj = dbContext.tblUserMasters.Where(u => u.UserId == userId && u.IsDeleted == false).FirstOrDefault();
                getObj.ProfilePic = null;

                dbContext.SaveChanges();
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateNotifySetting(FormCollection fc)
        {
            int userId = ((UserModel)Session["UserData"]).UserId;

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var getNotObj = dbContext.tblNotificationSettings.Where(n => n.UserId == userId).FirstOrDefault();

                    if (getNotObj == null)
                    {
                        tblNotificationSetting objAdd = new tblNotificationSetting();
                        objAdd.IsEmail = fc["Email"] == "on" ? true : false;
                        objAdd.IsSMS = fc["SMS"] == "on" ? true : false;
                        objAdd.IsWhatsApp = fc["Whatsapp"] == "on" ? true : false;
                        objAdd.UserId = userId;
                        objAdd.IsAppPush = fc["AppPushNotification"] == "on" ? true : false;
                        objAdd.IsDesktopPush = fc["DesktopPushNotification"] == "on" ? true : false;
                        objAdd.IsPopup = fc["PopupNotification"] == "on" ? true : false;

                        dbContext.tblNotificationSettings.Add(objAdd);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        getNotObj.IsEmail = fc["Email"] == "on" ? true : false;
                        getNotObj.IsSMS = fc["SMS"] == "on" ? true : false;
                        getNotObj.IsWhatsApp = fc["Whatsapp"] == "on" ? true : false;
                        getNotObj.IsAppPush = fc["AppPushNotification"] == "on" ? true : false;
                        getNotObj.IsDesktopPush = fc["DesktopPushNotification"] == "on" ? true : false;
                        getNotObj.IsPopup = fc["PopupNotification"] == "on" ? true : false;

                        dbContext.SaveChanges();
                    }
                }
                return Json("1", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("2", JsonRequestBehavior.AllowGet);
            }
        }
    }
}