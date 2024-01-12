using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Areas.RFI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [AreaSessionExpire]
    [HandleError]
    // [Authorize]
    //[Compress]
    //[SessionAuthorize]
    public class RFIBOQMasterController : Controller
    {
        // GET: RFIBOQMaster
        public ActionResult Index()
        {
            TempData["BOQCode"] = GenerateCode();
            return View();
        }

        #region --- List BOQ master Values ---
        public ActionResult BOQMaster_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<RFIBOQMasterModel> obj = new List<RFIBOQMasterModel>();

                obj = (from x in dbContext.tblBOQMasters
                       select new { x })
                                       .AsEnumerable().Select(s =>
                                          new RFIBOQMasterModel
                                          {
                                              BoqID = s.x.BoqID,
                                              BoqCode = s.x.BoqCode,
                                              BoqName = s.x.BoqName,
                                              BoqUnit = s.x.BoqUnit,
                                              BoqQty = s.x.BoqQty,
                                              BoqRate = s.x.BoqRate
                                          }).ToList();

                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region -- Add RFI BOQ Master Details --
        [HttpPost]
        public ActionResult SubmitRFIBOQ(RFIBOQMasterModel oModel)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.BoqID == 0)
                        {
                            var exist = db.tblBOQMasters.Where(u => u.BoqCode == oModel.BoqCode).ToList();
                            if (exist.Count != 0)
                            {
                                message = "1";
                            }
                            else if (db.tblBOQMasters.Where(u => u.BoqName == oModel.BoqName).ToList().Count() != 0)
                            {
                                message = "2";
                            }
                            else
                            {
                                tblBOQMaster objWG = new tblBOQMaster();
                                objWG.BoqCode = oModel.BoqCode;
                                objWG.BoqName = oModel.BoqName;
                                objWG.BoqQty = oModel.BoqQty;
                                objWG.BoqRate = oModel.BoqRate;
                                objWG.BoqUnit = oModel.BoqUnit;
                                db.tblBOQMasters.Add(objWG);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                        else
                        {
                            var exist = db.tblBOQMasters.Where(u => (u.BoqName == oModel.BoqName) && (u.BoqID != oModel.BoqID)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "2";
                            }
                            else
                            {
                                tblBOQMaster objGroupModel = db.tblBOQMasters.Where(u => u.BoqID == oModel.BoqID).SingleOrDefault();
                                objGroupModel.BoqName = oModel.BoqName;
                                objGroupModel.BoqQty = oModel.BoqQty;
                                objGroupModel.BoqRate = oModel.BoqRate;
                                objGroupModel.BoqUnit = oModel.BoqUnit;
                                db.SaveChanges();
                                message = "Updated Successfully";
                            }
                        }
                    }
                    var BOQCode = GenerateCode();
                    var result = new { message = message, Code = BOQCode };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View("_AddEditRFIBOQ", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- EDIT RFIBOQ Details --
        public ActionResult EditBOQDetails(int id)
        {
            RFIBOQMasterModel objModel = new RFIBOQMasterModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oBOQEdit = db.tblBOQMasters.Where(o => o.BoqID == id).SingleOrDefault();
                        if (oBOQEdit != null)
                        {
                            objModel.BoqID = oBOQEdit.BoqID;
                            objModel.BoqCode = oBOQEdit.BoqCode;
                            objModel.BoqName = oBOQEdit.BoqName;
                            objModel.BoqQty = oBOQEdit.BoqQty;
                            objModel.BoqRate = oBOQEdit.BoqRate;
                            objModel.BoqUnit = oBOQEdit.BoqUnit;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditRFIBOQ", objModel);
        }
        #endregion

        #region -- Delete Boq Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblBOQMaster obj = db.tblBOQMasters.SingleOrDefault(o => o.BoqID == id);
                    db.tblBOQMasters.Remove(obj);
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

        public string GenerateCode()
        {
            string ou = string.Empty;
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var lastBOQCode = db.GetNextPackageCode("tblBOQMaster").ToList();
                    if (lastBOQCode.Count() == 0)
                    {
                        ou = "BOQ001";
                    }
                    else
                    {
                        int s = Functions.ParseInteger(lastBOQCode[0].intNo.ToString()) + 1;
                        ou = "BOQ" + s;
                    }
                    return ou;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}