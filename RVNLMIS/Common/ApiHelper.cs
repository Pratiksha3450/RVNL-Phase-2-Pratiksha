using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace RVNLMIS.Common
{
    public class ApiHelper
    {
        internal static object VerifyBaseToken(string baseToken)
        {
            return GlobalVariables.BaseToken == baseToken.Trim() ? "valid" : "invalid-BaseToken";
        }

        /// <summary>
        /// API fail reason object
        /// </summary>
        /// <param name="isValid"></param>
        /// <returns></returns>
        internal static Results ReturnFailAPIdata(string isValid)
        {
            Results objResults = new Results();
            switch (isValid)
            {
                case "Invalid_Signature":
                    objResults.StatusCode = "203";
                    objResults.Message = "Invalid request, Invalid_Signature!";
                    break;
                case "invalid-token":
                    objResults.StatusCode = "203";
                    objResults.Message = "Invalid request, invalid_token!";
                    break;
                case "invalid-BaseToken":
                    objResults.StatusCode = "203";
                    objResults.Message = "Invalid request, invalid_token!";
                    break;
                case "invalid_sendAt":
                    objResults.StatusCode = "205";
                    objResults.Message = "Invalid request, Signature expired!";
                    break;
                case "time_out":
                    objResults.StatusCode = "202";
                    objResults.Message = "Request time out, time_out!";
                    break;
            }
            return objResults;
        }

        /// <summary>
        /// APIs the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        internal static Results ApiException(Exception ex)
        {
            try
            {
                ApiLogger.LogInfo(ex);
            }
            catch (Exception e)
            {                
            }            
            Results objResults = new Results();
            objResults.Type = "Response";
            objResults.StatusCode = "204";
            objResults.Message = "Exception occured : " + ex.Message;
            return objResults;
        }

      

        public static string GenerateSignature(string userToken)
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Encoding.ASCII.GetBytes(userToken);
            string token = Convert.ToBase64String(time.Concat(key).ToArray());
            return Functions.Encrypt(token);
        }

        public static string ValidateSignature(string signature)
        {
            signature = Functions.Decrypt(signature);
            byte[] data = Convert.FromBase64String(signature);
            DateTime when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            if (when < DateTime.UtcNow.AddHours(-24))
            {
                // too old
                return "@";
            }
            return "!";
        }

        internal static string CreateUserToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }

    public class Results
    {
        public string Type { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    //Added by Pratiksha
    public class ApiModel<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

}