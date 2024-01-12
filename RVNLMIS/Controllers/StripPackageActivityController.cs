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
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class StripPackageActivityController : Controller
    {
        public string IpAddress = "";
        // GET: StripPackageActivity
        public ActionResult Index()
        {
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Strip Package Activity", "View", UserID, IpAddress, "NA");
            StripPackageActivityModel objModel = new StripPackageActivityModel();
            return View(objModel);
        }

        #region ---- Page load ---        
        /// <summary>
        /// Gets the package constant activity.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public JsonResult Get_PackageConstActivity(string text)
        {
            List<ConstActModel> obj = new List<ConstActModel>();
            try
            {

                using (var db = new dbRVNLMISEntities())
                {
                    obj = db.tblConsActivities.Where(o=> o.IsDelete == false && o.isStripChart==true).Select(s => new ConstActModel
                    {
                        ActivityName=s.ActivityName,
                        ConsActID=s.ConsActId
                        
                    }).OrderBy(x=>x.ActivityName).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        obj = obj.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.ActivityName, text, CompareOptions.IgnoreCase) >= 0).ToList(); 
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
                    obj = db.tblDisciplines.Where(o => o.IsDeleted   == false ).Select(s => new Discipline
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

        #endregion

        #region ---- Grid fill ----
        /// <summary>
        /// Gets the strip package activity.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="packageId">The PKG identifier.</param>
        /// <returns></returns>
        public ActionResult GetStripPackageActivityGrid([DataSourceRequest]  DataSourceRequest request, int? packageId, int? DispId)
        {
            List<StripPackageActivityModel> lstResult = new List<StripPackageActivityModel>();

            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                try
                {
                    List<GetRoleAssignedPackageList_Result> pkgs = Functions.GetRoleAccessiblePackageList();
                    var accessiblePackageList = pkgs.Select(s => s.PackageId).ToList();

                    lstResult = (from a in db.tblStripPkgActs
                                 join b in db.tblConsActivities on a.ConsActId equals b.ConsActId
                                 join c in db.tblPackages on a.PackageId equals c.PackageId
                                 join d in db.tblDisciplines on a.DispId equals d.DispId
                                 where a.isDeleted==false
                                 select new { a, b,c,d }).AsEnumerable()
                                .Select(x => new StripPackageActivityModel
                                {
                                    ConsActId = x.b.ConsActId,
                                    PackageId = (int)x.a.PackageId,
                                    ActivityName=x.b.ActivityName,
                                    Color=x.a.Color,
                                    PackageName=x.c.PackageName,
                                    Sequence =(int) x.a.Sequence,
                                    StripActId = x.a.StripActId,
                                    DispId =(int) x.a.DispId,
                                    DisciplineName=x.d.DispName

                                }).ToList();
                    if (DispId == 0 || DispId == null)
                    {
                        lstResult = lstResult.Where(w => w.PackageId == packageId).OrderBy(x => x.Sequence).ToList();
                    }
                    else
                    {
                        lstResult = lstResult.Where(w => w.PackageId == packageId && w.DispId==DispId).OrderBy(x=>x.Sequence).ToList();
                    }
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion

        #region ---- add / update activity reord -----
        /// <summary>
        /// Adds the update activity.
        /// </summary>
        /// <param name="oModel">The o model.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddUpdateActivity(StripPackageActivityModel oModel)
        {
            if (!ModelState.IsValid)
            {
               // ModelState.Clear();
                return PartialView("_PartialAddUpdate", oModel);
               // return View("Index",oModel);
            }
            else
            {
                string isValid = CheckIfValidSequence(oModel);
                if (isValid!="0")
                {
                    return Json(isValid);
                }
                using (var db = new dbRVNLMISEntities())
                {
                    try
                    {
                        if (oModel.StripActId == 0)
                        {
                            var objAct = db.tblStripPkgActs.Where(o => o.PackageId == oModel.PackageId && o.DispId==oModel.DispId && o.ConsActId == oModel.ConsActId  && o.isDeleted==false ).SingleOrDefault();
                            if (objAct!=null)
                            {
                                if (objAct.Sequence==oModel.Sequence)
                                {
                                    return Json(4);
                                }
                                else
                                {
                                    return Json(5);
                                }
                            }
                            tblStripPkgAct objTbl = new tblStripPkgAct();
                            objTbl.Color = oModel.Color;
                            objTbl.ConsActId = oModel.ConsActId;
                            objTbl.PackageId = oModel.PackageId;
                            objTbl.Sequence = oModel.Sequence;
                            objTbl.DispId = oModel.DispId;
                            objTbl.isDeleted = false;
                            db.tblStripPkgActs.Add(objTbl);
                            db.SaveChanges();
                            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(IpAddress))
                            {
                                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                            }
                            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                            int UserID = ((UserModel)Session["UserData"]).UserId;
                            int k = Functions.SaveUserLog(pkgId, "Strip Package Activity", "Save", UserID, IpAddress, "Activity" + oModel.ActivityName);
                           
                            return Json(1);
                        }
                        else
                        {
                            var objTbl = db.tblStripPkgActs.Where(o => o.StripActId == oModel.StripActId && o.isDeleted == false).SingleOrDefault();
                            if (objTbl != null)
                            {
                                objTbl.Color = oModel.Color;
                                objTbl.ConsActId = oModel.ConsActId;
                                objTbl.PackageId = oModel.PackageId;
                                objTbl.Sequence = oModel.Sequence;                                
                                db.SaveChanges();
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                // int k = Functions.SaveUserLogs(pkgId, "Strip Package Activity", "Update", UserID, IpAddress);

                                var UpdatedValue = (from ul in db.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();

                                string str = UpdatedValue.Replace("CreatedOn, ", "");
                                str = str.Replace("CreatedOn", "");
                                str = str.TrimStart(',');
                                int k = Functions.SaveUserLog(pkgId, "Strip Package Activity", "Update", UserID, IpAddress, str);
                                return Json(2);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }                    
                }
            }
            return Json(1);
        }
        #endregion

        #region ---- Validate sequence ----
        /// <summary>
        /// Checks if valid sequence.
        /// </summary>
        /// <param name="oModel">The o model.</param>
        /// <returns></returns>
        private string CheckIfValidSequence(StripPackageActivityModel oModel)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var isExist = db.tblStripPkgActs.Where(o => o.PackageId == oModel.PackageId && o.DispId==oModel.DispId && o.StripActId != oModel.StripActId && o.Sequence == oModel.Sequence && o.isDeleted == false).ToList();
                    if (isExist.Count>0)
                    {
                        return "3";
                    }
                    return "0";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region ---- Call edit action ----
        /// <summary>
        /// Edits the data.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ActionResult EditData(int id)
        {
            try
            {
                StripPackageActivityModel objTbl = new StripPackageActivityModel();
                if (id!=0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oModel = db.tblStripPkgActs.Where(o => o.StripActId==id).SingleOrDefault();
                        if (oModel!=null)
                        {
                            objTbl.Color = oModel.Color;
                            objTbl.ConsActId = (int)oModel.ConsActId;
                            objTbl.PackageId = (int)oModel.PackageId;
                            objTbl.Sequence = (int)oModel.Sequence;
                            objTbl.StripActId = oModel.StripActId;
                        }                        
                    }
                }
                return View("_PartialAddUpdate", objTbl);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion

        #region ---- Up/Down Sequence ----

        [HttpPost]
        public JsonResult MoveUp(int id)
        {
            try
            {
                if (id!=0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {

                        var StripDetails = db.tblStripPkgActs.Where(o => o.StripActId == id && o.isDeleted == false).SingleOrDefault();
                        if (StripDetails!=null)
                        {
                            int _currSeq = (int)StripDetails.Sequence;
                            int _prevSeq = _currSeq - 1;
                            if (_prevSeq!=0)
                            {
                                var prevSeq = db.tblStripPkgActs.Where(o => o.PackageId == StripDetails.PackageId && o.DispId == StripDetails.DispId && o.Sequence == _prevSeq && o.isDeleted == false).SingleOrDefault();
                                if (prevSeq!=null)
                                {
                                    prevSeq.Sequence = _currSeq;
                                    StripDetails.Sequence = _prevSeq;
                                    db.SaveChanges();
                                }
                            }
                            
                        }
                    }
                }
                return Json(0);
            }
            catch (Exception ex)
            {

                return Json(1);
            }
            
        }
        
        [HttpPost]
        public JsonResult MoveDown(int id)
        {
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {

                        var StripDetails = db.tblStripPkgActs.Where(o => o.StripActId == id && o.isDeleted == false).SingleOrDefault();
                        if (StripDetails != null)
                        {
                            int _totalAct = db.tblStripPkgActs.Where(x => x.PackageId == StripDetails.PackageId && x.DispId==StripDetails.DispId && x.isDeleted == false).ToList().Count();
                            int _currSeq = (int)StripDetails.Sequence;
                            int _NextSeq = _currSeq + 1;
                            if (_NextSeq <= _totalAct)
                            {
                                var prevSeq = db.tblStripPkgActs.Where(o => o.PackageId == StripDetails.PackageId && o.DispId == StripDetails.DispId && o.Sequence == _NextSeq && o.isDeleted == false).SingleOrDefault();
                                if (prevSeq != null)
                                {
                                    prevSeq.Sequence = _currSeq;
                                    StripDetails.Sequence = _NextSeq;
                                    db.SaveChanges();
                                }
                            }

                        }
                    }
                }
                return Json(0);
            }
            catch (Exception ex)
            {

                return Json(1);
            }
        }
        #endregion

        #region ---- Change Color ----        
        /// <summary>
        /// Changes the color.
        /// </summary>
        /// <param name="SId">The s identifier.</param>
        /// <param name="NewClr">The new color.</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChangeColor(int SId, string NewClr)
        {
            try
            {
                if (SId != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var res = db.tblStripPkgActs.Where(o => o.StripActId == SId && o.isDeleted == false).SingleOrDefault();
                        if (res != null)
                        {
                            res.Color = NewClr;
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
                    int _currentSeq = 0;
                    int _packageId = 0;
                    int _DispId = 0;
                    using (var db = new dbRVNLMISEntities())
                    {
                        var res = db.tblStripPkgActs.Where(o => o.StripActId == id && o.isDeleted == false).SingleOrDefault();
                        if (res != null)
                        {
                            _DispId = (int)res.DispId;
                            _packageId =(int) res.PackageId;
                            _currentSeq = (int)res.Sequence;
                            var _LastSeq = (db.tblStripPkgActs.Where(o => o.PackageId == _packageId && o.DispId== _DispId && o.isDeleted == false).OrderByDescending(o => o.Sequence).FirstOrDefault().Sequence);
                            //                            db.tblStripPkgActs.Remove(res);
                            res.isDeleted = true;
                            db.SaveChanges();
                            if (_currentSeq!=_LastSeq)
                            {
                                var objActivitiesList = db.tblStripPkgActs.Where(o => o.PackageId == _packageId && o.DispId == _DispId && o.isDeleted == false).OrderBy(x=>x.Sequence).ToList();
                                int newSeqCount = 0;
                                foreach (var item in objActivitiesList)
                                {
                                    newSeqCount++;
                                    item.Sequence = newSeqCount;
                                }
                            }
                            db.SaveChanges();
                            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(IpAddress))
                            {
                                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                            }
                            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                            int UserID = ((UserModel)Session["UserData"]).UserId;
                            int k = Functions.SaveUserLog(pkgId, "Strip Package Activity", "Delete", UserID, IpAddress, "NA");
                            
                        }
                    }
                }
                return Json("1");
            }
            catch(Exception ex)
            {
                return Json("-1");
            }
        }
        #endregion ---- Delete Records ----        

        #region ---- GetNext Seq number ----
        /// <summary>
        /// Gets the next seq number.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public JsonResult Get_NextSeqNumber(int? id,int? dispId)
        {

            try
            {
                if (id == null)
                {
                    return Json("- Please select Activity", JsonRequestBehavior.AllowGet);
                }
                using (var db = new dbRVNLMISEntities())
                {
                    if (dispId==null)
                    {
                        return Json("", JsonRequestBehavior.AllowGet);
                    }
                    var _firstSeq = (db.tblStripPkgActs.Where(o => o.PackageId == id && o.DispId==dispId && o.isDeleted==false).OrderByDescending(o => o.Sequence).FirstOrDefault().Sequence);
                    if (_firstSeq==null)
                    {
                        return Json("1", JsonRequestBehavior.AllowGet);
                    }
                    return Json(((int)_firstSeq+1), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("- Exception occured while validating activity sequense", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }

    public class ConstActModel
    {
        public int ConsActID { get; set; }
        public string ActivityName { get; set; }
    }
}