using ClosedXML.Excel;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [SessionAuthorize]
    public class PackageStatusController : Controller
    {
        //[PageAccessFilter]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PackageStatus_Details([DataSourceRequest]  DataSourceRequest request)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {

                List<PackageStatusModel> lst = (from x in dbContext.PackageStatusByCount()
                                                select new { x }).AsEnumerable().Select(s => new PackageStatusModel
                                                {
                                                    EDName = s.x.EDName,
                                                    CPMCode = s.x.CPMCode,
                                                    PackageCode = s.x.PackageCode,
                                                    PackageShortName = s.x.PackageShortName,
                                                    PackageName = s.x.PackageName,
                                                    SectionCount = s.x.SectionCount,
                                                    EntityCount = s.x.EntityCount,
                                                    EnggDwgCount = s.x.EnggDwgCount,
                                                    MaterialAssignedCount = s.x.MaterialAssignedCount,
                                                    MaterialDataUpdatedCount = s.x.MaterialDataUpdatedCount,
                                                    ConActCount = s.x.ConActCount,
                                                    ConActDataUpdateCount = s.x.ConActDataUpdateCount,
                                                    InvoiceCount = s.x.InvoiceCount,
                                                    Points = s.x.Points
                                                }).ToList();

                return Json(lst.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        public string DownloadExcel()
        {
            string filename = string.Empty;
            using (XLWorkbook xlWorkBook = new XLWorkbook())
            {
                IXLWorksheet xlWorkSheet = xlWorkBook.Worksheets.Add("PackageStatus_" + DateTime.Now.ToString("dd_MMMM_yyyy"));
                List<PackageStatusModel> lst = new List<PackageStatusModel>();
                using (var db = new dbRVNLMISEntities())
                {
                    using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                    {
                        lst = (from x in dbContext.PackageStatusByCount()
                               select new { x }).AsEnumerable().Select(s => new PackageStatusModel
                               {
                                   EDName = s.x.EDName,
                                   CPMCode = s.x.CPMCode,
                                   PackageCode = s.x.PackageCode,
                                   PackageShortName = s.x.PackageShortName,
                                   PackageName = s.x.PackageName,
                                   SectionCount = s.x.SectionCount,
                                   EntityCount = s.x.EntityCount,
                                   EnggDwgCount = s.x.EnggDwgCount,
                                   MaterialAssignedCount = s.x.MaterialAssignedCount,
                                   MaterialDataUpdatedCount = s.x.MaterialDataUpdatedCount,
                                   ConActCount = s.x.ConActCount,
                                   ConActDataUpdateCount = s.x.ConActDataUpdateCount,
                                   InvoiceCount = s.x.InvoiceCount,
                                   Points = s.x.Points
                               }).ToList();
                    }
                }
               
                xlWorkSheet.Cell(1, 6).Value = "Package Details";

                xlWorkSheet.Cell(1, 8).Value = "Engineering";

                xlWorkSheet.Cell(1, 9).Value = "Procurement";

                xlWorkSheet.Cell(1, 11).Value = "Construction";

                xlWorkSheet.Cell(1, 13).Value = "Invoice";

                xlWorkSheet.Cell(2, 1).Value = "EDName";

                xlWorkSheet.Cell(2, 2).Value = "CPMCode";

                xlWorkSheet.Cell(2, 3).Value = "PackageCode";

                xlWorkSheet.Cell(2, 4).Value = "PackageShortName";

                xlWorkSheet.Cell(2, 5).Value = "PackageName";

                xlWorkSheet.Cell(2, 6).Value = "SectionCount";

                xlWorkSheet.Cell(2, 7).Value = "EntityCount";

                xlWorkSheet.Cell(2, 8).Value = "EnggDwgCount";

                xlWorkSheet.Cell(2, 9).Value = "MaterialAssignedCount";

                xlWorkSheet.Cell(2, 10).Value = "MaterialDataUpdatedCount";

                xlWorkSheet.Cell(2, 11).Value = "ConActCount";

                xlWorkSheet.Cell(2, 12).Value = "ConActDataUpdateCount";

                xlWorkSheet.Cell(2, 13).Value = "InvoiceCount";

                xlWorkSheet.Cell(2, 14).Value = "Points";


                if (lst != null && lst.Count() > 0)
                {
                    //insert data to from second row on
                    xlWorkSheet.Cell(3, 1).InsertData(lst);
                    xlWorkSheet.Columns().AdjustToContents();
                }

                xlWorkSheet.Columns("4").Width = 25;
                xlWorkSheet.Columns("5").Width = 45;
                xlWorkSheet.Columns("6").Width = 10;
                xlWorkSheet.Columns("7").Width = 10;
                xlWorkSheet.Columns("8").Width = 12;
                xlWorkSheet.Columns("9").Width = 10;
                xlWorkSheet.Columns("10").Width = 10;
                xlWorkSheet.Columns("11").Width = 10;
                xlWorkSheet.Columns("12").Width = 10;
                xlWorkSheet.Columns("13").Width = 10;
                xlWorkSheet.Columns("14").Width = 8;

                xlWorkSheet.Range("A1:E1").Merge();
                xlWorkSheet.Range("F1:G1").Merge();
                xlWorkSheet.Range("I1:J1").Merge();
                xlWorkSheet.Range("K1:L1").Merge();

                xlWorkSheet.Row(2).Style.Alignment.WrapText = true;
                xlWorkSheet.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                xlWorkSheet.Cells().Style.Border.OutsideBorderColor = XLColor.Black;

                using (var memoryStream = new MemoryStream())
                {

                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    filename = "PrimaBi_PackageStatus_" + DateTime.Now.ToString("dd_MMM_yyyy_hhmmtt") + ".xlsx";
                    Response.AddHeader("content-disposition", "attachment;  filename=" + filename);
                    xlWorkBook.SaveAs(memoryStream);
                    //add your destination folder
                    Functions.CreateIfMissing(Server.MapPath("~/Uploads/TemporaryFiles"));
                    //FileStream fileStream = new FileStream(@"D:\Weekely_Report\" + filename, FileMode.Create, FileAccess.Write, FileShare.Write);
                    FileStream fileStream = new FileStream(Server.MapPath(Path.Combine("~/Uploads/TemporaryFiles/", filename)), FileMode.Create, FileAccess.Write, FileShare.Write);
                    memoryStream.WriteTo(fileStream);
                    fileStream.Close();
                    memoryStream.WriteTo(Response.OutputStream);
                    memoryStream.Close();
                    // memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
            return filename;
        }

        public ActionResult GetSendmailPartial()
        {
            try
            {
                return View("_SendReportMail");
            }
            catch
            {
                return View("_SendReportMail");
            }
        }

        public ActionResult SendPackageStatusMail(string txtMail)
        {
            string attachmentName = DownloadExcel();
            try
            {
                string localFilename = string.Empty;
                if (System.IO.File.Exists(Server.MapPath(Path.Combine("~/Uploads/TemporaryFiles/", attachmentName))))
                {
                    localFilename = System.Web.HttpContext.Current.Server.MapPath(Path.Combine("~/Uploads/TemporaryFiles/", attachmentName));
                }

                Attachment reportAttachment = new Attachment(localFilename, MediaTypeNames.Application.Octet);
                List<Attachment> attachment = new List<Attachment>();
                attachment.Add(reportAttachment);

                int rs = SendEmailWithReport(txtMail, attachment);

                Functions.DeleteFilesInFolder(Server.MapPath(Path.Combine("~/Uploads/TemporaryFiles/", attachmentName)), false);
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Functions.DeleteFilesInFolder(Server.MapPath(Path.Combine("~/Uploads/TemporaryFiles/", attachmentName)), false);
                return Json("0", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Sends the email to share report
        /// </summary>
        private int SendEmailWithReport(string emailString, List<Attachment> attachment)
        {
            Email objEmail = new Email();

            string msgBody = "Hello Sir,<br/><br/>" +
                             "Please find the attached PrimaBI - Package Data Update Status Report generated on " + DateTime.Now.ToString("dd MMMM yyyy hh:mm tt") + ".<br/><br/>" +
                             "Thanks and Regards,<br/>" +
                             "PrimaBI Support ";

            string msgSubject = "PrimaBI - Package Data Update Status Report";

            return objEmail.SendMail(emailString, null, null, attachment, msgSubject, msgBody, ConfigurationManager.AppSettings["SUPPORTFROM"]);
        }
    }
}