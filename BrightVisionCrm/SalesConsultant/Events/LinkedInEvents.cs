
using BrightVision.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.Events
{
    public class LinkedInEvents
    {
        public class LinkedInArgs
        {
            public string LinkedInUrl { get; set; }
        }


        public class OnSave
        {
            public class ManageCampaignBooking
            {
                public LinkedInArgs OnSaveArgs { get; set; }
            }
            public class ManageCampaignList
            {
                public LinkedInArgs OnSaveArgs { get; set; }
            }
        }
    }
}
