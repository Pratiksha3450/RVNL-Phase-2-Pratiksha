using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class UsersController : Controller
    {
        [PageAccessFilter]
        // GET: Users
        public ActionResult Index()
        {
            GetRoleList();
            ViewBag.RoleTableList = new SelectList(new List<UserViewModel>(), "RoleTableId", "RoleTableName");
            return View();
        }

        private void GetRoleList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var _RolesList = dbContext.tblRoles.ToList();
                ViewBag.RolesList = new SelectList(_RolesList, "RoleId", "RoleName");
            }
        }
        private int? ConvertToDbInt(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return int.Parse(text);
        }
        [HttpPost]
        public ActionResult AddUserDetails(UserViewModel oModel)
        {
            GetRoleList();
            int userid = oModel.UserId;
            ViewBag.RoleTableList = new SelectList(new List<UserViewModel>(), "RoleTableId", "RoleTableName");
            try
            {
                if (ModelState.IsValid)
                {
                    int _UserId = 0;
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
                                objUser.RoleId = oModel.RoleId;
                                objUser.RoleTableId = oModel.RoleTableId ?? 0;
                                objUser.Discipline = 0;
                                objUser.IsActive = true;
                                objUser.IsDeleted = false;
                                objUser.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblUserMasters.Add(objUser);
                                db.SaveChanges();
                                _UserId = objUser.UserId;

                                if (objUser.RoleId == 800)
                                {
                                    tblUserDataAccess objUserDataAccess = new tblUserDataAccess();
                                    if (!string.IsNullOrEmpty(Session["customPackageList"].ToString()))
                                    {
                                        string strValue = Session["customPackageList"].ToString();
                                        string[] strArray = strValue.Split(',');

                                        foreach (object item in strArray)
                                        {
                                            objUserDataAccess.PackageId = Functions.ParseInteger(item.ToString());
                                            objUserDataAccess.UserId = _UserId;
                                            db.tblUserDataAccesses.Add(objUserDataAccess);
                                            db.SaveChanges();
                                        }

                                    }
                                }
                                else
                                {
                                    var oUserDetails = db.SpGetPackageByRoleId(objUser.RoleId, objUser.RoleTableId).ToList();
                                    foreach (var o in oUserDetails)
                                    {
                                        tblUserDataAccess objUserDataAccess = new tblUserDataAccess();
                                        objUserDataAccess.PackageId = o.PackageId;
                                        objUserDataAccess.UserId = _UserId;
                                        db.tblUserDataAccesses.Add(objUserDataAccess);
                                        db.SaveChanges();
                                    }

                                }
                            }
                            return Json("1");

                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var Res = db.tblUserMasters.Where(o => (o.UserName == oModel.UserName && o.IsDeleted == false) && (o.UserId != oModel.UserId)).ToList();
                            if (Res.Count != 0)
                            {
                                return Json("0", JsonRequestBehavior.AllowGet);
                            }

                            tblUserMaster objUser = db.tblUserMasters.Where(o => o.UserId == oModel.UserId).FirstOrDefault();
                            objUser.Name = oModel.Name;
                            objUser.UserName = oModel.UserName;
                            objUser.Password = oModel.Password;
                            objUser.EmailId = oModel.EmailId;
                            objUser.MobileNo = oModel.MobileNo;
                            objUser.RoleId = oModel.RoleId;
                            objUser.RoleTableId = oModel.RoleTableId ?? 0;
                            objUser.Discipline = 0;
                            objUser.IsActive = true;
                            objUser.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                            db.SaveChanges();
                            _UserId = objUser.UserId;

                            var oUserId = db.tblUserDataAccesses.FirstOrDefault(s => s.UserId == _UserId);
                            if (oUserId != null)
                            {
                                db.tblUserDataAccesses.RemoveRange(db.tblUserDataAccesses.Where(x => x.UserId == _UserId));
                                db.SaveChanges();

                            }
                            if (objUser.RoleId == 800)
                            {
                                tblUserDataAccess objUserDataAccess = new tblUserDataAccess();
                                if (!string.IsNullOrEmpty(Session["customPackageList"].ToString()))
                                {
                                    string strValue = Session["customPackageList"].ToString();
                                    string[] strArray = strValue.Split(',');
                                    if (strArray != null)
                                    {
                                        foreach (object item in strArray)
                                        {
                                            objUserDataAccess.PackageId = Functions.ParseInteger(item.ToString());
                                            objUserDataAccess.UserId = _UserId;
                                            db.tblUserDataAccesses.Add(objUserDataAccess);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var oUserDetails = db.SpGetPackageByRoleId(objUser.RoleId, objUser.RoleTableId).ToList();
                                foreach (var o in oUserDetails)
                                {
                                    tblUserDataAccess objUserDataAccess = new tblUserDataAccess();
                                    objUserDataAccess.PackageId = o.PackageId;
                                    objUserDataAccess.UserId = _UserId;
                                    db.tblUserDataAccesses.Add(objUserDataAccess);

                                }
                                db.SaveChanges();
                            }

                            return Json("2");
                        }
                    }

                }
                else
                {
                    ModelState.Clear();
                    return View("Index", oModel);
                }

            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }

        public ActionResult EditUserByUserId(int id)
        {

            GetRoleList();
            ViewBag.RoleTableList = new SelectList(new List<UserViewModel>(), "RoleTableId", "RoleTableName");
            int userId = id;
            UserViewModel objModelView = new UserViewModel();
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
                            objModelView.Name = oUserDetails.Name;
                            objModelView.RoleId = (int)oUserDetails.RoleId;
                            // objModelView.PackageList = db.PackagelistByUserId(oUserDetails.UserId).FirstOrDefault();

                            objModelView.RoleName = db.tblRoles.Where(o => o.RoleId == (int)oUserDetails.RoleId).SingleOrDefault().RoleName;
                            if (oUserDetails.RoleTableId != null)
                            {
                                GetRoleTableDetailsByRoleName(objModelView.RoleName, (int)oUserDetails.RoleTableId);
                            }
                            objModelView.RoleTableId = oUserDetails.RoleTableId ?? 0;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditUser", objModelView);

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

        private void GetRoleTableDetailsByRoleName(string roleName, int roleTableId)
        {
            string _roleName = Regex.Replace(roleName, @"\r\n?|\n", "");
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                switch (_roleName)
                {

                    case "Executive Director":
                        var entityExecutiveDirectorList = (from e in dbContext.tblMasterEDs

                                                           select new { e }).AsEnumerable().Select(s => new EntityRoleTableOptions()
                                                           {
                                                               Id = s.e.EDId,
                                                               Name = s.e.EDName
                                                           }).ToList();
                        ViewBag.RoleTableList = new SelectList(entityExecutiveDirectorList, "Id", "Name");
                        break;
                    case "CPM":
                        var entityCPMList = (from e in dbContext.tblMasterPIUs

                                             select new { e }).AsEnumerable().Select(s => new EntityRoleTableOptions()
                                             {
                                                 Id = s.e.PIUId,
                                                 Name = s.e.PIUName
                                             }).ToList();
                        ViewBag.RoleTableList = new SelectList(entityCPMList, "Id", "Name");
                        break;
                    case "Project User":
                        var entityProjectUserList = (from e in dbContext.tblMasterProjects

                                                     select new { e }).AsEnumerable().Select(s => new EntityRoleTableOptions()
                                                     {
                                                         Id = s.e.ProjectId,
                                                         Name = s.e.ProjectName
                                                     }).ToList();
                        ViewBag.RoleTableList = new SelectList(entityProjectUserList, "Id", "Name");
                        break;
                    case "Package User":

                        var entityPackageUserList = (from e in dbContext.tblPackages

                                                     select new { e }).AsEnumerable().Select(s => new EntityRoleTableOptions()
                                                     {
                                                         Id = s.e.PackageId,
                                                         Name = s.e.PackageName
                                                     }).ToList();
                        ViewBag.RoleTableList = new SelectList(entityPackageUserList, "Id", "Name");

                        break;
                    default:

                        break;
                }
            }

        }

        #region --- List UserMasters Values ---

        public ActionResult User_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                //var lst = (from x in dbContext.UserDetailsWithRoles
                //           select new UserViewModel
                //           {
                //               UserId = x.UserId,
                //               Name = x.Name,
                //               UserName = x.UserName,
                //               EmailId = x.EmailId,
                //               Password = x.Password,
                //               MobileNo = x.MobileNo,
                //               RoleId = (int)x.RoleId,
                //               RoleName = x.RoleName,
                //               RoleTableId = (int)x.RoleTableId,
                //               RoleTableName = x.TableName,
                //               TableDataName = x.TableDataName,
                //               CreatedOn = x.CreatedOn,
                //           }).ToList();

                var lst = (from x in dbContext.UserDetailsWithRoles
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new UserViewModel
                               {
                                   UserId = s.x.UserId,
                                   Name = s.x.Name,
                                   UserName = s.x.UserName,
                                   EmailId = s.x.EmailId,
                                   Password = Functions.Decrypt(s.x.Password),
                                   MobileNo = s.x.MobileNo,
                                   RoleId = (int)s.x.RoleId,
                                   RoleName = s.x.RoleName,
                                   RoleTableId = s.x.RoleTableId,
                                   RoleTableName = s.x.TableName,
                                   TableDataName = s.x.TableDataName,
                                   TableDataCode = s.x.TableDataCode,
                                   CreatedOn = s.x.CreatedOn,
                                   PackageList = s.x.PackageList,

                               }).ToList();

                //var lst = (from t1 in dbContext.tblUserMasters
                //           join t2 in dbContext.tblRoles
                //           on t1.RoleId equals t2.RoleId
                //           select new UserViewModel
                //           {
                //               UserId = t1.UserId,
                //               Name = t1.Name,
                //               UserName = t1.UserName,
                //               EmailId = t1.EmailId,
                //               MobileNo = t1.MobileNo,
                //               RoleId = (int)t1.RoleId,
                //               RoleName = t2.RoleName,
                //               CreatedOn = t1.CreatedOn,

                //           }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        [HttpPost]
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

                }
                return Json("1");
            }
            catch
            {
                // _data = CreateData();
                return Json("-1");
            }
        }




        [HttpPost]
        public JsonResult AddCustomUserDetails(string id)
        {
            string PackageList = id;
            try
            {
                Session["customPackageList"] = id;
                return Json("1");
            }
            catch
            {
                // _data = CreateData();
                return Json("-1");
            }
        }
        public JsonResult GetRoleTableDetails(string rolename)
        {
            string _roleName = Regex.Replace(rolename, @"\r\n?|\n", "");
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                switch (_roleName)
                {

                    case "Executive Director":

                        var _ExecutiveDirectorList = (from e in dbContext.tblMasterEDs
                                                      where (e.IsDelete == false)
                                                      select new { e }).AsEnumerable().Select(s => new EntityRoleTableOptions()
                                                      {
                                                          Id = s.e.EDId,
                                                          Name = s.e.EDName
                                                      }).ToList();
                        ViewBag.RoleTableList = new SelectList(_ExecutiveDirectorList, "Id", "Name");
                        return Json(_ExecutiveDirectorList, JsonRequestBehavior.AllowGet);

                        break;
                    case "CPM":
                        var _CPMList = (from e in dbContext.tblMasterPIUs
                                        where (e.IsDelete == false)
                                        select new { e }).AsEnumerable().Select(s => new EntityRoleTableOptions()
                                        {
                                            Id = s.e.PIUId,
                                            Name = s.e.PIUName
                                        }).ToList();
                        ViewBag.RoleTableList = new SelectList(_CPMList, "Id", "Name");
                        return Json(_CPMList, JsonRequestBehavior.AllowGet);

                        break;
                    case "Project User":
                        var _ProjectList = (from e in dbContext.tblMasterProjects
                                            where (e.IsDeleted == false)
                                            select new { e }).AsEnumerable().Select(s => new EntityRoleTableOptions()
                                            {
                                                Id = s.e.ProjectId,
                                                Name = s.e.ProjectName
                                            }).ToList();
                        ViewBag.RoleTableList = new SelectList(_ProjectList, "Id", "Name");
                        return Json(_ProjectList, JsonRequestBehavior.AllowGet);
                        break;
                    case "Package User":
                        var _PackagesList = (from e in dbContext.tblPackages
                                             where (e.IsDeleted == false)
                                             select new { e }).AsEnumerable().Select(s => new EntityRoleTableOptions()
                                             {
                                                 Id = s.e.PackageId,
                                                 Name = s.e.PackageName
                                             }).ToList();
                        ViewBag.RoleTableList = new SelectList(_PackagesList, "Id", "Name");
                        return Json(_PackagesList, JsonRequestBehavior.AllowGet);

                        break;
                    default:
                        return Json("", JsonRequestBehavior.AllowGet);
                        break;
                }

            }

        }
        [HttpGet]
        public JsonResult ServerFiltering_GetPackage(string text)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var lst = dbContext.tblPackages.Where(o => o.IsDeleted == false).Select(x => new PackageList
                    {
                        PkgId = x.PackageId,
                        PackageName = "(" + x.PackageCode + ") " + x.PackageName
                    });
                    if (!string.IsNullOrEmpty(text))
                    {
                        lst = lst.Where(p => p.PackageName.Contains(text));
                    }
                    return Json(lst.ToList(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        [HttpPost]
        public JsonResult SetPreSelectValue(int id)
        {
            try
            {

                using (var db = new dbRVNLMISEntities())
                {
                    var obj = db.PackagelistByUserId(id).ToList();
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                // _data = CreateData();
                return Json("-1");
            }
        }
    }


}
