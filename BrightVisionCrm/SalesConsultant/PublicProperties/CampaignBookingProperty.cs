
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using SalesConsultant.PublicProperties;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;

namespace SalesConsultant.PublicProperties
{    
    public class CampaignBookingProperty
    {
        #region Public Properties
        public QuestionniareProperty Questionnaire = new QuestionniareProperty();

        private List<CTScSubCampaignContactList> _ContactList = new List<CTScSubCampaignContactList>();
        public List<CTScSubCampaignContactList> ContactList {
            get { return _ContactList; }
            set { _ContactList = value; }
        }

        private int _ContactCount = 0;
        public int ContactCount {
            get { return _ContactCount; }
            set { _ContactCount = value; }
        }

        private string _CampaignInformation = string.Empty;
        public string CampaignInformation {
            get { return _CampaignInformation; }
            set { _CampaignInformation = value; }
        }

        private string _ToolTipInformation = string.Empty;
        public string ToolTipInformation {
            get { return _ToolTipInformation; }
            set { _ToolTipInformation = value; }
        }

        private SelectionProperty.CampaignBookingMode _Mode = SelectionProperty.CampaignBookingMode.None;
        public SelectionProperty.CampaignBookingMode Mode {
            get { return _Mode; }
            set { _Mode = value; }
        }

        private SelectionProperty.ParentView _ParentView = SelectionProperty.ParentView.None;
        public SelectionProperty.ParentView ParentView {
            get { return _ParentView; }
            set { _ParentView = value; }
        }

        public AppointmentProperty Appointment { get; set; }
        public AccountStatusProperty AccountStatus = null;
        public ContactStatusProperty ContactStatus = null;

        public string BreadCrumb { get; set; }

        public bool LoadNextCompanySuccess { get; set; }
        public bool LoadPreviousCompanySuccess { get; set; }
        #endregion

        #region Properties
        public class CampaignBoookingArguments {
            public int Id { get; set; }
            public string CustomerName { get; set; }
            public string CampaignName { get; set; }
            public string SubCampaignName { get; set; }
            public int ContactId { get; set; }
            public string CompanyName { get; set; }
            public string City { get; set; }
            public string BreadCrumb { get; set; }
            public string CampaignInfo { get; set; }
            public string ToolTipInfo { get; set; }
            public string Remarks { get; set; }
            public bool IsDone { get; set; }
            public SubCampaignAppointment oAppointment { get; set; }
            //public int AccountLeadStatusSelectedIndex { get; set; }
            //public int AccountStatusSelectedIndex { get; set; }
            //public int ContactStatusSelectedIndex { get; set; }
            //public int AccountLeadStatusNotQualifiedIndex { get; set; }
            //public int AccountStatusNotQualifiedIndex { get; set; }
            //public int ContactStatusNotQualifiedIndex { get; set; }
            //public List<string> AccountLeadStatuses { get; set; }
            //public List<string> AccountStatuses { get; set; }
            //public List<string> ContactStatuses { get; set; }
            public List<XmlUtility.SubCampaignConfig> AccountLeadStatuses { get; set; }
            public List<XmlUtility.SubCampaignConfig> AccountStatuses { get; set; }
            public List<XmlUtility.SubCampaignConfig> ContactStatuses { get; set; }
            public bool IsAssignedToTeam = false;
        }
        public class AppointmentProperty {
            public string CompanyWebsite { get; set; }
            public string CompanyAppointmentStatus { get; set; }
            public string CompanyBoardNumber { get; set; }
            public string CompanyAppointmentLeadStatus { get; set; }
        }
        public class AccountStatusProperty {
            public List<XmlUtility.SubCampaignConfig> AccountLeadStatuses { get; set; }
            //public List<string> AccountLeadStatuses { get; set; }
            public List<XmlUtility.SubCampaignConfig> AccountStatuses { get; set; }
            //public List<string> AccountStatuses { get; set; }
            //public int AccountLeadStatusSelectedIndex { get; set; }
            //public int AccountStatusSelectedIndex { get; set; }
            //public int AccountLeadStatusNotQualifiedIndex { get; set; }
            //public int AccountStatusNotQualifiedIndex { get; set; }
            //public int AccountStatusSendEmailIndex { get; set; }
        }
        public class ContactStatusProperty {
            public List<XmlUtility.SubCampaignConfig> ContactStatuses { get; set; }
            //public List<string> ContactStatuses { get; set; }
            //public int ContactStatusSelectedIndex { get; set; }
            //public int ContactStatusNotQualifiedIndex { get; set; }
        }
        #endregion
    }
}
