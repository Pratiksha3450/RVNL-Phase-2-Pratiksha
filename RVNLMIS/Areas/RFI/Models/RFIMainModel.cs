using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFIMainModel
    {
        public int RFIId{ get; set; }

        [Required(ErrorMessage ="required")]
        public int PackageId { get; set; }

        [Required(ErrorMessage = "required")]
        public int? WorkgroupId { get; set; }

        [Required(ErrorMessage = "required")]
        public string RFICode{ get; set; }

        [Required(ErrorMessage = "required")]
        public int? RFIActivityId { get; set; }

        public int? AssignToPMC{ get; set; }

        public int RevisionNo{ get; set; }

        public string AssignToPMCName{ get; set; }

        public int? LayerNo { get; set; }

        public string Layer { get; set; }

        public string LocationType { get; set; }

        public string Workgroup{ get; set; }

        public string PackageName{ get; set; }

        public string EntityName { get; set; }

        public string ActivityName { get; set; }

        public string WorkDescription{ get; set; }

        public string StartChainage { get; set; }

        public string EndChainage { get; set; }

        public string EntityId{ get; set; }

        public string OtherWorkLocation { get; set; }

        [Required(ErrorMessage = "required")]
        public string WorkSide{ get; set; }

        public string InspStatus { get; set; }

        public string RFIStatus{ get; set; }

        public DateTime? RFIOpenDate{ get; set; }

        public DateTime? InspectionDate{ get; set; }

        public string RFIClosedDate{ get; set; }
    }
}