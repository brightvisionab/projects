
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.Events
{
    public class CampaignListEvents
    {
        //public class AccountInfoChange
        //{
        //}
        public class PrepareExtraDetail
        {
        }
        public class PrepareCampaignBooking
        {
        }
        public class GetLatestProperties
        {
            public bool ForWorkModePurpose { get; set; }
        }
        public class OnChangeView
        {
        }
    }
}
