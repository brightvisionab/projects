using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.PublicProperties
{
    [Serializable()]
    public class CampaignListProperty
    {
        #region Private Fields
        private long campaignListId = 0;
        private string companyName = null;
        private string companyWebsite = null;
        private string companyBoardNumber = null;
        //private string companyAppointmentStatus = null;
        //private string companyAppointmentLeadStatus = null;
        private string companyCity = null;
        private string companyCountry = null;
        private string accountLockUser = null;
        private bool accountLocked = false;
        private int accountLockedBy = 0;

        private SelectionProperty.CampaignListMode campaignListMode = SelectionProperty.CampaignListMode.CompaniesOnly;
        private SelectionProperty.CampaignListMode prevCampaignListMode;
        #endregion

        //public List<string> ContactStatuses { get; set; }
        //public int ContactStatusSelectedIndex { get; set; }
        //public int ContactStatusNotQualifiedIndex { get; set; }
        //public int AccountLeadStatusSelectedIndex { get; set; }
        //public int AccountStatusSelectedIndex { get; set; }
        //public int AccountLeadStatusNotQualifiedIndex { get; set; }
        //public int AccountStatusNotQualifiedIndex { get; set; }
        //public List<string> AccountLeadStatuses { get; set; }
        //public List<string> AccountStatuses { get; set; }
        public SelectionProperty.CampaignListMode CampaignListMode
        {
            get {
                return campaignListMode;
            }
            set
            {
                prevCampaignListMode = campaignListMode;
                campaignListMode = value;
            }
        }

        #region Public Properties
        public long CampaignListId
        {
            get
            {
                return campaignListId;
            }
            set
            {
                campaignListId = value;
            }
        }
        public string CompanyName
        {
            get
            {
                return companyName;
            }
            set
            {
                companyName = value;
            }
        }
        public string CompanyWebsite
        {
            get
            {
                return companyWebsite;
            }
            set
            {
                companyWebsite = value;
            }
        }
        public string CompanyBoardNumber
        {
            get
            {
                return companyBoardNumber;
            }
            set
            {
                companyBoardNumber = value;
            }
        }

        /** /
        public string CompanyAppointmentStatus
        {
            get
            {
                return companyAppointmentStatus;
            }
            set
            {
                companyAppointmentStatus = value;
            }
        }
        public string CompanyAppointmentLeadStatus
        {
            get
            {
                return companyAppointmentLeadStatus;
            }
            set
            {
                companyAppointmentLeadStatus = value;
            }
        }
        /**/

        public string CompanyCity
        {
            get
            {
                return companyCity;
            }
            set
            {
                companyCity = value;
            }
        }
        public string CompanyCountry
        {
            get
            {
                return companyCountry;
            }
            set
            {
                companyCountry = value;
            }
        }
        public string AccountLockUser
        {
            get
            {
                return accountLockUser;
            }
            set
            {
                accountLockUser = value;
            }
        }
        public bool AccountLocked
        {
            get
            {
                return accountLocked;
            }
            set
            {
                accountLocked = value;
            }
        }
        public int AccountLockedBy
        {
            get
            {
                return accountLockedBy;
            }
            set
            {
                accountLockedBy = value;
            }
        }

        public bool IsFirstRow { get; set; }
        public bool IsLastRow { get; set; }
        #endregion
    }
}
