using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models.PowerBI
{
    public class AchievementModel
    {
        public int AchId { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> projectId { get; set; }

        public string ProjectCode { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ProjectName { get; set; }

        public string ProjectFullName { get; set; }
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid Sequence")]
        public Nullable<int> AchSeq { get; set; }
        [Required(ErrorMessage = "Required")]
        public string AchDesc { get; set; }
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid Quantity")]
        public Nullable<double> AchQty { get; set; }
        [Required(ErrorMessage = "Required")]
        public string AchQtyUnit { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Remark { get; set; }
    }
}