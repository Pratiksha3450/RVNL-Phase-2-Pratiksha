using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class DroneImageVideoModel
    {
        public int FileID { get; set; }
        public int DSId { get; set; }
        public string DFileName { get; set; }
        public int DFileType { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}