using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    public class ListBoxController : Controller
    {
        // GET: ListBox
        public ActionResult Index()
        {

            ViewBag.Attendees = new SelectList(
        new List<SelectListItem>
        {
            new SelectListItem {Text = "Google", Value = "1"},
            new SelectListItem {Text = "Other", Value = "2"},
        }, "Value", "Text");
            return View();
        }
    }
}