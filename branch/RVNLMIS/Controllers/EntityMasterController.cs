using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class EntityMasterController : Controller
    {
        public string IpAddress = "";
        // GET: EntityMaster
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
            int k = Functions.SaveUserLog(pkgId, "Entity Master", "View", UserID, IpAddress, "NA");

            CreateEntityCode();
            BindDropdown();
            return View();
        }

        private void BindDropdown()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var projects = Functions.GetroleAccessibleProjectsList();
                ViewBag.ProjectList = new SelectList(projects, "ProjectId", "ProjectName");

                // var pkgs = Functions.GetRoleAccessiblePackageList();
                // ViewBag.PackageList = new SelectList(new List<tblPackage>(), "PackageId", "PackageName");

                //ViewBag.SectionList = new SelectList(new List<tblSection>(), "SectionID", "SectionName");

                ViewBag.EntityType = new SelectList(dbContext.tblEntityTypes.Where(e => e.IsDeleted == false).ToList(), "Id", "EntityType");
            }
        }

        public ActionResult EntityMaster_Read([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var pkgs = Functions.GetRoleAccessiblePackageList();
                var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();

                var lst = (from x in dbContext.tblMasterEntities
                           join p in dbContext.tblMasterProjects on x.ProjectId equals p.ProjectId
                           join s in dbContext.tblSections on x.SectionID equals s.SectionID
                           join pkg in dbContext.tblPackages on x.PackageId equals pkg.PackageId
                           // join et in dbContext.tblEntityTypes on x.EntityType equals et.EntityType
                           where x.IsDelete == false
                           select new { x, p, s, pkg }).AsEnumerable().Select(w => new EntityMasterModel
                           {
                               PackageId = (int)w.x.PackageId,
                               EntityID = w.x.EntityID,
                               EntityName = w.x.EntityName,
                               SectionName = w.s.SectionName,
                               ProjectName = w.p.ProjectName,
                               PackageName = w.pkg.PackageName,
                               PackageCode = w.pkg.PackageCode,
                               EntityCode = w.x.EntityCode,
                               EntityTypeId = w.x.EntityType,
                               StartChainageNumber = Functions.RepalceCharacter(w.x.StartChainage),
                               Lat = w.x.Lat,
                               Long = w.x.Long,
                               StartChainage = w.x.StartChainage,
                               EndChainage = w.x.EndChainage,
                           }).OrderBy(o => o.StartChainageNumber).ToList();

                lst = lst.Where(w => accessiblePackageList.Contains(w.PackageId)).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        [Audit]
        public ActionResult AddEditEntity(int id)
        {
            EntityMasterModel objModel = new EntityMasterModel();
            objModel.ModalHeader = "Add Entity Details";
            objModel.EntityCode = CreateEntityCode();
            BindDropdown();

            if (id != 0)
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    try
                    {
                        objModel = (from e in dbContext.tblMasterEntities
                                    join et in dbContext.tblEntityTypes on e.EntityType equals et.EntityType
                                    where e.EntityID == id && et.IsDeleted == false
                                    select new { e, et }).AsEnumerable().Select(s => new EntityMasterModel
                                    {
                                        EntityID = s.e.EntityID,
                                        EntityCode = s.e.EntityCode,
                                        EntityName = s.e.EntityName,
                                        PackageId = (int)s.e.PackageId,
                                        ProjectId = s.e.ProjectId,
                                        SectionID = s.e.SectionID,
                                        EntityTypeId = s.et.Id.ToString(),
                                        EntityTypeName = s.et.EntityType,
                                        Lat = s.e.Lat,
                                        Long = s.e.Long,
                                        StartChainage = s.e.StartChainage,
                                        EndChainage = s.e.EndChainage,
                                        ModalHeader = "Update Entity Details"
                                    }).FirstOrDefault();

                        objModel.ModalHeader = "Update Entity Details";

                        //var packagesForProject = GetPackages(objModel.ProjectId);
                        //ViewBag.PackageList = new SelectList(packagesForProject, "PackageId", "PackageName");

                        //var sectionsForPackage = GetSections(objModel.PackageId);
                        //ViewBag.SectionList = new SelectList(sectionsForPackage, "SectionID", "SectionName");
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            return View("_PartialEntityMaster", objModel);
        }

        [Audit]
        [HttpPost]
        public ActionResult SubmitEntity(EntityMasterModel objModel)
        {
            try
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;
                string message = string.Empty;
                if (!ModelState.IsValid)
                {
                    BindDropdown();

                    //var packagesForProject = GetPackages(objModel.ProjectId);
                    //ViewBag.PackageList = new SelectList(packagesForProject, "PackageId", "PackageName");

                    //var sectionsForPackage = GetSections(objModel.PackageId);
                    //ViewBag.SectionList = new SelectList(sectionsForPackage, "SectionID", "SectionName");

                    objModel.ModalHeader = objModel.EntityID == 0 ? "Add Entity Details" : "Update Entity Details";
                    return View("_PartialEntityMaster", objModel);
                }

                string msgChainage = ChainageValidation(objModel.StartChainage, objModel.EndChainage, (int)objModel.SectionID);

                if (msgChainage != "Empty")
                {
                    return Json(msgChainage, JsonRequestBehavior.AllowGet);
                }

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    if (objModel.EntityID == 0)   //add operation
                    {
                        var isNameEnteredExist = dbContext.tblMasterEntities.Where(e => e.EntityName == objModel.EntityName && e.IsDelete == false && e.PackageId == objModel.PackageId).FirstOrDefault();

                        if (isNameEnteredExist != null)
                        {
                            return Json("2", JsonRequestBehavior.AllowGet);
                        }

                        tblMasterEntity objAdd = new tblMasterEntity();
                        objAdd.EntityCode = objModel.EntityCode;
                        objAdd.EntityName = objModel.EntityName;
                        objAdd.PackageId = objModel.PackageId;
                        objAdd.ProjectId = objModel.ProjectId;
                        objAdd.SectionID = objModel.SectionID;
                        objAdd.EntityType = objModel.EntityTypeName;
                        objAdd.Lat = objModel.Lat;
                        objAdd.Long = objModel.Long;
                        objAdd.StartChainage = objModel.StartChainage;
                        objAdd.EndChainage = objModel.EndChainage;
                        objAdd.IsDelete = false;
                        objAdd.CreatedOn = DateTime.Now;
                        objAdd.CreatedBy = ((UserModel)Session["UserData"]).UserId;
                        objAdd.ModifiedOn = DateTime.Now;
                        objAdd.ModifiedBy = ((UserModel)Session["UserData"]).UserId;
                        dbContext.tblMasterEntities.Add(objAdd);
                        dbContext.SaveChanges();
                        message = "Data added successfully.";

                        int k = Functions.SaveUserLog(pkgId, "Manage Entity", "save", UserID, IpAddress, Convert.ToString("Entity Code: " + objModel.EntityCode));
                       

                    }
                    else                      //edit operation
                    {
                        var isNameEnteredExist = dbContext.tblMasterEntities.Where(e => e.EntityName == objModel.EntityName && e.IsDelete == false && e.PackageId == objModel.PackageId && e.EntityID != objModel.EntityID).FirstOrDefault();

                        if (isNameEnteredExist != null)
                        {
                            return Json("2", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var objEdit = dbContext.tblMasterEntities.Where(e => e.EntityID == objModel.EntityID && e.IsDelete == false).SingleOrDefault();
                            objEdit.EntityName = objModel.EntityName;
                            objEdit.PackageId = objModel.PackageId;
                            objEdit.ProjectId = objModel.ProjectId;
                            objEdit.SectionID = objModel.SectionID;
                            objEdit.EntityType = objModel.EntityTypeName;
                            objEdit.Lat = objModel.Lat;
                            objEdit.Long = objModel.Long;
                            objEdit.StartChainage = objModel.StartChainage;
                            objEdit.EndChainage = objModel.EndChainage;
                            objEdit.ModifiedOn = DateTime.Now;
                            objEdit.ModifiedBy = ((UserModel)Session["UserData"]).UserId;
                            dbContext.SaveChanges();
                            message = "Data updated successfully.";

                            try
                            {
                                string str = "";;
                                var UpdatedValue = (from ul in dbContext.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                                if (UpdatedValue != null)
                                {
                                    str = UpdatedValue;
                                    str = str.Replace("ModifiedOn", "");
                                    if (String.IsNullOrEmpty(str))
                                    {
                                        str = "Entity Name: " + objModel.EntityName + "= " + "NA";
                                    }
                                    else
                                    {
                                        str = "Entity Name: " + objModel.EntityName + "= " + str.TrimEnd();
                                    }
                                    str = str.TrimStart(',');
                                    str = str.Replace(", ,", ",");
                                    str = str.TrimEnd(',');

                                }
                                else
                                {
                                    str = "NA";
                                }
                                
                                int k = Functions.SaveUserLog(pkgId, "Manage Entity", "Update", UserID, IpAddress, str);
                            }
                            catch (Exception ex)
                            {
                                
                            }
                           
                        }

                    }
                    
                }
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public string CreateEntityCode()
        {
            EntityMasterModel objUser = new EntityMasterModel();
            string ou = string.Empty;
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    //var lastEntityCode = db.tblMasterEntities.Where(o => o.IsDelete == false).OrderByDescending(o => o.EntityCode).FirstOrDefault();
                    var lastEntityCode = db.GetNextPackageCode("tblMasterEntity").ToList();
                    if (lastEntityCode == null)
                    {
                        //objUser.EntityCode = "ENT0001";
                        ou = "ENT0001";
                    }
                    else
                    {
                        //string get = lastEntityCode[0].Code.Substring(4); //label1.text=ATHCUS-100
                        int s = Functions.ParseInteger(lastEntityCode[0].intNo.ToString()) + 1;
                        ou = "ENT" + s;
                    }
                    return ou;
                }
            }
            catch (Exception ex)
            {
                return objUser.EntityCode;
            }
        }

        [Audit]
        public JsonResult Delete(int id)
        {
            try
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var getEntityToDelete = dbContext.tblMasterEntities.Where(e => e.EntityID == id).SingleOrDefault();
                    if (getEntityToDelete != null)
                    {
                        getEntityToDelete.IsDelete = true;
                        dbContext.SaveChanges();
                        int k = Functions.SaveUserLog(pkgId, "Manage Entity", "Delete", UserID, IpAddress, "Entity Code:" + Convert.ToString(getEntityToDelete.EntityCode));
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json("1", JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetPackageUnderProject(int? id)
        //{
        //    List<PackageModel> _PackageList = new List<PackageModel>();

        //    try
        //    {
        //        _PackageList = GetPackages(id);

        //        return Json(_PackageList, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json("1", JsonRequestBehavior.AllowGet);
        //    }
        //}

        //public List<PackageModel> GetPackages(int? projectId)
        //{
        //    List<PackageModel> _PackageList = new List<PackageModel>();
        //    string roleCode = ((UserModel)Session["UserData"]).RoleCode;
        //    int userId = ((UserModel)HttpContext.Session["UserData"]).UserId;
        //    int roleId = ((UserModel)HttpContext.Session["UserData"]).RoleId;

        //    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
        //    {
        //        //var _PackageList = (from e in dbContext.tblPackages
        //        //                    where (e.ProjectId == projectId && e.IsDeleted == false)
        //        //                    select new PackageModel
        //        //                    {
        //        //                        PackageId = e.PackageId,
        //        //                        PackageName = e.PackageCode + " - " + e.PackageName
        //        //                    }).ToList();
        //        //return _PackageList;

        //        if (roleCode == "PKG")
        //        {
        //            _PackageList = dbContext.GetRoleAssignedPackageList(userId, roleId).Select(s => new PackageModel
        //            {
        //                PackageId = s.PackageId,
        //                PackageName = s.PackageName
        //            }).ToList();
        //        }
        //        else
        //        {
        //            _PackageList = (from e in dbContext.tblPackages
        //                            where (e.ProjectId == projectId && e.IsDeleted == false)
        //                            select new PackageModel
        //                            {
        //                                PackageId = e.PackageId,
        //                                PackageName = e.PackageCode + " - " + e.PackageName
        //                            }).ToList();
        //        }
        //    }

        //    return _PackageList;
        //}

        //public JsonResult GetSectionsUnderPackage(int id)
        //{
        //    try
        //    {
        //        var _SectionList = GetSections(id);

        //        return Json(_SectionList, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json("1", JsonRequestBehavior.AllowGet);
        //    }
        //}

        //public List<SectionModel> GetSections(int? packageId)
        //{
        //    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
        //    {
        //        var _SectionList = (from e in dbContext.tblSections
        //                            where (e.PackageId == packageId && e.IsDeleted == false)
        //                            select new SectionModel
        //                            {
        //                                SectionId = e.SectionID,
        //                                SectionName = e.SectionCode + " - " + e.SectionName
        //                            }).ToList();
        //        return _SectionList;
        //    }
        //}

        public JsonResult Get_SectionsByPackage(int? id, string text)
        {
            List<SectionModel> _SectionList = new List<SectionModel>();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    _SectionList = (from e in dbContext.tblSections
                                    where (e.PackageId == id && e.IsDeleted == false)
                                    select new SectionModel
                                    {
                                        SectionId = e.SectionID,
                                        SectionName = e.SectionCode + " - " + e.SectionName
                                    }).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        _SectionList = _SectionList.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.SectionName, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(_SectionList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(_SectionList, JsonRequestBehavior.AllowGet);
            }
        }

        public string ChainageValidation(string sC, string eC, int sectionId)
        {
            int entityStartC = Functions.RepalceCharacter(sC);
            int entityEndC = Functions.RepalceCharacter(eC);

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var sectionObj = dbContext.tblSections.Where(s => s.SectionID == sectionId && s.IsDeleted == false).FirstOrDefault();
                    int sectionStartC = Functions.RepalceCharacter(sectionObj.StartChainage);
                    int sectionEndC = Functions.RepalceCharacter(sectionObj.EndChainage);

                    if ((entityStartC >= sectionStartC && entityStartC <= sectionEndC) && (entityEndC >= sectionStartC && entityEndC <= sectionEndC))
                    {
                        return "Empty";
                    }
                    return "Entity Start and End Chainage has to be within section's Start (" + sectionObj.StartChainage + ") and End Chainage (" + sectionObj.EndChainage + ").";
                }
            }
            catch (Exception ex)
            {
                return "Empty";
            }
        }
    }
}