using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class TicketModel
    {
        public int TicketId { get; set; }
        public string TicketNo { get; set; }

        [Required(ErrorMessage = "required")]
        public int PackageId { get; set; }

        [Required(ErrorMessage = "required")]
        public string Issue { get; set; }

        public string Icon { get; set; }
        public string Description { get; set; }
        public string ImageFileName { get; set; }

        public bool IsDelete { get; set; }

        public int AddedBy { get; set; }

        public DateTime AddedOn { get; set; }

        public Status StatusID { get; set; }

        public int AttachmentId { get; set; }   

        public string Comment { get; set; }
        
        public string FilePath { get; set; }
        //public HttpPostedFileBase[] files { get; set; }

        [DataType(DataType.Upload)]
        public HttpPostedFileBase AttachmentFile { get; set; }

        public enum Status:int
        {
            Open = 1,
            WIP = 2,
            Tested = 3,
            Closed=4,
        }
    }
}