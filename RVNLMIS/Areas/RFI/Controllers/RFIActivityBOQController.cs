using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [AreaSessionExpire]
    [HandleError]
    public class RFIActivityBOQController : Controller
    {
        // GET: RFIBOQMaster
        public ActionResult Index()
        {
         
            return View();
        }

        #region --- List BOQ master Values ---
        public ActionResult ActivityBOQ_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<RFIActivityBOQModel> obj = new List<RFIActivityBOQModel>();

                obj = (from x in dbContext.tblRFIActivityBOQs 
                       join a in dbContext.tblRFIActivities on x.RFIActId equals a.RFIActId
                       join b in dbContext.tblBOQMasters on x.RFIBOQId equals b.BoqID
                       select new { x,a,b })
                                       .AsEnumerable().Select(s =>
                                          new RFIActivityBOQModel
                                          {
                                              RFIActBOQId = s.x.RFIActBOQId,
                                              RFIActId = Convert.ToInt32(s.x.RFIActId),
                                              RFIBOQId = Convert.ToInt32(s.x.RFIBOQId),
                                              RFIBOQName= s.b.BoqName,
                                              RFIActName=s.a.RFIActName,
                                              RFIBOQCode = s.x.RFIBOQCode
                                             
                                          }).ToList();

                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Bind Dropdown
        public JsonResult BindActivityDrpValues(string text)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var _Activity = (from a in dbContext.tblRFIActivities where a.isDeleted==false select new  { RFIActId = a.RFIActId, RFIActName = a.RFIActName }).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        _Activity = _Activity.Where(p => p.RFIActName.ToLower().Contains(text)).ToList();
                    }
                    return Json(_Activity, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult BindRFIBOQDrpValues(string text)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var _Activity = (from a in dbContext.tblBOQMasters select new drpBOQGroup { RFIBOQId = a.BoqID, BOQName = a.BoqName }).ToList();
                    if (!string.IsNullOrEmpty(text))
                    {
                        _Activity = _Activity.Where(p => p.BOQName.ToLower().Contains(text)).ToList();
                    }
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
            public int RFIActId { get; set; }
            public string RFIActName { get; set; }
        }
        public class drpBOQGroup
        {
            public int RFIBOQId { get; set; }
            public string BOQName { get; set; }
        }
        #endregion

        #region -- Add RFI BOQ Master Details --
        [HttpPost]
        public ActionResult SubmitRFIBOQ(RFIActivityBOQModel oModel)
        {
            string message = string.Empty;
            try
            {
                
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.RFIActBOQId == 0 || oModel.RFIActBOQId == null)
                        {
                            var exist = db.tblRFIActivityBOQs.Where(u => u.RFIBOQCode==oModel.RFIBOQCode).ToList();
                            if (exist.Count != 0)
                            {
                                message = "3";
                            }                           
                            else
                            {
                                tblRFIActivityBOQ objWG = new tblRFIActivityBOQ();
                                objWG.RFIActId = oModel.RFIActId;
                                objWG.RFIBOQId = oModel.RFIBOQId;
                                objWG.RFIBOQCode = oModel.RFIBOQCode;
                                db.tblRFIActivityBOQs.Add(objWG);
                                db.SaveChanges();
                                message = "1";
                            }
                        }
                        else
                        {
                            var exist = db.tblRFIActivityBOQs.Where(u => (u.RFIBOQCode == oModel.RFIBOQCode) && (u.RFIActBOQId != oModel.RFIActBOQId)).ToList();
                            if (exist.Count != 0)
                            {
                                message = "3";
                            }
                            else
                            {
                                tblRFIActivityBOQ objGroupModel = db.tblRFIActivityBOQs.Where(u => u.RFIActBOQId == oModel.RFIActBOQId).SingleOrDefault();
                                objGroupModel.RFIActId = oModel.RFIActId;
                                objGroupModel.RFIBOQId = oModel.RFIBOQId;
                                objGroupModel.RFIBOQCode = oModel.RFIBOQCode;
                                db.SaveChanges();
                                message = "2";
                            }
                        }
                    }
                  
                  //  var result = new { message = message };
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View("_ViewAddEditActBOQ", oModel);
                }
            }
            catch (Exception ex)
            {
                message = "2";
                return View("_ViewAddEditActBOQ", oModel);
            }
        }
        #endregion

        #region -- EDIT RFIBOQ Details --
        public ActionResult EditBOQDetails(int id)
        {
            RFIActivityBOQModel objModel = new RFIActivityBOQModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oBOQEdit = db.tblRFIActivityBOQs.Where(o => o.RFIActBOQId == id).SingleOrDefault();
                        if (oBOQEdit != null)
                        {
                            objModel.RFIActBOQId = oBOQEdit.RFIActBOQId;
                            objModel.RFIActId = Convert.ToInt32( oBOQEdit.RFIActId);
                            objModel.RFIBOQId = Convert.ToInt32(oBOQEdit.RFIBOQId);
                            objModel.RFIBOQCode = oBOQEdit.RFIBOQCode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_ViewAddEditActBOQ", objModel);
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
                    tblRFIActivityBOQ obj = db.tblRFIActivityBOQs.SingleOrDefault(o => o.RFIActBOQId == id);
                    db.tblRFIActivityBOQs.Remove(obj);
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
                    var lastBOQCode = db.GetNextPackageCode("tblRFIActivityBOQ").ToList();
                    if (lastBOQCode.Count() == 0)
                    {
                        ou = "ACTBOQ";
                    }
                    else
                    {
                        int s = Functions.ParseInteger(lastBOQCode[0].intNo.ToString()) + 1;
                        ou = "ACTBOQ" + s;
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