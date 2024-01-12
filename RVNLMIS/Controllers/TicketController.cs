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
    public class TicketController : Controller
    {

        UserModel objUserM = new UserModel();
     
        // GET: Ticket
        public ActionResult Index()
        {

            //List<TicketModel> objTicketList = new List<TicketModel>();
            //DataTable dt = new DataTable();
            //dt = GetList();
            //objTicketList = Common.DataTableHelper.DataTableToList<TicketModel>(dt);
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



        #region -- Add Ticket Details --

        public ActionResult Create()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
               
                var sessionPackages = Functions.GetRoleAccessiblePackageList();
                ViewBag.Package = new SelectList(sessionPackages, "PackageId", "PackageName");

            }
            TicketModel objModelView = new TicketModel();
            objModelView.AddedOn = DateTime.Now;
            objModelView.TicketNo = GenerateCode();


            return View("_ViewAddEditTicket", objModelView);
        }

        [HttpPost]
        public ActionResult SubmitTicket(TicketModel oModel )
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
                        var exist = db.tblTickets.Where(u => u.TicketNo == oModel.TicketNo).ToList();
                        if (exist.Count != 0)
                        {
                            message = "3";
                        }
                        else
                        {
                            if (oModel.AttachmentFile!=null)
                            {
                                FileInfo fi = new FileInfo(oModel.AttachmentFile.FileName);
                                string extn = fi.Extension;                               
                                attachmentId = Functions.AttachmentCommonFun(oModel.AttachmentFile, DateTime.Now.ToString("yyyy_MMM_dd_HHmmss"), "Ticket", "Ticket Attachment", null);
                                tblTicket objWG = new tblTicket();
                                objWG.PackageId = oModel.PackageId;
                                objWG.Issue = oModel.Issue;
                                objWG.Description = oModel.Description;
                                objWG.AddedOn = DateTime.Now;
                                objWG.Status = 1;
                                objWG.AddedBy = Convert.ToInt32(userId);
                                objWG.TicketNo = oModel.TicketNo;
                                objWG.AttachmentId = attachmentId;
                                objWG.IsDelete = false;
                                db.tblTickets.Add(objWG);
                                db.SaveChanges();
                                message = "1";
                            }
                            else // without file
                            {
                                tblTicket objWG = new tblTicket();
                                objWG.PackageId = oModel.PackageId;
                                objWG.Issue = oModel.Issue;
                                objWG.Description = oModel.Description;
                                objWG.AddedOn = DateTime.Now;
                                objWG.Status = 1;
                                objWG.IsDelete = false;
                                objWG.AddedBy = Convert.ToInt32(userId);
                                objWG.TicketNo = oModel.TicketNo;
                                objWG.AttachmentId = 0;
                                db.tblTickets.Add(objWG);
                                db.SaveChanges();
                                message = "1";
                            }
                            str = CreateHTML();
                        }
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
                    str += "<a href='#' class='text-danger f-20 float-right mr-3 btnDelete' data-key=" + dt1.Rows[i]["TicketId"] + "><i class='feather icon-trash-2 mr-1'></i></a>";
                    str += "</div>";
                    str += "<div class='ticket-type-icon private mt-1 mb-1'><i class='feather icon-lock mr-1 f-14 text-info'></i>" + dt1.Rows[i]["Issue"] + "</div>";
                    str += "<ul class='list-inline mt-2 mb-0'>";
                    str += "<li class='list-inline-item'><img src='/Content/assetsNew/images/developer.jpg' alt='' class='wid-20 rounded mr-1 img-fluid'>Assigned to Development Team</li>";
                    str += "<li class='list-inline-item'><i class='feather icon-calendar mr-1 f-14 text-orange'></i>" + dt1.Rows[i]["AddedOn"] + "</li>";
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
        #endregion

        #region -- EDIT RFIBOQ Details --
        public ActionResult EditBOQDetails(int id)
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
            }
            catch (Exception ex)
            {
                throw;
            }
            return View("_ViewAddEditActBOQ", objModel);
        }
        #endregion

        #region -- Delete Boq Details --
        [HttpPost]
        public JsonResult Delete(int TicketID)
        {
            string str = string.Empty;
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblTicket obj = db.tblTickets.SingleOrDefault(o => o.TicketId == TicketID);
                    obj.IsDelete = true;
                    db.SaveChanges();
                    str = CreateHTML();
                }
                return Json(new { SucMessage = "1", ListStr = str }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("-1");
            }
        }
        #endregion

        public string GenerateCode()
        {
            string NewStr = string.Empty;
            int CodeNo = 0;
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    var LastTicket = db.tblTickets.OrderByDescending(o => o.TicketNo).FirstOrDefault();
                    if (LastTicket == null)
                    {
                        NewStr = "TCK001";
                    }
                    else
                    {
                        string abc = LastTicket.TicketNo.ToString();
                        NewStr = abc.Remove(0, 3);
                        CodeNo = Convert.ToInt32(NewStr);
                        if (CodeNo > 0 && CodeNo < 9)
                        {
                            NewStr = "TCK" + "00" + Convert.ToString(CodeNo + 1);
                        }
                        else if (CodeNo >= 9 && CodeNo < 99)
                        {
                            NewStr = "TCK" + "0" + Convert.ToString(CodeNo + 1);
                        }
                        else if (CodeNo >= 99 && CodeNo < 1000)
                        {
                            NewStr = "TCK" + Convert.ToString(CodeNo + 1);
                        }
                    }
                }
                return NewStr;
            }

            catch (Exception ex)
            {
                return string.Empty;
            }
        }

    }
}