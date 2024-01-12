using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace PrimaBiWeb.Common
{

    public class SMSSend
    {
        public SMSSend()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static string SendSMS(string strUsernamePrefix,
                              string strUsername,
                              string strPassword,
                              string strType,
                              string strDelivery,
                              string strMobileNumbers,
                              string strSource,
                              string Message)
        {

            //Username with username prefix
            strUsernamePrefix = string.IsNullOrEmpty(strUsernamePrefix) ? "stec-" : strUsernamePrefix;
            strUsername = string.Concat(strUsernamePrefix, strUsername);
            strType = string.IsNullOrEmpty(strType) ? "0" : strType;
            strDelivery = string.IsNullOrEmpty(strDelivery) ? "1" : strDelivery;
            strSource = string.IsNullOrEmpty(strSource) ? "shtech" : strSource;

            if (strType == "2" || strType == "6")
            {
                Message = ConvertToUnicode(Message + " ");
            }

            string strMessage = HttpUtility.UrlEncode(Message);
            string strUrl = "http://103.16.101.52/sendsms/bulksms?username=" + strUsername
                + "&password=" + strPassword
                + "&type=" + strType
                + "&dlr=" + strDelivery
                + "&destination=" + strMobileNumbers
                + "&source=" + strSource
                + "&message=" + strMessage;

            try
            {
                WebRequest request = HttpWebRequest.Create(strUrl);
                request.Timeout = 25000;
                request.Proxy = null;

                WebResponse response = request.GetResponse();
                Stream str = response.GetResponseStream();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string strResponse = reader.ReadToEnd();
                string[] strResponseArray = strResponse.Split(',');
                return strResponse;
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return "Error - SMS Function Not Working !" + ex.Message;
            }
        }

        public static string ConvertToUnicode(string Message)
        {

            string UnicodeString = string.Empty;
            byte[] ArrayOFBytes = System.Text.Encoding.Unicode.GetBytes(Message);

            for (int v = 0; v < ArrayOFBytes.Length - 1; v++)
            {
                if (v % 2 == 0)
                {
                    int t = ArrayOFBytes[v];
                    ArrayOFBytes[v] = ArrayOFBytes[v + 1];
                    ArrayOFBytes[v + 1] = (byte)t;
                }
            }


            for (int v = 0; v < ArrayOFBytes.Length - 1; v++)
            {
                int val = Convert.ToInt32(ArrayOFBytes[v]);
                string c = ArrayOFBytes[v].ToString("X");

                if (c.Length == 1)
                {
                    c = "0" + c;
                }

                UnicodeString = UnicodeString + c;
            }

            return UnicodeString.Substring(0, UnicodeString.Length - 2);
        }

        /// <summary>
        /// Sends the SMS. Now calling roundsms service via notify project api
        /// </summary>
        /// <param name="strMobileNumbers">The string mobile numbers.</param>
        /// <param name="Message">The message.</param>
        /// <returns></returns>
        public static string SendSMS(string strMobileNumbers, string Message)
        {
            try
            {
                //bypass ssl validation check by using RestClient object
                //var restClient = new RestClient("https://notify.primabi.com");

                var client = new RestClient("https://notify.primabi.com/api/SendSMS?_mobileNumbers=" + strMobileNumbers + "&_text=" + Message + "");
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
        public static string SendSMSBase(string strMobileNumbers, string Message)
        {

            /*
                <add key="authkey" value="YmJmOGU0ZmUxMDY" />
                <add key="sender" value="PRIMAB" />
                <add key="type" value="1" />
                <add key="route" value="2" />            
             */
            string strAuthkey = ConfigurationManager.AppSettings["authkey"];
            string strSender = ConfigurationManager.AppSettings["sender"];
            string strType = ConfigurationManager.AppSettings["type"];
            string strRoute = ConfigurationManager.AppSettings["route"];  //"1" for Promotional | "2" for Transactional | "3" for Promo SenderId

            strType = string.IsNullOrEmpty(strType) ? "0" : strType;

            Message = strType == "2" ? ConvertToUnicode(Message + " ") : Message;

            string strMessage = HttpUtility.UrlEncode(Message);

            // http://roundsms.com/api/sendhttp.php?authkey=YmJmOGU0ZmUxMDY&mobiles=Mobile1,Mobile2,Mobile3,...&message=Your Message&sender=Sender Id&type=type&route=route

            string strUrl =
                "http://roundsms.com/api/sendhttp.php?"
                + "authkey=" + strAuthkey
                + "&mobiles=" + strMobileNumbers
                + "&message=" + strMessage
                + "&sender=" + strSender
                + "&type=" + strType
                + "&route=" + strRoute;

            try
            {
                WebRequest request = HttpWebRequest.Create(strUrl);
                request.Timeout = 25000;
                request.Proxy = null;

                WebResponse response = request.GetResponse();
                Stream str = response.GetResponseStream();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string strResponse = reader.ReadToEnd();

                var res = JsonConvert.DeserializeObject<RoundsmsReply>(strResponse);
                switch (res.error)
                {
                    case "":
                        break;
                    case null:
                        break;
                    default:
                        //Logger.LogInfo(res.error.ToString());
                        return "Error - SMS Not sent !";
                }
                return strResponse;
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return "Error - SMS Function Not Working !" + ex.Message;
            }
        }
        public static string SendSMSOld1(string strMobileNumbers, string Message)
        {

            /*
                <add key="strUsernamePrefix" value="stec-" />
                <add key="strUsername" value="primabi" />
                <add key="strPassword" value="primabi" />
                <add key="strType" value="0" />
                <add key="strDelivery" value="1" />
                <add key="strSource" value="SQUARE" />
             */
            string strUsernamePrefix = ConfigurationManager.AppSettings["strUsernamePrefix"];
            string strUsername = ConfigurationManager.AppSettings["strUsername"];
            string strType = ConfigurationManager.AppSettings["strType"];
            string strDelivery = ConfigurationManager.AppSettings["strDelivery"];
            string strSource = ConfigurationManager.AppSettings["strSource"];
            string strPassword = ConfigurationManager.AppSettings["strPassword"];

            strType = string.IsNullOrEmpty(strType) ? "0" : strType;
            strDelivery = string.IsNullOrEmpty(strDelivery) ? "1" : strDelivery;
            strSource = string.IsNullOrEmpty(strSource) ? "PRIMAB" : strSource;

            if (strType == "2" || strType == "6")
            {
                Message = ConvertToUnicode(Message + " ");
            }

            string strMessage = HttpUtility.UrlEncode(Message);

            //string NEW strUrlNew = "http://sms.indiatext.in/api/mt/SendSMS?user=primabi&password=primabi&senderid=SQUARE&channel=Trans&DCS=0&flashsms=0&number=9960030007&text=test%20message&route=1";
            string strUrl = "http://sms.indiatext.in/api/mt/SendSMS?" +
                  "user=" + strUsername
                + "&password=" + strPassword
                + "&senderid=" + strSource
                + "&channel=" + "Trans"
                + "&DCS=" + 0
                + "&flashsms=" + 0
                + "&number=" + strMobileNumbers
                + "&text=" + strMessage
                + "&route=" + 1;

            try
            {
                WebRequest request = HttpWebRequest.Create(strUrl);
                request.Timeout = 25000;
                request.Proxy = null;

                WebResponse response = request.GetResponse();
                Stream str = response.GetResponseStream();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string strResponse = reader.ReadToEnd();

                var res = JsonConvert.DeserializeObject<SMSRootobject>(strResponse);
                switch (res.ErrorCode)
                {
                    case "000":
                        return "1";
                        
                    default:                        
                        return "Error - SMS Not sent !";
                }
                return strResponse;
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return "Error - SMS Function Not Working !" + ex.Message;
            }
        }


        public static string SendSMSOld2(string strMobileNumbers, string Message)
        {

            /*
                <add key="strUsernamePrefix" value="stec-" />
                <add key="strUsername" value="cyborgsy" />
                <add key="strPassword" value="cyborg123" />
                <add key="strType" value="0" />
                <add key="strDelivery" value="1" />
                <add key="strSource" value="PRIMAB" />
             */
            string strUsernamePrefix = ConfigurationManager.AppSettings["strUsernamePrefix"];
            string strUsername = ConfigurationManager.AppSettings["strUsername"];
            string strType = ConfigurationManager.AppSettings["strType"];
            string strDelivery = ConfigurationManager.AppSettings["strDelivery"];
            string strSource = ConfigurationManager.AppSettings["strSource"];
            string strPassword = ConfigurationManager.AppSettings["strPassword"];

            //Username with username prefix
            //strUsernamePrefix = string.IsNullOrEmpty(strUsernamePrefix) ? "stec-" : strUsernamePrefix;
            //strUsername = string.Concat(strUsernamePrefix, strUsername);
            strType = string.IsNullOrEmpty(strType) ? "0" : strType;
            strDelivery = string.IsNullOrEmpty(strDelivery) ? "1" : strDelivery;
            strSource = string.IsNullOrEmpty(strSource) ? "SQUARE" : strSource;

            if (strType == "2" || strType == "6")
            {
                Message = ConvertToUnicode(Message + " ");
            }

            string strMessage = HttpUtility.UrlEncode(Message);

            //string strUrlNew = "https://49.50.67.32/smsapi/httpapi.jsp?username=cyborgsy&password=cyborg123&from=PRIMAB&to=9960030007&text=This%20is%20test%20message%20from%20gunvant";

            string strUrl = "https://49.50.67.32/smsapi/httpapi.jsp?username=" + strUsername
                + "&password=" + strPassword
                + "&from=" + strSource
                + "&to=" + strMobileNumbers
                + "&text=" + strMessage;

            try
            {
                WebRequest request = HttpWebRequest.Create(strUrl);
                request.Timeout = 25000;
                request.Proxy = null;

                WebResponse response = request.GetResponse();
                Stream str = response.GetResponseStream();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string strResponse = reader.ReadToEnd();
                string[] strResponseArray = strResponse.Split(',');

                var xDoc = XDocument.Parse(strResponse);
                string Statuscode, Description, Ack_Id, MessageStatus;

                if (strResponse.Contains("Error"))
                {
                    Statuscode = xDoc.Root.Element("ErrorCode").Value;
                    Ack_Id = "";
                    Description = xDoc.Root.Element("ErrorDesc").Value;
                    MessageStatus = "Failed";
                }
                else
                {
                    Statuscode = "200";
                    Ack_Id = xDoc.Root.Element("ack_id").Value;
                    Description = xDoc.Root.Element("msgid").Value;
                    MessageStatus = "Success";
                }
                return strResponse;
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return "Error - SMS Function Not Working !" + ex.Message;
            }
        }

        public static string SendSMSOld(string strMobileNumbers, string Message)
        {
            /*
                <add key="strUsernamePrefix" value="stec-" />
                <add key="strUsername" value="handge" />
                <add key="strPassword" value="handge" />
                <add key="strType" value="0" />
                <add key="strDelivery" value="1" />
                <add key="strSource" value="HANDGE" />
             */
            //Username with username prefix
            string strUsernamePrefix = string.IsNullOrEmpty(ConfigurationManager.AppSettings["strUsernamePrefix"]) ? "stec-" : ConfigurationManager.AppSettings["strUsernamePrefix"];
            string strUsername = string.Concat(strUsernamePrefix, ConfigurationManager.AppSettings["strUsername"]);
            string strType = string.IsNullOrEmpty(ConfigurationManager.AppSettings["strType"]) ? "0" : ConfigurationManager.AppSettings["strType"];
            string strDelivery = string.IsNullOrEmpty(ConfigurationManager.AppSettings["strDelivery"]) ? "1" : ConfigurationManager.AppSettings["strDelivery"];
            string strSource = string.IsNullOrEmpty(ConfigurationManager.AppSettings["strSource"]) ? "shtech" : ConfigurationManager.AppSettings["strSource"];
            string strPassword = ConfigurationManager.AppSettings["strPassword"];

            if (strType == "2" || strType == "6")
            {
                Message = ConvertToUnicode(Message + " ");
            }

            string strMessage = HttpUtility.UrlEncode(Message);
            string strUrl = "http://103.16.101.52:8080/sendsms/bulksms?username=" + strUsername
                + "&password=" + strPassword
                + "&type=" + strType
                + "&dlr=" + strDelivery
                + "&destination=" + strMobileNumbers
                + "&source=" + strSource
                + "&message=" + strMessage;

            try
            {
                WebRequest request = HttpWebRequest.Create(strUrl);
                request.Timeout = 25000;
                request.Proxy = null;

                WebResponse response = request.GetResponse();
                Stream str = response.GetResponseStream();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string strResponse = reader.ReadToEnd();
                string[] strResponseArray = strResponse.Split(',');
                return strResponse;
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return "Error - SMS Function Not Working !" + ex.Message;
            }
        }

    }


    public class SMSRootobject
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string JobId { get; set; }
        public Messagedata[] MessageData { get; set; }
    }
    
    public class Messagedata
    {
        public string Number { get; set; }
        public string MessageId { get; set; }
    }

    public class RoundsmsReply
    {
        public object error { get; set; }
        public int[] msg_id { get; set; }
    }
}
public class SMSresults
{
    public Objresults objResults { get; set; }
}

public class Objresults
{
    public string Status { get; set; }
    public int Code { get; set; }
    public SMSData Data { get; set; }
}

public class SMSData
{
    public object error { get; set; }
    public int[] msg_id { get; set; }
}