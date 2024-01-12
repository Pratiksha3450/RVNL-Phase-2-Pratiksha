using Newtonsoft.Json;
using RestSharp;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace RVNLMIS.Common
{
    public class Functions
    {

        public static string GetBase64StringForImage(string path)
        {
            string imagePath = path;
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }
        public static void CreateIfMissing(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
                Directory.CreateDirectory(path);
        }
        public static double ParseDouble(string doubleValue)
        {
            double doubleVar = 0;
            double result = 0;

            if (!string.IsNullOrEmpty(doubleValue))
            {
                if (double.TryParse(doubleValue, out doubleVar))
                {
                    result = Convert.ToDouble(doubleValue);
                }
            }

            return result;
        }
        public static int ParseInteger(string intValue)
        {
            int intVar = 0;
            int result = 0;

            if (!string.IsNullOrEmpty(intValue))
            {
                if (int.TryParse(intValue, out intVar))
                {
                    result = Convert.ToInt32(intValue);
                }
            }

            return result;
        }

        public static string GeneratePassword(int PwdLength)
        {
            string pwdChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            char[] pwdElements = pwdChars.ToCharArray();
            Random random = new Random();
            StringBuilder builder = new StringBuilder();
            //Generate password in loop
            for (int i = 0; i <= PwdLength; i++)
            {
                int randomChar = random.Next(0, pwdElements.Length);
                builder.Append(pwdElements[randomChar]);
            }
            return builder.ToString();
        }

        public static string GeneratePassword1(int PwdLength)
        {
            string pwdChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*?_-";
            char[] pwdElements = pwdChars.ToCharArray();
            Random random = new Random();
            StringBuilder builder = new StringBuilder();
            //Generate password in loop
            for (int i = 0; i <= pwdElements.Length; i++)
            {
                int randomChar = random.Next(0, pwdElements.Length);

                char singleChar = pwdElements[randomChar];

                ///char and less than 4 then ok
                ///=5 or =6 special char
                ///=7 or =8 number
                if (Char.IsLetter(singleChar) && builder.Length <= 3)
                {
                    builder.Append(pwdElements[randomChar]);
                }
                else if (Char.IsSymbol(singleChar) && (builder.Length == 4 || builder.Length == 5))
                {
                    builder.Append(pwdElements[randomChar]);
                }
                else if (Char.IsNumber(singleChar) && (builder.Length == 6 || builder.Length == 7))
                {
                    builder.Append(pwdElements[randomChar]);
                }

                if (builder.Length == PwdLength)
                {
                    break;
                }
                else
                {
                    continue;
                }
            }
            return builder.ToString();
        }

        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        /// <summary>
        /// Sends the web push.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="title">The title.</param>
        /// <param name="msg">The MSG.</param>
        /// <exception cref="NotImplementedException"></exception>
        internal static string SendWebPush(int userId, string title, string msg, string userType)
        {
            try
            {                
                var client = new RestClient(ConfigurationManager.AppSettings["notifyUrl"] + "api/WebPush?userId=" + userId + "&title=" + title + "&msg=" + msg + "&userType="+ userType+"");
                client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                //client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("token", "ijedgi_TB55kloaq@yyO0");
                IRestResponse response = client.Execute(request);
                var res = JsonConvert.DeserializeObject<SMSresults>(response.Content);
                return res.objResults.Code == 200 ? "1" : "0";
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return "0";
            }
        }

        public static string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        public static List<GetRoleAssignedProjectList_Result> GetroleAccessibleProjectsList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                int userId = ((UserModel)HttpContext.Current.Session["UserData"]).UserId;
                int roleId = ((UserModel)HttpContext.Current.Session["UserData"]).RoleId;

                var sessionProjects = dbContext.GetRoleAssignedProjectList(userId, roleId).ToList();
                return sessionProjects;
            }
        }

        public static List<GetRoleAssignedPackageList_Result> GetRoleAccessiblePackageList()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                int userId = ((UserModel)HttpContext.Current.Session["UserData"]).UserId;
                int roleId = ((UserModel)HttpContext.Current.Session["UserData"]).RoleId;

                var pkgs = dbContext.GetRoleAssignedPackageList(userId, roleId).ToList();
                return pkgs;
            }
        }

        public static List<GetRoleAssignedPackageListForMaterialPackage_Result> GetRoleAssignedPackageListForMaterialPackage()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                int userId = ((UserModel)HttpContext.Current.Session["UserData"]).UserId;
                int roleId = ((UserModel)HttpContext.Current.Session["UserData"]).RoleId;

                var pkgs = dbContext.GetRoleAssignedPackageListForMaterialPackage(userId, roleId).ToList();
                return pkgs;
            }
        }
        public static List<GetRoleAssignedPackageListForMaterialPackage_Result> GetRoleAssignedPackageListForMaterialPackageRFI()
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                int userId = ((UserModel)HttpContext.Current.Session["RFIUserSession"]).UserId;
                int roleId = ((UserModel)HttpContext.Current.Session["RFIUserSession"]).RoleId;

                var pkgs = dbContext.GetRoleAssignedPackageListForMaterialPackage(userId, roleId).ToList();
                return pkgs;
            }
        }

        public static int AttachmentCommonFun(HttpPostedFileBase AttachmentFile, string packageCode, string folderName, string attachType, dynamic present)
        {
            int attachmentId = 0;
            string localPath = "~/Uploads/Attachments/" + folderName;
            Functions.CreateIfMissing(HostingEnvironment.MapPath(localPath));

            string getFileName = Path.GetFileName(AttachmentFile.FileName);
            string fileName = string.Concat(packageCode, "-", getFileName.Replace(' ', '_'));
            string filePath = "/Uploads/Attachments/" + folderName + "/" + fileName;
            string FileSizeStr = "";
            long fileSize = AttachmentFile.ContentLength/1000;
            if (fileSize > 999)
            {
                FileSizeStr = Convert.ToString(fileSize/1000) + " MB";
            }
            else
            {
                FileSizeStr = Convert.ToString(fileSize) + " KB";
            }
            try
            {
                AttachmentFile.SaveAs(HostingEnvironment.MapPath(filePath));
                //Save Attachment to DB

                if (present != null)   //update values
                {
                    attachmentId = Functions.AddEditAttachment(fileName, filePath, present.AttachmentId, attachType, FileSizeStr);
                }
                else
                {
                    attachmentId = Functions.AddEditAttachment(fileName, filePath, null, attachType, FileSizeStr);
                }
            }
            catch (Exception ex)
            {
                //deleting excel file from folder  
                if ((System.IO.File.Exists(filePath)))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            return attachmentId;
        }

        public static int AddEditAttachment(string fileName, string filePath, int? existingAttachmentId, string attachType, string FileSize)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                int attachmentId = 0;
                tblAttachment objAttach = new tblAttachment();

                if (existingAttachmentId != null)
                {
                    objAttach = dbContext.tblAttachments.Where(a => a.AttachmentID == existingAttachmentId).FirstOrDefault();
                    objAttach.FileName = fileName;
                    objAttach.Path = filePath;
                    attachmentId = objAttach.AttachmentID;
                    objAttach.FileSize = FileSize;
                    dbContext.SaveChanges();
                }
                else
                {
                    objAttach.FileName = fileName;
                    objAttach.Path = filePath;
                    objAttach.Type = attachType;
                    objAttach.CreatedOn = DateTime.Now;
                    objAttach.IsDeleted = false;
                    objAttach.CreatedBy = ((UserModel)HttpContext.Current.Session["UserData"]).UserId;
                    objAttach.FileSize = FileSize;
                    dbContext.tblAttachments.Add(objAttach);
                    dbContext.SaveChanges();
                    attachmentId = objAttach.AttachmentID;
                }
                return attachmentId;
            }
        }

        /// <summary>
        /// Delete Folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool DeleteFilesInFolder(string folderPath, bool recursive)
        {
            //Safety check for directory existence.
            if (!Directory.Exists(folderPath))
                return false;

            string[] files = Directory.GetFiles(folderPath);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime < DateTime.Now.AddMinutes(-10))
                    fi.Delete();
            }

            //foreach (string file in Directory.GetFiles(folderPath))
            //{
            //    File.Delete(file);
            //}

            //Iterate to sub directory only if required.
            if (recursive)
            {
                foreach (string dir in Directory.GetDirectories(folderPath))
                {
                    DeleteFilesInFolder(dir, recursive);
                }
            }
            ////Delete the parent directory before leaving
            //Directory.Delete(folderPath);
            return true;
        }

        public static string GetLocation(string ip)
        {
            var res = "";
            WebRequest request = WebRequest.Create("http://ipinfo.io/" + ip);
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    res += line;
                }
            }
            return res;
        }

        public static int RepalceCharacter(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return Convert.ToInt32(value.Replace("+", ""));
            }
            else return 0;
        }

        public static void ReadExcelintoDatatable(HttpPostedFileBase FileUpload, out string pathToExcelFile, out DataTable dtable)
        {
            string filename = FileUpload.FileName;
            string targetpath = HostingEnvironment.MapPath("~/Uploads/ExcelImport");
            FileUpload.SaveAs(targetpath + filename);
            pathToExcelFile = targetpath + filename;
            var connectionString = "";
            if (filename.EndsWith(".xls"))
            {
                connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
            }
            else if (filename.EndsWith(".xlsx"))
            {
                connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
            }

            var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
            var ds = new DataSet();

            adapter.Fill(ds, "ExcelTable");

            dtable = ds.Tables["ExcelTable"];
        }

        /// <summary>
        /// Sends the email to share url
        /// </summary>
        public static void SendLoginEmail(string userName, string userEmail)
        {
            string txtFile = HostingEnvironment.MapPath("~/Common/EmailTemplate/loginsuccess.txt");//get location of file
            string body = System.IO.File.ReadAllText(txtFile); //get all file textfile data in string
            OrderedDictionary EmailPlaceholderObject = new OrderedDictionary();
            EmailPlaceholderObject.Add("Fname", userName);

            Email objEmail = new Email();
            string msgBody = objEmail.SetTemplate(body, EmailPlaceholderObject);
            string msgSubject = "Security alert";

            int rs = objEmail.SendMail(userEmail, null, null, null, msgSubject, msgBody, ConfigurationManager.AppSettings["SUPPORTFROM"]);
        }


        //public static int SaveUserLogs(int PackageID, string FormName, string ActionName, int UserID, string IpAddress)
        //{
        //    int i = 0;
        //    try
        //    {
        //        using (var db = new dbRVNLMISEntities())
        //        {
        //            tblUserLog obj = new tblUserLog();
        //            obj.PackageID = Convert.ToInt32(PackageID);
        //            obj.FormName = FormName;
        //            obj.ActionName = ActionName;
        //            obj.UserID = Convert.ToInt32(UserID);
        //            obj.AccessedOn = DateTime.Now;
        //            obj.IPAddress = IpAddress;
        //            db.tblUserLogs.Add(obj);
        //            db.SaveChanges();
        //            i = 1;
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        i = 0;
        //    }
        //    return i;
        //}

        public static int SaveUserLog(int PackageID, string FormName, string ActionName, int UserID, string IpAddress, string updatedValue)
        {
            int i = 0;
            try
            {
                using (var db = new dbRVNLMISEntities())
                {
                    tblUserLog obj = new tblUserLog();
                    obj.PackageID = Convert.ToInt32(PackageID);
                    obj.FormName = FormName;
                    obj.ActionName = ActionName;
                    obj.UserID = Convert.ToInt32(UserID);
                    obj.AccessedOn = DateTime.Now;
                    obj.IPAddress = IpAddress;
                    obj.UpdatedValue = updatedValue;
                    db.tblUserLogs.Add(obj);
                    db.SaveChanges();

                    db.Database.ExecuteSqlCommand("Delete from UserLogAudit");
                    i = 1;
                }

            }
            catch (Exception e)
            {
                i = 0;
            }
            return i;
        }
    }
}