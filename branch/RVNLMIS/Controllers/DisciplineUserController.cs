using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class DisciplineUserController : Controller
    {
        public string IpAddress = "";
        // GET: DisciplineUser
        [PageAccessFilter]
        public ActionResult Index()
        {
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Discipline User", "View", UserID, IpAddress, "NA");
            GetRoleList();
            return View();
        }

        private void GetRoleList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                //var _RolesList = dbContext.tblRoles.ToList();
                //ViewBag.RolesList = new SelectList(_RolesList, "RoleId", "RoleName");

                int packageId = ((UserModel)Session["UserData"]).RoleTableID;

                var _dispList = dbContext.tblDisciplines.Where(d => d.IsDeleted == false).ToList();
                ViewBag.DisciplineList = new SelectList(_dispList, "DispId", "DispName");

                var entityPackageUserList = (from e in dbContext.tblPackages
                                             where e.PackageId == packageId
                                             select new { e }).AsEnumerable().Select(s => new EntityRoleTableOptions()
                                             {
                                                 Id = s.e.PackageId,
                                                 Name = s.e.PackageName
                                             }).ToList();
                ViewBag.RoleTableList = new SelectList(entityPackageUserList, "Id", "Name");
            }
        }

        [HttpPost]
        [Audit]
        public ActionResult AddDisciplineUser(DisciplineUserModel oModel)
        {
            int userid = oModel.UserId;
            ViewBag.RoleTableList = new SelectList(new List<DisciplineUserModel>(), "RoleTableId", "RoleTableName");
            try
            {
                if (ModelState.IsValid)
                {
                    // int _UserId = 0;
                    if (userid == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var Res = db.tblUserMasters.Where(o => o.UserName == oModel.UserName && o.IsDeleted == false).ToList();
                            if (Res.Count > 0)
                            {
                                return Json("0");
                            }
                            else
                            {
                                tblUserMaster objUser = new tblUserMaster();
                                objUser.Name = oModel.Name;
                                objUser.UserName = oModel.UserName;
                                int PasswordLength = Functions.ParseInteger(WebConfigurationManager.AppSettings["PasswordLength"]);
                                objUser.Password = Functions.Encrypt(Functions.GeneratePassword(PasswordLength));
                                objUser.EmailId = oModel.EmailId;
                                objUser.MobileNo = oModel.MobileNo;
                                objUser.RoleId = 700;
                                objUser.RoleTableId = oModel.RoleTableId;
                                objUser.Discipline = oModel.DisciplineId;
                                objUser.IsActive = true;
                                objUser.IsDeleted = false;
                                objUser.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblUserMasters.Add(objUser);
                                db.SaveChanges();
                                // _UserId = objUser.UserId;

                                //var oUserDetails = db.SpGetPackageByRoleId(objUser.RoleId, objUser.RoleTableId);
                                //foreach (var o in oUserDetails)
                                //{
                                //    tblUserDataAccess objUserDataAccess = new tblUserDataAccess();
                                //    objUserDataAccess.PackageId = o.PackageId;
                                //    objUserDataAccess.UserId = _UserId;
                                //    db.tblUserDataAccesses.Add(objUserDataAccess);
                                //}
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                int k = Functions.SaveUserLog(pkgId, "Discipline User", "Save", UserID, IpAddress, "Username: "+ oModel.UserName);
                            }
                            //db.SaveChanges();
                            return Json("1");

                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var Res = db.tblUserMasters.Where(o => o.UserName == oModel.UserName && o.IsDeleted == false).ToList();

                            tblUserMaster objUser = db.tblUserMasters.Where(o => o.UserId == oModel.UserId).SingleOrDefault();

                            if (objUser.UserName != oModel.UserName)
                            {
                                if (Res != null)
                                {
                                    return Json("0", JsonRequestBehavior.AllowGet);
                                }
                            }

                            objUser.Name = oModel.Name;
                            objUser.UserName = oModel.UserName;
                            objUser.Password = oModel.Password;
                            objUser.EmailId = oModel.EmailId;
                            objUser.MobileNo = oModel.MobileNo;
                            objUser.RoleId = 700;
                            objUser.RoleTableId = oModel.RoleTableId;
                            objUser.Discipline = oModel.DisciplineId;
                            objUser.IsActive = true;
                            objUser.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                            db.SaveChanges();
                            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(IpAddress))
                            {
                                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                            }
                            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                            int UserID = ((UserModel)Session["UserData"]).UserId;

                          
                            try
                            {
                                string str = ""; ;
                                var UpdatedValue = (from ul in db.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                                if (UpdatedValue != null)
                                {
                                    str = UpdatedValue.Replace("CreatedOn, ", "");
                                    str = str.Replace("CreatedOn", "");

                                    str = str.TrimStart(',');
                                    str = str.Replace(", ,", ",");
                                    str = str.TrimEnd(',');
                                    if (String.IsNullOrEmpty(str))
                                    {
                                        str = "Username: " + oModel.UserName + "= NA ";
                                    }
                                    else
                                    {
                                        str = "Username: " + oModel.UserName + "= " + str.TrimStart(',');
                                    }


                                    
                                }
                                else
                                {
                                    str = "NA";
                                }

                                int k = Functions.SaveUserLog(pkgId, "Discipline User", "Update", UserID, IpAddress, str);
                            }
                            catch (Exception ex)
                            {

                            }


                            // int k = Functions.SaveUserLogs(pkgId, "Discipline User", "Update", UserID, IpAddress);
                            //_UserId = objUser.UserId;

                            //var oUserId = db.tblUserDataAccesses.FirstOrDefault(s => s.UserId == _UserId);
                            //if (oUserId != null)
                            //{
                            //    db.tblUserDataAccesses.Remove(oUserId);
                            //    db.SaveChanges();

                            //}
                            //var oUserDetails = db.SpGetPackageByRoleId(objUser.RoleId, objUser.RoleTableId);
                            //foreach (var o in oUserDetails)
                            //{
                            //    tblUserDataAccess objUserDataAccess = new tblUserDataAccess();
                            //    objUserDataAccess.PackageId = o.PackageId;
                            //    objUserDataAccess.UserId = _UserId;
                            //    db.tblUserDataAccesses.Add(objUserDataAccess);

                            //}
                            //db.SaveChanges();
                            return Json("2");
                        }
                    }

                }
                else
                {
                    GetRoleList();
                    ModelState.Clear();
                    return View("Index", oModel);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [Audit]
        public ActionResult EditUserByUserId(int id)
        {

            GetRoleList();
            // ViewBag.RoleTableList = new SelectList(new List<DisciplineUserModel>(), "RoleTableId", "RoleTableName");
            int userId = id;
            DisciplineUserModel objModelView = new DisciplineUserModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oUserDetails = db.tblUserMasters.Where(o => o.UserId == id).SingleOrDefault();
                        if (oUserDetails != null)
                        {
                            objModelView.UserId = oUserDetails.UserId;
                            objModelView.UserName = oUserDetails.UserName;
                            objModelView.Password = oUserDetails.Password;
                            objModelView.EmailId = oUserDetails.EmailId;
                            objModelView.MobileNo = oUserDetails.MobileNo;
                            objModelView.DisciplineId = (int)oUserDetails.Discipline;
                            objModelView.Name = oUserDetails.Name;
                            objModelView.RoleId = (int)oUserDetails.RoleId;
                            objModelView.RoleName = db.tblRoles.Where(o => o.RoleId == (int)oUserDetails.RoleId).SingleOrDefault().RoleName;
                            //if (oUserDetails.RoleTableId != null)
                            //{
                            //    GetRoleTableDetailsByRoleName(objModelView.RoleName, (int)oUserDetails.RoleTableId);
                            //}
                            objModelView.RoleTableId = oUserDetails.RoleTableId ?? 0;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialAddDisciplineUser", objModelView);

        }

        [HttpPost]
        public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

        [HttpPost]
        public ActionResult Pdf_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

        #region --- List UserMasters Values ---

        public ActionResult User_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                int packageId = ((UserModel)Session["UserData"]).RoleTableID;

                var lst = (from x in dbContext.UserDetailsWithRoles
                           join d in dbContext.tblDisciplines on x.Discipline equals d.DispId
                           where x.RoleTableId == packageId
                           select new { x, d })
                            .AsEnumerable().Select(s =>
                               new DisciplineUserModel
                               {
                                   UserId = s.x.UserId,
                                   Name = s.x.Name,
                                   UserName = s.x.UserName,
                                   EmailId = s.x.EmailId,
                                   Password = Functions.Decrypt(s.x.Password),
                                   MobileNo = s.x.MobileNo,
                                   RoleId = (int)s.x.RoleId,
                                   RoleName = s.x.RoleName,
                                   DisciplineName = s.d.DispName,
                                   RoleTableId = (int)s.x.RoleTableId,
                                   RoleTableName = s.x.TableName,
                                   TableDataName = s.x.TableDataName,
                                   CreatedOn = s.x.CreatedOn,
                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        [HttpPost]
        [Audit]
        public JsonResult Delete(int id)
        {
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {
                    //db.tblSections.RemoveRange(db.tblSections.Where(o => o.SectionID == id).ToList());
                    tblUserMaster objUser = db.tblUserMasters.SingleOrDefault(o => o.UserId == id);
                    objUser.IsDeleted = true;
                    // _data = CreateData();
                    db.SaveChanges();
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    int k = Functions.SaveUserLog(pkgId, "Discipline User", "Delete", UserID, IpAddress, "Username: " + objUser.UserName);

                }
                return Json("1");
            }
            catch
            {
                // _data = CreateData();
                return Json("-1");
            }
        }
    }
}