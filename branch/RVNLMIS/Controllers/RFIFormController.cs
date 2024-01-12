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
  //  [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class RFIFormController : Controller
    {
        // GET: RFIForm
        [PageAccessFilter]
        public ActionResult Index()
        {
            RFIModel model = new RFIModel();
            try
            {
                string RoleValue = string.Empty;
                string Role = string.Empty;
                string tableDataName = ((UserModel)Session["UserData"]).TableDataName;
                string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                Hashtable values = new Hashtable();
                values = RowLevelSecurity.getUsernameAndRole(tableDataName, roleCode);
                BindDropDownList();
                TempData["RFICode"] = GenerateCode(); // For generate RFI Code
                TempData["RoleValue"] = RoleValue.TrimStart();
                Role = Convert.ToString(values["Role"]);
                TempData["Role"] = Role.TrimStart();
                model.SubDate = DateTime.Today;

                //string dt = DateTime.Now.ToString("dd-MM-yyyy");
                //model.DueDate = Convert.ToDateTime(dt);
            }

            catch (Exception ex)
            {
                // return Json("1", JsonRequestBehavior.AllowGet);
            }

            return View();

        }

        #region -- Bind Dropdown list --

        public void BindDropDownList()
        {
            string PKGCode = "";
            UserModel objU = new UserModel();
            objU = (UserModel)Session["UserData"];
            PKGCode = objU.TableDataCode.ToString();

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var _BOQList = (from w in dbContext.tblBOQMasters select new drpBOQGroup { BoqID = w.BoqID, BoqCode = w.BoqCode }).ToList();

                    var _EntityList = (from e in dbContext.tblMasterEntities
                                       join p in dbContext.tblPackages on e.PackageId equals p.PackageId
                                       where (p.PackageCode == PKGCode && e.IsDelete == false)
                                       select new drpEntityModel
                                       {
                                           EntityID = e.EntityID,
                                           EntityName = e.EntityCode + " - " + e.EntityName
                                       }).ToList();
                    List<Item> items = new List<Item>();
                    items = GetLayorNoList();

                    var _WorkGroup = (from w in dbContext.tblWorkGroups select new drpWorGroup { WorkGroupID = w.WorkGrId, WorkGroupName = w.WorkGrName }).ToList();
                    var _WorkSite = (from w in dbContext.tblWorkSides where (w.IsDeleted == false) select new drpWorkSide { WorksideID = w.WorkSideId, WorkSideName = w.WorkSideName }).ToList();
                    var _Activity = (from a in dbContext.tblConsActivities where (a.IsDelete == false) select new drpActivityGroup { ActivityID = a.ConsActId, ActivityName = a.ActivityName }).ToList();

                    ViewBag.BOQ_RefID = new SelectList(_BOQList, "BoqID", "BoqCode");
                    ViewBag.WorkGroup = new SelectList(_WorkGroup, "WorkGroupID", "WorkGroupName");
                    ViewBag.WorkSite = new SelectList(_WorkSite, "WorksideID", "WorkSideName");
                    ViewBag.LayorNo = new SelectList(items, "Value", "Text");
                    ViewBag.Activity = new SelectList(_Activity, "ActivityID", "ActivityName");
                    ViewBag.Entity = new SelectList(_EntityList, "EntityID", "EntityName");
                }
            }
            catch (Exception ex)
            {
               // return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BindEntityDrpValues()
        {
            string PKGCode = "";
            UserModel objU = new UserModel();
            objU = (UserModel)Session["UserData"];
            PKGCode = objU.TableDataCode.ToString();
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var _EntityList = (from e in dbContext.tblMasterEntities
                                       join p in dbContext.tblPackages on e.PackageId equals p.PackageId
                                       where (p.PackageCode == PKGCode && e.IsDelete == false)
                                       select new drpEntityModel
                                       {
                                           EntityID = e.EntityID,
                                           EntityName = e.EntityCode + " - " + e.EntityName
                                       }).ToList();
                    return Json(_EntityList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult BindWorkGroupDrpValues()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var _WorkGroup = (from w in dbContext.tblWorkGroups select new drpWorGroup { WorkGroupID = w.WorkGrId, WorkGroupName = w.WorkGrName }).ToList();
                    return Json(_WorkGroup, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult BindWorksideDrpValues()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var _WorkSite = (from w in dbContext.tblWorkSides where (w.IsDeleted == false) select new drpWorkSide { WorksideID = w.WorkSideId, WorkSideName = w.WorkSideName }).ToList();
                    return Json(_WorkSite, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult BindLayerDrpValues()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    List<Item> items = new List<Item>();
                    items = GetLayorNoList();
                    return Json(items, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public List<Item> GetLayorNoList()
        {
            List<Item> items = new List<Item>();
            items.Add(new Item() { Text = "Top", Value = "0" });
            items.Add(new Item() { Text = "1", Value = "1" });
            items.Add(new Item() { Text = "2", Value = "2" });
            items.Add(new Item() { Text = "3", Value = "3" });
            items.Add(new Item() { Text = "4", Value = "4" });
            items.Add(new Item() { Text = "5", Value = "5" });
            items.Add(new Item() { Text = "6", Value = "6" });
            items.Add(new Item() { Text = "7", Value = "7" });
            items.Add(new Item() { Text = "8", Value = "8" });
            items.Add(new Item() { Text = "9", Value = "9" });
            items.Add(new Item() { Text = "10", Value = "10" });
            items.Add(new Item() { Text = "11", Value = "11" });
            items.Add(new Item() { Text = "12", Value = "12" });
            items.Add(new Item() { Text = "13", Value = "13" });
            items.Add(new Item() { Text = "14", Value = "14" });
            items.Add(new Item() { Text = "15", Value = "14" });
            items.Add(new Item() { Text = "16", Value = "14" });
            items.Add(new Item() { Text = "17", Value = "14" });
            items.Add(new Item() { Text = "18", Value = "14" });
            items.Add(new Item() { Text = "19", Value = "14" });
            items.Add(new Item() { Text = "20", Value = "14" });
            items.Add(new Item() { Text = "21", Value = "14" });
            items.Add(new Item() { Text = "22", Value = "14" });
            items.Add(new Item() { Text = "23", Value = "14" });
            items.Add(new Item() { Text = "24", Value = "14" });
            items.Add(new Item() { Text = "25", Value = "14" });
            items.Add(new Item() { Text = "16", Value = "14" });
            items.Add(new Item() { Text = "27", Value = "14" });
            items.Add(new Item() { Text = "28", Value = "14" });
            items.Add(new Item() { Text = "29", Value = "14" });
            items.Add(new Item() { Text = "30", Value = "14" });
            items.Add(new Item() { Text = "31", Value = "14" });
            items.Add(new Item() { Text = "32", Value = "14" });
            items.Add(new Item() { Text = "33", Value = "14" });
            items.Add(new Item() { Text = "34", Value = "14" });
            items.Add(new Item() { Text = "35", Value = "14" });
            items.Add(new Item() { Text = "36", Value = "14" });
            items.Add(new Item() { Text = "37", Value = "14" });
            items.Add(new Item() { Text = "38", Value = "14" });
            items.Add(new Item() { Text = "39", Value = "14" });
            items.Add(new Item() { Text = "40", Value = "14" });
            items.Add(new Item() { Text = "14", Value = "14" });
            items.Add(new Item() { Text = "14", Value = "14" });
            items.Add(new Item() { Text = "14", Value = "14" });
            items.Add(new Item() { Text = "14", Value = "14" });
            items.Add(new Item() { Text = "14", Value = "14" });
            items.Add(new Item() { Text = "14", Value = "14" });
            return items;
        }
        public class Item
        {
            public Item() { }

            public string Value { set; get; }
            public string Text { set; get; }
        }
        public JsonResult BindActivityDrpValues()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var _Activity = (from a in dbContext.tblConsActivities where (a.IsDelete == false) select new drpActivityGroup { ActivityID = a.ConsActId, ActivityName = a.ActivityName }).ToList();
                    return Json(_Activity, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class drpActivityGroup
        {
            public int ActivityID { get; set; }
            public string ActivityName { get; set; }
        }
        public class EntityDropdownOptions
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
        #endregion

        #region --- List RFI Values ---
        public ActionResult rfi_details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbcontext = new dbRVNLMISEntities())
            {
                var lst = (from r in dbcontext.tblRFIs.Where(s => s.IsDeleted != true)
                           join w in dbcontext.tblWorkGroups on r.WorkGroupID equals w.WorkGrId
                           join a in dbcontext.tblConsActivities on r.ActivityID equals a.ConsActId
                           join s in dbcontext.tblWorkSides on r.WorksideID equals s.WorkSideId

                           select new { r, w, a, s }).AsEnumerable().Select(s => new RFIModel
                           {
                               ID = s.r.ID,
                               SubDate = Convert.ToDateTime(s.r.SubDate),
                               WorkGroup = s.w.WorkGrName,
                               RFIID = s.r.RFIID,
                               Activity = s.a.ActivityName,
                               Workside = s.s.WorkSideName,
                               LayerNo = s.r.LayerNo,                              
                           }).ToList();


                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
                   


        #region -- Add RFI Details --
        [HttpPost]
        public ActionResult AddRFIDetails(RFIModel oModel, FormCollection fc)
        {
            try
            {
                
                UserModel objU = new UserModel();
                objU = (UserModel)Session["UserData"];
                string message = string.Empty;

                if (oModel.StartChainage == null && oModel.EndChainage == null && oModel.OtherLocation == null && oModel.EntityID != 0)    // Entity Selected
                {
                    oModel.Location = 2;
                    oModel.StartChainage = "0";
                    oModel.EndChainage = "0";
                    oModel.OtherLocation = " ";
                }
                else if (oModel.StartChainage == null && oModel.EndChainage == null && oModel.OtherLocation != null && oModel.EntityID == 0)    // other Location
                {
                    oModel.Location = 3;
                    oModel.StartChainage = "0";
                    oModel.EndChainage = "0";
                    oModel.EntityID = 0;
                }
                else
                {
                    oModel.Location = 1;
                    oModel.OtherLocation = " ";
                    oModel.EntityID = 0;
                }
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.ID == 0)
                        {
                            var exist = db.tblRFIs.Where(u => u.WorkGroupID == oModel.WorkGroupID && u.ActivityID == oModel.ActivityID).ToList();
                            if (exist.Count != 0)
                            {
                                message = "already exists";
                            }

                            else
                            {
                                oModel.selectedBOQIDs = fc["ddlBOQ"]; // for selected BOQ ID's

                                tblRFI objRFI = new tblRFI();
                                objRFI.ID = oModel.ID;
                                objRFI.RFIID = oModel.RFIID;
                                objRFI.SubDate = oModel.SubDate;
                                objRFI.WorkGroupID = oModel.WorkGroupID;
                                objRFI.RFIID = oModel.RFIID;
                                objRFI.LayerNo = oModel.LayerNo;
                                objRFI.ActivityID = oModel.ActivityID;
                                objRFI.Location = oModel.Location;
                                objRFI.StartChainage = oModel.StartChainage;
                                objRFI.EndChainage = oModel.EndChainage;
                                objRFI.EntityID = oModel.EntityID;
                                objRFI.OtherLocation = oModel.OtherLocation;
                                objRFI.WorksideID = oModel.WorksideID;
                                objRFI.Remark = oModel.Remark;
                                objRFI.CreatedOn = DateTime.Now;
                                objRFI.IsDeleted = false;



                                db.tblRFIs.Add(objRFI);
                                db.SaveChanges();
                                int lastInsertedId = db.tblRFIs.Max(item => item.ID);
                                TempData["RFICode"] = GenerateCode();

                                try
                                {
                                    db.RFIBOQInsert(lastInsertedId, oModel.selectedBOQIDs, objU.UserId);
                                }
                                catch (Exception ex)
                                {

                                }
                                message = "Added Successfully";
                            }

                        }
                        else
                        {
                            var codexist = db.tblRFIs.Where(u => (u.RFIID == oModel.RFIID && u.WorkGroupID == oModel.WorkGroupID && u.ActivityID == oModel.ActivityID) && (u.ID != oModel.ID)).ToList();
                            if (codexist.Count != 0)
                            {
                                message = "RFI record already exists";
                            }
                            else
                            {
                                tblRFI objRFI = db.tblRFIs.Where(u => u.ID == oModel.ID).SingleOrDefault();
                                objRFI.RFIID = oModel.RFIID;
                                objRFI.SubDate = oModel.SubDate;
                                objRFI.WorkGroupID = oModel.WorkGroupID;
                                objRFI.RFIID = oModel.RFIID;
                                objRFI.LayerNo = oModel.LayerNo;
                                objRFI.ActivityID = oModel.ActivityID;
                                objRFI.Location = oModel.Location;
                                objRFI.StartChainage = oModel.StartChainage;
                                objRFI.EndChainage = oModel.EndChainage;
                                objRFI.EntityID = oModel.EntityID;
                                objRFI.OtherLocation = oModel.OtherLocation;
                                objRFI.WorksideID = oModel.WorksideID;
                                objRFI.Remark = oModel.Remark;
                                objRFI.CreatedOn = DateTime.Now;
                                objRFI.IsDeleted = false;
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }
                        }

                    }
                    ModelState.Clear();

                    return Json(message, JsonRequestBehavior.AllowGet);
                    //BindDropDownList();
                    //add the data to the model

                    //return View("_PreviewPartialRFI", oModel); //return the model with the view 

                }
                else
                {
                    ModelState.Clear();
                    return View("_AddEditRFIPartialView", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- EDIT RFI Details --
        public ActionResult EditbyID(int ID)
        {
            BindDropDownList();          
            RFIModel objModel = new RFIModel();
            try
            {
                string MenuArray = string.Empty;
                if (ID != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oRFIDetails = db.tblRFIs.Where(o => o.ID == ID).SingleOrDefault();
                        if (oRFIDetails != null)
                        {
                            objModel.ID = oRFIDetails.ID;
                            objModel.SubDate = Convert.ToDateTime( oRFIDetails.SubDate);
                            ViewBag.SubDate = objModel.SubDate;
                            objModel.WorkGroupID = Convert.ToInt32( oRFIDetails.WorkGroupID);
                            objModel.RFIID = oRFIDetails.RFIID;
                            objModel.LayerNo = oRFIDetails.LayerNo;
                            objModel.Location = Convert.ToInt32(oRFIDetails.Location);
                            objModel.StartChainage = oRFIDetails.StartChainage;
                            objModel.EndChainage = oRFIDetails.EndChainage;
                            objModel.EntityID = Convert.ToInt32(oRFIDetails.EntityID);
                            objModel.OtherLocation = oRFIDetails.OtherLocation;
                            objModel.WorksideID = Convert.ToInt32(oRFIDetails.WorksideID);
                            objModel.ActivityID= Convert.ToInt32(oRFIDetails.ActivityID);
                            objModel.Remark = oRFIDetails.Remark;
                          //  MenuArray = string.Join(",", db.tblRFIBOQs.Where(x => x.RFIID == ID).Select(a => a.BoqID.ToString()).ToArray());
                            objModel.selectedBOQIDs = MenuArray;
                            ///ViewBag.SeletedBOQs = MenuArray;
                            objModel.IsDeleted = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditRFIPartialView", objModel);
        }
        #endregion

        #region -- Delete RFI Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblRFI obj = db.tblRFIs.SingleOrDefault(o => o.ID == id);
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

        #region Generate RFI CODE
        public string GenerateCode()
        {
            string NewStr = string.Empty;
            int CodeNo = 0;
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var lastRFICode = db.tblRFIs.OrderByDescending(o => o.RFIID).FirstOrDefault();
                    if (lastRFICode == null)
                    {
                        NewStr = "RFI001";
                    }
                    else
                    {
                        string abc = lastRFICode.RFIID.ToString();
                        NewStr = abc.Remove(0, 3);
                        CodeNo = Convert.ToInt32(NewStr);
                        if (CodeNo > 0 && CodeNo < 10)
                        {
                            NewStr = "RFI" + "00" + Convert.ToString(CodeNo + 1);
                        }
                        else if (CodeNo > 10 && CodeNo < 99)
                        {
                            NewStr = "RFI" + "0" + Convert.ToString(CodeNo + 1);
                        }
                        else if (CodeNo >= 99 && CodeNo < 1000)
                        {
                            NewStr = "RFI" + Convert.ToString(CodeNo + 1);
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


        public ActionResult LoadPreviewForm(int ID)
        {
            BindDropDownList();
            RFIModel objModel = new RFIModel();
            try
            {
                string MenuArray = string.Empty;
                if (ID != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oRFIDetails = db.tblRFIs.Where(o => o.ID == ID).SingleOrDefault();
                        if (oRFIDetails != null)
                        {
                            objModel.ID = oRFIDetails.ID;
                            objModel.SubDate = Convert.ToDateTime(oRFIDetails.SubDate);
                            ViewBag.SubDate = objModel.SubDate;
                            objModel.WorkGroupID = Convert.ToInt32(oRFIDetails.WorkGroupID);
                            objModel.RFIID = oRFIDetails.RFIID;
                            objModel.LayerNo = oRFIDetails.LayerNo;
                            objModel.Location = Convert.ToInt32(oRFIDetails.Location);
                            objModel.StartChainage = oRFIDetails.StartChainage;
                            objModel.EndChainage = oRFIDetails.EndChainage;
                            objModel.EntityID = Convert.ToInt32(oRFIDetails.EntityID);
                            objModel.OtherLocation = oRFIDetails.OtherLocation;
                            objModel.WorksideID = Convert.ToInt32(oRFIDetails.WorksideID);
                            objModel.ActivityID = Convert.ToInt32(oRFIDetails.ActivityID);
                            objModel.Remark = oRFIDetails.Remark;
                           // MenuArray = string.Join(",", db.tblRFIBOQs.Where(x => x.RFIID == ID).Select(a => a.BoqID.ToString()).ToArray());
                            objModel.selectedBOQIDs = MenuArray;
                            ///ViewBag.SeletedBOQs = MenuArray;
                            objModel.IsDeleted = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PreviewPartialRFI", objModel);
        }

             
        [HttpPost]
        // public ActionResult FillPreviewForm(string SubDate, int WorkGroupID, string RFIID, int ActivityID, string LayorNo, int WorksideID, string Remark)

        public ActionResult FillPreviewForm(RFIModel oModel, FormCollection fc)
        {
            //RFIModel oModel = new RFIModel();
            //oModel.SubDate = Convert.ToDateTime( SubDate);
            //oModel.WorkGroupID = WorkGroupID;
            //oModel.RFIID = RFIID;
            //oModel.ActivityID = ActivityID;


            //oModel.selectedBOQIDs =  fc["ddlBOQ"]; // for selected BOQ ID's
            BindDropDownList();
            //add the data to the model

            return View("_PreviewPartialRFI", oModel); //return the model with the view 
        }

    }

}