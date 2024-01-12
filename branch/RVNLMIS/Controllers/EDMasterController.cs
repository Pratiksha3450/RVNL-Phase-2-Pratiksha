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
    public class EDMasterController : Controller
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
                TempData["EDCode"] = GenerateCode(); // For generate ED Code

            }
            catch (Exception e)
            {

            }
            return View();
        }

        #region --- List ED Values ---
        public ActionResult ED_Details([DataSourceRequest]  DataSourceRequest request, string EDName, string role)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<EDMasterModel> obj = new List<EDMasterModel>();

                obj = (from x in dbContext.tblMasterEDs.Where(s => s.IsDelete != true)
                       select new { x })
                                       .AsEnumerable().Select(s =>
                                          new EDMasterModel
                                          {
                                              EDId = s.x.EDId,
                                              EDCode = s.x.EDCode,
                                              EDName = s.x.EDName

                                          }).ToList();

                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region -- Add ED Details --
        [HttpPost]
        public ActionResult AddEDDetails(EDMasterModel oModel)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.EDId == 0)
                        {
                            var exist = db.tblMasterEDs.Where(u => u.EDCode == oModel.EDCode || u.EDName == oModel.EDName).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                string max = db.tblMasterEDs.OrderByDescending(p => p.EDId).FirstOrDefault().EDCode;
                                //if (!string.IsNullOrEmpty(max))
                                //{
                                //    oModel.EDCode = GenerateCode(max);
                                //}
                                tblMasterED objED = new tblMasterED();
                                objED.EDId = oModel.EDId;
                                objED.EDCode = oModel.EDCode;
                                objED.EDName = oModel.EDName;
                                objED.IsDelete = false;
                                db.tblMasterEDs.Add(objED);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                        else
                        {
                            var exist = db.tblMasterEDs.Where(u => (u.EDName == oModel.EDName) && (u.EDId != oModel.EDId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblMasterED objEDmodel = db.tblMasterEDs.Where(u => u.EDId == oModel.EDId).SingleOrDefault();
                                objEDmodel.EDCode = oModel.EDCode;
                                objEDmodel.EDName = oModel.EDName;
                                objEDmodel.IsDelete = false;
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }
                        }
                    }
                    ModelState.Clear();
                    var EDCODE = GenerateCode();
                    var result = new { message = message, Code = EDCODE };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_AddEditED", oModel);
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
                    var lastEDCode = db.tblMasterEDs.OrderByDescending(o => o.EDCode).FirstOrDefault();
                    if (lastEDCode == null)
                    {
                        NewStr = "ED001";
                    }
                    else
                    {
                        string abc = lastEDCode.EDCode.ToString();
                        NewStr = abc.Remove(0, 2);
                        CodeNo = Convert.ToInt32(NewStr);
                        if (CodeNo > 0 && CodeNo < 10)
                        {
                            NewStr = "ED" + "00" + Convert.ToString(CodeNo + 1);
                        }
                        else if (CodeNo > 10 && CodeNo < 99)
                        {
                            NewStr = "ED" + "0" + Convert.ToString(CodeNo + 1);
                        }
                        else if (CodeNo >= 99 && CodeNo < 1000)
                        {
                            NewStr = "ED" + Convert.ToString(CodeNo + 1);
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

        #region -- EDIT ED Details --
        public ActionResult EditEDByEDId(int id)
        {
            int EDId = id;
            EDMasterModel objModel = new EDMasterModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oEDDetails = db.tblMasterEDs.Where(o => o.EDId == id).SingleOrDefault();
                        if (oEDDetails != null)
                        {
                            objModel.EDId = oEDDetails.EDId;
                            objModel.EDCode = oEDDetails.EDCode;
                            objModel.EDName = oEDDetails.EDName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditED", objModel);
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
                    tblMasterED obj = db.tblMasterEDs.SingleOrDefault(o => o.EDId == id);
                    obj.IsDelete = true;
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