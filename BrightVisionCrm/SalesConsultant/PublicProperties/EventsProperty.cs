using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.PublicProperties
{
    public class EventsProperty
    {
        private CampaignBookingProperty.CampaignBoookingArguments _CampaignBookingArgs = new CampaignBookingProperty.CampaignBoookingArguments();
        public CampaignBookingProperty.CampaignBoookingArguments CampaignBookingArgs {
            get { return _CampaignBookingArgs; }
            set { _CampaignBookingArgs = value; }
        }

        private bool _HasDataRows = false;
        public bool HasDataRows {
            get { return _HasDataRows; }
            set { _HasDataRows = value; }
        }
    }
}
