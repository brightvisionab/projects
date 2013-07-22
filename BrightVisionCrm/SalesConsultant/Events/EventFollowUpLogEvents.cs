
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using SalesConsultant.PublicProperties;
using SalesConsultant.Modules;
using BrightVision.Common.Business;
using SalesConsultant.Facade;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Events
{
    public class EventFollowUpLogEvents
    {
        public class OnSaveArguments {
            public event_followup_log EventLog { get; set; }
        }
        public class CompanyRemarksSaved
        {
            public class FrmSalesConsultant
            {
                public string CompanyRemarks { get; set; }
            }
            public class ManageCampaignBooking
            {
                public string CompanyRemarks { get; set; }
            }
        }
        public class AfterDelete
        {
            public int DeletedCallLogId { get; set; }
        }
        public class OnSelectContactGridMenuClick
        {
            public int ContactId { get; set; }
        }
        public class OnSave
        {
            public OnSaveArguments OnSaveArgs { get; set; }
        }
        public class OnWorkNurtureEvent
        {
            public CampaignBookingProperty.CampaignBoookingArguments OnWorkArgs { get; set; }
        }
    }
}
