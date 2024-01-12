
namespace RVNLMIS.Models
{

    // data required for embedding a report
    public class ReportEmbeddingData
    {
        public string reportId;
        public string tenantId;
        public string reportName;
        public string embedUrl;
        public string accessToken;
        public string DatasetId;
        public string WorkspaceId;
        public string applicationSecret;
        public string applicationId;
        public int MenuId;
    }

    // data required for embedding a new report
    public class NewReportEmbeddingData
    {
        public string workspaceId;
        public string datasetId;
        public string embedUrl;
        public string accessToken;
    }

    // data required for embedding a dashboard
    public class DashboardEmbeddingData
    {
        public string dashboardId;
        public string dashboardName;
        public string embedUrl;
        public string accessToken;
    }

    // data required for embedding a dashboard
    public class QnaEmbeddingData
    {
        public string datasetId;
        public string embedUrl;
        public string accessToken;
    }

}