using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.DAC;
using RVNLMIS.Areas.RFI.Common;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using System.IO;
using RVNLMIS.Common;

namespace RVNLMIS.Areas.RFI.Controllers
{
    public class RFIDocumentController : Controller
    {
        #region ---- font collection ----        
        public Font times_Title = new Font(BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, false), 10, Font.UNDERLINE | Font.BOLD, BaseColor.BLACK);
        public Font times_7 = new Font(BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, false), 7, Font.NORMAL, BaseColor.BLACK);
        public Font calibri_7b = FontFactory.GetFont("Calibri", 7, Font.BOLD);
        public Font calibri_7 = FontFactory.GetFont("Calibri", 7, Font.NORMAL);
        public Font calibri_7e = FontFactory.GetFont("Calibri", 7, Font.NORMAL);
        public Font calibri_7White = FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.WHITE);
        public Font calibri_7_italic = FontFactory.GetFont("Arial", 7, Font.ITALIC);
        public Font calibri_9Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
        public Font calibri_9Bold_sub = FontFactory.GetFont("Arial", 7, Font.BOLD);
        public Font calibri_9Bold_sub1 = FontFactory.GetFont("Arial", 7, Font.BOLD);
        public Font calibriBold = FontFactory.GetFont("Calibri", 10, Font.BOLD);
        public Font calibriTitle = FontFactory.GetFont("Calibri", 10, Font.UNDERLINE | Font.BOLD);
        public Font arialTitle = FontFactory.GetFont("Arial", 5, Font.BOLD);
        public Font arialTitle1 = FontFactory.GetFont("Arial", 7, Font.BOLD);
        #endregion

        // GET: RFI/RFIDocument
        public ActionResult Index(int? id)
        {
            id = id == null ? 10138 : id;
            //GetDocument((int)id);
            GetOnePageDocument((int)id);
            return View();
        }

        public void GetOnePageDocument(int id)
        {
            #region ---- RFI details ----

            RFIDocumentModel objModel = new RFIDocumentModel();

            List<AttachDetails> objAttachDetail = new List<AttachDetails>();
            List<CommentList> objCommList = new List<CommentList>();
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                if (id != 0)
                {
                    objModel = db.ViewRFIPDFDetails.Where(r => r.RFIId == id)
                        .Select(s => new RFIDocumentModel
                        {
                            RFIId = s.RFIId,
                            PackageId = s.PkgId,
                            EndChainage = s.EndChainage,
                            StartChainage = s.StartChainge,
                            EntityId = s.Entity,
                            LayerNo = s.LayerNo,
                            LocationType = s.LocationType,
                            OtherWorkLocation = s.OtherWorkLocation,
                            RFIActivityId = s.RFIActivityId,
                            RFICode = s.RFICode,
                            RFIStatus = s.RFIStatus,
                            WorkgroupId = s.WorkGroupId,
                            WorkSide = s.WorkSide,
                            AssignToPMC = s.AssignedTo,
                            RFIOpenDate = s.RFIOpenDate,
                            InspDate = s.InspDate,
                            AssignToPMCName = s.AssignedToName,
                            InspStatus = s.InspStatus,
                            InspId = s.InspId,
                            ProjectName = s.Description,// Convert.ToString( s.ProjectName)+"- "+ Convert.ToString(s.Description),
                            Contractor = s.Contractor,
                            ActivityName = s.RFIActName,
                            BillNo = "",
                            BOQItemNo = s.BOQCode,
                            ClientName = s.Client,
                            DateOfSubmission = (DateTime)s.DateOfSubmission,
                            EntityName = s.EntityName,
                            Layer = ((int)s.LayerNo).ToString(),
                            PackageName = s.PackageName,
                            PMC = s.PMC,
                            PrevRFICode = s.PrevRFICode,
                            RFINo = s.RFICode,
                            WorkDescription = s.WorkDescription,
                            Workgroup = s.WorkGrName,
                            Requestedby = s.Requestedby,
                            Revision = s.Revision,
                            RFIRevId = s.RFIRevId,
                            AcceptedDate = (DateTime)s.AcceptedDate

                        })
                        .FirstOrDefault();

                    if (objModel.Revision != "0" && objModel.Revision != string.Empty && objModel.Revision != null)
                    {
                        try
                        {
                            objCommList = (from a in db.tblRFIRevisions
                                           join b in db.tblRFIUsers on a.AssignedTo equals b.RFIUserId
                                           join c in db.tblRFIDesignations on b.DesignationId equals c.RFIDesignId
                                           where a.RFIId == objModel.RFIId //&& a.RFIRevId!=objModel.RFIRevId
                                           select new { a, b, c })
                                     .AsEnumerable()
                                     .Select(o =>
                                     new CommentList
                                     {
                                         Status = o.a.InspStatus,
                                         Comment = o.a.Note == null ? "" : o.a.Note,
                                         CommentedBy = o.b.FullName,
                                         Designation = o.c.Designation
                                        ,
                                         CommentDate = o.a.InspDate == null ? "" : ((DateTime)o.a.InspDate).ToString("dd-MMM-yyyy hh:mm tt")
                                     }).ToList();
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                    objAttachDetail = db.tblRFIEnclosures.Join(db.tblEnclosures, re => re.EnclId, e => e.EnclId, (re, e)
                           => new { re, e }).Where(r => r.re.RFIId == id).AsEnumerable()
                     .Select(s => new AttachDetails
                     {
                         RFIEnclId = s.re.RFIEnclId,
                         EnclType = s.e.EnclName + " (" + s.re.EnclAttach.Split('.')[0] + ")",
                         FileName = s.re.EnclAttach,
                         Path = s.re.EnclAttachPath
                     }).ToList();

                }
            }

            if (objModel != null)
            {
                objModel.WorkLocation = objModel.LocationType == "Other" ?
                                                objModel.OtherWorkLocation :
                                                objModel.LocationType == "Entity" ?
                                                objModel.EntityName :
                                                (objModel.StartChainage == null || objModel.EndChainage == null ? "" : "From " + IntToStringChainage(Convert.ToInt32((double)objModel.StartChainage)) + " To " + IntToStringChainage(Convert.ToInt32((double)objModel.EndChainage)));
                string fileName = objModel.RFICode != string.Empty ? objModel.RFICode.Replace("/", "-").Replace("--", "-") + ".pdf" : DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                #endregion

                DownloadPDF(objModel, objAttachDetail, objCommList, fileName);
                //SaveRfiPDF(objModel, objAttachDetail, objCommList, fileName);

            }
        }

        private void DownloadPDF(RFIDocumentModel objModel, List<AttachDetails> objAttachDetail, List<CommentList> objCommList, string fileName)
        {
            RFIPdfHelper objPdfController = new RFIPdfHelper();
            using (Document pdfDoc = new Document(PageSize.A4, Utilities.MillimetersToPoints(-15), Utilities.MillimetersToPoints(-15), Utilities.MillimetersToPoints(10), Utilities.MillimetersToPoints(0)))
            {
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();

                #region --- Page 1 ----
                PdfPTable mastertableP1 = new PdfPTable(1); // Main master table page 1

                mastertableP1.AddCell(objPdfController.GetSinglePageCell(objModel, objAttachDetail, objCommList)); //---- Header Section ---- 

                pdfDoc.Add(mastertableP1); // add master table to the pdf document
                #endregion --- Page 1 ----

                pdfDoc.Close();

                #region ---- download pdf ----
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Write(pdfDoc);
                Response.End();
                #endregion ---- download pdf ----
            }
        }

        public string SaveRfiPDF(RFIDocumentModel objModel, List<AttachDetails> objAttachDetail, List<CommentList> objCommList, string fileName)
        {
            RFIPdfHelper objPdfController = new RFIPdfHelper();
            using (Document pdfDoc = new Document(PageSize.A4, Utilities.MillimetersToPoints(-15), Utilities.MillimetersToPoints(-15), Utilities.MillimetersToPoints(10), Utilities.MillimetersToPoints(0)))
            {
                Functions.CreateIfMissing(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/RFIDocuments"));
                string path = System.Web.HttpContext.Current.Server.MapPath(Path.Combine("~/Uploads/RFIDocuments/", fileName));
                //PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, new FileStream(path, FileMode.Create));
                pdfDoc.Open();

                #region --- Page 1 ----
                PdfPTable mastertableP1 = new PdfPTable(1); // Main master table page 1

                mastertableP1.AddCell(objPdfController.GetSinglePageCell(objModel, objAttachDetail, objCommList)); //---- Header Section ---- 

                pdfDoc.Add(mastertableP1); // add master table to the pdf document
                #endregion --- Page 1 ----

                pdfDoc.Close();

                return string.Concat("/Uploads/RFIDocuments/", fileName);
            }
        }

        public string IntToStringChainage(int chainage)
        {
            return chainage.ToString("D6").Insert(chainage.ToString("D6").Length - 3, "+");
        }

        #region ---- old method -----
        private void GetDocument(int id)
        {
            #region ---- RFI details ----

            RFIDocumentModel objModel = new RFIDocumentModel();
            RFIDocCommonHeader objHeader = new RFIDocCommonHeader();
            RFIPdfHelper objPdfController = new RFIPdfHelper();
            using (dbRVNLMISEntities db = new dbRVNLMISEntities())
            {
                if (id != 0)
                {
                    objModel = db.ViewRFIMainDetails.Where(r => r.RFIId == id)
                        .Select(s => new RFIDocumentModel
                        {
                            RFIId = s.RFIId,
                            PackageId = s.PkgId,
                            EndChainage = s.EndChainage,
                            StartChainage = s.StartChainge,
                            EntityId = s.Entity,
                            LayerNo = s.LayerNo,
                            LocationType = s.LocationType,
                            OtherWorkLocation = s.OtherWorkLocation,
                            RFIActivityId = s.RFIActivityId,
                            RFICode = s.RFICode,
                            RFIStatus = s.RFIStatus,
                            WorkgroupId = s.WorkGroupId,
                            WorkSide = s.WorkSide,
                            AssignToPMC = s.AssignedTo,
                            RFIOpenDate = s.RFIOpenDate,
                            InspDate = s.InspDate,
                            AssignToPMCName = s.AssignedToName,
                            InspStatus = s.InspStatus

                        })
                        .FirstOrDefault();
                    int packageId = objModel.PackageId;
                    var obj = (from a in db.tblPackages
                               join b in db.tblMasterProjects on a.ProjectId equals b.ProjectId
                               where a.PackageId == packageId && a.IsDeleted == false
                               select new { a, b }).SingleOrDefault();
                    var BOQCOde = (from a in db.tblRFIActivityBOQs
                                   join b in db.tblBOQMasters on a.RFIBOQId equals b.BoqID
                                   where a.RFIActId == objModel.RFIActivityId
                                   select new { b }).SingleOrDefault();

                    objHeader.ProjectName = obj.a.Description;
                    objHeader.ClientName = obj.a.Client;
                    objHeader.PMC = obj.a.PMC;
                    objHeader.Contractor = obj.a.Contractor;
                    objHeader.BillNo = "1";
                    objHeader.BOQItemNo = BOQCOde == null ? "" : Convert.ToString(BOQCOde.b.BoqCode);
                    objHeader.DateOfSubmission = ((DateTime)objModel.RFIOpenDate).ToString("dd/MM/yyyy");
                    objHeader.RFINo = objModel.RFICode;
                    objModel.Contractor = obj.a.Contractor;
                }
            }
            string fileName = objModel.RFICode != string.Empty ? objModel.RFICode.Replace("/", "-") + ".pdf" : DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            #endregion

            using (Document pdfDoc = new Document(PageSize.A4, Utilities.MillimetersToPoints(-15), Utilities.MillimetersToPoints(-15), Utilities.MillimetersToPoints(10), Utilities.MillimetersToPoints(0)))
            {
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();

                #region --- Page 1 ----
                PdfPTable mastertableP1 = new PdfPTable(1); // Main master table page 1

                mastertableP1.AddCell(objPdfController.GetHeaderCell(objModel, objHeader)); //---- Header Section ---- 
                mastertableP1.AddCell(objPdfController.SheetOneCell(objModel)); // ---- SH1  table1 ----              
                mastertableP1.AddCell(objPdfController.GetCommonInstructionCell()); //---- common instruction ----
                mastertableP1.AddCell(objPdfController.GetInspectionDescCell(objModel)); //----inspection description ----
                mastertableP1.AddCell(objPdfController.GetFooterCell(objModel)); //---- Common footer ---- 

                pdfDoc.Add(mastertableP1); // add master table to the pdf document
                #endregion --- Page 1 ----

                pdfDoc.NewPage();

                #region ---- Page 2 ----
                PdfPTable mastertableP2 = new PdfPTable(1); // Main master table page 1

                mastertableP2.AddCell(objPdfController.GetHeaderCell(objModel, objHeader)); //---- Header Section ---- 
                mastertableP2.AddCell(objPdfController.EarthworkCell(objModel)); // ---- SH1  table1 ----              
                                                                                 // mastertableP2.AddCell(objPdfController.GetCommonInstructionCell()); //---- common instruction ----
                mastertableP2.AddCell(objPdfController.GetInspectionDescCell(objModel)); //----inspection description ----
                mastertableP2.AddCell(objPdfController.GetFooterCell(objModel)); //---- Common footer ---- 

                pdfDoc.Add(mastertableP2); // add master table to the pdf document
                #endregion ---- Page 2 ----

                pdfDoc.NewPage();

                #region ---- Page 3 ----
                PdfPTable mastertableP3 = new PdfPTable(1); // Main master table page 1

                mastertableP3.AddCell(objPdfController.GetHeaderCell(objModel, objHeader)); //---- Header Section ---- 
                mastertableP3.AddCell(objPdfController.ConcreteworkCell(objModel)); // ---- SH1  table1 ----              
                // mastertableP3.AddCell(objPdfController.GetCommonInstructionCell()); //---- common instruction ----
                mastertableP3.AddCell(objPdfController.GetInspectionDescCell(objModel)); //----inspection description ----
                mastertableP3.AddCell(objPdfController.GetFooterCell(objModel)); //---- Common footer ---- 

                pdfDoc.Add(mastertableP3); // add master table to the pdf document
                #endregion ---- Page 3 ----

                pdfDoc.NewPage();

                #region ---- Page 4 ----
                PdfPTable mastertableP4 = new PdfPTable(1); // Main master table page 1

                mastertableP4.AddCell(objPdfController.GetHeaderCell(objModel, objHeader)); //---- Header Section ---- 
                mastertableP4.AddCell(objPdfController.PwayCell(objModel)); // ---- SH1  table1 ----              
                // mastertableP4.AddCell(objPdfController.GetCommonInstructionCell()); //---- common instruction ----
                mastertableP4.AddCell(objPdfController.GetInspectionDescCell(objModel)); //----inspection description ----
                mastertableP4.AddCell(objPdfController.GetFooterCell(objModel)); //---- Common footer ---- 

                pdfDoc.Add(mastertableP4); // add master table to the pdf document
                #endregion ---- Page 4 ----

                pdfDoc.Close();

                #region ---- download pdf ----
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Write(pdfDoc);
                Response.End();
                #endregion ---- download pdf ----
            }
        }
        #endregion


    }
}