
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace RVNLMIS.DAC
{

using System;
    using System.Collections.Generic;
    
public partial class rskRegister
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public rskRegister()
    {

        this.rskActions = new HashSet<rskAction>();

        this.rskAssignees = new HashSet<rskAssignee>();

        this.rskAttachments = new HashSet<rskAttachment>();

        this.rskComments = new HashSet<rskComment>();

        this.rskResponses = new HashSet<rskResponse>();

        this.rskReviews = new HashSet<rskReview>();

    }


    public int RiskId { get; set; }

    public string RiskType { get; set; }

    public Nullable<int> ProjectId { get; set; }

    public string RiskCode { get; set; }

    public Nullable<int> RiskCategoryId { get; set; }

    public Nullable<int> RiskStatusId { get; set; }

    public Nullable<int> RiskOwnerId { get; set; }

    public int RiskPriorityId { get; set; }

    public string RiskTitle { get; set; }

    public Nullable<System.DateTime> RiskTargetDate { get; set; }

    public string RiskCause { get; set; }

    public string RiskDetail { get; set; }

    public string RiskEffect { get; set; }

    public Nullable<int> RiskCostImpact { get; set; }

    public Nullable<int> RiskScheduleImpact { get; set; }

    public Nullable<int> RiskPerformanceImpact { get; set; }

    public Nullable<int> RiskSeverity { get; set; }

    public string RiskContingencyTrigger { get; set; }

    public string RiskContingencyPlan { get; set; }

    public Nullable<bool> isPrivate { get; set; }

    public Nullable<bool> isClosed { get; set; }

    public Nullable<bool> isDeleted { get; set; }

    public Nullable<decimal> RiskBudget { get; set; }

    public Nullable<System.DateTime> CreatedOn { get; set; }

    public Nullable<int> CreatedBy { get; set; }

    public Nullable<System.DateTime> ModifiedOn { get; set; }

    public Nullable<int> ModifiedBy { get; set; }

    public string RiskActionPlan { get; set; }

    public Nullable<int> RiskPrimaryCategoryId { get; set; }

    public Nullable<int> RiskSecondaryCategoryId { get; set; }

    public Nullable<int> RiskTertiaryCategoryId { get; set; }

    public Nullable<System.DateTime> RiskActualClosureDate { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<rskAction> rskActions { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<rskAssignee> rskAssignees { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<rskAttachment> rskAttachments { get; set; }

    public virtual rskCategory rskCategory { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<rskComment> rskComments { get; set; }

    public virtual rskStatu rskStatu { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<rskResponse> rskResponses { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<rskReview> rskReviews { get; set; }

}

}