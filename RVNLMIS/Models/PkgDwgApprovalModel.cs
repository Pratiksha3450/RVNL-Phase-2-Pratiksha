using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class PkgDwgApprovalModel
    {
        public int AutoId { get; set; }

        public int DwgId { get; set; }

        public string EntityName { get; set; }

        public string ApprovedBy { get; set; }

        public string Drawing { get; set; }

        public DateTime? Date { get; set; }

        public string Revision { get; set; }

        public string ActionName { get; set; }

        public string Remark { get; set; }

        public string AttachFilePath { get; set; }

        public string AttachFileName { get; set; }

        public string DrawingName{ get; set; }
    }

    public class DrawingUpdateModel
    {
        public int AutoId { get; set; }

        //[Required]
        public int? PackageId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? EntityId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? ApprovedById { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? DrawingTypeId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? ActionId { get; set; }

        [Required(ErrorMessage = "Required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Revision { get; set; }

        public string Remark { get; set; }

        public string OperationType { get; set; }

        [Required(ErrorMessage = "Required")]
        public string StrDwgDate { get; set; }

        public bool IsFinal { get; set; }

        public string AttachFilePath { get; set; }

        public string AttachFileName { get; set; }

        //[ValidateFile]
       // public HttpPostedFileBase AttachmentFile { get; set; }
        public IEnumerable<HttpPostedFileBase> AttachmentFile { get; set; }
        public string DrawingName { get; set; }

        public string HdnDrawingName { get; set; }
    }

    public class DrawingApiModel
    {
        public string DrawingName { get; set; }
        public List<PkgDwgApprovalModel> DrawingInfo { get; set; }
    }

    public class DrwAttachment
    {
        public int AutoId { get; set; }
        public int ListId { get; set; }
        public int AttachmentID { get; set; }
        public int EnggDwgId { get; set; }

        public string AttachFilePath { get; set; }

        public string AttachFileName { get; set; }

    }
}