using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Events
{
    public class MyFollowUpsEvents
    {
        public class OnCampaignBookingLoad
        {
            public CampaignBookingProperty.CampaignBoookingArguments OnCampaignBookingLoadArgs { get; set; }
        }
    }
}
