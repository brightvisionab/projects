
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Xml;
using System.IO;
using System.Xml.XPath;
using System.Reflection;

namespace BrightVision.Common.Utilities
{
    public class XmlUtility
    {
        public class SubCampaignConfig {
            public string status { get; set; }
            public string field_checks { get; set; }
            public string description { get; set; }
            public bool selected { get; set; }
            public bool not_qualified_default { get; set; }
            public bool send_email { get; set; }
        }
        public static string ConvertToXml(string pXmlString)
        {
            string Result = "";
            MemoryStream mStream = null;
            XmlTextWriter writer = null;
            XmlDocument document = new XmlDocument();;

            try
            {
                document.LoadXml(pXmlString);
                mStream = new MemoryStream();
                writer = new XmlTextWriter(mStream, Encoding.Unicode);
                writer.Formatting = Formatting.Indented;

                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();
                mStream.Position = 0;

                StreamReader sReader = new StreamReader(mStream);
                String FormattedXML = sReader.ReadToEnd();
                Result = FormattedXML;
            }
            catch { }

            if (mStream != null)
                mStream.Close();
            if (writer != null)
                writer.Close();

            return Result;
        }
        public static string GetXmlNodeOuterData(string pXmlData, string pXPath)
        {
            try {
                XmlDocument _XmlDoc = new XmlDocument();
                _XmlDoc.LoadXml(pXmlData);
                XmlNodeList _XmlNodeList = _XmlDoc.SelectNodes(pXPath);
                return ConvertToXml(_XmlNodeList.Item(0).OuterXml);
            }
            catch {
                return null;
            }
        }
        public static string GetXmlNodeInnerData(string pXmlData, string pXPath)
        {
            try
            {
                XmlDocument _XmlDoc = new XmlDocument();
                _XmlDoc.LoadXml(pXmlData);
                XmlNodeList _XmlNodeList = _XmlDoc.SelectNodes(pXPath);
                return ConvertToXml(_XmlNodeList.Item(0).InnerXml);
            }
            catch
            {
                return null;
            }
        }
        public static string GetXmlNodeInnerText(string pXmlData, string pXPath, bool pDecodeNewLines = false)
        {
            try {
                XmlDocument _XmlDoc = new XmlDocument();
                _XmlDoc.LoadXml(pXmlData);
                XmlNodeList _XmlNodeList = _XmlDoc.SelectNodes(pXPath);

                if (pDecodeNewLines)
                    return _XmlNodeList.Item(0).InnerXml.Replace("{break}", Environment.NewLine);

                return _XmlNodeList.Item(0).InnerXml;
            }
            catch {
                return null;
            }
        }
        public static List<string> GetXmlNodeDataAsList(string pXmlData, string pXPath, ref int pSelectedItem, ref int pNotQualifiedDefault, ref int pSendEmail)
        {
            List<string> _XmlNodeData = new List<string>();
            pSelectedItem = 0;
            pNotQualifiedDefault = -1;
            pSendEmail = -1;
            try
            {
                XmlDocument _XmlDoc = new XmlDocument();
                _XmlDoc.LoadXml(pXmlData);
                XmlNodeList _XmlNodeList = _XmlDoc.SelectNodes(pXPath);
                int _index = 0;
                foreach (XmlElement _item in _XmlNodeList.Item(0).ChildNodes)
                {
                    if (_item.Attributes["selected"] != null)
                        pSelectedItem = _index;

                    if (_item.Attributes["not_qualified_default"] != null)
                        pNotQualifiedDefault = _index;

                    if (_item.Attributes["send_email"] != null)
                        pSendEmail = _index;

                    _XmlNodeData.Add(_item.InnerText);
                    _index++;
                }
                return _XmlNodeData;
            }
            catch
            {
                pSelectedItem = 0;
                return new List<string>();
            }
        }
        public static List<string> GetXmlNodeDataAsList(string pXmlData, string pXPath, ref int pSelectedItem, ref int pNotQualifiedDefault, ref int pSendEmail, ref List<string> pFieldChecks)
        {
            List<string> _XmlNodeData = new List<string>();
            pSelectedItem = 0;
            pNotQualifiedDefault = -1;
            pSendEmail = -1;
            try {
                XmlDocument _XmlDoc = new XmlDocument();
                _XmlDoc.LoadXml(pXmlData);
                XmlNodeList _XmlNodeList = _XmlDoc.SelectNodes(pXPath);
                int _index = 0;
                foreach (XmlElement _item in _XmlNodeList.Item(0).ChildNodes) {
                    if (_item.Attributes["selected"] != null)
                        pSelectedItem = _index;

                    if (_item.Attributes["not_qualified_default"] != null)
                        pNotQualifiedDefault = _index;

                    if (_item.Attributes["send_email"] != null)
                        pSendEmail = _index;

                    string _FieldChecks = string.Empty;
                    if (_item.Attributes["field_checks"] != null)
                        _FieldChecks = _item.Attributes["field_checks"].Value.ToString();

                    pFieldChecks.Add(_FieldChecks);
                    _XmlNodeData.Add(_item.InnerText);
                    _index++;
                }
                return _XmlNodeData;
            }
            catch {
                pSelectedItem = 0;
                return new List<string>();
            }
        }
        public static List<SubCampaignConfig> GetXmlNodeDataAsList(string pXmlData, string pXPath, bool pFieldCheckLineBreaks = false)
        {
            List<SubCampaignConfig> _XmlConfigData = new List<SubCampaignConfig>();
            SubCampaignConfig _Config = null;
            try {
                XmlDocument _XmlDoc = new XmlDocument();
                _XmlDoc.LoadXml(pXmlData);
                XmlNodeList _XmlNodeList = _XmlDoc.SelectNodes(pXPath);
                int _index = 0;
                foreach (XmlElement _item in _XmlNodeList.Item(0).ChildNodes) {
                    _XmlConfigData.Add(_Config = new SubCampaignConfig() {
                        status = _item.InnerText,
                        field_checks = _item.Attributes["field_checks"] != null ? (pFieldCheckLineBreaks ? _item.Attributes["field_checks"].Value.ToString().Replace(",", Environment.NewLine) : _item.Attributes["field_checks"].Value.ToString()) : string.Empty,
                        description = _item.Attributes["description"] != null ? _item.Attributes["description"].Value.ToString() : string.Empty,
                        selected = _item.Attributes["selected"] != null ? true : false,
                        not_qualified_default = _item.Attributes["not_qualified_default"] != null ? true : false,
                        send_email = _item.Attributes["send_email"] != null ? true : false
                    });

                    _index++;
                }
                return _XmlConfigData;
            }
            catch {
                return new List<SubCampaignConfig>();
            }
        }
        public static string RemoveInvalidXmlData(string pXmlData)
        {
            List<string> _InvalidXmlData = new List<string>();
            _InvalidXmlData.Add("&#x0B;");

            foreach (string _ToRemoveData in _InvalidXmlData)
                pXmlData = pXmlData.Replace(_ToRemoveData, "");

            return pXmlData;
        }
    }
}
