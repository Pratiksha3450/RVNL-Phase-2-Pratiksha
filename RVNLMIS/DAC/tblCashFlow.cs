
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
    
public partial class tblCashFlow
{

    public int CashFlowId { get; set; }

    public Nullable<int> PackageId { get; set; }

    public Nullable<int> ResourceId { get; set; }

    public Nullable<System.DateTime> DataDate { get; set; }

    public Nullable<decimal> ActualValue { get; set; }

    public Nullable<decimal> PlannedValue { get; set; }



    public virtual tblPackage tblPackage { get; set; }

}

}
