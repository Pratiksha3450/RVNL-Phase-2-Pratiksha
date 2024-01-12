using RVNLMIS.Common;
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
    public class EntityYardApiController : ApiController
    {
        public HttpResponseMessage EntityDetails_Read(int packageId)
        {
            List<EntityMasterModel> lst = new List<EntityMasterModel>();

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    lst = (from x in dbContext.tblMasterEntities
                           join p in dbContext.tblMasterProjects on x.ProjectId equals p.ProjectId
                           where p.IsDeleted == false
                           join s in dbContext.tblSections on x.SectionID equals s.SectionID
                           where s.IsDeleted == false
                           join pkg in dbContext.tblPackages on x.PackageId equals pkg.PackageId
                           where p.IsDeleted == false
                           // join et in dbContext.tblEntityTypes on x.EntityType equals et.EntityType
                           where x.IsDelete == false && pkg.PackageId == packageId
                           select new EntityMasterModel
                           {
                               PackageId = (int)x.PackageId,
                               ProjectId = x.ProjectId,
                               EntityID = x.EntityID,
                               SectionID = x.SectionID,
                               EntityName = x.EntityName,
                               SectionName = s.SectionName,
                               ProjectName = p.ProjectName,
                               PackageName = pkg.PackageName,
                               EntityCode = x.EntityCode,
                               EntityTypeId = x.EntityType,
                               Lat = x.Lat ?? "NA",
                               Long = x.Long ?? "NA",
                               StartChainage = x.StartChainage,
                               EndChainage = x.EndChainage,
                           }).ToList();
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lst });
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new { lst });
            }
        }

        public IEnumerable<DrpOptionsModel> GetSectionOnPackageSelection(int packageId)
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var _SectionList = (from e in dbContext.tblSections
                                    where (e.PackageId == packageId && e.IsDeleted == false)
                                    select new DrpOptionsModel
                                    {
                                        ID = e.SectionID,
                                        Name = e.SectionCode + " - " + e.SectionName
                                    }).ToList();
                return _SectionList;
            }
        }

        public IEnumerable<DrpOptionsModel> GetEntityType()
        {
            using (var dbContext = new dbRVNLMISEntities())
            {
                var _entityList = (from e in dbContext.tblEntityTypes
                                   where e.IsDeleted == false
                                   select new DrpOptionsModel
                                   {
                                       ID = e.Id,
                                       Name = e.EntityType
                                   }).ToList();
                return _entityList;
            }
        }

        [HttpGet]
        public HttpResponseMessage DeleteEntity(int id)
        {
            string message = string.Empty;

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var getEntityToDelete = dbContext.tblMasterEntities.Where(e => e.EntityID == id).SingleOrDefault();
                    if (getEntityToDelete != null)
                    {
                        getEntityToDelete.IsDelete = true;
                        dbContext.SaveChanges();
                    }
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Data deleted successfully." });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message });
            }
        }

        [HttpPost]
        public HttpResponseMessage AddEditEntity(EntityMasterModel objModel)
        {
            try
            {
                EntityMasterController objContrEntity = new EntityMasterController();
                string message = string.Empty;
                string chainmessage = string.Empty;

                using (var dbContext = new dbRVNLMISEntities())
                {
                    var isNameEnteredExist = dbContext.tblMasterEntities.Where(e => e.EntityName == objModel.EntityName && e.IsDelete == false).FirstOrDefault();
                    var SectionDetail = dbContext.tblSections.Where(a => a.SectionID == objModel.SectionID).SingleOrDefault();
                    if (SectionDetail == null)
                    {
                        message = "Invalid Section";
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
                    }
                    chainmessage = CheckChainageStatus(objModel, SectionDetail);
                    if (chainmessage != "OK")
                    {
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message= chainmessage });
                    }
                    if (objModel.EntityID == 0)   //add operation
                    {
                        if (isNameEnteredExist != null)
                        {
                            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Entity Name already exists." });
                        }

                        tblMasterEntity objAdd = new tblMasterEntity();
                        objAdd.EntityCode = objContrEntity.CreateEntityCode();
                        objAdd.EntityName = objModel.EntityName;
                        objAdd.PackageId = objModel.PackageId;
                        objAdd.ProjectId = objModel.ProjectId;
                        objAdd.SectionID = objModel.SectionID;
                        objAdd.EntityType = objModel.EntityTypeName;
                        objAdd.Lat = objModel.Lat;
                        objAdd.Long = objModel.Long;
                        objAdd.StartChainage = objModel.StartChainage;
                        objAdd.EndChainage = objModel.EndChainage;
                        objAdd.IsDelete = false;
                        objAdd.CreatedOn = DateTime.Now;
                        objAdd.CreatedBy = objModel.UserId;
                        objAdd.ModifiedOn = DateTime.Now;
                        objAdd.ModifiedBy = objModel.UserId;
                        dbContext.tblMasterEntities.Add(objAdd);
                        message = "Data added successfully.";
                    }
                    else                      //edit operation
                    {
                        var objEdit = dbContext.tblMasterEntities.Where(e => e.EntityID == objModel.EntityID && e.IsDelete == false).SingleOrDefault();

                        if (objEdit.EntityName != objModel.EntityName)
                        {
                            if (isNameEnteredExist != null)
                            {
                                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Entity Name already exists." });
                            }
                        }

                        objEdit.EntityName = objModel.EntityName;
                        objEdit.PackageId = objModel.PackageId;
                        objEdit.ProjectId = objModel.ProjectId;
                        objEdit.SectionID = objModel.SectionID;
                        objEdit.EntityType = objModel.EntityTypeName;
                        objEdit.Lat = objModel.Lat;
                        objEdit.Long = objModel.Long;
                        objEdit.StartChainage = objModel.StartChainage;
                        objEdit.EndChainage = objModel.EndChainage;
                        objEdit.ModifiedOn = DateTime.Now;
                        objEdit.ModifiedBy = objModel.UserId;
                        message = "Data updated successfully.";
                    }
                    dbContext.SaveChanges();
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message });
            }
        }

        private string CheckChainageStatus(EntityMasterModel oModel, tblSection secObj)
        {
            int StartPC = Functions.RepalceCharacter(secObj.StartChainage);
            int EndPC = Functions.RepalceCharacter(secObj.EndChainage);
            string _Status = string.Empty;
            int oStartC = Functions.RepalceCharacter(oModel.StartChainage);
            int oEndC = Functions.RepalceCharacter(oModel.EndChainage);

            //if (oEndC != 0 && oStartC != 0)
            //{
                if ((oStartC >= StartPC && oStartC <= EndPC) && (oEndC >= StartPC && oEndC <= EndPC))
                {
                    _Status = "OK";
                }
                else
                {
                    _Status = "Invalid Chainage, Please enter start and end Chainage within Selected Section Chainage range.";
                }
            //}
            return _Status;
        }

        public HttpResponseMessage GetEntityCode()
        {
            string code = string.Empty;
            EntityMasterController objContrEntity = new EntityMasterController();

            try
            {
                code = objContrEntity.CreateEntityCode();
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { code });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.InternalServerError, "Error occurred!");
            }
        }
    }
}