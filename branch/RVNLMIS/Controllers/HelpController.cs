//using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            //object fileSavePath = Server.MapPath("~/Uploads/RVNL.docx");
            //object documentFormat = 8;
            //string randomName = DateTime.Now.Ticks.ToString();
            //object htmlFilePath = Server.MapPath("~/Uploads/") + randomName + ".htm";
            //string directoryPath = Server.MapPath("~/Uploads/") + randomName + "_files";

            ////Open the word document in background.
            //_Application applicationclass = new Application();
            //applicationclass.Documents.Open(ref fileSavePath);
            //applicationclass.Visible = false;
            //Document document = applicationclass.ActiveDocument;

            ////Save the word document as HTML file.
            //document.SaveAs(ref htmlFilePath, ref documentFormat);

            ////Close the word document.
            //document.Close();

            ////Read the saved Html File.
            //string wordHTML = System.IO.File.ReadAllText(htmlFilePath.ToString());

            ////Loop and replace the Image Path.
            //foreach (Match match in Regex.Matches(wordHTML, "<v:imagedata.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase))
            //{
            //    wordHTML = Regex.Replace(wordHTML, match.Groups[1].Value, "Temp/" + match.Groups[1].Value);
            //}

            ////Delete the Uploaded Word File.
            ////System.IO.File.Delete(fileSavePath.ToString());

            //ViewBag.WordHtml = wordHTML;
            return View();
        }
    }
}