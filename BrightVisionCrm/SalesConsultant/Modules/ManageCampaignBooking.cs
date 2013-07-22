
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Globalization;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraVerticalGrid.Rows;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.Data.Filtering;
using DevExpress.XtraRichEdit;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;
using BrightVision.Common.Events.Core;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BrightVision.Common.Modules;
using BrightVision.Common.Business;
using SalesConsultant.Modules;
using SalesConsultant.Business;
using SalesConsultant.Forms;
using SalesConsultant.Events;
using BrightVision.Logging.Enums;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using DevExpress.XtraEditors.Popup;
using DevExpress.Utils.Win;

namespace SalesConsultant.Modules 
{
    public partial class ManageCampaignBooking : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Event Handlers
        public delegate void ContactViewPopulateContactViewInitiatedEventHandler(object sender, ContactViewPopulateContactView e);
        public event ContactViewPopulateContactViewInitiatedEventHandler oContactView_PopulateContactView_Initiated;
        public class ContactViewPopulateContactView : EventArgs {
            public List<CTScSubCampaignContactList> ContactPersons { get; set; }
            public int SelectedContactPersonIndex { get; set; }
        }

        public delegate void ModuleSetStateInitiatedEventHandler(object sender, ModuleSetStateArgs e);
        public event ModuleSetStateInitiatedEventHandler ModuleSetStateInitiated;
        public class ModuleSetStateArgs : EventArgs {
            public SelectionProperty.CampaignBookingMode CampaignBookingMode { get; set; }
        }

        public delegate void CampaignExtraDetailOnContactSavedEventHandler(object sender, CampaignExtraDetailOnContactSavedArgs e);
        public event CampaignExtraDetailOnContactSavedEventHandler CampaignExtraDetail_OnContactSaved;
        public class CampaignExtraDetailOnContactSavedArgs : EventArgs {
            public CTScSubCampaignContactList ContactPerson { get; set; }
        }

        public delegate void ContactViewOnContactSavedEventHandler(object sender, ContactViewOnContactSavedArgs e);
        public event ContactViewOnContactSavedEventHandler ContactView_OnContactSaved;
        public class ContactViewOnContactSavedArgs : EventArgs {
            public CTScSubCampaignContactList ContactPerson { get; set; }
        }

        public delegate void ContactViewFocusedRowChangedEventHandler(object sender, ContactViewFocusedRowChangedArgs e);
        public event ContactViewFocusedRowChangedEventHandler ContactView_FocusedRowChanged;
        public class ContactViewFocusedRowChangedArgs : EventArgs {
            public CTScSubCampaignContactList ContactPerson { get; set; }
        }

        public delegate void SetCompanyModificationInfoHandler();
        public event SetCompanyModificationInfoHandler SetCompanyModificationInfo;

        public delegate void DialogOnEditModeEventHandler();
        public event DialogOnEditModeEventHandler DialogOnEditMode;
        #endregion

        #region Public Event Handler Arguments
        public delegate void EventFollowupLogWorkNurtureEventEventHandler(CampaignBookingProperty.CampaignBoookingArguments e);
        public event EventFollowupLogWorkNurtureEventEventHandler EventFollowupLog_WorkNurtureEvent;
        #endregion

        #region Public Properties
        public SubCampaignAppointment oAppointment { get; set; }
        public string SelectedCompany { get; set; }
        public int SelectedContactId { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyStatus {
            get {
                if (cboAccountStatus.Text.Length < 1)
                    return string.Empty;
                else
                    return cboAccountStatus.Text;
            }
        }
        public string CompanyLeadStatus {
            get {
                if (cboAccountLeadStatus.Text.Length < 1)
                    return null;
                else
                    return cboAccountLeadStatus.Text;
            }
        }
        public bool StatusChanged {
            get { return m_StatusChanged; }
        }
        public bool DialogChanged
        {
            get { return m_DialogChanged; }
        }
        public bool ShouldSave {
            get {
                if (m_DialogChanged || m_StatusChanged)
                    return true;

                return false;
            }
        }
        public bool IsLoadingCampaignBooking {
            get;
            set;
        }
        #endregion

        #region Private Properties
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private ContactView m_oContactView = null;
        private DialogEditor m_oDialogEditor = null;
        private EventFollowUpLog m_oEventFollowupLog = null;
        private CampaignExtraDetail m_CampaignExtraDetail = null;
        private bool m_ContactViewOnInitialize = false;
        private bool m_IsReleaseCancel = false;
        /**
         * [@jeff 07.10.2012]: https://brightvision.jira.com/browse/PLATFORM-1461
         * this will hold the current loaded company and lead statuses.
         * for use in comparison with the latest company and lead status when saving campaign booking.
         * used to distinguish event ids 12 & 13
         * google docs: https://docs.google.com/a/myndconsulting.com/spreadsheet/ccc?key=0Av2stuK1YKUAdGEwTmhsZ0NlVDRHVXl6elYwZzh6SXc#gid=20
         */
        private string m_LastSavedCompanyStatus = string.Empty;
        private string m_LastSavedCompanyLeadStatus = string.Empty;

        private bool m_StatusChanged = false;
        private bool m_DialogChanged = false;

        /// <summary>
        /// tells the gui that the campaign booking is currently being loaded from a (work/load) button event.
        /// </summary>
        private bool m_LoadingFromButtonPress = false;
        #endregion

        #region Constructors
        public ManageCampaignBooking() 
        {
            InitializeComponent();
            this.RegisterEvents();
        }
        #endregion

        #region Control Events
        private void vGridControlCompanyInfo_RecordCellStyle(object sender, DevExpress.XtraVerticalGrid.Events.GetCustomRowCellStyleEventArgs e) 
        {
            e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
        }
        private void vGridControlContact_RecordCellStyle(object sender, DevExpress.XtraVerticalGrid.Events.GetCustomRowCellStyleEventArgs e) 
        {
            DevExpress.XtraVerticalGrid.VGridControl vgrid = sender as DevExpress.XtraVerticalGrid.VGridControl;
            if (e.Row.Properties.ReadOnly) {
                //Apply the appearance of the SelectedRow                                
                e.Appearance.Assign(vgrid.Appearance.ReadOnlyRow);
                //Just to illustrate how the code works. Remove the following lines to see the desired appearance.
                e.Appearance.Options.UseForeColor = true;
                e.Appearance.ForeColor = Color.Black;
            }
        }
        private void cmdNextRecord_Click(object sender, EventArgs e)
        {
            if (m_oDialogEditor.OnEditMode)
                return;

            if (!m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return;
            }

            this.SuspendLayout();
            cboAccountLeadStatus.SuspendLayout();
            cboAccountStatus.SuspendLayout();
            WaitDialog.Show("Saving and loading.");
            IsLoadingCampaignBooking = true;
            pnlContactView.Enabled = false;
            pnlDialogEditor.Enabled = false;
            this.SaveCampaignBookingData();

            if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList) {
                m_EventBus.Notify(new ManageCampaignBookingEvents.OnLoadNextCompany.CampaignList());
                m_EventBus.Notify(new ManageCampaignBookingEvents.SetCallViewBarState() {
                    State = !m_BrightSalesProperty.CommonProperty.CompanyLocked && m_BrightSalesProperty.CampaignBooking.LoadNextCompanySuccess ? true : false
                });
            }

            if (m_BrightSalesProperty.CampaignBooking.LoadNextCompanySuccess) {
                m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = true });
                //this.SetBreadCrumb(m_BrightSalesProperty.CampaignBooking.BreadCrumb);
                this.ResetContactParameters();
                this.InitializeCampaignBooking();
                if (m_BrightSalesProperty.CommonProperty.CompanyLocked) {
                    m_BrightSalesProperty.CampaignBooking.Mode = SelectionProperty.CampaignBookingMode.Browse;
                    this.LoadCampaignBookingData(true);
                    lblBreadCrumb.AppearanceItemCaption.BackColor = System.Drawing.SystemColors.ControlDark;
                }
                else {
                    m_BrightSalesProperty.CampaignBooking.Mode = SelectionProperty.CampaignBookingMode.Work;
                    this.LoadCampaignBookingData();
                    lblBreadCrumb.AppearanceItemCaption.BackColor = System.Drawing.Color.FromName("0");
                }
                btnPreviousRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsFirstRow;
                btnNextRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsLastRow;


                this.SetBreadCrumb(m_BrightSalesProperty.CampaignBooking.BreadCrumb);
            }

            pnlContactView.Enabled = true;
            pnlDialogEditor.Enabled = true;
            IsLoadingCampaignBooking = false;
            cboAccountLeadStatus.ResumeLayout(true);
            cboAccountStatus.ResumeLayout(true);
            this.ResumeLayout(true);
            WaitDialog.Close(true);
        }
        private void cmdPreviousRecord_Click(object sender, EventArgs e)
        {
            if (m_oDialogEditor.OnEditMode)
                return;

            if (!m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return;
            }

            WaitDialog.Show("Checking/saving for changes.");
            IsLoadingCampaignBooking = true;
            pnlContactView.Enabled = false;
            pnlDialogEditor.Enabled = false;
            this.SaveCampaignBookingData();

            if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList) {
                m_EventBus.Notify(new ManageCampaignBookingEvents.OnLoadPreviousCompany.CampaignList());
                m_EventBus.Notify(new ManageCampaignBookingEvents.SetCallViewBarState() {
                    State = !m_BrightSalesProperty.CommonProperty.CompanyLocked && m_BrightSalesProperty.CampaignBooking.LoadPreviousCompanySuccess ? true : false
                });
            }
            
            if (m_BrightSalesProperty.CampaignBooking.LoadPreviousCompanySuccess) {
                m_EventBus.Notify(new CampaignListEvents.GetLatestProperties() { ForWorkModePurpose = true });
                this.SetBreadCrumb(m_BrightSalesProperty.CampaignBooking.BreadCrumb);
                this.ResetContactParameters();
                this.InitializeCampaignBooking();
                if (m_BrightSalesProperty.CommonProperty.CompanyLocked) {
                    m_BrightSalesProperty.CampaignBooking.Mode = SelectionProperty.CampaignBookingMode.Browse;
                    this.LoadCampaignBookingData(true);
                    lblBreadCrumb.AppearanceItemCaption.BackColor = System.Drawing.SystemColors.ControlDark;
                }
                else {
                    m_BrightSalesProperty.CampaignBooking.Mode = SelectionProperty.CampaignBookingMode.Work;
                    this.LoadCampaignBookingData();
                    lblBreadCrumb.AppearanceItemCaption.BackColor = System.Drawing.Color.FromName("0");
                }
                btnPreviousRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsFirstRow;
                btnNextRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsLastRow;
            }

            pnlContactView.Enabled = true;
            pnlDialogEditor.Enabled = true;
            IsLoadingCampaignBooking = false;
            WaitDialog.Close(true);
        }  
        private void cboAccountLeadStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            /**
             * [@jeff 05.16.2013]
             * https://brightvision.jira.com/browse/PLATFORM-2908
             */
            WaitDialog.Show("Saving changes ...");
            this.SaveCampaignBookingWorkStatus();
            WaitDialog.Close();

            m_StatusChanged = true;
        }
        private void cboAccountStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            /**
             * on release, send email.
             * depends on the subcampaign xml configuration on what is set as send_email="true".
             */
            //if (cboAccountStatus.SelectedIndex == m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatusSendEmailIndex && !m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountLockedByOtherUser()) {
            if (cboAccountStatus.SelectedIndex == m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatuses.FindIndex(i => i.send_email == true) && !m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountLockedByOtherUser()) {

                /**
                 * load send email contacts form.
                 * if return is OK(release button pressed), then load the send email recipients form.
                 */
                FrmSendEmailContacts _control = new FrmSendEmailContacts(FrmSendEmailContacts.eCallMode.OnRelease) {
                    StartPosition = FormStartPosition.CenterParent
                };
                DialogResult _dlg = _control.ShowDialog();

                /**
                 * release and send email.
                 */
                if (_dlg == DialogResult.OK) {
                    this.SaveLeadStatusChanges();
                    FrmSendEmailRecipients _recipients = new FrmSendEmailRecipients() {
                        StartPosition = FormStartPosition.CenterParent
                    };
                    _recipients.ShowDialog();
                }

                /**
                 * release only.
                 */
                else if (_dlg == DialogResult.Ignore)
                    this.SaveLeadStatusChanges();

                else if (_dlg == DialogResult.Cancel) {
                    m_IsReleaseCancel = true;
                    cboAccountStatus.SelectedIndex = int.Parse(cboAccountStatus.Tag.ToString());
                    m_IsReleaseCancel = false;
                }

            }
            else if (!m_IsReleaseCancel)
                this.SaveLeadStatusChanges();

            /**
                * [@jeff 05.16.2013]
                * https://brightvision.jira.com/browse/PLATFORM-2908
            */
            //WaitDialog.Show("Saving changes ...");
            //this.SaveCampaignBookingWorkStatus();
            //WaitDialog.Close();

            cboAccountStatus.Tag = cboAccountStatus.SelectedIndex;
            m_StatusChanged = true;
        }
        private void SaveLeadStatusChanges()
        {
            WaitDialog.Show("Saving changes ...");
            this.SaveCampaignBookingWorkStatus();
            WaitDialog.Close();
        }
        private void btnSendMail_Click(object sender, EventArgs e)
        {
            FrmSendEmailContacts _control = new FrmSendEmailContacts(FrmSendEmailContacts.eCallMode.OnSendEmail) {
                StartPosition = FormStartPosition.CenterParent
            };
            DialogResult _dlg = _control.ShowDialog();
            if (_dlg == DialogResult.OK) {
                FrmSendEmailRecipients _recipients = new FrmSendEmailRecipients() {
                    StartPosition = FormStartPosition.CenterParent
                };
                _recipients.ShowDialog();
            }
        }
        private void cboAccountLeadStatus_Popup(object sender, EventArgs e)
        {
            PopupListBoxForm _ttcboAccountLeadStatus = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            _ttcboAccountLeadStatus.ListBox.MouseMove += _ttcboAccountLeadStatus_MouseMove;
            _ttcboAccountLeadStatus.ListBox.MouseLeave += _ttcboAccountLeadStatus_MouseLeave;
        }
        private void _ttcboAccountLeadStatus_MouseLeave(object sender, EventArgs e)
        {
            popupDetails.HideHint();
        }
        private void _ttcboAccountLeadStatus_MouseMove(object sender, MouseEventArgs e)
        {
            PopupListBox _lbxDialog = sender as PopupListBox;
            ComboBoxEdit _cboLeadStatus = _lbxDialog.OwnerEdit as ComboBoxEdit;
            int index = _lbxDialog.IndexFromPoint(new Point(e.X, e.Y));
            if (index == -1)
                popupDetails.HideHint();
            else {
                string _item = _cboLeadStatus.Properties.Items[index].ToString();
                XmlUtility.SubCampaignConfig _ConfigItem = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatuses.FirstOrDefault(i => i.status.Equals(_item));
                string _dlgPopup = string.Format("Lead Status: {0}{1}{1}{2}",
                    _ConfigItem.status,
                    Environment.NewLine,
                    _ConfigItem.description
                );
                popupDetails.ShowHint(_dlgPopup, _lbxDialog.PointToScreen(new Point(e.X, e.Y)));
            }
        }
        private void cboAccountStatus_Popup(object sender, EventArgs e)
        {
            PopupListBoxForm _ttcboAccountStatus = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            _ttcboAccountStatus.ListBox.MouseMove += _ttcboAccountStatus_MouseMove;
            _ttcboAccountStatus.ListBox.MouseLeave += _ttcboAccountStatus_MouseLeave;
        }
        private void _ttcboAccountStatus_MouseLeave(object sender, EventArgs e)
        {
            popupDetails.HideHint();
        }
        private void _ttcboAccountStatus_MouseMove(object sender, MouseEventArgs e)
        {
            PopupListBox _lbxDialog = sender as PopupListBox;
            ComboBoxEdit _cboLeadStatus = _lbxDialog.OwnerEdit as ComboBoxEdit;
            int index = _lbxDialog.IndexFromPoint(new Point(e.X, e.Y));
            if (index == -1)
                popupDetails.HideHint();
            else {
                string _item = _cboLeadStatus.Properties.Items[index].ToString();
                XmlUtility.SubCampaignConfig _ConfigItem = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatuses.FirstOrDefault(i => i.status.Equals(_item));
                string _dlgPopup = string.Format("Lead Status: {0}{1}{1}{2}",
                    _ConfigItem.status,
                    Environment.NewLine,
                    _ConfigItem.description
                );
                popupDetails.ShowHint(_dlgPopup, _lbxDialog.PointToScreen(new Point(e.X, e.Y)));
            }
        }
        #endregion

        #region Private Methods
        private void SaveCampaignBookingWorkStatus()
        {
            /**
             * [@jeff 07.10.2012]: https://brightvision.jira.com/browse/PLATFORM-1461
             * event logging for event ids 12 & 13
             */
            if (m_LastSavedCompanyStatus != cboAccountStatus.Text) {
                string _PriorState = m_LastSavedCompanyStatus;
                string _NewState = cboAccountStatus.Text;
                m_LastSavedCompanyStatus = cboAccountStatus.Text;
                this.LogAnEvent(
                    BrightVision.Common.Classes.EventLog.EventTypes.CHANGE_COMPANY_STATUS_SAVE_EVENT,
                    new string[] {
                        null,
                        _PriorState,
                        _NewState,
                    }
                );
            }
            if (m_LastSavedCompanyLeadStatus != cboAccountLeadStatus.Text) {
                string _PriorState = m_LastSavedCompanyLeadStatus;
                string _NewState = cboAccountLeadStatus.Text;
                m_LastSavedCompanyLeadStatus = cboAccountLeadStatus.Text;
                this.LogAnEvent(
                    BrightVision.Common.Classes.EventLog.EventTypes.CHANGE_LEAD_STATUS_SAVE_EVENT,
                    new string[] {
                        null,
                        _PriorState,
                        _NewState,
                    }
                );
            }

            /**
             * [@jeff 05.16.2013]: https://brightvision.jira.com/browse/PLATFORM-2908
             * save statuses
             */
            m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus = cboAccountStatus.Text;
            m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus = cboAccountLeadStatus.Text;
            m_EventBus.Notify(new ManageCampaignBookingEvents.OnStatusChange());
            this.SetLastUpdatedInfo();
        }
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<EventFollowUpLogEvents.CompanyRemarksSaved.ManageCampaignBooking>().Subscribe(CompanyRemarskSaved);
            m_EventBus.GetEvent<ContactViewEvents.ContactChanged>().Subscribe(ContactChanged);
            m_EventBus.GetEvent<LinkedInEvents.OnSave.ManageCampaignBooking>().Subscribe(LinkedInChanged);
            m_EventBus.GetEvent<FrmReleaseEvents.SaveCampaignBooking>().Subscribe(SaveBeforeSendEmail);
            m_EventBus.GetEvent<DialogEditorEvents.OnAnswerChange>().Subscribe(DialogAnswerChanged);
            m_EventBus.GetEvent<DialogEditorEvents.OnDelete>().Subscribe(DialogEditorOnDelete);
            m_EventBus.GetEvent<DialogEditorEvents.AfterDelete>().Subscribe(DialogEditorAfterDelete);
            m_EventBus.GetEvent<DialogEditorEvents.OnSaveCompleted>().Subscribe(DialogEditorOnSaveCompleted);
            m_EventBus.GetEvent<DialogEditorEvents.OnEditDialog>().Subscribe(DialogEditorOnEditDialog);
            m_EventBus.GetEvent<EventFollowUpLogEvents.OnSelectContactGridMenuClick>().Subscribe(EventLogSelectContactGridMenuClicked);
            m_EventBus.GetEvent<CompanyInformationEvents.OnSave.ManageCampaignBooking>().Subscribe(CompanyInformationOnSave);
            m_EventBus.GetEvent<ContactInformationEvents.OnSave.ManageCampaignBooking>().Subscribe(ContactInformationOnSave);
            m_EventBus.GetEvent<ContactInformationEvents.OnDelete.ManageCampaignBooking>().Subscribe(ContactInformationOnDelete);
        }
        private void ContactInformationOnDelete(ContactInformationEvents.OnDelete.ManageCampaignBooking e)
        {
            m_oContactView.DeActivateContact();
        }
        private void ContactInformationOnSave(ContactInformationEvents.OnSave.ManageCampaignBooking e)
        {
            m_oContactView.ExtraDetailContactId = e.OnSaveArgs.ContactDetail.id;
            if (m_oContactView.ContactList.Exists(i => i.id == e.OnSaveArgs.ContactDetail.id)) {
                m_oContactView.UpdateContact(e.OnSaveArgs.ContactDetail);
                m_oDialogEditor.BindDialogManagerData(true); //marker...
            }
            else {
                m_oContactView.SuspendEventDataSourceChange(true);
                m_oContactView.PopulateContacts();
                m_oContactView.ContactSetFocus(e.OnSaveArgs.ContactDetail.id);
                m_oContactView.SuspendEventDataSourceChange(false);                
            }

            m_oContactView.gvContact.Focus();
            var _Contact = m_BrightSalesProperty.CommonProperty.ContactPerson;
            if (_Contact == null)
                return;

            /**
             * [jeff 05.16.2012]: https://brightvision.jira.com/browse/PLATFORM-1407
             * when on call mode, button states should be retained, until the user
             * hangs up the current call session.
             */
            if (m_BrightSalesProperty.CommonProperty.OnCallMode)
                return;

            if (CampaignExtraDetail_OnContactSaved != null)
                CampaignExtraDetail_OnContactSaved(this, new CampaignExtraDetailOnContactSavedArgs() {
                    ContactPerson = _Contact
                });
        }
        private void CompanyInformationOnSave(CompanyInformationEvents.OnSave.ManageCampaignBooking e)
        {
            if (m_oEventFollowupLog != null)
                m_oEventFollowupLog.UpdateCompanyRemarks();

            this.SetCompanyDirectCallPolicyImage();
        }
        private void EventLogSelectContactGridMenuClicked(EventFollowUpLogEvents.OnSelectContactGridMenuClick e)
        {
            if (m_oDialogEditor.OnEditMode) {
                if (DialogOnEditMode != null)
                    DialogOnEditMode();

                return;
            }

            if (m_oContactView != null)
                m_oContactView.ContactSetFocus(e.ContactId);
        }
        private void DialogEditorOnEditDialog(DialogEditorEvents.OnEditDialog e)
        {
            m_DialogChanged = true;
            m_CampaignExtraDetail.SetDialogEditorSaveMode(SelectionProperty.DialogSaveMode.Edit);
            m_CampaignExtraDetail.SetModulesAsReadOnly(true);
            this.ToggleButtonHeader(false);
        }
        private void DialogEditorOnSaveCompleted(DialogEditorEvents.OnSaveCompleted e)
        {
            if (m_oContactView != null) {
                try {
                    if (e.TransactionSource == DialogEditorEvents.eTransactionSource.OnSaveButtonClick)
                        m_oContactView.DialogContactId = e.OnSaveCompletedArgs.ContactId;

                    m_oContactView.SaveSubCampaignContactAppointmentStatus(
                        e.OnSaveCompletedArgs.Status,
                        m_BrightSalesProperty.CommonProperty.FinalListId,
                        m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                        m_BrightSalesProperty.CommonProperty.SubCampaignId
                    );
                    m_oContactView.UpdateContactLastUser();
                }
                catch (Exception ex) {
                    m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                        ErrorMessage = ex.Message
                    });
                }

                if (e.TransactionSource == DialogEditorEvents.eTransactionSource.OnSaveButtonClick)
                    m_oDialogEditor.CloseDialogEditor();
            }

            if (e.OnSaveCompletedArgs.EventLog != null)
                this.LogAnEvent(e.OnSaveCompletedArgs.EventLog.EventType, e.OnSaveCompletedArgs.EventLog.Parameters);

            if (m_CampaignExtraDetail != null) {
                m_CampaignExtraDetail.UpdateStatisticsData();
                if (e.TransactionSource == DialogEditorEvents.eTransactionSource.OnSaveButtonClick) {
                    m_CampaignExtraDetail.ReloadCollectedData = true;
                    m_CampaignExtraDetail.LoadSelectedPage();
                }
            }

            if (e.TransactionSource == DialogEditorEvents.eTransactionSource.OnSaveButtonClick) {
                m_oContactView.DialogContactId = 0;
                m_CampaignExtraDetail.SetDialogEditorSaveMode(SelectionProperty.DialogSaveMode.Unspecified);
                m_CampaignExtraDetail.SetModulesAsReadOnly(false);
                m_CampaignExtraDetail.RefreshBvStandardQuestion();
                this.ToggleButtonHeader(true);
                btnPreviousRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsFirstRow;
                btnNextRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsLastRow;
                m_DialogChanged = false;
            }
        }
        private void DialogEditorAfterDelete(DialogEditorEvents.AfterDelete e)
        {
            if (!e.IsCancelled) {
                m_DialogChanged = true;
                if (m_oContactView.ContactList != null && m_oContactView.ContactList.Count > 0)
                    m_oContactView.SaveSubCampaignContactAppointmentStatus(
                        string.Empty,
                        m_BrightSalesProperty.CommonProperty.FinalListId,
                        m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                        m_BrightSalesProperty.CommonProperty.SubCampaignId
                    );
            }
            else
                m_DialogChanged = false;

            m_CampaignExtraDetail.SetDialogEditorSaveMode(SelectionProperty.DialogSaveMode.Unspecified);
            m_CampaignExtraDetail.SetModulesAsReadOnly(false);
            this.ToggleButtonHeader(true);
        }
        private void DialogEditorOnDelete(DialogEditorEvents.OnDelete e)
        {
            m_DialogChanged = false;
            m_CampaignExtraDetail.SetDialogEditorSaveMode(SelectionProperty.DialogSaveMode.Unspecified);
            m_CampaignExtraDetail.SetModulesAsReadOnly(false);
            this.ToggleButtonHeader(true);

            btnPreviousRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsFirstRow;
            btnNextRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsLastRow;
        }
        private void DialogAnswerChanged(DialogEditorEvents.OnAnswerChange e)
        {
            if (IsLoadingCampaignBooking)
                return;

            m_DialogChanged = true;
            //m_DialogChanged = false;
        }
        private void CompanyRemarskSaved(EventFollowUpLogEvents.CompanyRemarksSaved.ManageCampaignBooking e)
        {
            if (m_CampaignExtraDetail != null) {
                if (m_CampaignExtraDetail.CompanyInformation_AccountId == m_BrightSalesProperty.CommonProperty.AccountId &&
                    m_CampaignExtraDetail.CompanyInformation_FinalListId == m_BrightSalesProperty.CommonProperty.FinalListId)
                    m_CampaignExtraDetail.SetCompanyRemarks(e.CompanyRemarks);
            }
        }
        private void LinkedInChanged(LinkedInEvents.OnSave.ManageCampaignBooking e)
        {
            if (m_oContactView != null)
            {
                m_oContactView.UpdateContactLinkedInURL(e.OnSaveArgs.LinkedInUrl);
            }
        }

        private void ContactChanged(ContactViewEvents.ContactChanged e)
        {
            if (m_oDialogEditor != null && !m_LoadingFromButtonPress) {
                m_oDialogEditor.SuspendEventDialogStatusChange(true);
                this.DisplayDialogEditor();
                m_oDialogEditor.SuspendEventDialogStatusChange(false);
            }

            if (m_CampaignExtraDetail != null) {
                m_CampaignExtraDetail.ShowSpecificContactMapLocation = true;
                m_CampaignExtraDetail.LoadSelectedPage();
            }
            
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse) {
                m_oDialogEditor.EnableEditDialogButton = false;
                m_CampaignExtraDetail.SetState();
            }

            if (ContactView_FocusedRowChanged != null)
                ContactView_FocusedRowChanged(this, new ContactViewFocusedRowChangedArgs() {
                    ContactPerson = m_BrightSalesProperty.CommonProperty.ContactPerson
                });
        }
        private void SaveBeforeSendEmail(FrmReleaseEvents.SaveCampaignBooking e)
        {
            this.SaveCampaignBookingData(true);
        }

        private enum ToggleOption {
            Work,
            Browse
        }
        private void ToggleWorkBrowseButton(ToggleOption pOptionToImplement)
        {
            if (pOptionToImplement == ToggleOption.Browse) {
                btnWorkBrowse.Text = "Browse";
                btnWorkBrowse.Image = global::SalesConsultant.Properties.Resources.browse;
            }
            else if (pOptionToImplement == ToggleOption.Work) {
                btnWorkBrowse.Text = "Work";
                btnWorkBrowse.Image = global::SalesConsultant.Properties.Resources.work;
            }
        }
        private void HideModules()
        {
            pnlContactView.Hide();
            pnlDialogEditor.Hide();
            pnlEventFollowUpLog.Hide();
            pnlCampaignExtraDetail.Hide();
        }
        private void LogAnEvent(BrightVision.Common.Classes.EventLog.EventTypes eType, params string[] values)
        {
            int? _ContactId = null;
            if (m_BrightSalesProperty.CommonProperty.ContactPerson != null && m_BrightSalesProperty.CommonProperty.ContactPerson.id > 0)
                _ContactId = m_BrightSalesProperty.CommonProperty.ContactPerson.id;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efDbContext.event_log.AddObject(
                    new event_log() {
                        event_id = (int)eType,
                        user_id = UserSession.CurrentUser.UserId,
                        subcampaign_id = m_BrightSalesProperty.CommonProperty.SubCampaignId,
                        account_id = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                        contact_id = _ContactId,
                        local_datetime = DateTime.Now,
                        computer_name = UserSession.CurrentUser.ComputerName,
                        param1 = values.Length >= 1 ? values[0] : null,
                        param2 = values.Length >= 2 ? values[1] : null,
                        param3 = values.Length >= 3 ? values[2] : null,
                        param4 = values.Length >= 4 ? values[3] : null,
                        param5 = values.Length >= 5 ? values[4] : null,
                        param6 = values.Length >= 6 ? values[5] : null
                    }
                );
                _efDbContext.SaveChanges();
            }
        }
        private void SetDefaultSelectedDropdownItems()
        {
            if (m_BrightSalesProperty.CampaignBooking.Appointment == null)
                return;

            this.cboAccountStatus.SelectedIndexChanged -= new System.EventHandler(this.cboAccountStatus_SelectedIndexChanged);
            this.cboAccountLeadStatus.SelectedIndexChanged -= new System.EventHandler(this.cboAccountLeadStatus_SelectedIndexChanged);

            cboAccountLeadStatus.Text = "None";
            int _SelectedItem = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatuses.FindIndex(i => i.selected == true);

            if (!string.IsNullOrEmpty(m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus))
                cboAccountLeadStatus.Text = m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus;
            else if (_SelectedItem >= 0)
                cboAccountLeadStatus.SelectedIndex = _SelectedItem;
            //else if (m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatusSelectedIndex >= 0)
            //    cboAccountLeadStatus.SelectedIndex = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatusSelectedIndex;
            //else
            //    cboAccountLeadStatus.Text = "None";

            cboAccountStatus.Text = "None";
            _SelectedItem = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatuses.FindIndex(i => i.selected == true);
            if (!string.IsNullOrEmpty(m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus))
                cboAccountStatus.Text = m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus;
            //else if (m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatusSelectedIndex >= 0)
            else if (_SelectedItem >= 0)
                cboAccountStatus.SelectedIndex = _SelectedItem;
                //cboAccountStatus.SelectedIndex = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatusSelectedIndex;
            //else
            //    cboAccountStatus.Text = "None";

            this.cboAccountStatus.SelectedIndexChanged += new System.EventHandler(this.cboAccountStatus_SelectedIndexChanged);
            this.cboAccountLeadStatus.SelectedIndexChanged += new System.EventHandler(this.cboAccountLeadStatus_SelectedIndexChanged);
            cboAccountStatus.Tag = cboAccountStatus.SelectedIndex;
        }
        private void ShowModules()
        {
            pnlContactView.Show();
            pnlDialogEditor.Show();
            pnlEventFollowUpLog.Show();
            pnlCampaignExtraDetail.Show();
        }
        private void SetEachModuleState()
        {
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse) {
                cboAccountLeadStatus.Properties.ReadOnly = true;
                cboAccountStatus.Properties.ReadOnly = true;
            }
            else if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Work) {
                bool _State = !m_BrightSalesProperty.CommonProperty.CompanyLocked? true: false;
                m_oDialogEditor.SetEditableComponent(false);
                m_oDialogEditor.SetState(_State);
            }

            m_oContactView.SetState();
            m_oEventFollowupLog.SetState();
            m_CampaignExtraDetail.SetState();
            if (ModuleSetStateInitiated != null)
                ModuleSetStateInitiated(this, new ModuleSetStateArgs() { 
                    CampaignBookingMode = m_BrightSalesProperty.CampaignBooking.Mode 
                });
        }
        private void ToggleButtonHeader(bool State)
        {
            btnCancel.Enabled = State;
            cboAccountLeadStatus.Enabled = State;
            cboAccountStatus.Enabled = State;
            btnNextRecord.Enabled = State;
            btnPreviousRecord.Enabled = State;
            this.ToggleWorkBrowseButton(State ? ToggleOption.Browse : ToggleOption.Work);
        }
        private void WorkOnSelectedCompany()
        {
            m_BrightSalesProperty.CampaignBooking.Mode = SelectionProperty.CampaignBookingMode.Work;
            cboAccountLeadStatus.Properties.ReadOnly = false;
            cboAccountStatus.Properties.ReadOnly = false;
            btnCloseCampaignBooking.Enabled = true;
            this.ToggleButtonHeader(true);
            if (m_BrightSalesProperty.CampaignBooking.ParentView == SelectionProperty.ParentView.CampaignList) {
                btnPreviousRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsFirstRow;
                btnNextRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsLastRow;
            }
            else {
                btnPreviousRecord.Enabled = false;
                btnNextRecord.Enabled = false;
            }
            m_BrightSalesProperty.CommonProperty.UserOnWorkModeState = true;
            SetEachModuleState();
            m_CampaignExtraDetail.SetModulesAsReadOnly(false);
            m_CampaignExtraDetail.SetModuleSaving(true);
        }
        private void SaveCampaignBookingData(bool pCalledFromSendEmail = false)
        {
            if (!m_BrightSalesProperty.CommonProperty.CallLogSaved) {
                NotificationDialog.Error("Bright Sales", "Please kindly save your call log first.");
                return;
            }

            /**
             * bypass checking to force update campaign booking data when sending emails.
             * saving of default values only applies to save dialog button, not on auto save features.
             */
            if (!pCalledFromSendEmail)
                if (!m_DialogChanged && !m_StatusChanged)
                    return;

            #region Old Code (might be needed later)
            //if (!m_ShouldSave)
            //    return;

            //if (m_oDialogEditor.DialogStatus == "None") {
            //    NoDialogStatusSelected();
            //    return;
            //}

            //if (m_oDialogEditor.DialogSaveMode == SaveModeEnum.Add || m_oDialogEditor.DialogSaveMode == SaveModeEnum.Edit) {
            //    DialogResult _dlg = MessageBox.Show("Dialog is being edited. Save & Close anyway?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //    if (_dlg == DialogResult.No)
            //        return;
            //}
            //if (!m_oDialogEditor.HasDialogQuestionnaire)
            //    return;

            //if (!m_oDialogEditor.Save())
            //    return;

            //if (oAppointment.AccountStatus.AccountLeadStatuses.Count < 1 || oAppointment.ContactStatus.ContactStatuses.Count < 1)
            //    NotificationDialog.Warning("Bright Sales", "Please set status in dropdown defined. Please contact your project manager.");

            //oAppointment.CompanyAppointmentStatus = cboAccountStatus.Text;
            //oAppointment.CompanyAppointmentLeadStatus = cboAccountLeadStatus.Text;
            #endregion

            if (m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatuses.Count < 1 || 
                m_BrightSalesProperty.CampaignBooking.ContactStatus.ContactStatuses.Count < 1)
                NotificationDialog.Warning("Bright Sales", "Please set status in dropdown defined. Please contact your project manager.");

            m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentStatus = cboAccountStatus.Text;
            m_BrightSalesProperty.CampaignBooking.Appointment.CompanyAppointmentLeadStatus = cboAccountLeadStatus.Text;

            /**
             * [@jeff 07.10.2012]: https://brightvision.jira.com/browse/PLATFORM-1461
             * event logging for event ids 12 & 13
             */
            #region Code Logic
            if (m_LastSavedCompanyStatus != cboAccountStatus.Text) {
                string _PriorState = m_LastSavedCompanyStatus;
                string _NewState = cboAccountStatus.Text;
                m_LastSavedCompanyStatus = cboAccountStatus.Text;
                this.LogAnEvent(
                    BrightVision.Common.Classes.EventLog.EventTypes.CHANGE_COMPANY_STATUS_SAVE_EVENT,
                    new string[] {
                        null,
                        _PriorState,
                        _NewState,
                    }
                );
            }

            if (m_LastSavedCompanyLeadStatus != cboAccountLeadStatus.Text) {
                string _PriorState = m_LastSavedCompanyLeadStatus;
                string _NewState = cboAccountLeadStatus.Text;
                m_LastSavedCompanyLeadStatus = cboAccountLeadStatus.Text;
                this.LogAnEvent(
                    BrightVision.Common.Classes.EventLog.EventTypes.CHANGE_LEAD_STATUS_SAVE_EVENT,
                    new string[] {
                        null,
                        _PriorState,
                        _NewState,
                    }
                );
            }
            #endregion
            
            m_EventBus.Notify(new ManageCampaignBookingEvents.OnSave.FrmSalesConsultant() { CalledFromSendEmail = pCalledFromSendEmail });
            m_EventBus.Notify(new ManageCampaignBookingEvents.OnSave.ManageCampaignList() { IsReleasedCancel = m_IsReleaseCancel });

            /**
             * special case: save only if necessary since its already automatically saved
             * when being edited/written.
             */
            string _CompanyRemarks = string.Empty;
            if (m_oEventFollowupLog != null) {
                _CompanyRemarks = m_oEventFollowupLog.CompanyRemarks;
                ObjectCompany.SaveCompanyRemarks(m_BrightSalesProperty.CommonProperty.SubCampaignId, m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId, _CompanyRemarks);
                if (m_CampaignExtraDetail != null)
                    m_CampaignExtraDetail.SetCompanyRemarks(_CompanyRemarks);
            }

            this.SetLastUpdatedInfo();
            if (!m_oDialogEditor.HasDialogQuestionnaire || !m_DialogChanged)
                return;
            
            /**
             * flag to know if any of the dialog questionnaire answers
             * have been changed or has been edited.
             * else, just bypass saving to save time.
             * 
             * [@jeff 05.16.2013]
             * commented for reason that default marked values be saved.
             */
            //if (!m_oDialogEditor.HasMustSaveDefaultValues)
            //    return;

            if (!m_oDialogEditor.Save())
                return;
        }
        private void PrepareStatusCombos()
        {
            this.cboAccountStatus.SelectedIndexChanged -= new System.EventHandler(this.cboAccountStatus_SelectedIndexChanged);
            this.cboAccountLeadStatus.SelectedIndexChanged -= new System.EventHandler(this.cboAccountLeadStatus_SelectedIndexChanged);
            cboAccountLeadStatus.Properties.Items.Clear();
            cboAccountStatus.Properties.Items.Clear();

            //foreach (string _item in m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatuses)
            //    cboAccountLeadStatus.Properties.Items.Add(_item);
            foreach (XmlUtility.SubCampaignConfig _item in m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatuses)
                cboAccountLeadStatus.Properties.Items.Add(_item.status);

            //foreach (string _item in m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatuses)
                //cboAccountStatus.Properties.Items.Add(_item);
            foreach (XmlUtility.SubCampaignConfig _item in m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatuses)
                cboAccountStatus.Properties.Items.Add(_item.status);

            //cboAccountLeadStatus.SelectedIndex = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatusSelectedIndex;
            //cboAccountStatus.SelectedIndex = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatusSelectedIndex;
            cboAccountLeadStatus.SelectedIndex = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountLeadStatuses.FindIndex(i => i.selected == true);
            cboAccountStatus.SelectedIndex = m_BrightSalesProperty.CampaignBooking.AccountStatus.AccountStatuses.FindIndex(i => i.selected == true);
            this.cboAccountStatus.SelectedIndexChanged += new System.EventHandler(this.cboAccountStatus_SelectedIndexChanged);
            this.cboAccountLeadStatus.SelectedIndexChanged += new System.EventHandler(this.cboAccountLeadStatus_SelectedIndexChanged);
        }
        private void DisplayDialogEditor(bool pDisposeQuestionnaire = false) //marker...
        //private void DisplayDialogEditor()
        {
            /**
             * lets set if the campaign booking has a dialog qestionnaire.
             * and set a flag.
             */
            dialog _efeDialog = null;
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efeDialog = _efDbModel.dialogs.FirstOrDefault(
                    i => i.subcampaign_id == m_BrightSalesProperty.CommonProperty.SubCampaignId &&
                    i.is_active == true &&
                    i.dialog_text_json.Length > 0 &&
                    i.dialog_text_json != "[]");

                if (_efeDialog != null)
                    _efDbModel.Detach(_efeDialog);
            }

            if (_efeDialog != null && m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Work) {
                m_oDialogEditor.HasDialogQuestionnaire = true;
                if (_efeDialog.dialog_text_json.Contains("\"account_level\":true"))
                    m_oDialogEditor.HasDialogAccountLevelQuestion = true;

                var log = BrightSalesFacade.Logger;
                log.SetLogField(LoggingField.dialog_id, _efeDialog.id.ToString());
            }
            else {
                m_oDialogEditor.HasDialogQuestionnaire = false;
                m_oDialogEditor.HasDialogAccountLevelQuestion = false;
            }

            m_oDialogEditor.SelectedContact = m_BrightSalesProperty.CommonProperty.ContactPerson;
            m_oDialogEditor.BreadCrumbText = lblBreadCrumb.Text;
            m_oDialogEditor.AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId;
            m_oDialogEditor.CampaignId = m_BrightSalesProperty.CommonProperty.CampaignId;
            m_oDialogEditor.SubCampaignId = m_BrightSalesProperty.CommonProperty.SubCampaignId;
            m_oDialogEditor.FinalListId = m_BrightSalesProperty.CommonProperty.FinalListId;
            m_oDialogEditor.SubCampaignContactList = m_oContactView.ContactList;
            m_oDialogEditor.ContactStatus = m_BrightSalesProperty.CampaignBooking.ContactStatus;
            //m_oDialogEditor.LoadDialogQuestionnaires();
            m_oDialogEditor.LoadDialogQuestionnaires(pDisposeQuestionnaire); //marker...
            //m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;

            m_oDialogEditor.EnableEditDialogButton = false;
            if (m_oContactView.ContactList != null && m_oContactView.ContactList.Count > 0 && m_oDialogEditor.HasDialogQuestionnaire)
                m_oDialogEditor.EnableEditDialogButton = true;

            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Work && m_oDialogEditor.HasDialogQuestionnaire) {
                if (m_oContactView != null) {
                    if (m_oContactView.gvContact.RowCount > 0 || (m_oContactView.gvContact.RowCount < 1 && m_oDialogEditor.HasDialogAccountLevelQuestion)) {
                        m_oDialogEditor.EnableEditDialogButton = true;
                        m_oDialogEditor.EnableDialogManagerDeleteButton = true;
                    }
                }
                else if (m_oContactView == null && (m_oContactView.gvContact.RowCount < 1 && m_oDialogEditor.HasDialogAccountLevelQuestion)) {
                    m_oDialogEditor.EnableEditDialogButton = true;
                    m_oDialogEditor.EnableDialogManagerDeleteButton = true;
                }
            }
        }
        #endregion

        #region Public Methods
        public void SaveCampaignBooking()
        {
            this.SaveCampaignBookingData();
        }
        public void UpdateCompanyInformation()
        {
            this.SetCompanyDirectCallPolicyImage();
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.SetCompanyDetails(m_BrightSalesProperty.CommonProperty.AccountId);
        }
        public void SaveAndCloseFollowUp()
        {
            pnlContactView.Enabled = false;
            pnlDialogEditor.Enabled = false;
            this.SaveCampaignBookingData();

            pnlContactView.Enabled = true;
            pnlDialogEditor.Enabled = true;
        }
        public void SetCompanyDirectCallPolicyImage()
        {
            account _item = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _item = _efDbContext.accounts.FirstOrDefault(i => i.id == m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId);
                if (_item != null)
                    _efDbContext.Detach(_item);
            }

            if (_item == null) {
                lciPolicyDirectCall.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                return;
            }

            lciPolicyDirectCall.Visibility = _item.policy_direct_call ? DevExpress.XtraLayout.Utils.LayoutVisibility.Always : DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
        }
        public void SaveCampaignBookingOnWorkAnotherCompany() 
        {
            pnlContactView.Enabled = false;
            pnlDialogEditor.Enabled = false;
            this.SaveCampaignBookingData();
            this.SetBreadCrumb(m_BrightSalesProperty.CampaignBooking.BreadCrumb);
            this.ResetContactParameters();
            btnPreviousRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsFirstRow;
            btnNextRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsLastRow;
            pnlContactView.Enabled = true;
            pnlDialogEditor.Enabled = true;
        }
        public void EnableCampaignBooking()
        {
            if (!this.Enabled)
                this.Enabled = true;
        }
        public void DisableCampaignBooking()
        {
            this.Enabled = false;
            lblBreadCrumb.Text = string.Empty;
        }
        public void InitializeCampaignBooking()
        {
            m_LoadingFromButtonPress = true;
            m_IsReleaseCancel = false;
            #region Original Code
            //public void LoadCampaignBookingData(bool InitializeOnly = false)
            //this.InitializeMySalesScriptView();
            //this.InitializeContactInformationForm();
            //this.InitializeCompanyInformationForm();
            //this.InitializeStandardQuestionView();
            //this.InitializeCompanyWebsiteView();
            //this.InitializeGoogleSearchView();
            //this.InitializeCollectedDataView();
            //this.InitializeGeoMapContactView();
            #endregion

            #region Contact View
            m_ContactViewOnInitialize = true;
            if (m_oContactView == null) {
                m_oContactView = new ContactView();
                m_oContactView.Dock = DockStyle.Fill;
                m_oContactView.OnContactSaved += new ContactView.ContactSavedEventHandler(m_oContactView_OnContactSaved);
                m_oContactView.GridControlMouseDown += new MouseEventHandler(m_oContactView_GridControlMouseDown);
                m_oContactView.GridControlMouseUp += new MouseEventHandler(m_oContactView_GridControlMouseUp);
                m_oContactView.GridControlKeyUp += new KeyEventHandler(m_oContactView_GridControlKeyUp);
                m_oContactView.GridControlKeyDown += new KeyEventHandler(m_oContactView_GridControlKeyDown);
                m_oContactView.ActiveChanged += new ContactView.ActiveChangedEventHander(m_oContactView_ActiveChanged);
                m_oContactView.PopulateContactView_Initiated += new ContactView.PopulateContactViewInitiatedEventHandler(m_oContactView_PopulateContactView_Initiated);
                pnlContactView.Controls.Clear();
                pnlContactView.Controls.Add(m_oContactView);
            }
            m_ContactViewOnInitialize = false;
            #endregion

            #region Dialog Editor
            if (m_oDialogEditor == null) {
                m_oDialogEditor = new DialogEditor() {
                    Dock = DockStyle.Fill
                };
                pnlDialogEditor.Controls.Clear();
                pnlDialogEditor.Controls.Add(m_oDialogEditor);
                m_oContactView.DialogEditorModule = m_oDialogEditor;
            }
            #endregion

            #region Event Followup Log
            if (m_oEventFollowupLog == null) {
                m_oEventFollowupLog = new EventFollowUpLog() {
                    AllowSaving = true,
                    Dock = DockStyle.Fill
                };
                pnlEventFollowUpLog.Controls.Clear();
                pnlEventFollowUpLog.Controls.Add(m_oEventFollowupLog);
            }
            #endregion

            #region Campaign Extra Detail
            if (m_CampaignExtraDetail == null) {
                m_CampaignExtraDetail = new CampaignExtraDetail() {
                    CallingEnvironment = SelectionProperty.CallingEnvironment.CampaignBooking
                };

                m_CampaignExtraDetail.Dock = DockStyle.Fill;
                pnlCampaignExtraDetail.Controls.Clear();
                pnlCampaignExtraDetail.Controls.Add(m_CampaignExtraDetail);
            }
            #endregion
        }
        public void LoadCampaignBookingData(bool pOnCampaignBookingBrowseMode = false) 
        {
            this.HideModules();
            this.PrepareStatusCombos();
            lblBreadCrumb.AppearanceItemCaption.BackColor = System.Drawing.Color.FromName("0");

            /**
             * we will need to load first the contact view.
             * since all modules are related to this view.
             */
            #region Contact View
            m_oDialogEditor.ResetToDefaultState(); // clear the dialog first
            m_ContactViewOnInitialize = true;
            m_oContactView.ContactId = SelectedContactId;
            m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId = SelectedContactId;
            
            /**
             * [@jeff 05.30.2012]: https://brightvision.jira.com/browse/PLATFORM-1422
             * cleaned-up left-over contact id handler.
             */
            m_oContactView.DialogContactId = 0;
            m_oContactView.CallLogViewContactId = null;
            m_oContactView.ExtraDetailContactId = 0;
            
            bool _HasSelectedContact = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId > 0 ? true : false;
            m_oContactView.LoadContactData(_HasSelectedContact);
            m_ContactViewOnInitialize = false;

            CTScSubCampaignContactList _Contact = null;
            if (m_BrightSalesProperty.CommonProperty.ContactPerson != null)
                _Contact = m_BrightSalesProperty.CommonProperty.ContactPerson;
            #endregion

            #region Load Campaign Booking Data Event
            m_EventBus.Notify(new ManageCampaignBookingEvents.OnFormLoad() {
                ContactPerson = _Contact
            });
            #endregion

            #region Event Followup Log
            m_oEventFollowupLog.SubCampaignId = m_BrightSalesProperty.CommonProperty.SubCampaignId;
            m_oEventFollowupLog.AccountId = m_BrightSalesProperty.CommonProperty.AccountId;
            m_oEventFollowupLog.FinalListId = m_BrightSalesProperty.CommonProperty.FinalListId;
            m_oEventFollowupLog.InitializeView();
            #endregion

            #region Campaign Extra Detail
            m_CampaignExtraDetail.LoadSelectedPage();
            #endregion

            #region Dialog Editor
            this.DisplayDialogEditor();
            #endregion

            #region Get Current Company & Lead Statuses
            this.SetDefaultSelectedDropdownItems();
            m_LastSavedCompanyStatus = cboAccountStatus.Text;
            m_LastSavedCompanyLeadStatus = cboAccountLeadStatus.Text;
            #endregion

            #region Direct Call Policy Image
            this.SetCompanyDirectCallPolicyImage();
            #endregion

            if (!pOnCampaignBookingBrowseMode)
                this.WorkOnSelectedCompany();
            else
                this.SetEachModuleState();

            this.ShowModules();
            m_StatusChanged = false;
            m_DialogChanged = false;
            m_LoadingFromButtonPress = false;
        }
        public void SetBreadCrumb(string pText) 
        {
            lblBreadCrumb.Text = pText;
        }
        public void SetDefaultTab()
        {
            m_CampaignExtraDetail.SetDefaultTab();
        }
        public void ResetContactParameters()
        {
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.ResetContactParams();
        }
        public void SetLastUpdatedInfo()
        {
            if (SetCompanyModificationInfo != null)
                SetCompanyModificationInfo();
        }
        public void OnCampaignListEmpty()
        {
            m_CampaignExtraDetail.SetModuleSaving(false);
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse) {
                btnPreviousRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsFirstRow;
                btnNextRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsLastRow;
            }
        }
        public void OnCampaignListHasRows()
        {
            m_CampaignExtraDetail.SetModuleSaving(true);
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse) {
                btnPreviousRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsFirstRow;
                btnNextRecord.Enabled = !m_BrightSalesProperty.CampaignList.IsLastRow;
            }
        }
        public void SetCampaignExtraDetailSelectedPage(CampaignExtraDetail.eSelectedPage pSelectedPage)
        {
            if (m_CampaignExtraDetail != null && (oAppointment != null && oAppointment.AccountId > 0))
                m_CampaignExtraDetail.LoadSelectedPage(pSelectedPage);
        }
        public void EditCampaignDialog()
        {
            if (m_BrightSalesProperty.CampaignBooking.Mode != SelectionProperty.CampaignBookingMode.Work)
                return;

            if (m_oDialogEditor != null)
                if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Unspecified)
                    m_oDialogEditor.EditDialog();
        }
        public void LoadNextCompany()
        {
            if (!m_BrightSalesProperty.CampaignList.IsLastRow && btnNextRecord.Enabled)
                btnNextRecord.PerformClick();
        }
        public void LoadPreviousCompany()
        {
            if (!m_BrightSalesProperty.CampaignList.IsFirstRow && btnPreviousRecord.Enabled)
                btnPreviousRecord.PerformClick();
        }
        public void DeleteCallLog(int pCallLogId)
        {
            if (m_oEventFollowupLog != null)
                m_oEventFollowupLog.Delete(pCallLogId);
        }
        public void ContactSetFocus(int pContactId)
        {
            if (m_oContactView != null)
                m_oContactView.ContactSetFocus(pContactId);
        }

        public void m_oCallViewBar_CallAttemptMade()
        {
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.UpdateStatisticsData();

            if (m_oContactView != null)
                m_oContactView.UpdateContactCallAttempts();
        }
        public void m_oCallLogBar_btnHangUp_OnClick(int? pContactId)
        {
            if (m_oContactView == null)
                return;

            m_oContactView.CallLogViewContactId = pContactId;
            m_oContactView.SetDefaultSelectedItem();
        }
        public void m_oCallLogBar_btnSaveCallLog_OnClick(int eventId)
        {
            if (m_oEventFollowupLog != null)
                m_oEventFollowupLog.PopulateEventFollowUpLogView(eventId);
        }
        public CTScSubCampaignContactList m_oCallLogBar_btnUseContact_OnClick()
        {
            return m_BrightSalesProperty.CommonProperty.ContactPerson;
        }
        public void m_oFollowUp_btnSave_OnClick()
        {
            m_oEventFollowupLog.PopulateEventFollowUpLogView();
        }
        public List<CTScSubCampaignContactList> GetCampaignBookingContactList()
        {
            if (m_oContactView == null)
                return new List<CTScSubCampaignContactList>();

            return m_oContactView.GetContactList();
        }
        public bool DialogEditor_OnEditMode()
        {
            if (m_oDialogEditor != null)
                return m_oDialogEditor.OnEditMode;

            return false;
        }
        public void m_oEventFollowupLog_UnHideAll()
        {
            if (m_oEventFollowupLog != null)
                m_oEventFollowupLog.UnHideAll();
        }
        public void UpdateContactViewLastUser()
        {
            if (m_oContactView != null) {
                try {
                    m_oContactView.SaveSubCampaignContactAppointmentStatus(
                        m_oDialogEditor.DialogStatus,
                        m_BrightSalesProperty.CommonProperty.FinalListId,
                        m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                        m_BrightSalesProperty.CommonProperty.SubCampaignId
                    );
                    m_oContactView.UpdateContactLastUser();
                }
                catch (Exception ex) {
                    m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
                        ErrorMessage = ex.Message
                    });
                }
            }
        }
        #endregion

        #region Subscribed Events
        private void m_oContactView_ActiveChanged(bool IsActive)
        {
            if (m_CampaignExtraDetail != null)
                m_CampaignExtraDetail.ContactActiveChanged(IsActive);
        }
        private void m_oContactView_GridControlMouseUp(object sender, MouseEventArgs e) 
        {
            if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit)
                (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;
        }
        private void m_oContactView_GridControlMouseDown(object sender, MouseEventArgs e) 
        {
            if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit)
                (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;
        }
        private void m_oContactView_GridControlKeyUp(object sender, KeyEventArgs e) 
        {
            if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit)
                e.Handled = true;
        }
        private void m_oContactView_GridControlKeyDown(object sender, KeyEventArgs e) 
        {
            if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit)
                e.Handled = true;
        }
        private void m_oContactView_OnContactSaved(object sender, ContactView.ContactEventArgs e)
        {
            m_CampaignExtraDetail.ReloadContacInformationtData = true;
            m_CampaignExtraDetail.LoadSelectedPage();
            m_oDialogEditor.SelectedContact = m_BrightSalesProperty.CommonProperty.ContactPerson;
            m_oDialogEditor.SubCampaignContactList = m_oContactView.ContactList;
            m_oDialogEditor.RefreshContactAttendies();
            //m_oDialogEditor.BindDialogManagerData();
            m_oDialogEditor.BindDialogManagerData(true); // marker...

            if (ContactView_OnContactSaved != null)
                ContactView_OnContactSaved(this, new ContactViewOnContactSavedArgs() {
                    ContactPerson = m_BrightSalesProperty.CommonProperty.ContactPerson
                });
        }
        private void m_oContactView_PopulateContactView_Initiated(object sender, ContactView.PopulateContactViewArgs e)
        {
            if (oContactView_PopulateContactView_Initiated != null)
                oContactView_PopulateContactView_Initiated(this, new ContactViewPopulateContactView() {
                    ContactPersons = e.ContactPersons,
                    SelectedContactPersonIndex = e.SelectedContactPersonIndex
                });
        }

        private void m_oEventFollowupLog_WorkNurtureEvent(CampaignBookingProperty.CampaignBoookingArguments e)
        {
            if (EventFollowupLog_WorkNurtureEvent != null)
                EventFollowupLog_WorkNurtureEvent(e);
        }
        #endregion
    }
}