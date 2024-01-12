using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
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
    public class RVNLMISCronController : Controller
    {
        public ActionResult Index()
        {
            DashboardReportsController objCtr = new DashboardReportsController();
            objCtr.RefreshAllDatasets();
            return View();
        }
    }
}