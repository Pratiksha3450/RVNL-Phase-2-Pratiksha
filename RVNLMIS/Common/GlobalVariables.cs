using System.Configuration;

namespace RVNLMIS.Common
{
    public class GlobalVariables
    {
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
        public static string ErrorLog = ConfigurationManager.AppSettings["LogFile"];
        public static string ApiLog = ConfigurationManager.AppSettings["ApiLog"];
        public static string BaseToken = ConfigurationManager.AppSettings["BaseToken"];
        public static string RFIApiPath =string.Concat(ConfigurationManager.AppSettings["ServerPath"], "/Areas/RFIModule/API/");
        public static string RFIUserAddSMS = "You have successfully registered to "+ ConfigurationManager.AppSettings["ServerPath"] + "."+"\r\n" +
                                             "Please find login credentials,"+ "\r\n" +
                                             "Username: #email#"+ "\r\n" +
                                             "Password: #password#";
    }
}