using DocumentFormat.OpenXml.Math;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using RVNLMIS.Models.Config;
using RVNLMIS.Models.PowerBI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    public class ConfigController : Controller
    {
        // GET: Config
        [PageAccessFilter]
        public ActionResult Index()
        {
            GetProjects();
            return View();
        }

        public ActionResult _AddEditPhysicalProgress()
        {
            GetProjects();
            return View();
        }

        public ActionResult GetProjects()
        {
            dbRVNLMISEntities db = new dbRVNLMISEntities();
            var GetProjects = db.tblMasterProjects.ToList();
            SelectList list = new SelectList(GetProjects, "ProjectId", "ProjectName");
            ViewBag.ProjectList = list;
            return View();
        }

        #region -- Add and Update Physical Progress Details --
        [HttpPost]
        public ActionResult AddEditPhysicalProgress(CreatePhyProgress oModel)
        {
            try
            {
                string message = string.Empty;
                if (oModel.PhyID == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblphyParamenters.Where(u => u.ProjID == oModel.ProjId && u.PhyParaName== oModel.PhysicalParamName).FirstOrDefault();
                        if (exist != null)
                        {
                            //message = "Already Exists";
                            return Json(new { message = "0" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblphyParamenter objphy = new tblphyParamenter();
                            objphy.ProjID = oModel.ProjId;
                            objphy.PhyParaName = oModel.PhysicalParamName;
                            objphy.PhyParaCode = oModel.PhysicalParamCode;
                            db.tblphyParamenters.Add(objphy);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {

                        tblphyParamenter objphy = db.tblphyParamenters.Where(u => u.PhyID == oModel.PhyID).SingleOrDefault();
                        objphy.ProjID = oModel.ProjId;
                        objphy.PhyParaName = oModel.PhysicalParamName;
                        objphy.PhyParaCode = oModel.PhysicalParamCode;
                        db.SaveChanges();
                        message = "Updated Successfully";

                    }
                }

                return Json(new { message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion



        #region --- List Resource Values ---
        public ActionResult PhyProgress_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<PhysicalProgressView> obj = new List<PhysicalProgressView>();
                obj = dbContext.PhysicalProgressViews.ToList();
                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit Resource Details --
       
        public ActionResult EditConfigById(int id)
        {
            GetProjects();
            CreatePhyProgress objphy = new CreatePhyProgress();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var oPhyDetails = db.tblphyParamenters.Where(o => o.PhyID == id ).SingleOrDefault();
                        if (oPhyDetails != null)
                        {
                           
                            objphy.PhysicalParamName = oPhyDetails.PhyParaName;
                            objphy.PhysicalParamCode = oPhyDetails.PhyParaCode;
                            objphy.PhyID = oPhyDetails.PhyID;
                            objphy.ProjId = oPhyDetails.ProjID;
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditPhysicalProgress", objphy);
        }
        #endregion

        #region -- Delete Phy Details --
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblphyParamenter objPhy = db.tblphyParamenters.Find(id);
                    if(objPhy != null)
                    {
                        db.tblphyParamenters.Remove(objPhy);
                        db.SaveChanges();
                        return Json("success");
                    }
                    return View("Index");    
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }
        #endregion


        //Cash Parameter Start//.................................................................................................................//Cash Parameter Start//


        public ActionResult ListOfCashParameter()
        {
            GetProjects();
            return View();
        }

        public ActionResult _AddEditCashParameter()
        {
            GetProjects();
            return View();
        }

       
        #region -- Add and Update cash Details --
        [HttpPost]
        public ActionResult AddEditCashParameter(CreateCashParam oModel)
        {
            try
            {
                string message = string.Empty;
                if (oModel.CashID == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblcashParamenters.Where(u => u.ProjID == oModel.ProjId ).FirstOrDefault();
                        if (exist != null)
                        {
                            message = "Already Exists";
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblcashParamenter objcash = new tblcashParamenter();
                            objcash.ProjID = oModel.ProjId;
                            objcash.CashParameter = oModel.CashParameter;
                            objcash.ProjectCurrency = oModel.ProjectCurrency;
                            db.tblcashParamenters.Add(objcash);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {

                        tblcashParamenter objcash = db.tblcashParamenters.Where(u => u.CashID == oModel.CashID).SingleOrDefault();
                        objcash.ProjID = oModel.ProjId;
                        objcash.CashParameter = oModel.CashParameter;
                        objcash.ProjectCurrency = oModel.ProjectCurrency;
                        db.SaveChanges();
                        message = "Updated Successfully";

                    }
                }
                ModelState.Clear();
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion



        #region --- List Resource Values ---
        public ActionResult Cash_Details([DataSourceRequest] DataSourceRequest request)
         {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<CasParameterView> obj = new List<CasParameterView>();
                obj = dbContext.CasParameterViews.ToList();
                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit Resource Details --

        public ActionResult EditCashById(int id)
        {
            GetProjects();
            CreateCashParam objcash = new CreateCashParam();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var oCashDetails = db.tblcashParamenters.Where(o => o.CashID == id).SingleOrDefault();
                        if (oCashDetails != null)
                        {

                            objcash.CashParameter = oCashDetails.CashParameter;
                            objcash.ProjectCurrency = oCashDetails.ProjectCurrency;
                            objcash.CashID = oCashDetails.CashID;
                            objcash.ProjId = Convert.ToInt32(oCashDetails.ProjID);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditCashParameter", objcash);
        }
        #endregion

        #region -- Delete cash Details --
        [HttpPost]
        public ActionResult DeleteCash(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblcashParamenter obj = db.tblcashParamenters.Find(id);
                    if (obj != null)
                    {
                        db.tblcashParamenters.Remove(obj);
                        db.SaveChanges();
                        return Json("success");
                    }
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }
        #endregion


        //Project Kpi Parameter//...............................................................................................................//Project Kpi Parameter//

        public ActionResult ListOfKpiParameter()
        {
            GetProjects();
            return View();
        }

        public ActionResult _AddEditKpiParameter()
        {
            GetProjects();
            return View();
        }


        #region -- Add and Update Kpi Details --
        [HttpPost]
        public ActionResult AddEditKpiParameter(CreateKPIParam oModel)
        {
            try
            {
                string message = string.Empty;
                if (oModel.KPID == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblkpiParamenters.Where(u => u.ProjID == oModel.ProjId).FirstOrDefault();
                        if (exist != null)
                        {
                            message = "Already Exists";
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblkpiParamenter objKpi = new tblkpiParamenter();
                            objKpi.ProjID = oModel.ProjId;
                            objKpi.KpiParameter = oModel.KpiParameter;
                            objKpi.KpiSeq = oModel.KpiSequence;
                            objKpi.Remark = oModel.Remark;
                            db.tblkpiParamenters.Add(objKpi);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {

                        tblkpiParamenter objKpi = db.tblkpiParamenters.Where(u => u.KpiID == oModel.KPID).SingleOrDefault();
                        objKpi.ProjID = oModel.ProjId;
                        objKpi.KpiParameter = oModel.KpiParameter;
                        objKpi.KpiSeq = oModel.KpiSequence;
                        objKpi.Remark = oModel.Remark;
                        db.SaveChanges();
                        message = "Updated Successfully";

                    }
                }
                ModelState.Clear();
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion



        #region --- List Resource Values ---
        public ActionResult Kpi_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<KpiParameterView> obj = new List<KpiParameterView>();
                obj = dbContext.KpiParameterViews.ToList();
                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit Resource Details --

        public ActionResult EditKpiById(int id)
        {
            GetProjects();
            CreateKPIParam objkpi = new CreateKPIParam();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var oKpiDetails = db.tblkpiParamenters.Where(o => o.KpiID == id).SingleOrDefault();
                        if (oKpiDetails != null)
                        {

                            objkpi.KpiParameter = oKpiDetails.KpiParameter;
                            objkpi.KpiSequence = oKpiDetails.KpiSeq;
                            objkpi.Remark = oKpiDetails.Remark;
                            objkpi.ProjId = Convert.ToInt32(oKpiDetails.ProjID);
                            objkpi.KPID = oKpiDetails.KpiID;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditKpiParameter", objkpi);
        }
        #endregion

        #region -- Delete cash Details --
        [HttpPost]
        public ActionResult DeleteKpi(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblkpiParamenter obj = db.tblkpiParamenters.Find(id);
                    if (obj != null)
                    {
                        db.tblkpiParamenters.Remove(obj);
                        db.SaveChanges();
                        return Json("success");
                    }
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }

        #endregion




        //HSE Parameter Start//..............................................................................................//HSE Parameter Start

        public ActionResult ListOfHseParameter()
        {
            GetProjects();
            return View();
        }

        public ActionResult _AddEditHseParameter()
        {
            GetProjects();
            return View();
        }


        #region -- Add and Update Hse Details --
        [HttpPost]
        public ActionResult AddEditHseParameter( CreateHseParam oModel)
        {
            try
            {
                string message = string.Empty;
                if (oModel.HPID == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblHscParamenters.Where(u => u.ProjID == oModel.ProjId).FirstOrDefault();
                        if (exist != null)
                        {
                            message = "Already Exists";
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblHscParamenter objhse = new tblHscParamenter();
                            objhse.ProjID = oModel.ProjId;
                            objhse.HParName = oModel.HseParamName;
                            objhse.HParSeq = oModel.HseParamSeq;
                            db.tblHscParamenters.Add(objhse);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {

                        tblHscParamenter objhse = db.tblHscParamenters.Where(u => u.HPID == oModel.HPID).SingleOrDefault();
                        objhse.ProjID = oModel.ProjId;
                        objhse.HParName = oModel.HseParamName;
                        objhse.HParSeq = oModel.HseParamSeq;
                        db.SaveChanges();
                        message = "Updated Successfully";

                    }
                }
                ModelState.Clear();
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion



        #region --- List Resource Values ---
        public ActionResult Hse_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<HscParamenterView> obj = new List<HscParamenterView>();
                obj = dbContext.HscParamenterViews.ToList();
                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit Resource Details --

        public ActionResult EditHseById(int id)
        {
            GetProjects();
            CreateHseParam obj = new CreateHseParam();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var Details = db.tblHscParamenters.Where(o => o.HPID == id).SingleOrDefault();
                        if (Details != null)
                        {

                            obj.HseParamSeq = Details.HParSeq;
                            obj.HPID = Details.HPID;
                            obj.HseParamName = Details.HParName;
                            obj.ProjId = Convert.ToInt32(Details.ProjID);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditHseParameter", obj);
        }
        #endregion

        #region -- Delete cash Details --
        [HttpPost]
        public ActionResult DeleteHse(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblHscParamenter obj = db.tblHscParamenters.Find(id);
                    if (obj != null)
                    {
                        db.tblHscParamenters.Remove(obj);
                        db.SaveChanges();
                        return Json("success");
                    }
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }

        #endregion



        //Qaqc Parameters Start//.........................................................................................................................//Qaqc Parameters Start//


        public ActionResult ListOfQaqcParameter()
        {
            GetProjects();
            return View();
        }

        public ActionResult _AddEditQaqcParameter()
        {
            GetProjects();
            return View();
        }


        #region -- Add and Update Qaqc Details --
        [HttpPost]
        public ActionResult AddEditQaqcParameter(CreateQaQcParam oModel)
        {
            try
            {
                string message = string.Empty;
                if (oModel.QPID == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblQaqcParamenters.Where(u => u.ProjID == oModel.ProjId).FirstOrDefault();
                        if (exist != null)
                        {
                            message = "Already Exists";
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblQaqcParamenter obj = new tblQaqcParamenter();
                            obj.ProjID = oModel.ProjId;
                            obj.QParName = oModel.QParName;
                            obj.QParSeq = oModel.QParSeq;
                            db.tblQaqcParamenters.Add(obj);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    { 
                        tblQaqcParamenter obj = db.tblQaqcParamenters.Where(u => u.QPID == oModel.QPID).SingleOrDefault();
                        obj.ProjID = oModel.ProjId;
                        obj.QParSeq = oModel.QParSeq;
                        obj.QParName = oModel.QParName;
                        db.SaveChanges();
                        message = "Updated Successfully";

                    }
                }
                ModelState.Clear();
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion



        #region --- List Resource Values ---
        public ActionResult Qaqc_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<QaqcParameterView> obj = new List<QaqcParameterView>();
                obj = dbContext.QaqcParameterViews.ToList();
                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit Resource Details --

        public ActionResult EditQaqcById(int id)
        {
            GetProjects();
            CreateQaQcParam obj = new CreateQaQcParam();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var Details = db.tblQaqcParamenters.Where(o => o.QPID == id).SingleOrDefault();
                        if (Details != null)
                        { 
                            obj.QParSeq = Details.QParSeq;
                            obj.QParName = Details.QParName;
                            obj.ProjId = Convert.ToInt32(Details.ProjID);
                            obj.QPID = Details.QPID;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditQaqcParameter", obj);
        }
        #endregion

        #region -- Delete Qaqc Details --
        [HttpPost]
        public ActionResult DeleteQaqc(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblQaqcParamenter obj = db.tblQaqcParamenters.Find(id);
                    if (obj != null)
                    {
                        db.tblQaqcParamenters.Remove(obj);
                        db.SaveChanges();
                        return Json("success");
                    }
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }

        #endregion

        //Invoice due periods//.....................................................................................................................//Invoice due periods//

        public ActionResult ListOfInvoiceDueParameter()
        {
            GetProjects();
            return View();
        }

        public ActionResult _AddEditInvoiceDueParameter()
        {
            GetProjects();
            return View();
        }


        #region -- Add and Update Invoice Due Period Details --
        [HttpPost]
        public ActionResult AddEditInvoiceDueParameter(CreateInvoiceDuePeriod oModel)
        {
            try
            {
                string message = string.Empty;
                if (oModel.InvDID == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.prjInvDueDates.Where(u => u.ProjID == oModel.ProjId).FirstOrDefault();
                        if (exist != null)
                        {
                            message = "Already Exists";
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            prjInvDueDate obj = new prjInvDueDate();
                            obj.ProjID = oModel.ProjId;
                            obj.ApprovalPeriod = oModel.ApprovalPeriod;
                            obj.PaymentPeriod = oModel.PaymentPeriod;
                            db.prjInvDueDates.Add(obj);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        prjInvDueDate obj = db.prjInvDueDates.Where(u => u.InvDID == oModel.InvDID).SingleOrDefault();
                        obj.ProjID = oModel.ProjId;
                        obj.ApprovalPeriod = oModel.ApprovalPeriod;
                        obj.PaymentPeriod = oModel.PaymentPeriod;
                        db.SaveChanges();
                        message = "Updated Successfully";

                    }
                }
                ModelState.Clear();
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion



        #region --- List Resource Values ---
        public ActionResult InvoiceDue_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<prjInvDueDateView> obj = new List<prjInvDueDateView>();
                obj = dbContext.prjInvDueDateViews.ToList();
                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit Resource Details --

        public ActionResult EditInvoiceDueById(int id)
        {
            GetProjects();
            CreateInvoiceDuePeriod obj = new CreateInvoiceDuePeriod();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var Details = db.prjInvDueDates.Where(o => o.InvDID == id).SingleOrDefault();
                        if (Details != null)
                        {
                            obj.ApprovalPeriod = Details.ApprovalPeriod;
                            obj.PaymentPeriod = Details.PaymentPeriod;
                            obj.ProjId = Convert.ToInt32(Details.ProjID);
                            obj.InvDID = Details.InvDID;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditInvoiceDueParameter", obj);
        }
        #endregion

        #region -- Delete Invoice Due Details --
        [HttpPost]
        public ActionResult DeleteInvoiceDue(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    prjInvDueDate obj = db.prjInvDueDates.Find(id);
                    if (obj != null)
                    {
                        db.prjInvDueDates.Remove(obj);
                        db.SaveChanges();
                        return Json("success");
                    }
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }

        #endregion


        //Project Basline Start//..............................................................................................................................//Project Baseline Start//

        public ActionResult ListOfPrjBaselineParameter()
        {
            GetProjects();
            return View();
        }

        public ActionResult _AddEditPrjBaselineParameter()
        {
            GetProjects();
            return View();
        }


        #region -- Add and Update BaseLine Details --
        [HttpPost]
        public ActionResult AddEditPrjBaselineParameter(CreateProjectBaseline oModel)
        {
            try
            {
                string message = string.Empty;
                if (oModel.BaseID == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblBaselines.Where(u => u.ProjID == oModel.ProjId).FirstOrDefault();
                        if (exist != null)
                        {
                            message = "Already Exists";
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            tblBaseline obj = new tblBaseline();
                            obj.ProjID = oModel.ProjId;
                            obj.BaselineStatus = oModel.BaselineStatus;
                            obj.BaseRevision = oModel.BaseRevision;
                            obj.ResponseDate = Convert.ToDateTime(oModel.ResponseDate);
                            obj.BaseSubmissionDate = Convert.ToDateTime(oModel.BaseSubmissionDate);
                            obj.Remark = oModel.Remark;
                            obj.IsActive = oModel.IsActive;
                            db.tblBaselines.Add(obj);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        tblBaseline obj = db.tblBaselines.Where(u => u.BaseID == oModel.BaseID).SingleOrDefault();
                        obj.ProjID = oModel.ProjId;
                        obj.BaselineStatus = oModel.BaselineStatus;
                        obj.BaseRevision = oModel.BaseRevision;
                        obj.ResponseDate = Convert.ToDateTime(oModel.ResponseDate);
                        obj.BaseSubmissionDate = Convert.ToDateTime(oModel.BaseSubmissionDate);
                        obj.Remark = oModel.Remark;
                        obj.IsActive = oModel.IsActive;
                        db.SaveChanges();
                        message = "Updated Successfully";

                    }
                }
                ModelState.Clear();
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion



        #region --- List Resource Values ---
        public ActionResult PrjBaseLine_Details([DataSourceRequest] DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<prjBaselineView> obj = new List<prjBaselineView>();
                obj = dbContext.prjBaselineViews.ToList();
                return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -- Edit Resource Details --

        public ActionResult EditBaselineById(int id)
        {
            GetProjects();
            CreateProjectBaseline obj = new CreateProjectBaseline();
            try
            {
                if (id != 0)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        var oModel = db.tblBaselines.Where(o => o.BaseID == id).SingleOrDefault();
                        if (oModel != null)
                        {
                            obj.ProjId = Convert.ToInt32(oModel.ProjID);
                            obj.BaselineStatus = oModel.BaselineStatus;
                            obj.BaseRevision = oModel.BaseRevision;
                            obj.ResponseDate = Convert.ToDateTime(oModel.ResponseDate).ToString("yyyy-MM-dd");
                            obj.BaseSubmissionDate = Convert.ToDateTime(oModel.BaseSubmissionDate).ToString("yyyy-MM-dd");
                            obj.Remark = oModel.Remark;
                            obj.IsActive = Convert.ToBoolean(oModel.IsActive);
                            obj.BaseID = oModel.BaseID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_AddEditPrjBaseLineParameter", obj);
        }
        #endregion

        #region -- Delete BaseLine Details --
        [HttpPost]
        public ActionResult DeleteBaseLine(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblBaseline obj = db.tblBaselines.Find(id);
                    if (obj != null)
                    {
                        db.tblBaselines.Remove(obj);
                        db.SaveChanges();
                        return Json("success");
                    }
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }

        #endregion

    }
}