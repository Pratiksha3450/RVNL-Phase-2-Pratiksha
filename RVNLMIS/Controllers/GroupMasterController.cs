using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections;
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
    public class GroupMasterController : Controller
    {
        [PageAccessFilter]
        public ActionResult Index()
        {
            try
            {
                string RoleValue = string.Empty;
                string Role = string.Empty;
                string tableDataName = ((UserModel)Session["UserData"]).TableDataName;
                string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                Hashtable values = new Hashtable();
                values = RowLevelSecurity.getUsernameAndRole(tableDataName, roleCode);
                RoleValue = Convert.ToString(values["Username"]);
                TempData["RoleValue"] = RoleValue.TrimStart();

                Role = Convert.ToString(values["Role"]);
                TempData["Role"] = Role.TrimStart();
                TempData["GroupCode"] = GenerateCode(); // For generate ED Code

            }
            catch (Exception e)
            {

            }
            return View();
        }

        #region --- List ED Values ---
        public ActionResult Group_Details([DataSourceRequest]  DataSourceRequest request, string GroupName, string role)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<GroupMasterModel> obj = new List<GroupMasterModel>();

                obj = (from x in dbContext.tblMasterGroups.Where(s => s.IsDeleted != true)
                       select new { x })
                                       .AsEnumerable().Select(s =>
                                          new GroupMasterModel
                                          {
                                              GroupId = s.x.GroupId,
                                              GroupCode = s.x.GroupCode,
                                              GroupName = s.x.GroupName

                                          }).ToList();

                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region -- Add ED Details --
        [HttpPost]
        public ActionResult AddGroupDetails(GroupMasterModel oModel)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.GroupId == 0)
                        {
                            var exist = db.tblMasterGroups.Where(u => (u.GroupCode == oModel.GroupCode || u.GroupName == oModel.GroupName)&& u.IsDeleted==false).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                //string max = db.tblMasterGroups.OrderByDescending(p => p.GroupId).FirstOrDefault().GroupCode;
                                //if (!string.IsNullOrEmpty(max))
                                //{
                                //    oModel.GroupCode = GenerateCode(max);
                                //}
                                tblMasterGroup objED = new tblMasterGroup();
                                objED.GroupId = oModel.GroupId;
                                objED.GroupCode = oModel.GroupCode;
                                objED.GroupName = oModel.GroupName;
                                objED.IsDeleted = false;
                                db.tblMasterGroups.Add(objED);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                        else
                        {
                            var exist = db.tblMasterGroups.Where(u => (u.GroupName == oModel.GroupName) && (u.GroupId != oModel.GroupId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblMasterGroup objGroupModel = db.tblMasterGroups.Where(u => u.GroupId == oModel.GroupId).SingleOrDefault();
                                objGroupModel.GroupCode = oModel.GroupCode;
                                objGroupModel.GroupName = oModel.GroupName;
                                objGroupModel.IsDeleted = false;
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }
                        }
                    }
                    ModelState.Clear();
                    var GroupCode = GenerateCode();
                    var result = new { message = message, Code = GroupCode };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_AddEditGroup", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- Generate Code --
        public string GenerateCode()
        {
            string NewStr = string.Empty;
            int CodeNo = 0;
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var lastGroupCode = db.tblMasterGroups.OrderByDescending(o => o.GroupCode).FirstOrDefault();
                    if (lastGroupCode == null)
                    {
                        NewStr = "Group001";
                    }
                    else
                    {
                        string abc = lastGroupCode.GroupCode.ToString();
                        NewStr = abc.Remove(0, 5);
                        CodeNo = Convert.ToInt32(NewStr);
                        if (CodeNo > 0 && CodeNo < 10)
                        {
                            NewStr = "Group" + "00" + Convert.ToString(CodeNo + 1);
                        }
                        else if (CodeNo > 10 && CodeNo < 99)
                        {
                            NewStr = "Group" + "0" + Convert.ToString(CodeNo + 1);
                        }
                        else if (CodeNo >= 99 && CodeNo < 1000)
                        {
                            NewStr = "Group" + Convert.ToString(CodeNo + 1);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return NewStr;
        }
        #endregion

        #region -- EDIT Group Details --
        public ActionResult EditGroupByGroupId(int id)
        {
            int GroupId = id;
            GroupMasterModel objModel = new GroupMasterModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oGroupDetails = db.tblMasterGroups.Where(o => o.GroupId == id).SingleOrDefault();
                        if (oGroupDetails != null)
                        {
                            objModel.GroupId = oGroupDetails.GroupId;
                            objModel.GroupCode = oGroupDetails.GroupCode;
                            objModel.GroupName = oGroupDetails.GroupName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditGroup", objModel);
        }
        #endregion

        #region -- Delete Group Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblMasterGroup obj = db.tblMasterGroups.SingleOrDefault(o => o.GroupId == id);
                    obj.IsDeleted = true;
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