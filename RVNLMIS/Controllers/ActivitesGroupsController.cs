using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class ActivitesGroupsController : Controller
    {
        public string IpAddress = "";      
        [PageAccessFilter]
        public ActionResult Index()
        {
          
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Activity Group", "View", UserID, IpAddress, "View");
            return View();
        }
        #region --- List PMCReport Type Values ---
        public ActionResult ActivityGroup_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = (from x in dbContext.tblActivityGroups
                           where x.IsDeleted == false
                           select new { x })
                            .AsEnumerable().Select(s =>
                               new ActivityGroup
                               {
                                   ActGId = s.x.ActGId,
                                   ActivityGroupName = s.x.ActivityGroupName,
                                   //CreatedOn = s.x.CreatedOn
                                   IsDeleted = s.x.IsDeleted

                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit PMCReport Type Details --
        public ActionResult EditActivityGroupByPRId(int id)
        {
            int entiytTypeId = id;
            ActivityGroup objModel = new ActivityGroup();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oActivityGroupDetails = db.tblActivityGroups.Where(o => o.ActGId == id).SingleOrDefault();
                        if (oActivityGroupDetails != null)
                        {
                            objModel.ActGId = oActivityGroupDetails.ActGId;
                            objModel.ActivityGroupName = oActivityGroupDetails.ActivityGroupName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditActivityGroup", objModel);
        }
        #endregion

        #region -- Add and Update PMC Report Types Details --
        [HttpPost]
        public ActionResult AddActivityGroup(ActivityGroup oModel)
        {
            //int ActivityGroupId = oModel.ActGId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblActivityGroups.Where(u => (u.ActivityGroupName == oModel.ActivityGroupName) && u.IsDeleted == false).FirstOrDefault();

                        if (oModel.ActGId == 0)
                        {
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblActivityGroup objActivityGroup = new tblActivityGroup();
                                //objDiscipline.DispId = oModel.DisciplineId;
                                objActivityGroup.ActivityGroupName = oModel.ActivityGroupName;
                                objActivityGroup.IsDeleted = false;
                                db.tblActivityGroups.Add(objActivityGroup);
                                db.SaveChanges();
                                message = "Added Successfully";
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                int k = Functions.SaveUserLog(pkgId, "Activity Group", "Save", UserID, IpAddress, "Act Group Name:" + oModel.ActivityGroupName);
                            }
                        }
                        else
                        {

                            var exist1 = db.tblActivityGroups.Where(u => u.ActivityGroupName == oModel.ActivityGroupName && u.IsDeleted == false && u.ActGId != oModel.ActGId).ToList();
                            if (exist1.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblActivityGroup objActivityGroup = db.tblActivityGroups.Where(o => o.ActGId == oModel.ActGId).SingleOrDefault();
                                objActivityGroup.ActivityGroupName = oModel.ActivityGroupName;
                                objActivityGroup.IsDeleted = false;
                                db.SaveChanges();
                                message = "Updated Successfully";
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                try
                                {
                                    string str = ""; ;
                                    var UpdatedValue = (from ul in db.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                                    if (UpdatedValue != null)
                                    {
                                        str = UpdatedValue;
                                        str = "Act Group Name: " + oModel.ActivityGroupName + "= " + str;
                                    }
                                    else
                                    {
                                        str = "NA";
                                    }

                                    int k = Functions.SaveUserLog(pkgId, "Activity Group", "Update", UserID, IpAddress, str);
                                }
                                catch (Exception ex)
                                {

                                }
                            }


                        }
                        ModelState.Clear();
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ModelState.Clear();
                    return View("_PartialEditActivityGroup", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- Delete PMC Report Types Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblActivityGroup objActivityGroup = db.tblActivityGroups.SingleOrDefault(o => o.ActGId == id);
                    objActivityGroup.IsDeleted = true;
                    db.SaveChanges();
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;                  
                    int k = Functions.SaveUserLog(pkgId, "Activity Group", "Delete", UserID, IpAddress, "Act Group Name:" + objActivityGroup.ActivityGroupName);
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