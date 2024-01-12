using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using RVNLMIS.Areas.RFI.Models;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [Compress]
    [SessionAuthorize]
    public class TicketAdminController : Controller
    {

        UserModel objUserM = new UserModel();
     
        // GET: Ticket
        public ActionResult Index()
        {  
            return View();
        }

        public DataTable GetList()
        {
            DataTable dt1 = new DataTable();
            UserModel objUserM = new UserModel();
            objUserM = (UserModel)Session["UserData"];
            string userId = Convert.ToString(objUserM.UserId);
            List<TicketModel> objTicketList = new List<TicketModel>();
            try
            {
                string ConnectionString = GlobalVariables.ConnectionString;
                List<SqlParameter> paramlist = new List<SqlParameter>();
                paramlist.Add(new SqlParameter("@UserID", userId));
                DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, System.Data.CommandType.StoredProcedure, "GetTicketDetails", paramlist.ToArray());
                dt1 = dsDataset.Tables[0];
            }
            catch (Exception ex)
            {

            }
            return dt1;
        }

        [HttpPost]
        public ActionResult SaveStatus(TicketModel oModel)
        {
            int attachmentId = 0;
            string message = string.Empty;
            objUserM = (UserModel)Session["UserData"];
            string userId = Convert.ToString(objUserM.UserId);
            string str = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {

                    using (var db = new dbRVNLMISEntities())
                    {
                        tblTicket objWG = db.tblTickets.Where(u => u.TicketId == oModel.TicketId).SingleOrDefault();
                                objWG.Comment = oModel.Comment;
                                objWG.AddedOn = DateTime.Now;
                                objWG.Status = Convert.ToInt32(oModel.StatusID);
                                objWG.AddedBy = Convert.ToInt32(userId);
                                objWG.TicketNo = oModel.TicketNo;
                                objWG.AttachmentId = attachmentId;
                                objWG.IsDelete = false;
                                db.tblTickets.Add(objWG);
                                db.SaveChanges();
                                message = "1";
                            
                            
                            str = CreateHTML();
                        
                    }


                    return Json(new { SucMessage = message, ListStr = str }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return View("_ViewAddEditTicket", oModel);
                }
            }

            catch (Exception ex)
            {
                message = "2";
                return View("_ViewAddEditTicket", oModel);
            }
        }


        [HttpGet]
        public JsonResult GetTicketList()
        {
            int message=0;
            string str = "";
            str = CreateHTML();
            if (str != null)
            {
                message = 1;
            }
            return Json(new { SucMessage = message, ListStr = str }, JsonRequestBehavior.AllowGet);
        }
        #region --- List BOQ master Values ---

        #endregion



       

       
        public string CreateHTML()
        {
            string str = string.Empty;
            DataTable dt1 = new DataTable();
            dt1 = GetList();
            if (dt1.Rows.Count > 0)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    str += "<div class='row no-gutters'>";
                    str += "<div class='col'>";
                    str += "<div class='card hd-body'>";
                    str += "<div class='row align-items-center'>";
                    str += "<div class='col border-right pr-0'>";
                    str += "<div class='card-body inner-center'>";
                    str += "<div class='ticket-customer font-weight-bold'>";
                    str += "<span>" + dt1.Rows[i]["TicketNo"] + "</span>";

                    if (dt1.Rows[i]["Status"].ToString() == "Working on Issue")
                    {
                        str += "<span class='float-right mr-3'><a href='#' class='btn btn-secondary btn-sm ChangeStatus' data-key=" + dt1.Rows[i]["TicketId"] + "> " + dt1.Rows[i]["Status"] + "</a></span>";
                    }
                    else if (dt1.Rows[i]["Status"].ToString() == "Tested")
                    {
                        str += "<span class='float-right mr-3'><a href='#' class='btn btn-success btn-sm ChangeStatus' data-key=" + dt1.Rows[i]["TicketId"] + "> " + dt1.Rows[i]["Status"] + "</a></span>";
                    }
                    else if (dt1.Rows[i]["Status"].ToString() == "Closed")
                    {
                        str += "<span class='float-right mr-3'><a href='#' class='btn btn-info btn-sm ChangeStatus' data-key=" + dt1.Rows[i]["TicketId"] + "> " + dt1.Rows[i]["Status"] + "</a></span>";
                    }
                    else
                    {
                        str += "<span class='float-right mr-3'><a href='#' class='btn btn-warning btn-sm ChangeStatus' data-key=" + dt1.Rows[i]["TicketId"] + "> " + dt1.Rows[i]["Status"] + "</a></span>";
                    }

                    
                    
                    str += "</div>";
                    str += "<div class='ticket-type-icon private mt-1 mb-1'><i class='feather icon-lock mr-1 f-14 text-info'></i>" + dt1.Rows[i]["Issue"] + "</div>";
                    str += "<ul class='list-inline mt-2 mb-0'>";
                    str += "<li class='list-inline-item'><img src='/Content/assetsNew/images/developer.jpg' alt='' class='wid-20 rounded mr-1 img-fluid'>Assigned to Development Team</li>";
                    str += "<li class='list-inline-item'><i class='feather icon-calendar mr-1 f-14 text-orange'></i>" + dt1.Rows[i]["AddedOn"] + "  <b> Package: </b> " + dt1.Rows[i]["PackageName"] + "</li>";
                    str += "</ul>";
                    str += "<div class='row excerpt m-2'>";
                    str += "<div class='col-md-7'>";
                    str += "<h6> Issue Description </h6>";
                    str += "<p class='pb-3'>";
                    str += "" + dt1.Rows[i]["Description"] + "";
                    str += "</p>";
                    str += "</div>";
                    str += "<div class='col-md-5 text-right'>";
                    if (Convert.ToInt32(dt1.Rows[i]["AttachmentId"]) == 0)
                    {
                        str += "<h6>  No Attachments found! </h6>";
                        str += "<div class='thumbnail mb-4'>";                        
                      
                        str += "</div>";
                    }
                    else
                    {
                        str += "<h6> <i class='fa fa-paperclip mr-1 text-magenta' aria-hidden='true'></i> Attachments </h6>";
                        str += "<div class='mb-4'>";
                        str += "<div class='thumb'>";
                        str += "<a href='" + dt1.Rows[i]["FilePath"] + "' data-toggle='lightbox' data-title='Attachment Preview'>";
                        str += "<img src='" + dt1.Rows[i]["FilePath"] + "' alt='' class='img-fluid img-thumbnail wid-50 height-50 rounded mr-1'>";
                        str += "</a>";
                        str += "</div>";
                        str += "</div>";
                    }               


                   
                    str += "</div>";
                    str += "</div>";
                    str += "</div>";
                    str += "</div>";
                    str += "</div>";
                    str += "</div>";
                    str += "</div>";
                    str += "</div>";

                    

                }
            }
            return str;
        }
       

        #region -- EDIT Ticket Details --
        public ActionResult EditTicketDetails(int id)
        {
            TicketModel objModel = new TicketModel();
            try
            {
                if (id != 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var objTicket = db.tblTickets.Where(o => o.TicketId == id).SingleOrDefault();
                        if (objTicket != null)
                        {
                            objModel.TicketId = objTicket.TicketId;
                            objModel.PackageId = Convert.ToInt32(objTicket.PackageId);
                            objModel.Issue = objTicket.Issue;
                            objModel.TicketNo = objTicket.TicketNo;
                        }
                    }
                }
                //var lstStatus = from Status e in Enum.GetValues(typeof(Status))
                //               select new
                //               {
                //                   ID = e,
                //                   Name = e.ToString()
                //               };
                //ViewBag.lstStatus = new SelectList(lstStatus, "ID", "Name");
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_changeStatus", objModel);
        }
        #endregion

        

     
    }
}