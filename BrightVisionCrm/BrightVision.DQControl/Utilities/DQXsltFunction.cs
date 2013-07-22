using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text;
using System.Collections.Generic;

using BrightVision.DQControl.Business;
using BrightVision.Common.Utilities;
using System.IO;

namespace BrightVision.DQControl.Utilities
{
    public class DQXsltFunction
    {
        public string tolower(string str)
        {
            return str.ToLower();
        }
        public string trim(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return str.Trim();
        }
        public int len(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            return str.Length;
        }
        public bool contains(string str, string tmp)
        {
            return str.Contains(tmp);
        }
        public string substring(string str, int min)
        {
            if (str.Length > min && min > 0)
                return str.Substring(0, min);
            else
                return str;
        }
        public string getDropboxValue(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var val = DropboxValue.Instanciate(str);
            if (val != null)
                return val.SelectionValue + (!string.IsNullOrEmpty(val.OtherValue) ? ", " + val.OtherValue : "");
            return string.Empty;
        }
        public string getSmartTextValues(string str)
        {
            var values = SmartTextValue.Instanciate(str).OrderByDescending(a => a.CreationDate);
            StringBuilder strBuiler = new StringBuilder();
            foreach (var val in values) {
                //var datetime = DateTime.Parse(val.CreationDate).ToString("yyyy-MM-dd HH:mm");
                //strBuiler.AppendFormat("{0}, {1}: {2} {3}", datetime, val.CustomerContact, val.Comment.Trim(), Environment.NewLine);
                strBuiler.AppendFormat("{0}, {1}: {2} {3}", val.CreationDate, val.CustomerContact, val.Comment.Trim(), Environment.NewLine);
            }
            return strBuiler.ToString().TrimEnd(new char[] { '\r', '\n' });
        }
        public string getAccountSmartTextValues(string str, string contactTitle)
        {
            var values = SmartTextValue.Instanciate(str).OrderByDescending(a => a.CreationDate);
            StringBuilder strBuiler = new StringBuilder();
            foreach (var val in values)
            {
                //var datetime = DateTime.Parse(val.CreationDate).ToString("yyyy-MM-dd HH:mm");
                //strBuiler.AppendFormat("{0}, {1}, {2}:\n {3}{4}", datetime, val.CustomerContact, contactTitle, val.Comment.Trim(), Environment.NewLine + Environment.NewLine);
                strBuiler.AppendFormat("{0}, {1}, {2}:\n {3}{4}", val.CreationDate, val.CustomerContact, contactTitle, val.Comment.Trim(), Environment.NewLine + Environment.NewLine);
            }
            return strBuiler.ToString().TrimEnd(new char[] { '\r', '\n' });
        }
        public string getSmartTextValuesCustomer(string str)
        {
            var values = SmartTextValue.Instanciate(str).OrderByDescending(a => a.CreationDate);
            StringBuilder strBuiler = new StringBuilder();
            foreach (var val in values)
            {
                //var datetime = DateTime.Parse(val.CreationDate).ToString("yyyy-MM-dd HH:mm");
                //strBuiler.AppendFormat("{0}: {1} {2}", datetime, val.Comment.Trim(), Environment.NewLine);
                strBuiler.AppendFormat("{0}: {1} {2}", val.CreationDate, val.Comment.Trim(), Environment.NewLine);
            }
            return strBuiler.ToString().TrimEnd(new char[] { '\r', '\n' });
        }
        public string getSmartTextValuesCustomer(string str, string contactTitle)
        {
            var values = SmartTextValue.Instanciate(str).OrderByDescending(a => a.CreationDate);
            StringBuilder strBuiler = new StringBuilder();
            foreach (var val in values)
            {
                //var datetime = DateTime.Parse(val.CreationDate).ToString("yyyy-MM-dd HH:mm");
                //strBuiler.AppendFormat("{0}: \n{1}{2}", datetime, val.Comment.Trim(), Environment.NewLine + Environment.NewLine);
                strBuiler.AppendFormat("{0}: \n{1}{2}", val.CreationDate, val.Comment.Trim(), Environment.NewLine + Environment.NewLine);
            }
            return strBuiler.ToString().TrimEnd(new char[] { '\r', '\n' });
        }
        public string getAttendiesValue(string str)
        {
            var val = Attendie.Instanciate(str);
            if (val != null)
                return val.Name;
            return string.Empty;
        }
        public string replace(string str, string findString, string replacement)
        {
            return str.Replace(findString, replacement);
        }
        public string encodeName(string str)
        {
            //try
            //{

                return XmlConvert.EncodeLocalName(str);
                
            //}
            //catch { 
            
            //}
            //return "a";
        }
        public string decode(string str) {
            return System.Net.WebUtility.HtmlDecode(str);
        }
        public XPathNodeIterator toNodeSet(string str)
        {
            XDocument doc = XDocument.Parse(str);
            return doc.CreateNavigator().Select("/");

        }
        public XPathNodeIterator SmartTextValueToXmlNode(string str)
        {
            try
            {
                var values = SmartTextValue.Instanciate(str).OrderByDescending(a => a.CreationDate);
                SmartTextValuesContainer list = new SmartTextValuesContainer();
                list.Values = values.ToList();
                str = SerializeUtility.Serialize(list);
                XDocument doc = XDocument.Parse(str);
                return doc.CreateNavigator().Select("/");
            }
            catch {
                XDocument doc = new XDocument();
                return doc.CreateNavigator().Select("/");
            }
        }
        public string dateFormat(string val, string format) {
            try
            {
                return DateTime.Parse(val).ToString(format);
            }
            catch {
                return "";
            }
        }
        public string getScheduleValue(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            var val = ScheduleValue.Instanciate(str);
            if (val != null)
            {
                if (!string.IsNullOrEmpty(val.Description))
                    return val.Description;
            }
            return string.Empty;
        }
        public string TimeDifference(string first, string second, string format)
        {
            try
            {

                return string.Format(format, (DateTime.Parse(second) - DateTime.Parse(first)));
            }
            catch
            {
                return "";
            }
        }
    }

}
