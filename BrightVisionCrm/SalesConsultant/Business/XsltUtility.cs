using System;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;

using Mvp.Xml;
using Mvp.Xml.Common;
using Mvp.Xml.Common.Xsl;
using Mvp.Xml.Exslt;

using BrightVision.DQControl.Business;

/// <summary>
/// Summary description for XsltUtility
/// </summary>
/// 
namespace SalesConsultant.Business {
    public class XsltUtility {
        public static string GetXmlString(string stylesheet, string xmlstring, List<XsltExtensionParam> listExtensionParam,
            bool enableDocumentFunction, bool enableScript) {

            MvpXslTransform xslt = new MvpXslTransform();
            string text = string.Empty;
            try {
                using (XmlReader strm = XmlReader.Create(new MemoryStream(ASCIIEncoding.Default.GetBytes(stylesheet)))) {
                    if (enableDocumentFunction && enableScript) {
                        XsltSettings xslSettings = new XsltSettings(enableDocumentFunction, enableScript);
                        XmlResolver xmlResolver = new XmlUrlResolver();
                        xslt.Load(strm, xslSettings, xmlResolver);
                    } else if (enableDocumentFunction && !enableScript) {
                        XsltSettings xslSettings = new XsltSettings(enableDocumentFunction, false);
                        XmlResolver xmlResolver = new XmlUrlResolver();
                        xslt.Load(strm, xslSettings, xmlResolver);
                    } else {
                        xslt.Load(strm);
                    }
                }
                using (TextReader stringReader = new StringReader(xmlstring)) {
                    XPathDocument mydata = new XPathDocument(stringReader);
                    XmlInput a = new XmlInput(mydata);
                    using (TextWriter txtWriter = new StringWriter()) {
                        XsltArgumentList b = new XsltArgumentList();
                        foreach (XsltExtensionParam oep in listExtensionParam) {
                            b.AddExtensionObject(oep.Namespace, oep.Object);
                        }
                        xslt.Transform(a, b, new XmlOutput(txtWriter));
                        text = txtWriter.ToString();
                    }
                }
            } catch (Exception ex) {

            }
            return text;
        }
    }
    public class XsltExtension {
        public string tolower(string str) {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return str.ToLower();
        }
        public string trim(string str) {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return str.Trim();
        }
        public bool contains(string str, string tmp) {            
            return str.Contains(tmp);
        }
        public int len(string str) {
            if(string.IsNullOrEmpty(str)) return 0;
            return str.Length;
        }
        public string substring(string str, int min) {
            if (str.Length > min && min > 0)
                return str.Substring(0, min);
            else
                return str;
        }
        public string getDropboxValue(string str) {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            var val = DropboxValue.Instanciate(str);
            if (val != null) {
                if(val.SelectionValue != null)
                    return val.SelectionValue + (!string.IsNullOrEmpty(val.OtherValue) ? ", " + val.OtherValue : "");
            }
            return string.Empty;
        }
        public string getSmartTextValues(string str) {
            var values = SmartTextValue.Instanciate(str).OrderByDescending(a => a.CreationDate);
            StringBuilder strBuiler = new StringBuilder();
            foreach (var val in values) {              
                var datetime = DateTime.Parse(val.CreationDate).ToString("yyyy-MM-dd HH:mm");
                strBuiler.AppendFormat("{0}, {1}: {2} {3}", datetime, val.CustomerContact, val.Comment.Trim(), "\r\n");
            }
            return strBuiler.ToString().TrimEnd(new char[] { '\r', '\n' });
        }
        public string getSmartTextValuesCustomer(string str) {
            var values = SmartTextValue.Instanciate(str).OrderByDescending(a=>a.CreationDate);
            StringBuilder strBuiler = new StringBuilder();
            foreach (var val in values) {
                var datetime = DateTime.Parse(val.CreationDate).ToString("yyyy-MM-dd HH:mm");
                strBuiler.AppendFormat("{0}: {1} {2}", datetime, val.Comment.Trim(), "\r\n");
            }
            return strBuiler.ToString().TrimEnd(new char[] { '\r', '\n' });
        }
        public string getScheduleValue(string str) {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            var val = ScheduleValue.Instanciate(str);
            if (val != null) {
                if(!string.IsNullOrEmpty(val.Description))
                    return val.Description;
            }
            return string.Empty;
        }
        public string getAttendiesValue(string str) {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            var val = Attendie.Instanciate(str);
            if (val != null) {
                if (!string.IsNullOrEmpty(val.Name))
                    return val.Name;
            }
            return string.Empty;
        }
        public string replace(string str, string findString, string replacement) {
            return str.Replace(findString, replacement);
        }
        public string encodeName(string str) {
            return XmlConvert.EncodeName(str);
        }
        public XPathNodeIterator toNodeSet(string str) {
            XDocument doc = XDocument.Parse(str);
            return doc.CreateNavigator().Select("/");

        }
    }
    public class XsltExtensionParam {
        public string Namespace { get; set; }
        public object Object { get; set; }
    }
}