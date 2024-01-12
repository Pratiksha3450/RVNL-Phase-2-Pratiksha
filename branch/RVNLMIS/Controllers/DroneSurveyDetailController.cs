using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class DroneSurveyDetailController : Controller
    {
        // GET: DroneSurveyDetail

        public ActionResult Index()
        {
            //GetImageVideoList();
            return View();
        }

        [HttpPost]
        public JsonResult GetImageAndVideoListById(int? sid, int? eid, int? filetype)
        {
            using (var _context = new dbRVNLMISEntities())
            {
                int _eid = (int)(eid == null ? 0 : eid);
                if (sid != 0 && _eid == 0)
                {
                    var resultSection = (from obj in _context.tblDroneImageVideos.AsEnumerable().Where(o => o.DFileType == filetype && o.DSId == _context.tblDroneSurveyDetails.Where(c => c.SectionID == sid).Select(r => r.DSId).FirstOrDefault())
                                         select new DroneImageVideoModel
                                         {
                                             DSId = obj.DSId,
                                             FileID = obj.FileID,
                                             DFileType = (int)obj.DFileType,
                                             DFileName = (obj.DFileType != 0) ? "/DroneVideo/" + obj.DFileName : "/DroneImage/" + obj.DFileName,
                                         }).OrderByDescending(o => o.FileID).ToList();

                    return Json(resultSection);
                }
                else if (sid != 0 && eid != 0)
                {
                    var resultE = (from obj in _context.tblDroneImageVideos.AsEnumerable().Where(o => o.DFileType == filetype && o.DSId == _context.tblDroneSurveyDetails.Where(c => c.SectionID == sid && c.EntityID == _eid).Select(r => r.DSId).FirstOrDefault())
                                   select new DroneImageVideoModel
                                   {
                                       DSId = obj.DSId,
                                       FileID = obj.FileID,
                                       DFileType = (int)obj.DFileType,
                                       DFileName = (obj.DFileType != 0) ? "/DroneVideo/" + obj.DFileName : "/DroneImage/" + obj.DFileName,
                                   }).OrderByDescending(o => o.FileID).ToList();

                    return Json(resultE);
                }
                else
                {
                    var result = (from obj in _context.tblDroneImageVideos.AsEnumerable().Where(o => o.DFileType == filetype)
                                  select new DroneImageVideoModel
                                  {
                                      DSId = obj.DSId,
                                      FileID = obj.FileID,
                                      DFileType = (int)obj.DFileType,
                                      DFileName = (obj.DFileType != 0) ? "/DroneVideo/" + obj.DFileName : "/DroneImage/" + obj.DFileName,
                                  }).OrderByDescending(o => o.FileID).ToList();

                    return Json(result);
                }
            }
        }

        public JsonResult ServerFiltering_GetProducts(string text)
        {
            List<GetRoleAssignedPackageList_Result> sessionPackages = Functions.GetRoleAccessiblePackageList();
            if (!string.IsNullOrEmpty(text))
            {
                sessionPackages = sessionPackages.Where(p =>
               CultureInfo.CurrentCulture.CompareInfo.IndexOf
               (p.PackageName, text, CompareOptions.IgnoreCase) >= 0).ToList();
            }
            return Json(sessionPackages, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSectionsByPackage(int? id, string text)
        {
            List<SectionModel> _SectionList = new List<SectionModel>();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    _SectionList = (from e in dbContext.tblSections
                                    where (e.PackageId == id && e.IsDeleted == false)
                                    select new SectionModel
                                    {
                                        SectionId = e.SectionID,
                                        SectionName = e.SectionCode + " - " + e.SectionName
                                    }).ToList();

                    if (!string.IsNullOrEmpty(text))
                    {
                        _SectionList = _SectionList.Where(p =>
                       CultureInfo.CurrentCulture.CompareInfo.IndexOf
                       (p.SectionName, text, CompareOptions.IgnoreCase) >= 0).ToList();
                    }
                    return Json(_SectionList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(_SectionList, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetEntityBySection(int? sectionId)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var _EntityList = (from e in dbContext.tblMasterEntities
                                       where (e.SectionID == sectionId && e.IsDelete == false)
                                       select new drpEntityModel
                                       {
                                           EntityID = e.EntityID,
                                           EntityName = e.EntityCode + " - " + e.EntityName
                                       }).ToList();

                    return Json(_EntityList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult VerifyDetails()
        {
            string response = string.Empty;
            int fileType = 0;
            try
            {
                tblDroneSurveyDetail objDroneSurveyDetail = new tblDroneSurveyDetail();
                using (dbRVNLMISEntities _context = new dbRVNLMISEntities())
                {
                    DroneSModel objDroneSModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DroneSModel>(Request["objDroneImageform"]);
                    string fileUploadPath = (fileType != objDroneSModel.FilteType) ? "~/Uploads/Drone/DroneVideo" : "~/Uploads/Drone/DroneImage";
                    var isExists = _context.tblDroneSurveyDetails.Where(o => o.SectionID == objDroneSModel.SectionId && o.EntityID == objDroneSModel.EntityId).Select(c => c.DSId).FirstOrDefault();
                    if (isExists == 0)
                    {
                        objDroneSurveyDetail.PackageId = objDroneSModel.PackageId;
                        objDroneSurveyDetail.SectionID = objDroneSModel.SectionId;
                        objDroneSurveyDetail.EntityID = objDroneSModel.EntityId;
                        objDroneSurveyDetail.IsDeleted = false;
                        objDroneSurveyDetail.CreatedOn = DateTime.UtcNow;
                        _context.tblDroneSurveyDetails.Add(objDroneSurveyDetail);
                        _context.SaveChanges();
                        response = "Add successfully";
                    }

                    HttpFileCollectionBase files = Request.Files;
                    if (files.Count != 0)
                    {
                        if (Directory.Exists(Server.MapPath(fileUploadPath)) == false)
                        {
                            Directory.CreateDirectory(Server.MapPath(fileUploadPath));
                        }
                        for (int i = 0; i < files.Count; i++)
                        {
                            HttpPostedFileBase file = files[i];
                            string fname;
                            // Checking for Internet Explorer
                            if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                            {
                                string[] testfiles = file.FileName.Split(new char[] { '\\' });
                                fname = testfiles[testfiles.Length - 1];
                            }
                            else
                            {
                                fname = file.FileName;
                            }
                            string newFilename = Guid.NewGuid() + Path.GetExtension(fname);
                            // Get the complete folder path and store the file inside it.
                            fname = Path.Combine(Server.MapPath(fileUploadPath + "/"), newFilename);
                            file.SaveAs(fname);

                            tblDroneImageVideo fileObj = new tblDroneImageVideo()
                            {
                                DFileName = newFilename,
                                DFileType = objDroneSModel.FilteType,
                                DSId = (isExists == 0) ? objDroneSurveyDetail.DSId : isExists,
                                CreatedOn = DateTime.UtcNow
                            };
                            _context.tblDroneImageVideos.Add(fileObj);
                            _context.SaveChanges();
                        }
                        response = "Add successfully";
                    }
                    else if (files.Count == 0 && objDroneSModel.FilteType == 1)
                    {
                        tblDroneImageVideo fileObj = new tblDroneImageVideo()
                        {
                            DFileName = objDroneSModel.FileName,
                            DFileType = objDroneSModel.FilteType,
                            DSId = (isExists == 0) ? objDroneSurveyDetail.DSId : isExists,
                            CreatedOn = DateTime.UtcNow
                        };
                        _context.tblDroneImageVideos.Add(fileObj);
                        _context.SaveChanges();
                        response = "Add successfully";
                    }
                    else if (files.Count == 0 && objDroneSModel.FilteType == 0)

                    {
                        response = "file Empty";
                        return Json(response);
                    }
                }
                return Json(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                return Json(response);
                //throw;
            }
        }
    }
}