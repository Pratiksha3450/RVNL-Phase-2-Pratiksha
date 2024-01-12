//using ClosedXML.Excel;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class PackageMaterialsController : Controller
    {
        public string IpAddress = "";
        // GET: PackageMaterials
        [PageAccessFilter]
        public ActionResult Index()
        {
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, " Procurement- Assign Material", "View", UserID, IpAddress, "NA");
            BindDropdown();
            return View();
        }
        private void BindDropdown()
        {
            try
            {
                using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                {
                    var Dis = db.Get_DicsiplineWithCode().ToList();
                    ViewBag.DispList = new SelectList(Dis, "DispID", "DispName");

                    List<GetRoleAssignedPackageListForMaterialPackage_Result> sessionProjects = Functions.GetRoleAssignedPackageListForMaterialPackage();
                    var pkgList = (from p in sessionProjects
                                   select new drpOptions
                                   {
                                       Category = p.ProjectName,
                                       Id = p.PackageId,
                                       Name = p.PackageName
                                   }).ToList().OrderBy(N => N.Category);
                    ViewBag.ProjectPackageList = new SelectList(pkgList, "Id", "Name", "Category", 0);
                    ViewBag.firstPackage = sessionProjects.FirstOrDefault().PackageId;



                    var packages = Functions.GetRoleAccessiblePackageList();
                    ViewBag.PackageList = new SelectList(packages, "PackageId", "PackageName");

                    string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                    int dispId = ((UserModel)Session["UserData"]).Discipline;

                    if (roleCode == "DISP")
                    {
                        var discipline = db.tblDisciplines.Where(d => d.DispId == dispId && d.IsDeleted == false).Select(x => new Discipline
                        {
                            DispId = x.DispId,
                            DispCode = x.DispCode + "-" + x.DispName
                        }).ToList();
                        ViewBag.DisciplineList = new SelectList(discipline, "DispId", "DispCode", discipline.First().DispId);
                    }
                    else
                    {
                        var discipline = db.tblDisciplines.Where(d => d.IsDeleted == false).Select(x => new Discipline
                        {
                            DispId = x.DispId,
                            DispCode = x.DispCode + "-" + x.DispName
                        }).ToList();
                        ViewBag.DisciplineList = new SelectList(discipline, "DispId", "DispCode");
                    }
                }
                Session["PkgCodeForAttachment"] = "";




            }
            catch (Exception ex)
            {

            }
        }

        public ActionResult GetImortModal()
        {
            List<GetRoleAssignedPackageListForMaterialPackage_Result> sessionProjects = Functions.GetRoleAssignedPackageListForMaterialPackage();
            var pkgList = (from p in sessionProjects
                           select new drpOptions
                           {
                               Category = p.ProjectName,
                               Id = p.PackageId,
                               Name = p.PackageName
                           }).ToList().OrderBy(N => N.Category);
            ViewBag.ProjectPackageList = new SelectList(pkgList, "Id", "Name", "Category", 0);
            //ViewBag.firstPackage = sessionProjects.FirstOrDefault().PackageId;
            return View("_PartialImportExcel");
        }

        //public FileResult ExportExcel(int packageId)
        //{
        //    try
        //    {
        //        using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
        //        {
        //            var lst1 = (from x in dbContext.PackageMaterialDetailsViews
        //                        where (x.PackageId == packageId)
        //                        select new PackageMaterialImportList
        //                        {
        //                            MaterialId = (int)x.MaterialId,
        //                            MaterialName = x.MaterialName,
        //                            OriginalQty = x.OriginalQty,
        //                            RatePerUnit = x.RatePerUnit,

        //                        }).OrderByDescending(o => o.MaterialId).ToList();

        //            var list = lst1.ToList();
        //            DataTable dt = Functions.ToDataTable(list);
        //            using (XLWorkbook wb = new XLWorkbook())
        //            {
        //                wb.Worksheets.Add(dt);
        //                using (MemoryStream stream = new MemoryStream())
        //                {
        //                    wb.SaveAs(stream);
        //                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MaterialList.xlsx");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}

        [HttpPost]
        public ActionResult Async_Save(HttpPostedFileBase files, string packageId)
        {
            // The Name of the Upload component is "files"

            string pathToExcelFile = string.Empty;
            List<string> data = new List<string>();
            if (files.ContentType == "application/vnd.ms-excel" || files.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                try
                {
                    string filename = files.FileName;
                    string targetpath = Server.MapPath("~/Uploads/ExcelImport");
                    files.SaveAs(targetpath + filename);
                    pathToExcelFile = targetpath + filename;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }
                    var adapter = new OleDbDataAdapter("SELECT * FROM [PackageMaterialImportList$]", connectionString);
                    var ds = new DataSet();
                    adapter.Fill(ds, "ExcelTable");
                    DataTable dtable = ds.Tables["ExcelTable"];

                    using (var db = new dbRVNLMISEntities())
                    {
                        foreach (DataRow row in dtable.Rows)
                        {
                            int PackageId = Functions.ParseInteger(packageId);

                            if (PackageId != 0)
                            {
                                int MaterialId = Convert.ToInt32(Convert.ToString(row[0]));
                                var objAvail = db.tblProcPkgMaterials.Where(o => o.PackageId == PackageId && o.MaterialId == MaterialId).FirstOrDefault();

                                if (objAvail != null)
                                {
                                    objAvail.OriginalQty = Functions.ParseDouble(Convert.ToString(row[2]));
                                    objAvail.RatePerUnit = Convert.ToDecimal(Convert.ToString(row[3]));
                                }
                                db.SaveChanges();
                                //GetProjectWBS();
                            }
                        }
                    }
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    ViewBag.Message = "Data Imported Successfully.";
                    return Content("");
                }
                catch (Exception ex)
                {
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    ViewBag.Message = "Data Error.";
                    return Json(ex.Message + " -- " + ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                ViewBag.Message = "Please select the file first to upload.";


            }
            return Json("0");

        }
        private static DataTable GetSchemaFromExcel(OleDbConnection excelOledbConnection)
        {
            return excelOledbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
        }
        public ActionResult Async_Remove(string[] fileNames)
        {
            // The parameter of the Remove action must be called "fileNames"

            if (fileNames != null)
            {
                foreach (var fullName in fileNames)
                {
                    var fileName = Path.GetFileName(fullName);
                    var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);

                    // TODO: Verify user permissions

                    if (System.IO.File.Exists(physicalPath))
                    {
                        // The files are not actually removed in this demo
                        // System.IO.File.Delete(physicalPath);
                    }
                }
            }

            // Return an empty string to signify success
            return Content("");
        }

        //[HttpPost]
        //public ActionResult Upload(HttpPostedFileBase file)
        //{

        //    try
        //    {
        //        var postedFile = file;

        //        string filePath = string.Empty;
        //        if (postedFile != null)
        //        {
        //            string path = Server.MapPath("~/Uploads/");
        //            if (!Directory.Exists(path))
        //            {
        //                Directory.CreateDirectory(path);
        //            }

        //            filePath = path + Path.GetFileName(postedFile.FileName);
        //            string extension = Path.GetExtension(postedFile.FileName);
        //            postedFile.SaveAs(filePath);

        //            string conString = string.Empty;
        //            switch (extension)
        //            {
        //                case ".xls": //Excel 97-03.
        //                    conString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;HDR=YES'";

        //                    break;
        //                case ".xlsx": //Excel 07 and above.
        //                    conString = "Provider = Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=YES'";
        //                    break;
        //            }

        //            DataTable dt = new DataTable();
        //            conString = string.Format(conString, filePath);

        //            using (OleDbConnection connExcel = new OleDbConnection(conString))
        //            {
        //                using (OleDbCommand cmdExcel = new OleDbCommand())
        //                {
        //                    using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
        //                    {
        //                        cmdExcel.Connection = connExcel;

        //                        //Get the name of First Sheet.
        //                        connExcel.Open();
        //                        DataTable dtExcelSchema;
        //                        dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
        //                        string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
        //                        connExcel.Close();

        //                        //Read Data from First Sheet.
        //                        connExcel.Open();
        //                        cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
        //                        odaExcel.SelectCommand = cmdExcel;
        //                        odaExcel.Fill(dt);
        //                        connExcel.Close();
        //                    }
        //                }
        //            }

        //            conString = ConfigurationManager.ConnectionStrings["Constring"].ConnectionString;
        //            using (SqlConnection con = new SqlConnection(conString))
        //            {
        //                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
        //                {
        //                    //Set the database table name.
        //                    sqlBulkCopy.DestinationTableName = "dbo.Customers";

        //                    //[OPTIONAL]: Map the Excel columns with that of the database table
        //                    sqlBulkCopy.ColumnMappings.Add("Id", "CustomerId");
        //                    sqlBulkCopy.ColumnMappings.Add("Name", "Name");
        //                    sqlBulkCopy.ColumnMappings.Add("Country", "Country");

        //                    con.Open();
        //                    sqlBulkCopy.WriteToServer(dt);
        //                    con.Close();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //    return Content(file.FileName.ToString(), Telerik.Web.Spreadsheet.MimeTypes.JSON);


        //}

        public ActionResult Material_Details([DataSourceRequest]  DataSourceRequest request, int? packageId, int? disp)
        {
            List<int> MatIDs = new List<int>();
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                var lst = (from x in dbContext.GetNotExistMaterialListByPackage(packageId)
                           select new PackageMaterialDetailsModel
                           {
                               DispID = (int)x.DispId,
                               MaterialId = x.MaterialId,
                               MaterialCode = x.MaterialCode,
                               MaterialName = x.MaterialCode + "-" + x.MaterialName
                           }).OrderBy(x => PadNumbers(x.MaterialCode)).ToList();

                string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                int dispId = ((UserModel)Session["UserData"]).Discipline;

                if (roleCode == "DISP")
                {
                    lst = lst.Where(w => w.DispID == dispId).ToList();
                }
                if (disp != null)
                {
                    lst = lst.Where(w => w.DispID == disp).ToList();
                }
                return Json(lst, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Material_Details_PackageId([DataSourceRequest]  DataSourceRequest request, int? packageId, int? disp)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var lst1 = (from x in dbContext.PackageMaterialDetailsViews
                                where (x.PackageId == packageId)
                                select new {x }).AsEnumerable().Select(s=> new PackageMaterialDetailsModel
                                {
                                    DispID = (int)s.x.DispId,
                                    PkgMatId = s.x.PkgMatId,
                                    MaterialId = (int)s.x.MaterialId,
                                    MaterialCode = s.x.MaterialCode,
                                    MaterialName = s.x.MaterialName,
                                    OriginalQty = s.x.OriginalQty,
                                    RatePerUnit = s.x.RatePerUnit,
                                    DisciplineName = s.x.DisciplineName,
                                    Unit = !string.IsNullOrEmpty(s.x.Unit) ? s.x.Unit : "-"
                                }).OrderBy(x => PadNumbers(x.MaterialCode)).ToList();

                    string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                    int dispId = ((UserModel)Session["UserData"]).Discipline;

                    if (roleCode == "DISP")
                    {
                        lst1 = lst1.Where(w => w.DispID == dispId).ToList();
                    }
                    if (disp != null)
                    {
                        lst1 = lst1.Where(w => w.DispID == disp).ToList();
                    }
                    return Json(lst1.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public static string PadNumbers(string input)
        {
            return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }

        public ActionResult Material_DetailsByPackageId([DataSourceRequest]  DataSourceRequest request, int? packageId, int? disp)
        {
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    var lst1 = (from x in dbContext.GetExistMaterialListByPackage(packageId)
                                select new PackageMaterialDetailsModel
                                {
                                    PkgMatId = x.PkgMatId,
                                    DispID = (int)x.DispId,
                                    MaterialId = (int)x.MaterialId,
                                    MaterialCode = x.MaterialCode,
                                    MaterialName = x.MaterialCode + "-" + x.MaterialName,

                                }).OrderBy(x => PadNumbers(x.MaterialCode)).ToList();

                    string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                    int dispId = ((UserModel)Session["UserData"]).Discipline;

                    if (roleCode == "DISP")
                    {
                        lst1 = lst1.Where(w => w.DispID == dispId).ToList();
                    }
                    if (disp != null)
                    {
                        lst1 = lst1.Where(w => w.DispID == disp).ToList();
                    }
                    return Json(lst1, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [Audit]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditingCustom_Update([DataSourceRequest] DataSourceRequest request,
             [Bind(Prefix = "models")]IEnumerable<PackageMaterialDetailsModel> objPackageMaterialDetails)
        {
            if (objPackageMaterialDetails != null)
            {
                foreach (var item in objPackageMaterialDetails)
                {
                    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                    {
                        var target = dbContext.tblProcPkgMaterials.Where(o => o.PkgMatId == item.PkgMatId).FirstOrDefault();
                        if (target != null)
                        {
                            tblProcPkgMaterial objProcPkgMaterial = dbContext.tblProcPkgMaterials.Where(o => o.PkgMatId == target.PkgMatId).FirstOrDefault();
                            objProcPkgMaterial.OriginalQty = item.OriginalQty;
                            objProcPkgMaterial.RatePerUnit = item.RatePerUnit;
                            objProcPkgMaterial.Unit = item.Unit;
                            dbContext.SaveChanges();
                        }
                    }
                }
            }
            return Json(objPackageMaterialDetails.ToDataSourceResult(request, ModelState));
        }


        [Audit]
        public ActionResult Create(List<PackageMaterialDetailsModel> selectedItems, int? packageId, int? disp)
        {
            try
            {
                IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(IpAddress))
                {
                    IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                int UserID = ((UserModel)Session["UserData"]).UserId;
                string Message = string.Empty;
                string joined = string.Empty;
                string roleCode = ((UserModel)Session["UserData"]).RoleCode;
                int dispId = ((UserModel)Session["UserData"]).Discipline;
                if (dispId == 0)
                {
                    dispId = (int)(disp == null ? 0 : disp);
                }
                using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                {
                    if (packageId == null)
                    {

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(selectedItems)))
                        {
                            joined = string.Join(",", selectedItems.Select(x => x.MaterialId.ToString()).ToArray());

                            if (dispId != 0)
                            {
                                db.SP_INSERT_MaterialIDWithPackage(Convert.ToInt32(packageId), joined, dispId);
                            }
                            else
                            {
                                db.SP_INSERT_MaterialIDWithPackage(Convert.ToInt32(packageId), joined, 0);
                            }

                            Message = "Successfully assigned materials to selected package.";

                            int k = Functions.SaveUserLog(pkgId, "Procurement-Assign Material", "Assign", UserID, IpAddress,"NA");
                        }
                        else
                        {
                            db.SP_UnAssignAllMaterialsWithPackageId(Convert.ToInt32(packageId), dispId);
                            Message = "Successfully Unassigned All materials to selected package.";
                            int k = Functions.SaveUserLog(pkgId, "Procurement-Assign Material", "Unassigned", UserID, IpAddress, "NA");
                        }
                    }
                   
                    return Json(Message, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("error occurred.", JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult CheckIsTransactionAdded(int pkgMatId)
        //{
        //    try
        //    {
        //        using (dbRVNLMISEntities db = new dbRVNLMISEntities())
        //        {
        //            var checktrans = db.tblPkgMatTransactions.Where(w => w.PkgMatId == pkgMatId).FirstOrDefault();

        //            if (checktrans != null)
        //            {
        //                return Json("1", JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return Json("2", JsonRequestBehavior.AllowGet);
        //}
    }

    internal class PackageMaterialImportList
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public double OriginalQty { get; set; }
        public decimal RatePerUnit { get; set; }
    }

    internal class Discipline
    {
        public string DispCode { get; set; }
        public int DispId { get; set; }
        public string DisciplineName { get; set; }
    }
}