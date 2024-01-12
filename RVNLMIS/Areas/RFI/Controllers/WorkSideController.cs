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
using RVNLMIS.Areas.RFI.Models;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [AreaSessionExpire]
    [HandleError]
    public class WorkSideController : Controller
    {
        // GET: WorkSide
        public ActionResult Index()
        {
            return View();
        }

        #region --- List WorkSide Values ---
        public ActionResult WorkSide_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.tblWorkSides
                           where x.IsDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new WorkSideModel
                               {
                                   WorkSideId = s.x.WorkSideId,                                  
                                   WorkSideName = s.x.WorkSideName,
                                   CreatedOn = s.x.CreatedOn,
                                   //IsDeleted=s.x.IsDeleted

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Add and Update WorkSide Details --
        [HttpPost]
        public ActionResult AddWorkSideDetails(WorkSideModel oModel)
        {
            int WorkSideId = oModel.WorkSideId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    if (WorkSideId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblWorkSides.Where(u => u.WorkSideName == oModel.WorkSideName && u.IsDeleted==false).SingleOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblWorkSide objWorkSide = new tblWorkSide();
                                //objWorkSide.WorkSideId = oModel.WorkSideId;
                                objWorkSide.WorkSideName = oModel.WorkSideName;
                                objWorkSide.IsDeleted = false;
                                objWorkSide.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblWorkSides.Add(objWorkSide);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblWorkSides.Where(u => (u.WorkSideName == oModel.WorkSideName) && (u.WorkSideId != oModel.WorkSideId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblWorkSide objWorkSide = db.tblWorkSides.Where(o => o.WorkSideId == oModel.WorkSideId).SingleOrDefault();                               
                                objWorkSide.WorkSideName = oModel.WorkSideName;
                                objWorkSide.IsDeleted = false;
                                objWorkSide.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }
                        }
                    }
                    ModelState.Clear();
                    //return View("Index", oModel);
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_EditPartialWorksideView", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- Edit WorkSide Details --
        public ActionResult EditWorkSideByWorkSideId(int id)
        {
            int WorkSideId = id;
            WorkSideModel objModel = new WorkSideModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oWorkSideDetails = db.tblWorkSides.Where(o => o.WorkSideId == id).SingleOrDefault();
                        if (oWorkSideDetails != null)
                        {
                            objModel.WorkSideId = oWorkSideDetails.WorkSideId;                         
                            objModel.WorkSideName = oWorkSideDetails.WorkSideName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_EditPartialWorksideView", objModel);
        }
        #endregion

        #region -- Delete WorkSide Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblWorkSide objDisp = db.tblWorkSides.SingleOrDefault(o => o.WorkSideId == id);
                    objDisp.IsDeleted = true;
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