using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class ProjectModel
    {

        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Required")]
        public int? EDId { get; set; }
        public string EDName { get; set; }
        [Required(ErrorMessage = "Required")]
        public int? PIUId { get; set; }
        public string PIUName { get; set; }
        public string PIUCode { get; set; }
        [Required(ErrorMessage = "Required")]
        [StringLength(10, ErrorMessage = "Should not greater than 10 Digit")]
        public string ProjectCode { get; set; }

        [Required(ErrorMessage = "Required")]
        public string ProjectName { get; set; }
        public string ProjectFullName { get; set; }
        public DateTime? DateOfTransfer { get; set; }
        public string DateOfTransfers { get; set; }
        public decimal? ProjectLength { get; set; }
        public string ProjectLengths { get; set; }
        [Required(ErrorMessage = "Required")]
        public int? RailwayId { get; set; }
        public string RailwayName { get; set; }
        public string RailwayCode { get; set; }
        [Required(ErrorMessage = "Required")]
        public int? ProjectTypeId { get; set; }
        public string ProjectTypeName { get; set; }
        public decimal? AnticipatedValue { get; set; }
        public string AnticipatedValues { get; set; }
        public decimal? ValueTillDate { get; set; }
        public string ValueTillDates { get; set; }
        public bool IsMonitorFlag { get; set; }
        public bool IsDeleted { get; set; }

    }
}