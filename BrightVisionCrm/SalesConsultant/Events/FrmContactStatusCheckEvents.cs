
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Common.Utilities;

namespace SalesConsultant.Events
{
    public class FrmContactStatusCheckEvents
    {
        public class OnClose {
            public List<XmlUtility.SubCampaignConfig> ContactStatuses { get; set; }
        }
    }
}
