using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [Authorize]
    [Compress]
    public class ResourceController : Controller
    {
        // GET: Resource
        public ActionResult Index()
        {
            GetPackageList();
            return View();
        }

        #region -- Load Package List --
        public void GetPackageList()
        {
            using (dbRVNLMISEntities dbContext=new dbRVNLMISEntities())
            {
                var _PackageList = (from q in dbContext.tblPackages
                                    select new
                                    {
                                        q.PackageId,
                                        q.PackageName
                                    }).ToList();
                ViewBag.PackageList = new SelectList(_PackageList, "PackageId", "PackageName");
            }
        }
        #endregion

        #region --- List Resource Values ---
        public ActionResult Resource_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var lst = (from x in dbContext.tblResources
                               join sa in dbContext.tblPackages
                               on x.PackageId equals sa.PackageId
                               where x.IsDeleted == false
                               select new { x })
                             .AsEnumerable().Select(s =>
                              new ResourceModel
                              {
                                  ResourceId=s.x.ResourceId,
                                  PackageId=s.x.PackageId,
                                  PackageName=s.x.tblPackage.PackageName,
                                  ResourceName=s.x.ResourceName,
                                  ResourceUnit=s.x.ResourceUnit,
                                  CreatedOn=s.x.CreatedOn
                              }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region -- Add and Update Resource Details --
        [HttpPost]
        public ActionResult AddResourceDetails(ResourceModel oModel)
        {
            GetPackageList();
            int resourceId = oModel.ResourceId;
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    if (resourceId == 0)
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            var exist = db.tblResources.Where(u => u.ResourceName == oModel.ResourceName).SingleOrDefault();
                            if (exist != null)
                            {
                                message = "Already Exists";
                            }
                            else
                            {
                                tblResource objResource = new tblResource();
                                //objDiscipline.DispId = oModel.DisciplineId;
                                objResource.PackageId = oModel.PackageId;      // Add Package Dropdown
                                objResource.ResourceName = oModel.ResourceName;
                                objResource.ResourceUnit = oModel.ResourceUnit;
                                objResource.IsDeleted = false;
                                objResource.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                                db.tblResources.Add(objResource);
                                db.SaveChanges();
                                message = "Added Successfully";
                            }
                        }
                    }
                    else
                    {
                        using (var db = new dbRVNLMISEntities())
                        {
                            tblResource objResource = db.tblResources.Where(o => o.ResourceId == oModel.ResourceId).SingleOrDefault();
                            objResource.PackageId = oModel.PackageId;
                            objResource.ResourceName = oModel.ResourceName;
                            objResource.ResourceUnit = oModel.ResourceUnit;
                            objResource.IsDeleted = false;
                            objResource.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                            db.SaveChanges();
                            message = "Updated Successfully";
                        }
                    }
                    ModelState.Clear();
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_PartialEditResource", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion


        #region -- Edit Resource Details --
        public ActionResult EditResourceByResourceId(int id)
        {
            GetPackageList();
            int resourceId = id;
            ResourceModel objModel = new ResourceModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        //var oResourceDetails = db.tblResources.Where(o => o.ResourceId == id).SingleOrDefault();

                        var oResourceDetails = (from x in db.tblResources
                                   join sa in db.tblPackages
                                   on x.PackageId equals sa.PackageId
                                   where x.IsDeleted == false && x.ResourceId==id
                                   select new { x })
                             .AsEnumerable().Select(s =>
                              new ResourceModel
                              {
                                  ResourceId = s.x.ResourceId,
                                  PackageId = s.x.PackageId,
                                  PackageName = s.x.tblPackage.PackageName,
                                  ResourceName = s.x.ResourceName,
                                  ResourceUnit = s.x.ResourceUnit,
                                  CreatedOn = s.x.CreatedOn
                              }).SingleOrDefault();

                        if (oResourceDetails != null)
                        {
                            objModel.ResourceId = oResourceDetails.ResourceId;
                            objModel.PackageId = oResourceDetails.PackageId;
                            //objModel.PackageName = oResourceDetails.PackageName;
                            objModel.ResourceName = oResourceDetails.ResourceName;
                            objModel.ResourceUnit = oResourceDetails.ResourceUnit;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialEditResource", objModel);
        }
        #endregion


        #region -- Delete Resource Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblResource objDisp = db.tblResources.SingleOrDefault(o => o.ResourceId == id);
                    objDisp.IsDeleted = true;
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


    }
}