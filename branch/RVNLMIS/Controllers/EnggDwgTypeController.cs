using Kendo.Mvc.UI;
using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.Models;
using Kendo.Mvc.Extensions;
using RVNLMIS.Common.ActionFilters;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class EnggDwgTypeController : Controller
    {
        // GET: EnggDwgType
        [PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }

        #region --- List Engg Dwg Type Values ---
        public ActionResult EnggDwgType_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.tblEnggDwgTypes
                           where x.IsDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new EnggDwgTypeModel
                               {
                                   DwgId = s.x.DwgId,
                                   DwgName = s.x.DwgName,
                                   CreatedOn = s.x.CreatedOn
                                   //IsDeleted=s.x.IsDeleted

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region -- Edit Discipline Details --
        public ActionResult EditEnggDwgTypeByDwgId(int id)
        {
            int dwgId = id;
            EnggDwgTypeModel objModel = new EnggDwgTypeModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oEnggDwgTypeDetails = db.tblEnggDwgTypes.Where(o => o.DwgId == id).SingleOrDefault();
                        if (oEnggDwgTypeDetails != null)
                        {
                            objModel.DwgId = oEnggDwgTypeDetails.DwgId;
                            objModel.DwgName = oEnggDwgTypeDetails.DwgName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditEnggDwgType", objModel);
        }
        #endregion


        #region -- Add and Update Engg Dwg Type Details --
        [HttpPost]
        public ActionResult AddEnggDwgTypeDetails(EnggDwgTypeModel oModel)
        {
            int dwgId = oModel.DwgId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    if (dwgId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblEnggDwgTypes.Where(u => u.DwgName == oModel.DwgName && u.IsDeleted == false).SingleOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblEnggDwgType objEnggDwgType = new tblEnggDwgType();
                                //objDiscipline.DispId = oModel.DisciplineId;
                                objEnggDwgType.DwgName = oModel.DwgName;
                                objEnggDwgType.IsDeleted = false;
                                objEnggDwgType.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblEnggDwgTypes.Add(objEnggDwgType);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblEnggDwgTypes.Where(u => u.DwgName == oModel.DwgName && u.IsDeleted == false && u.DwgId != oModel.DwgId).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblEnggDwgType objEnggDwgType = db.tblEnggDwgTypes.Where(o => o.DwgId == oModel.DwgId).SingleOrDefault();
                                objEnggDwgType.DwgName = oModel.DwgName;
                                objEnggDwgType.IsDeleted = false;
                                objEnggDwgType.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }
                        }
                    }
                    ModelState.Clear();
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_PartialEditEnggDwgType", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion


        #region -- Delete Engg Dwg Type Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblEnggDwgType objEnggDwgType = db.tblEnggDwgTypes.SingleOrDefault(o => o.DwgId == id);
                    objEnggDwgType.IsDeleted = true;
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