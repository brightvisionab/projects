
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.Data.Objects;
using System.Linq;
using System.Globalization;

using DevExpress.XtraEditors;
using BrightVision.Common.Business;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using BrightVision.Common.Events.Core;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using SalesConsultant.Events;

namespace SalesConsultant.Modules
{
    public partial class CompanyInformation : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Event Handlers
        //public delegate void CompanyInformationSavedEventHandler(object sender, CompanyInformationArgs e);
        //public event CompanyInformationSavedEventHandler OnCompanyInformationSaved;
        //public delegate void CompanyInformationRefreshEventHandler(object sender, CompanyInformationArgs e);
        //public event CompanyInformationRefreshEventHandler OnCompanyInformationRefresh;
        #endregion

        #region Public Event Args
        public class CompanyInformationArgs : EventArgs
        {
            public account Account { get; set; }
            public string AccountSubCampaignRemarks { get; set; }
        }
        #endregion

        #region Public Members
        public string CompanyPhone { get; set; }
        public string CompanyWebsite { get; set; }
        public bool AllowSaving { get; set; }
        //public string CompanyRemarks { get; set; }

        public int AccountId {
            get { return m_AccountId; }
        }
        public int FinalListId {
            get { return m_FinalListId; }
        }
        #endregion

        #region Private Members
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private BrightPlatformEntities m_objDatabaseModel = null;
        private ObjectResult m_objCompany = null;
        private DataTable m_objStatistics = null;
        private int m_AccountId = 0;
        private int m_FinalListId = 0;
        private int m_PreviousAccountId = 0;
        private SelectionProperty.CallingEnvironment m_CallingEnvironment = SelectionProperty.CallingEnvironment.None;
        #endregion

        #region Constructor
        public CompanyInformation(SelectionProperty.CallingEnvironment pCallingEnvironment)
        {
            m_CallingEnvironment = pCallingEnvironment;
            InitializeComponent();
            //SplitContainerControl
        }
        //public CompanyInformation(BrightPlatformEntities DatabaseModel)
        //{
        //    InitializeComponent();
        //    if (DatabaseModel != null)
        //        m_objDatabaseModel = DatabaseModel;
        //}
        #endregion

        #region Public Methods
        public void Show(int pAccountId, int pFinalListId)
        {
            this.SetParameters(pAccountId, pFinalListId);
            if (m_AccountId != m_PreviousAccountId)
                this.LoadData();
            else
                this.LoadStatistics();
        }
        public void SetAsReadOnly(bool pState)
        {
            //vgridContact.OptionsBehavior.Editable = !state;
            //vgridCompanyInfo.OptionsBehavior.Editable = !pState;
            vgridAccountStat.OptionsBehavior.Editable = !pState;
            btnSave.Enabled = !pState;
            tbxAccountSubCampaignRemark.Properties.ReadOnly = pState;
        }
        public void UpdateStatisticsData()
        {
            this.LoadStatistics();
            //if (m_AccountId > 0 && m_objCompany != null)
            //{
            //    vgridAccountStat.DataSource = null;
            //    m_objStatistics = BrightVision.Common.Utilities.DatabaseUtility.ExecuteStoredProcedure(
            //        "bvGetStatistics_sp", "AccountStat", new SqlParameter("account_id", m_AccountId));
            //    if (m_objStatistics != null)
            //        vgridAccountStat.DataSource = m_objStatistics;
            //}
        }
        public void SetCompanyDetails(int pAccountId)
        {
            account _item = this.GetAccountInformation();
            if (_item != null)
                if (_item.id != pAccountId)
                    return;

            this.LoadData();
            //tbxAccountSubCampaignRemark.Text = pRemarks;
        }
        public void SetCompanyRemarks(string pRemarks)
        {
            tbxAccountSubCampaignRemark.Text = pRemarks;              
        }
        public void Clear()
        {
            vgridCompanyInfo.DataSource = null;
            vgridAccountStat.DataSource = null;
            tbxAccountSubCampaignRemark.Text = null;
        }
        #endregion

        #region Private Methods
        private Uri CheckUri()
        {
            Uri _CompanyWebsite = null;
            try
            {
                string _RawUrl = vgridCompanyInfo.GetRowByFieldName("www").Properties.Value as string;
                if (!_RawUrl.Contains("http://"))
                    _RawUrl = "http://" + _RawUrl;

                if (Uri.IsWellFormedUriString(_RawUrl, UriKind.Absolute))
                {
                    if (!Uri.TryCreate(_RawUrl, UriKind.Absolute, out _CompanyWebsite))
                        _CompanyWebsite = null;
                }
            }
            catch { }

            return _CompanyWebsite;
        }
        private account GetAccountInformation()
        {
            return vgridCompanyInfo.GetRecordObject(0) as account;
        }
        private CompanyInformationArgs GetCompanyInformationArgs()
        {
            return new CompanyInformationArgs() {
                Account = this.GetAccountInformation(),
                AccountSubCampaignRemarks = tbxAccountSubCampaignRemark.Text
            };
        }
        private void LoadData()
        {
            vgridCompanyInfo.DataSource = null;
            m_objCompany = null;
            this.Enabled = false;
            m_objDatabaseModel = new BrightPlatformEntities(UserSession.EntityConnection);

            if (m_AccountId > 0)
                m_objCompany = ObjectCompany.GetCompanyDetails(m_AccountId, m_objDatabaseModel).Execute(MergeOption.AppendOnly);

            if (m_objCompany != null) {
                vgridCompanyInfo.DataSource = m_objCompany;
                this.LoadStatistics();
                this.LoadCompanySubCampaignRemarks();
                this.Enabled = true;
            }
        }
        private void LoadCompanySubCampaignRemarks()
        {
            if (m_objDatabaseModel == null)
                m_objDatabaseModel = new BrightPlatformEntities(UserSession.EntityConnection);

            tbxAccountSubCampaignRemark.Text = string.Empty;
            int _SubCampaignId =  m_objDatabaseModel.final_lists.FirstOrDefault(i => i.id == m_FinalListId).sub_campaign_id;
            sub_campaign_account_remarks _item = m_objDatabaseModel.sub_campaign_account_remarks.FirstOrDefault(i => i.account_id == m_AccountId && i.sub_campaign_id == _SubCampaignId);
            if (_item != null)
                tbxAccountSubCampaignRemark.Text = _item.remarks;
        }
        private void LoadStatistics()
        {
            if (m_AccountId > 0 && m_objCompany != null) {
                int? _SubCampaignId = null;
                final_lists _item = m_objDatabaseModel.final_lists.FirstOrDefault(i => i.id == m_FinalListId);
                if (_item != null)
                    _SubCampaignId = _item.sub_campaign_id;

                vgridAccountStat.DataSource = null;
                m_objStatistics = BrightVision.Common.Utilities.DatabaseUtility.ExecuteStoredProcedure(
                    "bvGetStatistics_sp", 
                    "AccountStat", 
                    new SqlParameter[] {
                        new SqlParameter("account_id", m_AccountId),
                        new SqlParameter("p_sub_campaign_id", _SubCampaignId)
                    }
                );

                if (m_objStatistics != null)
                    vgridAccountStat.DataSource = m_objStatistics;

                this.LoadCompanySubCampaignRemarks();
            }
        }
        private void Save()
        {
            if (vgridCompanyInfo.GetRecordObject(0) == null)
                return;
            
            account accnt = vgridCompanyInfo.GetRecordObject(0) as account;
            if (accnt == null) 
                return;

            //if (CompanyRemarks != null)
            //    accnt.remarks = CompanyRemarks;

            /**
             * [@jeff 06.20.2012]: https://brightvision.jira.com/browse/PLATFORM-1460
             * add event logging for edited accounts.
             */
            accnt.last_modified_machine = UserSession.CurrentUser.ComputerName;
            accnt.last_modified_source = BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Sales_Company_Information;
            accnt.modified_by = UserSession.CurrentUser.UserId;
            accnt.modified_date = DateTime.Now;
            m_objDatabaseModel.accounts.FirstOrDefault(i => i.id == accnt.id);
            m_objDatabaseModel.accounts.ApplyCurrentValues(accnt);
            
            int SubCampaignId = m_objDatabaseModel.final_lists.FirstOrDefault(i => i.id == m_FinalListId).sub_campaign_id;
            sub_campaign_account_remarks _efeCompanyRemarks = m_objDatabaseModel.sub_campaign_account_remarks.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId && i.account_id == m_AccountId);
            if (_efeCompanyRemarks == null) {
                m_objDatabaseModel.sub_campaign_account_remarks.AddObject(
                    new sub_campaign_account_remarks() {
                        sub_campaign_id = SubCampaignId,
                        account_id = m_AccountId,
                        remarks = tbxAccountSubCampaignRemark.Text
                    }
                );
            }
            else {
                _efeCompanyRemarks.remarks = tbxAccountSubCampaignRemark.Text;
                m_objDatabaseModel.sub_campaign_account_remarks.ApplyCurrentValues(_efeCompanyRemarks);
            }
          
            m_objDatabaseModel.SaveChanges();
            CompanyPhone = accnt.telephone;
            CompanyWebsite = accnt.www;

            //if (OnCompanyInformationSaved != null) {
            //    CompanyInformationArgs cargs = new CompanyInformationArgs(accnt);
            //    OnCompanyInformationSaved(this, cargs);
            //}

            /**
             * save company geo code information
             */
            #region Code Logic
            StringBuilder _address = new StringBuilder();
            if (!string.IsNullOrEmpty(accnt.box_address))
                _address.Append(accnt.box_address);
            if (!string.IsNullOrEmpty(accnt.street_address))
                _address.Append(_address.Length > 0 ? "," + accnt.street_address : accnt.street_address);
            if (!string.IsNullOrEmpty(accnt.zipcode))
                _address.Append(_address.Length > 0 ? "," + accnt.zipcode : accnt.zipcode);
            if (!string.IsNullOrEmpty(accnt.city))
                _address.Append(_address.Length > 0 ? "," + accnt.city : accnt.city);
            if (!string.IsNullOrEmpty(accnt.country))
                _address.Append(_address.Length > 0 ? "," + accnt.country : accnt.country);

            GoogleMapUtility _oMapUtility = new GoogleMapUtility();
            if (!string.IsNullOrEmpty(_address.ToString()) || !GoogleMapUtility.IsValidGeoAddress(_address.ToString()))
            {
                string[] _oGeoData = _oMapUtility.GetGeographicalData(_address.ToString()).Split(',');
                double _Latitude = 0;
                double _Longitude = 0;

                // save only if returned success code
                if (_oGeoData[0].ToString().Equals("200"))
                {
                    if (_oGeoData[2] != null)
                        if (ValidationUtility.IsCurrency(_oGeoData[2]))
                            _Latitude = Convert.ToDouble(_oGeoData[2], CultureInfo.InvariantCulture);

                    if (_oGeoData[3] != null)
                        if (ValidationUtility.IsCurrency(_oGeoData[3]))
                            _Longitude = Convert.ToDouble(_oGeoData[3], CultureInfo.InvariantCulture);

                    if (_Latitude != 0 || _Longitude != 0)
                    {
                        BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
                        _efDbModel.FISaveGeographicalData(0, accnt.id, (decimal)_Latitude, (decimal)_Longitude, UserSession.CurrentUser.UserId);
                    }
                }
            }
            #endregion
        }
        private void SetParameters(int pAccountId, int pFinalListId)
        {
            m_PreviousAccountId = m_AccountId;
            m_AccountId = pAccountId;
            m_FinalListId = pFinalListId;
        }
        #endregion

        #region Object Events
        private void txtCompanyWWW_Click(object sender, EventArgs e)
        {
            Uri _CompanyWebsite = this.CheckUri();
            if (_CompanyWebsite != null)
                System.Diagnostics.Process.Start(_CompanyWebsite.ToString());
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!AllowSaving)
                return;

            account _efeAccount = this.GetAccountInformation();
            if (_efeAccount == null)
                return;

            if (_efeAccount.company_name.Length < 1)
                return;

            BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            account _efeExistingAcct = _efDbModel.accounts.FirstOrDefault(i => i.company_name.Equals(_efeAccount.company_name) && i.country.Equals(_efeAccount.country));
            if (_efeExistingAcct != null && _efeExistingAcct.id != _efeAccount.id) {
                NotificationDialog.Information("Bright Sales", "Company already exist.");
                return;
            }

            sub_campaign_account_lists oAccount = _efDbModel.sub_campaign_account_lists.FirstOrDefault(p => p.account_id == m_AccountId && p.final_list_id == m_FinalListId);
            if (oAccount.locked && oAccount.locked_by != UserSession.CurrentUser.UserId) {
                NotificationDialog.Error("Bright Sales", "Company is currently on work mode by another user.");
                return;
            }

            //DialogResult objResult = MessageBox.Show("Are you sure to update this company?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (objResult == DialogResult.No)
            //    return;
           
            //if (m_objCompanyAndContact != null)
            //    m_SelectedCompanyId = (int)m_objCompanyAndContact.account_id;

            
            WaitDialog.Show(ParentForm, "Saving company data...");
            this.Save();
            //this.Clear();

            //var _args = this.GetCompanyInformationArgs();
            m_EventBus.Notify(new CompanyInformationEvents.OnSave.CampaignExtraDetail() {
                OnSaveArgs = new CompanyInformationEvents.CompanyInformationArgs() {
                    Account = this.GetAccountInformation(),
                    AccountSubCampaignRemarks = tbxAccountSubCampaignRemark.Text
                }
            });
            if (m_CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignBooking) {
                m_EventBus.Notify(new CompanyInformationEvents.OnSave.ManageCampaignBooking() {
                    OnSaveArgs = new CompanyInformationEvents.CompanyInformationArgs() {
                        Account = this.GetAccountInformation(),
                        AccountSubCampaignRemarks = tbxAccountSubCampaignRemark.Text
                    }
                });
            }
            else if (m_CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList) {
                m_EventBus.Notify(new CompanyInformationEvents.OnSave.ManageCampaignList() {
                    OnSaveArgs = new CompanyInformationEvents.CompanyInformationArgs() {
                        Account = this.GetAccountInformation(),
                        AccountSubCampaignRemarks = tbxAccountSubCampaignRemark.Text
                    }
                });
            }
            m_EventBus.Notify(new CompanyInformationEvents.OnSave.FrmSalesConsultant() {
                OnSaveArgs = new CompanyInformationEvents.CompanyInformationArgs() {
                    Account = this.GetAccountInformation(),
                    AccountSubCampaignRemarks = tbxAccountSubCampaignRemark.Text
                }
            });

            //if (OnCompanyInformationSaved != null)
            //    OnCompanyInformationSaved(this, this.GetCompanyInformationArgs());

            //m_IsRefreshingData = true;
            //this.LoadCompanyContactData();
            //m_IsRefreshingData = false;
            //this.SetDefaultSelectedCompany();
            WaitDialog.Close();

            #region Old Code
            //if (m_objCompanyAndContact == null || m_objCompanyInformationForm.vgridCompanyInfo.GetRecordObject(0) == null)
            //{
            //    MessageBox.Show("Company is currently on work mode by another user.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            //BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            //sub_campaign_account_lists oAccount = _efDbModel.sub_campaign_account_lists.SingleOrDefault(p => p.account_id == m_objCompanyAndContact.account_id && p.final_list_id == m_objCompanyAndContact.final_list_id);
            //if (oAccount.locked && oAccount.locked_by != UserSession.CurrentUser.UserId)
            //{
            //    MessageBox.Show("Company is currently on work mode by another user.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            //DialogResult objResult = MessageBox.Show("Are you sure to update this company?", m_MessageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (objResult == DialogResult.Yes)
            //{
            //    if (m_objCompanyAndContact != null)
            //        m_SelectedCompanyId = (int)m_objCompanyAndContact.account_id;

            //    
            //    WaitDialog.Show(ParentForm, "Saving company information.");
            //    m_objCompanyInformationForm.SaveCompanyInformation();
            //    m_objCompanyInformationForm.ClearCompanyInformation();
            //    m_IsRefreshingData = true;
            //    this.LoadCompanyContactData();
            //    m_IsRefreshingData = false;
            //    this.SetDefaultSelectedCompany();
            //    WaitDialog.Close();
            //    MessageBox.Show("Company information has been saved.", "System Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            #endregion
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading data ...");
            this.LoadData();
            this.LoadStatistics();

            var _args = this.GetCompanyInformationArgs();
            m_EventBus.Notify(new CompanyInformationEvents.OnRefresh() {
                OnRefreshArgs = new CompanyInformationEvents.CompanyInformationArgs() {
                    Account = this.GetAccountInformation(),
                    AccountSubCampaignRemarks = tbxAccountSubCampaignRemark.Text
                }
            });

            //if (OnCompanyInformationRefresh != null)
            //    OnCompanyInformationRefresh(this, this.GetCompanyInformationArgs());

            WaitDialog.Close();
        }
        #endregion
    }
}