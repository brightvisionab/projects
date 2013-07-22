
using BrightVision.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.Events
{
    public class ContactInformationEvents
    {
        public class ContactInformationArgs
        {
            public CTContactDetails ContactDetail { get; set; }
            public bool IsNewContact { get; set; }
            public int NewContactId { get; set; }
        }

        public class OnAddNew
        {
            public ContactInformationArgs OnAddNewArgs { get; set; }
        }
        public class OnSave
        {
            public class ManageCampaignBooking
            {
                public ContactInformationArgs OnSaveArgs { get; set; }
            }
            public class ManageCampaignList
            {
                public ContactInformationArgs OnSaveArgs { get; set; }
            }
        }
        public class OnCancel
        {
            public ContactInformationArgs OnCancelArgs { get; set; }
        }
        public class OnDelete
        {
            public class ManageCampaignBooking
            {
                public ContactInformationArgs OnDeleteArgs { get; set; }
            }
            public class ManageCampaignList
            {
                public ContactInformationArgs OnDeleteArgs { get; set; }
            }
        }
    }
}
