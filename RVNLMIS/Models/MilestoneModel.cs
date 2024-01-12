using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class MilestoneModel
    {
        public int MilestoneId { get; set; }

        [Required(ErrorMessage ="Required")]
        public Nullable<int> PackageId { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> ProjectId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string PrimaMileCode { get; set; }
        [Required(ErrorMessage = "Required")]
        public string MileName { get; set; }
        [Required(ErrorMessage = "Required")]
        public string MilePlanDate { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ContractMonitor { get; set; }
       
        public bool ProjCompFlag { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> Revision { get; set; }

        public bool isActive { get; set; }

        [Required(ErrorMessage = "Required")]
        public string ProjectName { get; set; }
        [Required(ErrorMessage = "Required")]
        public string PackageName { get; set; }

        public string PackageCode { get; set; }

        

        public virtual tblPackage tblPackage { get; set; }
    }
}