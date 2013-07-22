using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Common.Business;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.PublicProperties
{
    public class CurrentAccountContactProperty
    {
        public string BreadCrumb { get; set; }
        public string CompanyName { get; set; }
        public int ContactId { get; set; }
        public SelectionProperty.CampaignListMode CampaignListMode { get; set; }
        public SubCampaignAppointment CampaignBookingAppointment { get; set; }
        public int AccountLeadStatusSelectedIndex { get; set; }
        public int AccountStatusSelectedIndex { get; set; }
        public int ContactStatusSelectedIndex { get; set; }
        public int AccountLeadStatusNotQualifiedIndex { get; set; }
        public int AccountStatusNotQualifiedIndex { get; set; }
        public int ContactStatusNotQualifiedIndex { get; set; }
        public List<string> AccountLeadStatuses { get; set; }
        public List<string> AccountStatuses { get; set; }
        public List<string> ContactStatuses { get; set; }
        public bool IsEmptyList { get; set; }
    }
}
