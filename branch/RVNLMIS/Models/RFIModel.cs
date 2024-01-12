using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class RFIModel
    {
		public int ID  { get; set; }

		[Required(ErrorMessage = "Required")]
		public DateTime SubDate { get; set; }

		[Required(ErrorMessage = "Required")]
		public int? WorkGroupID { get; set; }

		[Required(ErrorMessage = "Required")]
		public string RFIID { get; set; }

		[Required(ErrorMessage = "Required")]
		public int? ActivityID { get; set; }

		[Required(ErrorMessage = "Required")]
		public int BOQ_RefID { get; set; }

		[Required(ErrorMessage = "Required")]
		
		public string LayerNo { get; set; }



		[Required(ErrorMessage = "Required")]
		public int Location { get; set; }

		//[Required(ErrorMessage = "Start Chainage is required")]
		public string StartChainage { get; set; }

		//[Required(ErrorMessage = "End Chainage is required")]
		public string EndChainage { get; set; }
		public int? EntityID { get; set; }
		public string OtherLocation { get; set; }

		[Required(ErrorMessage = "Required")]
		public int? WorksideID { get; set; }

		[Required(ErrorMessage = "Required")]
		public string Remark { get; set; }


		public DateTime? CreatedOn { get; set; }
		public bool? IsDeleted { get; set; }   // Added

		[NotMapped]
		public string WorkGroup { get; set; }

		[NotMapped]
		public string Activity { get; set; }

		[NotMapped]
		public string Workside { get; set; }

		[NotMapped]
		//[Required(ErrorMessage = "Required")]
		public string selectedBOQIDs { get; set; }
		
	}

	public class drpWorkSide
	{
		public int WorksideID { get; set; }
		public string WorkSideName { get; set; }
	}

	public class drpLayorNo
	{
		public int LayerID { get; set; }
		public string LayorName { get; set; }
	}
	public class drpWorGroup
	{
		public int WorkGroupID { get; set; }
		public string WorkGroupName { get; set; }
	}
	public class drpActivityGroup
	{
		public int ActivityID { get; set; }
		public string ActivityName { get; set; }
	}

	public class drpBOQGroup
	{
		public int BoqID { get; set; }
		public string BoqCode { get; set; }
	}
}