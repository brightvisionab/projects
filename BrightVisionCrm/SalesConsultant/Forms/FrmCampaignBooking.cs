
#region Namespace
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
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BrightVision.Common.Modules;
using BrightVision.Common.Business;
using SalesConsultant.Modules;
using SalesConsultant.Business;
#endregion

namespace SalesConsultant.Forms {
    public partial class FrmCampaignBooking : DevExpress.XtraEditors.XtraForm
    {
        #region Public Properties
        public eParentView ParentView { get; set; }
        public ManageCompanyContact CompanyContactView = null;
        public MyFollowUps MyFollowUpView = null;
        public SubCampaignAppointment oAppointment { get; set; }
        #endregion

        #region Enumeration
        public enum eParentView
        {
            CompanyContactView,
            MyFollowUpView
        }

        private enum eCheckEditType
        {
            Done,
            UserTaken
        }

        enum SaveMode {
            Unspecified = 0,
            Edit = 1,
            Add = 2,
            Delete = 3

        }
        #endregion

        #region Member Variables
        private BrightPlatformEntities BPContext = null;
        private dialog m_oDialog = null;
        private ContactView m_oContactView = null;
        private AddContact m_objContactForm = null;
        private bool m_CancelledAddContact = true;
        private string m_SelectedPageName = string.Empty;
        private string m_MessageCaption = "Sales Consultant Campaign Boooking";
        private ContactInformation m_objContactInformationForm = null;
        private CompanyInformation m_objCompanyInformationForm = null;
        private BrightPlatformEntities m_DatabaseModel = null;
        private StandardQuestionView m_objStandardQuestionView = null;
        private CompanyWebsiteView m_objCompanyWebsiteView = null;
        private GoogleSearchView m_objGoogleSearchView = null;
        private CollectedDataView m_objCollectedDataView = null;
        private GoogleMapUtility m_objMapUtility = new GoogleMapUtility();
        private GeoLocationViewer m_objGeoLocationViewerContact = new GeoLocationViewer();
        private List<GeoLocationViewer.GeoMapLocation> m_GeoMapListContact = new List<GeoLocationViewer.GeoMapLocation>();
        private GridView m_objGridViewContact = null;
        private CTScSubCampaignContactList m_objContact = null;
        private bool m_DoneLoadingContactMap = false;
        private SalesScriptView m_objMySalesScriptForm = null;
        private FrmSchedulingPopup frmSchedulingPopup = null;
        private bool m_DoneLoadingMySalesScript = false;
        private ManageEventFollowUp m_objFollowUpForm = null;
        private List<CTScEventAndFollowUpLog> m_EventFollowUpList = new List<CTScEventAndFollowUpLog>();
        private CTScEventAndFollowUpLog m_EventFollowUp = null;
        private bool m_ContactRemarkChanged = false;
        private DateTime dtStart = DateTime.MinValue;
        private DateTime dtEnd = DateTime.MinValue;
        private SaveMode oMode = SaveMode.Unspecified;
        private eCheckEditType m_CheckEditType { get; set; }
        private bool m_IsChecked = false;        
        private Schedule objScheduleComponent = null;
        #endregion

        #region Constructor
        public FrmCampaignBooking() 
        {
            InitializeComponent();
            FormUtility.DisableCloseButton(this.Handle);
            this.SetRecordNavigationButtons(false);
            //oAppointment = appointment;
        }        
        #endregion

        #region Properties
        public string SelectedCompany { get; set; }
        public string CompanyWebsite { get; set; }
        #endregion

        #region Controllers
        private void m_oContactView_GridControlMouseUp(object sender, MouseEventArgs e) {
            if (oMode == SaveMode.Edit)
                (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;
        }

        private void m_oContactView_GridControlMouseDown(object sender, MouseEventArgs e) {
            if (oMode == SaveMode.Edit)
                (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;
        }

        private void btnFollowupAdd_Click(object sender, EventArgs e)
        {
            if (m_oContactView.SelectedContact == null || m_oContactView.SelectedContact.id < 1)
            {
                MessageBox.Show("No selected contact for follow up.", "Event Follow Up", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            this.SaveEventFollowUp();
            this.SetDefaultEventLogInfos();
        }

        private void cmdSaveEventChanges_Click(object sender, EventArgs e)
        {
            this.SaveUpdatedEventFollowUps();
        }

        private void cbxUserTaken_EditValueChanging(object sender, ChangingEventArgs e)
        {
            this.SetFocusedViewInstance();
            if (m_EventFollowUp.event_type.Equals("Call Log"))
                e.Cancel = true;
        }

        private void cbxDone_EditValueChanging(object sender, ChangingEventArgs e)
        {
            this.SetFocusedViewInstance();
            if (m_EventFollowUp.event_type.Equals("Call Log"))
                e.Cancel = true;
        }

        private void cbxDone_CheckedChanged(object sender, EventArgs e)
        {
            this.SetFocusedViewInstance();
            if (m_EventFollowUp.event_type.Equals("Call Log"))
                return;
            
            m_CheckEditType = eCheckEditType.Done;
            m_IsChecked = (sender as CheckEdit).Checked;
            this.QueueUpdatedEventFollowUp();
        }

        private void cbxUserTaken_CheckedChanged(object sender, EventArgs e)
        {
            this.SetFocusedViewInstance();
            if (m_EventFollowUp.event_type.Equals("Call Log"))
                return;

            m_CheckEditType = eCheckEditType.UserTaken;
            m_IsChecked = (sender as CheckEdit).Checked;
            this.QueueUpdatedEventFollowUp();
        }

        private void gvEventLog_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            this.SetFocusedViewInstance();
            if (m_EventFollowUp.event_type.Equals("Call Log"))
                return;

            this.QueueUpdatedEventFollowUp();
        }

        private void gvEventLog_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.SetFocusedViewInstance();
        }

        private void oSchedule_ShowCalendarBookingClick(object sender, EventArgs e)
        {
            var obj = sender as Schedule;
            if (obj != null) {
                objScheduleComponent = obj;
                //WaitDialog.CreateWaitDialog("Loading Schedule Dialog...");
                frmSchedulingPopup = new FrmSchedulingPopup();
                frmSchedulingPopup.SetBreadCrumb(simpleLabelItemBreadCrumb.Text);
                frmSchedulingPopup.CurrentScheduleID = obj.CurrentSelectedMeeting;
                if(m_oContactView.SelectedContact != null)
                    frmSchedulingPopup.CurrentContactID = m_oContactView.SelectedContact.id;
                frmSchedulingPopup.CurrentAccountID = oAppointment.AccountId;
                //frmSchedulingPopup.AccountID = oAppointment.AccountId;
                //frmSchedulingPopup.ContactID = m_oContactView.SelectedContact.id;
                frmSchedulingPopup.SubCampaignID = oAppointment.SubCampaignId;
                if (frmSchedulingPopup.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    if (frmSchedulingPopup.CreatedScheduleID > 0) {
                        obj.SetCreatedMeetingSchedule(frmSchedulingPopup.CreatedScheduleID);
                        obj.Questionnaire.Form.Settings.DataBindings.schedule_id = frmSchedulingPopup.CreatedScheduleID.ToString();
                    }
                }
            }
        }        

        private void FrmCampaignBooking_Load(object sender, EventArgs e)
        {
            this.OnLoadInitialization();

            #region Commented
            //#region Toggle Buttons Enabled for Contacts grid
            //m_oDialog = BPContext.dialogs.FirstOrDefault(p =>
            //        p.subcampaign_id == oAppointment.SubCampaignId &&
            //            //p.customers_id == oAppointment.CustomerId &&
            //        p.is_active == true);
            //if (m_oDialog == null) {
            //    //btnDeleteDialog.Enabled = false;
            //    btnEditDialog.Enabled = false;
            //} else {
            //    //btnDeleteDialog.Enabled = true;
            //    btnEditDialog.Enabled = true;
            //}
            //#endregion

            //#region Load Company Information in Group Tab
            //this.LoadCompanyInformation();
            //#endregion

            //#region Load Contact Information in Group Tab
            //this.LoadContactInformation();
            //#endregion

            //this.LoadCustomEvents();
            ////comboBoxEditCompanyStatus.Text = oAppointment.CompanyAppointmentStatus;
            //m_SelectedPageName = lcgContactInformation.Name;
            //this.SetContactViewButtons(false);
            ////btnDeleteDialog.Enabled = false;
            //btnEditDialog.Enabled = false;
            //btnCallDirect.Enabled = false;
            //btnCallMobile.Enabled = false;
            //btnHangUp.Enabled = false;
            //btnCallNumber.Enabled = true;

            //if (m_oContactView.SelectedContact != null)
            //{
            //    this.InitializeFollowUpView();
            //    this.InitEventAndFollowUpObjects();
            //    ViewDialog();
            //    //btnDeleteDialog.Enabled = true;
            //    btnEditDialog.Enabled = true;

            //    if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.direct_phone))
            //        btnCallDirect.Enabled = true;

            //    if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.mobile))
            //        btnCallMobile.Enabled = true;

            //    this.SetContactViewButtons(true);
            //}

            //if (oAppointment.CompanyAppointmentStatus.Equals("Open") || string.IsNullOrEmpty(oAppointment.CompanyAppointmentStatus))
            //{
            //    cmdSaveExitAsOpen.ButtonStyle = BorderStyles.Office2003;
            //    cmdSaveExitAsOpen.Refresh();
            //}
            //else if (oAppointment.CompanyAppointmentStatus.Equals("In Progress"))
            //{
            //    cmdSaveExitAsInProgress.ButtonStyle = BorderStyles.Office2003;
            //    cmdSaveExitAsInProgress.Refresh();
            //}
            //else if (oAppointment.CompanyAppointmentStatus.Equals("Follow Up"))
            //{
            //    cmdSaveExitAsFollowUp.ButtonStyle = BorderStyles.Office2003;
            //    cmdSaveExitAsFollowUp.Refresh();
            //}
            //else if (oAppointment.CompanyAppointmentStatus.Equals("Finished"))
            //{
            //    cmdSaveExitAsFinished.ButtonStyle = BorderStyles.Office2003;
            //    cmdSaveExitAsFinished.Refresh();
            //}

            ////dateEditDateLog.DateTime = DateTime.Now;
            ////timeEditStartTime.Time = DateTime.Now;
            ////timeEditEndtime.Time = DateTime.Now;
            #endregion
        }

        private void btnExit_Click(object sender, EventArgs e) 
        {
            if (ParentView == eParentView.CompanyContactView)
                CompanyContactView.SaveCompanyAppointment("Finished");            

            if (ParentView == eParentView.MyFollowUpView)
                MyFollowUpView.SaveCompanyAppointment("Finished");

            oAppointment.CompanyAppointmentStatus = "Finished";
            this.SaveCompanyRemarks();
            //this.SetRecordNavigationButtons(true);
            this.SetSaveButtonAppearance();
            //this.EditCampaignBooking(false);
            //this.Close();
        }

        private void btnEditDialog_Click(object sender, EventArgs e) {
            //set readonly and enable controls
            //this.layoutControlQuestionnaire.OptionsView.IsReadOnly = DevExpress.Utils.DefaultBoolean.False;
            SetEditableComponent(true);            
            btnEditDialog.Enabled = false;
            lcgContact.Enabled = false;
            m_oContactView.gvContact.OptionsFind.AlwaysVisible = false;
            oMode = SaveMode.Edit;
            btnShowMissingFields.Enabled = true;
            btnSaveAsCompleted.Enabled = true;
            btnSaveAsDone.Enabled = true;
            btnSaveAsInProgress.Enabled = true;
            btnDeleteDialog.Enabled = true;
            //btnDontSaveAndClose.Enabled = true;

            //disable campaign booking saving
            cmdSaveExitAsOpen.Enabled = false;
            cmdSaveExitAsInProgress.Enabled = false;
            cmdSaveExitAsFinished.Enabled = false;
            cmdSaveExitAsFollowUp.Enabled = false;
            cmdSelectCompany.Enabled = false;
        }

        private void btnDontSaveAndClose_Click(object sender, EventArgs e) {
            oMode = SaveMode.Unspecified;
            CloseDialogEditor(false);
            ViewDialog();
            lcgContact.Enabled = true;

            //enable campaign booking saving
            cmdSaveExitAsOpen.Enabled = true;
            cmdSaveExitAsInProgress.Enabled = true;
            cmdSaveExitAsFinished.Enabled = true;
            cmdSaveExitAsFollowUp.Enabled = true;
        }

        private void btnSaveAsDone_Click(object sender, EventArgs e) {
            if (!SaveDialogAnswers(ContactView.eDialogStatus.Done)) return;
            //set readonly and disabled controls -> for viewing only
            //this.layoutControlQuestionnaire.OptionsView.IsReadOnly = DevExpress.Utils.DefaultBoolean.True;
            SetEditableComponent(false);            
            lcgContact.Enabled = true;

            //enable campaign booking saving
            cmdSaveExitAsOpen.Enabled = true;
            cmdSaveExitAsInProgress.Enabled = true;
            cmdSaveExitAsFinished.Enabled = true;
            cmdSaveExitAsFollowUp.Enabled = true;
            cmdSelectCompany.Enabled = true;
        }

        private void btnSaveAsCompleted_Click(object sender, EventArgs e) {
            //set readonly and disabled controls -> for viewing only            
            if (!SaveDialogAnswers(ContactView.eDialogStatus.Completed)) return;
            //this.layoutControlQuestionnaire.OptionsView.IsReadOnly = DevExpress.Utils.DefaultBoolean.True;
            SetEditableComponent(false);            
            lcgContact.Enabled = true;

            //enable campaign booking saving
            cmdSaveExitAsOpen.Enabled = true;
            cmdSaveExitAsInProgress.Enabled = true;
            cmdSaveExitAsFinished.Enabled = true;
            cmdSaveExitAsFollowUp.Enabled = true;
            cmdSelectCompany.Enabled = true;
        }

        private void btnSaveAsInProgress_Click(object sender, EventArgs e) {
            if (!SaveDialogAnswers(ContactView.eDialogStatus.InProgress)) return;
            //set readonly and disabled controls -> for viewing only
            //this.layoutControlQuestionnaire.OptionsView.IsReadOnly = DevExpress.Utils.DefaultBoolean.True;
            SetEditableComponent(false);            
            lcgContact.Enabled = true;

            //enable campaign booking saving
            cmdSaveExitAsOpen.Enabled = true;
            cmdSaveExitAsInProgress.Enabled = true;
            cmdSaveExitAsFinished.Enabled = true;
            cmdSaveExitAsFollowUp.Enabled = true;
            cmdSelectCompany.Enabled = true;
        }

        private void cmdSaveContactInformation_Click(object sender, EventArgs e)
        {
            DialogResult objResult = MessageBox.Show("Are you sure to update this contact's notes?", m_MessageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes)
            {
                WaitDialog objDialog = new WaitDialog();
                objDialog.CreateNonStaticWaitDialog("Saving contact notes.");
                m_objContactInformationForm.ContactRemarks = txtContactRemarks.Text;
                m_objContactInformationForm.SaveContactInformation();
                objDialog.CloseNonStaticWaitDialog();
            }
        }

        private void btnSaveCompanyDetails_Click(object sender, EventArgs e) 
        {
            DialogResult objResult = MessageBox.Show("Are you sure to update this company?", m_MessageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objResult == DialogResult.Yes) 
            {
                WaitDialog objDialog = new WaitDialog();
                objDialog.CreateNonStaticWaitDialog("Saving Company Information.");
                m_objCompanyInformationForm.CompanyRemarks = txtCompanySpecificRemarks.Text;
                m_objCompanyInformationForm.SaveCompanyInformation();
                this.LoadCompanyInformation();
                oAppointment.CompanyBoardNumber = m_objCompanyInformationForm.CompanyPhone;
                oAppointment.CompanyWebsite = m_objCompanyInformationForm.CompanyWebsite;
                if (ParentView == eParentView.CompanyContactView)
                {
                    CompanyContactView.InitializeCompanyInformationForm(m_DatabaseModel);
                    CompanyContactView.LoadCompanyInformation();
                }
                objDialog.CloseNonStaticWaitDialog();
            }
        }

        private void vGridControlCompanyInfo_RecordCellStyle(object sender, DevExpress.XtraVerticalGrid.Events.GetCustomRowCellStyleEventArgs e) {
            e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
        }

        private void vGridControlContact_RecordCellStyle(object sender, DevExpress.XtraVerticalGrid.Events.GetCustomRowCellStyleEventArgs e) {
            DevExpress.XtraVerticalGrid.VGridControl vgrid = sender as DevExpress.XtraVerticalGrid.VGridControl;
            if (e.Row.Properties.ReadOnly) {
                //Apply the appearance of the SelectedRow                                
                e.Appearance.Assign(vgrid.Appearance.ReadOnlyRow);
                //Just to illustrate how the code works. Remove the following lines to see the desired appearance.
                e.Appearance.Options.UseForeColor = true;
                e.Appearance.ForeColor = Color.Black;
            }
        }

        private void btnAddContact_Click(object sender, EventArgs e) {
            DisplayContactForm(true);
        }

        private void btnEditContact_Click(object sender, EventArgs e) {
            DisplayContactForm(false);
        }

        private void btnDeleteContact_Click(object sender, EventArgs e) 
        {
            string Message = "This is a global deletion from all campaigns." + Environment.NewLine + "Are you sure you want to delete this contact?";
            DialogResult objDialog = MessageBox.Show(Message, "Delete Contact", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objDialog == DialogResult.No)
                return;

            WaitDialog.CreateWaitDialog("Deleting Contact...", "Please wait...");
            ObjectContact.DeActivateContact(m_oContactView.SelectedContact.id);
            WaitDialog.CloseWaitDialog();
            m_oContactView.PopulateContactView(
                oAppointment.SubCampaignId,
                oAppointment.AccountId,
                oAppointment.FinalListId);
        }

        private void m_objContactForm_FormClosed(object sender, EventArgs e) 
        {
            if (!m_CancelledAddContact) 
            {
                #region Load Contact Information in Group Tab
                m_oContactView.ContactId = m_objContactForm.ContactId;
                m_oContactView.PopulateContactView
                (
                    oAppointment.SubCampaignId,
                    oAppointment.AccountId,
                    oAppointment.FinalListId
                );

                this.LoadContactInformation();
                #endregion

                MessageBox.Show("Successfully updated contacts.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                m_CancelledAddContact = true;
            }
        }

        private void m_objContactFormCancelButton_Click(object sender, EventArgs e) {
            m_CancelledAddContact = true;
        }

        private void m_objContactFormSaveButton_Click(object sender, EventArgs e) {
            m_CancelledAddContact = false;
        }

        private void tcCampaignBooking_SelectedPageChanged(object sender, LayoutTabPageChangedEventArgs e) {
            m_SelectedPageName = e.Page.Name;
            this.LoadSelectedPage(e.Page.Name);
        }

        private void cmdShowAllContact_Click(object sender, EventArgs e) {
            m_objGeoLocationViewerContact.ShowLocations();
        }

        private void btnShowMissingFields_Click(object sender, EventArgs e) {
            ValidateDialog();
        }

        private void btnAddFollowUp_Click(object sender, EventArgs e)
        {
            this.DisplayFollowUpForm();
        }

        private void gvEventLog_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Equals("date_of_transaction"))
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd");
        }

        private void btnAddLog_Click(object sender, EventArgs e) 
        {
            if (txtLogFirstName.Text.Length < 1 || txtLogLastname.Text.Length < 1)
            {
                MessageBox.Show("Firstname / Lastname is required.", "Add Call Log", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            else if (txtLogFirstName.Text.Equals("First name") || txtLogLastname.Text.Equals("Last name"))
            {
                MessageBox.Show("Firstname / Lastname is required.", "Add Call Log", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            WaitDialog objDialog = new WaitDialog();
            objDialog.CreateNonStaticWaitDialog("Saving event call log.");

            #region Adding of Contact
            //contact objContact = new contact()
            //{
            //    title = txtLogTitle.Text.Trim(),
            //    first_name = txtLogFirstName.Text.Trim(),
            //    last_name = txtLogLastname.Text.Trim(),
            //    active = true
            //};

            //bool IsEdit = false;
            //contact objExistingContact = ObjectContact.GetContact(txtLogFirstName.Text.Trim(), txtLogLastname.Text.Trim(), oAppointment.AccountId);
            //if (objExistingContact != null)
            //{
            //    objContact = null;
            //    objContact = objExistingContact;

            //    if (!objContact.active)
            //    {
            //        string Message = "This contact is currently de-activated." + Environment.NewLine + "Would you like to re-activate this contact?";
            //        DialogResult objResult = MessageBox.Show(Message, "Add Contact", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //        if (objResult == DialogResult.Yes)
            //        {
            //            objContact.active = true;
            //            IsEdit = true;
            //        }
            //        else
            //        {
            //            WaitDialog.CloseWaitDialog();
            //            return;
            //        }
            //    }
            //}
            //else
            //{
            //    //objContact.id = ObjectContact.GetContactId(txtLogFirstName.Text.Trim(), txtLogLastname.Text.Trim(), oAppointment.AccountId);
            //    IsEdit = false;
            //}

            //int ContactId = ObjectContact.SaveContact(objContact, IsEdit, 0);
            //if (objContact.id < 1)
            //    objContact.id = ContactId;

            //if (!IsEdit)
            //    ObjectCompany.SaveCompanyContact(oAppointment.AccountId, objContact.id);
            
            //var DuplicateEntry = BPContext.sub_campaign_contact_lists.FirstOrDefault(i => i.final_list_id == oAppointment.FinalListId && i.contact_id == objContact.id);
            //if (DuplicateEntry == null)
            //{
            //    BPContext.sub_campaign_contact_lists.AddObject(new sub_campaign_contact_lists()
            //    {
            //        active = true,
            //        created_by = UserSession.CurrentUser.UserId,
            //        final_list_id = oAppointment.FinalListId,
            //        contact_id = objContact.id,
            //        created_on = DateTime.Now
            //    });
            //}
            #endregion

            this.HangUpCall();
            BPContext.event_followup_log.AddObject(new event_followup_log()
            {
                subcampaign_id = oAppointment.SubCampaignId,
                account_id = oAppointment.AccountId,
                //contact_id = objContact.id,
                contact_id = m_oContactView.SelectedContact != null? m_oContactView.SelectedContact.id: 0,
                title = txtLogTitle.Text,
                short_message = txtLogShortMessage.Text,
                event_type = "Call Log",
                event_status = cbLogCallStatus.Text,
                date_of_transaction = DateTime.Now,
                assigned_user = UserSession.CurrentUser.UserId,
                done = true,
                start_time = Convert.ToDateTime(esiLogCallStart.Text).TimeOfDay,
                end_time = Convert.ToDateTime(esiLogCallEnd.Text).TimeOfDay,
                date_created = DateTime.Now,
                created_by = UserSession.CurrentUser.UserId,
                contact_name = txtLogFirstName.Text + " " + txtLogLastname.Text
            });

            BPContext.SaveChanges();
            m_oContactView.PopulateContactView(oAppointment.SubCampaignId, oAppointment.AccountId, oAppointment.FinalListId);
            //CompanyContactView.PopulateContactView(oAppointment.AccountId, oAppointment.SubCampaignId, oAppointment.FinalListId);
            this.PopulateEventFollowUpView();
            this.SetDefaultEventLogInfos();
            objDialog.CloseNonStaticWaitDialog();
        }

        private void txtContactRemark_KeyUp(object sender, KeyEventArgs e)
        {
            m_ContactRemarkChanged = true;
        }

        private void timerCallLog_Tick(object sender, EventArgs e) {
            dtEnd = DateTime.Now;
            TimeSpan timeElapse = dtEnd.Subtract(dtStart);
            esiLogLength.Text = string.Format("{0}:{1}", timeElapse.Minutes, timeElapse.Seconds);
            esiLogCallEnd.Text = dtEnd.ToString("H:mm");
        }
        
        private void btnMute_Click(object sender, EventArgs e) {
            btnMute.Enabled = false;
            btnMute.Enabled = true;
        }

        private void btnHangUp_Click(object sender, EventArgs e) 
        {
            this.HangUpCall();
            //timerCallLog.Enabled = false;
            //EnableControls(true);

            //btnCallBoard.Enabled = false;
            //btnCallDirect.Enabled = false;
            //btnCallMobile.Enabled = false;
            //btnHangUp.Enabled = false;
            //btnCallNumber.Enabled = true;

            //if (!string.IsNullOrEmpty(oAppointment.CompanyBoardNumber))
            //    btnCallBoard.Enabled = true;

            //if (m_oContactView.SelectedContact != null)
            //{
            //    if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.direct_phone))
            //        btnCallDirect.Enabled = true;
            //    if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.mobile))
            //        btnCallMobile.Enabled = true;
            //}
        }

        private void btnCallBoard_Click(object sender, EventArgs e) {
            ResetTimers();
            EnableControls(false);
            txtLogFirstName.Text = "Call";
            txtLogLastname.Text = "Log";
            btnHangUp.Enabled = true;
            InitiateCall(oAppointment.CompanyBoardNumber);
        }

        private void btnCallDirect_Click(object sender, EventArgs e) {
            ResetTimers();
            EnableControls(false);
            btnHangUp.Enabled = true;
            InitiateCall(m_oContactView.SelectedContact.direct_phone);
        }

        private void btnCallMobile_Click(object sender, EventArgs e) {
            ResetTimers();
            EnableControls(false);
            btnHangUp.Enabled = true;
            InitiateCall(m_oContactView.SelectedContact.mobile);
        }
        
        private void btnCallNumber_Click(object sender, EventArgs e) {
            ResetTimers();
            EnableControls(false);
            btnHangUp.Enabled = true;
            txtLogFirstName.Text = "Call";
            txtLogLastname.Text = "Log";
            InitiateCall(txtEnterNumber.Text.Trim());
        }

        private void cboFollowUpTimeStart_CustomDisplayText(object sender, CustomDisplayTextEventArgs e)
        {
            e.DisplayText = Convert.ToDateTime(e.DisplayText).ToString("HH:mm");
        }

        private void cboFollowUpTimeEnd_CustomDisplayText(object sender, CustomDisplayTextEventArgs e)
        {
            e.DisplayText = Convert.ToDateTime(e.DisplayText).ToString("HH:mm");
        }
        
        private void btnDeleteDialog_Click(object sender, EventArgs e) 
        {
            if (oAppointment != null && m_oContactView != null && m_oContactView.SelectedContact != null) 
            {
                DialogResult objDialog = MessageBox.Show("Are you sure to delete values in this dialog for this contact?", "Dialogs", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (objDialog == DialogResult.No)
                    return;

                lcgContact.Enabled = false;
                if (m_oDialog == null)
                {
                    MessageBox.Show("There is no current dialog created for this customer's subcampaign.");
                    return;
                    //prompt message saying that no dialog for current subcampaign and customer.
                }
                WaitDialog wdiag = new WaitDialog();
                wdiag.CreateNonStaticWaitDialog("Deleting dialog values.");
                //WaitDialog.CreateWaitDialog("Deleting dialog values...");
                BusinessAnswer.DeleteAnswers(
                    m_oDialog.id,
                    m_oContactView.SelectedContact.id,
                    oAppointment.CampaignId,
                    oAppointment.AccountId);
                wdiag.CloseNonStaticWaitDialog();

                CloseDialogEditor(false);
                m_oContactView.SaveSubCampaignContactAppointmentStatus(
                    ContactView.eDialogStatus.None,
                    oAppointment.FinalListId,
                    oAppointment.AccountId,
                    oAppointment.SubCampaignId);
                oMode = SaveMode.Unspecified;
                CloseDialogEditor(false);
                ViewDialog();
                lcgContact.Enabled = true;
                //enable campaign booking saving
                cmdSaveExitAsOpen.Enabled = true;
                cmdSaveExitAsInProgress.Enabled = true;
                cmdSaveExitAsFinished.Enabled = true;
                cmdSaveExitAsFollowUp.Enabled = true;
                cmdSelectCompany.Enabled = true;
            }
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            txtLogFirstName.Properties.ReadOnly = false;
            txtLogLastname.Properties.ReadOnly = false;
        }

        private void cmdSaveExitAsOpen_Click(object sender, EventArgs e)
        {
            if (ParentView == eParentView.CompanyContactView)
                CompanyContactView.SaveCompanyAppointment("Open");

            if (ParentView == eParentView.MyFollowUpView)
                MyFollowUpView.SaveCompanyAppointment("Open");

            oAppointment.CompanyAppointmentStatus = "Open";
            this.SaveCompanyRemarks();
            //this.SetRecordNavigationButtons(true);
            this.SetSaveButtonAppearance();
            //this.EditCampaignBooking(false);
            //this.Close();
        }

        private void cmdSaveExitAsInProgress_Click(object sender, EventArgs e)
        {
            if (ParentView == eParentView.CompanyContactView)
                CompanyContactView.SaveCompanyAppointment("In Progress");

            if (ParentView == eParentView.MyFollowUpView)
                MyFollowUpView.SaveCompanyAppointment("In Progress");

            oAppointment.CompanyAppointmentStatus = "In Progress";
            this.SaveCompanyRemarks();
            //this.SetRecordNavigationButtons(true);
            this.SetSaveButtonAppearance();
            //this.EditCampaignBooking(false);
            //this.Close();
        }

        private void cmdSaveExitAsFollowUp_Click(object sender, EventArgs e)
        {
            if (ParentView == eParentView.CompanyContactView)
                CompanyContactView.SaveCompanyAppointment("Follow Up");

            if (ParentView == eParentView.MyFollowUpView)
                MyFollowUpView.SaveCompanyAppointment("Follow Up");

            oAppointment.CompanyAppointmentStatus = "Follow Up";
            this.SaveCompanyRemarks();
            //this.SetRecordNavigationButtons(true);
            this.SetSaveButtonAppearance();
            //this.EditCampaignBooking(false);
            //this.Close();
        }

        private void cmdUseContact_Click(object sender, EventArgs e)
        {
            if (m_oContactView.SelectedContact == null)
                return;

            txtLogFirstName.Text = m_oContactView.SelectedContact.first_name;
            txtLogLastname.Text = m_oContactView.SelectedContact.last_name;
        }

        private void cmdDeleteCallLog_Click(object sender, EventArgs e)
        {
            this.DeleteCallLog();
        }

        private void cmdNextRecord_Click(object sender, EventArgs e)
        {
            if (ParentView == eParentView.CompanyContactView) {
                CompanyContactView.MoveToNextCompanyRecord();
                if (CompanyContactView.IsLastRow)
                    cmdNextRecord.Enabled = false;
                else
                    cmdNextRecord.Enabled = true;
            } else if (ParentView == eParentView.MyFollowUpView) {
                MyFollowUpView.MoveToNextCompanyRecord();
                if (MyFollowUpView.IsLastRow)
                    cmdNextRecord.Enabled = false;
                else
                    cmdNextRecord.Enabled = true;
            }
            //this.SetRecordNavigationButtons(false);
            //this.EditCampaignBooking(true);
        }

        private void cmdPreviousRecord_Click(object sender, EventArgs e)
        {
            if (ParentView == eParentView.CompanyContactView) {
                CompanyContactView.MoveToPreviousCompanyRecord();
                if (CompanyContactView.IsFirstRow)
                    cmdPreviousRecord.Enabled = false;
                else
                    cmdPreviousRecord.Enabled = true;
            }else if (ParentView == eParentView.MyFollowUpView){
                MyFollowUpView.MoveToPreviousCompanyRecord();
                if (MyFollowUpView.IsFirstRow)
                    cmdPreviousRecord.Enabled = false;
                else
                    cmdPreviousRecord.Enabled = true;
            }
            
            //this.SetRecordNavigationButtons(false);
            //this.EditCampaignBooking(true);
        }

        private void FrmCampaignBooking_SizeChanged(object sender, EventArgs e)
        {
            FormUtility.DisableCloseButton(this.Handle);
        }

        private void cmdSelectCompany_Click(object sender, EventArgs e)
        {
            this.SetRecordNavigationButtons(true);
            this.EditCampaignBooking(false);
        }

        private void cmdWork_Click(object sender, EventArgs e)
        {
            this.SetRecordNavigationButtons(false);
            this.EditCampaignBooking(true);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// set campaign booking components
        /// </summary>
        private void EditCampaignBooking(bool OnEdit)
        {
            if (OnEdit)
            {
                cmdSelectCompany.Enabled = true;
                cmdSaveExitAsOpen.Enabled = true;
                cmdSaveExitAsFollowUp.Enabled = true;
                cmdSaveExitAsInProgress.Enabled = true;
                cmdSaveExitAsFinished.Enabled = true;
                cmdSelectCompany.Enabled = true;
                layoutControlGroup6.Enabled = true;
                gcEventLog.Enabled = true;
                cmdDeleteCallLog.Enabled = true;
                layoutControlGroup7.Enabled = true;
                lcgContactInformation.Enabled = true;
                lcgCollectedData.Enabled = true;
                lcgCompanyWebsite.Enabled = true;
                lcgGoogleSearch.Enabled = true;
                lcgStandardQuestions.Enabled = true;
                lcgMapContacts.Enabled = true;
                lcgBVSalesScript.Enabled = true;
                lcgMySalesScript.Enabled = true;
                layoutControlGroup15.Enabled = true;
                layoutControlGroup5.Enabled = true;
            }
            else
            {
                cmdSelectCompany.Enabled = false;
                cmdSaveExitAsOpen.Enabled = false;
                cmdSaveExitAsFollowUp.Enabled = false;
                cmdSaveExitAsInProgress.Enabled = false;
                cmdSaveExitAsFinished.Enabled = false;
                cmdSelectCompany.Enabled = false;
                layoutControlGroup6.Enabled = false;
                gcEventLog.Enabled = false;
                cmdDeleteCallLog.Enabled = false;
                layoutControlGroup7.Enabled = false;
                lcgContactInformation.Enabled = false;
                lcgCollectedData.Enabled = false;
                lcgCompanyWebsite.Enabled = false;
                lcgGoogleSearch.Enabled = false;
                lcgStandardQuestions.Enabled = false;
                lcgMapContacts.Enabled = false;
                lcgBVSalesScript.Enabled = false;
                lcgMySalesScript.Enabled = false;
                layoutControlGroup15.Enabled = false;
                layoutControlGroup5.Enabled = false;
            }
        }

        /// <summary>
        /// Set enabled/ disabled the record navigational buttons
        /// </summary>
        private void SetRecordNavigationButtons(bool IsEnabled)
        {
            cmdNextRecord.Enabled = IsEnabled;
            cmdPreviousRecord.Enabled = IsEnabled;
            cmdWork.Enabled = IsEnabled;

            if (IsEnabled)
            {
                FormUtility.EnableCloseButton(this.Handle);
                cmdSelectCompany.Enabled = false;
                cmdSaveExitAsOpen.Enabled = false;
                cmdSaveExitAsFollowUp.Enabled = false;
                cmdSaveExitAsInProgress.Enabled = false;
                cmdSaveExitAsFinished.Enabled = false;
            }
            else
            {
                FormUtility.DisableCloseButton(this.Handle);
                cmdSelectCompany.Enabled = true;
                cmdSaveExitAsOpen.Enabled = true;
                cmdSaveExitAsFollowUp.Enabled = true;
                cmdSaveExitAsInProgress.Enabled = true;
                cmdSaveExitAsFinished.Enabled = true;
            }
        }

        private bool ValidateDialog() {
            bool valid = true, valid2 = true;
            IQuestionnaire iQuestion = null;
            var objGroupItems = this.layoutControlGroupQuestionnaire.Items;
            foreach (BaseLayoutItem item in objGroupItems) {
                if (item.IsGroup) {
                    if (item.Tag != null) {
                        iQuestion = item.Tag as IQuestionnaire;
                        if (iQuestion != null) {
                            //validate all controls
                            valid = iQuestion.Validate();
                            //check if not valid then log to return false for invalid
                            //it would just evaluate once when the valid2 value changes
                            if (!valid && valid2)
                                valid2 = false;
                        }
                    }
                }
            }
            return valid2;
        }

        /// <summary>
        /// Delete a call log
        /// </summary>
        private void DeleteCallLog()
        {
            if (gvEventLog.RowCount < 1)
                return;

            GridView objEventFollowUpGrid = (GridView) gcEventLog.FocusedView;
            CTScEventAndFollowUpLog Item = (CTScEventAndFollowUpLog) objEventFollowUpGrid.GetFocusedRow();
            //if (!Item.event_type.Equals("Call Log"))
            //{
            //    MessageBox.Show("You cannot delete this Follow-Up log entry", "Call & Event Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            if (Item.assigned_user != UserSession.CurrentUser.UserId)
            {
                MessageBox.Show("You cannot delete this Call Log entry", "Call & Event Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult objDialog = MessageBox.Show("Are you sure to delete this call log entry / follow-up?", "Event & Follow Ups", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objDialog == DialogResult.No)
                return;

            WaitDialog.CreateWaitDialog("Deleting call log entry.");
            EventFollowUp.Delete(Item.id);
            this.PopulateEventFollowUpView();
            WaitDialog.CloseWaitDialog();
        }

        /// <summary>
        /// hang up the current call
        /// </summary>
        private void HangUpCall()
        {
            timerCallLog.Enabled = false;
            EnableControls(true);

            btnCallBoard.Enabled = false;
            btnCallDirect.Enabled = false;
            btnCallMobile.Enabled = false;
            btnHangUp.Enabled = false;
            btnCallNumber.Enabled = true;

            if (!string.IsNullOrEmpty(oAppointment.CompanyBoardNumber))
                btnCallBoard.Enabled = true;

            if (m_oContactView.SelectedContact != null)
            {
                if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.direct_phone))
                    btnCallDirect.Enabled = true;
                if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.mobile))
                    btnCallMobile.Enabled = true;
            }
        }

        private void CloseDialogEditor(bool Dispose) {
            if (Dispose) {
                layoutControlGroupQuestionnaire.BeginUpdate();
                layoutControlGroupQuestionnaire.BeginInit();
                DisposeGroupControls(layoutControlGroupQuestionnaire);
                this.layoutControlGroupQuestionnaire.EndInit();
                this.layoutControlGroupQuestionnaire.EndUpdate();
            }
            btnShowMissingFields.Enabled = false;
            btnSaveAsCompleted.Enabled = false;
            btnSaveAsDone.Enabled = false;
            btnSaveAsInProgress.Enabled = false;            
            btnDeleteDialog.Enabled = false;
            btnEditDialog.Enabled = true;
        }

        /// <summary>
        /// Set a default values for event call log and follow up forms
        /// </summary>
        private void SetDefaultEventLogInfos()
        {
            txtLogFirstName.Text = "";
            txtLogLastname.Text = "";
            txtLogTitle.Text = "";
            lblContact.Text = "";
            txtFollowUpShortMessage.Text = "Short Message (referal data etc)";
            txtLogShortMessage.Text = "Short Message (referal data etc)";
            cbLogCallStatus.Text = "Successfull";
            cbFollowUpStatus.Text = "Follow-Up Call";
            cboFollowUpAssignedTo.EditValue = UserSession.CurrentUser.UserId;
            cboFollowUpDate.EditValue = DateTime.Now;
            cboFollowUpTimeStart.EditValue = DateTime.Now.TimeOfDay;

            if (m_oContactView.SelectedContact != null)
            {
                txtLogFirstName.Text = m_oContactView.SelectedContact.first_name;
                txtLogLastname.Text = m_oContactView.SelectedContact.last_name;
                txtLogTitle.Text = m_oContactView.SelectedContact.title;
                lblContact.Text = "Contact: " + m_oContactView.SelectedContact.first_name + " " + m_oContactView.SelectedContact.last_name;
            }
        }

        private void ResetTimers() {
            dtStart = DateTime.Now;
            dtEnd = DateTime.Now;
            timerCallLog.Enabled = true;
            esiLogCallStart.Text = dtStart.ToString("H:mm");
            esiLogCallEnd.Text = dtStart.ToString("H:mm");
        }

        private void InitiateCall(string number) {            
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = "SIP:"+ number.ToSwedishPhoneNumber();
            proc.Start();
            proc.Close();
        }

        private void EnableControls(bool enabled) {
            txtEnterNumber.Enabled = enabled;
            btnCallBoard.Enabled = enabled;            
            btnCallNumber.Enabled = enabled;
            btnCallDirect.Enabled = enabled;
            btnCallMobile.Enabled = enabled;
            btnHangUp.Enabled = false;            
            if (enabled) {                
                oMode = SaveMode.Edit;
            }
        }

        /// <summary>
        /// Populate the assign to list combo box
        /// </summary>
        private void PopulateFollowUpAssignedToComboList()
        {
            try
            {
                cboFollowUpAssignedTo.Properties.Columns.Clear();
                cboFollowUpAssignedTo.Properties.DataSource = null;
                cboFollowUpAssignedTo.Properties.DataSource = SalesScript.GetUsers(oAppointment.FinalListId).Execute(MergeOption.AppendOnly);
                cboFollowUpAssignedTo.Properties.DisplayMember = "name";
                cboFollowUpAssignedTo.Properties.ValueMember = "id";
                cboFollowUpAssignedTo.Properties.Columns.Add(new LookUpColumnInfo("name"));
                cboFollowUpAssignedTo.EditValue = UserSession.CurrentUser.UserId;
            }
            catch { }
        }

        /// <summary>
        /// Set defaults to follow up tab
        /// </summary>
        private void InitializeFollowUpView()
        {
            txtLogFirstName.Properties.ReadOnly = true;
            txtLogLastname.Properties.ReadOnly = true;
            lblContact.Text = "Contact:";
            cbFollowUpStatus.Text = "Follow-Up Call";
            txtFollowUpShortMessage.Text = "Short Message (referral data etc)";
            cboFollowUpAssignedTo.EditValue = UserSession.CurrentUser.UserId;
            cboFollowUpDate.EditValue = DateTime.Now;
            cboFollowUpTimeStart.EditValue = DateTime.Now.TimeOfDay;
        }

        /// <summary>
        /// Save event follow
        /// </summary>
        private void SaveEventFollowUp()
        {
            //if (txtFollowUpShortMessage.Text.Length < 1 || txtFollowUpShortMessage.Text.Equals("Short Message (referral data etc)"))
            //{
            //    MessageBox.Show("Please enter a short message", "Event Follow-Up", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    txtFollowUpShortMessage.SelectAll();
            //    txtFollowUpShortMessage.Focus();
            //    return;
            //}

            WaitDialog.CreateWaitDialog("Saving event follow-up.");
            event_followup_log objFollowUpData = new event_followup_log()
            {
                subcampaign_id = oAppointment.SubCampaignId,
                account_id = oAppointment.AccountId,
                contact_id = m_oContactView.SelectedContact != null? m_oContactView.SelectedContact.id: 0,
                event_type = cbFollowUpStatus.Text.Equals("Follow-Up Call")? "Make Call": "Send Mail",
                event_status = cbFollowUpStatus.Text,
                short_message = txtFollowUpShortMessage.Text,
                date_of_transaction = (DateTime) cboFollowUpDate.EditValue,
                start_time = cboFollowUpTimeStart.Time.TimeOfDay,
                end_time = cboFollowUpTimeStart.Time.TimeOfDay, // set as the same as end time will not apply for follow ups
                assigned_user = (int) cboFollowUpAssignedTo.EditValue,
                date_created = DateTime.Now,
                created_by = UserSession.CurrentUser.UserId,
                contact_name = m_oContactView.SelectedContact.first_name + " " + m_oContactView.SelectedContact.last_name
            };

            EventFollowUp.Save(objFollowUpData);
            WaitDialog.CloseWaitDialog();
            WaitDialog.CreateWaitDialog("Populating event and follow up list view.");
            this.PopulateEventFollowUpView();
            this.InitializeFollowUpView();
            WaitDialog.CloseWaitDialog();
            MessageBox.Show("Successfully saved event follow up.", "Event Follow Up", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Save contact remarks
        /// </summary>
        private void SaveCompanyRemarks()
        {
            ObjectCompany.SaveCompanyRemarks(oAppointment.SubCampaignId, oAppointment.AccountId, txtCompanyRemark.Text);
        }

        /// <summary>
        /// Dispose group controls
        /// </summary>
        /// <param name="Item"></param>
        private void DisposeGroupControls(BaseLayoutItem Item) {
            if (Item == null) return;
            var FlatList = new DevExpress.XtraLayout.Helpers.FlatItemsList();
            List<BaseLayoutItem> Items = FlatList.GetItemsList(Item);
            ILayoutControl Layout = Item.Owner;
            if (Layout != null) {
                Layout.BeginUpdate();
                BaseLayoutItem li;
                for (int i = Items.Count - 1; i >= 0; --i) {
                    li = Items[i];
                    if (!li.Equals(Item) && li.Name != Item.Name) {
                        if (li is LayoutControlItem) {
                            Control TempControl = (li as LayoutControlItem).Control;
                            if (TempControl != null) {
                                TempControl.Dispose();
                            }
                            li.Dispose();
                            Items.RemoveAt(i);
                        } else {
                            li.Dispose();
                            Items.RemoveAt(i);
                        }
                    }
                }
                Layout.EndUpdate();
            }
        }

        private void SetEditableComponent(bool isEditable) {
            IQuestionnaire iQuestion = null;
            var objGroupItems = this.layoutControlGroupQuestionnaire.Items;
            foreach (BaseLayoutItem item in objGroupItems) {
                if (item.IsGroup) {
                    if (item.Tag != null) {
                        iQuestion = item.Tag as IQuestionnaire;
                        iQuestion.ReadOnly = isEditable;
                    }
                }
            }
        }

        /// <summary>
        /// Save dialog answers
        /// </summary>
        private bool SaveDialogAnswers(ContactView.eDialogStatus status) {
            if (!ValidateDialog()) return false;

            WaitDialog wdiag = new WaitDialog();
            wdiag.CreateNonStaticWaitDialog("Saving dialog answers...");
            IQuestionnaire iQuestion = null;
            var objGroupItems = this.layoutControlGroupQuestionnaire.Items;
            foreach (BaseLayoutItem item in objGroupItems) {
                if (item.IsGroup) {
                    if (item.Tag != null) {
                        iQuestion = item.Tag as IQuestionnaire;
                        BusinessAnswer.SaveAnswer(iQuestion.Questionnaire);
                    }
                }
            }
            if (m_oContactView != null) {
                try {
                    m_oContactView.SaveSubCampaignContactAppointmentStatus(
                        status,
                        oAppointment.FinalListId,
                        oAppointment.AccountId,
                        oAppointment.SubCampaignId);
                } catch { }
                oMode = SaveMode.Unspecified;
                CloseDialogEditor(false);               
            }
            wdiag.CloseNonStaticWaitDialog();
            return true;
        }

        /// <summary>
        /// View dialog
        /// </summary>
        private void ViewDialog() {            
            m_oContactView.gvContact.OptionsFind.AlwaysVisible = true;
            WaitDialog.CreateWaitDialog("Loading dialog questionnaires...");
            
            #region Populate each JSON questionnaire from dialog text to list object type
            var selectedContact = m_oContactView.SelectedContact;
            groupControlDialog.Text = string.Format("Dialog with: {0} {1}, {2}", selectedContact.first_name, selectedContact.last_name, SelectedCompany);

            m_oDialog = BPContext.dialogs.FirstOrDefault(p =>
                p.subcampaign_id == oAppointment.SubCampaignId &&
                    //p.customers_id == oAppointment.CustomerId &&
                p.is_active == true);
            if (m_oDialog == null) {
                MessageBox.Show("There is no current dialog created for this customer's subcampaign.");
                WaitDialog.CloseWaitDialog();
                return;
                //prompt message saying that no dialog for current subcampaign and customer.
            }
            var CQList = new List<CampaignQuestionnaire>();
            CampaignQuestionnaire oQuestionnaire = null;
            List<string> cbdList = new List<string>();
            DataBindings oBindings = null;
            if (!string.IsNullOrEmpty(m_oDialog.dialog_text_json)) {
                var jaDiag = JArray.Parse(m_oDialog.dialog_text_json);
                jaDiag.ForEach(delegate(JToken jt) {
                    oQuestionnaire = CampaignQuestionnaire.InstanciateWith(jt.ToString(Formatting.None).Unescape());
                    if (oQuestionnaire != null) {
                        CQList.Add(oQuestionnaire);
                        oBindings = oQuestionnaire.Form.Settings.DataBindings;
                        if (oBindings != null) {
                            if (!string.IsNullOrEmpty(oBindings.questionlayout_id)) {
                                cbdList.Add(oBindings.questionlayout_id);
                            }
                        }
                    }
                });
            }
            #endregion

            #region Populate Answers to each questionnaire

            int? campaign_id = oAppointment.CampaignId;
            int? account_id = oAppointment.AccountId;
            int? contact_id = m_oContactView.SelectedContact.id;
            int? dialog_id = m_oDialog.id;

            //get all dialog answers based on questionlayout_ids and other params
            var listDialogAnswers = BPContext.FIGetDialogAnswers(
                string.Join(",", cbdList.Distinct().ToArray()),
                campaign_id,
                account_id,
                contact_id,
                dialog_id).ToList().Clone();

            layoutControlGroupQuestionnaire.BeginUpdate();
            layoutControlGroupQuestionnaire.BeginInit();
            DisposeGroupControls(layoutControlGroupQuestionnaire);
            int rowcount = CQList.Count;

            CTDialogAnswers dlgAnswer = null;
            for (int i = 0; i < rowcount; ++i) {
                oQuestionnaire = CQList[i];
                oBindings = oQuestionnaire.Form.Settings.DataBindings;
                dlgAnswer = listDialogAnswers.FirstOrDefault(p =>
                            p.questionlayout_id == int.Parse(oBindings.questionlayout_id) &&
                            p.campaign_id == campaign_id &&
                            p.account_id == account_id &&
                            p.contact_id == contact_id &&
                            p.dialog_id == dialog_id);

                if (dlgAnswer != null) {
                    oBindings.answer_id = dlgAnswer.id.ToString();
                } else {
                    oBindings.account_id = account_id.ToString();
                    oBindings.campaign_id = campaign_id.ToString();
                    oBindings.contact_id = contact_id.ToString();
                    oBindings.dialog_id = dialog_id.ToString();
                    oBindings.created_by = UserSession.CurrentUser.UserId.ToString();                    
                }

                switch (oQuestionnaire.Type.ToLower()) {
                    case QuestionTypeConstants.Dropbox:
                        Dropbox oDropbox = new Dropbox(this.layoutControlQuestionnaire);
                        oDropbox.DisableSelection = true;
                        oDropbox.ToolTipController = defaultToolTipController1;

                        //load answers for this questionnaire                        
                        if (dlgAnswer != null) {
                            oDropbox.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire);
                        } else {
                            oDropbox.Questionnaire = oQuestionnaire;
                        }

                        oDropbox.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(oDropbox.ControlGroup);
                        break;
                    case QuestionTypeConstants.MultipleChoice:
                        Multiplechoice oMultipleChoice = new Multiplechoice(this.layoutControlQuestionnaire);
                        oMultipleChoice.DisableSelection = true;
                        oMultipleChoice.ToolTipController = defaultToolTipController1;

                        //load answers for this questionnaire                        
                        if (dlgAnswer != null) {
                            oMultipleChoice.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire);
                        } else {
                            oMultipleChoice.Questionnaire = oQuestionnaire;
                        }

                        oMultipleChoice.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(oMultipleChoice.ControlGroup);
                        break;
                    case QuestionTypeConstants.Textbox:
                        Textbox oTextbox = new Textbox(this.layoutControlQuestionnaire);
                        oTextbox.DisableSelection = true;
                        oTextbox.ToolTipController = defaultToolTipController1;

                        //load answers for this questionnaire                        
                        if (dlgAnswer != null) {
                            oTextbox.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire);
                        } else {
                            oTextbox.Questionnaire = oQuestionnaire;
                        }

                        oTextbox.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(oTextbox.ControlGroup);
                        break;
                    case QuestionTypeConstants.Schedule:
                        Schedule oSchedule = new Schedule(this.layoutControlQuestionnaire);
                        var result = m_oContactView.gcContact.DataSource as List<CTScSubCampaignContactList>;
                        if (result != null)
                            oSchedule.SubCampaignContactList = result;
                        oSchedule.SubcampaignID = oAppointment.SubCampaignId;
                        oSchedule.DisableSelection = true;
                        oSchedule.ToolTipController = defaultToolTipController1;
                        if (m_oContactView.SelectedContact != null) {
                            oSchedule.SetCurrentCaller(m_oContactView.SelectedContact, oAppointment.AccountId);
                        }
                        
                        //load answers for this questionnaire                        
                        if (dlgAnswer != null) {                            
                            oSchedule.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire);
                        } else {
                            oSchedule.Questionnaire = oQuestionnaire;
                        }

                        oSchedule.BindControls();
                        oSchedule.ShowCalendarBookingClick += new EventHandler(oSchedule_ShowCalendarBookingClick);                        
                        this.layoutControlGroupQuestionnaire.Add(oSchedule.ControlGroup);
                        break;
                    //case QuestionTypeConstants.Seminar:
                    //    Seminar oSeminar = new Seminar(this.layoutControlQuestionnaire);
                    //    oSeminar.DisableSelection = true;
                    //    oSeminar.ToolTipController = defaultToolTipController1;

                    //    //load answers for this questionnaire                        
                    //    if (dlgAnswer != null) {
                    //        oSeminar.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire);
                    //    } else {
                    //        oSeminar.Questionnaire = oQuestionnaire;
                    //    }

                    //    oSeminar.BindControls();
                    //    this.layoutControlGroupQuestionnaire.Add(oSeminar.ControlGroup);
                    //    break;
                }
            }
            EmptySpaceItem emptySpaceItem1 = new EmptySpaceItem();
            emptySpaceItem1.CustomizationFormText = "emptySpaceItemBottom";
            emptySpaceItem1.Name = "emptySpaceItemBottom";
            emptySpaceItem1.ShowInCustomizationForm = false;
            emptySpaceItem1.Size = new System.Drawing.Size(0, 0);
            emptySpaceItem1.Text = "emptySpaceItemBottom";
            emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);

            //add bottom spacer
            this.layoutControlGroupQuestionnaire.AddItem(emptySpaceItem1);

            //set readonly and disabled controls -> for viewing only
            //this.layoutControlQuestionnaire.OptionsView.IsReadOnly = DevExpress.Utils.DefaultBoolean.True;
            SetEditableComponent(false);
            ValidateDialog();

            this.layoutControlGroupQuestionnaire.EndInit();
            this.layoutControlGroupQuestionnaire.EndUpdate();
            this.layoutControlQuestionnaire.BestFit();
            #endregion
            WaitDialog.CloseWaitDialog();
        }

        /// <summary>
        /// Populate the assign to combo list
        /// </summary>
        private void PopulateAssignToComboList()
        {
            try
            {
                cboAssignTo.Columns.Clear();
                cboAssignTo.DataSource = null;
                cboAssignTo.DataSource = SalesScript.GetUsers(oAppointment.FinalListId).Execute(MergeOption.AppendOnly);
                cboAssignTo.DisplayMember = "name";
                cboAssignTo.ValueMember = "id";
                cboAssignTo.Columns.Add(new LookUpColumnInfo("name"));
            }
            catch { }
        }

        /// <summary>
        /// Save the updated events and follow ups on the database
        /// </summary>
        private void SaveUpdatedEventFollowUps()
        {
            //DialogResult objResult = MessageBox.Show("Are you sure to save changes for this entry?", "Event Follow Up", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (objResult == DialogResult.Yes)
            //{
            //    WaitDialog.CreateWaitDialog("Saving changes for event and follow-up logs.");
            //    EventFollowUp.Save(m_EventFollowUpList);
            //    this.PopulateEventFollowUpView();
            //    this.InitializeFollowUpView();
            //    WaitDialog.CloseWaitDialog();
            //}

            WaitDialog.CreateWaitDialog("Saving changes for event and follow-up logs.");
            EventFollowUp.Save(m_EventFollowUpList);
            this.PopulateEventFollowUpView();
            this.InitializeFollowUpView();
            WaitDialog.CloseWaitDialog();
        }

        /// <summary>
        /// Queue updated event and follow ups
        /// </summary>
        private void QueueUpdatedEventFollowUp()
        {
            this.SetSelectedCheckedvalue();
            m_EventFollowUpList.Clear();
            m_EventFollowUpList.Add(m_EventFollowUp);
            this.SaveUpdatedEventFollowUps();

            //if (!m_EventFollowUpList.Contains(m_EventFollowUp))
            //    m_EventFollowUpList.Add(m_EventFollowUp);
        }

        /// <summary>
        /// Sets the exact checked value for a specified row to be updated for "done" or "i take it"
        /// </summary>
        private void SetSelectedCheckedvalue()
        {
            switch (m_CheckEditType)
            {
                case eCheckEditType.Done:
                    m_EventFollowUp.done = m_IsChecked;
                    break;

                case eCheckEditType.UserTaken:
                    m_EventFollowUp.user_taken = m_IsChecked;
                    break;
            }
        }

        /// <summary>
        /// Sets the focused view instance of the grid. Instantiates the objects that can be used for data manipulation.
        /// </summary>
        private void SetFocusedViewInstance()
        {
            m_EventFollowUp = null;
            GridView objGridView = (GridView) gcEventLog.FocusedView;
            m_EventFollowUp = (CTScEventAndFollowUpLog) objGridView.GetFocusedRow();
        }

        /// <summary>
        /// Displays the add follow up form
        /// </summary>
        private void DisplayFollowUpForm()
        {
            if (oAppointment == null || m_oContactView.SelectedContact == null)
            {
                MessageBox.Show("No selected company/contact.", "Add Event Follow Ups", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            WaitDialog.CreateWaitDialog("Loading add follow up form.");
            m_objFollowUpForm = new ManageEventFollowUp(m_oContactView.SelectedContact.id, oAppointment);
            m_objFollowUpForm.ParentModule = this;
            PopupDialog objPopupDialog = new PopupDialog();
            objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            objPopupDialog.MinimizeBox = false;
            objPopupDialog.MaximizeBox = false;
            objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            objPopupDialog.Text = "Add Event Follow Up";
            objPopupDialog.Controls.Add(m_objFollowUpForm);
            objPopupDialog.ClientSize = new Size(m_objFollowUpForm.Width + 2, m_objFollowUpForm.Height + 2);
            WaitDialog.CloseWaitDialog();
            objPopupDialog.ShowDialog(this.ParentForm);
        }        

        /// <summary>
        /// Displays the contact form
        /// </summary>
        /// <param name="IsNew"></param>
        private void DisplayContactForm(bool IsNew)
        {
           // WaitDialog.CreateWaitDialog("Loading add contact form...");

            if (IsNew)
                m_objContactForm = new AddContact(oAppointment.AccountId);
            else
                m_objContactForm = new AddContact(oAppointment.AccountId, m_oContactView.SelectedContact.id);

            m_objContactForm.simpleButtonCancel.Click += new EventHandler(m_objContactFormCancelButton_Click);
            m_objContactForm.simpleButtonSave.Click += new EventHandler(m_objContactFormSaveButton_Click);
            m_objContactForm.FormClosed += new FormClosedEventHandler(m_objContactForm_FormClosed);
            m_objContactForm.StartPosition = FormStartPosition.CenterScreen;
            //WaitDialog.CloseWaitDialog();
            m_objContactForm.ShowDialog(this.ParentForm);
        }

        /// <summary>
        /// Load geo map locations
        /// </summary>
        private void LoadContactGeoLocations() {
            double Latitude = 0;
            double Longitude = 0;
            GeoLocationViewer.GeoMapLocation Location = null;

            m_GeoMapListContact = null;
            m_objGridViewContact = null;
            m_GeoMapListContact = new List<GeoLocationViewer.GeoMapLocation>();
            m_objGridViewContact = (DevExpress.XtraGrid.Views.Grid.GridView)m_oContactView.gcContact.FocusedView;

            for (int i = 0; i < m_objGridViewContact.RowCount; i++) {
                m_objContact = null;
                m_objContact = (CTScSubCampaignContactList)m_objGridViewContact.GetRow(i);

                string strAddress = m_objContact.complete_address;
                if (string.IsNullOrEmpty(strAddress))
                    continue;

                string[] objGeoData = m_objMapUtility.GetGeographicalData(strAddress).Split(',');

                /**
                 * where: 
                 * objGeoData[2] = latitude
                 * objGeoData[3] = longitude
                 */

                Latitude = 0;
                Longitude = 0;

                if (objGeoData[2] != null)
                    if (ValidationUtility.IsCurrency(objGeoData[2]))
                        Latitude = Convert.ToDouble(objGeoData[2], CultureInfo.InvariantCulture);

                if (objGeoData[3] != null)
                    if (ValidationUtility.IsCurrency(objGeoData[3]))
                        Longitude = Convert.ToDouble(objGeoData[3], CultureInfo.InvariantCulture);

                // display geo data on grid
                m_objGridViewContact.SetRowCellValue(i, "latitude", Latitude.ToString());
                m_objGridViewContact.SetRowCellValue(i, "longitude", Longitude.ToString());
                //m_objGridViewCompany.SetRowCellValue(i, "geo_status", objGeoData[2].Equals("0") && objGeoData[3].Equals("0") ? "not found" : "found");

                if (Latitude != 0 || Longitude != 0) {
                    Location = null;
                    Location = new GeoLocationViewer.GeoMapLocation();
                    Location.Latitude = Latitude;
                    Location.Longitude = Longitude;
                    Location.Tooltip = m_objContact.first_name + " " + m_objContact.last_name;
                    m_GeoMapListContact.Add(Location);
                }
            }
        }

        /// <summary>
        /// Load geo map viewer contacts
        /// </summary>
        private void InitializeGeoMapContactView() {
            m_objGeoLocationViewerContact = null;
            m_objGeoLocationViewerContact = new GeoLocationViewer();
            m_objGeoLocationViewerContact.Dock = DockStyle.Fill;
            pcGeoMapContact.Controls.Clear();
            pcGeoMapContact.Controls.Add(m_objGeoLocationViewerContact);
        }

        /// <summary>
        /// Load company information on the tab
        /// </summary>
        private void LoadCompanyInformation() 
        {
            if (oAppointment.AccountId > 0)
            {
                m_objCompanyInformationForm.LoadCompanyInformation(oAppointment.AccountId);
                txtCompanySpecificRemarks.Text = m_objCompanyInformationForm.CompanyRemarks;
            }
            else
            {
                m_objCompanyInformationForm.ClearCompanyInformation();
                txtCompanySpecificRemarks.Text = "";
            }
        }

        /// <summary>
        /// Load contact information
        /// </summary>
        private void LoadContactInformation() 
        {
            //if (ContactId > 0)
            //{
            //    m_objContactInformationForm.LoadContactInformation(ContactId);
            //    txtContactRemarks.Text = m_objContactInformationForm.ContactRemarks;
            //}
            if (m_oContactView.SelectedContact != null)
            {
                m_objContactInformationForm.LoadContactInformation(m_oContactView.SelectedContact.id);
                txtContactRemarks.Text = m_objContactInformationForm.ContactRemarks;
            }
            else
            {
                m_objContactInformationForm.ClearContactInformation();
                txtContactRemarks.Text = "";
            }
        }

        /// <summary>
        /// Load collected data user control
        /// </summary>
        private void InitializeCollectedDataView() {
            m_objCollectedDataView = null;
            m_objCollectedDataView = new CollectedDataView();
            m_objCollectedDataView.Dock = DockStyle.Fill;
            pcCollectedData.Controls.Clear();
            pcCollectedData.Controls.Add(m_objCollectedDataView);
            m_objCollectedDataView.CollectedDataFilterByContact = true;
        }

        /// <summary>
        /// Load google search user control
        /// </summary>
        private void InitializeGoogleSearchView() {
            m_objGoogleSearchView = null;
            m_objGoogleSearchView = new GoogleSearchView();
            m_objGoogleSearchView.Dock = DockStyle.Fill;
            pcGoogleSearch.Controls.Clear();
            pcGoogleSearch.Controls.Add(m_objGoogleSearchView);
        }

        /// <summary>
        /// Load company website user control
        /// </summary>
        private void InitializeCompanyWebsiteView() {
            m_objCompanyWebsiteView = null;
            m_objCompanyWebsiteView = new CompanyWebsiteView();
            m_objCompanyWebsiteView.Dock = DockStyle.Fill;
            pcCompanyWebsite.Controls.Clear();
            pcCompanyWebsite.Controls.Add(m_objCompanyWebsiteView);
        }

        /// <summary>
        /// Load standard questions information user control
        /// </summary>
        private void InitializeStandardQuestionView() {
            m_objStandardQuestionView = null;
            m_objStandardQuestionView = new StandardQuestionView();
            m_objStandardQuestionView.Dock = DockStyle.Fill;
            pcStandardQuestion.Controls.Clear();
            pcStandardQuestion.Controls.Add(m_objStandardQuestionView);
        }

        /// <summary>
        /// Load company information user control
        /// </summary>
        private void InitializeCompanyInformationForm() 
        {
            m_DatabaseModel = new BrightPlatformEntities(UserSession.EntityConnection);
            m_objCompanyInformationForm = null;
            m_objCompanyInformationForm = new CompanyInformation(m_DatabaseModel);
            m_objCompanyInformationForm.Dock = DockStyle.Fill;
            pcCompanyInformation.Controls.Clear();
            pcCompanyInformation.Controls.Add(m_objCompanyInformationForm);
        }

        /// <summary>
        /// Load contact information user control
        /// </summary>
        private void InitializeContactInformationForm() 
        {
            m_objContactInformationForm = null;
            m_objContactInformationForm = new ContactInformation();
            m_objContactInformationForm.Dock = DockStyle.Fill;
            pcContactInformation.Controls.Clear();
            pcContactInformation.Controls.Add(m_objContactInformationForm);
        }

        /// <summary>
        /// Initialize my sales script view
        /// </summary>
        private void InitializeMySalesScriptView() 
        {
            m_DoneLoadingMySalesScript = false;
            m_objMySalesScriptForm = null;
            m_objMySalesScriptForm = new SalesScriptView(oAppointment.FinalListId);
            m_objMySalesScriptForm.Dock = DockStyle.Fill;
            pcMySalesScript.Controls.Clear();
            pcMySalesScript.Controls.Add(m_objMySalesScriptForm);
            m_DoneLoadingMySalesScript = true;
        }

        /// <summary>
        /// Load the selected tab  
        /// </summary>
        /// <param name="pageName"></param>
        private void LoadSelectedPage(string pageName) {
            switch (pageName) {
                case "lcgBVSalesScript":
                    docBvSalesScript.LoadDocument(Application.StartupPath + @"\docs_temp\bv_script.doc", DocumentFormat.WordML);
                    break;

                case "lcgStandardQuestions":
                    if (m_objStandardQuestionView.StandardQuestionData == null && m_oContactView.SelectedContact != null) {
                        WaitDialog.CreateWaitDialog("Loading standard questions.");
                        m_objStandardQuestionView.LoadStandardQuestions(oAppointment.SubCampaignId, oAppointment.AccountId, m_oContactView.SelectedContact.id);
                        WaitDialog.CloseWaitDialog();
                    }
                    break;

                case "lcgCollectedData":
                    if (m_objCollectedDataView.CollectedDataResult == null) {
                        WaitDialog.CreateWaitDialog("Loading collected data.");
                        m_objCollectedDataView.SubCampaignId = oAppointment.SubCampaignId;
                        m_objCollectedDataView.AccountId = oAppointment.AccountId;
                        if (m_oContactView.SelectedContact == null)
                        {
                            m_objCollectedDataView.ContactId = null;
                            m_objCollectedDataView.LoadCollectedData();
                        }
                        else
                        {
                            m_objCollectedDataView.ContactId = m_oContactView.SelectedContact.id;
                            m_objCollectedDataView.LoadCollectedData();
                        }
                        //this.LoadCollectedData(oAppointment.SubCampaignId, oAppointment.AccountId);
                        m_objCollectedDataView.CollectedDataFilterByContact = true;
                        //m_objCollectedDataView.cbxBrightvisionOwned.Checked = true;
                        //m_objCollectedDataView.cbxCustomerOwned.Checked = true;

                        if (m_oContactView.SelectedContact != null) {
                            m_objCollectedDataView.ContactId = m_oContactView.SelectedContact.id;
                            m_objCollectedDataView.FilterCollectedData();
                        }
                        //this.FilterCollectedData();
                        WaitDialog.CloseWaitDialog();
                    }
                    break;

                case "lcgCompanyWebsite":
                    m_objCompanyWebsiteView.ClearBrowser();
                    if (oAppointment != null)
                        if (!string.IsNullOrEmpty(oAppointment.CompanyWebsite))
                            m_objCompanyWebsiteView.LoadCompanyWebsite(oAppointment.CompanyWebsite);
                    break;

                case "lcgMapContacts":
                    if (m_oContactView.SelectedContact != null) 
                    {
                        m_DoneLoadingContactMap = false;
                        this.LoadContactGeoLocations();
                        if (m_GeoMapListContact.Count > 0)
                        {
                            m_objGeoLocationViewerContact.SetGeoMapLocation(m_GeoMapListContact);
                            m_objGeoLocationViewerContact.ShowLocations();
                        }
                        m_DoneLoadingContactMap = true;
                    }
                    break;

                case "lcgGoogleSearch":
                    m_objGoogleSearchView.LoadGoogleSearch();
                    break;

                case "lcgMySalesScript":
                    WaitDialog objDialog = new WaitDialog();
                    objDialog.CreateNonStaticWaitDialog("Loading sales script.");
                    if (!m_DoneLoadingMySalesScript)
                        this.InitializeMySalesScriptView();
                    m_objMySalesScriptForm.LoadUserDocument();
                    objDialog.CloseNonStaticWaitDialog();
                    break;

                case "lcgContactInformation":
                    WaitDialog.CreateWaitDialog("Loading contact information.");
                    this.LoadContactInformation();
                    WaitDialog.CloseWaitDialog();
                    break;
            }
        }

        /// <summary>
        /// Load custom events to this current module
        /// </summary>
        private void LoadCustomEvents() {
            m_oContactView.gvContact.FocusedRowChanged += new FocusedRowChangedEventHandler(m_oContactView_gvContact_FocusedRowChanged);
            m_oContactView.gvContact.DataSourceChanged += new EventHandler(m_oContactView_DataSourceChanged);
        }

        /// <summary>
        /// Loads the contact remarks
        /// </summary>
        private void LoadCompanyRemark()
        {
            txtCompanyRemark.Text = ObjectCompany.GetCompanyRemarks(oAppointment.SubCampaignId, oAppointment.AccountId);
        }        

        /// <summary>
        /// Set default control values and bahaviours
        /// </summary>
        private void InitEventAndFollowUpObjects()
        {
            this.InitializeFollowUpView();
            txtLogFirstName.Properties.ReadOnly = true;
            txtLogLastname.Properties.ReadOnly = true;

            if (m_oContactView.SelectedContact != null)
            {
                lblContact.Text = "Contact: " + m_oContactView.SelectedContact.first_name + " " + m_oContactView.SelectedContact.last_name;
                txtLogFirstName.Text = m_oContactView.SelectedContact.first_name;
                txtLogLastname.Text = m_oContactView.SelectedContact.last_name;
                txtLogTitle.Text = m_oContactView.SelectedContact.title;
                cbLogCallStatus.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Method to enable disable contact transaction buttons
        /// </summary>
        private void SetContactViewButtons(bool IsEnabled)
        {
            btnAddContact.Enabled = true;
            btnEditContact.Enabled = IsEnabled;
            btnDeleteContact.Enabled = IsEnabled;
            cmdSaveContactInformation.Enabled = IsEnabled;
        }

        /// <summary>
        /// Enable / Disable view and delete dialog buttons
        /// </summary>
        /// <param name="IsEnabled"></param>
        private void SetDialogViewButtons(bool IsEnabled)
        {
            //btnDeleteDialog.Enabled = IsEnabled;
            btnEditDialog.Enabled = IsEnabled;
        }

        /// <summary>
        /// Set the button appearances as per company status
        /// </summary>
        private void SetSaveButtonAppearance()
        {
            cmdSaveExitAsOpen.StyleController = null;
            cmdSaveExitAsInProgress.StyleController = null;
            cmdSaveExitAsFollowUp.StyleController = null;
            cmdSaveExitAsFinished.StyleController = null;
            cmdSaveExitAsOpen.ButtonStyle = BorderStyles.Default;
            cmdSaveExitAsInProgress.ButtonStyle = BorderStyles.Default;
            cmdSaveExitAsFollowUp.ButtonStyle = BorderStyles.Default;
            cmdSaveExitAsFinished.ButtonStyle = BorderStyles.Default;
            cmdSaveExitAsOpen.Refresh();
            cmdSaveExitAsInProgress.Refresh();
            cmdSaveExitAsFollowUp.Refresh();
            cmdSaveExitAsFinished.Refresh();

            if (oAppointment.CompanyAppointmentStatus.Equals("Open") || string.IsNullOrEmpty(oAppointment.CompanyAppointmentStatus))
            {
                cmdSaveExitAsOpen.ButtonStyle = BorderStyles.Office2003;
                cmdSaveExitAsOpen.Refresh();
            }
            else if (oAppointment.CompanyAppointmentStatus.Equals("In Progress"))
            {
                cmdSaveExitAsInProgress.ButtonStyle = BorderStyles.Office2003;
                cmdSaveExitAsInProgress.Refresh();
            }
            else if (oAppointment.CompanyAppointmentStatus.Equals("Follow Up"))
            {
                cmdSaveExitAsFollowUp.ButtonStyle = BorderStyles.Office2003;
                cmdSaveExitAsFollowUp.Refresh();
            }
            else if (oAppointment.CompanyAppointmentStatus.Equals("Finished"))
            {
                cmdSaveExitAsFinished.ButtonStyle = BorderStyles.Office2003;
                cmdSaveExitAsFinished.Refresh();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Load Form_OnLoad() initializations
        /// </summary>
        public void OnLoadInitialization()
        {
            #region Toggle Buttons Enabled for Contacts grid
            m_oDialog = BPContext.dialogs.FirstOrDefault(p =>
                    p.subcampaign_id == oAppointment.SubCampaignId &&
                        //p.customers_id == oAppointment.CustomerId &&
                    p.is_active == true);
            if (m_oDialog == null)
            {
                //btnDeleteDialog.Enabled = false;
                btnEditDialog.Enabled = false;
            }
            else
            {
                //btnDeleteDialog.Enabled = true;
                btnEditDialog.Enabled = true;
            }
            #endregion

            #region Load Company Information in Group Tab
            this.LoadCompanyInformation();
            #endregion

            #region Load Contact Information in Group Tab
            this.LoadContactInformation();
            #endregion

            this.LoadCustomEvents();
            //comboBoxEditCompanyStatus.Text = oAppointment.CompanyAppointmentStatus;
            m_SelectedPageName = lcgContactInformation.Name;
            this.SetContactViewButtons(false);
            //btnDeleteDialog.Enabled = false;
            btnEditDialog.Enabled = false;
            btnCallDirect.Enabled = false;
            btnCallMobile.Enabled = false;
            btnHangUp.Enabled = false;
            btnCallNumber.Enabled = true;

            if (m_oContactView.SelectedContact != null)
            {
                this.InitializeFollowUpView();
                this.InitEventAndFollowUpObjects();
                ViewDialog();
                //btnDeleteDialog.Enabled = true;
                btnEditDialog.Enabled = true;

                if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.direct_phone))
                    btnCallDirect.Enabled = true;

                if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.mobile))
                    btnCallMobile.Enabled = true;

                this.SetContactViewButtons(true);
            }

            //if (oAppointment.CompanyAppointmentStatus.Equals("Open") || string.IsNullOrEmpty(oAppointment.CompanyAppointmentStatus))
            //{
            //    cmdSaveExitAsOpen.ButtonStyle = BorderStyles.Office2003;
            //    cmdSaveExitAsOpen.Refresh();
            //}
            //else if (oAppointment.CompanyAppointmentStatus.Equals("In Progress"))
            //{
            //    cmdSaveExitAsInProgress.ButtonStyle = BorderStyles.Office2003;
            //    cmdSaveExitAsInProgress.Refresh();
            //}
            //else if (oAppointment.CompanyAppointmentStatus.Equals("Follow Up"))
            //{
            //    cmdSaveExitAsFollowUp.ButtonStyle = BorderStyles.Office2003;
            //    cmdSaveExitAsFollowUp.Refresh();
            //}
            //else if (oAppointment.CompanyAppointmentStatus.Equals("Finished"))
            //{
            //    cmdSaveExitAsFinished.ButtonStyle = BorderStyles.Office2003;
            //    cmdSaveExitAsFinished.Refresh();
            //}

            // finally, set record navigation buttons and style appearance
            this.SetSaveButtonAppearance();
            //this.SetRecordNavigationButtons(false);

            //dateEditDateLog.DateTime = DateTime.Now;
            //timeEditStartTime.Time = DateTime.Now;
            //timeEditEndtime.Time = DateTime.Now;
        }

        /// <summary>
        /// Load the campaign booking data
        /// </summary>
        public void LoadCampaignBookingData()
        {
            this.InitializeFollowUpView();
            //this.InitEventAndFollowUpObjects();
            this.PopulateFollowUpAssignedToComboList();
            this.InitializeMySalesScriptView();
            this.InitializeContactInformationForm();
            this.InitializeCompanyInformationForm();
            this.InitializeStandardQuestionView();
            this.InitializeCompanyWebsiteView();
            this.InitializeGoogleSearchView();
            this.InitializeCollectedDataView();
            this.InitializeGeoMapContactView();

            this.PopulateAssignToComboList();
            this.PopulateEventFollowUpView();

            
            m_oContactView = null;
            m_oContactView = new ContactView(oAppointment);
            pnlContactView.Controls.Clear();
            pnlContactView.Controls.Add(m_oContactView);
            m_oContactView.Dock = DockStyle.Fill;
            m_oContactView.GridControlMouseDown += new MouseEventHandler(m_oContactView_GridControlMouseDown);
            m_oContactView.GridControlMouseUp += new MouseEventHandler(m_oContactView_GridControlMouseUp);
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);

            this.LoadCompanyRemark();
            lcgContactInformation.Selected = true;
        }

        /// <summary>
        /// Populate event and follow up logs view
        /// </summary>
        public void PopulateEventFollowUpView()
        {
            try
            {
                gcEventLog.DataSource = null;
                gcEventLog.DataSource = EventFollowUp.GetEventFollowUpLogs(oAppointment.SubCampaignId, oAppointment.AccountId);
            }
            catch (Exception e) { }
        }

        public void SetBreadCrumb(string text) {
            simpleLabelItemBreadCrumb.Text = text;
        }
        #endregion

        #region Custom Events
        private void m_oContactView_DataSourceChanged(object sender, EventArgs e)
        {
            this.InitEventAndFollowUpObjects();
            if (m_oContactView.SelectedContact == null)
            {
                this.SetContactViewButtons(false);
                this.SetDialogViewButtons(false);
            }
            else
            {
                this.SetContactViewButtons(true);
                this.SetDialogViewButtons(true);
            }
        }

        private void m_oContactView_gvContact_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            m_objStandardQuestionView.StandardQuestionData = null;
            m_objCollectedDataView.CollectedDataResult = null;

            #region Enable / Disable Buttons
            if (m_oContactView.SelectedContact == null)
            {
                this.SetContactViewButtons(false);
                this.SetDialogViewButtons(false);
            }
            else
            {
                this.SetContactViewButtons(true);
                this.SetDialogViewButtons(true);
            }
            #endregion

            #region Map Contact
            if (m_SelectedPageName == lcgCollectedData.Name && m_oContactView.SelectedContact != null)
                m_objCollectedDataView.ContactId = m_oContactView.SelectedContact.id;

            if (m_DoneLoadingContactMap) 
            {
                if (m_oContactView.SelectedContact != null) 
                {
                    if (m_oContactView.SelectedContact.latitude != 0 || m_oContactView.SelectedContact.longitude != 0) 
                    {
                        GeoLocationViewer.GeoMapLocation Location = new GeoLocationViewer.GeoMapLocation();
                        Location.Latitude = m_oContactView.SelectedContact.latitude;
                        Location.Longitude = m_oContactView.SelectedContact.longitude;
                        m_objGeoLocationViewerContact.LocationFocus(Location);
                    } 
                    else 
                    {
                        //MessageBox.Show("No coordinates available for this contact. Please check address.", m_MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        m_objGeoLocationViewerContact.ShowLocations();
                    }
                } 
                else
                    m_objGeoLocationViewerContact.ShowLocations();
            }

            if (!m_SelectedPageName.Equals(lcgMapContacts.Name))
                this.LoadSelectedPage(m_SelectedPageName);
            #endregion

            #region Follow-Up View & Call Log
            this.InitEventAndFollowUpObjects();
            #endregion

            #region Disable Call buttons
            btnCallBoard.Enabled = false;
            btnCallDirect.Enabled = false;
            btnCallMobile.Enabled = false;
            btnHangUp.Enabled = false;

            if (!string.IsNullOrEmpty(oAppointment.CompanyBoardNumber))
                btnCallBoard.Enabled = true;

            if (m_oContactView.SelectedContact != null)
            {
                if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.direct_phone))
                    btnCallDirect.Enabled = true;
                if (!string.IsNullOrEmpty(m_oContactView.SelectedContact.mobile))
                    btnCallMobile.Enabled = true;
            }
            #endregion

            #region Disable View and Delete buttons
            //btnDeleteDialog.Enabled = false;
            btnEditDialog.Enabled = false;

            if (m_oContactView.SelectedContact != null)
            {
                ViewDialog();
                //btnDeleteDialog.Enabled = true;
                btnEditDialog.Enabled = true;
            }
            #endregion
        }
        #endregion

        
    }
}