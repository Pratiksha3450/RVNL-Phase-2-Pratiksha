using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using PrimaBiWeb.Common;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [Authorize]
    public class RFIUserController : Controller
    {
        // GET: RFIModule/RFIUser
        public ActionResult Index()
        {
            BindDropdown();
            return View();
        }

        #region --- List RFI Users details ---

        public ActionResult Read_RFIUsers([DataSourceRequest]  DataSourceRequest request)
        {
            var pkgs = Functions.GetRoleAccessiblePackageList();
            var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<RFIUserModel> lst = (from s in dbContext.ViewRFIUsersDetails
                                          select new { s}).AsEnumerable()
                                          .Select(u=>new RFIUserModel
                                          {
                                              RFIUserId = u.s.RFIUserId,
                                              FullName = u.s.FullName,
                                              Password= Functions.Decrypt(u.s.Password),
                                              Designation = u.s.Designation,
                                              Email = u.s.Email,
                                              Mobile = u.s.Mobile,
                                              PackgeId =(int)u.s.PackgeId,
                                              PackageName = u.s.PackageName,
                                              Organisation = u.s.Organisation,
                                              ReportingToUser = u.s.ReportingToUser,
                                          }).ToList();
                lst = lst.Where(w => accessiblePackageList.Contains(w.PackgeId)).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region -----ADD EDIT USER DETAILS-----
        [HttpPost]
        public async Task<ActionResult> AddEditRFIUser(RFIUserModel objModel)
        {
            string message = string.Empty;
            string orgName = GetOrgNameById(Convert.ToInt32(objModel.Organisation));

            if (!ModelState.IsValid)
            {
                BindDropdown();
                return View("_PartialAddEditRfiUser", objModel);
            }
            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    if (objModel.RFIUserId == 0)
                    {
                        #region ---- CHECK IF EXISTS WHEN ADD USER------
                        var isEmailEnteredExist = dbContext.tblRFIUsers.Where(e => e.Email == objModel.Email).FirstOrDefault();

                        if (isEmailEnteredExist != null)
                        {
                            return Json("3", JsonRequestBehavior.AllowGet);  ////////Email already exists
                        }
                        else if (dbContext.tblRFIUsers.Where(e => e.Mobile == objModel.Mobile).FirstOrDefault() != null)
                        {
                            return Json("4", JsonRequestBehavior.AllowGet); ////////Mobile Already exists
                        }

                        #endregion

                        #region ----ADD USER -----

                        tblRFIUser objAdd = new tblRFIUser();
                        objAdd.PackgeId = objModel.PackgeId;
                        objAdd.FullName = objModel.FullName;
                        objAdd.Email = objModel.Email;
                        objAdd.DesignationId = objModel.DesignationId;
                        objAdd.Organisation = orgName;
                        objAdd.Mobile = objModel.Mobile;
                        objAdd.ReportingTo = objModel.ReportingTo;
                        objAdd.Password = Functions.Encrypt(objModel.Password);
                        dbContext.tblRFIUsers.Add(objAdd);
                        dbContext.SaveChanges();

                        #endregion

                        #region ---- Send Notification -------

                        if (objModel.SendNotification)
                        {
                            try
                            {
                                string smsText = GlobalVariables.RFIUserAddSMS.Replace("#email#", objModel.Email);
                                smsText = smsText.Replace("#password#", objModel.Password);
                                SMSSend.SendSMS(objModel.Mobile, smsText);
                                int rs = await SendMail(objModel.Email, objModel.Password, objModel.FullName);
                            }
                            catch (Exception ex)
                            {
                                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                                // Logger.LogInfo(ex);
                            }
                        }

                        #endregion

                        message = "1";
                    }
                    else
                    {
                        #region ----- CHECK IF EXISTS WHEN UPDATE USER------

                        var isMailEnteredExist = dbContext.tblRFIUsers.Where(e => e.Email == objModel.Email && e.RFIUserId != objModel.RFIUserId).FirstOrDefault();

                        if (isMailEnteredExist != null)
                        {
                            return Json("3", JsonRequestBehavior.AllowGet);      ////////Email already exists
                        }
                        else if (dbContext.tblRFIUsers.Where(e => e.Mobile == objModel.Mobile && e.RFIUserId != objModel.RFIUserId).FirstOrDefault() != null)
                        {
                            return Json("4", JsonRequestBehavior.AllowGet);     ////////Mobile Already exists
                        }
                        #endregion

                        #region -----UPDATE USER-------

                        var objEdit = dbContext.tblRFIUsers.Where(u => u.RFIUserId == objModel.RFIUserId).FirstOrDefault();
                        // objEdit.PackgeId = objModel.PackgeId;
                        objEdit.FullName = objModel.FullName;
                        objEdit.Email = objModel.Email;
                        objEdit.DesignationId = objModel.DesignationId;
                        objEdit.Organisation = orgName;
                        objEdit.Mobile = objModel.Mobile;
                        objEdit.ReportingTo = objModel.ReportingTo;
                        //objEdit.Password = Functions.Encrypt(objModel.Password);
                        dbContext.SaveChanges();


                        #endregion

                        message = "2";
                    }
                }
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditRfiUser(int id)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                BindDropdown();
                RFIUserModel objModel = new RFIUserModel();

                if (id != 0)
                {
                    var objEdit = dbContext.tblRFIUsers.Where(u => u.RFIUserId == id).FirstOrDefault();

                    if (objEdit != null)
                    {
                        int orgId = dbContext.tblOrganisations.Where(o => o.OrgName == objEdit.Organisation).Select(i => i.OrganisationId).FirstOrDefault();
                        objModel.RFIUserId = objEdit.RFIUserId;
                        objModel.PackgeId = (int)objEdit.PackgeId;
                        objModel.Password = objEdit.Password;
                        objModel.FullName = objEdit.FullName;
                        objModel.Email = objEdit.Email;
                        objModel.DesignationId = objEdit.DesignationId;
                        objModel.Organisation = orgId.ToString();
                        objModel.Mobile = objEdit.Mobile;
                        objModel.ReportingTo = objEdit.ReportingTo;
                    }
                }
                return View("_PartialAddEditRfiUser", objModel);
            }
        }


        #endregion

        #region ----DELETE USER----

        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                dbContext.tblRFIUsers.Remove(dbContext.tblRFIUsers.Where(r => r.RFIUserId == id).FirstOrDefault());
                dbContext.SaveChanges();
            }
            return Json("1", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ------SUPPORTIVE METHODS -----

        private Task<int> SendMail(string email, string password, string name)
        {
            int result = 0;
            Email objEmail = new Email();
            string txtFile = Server.MapPath("~/Common/EmailTemplate/RFIUserSuccess.txt");//get location of file
            string body = System.IO.File.ReadAllText(txtFile); //get all file textfile data in string
            OrderedDictionary EmailPlaceholderObject = new OrderedDictionary();
            EmailPlaceholderObject.Add("password", password);
            EmailPlaceholderObject.Add("email", email);
            EmailPlaceholderObject.Add("Fname", name);

            string msgBody = objEmail.SetTemplate(body, EmailPlaceholderObject);
            string msgSubject = "PrimaBI - Registration Success";

            result = objEmail.SendMail(email, null, null, null, msgSubject, msgBody, ConfigurationManager.AppSettings["SUPPORTFROM"]);
            return Task.FromResult<int>(result);
        }

        public JsonResult Get_Designation(int? orgId)
        {
            List<DropDownOptionModel> degList = new List<DropDownOptionModel>();
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    degList = dbContext.tblRFIDesignations.Where(o => o.OrganisationId == orgId)
                    .Select(s => new DropDownOptionModel { ID = s.RFIDesignId, Name = s.Designation })
                    .ToList();
                    return Json(degList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(degList, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult Get_ReportingToList(int? orgId)
        {
            List<DropDownOptionModel> userList = new List<DropDownOptionModel>();
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    if (orgId != null)
                    {
                        string orgName = GetOrgNameById(orgId);
                        userList = dbContext.tblRFIUsers.Where(o => o.Organisation == orgName)
                           .Select(s => new DropDownOptionModel { ID = s.RFIUserId, Name = s.FullName })
                           .ToList();
                    }
                    return Json(userList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(userList, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public string GetOrgNameById(int? id)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                return dbContext.tblOrganisations.Where(o => o.OrganisationId == id).Select(s => s.OrgName).FirstOrDefault();
            }
        }

        public void BindDropdown()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var packages = Functions.GetRoleAccessiblePackageList();
                ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");

                var org = dbContext.tblOrganisations.ToList();
                ViewBag.OrgList = new SelectList(org, "OrganisationId", "OrgName");
            }
        }

        #endregion
    }
}