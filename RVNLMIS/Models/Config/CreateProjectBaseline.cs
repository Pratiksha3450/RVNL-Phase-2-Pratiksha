using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Models.Config
{
    public class CreateProjectBaseline
    {
        public int BaseID { get; set; }
        public int ProjId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string BaseRevision { get; set; }
        [Required(ErrorMessage = "Required")]
        public string BaseSubmissionDate { get; set; }
        [Required(ErrorMessage = "Required")]
        public string BaselineStatusId { get; set; }
        public string BaselineStatus { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ResponseDate { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<SelectListItem> BaselineStatusList
        {
            get
            {
                return new[]
                {
                    new SelectListItem{  Text = "Select Status",Value = "",Selected = true},
                    new SelectListItem{  Text = "Approved", Value = "1"},
                    new SelectListItem{  Text = "Approved with Comments", Value = "2"},
                    new SelectListItem{  Text = "Rejected", Value = "3"},
                    new SelectListItem{  Text = "Revise & Re-submit", Value = "4"}
            };
            }
        }
    }
}