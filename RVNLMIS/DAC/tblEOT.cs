
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
    
public partial class tblEOT
{

    public int EotID { get; set; }

    public int ProjectID { get; set; }

    public string EOTCode { get; set; }

    public string EOTDesc { get; set; }

    public Nullable<System.DateTime> SubmissionDate { get; set; }

    public string SubDocRef { get; set; }

    public string ResponseStatus { get; set; }

    public Nullable<System.DateTime> ResponseExceptedDate { get; set; }

    public Nullable<System.DateTime> ResponseActualDate { get; set; }

    public string Remark { get; set; }

    public Nullable<bool> IsDelete { get; set; }



    public virtual tblMasterProject tblMasterProject { get; set; }

}

}