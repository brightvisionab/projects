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


namespace BrightVision.Mandrill
{
    public class Mandrill
    {
        private string _key = ConfigManager.AppSettings["MandrillKey"];
        private string _from = ConfigManager.AppSettings["MandrillAccount"];
        private string _fromName = ConfigManager.AppSettings["MandrillAccountName"];
        private string _to = "";
        private string _toName = "Unknown";
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

        public string To
        {
            set { _to = value; }
            get { return _to; }
        }
        public string ToName
        {
            set { _toName = value; }
            get { return _toName; }
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
            get { return "\"to\" : [{ \"email\": \"" + To + "\", \"name\": \"" + ToName + "\" }]"; }
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

        public bool Send()
        {
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

                JObject o = JObject.Parse(json.TrimStart('[').TrimEnd(']'));
                string status = (string)o["status"];
                if (status.ToLower() == "invalid")
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public string SendMail()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mandrillapp.com/api/1.0/users/info.json");
            request.ContentType = "application/json; charset=utf-8";
            request.Accept = "application/json, text/javascript, */*";
            request.Method = "POST";
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write("{\"key\" : \"azWL4nZUpDpJj79-ltELUA\"}");
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

            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            var aa = jss.DeserializeObject(json);

            /*
            var values = new Dictionary<string, object>();
            values.Add("Title", "Hello World!");
            values.Add("Text", "My first post");
            values.Add("Tags", new[] { "hello", "world" });

            var post = new DynamicEntity(values);

            dynamic dynPost = post;
            var text = dynPost.Text;
            */


            InfoJSON ij = new InfoJSON();
            //ij.stats.today.sent = "11";
            //ij.public_id = "1";

            return json;
        }

    }
}
