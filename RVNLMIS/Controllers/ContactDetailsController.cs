using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.Common;
using System.Configuration;
using PrimaBiWeb.Common;

namespace RVNLMIS.Controllers
{
    public class ContactDetailsController : Controller
    {
        // GET: ContactDetails
        public ActionResult Index()
        {
            var objUserM = (UserModel)Session["UserData"];
            if (objUserM != null)
            {
                ContactDetailsModel obj = new ContactDetailsModel();

                obj.UserId = objUserM.UserId;
                obj.PackageId = objUserM.RoleTableID;

                using (var db = new dbRVNLMISEntities())
                {
                    obj.PackageInfo = Convert.ToString(db.tblPackages.Where(o => o.PackageId == objUserM.RoleTableID).FirstOrDefault().PMC);
                }
                return View(obj);
            }
            else
            {
                return RedirectToAction("LockScreen", "Login");
            }

        }

        public ActionResult AddUserContact(ContactDetailsModel obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index");
                }
                using (var db = new dbRVNLMISEntities())
                {
                    tblPackageUserContact tblobj = new tblPackageUserContact();
                    tblobj.PackageId = obj.PackageId;
                    tblobj.UserId = obj.UserId;
                    tblobj.Email = obj.Email;
                    tblobj.FullName = obj.FullName;
                    tblobj.Mobile = "NA";
                    tblobj.WhatsappNo = obj.WhatsappNo;
                    tblobj.NoOfUser = 0;
                    tblobj.IsAppUser = obj.IsAppUser;
                    db.tblPackageUserContacts.Add(tblobj);
                    db.SaveChanges();
                    if (tblobj.AutoId != 0)
                    {
                        tblResidentEnggDetail tblobject = new tblResidentEnggDetail();
                        tblobject.AutoId = tblobj.AutoId;

                        tblobject.ResidentEngineerName = obj.ResidentEngineerName;
                        tblobject.ResidentEngineerName2 = obj.ResidentEngineerName2 == null ? "-" : obj.ResidentEngineerName2;
                        tblobject.ReWhatsAppNum = obj.ReWhatsAppNum;
                        tblobject.Re2WhatsAppNum = obj.Re2WhatsAppNum == null ? "-" : obj.Re2WhatsAppNum;
                        tblobject.IsDeleted = false;
                        tblobject.CreatedOn = DateTime.UtcNow;
                        db.tblResidentEnggDetails.Add(tblobject);
                        db.SaveChanges();
                    }
                }
                if (!obj.IsAppUser)
                {
                    try
                    {
                        string _Mobiles = obj.ReWhatsAppNum.Trim() + "," + obj.WhatsappNo.Trim();
                        SMSSend.SendSMS(_Mobiles, ConfigurationManager.AppSettings["AppLinkSMS"]);
                    }
                    catch (Exception)
                    {
                    }
                }

                return Json("1");
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }
    }
}