
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;

using BrightVision.Model;
using BrightVision.Reporting.Utility;
using BrightVision.Common.Utilities;
using BrightVision.Common.Classes;

namespace BrightVision.WebPortal.Facade
{
    public class FacadeCommon
    {
        public static string SendEmail(string pFile = "", ReportsProperty.SendEmailInfoCollection _SendMailCollection = null, serverside_report_requests _eftRequest = null)
        {
            string return_message = string.Empty;
            BrightVision.Mandrill.MandrillServerSide mandrillEx = new BrightVision.Mandrill.MandrillServerSide();
            mandrillEx.Key = ConfigurationManager.AppSettings["MandrillKey"].ToString();
            mandrillEx.FromName = _SendMailCollection.SenderName;
            mandrillEx.From = _SendMailCollection.SenderEmail;

            string mailTo = string.Empty;
            string mailCC = string.Empty;
            string mailBCC = string.Empty;
            string mailAttachmentUrl = !string.IsNullOrEmpty(pFile) ? "1" : string.Empty;
            string SMS = string.Empty;
            bool bolProceed = false;

            string xml = @"<message>
                            <sender_email>@@sender_mail</sender_email>
                            <email_to>@@emailTo</email_to>
                            <email_cc>@@emailCC</email_cc>
                            <email_bcc>@@emailBCC</email_bcc>
                            <subject>@@subject</subject>
                            <body>@@body</body>
                            <attachment_url>@@attachment_url</attachment_url>
                        </message>";

            /**
             * mail to.
             */
            for (int i = 0; i < _SendMailCollection.SendToContacts.Count; i++) {
                mandrillEx.TO.Add(FacadeCommon.IsNull(_SendMailCollection.SendToContacts[i].EmailAddress, ""), FacadeCommon.IsNull(_SendMailCollection.SendToContacts[i].Name, ""));
                if (mailTo != "")
                    mailTo += "\n";

                mailTo += "<email>" + _SendMailCollection.SendToContacts[i].EmailAddress + "</email>";
                bolProceed = true;

            }

            /**
             * mail bcc.
             */
            for (int i = 0; i < _SendMailCollection.BlindCarbonCopyContacts.Count; i++) {
                mandrillEx.BCC.Add(FacadeCommon.IsNull(_SendMailCollection.BlindCarbonCopyContacts[i].EmailAddress, ""), FacadeCommon.IsNull(_SendMailCollection.BlindCarbonCopyContacts[i].Name, ""));
                if (mailBCC != "")
                    mailBCC += "\n";

                mailBCC += "<email>" + _SendMailCollection.BlindCarbonCopyContacts[i].EmailAddress + "</email>";
                bolProceed = true;
            }


            xml = xml.Replace("@@sender_mail", mandrillEx.From)
                     .Replace("@@emailTo", mailTo)
                     .Replace("@@emailCC", mailCC)
                     .Replace("@@emailBCC", mailBCC)
                     .Replace("@@subject", _SendMailCollection.MailSubject)
                     .Replace("@@body", _SendMailCollection.MailContent)
                     .Replace("@@attachment_url", mailAttachmentUrl);

            long? message_log_id = null;
            message_log_id = FacadeCommon.SaveMail(xml, _eftRequest);

            if (bolProceed) {
                mandrillEx.Subject = _SendMailCollection.MailSubject;
                if (pFile != "")
                    mandrillEx.Attachment.Add("application/pdf", pFile);

                bool bolSend = mandrillEx.Send(ref return_message);
                if (bolSend)
                    UpdateMail(message_log_id, Guid.NewGuid().ToString(), return_message);
            }

            mandrillEx = null;
            return return_message;
        }
        public static string SendSMS(ReportsProperty.SendSmsInfoCollection _SendSmsCollection = null, serverside_report_requests _eftRequest = null)
        {
            /*Sample XML
             * <SMS>
	                <contact>
		                <name>Dan</name>
		                <number>+6394992380</number>
	                </contact>
	                <contact>
		                <name>Venus</name>
		                <number>+639069163504</number>
	                </contact>
                </SMS>;
             */

            bool bolProceed = false;
            string smsRecipients = "";
            string xml = "";
            for (int i = 0; i < _SendSmsCollection.SmsContacts.Count; i++) {
                if (xml != "") xml += "\n";
                xml += "<contact><name>" + _SendSmsCollection.SmsContacts[i].Name + "</name><number>" + _SendSmsCollection.SmsContacts[i].MobileNo + "</number></contact>";
                if (smsRecipients != "") smsRecipients += "\n";
                smsRecipients += _SendSmsCollection.SmsContacts[i].Name;
                bolProceed = true;
            }

            if (bolProceed) {
                xml = "<SMS>" + xml + "</SMS>";
                LogEventSMS(xml, _SendSmsCollection.SmsMessage, _SendSmsCollection.ComputerName, _SendSmsCollection.ComputerIP, _eftRequest);
            }

            return xml;
        }
        public static long? SaveMail(string pXml, serverside_report_requests _eftRequest)
        {
            string _Connection = ConfigurationManager.ConnectionStrings["DefaultEntityConnection"].ToString();
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(_Connection)) {
                int? requested_by = _eftRequest.requested_by;

                string[] _ids = _eftRequest.sub_campaign_ids.Split(',');
                string _SubCampaignIds = _ids[0];

                message_log _eftMessageLog = new message_log() {
                    message_type = (int)SendMail.eMailType.Send_Mail_To_Prospect,
                    sub_campaign_id = int.Parse(_SubCampaignIds),
                    company_id = _eftRequest.account_id,
                    user_id = _eftRequest.requested_by,
                    XML = pXml,
                    created_date = DateTime.Now
                };
                _efDbContext.message_log.AddObject(_eftMessageLog);
                _efDbContext.SaveChanges();
                _efDbContext.Detach(_eftMessageLog);
                return _eftMessageLog.id;
            }
        }
        public static void UpdateMail(long? message_log_id, string thread_id, string return_message)
        {
            string proc1 = "";
            if (return_message != "") proc1 = return_message.Split(':')[0];
            string _Connection = ConfigurationManager.ConnectionStrings["DefaultEntityConnection"].ToString();
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(_Connection)) {
                var objItem = _efDbContext.message_log.Where(i => i.id == message_log_id).FirstOrDefault();
                if (objItem != null) {
                    objItem.message_thred_id = thread_id;
                    objItem.sent_date = System.DateTime.Now;
                    objItem.process1 = proc1;
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(objItem);
                }
            }
        }
        public static void LogEventSMS(string pXML, string pMessage, string pComputerName, string pComputerIP, serverside_report_requests _eftRequest)
        {
            string Source = "BrightSales";
            string _Connection = ConfigurationManager.ConnectionStrings["DefaultEntityConnection"].ToString();
            int? requested_by = _eftRequest.requested_by;
            string[] _ids = _eftRequest.sub_campaign_ids.Split(',');
            string _SubCampaignIds = _ids[0];

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(_Connection)) {
                _efDbContext.event_log.AddObject(new event_log() {
                    event_id = (int)EventLog.EventTypes.SEND_SMS,
                    user_id = requested_by,
                    subcampaign_id = int.Parse(_SubCampaignIds),
                    account_id = _eftRequest.account_id,
                    contact_id = null,
                    local_datetime = DateTime.Now,
                    computer_name = pComputerName,
                    param1 = pXML,
                    param2 = pMessage,
                    param3 = Source,
                    param4 = _eftRequest.account_id.ToString(),
                    param5 = pComputerIP,
                    param6 = null
                });

                _efDbContext.SaveChanges();
            }
        }
        public static string IsNull(object obj, string defaultValue)
        {
            if (obj == null)
                return defaultValue;
            else
                return obj.ToString();
        }
    }
}