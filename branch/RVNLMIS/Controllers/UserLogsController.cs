using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;


namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class UserLogsController : Controller
    {
        // GET: UserLogs
        public string IpAddress = "";
        [PageAccessFilter]
        public ActionResult Index()
        {
            int UserID = ((UserModel)Session["UserData"]).UserId;
            

            string whereCond = string.Empty;
            UserModel objUserM = new UserModel();
            objUserM = (UserModel)Session["UserData"];
            string userId = Convert.ToString(objUserM.UserId);
            string fmDt = DateTime.Now.ToString("yyyy-MM-dd");
            string toDt = DateTime.Now.ToString("yyyy-MM-dd");
            List<UserLogsModel> objUserList = new List<UserLogsModel>();
            try
            {
                string ConnectionString = GlobalVariables.ConnectionString;
                List<SqlParameter> paramlist = new List<SqlParameter>();
                  paramlist.Add(new SqlParameter("@UserID", UserID));
                paramlist.Add(new SqlParameter("@FromDate", fmDt));
                paramlist.Add(new SqlParameter("@ToDate", toDt));

                DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, System.Data.CommandType.StoredProcedure, "GetPackageUserLogs", paramlist.ToArray());
                DataTable dt1 = dsDataset.Tables[0];
                objUserList = Common.DataTableHelper.DataTableToList<UserLogsModel>(dt1);

            }
            catch (Exception ex)
            {

            }
            return View(objUserList);

        }

        [HttpPost]
        public JsonResult GetLogsDatewise(FormCollection fc)
        {
            string fromDate = fc["fromDate"];
            string toDate = fc["toDate"];
            string str = string.Empty;
            string whereCond = string.Empty;
            UserModel objUserM = new UserModel();
            objUserM = (UserModel)Session["UserData"];
            string userId = Convert.ToString(objUserM.UserId);

            try
            {
                string ConnectionString = GlobalVariables.ConnectionString;
                List<SqlParameter> paramlist = new List<SqlParameter>();
                //if (!string.IsNullOrEmpty(userId) && fromDate != null && toDate != null)
                //{
                //    whereCond = " WHERE UserId=" + userId + " AND" +
                //                "(CAST(TimeAccessed AS DATE) >= CAST('" + fromDate + "' AS datetime) AND CAST(TimeAccessed AS DATE) <= CAST('" + toDate + "' AS datetime))  order by timeAccessed desc";
                //}

                paramlist.Add(new SqlParameter("@UserID", userId));
                paramlist.Add(new SqlParameter("@FromDate", fromDate));
                paramlist.Add(new SqlParameter("@ToDate", toDate));

                DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, System.Data.CommandType.StoredProcedure, "GetPackageUserLogs", paramlist.ToArray());
                DataTable dt1 = dsDataset.Tables[0];


                if (dt1.Rows.Count > 0)
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        str += "<div class='ticket-block'>";
                        str += "<div class='row'>";
                       
                        str += "<div class='col'>";
                        str += "<div class='card hd-body'>";
                        str += "<div class='row align-items-center'>";
                        str += "<div class='col-2 left-icon border-right'>";
                        str += "<div class='card-body'>";
                        str += "<b class='float-right ml-3 text-magenta'>" + dt1.Rows[i]["AccessedTime"] + "</b>";
                        str += "<b class='float-right'>" + dt1.Rows[i]["AccessedDate"] + "</b>";
                        str += "</div>";
                        str += "</div>";
                       


                        str += "<div class='col-2 border-right pr-0'>";
                        str += "<div class='card-body inner-center'>";
                        str += "<span class='text-right'>" + dt1.Rows[i]["IPAddress"] + "</span>";
                        str += "</div>";
                        str += "</div>";
                       
                        str += "<div class='col-3 border-right pr-0'>";
                        str += "<div class='card-body inner-center'>";
                        str += "<div class='ticket-type-icon private mt-1 mb-1'>";
                        str += "<i class='fas fa-window-restore mr-1 f-16 text-magenta'></i>";
                        str += "" + dt1.Rows[i]["FormName"] + "";
                        str += "</div>";
                        str += "</div>";
                        str += "</div>";
                        str += "<div class='col-1 border-right pr-0'>";
                        str += "<div class='card-body inner-center'>";
                        str += "<div class='ticket-type-icon private mt-1 mb-1'>";
                        str += "<i class='fas fa-user-cog mr-1 f-16 text-magenta'></i>";
                        str += "" + dt1.Rows[i]["ActionDone"] + "";
                        str += "</div>";
                        str += "</div>";
                        str += "</div>";

                        str += "<div class='col-4 border-right pr-0'>";
                        str += "<div class='card-body inner-center'>";
                        str += "<div class='ticket-type-icon private mt-1 mb-1'>";                      
                        str += "" + dt1.Rows[i]["UpdatedValue"] + "";
                        str += "</div>";
                        str += "</div>";
                        str += "</div>";

                        str += "</div>";
                        str += "</div>";
                        str += "</div>";
                        str += "</div>";
                        str += "</div>";

                    }
                }

            }
            catch (Exception ex)
            {
                return Json("Error occurred.", JsonRequestBehavior.AllowGet);
            }

            return Json(str, JsonRequestBehavior.AllowGet);
        }


    }
}