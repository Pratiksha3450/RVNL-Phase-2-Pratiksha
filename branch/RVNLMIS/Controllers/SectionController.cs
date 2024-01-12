using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.Common;
using System.Data.OleDb;
using System.Data;
using System.Data.Entity.Core.Objects;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class SectionController : Controller
    {
        public string IpAddress = "";
        // GET: Section
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
            int k = Functions.SaveUserLog(pkgId, "Manage Section", "View", UserID, IpAddress, "NA");

            GetPackageList();
            return View();
        }

        [Audit]
        public ActionResult Create()
        {
            SectionModel objModelView = new SectionModel();
            GetPackageList();
            return View("_PartialEditSections", objModelView);
        }
        //public JsonResult GetPackageUnderProject(int? id)
        //{
        //    try
        //    {
        //        var _PackageList = GetPackages(id);

        //        return Json(_PackageList, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json("1", JsonRequestBehavior.AllowGet);
        //    }
        //}

        #region -- Load Package List --
        public void GetPackageList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var projects = Functions.GetroleAccessibleProjectsList();
                ViewBag.ProjectList = new SelectList(projects, "ProjectId", "ProjectName");

                // var pkgs = Functions.GetRoleAccessiblePackageList();
                // ViewBag.PackageList = new SelectList(new List<tblPackage>(), "PackageId", "PackageName");
            }
        }
        #endregion

        //public List<PackageModel> GetPackages(int? projectId)
        //{
        //    List<PackageModel> _PackageList = new List<PackageModel>();
        //    string roleCode = ((UserModel)Session["UserData"]).RoleCode;
        //    int userId = ((UserModel)HttpContext.Session["UserData"]).UserId;
        //    int roleId = ((UserModel)HttpContext.Session["UserData"]).RoleId;


        //    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
        //    {
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
        //        return _PackageList;
        //    }
        //}

        #region --- List Resource Values ---
        public ActionResult Section_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var pkgs = Functions.GetRoleAccessiblePackageList();
                var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();

                var lst = (from x in dbContext.SectionViews

                           select new SectionModel
                           {
                               ProjectId = (int)x.ProjectId,
                               ProjectName = x.ProjectName,
                               SectionId = x.SectionID,
                               PackageId = (int)x.PackageId,
                               PackageName = x.PackageName,
                               PackageCode = x.PackageCode,
                               SectionName = x.SectionName,
                               SectionCode = x.SectionCode,
                               SectionStart = x.SectionStart,
                               SectionFinish = x.SectionFinish,
                               StartChaining = x.StartChainage,
                               EndChaining = x.EndChainage,
                               SectionLength = x.SectionLength,
                               Length = x.Length
                           }).OrderByDescending(o => o.SectionId).ToList();

                lst = lst.Where(w => accessiblePackageList.Contains(w.PackageId)).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Add and Update Section Details --
        [HttpPost]
        [Audit]
        public ActionResult AddSectionDetails(SectionModel oModel)
        {
            //GetPackageList();
            int sectionId = oModel.SectionId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    string msgChainage = ChainageValidation(oModel.StartChaining, oModel.EndChaining, (int)oModel.PackageId);

                    if (msgChainage != "Empty")
                    {
                        return Json(msgChainage, JsonRequestBehavior.AllowGet);
                    }
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    if (sectionId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblSections.Where(u => u.SectionName == oModel.SectionName && u.ProjectId == oModel.ProjectId && u.IsDeleted == false).FirstOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                                return Json(message, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                tblSection objSection = new tblSection();
                                objSection.ProjectId = oModel.ProjectId;
                                objSection.PackageId = oModel.PackageId;      // Add Package Dropdown
                                objSection.SectionName = oModel.SectionName;
                                objSection.SectionCode = oModel.SectionCode;
                                objSection.SectionStart = (oModel.SectionStarts == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionStarts);
                                objSection.SectionFinish = (oModel.SectionFinishs == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionFinishs);
                                objSection.StartChainage = oModel.StartChaining;
                                objSection.EndChainage = oModel.EndChaining;
                                objSection.IsDeleted = false;
                                objSection.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                CalculateSectionLength(objSection);
                                db.tblSections.Add(objSection);
                                db.SaveChanges();
                                message = "Added Successfully";
                               

                                int k = Functions.SaveUserLog(pkgId, "Manage Section", "save", UserID, IpAddress, Convert.ToString("Section Code: " +oModel.SectionCode));
                               
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblSections.Where(u => u.SectionName == oModel.SectionName && u.ProjectId == oModel.ProjectId && u.IsDeleted == false && u.SectionID != oModel.SectionId).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblSection objSection = db.tblSections.Where(u => u.SectionID == oModel.SectionId).SingleOrDefault();
                                objSection.ProjectId = oModel.ProjectId;
                                objSection.PackageId = oModel.PackageId;      // Add Package Dropdown
                                objSection.SectionName = oModel.SectionName;
                                //objSection.SectionCode = oModel.SectionCode;
                                //objSection.SectionStart = Convert.ToDateTime(oModel.SectionStarts);
                                objSection.SectionStart = (oModel.SectionStarts == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionStarts);
                                objSection.SectionFinish = (oModel.SectionFinishs == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionFinishs);
                                //objSection.SectionFinish = Convert.ToDateTime(oModel.SectionFinishs);
                                objSection.StartChainage = oModel.StartChaining;
                                objSection.EndChainage = oModel.EndChaining;
                                objSection.IsDeleted = false;
                                objSection.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                CalculateSectionLength(objSection);
                                db.SaveChanges();
                                message = "Updated Successfully";
                                                             
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
                                            str = "Section Name: " + oModel.SectionName + "= NA ";
                                        }
                                        else
                                        {
                                            str = "Section Name: " + oModel.SectionName + "= " + str;
                                        }
                                        
                                    }
                                    else
                                    {
                                        str = "NA";
                                    }

                                    int k = Functions.SaveUserLog(pkgId, "Manage Section", "Update", UserID, IpAddress, str);
                                }
                                catch (Exception ex)
                                {

                                }

                            }
                        }
                    }

                    //Update Package TotalKM length
                    using (var dbContext = new dbRVNLMISEntities())
                    {
                        dbContext.UpdatePackageLength(oModel.PackageId);
                    }
                    ModelState.Clear();
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    #region -- Bind Dropdowns --
                    var projects = Functions.GetroleAccessibleProjectsList();
                    ViewBag.ProjectList = new SelectList(projects, "ProjectId", "ProjectName");

                    //var pkgs = Functions.GetRoleAccessiblePackageList();
                    //ViewBag.PackageList = new SelectList(pkgs, "PackageId", "PackageName");
                    #endregion

                    oModel = new SectionModel();
                    return View("_PartialEditSections", oModel);
                }
            }
            catch (Exception ex)
            {
                oModel = new SectionModel();
                return View("_PartialEditSections", oModel);
            }
        }

        public void CalculateSectionLength(tblSection objSection)
        {
            int EC = string.IsNullOrEmpty(objSection.EndChainage) ? 0 : Convert.ToInt32(objSection.EndChainage.Replace("+", ""));
            int SC = string.IsNullOrEmpty(objSection.StartChainage) ? 0 : Convert.ToInt32(objSection.StartChainage.Replace("+", ""));
            objSection.Length = Math.Abs(EC - SC);
        }
        #endregion


        #region -- Edit Resource Details --
        [Audit]
        public ActionResult EditSectionBySectionId(int id)
        {
            GetPackageList();
            int resourceId = id;
            SectionModel objModel = new SectionModel();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var oSectionDetails = db.tblSections.Where(o => o.SectionID == id && o.IsDeleted == false).SingleOrDefault();
                        if (oSectionDetails != null)
                        {
                            objModel.ProjectId = (oSectionDetails.ProjectId == null) ? 0 : (int)oSectionDetails.ProjectId; ;

                            objModel.SectionId = oSectionDetails.SectionID;
                            objModel.PackageId = (int)oSectionDetails.PackageId;
                            //objModel.PackageName = oResourceDetails.PackageName;
                            objModel.SectionName = oSectionDetails.SectionName;
                            objModel.SectionCode = oSectionDetails.SectionCode;
                            //objModel.SectionStart = oSectionDetails.SectionStart;
                            //objModel.SectionFinish =oSectionDetails.SectionFinish;
                            objModel.SectionStart = Convert.ToDateTime(oSectionDetails.SectionStart);
                            objModel.SectionStarts = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.SectionStart)) ? "" : Convert.ToDateTime(Convert.ToString(oSectionDetails.SectionStart)).ToString("yyyy-MM-dd");
                            objModel.SectionFinish = Convert.ToDateTime(oSectionDetails.SectionFinish);
                            objModel.SectionFinishs = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.SectionFinish)) ? "" : Convert.ToDateTime(Convert.ToString(oSectionDetails.SectionFinish)).ToString("yyyy-MM-dd");
                            objModel.StartChaining = oSectionDetails.StartChainage;
                            objModel.EndChaining = oSectionDetails.EndChainage;
                        }

                        //var packagesForProject = GetPackages(objModel.ProjectId);
                        //ViewBag.PackageList = new SelectList(packagesForProject, "PackageId", "PackageName");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditSections", objModel);
        }
        #endregion

        public JsonResult Get_PackageByProject(int? id)
        {
            List<PackageModel> _PackageList = new List<PackageModel>();

            try
            {
                string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                int userId = ((UserModel)HttpContext.Session["UserData"]).UserId;
                int roleId = ((UserModel)HttpContext.Session["UserData"]).RoleId;

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    if (roleCode == "PKG")
                    {
                        _PackageList = dbContext.GetRoleAssignedPackageList(userId, roleId).Select(s => new PackageModel
                        {
                            PackageId = s.PackageId,
                            PackageName = s.PackageName
                        }).ToList();
                    }
                    else
                    {
                        _PackageList = (from e in dbContext.tblPackages
                                        where (e.ProjectId == id && e.IsDeleted == false)
                                        select new PackageModel
                                        {
                                            PackageId = e.PackageId,
                                            PackageName = e.PackageCode + " - " + e.PackageName
                                        }).ToList();
                    }
                    return Json(_PackageList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(_PackageList, JsonRequestBehavior.AllowGet);
            }
        }

        #region -- Delete Section Details --
        [HttpPost]
        [Audit]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    if (db.tblMasterEntities.Where(s => s.SectionID == id && s.IsDelete == false).ToList().Count() != 0)
                    {
                        return Json("2");
                    }
                    else
                    {
                        tblSection objSection = db.tblSections.FirstOrDefault(o => o.SectionID == id);
                        objSection.IsDeleted = true;
                        db.SaveChanges();

                        IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (string.IsNullOrEmpty(IpAddress))
                        {
                            IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                        }
                        int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                        int UserID = ((UserModel)Session["UserData"]).UserId;
                        
                        int k = Functions.SaveUserLog(pkgId, "Manage Section", "Delete", UserID, IpAddress, "Section Code:" + Convert.ToString(objSection.SectionCode));

                        return Json("1");
                    }
                }
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion

        public JsonResult GetSectionCode(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var package = db.tblPackages.Where(o => o.PackageId == id && o.IsDeleted == false).SingleOrDefault().PackageCode;
                    var section = package.Split('G')[1];
                    var count = db.tblSections.Where(o => o.PackageId == id && o.IsDeleted == false).OrderByDescending(o => o.SectionID).Select(o => o.SectionCode).FirstOrDefault();
                    if (count != null)
                    {
                        int _lastNo = Functions.ParseInteger(Convert.ToString(count).Split('S')[1]);
                        int newcode = _lastNo + 1;
                        section = "P" + section + "S" + newcode.ToString();
                    }
                    else
                    {

                        section = "P" + section + "S01";
                    }
                    return Json(section, new JsonRequestBehavior());
                }
            }
            catch (Exception ex)
            {

                return Json("0");
            }
        }

        #region Import Excel

        public ActionResult GetImortModal()
        {
            List<GetRoleAssignedPackageListForMaterialPackage_Result> sessionProjects = Functions.GetRoleAssignedPackageListForMaterialPackage();
            var pkgList = (from p in sessionProjects
                           select new drpOptions
                           {
                               Category = p.ProjectName,
                               Id = p.PackageId,
                               Name = p.PackageName
                           }).ToList().OrderBy(N => N.Category);
            ViewBag.ProjectPackageList = new SelectList(pkgList, "Id", "Name", "Category", 0);
            //ViewBag.firstPackage = sessionProjects.FirstOrDefault().PackageId;
            return View("_ImportExcel");
        }

        [HttpPost]
        public ActionResult ImportExcel(int packageId, HttpPostedFileBase FileUpload)
        {
            int updateCnt = 0, addCnt = 0;
            string pathToExcelFile = string.Empty;
            List<string> data = new List<string>();
            // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
            if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                try
                {
                    string filename = FileUpload.FileName;
                    string targetpath = Server.MapPath("~/Uploads/ExcelImport");
                    FileUpload.SaveAs(targetpath + filename);
                    pathToExcelFile = targetpath + filename;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }

                    var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                    var ds = new DataSet();

                    adapter.Fill(ds, "ExcelTable");

                    DataTable dtable = ds.Tables["ExcelTable"];

                    for (int i = 0; i < dtable.Rows.Count; i++)
                    {
                        var outputPram = new ObjectParameter("OutputValue", 0);
                        DataRow row = dtable.Rows[i];
                        string SectionName = Convert.ToString(row["Section Name"]);
                        string SectionStart = Convert.ToString(row["Start Date (yyyy/mm/dd)"]);
                        string SectionEnd = Convert.ToString(row["End Date (yyyy/mm/dd)"]);
                        string StartChainage = Convert.ToString(row["Start Chainage"]);
                        string EndChainage = Convert.ToString(row["End Chainage"]);

                        using (var db = new dbRVNLMISEntities())
                        {
                            db.SectionImport(packageId, SectionName, GetSectionCode(packageId).Data.ToString(), Convert.ToDateTime(SectionStart),
                                             Convert.ToDateTime(SectionEnd), StartChainage, EndChainage, outputPram);
                        }

                        if (Convert.ToInt32(outputPram.Value) == 1)
                        {
                            //update
                            updateCnt = updateCnt + 1;
                        }
                        else
                        {
                            //add
                            addCnt = addCnt + 1;
                        }
                    }
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return Json(addCnt + " new sections are added and " + updateCnt + " sections are updated. ", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return Json(ex.Message + " -- " + ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //alert message for invalid file format  
                return Json("Only Excel file format is allowed", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        public string ChainageValidation(string sC, string eC, int packageId)
        {
            int sectionStartC = Functions.RepalceCharacter(sC);
            int sectionEndC = Functions.RepalceCharacter(eC);

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var packageObj = dbContext.tblPackages.Where(s => s.PackageId == packageId && s.IsDeleted == false).FirstOrDefault();
                    int packageStartC = Functions.RepalceCharacter(packageObj.StartChainage);
                    int packageEndC = Functions.RepalceCharacter(packageObj.EndChainage);

                    if ((sectionStartC >= packageStartC && sectionStartC <= packageEndC) && (sectionEndC >= packageStartC && sectionEndC <= packageEndC))
                    {
                        return "Empty";
                    }
                    return "Selected Start and End Chainage has to be within Package's Start (" + packageObj.StartChainage + ") and End Chainage (" + packageObj.EndChainage + ").";
                }
            }
            catch (Exception ex)
            {
                return "Empty";
            }
        }
    }
}