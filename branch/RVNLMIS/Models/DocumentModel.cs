using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;

namespace RVNLMIS.Models
{
    public class DocumentModel
    {
        public int? DocumentId { get; set; }
        public int UserID { get; set; }
        public string DocumnetType { get; set; }
        public DateTime UploadedOn { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string FileFullName { get; set; }
        public bool IsDelete { get; set; }

        public int PackageID { get; set; }

        public string DocDate { get; set; }

        public string DocTime { get; set; }

        public string PackageName { get; set; }

        public string Name { get; set; }

        public string FileSize { get; set; }
        public string Extention { get; set; }
    }

    public class FileInf
    {
        public int FileId
        {
            get;
            set;
        }
        public string FileName
        {
            get;
            set;
        }
        public string FilePath
        {
            get;
            set;
        }
    }
}
