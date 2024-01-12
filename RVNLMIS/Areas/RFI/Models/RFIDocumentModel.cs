using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Models
{
    public class RFIDocumentModel
    {
        public int RFIId { get; set; }
        public int? InspId { get; set; }
        public int RFIRevId { get; set; }
        public int PackageId { get; set; }
        public int? WorkgroupId { get; set; }
        public string RFICode { get; set; }
        public int? RFIActivityId { get; set; }
        public int? AssignToPMC { get; set; }
        public string AssignToPMCName { get; set; }
        public int? LayerNo { get; set; }
        public string Layer { get; set; }
        public string LocationType { get; set; }
        public string Workgroup { get; set; }
        public string PackageName { get; set; }
        public string EntityName { get; set; }
        public string ActivityName { get; set; }
        public double? StartChainage { get; set; }
        public double? EndChainage { get; set; }
        public string EntityId { get; set; }
        public string OtherWorkLocation { get; set; }
        public string WorkSide { get; set; }
        public string InspStatus { get; set; }
        public string RFIStatus { get; set; }
        public DateTime? RFIOpenDate { get; set; }
        public DateTime? InspDate { get; set; }
        public DateTime? AcceptedDate  { get; set; }
        public string RFIClosedDate { get; set; }
        
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string PMC { get; set; }
        public string Contractor { get; set; }

        // ------ TABLE  ------------
        public string RFINo { get; set; }
        public DateTime DateOfSubmission { get; set; }
        public string BillNo { get; set; }
        public string BOQItemNo { get; set; }

        public string PrevRFICode { get; set; }
        public string WorkDescription { get; set; }
        public string WorkLocation { get; set; }
        public string Requestedby  { get; set; }
        public string   Revision { get; set; }
    }

    public class CommentList
    {
        public string Status { get; set; }
        public string Comment { get; set; }
        public string CommentedBy { get; set; }

        public string CommentDate { get; set; }
        public string Designation{ get; set; }
    }

    #region -- multipage model -- 
    public class RFIDocSheetOne
    {
        //------ SH1 TABLE 1 ------------
        public string Survey { get; set; }
        public string Material { get; set; }
        public string USSORItem { get; set; }
        public string ElectricalWork { get; set; }
        public string SnTWorkes { get; set; }
        public string Misc { get; set; }
        //-----------------------------------
    }

    // page2
    public class RFIDocEarthWork
    {
        public string Embankment { get; set; }
        public string SubGrade { get; set; }
        public string Blanket { get; set; }
    }

    // page 3
    public class RFIDocConcrete
    {
        public string Bridges { get; set; } = "";
        public string Building { get; set; } = "";
        public string Platform { get; set; } = "";
    }


    public class RFIDocCommonHeader
    {
        // ----- common header -----
        public int ProjectId { get; set; }
        public int PackageId { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string PMC { get; set; }
        public string Contractor { get; set; }

        // ------ TABLE  ------------
        public string RFINo { get; set; }
        public string DateOfSubmission { get; set; }
        public string BillNo { get; set; }
        public string BOQItemNo { get; set; }

        // common header end ----------------
    }

    public class RFIDocInspectionDescription
    {
        // ---- SH1 TABLE 2 -------------
        public string SectionOrChainage { get; set; }
        public string RHS_LHS { get; set; }
        public string Resources { get; set; }
        public string WorkDescription { get; set; }
        public string InspectionDateTime { get; set; }
        public string Inspection_ContarctorName { get; set; }
        public string Inspection_ContarctorDateTime { get; set; }
        public string Inspection_ClientPMCName { get; set; }
        public string Inspection_ClientPMCDateTime { get; set; }
        //---------------------------
    }

    public class RFIDocCommonFooter
    {
        public string PMCRecommendation { get; set; }
        public string ContractorRecommendation { get; set; }
        //----------------------------------------

        public string Inspection_Proceed { get; set; }
        public string Inspection_Comply { get; set; }
        public string Inspection_Redo { get; set; }
        public string Enclosures { get; set; }
        //------------------------------------------

        public string isApproved { get; set; }
        public string NotApproved { get; set; }
        public string RemarkByClient { get; set; }
        //--------------------------------
    }

    #endregion -- multipage model
}