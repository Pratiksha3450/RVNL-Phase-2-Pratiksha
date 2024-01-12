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
    //[AreaSessionExpire]
    [HandleError]
    public class EnclosureController : Controller
    {
        // GET: Enclouser
        public ActionResult Index()
        {
            return View();
        }

        #region --- List Enclouser Values ---
        public ActionResult Enclouser_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.tblEnclosures
                           where x.IsDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new EnclouserModel
                               {
                                   EnclId = s.x.EnclId,                                  
                                   EnclName = s.x.EnclName,
                                   CreatedOn = s.x.CreatedOn,
                                   //IsDeleted=s.x.IsDeleted

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Add and Update Enclouser Details --
        [HttpPost]
        public ActionResult AddEnclouserDetails(EnclouserModel oModel)
        {
            int EnclId = oModel.EnclId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    if (EnclId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblEnclosures.Where(u => u.EnclName == oModel.EnclName && u.IsDeleted==false).SingleOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblEnclosure objEnclouser = new tblEnclosure();
                                //objEnclouser.EnclId = oModel.EnclId;
                                objEnclouser.EnclName = oModel.EnclName;
                                objEnclouser.IsDeleted = false;
                                objEnclouser.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblEnclosures.Add(objEnclouser);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblEnclosures.Where(u => (u.EnclName == oModel.EnclName) && (u.EnclId != oModel.EnclId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblEnclosure objEnclouser = db.tblEnclosures.Where(o => o.EnclId == oModel.EnclId).SingleOrDefault();                               
                                objEnclouser.EnclName = oModel.EnclName;
                                objEnclouser.IsDeleted = false;
                                objEnclouser.CreatedOn = DateTime.UtcNow.AddHours(5.5);
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
                    return View("_EditPartialEnclouserView", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- Edit Enclouser Details --
        public ActionResult EditEnclouserByEnclId(int id)
        {
            int EnclId = id;
            EnclouserModel objModel = new EnclouserModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oEnclouserDetails = db.tblEnclosures.Where(o => o.EnclId == id).SingleOrDefault();
                        if (oEnclouserDetails != null)
                        {
                            objModel.EnclId = oEnclouserDetails.EnclId;                         
                            objModel.EnclName = oEnclouserDetails.EnclName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_EditPartialEnclouserView", objModel);
        }
        #endregion

        #region -- Delete Enclouser Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblEnclosure objDisp = db.tblEnclosures.SingleOrDefault(o => o.EnclId == id);
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