using RVNLMIS.Common;
using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http;
using System.Web.Mvc;
using RVNLMIS.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common.ActionFilters;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    //[Compress]
    [SessionAuthorize]
    public class CrossSectionController : Controller
    {
        // GET: CrossSection
        [PageAccessFilter]
        public ActionResult Index()
        {
            var packages = Functions.GetRoleAccessiblePackageList();
            ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");

            return View();
        }

        #region --- Strip chart activity operations ---

        [HttpPost]
        public ActionResult SubmitScActivity(ScActivityDetailsModel oModel)
        {
            try
            {
                if (oModel.PackageId == null)
                {
                    return Json("3", JsonRequestBehavior.AllowGet);
                }

                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var isNameEnteredExist = db.tblScActivityDetails.Where(o => o.ScActName == oModel.ScActName && o.IsDeleted == false && o.PackageId == oModel.PackageId).FirstOrDefault();

                        if (oModel.ScActID == 0)                    //add operation
                        {
                            if (isNameEnteredExist != null)
                            {
                                return Json("0", JsonRequestBehavior.AllowGet);
                            }

                            tblScActivityDetail objScActAdd = new tblScActivityDetail();
                            objScActAdd.ScActName = oModel.ScActName;
                            objScActAdd.PackageId = oModel.PackageId;
                            objScActAdd.PlotColour = oModel.PlotColour;
                            objScActAdd.PlotThk = oModel.PlotThk;
                            objScActAdd.CreatedOn = DateTime.Now;
                            objScActAdd.IsDeleted = false;
                            db.tblScActivityDetails.Add(objScActAdd);
                            db.SaveChanges();
                            ModelState.Clear();
                            return Json("Data added successfully.", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblScActivityDetail objScActEdit = db.tblScActivityDetails.Where(o => o.ScActID == oModel.ScActID).SingleOrDefault();
                            if (objScActEdit.ScActName != oModel.ScActName)
                            {
                                if (isNameEnteredExist != null)
                                {
                                    return Json("0", JsonRequestBehavior.AllowGet);
                                }
                            }

                            objScActEdit.ScActName = oModel.ScActName;
                            objScActEdit.PackageId = oModel.PackageId;
                            objScActEdit.PlotColour = oModel.PlotColour;
                            objScActEdit.PlotThk = oModel.PlotThk;
                            db.SaveChanges();
                            ModelState.Clear();
                            return Json("Data updated successfully.", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    return View("_ScActivityDetails", oModel);
                }
            }
            catch (Exception ex)
            {
                return Json("-1", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Read_CsActivityDetails([DataSourceRequest]  DataSourceRequest request, int? pkgId)
        {
            List<ScActivityDetailsModel> lstResult = new List<ScActivityDetailsModel>();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    lstResult = dbContext.tblScActivityDetails.Where(p => p.PackageId == pkgId && p.IsDeleted == false)
                        .Select(s => new ScActivityDetailsModel
                        {
                            ScActID = s.ScActID,
                            ScActName = s.ScActName,
                            PlotColour = s.PlotColour,
                            PlotThk = (double)s.PlotThk
                        }).ToList();
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult EditScActivity(int id)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    ScActivityDetailsModel objScActEdit = dbContext.tblScActivityDetails.Where(o => o.ScActID == id)
                        .Select(s => new ScActivityDetailsModel
                        {
                            ScActID = s.ScActID,
                            PackageId = s.PackageId,
                            ScActName = s.ScActName,
                            PlotColour = s.PlotColour,
                            PlotThk = (double)s.PlotThk,
                        }).SingleOrDefault();
                    return View("_ScActivityDetails", objScActEdit);
                }
                catch (Exception ex)
                {
                    return View("_ScActivityDetails", new tblScActivityDetail());
                }
            }
        }

        #region -- Delete ScActivity ---

        [HttpPost]
        [Audit]
        public JsonResult DeleteScActivity(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblScActivityDetail objToDelete = db.tblScActivityDetails.FirstOrDefault(o => o.ScActID == id);
                    objToDelete.IsDeleted = true;
                    db.SaveChanges();
                    return Json("ScActivity");
                }
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion

        #endregion

        #region --- PkgCross Section operations ---

        [HttpPost]
        public ActionResult SubmitPkgCrossSection(ScPkgCrossSectionModel oModel)
        {
            try
            {
                if (oModel.CSPackageId == null)
                {
                    return Json("3", JsonRequestBehavior.AllowGet);
                }

                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var isNameEnteredExist = db.tblSCPkgCrossSections.Where(o => o.CSName == oModel.CSName && o.PackageId == oModel.CSPackageId && o.IsDeleted == false).FirstOrDefault();

                        if (oModel.CsID == 0)                    //add operation
                        {
                            if (isNameEnteredExist != null)
                            {
                                return Json("0", JsonRequestBehavior.AllowGet);
                            }

                            tblSCPkgCrossSection objScActAdd = new tblSCPkgCrossSection();
                            objScActAdd.CSName = oModel.CSName;
                            objScActAdd.PackageId = oModel.CSPackageId;
                            objScActAdd.CreatedOn = DateTime.Now;
                            objScActAdd.IsDeleted = false;
                            db.tblSCPkgCrossSections.Add(objScActAdd);
                            db.SaveChanges();
                            return Json("Data added successfully.", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblSCPkgCrossSection objScActEdit = db.tblSCPkgCrossSections.Where(o => o.CsID == oModel.CsID).SingleOrDefault();
                            if (objScActEdit.CSName != oModel.CSName)
                            {
                                if (isNameEnteredExist != null)
                                {
                                    return Json("0", JsonRequestBehavior.AllowGet);
                                }
                            }

                            objScActEdit.CSName = oModel.CSName;
                            objScActEdit.PackageId = oModel.CSPackageId;
                            db.SaveChanges();
                            return Json("Data updated successfully.", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    return View("_ScPkgCrossSection", oModel);
                }

            }
            catch (Exception ex)
            {
                return Json("-1", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditCrossSection(int id)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    ScPkgCrossSectionModel objScPkgCsEdit = dbContext.tblSCPkgCrossSections.Where(o => o.CsID == id)
                        .Select(s => new ScPkgCrossSectionModel
                        {
                            CsID = s.CsID,
                            CSPackageId = s.PackageId,
                            CSName = s.CSName,
                        }).SingleOrDefault();
                    return View("_ScPkgCrossSection", objScPkgCsEdit);
                }
                catch (Exception ex)
                {
                    return View("_ScPkgCrossSection", new tblScActivityDetail());
                }
            }
        }

        public ActionResult Read_PkgCrossSectionList([DataSourceRequest]  DataSourceRequest request, int? pkgId)
        {
            List<ScPkgCrossSectionModel> lstResult = new List<ScPkgCrossSectionModel>();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    lstResult = dbContext.tblSCPkgCrossSections.Where(p => p.PackageId == pkgId && p.IsDeleted == false)
                      .Select(s => new ScPkgCrossSectionModel
                      {
                          CsID = s.CsID,
                          CSName = s.CSName,
                      }).ToList();
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
        }

        #region -- Delete Package Cross Section ---

        [HttpPost]
        [Audit]
        public JsonResult DeleteScPkgCs(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblSCPkgCrossSection objToDelete = db.tblSCPkgCrossSections.FirstOrDefault(o => o.CsID == id);
                    objToDelete.IsDeleted = true;
                    db.SaveChanges();
                    return Json("ScpkgCs");
                }
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion

        #endregion

        #region --- ScCsDetails operations ---

        [HttpPost]
        public ActionResult SubmitScCsDetails(ScCrossSectionDetailsModel oModel)
        {
            try
            {
                if (oModel.CsDPackageId == null)
                {
                    return Json("3", JsonRequestBehavior.AllowGet);
                }

                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        // var isNameEnteredExist = db.tblScCsDetails.Where(o => o. == oModel.CSName && o.IsDeleted == false).FirstOrDefault();

                        if (oModel.AutoID == 0)                    //add operation
                        {
                            //if (isNameEnteredExist != null)
                            //{
                            //    return Json("0", JsonRequestBehavior.AllowGet);
                            //}

                            tblScCsDetail objScActAdd = new tblScCsDetail();
                            objScActAdd.CsID = oModel.CrossID;
                            objScActAdd.PackageId = oModel.CsDPackageId;
                            objScActAdd.ScActID = oModel.ScActivityID;
                            objScActAdd.BottomWd = oModel.BottomWd;
                            objScActAdd.Layer = oModel.Layer;
                            objScActAdd.MaxLayerThk = oModel.MaxLayerThk;
                            objScActAdd.Slope = oModel.Slope;
                            objScActAdd.TopWd = oModel.TopWd;
                            objScActAdd.TotalThk = oModel.TotalThk;
                            objScActAdd.CreatedOn = DateTime.Now;
                            objScActAdd.IsDeleted = false;
                            db.tblScCsDetails.Add(objScActAdd);
                            db.SaveChanges();
                            return Json("Data added successfully.", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblScCsDetail objScActEdit = db.tblScCsDetails.Where(o => o.AutoID == oModel.AutoID).SingleOrDefault();
                            //if (objScActEdit.CSName != oModel.CSName)
                            //{
                            //    if (isNameEnteredExist != null)
                            //    {
                            //        return Json("0", JsonRequestBehavior.AllowGet);
                            //    }
                            //}

                            objScActEdit.CsID = oModel.CrossID;
                            objScActEdit.PackageId = oModel.CsDPackageId;
                            objScActEdit.ScActID = oModel.ScActivityID;
                            objScActEdit.BottomWd = oModel.BottomWd;
                            objScActEdit.Layer = oModel.Layer;
                            objScActEdit.MaxLayerThk = oModel.MaxLayerThk;
                            objScActEdit.Slope = oModel.Slope;
                            objScActEdit.TopWd = oModel.TopWd;
                            objScActEdit.TotalThk = oModel.TotalThk;
                            db.SaveChanges();
                            return Json("Data updated successfully.", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    return View("_ScCsDetails", oModel);
                }

            }
            catch (Exception ex)
            {
                return Json("-1", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditCsD(int id)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    ScCrossSectionDetailsModel objScCsDEdit = dbContext.tblScCsDetails.Where(o => o.AutoID == id)
                        .Select(s => new ScCrossSectionDetailsModel
                        {
                            AutoID = s.AutoID,
                            CsDPackageId = s.PackageId,
                            CrossID = s.CsID,
                            ScActivityID = s.ScActID,
                            BottomWd = (double)s.BottomWd,
                            TopWd = (double)s.TopWd,
                            TotalThk = (double)s.TotalThk,
                            MaxLayerThk = (double)s.MaxLayerThk,
                            Slope = (double)s.Slope,
                            Layer = (bool)s.Layer
                        }).SingleOrDefault();
                    return View("_ScCsDetails", objScCsDEdit);
                }
                catch (Exception ex)
                {
                    return View("_ScCsDetails", new tblScActivityDetail());
                }
            }
        }

        public ActionResult Read_CrossSectionDetails([DataSourceRequest]  DataSourceRequest request, int? pkgId, int? CsId)
        {
            List<ScCrossSectionDetailsModel> lstResult = new List<ScCrossSectionDetailsModel>();
            List<CrossSectionDetailsView> tempResult = new List<CrossSectionDetailsView>();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    if (CsId == null && pkgId != null)
                    {
                        tempResult = dbContext.CrossSectionDetailsViews.Where(p => p.PackageId == pkgId).ToList();
                    }
                    else if (pkgId != null && CsId != null)
                    {
                        tempResult = dbContext.CrossSectionDetailsViews.Where(p => p.PackageId == pkgId && p.CsID == CsId).ToList();
                    }

                    lstResult =
                    tempResult.Select(s => new ScCrossSectionDetailsModel
                    {
                        SeqNo = s.SeqNo,
                        ActivityName = s.ActivityName,
                        CrossSectionName = s.CrossSectionName,
                        AutoID = s.AutoID,
                        BottomWd = s.BottomWd,
                        TopWd = s.TopWd,
                        StrLayer = s.StrLayer,
                        MaxLayerThk = s.MaxLayerThk,
                        Slope = s.Slope,
                        TotalThk = s.TotalThk
                    }).ToList();

                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditingCustomCsD_Update([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")]IEnumerable<ScCrossSectionDetailsModel> products)
        {
            if (products != null)
            {
                foreach (var product in products)
                {
                    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                    {
                        tblScCsDetail target = dbContext.tblScCsDetails.Where(o => o.AutoID == product.AutoID && o.IsDeleted == false).FirstOrDefault();

                        if (target != null)
                        {
                            if (dbContext.tblScCsDetails.Where(t => t.CsID == target.CsID && t.PackageId == target.PackageId && t.SeqNo == product.SeqNo).ToList().Count != 0)
                            {
                                return Json(new DataSourceResult
                                {
                                    Errors = "Please enter unique sequence for each cross section activity."
                                });
                            }
                            else
                            {
                                target.SeqNo = product.SeqNo;
                                dbContext.SaveChanges();
                            }
                        }
                    }
                }
            }

            return Json(products.ToDataSourceResult(request, ModelState));
        }

        public JsonResult ServerFiltering_GetCrossSection(int? pkgId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var _CSList = (from e in dbContext.tblSCPkgCrossSections
                                   where (e.PackageId == pkgId && e.IsDeleted == false)
                                   select new
                                   {
                                       e.CsID,
                                       e.CSName
                                   }).ToList();

                    return Json(_CSList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new tblMasterPIU(), JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult ServerFiltering_GetActivity(int? pkgId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var _SCActivityList = (from e in dbContext.tblScActivityDetails
                                           where (e.PackageId == pkgId && e.IsDeleted == false)
                                           select new
                                           {
                                               e.ScActID,
                                               e.ScActName
                                           }).ToList();

                    return Json(_SCActivityList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new tblMasterPIU(), JsonRequestBehavior.AllowGet);
                }

            }
        }

        #region -- Delete Strip Chart Activity Detials ---

        [HttpPost]
        [Audit]
        public JsonResult DeleteScCsDetails(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblScCsDetail objToDelete = db.tblScCsDetails.FirstOrDefault(o => o.AutoID == id);
                    objToDelete.IsDeleted = true;
                    db.SaveChanges();
                    return Json("ScCsDetails");
                }
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion

        #endregion
    }
}