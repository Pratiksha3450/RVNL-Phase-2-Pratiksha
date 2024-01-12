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
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class MenuSubMenuController : Controller
    {
        // GET: MenuSubMenu
        [PageAccessFilter]
        public ActionResult Index()
        {
            ViewBag.MenuCode = CreateMenuCode();
            BindDropdown();
            return View();
        }

        private void BindDropdown()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var menulist = dbContext.tblAppMenus.Where(m => m.IsDeleted == false).ToList().OrderBy(o => o.MenuName);
                ViewBag.DrpMenuList = new SelectList(menulist, "MenuId", "MenuName");
            }
        }

        public ActionResult MenuSubmenu_Read([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var lst = dbContext.GetMenusList().Select(s => new MenuModel
                {
                    MenuID = s.MenuId,
                    MenuCode = s.MenuCode,
                    MenuName = s.MenuName,
                    Description = s.Description,
                    Url = s.URL,
                    Icon = s.Icon,
                    MenuOrder = s.ParentOrder,
                    ParentMenuName = s.ParentMenuName,
                }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditMenuById(int id)
        {
            BindDropdown();
            MenuModel objModelView = new MenuModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var oMenuDetails = db.tblAppMenus.Where(o => o.MenuId == id).FirstOrDefault();
                        if (oMenuDetails != null)
                        {
                            objModelView.MenuID = oMenuDetails.MenuId;
                            objModelView.MenuName = oMenuDetails.MenuName;
                            objModelView.MenuCode = oMenuDetails.MenuCode;
                            objModelView.MenuParentID = oMenuDetails.ParrentId;
                            objModelView.MenuOrder = oMenuDetails.ParentOrder;
                            objModelView.Description = oMenuDetails.Description;
                            objModelView.Url = oMenuDetails.URL;
                            objModelView.Icon = oMenuDetails.Icon;
                            objModelView.IsRFIMenu = oMenuDetails.IsRFI ?? false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_PartialAddEditMenu", objModelView);
        }

        [HttpPost]
        public ActionResult SubmitMenu(MenuModel objModel)
        {
            try
            {
                string message = string.Empty;
                if (!ModelState.IsValid)
                {
                    BindDropdown();
                    return View("_PartialAddEditMenu", objModel);
                }

                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var isNameEnteredExist = dbContext.tblAppMenus.Where(e => e.MenuName == objModel.MenuName && e.IsDeleted == false).FirstOrDefault();

                    if (objModel.MenuID == 0)   //add operation
                    {
                        if (isNameEnteredExist != null)
                        {
                            return Json(new { message = "0", Code = CreateMenuCode() }, JsonRequestBehavior.AllowGet);
                        }

                        tblAppMenu objAdd = new tblAppMenu();
                        objAdd.MenuCode = objModel.MenuCode;
                        objAdd.MenuName = objModel.MenuName;
                        objAdd.ParentOrder = objModel.MenuOrder == 0 ? null : objModel.MenuOrder;
                        objAdd.Description = objModel.Description;
                        objAdd.URL = objModel.Url;
                        objAdd.Icon = objModel.Icon;
                        objAdd.IsRFI = objModel.IsRFIMenu;
                        objAdd.ParrentId = objModel.MenuParentID ?? 0;

                        dbContext.tblAppMenus.Add(objAdd);
                        dbContext.SaveChanges();

                        #region Assigned menu to Super Admin when add new

                        tblRoleMenuAccess objMenu = new tblRoleMenuAccess();
                        objMenu.RoleId = 100;
                        objMenu.MenuId = objAdd.MenuId;
                        objMenu.CreatedBy = 1;
                        objMenu.CreatedOn = DateTime.Now;
                        dbContext.tblRoleMenuAccesses.Add(objMenu);
                        dbContext.SaveChanges();

                        #endregion

                        message = "Data added successfully.";
                    }
                    else                      //edit operation
                    {
                        var objEdit = dbContext.tblAppMenus.Where(e => e.MenuId == objModel.MenuID && e.IsDeleted == false).SingleOrDefault();

                        if (objEdit.MenuName != objModel.MenuName)
                        {
                            if (isNameEnteredExist != null)
                            {
                                return Json(new { message = "0", Code = CreateMenuCode() }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        objEdit.MenuName = objModel.MenuName;
                        objEdit.ParentOrder = objModel.MenuOrder == 0 ? null : objModel.MenuOrder;
                        objEdit.Description = objModel.Description;
                        objEdit.URL = objModel.Url;
                        objEdit.Icon = objModel.Icon;
                        objEdit.ParrentId = objModel.MenuParentID ?? 0;
                        objEdit.IsRFI = objModel.IsRFIMenu;
                        dbContext.SaveChanges();

                        message = "Data updated successfully.";
                    }

                }
                return Json(new { message, Code = CreateMenuCode() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {
                    tblAppMenu objMenu = db.tblAppMenus.FirstOrDefault(o => o.MenuId == id);
                    objMenu.IsDeleted = true;
                    db.SaveChanges();

                }
                return Json("1");
            }
            catch
            {
                return Json("-1");
            }
        }

        public string CreateMenuCode()
        {
            MenuModel objUser = new MenuModel();
            string ou = string.Empty;
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var lastMenuCode = db.GetNextPackageCode("tblAppMenus").ToList();
                    if (lastMenuCode == null)
                    {
                        objUser.MenuCode = "MENU01";
                    }
                    else
                    {
                        string get = lastMenuCode[0].Code.Substring(4); //label1.text=ATHCUS-100
                        string s = (Convert.ToInt32(get) + 1).ToString();
                        ou = "MENU0" + s;
                    }
                    return ou;
                }
            }
            catch (Exception ex)
            {
                return objUser.MenuCode;
            }
        }
    }
}