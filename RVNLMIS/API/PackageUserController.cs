using RVNLMIS.Common;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace RVNLMIS.API
{
    public class PackageUserController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public object Post(FormDataCollection form)
        {
            ApiResult obj = new ApiResult();
            try
            {
                int PackageId = Functions.ParseInteger(form.Get("PackageId"));
                string EmailId = form.Get("EmailId");
                string Mobile = form.Get("Mobile");
                string company = form.Get("company");
                string name = form.Get("name");

                int _UserId = 0;
                using (dbRVNLMISEntities db = new dbRVNLMISEntities())
                {
                    var Res = db.tblUserMasters.Where(o => o.EmailId == EmailId && o.IsDeleted == false && o.RoleId == 600 && o.RoleTableId == PackageId).SingleOrDefault();
                    if (Res != null)
                    {
                        obj.Code = 202;
                        obj.Msg = "User with same email and package already added";
                        obj.Data = Res.UserId;
                    }
                    else
                    {
                        //TODO:If email is allready present and package is present

                        var emailExists = db.tblUserMasters.Where(e => e.EmailId == EmailId).FirstOrDefault();
                        if (emailExists != null)
                        {
                            obj.Code = 204;
                            obj.Msg = "Please provide other Email Id as current email Id is used for other package already";
                            obj.Data = _UserId;
                            return obj;
                        }

                        var userName = GetUniqueName(EmailId.Split('@')[0]);
                        tblUserMaster objUser = new tblUserMaster();
                        objUser.UserName = userName.ToString();
                        int PasswordLength = Functions.ParseInteger(ConfigurationManager.AppSettings["PasswordLength"]);
                        objUser.Password = Functions.Encrypt(Functions.GeneratePassword(PasswordLength));
                        objUser.EmailId = EmailId;
                        objUser.MobileNo = Mobile;
                        objUser.RoleId = 600;
                        objUser.RoleTableId = PackageId;
                        objUser.CompanyName = company;
                        objUser.Discipline = 0;
                        objUser.Name = name;
                        objUser.IsActive = true;
                        objUser.IsDeleted = false;
                        objUser.CreatedOn = DateTime.UtcNow.AddHours(5.5);
                        db.tblUserMasters.Add(objUser);
                        db.SaveChanges();
                        _UserId = objUser.UserId;

                        var oUserDetails = db.SpGetPackageByRoleId(objUser.RoleId, objUser.RoleTableId);
                        foreach (var o in oUserDetails)
                        {
                            tblUserDataAccess objUserDataAccess = new tblUserDataAccess();
                            objUserDataAccess.PackageId = o.PackageId;
                            objUserDataAccess.UserId = _UserId;
                            db.tblUserDataAccesses.Add(objUserDataAccess);
                        }
                        obj.Code = 200;
                        obj.Msg = "User added";
                        obj.Data = _UserId;
                    }
                }
                return obj;
            }
            catch (Exception ex)
            {
                obj.Code = 204;
                obj.Msg = "error" + ex.Message;
                obj.Data = "";
                return obj;
            }

        }

        private object GetUniqueName(string emailPart)
        {
            int count = 1;
            string user = emailPart + count.ToString();
            using (var db = new dbRVNLMISEntities())
            {

                while (db.tblUserMasters.Any(o => o.UserName == user))
                {
                    count++;
                    user = emailPart + count.ToString();
                }
            }
            return user;
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}