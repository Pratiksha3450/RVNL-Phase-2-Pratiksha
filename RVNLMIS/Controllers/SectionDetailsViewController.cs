using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class SectionDetailsViewController : Controller
    {
        // GET: SectionDetailsView
        //[PageAccessFilter]
        public ActionResult Index(int id)
        {
            var SectionId = id;
            var builder = new StringBuilder();
            SectionModel objSectionModel = new SectionModel();
            try
            {
                if (SectionId != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oSectionDetails = db.SectionViews.Where(o => o.SectionID == SectionId).SingleOrDefault();
                        if (oSectionDetails != null)
                        {
                            //objSectionModel.ProjectId = (oSectionDetails.ProjectId == null) ? 0 : (int)oSectionDetails.ProjectId; ;
                            objSectionModel.SectionId = oSectionDetails.SectionID;
                            objSectionModel.PackageId = (int)oSectionDetails.PackageId;
                            objSectionModel.PackageName = oSectionDetails.PackageName;
                            objSectionModel.ProjectName = oSectionDetails.ProjectName;
                            objSectionModel.SectionName = oSectionDetails.SectionCode + " - " + oSectionDetails.SectionName;
                            objSectionModel.SectionCode = oSectionDetails.SectionCode;
                            objSectionModel.SectionStart = Convert.ToDateTime(oSectionDetails.SectionStart);
                            objSectionModel.SectionStarts = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.SectionStart)) ? "" : Convert.ToDateTime(Convert.ToString(oSectionDetails.SectionStart)).ToString("yyyy-MM-dd");
                            objSectionModel.SectionFinish = Convert.ToDateTime(oSectionDetails.SectionFinish);
                            objSectionModel.SectionFinishs = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.SectionFinish)) ? "" : Convert.ToDateTime(Convert.ToString(oSectionDetails.SectionFinish)).ToString("yyyy-MM-dd");
                            objSectionModel.StartChaining = oSectionDetails.StartChainage;
                            objSectionModel.EndChaining = oSectionDetails.EndChainage;
                            objSectionModel.SectionLength = oSectionDetails.SectionLength;
                        }
                    }
                }
                ViewBag.SectionDetails = SectionDetails(SectionId);
                //return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
                return View(objSectionModel);
            }
            catch (Exception ex)
            {
                return Json("1");
            }
        }
        public ActionResult AddEditEntity(int id)
        {
            EntityMasterModel objModel = new EntityMasterModel();
            try
            {

                objModel.ModalHeader = "Entity View";
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    if (id != 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var oSectionDetails = db.GetEntityByEId(id).Where(o=>o.IsDelete==false).FirstOrDefault();
                            if (oSectionDetails != null)
                            {
                                //objSectionModel.ProjectId = (oSectionDetails.ProjectId == null) ? 0 : (int)oSectionDetails.ProjectId; ;

                                objModel.PackageName = oSectionDetails.PackageName;
                                objModel.ProjectName = oSectionDetails.ProjectName;
                                objModel.SectionName = oSectionDetails.SectionName;
                                objModel.EntityName = oSectionDetails.EntityCode + " - " + oSectionDetails.EntityName;
                                objModel.EntityTypeName = oSectionDetails.EntityType;
                                objModel.StartChainage = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.StartChainage)) ? "NA" : Convert.ToString(oSectionDetails.StartChainage);
                                objModel.EndChainage = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.EndChainage)) ? "NA" : Convert.ToString(oSectionDetails.EndChainage);
                                objModel.Lat = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.Lat)) ? "NA" : Convert.ToString(oSectionDetails.Lat);
                                objModel.Long = string.IsNullOrEmpty(Convert.ToString(oSectionDetails.Long)) ? "NA" : Convert.ToString(oSectionDetails.Long);
                                objModel.EntityID = oSectionDetails.EntityID;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                //throw;
            }
            return View("_PartialEntityView", objModel);
        }
        public string SectionDetails(int SectionId)
        {
            var builder = new StringBuilder();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    ///int PackageCount = dbContext.GetNullPackageFields(PackageId).Where(a => a.ErrorMsg != null).Count();
                    var lst = dbContext.tblMasterEntities.Where(a => a.SectionID == SectionId && a.IsDelete == false).ToList();
                    if (lst.Count() != 0)
                    {
                        foreach (var item in lst)
                        {
                            builder.AppendLine("<div class='align-middle m-b-10'>");
                            builder.AppendLine("<div class='d-inline-block'>");
                            builder.AppendLine("<a href='#!'><h6>" + item.EntityName + "</h6></a>");
                            builder.AppendLine("<p class='m-b-0 text-danger'>" + item.EntityType + "," + item.EntityCode + "</p>");
                            builder.AppendLine("<button  data-url='/SectionDetailsView/AddEditEntity/" + item.EntityID + "'  class='btnViewDetails status deactive btn btn-xs btn-info'><i class='fas fa-eye'></i></button></div></div>");



                            builder.AppendLine("<hr class='my- 2'>");
                        }
                    }
                    else
                    {
                        builder.AppendLine("<div class='align-middle m-b-10'>");
                        builder.AppendLine("<div class='d-inline-block'>");
                        builder.AppendLine("<a href='#!'><h6> No Data Found </h6></a>");
                        builder.AppendLine("<p class='m-b-0'> - </p>");
                    }
                    return builder.ToString();
                    // return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return "1";
                //return Json("1");
            }
        }
    }
}