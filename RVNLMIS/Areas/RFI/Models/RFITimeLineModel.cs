using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFITimeLineModel
    {
        public string PackageName { get; set; }

        public string RFICode { get; set; }

        public int RevisionNo { get; set; }

        public string CurrentStatus { get; set; }

        public List<RFIListTimeLineModel> objList { get; set; }
    }

    public class RFIListTimeLineModel
    {
        public string TimelineText { get; set; }

        public string StatusClass { get; set; }

        public string StrTimelineDate { get; set; }

        //public string AttachmentType { get; set; }

        public DateTime? TimelineDate { get; set; }

        public List<AttachDetails> objAttachList { get; set; }
    }
}