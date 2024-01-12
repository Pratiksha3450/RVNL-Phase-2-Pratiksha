using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RVNLMIS.Controllers;
using System.Net.Http.Formatting;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Areas.RFI.Controllers;
using System.Web;
using System.Configuration;
using System.Web.Hosting;
using System.IO;
using RVNLMIS.Areas.RFI.Common;
using System.Threading.Tasks;

namespace RVNLMIS.Areas.RFI.API
{
    public class RFISummaryApiController : ApiController
    {
        CommonRFIMethodsController _objCommon = new CommonRFIMethodsController();

        [HttpPost]
        [Route("RFISummaryApi/GetRfiCountOnLoad")]
        public HttpResponseMessage GetRfiCountOnLoad(FormDataCollection obj)
        {
            RFIcountSummary objList = new RFIcountSummary();
            try
            {
                int viewerId =Convert.ToInt32(obj.Get("viewerId"));
                int selectedUserId = Convert.ToInt32(obj.Get("selectedUserId"));
                string orgnisation = Convert.ToString(obj.Get("orgnisation"));
                int designId = Convert.ToInt32(obj.Get("designId"));

                objList = _objCommon._GetRfiCountObj(selectedUserId, viewerId,designId,orgnisation);
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { objList });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { objList });
            }
        }

        [HttpPost]
        [Route("RFISummaryApi/GetUsersDrp")]
        public HttpResponseMessage GetUsersDrp(FormDataCollection obj)
        {
            List<DropDownOptionModel> lstInfo = new List<DropDownOptionModel>();

            try
            {
                int packageId = Convert.ToInt32(obj.Get("pkgId"));
                int userId = Convert.ToInt32(obj.Get("userId"));
                string org = Convert.ToString(obj.Get("org"));
                string designName = Convert.ToString(obj.Get("designName"));

                lstInfo = _objCommon._GetUsersListDrp(packageId, userId, org, designName);

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lstInfo });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lstInfo });
            }
        }
    }
}
