using RVNLMIS.Common;
using RVNLMIS.Controllers;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;


namespace RVNLMIS.API
{
    public class PowerbiReportApiController : ApiController
    {
        [HttpGet]
        public async Task<ReportEmbeddingData> ShowReport(string ReportId, string TableDataName, string RoleCode)
        {
            string reportId = ReportId;
            string tableDataName = TableDataName;
            string roleCode = RoleCode;

            ReportEmbeddingData embeddingData;
            Hashtable values = new Hashtable();
            values = RowLevelSecurity.getUsernameAndRole(tableDataName, roleCode);

            embeddingData = await PbiEmbeddedManager.GetReportEmbeddingDataWithSecurity(reportId, tableDataName, roleCode);
            return embeddingData;
        }

        [HttpGet]
        public ReportListApiModelByGroup GetReportsListWithGrouping()
        {
            ReportListApiModelByGroup obj = new ReportListApiModelByGroup();
            try
            {


                List<GroupsDetails> rListGroupsDetails = new List<GroupsDetails>();
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    string root = ConfigurationManager.AppSettings["ServerPath"].ToString();
                    rListGroupsDetails = dbContext.tblMasterGroups.Where(d => d.IsDeleted == false).ToList().Select(s => new GroupsDetails
                    {
                        GroupId = (int)s.GroupId,
                        GroupName = s.GroupName
                    }).ToList();

                    foreach (var item in rListGroupsDetails)
                    {
                        List<ReportListApiModel> rList = new List<ReportListApiModel>();
                        rList = dbContext.tblPowerBIReports.Where(d => d.isDeleted == false && d.GroupId == item.GroupId).ToList().Select(s => new ReportListApiModel
                        {
                            ReportId = (int)s.MenuId,
                            ReportName = s.ReportName,
                            ReportImage = string.Concat(root, "/Uploads/PowerBiReportImages/", s.ReportImage)
                        }).ToList();
                        item.ReportList = (rList);
                    }

                    if (rListGroupsDetails.Count > 0)
                    {
                        obj.StatusCode = 200;
                        obj.Message = "Successfully";
                        obj.Grouplist = rListGroupsDetails;

                    }
                    else
                    {
                        obj.StatusCode = 201;
                        obj.Message = "Data Not Found";

                    }

                }

            }
            catch (Exception ex)
            {

                obj.StatusCode = 202;
                obj.Message = ex.ToString();
            }
            return obj;
        }

        [HttpGet]
        public List<ReportListApiModel> GetReportsList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                string root = ConfigurationManager.AppSettings["ServerPath"].ToString();

                List<ReportListApiModel> rList = dbContext.tblPowerBIReports.Where(d => d.isDeleted == false).ToList().Select(s => new ReportListApiModel
                {
                    ReportId = (int)s.MenuId,
                    ReportName = s.ReportName,
                    ReportImage = string.Concat(root, "/Uploads/PowerBiReportImages/", s.ReportImage)
                }).ToList();
                return rList;
            }
        }

        public HttpResponseMessage RefreshDataset(int menuId)
        {
            string groupId = string.Empty;
            string datasetId = string.Empty;
            string message = string.Empty;

            try
            {
                using (var dbContext = new dbRVNLMISEntities())
                {
                    var reportObj = dbContext.tblPowerBIReports.Where(r => r.MenuId == menuId && r.isDeleted == false).FirstOrDefault();
                    groupId = reportObj.WorkSpaceId;
                    datasetId = reportObj.DatasetId;
                    DashboardReportsController.RefreshDataset(groupId, datasetId, reportObj.AppSecret, reportObj.ApplicationId,reportObj.TenantId);
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Success" });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Error" });
            }
        }


    }


    public class ReportListApiModelByGroup
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<GroupsDetails> Grouplist { get; set; }



    }
    public class GroupsDetails
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<ReportListApiModel> ReportList { get; set; }
    }

    public class ReportListApiModel
    {
        public int ReportId { get; set; }

        public string ReportName { get; set; }

        public string ReportImage { get; set; }



    }
}
