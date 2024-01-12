using RVNLMIS.Controllers;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RVNLMIS.API
{
    public class PlanAndDrawingApiController : ApiController
    {
        public IEnumerable<DrpOptionsModel> GetPackageList(int userId, int roleId)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var pkgs = dbContext.GetRoleAssignedPackageList(userId, roleId).Select(s => new DrpOptionsModel
                {
                    ID = s.PackageId,
                    Name = s.PackageName
                }).ToList();
                return pkgs;
            }
        }

        public IEnumerable<DrpOptionsModel> GetEntityOnPackageSelection(int selectedPackage)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var entities = dbContext.tblMasterEntities.Where(e => e.PackageId == selectedPackage && e.IsDelete == false).Select(s => new DrpOptionsModel
                {
                    ID = s.EntityID,
                    Name = s.EntityCode + " " + s.EntityName
                }).ToList();

                return entities;
            }
        }

        public IEnumerable<DrpOptionsModel> GetActionsByApprover(int selectedApprId)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var actions = dbContext.tblApprActions.Where(e => e.ApproverId == selectedApprId && e.IsDeleted == false).Select(s => new DrpOptionsModel
                {
                    ID = s.ActionId,
                    Name = s.ActionName
                }).ToList();

                return actions;
            }
        }

        public HttpResponseMessage GetDwgTypeAndApprList()
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var drawingType = dbContext.tblEnggDwgTypes.Where(d => d.IsDeleted == false).Select(s => new DrpOptionsModel
                {
                    ID = s.DwgId,
                    Name = s.DwgName
                }).ToList();

                var approver = dbContext.tblEnggApprGates.Where(a => a.IsDeleted == false).Select(s => new DrpOptionsModel
                {
                    ID = s.ApprGateId,
                    Name = s.ApprGateName
                }).ToList();

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { drawingType, approver });
            }
        }

        public HttpResponseMessage GetDrawingList(int entityId, int drawingTypeId)
        {
            List<PkgDwgApprovalModel> lstInfo = new List<PkgDwgApprovalModel>();

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    if (drawingTypeId == 0 && entityId != 0)
                    {
                        lstInfo = dbContext.EntityDrawingApprViews.Where(e => e.EntityId == entityId).Select(s => new PkgDwgApprovalModel
                        {
                            AutoId = s.Id,
                            EntityName = s.EntityName,
                            ApprovedBy = s.ApprGateName,
                            Drawing = s.DwgName,
                            Date = s.ApprStartDate,
                            Revision = "Rev " + s.Revision,
                            ActionName = s.ActionName,
                            Remark = s.Remark,
                            AttachFileName = s.FileName,
                            AttachFilePath = s.Path,
                            DrawingName=s.DrawingName
                        }).ToList();
                    }
                    else if (drawingTypeId != 0 && entityId != 0)
                    {
                        lstInfo = dbContext.EntityDrawingApprViews.Where(e => e.DwgId == drawingTypeId && e.EntityId == entityId).Select(s => new PkgDwgApprovalModel
                        {
                            AutoId = s.Id,
                            EntityName = s.EntityName,
                            ApprovedBy = s.ApprGateName,
                            Drawing = s.DwgName,
                            Date = s.ApprStartDate,
                            Revision = "Rev " + s.Revision,
                            ActionName = s.ActionName,
                            Remark = s.Remark,
                            AttachFileName = s.FileName,
                            AttachFilePath = s.Path,
                            DrawingName = s.DrawingName
                        }).ToList();
                    }
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { lstInfo });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, new { lstInfo });
            }
        }

        public HttpResponseMessage GetDrawingListByType(int entityId, int drawingTypeId)
        {
            List<PkgDwgApprovalModel> lstInfo = new List<PkgDwgApprovalModel>();
            List<DrawingApiModel> apiResponseList = new List<DrawingApiModel>();

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    if (drawingTypeId == 0 && entityId != 0)
                    {
                        lstInfo = dbContext.EntityDrawingApprViews.Where(e => e.EntityId == entityId).Select(s => new PkgDwgApprovalModel
                        {
                            DwgId = s.DwgId,
                            Drawing = s.DwgName,
                        }).Distinct().ToList();
                    }
                    else if (drawingTypeId != 0 && entityId != 0)
                    {
                        lstInfo = dbContext.EntityDrawingApprViews.Where(e => e.DwgId == drawingTypeId && e.EntityId == entityId).Select(s => new PkgDwgApprovalModel
                        {
                            DwgId = s.DwgId,
                            Drawing = s.DwgName,
                        }).Distinct().ToList();
                    }

                    foreach (var dwg in lstInfo)
                    {
                        DrawingApiModel apiResponse = new DrawingApiModel();
                        apiResponse.DrawingName = dwg.Drawing;

                        apiResponse.DrawingInfo = dbContext.EntityDrawingApprViews.Where(e => e.DwgId == dwg.DwgId && e.EntityId == entityId).Select(s => new PkgDwgApprovalModel
                        {
                            DwgId = s.DwgId,
                            AutoId = s.Id,
                            EntityName = s.EntityName,
                            ApprovedBy = s.ApprGateName,
                            Drawing = s.DwgName,
                            Date = s.ApprStartDate,
                            Revision = "Rev " + s.Revision,
                            ActionName = s.ActionName,
                            Remark = s.Remark,
                            AttachFileName = s.FileName,
                            AttachFilePath = s.Path,
                            DrawingName = s.DrawingName
                        }).ToList();

                        apiResponseList.Add(apiResponse);
                    }
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { apiResponseList });
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lstInfo });
            }
        }

        [HttpPost]
        public HttpResponseMessage LoadDrawingToEdit(int? entDwgApprId)
        {
            DrawingUpdateModel objToEdit = new DrawingUpdateModel();

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    var present = dbContext.EntityDrawingApprViews.Where(t => t.Id == entDwgApprId).FirstOrDefault();

                    if (present != null)
                    {
                        objToEdit.PackageId = present.PackageId;
                        objToEdit.AutoId = present.Id;
                        objToEdit.EntityId = present.EntityId;
                        objToEdit.DrawingTypeId = present.DwgId;
                        objToEdit.ApprovedById = present.ApprGateId;
                        objToEdit.StrDwgDate = Convert.ToDateTime(present.ApprStartDate).ToString("yyyy-MM-dd");
                        objToEdit.ActionId = present.ActionId;
                        objToEdit.Remark = present.Remark;
                        objToEdit.DrawingName = present.DrawingName;
                        objToEdit.Revision = Convert.ToInt32(present.Revision);
                    }
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { objToEdit });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                     .CreateResponse(HttpStatusCode.OK, new { objToEdit });
            }
        }

        [HttpPost]
        public HttpResponseMessage SubmitDrawing(DrawingUpdateModel objModel)
        {
            string message = string.Empty;
            EnggDrawingController objEnggDwg = new EnggDrawingController();

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    DateTime? SelectedStartDate = (objModel.StrDwgDate == null) ? (Nullable<DateTime>)null : Convert.ToDateTime(objModel.StrDwgDate);

                    if (objModel.AutoId != 0)   //Update operation
                    {
                        var getExisting = dbContext.tblEnggEntDwgApprs.Where(d => d.Id == objModel.AutoId).FirstOrDefault();

                        //if (objEnggDwg.CheckIsLastRevToUpdate(objModel.EntityId, objModel.DrawingTypeId, objModel.AutoId))
                        //{
                        //    if (objModel.IsFinal)
                        //    {
                        //        getExisting.Revision = "Rev Final";
                        //    }
                        //}
                        getExisting.EntityId = objModel.EntityId;
                        getExisting.DwgId = objModel.DrawingTypeId;
                        getExisting.AppGateId = objModel.ApprovedById;
                        getExisting.ApprStartDate = SelectedStartDate;
                        getExisting.Revision = Convert.ToString(objModel.Revision);
                        getExisting.ActionId = objModel.ActionId;
                        getExisting.Remark = objModel.Remark;
                        getExisting.DrawingName = objModel.DrawingName;
                        message = "Data updated successfully.";
                    }
                    else                       //Add operation
                    {
                        //check is exists
                        var isexist = dbContext.tblEnggEntDwgApprs.Where(e => e.EntityId == objModel.EntityId && e.DwgId == objModel.DrawingTypeId && e.AppGateId == objModel.ApprovedById
                                      && e.ApprStartDate == SelectedStartDate).FirstOrDefault();

                        if (isexist != null)
                        {
                            message = "Record is already exists.";
                            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
                        }

                        EnggDrawingController objED = new EnggDrawingController();
                        //check isFinal
                        //if (objED.CheckRevisionFinal(objModel.DrawingTypeId, objModel.EntityId))
                        //{
                        //    message = "The Drawing is marked as final.You cannot add new one.";
                        //    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
                        //}

                        tblEnggEntDwgAppr objAdd = new tblEnggEntDwgAppr();
                        objAdd.EntityId = objModel.EntityId;
                        objAdd.DwgId = objModel.DrawingTypeId;
                        objAdd.AppGateId = objModel.ApprovedById;
                        objAdd.ApprStartDate = SelectedStartDate;
                        objAdd.Revision = Convert.ToString(objModel.Revision);
                        objAdd.ActionId = objModel.ActionId;
                        objAdd.Remark = objModel.Remark;
                        objAdd.DrawingName = objModel.DrawingName;

                        dbContext.tblEnggEntDwgApprs.Add(objAdd);
                        message = "Data added successfully.";
                    }
                    dbContext.SaveChanges();

                    dbContext.UpdateApprFinishDate(objModel.ApprovedById, SelectedStartDate, objModel.EntityId, objModel.DrawingTypeId);

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage DeleteEnggEntDwgAppr(int id)
        {
            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    var objTodelete = dbContext.tblEnggEntDwgApprs.Where(d => d.Id == id).SingleOrDefault();
                    dbContext.tblEnggEntDwgApprs.Remove(objTodelete);
                    dbContext.SaveChanges();
                }

                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, "Data deleted successfully.");
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public IEnumerable<DrpOptionsModel> GetDrawingNameList()
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var _DwgNameList = (from e in dbContext.tblEnggEntDwgApprs
                                    where e.DrawingName != "-"
                                    select new DrpOptionsModel
                                    {
                                        // ID = e.Id,
                                        Name = e.DrawingName
                                    }).Distinct().ToList();
                return _DwgNameList;
            }
        }
    }
}
