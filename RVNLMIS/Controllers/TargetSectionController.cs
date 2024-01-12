using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections;
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
    public class TargetSectionController : Controller
    {
       [PageAccessFilter]
        public ActionResult Index()
        {
            
            return View();
        }

        #region --- List TargetSection Values ---
        public ActionResult TargetSection_Details([DataSourceRequest]  DataSourceRequest request, string SectionId, string role)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<TargetSectionModel> obj = new List<TargetSectionModel>();

                obj = (from x in dbContext.tblTargetSections.Where(s => s.IsDeleted != true)
                       join p in dbContext.tblPackages on x.PackageId equals p.PackageId 
                       join s in dbContext.tblSections on x.SectionId equals s.SectionID
                       select new { x,p,s })
                                       .AsEnumerable().Select(s =>
                                          new TargetSectionModel
                                          {
                                              TargetSectionId = s.x.TargetSectionId,
                                              PackageId =(int) s.x.PackageId,
                                              SectionId =(int) s.x.SectionId,
                                              SectionName= s.s.SectionName,
                                              PackageName=s.p.PackageCode + "- "+ s.p.PackageName,
                                              Year= Convert.ToInt32( s.x.Year),
                                              YearStr= Convert.ToString(s.x.Year)

                                          }).ToList();

                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region -- Add TargetSection Details --
        [HttpPost]
        public ActionResult AddEditTargetSection(TargetSectionModel oModel)
        {
            try
            {
               
                if (ModelState.IsValid==true)
                {
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    string message = string.Empty;
                   // int packageID = Convert.ToInt32(fc["ddlPackage"]);
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.TargetSectionId == 0)
                        {
                            var exist = db.tblTargetSections.Where(u => u.PackageId == oModel.PackageId && u.SectionId == oModel.SectionId && u.Year ==oModel.Year).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {                               
                                tblTargetSection objTargetSection = new tblTargetSection();                               
                                objTargetSection.PackageId = oModel.PackageId;
                                objTargetSection.SectionId = oModel.SectionId;
                                objTargetSection.Year = oModel.Year;
                                objTargetSection.IsDeleted = false;
                                objTargetSection.AddedBy = UserID;
                                objTargetSection.AddedOn = DateTime.Now;
                                db.tblTargetSections.Add(objTargetSection);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                        else
                        {
                            var exist = db.tblTargetSections.Where(u => (u.PackageId == oModel.PackageId && u.SectionId == oModel.SectionId && u.Year == oModel.Year) && (u.TargetSectionId != oModel.TargetSectionId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblTargetSection objTargetSectionmodel = db.tblTargetSections.Where(u => u.TargetSectionId == oModel.TargetSectionId).SingleOrDefault();
                                objTargetSectionmodel.PackageId = oModel.PackageId;
                                objTargetSectionmodel.SectionId = oModel.SectionId;
                                objTargetSectionmodel.IsDeleted = false;
                                objTargetSectionmodel.Year = oModel.Year;
                                objTargetSectionmodel.IsDeleted = false;
                                objTargetSectionmodel.AddedBy = UserID;
                                objTargetSectionmodel.AddedOn = DateTime.Now;
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }
                        }
                    }
                    ModelState.Clear();                   
                    var result = new { message = message };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_AddEditTargetSection", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region ---Bind Drop Down ---
        public JsonResult ServerFiltering_GetPackage(string text)// Bind Package Dropdown
        {
            List<GetRoleAssignedPackageList_Result> sessionPackages = Functions.GetRoleAccessiblePackageList();
            if (!string.IsNullOrEmpty(text))
            {
                sessionPackages = sessionPackages.Where(p =>
               CultureInfo.CurrentCulture.CompareInfo.IndexOf
               (p.PackageName, text, CompareOptions.IgnoreCase) >= 0).ToList();
            }
            return Json(sessionPackages, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Get_SectionsByPackage(int? id)
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
                    return Json(_SectionList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(_SectionList, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion 

        #region -- Edit Target Section Details --
        public ActionResult EditByTargetSectionId(int id)
        {           
            TargetSectionModel objModel = new TargetSectionModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oTargetSectionDetails = db.tblTargetSections.Where(o => o.TargetSectionId == id).SingleOrDefault();
                        if (oTargetSectionDetails != null)
                        {
                            objModel.TargetSectionId = oTargetSectionDetails.TargetSectionId;
                            objModel.PackageId =(int) oTargetSectionDetails.PackageId;
                            objModel.SectionId =(int) oTargetSectionDetails.SectionId;
                            objModel.Year= (int)oTargetSectionDetails.Year;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditTargetSection", objModel);
        }
        #endregion

        #region -- Delete Target Section Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblTargetSection obj = db.tblTargetSections.SingleOrDefault(o => o.TargetSectionId == id);
                    obj.IsDeleted = true;
                    db.SaveChanges();
                }
                return Json("1");
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion
    }
}