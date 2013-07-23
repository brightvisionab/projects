
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using BrightVision.Common.Business;
using BrightVision.Common.UI;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using SalesConsultant.Events;
using BrightVision.Common.Events.Core;


namespace SalesConsultant.Modules
{
    public partial class ContactInformation : XtraUserControl
    {
        #region Public Event Handlers
        //public delegate void btnNewOnClickHandler(object sender, ContactInformationArgs e);
        //public event btnNewOnClickHandler btnNew_OnClick;

        //public delegate void btnSaveOnClickHandler(object sender, ContactInformationArgs e);
        //public event btnSaveOnClickHandler btnSave_OnClick;

        //public delegate void btnCancelOnClickHandler(object sender, ContactInformationArgs e);
        //public event btnCancelOnClickHandler btnCancel_OnClick;

        //public delegate void btnDeleteOnClickHandler(object sender, ContactInformationArgs e);
        //public event btnDeleteOnClickHandler btnDelete_OnClick;
        #endregion

        #region Public Event Arguments
        public class ContactInformationArgs : EventArgs
        {
            public CTContactDetails ContactDetail { get; set; }
            public bool IsNewContact { get; set; }
            public int NewContactId { get; set; }
        }
        #endregion

        #region Public Properties
        public bool AllowSaving { get; set; }
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private BrightPlatformEntities m_efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        private RepositoryItemBVPopupContainerEdit repositoryItemComboBoxAuto = null;
        private CTContactDetails m_ContactDetail = null;
        private bool m_IsEmpty = true;
        private bool m_IsNewContact = false;
        private int m_ContactId = 0;
        private int m_AccountId = 0;
        private int m_FinalListId = 0;
        private int m_PreviousContactId = 0;
        private SelectionProperty.DialogSaveMode m_DialogEditorSaveMode = SelectionProperty.DialogSaveMode.Unspecified;
        private bool m_IsActive = false;
        private SelectionProperty.CallingEnvironment m_CallingEnvironment = SelectionProperty.CallingEnvironment.None;
        private WebBrowser m_WebBrowser = null;
        #endregion

        #region Constructor
        public ContactInformation(SelectionProperty.CallingEnvironment pCallingEnvironment)
        {
            m_CallingEnvironment = pCallingEnvironment;
            InitializeComponent();
            repositoryItemComboBoxAuto = new RepositoryItemBVPopupContainerEdit() { BorderStyle = BorderStyles.NoBorder };
            repositoryItemComboBoxAuto.Appearance.BorderColor = Color.Transparent;
            repositoryItemComboBoxAuto.Validating += new CancelEventHandler(repositoryItemComboBoxAuto_Validating);
            vgridContact.RepositoryItems.Add(repositoryItemComboBoxAuto);
            editorRowTitle.Properties.RowEdit = repositoryItemComboBoxAuto;
            btnCancel.Enabled = false;
            //m_EventBus.GetEvent<CampaignListChangedViewEventNotifier>().Subscribe(CampaignListChangedView);
        }
        #endregion

        #region Public Methods
        public bool CompliedRequiredFields()
        {
            var _efeContact = vgridContact.GetRecordObject(0) as CTContactDetails;
            if (_efeContact == null) 
                return false;

            if (string.IsNullOrEmpty(_efeContact.first_name) || string.IsNullOrEmpty(_efeContact.last_name))
                return false;
            else if (!string.IsNullOrEmpty(_efeContact.email) && !ValidationUtility.IsEmail(_efeContact.email))
                return false;
            else
                return true;
        }
        public int GetContactId()
        {
            this.GetContactInformation();
            return m_ContactDetail.id;
        }
        public bool IsEmpty()
        {
            return m_IsEmpty;
        }
        public void ResetParamaters()
        {
            m_ContactId = 0;
            m_PreviousContactId = 0;
        }
        public void SetAsReadOnly(bool state) 
        {
            //vgridContact.OptionsBehavior.Editable = !state;
            if (state)
            {
                btnNew.Enabled = !state;
                btnCancel.Enabled = !state;
                btnSave.Enabled = !state;
                btnDelete.Enabled = !state;
            }
            else
            {
                if (!m_IsEmpty && m_ContactId > 0)
                {
                    //btnDelete.Enabled = true;
                    btnSave.Enabled = true;
                    btnNew.Enabled = true;
                    btnCancel.Enabled = false;
                    if (m_IsActive)
                        btnDelete.Enabled = true;
                    else
                        btnDelete.Enabled = false;
                }
                else
                {
                    btnDelete.Enabled = false;
                    btnSave.Enabled = false;
                    btnNew.Enabled = true;
                    btnCancel.Enabled = false;
                }
            }
        }
        public void SetAccountId(int pAcctId)
        {
            m_AccountId = pAcctId;
        }
        public void Show(int pContactId, int pFinalListId, bool pForceReload = false, int pAccountId = 0)
        {
            m_AccountId = pAccountId;

            //try
            //{
            //    if (pContactId == 0 && BrightSalesFacade.Property.CommonProperty.ContactPerson.id != 0)
            //        pContactId = BrightSalesFacade.Property.CommonProperty.ContactPerson.id;
            //}
            //catch { }

            m_FinalListId = pFinalListId;
            m_IsActive = false;
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                if (pContactId > 0)
                    m_IsActive = _efDbModel.contacts.FirstOrDefault(i => i.id == pContactId).active;
            }

            /**
             * we force clear the contact information when the selected item 
             * on campaign list has no contact person yet.
             * this is a special case for the campaign list selected row
             */
            this.SetParameters(pContactId);
            if ((m_ContactId == 0 && m_PreviousContactId == 0) || m_ContactId == -1) {
                vgridContact.DataSource = null;                
                vgridContactStat.DataSource = null;
                vgridContact.OptionsBehavior.Editable = false;
                m_IsActive = false;
            }
            else if (m_ContactId != m_PreviousContactId || pForceReload)
                this.LoadData();
            else
                this.LoadStatistics();

            //if (!m_IsEmpty && m_ContactId > 0) {
            //    btnSave.Enabled = true;
            //    btnNew.Enabled = true;
            //    btnCancel.Enabled = false;
            //    if (m_IsActive)
            //        btnDelete.Enabled = true;
            //    else
            //        btnDelete.Enabled = false;
            //}
            //else {
            //    btnDelete.Enabled = false;
            //    btnSave.Enabled = false;
            //    btnNew.Enabled = true;
            //    btnCancel.Enabled = false;
            //}

            bool _editable = true;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _eftCurrentCompany = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.final_list_id == m_FinalListId && i.account_id == m_AccountId);
                if (_eftCurrentCompany != null) {
                    if (_eftCurrentCompany.locked_by != null && _eftCurrentCompany.locked_by != UserSession.CurrentUser.UserId)
                        _editable = false;

                    _efDbContext.Detach(_eftCurrentCompany);
                }
            }

            if (!_editable) {
                NotificationDialog.Error("Bright Sales", "Currently worked by another user.");
                vgridContact.OptionsBehavior.Editable = false;
                vgridContactStat.OptionsBehavior.Editable = false;
                btnSave.Enabled = false;
                btnNew.Enabled = false;
                btnCancel.Enabled = false;
                btnDelete.Enabled = false;
            }
            else {
                if (vgridContact.DataSource != null)
                    vgridContact.OptionsBehavior.Editable = true;

                if (vgridContactStat.DataSource != null)
                    vgridContactStat.OptionsBehavior.Editable = true;

                if (!m_IsEmpty && m_ContactId > 0) {
                    btnSave.Enabled = true;
                    btnNew.Enabled = true;
                    btnCancel.Enabled = false;
                    if (m_IsActive)
                        btnDelete.Enabled = true;
                    else
                        btnDelete.Enabled = false;
                }
                else {
                    btnDelete.Enabled = false;
                    btnSave.Enabled = false;
                    btnNew.Enabled = true;
                    btnCancel.Enabled = false;
                }
            }
        }
        public void SetState()
        {
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse) {
                vgridContact.OptionsBehavior.Editable = false;
                lcgButtons.Enabled = false;
            }
            else
            {
                vgridContact.OptionsBehavior.Editable = vgridContact.DataSource != null ? true : false;
                lcgButtons.Enabled = true;
                if (vgridContact.DataSource == null)
                {
                    btnDelete.Enabled = false;
                    btnSave.Enabled = false;
                    btnNew.Enabled = true;
                    btnCancel.Enabled = false;
                }
                else
                {
                    this.GetContactInformation();
                    if (m_ContactDetail.id < 1)
                    {
                        btnDelete.Enabled = false;
                        btnSave.Enabled = false;
                        btnNew.Enabled = true;
                        btnCancel.Enabled = false;
                    }
                    else
                    {
                        //btnDelete.Enabled = true;
                        btnSave.Enabled = true;
                        btnNew.Enabled = true;
                        btnCancel.Enabled = false;
                        if (m_IsActive)
                            btnDelete.Enabled = true;
                        else
                            btnDelete.Enabled = false;
                    }
                }

                //if (vgridContact.DataSource != null) {
                //    btnDelete.Enabled = true;
                //    btnSave.Enabled = true;
                //    btnNew.Enabled = true;
                //    btnCancel.Enabled = false;
                //} else {
                //    btnDelete.Enabled = false;
                //    btnSave.Enabled = false;
                //    btnNew.Enabled = true;
                //    btnCancel.Enabled = false;
                //}
            }
        }
        public void SetDialogEditorSaveMode(SelectionProperty.DialogSaveMode pSaveMode)
        {
            m_DialogEditorSaveMode = pSaveMode;
        }
        public void ActiveChanged(bool pIsActive)
        {
            if (pIsActive)
            {
                btnDelete.Enabled = true;
                m_IsActive = true;
            }
            else
            {
                btnDelete.Enabled = false;
                m_IsActive = false;
            }
        }
        public void UpdateStatisticsData()
        {
            this.LoadStatistics();
        }
        public void Clear()
        {
            vgridContact.DataSource = null;
            vgridContactStat.DataSource = null;
        }
        public void UpdateContactData(int pContactId)
        {
            if (m_ContactId != pContactId)
                return;

            var objContact = m_efDbModel.FIGetContactByID(m_ContactId, true).ToList();
            if (objContact != null && objContact.Count > 0)
                vgridContact.DataSource = objContact;
        }
        public void Disable()
        {
            vgridContact.DataSource = null;
            vgridContactStat.DataSource = null;
            vgridContact.OptionsBehavior.Editable = false;
            vgridContactStat.OptionsBehavior.Editable = false;
            btnDelete.Enabled = false;
            btnSave.Enabled = false;
            btnNew.Enabled = false;
            btnCancel.Enabled = false;
        }
        #endregion

        #region Private Methods
        private ContactInformationArgs GetArguments()
        {
            ContactInformationArgs _Args = new ContactInformationArgs();
            this.GetContactInformation();
            _Args.ContactDetail = m_ContactDetail;
            _Args.IsNewContact = m_IsNewContact;
            _Args.NewContactId = m_ContactId;
            return _Args;
        }
        private void GetContactInformation()
        {
            m_ContactDetail = null;
            m_ContactDetail = vgridContact.GetRecordObject(0) as CTContactDetails;
            if (m_ContactDetail != null) {
                if (string.IsNullOrEmpty(m_ContactDetail.remarks))
                    m_ContactDetail.has_contact_remarks = false;
                else
                    m_ContactDetail.has_contact_remarks = true;
            }

        }
        private void LoadData() 
        {
            if (m_ContactId == 0)
                m_ContactId = m_PreviousContactId;

            //this.Enabled = false;
            m_IsEmpty = true;
            if (m_ContactId > 0) {
                var objContact = m_efDbModel.FIGetContactByID(m_ContactId, true).ToList();
                //var objContactStat = DatabaseUtility.ExecuteStoredProcedure("bvGetStatistics_sp", "ContactStat", new SqlParameter("contact_id", m_ContactId));
                if (objContact != null && objContact.Count > 0) {
                    vgridContact.DataSource = objContact;
                    vgridContact.OptionsBehavior.Editable = true;
                    //vgridAccountStat.DataSource = objContactStat;
                    this.LoadStatistics();
                    m_IsEmpty = false;
                    m_IsNewContact = false;
                    //this.Enabled = true;
                }
            }

            editorRowLinkedInURL.Appearance.ForeColor = editorRow33.Appearance.ForeColor;
            editorRowLinkedInURL.Appearance.BackColor = editorRow33.Appearance.BackColor;
            editorRowLinkedInURL.Appearance.BackColor2 = editorRow33.Appearance.BackColor2;
        }
        private void LoadStatistics()
        {
            if (m_ContactId > 0) {
                int? _SubCampaignId = null;
                final_lists _item = m_efDbModel.final_lists.FirstOrDefault(i => i.id == m_FinalListId);
                if (_item != null)
                    _SubCampaignId = _item.sub_campaign_id;

                var objContactStat = DatabaseUtility.ExecuteStoredProcedure(
                    "bvGetStatistics_sp",
                    "ContactStat", 
                    new SqlParameter[] {
                        new SqlParameter("contact_id", m_ContactId),
                        new SqlParameter("p_sub_campaign_id", _SubCampaignId)
                    }
                );

                if (objContactStat != null) {
                    vgridContactStat.DataSource = null;
                    vgridContactStat.DataSource = objContactStat;
                }
            }
        }
        private void SetParameters(int pContactId)
        {
            m_PreviousContactId = m_ContactId;
            m_ContactId = pContactId;
        }
        private void CheckUrl(string url)
        {
            if (m_WebBrowser == null)
            {
                m_WebBrowser = new WebBrowser();
                m_WebBrowser.Navigated += m_WebBrowser_Navigated;
            }

            if (url.IndexOf("http://") == -1) url = "http://" + url;

            try
            {
                m_WebBrowser.Url = new Uri(url);
            }
            catch
            {
                m_WebBrowser_Navigated(null, null);
            }
        }
        //private void CampaignListChangedView(CampaignListChangedViewEventNotifier e)
        //{

        //}
        #endregion

        #region Object Events
        #region Buttons
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!AllowSaving)
                return;            

            vgridContact.DataSource = null;
            vgridContactStat.DataSource = null;
            
            bool prevIsActiveStatus = m_IsActive;

            this.Show(0, m_FinalListId);

            m_IsActive = prevIsActiveStatus;

            if (!m_IsEmpty && m_ContactId > 0)
            {
                //btnDelete.Enabled = true;
                btnSave.Enabled = true;
                btnNew.Enabled = true;
                btnCancel.Enabled = false;
                if (m_IsActive)
                    btnDelete.Enabled = true;
                else
                    btnDelete.Enabled = false;
            }
            else
            {
                btnDelete.Enabled = false;
                btnSave.Enabled = false;
                btnNew.Enabled = true;
                btnCancel.Enabled = false;
            }

            this.GetContactInformation();
            m_EventBus.Notify(new ContactInformationEvents.OnCancel() {
                OnCancelArgs = new ContactInformationEvents.ContactInformationArgs() {
                    ContactDetail = m_ContactDetail,
                    IsNewContact = m_IsNewContact,
                    NewContactId = m_ContactId
                }
            });


            editorRowLinkedInURL.Appearance.ForeColor = editorRow33.Appearance.ForeColor;
            editorRowLinkedInURL.Appearance.BackColor = editorRow33.Appearance.BackColor;
            editorRowLinkedInURL.Appearance.BackColor2 = editorRow33.Appearance.BackColor2;

            //if (btnCancel_OnClick != null)
            //    btnCancel_OnClick(this, this.GetArguments());

            #region Original Code
            //if (m_objContactInformationForm != null && m_oContactView.SelectedContact != null && m_oContactView.SelectedContact.id > 0) {
            //   // m_objContactInformationForm.LoadContactInformation((int)m_oContactView.SelectedContact.id);
            //    btnDeleteContact.Enabled = true;
            //    cmdSaveContactInformation.Enabled = true;
            //} else {
            //    btnDeleteContact.Enabled = false;
            //    cmdSaveContactInformation.Enabled = false;
            //}
            //btnAddContact.Enabled = true;            
            //btnCancelContact.Enabled = true;
            #endregion
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!AllowSaving)
                return;

            if (m_IsEmpty || vgridContact.DataSource == null)
                return;

            bool _editable = true;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _eftCurrentCompany = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.final_list_id == m_FinalListId && i.account_id == m_AccountId);
                if (_eftCurrentCompany != null) {
                    if (_eftCurrentCompany.locked_by != null && _eftCurrentCompany.locked_by != UserSession.CurrentUser.UserId)
                        _editable = false;

                    _efDbContext.Detach(_eftCurrentCompany);
                }
            }

            if (!_editable) {
                NotificationDialog.Error("Bright Sales", "Currently worked by another user.");
                return;
            }

            string Message = String.Format("This is a global deletion from all campaigns.{0}Are you sure you want to delete this contact?", Environment.NewLine);
            DialogResult objDialog = MessageBox.Show(Message, "Delete Contact", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objDialog == DialogResult.No)
                return;

            WaitDialog.Show(ParentForm, "Removing contact...");
            ObjectContact.DeActivateContact(this.GetContactId());
            vgridContact.DataSource = null;
            vgridContactStat.DataSource = null;
            btnDelete.Enabled = false;
            btnSave.Enabled = false;
            btnNew.Enabled = true;
            //btnCancel.Enabled = false;
            //vgridContact.DataSource = null;
            //vgridAccountStat.DataSource = null;
            //vgridContact.OptionsBehavior.Editable = vgridContact.DataSource != null ? true : false;
            //m_ContactId = 0;
            m_IsNewContact = false;
            //m_IsEmpty = true;

            this.GetContactInformation();
            if (m_CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignBooking)
                m_EventBus.Notify(new ContactInformationEvents.OnDelete.ManageCampaignBooking() {
                    OnDeleteArgs = new ContactInformationEvents.ContactInformationArgs() {
                        ContactDetail = m_ContactDetail,
                        IsNewContact = m_IsNewContact,
                        NewContactId = m_ContactId
                    }
                });
            else if (m_CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList)
                m_EventBus.Notify(new ContactInformationEvents.OnDelete.ManageCampaignList() {
                    OnDeleteArgs = new ContactInformationEvents.ContactInformationArgs() {
                        ContactDetail = m_ContactDetail,
                        IsNewContact = m_IsNewContact,
                        NewContactId = m_ContactId
                    }
                });

            //if (btnDelete_OnClick != null)
            //    btnDelete_OnClick(this, this.GetArguments());

            editorRowLinkedInURL.Appearance.ForeColor = editorRow33.Appearance.ForeColor;
            editorRowLinkedInURL.Appearance.BackColor = editorRow33.Appearance.BackColor;
            editorRowLinkedInURL.Appearance.BackColor2 = editorRow33.Appearance.BackColor2;

            WaitDialog.Close();

            #region Original Code
            //if (m_oContactView.SelectedContact == null || m_oContactView.SelectedContact.id <= 0) return;

            //string Message = "This is a global deletion from all campaigns." + Environment.NewLine + "Are you sure you want to delete this contact?";
            //DialogResult objDialog = MessageBox.Show(Message, "Delete Contact", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (objDialog == DialogResult.No)
            //    return;

            //WaitDialog.Show(ParentForm, "Deleting Contact...", "Please wait...");
            //ObjectContact.DeActivateContact(m_oContactView.SelectedContact.id);
            //m_oDialogEditor.ResetToDefaultState();
            //WaitDialog.Close();
            //m_oContactView.PopulateContactView(
            //    oAppointment.SubCampaignId,
            //    oAppointment.AccountId,
            //    oAppointment.FinalListId);
            //m_oContactView_gvContact_FocusedRowChanged(null, null);
            //this.LoadContactInformation();
            #endregion
        }
        private void btnNew_Click(object sender, EventArgs e)
        {
            if (!AllowSaving)
                return;

            bool _editable = true;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _eftCurrentCompany = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.final_list_id == m_FinalListId && i.account_id == m_AccountId);
                if (_eftCurrentCompany != null) {
                    if (_eftCurrentCompany.locked_by != null && _eftCurrentCompany.locked_by != UserSession.CurrentUser.UserId)
                        _editable = false;

                    _efDbContext.Detach(_eftCurrentCompany);
                }
            }

            if (!_editable) {
                NotificationDialog.Error("Bright Sales", "Currently worked by another user.");
                return;
            }

            vgridContactStat.DataSource = null;
            vgridContact.DataSource = null;
            var ds = new List<CTContactDetails> { new CTContactDetails { } };
            vgridContact.DataSource = ds;
            vgridContact.FocusedRow = vgridContact.GetRowByFieldName("first_name");
            if (vgridContact.FocusedRow != null)
                vgridContact.ShowEditor();
            vgridContact.OptionsBehavior.Editable = true;
            m_IsNewContact = true;
            m_IsEmpty = false;
            btnNew.Enabled = false;
            btnDelete.Enabled = false;
            btnSave.Enabled = true;
            btnCancel.Enabled = true;

            this.GetContactInformation();
            m_EventBus.Notify(new ContactInformationEvents.OnAddNew() {
                OnAddNewArgs = new ContactInformationEvents.ContactInformationArgs() {
                    ContactDetail = m_ContactDetail,
                    IsNewContact = m_IsNewContact,
                    NewContactId = m_ContactId
                }
            });

            editorRowLinkedInURL.Appearance.ForeColor = editorRow33.Appearance.ForeColor;
            editorRowLinkedInURL.Appearance.BackColor = editorRow33.Appearance.BackColor;
            editorRowLinkedInURL.Appearance.BackColor2 = editorRow33.Appearance.BackColor2;
            
            //if (btnNew_OnClick != null)
            //    btnNew_OnClick(this, this.GetArguments());

        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!AllowSaving)
                return;

            if (m_IsEmpty)
                return;

            if (!CompliedRequiredFields()) {
                NotificationDialog.Error("Bright Sales", "Required fields firstname/lastname/email cannot be empty or has invalid data.");
                return;
            }

            if (BrightVision.Common.Utilities.ValidationUtility.IFNullString(editorRowLinkedInURL.Properties.Value, "") != "" &&
                !BrightVision.Common.Utilities.ValidationUtility.IFNullString(editorRowLinkedInURL.Properties.Value, "").Contains("/profile/view?id="))
            {
                NotificationDialog.Error("Bright Sales", "Invalid LinkedIn url.");
                editorRowLinkedInURL.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                editorRowLinkedInURL.Appearance.BackColor = Color.FromArgb(244, 102, 102);//red
                editorRowLinkedInURL.Appearance.BackColor2 = Color.FromArgb(244, 102, 102);//red
                return;
            }

            bool _editable = true;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _eftCurrentCompany = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.final_list_id == m_FinalListId && i.account_id == m_AccountId);
                if (_eftCurrentCompany != null) {
                    if (_eftCurrentCompany.locked_by != null && _eftCurrentCompany.locked_by != UserSession.CurrentUser.UserId)
                        _editable = false;

                    _efDbContext.Detach(_eftCurrentCompany);
                }
            }

            if (!_editable) {
                NotificationDialog.Error("Bright Sales", "Currently worked by another user.");
                return;
            }

            //string _Message = string.Format("Are you sure to {0} this contact data?", (m_IsNewContact ? "save" : "update"));
            //DialogResult objResult = MessageBox.Show(_Message, "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (objResult == DialogResult.No)
            //    return;

            var contact = vgridContact.GetRecordObject(0) as CTContactDetails;
            if (contact.absence_start != null && contact.absence_end == null) {
                NotificationDialog.Error("Bright Sales", "Absence end required.");
                return;
            }

            WaitDialog.Show(ParentForm, "Saving contact data...");
            #region Save Contact Information
            if (contact.absence_start == null && contact.absence_end != null)
                contact.absence_start = DateTime.Now;

            int _Hours = Convert.ToDateTime(contact.absence_start).TimeOfDay.Hours;
            int _Minutes = Convert.ToDateTime(contact.absence_start).TimeOfDay.Minutes;
            if ((_Hours >= 0 && _Hours < 3) && _Minutes > 0) {
                DateTime _date = Convert.ToDateTime(contact.absence_start);
                TimeSpan _time = new TimeSpan(3, 0, 0);
                contact.absence_start = _date.Date + _time;
            }

            _Hours = Convert.ToDateTime(contact.absence_end).TimeOfDay.Hours;
            _Minutes = Convert.ToDateTime(contact.absence_end).TimeOfDay.Minutes;
            if ((_Hours >= 0 && _Hours < 3) && _Minutes > 0) {
                DateTime _date = Convert.ToDateTime(contact.absence_end);
                TimeSpan _time = new TimeSpan(3, 0, 0);
                contact.absence_end = _date.Date + _time;
            }

            if (contact.absence_start > contact.absence_end) {
                WaitDialog.Close();
                NotificationDialog.Error("Bright Sales", "Absence start must be less than absence end.");
                return;
            }

            int _AcctId = 0;
            if (m_CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList)
                _AcctId = m_BrightSalesProperty.CommonProperty.AccountId;
            else
                _AcctId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId;

            /**
             * set active = null, so the original active value stays as it is when saving contact.
             * activation/de-activation happens @ user level action (deactivate button & checkbox).
             */
            int _SubCampaignId = m_efDbModel.final_lists.FirstOrDefault(i => i.id == m_FinalListId).sub_campaign_id;

            if (!m_IsNewContact) {
                m_efDbModel.FIUpdateContactDetails(
                    contact.id, contact.first_name, contact.middle_name, contact.last_name, contact.direct_phone,
                    contact.mobile, contact.title_id, null, contact.role_tags, contact.address_1, contact.address_2, contact.city,
                    contact.zipcode, contact.country, contact.remarks, UserSession.CurrentUser.UserId,
                    contact.email, contact.linkedin_url, null, null, _AcctId, m_FinalListId,
                    UserSession.CurrentUser.ComputerName,
                    BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Sales_Contact_Information,
                    contact.absence_start,
                    contact.absence_end, 
                    _SubCampaignId,
                    _AcctId
                );
            }
            else {
                int? contactId = m_efDbModel.FIUpdateContactDetails(
                    null, contact.first_name, contact.middle_name, contact.last_name, contact.direct_phone,
                    contact.mobile, contact.title_id, null, contact.role_tags, contact.address_1, contact.address_2, contact.city,
                    contact.zipcode, contact.country, contact.remarks, UserSession.CurrentUser.UserId,
                    contact.email, contact.linkedin_url, 0, true, _AcctId, m_FinalListId,
                    UserSession.CurrentUser.ComputerName,
                    BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Sales_Contact_Information,
                    contact.absence_start,
                    contact.absence_end,
                    _SubCampaignId,
                    _AcctId
                ).FirstOrDefault();

                contact.id = contactId == null ? 0 : (int)contactId;
            }
            //m_efDbModel.SaveChanges();
            this.SetParameters(contact.id);
            #endregion
            
            /**
             * save contact geo code information
             */
            #region Code Logic
            StringBuilder _address = new StringBuilder();
            if (!string.IsNullOrEmpty(contact.address_1))
                _address.Append(contact.address_1);
            if (!string.IsNullOrEmpty(contact.address_2))
                _address.Append(_address.Length > 0 ? "," + contact.address_2 : contact.address_2);
            if (!string.IsNullOrEmpty(contact.zipcode))
                _address.Append(_address.Length > 0 ? "," + contact.zipcode : contact.zipcode);
            if (!string.IsNullOrEmpty(contact.city))
                _address.Append(_address.Length > 0 ? "," + contact.city : contact.city);
            if (!string.IsNullOrEmpty(contact.country))
                _address.Append(_address.Length > 0 ? "," + contact.country : contact.country);

            GoogleMapUtility _oMapUtility = new GoogleMapUtility();
            if (!string.IsNullOrEmpty(_address.ToString()) || !GoogleMapUtility.IsValidGeoAddress(_address.ToString()))
            {
                string[] _oGeoData = _oMapUtility.GetGeographicalData(_address.ToString()).Split(',');
                double _Latitude = 0;
                double _Longitude = 0;

                // save only if returned success code
                if (_oGeoData[0].Equals("200")) {
                    if (_oGeoData[2] != null)
                        if (ValidationUtility.IsCurrency(_oGeoData[2]))
                            _Latitude = Convert.ToDouble(_oGeoData[2], CultureInfo.InvariantCulture);

                    if (_oGeoData[3] != null)
                        if (ValidationUtility.IsCurrency(_oGeoData[3]))
                            _Longitude = Convert.ToDouble(_oGeoData[3], CultureInfo.InvariantCulture);

                    if (_Latitude != 0 || _Longitude != 0)
                    {
                        BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
                        _efDbModel.FISaveGeographicalData(1, contact.id, (decimal)_Latitude, (decimal)_Longitude, UserSession.CurrentUser.UserId);
                    }
                }
            }
            #endregion

            this.GetContactInformation();
            if (m_CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignBooking) {
                m_EventBus.Notify(new ContactInformationEvents.OnSave.ManageCampaignBooking() {
                    OnSaveArgs = new ContactInformationEvents.ContactInformationArgs() {
                        ContactDetail = m_ContactDetail,
                        IsNewContact = m_IsNewContact,
                        NewContactId = m_ContactId
                    }
                });
            }
            else if (m_CallingEnvironment == SelectionProperty.CallingEnvironment.CampaignList) {
                m_EventBus.Notify(new ContactInformationEvents.OnSave.ManageCampaignList() {
                    OnSaveArgs = new ContactInformationEvents.ContactInformationArgs() {
                        ContactDetail = m_ContactDetail,
                        IsNewContact = m_IsNewContact,
                        NewContactId = m_ContactId
                    }
                });
            }

            //if (btnSave_OnClick != null)
            //    btnSave_OnClick(this, this.GetArguments());

            this.Show(contact.id, m_FinalListId, false, _AcctId);
            btnNew.Enabled = true;
            btnSave.Enabled = true;
            btnCancel.Enabled = false;
            //btnDelete.Enabled = true;
            if (m_IsActive)
                btnDelete.Enabled = true;
            else
                btnDelete.Enabled = false;
            m_IsNewContact = false;

            WaitDialog.Close();

            #region Original Code
            //if (m_objContactInformationForm.IsEmpty)
            //    return;

            //if (!m_objContactInformationForm.ValidateRequiredFields())
            //{
            //    MessageBox.Show("Required fields firstname/lastname cannot be empty.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //DialogResult objResult = MessageBox.Show("Are you sure to " +
            //    (m_objContactInformationForm.IsNewContact ? "save" : "update")
            //    + " this contact data?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (objResult == DialogResult.Yes)
            //{
            //    
            //    WaitDialog.Show(ParentForm, "Saving contact data.");

            //    //m_objContactInformationForm.ContactRemarks = txtContactRemarks.Text;
            //    m_objContactInformationForm.AccountId = oAppointment.AccountId;
            //    m_objContactInformationForm.SaveContactInformation();
            //    m_oContactView.ContactId = m_objContactInformationForm.GetContactId();
            //    m_oContactView.PopulateContactView(oAppointment.SubCampaignId, oAppointment.AccountId, oAppointment.FinalListId);
            //    m_oContactView_gvContact_FocusedRowChanged(null, null);
            //    this.LoadContactInformation();
            //    var cont = m_oContactView.SelectedContact;

            //    if (cont == null)
            //        return;

            //    if (!string.IsNullOrEmpty(cont.direct_phone))
            //    {
            //        lcgCallDirect.Enabled = true;
            //        btnCallDirect.Enabled = true;
            //    }
            //    else
            //    {
            //        lcgCallDirect.Enabled = false;
            //        btnCallDirect.Enabled = false;
            //    }

            //    if (!string.IsNullOrEmpty(cont.mobile))
            //    {
            //        lcgCallMobile.Enabled = true;
            //        btnCallMobile.Enabled = true;
            //    }
            //    else
            //    {
            //        lcgCallMobile.Enabled = false;
            //        btnCallMobile.Enabled = false;
            //    }

            //    WaitDialog.Close();
            //}
            #endregion
        }
        #endregion

        #region Grid
        private void vgcContact_RecordCellStyle(object sender, DevExpress.XtraVerticalGrid.Events.GetCustomRowCellStyleEventArgs e)
        {
            if (e.Row.Properties.FieldName == "title")
            {
                var val = vgridContact.GetCellValue(e.Row, e.RecordIndex);
                if (val != null && val.ToString() != "")
                {
                    var edit = e.Row.Properties.RowEdit as RepositoryItemBVPopupContainerEdit;
                    var data = edit.PopupControl.Tag as BVPopupContainerControlData;
                    bool hasMatch = data.MatchKeyword(val.ToString().Trim());
                    if (hasMatch)
                    {
                        e.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                        e.Appearance.BackColor = Color.FromArgb(181, 245, 146);//green
                        e.Appearance.BackColor2 = Color.FromArgb(181, 245, 146);//green
                    }
                    else
                    {
                        e.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                        e.Appearance.BackColor = Color.FromArgb(244, 102, 102);//red
                        e.Appearance.BackColor2 = Color.FromArgb(244, 102, 102);//red
                    }
                }
                else
                {
                    e.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                }
            }
        }
        private void vgcContact_ShownEditor(object sender, EventArgs e)
        {
            var view = sender as DevExpress.XtraVerticalGrid.VGridControl;
            if (view != null && view.ActiveEditor != null)
            {
                if (view.ActiveEditor is PopupContainerEdit)
                {
                    var comboBoxEdit = view.ActiveEditor as PopupContainerEdit;
                    if (comboBoxEdit != null)
                    {
                        comboBoxEdit.IsModified = true;
                    }
                }
            }
        }       
        private void vgridContact_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e) 
        {
            if (m_DialogEditorSaveMode == SelectionProperty.DialogSaveMode.Unspecified) // && m_CellValueChangeValid)
                btnCancel.Enabled = true;

            if (e.Row.Name == editorRowLinkedInURL.Name && ValidationUtility.IFNullString(e.Value, "") != "")
            {                
                WaitDialog.Show("Verifying linkedin url...");
                CheckUrl(e.Value.ToString());                
            }
        }


        private void m_WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (BrightVision.Common.Utilities.ValidationUtility.IFNullString(m_WebBrowser.Url, "").Contains("/profile/view?id="))
            {
                //true
                editorRowLinkedInURL.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                editorRowLinkedInURL.Appearance.BackColor = Color.FromArgb(181, 245, 146);//green
                editorRowLinkedInURL.Appearance.BackColor2 = Color.FromArgb(181, 245, 146);//green
                btnSave.Enabled = true;
            }
            else
            {
                //false
                btnSave.Enabled = false;
                editorRowLinkedInURL.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                editorRowLinkedInURL.Appearance.BackColor = Color.FromArgb(244, 102, 102);//red
                editorRowLinkedInURL.Appearance.BackColor2 = Color.FromArgb(244, 102, 102);//red

                NotificationDialog.Error("Bright Sales", "Invalid LinkedIn url.");
            }
                        
            WaitDialog.Close();
        }
        #endregion

        #region Repository Item Combo Box
        private void repositoryItemComboBoxAuto_Validating(object sender, CancelEventArgs e)
        {
            var ctl = sender as PopupContainerEdit;
            if (ctl != null)
            {
                if (ctl.Text.Trim() == string.Empty)
                {
                    ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                    ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);
                }
                else
                {
                    var data = ctl.Properties.PopupControl.Tag as BVPopupContainerControlData;
                    if (data != null && data.HasMatch)
                    {
                        ctl.Properties.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                        ctl.Properties.Appearance.BackColor = Color.FromArgb(181, 245, 146);
                    }
                    else
                    {
                        ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                        ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);
                    }
                }
            }
        }
        #endregion

        #region This Module
        private void ContactInformation_EnabledChanged(object sender, EventArgs e)
        {
            if (this.Enabled)
            {
                if (!m_IsEmpty && m_ContactId > 0)
                {
                    //btnDelete.Enabled = true;
                    btnSave.Enabled = true;
                    btnNew.Enabled = true;
                    btnCancel.Enabled = false;
                    if (m_IsActive)
                        btnDelete.Enabled = true;
                    else
                        btnDelete.Enabled = false;
                }
                else
                {
                    btnDelete.Enabled = false;
                    btnSave.Enabled = false;
                    btnNew.Enabled = true;
                    btnCancel.Enabled = false;
                }
            }
        }
        #endregion
        #endregion
    }
}
