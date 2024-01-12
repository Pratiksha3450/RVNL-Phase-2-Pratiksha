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
    public class DisciplineController : Controller
    {
        // GET: Discipline
        [PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }

        #region --- List Discipline Values ---
        public ActionResult Discipline_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.tblDisciplines
                           where x.IsDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new DisciplineModel
                               {
                                   DisciplineId = s.x.DispId,
                                   DisciplineCode = s.x.DispCode,
                                   DisciplineName = s.x.DispName,
                                   CreatedOn = s.x.CreatedOn,
                                   //IsDeleted=s.x.IsDeleted

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Add and Update Discipline Details --
        [HttpPost]
        public ActionResult AddDisciplineDetails(DisciplineModel oModel)
        {
            int disciplineId = oModel.DisciplineId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    if (disciplineId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblDisciplines.Where(u => u.DispCode == oModel.DisciplineCode || u.DispName == oModel.DisciplineName).SingleOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblDiscipline objDiscipline = new tblDiscipline();
                                //objDiscipline.DispId = oModel.DisciplineId;
                                objDiscipline.DispCode = oModel.DisciplineCode;
                                objDiscipline.DispName = oModel.DisciplineName;
                                objDiscipline.IsDeleted = false;
                                objDiscipline.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblDisciplines.Add(objDiscipline);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblDisciplines.Where(u => (u.DispName == oModel.DisciplineName) && (u.DispId != oModel.DisciplineId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblDiscipline objDiscipline = db.tblDisciplines.Where(o => o.DispId == oModel.DisciplineId).SingleOrDefault();
                                objDiscipline.DispCode = oModel.DisciplineCode;
                                objDiscipline.DispName = oModel.DisciplineName;
                                objDiscipline.IsDeleted = false;
                                objDiscipline.CreatedOn = DateTime.UtcNow.AddHours(5.5);
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
                    return View("_PartialEditDiscipline", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- Edit Discipline Details --
        public ActionResult EditDisciplineByDisciplineId(int id)
        {
            int disciplineId = id;
            DisciplineModel objModel = new DisciplineModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oDisciplineDetails = db.tblDisciplines.Where(o => o.DispId == id).SingleOrDefault();
                        if (oDisciplineDetails != null)
                        {
                            objModel.DisciplineId = oDisciplineDetails.DispId;
                            objModel.DisciplineCode = oDisciplineDetails.DispCode;
                            objModel.DisciplineName = oDisciplineDetails.DispName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditDiscipline", objModel);
        }
        #endregion

        #region -- Delete Discipline Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblDiscipline objDisp = db.tblDisciplines.SingleOrDefault(o => o.DispId == id);
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