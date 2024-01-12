using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using RVNLMIS.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web;

namespace RVNLMIS.Models
{
    public class PbiEmbeddedManager
    {
        private static string urlPowerBiRestApiRoot = "https://api.powerbi.com/";
        private static string tenantId = "";
        private static string applicationId = "";
        private static string applicationSecret = "";
        private static string workspaceId = "";
        private static string reportId = "";
        private static string datasetId = "";
        private static int menuId;

        public static string GetAccessToken(string tenantId, string appId, string appSecret)
        {
            try
            {
                var tenantAuthority = $"https://login.microsoftonline.com/" + tenantId;

                var appConfidential = ConfidentialClientApplicationBuilder
                    .Create(appId)
                    .WithClientSecret(appSecret)
                    .WithAuthority(tenantAuthority)
                    .Build();

                var scopesDefault = new string[] { "https://analysis.windows.net/powerbi/api/.default" };
                var authResult = appConfidential
                    .AcquireTokenForClient(scopesDefault)
                    .ExecuteAsync()
                    .Result;

                return authResult.AccessToken;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private static PowerBIClient GetPowerBiClient(string tenantId, string applicationId, string applicationSecret)
        {
            var tokenCredentials = new TokenCredentials(GetAccessToken(tenantId, applicationId, applicationSecret), "Bearer");
            return new PowerBIClient(new Uri(urlPowerBiRestApiRoot), tokenCredentials);
        }

        public static async Task<ReportEmbeddingData> GetReportEmbeddingData(string ID)
        {
            //workspaceId = "3a0a242b-f911-4438-aafb-ad86403225c7";
            //reportId = "f5530685-1b16-4a0c-9696-8c7b26228c62";
            System.Data.DataSet ds = new System.Data.DataSet();
            DataTable dt = new DataTable();
            List<SqlParameter> objList = new List<SqlParameter>();
            objList.Add(new SqlParameter("@ID", ID));
            ds.Clear();
            ds = SqlHelper.ExecuteDataset(GlobalVariables.ConnectionString, System.Data.CommandType.StoredProcedure, "GetReportDataForPowerBIReport", objList.ToArray());
            dt = ds.Tables[0];
            foreach (DataRow dtrow in dt.Rows)
            {
                workspaceId = dtrow["WorkspaceID"].ToString();
                reportId = dtrow["ReportID"].ToString();
                tenantId = dtrow["TenantId"].ToString();
                applicationId = dtrow["ApplicationId"].ToString();
                applicationSecret = dtrow["AppSecret"].ToString();
                datasetId = dtrow["DatasetId"].ToString();
            }

            PowerBIClient pbiClient = GetPowerBiClient(tenantId, applicationId, applicationSecret);

            var report = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);
            var embedUrl = report.EmbedUrl;
            var reportName = report.Name;

            //string roleCode = ((UserModel)HttpContext.Current.Session["UserData"]).RoleCode;
            //string accessLevel = (roleCode=="SAD") ? "edit" : "view";

            GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
            string embedToken =
                  (await pbiClient.Reports.GenerateTokenInGroupAsync(workspaceId,
                                                                     report.Id,
                                                                     generateTokenRequestParameters)).Token;

            return new ReportEmbeddingData
            {
                reportId = reportId,
                reportName = reportName,
                embedUrl = embedUrl,
                accessToken = embedToken,
                WorkspaceId = workspaceId,
                DatasetId = datasetId,
            };

        }

        public static async Task<ReportEmbeddingData> GetReportEmbeddingDataWithSecurity(string ID, string Username, string Role)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            DataTable dt = new DataTable();
            List<SqlParameter> objList = new List<SqlParameter>();
            objList.Add(new SqlParameter("@ID", ID));
            ds.Clear();
            ds = SqlHelper.ExecuteDataset(GlobalVariables.ConnectionString, System.Data.CommandType.StoredProcedure, "GetReportDataForPowerBIReport", objList.ToArray());
            dt = ds.Tables[0];

            foreach (DataRow dtrow in dt.Rows)
            {
                menuId = Convert.ToInt32(dtrow["MenuId"].ToString());
                workspaceId = dtrow["WorkspaceID"].ToString();
                reportId = dtrow["ReportID"].ToString();
                tenantId = dtrow["TenantId"].ToString();
                applicationId = dtrow["ApplicationId"].ToString();
                applicationSecret = dtrow["AppSecret"].ToString();
                datasetId = dtrow["DatasetId"].ToString();
            }

            if (ID == "494")
            {
                workspaceId = "2601ae74-db3c-4f37-b5c4-c06d7ce55c30";
                reportId = "65c3d1df-dc02-484c-b0bc-29a84461d2b2";
                tenantId = "83d5a119-a55e-41c2-af64-9d73f57d19cc";
                applicationId = "fcf95cd0-3384-43cc-b8b9-d116dafacffb";
                applicationSecret = "yoZ8Q~AOzkieUtzG_xJpHWlduSSD7fIkRiEUlbeY";
                datasetId = "";
            }

            PowerBIClient pbiClient = GetPowerBiClient(tenantId, applicationId, applicationSecret);
            var report = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);
            var embedUrl = report.EmbedUrl;
            var reportName = report.Name;
            string embedToken = await ExtractEmbedtoken(Username, Role, pbiClient, report);
            return new ReportEmbeddingData
            {
                MenuId = menuId,
                tenantId = tenantId,
                reportId = reportId,
                reportName = reportName,
                embedUrl = embedUrl,
                accessToken = embedToken,
                WorkspaceId = workspaceId,
                DatasetId = datasetId,
                applicationId = applicationId,
                applicationSecret = applicationSecret
            };

        }

        private static async Task<string> ExtractEmbedtoken(string Username, string Role, PowerBIClient pbiClient, Report report)
        {
            bool isRDLReport = string.IsNullOrEmpty(report.DatasetId);
            GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest();
            string accessLevel;
            if (isRDLReport) //paginated report
            {
                accessLevel = "view";
                generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: accessLevel);
            }
            else
            {
                accessLevel = HttpContext.Current.User.IsInRole("SAD") ? "edit" : "view";
                generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: accessLevel, identities: new List<EffectiveIdentity> { new EffectiveIdentity(username: Username, roles: new List<string> { Role }, datasets: new List<string> { report.DatasetId }) });
            }
            return (await pbiClient.Reports.GenerateTokenInGroupAsync(workspaceId,
                                                                        report.Id,
                                                                        generateTokenRequestParameters)).Token;
        }
    }
}