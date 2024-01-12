using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace RVNLMIS.Common
{
    public class PushNotification
    {
        public static string PushNotify(string Message, string TagMsg, string FCMToken, string type)
        {
            string result = "";
            try
            {
                switch (type)
                {
                    case "FA": // firebase Android
                        string applicationID = "AAAAwfRAsMk:APA91bGGQaqOJIIHM1DWtNdvlBqW6aK5DJIbZIuy5KL24c7jKVwThVILqlXAGozR39Ztx4ETp9-7rGX-P5tLrrHiXbp79azcf1k_KuppiQs3XOtb4HvuTkLb8Y-mkefo5CT4ZcLNH1E-";
                        string senderId = "833026568393";
                        WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                        tRequest.Method = "post";
                        tRequest.ContentType = "application/json";

                        var data = new
                        {
                            to = FCMToken,
                            notification = new
                            {
                                body = Message,
                                title = TagMsg,
                                icon = "myicon"
                            }
                        };

                        var serializer = new JavaScriptSerializer();
                        var json = serializer.Serialize(data);

                        Byte[] byteArray = Encoding.UTF8.GetBytes(json);
                        tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));
                        tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
                        tRequest.ContentLength = byteArray.Length;

                        using (Stream dataStream = tRequest.GetRequestStream())
                        {
                            dataStream.Write(byteArray, 0, byteArray.Length);
                            using (WebResponse tResponse = tRequest.GetResponse())
                            {
                                using (Stream dataStreamResponse = tResponse.GetResponseStream())
                                {
                                    using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                    {
                                        String sResponseFromServer = tReader.ReadToEnd();
                                        result = sResponseFromServer;
                                    }
                                }
                            }
                        }
                        break;
                    case "FI": // firebase iOS
                        Uri FireBasePushNotificationsURL = new Uri("https://fcm.googleapis.com/fcm/send");
                        string ServerKey = "AAAAz3w_yVA:APA91bGS3IznFf3GpcqBZUQYq4Xw-R7uJv4ipJ2YonoAOPuZPZfAMCFg4nsXb7xlCwJE1pOQgbutlPD6pTnHmzmbT1hPWDssYCC8nVta1vzYMTq--to1PFIBlwTzldlIXMtQhKmSO50O";
                        var datas = new { action = "Play", userId = 5 };
                        bool sent = false;
                        //Object creation
                        string[] deviceTokens = { FCMToken };
                        var messageInformation = new Message()
                        {
                            notification = new Notification()
                            {
                                title = TagMsg,
                                text = Message
                            },
                            data = datas,
                            registration_ids = deviceTokens
                        };

                        //Object to JSON STRUCTURE => using Newtonsoft.Json;
                        string jsonMessage = JsonConvert.SerializeObject(messageInformation);

                        /*
                         ------ JSON STRUCTURE ------
                         {
                            notification: {
                                            title: "",
                                            text: ""
                                            },
                            data: {
                                    action: "Play",
                                    playerId: 5
                                    },
                            registration_ids = ["id1", "id2"]
                         }
                         ------ JSON STRUCTURE ------
                         */

                        //Create request to Firebase API
                        var request = new HttpRequestMessage(HttpMethod.Post, FireBasePushNotificationsURL);

                        request.Headers.TryAddWithoutValidation("Authorization", "key=" + ServerKey);
                        request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

                        HttpResponseMessage resulta;
                        using (var client = new HttpClient())
                        {
                            resulta = Task.Run(() => client.SendAsync(request)).Result;                            
                            sent = sent && resulta.IsSuccessStatusCode;
                        }
                        break;

                    case "OA": // one signal android                        
                        PushOneSignal(Message, FCMToken, TagMsg);                        
                        break;

                }
            }
            catch (Exception ex)
            {
                result = "-1";
            }
            return result;
        }

        private static void PushOneSignalToAll(string message, string fCMToken, string tagMsg)
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            request.Headers.Add("authorization", "Basic NjdiMjUyODgtNWZkNy00MDdjLWFkODEtYTc0YmQ1YmFlNTMy");

            byte[] byteArray = Encoding.UTF8.GetBytes("{"
                                                    + "\"app_id\": \"2fb1b07a-53c8-44ad-b8ea-281f693f1fda\","                                                    
                                                    + "\"contents\": {\"en\": \""+message+"\"},"
                                                    + "\"headings\": {\"en\": \""+tagMsg+"\"},"
                                                    + "\"included_segments\": [\"All\"]}"); 
            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            System.Diagnostics.Debug.WriteLine(responseContent);
        }

        private static void PushOneSignal(string message, string fCMToken, string tagMsg)
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            var serializer = new JavaScriptSerializer();
            var obj = new
            {
                app_id = "2fb1b07a-53c8-44ad-b8ea-281f693f1fda",
                contents = new { en = message },
                headings= new { en = tagMsg},
                include_player_ids = new string[] { fCMToken }  //include_external_user_ids
            };



            var param = serializer.Serialize(obj);
            byte[] byteArray = Encoding.UTF8.GetBytes(param);

            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            Console.WriteLine(responseContent);
        }
    }

    public class Message
    {
        public string[] registration_ids { get; set; }
        public Notification notification { get; set; }
        public object data { get; set; }
    }

    public class Notification
    {
        public string title { get; set; }
        public string text { get; set; }
    }
}