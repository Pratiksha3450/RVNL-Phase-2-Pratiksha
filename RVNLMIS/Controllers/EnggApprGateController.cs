using Kendo.Mvc.UI;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class EnggApprGateController : Controller
    {
        // GET: EnggApprGate
        [PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }

        #region --- List Entity Type Values ---
        public ActionResult EnggApprGate_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.tblEnggApprGates
                           where x.IsDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new EnggApprGateModel
                               {
                                   ApprGateId = s.x.ApprGateId,
                                   AppGateName = s.x.ApprGateName,
                                   Sequence = (int)s.x.Sequence,
                                   CreatedOn = s.x.CreatedOn
                                   //IsDeleted=s.x.IsDeleted

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region -- Edit Entity Type Details --
        public ActionResult EditEnggApprGateByApprId(int id)
        {
            int apprId = id;
            EnggApprGateModel objModel = new EnggApprGateModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oEnggApprGateDetails = db.tblEnggApprGates.Where(o => o.ApprGateId == id).SingleOrDefault();
                        if (oEnggApprGateDetails != null)
                        {
                            objModel.ApprGateId = oEnggApprGateDetails.ApprGateId;
                            objModel.AppGateName = oEnggApprGateDetails.ApprGateName;
                            //objModel.Sequence = Functions.ParseInteger(oEnggApprGateDetails.Sequence.ToString());
                            objModel.Sequence = (int)oEnggApprGateDetails.Sequence;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditEnggApprGate", objModel);
        }
        #endregion


        #region -- Add and Update Engg Dwg Type Details --
        [HttpPost]
        public ActionResult AddEnggApprGateDetails(EnggApprGateModel oModel)
        {
            int apprId = oModel.ApprGateId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    if (apprId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblEnggApprGates.Where(u => (u.ApprGateName == oModel.AppGateName) && u.IsDeleted == false).FirstOrDefault();
                            if (exist != null)
                            {
                                return Json("3", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                if (db.tblEnggApprGates.Where(u => (u.Sequence == oModel.Sequence) && u.IsDeleted == false).FirstOrDefault() != null)
                                {
                                    return Json("4", JsonRequestBehavior.AllowGet);
                                }

                                tblEnggApprGate objEnggApprGate = new tblEnggApprGate();
                                //objDiscipline.DispId = oModel.DisciplineId;
                                objEnggApprGate.ApprGateName = oModel.AppGateName;
                                objEnggApprGate.Sequence = oModel.Sequence;
                                objEnggApprGate.IsDeleted = false;
                                objEnggApprGate.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblEnggApprGates.Add(objEnggApprGate);
                                db.SaveChanges();
                                message = "1";
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblEnggApprGates.Where(u => u.ApprGateName == oModel.AppGateName && u.IsDeleted == false && u.ApprGateId != oModel.ApprGateId).ToList();
                            if (exist.Count != 0)
                            {
                                message = "3";
                            }
                            else
                            {
                                tblEnggApprGate objEnggApprGate = db.tblEnggApprGates.Where(o => o.ApprGateId == oModel.ApprGateId).SingleOrDefault();
                                objEnggApprGate.ApprGateName = oModel.AppGateName;
                                objEnggApprGate.Sequence = oModel.Sequence;
                                objEnggApprGate.IsDeleted = false;
                                objEnggApprGate.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.SaveChanges();
                                message = "2";
                            }
                        }
                    }
                    ModelState.Clear();
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_PartialEditEnggApprGate", oModel);
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
                    tblEnggApprGate objEnggApprGate = db.tblEnggApprGates.SingleOrDefault(o => o.ApprGateId == id);
                    objEnggApprGate.IsDeleted = true;
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