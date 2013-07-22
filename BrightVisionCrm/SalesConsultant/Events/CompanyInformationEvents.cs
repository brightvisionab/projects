
using BrightVision.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.Events
{
    public class CompanyInformationEvents
    {
        public class CompanyInformationArgs {
            public account Account { get; set; }
            public string AccountSubCampaignRemarks { get; set; }
        }
        public class OnSave
        {
            public class CampaignExtraDetail
            {
                public CompanyInformationArgs OnSaveArgs { get; set; }
            }
            public class ManageCampaignBooking
            {
                public CompanyInformationArgs OnSaveArgs { get; set; }
            }
            public class ManageCampaignList
            {
                public CompanyInformationArgs OnSaveArgs { get; set; }
            }
            public class FrmSalesConsultant
            {
                public CompanyInformationArgs OnSaveArgs { get; set; }
            }
        }
        public class OnRefresh
        {
            public CompanyInformationArgs OnRefreshArgs { get; set; }
        }
    }
}
