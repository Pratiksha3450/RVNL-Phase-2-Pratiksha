using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Models
{
    public class DataIssueReportWrapper
    {
        public DataIssueReportWrapper()
        {
            objModel = new DataIssueReport();
            objList = new List<DataIssueModel>();
        }
        public DataIssueReport objModel { get; set; }
        public List<DataIssueModel> objList { get; set; }
    }

    public class DataIssueWrapper
    {        
        public DataIssueWrapper()
        {
            objModel = new DataIssueModel();
            objLogList = new List<DataIssueStatusLogModel>();
        }
        public DataIssueModel objModel { get; set; }
        public List<DataIssueStatusLogModel> objLogList { get; set; }
    }

    public class DataIssueReport
    {
        public MvcHtmlString ReportMsg { get; set; }
        public  int NewTicket { get; set; }
        public int SubmitedForReview { get; set; }
        public int ReOpened { get; set; }
        public int Closed { get; set; }
    }
    public class DataIssueModel
    {
        public int IssueId { get; set; }
        public string DataTicket { get; set; }

        [Required(ErrorMessage = "Required")]
        public int PackageId { get; set; }

        [Required(ErrorMessage = "Required")]
        public string ShortDescription { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Description { get; set; }
        public string Attachment { get; set; }

        [Required(ErrorMessage = "Required")]
        public HttpPostedFileBase File { get; set; }

        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public int StatusId { get; set; }
        public string Remark { get; set; }
        public string NewRemark { get; set; }

        public string PackageCode { get; set; }
        public string Status { get; set; }

        public DateTime LastUpgrade { get; set; }
    }

    public class DataIssueStatusModel
    {
        public int StatusId { get; set; }
        public string Status { get; set; }
    }

    public class DataIssueStatusLogModel
    {
        public int LogId { get; set; }
        public int IssueId { get; set; }
        public int StatusId { get; set; }
        public string Remark { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int UpdatedBy { get; set; }

        public string Status { get; set; }
        public string Name { get; set; }
    }
}