using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class EnclosureAttachModel
    {
        public int RFIEncId { get; set; }

        public int RFIId { get; set; }

        public int EnclId { get; set; }

        public string EnclAttach { get; set; }

        public string Note { get; set; }

        public HttpPostedFileBase AttachmentFile { get; set; }

        public List<AttachDetails> objAttachDetail { get; set; }
    }

    public class AttachDetails
    {
        public int RFIEnclId { get; set; }

        public string FileName { get; set; }

        public string EnclType { get; set; }

        public string Path { get; set; }

        public string Icon { get; set; }
    }
}