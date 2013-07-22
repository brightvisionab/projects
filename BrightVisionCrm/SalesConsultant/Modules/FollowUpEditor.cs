
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightVision.Model;
using SalesConsultant.Business;
using DevExpress.XtraEditors.Controls;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using System.Data.Objects;
using SalesConsultant.PublicProperties;
using SalesConsultant.Events;

using SalesConsultant.Properties;

namespace SalesConsultant.Modules
{
    public partial class FollowUpEditor : UserControl
    {
        #region Constructors
        public FollowUpEditor()
        {
            InitializeComponent();
            IsNurtureEvent = false;

            m_images.AddImage(Resources.make_call);
            m_images.AddImage(Resources.todo);
            m_images.AddImage(Resources.nurture);
        }
        #endregion

        #region Public Events & Args
        public delegate void btnSaveOnClickEventHandler(object sender, EventFollowUpLogEvents.OnSaveArguments e);
        public event btnSaveOnClickEventHandler btnSave_OnClick;

        public delegate string GetListSourceEventHandler(GetListSourceArgs e);
        public event GetListSourceEventHandler GetListSource;
        public class GetListSourceArgs : EventArgs {
            public int CompanyId { get; set; }
            public int ContactId { get; set; }
        }
        #endregion

        #region Public Properties
        public CTScSubCampaignContactList ContactPerson { get; set; }

        public int AccountId { get; set; }
        public int SubCampaignId { get; set; }
        public int FollowUpId { get; set; }
        public bool IsNurtureEvent { get; set; }
        #endregion

        #region Private Properties
        private IList<IdName> salesScriptGetUsers = null;
        private List<CTScSubCampaignContactList> m_ContactPersons = null;
        private int m_FinalListId = 0;
        private int m_EventSubCampaignId = 0;
        DevExpress.Utils.ImageCollection m_images = new DevExpress.Utils.ImageCollection();

        private class Contact {
            public int id { get; set; }
            public string name { get; set; }
        }
        private class EventType {
            public int id { get; set; }
            public string name { get; set; }
        }
        #endregion

        #region Public Methods
        public void UpdateContactPerson(CTScSubCampaignContactList pContactPerson)
        {
            if (m_ContactPersons.Exists(i => i.id == pContactPerson.id)) {
                int? _Position = m_ContactPersons.FindIndex(i => i.id == pContactPerson.id);
                if (_Position != null)
                    m_ContactPersons[(int)_Position] = pContactPerson;
            }
            else
                m_ContactPersons.Add(pContactPerson);

            this.LoadContactPersons(m_ContactPersons);
            cboContact.EditValue = null; //to force trigger the on change event
            cboContact.EditValue = pContactPerson.id;
        }
        public void SetCompanyInfo()
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                account _efeCompany = _efDbContext.accounts.FirstOrDefault(i => i.id == AccountId);
                if (_efeCompany != null) {
                    lblCompanyInfo.Text = string.Format("Company: {0}{1}",
                        _efeCompany.company_name,
                        string.IsNullOrEmpty(_efeCompany.city) ? string.Empty : ", " + _efeCompany.city
                    );
                    _efDbContext.Detach(_efeCompany);
                }
            }
        }
        public void SetCampaignInfo(event_followup_log pArgs)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                account _efoCompany = _efDbContext.accounts.FirstOrDefault(i => i.id == pArgs.account_id);
                lblCompanyInfo.Text = string.Format("Company: {0}{1}",
                    _efoCompany.company_name,
                    string.IsNullOrEmpty(_efoCompany.city) ? string.Empty : ", " + _efoCompany.city
                );
                tbxRemarks.Text = pArgs.short_message;
                cbxDone.Checked = pArgs.done;
                lciHide.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                lciClear.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                cboContact.EditValue = pArgs.contact_id;
                dbxDate.EditValue = pArgs.date_of_transaction;
                cboTime.Text = string.Format("{0}:{1}", ((TimeSpan)pArgs.start_time).Hours.ToString().PadLeft(2, '0'), ((TimeSpan)pArgs.start_time).Minutes.ToString().PadLeft(2, '0'));
                FollowUpId = pArgs.id;
                _efDbContext.Detach(_efoCompany);
            }
        }
        public void SetCampaignInfo(string pCustomerName, string pCampaignName, string pSubCampaignName)
        {
            lciDone.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            FollowUpId = 0;
        }
        public void SetCampaignInfo(CampaignBookingProperty.CampaignBoookingArguments pArgs)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                event_followup_log _data = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == pArgs.Id);
                account _efoCompany = _efDbContext.accounts.FirstOrDefault(i => i.id == AccountId);
                lblCompanyInfo.Text = string.Format("Company: {0}{1}",
                    _efoCompany.company_name,
                    string.IsNullOrEmpty(_efoCompany.city) ? string.Empty : ", " + _efoCompany.city
                );
                tbxRemarks.Text = _data.short_message;
                cbxDone.Checked = _data.done;
                lciHide.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                lciClear.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                cboContact.EditValue = _data.contact_id;
                dbxDate.EditValue = _data.date_of_transaction;
                cboTime.Text = Convert.ToDateTime(_data.start_time.ToString()).ToString("HH:mm");
                FollowUpId = pArgs.Id;
                _efDbContext.Detach(_data);
                _efDbContext.Detach(_efoCompany);
            }
        }
        public void LoadContactPersons(List<CTScSubCampaignContactList> pContactPersons)
        {
            cboContact.Properties.DataSource = null;
            if (pContactPersons.Count < 1)
                return;

            m_ContactPersons = pContactPersons;
            List<Contact> _Contacts = new List<Contact>();
            foreach (CTScSubCampaignContactList item in pContactPersons) {
                _Contacts.Add(new Contact() {
                    id = item.id,
                    name = string.Format("{0} {1}{2}{3}",
                        item.first_name,
                        item.last_name,
                        string.IsNullOrEmpty(item.title) ? string.Empty : ", " + item.title,
                        string.IsNullOrEmpty(item.complete_address) ? string.Empty : ", " + item.complete_address
                    )
                });
            }

            cboContact.Properties.Columns.Clear();
            cboContact.Properties.DataSource = null;
            cboContact.Properties.DataSource = _Contacts;
            cboContact.Properties.DisplayMember = "name";
            cboContact.Properties.ValueMember = "id";
            cboContact.Properties.Columns.Add(new LookUpColumnInfo("name"));
            cboContact.EditValue = _Contacts[0].id;
        }
        public void LoadSalesUsers(int pSubCampaignId = 0, int DefaultSalesUserId = 0)
        {
            /**
             * DAN: As don't know what is the purpose as on below code, id=0 belongs to Team.
             * So if user select Team or 0 as id, it will not reflect because the default selected will be the
             * Current User Id.
             * https://brightvision.jira.com/browse/PLATFORM-2615
             * 
             * if (DefaultSalesUserId == 0)
             * DefaultSalesUserId = UserSession.CurrentUser.UserId;
             */

            int _SubCampaignId = pSubCampaignId;
            if (pSubCampaignId == 0)
                _SubCampaignId = SubCampaignId;

            /**
             * pSubCampaignId:
             * 0 = means that it will load the users for this current sub campaign.
             * greater than 0 = means we must load the users of the selected nurture sub campaign.
             */
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {

                if (pSubCampaignId == 0) {
                    int _FinalListId = 0;
                    final_lists _efeFinalList = _efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == _SubCampaignId);
                    _efDbContext.Detach(_efeFinalList);

                    if (_efeFinalList != null)
                        _FinalListId = _efeFinalList.id;
                    if (m_FinalListId != _FinalListId)
                        m_FinalListId = _FinalListId;
                    if (m_FinalListId < 1)
                        return;
                }

                try {
                    if (salesScriptGetUsers != null)
                        salesScriptGetUsers.Clear();

                    List<CTSubCampaignUser> _lstItems = new List<CTSubCampaignUser>();
                    if (pSubCampaignId > 0)
                        _lstItems.Add(new CTSubCampaignUser() { id = 0, fullname = "Team" });

                    List<CTSubCampaignUser> _lstData = _efDbContext.FIGetSubCampaignUsers(_SubCampaignId).ToList();
                    if (_lstData.Count > 0)
                        foreach (CTSubCampaignUser _item in _lstData)
                            _lstItems.Add(_item);

                    cboSalesUser.Properties.DataSource = null;
                    cboSalesUser.Properties.DataSource = _lstItems;
                    cboSalesUser.Properties.DisplayMember = "fullname";
                    cboSalesUser.Properties.ValueMember = "id";

                    if (_lstItems.Exists(i => i.id == DefaultSalesUserId))
                        cboSalesUser.EditValue = DefaultSalesUserId;
                    else if (_lstItems.Exists(i => i.id == 0))
                        cboSalesUser.EditValue = 0;
                }
                catch {
                    return;
                }
            }
        }
        public void LoadSelectedContact(bool pSetDefaultTime = true)
        {
            cboContact.EditValue = null;
            if (ContactPerson != null)
                if (ContactPerson.id > 0)
                    cboContact.EditValue = ContactPerson.id;

            if (pSetDefaultTime)
                cboTime.Text = "08:00";
        }
        public void Default()
        {
            cboContact.ItemIndex = 0;
            tbxRemarks.Text = "";
            dbxDate.EditValue = DateTime.Now;
            cboTime.Text = "08:00";
            cboSalesUser.EditValue = UserSession.CurrentUser.UserId;
        }
        public void Prepare()
        {
            cboContact.Properties.Columns.Clear();
            cboContact.Properties.DataSource = null;
            cboContact.EditValue = null;
            tbxRemarks.Text = "";
            dbxDate.EditValue = DateTime.Now;
            cboSalesUser.EditValue = UserSession.CurrentUser.UserId;

            if (IsNurtureEvent) {
                icboEventType.Properties.ReadOnly = true;
                cboContact.Properties.ReadOnly = true;
                lciDone.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            }
        }
        public void GetEventTypes(int pSubCampaignId = 0, string pDefaultEventType = null)
        {
            List<EventType> _lstEventTypes = new List<EventType>();
            _lstEventTypes.Add(new EventType() { id = -1, name = "Call Back" });
            //_lstEventTypes.Add(new EventType() { id = -2, name = "Follow Up Mail" }); //https://brightvision.jira.com/browse/PLATFORM-2285
            _lstEventTypes.Add(new EventType() { id = -3, name = "Todo" });

            if (pSubCampaignId > 0) {
                m_EventSubCampaignId = pSubCampaignId;
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    int _CampaignId = (int)_efDbContext.subcampaigns.FirstOrDefault(i => i.id == m_EventSubCampaignId).campaign_id;
                    int _CustomerId = (int)_efDbContext.campaigns.FirstOrDefault(i => i.id == _CampaignId).customer_id;
                    List<CTSubCampaignNurtureItem> _lstNurtureItems = _efDbContext.FIGetSubCampaignNurtureList(_CustomerId, _CampaignId, m_EventSubCampaignId).ToList();
                    if (_lstNurtureItems.Count > 0) {
                        foreach (CTSubCampaignNurtureItem _item in _lstNurtureItems) {
                            if (!Convert.ToBoolean(_item.selected))
                                continue;

                            _lstEventTypes.Add(new EventType() { id = _item.sub_campaign_id, name = string.Format("Nurture ({0} > {1} > {2})", _item.customer_name, _item.campaign_name, _item.sub_campaign_name) });
                        }
                    }
                }
            }

            /*
             * https://brightvision.jira.com/browse/PLATFORM-2759
            */
            icboEventType.Properties.Items.Clear();
            int iImageIndex = 0;
            for (int i = 0; i < _lstEventTypes.Count; i++) {
                switch (_lstEventTypes[i].name) {
                    case "Call Back": iImageIndex = 0; break;
                    case "Todo": iImageIndex = 1; break;
                    default: iImageIndex = 2; break;
                }
                icboEventType.Properties.Items.Add(new ImageComboBoxItem(_lstEventTypes[i].name, _lstEventTypes[i].id, iImageIndex));
            }
            icboEventType.Properties.SmallImages = m_images;

            this.icboEventType.EditValueChanged -= new System.EventHandler(this.icboEventType_EditValueChanged);
            if (string.IsNullOrEmpty(pDefaultEventType) || pDefaultEventType.Equals("Make Call"))
                icboEventType.EditValue = -1;
            else if (pDefaultEventType.Equals("Send Mail"))
                icboEventType.EditValue = -2;
            else if (pDefaultEventType.Equals("Todo"))
                icboEventType.EditValue = -3;
            this.icboEventType.EditValueChanged += new System.EventHandler(this.icboEventType_EditValueChanged);
        }
        public void SetSelectedEventType(int pSubCampaignId)
        {
            icboEventType.EditValue = pSubCampaignId;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Control Events
        private void cboContact_EditValueChanged(object sender, EventArgs e)
        {
            //cboContactNo.Text = "";
            //cboContactNo.Properties.Items.Clear();
            //if (cboContact.EditValue == null)
            //    return;

            //int _ContactId = Convert.ToInt32(cboContact.EditValue);
            //BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            //contact _efeContact = _efDbContext.contacts.FirstOrDefault(i => i.id == _ContactId);
            //cboContactNo.Properties.Items.Clear();
            //if (!string.IsNullOrEmpty(_efeContact.direct_phone))
            //    cboContactNo.Properties.Items.Add(_efeContact.direct_phone);
            //if (!string.IsNullOrEmpty(_efeContact.mobile))
            //    cboContactNo.Properties.Items.Add(_efeContact.mobile);
            //if (cboContactNo.Properties.Items.Count > 0)
            //    cboContactNo.SelectedIndex = 0;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cboContact.EditValue == null) {
                NotificationDialog.Information("Bright Sales", "No selected contact for follow up.");
                return;
            }
            else if (dbxDate.Text.Length < 1) {
                NotificationDialog.Information("Bright Sales", "Please select a date.");
                return;
            }
            else if (m_ContactPersons == null) {
                NotificationDialog.Information("Bright Sales", "No selected contact for follow up.");
                return;
            }

            WaitDialog.Show(ParentForm, "Saving data ...");
            string _ContactEventType = "Todo";
            if (Convert.ToInt32(icboEventType.EditValue) > 0)
                _ContactEventType = "Nurture Event";
            else if (icboEventType.Text.Equals("Call Back"))
                _ContactEventType = "Make Call";
            else if (icboEventType.Text.Equals("Follow-Up Mail"))
                _ContactEventType = "Send Mail";
            
            int _ContactId = Convert.ToInt32(cboContact.EditValue);
            CTScSubCampaignContactList _SelectedContact = m_ContactPersons.Find(i => i.id == _ContactId);
            event_followup_log objFollowUpData = null;
            if (_SelectedContact == null) {
                WaitDialog.Close();
                NotificationDialog.Information("Bright Sales", "No selected contact for follow up.");
                return;
            }

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                objFollowUpData = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == FollowUpId);
                if (objFollowUpData == null) {
                    objFollowUpData = new event_followup_log() {
                        account_id = AccountId,
                        date_created = DateTime.Now,
                        created_by = UserSession.CurrentUser.UserId
                    };

                    if (Convert.ToInt32(icboEventType.EditValue) > 0)
                        objFollowUpData.subcampaign_id = Convert.ToInt32(icboEventType.EditValue);
                    else
                        objFollowUpData.subcampaign_id = SubCampaignId;
                }

                objFollowUpData.date_of_transaction = Convert.ToDateTime(dbxDate.EditValue);
                objFollowUpData.start_time = TimeSpan.Parse(cboTime.EditValue.ToString());
                objFollowUpData.end_time = TimeSpan.Parse(cboTime.EditValue.ToString()); // set as the same as end time will not apply for follow ups
                objFollowUpData.contact_id = _SelectedContact.id;
                objFollowUpData.contact_no = string.Empty; //cboContactNo.Text;
                objFollowUpData.title = _SelectedContact.title;
                objFollowUpData.event_type = _ContactEventType;
                objFollowUpData.event_status = icboEventType.Text; //cboStatus.Text;
                objFollowUpData.short_message = tbxRemarks.Text;
                objFollowUpData.assigned_user = Convert.ToInt32(cboSalesUser.EditValue);
                objFollowUpData.done = cbxDone.Checked;
                objFollowUpData.is_saved = true;

                var _item = m_ContactPersons.Find(i => i.id == _SelectedContact.id);
                if (_item != null)
                    objFollowUpData.contact_name = string.Format("{0} {1}", _item.first_name, _item.last_name);

                if (Convert.ToInt32(icboEventType.EditValue) > 0)
                    objFollowUpData.source_sub_campaign_id = SubCampaignId;

                /**
                 * new event log.
                 */
                if (FollowUpId < 1) {
                    bool _SaveExistingFollowUpCallsAsDone = false;
                    if (EventFollowUp.FollowUpCallExists(objFollowUpData)) {
                        WaitDialog.Close();
                        DialogResult _dlg = MessageBox.Show(
                            string.Format("Follow-up call already exist for the current contact.{0}Would you like to mark existing follow-up calls for this contact as done?", Environment.NewLine),
                            "Bright Sales",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );
                        _SaveExistingFollowUpCallsAsDone = true;
                        if (_dlg == DialogResult.No)
                            _SaveExistingFollowUpCallsAsDone = false;

                        WaitDialog.Show(ParentForm, "Saving data ...");
                    }

                    // save the usual event log first
                    EventFollowUp.Save(objFollowUpData, _SaveExistingFollowUpCallsAsDone);

                    /**
                     * if nurture event, copy existing event follow up log to another object, for use in inserting nurture log.
                     * https://brightvision.jira.com/secure/attachment/14513/2012-11-19_2227.png
                     * https://docs.google.com/a/myndconsulting.com/spreadsheet/ccc?key=0Av2stuK1YKUAdFBwemdzOGdlaVdrYnY1VHQ4Rm1LRUE#gid=0
                     */
                    #region ===== Nurture Sub Campaign Accounts & Contacts Insert Handling =====

                    if (Convert.ToInt32(icboEventType.EditValue) > 0) {

                        /**
                         * insert nurture event log
                         */
                        event_followup_log _NurtureLog = new event_followup_log() {
                            subcampaign_id = SubCampaignId, //objFollowUpData.subcampaign_id,
                            account_id = objFollowUpData.account_id,
                            contact_id = objFollowUpData.contact_id,
                            title = objFollowUpData.title,
                            event_type = "Nurture Log",
                            event_status = objFollowUpData.event_status,
                            date_created = objFollowUpData.date_created,
                            start_time = objFollowUpData.start_time,
                            end_time = objFollowUpData.end_time,
                            created_by = objFollowUpData.created_by,
                            date_of_transaction = objFollowUpData.date_of_transaction,
                            contact_name = objFollowUpData.contact_name,
                            contact_no = objFollowUpData.contact_no,
                            is_saved = objFollowUpData.is_saved,
                            nurture_event_followup_log_id = objFollowUpData.id,
                            assigned_user = null,
                            done = false,
                            total_tries = 0,
                            source_sub_campaign_id = null
                        };
                        EventFollowUp.Save(_NurtureLog, false);

                        /**
                         * we will only insert current account not existing in the nurture sub-campaign.
                         */
                        int _SubCampaignId = Convert.ToInt32(icboEventType.EditValue);
                        int _FinalListId = (int)_efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == _SubCampaignId).id;
                        sub_campaign_account_lists _NurtureSubCampaignAccount = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i =>
                            i.final_list_id == _FinalListId &&
                            i.account_id == AccountId &&
                            i.active == true
                        );

                        /**
                         * we will only insert account if not existing in the nurture sub-campaign.
                         */
                        if (_NurtureSubCampaignAccount == null) {
                            _efDbContext.sub_campaign_account_lists.AddObject(new sub_campaign_account_lists() {
                                final_list_id = _FinalListId,
                                account_id = AccountId,
                                assigned_to = cboSalesUser.Text,
                                list_source = this.GetListSource(new GetListSourceArgs() { CompanyId = AccountId, ContactId = 0 }),
                                active = true,
                                created_by = UserSession.CurrentUser.UserId,
                                created_on = DateTime.Now,
                                modified_by = UserSession.CurrentUser.UserId,
                                modified_on = DateTime.Now
                            });
                            _efDbContext.SaveChanges();
                        }
                        else
                            _efDbContext.Detach(_NurtureSubCampaignAccount);

                        /**
                         * we will only insert current account's contacts not existing in the nurture sub-campaign.
                         * we will loop each contact and check if its already existing in the nurture sub-campaign.
                         */
                        List<CTScSubCampaignContactList> _lstContacts = ObjectSubCampaign.GetSubCampaignContacts(SubCampaignId, AccountId, m_FinalListId);
                        if (_lstContacts.Count > 0) {
                            foreach (CTScSubCampaignContactList _efoContact in _lstContacts) {
                                sub_campaign_contact_lists _efoExistingContact = _efDbContext.sub_campaign_contact_lists.FirstOrDefault(i =>
                                    i.final_list_id == _FinalListId &&
                                    i.contact_id == _efoContact.id
                                );

                                /**
                                 * do not insert if contact is existing.
                                 */
                                if (_efoExistingContact != null) {
                                    _efDbContext.Detach(_efoExistingContact);
                                    continue;
                                }

                                /**
                                 * insert contact to nurture sub-campaign.
                                 */
                                _efDbContext.sub_campaign_contact_lists.AddObject(new sub_campaign_contact_lists() {
                                    final_list_id = _FinalListId,
                                    contact_id = _efoContact.id,
                                    in_list = (_efoContact.id == Convert.ToInt32(cboContact.EditValue))? true: false,
                                    active = _efoContact.active,
                                    list_source = (_efoContact.id == Convert.ToInt32(cboContact.EditValue))? this.GetListSource(new GetListSourceArgs() { CompanyId = 0, ContactId = _efoContact.id }): string.Empty,
                                    created_by = UserSession.CurrentUser.UserId,
                                    created_on = DateTime.Now,
                                    modified_by = UserSession.CurrentUser.UserId,
                                    modified_on = DateTime.Now
                                });
                            }
                            _efDbContext.SaveChanges();
                        }
                    }
                    #endregion

                    this.Default();
                    if (btnSave_OnClick != null)
                        btnSave_OnClick(sender, new EventFollowUpLogEvents.OnSaveArguments());

                    WaitDialog.Close();
                    this.ParentForm.Hide();
                }

                /**
                 * edit event log.
                 */
                else {
                    _efDbContext.event_followup_log.ApplyCurrentValues(objFollowUpData);
                    _efDbContext.SaveChanges();
                    if (btnSave_OnClick != null)
                        btnSave_OnClick(sender, new EventFollowUpLogEvents.OnSaveArguments() {
                            EventLog = objFollowUpData
                        });

                    WaitDialog.Close();
                    this.ParentForm.Close();                    
                }
            }
        }
        private void btnHide_Click(object sender, EventArgs e)
        {
            this.ParentForm.Hide();
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            this.Default();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Default();
            this.ParentForm.Hide();
        }
        private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsNurtureEvent) {
                this.Dispose(true);
                return;
            }

            if (e.CloseReason == CloseReason.ApplicationExitCall ||
                e.CloseReason == CloseReason.FormOwnerClosing ||
                e.CloseReason == CloseReason.MdiFormClosing ||
                e.CloseReason == CloseReason.TaskManagerClosing ||
                e.CloseReason == CloseReason.WindowsShutDown)
                return;

            this.ParentForm.Hide();
            e.Cancel = true;
        }
        private void FollowUpEditor_Load(object sender, EventArgs e)
        {
            this.ParentForm.FormClosing += new FormClosingEventHandler(ParentForm_FormClosing);
        }
        private void icboEventType_EditValueChanged(object sender, EventArgs e)
        {
            cboSalesUser.Text = string.Empty;
            if (icboEventType.EditValue == null)
                return;

            if (Convert.ToInt32(icboEventType.EditValue) > 0)
                this.LoadSalesUsers(Convert.ToInt32(icboEventType.EditValue));
        }
        #endregion
    }
}