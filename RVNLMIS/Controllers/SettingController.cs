using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    public class SettingController : Controller
    {
        // GET: Setting
        public ActionResult Index()
        {
            BindDropdown();
            return View();
        }
        #region -- Bind Dropdown list --
        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var enumData = from DataTypeEnum.DataTypeList enumItem in Enum.GetValues(typeof(DataTypeEnum.DataTypeList))
                                   select new
                                   {
                                       Value = enumItem,
                                       Text = enumItem.ToString()
                                   };
                    ViewBag.EnumList = new SelectList(enumData, "Value", "Text");

                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region --- List SettingDetails Values ---
        public ActionResult Setting_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = dbContext.tblSettings.Where(o => o.IsDelete == false).ToList();
                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Add Setting Details --
        [HttpPost]
        public ActionResult AddSettingDetails(SettingModel oModel)
        {
            try
            {
                string message = string.Empty;
                if (!ModelState.IsValid)
                {
                    BindDropdown();
                    return View("_PartialAddEditSetting", oModel);

                }
                using (var db = new dbRVNLMISEntities())
                {
                    if (oModel.SettingID == 0)
                    {
                        var exist = db.tblSettings.Where(u => u.SKey == oModel.SKey && u.IsDelete == false).ToList();
                        if (exist.Count != 0)
                        {
                            message = "3";
                        }
                        else
                        {
                            tblSetting objSetting = new tblSetting();
                            objSetting.DataType = oModel.DataType;
                            objSetting.Value = oModel.Value;
                            objSetting.SKey = oModel.SKey;
                            objSetting.CreateOn = DateTime.Now;
                            objSetting.IsDelete = false;
                            db.tblSettings.Add(objSetting);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }

                    }
                    else
                    {
                        var objSetting = db.tblSettings.Where(u => u.SettingID == oModel.SettingID).SingleOrDefault();/*db.tblconsactivities.where(u => (u.activitycode != omodel.activitycode) && (u.actgid != omodel.actgid)).tolist();*/
                        if (objSetting != null)
                        {
                            var objConsActivity = db.tblSettings.Where(u => u.SKey == oModel.SKey && u.IsDelete == false && u.SettingID != oModel.SettingID).ToList();/*db.tblconsactivities.where(u => (u.activitycode != omodel.activitycode) && (u.actgid != omodel.actgid)).tolist();*/
                            if (objConsActivity.Count != 0)
                            {
                                //message = "ConsActivity Name already exists";
                                return Json("2", JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                objSetting.DataType = oModel.DataType;
                                objSetting.Value = oModel.Value;
                                objSetting.SKey = oModel.SKey;
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }

                        }

                    }
                    ModelState.Clear();
                    // return View("Index", oModel);
                    return Json(message, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }

        }
        #endregion

        #region -- EDIT ConsActivity Details --
        public ActionResult EditbySettingID(int id)
        {
            BindDropdown();
            int SettingID = id;
            SettingModel objModel = new SettingModel();
            try
            {
                if (SettingID != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oSettingDetails = db.tblSettings.Where(o => o.SettingID == SettingID).SingleOrDefault();
                        if (oSettingDetails != null)
                        {
                            objModel.SettingID = (int)oSettingDetails.SettingID;
                            objModel.Value = oSettingDetails.Value;
                            objModel.SKey = oSettingDetails.SKey;
                            objModel.DataType = oSettingDetails.DataType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialAddEditSetting", objModel);
        }
        #endregion
    }
}