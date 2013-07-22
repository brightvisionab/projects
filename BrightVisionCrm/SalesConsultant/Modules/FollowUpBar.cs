
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using SalesConsultant.Forms;
using BrightVision.Model;
using System.Linq;
using SalesConsultant.PublicProperties;
using SalesConsultant.Events;
using BrightVision.Common.Events.Core;
using SalesConsultant.Facade;

namespace SalesConsultant.Modules
{
    public partial class FollowUpBar : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public FollowUpBar()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Events & Args
        //public delegate void LoadFollowUpsEventHandler();
        //public event LoadFollowUpsEventHandler LoadFollowUps;

        //public delegate void btnSaveOnClickEventHandler();
        //public event btnSaveOnClickEventHandler btnSave_OnClick;

        //public delegate bool CanWorkOnCompanyEventHandler();
        //public event CanWorkOnCompanyEventHandler CanWorkOnCompany;

        //public delegate bool HasPendingCallAndLogEventHandler();
        //public event HasPendingCallAndLogEventHandler HasPendingCallAndLog;

        //public delegate bool HasBrowsableDataEventHandler();
        //public event HasBrowsableDataEventHandler HasBrowsableData;

        //public delegate CampaignBookingProperty.CampaignBoookingArguments GetCampaignBookingArgsEventHandler(bool pForWorkModePurpose);
        //public event GetCampaignBookingArgsEventHandler GetCampaignBookingArgs;

        //public delegate List<CTScSubCampaignContactList> GetCampaignBookingContactListEventHandler();
        //public event GetCampaignBookingContactListEventHandler GetCampaignBookingContactList;
        
        //public delegate void btnTopOnClickEventHandler();
        //public event btnTopOnClickEventHandler btnTop_OnClick;

        //public delegate void btnPreviousOnClickEventHandler();
        //public event btnPreviousOnClickEventHandler btnPrevious_OnClick;

        //public delegate void btnNextOnClickEventHandler();
        //public event btnNextOnClickEventHandler btnNext_OnClick;

        //public delegate void btnLoadOnClickEventHandler(object sender, CampaignBookingProperty.CampaignBoookingArguments e);
        //public event btnLoadOnClickEventHandler btnLoad_OnClick;
        //public class btnLoadOnClickArgs : EventArgs {
        //    public CampaignBookingProperty.CampaignBoookingArguments CampaignBookingArgs { get; set; }
        //}
        #endregion

        #region Public Properties
        public int EventId { 
            get; 
            set; 
        }
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private List<CTScSubCampaignContactList> m_ContactList = null;
        private FollowUpEditor m_Editor = null;
        private PopupDialog m_dlgEditor = null;
        #endregion

        #region Public Methods
        public void Clear()
        {
            tbxCampaignInfo.Text = string.Empty;
            tbxCampaignInfo.ToolTip = string.Empty;
            EventId = 0;
        }
        public void SetCampaignInfo(string pCampaignInfo, string pToolTipInfo, int pEventId = -1)
        {
            if (pEventId > 0) {
                if (!string.IsNullOrEmpty(tbxCampaignInfo.Text)) {
                    tbxCampaignInfo.Text = pCampaignInfo;
                    tbxCampaignInfo.ToolTip = pToolTipInfo;
                    EventId = pEventId;
                }
            }
            else {
                tbxCampaignInfo.Text = pCampaignInfo;
                tbxCampaignInfo.ToolTip = pToolTipInfo;
            }
        }
        #endregion

        #region Private Methods
        #endregion

        #region Control Events
        private void btnTop_Click(object sender, EventArgs e)
        {
            m_EventBus.Notify(new FollowUpBarEvents.CheckForBrowsableData());
            //if (!HasBrowsableData())
            if (!m_BrightSalesProperty.EventsProperty.HasDataRows)
                m_EventBus.Notify(new FollowUpBarEvents.OnLoadPress());

                //LoadFollowUps();
                //MessageBox.Show("No browsable data is available for my follow ups tab.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //return;

            //WaitDialog.Show("Loading data ...");
            this.Cursor = Cursors.WaitCursor;
            m_EventBus.Notify(new FollowUpBarEvents.OnMoveFirst());
            //if (btnTop_OnClick != null)
            //    btnTop_OnClick();
            this.Cursor = Cursors.Default;
            //WaitDialog.Close();
        }
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            //if (!HasBrowsableData()) {
           m_EventBus.Notify(new FollowUpBarEvents.CheckForBrowsableData());
           if (!m_BrightSalesProperty.EventsProperty.HasDataRows) {
                if (!m_BrightSalesProperty.EventsProperty.HasDataRows)
                NotificationDialog.Information("Bright Sales", "No browsable data is available for my follow ups tab.");
                return;
            }

            //WaitDialog.Show("Loading data ...");
            this.Cursor = Cursors.WaitCursor;
            m_EventBus.Notify(new FollowUpBarEvents.OnMovePrevious());
            //if (btnPrevious_OnClick != null)
            //    btnPrevious_OnClick();
            this.Cursor = Cursors.Default;
            //WaitDialog.Close();
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            //if (!HasBrowsableData()) {
            m_EventBus.Notify(new FollowUpBarEvents.CheckForBrowsableData());
            if (!m_BrightSalesProperty.EventsProperty.HasDataRows)
            {
                NotificationDialog.Information("Bright Sales", "No browsable data is available for my follow ups tab.");
                return;
            }

            //WaitDialog.Show("Loading data ...");
            this.Cursor = Cursors.WaitCursor;
            m_EventBus.Notify(new FollowUpBarEvents.OnMoveNext());
            //if (btnNext_OnClick != null)
            //    btnNext_OnClick();
            this.Cursor = Cursors.Default;
            //WaitDialog.Close();
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            //if (!CanWorkOnCompany())
            //    return;

            if (EventId < 1) {
                NotificationDialog.Error("Bright Sales", "No loadable data is available.");
                return;
            }
            else if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit) {
                NotificationDialog.Warning("Bright Sales", "Dialog on edit mode.");
                return;
            }
            else if (m_BrightSalesProperty.CommonProperty.OnCallMode || !m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Information("Bright Sales", "A call is already in progress or there is a pending call log to be saved.");
                return;
            }

            m_EventBus.Notify(new FollowUpBarEvents.OnLoad());

            //CampaignBookingProperty.CampaignBoookingArguments _args = GetCampaignBookingArgs(true);
            //if (_args == null)
            //    return;

            //WaitDialog.Show("Loading data ...");
            //if (btnLoad_OnClick != null)
            //    btnLoad_OnClick(this, _args);

            //WaitDialog.Close();
        }
        private void btnEditFollowUpDetails_Click(object sender, EventArgs e)
        {
            if (EventId < 1)
            {
                NotificationDialog.Error("Bright Sales", "No editable data is available.");
                return;
            }

            m_EventBus.Notify(new FollowUpBarEvents.GetCampaignBookingArgs() { 
                ForWorkModePurpose = false 
            });

            //CampaignBookingProperty.CampaignBoookingArguments _args = GetCampaignBookingArgs(false);
            if (m_BrightSalesProperty.EventsProperty.CampaignBookingArgs == null || m_BrightSalesProperty.EventsProperty.CampaignBookingArgs.oAppointment == null)
                return;

            WaitDialog.Show("Loading Data ...");
            #region Initialize Editor
            m_Editor = new FollowUpEditor() {
                Dock = DockStyle.Fill,
                IsNurtureEvent = false
            };
            m_Editor.btnSave_OnClick += new FollowUpEditor.btnSaveOnClickEventHandler(m_Editor_btnSave_OnClick);
            m_dlgEditor = new PopupDialog() {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Edit Current Follow Up",
                ClientSize = new Size(m_Editor.Width + 2, m_Editor.Height + 2),
                CloseBox = false
            };
            m_dlgEditor.Controls.Add(m_Editor);
            #endregion

            CampaignBookingProperty.CampaignBoookingArguments _args = m_BrightSalesProperty.EventsProperty.CampaignBookingArgs;
            //bool IsDeActivated = false;
            int _FinalListId;
            event_followup_log _data;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _data = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _args.Id);
                _FinalListId = (int)_efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == _data.subcampaign_id).id;
                _efDbContext.Detach(_data);


                /*
                 * https://brightvision.jira.com/browse/PLATFORM-3070
                 * DAN: Inrelated to fixing the issue as must not be able to edit when account is already deactivated.
                 */
                sub_campaign_account_lists _eftSubCampaignAccount = _efDbContext.sub_campaign_account_lists.FirstOrDefault(p =>
                   p.account_id == _data.account_id &&
                   p.final_list_id == _FinalListId &&
                   p.active == true
                );

                if (_eftSubCampaignAccount == null) {
                    NotificationDialog.Warning("Bright Sales", "This account has been de-activated");
                    WaitDialog.Close();
                    return;
                }

                _efDbContext.Detach(_eftSubCampaignAccount);

                //if (_eftSubCampaignAccount != null)
                //    _efDbContext.Detach(_eftSubCampaignAccount);
                //else
                //    IsDeActivated = true;
            }

            //if (IsDeActivated)
            //{
            //    NotificationDialog.Warning("Bright Sales", "This account has been de-activated");
            //    //return;
            //}

            if (_data.event_type.Equals("Nurture Event"))
                m_Editor.IsNurtureEvent = true;

            m_Editor.SubCampaignId = _args.oAppointment.SubCampaignId;
            m_Editor.AccountId = _args.oAppointment.AccountId;
            m_Editor.Prepare();

            if (!_data.event_type.Equals("Nurture Event")) {
                m_Editor.GetEventTypes(0, _data.event_type);
                m_Editor.LoadSalesUsers((int)_data.subcampaign_id, (int)((_data.assigned_user != null)?_data.assigned_user:0));
                m_Editor.SetCampaignInfo(_args);
                m_ContactList = ObjectSubCampaign.GetSubCampaignContacts(_args.oAppointment.SubCampaignId, _args.oAppointment.AccountId, _args.oAppointment.FinalListId);
            }
            else {
                m_Editor.GetEventTypes((int)_data.source_sub_campaign_id);
                m_Editor.SetSelectedEventType((int)_data.subcampaign_id);
                m_Editor.LoadSalesUsers((int)_data.subcampaign_id, (int)_data.assigned_user);
                m_Editor.SetCampaignInfo(_data);
                m_ContactList = ObjectSubCampaign.GetSubCampaignContacts((int)_data.subcampaign_id, (int)_data.account_id, _FinalListId);
            }
            
            if (m_ContactList.Count > 0) {
                m_Editor.LoadContactPersons(m_ContactList);
                CTScSubCampaignContactList _contact = m_ContactList.Find(i => i.id == (int)_data.contact_id);
                if (_contact != null) {
                    m_Editor.ContactPerson = _contact;
                    m_Editor.LoadSelectedContact(false);
                }
            }

            WaitDialog.Close();
            m_dlgEditor.ShowDialog(this);
        }
        #endregion

        #region Subscribed Events
        private void m_Editor_btnSave_OnClick(object sender, EventFollowUpLogEvents.OnSaveArguments e)
        {
            m_EventBus.Notify(new FollowUpBarEvents.OnSave());

            //if (btnSave_OnClick != null)
            //    btnSave_OnClick();
        }
        #endregion
    }
}