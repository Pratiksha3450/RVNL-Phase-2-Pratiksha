
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
    
public partial class tblPowerBIReport
{

    public int Id { get; set; }

    public string ReportName { get; set; }

    public string ReportImage { get; set; }

    public string WorkSpaceId { get; set; }

    public string ReportId { get; set; }

    public bool isDashboard { get; set; }

    public string Description { get; set; }

    public Nullable<int> MenuId { get; set; }

    public Nullable<int> GroupId { get; set; }

    public string URL { get; set; }

    public Nullable<bool> isDeleted { get; set; }

    public string ApplicationId { get; set; }

    public string AppSecret { get; set; }

    public string TenantId { get; set; }

    public string DatasetId { get; set; }



    public virtual tblMasterGroup tblMasterGroup { get; set; }

}

}
