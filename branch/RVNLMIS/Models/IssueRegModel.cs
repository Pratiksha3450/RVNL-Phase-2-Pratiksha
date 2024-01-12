using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class IssueRegModel
    {
        public int ID { get; set; }
        public string IssueCode { get; set; }

        
        public string IssueRegNo { get; set; }

        [Required(ErrorMessage = "Required")]
        public string  IssueDescription { get; set; }

        [Required(ErrorMessage = "Required")]
        public string IssueSubject { get; set; }

        [Required(ErrorMessage = "Required")]
        public int DisplineId { get; set; }

        public string DisciplineName { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? PackageId { get; set; }

        public string PackageName { get; set; }
        
        public int? EntityID { get; set; }

        public string OtherLocation { get; set; }

        [Required(ErrorMessage = "Required")]
        public string  Date { get; set; }

        public string LocationType { get; set; }

        public string Location { get; set; }

        public DateTime ? IssueDate { get; set; }
        
        public string StartChainage { get; set; }

        
        public string EndChainage { get; set; }


     
        public string AttachFilePath { get; set; }


        public string AttachFileName { get; set; }

       public int issueAddedby { get; set; }

        public string PMCName { get; set; }

        public string UserName { get; set; }


        public HttpPostedFileBase AttachmentFile { get; set; }
    }

   

    
}