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
    public class SectionApiController : ApiController
    {
        public HttpResponseMessage SectionDetails_Read(int packageId)
        {
            List<SectionModel> lst = new List<SectionModel>();

            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    lst = (from x in dbContext.SectionViews
                           where x.PackageId == packageId
                           select new SectionModel
                           {
                               ProjectId = (int)x.ProjectId,
                               ProjectName = x.ProjectName,
                               SectionId = x.SectionID,
                               PackageId = (int)x.PackageId,
                               PackageName = x.PackageName,
                               SectionName = x.SectionName,
                               SectionCode = x.SectionCode,
                               SectionStart = x.SectionStart,
                               SectionFinish = x.SectionFinish,
                               StartChaining = x.StartChainage,
                               EndChaining = x.EndChainage,
                               SectionLength = x.SectionLength,
                               Length = x.Length
                           }).OrderByDescending(o => o.SectionId).ToList();

                    return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.OK, new { lst });
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.OK, new { lst });
            }
        }

        public HttpResponseMessage GetSectionCode(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var package = db.tblPackages.Where(o => o.PackageId == id && o.IsDeleted == false).SingleOrDefault().PackageCode;
                    var section = package.Split('G')[1];
                    var count = db.tblSections.Where(o => o.PackageId == id).ToList();
                    if (count != null)
                    {
                        int newcode = count.Count() + 1;
                        section = "P" + section + "S" + newcode.ToString();
                    }
                    else
                    {
                        section = "P" + section + "S01";
                    }
                    return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.OK, new { section });
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request
                   .CreateResponse(HttpStatusCode.OK, "Error occurred!");
            }
        }

        [HttpPost]
        public HttpResponseMessage AddEditSection(SectionModel oModel)
        {
            try
            {
                string message = string.Empty;
                SectionController objSecContr = new SectionController();

                if (oModel.SectionId == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var pkgDetails = db.tblPackages.Where(a => a.PackageId == oModel.PackageId).SingleOrDefault();
                        if (pkgDetails==null)
                        {
                            message = "Invalid Package";
                            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
                        }

                        var exist = db.tblSections.Where(u => u.SectionName == oModel.SectionName && u.IsDeleted == false).SingleOrDefault();
                        if (exist != null)
                        {
                            message = "Section Name already exists";
                            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
                        }
                        else
                        {
                            message = CheckChainageStatus(oModel,pkgDetails);
                            if (message!="OK")
                            {
                                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
                            }
                            tblSection objSection = new tblSection();
                            objSection.ProjectId = oModel.ProjectId;
                            objSection.PackageId = oModel.PackageId;      // Add Package Dropdown
                            objSection.SectionName = oModel.SectionName;
                            objSection.SectionCode = oModel.SectionCode;
                            objSection.SectionStart = (oModel.SectionStarts == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionStarts);
                            objSection.SectionFinish = (oModel.SectionFinishs == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionFinishs);
                            objSection.StartChainage = oModel.StartChaining;
                            objSection.EndChainage = oModel.EndChaining;

                            objSection.IsDeleted = false;
                            objSection.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                            objSecContr.CalculateSectionLength(objSection);
                            db.tblSections.Add(objSection);
                            db.SaveChanges();
                            message = "Added Successfully";
                        }
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var exist = db.tblSections.Where(u => u.SectionName == oModel.SectionName && u.IsDeleted == false && u.SectionID != oModel.SectionId).ToList();
                        if (exist.Count != 0)
                        {
                            message = "Section Name already exists";
                        }
                        else
                        {
                            var pkgDetails = db.tblPackages.Where(a => a.PackageId == oModel.PackageId).SingleOrDefault();
                            message = CheckChainageStatus(oModel, pkgDetails);
                            if (message != "OK")
                            {
                                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
                            }
                            tblSection objSection = db.tblSections.Where(u => u.SectionID == oModel.SectionId).SingleOrDefault();
                            objSection.ProjectId = oModel.ProjectId;
                            objSection.PackageId = oModel.PackageId;      // Add Package Dropdown
                            objSection.SectionName = oModel.SectionName;
                            objSection.SectionCode = oModel.SectionCode;
                            //objSection.SectionStart = Convert.ToDateTime(oModel.SectionStarts);
                            objSection.SectionStart = (oModel.SectionStarts == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionStarts);
                            objSection.SectionFinish = (oModel.SectionFinishs == null) ? Convert.ToDateTime(DateTime.Now) : Convert.ToDateTime(oModel.SectionFinishs);
                            //objSection.SectionFinish = Convert.ToDateTime(oModel.SectionFinishs);
                            objSection.StartChainage = oModel.StartChaining;
                            objSection.EndChainage = oModel.EndChaining;
                            objSection.IsDeleted = false;
                            objSection.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                            objSecContr.CalculateSectionLength(objSection);
                            db.SaveChanges();
                            message = "Updated Successfully";
                        }
                    }
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new {message= ex.Message });
            }
        }

        private string CheckChainageStatus(SectionModel oModel, tblPackage pakg)
        {
            int StartPC = Functions.RepalceCharacter(pakg.StartChainage);
            int EndPC = Functions.RepalceCharacter(pakg.EndChainage);
            string _Status = string.Empty;
            int oStartC = Functions.RepalceCharacter(oModel.StartChaining);
            int oEndC = Functions.RepalceCharacter(oModel.EndChaining);

            //if (oEndC != 0 && oStartC != 0)
            //{
                if ((oStartC >= StartPC && oStartC <= EndPC) && (oEndC >= StartPC && oEndC <= EndPC))
                {
                    _Status = "OK";
                }
                else
                {
                    _Status = "Invalid Chainage, Please enter start and end Chainage within Selected Package Chainage range.";
                }
            //}
            return _Status;
        }

        [HttpGet]
        public HttpResponseMessage DeleteSection(int id)
        {
            string message = string.Empty;

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    if (dbContext.tblMasterEntities.Where(s => s.SectionID == id && s.IsDelete == false).ToList().Count() != 0)
                    {
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Entities are added to this section. You cannot delete this." });
                    }
                    else
                    {
                        var objTodelete = dbContext.tblSections.Where(d => d.SectionID == id).SingleOrDefault();
                        objTodelete.IsDeleted = true;
                        dbContext.SaveChanges();
                    }
                }

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Data deleted successfully." });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        public HttpResponseMessage EntityList_OnSectionView(int sectionId)
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
                           where x.IsDelete == false && s.SectionID == sectionId
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
                               StartChainage = x.StartChainage ?? "NA",
                               EndChainage = x.EndChainage ?? "NA",
                           }).ToList();
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lst });
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lst });
            }
        }
    }
}
