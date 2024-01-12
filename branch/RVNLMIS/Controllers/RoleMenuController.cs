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
    [SessionAuthorize]
    public class RoleMenuController : Controller
    {
        // GET: RoleMenu
        [PageAccessFilter]
        public ActionResult Index()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var roles = dbContext.tblRoles.ToList();
                ViewBag.RoleList = new SelectList(roles, "RoleId", "RoleName");

                var result = (from m in dbContext.tblAppMenus
                              where m.IsDeleted == false
                              select new { m }).AsEnumerable().Select(s => new MenuModel
                              {
                                  MenuID = s.m.MenuId,
                                  MenuParentID = s.m.ParrentId,
                                  MenuName = s.m.MenuName,
                                  MenuOrder = s.m.ParentOrder
                              }).ToList().OrderBy(o => o.MenuOrder);

                return View(result);
            }
        }

        [HttpPost]
        public ActionResult AddEditRoleAccess(int roleId, string selectedMenus)
        {
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                try
                {
                    db.RoleMenuInsert(roleId, selectedMenus, 1);
                    return Json("Added Successfully", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetPrevAssignedMenus(int id)
        {
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                try
                {
                    var selectedMenuList = db.tblRoleMenuAccesses.Where(r => r.RoleId == id).Select(s => s.MenuId).ToArray();
                    return Json(selectedMenuList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}