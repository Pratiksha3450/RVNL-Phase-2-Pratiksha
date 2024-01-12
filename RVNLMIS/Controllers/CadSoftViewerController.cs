using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;


namespace RVNLMIS.Controllers
{
    public class CadSoftViewerController : Controller
    {
        // GET: CadSoft
        public ActionResult Index()
        {
            string querystring = "Home/LoadFromWeb";
            string _BaseUrl = @"http://plansquares.com/";
            ViewBag.dataUrl = _BaseUrl + querystring; // set your dynamic URL here
            return View();
        }
    }
    
}