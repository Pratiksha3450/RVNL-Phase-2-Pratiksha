
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
    
public partial class tblHscParamenter
{

    public int HPID { get; set; }

    public Nullable<int> ProjID { get; set; }

    public Nullable<int> HParSeq { get; set; }

    public string HParName { get; set; }



    public virtual tblMasterProject tblMasterProject { get; set; }

}

}