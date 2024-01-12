using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;

namespace RVNLMIS.API
{
    public class DispUserApiController : ApiController
    {
        [Route("DispUserApi/User_Details")]
        public HttpResponseMessage User_Details(int packageId)
        {
            List<DisciplineUserModel> lst = new List<DisciplineUserModel>();

            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    lst = (from x in dbContext.UserDetailsWithRoles
                           join d in dbContext.tblDisciplines on x.Discipline equals d.DispId
                           where x.RoleTableId == packageId
                           select new { x, d })
                               .AsEnumerable().Select(s =>
                                  new DisciplineUserModel
                                  {
                                      UserId = s.x.UserId,
                                      Name = s.x.Name,
                                      UserName = s.x.UserName,
                                      EmailId = s.x.EmailId,
                                      Password = Functions.Decrypt(s.x.Password),
                                      MobileNo = s.x.MobileNo,
                                      RoleId = (int)s.x.RoleId,
                                      RoleName = s.x.RoleName,
                                      DisciplineName = s.d.DispName,
                                      RoleTableId = (int)s.x.RoleTableId,
                                      RoleTableName = s.x.TableName,
                                      TableDataName = s.x.TableDataName,
                                      DisciplineId = (int)s.x.Discipline,
                                      CreatedOn = s.x.CreatedOn,
                                  }).ToList();

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { lst });
                }
                catch (Exception ex)
                {
                    return ControllerContext.Request
                       .CreateResponse(HttpStatusCode.OK, new { lst });
                }
            }
        }

        [Route("DispUserApi/AddDisciplineUser")]
        [HttpPost]
        public HttpResponseMessage AddDisciplineUser(DisciplineUserModel oModel)
        {
            int userid = oModel.UserId;

            try
            {
                if (userid == 0)
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var Res = db.tblUserMasters.Where(o => o.UserName == oModel.UserName && o.IsDeleted == false).ToList();
                        if (Res.Count > 0)
                        {
                            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "User already exists." });
                        }
                        else
                        {
                            tblUserMaster objUser = new tblUserMaster();
                            objUser.Name = oModel.Name;
                            objUser.UserName = oModel.UserName;
                            int PasswordLength = Functions.ParseInteger(WebConfigurationManager.AppSettings["PasswordLength"]);
                            objUser.Password = Functions.Encrypt(Functions.GeneratePassword(PasswordLength));
                            objUser.EmailId = oModel.EmailId;
                            objUser.MobileNo = oModel.MobileNo;
                            objUser.RoleId = 700;
                            objUser.RoleTableId = oModel.RoleTableId;
                            objUser.Discipline = oModel.DisciplineId;
                            objUser.IsActive = true;
                            objUser.IsDeleted = false;
                            objUser.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                            db.tblUserMasters.Add(objUser);
                            db.SaveChanges();
                        }
                        //db.SaveChanges();
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Added successfully." });
                    }
                }
                else
                {
                    using (var db = new dbRVNLMISEntities())
                    {
                        var Res = db.tblUserMasters.Where(o => o.UserName == oModel.UserName && o.IsDeleted == false).ToList();

                        tblUserMaster objUser = db.tblUserMasters.Where(o => o.UserId == oModel.UserId).SingleOrDefault();

                        if (objUser.UserName != oModel.UserName)
                        {
                            if (Res.Count != 0)
                            {
                                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "User already exists." });
                            }
                        }

                        objUser.Name = oModel.Name;
                        objUser.UserName = oModel.UserName;
                        //objUser.Password = oModel.Password;
                        objUser.EmailId = oModel.EmailId;
                        objUser.MobileNo = oModel.MobileNo;
                        objUser.RoleId = 700;
                        objUser.RoleTableId = oModel.RoleTableId;
                        objUser.Discipline = oModel.DisciplineId;
                        objUser.IsActive = true;
                        objUser.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                        db.SaveChanges();
                        //_UserId = objUser.UserId;

                        //var oUserId = db.tblUserDataAccesses.FirstOrDefault(s => s.UserId == _UserId);
                        //if (oUserId != null)
                        //{
                        //    db.tblUserDataAccesses.Remove(oUserId);
                        //    db.SaveChanges();

                        //}
                        //var oUserDetails = db.SpGetPackageByRoleId(objUser.RoleId, objUser.RoleTableId);
                        //foreach (var o in oUserDetails)
                        //{
                        //    tblUserDataAccess objUserDataAccess = new tblUserDataAccess();
                        //    objUserDataAccess.PackageId = o.PackageId;
                        //    objUserDataAccess.UserId = _UserId;
                        //    db.tblUserDataAccesses.Add(objUserDataAccess);

                        //}
                        //db.SaveChanges();
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Updated successfully." });
                    }
                }
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Error." });
            }
        }

        [Route("DispUserApi/Delete")]
        [HttpPost]
        public HttpResponseMessage Delete(int userId)
        {
            try
            {
                // TODO: Add delete logic here
                using (var db = new dbRVNLMISEntities())
                {
                    //db.tblSections.RemoveRange(db.tblSections.Where(o => o.SectionID == id).ToList());
                    tblUserMaster objUser = db.tblUserMasters.SingleOrDefault(o => o.UserId == userId);
                    objUser.IsDeleted = true;
                    // _data = CreateData();
                    db.SaveChanges();

                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new { message = "Data deleted successfully." });
            }
            catch (Exception ex)
            {
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
    }
}
