using System.ComponentModel.DataAnnotations;

namespace RVNLMIS.Models
{
    public class DroneSModel
    {
        public int PackageId { get; set; }
        public int FilteType { get; set; }

        [Required(ErrorMessage = "required")]
        public int SectionId { get; set; }

        public int EntityId { get; set; }

        public string FileName { get; set; }
    }
}