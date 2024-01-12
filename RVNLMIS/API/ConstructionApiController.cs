using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RVNLMIS.Controllers;
using System.Net.Http.Formatting;

namespace RVNLMIS.API
{
    public class ConstructionApiController : ApiController
    {
        public HttpResponseMessage ReadConstructionActivity(int entityId, int ActivityGroupId)
        {
            List<ConstructionViewModel> lstInfo = new List<ConstructionViewModel>();

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    lstInfo = (from s in dbContext.ConstructionActivityViews
                               join a in dbContext.tblActivityGroups on s.ActGId equals a.ActGId
                               where s.EntityID == entityId && s.ActGId == ActivityGroupId
                               select new ConstructionViewModel
                               {
                                   AutoId = s.EntActId,
                                   EntityName = s.EntityName,
                                   EntityCode = s.EntityCode,
                                   ActivityName = s.ActivityName,
                                   BudgetedQty = s.BudgQty,
                                   ActivityUnit = s.ActUnit,
                                   PackageId = s.PackageId,
                                   EntityId = s.EntityID,
                                   ActivityId = s.ConsActId
                               }).ToList();
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lstInfo });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new { lstInfo });
            }
        }

        public IEnumerable<DrpOptionsModel> GetConstActivityList()
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var activities = dbContext.tblConsActivities.Where(d => d.IsDelete == false).Select(s => new DrpOptionsModel
                {
                    ID = s.ConsActId,
                    Name = s.ActivityName
                }).ToList();
                return activities;
            }
        }

        public IEnumerable<DrpOptionsModel> GetActivityGroups()
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var groups = dbContext.tblActivityGroups.Where(d => d.IsDeleted == false).Select(s => new DrpOptionsModel
                {
                    ID = s.ActGId,
                    Name = s.ActivityGroupName
                }).ToList();
                return groups;
            }
        }

        public IEnumerable<DrpOptionsModel> GetEntityOnSectionSelection(int selectedSection)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var entities = dbContext.tblMasterEntities.Where(e => e.SectionID == selectedSection && e.IsDelete == false).Select(s => new DrpOptionsModel
                {
                    ID = s.EntityID,
                    Name = s.EntityCode + " " + s.EntityName
                }).ToList();

                return entities;
            }
        }

        [HttpPost]
        public HttpResponseMessage ConstActivityLoadTransaction(int entActId)
        {
            List<ConstActivityViewModel> lstConsAct = new List<ConstActivityViewModel>();
            ConstActivityViewModel objInfo = new ConstActivityViewModel();
            ConstructionActivityController objConsAct = new ConstructionActivityController();

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    objInfo = objConsAct.GetConstActInfoBlock(dbContext, entActId);
                    lstConsAct = dbContext.GetExistingConstrEntityAct(entActId).Select(r => new ConstActivityViewModel
                    {
                        ConsTranId = r.Id,
                        EntityActId = entActId,
                        TransactionDate = r.TransDate,
                        RevisedQty = r.RevisedQty,
                        CompletedQty = r.CompletedQty,
                        TargetDate = r.TargetDate,
                        Remark = r.Remark,
                        AttachFileName = r.FileName,
                        AttachFilePath = r.Path
                    }).ToList();
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { objInfo, lstConsAct });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message });
            }
        }

        [HttpPost]
        public HttpResponseMessage AddEditProcTransaction(FormDataCollection obj)
        {
            ConstActivityViewModel objModel = new ConstActivityViewModel();
            int transactionId = Convert.ToInt32(obj.Get("TransactionId"));
            int entActId = Convert.ToInt32(obj.Get("entActId"));

            objModel.OperationType = "Add";
            objModel.EntityActId = entActId;

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    tblConsEntActTran present = dbContext.tblConsEntActTrans.Where(t => t.Id == transactionId).FirstOrDefault();
                    if (present != null)
                    {
                        objModel.ConsTranId = present.Id;
                        objModel.StrTransDate = Convert.ToDateTime(present.TransDate).ToString("yyyy-MM-dd");
                        objModel.EntityActId = entActId;
                        objModel.StrTargetDate = Convert.ToDateTime(present.TargetDate).ToString("yyyy-MM-dd");
                        objModel.TargetDate = present.TargetDate;
                        objModel.RevisedQty =Convert.ToDecimal(present.RevisedQty);
                        objModel.CompletedQty =Convert.ToDecimal(present.CompletedQty);
                        objModel.Remark = present.Remark;
                        objModel.OperationType = "Update";
                    }
                    return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.OK, new { objModel });
                }
                catch (Exception ex)
                {
                    return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message });
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage SubmitConstAct(ConstActivityViewModel objList)
        {
            string message = string.Empty;
            DateTime? dt = (objList.StrTargetDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objList.StrTargetDate);

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    DateTime transactionDate = Convert.ToDateTime(objList.StrTransDate);
                    tblConsEntActTran present = dbContext.tblConsEntActTrans.Where(t => t.Id == objList.ConsTranId).FirstOrDefault();

                    if (present != null)   //update values
                    {
                        present.TransDate = transactionDate;
                        present.RevisedQty = Convert.ToDouble(objList.RevisedQty);
                        present.CompletedQty = Convert.ToDouble(objList.CompletedQty);
                        present.Remark = objList.Remark;
                        present.TargetDate = dt;
                        message = "Data updated successfully.";
                    }
                    else                   //add values
                    {
                        tblConsEntActTran objAdd = new tblConsEntActTran();
                        objAdd.TransDate = transactionDate;
                        objAdd.CompletedQty = Convert.ToDouble(objList.CompletedQty);
                        objAdd.EntActId = objList.EntityActId;
                        objAdd.Remark = objList.Remark;
                        objAdd.RevisedQty = Convert.ToDouble(objList.RevisedQty);
                        objAdd.TargetDate = dt;
                        dbContext.tblConsEntActTrans.Add(objAdd);
                        message = "Data added successfully.";
                    }
                    dbContext.SaveChanges();
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage DeleteTransaction(int id)
        {
            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    var objTodelete = dbContext.tblConsEntActTrans.Where(d => d.Id == id).SingleOrDefault();

                    #region --Delete Attachment --

                    if (objTodelete.AttachmentId != null)
                    {
                        dbContext.tblAttachments.Remove(dbContext.tblAttachments.Where(a => a.AttachmentID == objTodelete.AttachmentId).FirstOrDefault());
                        // dbContext.SaveChanges();
                    }
                    #endregion

                    dbContext.tblConsEntActTrans.Remove(objTodelete);
                    dbContext.SaveChanges();
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, "Data deleted successfully.");
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
    }
}
