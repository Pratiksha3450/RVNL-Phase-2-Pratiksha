
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
    
public partial class tblScCsDetail
{

    public int AutoID { get; set; }

    public Nullable<int> CsID { get; set; }

    public Nullable<int> PackageId { get; set; }

    public Nullable<int> ScActID { get; set; }

    public Nullable<bool> Layer { get; set; }

    public Nullable<int> SeqNo { get; set; }

    public Nullable<double> TotalThk { get; set; }

    public Nullable<double> MaxLayerThk { get; set; }

    public Nullable<double> Slope { get; set; }

    public Nullable<double> TopWd { get; set; }

    public Nullable<double> BottomWd { get; set; }

    public Nullable<System.DateTime> CreatedOn { get; set; }

    public Nullable<bool> IsDeleted { get; set; }



    public virtual tblPackage tblPackage { get; set; }

    public virtual tblScActivityDetail tblScActivityDetail { get; set; }

    public virtual tblSCPkgCrossSection tblSCPkgCrossSection { get; set; }

}

}
