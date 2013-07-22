
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using SalesConsultant.PublicProperties;
using SalesConsultant.Modules;
using BrightVision.Common.Business;
using SalesConsultant.Facade;

namespace SalesConsultant.Events
{
    public class ManageCampaignBookingEvents
    {
        public class OnStatusChange
        {
        }
        public class OnSave
        {
            public class FrmSalesConsultant
            {
                public bool CalledFromSendEmail = false;
            }
            public class ManageCampaignList
            {
                public bool IsReleasedCancel = false;
            }
        }
        public class OnCompanyRelease
        {
        }

        public class OnDialogEditorSaved
        {
        }
        public class OnFormLoad
        {
            public CTScSubCampaignContactList ContactPerson { get; set; }
        }
        public class OnLoadNextCompany
        {
            public class CampaignList
            {
            }
        }
        public class OnLoadPreviousCompany
        {
            public class CampaignList
            {
            }
        }
        public class SetCallViewBarState
        {
            public bool State { get; set; }
        }
    }
}
