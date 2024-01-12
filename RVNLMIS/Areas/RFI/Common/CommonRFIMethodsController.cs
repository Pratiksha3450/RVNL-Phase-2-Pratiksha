using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using PrimaBiWeb.Common;
using RVNLMIS.Areas.RFI.Common;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.Controllers;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace RVNLMIS.Areas.RFI.Controllers
{
    [HandleError]
    public class CommonRFIMethodsController : Controller
    {
        #region -----RFI Listing-----

        public List<RFIMainModel> _GetRFIListing(int packageId, int userId, string orgnisation, string designName)
        {
            using (dbRVNLMISEntities dbcontext = new dbRVNLMISEntities())
            {
                List<RFIMainModel> lst = new List<RFIMainModel>();

                lst = dbcontext.ViewRFIMainDetails
                    .AsEnumerable()
                    .Select(s => new RFIMainModel
                    {
                        RFIId = s.RFIId,
                        RFICode = s.RFICode,
                        RevisionNo = Convert.ToInt32(s.revision),
                        StartChainage = s.StartChainge == null ? s.StartChainge.ToString() : IntToStringChainage(Convert.ToInt32((double)s.StartChainge)),
                        EndChainage = s.EndChainage == null ? s.EndChainage.ToString() : IntToStringChainage(Convert.ToInt32((double)s.EndChainage)),
                        PackageId = s.PkgId,
                        OtherWorkLocation = s.OtherWorkLocation,
                        LocationType = s.LocationType,
                        EntityName = s.EntityName,
                        Layer = GetLayer(s.LayerNo),
                        ActivityName = s.RFIActName,
                        PackageName = s.PackageName,
                        WorkSide = s.WorkSide,
                        InspStatus = s.InspStatus,
                        Workgroup = s.WorkGrName,
                        AssignToPMC = s.AssignedTo,
                        AssignToPMCName = s.AssignedToName,
                        RFIOpenDate = s.RFIOpenDate,
                        InspectionDate = s.InspDate
                    }).ToList();

                if (packageId != 0 && orgnisation == "PMC" && designName != "Project Manager")
                {
                    lst = lst.Where(p => p.AssignToPMC == userId).ToList();
                }
                else if (packageId != 0)
                {
                    lst = lst.Where(p => p.PackageId == packageId).ToList();
                }

                return lst;
            }
        }

        #endregion

        #region -----Add/Edit RFI-----

        public Tuple<string, string> _AddEditRFI(RFIMainModel objRFIModel, string userName, int userid)
        {
            string message = string.Empty;
            int entityId = Convert.ToInt32(objRFIModel.EntityId);

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                #region -------VALIDATIONS------

                if (objRFIModel.LocationType == "Entity")
                {
                    ///if entitiy selected update chainage with entity
                    var getEntity = dbContext.tblMasterEntities.Where(e => e.EntityID == entityId).FirstOrDefault();
                    if (getEntity != null)
                    {
                        objRFIModel.StartChainage = getEntity.StartChainage;
                        objRFIModel.EndChainage = getEntity.EndChainage;
                    }
                }

                int _startC = Functions.RepalceCharacter(objRFIModel.StartChainage);
                int _endC = Functions.RepalceCharacter(objRFIModel.EndChainage);
                var activityName = dbContext.tblRFIActivities.Where(a => a.RFIActId == objRFIModel.RFIActivityId).FirstOrDefault();
                SectionController objSecContr = new SectionController();

                if (objRFIModel.LocationType != "Other")
                {
                    string msgChainage = objSecContr.ChainageValidation(objRFIModel.StartChainage, objRFIModel.EndChainage, objRFIModel.PackageId);

                    if (msgChainage != "Empty")
                    {
                        return new Tuple<string, string>("3", msgChainage);
                    }

                    string isexist = _AlreadyExistValidation(objRFIModel.RFIActivityId, activityName.RFIActName, _startC, _endC, objRFIModel.RFIId);

                    if (isexist != "Empty")
                    {
                        return new Tuple<string, string>("5", isexist);
                    }
                }
                #endregion

                if (objRFIModel.RFIId == 0)         //add operation
                {
                    #region -----CheckCodeExists-------
                    var isCodeExists = dbContext.tblRFIMains.Where(a => a.RFICode == objRFIModel.RFICode).FirstOrDefault();

                    if (isCodeExists != null)
                    {
                        return new Tuple<string, string>("4", string.Empty);
                    }
                    #endregion

                    #region -----ADD TO RFIMain-----
                    tblRFIMain objAdd = new tblRFIMain();
                    objAdd.PkgId = objRFIModel.PackageId;
                    objAdd.RFIActivityId = objRFIModel.RFIActivityId;
                    objAdd.RFICode = objRFIModel.RFICode;
                    objAdd.ApprovedStartChainage = 0;
                    objAdd.ApprovedFinishChainage = 0;
                    objAdd.StartChainge = _startC;
                    objAdd.EndChainage = _endC;
                    objAdd.Entity = objRFIModel.EntityId;
                    objAdd.LayerNo = objRFIModel.LayerNo;
                    objAdd.WorkGroupId = objRFIModel.WorkgroupId;
                    objAdd.WorkSide = objRFIModel.WorkSide;
                    objAdd.WorkDescription = objRFIModel.WorkDescription;
                    objAdd.OtherWorkLocation = objRFIModel.OtherWorkLocation;
                    objAdd.LocationType = objRFIModel.LocationType;
                    objAdd.RFIStatus = "Open";
                    objAdd.RFIOpenDate = DateTime.Now;
                    objAdd.IsDeleted = false;

                    dbContext.tblRFIMains.Add(objAdd);
                    dbContext.SaveChanges();
                    #endregion

                    AddRevision(objAdd.RFIId, objRFIModel.AssignToPMC, string.Empty, userid);

                    #region -----ADD TIMELINE ACTIVITY-------

                    string actText = userName + " submit a new RFI with code " + objRFIModel.RFICode + " for following activity " + activityName.RFIActName + " Layer- " +
                        GetLayer(objRFIModel.LayerNo) + " and assigned to " + objRFIModel.AssignToPMCName ?? "NA";
                    RFIFunctions.AddTimelineActivity(objAdd.RFIId, actText, "bg-c-blue", string.Empty, "0");

                    #endregion

                    #region -----ADD NOTIFICATION-----

                    SaveNotificationDetails(objRFIModel, userName, userid);

                    #endregion

                    message = "1";
                }
                else                               //edit operation
                {
                    tblRFIMain objEdit = dbContext.tblRFIMains.Where(r => r.RFIId == objRFIModel.RFIId).FirstOrDefault();
                    objEdit.PkgId = objRFIModel.PackageId;
                    objEdit.RFIActivityId = objRFIModel.RFIActivityId;
                    objEdit.WorkDescription = objRFIModel.WorkDescription;
                    objEdit.StartChainge = _startC;
                    objEdit.EndChainage = _endC;
                    objEdit.Entity = Convert.ToString(objRFIModel.EntityId);
                    objEdit.LayerNo = objRFIModel.LayerNo;
                    objEdit.WorkGroupId = objRFIModel.WorkgroupId;
                    objEdit.WorkSide = objRFIModel.WorkSide;
                    objEdit.OtherWorkLocation = objRFIModel.OtherWorkLocation;
                    objEdit.LocationType = objRFIModel.LocationType;
                    dbContext.Entry(objEdit).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    #region ---find out which fields are updated-----
                    foreach (var entry in dbContext.ChangeTracker.Entries())
                    {
                        // Gets all properties from the changed entity by reflection.
                        foreach (var entityProperty in entry.Entity.GetType().GetProperties())
                        {
                            var propertyName = entityProperty.Name;
                            //var currentValue = entry.Property(propertyName).CurrentValue;
                            //var originalValue = entry.Property(propertyName).OriginalValue;
                        }
                    }
                    #endregion

                    if (objRFIModel.AssignToPMC != null)
                    {   //update assigned to revision
                        var updateRevision = dbContext.tblRFIRevisions.Where(r => r.RFIId == objRFIModel.RFIId)
                         .OrderByDescending(r => r.RFIRevId).FirstOrDefault();

                        if (updateRevision.AssignedTo != objRFIModel.AssignToPMC)
                        {
                            #region -----ADD NOTIFICATION-----

                            SaveNotificationDetails(objRFIModel, userName, userid);

                            #endregion
                        }
                        updateRevision.AssignedTo = objRFIModel.AssignToPMC;
                        dbContext.SaveChanges();
                    }

                    message = "2";
                }
            }
            return new Tuple<string, string>(message, string.Empty);
        }

        private static void SaveNotificationDetails(RFIMainModel objRFIModel, string userName, int userid)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                string msg = "Dear " + objRFIModel.AssignToPMCName + ", " +
                                    "\r\nA new RFI has been generated with code " + objRFIModel.RFICode + " by Mr. " + userName + ". for Following activity " + objRFIModel.ActivityName + " Location is Type: " + objRFIModel.LocationType +
                                    " . Please check and Schedule a Date and Time for Inspection.";

                tblNotificationMsg objAddNot = new tblNotificationMsg();
                objAddNot.Title = "New RFI Submitted";
                objAddNot.Message = msg;
                objAddNot.ReceiverId = objRFIModel.AssignToPMC;
                objAddNot.SenderId = userid;
                objAddNot.SentOn = DateTime.Now;
                dbContext.tblNotificationMsgs.Add(objAddNot);
                dbContext.SaveChanges();

                tblNotificationReadStatu objAddStatus = new tblNotificationReadStatu();
                objAddStatus.IsRead = false;
                objAddStatus.NotificationId = objAddNot.NotificationId;
                objAddStatus.ReceiverId = objRFIModel.AssignToPMC;
                dbContext.tblNotificationReadStatus.Add(objAddStatus);
                dbContext.SaveChanges();
            }
        }

        private string _AlreadyExistValidation(int? rFIActivityId, string activityName, int startC, int endC, int rfiId)
        {
            string mesg = "Empty";

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var isExists = dbContext.tblRFIMains.Where(r => (r.RFIActivityId == rFIActivityId) && (r.StartChainge < endC && r.EndChainage > startC) && r.IsDeleted == false && r.RFIId != rfiId)
                    .FirstOrDefault();

                if (isExists != null)
                {
                    mesg = "RFI for " + activityName + " already exist for chaiange From " + isExists.StartChainge + " To " + isExists.EndChainage + " with RFI No - " + isExists.RFICode + ".";
                }

                return mesg;
            }
        }

        public int AddRevision(int rfiId, int? assignedToId, string Comment, int userid)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                //var isExist = dbContext.tblRFIRevisions.Where(r => r.RFIId == rfiId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                //if (isExist == null)
                //{
                try
                {
                    tblRFIRevision objRev = new tblRFIRevision();
                    objRev.RFIId = rfiId;

                    var getTop1RevForRFI = dbContext.tblRFIRevisions.Where(r => r.RFIId == rfiId)
                        .OrderByDescending(r => r.RFIRevId).FirstOrDefault();

                    if (getTop1RevForRFI == null)
                    {
                        objRev.Revision = "0";
                    }
                    else
                    {
                        objRev.Revision = Convert.ToString(Convert.ToInt32(getTop1RevForRFI.Revision) + 1);
                    }

                    objRev.CreatedBy = userid;
                    objRev.CreatedDate = DateTime.Now;
                    objRev.CreatedLocation = "Web";
                    objRev.AssignedTo = assignedToId;
                    objRev.ContrComment = Comment;

                    dbContext.tblRFIRevisions.Add(objRev);
                    dbContext.SaveChanges();

                    return Convert.ToInt32(objRev.Revision);
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            //}
        }

        private string GetLayer(int? layerNo)
        {
            switch (layerNo)
            {
                case -1: return "NA";
                case 0: return "Top";
            }
            return layerNo.ToString();
        }

        public RFIMainModel _GetRFIToEdit(int id)
        {
            RFIMainModel objModel = new RFIMainModel();
            objModel.LocationType = "Chainage";

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                if (id != 0)
                {
                    objModel = dbContext.ViewRFIMainDetails.Where(r => r.RFIId == id)
                        .AsEnumerable()
                        .Select(s => new RFIMainModel
                        {
                            RFIId = s.RFIId,
                            PackageId = s.PkgId,
                            EndChainage = s.EndChainage == null ? s.EndChainage.ToString() : IntToStringChainage(Convert.ToInt32((double)s.EndChainage)),
                            StartChainage = s.StartChainge == null ? s.StartChainge.ToString() : IntToStringChainage(Convert.ToInt32((double)s.StartChainge)),
                            EntityId = s.Entity,
                            LayerNo = s.LayerNo,
                            LocationType = s.LocationType,
                            WorkDescription = s.WorkDescription,
                            OtherWorkLocation = s.OtherWorkLocation,
                            RFIActivityId = s.RFIActivityId,
                            RFICode = s.RFICode,
                            RFIStatus = s.RFIStatus,
                            WorkgroupId = s.WorkGroupId,
                            WorkSide = s.WorkSide,
                            AssignToPMC = s.AssignedTo
                        })
                        .FirstOrDefault();
                }
            }

            return objModel;
        }

        #endregion

        #region --------RFI Inspection Related methods

        public RFIInspStatusUpdateModel _ViewRFIInspStatus(int rfiId)
        {
            RFIInspStatusUpdateModel objModel = new RFIInspStatusUpdateModel();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                objModel = dbContext.ViewRFIMainDetails.Where(r => r.RFIId == rfiId)
                    .AsEnumerable()
                    .Select(s => new RFIInspStatusUpdateModel
                    {
                        RFIId = s.RFIId,
                        RevisionNo = Convert.ToInt32(s.revision),
                        RevisionId = s.RFIRevId,
                        PackageName = s.PackageName,
                        ActivityName = s.RFIActName,
                        Workgroup = s.WorkGrName,
                        AssignedTo = s.AssignedTo,
                        AssignedToName = s.AssignedToName,
                        Enclosure = s.Enclosure,
                        RFICode = s.RFICode,
                        EndChainage = s.EndChainage == null ? "-" : IntToStringChainage(Convert.ToInt32((double)s.EndChainage)),
                        StartChainage = s.StartChainge == null ? "-" : IntToStringChainage(Convert.ToInt32((double)s.StartChainge)),
                        AprrovedFinishChainage = s.EndChainage == null ? "-" : IntToStringChainage(Convert.ToInt32((double)s.ApprovedFinishChainage)),
                        AprrovedStartChainage = s.StartChainge == null ? "-" : IntToStringChainage(Convert.ToInt32((double)s.ApprovedStartChainage)),
                        EntityName = s.EntityName,
                        InspStatus = s.InspStatus ?? "Open",
                        Layer = GetLayer(s.LayerNo),
                        LocationType = s.LocationType,
                        OtherWorkLocation = s.OtherWorkLocation,
                        WorkSide = s.WorkSide,
                        //InspDate = s.InspDate == null ? string.Empty : Convert.ToDateTime(s.InspDate).ToString("yyyy-MM-ddTHH:mm"),
                        InspDate = s.InspDate,
                        InspId = s.InspId,
                        ContrComment = s.ContrComment,
                        InspName = s.InspStatus,
                        Note = s.note,
                        objPicture = GetRFIPictureModel(s.RFIRevId, s.RFIId, s.InspStatus)
                    }).FirstOrDefault();
            }

            return objModel;
        }

        public List<InspPictureModel> GetRFIPictureModel(int rfiRevId, int rfiId, string status)
        {
            string folderPath = string.Format("/Uploads/RFIPictures/{0}/{1}/", rfiId, rfiRevId);
            List<InspPictureModel> list = new List<InspPictureModel>();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                if (dbContext.tblRFIPictures.Where(r => r.RFIRevId == rfiRevId && r.InspStatus == status).FirstOrDefault() != null)
                {
                    list = dbContext.tblRFIPictures.Where(p => p.RFIRevId == rfiRevId && p.InspStatus == status).Select(s => new { s })
                        .AsEnumerable()
                        .Select(r => new InspPictureModel
                        {
                            PicId = r.s.RFIPicId,
                            ImgUrl = string.Concat(folderPath, rfiRevId, "-", r.s.Picture.Replace(" ", "_")),
                            Remarks = r.s.Remarks,
                            FileName = r.s.Picture
                        }).ToList();
                }
                return list;
            }
        }

        public Tuple<string, string> _RFIStatusUpdateProcess(RFIInspStatusUpdateModel objModel, int loginUserId, string userName, int pkgId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    if (objModel.UserOrgnisation == "Contractor")
                    {
                        int? assigneePMC = 0;
                        ///// submit comment to rfi revision and add new rfi revision
                        var getRFI = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                        //getRFI.ContrComment = objModel.ContrComment;
                        if (getRFI != null)
                        {
                            assigneePMC = getRFI.AssignedTo;
                        }
                        //dbContext.SaveChanges();

                        int newRev = AddRevision(objModel.RFIId, assigneePMC, objModel.ContrComment, loginUserId);

                        ///send notification pmc
                        string message = "RFI Revision submitted";
                        //SMSWhenNewRFI(assigneePMC,);

                        #region -----ADD TIMELINE ACTIVITY-------

                        string actText = "Contractor (" + userName + ") resubmit RFI with comment: " + objModel.ContrComment + ".\n RFI is Open now with revision no " +
                            newRev;
                        RFIFunctions.AddTimelineActivity(objModel.RFIId, actText, "bg-c-blue", string.Empty, "0");

                        #endregion

                        return new Tuple<string, string>("3", string.Empty); //// Contractor added a comment
                    }

                    if (objModel.InspStatus == "Open" && objModel.InspDate != null)
                    {
                        ////assign inspection date and send notification to contractor
                        var getRFI = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                        getRFI.InspDate = Convert.ToDateTime(objModel.InspDate);
                        //getRFI.AssignedTo = objModel.AssignedTo;
                        getRFI.AcceptedDate = DateTime.Now;
                        getRFI.AcceptedBy = loginUserId;
                        dbContext.SaveChanges();
                        //SMSWhenNewRFI(getRFI.AssignedTo,);

                        #region -----ADD TIMELINE ACTIVITY-------

                        //string userName = ((UserModel)Session["RFIUserSession"]).Name;
                        string actText = "PMC user " + userName + " set appoinment to inspect RFI on the date " + objModel.InspDate.Value.ToString("dd MMM yyyy HH:mm");
                        RFIFunctions.AddTimelineActivity(objModel.RFIId, actText, "bg-c-purple", string.Empty, "0");

                        #endregion

                        return new Tuple<string, string>("1", string.Empty);    /////RFI is scheduled
                    }
                    else
                    {
                        #region ------UPDATE INSPECTION STATUS ADD/UPDATE REVISION ENTRIES -----

                        switch (objModel.InspStatus)
                        {
                            case "Approved by PMC":
                                //Close RFI
                                var getRFI = dbContext.tblRFIMains.Where(r => r.RFIId == objModel.RFIId).FirstOrDefault();
                                getRFI.RFICloseDate = Convert.ToDateTime(objModel.InspDate);
                                //update in revision table 
                                //accepted by
                                //accepted date
                                //actual inspdate

                                var getRevision = dbContext.tblRFIRevisions.Where(rv => rv.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                                getRevision.InspStatus = objModel.InspStatus;
                                getRevision.InspId = objModel.InspId;
                                getRevision.InspDate = DateTime.Now;
                                getRevision.InspBy = Convert.ToString(loginUserId);
                                //getRevision.AcceptedBy = loginUserId;
                                //getRevision.AcceptedDate = DateTime.Now;
                                dbContext.SaveChanges();

                                #region -----ADD TIMELINE ACTIVITY-------

                                SaveTimelineAct(objModel.RFIId, userName, objModel.InspStatus, objModel.Note, "bg-c-green");

                                #endregion

                                break;
                            case "Revise and Resubmit":
                                //Update RFI Revision 
                                var getRFIRev = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();

                                getRFIRev.InspDate = Convert.ToDateTime(objModel.InspDate);
                                getRFIRev.Note = objModel.Note;
                                getRFIRev.InspId = objModel.InspId;
                                getRFIRev.InspStatus = objModel.InspStatus;
                                getRFIRev.ContrComment = string.Empty;
                                getRFIRev.InspBy = Convert.ToString(loginUserId);
                                dbContext.SaveChanges();

                                ///Notification send to Contractor

                                #region -----ADD TIMELINE ACTIVITY-------

                                SaveTimelineAct(objModel.RFIId, userName, objModel.InspStatus, objModel.Note, "bg-c-yellow");

                                #endregion

                                break;
                            case "Rejected":
                                var getRevToReject = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();

                                getRevToReject.InspDate = Convert.ToDateTime(objModel.InspDate);
                                getRevToReject.Note = objModel.Note;
                                getRevToReject.InspId = objModel.InspId;
                                getRevToReject.InspStatus = objModel.InspStatus;
                                getRevToReject.ContrComment = string.Empty;
                                getRevToReject.InspBy = Convert.ToString(loginUserId);
                                dbContext.SaveChanges();

                                #region -----ADD TIMELINE ACTIVITY-------

                                SaveTimelineAct(objModel.RFIId, userName, objModel.InspStatus, objModel.Note, "bg-c-red");

                                #endregion

                                break;
                            case "Partially Approved":
                                int _startC = Functions.RepalceCharacter(objModel.AprrovedStartChainage);
                                int _endC = Functions.RepalceCharacter(objModel.AprrovedFinishChainage);

                                var objRFIUpdateChainage = dbContext.tblRFIMains.Where(r => r.RFIId == objModel.RFIId).FirstOrDefault();

                                #region ---CHAINAGE VALIDATION-----

                                SectionController objSecContr = new SectionController();
                                string msgChainage = _ApprovedChainageValidation(objModel.AprrovedStartChainage, objModel.AprrovedFinishChainage, objRFIUpdateChainage.StartChainge, objRFIUpdateChainage.EndChainage);

                                if (msgChainage != "Empty")
                                {
                                    return new Tuple<string, string>("4", msgChainage);
                                }

                                #endregion

                                objRFIUpdateChainage.ApprovedStartChainage = _startC;
                                objRFIUpdateChainage.ApprovedFinishChainage = _endC;

                                var getPartiallyAppRev = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                                getPartiallyAppRev.Note = objModel.Note;
                                getPartiallyAppRev.InspId = objModel.InspId;
                                getPartiallyAppRev.InspStatus = objModel.InspStatus;
                                getPartiallyAppRev.InspBy = Convert.ToString(loginUserId);

                                dbContext.SaveChanges();

                                #region -----ADD TIMELINE ACTIVITY-------

                                SaveTimelineAct(objModel.RFIId, userName, objModel.InspStatus, objModel.Note, "bg-c-cyan");

                                #endregion

                                break;
                                //default:
                                //    var updateAssignedTo = dbContext.tblRFIRevisions.Where(r => r.RFIId == objModel.RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                                //    updateAssignedTo.AssignedTo = objModel.AssignedTo;
                                //    dbContext.SaveChanges();

                                //    // SMSWhenNewRFI(objModel.AssignedTo,);
                                //    break;
                        }

                        #endregion

                        return new Tuple<string, string>("2", string.Empty); ////RFI Status updated successfully
                    }
                }
                catch (Exception ex)
                {
                    return new Tuple<string, string>("Error", string.Empty);
                }
            }
        }

        private string _ApprovedChainageValidation(string sC, string eC, double? startChainge, double? endChainage)
        {
            int approvedStartC = Functions.RepalceCharacter(sC);
            int approvedEndC = Functions.RepalceCharacter(eC);

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    if ((approvedStartC >= startChainge && approvedStartC <= endChainage) && (approvedEndC >= startChainge && approvedEndC <= endChainage))
                    {
                        return "Empty";
                    }
                    return "Approved Start and End Chainage has to be within RFI's Start (" + startChainge + ") and End Chainage (" + endChainage + ").";
                }
            }
            catch (Exception ex)
            {
                return "Empty";
            }
        }

        private void SaveTimelineAct(int rFIId, string userName, string inspStatus, string note, string colorClass)
        {
            string actText = userName + " marked RFI as " + inspStatus + " with following comments: \n" + Environment.NewLine + note;

            RFIFunctions.AddTimelineActivity(rFIId, actText, colorClass, string.Empty, "0");
        }

        #endregion

        public string GetRFICode(int workGrpId)
        {
            string code = string.Empty;

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var res = dbContext.GenerateRFICode(workGrpId).FirstOrDefault();
                code = string.Concat(res.Code, "/", (Convert.ToInt32(res.Number) + 1));
            }

            return code;
        }

        #region ---- Chainage int to String ----
        /// <summary>
        /// Ints to string chainage.
        /// </summary>
        /// <param name="chainage">The chainage.</param>
        /// <returns></returns>
        public string IntToStringChainage(int chainage)
        {
            return chainage.ToString("D6").Insert(chainage.ToString("D6").Length - 3, "+");
        }
        #endregion

        #region -------DROPDOWN METHODS-------

        public List<DropDownOptionModel> _GetWorkgroupList()
        {
            List<DropDownOptionModel> obj = new List<DropDownOptionModel>();

            using (var db = new dbRVNLMISEntities())
            {
                obj = (from w in db.tblWorkGroups
                       select new { w })
                      .AsEnumerable()
                      .Select(s => new DropDownOptionModel
                      {
                          ID = s.w.WorkGrId,
                          Name = s.w.WorkGrName
                      }).ToList();
            }
            return obj;
        }

        public List<DropDownOptionModel> _GetActivity(int? workGrpId)
        {
            using (var db = new dbRVNLMISEntities())
            {
                List<DropDownOptionModel> obj = (from a in db.tblRFIActivities
                                                 where a.WorkGroupId == workGrpId && a.isDeleted == false
                                                 select new { a })
                      .AsEnumerable()
                      .Select(s => new DropDownOptionModel
                      {
                          ID = s.a.RFIActId,
                          Name = s.a.RFIActName
                      }).ToList();
                return obj;
            }
        }

        public List<DropDownOptionModel> _GetAssignedToPMC(int? packageId)
        {
            using (var db = new dbRVNLMISEntities())
            {
                List<DropDownOptionModel> obj = (from a in db.tblRFIUsers
                                                 join d in db.tblRFIDesignations on a.DesignationId equals d.RFIDesignId
                                                 where a.PackgeId == packageId && a.Organisation == "PMC"
                                                 select new { a, d })
                      .AsEnumerable()
                      .Select(s => new DropDownOptionModel
                      {
                          ID = s.a.RFIUserId,
                          Name = s.d.Designation + " (" + s.a.FullName + ")"
                      }).ToList();
                return obj;
            }
        }

        public List<DropDownOptionModel> _GetRFIWorkside()
        {
            using (var db = new dbRVNLMISEntities())
            {
                List<DropDownOptionModel> obj = (from a in db.tblWorkSides
                                                 where a.IsDeleted == false
                                                 select new { a })
                          .AsEnumerable()
                          .Select(s => new DropDownOptionModel
                          {
                              // ID = s.a.WorkSideId,
                              Name = s.a.WorkSideName
                          }).ToList();
                return obj;
            }
        }
        #endregion

        #region ----RFI Enclosure List-----

        public List<AttachDetails> _GetEnclAttachModel(int id)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                List<AttachDetails> objAttachDetail = dbContext.tblRFIEnclosures.Join(dbContext.tblEnclosures, re => re.EnclId, e => e.EnclId, (re, e)
                          => new { re, e }).Where(r => r.re.RFIId == id).AsEnumerable()
                    .Select(s => new AttachDetails
                    {
                        RFIEnclId = s.re.RFIEnclId,
                        EnclType = s.e.EnclName,
                        FileName = s.re.EnclAttach,
                        Path = s.re.EnclAttachPath,
                        Icon = GetFileIconFromExtension(s.re.EnclAttach)
                    }).ToList();
                return objAttachDetail;
            }
        }

        public string GetFileIconFromExtension(string enclAttachFileName)
        {
            var extension = enclAttachFileName.Substring(enclAttachFileName.LastIndexOf('.') + 1);
            string icon = string.Empty;

            switch (extension)
            {
                case "pdf":
                    icon = "fa fa-file-pdf";
                    break;
                case "xls":
                case "xlsx":
                    icon = "fa fa-file-excel";
                    break;
                case "doc":
                case "docx":
                    icon = "fa fa-file-word";
                    break;
                default:
                    icon = "fa fa-file-image";
                    break;
            }
            return icon;
        }

        #endregion

        #region -----Delete Eclosure-----

        public Tuple<string, string, int> _DeleteEncl(int id, string userName)
        {
            string enclType = string.Empty;
            string fileName = string.Empty;
            int rfiId = 0;

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                #region ----DELETE FILE FROM DB------

                var obj = dbContext.tblRFIEnclosures.Where(e => e.RFIEnclId == id).FirstOrDefault();
                string path = string.Concat("~/Uploads/", obj.EnclAttach);
                rfiId = obj.RFIId ?? 0;

                enclType = dbContext.tblEnclosures.Where(e => e.EnclId == obj.EnclId && e.IsDeleted == false).Select(s => s.EnclName).FirstOrDefault();
                fileName = obj.EnclAttach;

                dbContext.tblRFIEnclosures.Remove(obj);
                dbContext.SaveChanges();

                #endregion

                #region -------DELETE FILE FROM PATH------

                Functions.DeleteFilesInFolder(path, false);

                #endregion

                #region -----ADD TIMELINE ACTIVITY-------

                string actText = enclType + " Enclosure attachment " + fileName + " deleted by " + userName;
                RFIFunctions.AddTimelineActivity(rfiId, actText, "bg-c-purple", string.Empty, "0");

                #endregion
                return new Tuple<string, string, int>(enclType, fileName, rfiId);
            }
        }

        #endregion

        #region --------RFI Timeline------

        public RFITimeLineModel _TimelineList(int rfiId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                RFITimeLineModel objModel = (from v in dbContext.ViewRFIMainDetails
                                             where v.RFIId == rfiId
                                             select new { v }).AsEnumerable().Select(m => new RFITimeLineModel
                                             {
                                                 PackageName = m.v.PackageName,
                                                 RFICode = m.v.RFICode,
                                                 RevisionNo = Convert.ToInt32(m.v.revision),
                                                 CurrentStatus = m.v.InspStatus,
                                                 objList = dbContext.tblRFITimelineActivities.Where(r => r.RFIId == rfiId).ToList()
                                                  .Select(tl => new { tl })
                                         .AsEnumerable()
                                         .Select(s => new RFIListTimeLineModel
                                         {
                                             TimelineText = s.tl.TimelineActText,
                                             StatusClass = s.tl.StatusClass,
                                             TimelineDate = s.tl.TimelineActDate,
                                             objAttachList = _GetAttachModelForTimeline(s.tl.AttachmentType, s.tl.AttachAutoId, s.tl.RFIId)
                                         }).ToList().OrderByDescending(o => o.TimelineDate).ToList()
                                             }).FirstOrDefault();
                return objModel;
            }
        }

        public List<AttachDetails> _GetAttachModelForTimeline(string attachType, string attachId, int rfiId)
        {
            List<AttachDetails> objAttachDetail = new List<AttachDetails>();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                if (attachType == "Encl")
                {
                    int enclId = Convert.ToInt32(attachId);
                    objAttachDetail = _GetEnclAttachModel(rfiId).Where(x => x.RFIEnclId == enclId).ToList();
                }
                else if (attachType == "InspPic")
                {
                    List<int> RFIPicIdList = string.IsNullOrEmpty(attachId) ? new List<int>() : attachId.Split(',').Select(int.Parse).ToList();

                    //objAttachDetail = dbContext.tblRFIPictures.Where(x => RFIPicId.Contains(x.RFIPicId)).Select(s => new { s })
                    objAttachDetail = dbContext.tblRFIPictures.Where(x => RFIPicIdList.Contains(x.RFIPicId)).Select(s => new { s })
                         .AsEnumerable()
                         .Select(r => new AttachDetails
                         {
                             RFIEnclId = r.s.RFIPicId,
                             Path = r.s.PicPath,
                             EnclType = r.s.Remarks,
                             FileName = r.s.Picture,
                             Icon = "fa fa-file-image"
                         }).ToList();
                }
                return objAttachDetail;
            }
        }
        #endregion

        #region -----Reschedule RFI-------

        public void _CommonRescheduleRFI(DateTime NewInspDate, int RFIId, int loginUserId, string userName, string assignedTo)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                ////assign inspection date and send notification to contractor
                var getRFI = dbContext.tblRFIRevisions.Where(r => r.RFIId == RFIId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                getRFI.InspDate = Convert.ToDateTime(NewInspDate);
                getRFI.AssignedTo = !string.IsNullOrEmpty(assignedTo) ? Convert.ToInt32(assignedTo) : getRFI.AssignedTo;
                getRFI.InspStatus = "Rescheduled";
                getRFI.AcceptedDate = DateTime.Now;
                getRFI.AcceptedBy = loginUserId;

                dbContext.SaveChanges();
                //SMSWhenNewRFI(getRFI.AssignedTo,);

                #region -----ADD TIMELINE ACTIVITY-------

                //string userName = ((UserModel)Session["RFIUserSession"]).Name;
                string actText = "PMC user " + userName + " rescheduled appoinment to inspect RFI on the date " + NewInspDate.ToString("dd MMM yyyy HH:mm");

                if (!string.IsNullOrEmpty(assignedTo))
                {
                    using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                    {
                        int id = Convert.ToInt32(assignedTo);
                        string newInspctr = db.tblRFIUsers.Where(u => u.RFIUserId == id).Select(n => n.FullName).FirstOrDefault();
                        actText = string.Concat(actText, " and assign new inspector " + newInspctr);
                    }
                }
                RFIFunctions.AddTimelineActivity(RFIId, actText, "bg-c-yellow", string.Empty, "0");

                #endregion
            }
        }

        #endregion

        #region -----Approve by RVNL-------

        public void _CommonRVNLApproveRFI(int rfiId, string userName)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var getRFI = dbContext.tblRFIRevisions.Where(r => r.RFIId == rfiId).OrderByDescending(o => o.RFIRevId).FirstOrDefault();
                //getRFI.InspDate = Convert.ToDateTime(NewInspDate);
                getRFI.InspStatus = "Approved by RVNL";

                dbContext.SaveChanges();

                #region -----ADD TIMELINE ACTIVITY-------

                string actText = "RVNL user " + userName + " approved the RFI";
                RFIFunctions.AddTimelineActivity(rfiId, actText, "bg-c-gray", string.Empty, "0");

                #endregion
            }
        }

        #endregion

        #region -----RFI SUMMARY----

        public List<DropDownOptionModel> _GetUsersListDrp(int packageId, int userId, string org, string designName)
        {
            List<DropDownOptionModel> obj;
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                var allUsers = (from a in db.tblRFIUsers
                                join d in db.tblRFIDesignations on a.DesignationId equals d.RFIDesignId
                                where a.PackgeId == packageId
                                select new { a, d }).ToList();

                if (org != "RVNL" && designName == "Project Manager")
                {
                    allUsers = allUsers.Where(p => p.a.Organisation == org).ToList();
                }
                else if (org != "RVNL" && designName != "Project Manager")
                {
                    allUsers = allUsers.Where(p => p.a.RFIUserId == userId).ToList();
                }

                obj = allUsers.AsEnumerable()
                      .Select(s => new DropDownOptionModel
                      {
                          ID = s.a.RFIUserId,
                          Name = s.a.FullName + " (" + s.d.Designation + ")"
                      }).ToList();
            }

            return obj;
        }

        public RFIcountSummary _GetRfiCountObj(int? id, int userId, int designId, string orgnisation)
        {

            RFIcountSummary obj = new RFIcountSummary();
            id = id ?? 0;

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                obj = dbContext.RFICountSummary(id, userId, designId, orgnisation)
                    .Select(s => new RFIcountSummary
                    {
                        AddCount = s.AddCount,
                        ApprovedCount = s.ApprovedByPMCCount,
                        AssignedCount = s.AssignedCount,
                        ClosedCount = s.ApprByRVNLCount,
                        PartiallyAppCount = s.PartiallyAppCount,
                        RejectedCount = s.RejectedCount,
                        ReOpenCount = s.ReOpenCount,
                        RevisedCount = s.RevisedCount,
                        //ScheduledCount = s.ScheduledCount,
                        //ReScheduledCount = s.ReScheduledCount
                    }).FirstOrDefault();
            }

            return obj;
        }

        #endregion

        #region ----Notification-----

        public List<PushNotifyModel> _NotifyCommonList(int userId)
        {
            List<PushNotifyModel> notifyObj = new List<PushNotifyModel>();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                notifyObj = (from n in dbContext.tblNotificationMsgs
                             join r in dbContext.tblNotificationReadStatus
                             on n.NotificationId equals r.NotificationId

                             where n.ReceiverId == userId && r.IsRead.Value.Equals(false)
                             select new
                             {
                                 n,
                                 r
                             }).AsEnumerable()
                             .Select(s => new PushNotifyModel
                             {
                                 NotificationId = s.n.NotificationId,
                                 Title = s.n.Title,
                                 Message = s.n.Message,
                                 SentOn = s.n.SentOn
                             }).OrderByDescending(o => o.SentOn).Take(5).ToList();
            }
            return notifyObj;
        }

        #endregion
    }
}
