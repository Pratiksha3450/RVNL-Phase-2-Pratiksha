
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Common.ActionFilters
{
    public class PageAccessFilter : AuthorizeAttribute
    {
        //Core authentication, called before each action
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool result = false;
            var url = httpContext.Request.RawUrl;
            using (var db = new dbRVNLMISEntities())
            {
                if (string.IsNullOrEmpty(Convert.ToString(httpContext.Session["UserData"])))
                {
                    return false;
                }
                else
                {
                    int roleid = ((UserModel)(httpContext.Session["UserData"])).RoleId;
                    //int menuId = (int)(from x in db.tblAppMenus where x.URL.Contains(url.Trim()) select x.MenuId).FirstOrDefault();
                    int menuId = db.tblAppMenus.Where(m => (url.Contains(m.URL.Trim())|| m.URL.Contains(url.Trim())) && m.IsDeleted == false).Select(s => s.MenuId).FirstOrDefault();
                    int exists = db.tblRoleMenuAccesses.Where(r => r.RoleId == roleid && r.MenuId == menuId).Select(i => i.RoleMenuId).SingleOrDefault();
                    result = exists == 0 ? false : true;
                }
            }
            return result;
            // return (string.IsNullOrEmpty(Convert.ToString(httpContext.Session["UserData"]))) ? false : true;
        }
    }
}