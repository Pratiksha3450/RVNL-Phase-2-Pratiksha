using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class AllPackageUserController : Controller
    {
        // GET: AllPackageUser
        public ActionResult Index()
        {
            List<AllUsersModel> objUserList = new List<AllUsersModel>();
            try
            {
                string ConnectionString = GlobalVariables.ConnectionString;
                List<SqlParameter> paramlist = new List<SqlParameter>();

                paramlist.Add(new SqlParameter("@PackageId", 1));

                DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, System.Data.CommandType.StoredProcedure, "GetAllUserList", paramlist.ToArray());
                DataTable dt1 = dsDataset.Tables[0];
                objUserList = Common.DataTableHelper.DataTableToList<AllUsersModel>(dt1);
               
            }
            catch (Exception ex)
            {
                
            }
            return View(objUserList);
        }

        #region --- List All Users ---

        [HttpPost]
        public JsonResult UserListing(int id)
        {
            string str = string.Empty;
            try
            {
                string ConnectionString = GlobalVariables.ConnectionString;
                List<SqlParameter> paramlist = new List<SqlParameter>();

                paramlist.Add(new SqlParameter("@PackageId", id));

                DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, System.Data.CommandType.StoredProcedure, "GetAllUserList", paramlist.ToArray());
                DataTable dt1 = dsDataset.Tables[0];
                            

                if (dt1.Rows.Count > 0)
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        str += "<div class='col-xl-3 col-md-6'>";
                        str += "<div class='card user-card2'>";
                        str += "<div class='card-body text-center mt-2'>";
                        str += "<i class='feather icon-user text-c-blue d-block f-40'></i><h6 class='m-b-10 m-t-10'>" + dt1.Rows[i]["name"] + "</h6>";
                        str += "<a href='#!' class='text-c-green b-b-success'><span class='badge badge-light-info'>" + dt1.Rows[i]["Designation"] + "</span></a>";
                        str += "<div class='row justify-content-center m-t-10 b-t-default m-l-0 m-r-0'>";

                        str += "<div class='col m-t-15 b-r-default'>";
                        str += "<h6>" + dt1.Rows[i]["PackageName"] + "</h6></div></div></div>";

                        str += "<div class='badge badge-light-info btn-block'>" + dt1.Rows[i]["Organisation"] + "</div>";                       
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
      
        #endregion
    }
}