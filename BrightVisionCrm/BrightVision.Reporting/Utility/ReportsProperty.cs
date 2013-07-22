
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using System.Collections;

namespace BrightVision.Reporting.Utility
{
    public class ReportsProperty
    {
        [Serializable()]
        public class SortInfoCollection
        {
            public List<SortInfoProperty> SortInfo = new List<SortInfoProperty>();
        }
        public class SortInfoProperty
        {
            public string Column { get; set; }
            public string SortOrder { get; set; }
        }

        [Serializable()]
        public class ColumnsInfoCollection
        {
            public List<string> HiddenColumnsInfo = new List<string>();
        }

        [Serializable()]
        public class SendEmailInfoCollection
        {
            public string SenderEmail = string.Empty;
            public string SenderName = string.Empty;
            public string MailSubject = string.Empty;
            public string MailContent = string.Empty;
            public List<SendEmailRecipientInfoProperty> SendToContacts = new List<SendEmailRecipientInfoProperty>();
            public List<SendEmailRecipientInfoProperty> CarbonCopyContacts = new List<SendEmailRecipientInfoProperty>();
            public List<SendEmailRecipientInfoProperty> BlindCarbonCopyContacts = new List<SendEmailRecipientInfoProperty>();
        }
        public class SendEmailRecipientInfoProperty
        {
            public string Name = string.Empty;
            public string EmailAddress = string.Empty;
        }

        [Serializable()]
        public class SendSmsInfoCollection
        {
            public string SmsMessage = string.Empty;
            public string ComputerName = string.Empty;
            public string ComputerIP = string.Empty;
            public List<SendSmsRecipientInfoProperty> SmsContacts = new List<SendSmsRecipientInfoProperty>();
        }
        public class SendSmsRecipientInfoProperty
        {
            public string Name = string.Empty;
            public string MobileNo = string.Empty;
        }
    }
}
