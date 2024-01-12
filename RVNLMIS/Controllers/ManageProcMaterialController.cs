using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class ManageProcMaterialController : Controller
    {
        public string IpAddress = "";
        // GET: ManageProcMaterial
        [PageAccessFilter]
        public ActionResult Index()
        {
            BindDropdown();
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Manage Package Material", "View", UserID, IpAddress, "NA");
            return View();
        }
        #region -- Bind Dropdown list --
        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var DisciplinesList = dbContext.tblDisciplines.Where(o => o.IsDeleted == false).ToList();
                    ViewBag.DisciplinesList = new SelectList(DisciplinesList, "DispId", "DispName");
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region --- List ConsActivity Values ---
        public ActionResult ProcMaterial_Details([DataSourceRequest]  DataSourceRequest request, int? dID)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                var lst = dbContext.GetProcMaterial().Select(e => new Models.ProcMaterial
                {
                    DispId = (int)e.DispId,
                    MaterialName = e.MaterialName,
                    MaterialId = e.MaterialId,
                    MaterialCode = e.MaterialCode,
                    MaterialUnit = e.MaterialUnit,
                    DispName = e.DisciplineGroup

                }).OrderByDescending(o => o.MaterialId).ToList();

                if (dID != null)
                {
                    var lst1 = lst.Where(w => w.DispId == dID).ToList();
                    return Json(lst1.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region ---GetMaterialCode--
        public JsonResult GetMaterialCode(int id)
        {
            string AutoCode = string.Empty;
            int ProcMCount = 0;
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    //var package = db.tblDisciplines.Where(o => o.DispId == id && o.IsDeleted == false).Select(o => o.DispName).FirstOrDefault();
                    var DispName = db.tblDisciplines.Where(o => o.DispId == id && o.IsDeleted == false).Select(o => o.DispName).FirstOrDefault();
                    string result = string.Concat(Regex.Matches(DispName.ToString(), "[A-Z]").OfType<Match>().Select(match => match.Value));
                    ProcMCount = db.tblProcMaterials.Where(o => o.IsDelete == false && o.DispId == id).Count();
                    if (ProcMCount != 0)
                    {
                        AutoCode = result + "00" + (ProcMCount + 1);
                    }
                    else
                    {
                        AutoCode = result + "001";
                    }
                    return Json(AutoCode, new JsonRequestBehavior());
                }
            }
            catch (Exception ex)
            {

                return Json("0");
            }
        }
        #endregion

        #region -- Add ConsActivity Details --
        [HttpPost]
        public ActionResult AddProcMaterialDetails(ProcMaterial oModel)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        if (oModel.MaterialId == 0)
                        {
                            var exist = db.tblProcMaterials.Where(u => u.MaterialCode == oModel.MaterialCode && u.IsDelete==false).ToList();
                            if (exist.Count != 0)
                            {
                                message = "ProcMaterial Code already exists";
                            }
                            else if ((db.tblProcMaterials.Where(u => u.MaterialName == oModel.MaterialName && u.IsDelete == false).ToList().Count != 0))
                            {
                                message = "ProcMaterial Name already exists";
                            }
                            else
                            {
                                tblProcMaterial objProcMaterial = new tblProcMaterial();
                                objProcMaterial.DispId = oModel.DispId;
                                objProcMaterial.MaterialCode = oModel.MaterialCode;
                                objProcMaterial.MaterialName = oModel.MaterialName;
                                objProcMaterial.MaterialUnit = oModel.MaterialUnit;
                                objProcMaterial.IsDelete = false;
                                db.tblProcMaterials.Add(objProcMaterial);
                                db.SaveChanges();
                                message = "Added Successfully";
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                int k = Functions.SaveUserLog(pkgId, "Manage Package Material", "Save", UserID, IpAddress, "Material Code:" + oModel.MaterialCode);
                            }

                        }
                        else
                        {

                            var objProcMaterial = db.tblProcMaterials.Where(u => u.MaterialId == oModel.MaterialId).FirstOrDefault();/* db.tblProcMaterials.Where(u => (u.MaterialCode == oModel.MaterialCode) && (u.DispId != oModel.DispId)).ToList();*/
                            if (objProcMaterial != null)
                            {
                                if (objProcMaterial.MaterialCode != oModel.MaterialCode)
                                {
                                    message = "Materials Code already exists";
                                }
                                else if (objProcMaterial.MaterialName != oModel.MaterialName)
                                {
                                    if ((db.tblProcMaterials.Where(u => u.MaterialName == oModel.MaterialName && u.IsDelete == false).ToList().Count != 0))
                                    {
                                        message = "Materials Name already exists";
                                        return Json(message, JsonRequestBehavior.AllowGet);
                                    }

                                }


                                objProcMaterial.DispId = oModel.DispId;
                                objProcMaterial.MaterialCode = oModel.MaterialCode;
                                objProcMaterial.MaterialName = oModel.MaterialName;
                                objProcMaterial.MaterialUnit = oModel.MaterialUnit;
                                db.SaveChanges();
                                message = "Updated Successfully";
                                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (string.IsNullOrEmpty(IpAddress))
                                {
                                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                                }
                                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                                int UserID = ((UserModel)Session["UserData"]).UserId;
                                //int k = Functions.SaveUserLogs(pkgId, "Manage Package Material", "Update", UserID, IpAddress);

                                try
                                {
                                    string str = ""; ;
                                    var UpdatedValue = (from ul in db.UserLogAudits orderby ul.UserlogAuditId descending select ul.UpdatedValues).FirstOrDefault();
                                    if (UpdatedValue != null)
                                    {
                                        str = UpdatedValue;
                                        str = "Material Code: " + oModel.MaterialCode + "= " + str;
                                    }
                                    else
                                    {
                                        str = "NA";
                                    }

                                    int k = Functions.SaveUserLog(pkgId, "Manage Package Material", "Update", UserID, IpAddress, str);
                                }
                                catch (Exception ex)
                                {

                                }

                            }
                        }

                    }
                    ModelState.Clear();
                    // return View("Index", oModel);
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.Clear();
                    return View("_PartialAddEditProcMaterial", oModel);
                }
            }
            catch (Exception ex)
            {
                return View("Index", oModel);
            }
        }
        #endregion

        #region -- EDIT ConsActivity Details --
        public ActionResult EditbyProcMaterialID(int id)
        {
            BindDropdown();
            int MaterialId = id;
            ProcMaterial objModel = new ProcMaterial();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oProcMaterialsDetails = db.tblProcMaterials.Where(o => o.MaterialId == id).SingleOrDefault();
                        if (oProcMaterialsDetails != null)
                        {
                            objModel.DispId = (int)oProcMaterialsDetails.DispId;
                            objModel.MaterialId = (int)oProcMaterialsDetails.MaterialId;
                            objModel.MaterialCode = oProcMaterialsDetails.MaterialCode;
                            objModel.MaterialName = oProcMaterialsDetails.MaterialName;
                            objModel.MaterialUnit = oProcMaterialsDetails.MaterialUnit;


                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialAddEditProcMaterial", objModel);
        }
        #endregion

        #region -- Delete ED Details --
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblProcMaterial obj = db.tblProcMaterials.SingleOrDefault(o => o.MaterialId == id);
                    obj.IsDelete = true;
                    db.SaveChanges();
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    int k = Functions.SaveUserLog(pkgId, "Manage Package Material", "Delete", UserID, IpAddress, "Material Code:" + obj.MaterialCode);
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