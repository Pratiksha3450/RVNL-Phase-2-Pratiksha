using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class EntityMasterModel
    {
        public int EntityID { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? SectionID { get; set; }

        [Required(ErrorMessage = "Required")]
        public int PackageId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? ProjectId { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string EntityCode { get; set; }

        [Required(ErrorMessage = "Required")]
        public string EntityName { get; set; }

        [Required(ErrorMessage = "Required")]
        public string EntityTypeId { get; set; }

        public string EntityTypeName { get; set; }

        public string Lat { get; set; }

        public string Long { get; set; }

        public string ModalHeader { get; set; }

        public string StartChainage { get; set; }

        public int StartChainageNumber { get; set; }

        public string EndChainage { get; set; }

        public string SectionName { get; set; }

        public string PackageName { get; set; }

        public string PackageCode { get; set; }

        public string ProjectName { get; set; }

        public int UserId { get; set; }
    }
}