using System;
using System.ComponentModel.DataAnnotations;

namespace RVNLMIS.Models
{
    public class PackageModel
    {
        public int PackageId { get; set; }
        [Required(ErrorMessage = "Required")]
        public int? ProjectId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string PackageCode { get; set; }
        [MaxLength]
        [Required(ErrorMessage = "Required")]
        public string PackageName { get; set; }
        public string ProjectName { get; set; }
        [MaxLength]
        [Required(ErrorMessage = "Required")]
        public string PackageShortName { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Description { get; set; }
        public string Client { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Contractor { get; set; }
        [Required(ErrorMessage = "Required")]
        public string PMC { get; set; }
        public string PCPM { get; set; }
        public DateTime? PackageStart { get; set; }
        public DateTime? PackageFinish { get; set; }
        public string PackageStarts { get; set; }
        public string PackageFinishs { get; set; }
        public DateTime? ForecastComplDate { get; set; }
        public string ForecastComplDates { get; set; }
        //[Required(ErrorMessage = "Required")]
        public double? PackageValue { get; set; }
        public string PackageValues { get; set; }
        public double? RevisedPackageValue { get; set; }
        public string RevisedPackageValues { get; set; }
        public double? CompletedValue { get; set; }
        public string CompletedValues { get; set; }
        public double? BalanceValue { get; set; }
        public string BalanceValues { get; set; }
        public string StartChainage { get; set; }
        public string EndChainage { get; set; }
        public double? TotalKmLength { get; set; }
        public string TotalKmLengths { get; set; }
        public int? Duration { get; set; }
        public string Durations { get; set; }
        public int? EoTgranted { get; set; }
        public bool? IsDeleted { get; set; }
        public int ReadOnly { get; set; }

        public int UserId { get; set; }

        public string PackageCPM { get; set; }

        public string ExpiryDate { get; set; }

        public string UserName { get; set; }

        public string PackageED { get; set; }

        public int? SectionCount { get; set; }

        public int DaysRemain { get; set; }

        public int? EntityCount { get; set; }

        public int? ProcMatDataCount { get; set; }

        public int? ProcMatAssignCount { get; set; }

        public int? ConsActDataUpdateCount { get; set; }

        public int? ConsActivityCount { get; set; }

        public int? EnggDrawingCount { get; set; }

        public int? InvoiceCount { get; set; }

        public int? RFICount { get; set; }

        public int? RFIUsersCount { get; set; }

        public int? DispUsersCount { get; set; }

        public int? UpdateScore { get; set; }
    }

    public class PackageMaterialModel
    {
        public int PkgMatId { get; set; }
        public int PackageID { get; set; }
        public int MaterialID { get; set; }
    }
}