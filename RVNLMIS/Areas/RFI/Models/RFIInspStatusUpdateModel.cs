using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFIInspStatusUpdateModel
    {
        public int RFIId { get; set; }

        public int RevisionId { get; set; }

        public int RevisionNo { get; set; }

        public int? InspId { get; set; }

        public int? AssignedTo { get; set; }

        public string InspName { get; set; }

        public string InspStatus { get; set; }

        public string Note { get; set; }

        public string ContrComment { get; set; }

        public DateTime? InspDate { get; set; }

        public string RFICode { get; set; }

        public string Layer { get; set; }

        public string LocationType { get; set; }

        public string Workgroup { get; set; }

        public string PackageName { get; set; }

        public string EntityName { get; set; }

        public string AssignedToName { get; set; }

        public string Enclosure { get; set; }

        public string ActivityName { get; set; }

        public string StartChainage { get; set; }

        public string AprrovedStartChainage { get; set; }

        public string AprrovedFinishChainage { get; set; }

        public string EndChainage { get; set; }

        public string OtherWorkLocation { get; set; }

        public string WorkSide { get; set; }

        public HttpPostedFileBase Picture1 { get; set; }

        public HttpPostedFileBase Picture2 { get; set; }

        public HttpPostedFileBase Picture3 { get; set; }

        public string Remark1 { get; set; }

        public string Remark2 { get; set; }

        public string Remark3 { get; set; }

        public int Pic1Id { get; set; }

        public int Pic2Id { get; set; }

        public int Pic3Id { get; set; }

        public List<InspPictureModel> objPicture { get; set; }

        public string UserOrgnisation { get; set; }
    }
}

public class InspPictureModel
{
    public int PicId { get; set; }

    public string Remarks { get; set; }

    public string ImgUrl { get; set; }

    public string FileName { get; set; }
}