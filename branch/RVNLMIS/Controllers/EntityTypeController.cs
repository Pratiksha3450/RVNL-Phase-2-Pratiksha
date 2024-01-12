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
    public class EntityTypeController : Controller
    {
        // GET: EntityType
        [PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }

        #region --- List Entity Type Values ---
        public ActionResult EntityType_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.EntityTypeWithActGroupViews
                           where x.IsDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new EntityTypeModel
                               {
                                   EntityTypeId = s.x.Id,
                                   EntityTypeName = s.x.EntityType,
                                   ActGId = s.x.ActGId,
                                   ActivityGroupName = s.x.ActivityGroupName
                                   //IsDeleted=s.x.IsDeleted

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit Entity Type Details --
        public ActionResult EditEntityTypeByEntityTypeId(int id)
        {
            int entiytTypeId = id;
            EntityTypeModel objModel = new EntityTypeModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oEntityTypeDetails = db.tblEntityTypes.Where(o => o.Id == id).SingleOrDefault();
                        if (oEntityTypeDetails != null)
                        {
                            objModel.ActGId = oEntityTypeDetails.ActGId;
                            objModel.EntityTypeId = oEntityTypeDetails.Id;
                            objModel.EntityTypeName = oEntityTypeDetails.EntityType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditEntityType", objModel);
        }
        #endregion

        #region -- Add and Update Engg Dwg Type Details --
        [HttpPost]
        public ActionResult AddEntityTypeDetails(EntityTypeModel oModel)
        {
            int entityTypeId = oModel.EntityTypeId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblEntityTypes.Where(u => (u.EntityType == oModel.EntityTypeName) && u.IsDeleted == false).FirstOrDefault();

                        if (entityTypeId == 0)
                        {
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblEntityType objEntityType = new tblEntityType();
                                objEntityType.ActGId = oModel.ActGId;
                                objEntityType.EntityType = oModel.EntityTypeName;
                                objEntityType.IsDeleted = false;
                                objEntityType.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblEntityTypes.Add(objEntityType);
                                db.SaveChanges();
                                message = "Added Successfully";




                            }
                        }
                        else
                        {

                            var exist1 = db.tblEntityTypes.Where(u => u.EntityType == oModel.EntityTypeName && u.IsDeleted == false && u.Id != oModel.EntityTypeId).ToList();
                            if (exist1.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblEntityType objEntityType = db.tblEntityTypes.Where(o => o.Id == oModel.EntityTypeId).SingleOrDefault();
                                objEntityType.ActGId = oModel.ActGId;
                                objEntityType.EntityType = oModel.EntityTypeName;
                                objEntityType.IsDeleted = false;
                                objEntityType.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }


                        }
                        ModelState.Clear();
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ModelState.Clear();
                    return View("_PartialEditEntityType", oModel);
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
                    tblEntityType objEntityType = db.tblEntityTypes.SingleOrDefault(o => o.Id == id);
                    objEntityType.IsDeleted = true;
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

        public JsonResult ServerFiltering_GetActivityGroupsList(string text)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var activityGroups = dbContext.tblActivityGroups.Where(d => d.IsDeleted == false)
                    .Select(s => new { s.ActGId, s.ActivityGroupName }).ToList();
                return Json(activityGroups, JsonRequestBehavior.AllowGet);
            }
        }

    }
}