using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.DAC;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class UpdateProgressChartController : Controller
    {
        // GET: UpdateProgressChart
        public ActionResult Index()
        {
            var objUserM = (UserModel)Session["UserData"];

            UpdateProgressChartModel obj = new UpdateProgressChartModel();
            obj.PackageId = objUserM.RoleTableID;
            return View(obj);
        }

        /// <summary>
        /// Gets the status list.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public JsonResult GetStatusList(string text)
        {
            List<ProgressStatus> obj = new List<ProgressStatus>();
            obj.Add(new ProgressStatus { SId = 1, SName = "In progress" });
            obj.Add(new ProgressStatus { SId = 2, SName = "Completed" });
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the package activity.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public JsonResult Get_PackageActivity(int? id)
        {
            List<PackageActivityModel> obj = new List<PackageActivityModel>();
            try
            {

                using (var db = new dbRVNLMISEntities())
                {
                    obj = db.tblScActivityDetails.Where(o => o.PackageId == id && o.IsDeleted == false).Select(s => new PackageActivityModel
                    {
                        Activity = s.ScActName,
                        ActivityId = s.ScActID
                    }).ToList();

                    foreach (var item in obj)
                    {
                        var sq = db.tblScCsDetails.Where(o => o.ScActID == item.ActivityId).FirstOrDefault().SeqNo;
                        item.Seqense = sq == null ? 0 : (int)sq;
                    }
                    obj.OrderBy(o => o.Seqense);
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Checks if activity sequense isfollowed.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public JsonResult CheckIfActivitySequenseIsfollowed(int? id)
        {

            try
            {
                if (id == null)
                {
                    return Json("Please select Activity", JsonRequestBehavior.AllowGet);
                }
                using (var db = new dbRVNLMISEntities())
                {
                    var sqobj = db.tblScCsDetails.Where(o => o.ScActID == id).FirstOrDefault(); // get activity sequence number 
                    int sq = (int)sqobj.SeqNo;
                    if (sq == 1)
                    {
                        return Json("1", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        sq--;
                        var newact = db.tblScCsDetails.Where(o => o.PackageId == sqobj.PackageId && o.SeqNo == sq).FirstOrDefault(); // to get activity which prevoius to slected activity

                        var newSqObj = db.viewScChainageUpdateCharts.Where(o => o.PackageId == sqobj.PackageId && o.ActivityId == newact.ScActID).FirstOrDefault(); // check if previouse activiy is present in update table
                        if (newSqObj == null)
                        {
                            return Json("Sequence of activity must be followed", JsonRequestBehavior.AllowGet);
                        }

                    }
                    return Json("1", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Exception occured while validating activity sequense", JsonRequestBehavior.AllowGet);
            }
        }

        #region ---- Delete Records ----
        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var res = db.tblScChainageUpdates.Where(o => o.Id == id).SingleOrDefault();
                        if (res != null)
                        {
                            db.tblScChainageUpdates.Remove(res);
                            db.SaveChanges();
                        }
                    }
                }
                return Json("1");
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion ---- Delete Records ----

        public ActionResult LoadPartialView()
        {
            UpdateProgressChartModel model = new UpdateProgressChartModel();
            
            return PartialView("_PartialUpdateProgress", model);
        }
        #region ---- Add/Update data
        /// <summary>
        /// Adds & update progress.
        /// </summary>
        /// <param name="oModel">The o model.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddUpdateProgress(UpdateProgressChartModel oModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {                    
                    return PartialView("_PartialUpdateProgress", oModel);
                }
                else
                {
                    if (oModel.PackageId == 0 || oModel.ActivityId == 0 || oModel.Sid == 0)
                    {
                        return Json(0);
                    }

                    using (var db = new dbRVNLMISEntities())
                    {
                        int _start = Functions.RepalceCharacter(oModel.StartChainage);
                        int _end = Functions.RepalceCharacter(oModel.EndChainage);
                        if (_start > _end)
                        {
                            int temp = _start;
                            _start = _end;
                            _end = temp;
                        }
                        var objPkg = db.tblPackages.Where(o => o.PackageId == oModel.PackageId).SingleOrDefault();
                        if (oModel.Id == 0)
                        {
                            tblScChainageUpdate obj = new tblScChainageUpdate();

                            if (_start != 0 && _end != 0)
                            {
                                int _pkgStart = Functions.RepalceCharacter(objPkg.StartChainage);
                                int _pkgEnd = Functions.RepalceCharacter(objPkg.EndChainage);
                                if ((_start >= _pkgStart && _start <= _pkgEnd) && (_end >= _pkgStart && _end <= _pkgEnd))
                                {
                                    //check if new inpute activity chainage range is already exist in project or not
                                    string isChainageValid = CheckActivityValidation(oModel);
                                    if (isChainageValid != "ok")
                                    {
                                        return Json(isChainageValid);
                                    }
                                    obj.EndChainage = Functions.ParseDouble(_end.ToString());
                                    obj.StartChainage = Functions.ParseDouble(_start.ToString());
                                }
                                else
                                {
                                    return Json(-2);
                                }
                            }
                            else
                            {
                                return Json(-1);
                            }

                            obj.ActivityId = oModel.ActivityId;
                            obj.DataDate = oModel.DataDate;
                            obj.PackageId = oModel.PackageId;
                            obj.Status = oModel.Sid == 1 ? "In progress" : "Completed";
                            db.tblScChainageUpdates.Add(obj);
                            db.SaveChanges();
                            ModelState.Clear();
                            return Json(1);
                        }
                        else
                        {
                            var dbres = db.tblScChainageUpdates.Where(o => o.Id == oModel.Id).SingleOrDefault();
                            if (_start != 0 && _end != 0)
                            {
                                int _pkgStart = Functions.RepalceCharacter(objPkg.StartChainage);
                                int _pkgEnd = Functions.RepalceCharacter(objPkg.EndChainage);
                                if ((_start >= _pkgStart && _start <= _pkgEnd) && (_end >= _pkgStart && _end <= _pkgEnd))
                                {
                                    //check if new inpute activity chainage range is already exist in project or not
                                    string isChainageValid = CheckActivityValidation(oModel);
                                    if (isChainageValid != "ok")
                                    {
                                        return Json(isChainageValid);
                                    }
                                    dbres.EndChainage = Functions.ParseDouble(_end.ToString());
                                    dbres.StartChainage = Functions.ParseDouble(_start.ToString());
                                }
                                else
                                {
                                    return Json(-2);
                                }
                            }
                            else
                            {
                                return Json(-1);
                            }
                            dbres.ActivityId = oModel.ActivityId;
                            dbres.DataDate = oModel.DataDate;
                            dbres.PackageId = oModel.PackageId;
                            dbres.Status = oModel.Sid == 1 ? "In progress" : "Completed";
                            db.SaveChanges();
                            ModelState.Clear();
                            return Json(2);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                return View(oModel);
            }
        }
        #endregion

        #region ---- Validate Activity chainage ----
        /// <summary>
        /// Checks the activity validation.
        /// </summary>
        /// <param name="oModel">The o model.</param>
        /// <returns></returns>
        private string CheckActivityValidation(UpdateProgressChartModel oModel)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    string res = string.Empty;
                    int _start = Functions.RepalceCharacter(oModel.StartChainage);
                    int _end = Functions.RepalceCharacter(oModel.EndChainage);

                    var sqobj = db.tblScCsDetails.Where(o => o.ScActID == oModel.ActivityId).FirstOrDefault(); // get activity sequence number 
                    int sq = (int)sqobj.SeqNo;
                    if (sq != 1)
                    {
                        sq--;
                        var prevAct = db.tblScCsDetails.Where(o => o.PackageId == sqobj.PackageId && o.SeqNo == sq).FirstOrDefault(); // to get activity which prevoius to slected activity

                        var prevSqObj = db.viewScChainageUpdateCharts.Where(o => o.PackageId == sqobj.PackageId && o.ActivityId == prevAct.ScActID).FirstOrDefault(); // check if previouse activiy is present in update table
                        if (prevSqObj == null)
                        {
                            return "Warning : Sequence of activity must be followed";
                        }
                        else
                        {
                            var isChainageFilledInPrevAct = db.viewScChainageUpdateCharts.Where(o => o.PackageId == oModel.PackageId && 
                                                                                                o.ActivityId == prevAct.ScActID && o.StartChainage == _start &&
                                                                                                o.EndChainage == _end).SingleOrDefault();
                            if (isChainageFilledInPrevAct==null)
                            {
                                return "Warning : There is no records available for " + prevSqObj.ScActName + " activity on entered chainage.";
                            }
                        }

                    }

                    var objExistingAct = db.tblScChainageUpdates.Where(o => o.PackageId == oModel.PackageId && o.ActivityId == oModel.ActivityId).ToList().OrderBy(o => o.EndChainage).ToList();
                    if (objExistingAct.Count > 0)
                    {
                        foreach (var item in objExistingAct)
                        {
                            if (_start > item.StartChainage && _start < item.EndChainage)
                            {
                                res = "Warning : start chainage overlapping existing records";
                                break;
                            }
                            else if (_end > item.StartChainage && _end < item.EndChainage)
                            {
                                res = "Warning : end chainage overlapping existing records";
                                break;
                            }
                            else if (_start == item.StartChainage && _end == item.EndChainage && oModel.Id == 0)
                            {
                                res = "Warning : Records are already exist for selected package activity";
                                break;
                            }
                            else
                            {
                                res = "ok";
                            }
                        }
                        return res;
                    }
                    else
                    {
                        return "ok";
                    }

                }
            }
            catch (Exception ex)
            {

                return "Warning : Something went wrong during chainage verifcation, Please try again.";
            }
        }
        #endregion

        #region ---- Chainage int to String ----
        /// <summary>
        /// Ints to string chainage.
        /// </summary>
        /// <param name="chainage">The chainage.</param>
        /// <returns></returns>
        public string IntToStringChainage(int chainage)
        {
            return chainage.ToString("D6").Insert(chainage.ToString("D6").Length - 3, "+");
        }
        #endregion

        #region ---- Edit Call ---
        /// <summary>
        /// Edits the data.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ActionResult EditData(int id)
        {
            UpdateProgressChartModel obj = new UpdateProgressChartModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var res = db.viewScChainageUpdateCharts.Where(o => o.Id == id).SingleOrDefault();
                        if (res != null)
                        {
                            obj.PackageId = (int)res.PackageId;
                            obj.ActivityId = (int)res.ActivityId;
                            obj.Sid = res.Status == "Completed" ? 2 : 1;
                            obj.StartChainage = IntToStringChainage(Functions.ParseInteger(res.StartChainage.ToString()));
                            obj.EndChainage = IntToStringChainage(Functions.ParseInteger(res.EndChainage.ToString()));
                            obj.Lenght = (res.EndChainage - res.StartChainage).ToString();
                            obj.DataDate = (DateTime)res.DataDate;
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return View("_PartialUpdateProgress", obj);
        }
        #endregion

        #region ---- grid fill ----
        /// <summary>
        /// Gets the update chart detail list.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public ActionResult GetUpdateChartDetailList([DataSourceRequest]  DataSourceRequest request, int? packageId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<GetRoleAssignedPackageList_Result> pkgs = Functions.GetRoleAccessiblePackageList();
                var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();
                var lst = (from x in dbContext.viewScChainageUpdateCharts
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new UpdateProgressChartModel
                               {
                                   PackageCode=s.x.PackageCode,
                                   PackageShortName=s.x.PackageShortName,
                                   Id = s.x.Id,
                                   PackageId = (int)s.x.PackageId,
                                   PackageName = s.x.PackageName,
                                   Activity = s.x.ScActName,
                                   ActivityId = (int)s.x.ActivityId,
                                   DataDate = (DateTime)s.x.DataDate,
                                   EndChainage = IntToStringChainage(Convert.ToInt32((double)s.x.EndChainage)),
                                   StartChainage = IntToStringChainage(Convert.ToInt32((double)s.x.StartChainage)),
                                   Sid = s.x.Status == "completed" ? 2 : 1,
                                   Status = s.x.Status

                               }).ToList();

                if (packageId == 0 || packageId==null)
                {
                    lst = lst.Where(w => accessiblePackageList.Contains(w.PackageId)).ToList();
                }
                else 

                {
                    lst = lst.Where(w => w.PackageId == packageId).ToList();
                }

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
   

   
}