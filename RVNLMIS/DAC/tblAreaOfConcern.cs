
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
    
public partial class tblAreaOfConcern
{

    public int AutoId { get; set; }

    public Nullable<int> ProjectId { get; set; }

    public Nullable<int> AOCSeq { get; set; }

    public string AOCDesc { get; set; }

    public string AOCStatus { get; set; }

    public string ActionBy { get; set; }

    public Nullable<System.DateTime> DueDate { get; set; }

    public Nullable<System.DateTime> ActualCompleteDate { get; set; }

    public string Remark { get; set; }

    public Nullable<bool> IsDelete { get; set; }



    public virtual tblMasterProject tblMasterProject { get; set; }

    public virtual tblMasterProject tblMasterProject1 { get; set; }

}

}