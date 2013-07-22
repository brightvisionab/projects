
using System;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using BrightVision.DQControl.Utilities;
using System.Data.Objects;

namespace SalesConsultant.Business
{
    public class ObjectCollectedData
    {
        #region Business Methods
        public class DialogTextJsonData {
            public int DialogId { get; set; }
            public string DialogTextJson { get; set; }
        }

        public static DataTable GetCollectedData(int pSubCampaignId, int? pAccountId, int? pContactId, int? pCustomerId, bool? pCustomerOwned, bool? pBrightvisionOwned) 
        {
            if (pContactId == 0) 
                pContactId = null;

            if (pCustomerId == 0)
                pCustomerId = null;
            
            try {
                string dialogData = string.Empty, scheduleData = string.Empty, dialogJSON = string.Empty,
                       questionData = string.Empty, contactData = string.Empty, relationData = string.Empty, collectedData = string.Empty;
                string _AccountData = string.Empty;
                XElement dialogXml = new XElement("dialogs");
                object dataRecord = null;
           
                /**
                 * [@jeff 10.08.2012]
                 * get dialog xml data.
                 * stripped off on stored procedure since we need to process each dialog json data.
                 */
                List<dialog> _lstDialogs = new List<dialog>();
                if (pCustomerId != null) {
                    if (pCustomerId > 0) {
                        using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                            _lstDialogs = (
                                from objDialog in _efDbContext.dialogs
                                join objSubCampaign in _efDbContext.subcampaigns on objDialog.subcampaign_id equals objSubCampaign.id
                                join objCampaign in _efDbContext.campaigns on objSubCampaign.campaign_id equals objCampaign.id
                                join objAnswer in _efDbContext.answers on objDialog.id equals objAnswer.dialog_id
                                where objCampaign.customer_id == pCustomerId
                                   && objDialog.is_active == true
                                   && string.IsNullOrEmpty(objDialog.dialog_text_json) == false
                                   && (objAnswer.MGC == false || objAnswer.MGC == null)
                                   || (objCampaign.customer_id != pCustomerId && objAnswer.OwnershipBrightvision == true)
                                select objDialog
                            ).Distinct().ToList();
                        }
                    }
                }

                if (_lstDialogs.Count > 0) {
                    foreach (dialog _item in _lstDialogs) {
                        try {
                            dialogJSON = _item.dialog_text_json;
                            string _dialog_id = _item.id.ToString();
                            dialogJSON = JsonConvert.DeserializeXmlNode("{\"json_dialog\" : {\"item\" : " + dialogJSON + "}} ").OuterXml;
                            var xDialogJSON = XElement.Parse(dialogJSON);
                            xDialogJSON.Add(new XAttribute("dialog_id", _dialog_id));
                            dialogJSON = xDialogJSON.ToString();
                            dialogXml.Add(XElement.Parse(dialogJSON));
                        }
                        catch { 
                        }
                    }
                }


                IDataReader xmlData = DatabaseUtility.GetCollectedData(pSubCampaignId, pAccountId, pContactId, pCustomerId, pCustomerOwned, pBrightvisionOwned);
                if (xmlData != null) {
                    //read dialog data

                    /** /
                    //string dialogid = string.Empty;
                    //while (xmlData.Read())
                    //    dialogid = xmlData["id"].ToString();

                    //if (xmlData.NextResult()) {
                    while (xmlData.Read()) {
                        //dataRecord = xmlData["dialog_text_json"].ToString();
                        if (!dataRecord.Equals(System.DBNull.Value)) {
                            try {
                                //dialogJSON = (string)xmlData["dialog_text_json"];
                                dialogJSON = (string)xmlData["dialog_json_data"];
                                string _dialog_id = xmlData["id"].ToString();
                                //dialogJSON = JsonConvert.DeserializeXmlNode("{\"json_dialog\" : {\"item\" : " + dialogJSON + "}} ").OuterXml;
                                dialogJSON = JsonConvert.DeserializeXmlNode(string.Format("{\"json_dialog\" : {\"item\" : {0}}} ", dialogJSON)).OuterXml;
                                var xDialogJSON = XElement.Parse(dialogJSON);
                                //string dialogid = xmlData["id"].ToString();
                                //xDialogJSON.Add(new XAttribute("dialog_id", dialogid));
                                xDialogJSON.Add(new XAttribute("dialog_id", _dialog_id));
                                dialogJSON = xDialogJSON.ToString();
                                dialogXml.Add(XElement.Parse(dialogJSON));
                            }
                            catch { }
                        }
                    }
                    //}
                    /**/

                    while (xmlData.Read()) {
                        dataRecord = xmlData["accountdata"];
                        if (!dataRecord.Equals(System.DBNull.Value))
                            _AccountData = (string)xmlData["accountdata"];
                    }

                    if (xmlData.NextResult()) {
                        while (xmlData.Read()) {
                            dataRecord = xmlData["dialogdata"];
                            if (!dataRecord.Equals(System.DBNull.Value))
                                dialogData = (string)xmlData["dialogdata"];
                        }
                    }
                    if (xmlData.NextResult()) {
                        while (xmlData.Read()) {
                            dataRecord = xmlData["questiondata"];
                            if (!dataRecord.Equals(System.DBNull.Value))
                                questionData = (string)xmlData["questiondata"];
                        }
                    }
                    if (xmlData.NextResult()) {
                        while (xmlData.Read()) {
                            dataRecord = xmlData["scheduledata"];
                            if (!dataRecord.Equals(System.DBNull.Value))
                                scheduleData = (string)xmlData["scheduledata"];
                        }
                    }
                    if (xmlData.NextResult()) {
                        while (xmlData.Read()) {
                            dataRecord = xmlData["relationdata"];
                            if (!dataRecord.Equals(System.DBNull.Value))
                                relationData = (string)xmlData["relationdata"];
                        }
                    }                    
                    if (xmlData.NextResult()) {
                        while (xmlData.Read()) {
                            dataRecord = xmlData["contactdata"];
                            if (!dataRecord.Equals(System.DBNull.Value))
                                contactData = (string)xmlData["contactdata"];
                        }
                    }
                    if (xmlData.NextResult()) {
                        while (xmlData.Read()) {
                            dataRecord = xmlData["collecteddata"];
                            if (!dataRecord.Equals(System.DBNull.Value))
                                collectedData = (string)xmlData["collecteddata"];
                        }
                    }
                }
                

                var xelem = new XElement("data");
                if(dialogXml.Descendants().Count()>0) 
                    xelem.Add(dialogXml);
                //if (!string.IsNullOrEmpty(dialogJSON))
                //    xelem.Add(XElement.Parse(dialogJSON));
                if (!string.IsNullOrEmpty(_AccountData))
                    xelem.Add(XElement.Parse(XmlUtility.RemoveInvalidXmlData(_AccountData)));
                if (!string.IsNullOrEmpty(dialogData))
                    xelem.Add(XElement.Parse(XmlUtility.RemoveInvalidXmlData(dialogData)));
                if (!string.IsNullOrEmpty(questionData))
                    xelem.Add(XElement.Parse(XmlUtility.RemoveInvalidXmlData(questionData)));
                if (!string.IsNullOrEmpty(scheduleData))
                    xelem.Add(XElement.Parse(XmlUtility.RemoveInvalidXmlData(scheduleData)));
                if (!string.IsNullOrEmpty(relationData))
                    xelem.Add(XElement.Parse(XmlUtility.RemoveInvalidXmlData(relationData)));                
                if (!string.IsNullOrEmpty(contactData))
                    xelem.Add(XElement.Parse(XmlUtility.RemoveInvalidXmlData(contactData)));
                if (!string.IsNullOrEmpty(collectedData))
                    xelem.Add(XElement.Parse(XmlUtility.RemoveInvalidXmlData(collectedData)));
                var xmlSource = xelem.ToString();

                var xsltExt = new DQXsltFunction();
                var xsltExtParam = new XsltExtensionParam {
                    Namespace = "util:xsltextension",
                    Object = xsltExt
                };
                string xsl = Properties.Resources.collected_data;
                string xmloutput = XsltUtility.GetXmlString(
                    xsl,
                    xmlSource,
                    new List<XsltExtensionParam> { xsltExtParam },
                    true,
                    true);
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(new System.IO.StringReader(xmloutput));
                return dataSet.Tables[0];
            } catch (Exception e) {
                NotificationDialog.Information("System Error ...", e.InnerException.Message);
                return null;
            }
        }
        #endregion
    }
}
