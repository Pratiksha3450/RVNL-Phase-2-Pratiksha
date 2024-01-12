using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.Models;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using RVNLMIS.Common.ActionFilters;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class RoleMenuAccessController : Controller
    {
        //[PageAccessFilter]
        public ActionResult Index()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var menus = dbContext.tblAppMenus.ToList();
                ViewBag.MenuList = new SelectList(menus, "MenuId", "MenuName");

                var roles = dbContext.tblRoles.ToList();
                ViewBag.RoleList = new SelectList(roles, "RoleId", "RoleName");
            }
            return View();
        }

        public ActionResult AddEditRoleAccess(RoleMenuAccessModel objRoleAccess, FormCollection fc)
        {
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                if (!ModelState.IsValid)
                {
                    var menus = db.tblAppMenus.ToList();
                    ViewBag.MenuList = new SelectList(menus, "MenuId", "MenuName");

                    var roles = db.tblRoles.ToList();
                    ViewBag.RoleList = new SelectList(roles, "RoleId", "RoleName");

                    return View("_AddEdit", objRoleAccess);
                }
                string geSelectdIds = fc["ddlMenus"];
                int roleID = Convert.ToInt32(fc["RoleID"]);

                string Message = string.Empty;
                try
                {
                    db.RoleMenuInsert(roleID, geSelectdIds, 1);
                    return Json("Added Successfully", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                }
            }
            return Json(1, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetSelectedMenuIds(string Role)
        {
            int roleId = Convert.ToInt32(Role);
            string MenuArray = string.Empty;
            using (var db = new dbRVNLMISEntities())
            {
                MenuArray = string.Join(",", db.tblRoleMenuAccesses.Where(x => x.RoleId == roleId).Select(a => a.MenuId.ToString()).ToArray());
            }
            return Json(MenuArray, JsonRequestBehavior.AllowGet);
        }


        #region --- ListRoleAccess Controller ---
        public ActionResult RoleMenu_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                var lst = (from rm in db.tblRoleMenuAccesses
                           join r in db.tblRoles on rm.RoleId equals r.RoleId
                           join m in db.tblAppMenus on rm.MenuId equals m.MenuId
                           select new { rm, m, r })
                            .AsEnumerable().Select(s =>
                               new RoleMenuAccessModel
                               {
                                   RoleMenuID = s.rm.RoleMenuId,
                                   role = s.r.RoleName,
                                   MenuName = s.m.MenuName
                               }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}