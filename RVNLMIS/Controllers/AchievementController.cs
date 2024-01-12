using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNet.Identity;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using RVNLMIS.Models.PowerBI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    public class AchievementController : Controller
    {
        // GET: Achievement

        [PageAccessFilter]
        public ActionResult Index()
        {
            GetProjects();
            return View();
        }

        public ActionResult _AddEditAchievement()
        {
            GetProjects();
            return View();
        }

        public ActionResult GetProjects()
        {
            dbRVNLMISEntities db = new dbRVNLMISEntities();
            var GetProjects = db.tblMasterProjects.ToList();
            SelectList list = new SelectList(GetProjects, "ProjectId", "ProjectName");
            ViewBag.ProjectList = list;
            return View();
        }


        #region -- Add and Update Achievement Details --
        [HttpPost]
        public ActionResult AddEditAchievement(AchievementModel oModel)
        {
            try
            {
                string message = string.Empty;
                if (oModel.AchId == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblAchievements.Where(u => u.ProjectId == oModel.projectId && u.IsDelete == false).FirstOrDefault();
                        if (exist != null)
                        {
                            message = "Already Exists";
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblAchievement objachieve = new tblAchievement();
                            objachieve.ProjectId = oModel.projectId;
                            objachieve.AchSeq = oModel.AchSeq;      
                            objachieve.AchDesc = oModel.AchDesc;
                            objachieve.AchQty = oModel.AchQty;
                            objachieve.Remark = oModel.Remark;
                            objachieve.AchQtyUnit = oModel.AchQtyUnit;
                            objachieve.IsDelete = false;
                            db.tblAchievements.Add(objachieve);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {

                        tblAchievement objachieve = db.tblAchievements.Where(u => u.AchID == oModel.AchId).SingleOrDefault();
                        objachieve.ProjectId = oModel.projectId;
                        objachieve.AchSeq = oModel.AchSeq;
                        objachieve.AchDesc = oModel.AchDesc;
                        objachieve.AchQty = oModel.AchQty;
                        objachieve.Remark = oModel.Remark;
                        objachieve.AchQtyUnit = oModel.AchQtyUnit;
                        db.SaveChanges();
                        message = "Updated Successfully";

                    }
                }
                ModelState.Clear();
                var result = new { message = message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion


        #region --- List Resource Values ---
        public ActionResult Achievement_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<ViewAchievement> obj = new List<ViewAchievement>();
                obj = dbContext.ViewAchievements.ToList();
                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region -- Edit Resource Details --
        [Audit]
        public ActionResult EditAchievementById(int id)
        {
            GetProjects();
            AchievementModel objachieve = new AchievementModel();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var oAchieveDetails = db.tblAchievements.Where(o => o.AchID == id && o.IsDelete == false).SingleOrDefault();
                        if (oAchieveDetails != null)
                        {
                            objachieve.projectId = (oAchieveDetails.ProjectId == null) ? 0 : (int)oAchieveDetails.ProjectId; ;
                            objachieve.AchId = oAchieveDetails.AchID;
                            objachieve.AchSeq = oAchieveDetails.AchSeq;      
                            objachieve.AchDesc = oAchieveDetails.AchDesc;
                            objachieve.AchQty = oAchieveDetails.AchQty;
                            objachieve.Remark = oAchieveDetails.Remark;
                            objachieve.AchQtyUnit = oAchieveDetails.AchQtyUnit;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditAchievement", objachieve);
        }
        #endregion


        #region -- Delete Achievement Details --
        [HttpPost]
        public ActionResult DeleteAchievement(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblAchievement objAchieve = db.tblAchievements.FirstOrDefault(o => o.AchID == id);
                    objAchieve.IsDelete = true;
                    db.SaveChanges();
                    return Json("success");
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }
        #endregion


    }
}