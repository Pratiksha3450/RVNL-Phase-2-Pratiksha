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
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Globalization;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class StripPkgProgressController : Controller
    {
        public string IpAddress = "";
        #region --- INdex ----
        public ActionResult Index()
        {
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Strip Package Progress", "View", UserID, IpAddress, "NA");
            var objUserM = (UserModel)Session["UserData"];

            StripProgressModel obj = new StripProgressModel();
            obj.PackageId = objUserM.RoleTableID;
            return View(obj);
        }
        #endregion

        #region ---- status list ----
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

            if (!string.IsNullOrEmpty(text))
            {
                obj = obj.Where(p =>
               CultureInfo.CurrentCulture.CompareInfo.IndexOf
               (p.SName, text, CompareOptions.IgnoreCase) >= 0).ToList();
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ---- Get strip package activty list to bind dropdown
        /// <summary>
        /// Gets the package activity.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public JsonResult Get_PackageActivity(int? id, int? did, string text)
        {
            List<PackageActivityModel> obj = new List<PackageActivityModel>();
            try
            {

                using (var db = new dbRVNLMISEntities())
                {
                    obj = (from a in db.tblStripPkgActs
                           join b in db.tblConsActivities on a.ConsActId equals b.ConsActId
                           where a.PackageId == id && a.DispId == did && a.isDeleted == false
                           select new { a, b })
                            .AsEnumerable().Select(s =>
                               new PackageActivityModel
                               {
                                   Activity = s.b.ActivityName,
                                   ActivityId = (int)s.a.StripActId, // its a strip table id not as cons activity id, as we need stripactivity table id to store in progress table so to reduce time used it
                                   Seqense = (int)s.a.Sequence
                               }).OrderByDescending(x => x.Seqense).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.Activity, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

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
                        var res = db.tblStripPkgProgresses.Where(o => o.ActProgressId == id).SingleOrDefault();
                        if (res != null)
                        {
                            //db.tblStripPkgProgresses.Remove(res);
                            res.isDeleted = true;
                            res.ModifiedOn = DateTime.Now;
                            db.SaveChanges();
                            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(IpAddress))
                            {
                                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                            }
                            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                            int UserID = ((UserModel)Session["UserData"]).UserId;
                            int k = Functions.SaveUserLog(pkgId, "Strip Package Progress", "Delete", UserID, IpAddress, "NA" );
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

        #region ---- Bind Partial View ----
        public ActionResult LoadPartialView()
        {
            StripProgressModel model = new StripProgressModel();

            return PartialView("_PartialUpdateProgress", model);
        }
        #endregion

        #region ---- Add/Update data
        /// <summary>
        /// Adds & update progress.
        /// </summary>
        /// <param name="oModel">The o model.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddUpdateProgress(StripProgressModel oModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return PartialView("_PartialUpdateProgress", oModel);
                }
                else
                {
                    if (oModel.PackageId == 0 || oModel.StripActId == 0 || oModel.Sid == 0 || oModel.DispId == 0)
                    {
                        return Json(0);
                    }

                    using (var db = new dbRVNLMISEntities())
                    {
                        int _start = Functions.RepalceCharacter(oModel.StartChainage);
                        int _end = Functions.RepalceCharacter(oModel.EndChainage);

                        if (_start == _end) // length must be >0
                        {
                            return Json(22); // length must be >0
                        }
                        if (_start > _end)
                        {
                            int temp = _start;
                            _start = _end;
                            _end = temp;
                        }
                        var objPkg = db.tblPackages.Where(o => o.PackageId == oModel.PackageId).SingleOrDefault();
                        if (oModel.ActProgressId == 0)
                        {
                            tblStripPkgProgress obj = new tblStripPkgProgress();

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
                                    return Json(-2); // must be within package chainage rnge
                                }
                            }
                            else
                            {
                                return Json(-1); // start end chaninage can't be 0
                            }

                            obj.StripActId = oModel.StripActId;
                            obj.DataDate = oModel.DataDate;
                            obj.PackageId = oModel.PackageId;
                            obj.Status = oModel.Sid == 1 ? "In progress" : "Completed";
                            obj.isDeleted = false;
                            obj.DispId = oModel.DispId;
                            db.tblStripPkgProgresses.Add(obj);
                            db.SaveChanges();
                            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(IpAddress))
                            {
                                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                            }
                            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                            int UserID = ((UserModel)Session["UserData"]).UserId;
                            int k = Functions.SaveUserLog(pkgId, "Strip Package Progress", "Save", UserID, IpAddress, "Activity" + oModel.DisciplineName);
                            ModelState.Clear();
                            return Json(1);
                        }
                        else { return Json(0); }
                        #region ---- Edit button removed so below code not in use ----
                        //else
                        //{
                        //    var dbres = db.tblStripPkgProgresses.Where(o => o.ActProgressId == oModel.ActProgressId).SingleOrDefault();
                        //    if (_start != 0 && _end != 0)
                        //    {
                        //        int _pkgStart = Functions.RepalceCharacter(objPkg.StartChainage);
                        //        int _pkgEnd = Functions.RepalceCharacter(objPkg.EndChainage);
                        //        if ((_start >= _pkgStart && _start <= _pkgEnd) && (_end >= _pkgStart && _end <= _pkgEnd))
                        //        {
                        //            //check if new inpute activity chainage range is already exist in project or not
                        //            string isChainageValid = CheckActivityValidation(oModel);
                        //            if (isChainageValid != "ok")
                        //            {
                        //                return Json(isChainageValid);
                        //            }
                        //            dbres.EndChainage = Functions.ParseDouble(_end.ToString());
                        //            dbres.StartChainage = Functions.ParseDouble(_start.ToString());
                        //        }
                        //        else
                        //        {
                        //            return Json(-2);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        return Json(-1);
                        //    }
                        //    dbres.StripActId = oModel.StripActId;
                        //    dbres.DataDate = oModel.DataDate;
                        //    dbres.PackageId = oModel.PackageId;
                        //    dbres.Status = oModel.Sid == 1 ? "In progress" : "Completed";
                        //    db.SaveChanges();
                        //    ModelState.Clear();
                        //    return Json(2);
                        //}
                        #endregion ---- Edit button removed so below code not in use ----
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

                    var sqobj = db.tblStripPkgActs.Where(o => o.StripActId == id).FirstOrDefault(); // get activity sequence number 
                    //int _firstSeq = (int)(db.tblStripPkgActs.Where(o => o.PackageId == sqobj.PackageId && o.isDeleted==false).OrderBy(o => o.Sequence).FirstOrDefault().Sequence); // take first
                    int _firstSeq = (int)(db.tblStripPkgActs.Where(o => o.PackageId == sqobj.PackageId && o.DispId == sqobj.DispId && o.isDeleted == false).OrderByDescending(o => o.Sequence).FirstOrDefault().Sequence); // take last
                    int sq = (int)sqobj.Sequence;
                    if (sq == _firstSeq)
                    {
                        return Json("1", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // sq--;
                        sq++;
                        var prevSqObj = db.viewStripProgress1.Where(o => o.PackageId == sqobj.PackageId && o.DispId == sqobj.DispId && o.Sequence == sq && o.isDeleted == false).FirstOrDefault(); // check if previouse activiy is present in update table
                        if (prevSqObj == null)
                        {

                            int conActId = (int)(db.tblStripPkgActs.Where(o => o.PackageId == sqobj.PackageId && o.DispId == sqobj.DispId && o.Sequence == sq && o.isDeleted == false).SingleOrDefault().ConsActId);
                            string activityName = db.tblConsActivities.Where(o => o.ConsActId == conActId).SingleOrDefault().ActivityName;
                            return Json("Previous ACtivity '" + activityName + "' is Not Started- Please Check.", JsonRequestBehavior.AllowGet);
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
        #endregion

        #region ---- activity chainage and seq validation during add/update ----
        /// <summary>
        /// Checks the activity validation.
        /// </summary>
        /// <param name="oModel">The o model.</param>
        /// <returns></returns>
        private string CheckActivityValidation(StripProgressModel oModel)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    string res = string.Empty;
                    int _start = Functions.RepalceCharacter(oModel.StartChainage);
                    int _end = Functions.RepalceCharacter(oModel.EndChainage);
                    var sqobj = db.tblStripPkgActs.Where(o => o.StripActId == oModel.StripActId).FirstOrDefault(); // get activity sequence number 
                                                                                                                   // int _firstSeq = (int)(db.tblStripPkgActs.Where(o => o.PackageId == sqobj.PackageId).OrderBy(o => o.Sequence).FirstOrDefault().Sequence);  // take first
                    int _packageLastSeqNo = (int)(db.tblStripPkgActs.Where(o => o.PackageId == sqobj.PackageId && o.DispId == sqobj.DispId && o.isDeleted == false).OrderByDescending(o => o.Sequence).FirstOrDefault().Sequence);  // take last seq
                    int objModelSeqNo = (int)sqobj.Sequence;

                    if (objModelSeqNo != _packageLastSeqNo) // check if entered seq is package's last seq 
                    {
                        // sq--;                      
                        objModelSeqNo++;
                        var prevSqObj = db.viewStripProgress1.Where(o => o.PackageId == sqobj.PackageId && o.DispId == sqobj.DispId && o.Sequence == objModelSeqNo && o.isDeleted == false).FirstOrDefault(); // check if previouse activiy is present in update table
                        if (prevSqObj == null)
                        {
                            int conActId = (int)(db.tblStripPkgActs.Where(o => o.PackageId == sqobj.PackageId && o.DispId == sqobj.DispId && o.Sequence == objModelSeqNo && o.isDeleted == false).SingleOrDefault().ConsActId);
                            string activityName = db.tblConsActivities.Where(o => o.ConsActId == conActId).SingleOrDefault().ActivityName;
                            return "Warning : Previous ACtivity '" + activityName + "' is Not Started- Please Check.";
                        }
                        else
                        {
                            //var isChainageFilledInPrevAct = db.viewStripProgress1.Where(o => o.PackageId == oModel.PackageId && o.StripActId == prevSqObj.StripActId && o.StartChainage == _start && o.EndChainage == _end && o.isDeleted == false && o.Status== "Completed").SingleOrDefault();
                            var prevCompleteAct = db.viewStripProgress1.Where(o => o.PackageId == oModel.PackageId && 
                                                                                  o.DispId == prevSqObj.DispId && 
                                                                                  o.StripActId == prevSqObj.StripActId && 
                                                                                  o.isDeleted == false && 
                                                                                  o.Status == "Completed").OrderBy(o => o.EndChainage).ToList(); ;
                            if (prevCompleteAct == null || prevCompleteAct.Count == 0)
                            {
                                return "Warning : Previous ACtivity " + prevSqObj.ActivityName + " for same chainages is In-Progress or Not Started- Please Check.";
                            }
                            else
                            {
                                for (int i = 0; i < prevCompleteAct.Count; i++)
                                {
                                    if ((_start >= prevCompleteAct[i].StartChainage && _start <= prevCompleteAct[i].EndChainage))
                                    {
                                        if ((_end >= prevCompleteAct[i].StartChainage && _end <= prevCompleteAct[i].EndChainage))
                                        {
                                            res = "ok";
                                            break;
                                        }
                                        int j = i;
                                        while ((j != prevCompleteAct.Count - 1) && prevCompleteAct[j].EndChainage == prevCompleteAct[j + 1].StartChainage)
                                        {
                                            if (_end >= prevCompleteAct[j + 1].StartChainage && _end <= prevCompleteAct[j + 1].EndChainage)
                                            {
                                                res = "ok";
                                                break;
                                            }
                                            j++;
                                        }
                                        if (res == "ok")
                                        {
                                            break;
                                        }
                                    }

                                }
                                if (res != "ok")
                                {
                                    return "Warning : Previous ACtivity " + prevSqObj.ActivityName + " for same chainages is In-Progress or Not Started- Please Check.";
                                }
                            }

                        }

                    }
                    // check current sequece activity listings
                    var objExistingAct = db.viewStripProgress1.Where(o => o.PackageId == oModel.PackageId && o.DispId == oModel.DispId && o.StripActId == oModel.StripActId && o.isDeleted == false).ToList().OrderBy(o => o.EndChainage).ToList();
                    if (objExistingAct.Count > 0)
                    {
                        res = "ok";
                        if (oModel.Sid == 2) // status Completed
                        {
                            
                            foreach (var item in objExistingAct.Where(o => o.Status == "Completed").OrderBy(x => x.EndChainage).ToList())
                            {
                                if (_start > item.StartChainage && _start < item.EndChainage)
                                {
                                    res = "Warning : start chainage overlapping existing Completed records";
                                    break;
                                }
                                else if (_end > item.StartChainage && _end < item.EndChainage)
                                {
                                    res = "Warning : end chainage overlapping existing Completed records";
                                    break;
                                }
                                else if (_start == item.StartChainage && _end == item.EndChainage && oModel.ActProgressId == 0)
                                {
                                    res = "Warning : Records are already exist for selected package activity which are completed";
                                    break;
                                }
                                else
                                {
                                    res = "ok";
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in objExistingAct.Where(o => o.Status != "Completed").OrderBy(x => x.EndChainage).ToList())
                            {
                                if (_start > item.StartChainage && _start < item.EndChainage)
                                {
                                    res = "Warning : start chainage overlapping existing in progress records";
                                    break;
                                }
                                else if (_end > item.StartChainage && _end < item.EndChainage)
                                {
                                    res = "Warning : end chainage overlapping existing in progress records";
                                    break;
                                }
                                else if (_start == item.StartChainage && _end == item.EndChainage && oModel.ActProgressId == 0)
                                {
                                    res = "Warning : Records are already exist for selected in progress package activity";
                                    break;
                                }
                                else
                                {
                                    res = "ok";
                                }
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
            StripProgressModel obj = new StripProgressModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var res = db.viewStripProgress1.Where(o => o.ActProgressId == id).SingleOrDefault();
                        if (res != null)
                        {
                            obj.ActProgressId = (int)res.ActProgressId;
                            obj.PackageId = (int)res.PackageId;
                            obj.StripActId = (int)res.StripActId;
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
        public ActionResult GetUpdateChartDetailList([DataSourceRequest]  DataSourceRequest request, int? packageId, int? DispId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<GetRoleAssignedPackageList_Result> pkgs = Functions.GetRoleAccessiblePackageList();
                var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();
                var lst = (from x in dbContext.viewStripProgress1
                           where x.isDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new StripProgressModel
                               {
                                   ActProgressId = s.x.ActProgressId,
                                   PackageId = (int)s.x.PackageId,
                                   PackageName = s.x.PackageName,
                                   ActivityName = s.x.ActivityName,
                                   StripActId = (int)s.x.StripActId,
                                   DataDate = (DateTime)s.x.DataDate,
                                   EndChainage = IntToStringChainage(Convert.ToInt32((double)s.x.EndChainage)),
                                   StartChainage = IntToStringChainage(Convert.ToInt32((double)s.x.StartChainage)),
                                   Sid = s.x.Status == "completed" ? 2 : 1,
                                   Status = s.x.Status,
                                   Sequence = (int)s.x.Sequence,
                                   ConsActId = (int)s.x.ConsActId,
                                   DispId =  Convert.ToInt32(s.x.DispId),
                                   DisciplineName = Convert.ToString(s.x.DispName),
                                   StartC = (double)s.x.StartChainage,
                                   EndC = (double)s.x.EndChainage

                               }).ToList();

                if (packageId == 0 || packageId == null)
                {
                    lst = lst.Where(w => accessiblePackageList.Contains(w.PackageId)).ToList();
                }
                else if (DispId == 0 || DispId == null)
                {
                    lst = lst.Where(w => w.PackageId == packageId).ToList();
                }
                else
                {
                    lst = lst.Where(w => w.PackageId == packageId && w.DispId == DispId).ToList();

                }
                lst = lst.OrderBy(a => a.Sequence).ThenBy(a => a.StartC).ToList();
                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}