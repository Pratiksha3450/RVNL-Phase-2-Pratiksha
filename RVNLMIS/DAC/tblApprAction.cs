
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
    
public partial class tblApprAction
{

    public int ActionId { get; set; }

    public string ActionName { get; set; }

    public int ApproverId { get; set; }

    public bool IsDeleted { get; set; }



    public virtual tblEnggApprGate tblEnggApprGate { get; set; }

}

}