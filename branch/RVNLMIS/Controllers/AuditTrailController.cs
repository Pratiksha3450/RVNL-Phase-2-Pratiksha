using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RVNLMIS.Common;
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
    public class AuditTrailController : Controller
    {
        // GET: AuditTrail
        [PageAccessFilter]
        public ActionResult Index()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var users = dbContext.tblUserMasters.Where(u => u.IsDeleted == false).ToList();
                ViewBag.UserList = new SelectList(users, "UserId", "UserName");
                return View();
            }
        }



        #region --- List Details ---

        public ActionResult Audit_Details([DataSourceRequest]  DataSourceRequest request, string userId, DateTime fromdate, DateTime todate)
        {
            List<AuditModel> lstProc = new List<AuditModel>();
            string whereCond = string.Empty;
            string fmDt = fromdate.ToString("yyyy-MM-dd");
            string toDt = todate.ToString("yyyy-MM-dd");

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    if (!string.IsNullOrEmpty(userId) && fromdate != null && todate != null)
                    {
                        whereCond = " WHERE UserId=" + userId + " AND" +
                                    "(CAST(TimeAccessed AS DATE) >= CAST('" + fmDt + "' AS datetime) AND CAST(TimeAccessed AS DATE) <= CAST('" + toDt + "' AS datetime))";
                    }
                    else if (string.IsNullOrEmpty(userId) && fromdate != null && todate != null)
                    {
                        whereCond = " WHERE (CAST(TimeAccessed AS DATE) >= CAST('" + fmDt + "' AS datetime) AND CAST(TimeAccessed AS DATE) <= CAST('" + toDt + "' AS datetime))";
                    }

                    lstProc = dbContext.GetUserwiseAuditDetails(whereCond).Select(r => new AuditModel
                    {
                        AuditID = r.AuditID,
                        UserId = (int)r.UserId,
                        UserName = r.UserName,
                        IPAddress = r.IPAddress,
                        AreaAccessed = r.AreaAccessed,
                        TimeAccessed = r.TimeAccessed,
                        // GroupArea = SplitUrl(r.AreaAccessed)
                    }).ToList();

                  //  lstProc.ForEach(e => e.UserName = GetLocationFromIP(e.IPAddress));

                    return Json(lstProc.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(lstProc.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
        }

        private string SplitUrl(string url)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                result = url.Split('/')[1];
            }
            return result;
        }

        #endregion

        private static string GetLocationFromIP(string Ip)
        {
            var res = Functions.GetLocation(Ip);
            string concatLocation = string.Empty;
            var obj = JObject.Parse(res);
            string City = (string)obj["city"];
            string State = (string)obj["region"];
            string CountryCode = (string)obj["country"];
            string postal = (string)obj["postal"];
           return concatLocation = string.Concat(City, " ", State, " ", CountryCode, " ", postal);
        }
    }
}