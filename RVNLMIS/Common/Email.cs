using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Script.Serialization;
namespace RVNLMIS.Common
{
    public class Email
    {
        public static string _From;
        public static string _FromName;
        public static string _CompanyName;
        public static string _Smtp;
        public static string _Password;
        public static string _Host;
        public static string _SupportFrom;
        public static string _SupportTo;

        #region --Class Constructor--
        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        public Email()
        {
            _From = ConfigurationManager.AppSettings["FROM"];
            _FromName = ConfigurationManager.AppSettings["FROMNAME"];
            _CompanyName = ConfigurationManager.AppSettings["COMPANYNAME"];
            _Smtp = ConfigurationManager.AppSettings["SMTPPORT"];
            _Password = ConfigurationManager.AppSettings["PASSWORD"];
            _Host = ConfigurationManager.AppSettings["HOST"];
            _SupportFrom = ConfigurationManager.AppSettings["SUPPORTFROM"];
            _SupportTo = ConfigurationManager.AppSettings["SUPPORTTO"];
        }
        #endregion

        #region --Public Methods--

        #region -- Set Email Body --
        /// <summary>
        /// Reads the email template.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string SetEmailBody(string path, OrderedDictionary myOrderedDictionary)
        {
            string message = string.Empty;

            try
            {
                StreamReader streamreader = new StreamReader(path);
                string template, stream;
                template = "";
                stream = streamreader.ReadLine();
                while (stream != null)
                {
                    stream = streamreader.ReadLine();
                    template += stream;
                }

                streamreader.Close();
                message = template;
                foreach (DictionaryEntry de in myOrderedDictionary)
                {
                    message = message.Replace("#" + de.Key.ToString() + "#", de.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                // Logger.LogInfo(ex);
            }

            return message;
        }
        #endregion

        #region -- Set Template Parameters --
        /// <summary>
        /// Reads the email template.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string SetTemplate(string message, OrderedDictionary myOrderedDictionary)
        {
            try
            {
                foreach (DictionaryEntry de in myOrderedDictionary)
                    message = message.Replace("#" + de.Key.ToString() + "#", de.Value.ToString());
                return message;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion

        #region -- Set Email Subject --
        /// <summary>
        /// Reads the email template.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string SetSubject(string subject)
        {
            return subject = subject.Replace("#COMPANY_NAME#", _CompanyName);
        }
        #endregion

        #region -- Send Email --
        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="emailId">The email id.</param>
        /// <param name="ccAddress">The cc address.</param>
        /// <param name="bccAddress">The BCC address.</param>
        /// <param name="attachments">The attachments.</param>
        /// <param name="msgTitle">The MSG title.</param>
        /// <param name="msgBody">The MSG body.</param>
        /// <returns></returns>
        public int SendMail(String emailId, List<string> ccAddress, List<string> bccAddress, List<Attachment> attachments, String msgTitle, String msgBody, string fromMail)
        {
            try
            {
                MailMessage messsage;
                SmtpClient smtpClient;

                messsage = new MailMessage();
                messsage.Subject = msgTitle;
                messsage.From = new MailAddress(fromMail, _FromName);
                messsage.Priority = MailPriority.Low;
                smtpClient = new SmtpClient();
                smtpClient.Credentials = new NetworkCredential(fromMail, _Password);
                smtpClient.Port = Convert.ToInt32(Convert.ToString((_Smtp)));
                smtpClient.Host = (_Host);
                smtpClient.EnableSsl = false;

                if (emailId != null)
                {
                    //Attaching multitple emails
                    string[] emailIds = emailId.Split(',');

                    messsage.To.Clear();

                    for (int index = 0; index < emailIds.Length; index++)
                    {
                        if (!string.IsNullOrEmpty(emailIds[index]))
                        {
                            MailAddress mailAddress = new MailAddress(emailIds[index].Trim());
                            messsage.To.Add(mailAddress);
                        }
                    }
                }
                //smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;

                if (ccAddress != null)
                {
                    for (int i = 0; i < ccAddress.Count; i++)
                    {
                        messsage.CC.Add(ccAddress[i]);
                    }
                }
                // messsage.CC.Add("shankar.avhad@rediffmail.com");
                if (bccAddress != null)
                {
                    for (int i = 0; i < bccAddress.Count; i++)
                    {
                        messsage.Bcc.Add(bccAddress[i]);
                    }
                }
                if (attachments != null)
                {
                    for (int i = 0; i < attachments.Count; i++)
                    {
                        messsage.Attachments.Add(attachments[i]);
                    }
                }

                // Create the mailing List 


                messsage.Body = msgBody;
                System.IO.Stream stream = new System.IO.MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(messsage.Body));
                AlternateView alternate = new AlternateView(stream, new System.Net.Mime.ContentType("text/html"));
                messsage.AlternateViews.Add(alternate);

                smtpClient.Send(messsage);

               // Logger.LogInfo("Mail Response to : " + emailId + " is success");
                return (1);
            }
            catch (Exception ex)
            {
               // Logger.LogInfo("Failed to send mail to- " + emailId);
                Logger.LogInfo(ex);
                return (0);
            }
        }


        #endregion

        #endregion
    }
}