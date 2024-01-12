using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [SessionAuthorize]
    public class IssueController : Controller
    {
        //[PageAccessFilter]
        // GET: Issue
        public ActionResult Index()
        {
            IssueRegModel objIssueRegModel = new IssueRegModel();
            ViewBag.Visibility = "false";
            int userId = ((UserModel)Session["RFIUserSession"]).UserId;
            string RoleID = ((UserModel)Session["RFIUserSession"]).RoleCode;
            //userId = RoleID == 100 ? 0 : userId;
            if (RoleID == "RVNL")
            {
                ViewBag.Visibility = "true";
            }
               
                return View();
        }

        #region ---AddEditIssue---
        public ActionResult AddEditIssue(IssueRegModel oModel)
        {
            try
            {
                oModel.issueAddedby = ((UserModel)Session["RFIUserSession"]).UserId;
                string filesPath = "";
                string Message = string.Empty;

                if (!ModelState.IsValid)
                {
                    // oModel.IssueRegNo = CreatePackageCode();
                    return View("PartialAddEditIssue", oModel);

                }
                else
                {
                    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                    {
                        if (oModel.ID == 0)
                        {
                            if (oModel.LocationType == "Entity")    // Entity Selected
                            {
                                //oModel.Location = "Entity";
                                oModel.StartChainage = "0";
                                oModel.EndChainage = "0";
                                oModel.OtherLocation = "";
                            }
                            else if (oModel.LocationType == "Other")    // other Location
                            {
                                //oModel.Location = "Other";
                                oModel.StartChainage = "0";
                                oModel.EndChainage = "0";
                                oModel.EntityID = 0;
                            }
                            else
                            {
                                string msgChainage = ChainageValidation(oModel.StartChainage, oModel.EndChainage, (int)oModel.PackageId);

                                if (msgChainage != "Empty")
                                {
                                    return Json(msgChainage, JsonRequestBehavior.AllowGet);
                                }
                              

                            }
                           

                            var exist = dbContext.tblIssues.Where(i => i.IssueRegNo == oModel.IssueRegNo && i.IsDelete == false).FirstOrDefault();
                            if (exist != null)
                            {
                                Message = "already exists";
                            }
                            else
                            {

                                string filename = "";

                                if (oModel.AttachmentFile != null)
                                {
                                    string extention = Path.GetExtension(oModel.AttachmentFile.FileName);
                                    if (extention == ".gif" || extention == ".jpeg" || extention == ".jpg" || extention == ".png" || extention == ".pdf" || extention == ".xls" || extention == ".xlsx")
                                    {
                                        filename = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmssfff"), extention);
                                        oModel.AttachFilePath = filename;
                                        string filePath = (Server.MapPath("~/Uploads/IssueReg/") + filename);
                                        filesPath = "/Uploads/IssueReg/" + filename;
                                        oModel.AttachmentFile.SaveAs(filePath);
                                        oModel.AttachFileName = filename;

                                    }
                                    else
                                    {
                                        filename = "Default.jpg";
                                    }
                                }

                                tblIssue objModel = new tblIssue();
                                objModel.IssueDescription = oModel.IssueDescription;
                                objModel.IssueRegNo = oModel.IssueRegNo;
                                objModel.DisciplineId = oModel.DisplineId;
                                objModel.EntityId = oModel.EntityID;
                                objModel.StartChainage = oModel.StartChainage;
                                objModel.EndChainage = oModel.EndChainage;
                                objModel.Location = oModel.LocationType;
                                objModel.otherLocation = oModel.OtherLocation;
                                objModel.Attachment = oModel.AttachFileName;
                                objModel.IssueDate = Convert.ToDateTime(oModel.Date);
                                objModel.PackageId = oModel.PackageId;
                                objModel.AttachmentPath = filesPath;
                                objModel.IssueSubject = oModel.IssueSubject;
                                objModel.IssueAddeby = oModel.issueAddedby;
                                objModel.IsDelete = false;
                                dbContext.tblIssues.Add(objModel);

                                dbContext.SaveChanges();
                                Message = "Added Successfully";

                            }

                        }
                        else
                        {
                            var objIssue = dbContext.tblIssues.Where(t => t.Id == oModel.ID && t.IsDelete == false).FirstOrDefault();

                            if (oModel.LocationType == "Entity")    // Entity Selected
                            {
                                //oModel.Location = "Entity";
                                oModel.StartChainage = "0";
                                oModel.EndChainage = "0";
                                oModel.OtherLocation = "";
                            }
                            else if (oModel.LocationType == "Other")    // other Location
                            {
                                //oModel.Location = "Other";
                                oModel.StartChainage = "0";
                                oModel.EndChainage = "0";
                                oModel.EntityID = 0;
                            }
                            else
                            {
                                string msgChainage = ChainageValidation(oModel.StartChainage, oModel.EndChainage, (int)oModel.PackageId);

                                if (msgChainage != "Empty")
                                {
                                    return Json(msgChainage, JsonRequestBehavior.AllowGet);
                                }

                                //oModel.Location = "Chainage";
                                oModel.OtherLocation = "";
                                oModel.EntityID = 0;
                            }
                            string filename = "";

                            if (oModel.AttachmentFile != null)
                            {
                                string extention = Path.GetExtension(oModel.AttachmentFile.FileName);
                                if (extention == ".gif" || extention == ".jpeg" || extention == ".jpg" || extention == ".png" || extention == ".pdf" || extention == ".xls" || extention == ".xlsx")
                                {
                                    filename = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmssfff"), extention);
                                    oModel.AttachFilePath = filename;
                                    string filePath = (Server.MapPath("~/Uploads/IssueReg/") + filename);
                                    filesPath = "/Uploads/IssueReg/" + filename;
                                    oModel.AttachmentFile.SaveAs(filePath);
                                    oModel.AttachFileName = filename;

                                }
                                else
                                {
                                    filename = "Default.jpg";
                                }
                            }


                            objIssue.IssueDescription = oModel.IssueDescription;
                            objIssue.IssueRegNo = oModel.IssueRegNo;
                            objIssue.DisciplineId = oModel.DisplineId;
                            objIssue.EntityId = oModel.EntityID;
                            objIssue.StartChainage = oModel.StartChainage;
                            objIssue.EndChainage = oModel.EndChainage;
                            objIssue.Location = oModel.LocationType;
                            objIssue.otherLocation = oModel.OtherLocation;
                            objIssue.IssueDate = Convert.ToDateTime(oModel.Date);
                            objIssue.IssueSubject = oModel.IssueSubject;
                            objIssue.PackageId = oModel.PackageId;
                            if (oModel.AttachmentFile != null)
                            {
                                objIssue.AttachmentPath = filesPath;
                                objIssue.Attachment = oModel.AttachFileName;
                            }
                            objIssue.IssueAddeby = oModel.issueAddedby;
                            objIssue.IsDelete = false;
                            dbContext.SaveChanges();
                            Message = "Update Successfully";


                        }

                    }
                    return Json(Message, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return View("Index", oModel);
            }

        }
        #endregion

        #region --Issue Register Details List---
        public ActionResult IssueRegDetails([DataSourceRequest] DataSourceRequest request)
        
        {
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {

                // ViewBag.Visibility = "false";
                //int userId = ((UserModel)Session["UserData"]).UserId;
                //int RoleID = ((UserModel)Session["UserData"]).RoleId;
                int userId = ((UserModel)Session["RFIUserSession"]).UserId;
                string RoleID = ((UserModel)Session["RFIUserSession"]).RoleCode;
                List<IssueRegModel> issueList1 = new List<IssueRegModel>();
                //  userId = RoleID == 100 ? 0 : userId;
                var organisation = db.tblRFIUsers.Where(c => c.RFIUserId == userId).SingleOrDefault().Organisation;
                if (RoleID == "RVNL")
                {
                     issueList1 = (from x in db.GetAllIssueListUserwise()
                                   select new IssueRegModel
                                      {
                                          ID = x.Id,
                                          Date = Convert.ToDateTime(x.IssueDate).ToString("dd-MMM-yyyy"),
                                          IssueRegNo = x.IssueRegNo,
                                          IssueSubject = x.IssueSubject,
                                          DisciplineName = x.DisciplineName,
                                          PackageName = x.PackageName,
                                          AttachFileName = string.IsNullOrEmpty(x.Attachment) ? "NA" : x.Attachment,
                                          AttachFilePath = x.AttachmentPath,
                                          PMCName = x.PmcName,
                                          UserName = x.UserName,
                                          IssueDescription = x.IssueDescription,
                                          Location = x.Location == "Chainage" ? x.StartChainage + " to " + x.EndChainage : x.Location == "Entity" ? x.EntityName : x.Location == "Other" ? x.otherLocation : "No result"

                                      }).ToList();
                   // ViewBag.Visibility = "true";
                }
               
                return Json(issueList1.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }



        #endregion

        #region-- Bind Entity Dropdwon--
        public JsonResult BindEntityDrpValues(int? pkgId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var entityDrpList = (from e in dbContext.tblMasterEntities
                                         where e.PackageId == pkgId && e.IsDelete == false
                                         select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                                         {
                                             Id = s.e.EntityID,
                                             Name = s.e.EntityCode + " " + s.e.EntityName
                                         }).ToList();
                    return Json(entityDrpList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion

        #region---CreateIssueCode---
        public JsonResult CreateIssueCode()
        {
            IssueRegModel objIssueRegModel = new IssueRegModel();
            string ou = string.Empty;
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {

                    var maxPackage = db.GetNextPackageCode("tblIssue").ToList();
                    // var lastPackageCode = db.tblPackages.Where(o => o.IsDeleted == false).OrderByDescending(o => o.PackageCode).FirstOrDefault();
                    if (maxPackage.Count == 0)
                    {
                        //objPackageModel.PackageCode = "PKG1001";
                        ou = "ISU1001";
                    }
                    else
                    {
                        string get = maxPackage[0].Code.Split('U')[1]; //label1.text=ATHCUS-100
                        string s = (Convert.ToInt32(get) + 1).ToString();
                        ou = "ISU" + s;
                    }
                    return Json(ou, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // _data = CreateData();
                return Json(ou, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region--Bind Discipline Dropdwon---
        public JsonResult BindDispline()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var DisplineList = (from e in dbContext.tblDisciplines
                                    where (e.IsDeleted == false)
                                    select new DisciplineModel
                                    {
                                        DispId = e.DispId,
                                        DisciplineName = e.DispName
                                    }).ToList();

                return Json(DisplineList, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region--BindIssueEditDetails---

        public ActionResult BindIssueEditDetails(int id)
        {
            IssueRegModel objModel = new IssueRegModel();
            try
            {
                using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                {
                    var objIssue = db.tblIssues.Where(t => t.Id == id).FirstOrDefault();
                    if (objIssue != null)
                    {
                        objModel.IssueRegNo = objIssue.IssueRegNo;
                        objModel.PackageId = objIssue.PackageId;
                        objModel.IssueDescription = objIssue.IssueDescription;
                        objModel.IssueSubject = objIssue.IssueSubject;
                        objModel.DisplineId = Convert.ToInt32(objIssue.DisciplineId);
                        objModel.EntityID = Convert.ToInt32(objIssue.EntityId);
                        objModel.StartChainage = objIssue.StartChainage;
                        objModel.EndChainage = objIssue.EndChainage;
                        objModel.LocationType = objIssue.Location;
                        objModel.OtherLocation = objIssue.otherLocation;
                        objModel.AttachFileName = objIssue.Attachment;
                        objModel.AttachFilePath = objIssue.AttachmentPath;
                        objModel.Date = Convert.ToDateTime(objIssue.IssueDate).ToString("yyyy-MM-dd");
                        //objModel.IssueDate = string.IsNullOrEmpty(Convert.ToString(objIssue.IssueDate)) ? "" : Convert.ToDateTime(Convert.ToString(objIssue.IssueDate)).ToString("dd-MM-yyyy");

                    }
                }
                return View("PartialAddEditIssue", objModel);
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion


        #region -- Delete Operation ---

        public JsonResult Delete(int id)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var getIssueToDelete = dbContext.tblIssues.Where(e => e.Id == id).SingleOrDefault();
                    if (getIssueToDelete != null)
                    {
                        getIssueToDelete.IsDelete = true;
                        dbContext.SaveChanges();
                    }
                }
                return Json("1", JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion

        public ActionResult DownloadDoc(int ID)
        {
            try
            {
                using (dbRVNLMISEntities db = new dbRVNLMISEntities()) 
                {
                    var objIssue = db.tblIssues.Where(t => t.Id == ID).FirstOrDefault();
                    string name = objIssue.Attachment;
                    string path = objIssue.AttachmentPath;

                    path = string.Concat(Server.MapPath(path));
                    //if (Directory.Exists(path))
                    //{
                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                    string fileName = name;
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                    //}
                    //return View("index");
                }
            }
            catch (Exception ex)
            {
                return Json("1");
            }
        }

        public ActionResult ViewIssueDetails(int id)
        {
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                IssueRegModel objModel = new IssueRegModel();
                var viewIssueList = db.GetAllIssueDetailsById(id).ToList();
                if (viewIssueList != null)
                {
                    foreach (var item in viewIssueList)
                    {
                        objModel.Date = Convert.ToDateTime(item.IssueDate).ToString("dd-MMM-yyyy");
                        objModel.IssueRegNo = item.IssueRegNo;
                        objModel.IssueSubject = item.IssueSubject;
                        objModel.DisciplineName = item.DisciplineName;
                        objModel.PackageName = item.PackageName;
                        objModel.AttachFileName = item.Attachment;
                        objModel.AttachFilePath = item.AttachmentPath;
                        objModel.PMCName = item.PmcName;
                        objModel.UserName = item.UserName;
                        objModel.IssueDescription = item.IssueDescription;
                        objModel.Location = item.Location == "Chainage" ? item.StartChainage + " to " + item.EndChainage : item.Location == "Entity" ? item.EntityName : item.Location == "Other" ? item.otherLocation : "No result";
                        return View("_PartialViewIssueDetails", objModel);

                    }
                }
                else 
                {
                    return View("Index");
                }






            }
            return View();
        }

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
                    return "Section Start and End Chainage has to be within Package's Start (" + packageObj.StartChainage + ") and End Chainage (" + packageObj.EndChainage + ").";
                }
            }
            catch (Exception ex)
            {
                return "Empty";
            }
        }

        internal class DisciplineModel
        {
            public string DispCode { get; set; }
            public int DispId { get; set; }
            public string DisciplineName { get; set; }
        }

        public class EntityDropdownOptions
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }


    }
}