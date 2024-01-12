using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class DocumentsController : Controller
    {       
        public string IpAddress = "";
        public ActionResult Index()
        {
           
            Session["Download"]= "No";
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            UserModel objUserM = new UserModel();
            objUserM = (UserModel)Session["UserData"];
            string userId = Convert.ToString(objUserM.UserId);
            string fmDt = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
           // string fmDt ="2020-06-01";
            string toDt = DateTime.Now.ToString("yyyy-MM-dd");
            DataTable dtAll = new DataTable();
            List<DocumentModel> objUserList = new List<DocumentModel>();
            try
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
              
                int UserID = ((UserModel)Session["UserData"]).UserId;
                int k = Functions.SaveUserLog(pkgId, "Documents", "View", UserID, IpAddress, "NA");

                string ConnectionString = GlobalVariables.ConnectionString;
                List<SqlParameter> paramlist = new List<SqlParameter>();              
                paramlist.Add(new SqlParameter("@UserID", userId));
                paramlist.Add(new SqlParameter("@FromDate", fmDt));
                paramlist.Add(new SqlParameter("@ToDate", toDt));

                DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, System.Data.CommandType.StoredProcedure, "GetAttachementDetails", paramlist.ToArray());
                DataTable dt1 = dsDataset.Tables[0]; // invoice
                DataTable dt2 = dsDataset.Tables[1]; // Eng Drawing
                DataTable dt3 = dsDataset.Tables[2]; // PMC Reports
                DataTable dt4 = dsDataset.Tables[3]; // Construction
                DataTable dt5 = dsDataset.Tables[4]; // Procurement

                dtAll = dt1.Copy();
                dtAll.Merge(dt2);
                dtAll.Merge(dt3);
                dtAll.Merge(dt4);
                dtAll.Merge(dt5);


                objUserList = Common.DataTableHelper.DataTableToList<DocumentModel>(dtAll);
                ViewBag.ListData = objUserList.Count;

            }
            catch (Exception ex)
            {

            }
            return View(objUserList);
        }

        [HttpPost]
        public JsonResult GetDocumentDetails(FormCollection fc)
        {
            DataTable dtAll = new DataTable();
            string fromDate = fc["fromDate"];
            string toDate = fc["toDate"];
            string str = string.Empty;
            string whereCond = string.Empty;
            UserModel objUserM = new UserModel();
            objUserM = (UserModel)Session["UserData"];
            string userId = Convert.ToString(objUserM.UserId);
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            try
            {
                string ConnectionString = GlobalVariables.ConnectionString;
                List<SqlParameter> paramlist = new List<SqlParameter>();
               
                paramlist.Add(new SqlParameter("@UserID", userId));
                paramlist.Add(new SqlParameter("@FromDate", fromDate));
                paramlist.Add(new SqlParameter("@ToDate", toDate));

                DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, System.Data.CommandType.StoredProcedure, "GetAttachementDetails", paramlist.ToArray());
                DataTable dt1 = dsDataset.Tables[0]; // invoice
                DataTable dt2 = dsDataset.Tables[1]; // Eng Drawing
                DataTable dt3 = dsDataset.Tables[2]; // PMC Reports
                DataTable dt4 = dsDataset.Tables[3]; // Construction
                DataTable dt5 = dsDataset.Tables[4]; // Procurement

                dtAll = dt1.Copy();
                dtAll.Merge(dt2);
                dtAll.Merge(dt3);
                dtAll.Merge(dt4);
                dtAll.Merge(dt5);


                if (dtAll.Rows.Count > 0)
                {
                    for (int i = 0; i < dtAll.Rows.Count; i++)
                    {
                        str += "<div class='ticket-block'>";
                        str += "<div class='row'>";                     

                        str += "<div class='col'>";
                        str += "<div class='card hd-body mb-2'>";
                        str += "<div class='row align-items-center'>";
                        str += "<div class='col-2 left-icon border-right'>";
                        str += "<div class='card-body'>";
                        str += "<b class='float-right ml-3 text-magenta'>" + dtAll.Rows[i]["DocTime"] + "</b>";
                        str += "<b class='float-right'>" + dtAll.Rows[i]["DocDate"] + "</b>";
                        str += "</div>";
                        str += "</div>";
                        str += "<div class='col-2 col-auto pr-0 left-icon border-right'>";
                        str += "<div class='card-body'>";
                        str += "<span class='text - right'>" + dtAll.Rows[i]["Name"] + "</span>";
                        str += "</div>";
                        str += "</div>";




                        str += "<div class='col-2 border-right pr-0'>";
                        str += "<div class='card-body inner-center'>";
                        str += "<span class='text-right'> " + dtAll.Rows[i]["DocumnetType"] + "</span>";
                        str += "</div>";
                        str += "</div>";
                
                        str += "<div class='col-5 border-right pr-0'>";
                        str += "<div class='card-body inner-center'>";
                        str += "<div class='ticket-type-icon private mt-1 mb-1'>";

                        if (dtAll.Rows[i]["Extention"].ToString() == ".pdf")
                        {
                            str += "<img src='/Content/images/pdfIcn.png'/>";
                        }
                        else if (dtAll.Rows[i]["Extention"].ToString() == ".xls"|| dtAll.Rows[i]["Extention"].ToString() == "xlsx")
                        {
                            str += "<img src='/Content/images/excelIcn.png'/>";
                        }
                        else if (dtAll.Rows[i]["Extention"].ToString() == ".jpg" || dtAll.Rows[i]["Extention"].ToString() == "jpeg" || dtAll.Rows[i]["Extention"].ToString() == ".gif" || dtAll.Rows[i]["Extention"].ToString() == ".png")
                        {
                            str += "<img src='/Content/images/imageIcn.png'/>";
                        }
                        else if (dtAll.Rows[i]["Extention"].ToString() == ".doc" || dtAll.Rows[i]["Extention"].ToString() == ".docx")
                        {
                            str += "<img src='/Content/images/wordIcn.png'/>";
                        }
                        else if (dtAll.Rows[i]["Extention"].ToString() == ".rar" || dtAll.Rows[i]["Extention"].ToString() == ".zip")
                        {
                            str += "<img src ='/Content/images/rarImg.png' />";
                        }

                        str += "<a href='" + dtAll.Rows[i]["FileFullName"] + "' target='_blank'  title='Click here to Download the file'>  " + dtAll.Rows[i]["FileName"] + "</a>";
                        str += "</div>";
                        str += "</div>";
                        str += "</div>";
                        str += "<div class='col-1 border-right pr-0'>";
                        str += "<div class='card-body inner-center'>";
                        str += "<div class='ticket-type-icon private mt-1 mb-1'>";
                        str += "";
                        str += "" + dtAll.Rows[i]["FileSize"] + "";
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



        public List<Models.FileInf> GetFile(DateTime fromDate, DateTime toDate )
        {
            string fd= fromDate.ToString("yyyy-MM-dd");
            string td = toDate.ToString("yyyy-MM-dd");
            DataTable dtAll2 = new DataTable();
            string ConnectionString = GlobalVariables.ConnectionString;            
            List<Models.FileInf> listFiles = new List<Models.FileInf>();           
            List<SqlParameter> paramlist = new List<SqlParameter>();
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            UserModel objUserM = new UserModel();
            objUserM = (UserModel)Session["UserData"];
            string userId = Convert.ToString(objUserM.UserId);

            paramlist.Add(new SqlParameter("@UserID", userId));
            paramlist.Add(new SqlParameter("@FromDate", fd));
            paramlist.Add(new SqlParameter("@ToDate", td));
          
            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, System.Data.CommandType.StoredProcedure, "GetAttachementDetailsZip", paramlist.ToArray());
            DataTable dt1 = dsDataset.Tables[0]; // invoice
            DataTable dt2 = dsDataset.Tables[1]; // Eng Drawing
            DataTable dt3 = dsDataset.Tables[2]; // PMC Reports
            DataTable dt4 = dsDataset.Tables[3]; // Construction
            DataTable dt5 = dsDataset.Tables[4]; // Procurement

            dtAll2 = dt1.Copy();
            dtAll2.Merge(dt2);
            dtAll2.Merge(dt3);
            dtAll2.Merge(dt4);
            dtAll2.Merge(dt5);


            int i = 0;
            foreach (DataRow row in dtAll2.Rows)// invoice
            {
               
                listFiles.Add(new Models.FileInf()
                {
                    FileId = i + 1,
                    FileName = row["FileName"].ToString(),                
                    FilePath = System.Web.HttpContext.Current.Server.MapPath(row["FilePath"].ToString()) + @"\" + row["FileName"].ToString()
                });
                i = i + 1;
            }
           
            return listFiles;
        }

        [HttpGet]
        public ActionResult DownloadFiles(string fromDate, string toDate, string mode)
        {
            //string fromDate = fc["fromDate"];
            //string toDate = fc["toDate"];


            string TodaysDt = DateTime.Now.ToString();
                TodaysDt = TodaysDt.Replace(' ', '_');
                TodaysDt = TodaysDt.Replace('.', '_');
                bool fileExist = false;
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                string ZipName = "PKG-" + pkgId + " Files-" + TodaysDt + ".zip";
                var filesCol = GetFile( Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate)).ToList();
                if (filesCol.Count >= 1)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                    try
                    {
                        using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            for (int i = 0; i < filesCol.Count; i++)
                            {
                                string fileName = filesCol[i].FilePath.ToString();
                                FileInfo fi = new FileInfo(fileName);
                                bool exists = fi.Exists; // check physical file are exists or not
                                if (exists == true)
                                {
                                    ziparchive.CreateEntryFromFile(filesCol[i].FilePath, filesCol[i].FileName);
                                    fileExist = true;
                                }
                            }
                        }
                        IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (string.IsNullOrEmpty(IpAddress))
                        {
                            IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                        }

                        int UserID = ((UserModel)Session["UserData"]).UserId;
                        if (Session["Download"].ToString() == "No")
                        {
                            int k = Functions.SaveUserLog(pkgId, "Documents", "Download", UserID, IpAddress, "NA" );
                            Session["Download"] = "Yes";
                        }
                       
                        

                    }
                    catch (Exception e)
                    {

                    }
                    if (fileExist == true)
                    {
                        if (mode == "action")
                            return Json(new { fromDate = fromDate, toDate = toDate }, JsonRequestBehavior.AllowGet);
                        else
                        { 
                           return File(memoryStream.ToArray(), "application/zip", ZipName);
                        }
                    }
                    else
                    {
                        return Json("No Files Found!", JsonRequestBehavior.AllowGet);
                    }
                   
                    }

                }
                else
                {
                    return Json("No Files Found!", JsonRequestBehavior.AllowGet);
                }

        }
    }
}