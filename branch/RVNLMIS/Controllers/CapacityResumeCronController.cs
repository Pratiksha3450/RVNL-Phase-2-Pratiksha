using Microsoft.Identity.Client;
using RestSharp;
using RVNLMIS.Common.ActionFilters;
using System;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{

    public class CapacityResumeCronController : Controller
    {
        public void Index()
        {
            try
            {
                string subscriptionId = "f2b2e799-def6-4be2-8490-f801246481f0";
                string resourceGroupName = "PowerBIResource";
                string capacityName = "rvnl";

                string apiUrl = string.Format("https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.PowerBIDedicated/capacities/{2}/resume?api-version=2017-10-01", subscriptionId, resourceGroupName, capacityName);
                string token = GetAppOnlyAccessToken();
                var client = new RestSharp.RestClient(apiUrl);
                client.Timeout = -1;
                var request = new RestSharp.RestRequest(Method.POST);
                request.AddHeader("Authorization", string.Concat("Bearer ", token));
                IRestResponse response = client.Execute(request);

                Logger.LogErrorToLogFile(DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt") + "Capacity Resume REST api response: " + response.StatusDescription);
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
            }
        }

        static string GetAppOnlyAccessToken()
        {
            try
            {
                string tenantId = "83d5a119-a55e-41c2-af64-9d73f57d19cc", appId = "b7181df8-e7a7-48f7-860e-293592a833ef",
                    appSecret = "pvY7BJVRiRen@_J27m?4l[-16CU._Ck0";
                var tenantAuthority = $"https://login.microsoftonline.com/" + tenantId;

                var appConfidential = ConfidentialClientApplicationBuilder
                    .Create(appId)
                    .WithClientSecret(appSecret)
                    .WithAuthority(tenantAuthority)
                    .Build();

                var scopesDefault = new string[] { "https://management.azure.com/.default" };
                var authResult = appConfidential
                    .AcquireTokenForClient(scopesDefault)
                    .ExecuteAsync()
                    .Result;

                return authResult.AccessToken;
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return "";
            }
        }

    }
}