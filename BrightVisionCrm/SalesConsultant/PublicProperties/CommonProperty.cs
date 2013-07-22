
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using SalesConsultant.PublicProperties;
using BrightVision.Common.Business;

namespace SalesConsultant.PublicProperties
{
    public class CommonProperty
    {
        #region Properties
        private CTScSubCampaignContactList _ContactPerson = null;
        public CTScSubCampaignContactList ContactPerson
        {
            get {
                if (_ContactPerson == null)
                    return new CTScSubCampaignContactList();

                return _ContactPerson; 
            }
            set {
                if (value == null)
                    _ContactPerson = new CTScSubCampaignContactList();
                else
                    _ContactPerson = value;

                _CurrentWorkedContactId = _ContactPerson.id;
            }
        }

        private SelectionProperty.CurrentTab _CurrentTab = SelectionProperty.CurrentTab.CampaignList;
        public SelectionProperty.CurrentTab CurrentTab
        {
            get { return _CurrentTab; }
            set { _CurrentTab = value; }
        }

        private int _AccountId = 0;
        public int AccountId 
        {
            get { return _AccountId; }
            set {
                _PrevAccountId = _AccountId;
                _AccountId = value;
            }
        }

        private int _ContactId = 0;
        public int ContactId 
        {
            get { return _ContactId; }
            set {
                _PrevContactId = _ContactId;
                _ContactId = value;
            }
        }

        private int _PrevAccountId = 0;
        public int PrevAccountId 
        {
            get { return _PrevAccountId; }
        }

        private int _PrevContactId = 0;
        public int PrevContactId
        {
            get { return _PrevContactId; }
        }

        private int _CurrentWorkedAccountId = 0;
        public int CurrentWorkedAccountId
        {
            get { return _CurrentWorkedAccountId; }
            set { _CurrentWorkedAccountId = value; }
        }

        private int _CurrentWorkedContactId = 0;
        public int CurrentWorkedContactId
        {
            get { return _CurrentWorkedContactId; }
            set { _CurrentWorkedContactId = value; }
        }

        private int _CustomerId = 0;
        public int CustomerId 
        {
            get { return _CustomerId;  }
            set { _CustomerId = value; }
        }

        private int _CampaignId = 0;
        public int CampaignId 
        {
            get { return _CampaignId; }
            set { _CampaignId = value; }
        }

        private int _SubCampaignId = 0;
        public int SubCampaignId
        {
            get { return _SubCampaignId; }
            set { _SubCampaignId = value; }
        }

        private int _FinalListId = 0;
        public int FinalListId
        {
            get { return _FinalListId; }
            set { _FinalListId = value; }
        }

        private bool _CallLogSaved = true;
        public bool CallLogSaved 
        {
            get { return _CallLogSaved; }
            set { _CallLogSaved = value; }
        }

        private int _CallModeAudioSettings = 1;
        public int CallModeAudioSettings
        {
            get { return _CallModeAudioSettings; }
            set { _CallModeAudioSettings = value; }
        }

        private bool _IsEmptyList = false;
        public bool IsEmptyCampaignList
        {
            get { return _IsEmptyList; }
            set { _IsEmptyList = value; }
        }

        public bool UserOnWorkModeState { get; set; }
        public bool OnCallMode { get; set; }

        private bool _CompanyLocked = true;
        public bool CompanyLocked
        {
            get { return _CompanyLocked; }
            set { _CompanyLocked = value; }
        }

        private bool _IsLoadEventLog = false;
        public bool IsLoadEventLog {
            get { return _IsLoadEventLog; }
            set { _IsLoadEventLog = value; }
        }

        public string CustomerName { get; set; }
        public string CampaignName { get; set; }
        public string SubCampaignName { get; set; }
        public string CompanyName { get; set; }

        public string CompanySubCampaignLevelRemarks { get; set; }

        private int _CallLogContactId = 0;
        public int CallLogContactId
        {
            get { return _CallLogContactId; }
            set { _CallLogContactId = value; }
        }

        #endregion

        #region Public Methods
        public void Clear()
        {
            this._AccountId = 0;
            this._ContactId = 0;
            this.FinalListId = 0;
        }
        public bool CurrentWorkedAccountLockedByOtherUser()
        {
            bool _locked = false;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _eftCurrentCompany = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.final_list_id == _FinalListId && i.account_id == _CurrentWorkedAccountId);
                if (_eftCurrentCompany != null) {
                    if (_eftCurrentCompany.locked_by != null && _eftCurrentCompany.locked_by != UserSession.CurrentUser.UserId)
                        _locked = true;

                    _efDbContext.Detach(_eftCurrentCompany);
                }
            }

            return _locked;
        }
        public account GetCurrentWorkedAccountData()
        {
            account _eftAccount = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftAccount = _efDbContext.accounts.FirstOrDefault(i => i.id == _CurrentWorkedAccountId);
                if (_eftAccount != null)
                    _efDbContext.Detach(_eftAccount);
            }

            return _eftAccount;
        }
        #endregion
    }
}
