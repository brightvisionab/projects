using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Events
{
    public class CallViewBarEvents
    {
        public class PhoneCallArguments {
            public CTScSubCampaignContactList ContactPerson { get; set; }
            public int? ContactId { get; set; }
            public string ContactNo { get; set; }
            public SelectionProperty.CallMethod CallMethod { get; set; }
        }
        public class PhoneCallStart
        {
            public class FrmSalesConsultant
            {
                public PhoneCallArguments PhoneCallArgs { get; set; }
            }
            public class CallLogBar
            {
                public PhoneCallArguments PhoneCallArgs { get; set; }
            }
            public class CallLogRemarks
            {
                public PhoneCallArguments PhoneCallArgs { get; set; }
            }
        }
        public class PhoneCallAttempt
        {
        }
    }
}
