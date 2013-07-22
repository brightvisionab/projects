using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BrightVision.Common.Business;
using BrightVision.Model;


namespace BrightVision.Mandrill
{
    public class MandrillEx
    {
        private string _key = ConfigManager.AppSettings["MandrillKey"];
        private static string _from = ConfigManager.AppSettings["MandrillAccount"];
        private static string _fromName = ConfigManager.AppSettings["MandrillAccountName"];
        public Dictionary<string, string> TO = new Dictionary<string, string>();
        public Dictionary<string, string> CC = new Dictionary<string, string>();
        public Dictionary<string, string> BCC = new Dictionary<string, string>();
        public Dictionary<string, string> Attachment = new Dictionary<string, string>();
        private string _subject = "";
        private string _messageHTML = null;
        private string _messageTEXT = "";
        


        public string Key
        {
            set { _key = value; }
            get { return _key; }
        }
        public string From
        {
            set { _from = value; }
            get { return _from; }
        }
        public string FromName
        {
            set { _fromName = value; }
            get { return _fromName; }
        }
        public string Subject
        {
            set { _subject = value; }
            get { return _subject; }
        }
        public string MessageHTML
        {
            set { _messageHTML = value; }
            get { return _messageHTML; }
        }
        public string MessageTEXT
        {
            set { _messageTEXT = value; }
            get { return _messageTEXT; }
        }

        
        private string GetKey
        {
            get { return "\"key\" : \"" + Key + "\" "; }
        }
        private string GetFrom
        {
            get { return "\"from_email\" : \"" + _from + "\", \"from_name\" : \"" + _fromName + "\" "; }
        }
        private string GetTo
        {
            //get { return "\"to\" : [{ \"email\": \"" + To + "\", \"name\": \"" + ToName + "\" }]"; }
            get
            {
                string _to = "";
                foreach (KeyValuePair<string, string> pair in TO)
                {
                    if (_to != "") _to += ", ";
                    _to += "{ \"email\": \"" + pair.Key + "\", \"name\": \"" + pair.Value + "\" }";
                }                

                return "\"to\" : [" +  _to + "]";
            }
        }
        private string GetCC
        {
            //get { return "\"to\" : [{ \"email\": \"" + To + "\", \"name\": \"" + ToName + "\" }]"; }
            get
            {
                string _cc = "";
                foreach (KeyValuePair<string, string> pair in CC)
                {
                    if (_cc != "") _cc += ", ";
                    _cc += "{ \"email\": \"" + pair.Key + "\", \"name\": \"" + pair.Value + "\" }";
                }

                return _cc;
            }
        }
        private string GetBCC
        {
            //get { return "\"to\" : [{ \"email\": \"" + To + "\", \"name\": \"" + ToName + "\" }]"; }
            get
            {
                string _bcc = "";
                foreach (KeyValuePair<string, string> pair in BCC)
                {
                    //if (_bcc != "") _bcc += ", ";
                    //_bcc += "{ \"email\": \"" + pair.Key + "\", \"name\": \"" + pair.Value + "\" }";
                    _bcc = ", \"bcc_address\" : \"" + pair.Key + "\" ";
                }

                return _bcc;
            }
        }
        private string GetAttachment
        {
            //get { return "\"to\" : [{ \"email\": \"" + To + "\", \"name\": \"" + ToName + "\" }]"; }
            get
            {
                if (Attachment.Count == 0) return "";

                string _attachments = "";
                foreach (KeyValuePair<string, string> pair in Attachment)
                {
                    if (_attachments != "") _attachments += ", ";
                    _attachments += "{ \"type\": \"" + pair.Key + "\", \"name\": \"" + Path.GetFileName(pair.Value) + "\", \"content\": \"" + EncodeBase64(pair.Value) + "\" }";
                }

                return ", \"attachments\" : [" + _attachments + "]";
            }
        }
        private string GetSubject
        {
            get { return "\"subject\" : \"" + Subject + "\" "; }
        }
        private string GetMessage
        {
            get 
            {
                string message = "\"message\": { ";
                
                if(MessageHTML != null) message += "\"html\" : \"" + MessageHTML + "\", ";
                else message += "\"text\" : \"" + MessageTEXT + "\", ";

                message += GetSubject + ", ";
                message += GetFrom + ", ";
                message += GetTo;
                message += GetBCC;
                message += GetAttachment;                

                message += "} ";

                return message;
            }
        }
        private string MessageSendRequestJSONTemplate
        {
            get
            {
                string template = "{";
                template += GetKey + ", ";
                template += GetMessage;
                template += "}";

                return template;
            }
        }

        public bool Send(long? message_log_id = null)
        {
            bool bolResult = true;
            string _id = "";
            string return_message = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mandrillapp.com/api/1.0/messages/send.json");
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json, text/javascript, */*";
                request.Method = "POST";
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                   // writer.Write("{\"key\" : \"azWL4nZUpDpJj79-ltELUA\"}");
                    writer.Write(MessageSendRequestJSONTemplate);
                }

                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                string json = "";

                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        json += reader.ReadLine();
                    }
                }

                string[] arrJson = ((json.TrimStart('[').TrimEnd(']')).Replace("},{", "};{")).Split(';');

                JObject o = null;
                string failedEmail = "";
                string okEmail = "";
                string queued = "";
                string others = "";
                for (int i = 0; i < arrJson.Length; i++)
                {
                    o = JObject.Parse(arrJson[i]);
                    string status = (string)o["status"];

                    try
                    {
                        _id = (string)o["_id"];
                    }
                    catch { }

                    if (status.ToLower() == "invalid")
                    {
                        failedEmail += (string)o["email"] + "\n";
                        return_message = status;
                    }
                    else if (status.ToLower() == "sent")
                    {
                        okEmail += (string)o["email"] + "\n";
                        return_message = status;
                    }
                    else if (status.ToLower() == "queued")
                    {
                        queued += (string)o["email"] + "\n";
                        return_message = status;
                    }
                    else
                    {
                        others = "Error sending mail";
                        return_message = "Error sending mail";
                    }
                }

                if (failedEmail != "")
                {
                    BrightVision.Common.UI.NotificationDialog.Error("Failed", "Failed send email to following:\n" + failedEmail);
                }

                if (others != "")
                {
                    BrightVision.Common.UI.NotificationDialog.Error("Error", others);
                }

                if (queued != "")
                {
                    BrightVision.Common.UI.NotificationDialog.Information("Success", "Email on queued:\n" + queued);
                }

                if (okEmail != "")
                {
                    BrightVision.Common.UI.NotificationDialog.Information("Success", "Successfully send email to following:\n" + okEmail);
                }
            }
            catch (Exception e)
            {
                BrightVision.Common.UI.NotificationDialog.Error("Error", "An error has encountered when trying to send email.\nPlease contact system administrator.");
                bolResult = false;
                return_message = "An error has encountered when trying to send email";            
            }

            if (message_log_id != null) UpdateMail(message_log_id, _id, return_message);

            return bolResult;
        }

        private string EncodeBase64(string file)
        {
            string encodedData = "";

            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    byte[] filebytes = new byte[fs.Length];
                    fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
                    encodedData = Convert.ToBase64String(filebytes, Base64FormattingOptions.None);
                }
                
                
            }
            catch
            {
                BrightVision.Common.UI.NotificationDialog.Error("Error", "An error has encountered when trying to read temporarily created pdf file.\nPlease contact system administrator.");
            }

            return encodedData;
        }

        private void UpdateMail(long? message_log_id, string thread_id, string return_message)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                var objItem = _efDbContext.message_log.Where(i => i.id == message_log_id).FirstOrDefault();
                if (objItem != null)
                {
                    objItem.message_thred_id = thread_id;
                    objItem.sent_date = System.DateTime.Now;
                    objItem.process1 = return_message;
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(objItem);
                }
            }
        }
    }
}
