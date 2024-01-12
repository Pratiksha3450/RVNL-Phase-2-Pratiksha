using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class ManageActivitiesController : Controller
    {
        public string IpAddress = "";
        // GET: ManageActivities
        [PageAccessFilter]
        public ActionResult Index()
        {
            BindDropdown();
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Manage Activity", "View", UserID, IpAddress, "NA");
            return View();
        }
        #region -- Bind Dropdown list --
        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var ActivityGroupsList = dbContext.tblActivityGroups.Where(o => o.IsDeleted == false).ToList();
                    ViewBag.ActivityGroupsList = new SelectList(ActivityGroupsList, "ActGId", "ActivityGroupName");
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region --- List ConsActivity Values ---
        public ActionResult ConsActivity_Details([DataSourceRequest]  DataSourceRequest request, int? actID)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = dbContext.GetConsActivity().Select(e => new Models.ConsActivity
                {
                    ActGId = (int)e.ActGId,
                    ActivityName = e.ActivityName,
                    ConsActId = e.ConsActId,
                    ActivityCode = e.ActivityCode,
                    ActUnit = e.ActUnit,
                    ActWtg = (float)e.ActWtg,
                    ActivityGroup = e.ActivityGroup,
                    StripChart=e.StripChart,
                    RFI=e.RFI
                   
                   

                }).ToList();
                if (actID != null)
                {
                    var lst1 = lst.Where(w => w.ActGId == actID).ToList();
                    return Json(lst1.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Add ConsActivity Details --
        [HttpPost]
        public ActionResult AddConsActivityDetails(ConsActivity oModel)
        {
            try
            {
                oModel.IsDelete = false;
                oModel.RFI = "";
                oModel.StripChart = "";
                string message = string.Empty;
                //if (ModelState.IsValid==false)
                //{
                //    ModelState.Clear();
                //    return View("_PartialAddEditConsActivity", oModel);

                //}
                using (var db = new dbRVNLMISEntities())
                {
                    var isNameEnteredExist = db.tblConsActivities.Where(u => u.ActivityName == oModel.ActivityName && u.IsDelete == false).FirstOrDefault();

                    if (oModel.ConsActId == 0)
                    {
                        var exist = db.tblConsActivities.Where(u => u.ActivityCode == oModel.ActivityCode && u.IsDelete == false).ToList();
                        if (exist.Count != 0)
                        {
                            message = "3";
                        }
                        else if ((db.tblConsActivities.Where(u => u.ActivityName == oModel.ActivityName && u.IsDelete == false).ToList().Count != 0))
                        {
                            message = "2";
                        }
                        else
                        {
                            tblConsActivity objConsActivity = new tblConsActivity();
                            objConsActivity.ActGId = oModel.ActGId;
                            objConsActivity.ActivityGroup = db.tblActivityGroups.Where(o => o.ActGId == oModel.ActGId && o.IsDeleted == false).FirstOrDefault().ActivityGroupName;
                            objConsActivity.ActivityCode = oModel.ActivityCode;
                            objConsActivity.ActivityName = oModel.ActivityName;
                            objConsActivity.ActWtg = oModel.ActWtg;
                            objConsActivity.ActUnit = oModel.ActUnit;
                            objConsActivity.IsDelete = false;
                            objConsActivity.isStripChart = oModel.isStripChart;
                            objConsActivity.IsRFI = oModel.IsRFI;
                            db.tblConsActivities.Add(objConsActivity);
                            db.SaveChanges();
                            message = "Added Successfully";
                            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(IpAddress))
                            {
                                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                            }
                            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                            int UserID = ((UserModel)Session["UserData"]).UserId;
                            int k = Functions.SaveUserLog(pkgId, "Manage Activity", "Save", UserID, IpAddress,"Actvity Name:"+ oModel.ActivityName);
                        }

                    }
                    else
                    {
                        var objConsActivity = db.tblConsActivities.Where(u => u.ConsActId == oModel.ConsActId).SingleOrDefault();/*db.tblconsactivities.where(u => (u.activitycode != omodel.activitycode) && (u.actgid != omodel.actgid)).tolist();*/
                        if (objConsActivity != null)
                        {
                            if (objConsActivity.ActivityCode != oModel.ActivityCode)
                            {
                                message = "ConsActivity Code already exists";
                            }
                            if (objConsActivity.ActivityName != oModel.ActivityName)
                            {
                                if (isNameEnteredExist != null)
                                {
                                    //message = "ConsActivity Name already exists";
                                    return Json("2", JsonRequestBehavior.AllowGet);
                                }
                            }

                            objConsActivity.ActGId = oModel.ActGId;
                            objConsActivity.ActivityGroup = db.tblActivityGroups.Where(o => o.ActGId == oModel.ActGId && o.IsDeleted == false).FirstOrDefault().ActivityGroupName;
                            objConsActivity.ActivityCode = oModel.ActivityCode;
                            objConsActivity.ActivityName = oModel.ActivityName;
                            objConsActivity.ActWtg = oModel.ActWtg;
                            objConsActivity.ActUnit = oModel.ActUnit;
                            objConsActivity.isStripChart = oModel.isStripChart;
                            objConsActivity.IsRFI = oModel.IsRFI;
                            db.SaveChanges();
                            message = "Updated Successfully";
                            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(IpAddress))
                            {
                                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                            }
                            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                            int UserID = ((UserModel)Session["UserData"]).UserId;
                            int k = Functions.SaveUserLog(pkgId, "Manage Activity", "Update", UserID, IpAddress, "Activity Name:" + oModel.ActivityName);

                        }
                    }

                }
                ModelState.Clear();
                // return View("Index", oModel);
                return Json(message, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- EDIT ConsActivity Details --
        public ActionResult EditbyConsActivityID(int id)
        {
            BindDropdown();
            int ConsActivityId = id;
            ConsActivity objModel = new ConsActivity();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oConsActivityDetails = db.tblConsActivities.Where(o => o.ConsActId == id).SingleOrDefault();
                        if (oConsActivityDetails != null)
                        {
                            objModel.ActGId = (int)oConsActivityDetails.ActGId;
                            objModel.ConsActId = (int)oConsActivityDetails.ConsActId;
                            objModel.ActivityCode = oConsActivityDetails.ActivityCode;
                            objModel.ActivityName = oConsActivityDetails.ActivityName;
                            objModel.ActUnit = oConsActivityDetails.ActUnit;
                            objModel.ActWtg = oConsActivityDetails.ActWtg == null ? 00 : (float)oConsActivityDetails.ActWtg;
                            objModel.isStripChart = oConsActivityDetails.isStripChart==null? false : (bool)oConsActivityDetails.isStripChart;
                            objModel.IsRFI = oConsActivityDetails.IsRFI == null ? false : (bool)oConsActivityDetails.IsRFI;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialAddEditConsActivity", objModel);
        }
        #endregion

        #region -- Delete ED Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var obj = db.tblConsActivities.SingleOrDefault(o => o.ConsActId == id);
                    obj.IsDelete = true;
                    db.SaveChanges();
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    int k = Functions.SaveUserLog(pkgId, "Manage Activity", "Delete", UserID, IpAddress, "Activity Name:"+ obj.ActivityName);
                }
                return Json("1");
            }
            catch (Exception ex)
            {
                return Json("-1");
            }
        }
        #endregion

        public JsonResult ActivityCode(int id)
        {
            try
            {
                string AutoCode = string.Empty;
                int ConsActivities = 0;
                try
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        //var package = db.tblDisciplines.Where(o => o.DispId == id && o.IsDeleted == false).Select(o => o.DispName).FirstOrDefault();
                        var ActivityGroup = db.tblActivityGroups.Where(o => o.ActGId == id && o.IsDeleted == false).Select(o => o.ActivityGroupName).FirstOrDefault();
                        string result = string.Concat(Regex.Matches(ActivityGroup.ToString(), "[A-Z]").OfType<Match>().Select(match => match.Value));
                        var numberString = db.tblConsActivities.Where(o => o.ActGId == id && o.IsDelete == false).OrderByDescending(o => o.ConsActId).Select(o => o.ActivityCode).FirstOrDefault();
                        string resultNum = new String(numberString.Where(x => Char.IsDigit(x)).ToArray());
                        ConsActivities = Functions.ParseInteger(resultNum);          // db.tblConsActivities.Where(o => o.IsDelete == false && o.ActGId == id).Count();
                        if (ConsActivities != 0)
                        {
                            AutoCode = result + (ConsActivities + 10);
                        }
                        else
                        {
                            AutoCode = result + "10010";
                        }
                        return Json(AutoCode, new JsonRequestBehavior());
                    }
                }
                catch (Exception ex)
                {

                    return Json("0");
                }


            }
            catch (Exception)
            {

                return Json("0");
            }
        }
    }
}