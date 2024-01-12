using DocumentFormat.OpenXml.Drawing.Charts;
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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class MaterialDistributionController : Controller
    {
        // GET: MaterialDistribution
        public ActionResult Index()
        {
            MaterialDistributionModel objModel = new MaterialDistributionModel();
            return View(objModel);
        }

        #region ---- Bind dropdowns ----
        /// <summary>
        /// Gets the discipline.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public JsonResult Get_Discipline(string text)
        {
            List<Discipline> obj = new List<Discipline>();
            try
            {

                using (var db = new dbRVNLMISEntities())
                {
                    obj = db.tblDisciplines.Where(o => o.IsDeleted == false).Select(s => new Discipline
                    {
                        DispId = s.DispId,
                        DisciplineName = s.DispName

                    }).OrderBy(x => x.DisciplineName).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.DisciplineName, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public JsonResult Get_EntityType(string text)
        {
            List<EntityTypeModel> obj = new List<EntityTypeModel>();
            try
            {

                using (var db = new dbRVNLMISEntities())
                {
                    obj = db.tblEntityTypes.Where(o => o.IsDeleted == false).Select(s => new EntityTypeModel
                    {
                        EntityTypeId = s.Id,
                        EntityTypeName = s.EntityType

                    }).OrderBy(x => x.EntityTypeName).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.EntityTypeName, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region ---- Get package material list ----
        /// <summary>
        /// Gets the package materials.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="pkgId">The PKG identifier.</param>
        /// <param name="dispId">The disp identifier.</param>
        /// <returns></returns>
        public ActionResult GetPackage_Materials([DataSourceRequest]DataSourceRequest request, string packageId, string disp)
        {
            List<ProcurementViewModel> lstProc = new List<ProcurementViewModel>();
            string whereCond = string.Empty;

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                   
                    if (!string.IsNullOrEmpty(packageId))
                    {
                        if (((UserModel)Session["UserData"]).RoleId == 600)
                        {
                            packageId = ((UserModel)Session["UserData"]).RoleTableID.ToString();
                        }
                        if (!string.IsNullOrEmpty(packageId) && string.IsNullOrEmpty(disp))
                        {
                            whereCond = " WHERE PackageId=" + packageId;
                        }
                        else if (!string.IsNullOrEmpty(packageId) && !string.IsNullOrEmpty(disp))
                        {
                            whereCond = " WHERE PackageId=" + packageId + " AND DispId=" + disp;
                        }

                        lstProc = dbContext.GetProcurementIndexList(whereCond).Select(r => new ProcurementViewModel
                        {
                            PackMatId = r.PkgMatId,
                            PackageId = r.PackageId,
                            DispId = r.DispId,
                            Discipline = r.Discipline,
                            MaterialName = r.MaterialName,
                            MaterialUnit = r.MaterialUnit,
                            OriginalQty = r.OriginalQty,
                            CumDeliveredQty = r.CumDeliveredQty,
                            CumOrderdQty = r.CumOrderedQty,
                            RatePerUnit = Convert.ToString(r.RatePerUnit),
                            RevisedQty = (decimal)r.RevisedQty

                        }).ToList();

                        lstProc = lstProc.Where(o => o.CumOrderdQty > 0 ).ToList(); //get only those package materials which has ordered

                        return Json(lstProc, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(lstProc, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(lstProc, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion ---- Get package material list

        #region ---- Bind entity grid ----
        /// <summary>
        /// Entities the master read.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="pkgId">The PKG identifier.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public ActionResult EntityMaster_Read([DataSourceRequest] DataSourceRequest request, int? pkgId, string entityType, int? PackMatId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                pkgId = pkgId == null ? 0 : pkgId;
                if (PackMatId != 0 && PackMatId != null)
                {
                    var lst = (from x in dbContext.tblMasterEntities
                               
                               where x.IsDelete == false && x.PackageId == pkgId
                               select new { x }).AsEnumerable().Select(w => new MaterialDistributionModel
                               {
                                   PackMatId = (int)PackMatId,
                                   EMOrderedQty = 0,
                                   EMDeliveredQty = 0,
                                   EMDId = 0,
                                   PackageId = (int)w.x.PackageId,
                                   EntityID = w.x.EntityID,
                                   EntityName = w.x.EntityName,
                                   EntityCode = w.x.EntityCode,
                                   EntityType = w.x.EntityType,
                               }).OrderBy(o => o.EntityName).ToList();
                    lst = lst.Count > 0 ? (!string.Equals(entityType, "Select Entity Type")) ? lst.Where(o => o.EntityType == entityType).ToList() : lst : lst;

                    if (lst.Count > 0 && dbContext.tblMaterialDistributions.Any(o => o.PackMatId == PackMatId))
                    {
                        foreach (var item in lst)
                        {
                            var packMat = dbContext.tblMaterialDistributions.Where(o => o.PackMatId == PackMatId && o.EntityId == item.EntityID).SingleOrDefault();
                            if (packMat != null)
                            {
                                item.EMDId = packMat.EMDId;
                                item.EMOrderedQty = packMat.EMOrderedQty;
                                item.EMDeliveredQty = packMat.EMDeliveredQty;
                            }

                        }
                    }

                    return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<MaterialDistributionModel> obj = new List<MaterialDistributionModel>();
                    return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }


            }
        }
        #endregion

        #region ---- UPDATE Entity Quantity ----
        /// <summary>
        /// update entity value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditingCustom_Update([DataSourceRequest] DataSourceRequest request,
         [Bind(Prefix = "models")]IEnumerable<MaterialDistributionModel> obj) // EDIT QTY, Rate 
        {
            if (obj != null)
            {
                using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                {
                    int pkgMatid = obj.Select(o => o.PackMatId).FirstOrDefault();
                    var pkgObj = db.C100ProcForm.Where(o => o.PkgMatId == pkgMatid).SingleOrDefault(); // get package material quanity

                    var ExistingPkgEData = db.tblMaterialDistributions.Where(o => o.PackMatId == pkgMatid).Select(x=> new MaterialDistributionModel { 
                            EMDId=x.EMDId,
                            PackMatId=(int)x.PackMatId,
                            EntityID=(int)x.EntityId,
                            EMOrderedQty=(double)x.EMOrderedQty,
                            EMDeliveredQty =(double)x.EMDeliveredQty
                    }).ToList();

                    foreach (var item in obj)
                    {
                        if (item.EMOrderedQty < item.EMDeliveredQty)
                        {
                            string txt = item.EntityName + "'s delivered Qty exceeding its Ordered Qty";
                            ModelState.AddModelError("order", txt);
                            var result = ModelState.ToDataSourceResult();
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        if (item.EMDId==0) // this if else created to check Qty if user set entity type filter 
                        {
                            ExistingPkgEData.Add(new MaterialDistributionModel() { EMOrderedQty=item.EMOrderedQty,EMDeliveredQty=item.EMDeliveredQty});
                        }
                        else
                        {
                            var lst = ExistingPkgEData.FirstOrDefault(d => d.EMDId == item.EMDId);
                            if (lst != null) { lst.EMOrderedQty = item.EMOrderedQty; lst.EMDeliveredQty = item.EMDeliveredQty; }
                        }
                    }

                    if (pkgObj.CumOrderedQty < ExistingPkgEData.Sum(o => o.EMOrderedQty))
                    {
                        ModelState.AddModelError("order", "Total Entity Ordered Qty exceeding selected Material's Ordered Qty");
                        var result = ModelState.ToDataSourceResult();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    if (pkgObj.CumDeliveredQty < ExistingPkgEData.Sum(o => o.EMDeliveredQty))
                    {
                        ModelState.AddModelError("order", "Total Entity Delivered Qty exceeding selected Material's Delivered Qty");
                        var result = ModelState.ToDataSourceResult();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                   
                    foreach (var item in obj)
                    {

                        var objtbl = db.tblMaterialDistributions.Where(o => o.EMDId == item.EMDId).SingleOrDefault();
                        if (objtbl != null) //update existing
                        {
                            objtbl.EMOrderedQty = item.EMOrderedQty;
                            objtbl.EMDeliveredQty = item.EMDeliveredQty;
                            db.SaveChanges();
                        }
                        else if (item.EMOrderedQty != 0 || item.EMDeliveredQty != 0) // add records if not exist before with condition
                        {
                            tblMaterialDistribution objNew = new tblMaterialDistribution();
                            objNew.PackMatId = item.PackMatId;
                            objNew.EntityId = item.EntityID;
                            objNew.EMOrderedQty = item.EMOrderedQty;
                            objNew.EMDeliveredQty = item.EMDeliveredQty;
                            db.tblMaterialDistributions.Add(objNew);
                            db.SaveChanges();
                            db.Entry(objNew).GetDatabaseValues();
                            item.EMDId = objNew.EMDId;
                        }
                    }
                }
            }
            return Json(obj.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region ---- package material details to shown in Card ----
        /// <summary>
        /// Get package material details
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [HttpPost]
        [Audit]
        public JsonResult GetPackgMatDetails(ProcurementViewModel dataObject, int packageId)
        {
            MaterialQtyModel obj = new MaterialQtyModel();
            if (dataObject.CumOrderdQty != 0)
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var objList = db.tblMaterialDistributions.Where(o => o.PackMatId == dataObject.PackMatId).ToList();
                    obj.TUsedOrderedQty = objList.Sum(x => x.EMOrderedQty);
                    obj.TUsedDeliveredQty = objList.Sum(x => x.EMDeliveredQty);
                    obj.TRemainDeliveredQty = dataObject.CumDeliveredQty - obj.TUsedDeliveredQty;
                    obj.TRemainOrderedQty = dataObject.CumOrderdQty - obj.TUsedOrderedQty;

                    obj.TUsedDeliveredQtyPerc = (int)Math.Round(((double)obj.TUsedDeliveredQty / (double)dataObject.CumDeliveredQty) * 100);
                    obj.TUsedOrderedQtyPerc = (int)Math.Round(((double)obj.TUsedOrderedQty / (double)dataObject.CumOrderdQty) * 100);
                }
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        #endregion ---- package material details to shown in Card ----
    }
}