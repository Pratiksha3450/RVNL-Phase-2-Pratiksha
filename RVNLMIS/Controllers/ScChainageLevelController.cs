using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    //[Compress]
    [SessionAuthorize]
    public class ScChainageLevelController : Controller
    {
        // GET: ScChainageLevel
        [PageAccessFilter]
        public ActionResult Index()
        {
            var packages = Functions.GetRoleAccessiblePackageList();
            ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");

            return View();
        }

        public ActionResult Read_ExistingList([DataSourceRequest]  DataSourceRequest request, int? pkgId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var lst = (from x in dbContext.tblSCPkgChngLvls
                           join p in dbContext.tblSCPkgCrossSections
                           on x.CsID equals p.CsID
                           //where x.PackageId == pkgId && x.CsID == crossId && x.IsDeleted == false
                           where x.PackageId == pkgId && x.IsDeleted == false
                           select new { x, p }).AsEnumerable().Select(s => new ScPkgChngLevelModel
                           {
                               GridCloCS = s.p.CSName,
                               AutoId = s.x.ID,
                               Chainage = s.x.Chainage,
                               OGL = Convert.ToString(s.x.OGL),
                               FRL = Convert.ToString(s.x.FRL),
                           }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ImportExcel(int packageId, HttpPostedFileBase FileUpload, string crossSectionName)
        {
            int updateCnt = 0, addCnt = 0;
            string pathToExcelFile = string.Empty;
            string errorRows = string.Empty;
            List<string> data = new List<string>();
            // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
            if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                try
                {
                    DataTable dtable;

                    Functions.ReadExcelintoDatatable(FileUpload, out pathToExcelFile, out dtable);
                    using (var db = new dbRVNLMISEntities())
                    {
                        int crossSecId = db.tblSCPkgCrossSections.Where(n => n.CSName == crossSectionName && n.IsDeleted == false && n.PackageId == packageId).Select(s => s.CsID).FirstOrDefault();

                        if (crossSecId != 0)
                        {
                            #region  ----- VALIDATE EXCEL -------

                            for (int i = 0; i < dtable.Rows.Count; i++)
                            {
                                DataRow row = dtable.Rows[i];

                                string CSecNameFrmFile = Convert.ToString(row["Cross Section Name"]);

                                if (crossSectionName != CSecNameFrmFile)
                                {
                                    errorRows = string.Concat(errorRows, i, ", ");
                                    continue;
                                }
                            }

                            if (!string.IsNullOrEmpty(errorRows))
                                return Json("Entered Cross section does not match with the cross section in file at row no(s). " + errorRows.TrimEnd(','), JsonRequestBehavior.AllowGet);

                            #endregion

                            #region ----- SAVE TO DB -------

                            for (int i = 0; i < dtable.Rows.Count; i++)
                            {
                                var outputPram = new ObjectParameter("OutputValue", 0);
                                DataRow row = dtable.Rows[i];

                                string Chainage = Convert.ToString(row["Chainage"]);
                                string OGL = Convert.ToString(row["OGL"]);
                                string FRL = Convert.ToString(row["FRL"]);

                                db.ImportScPkgChngLevel(packageId, crossSecId, Chainage, OGL, FRL, outputPram);

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

                            #endregion
                        }
                        else
                        {
                            return Json("Please enter valid Cross Section.", JsonRequestBehavior.AllowGet);
                        }
                    }
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return Json(addCnt + " new records are added and " + updateCnt + " records are updated.", JsonRequestBehavior.AllowGet);
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

        [Audit]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditingCustom_Update([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")]IEnumerable<ScPkgChngLevelModel> objScChainage)
        {
            if (objScChainage != null)
            {
                foreach (var item in objScChainage)
                {
                    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                    {
                        var objUpdate = dbContext.tblSCPkgChngLvls.Where(o => o.ID == item.AutoId).FirstOrDefault();
                        if (objUpdate != null)
                        {
                            int csId = dbContext.tblSCPkgCrossSections.Where(c => c.CSName == item.GridCloCS && c.IsDeleted == false)
                                .Select(s => s.CsID)
                                .FirstOrDefault();
                            objUpdate.OGL = Convert.ToDouble(item.OGL);
                            objUpdate.FRL = Convert.ToDouble(item.FRL);

                            if (csId != 0)
                            {
                                objUpdate.CsID = csId;
                            }
                            dbContext.SaveChanges();
                        }
                    }
                }
            }
            return Json(objScChainage.ToDataSourceResult(request, ModelState));
        }

        public JsonResult GetChildData(int? pkgId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var categories = dbContext.tblSCPkgCrossSections
                    .Where(w=>w.PackageId==pkgId && w.IsDeleted==false)
                        .Select(c => new ScPkgCrossSectionModel
                        {
                            CsID = c.CsID,
                            CSName = c.CSName
                        }).ToList()
                        .OrderBy(e => e.CSName);

                return Json(categories, JsonRequestBehavior.AllowGet);
            }
        }
    }
}