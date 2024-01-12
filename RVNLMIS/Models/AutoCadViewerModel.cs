using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class AutoCadViewerModel
    {        
        public string FileName { get; set; }

        public int PackageId { get; set; }
        public int SectionId { get; set; }
        public int EntityId { get; set; }
        
        public string StartChainage { get; set; }
        public string EndChainage { get; set; }

        public string Scale { get; set; }
    }
}