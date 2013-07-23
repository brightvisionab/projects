
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
using System.Reflection;

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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BrightVision.Common.Modules;
using BrightVision.Common.Business;
using SalesConsultant.Modules;
using SalesConsultant.Business;
using SalesConsultant.Forms;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using BrightVision.Common.Events.Core;
using SalesConsultant.Events;
using DevExpress.XtraEditors.Popup;
using DevExpress.Utils.Win;

namespace SalesConsultant.Modules 
{
    public partial class DialogEditor : DevExpress.XtraEditors.XtraUserControl 
    {
        #region Constructors
        public DialogEditor() 
        {
            InitializeComponent();
            EnableDialogManagerContactList = true;
            EnableDialogManagerStatus = false;
            EnableEditDialogButton = true;
            EnableDialogManagerSaveButton = false;
            EnableDialogManagerDeleteButton = true;
            ricbDialogStatus.Popup += ricbDialogStatus_Popup;
        }
        #endregion

        #region Public Properties
        public CampaignBookingProperty.ContactStatusProperty ContactStatus { get; set; }
        public CTScSubCampaignContactList SelectedContact { get; set; }
        public List<CTScSubCampaignContactList> SubCampaignContactList { get; set; }

        private int _AccountId = 0;
        public int AccountId {
            get { return _AccountId; }
            set { _AccountId = value; }
        }
        public int CampaignId { get; set; }
        public int SubCampaignId { get; set; }
        public int FinalListId { get; set; }
        public int DialogManagerContactSelected
        {
            get {
                if (bbiContactList.EditValue != null)
                    return (int)bbiContactList.EditValue;
                else return 0;
            }
            set { bbiContactList.EditValue = value; }
        }

        public bool HasDialogQuestionnaire { get; set; }
        public bool HasDialogAccountLevelQuestion { get; set; }
        
        public string BreadCrumbText { get; set; }
        public string DialogManagerStatusSelected {
            get {
                if(bbiDialogStatus.EditValue != null)
                    return (string) bbiDialogStatus.EditValue; 
                else return String.Empty;
            }
            set { bbiDialogStatus.EditValue = value; }
        }
        public string DialogStatus
        {
            get
            {
                return bbiDialogStatus.EditValue.ToString();
            }
        }

        public bool EnableDialogManager {
            get { return pnlDialogControls.Enabled; }
            set { pnlDialogControls.Enabled = value; }
        }
        public bool EnableDialogManagerContactList {            
            get { return !bbiContactList.Edit.ReadOnly; }
            set { bbiContactList.Edit.ReadOnly = !value; }
        }
        public bool EnableDialogManagerStatus {
            get { return !bbiDialogStatus.Edit.ReadOnly; }
            set { bbiDialogStatus.Edit.ReadOnly = !value; }
        }
        public bool EnableEditDialogButton {
            get { return bbiEditDialog.Enabled; }
            set { bbiEditDialog.Enabled = value; }
        }
        public bool EnableDialogManagerSaveButton {
            get { return bbiSaveDialog.Enabled; }
            set { bbiSaveDialog.Enabled = value; }
        }
        public bool EnableDialogManagerDeleteButton 
        {
            get { return bbiDeleteDialog.Enabled; }
            set { bbiDeleteDialog.Enabled = value; }
        }

        public bool OnEditMode {
            get {
                if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit)
                    return true;
                else
                    return false;
            }
        }
        public bool HasMustSaveDefaultValues {
            get { return m_HasMustSaveDefaultValues; }
        }
        #endregion

        #region Private Properties
        private class ObjectContactList {
            public int id { get; set; }
            public string display_name { get; set; }
        }

        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;

        private dialog m_oDialog = null;
        private Schedule objScheduleComponent = null;
        private FrmSchedulingPopup frmSchedulingPopup = null;
        private bool IsInitializingDialogComponents;
        private bool IsInitializedComponentsValid;
        private string m_CurrentStatus = string.Empty;
        private bool m_AllowEditValueChanged = false;
        private List<LayoutControlGroup> controlsInDialog = new List<LayoutControlGroup>();
        private bool m_HasMustSaveDefaultValues = false;
        private bool m_AnswerBindingOnProgress = false;

        private List<CampaignQuestionnaire> m_lstQuestionnaireDialog = null;
        private List<string> m_lstQuestionLayoutIds = null;
        #endregion

        #region Control Events
        private void ricbDialogStatus_Popup(object sender, EventArgs e)
        {
            PopupListBoxForm _ttcboContactStatus = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            _ttcboContactStatus.ListBox.MouseMove += _ttcboContactStatus_MouseMove;
            _ttcboContactStatus.ListBox.MouseLeave += _ttcboContactStatus_MouseLeave;
        }
        private void _ttcboContactStatus_MouseLeave(object sender, EventArgs e)
        {
            popupDetails.HideHint();
        }
        private void _ttcboContactStatus_MouseMove(object sender, MouseEventArgs e)
        {
            PopupListBox _lbxDialog = sender as PopupListBox;
            ComboBoxEdit _cboLeadStatus = _lbxDialog.OwnerEdit as ComboBoxEdit;
            int index = _lbxDialog.IndexFromPoint(new Point(e.X, e.Y));
            if (index == -1)
                popupDetails.HideHint();
            else {
                string _item = _cboLeadStatus.Properties.Items[index].ToString();
                XmlUtility.SubCampaignConfig _ConfigItem = m_BrightSalesProperty.CampaignBooking.ContactStatus.ContactStatuses.FirstOrDefault(i => i.status.Equals(_item));
                string _dlgPopup = string.Format("Lead Status: {0}{1}{1}{2}",
                    _ConfigItem.status,
                    Environment.NewLine,
                    _ConfigItem.description
                );
                popupDetails.ShowHint(_dlgPopup, _lbxDialog.PointToScreen(new Point(e.X, e.Y)));
            }
        }
        private void bbiContactList_EditValueChanged(object sender, EventArgs e)
        {
            m_EventBus.Notify(new DialogEditorEvents.OnContactDropdownChange() {
                ContactId = Convert.ToInt32(bbiContactList.EditValue)
            });
        }
        private void bbiEditDialog_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) 
        {
            if (m_oDialog == null)
                return;

            EnableEditDialogButton = false;
            EnableDialogManagerContactList = false;
            EnableDialogManagerStatus = true;
            EnableDialogManagerSaveButton = true;
            rilueContacts.ReadOnly = true;
            this.SetEditableComponent(true);
            m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Edit;
            int? _ContactId = null;
            if (SelectedContact != null && SelectedContact.id > 0)
                _ContactId = SelectedContact.id;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efDbContext.event_log.AddObject(
                    new event_log() {
                        event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.DIALOG_EVENT,
                        user_id = UserSession.CurrentUser.UserId,
                        subcampaign_id = SubCampaignId,
                        account_id = AccountId,
                        contact_id = _ContactId,
                        local_datetime = DateTime.Now,
                        computer_name = UserSession.CurrentUser.ComputerName,
                        param1 = "Edit Dialog",
                        param2 = "Success",
                        param3 = string.Format("Dialog Id: {0}", m_oDialog.id),
                        param4 = null,
                        param5 = null,
                        param6 = null
                    }
                );
                _efDbContext.SaveChanges();
            }
            m_EventBus.Notify(new DialogEditorEvents.OnEditDialog());
        }
        private void bbiSaveDialog_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) 
        {
            if (!HasDialogQuestionnaire) {
                m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
                EnableEditDialogButton = true;
                EnableDialogManagerSaveButton = false;
                EnableDialogManagerDeleteButton = true;
                //bbiDialogStatus.Edit.ReadOnly = false;
                rilueContacts.ReadOnly = false;

                /*
                 * https://brightvision.jira.com/browse/PLATFORM-3104
                 */
                m_EventBus.Notify(new DialogEditorEvents.OnDelete());
                return;
            }

            if (ContactStatus.ContactStatuses.Count < 1)
                NotificationDialog.Error("Bright Sales", "No values for dropdown defined. Please contact your project manager.");

            string _ContactStat = string.Empty;
            if (bbiDialogStatus.EditValue != null)
                _ContactStat = bbiDialogStatus.EditValue.ToString();

            if (ricbDialogStatus.Items.Count > 0 && bbiDialogStatus.EditValue == null || _ContactStat == "None") {
                NotificationDialog.Warning("Bright Sales", string.Format("No dialog status selected.{0}Please select a dialog status before saving.", Environment.NewLine));
                return;
            }

            WaitDialog.Show("Saving data ...");
            if (!SaveDialogAnswers(_ContactStat, true)) {
                int? _ContactId = null;
                if (SelectedContact != null && SelectedContact.id > 0)
                    _ContactId = SelectedContact.id;

                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    _efDbContext.event_log.AddObject(
                        new event_log() {
                            event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.DIALOG_EVENT,
                            user_id = UserSession.CurrentUser.UserId,
                            subcampaign_id = SubCampaignId,
                            account_id = AccountId,
                            contact_id = _ContactId,
                            local_datetime = DateTime.Now,
                            computer_name = UserSession.CurrentUser.ComputerName,
                            param1 = "Save Dialog",
                            param2 = "Failed",
                            param3 = string.Format("Dialog Id: {0}", m_oDialog.id),
                            param4 = null,
                            param5 = null,
                            param6 = null
                        }
                    );
                    _efDbContext.SaveChanges();
                }
                msgDialogStatus.Show(this.ParentForm, "Bright Sales", "Dialog saving failed.");
                return;
            }
            else {
                int? _ContactId = null;
                if (SelectedContact != null && SelectedContact.id > 0)
                    _ContactId = SelectedContact.id;

                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    _efDbContext.event_log.AddObject(
                        new event_log() {
                            event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.DIALOG_EVENT,
                            user_id = UserSession.CurrentUser.UserId,
                            subcampaign_id = SubCampaignId,
                            account_id = AccountId,
                            contact_id = _ContactId,
                            local_datetime = DateTime.Now,
                            computer_name = UserSession.CurrentUser.ComputerName,
                            param1 = "Save Dialog",
                            param2 = "Success",
                            param3 = string.Format("Dialog Id: {0}", m_oDialog.id),
                            param4 = null,
                            param5 = null,
                            param6 = null
                        }
                    );
                    _efDbContext.SaveChanges();
                }
                msgDialogStatus.Show(this.ParentForm, "Bright Sales", "Dialog successfully saved.");
                m_EventBus.Notify(new ManageCampaignBookingEvents.OnDialogEditorSaved());
            }

            m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
            EnableEditDialogButton = true;
            EnableDialogManagerSaveButton = false;
            EnableDialogManagerDeleteButton = true;
            //bbiDialogStatus.Edit.ReadOnly = true;
            rilueContacts.ReadOnly = false;
            WaitDialog.Close(true);
        }
        private void ___bbiDeleteDialog_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_oDialog == null)
                return;

            if (CampaignId <= 0 && AccountId <= 0 && FinalListId <= 0 && SubCampaignId <= 0)
                return;

            bool _editable = true;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                sub_campaign_account_lists _eftCurrentCompany = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.final_list_id == FinalListId && i.account_id == AccountId);
                if (_eftCurrentCompany != null)
                {
                    if (_eftCurrentCompany.locked_by != null && _eftCurrentCompany.locked_by != UserSession.CurrentUser.UserId)
                        _editable = false;

                    _efDbContext.Detach(_eftCurrentCompany);
                }
            }

            if (!_editable)
            {
                NotificationDialog.Information("Bright Sales", "Delete not allowed. Currently worked by another user.");
                return;
            }

            int? contact_id = SelectedContact == null || SelectedContact.id <= 0 ? (int?)null : SelectedContact.id;

            //if (MessageBox.Show("Are you sure to delete values in this dialog for this contact?", "System Information",
            //    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            DeleteDialog ddDiag = new DeleteDialog();
            PopupDialog wDialog = new PopupDialog();
            wDialog.Text = "Cancel or Delete";
            wDialog.ClientSize = new Size(ddDiag.Size.Width + 5, ddDiag.Size.Height + 5);
            ddDiag.Dock = DockStyle.Fill;
            wDialog.Controls.Clear();
            wDialog.Controls.Add(ddDiag);
            wDialog.MinimizeBox = false;
            wDialog.MaximizeBox = false;
            wDialog.CloseBox = true;
            wDialog.StartPosition = FormStartPosition.CenterScreen;
            wDialog.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            bool _IsCancelled = false;
            if (wDialog.ShowDialog() == DialogResult.OK)
            {
                //Cancel all changes made since last saved (reload all data);
                if (ddDiag.SelectedValue == 1)
                {
                    //m_OnPressCancel = true;
                    m_AnswerBindingOnProgress = true;
                    this.LoadDialogQuestionnaires();
                    m_AnswerBindingOnProgress = false;
                    int? _ContactId = null;
                    if (SelectedContact != null && SelectedContact.id > 0)
                        _ContactId = SelectedContact.id;

                    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
                    {
                        _efDbContext.event_log.AddObject(new event_log()
                        {
                            event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.DIALOG_EVENT,
                            user_id = UserSession.CurrentUser.UserId,
                            subcampaign_id = SubCampaignId,
                            account_id = AccountId,
                            contact_id = _ContactId,
                            local_datetime = DateTime.Now,
                            computer_name = UserSession.CurrentUser.ComputerName,
                            param1 = "Cancel Dialog",
                            param2 = "Success",
                            param3 = string.Format("Dialog Id: {0}", m_oDialog.id),
                            param4 = null,
                            param5 = null,
                            param6 = null
                        });

                        _efDbContext.SaveChanges();
                    }
                    msgDialogStatus.Show(this.ParentForm, "Bright Sales", "Dialog successfully cancelled.");
                    _IsCancelled = true;
                    m_EventBus.Notify(new DialogEditorEvents.OnDelete());
                }
                //Delete dialog contact question data (delete all stored data related to contact)
                else if (ddDiag.SelectedValue == 2)
                {
                    if (SelectedContact == null || SelectedContact.id < 1)
                    {
                        NotificationDialog.Information("Bright Sales", "No contact selected.");
                        return;
                    }

                    WaitDialog.Show(this.ParentForm, "Deleting...");
                    BusinessAnswer.DeleteAnswers(m_oDialog.id, contact_id, CampaignId, AccountId, false);
                    int? _ContactId = null;
                    if (SelectedContact != null && SelectedContact.id > 0)
                        _ContactId = SelectedContact.id;

                    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
                    {
                        _efDbContext.event_log.AddObject(
                            new event_log()
                            {
                                event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.DIALOG_EVENT,
                                user_id = UserSession.CurrentUser.UserId,
                                subcampaign_id = SubCampaignId,
                                account_id = AccountId,
                                contact_id = _ContactId,
                                local_datetime = DateTime.Now,
                                computer_name = UserSession.CurrentUser.ComputerName,
                                param1 = "Delete Contact Dialog",
                                param2 = "Success",
                                param3 = string.Format("Dialog Id: {0}", m_oDialog.id),
                                param4 = null,
                                param5 = null,
                                param6 = null
                            }
                        );
                        _efDbContext.SaveChanges();
                    }
                    msgDialogStatus.Show(this.ParentForm, "Bright Sales", "Dialog contacts successfully deleted.");
                    _IsCancelled = false;

                    DisposeGroupControls(layoutControlGroupQuestionnaire);
                    m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
                    m_EventBus.Notify(new DialogEditorEvents.AfterDelete()
                    {
                        IsCancelled = _IsCancelled
                    });

                    WaitDialog.Close();
                    this.LoadDialogQuestionnaires();
                }
                //Delete dialog company question data (delete all stored data related to company)
                else if (ddDiag.SelectedValue == 3)
                {
                    WaitDialog.Show(this.ParentForm, "Deleting...");
                    BusinessAnswer.DeleteAnswers(m_oDialog.id, contact_id, CampaignId, AccountId, true);
                    int? _ContactId = null;
                    if (SelectedContact != null && SelectedContact.id > 0)
                        _ContactId = SelectedContact.id;

                    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
                    {
                        _efDbContext.event_log.AddObject(
                            new event_log()
                            {
                                event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.DIALOG_EVENT,
                                user_id = UserSession.CurrentUser.UserId,
                                subcampaign_id = SubCampaignId,
                                account_id = AccountId,
                                contact_id = _ContactId,
                                local_datetime = DateTime.Now,
                                computer_name = UserSession.CurrentUser.ComputerName,
                                param1 = "Delete Account Dialog",
                                param2 = "Success",
                                param3 = string.Format("Dialog Id: {0}", m_oDialog.id),
                                param4 = null,
                                param5 = null,
                                param6 = null
                            }
                        );
                        _efDbContext.SaveChanges();
                    }
                    msgDialogStatus.Show(this.ParentForm, "Bright Sales", "Dialog accounts successfully deleted.");
                    _IsCancelled = false;

                    DisposeGroupControls(layoutControlGroupQuestionnaire);
                    m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
                    m_EventBus.Notify(new DialogEditorEvents.AfterDelete()
                    {
                        IsCancelled = _IsCancelled
                    });

                    WaitDialog.Close();
                    this.LoadDialogQuestionnaires();
                }



                //if (ddDiag.SelectedValue != 1) {
                //    DisposeGroupControls(layoutControlGroupQuestionnaire);
                //    m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
                //    m_EventBus.Notify(new DialogEditorEvents.AfterDelete() {
                //        IsCancelled = _IsCancelled
                //    });

                //    //DialogEditorAfterDeleteEventNotifier
                //    //if (OnDeleteCompleted != null)
                //    //    OnDeleteCompleted(this, _args);

                //    WaitDialog.Close();
                //    this.LoadDialogQuestionnaires();                    
                //}
                //else {
                //    m_EventBus.Notify(new DialogEditorEvents.OnDelete());

                //    //if (OnDeleleteToggleButtons != null)
                //    //    OnDeleleteToggleButtons(this, new EventArgs());
                //}
            }
        }
        private void bbiDeleteDialog_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) 
        {
            if (m_oDialog == null)
                return;

            if (CampaignId <= 0 && AccountId <= 0 && FinalListId <= 0 && SubCampaignId <= 0)
                return;

            bool _editable = true;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                sub_campaign_account_lists _eftCurrentCompany = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.final_list_id == FinalListId && i.account_id == AccountId);
                if (_eftCurrentCompany != null) {
                    if (_eftCurrentCompany.locked_by != null && _eftCurrentCompany.locked_by != UserSession.CurrentUser.UserId)
                        _editable = false;

                    _efDbContext.Detach(_eftCurrentCompany);
                }
            }

            if (!_editable) {
                NotificationDialog.Information("Bright Sales", "Delete not allowed. Currently worked by another user.");
                return;
            }

            int? contact_id = SelectedContact == null || SelectedContact.id <= 0 ? (int?)null : SelectedContact.id;
            DeleteDialog ddDiag = new DeleteDialog();
            PopupDialog wDialog = new PopupDialog();
            wDialog.Text = "Cancel or Delete";
            wDialog.ClientSize = new Size(ddDiag.Size.Width + 5, ddDiag.Size.Height + 5);
            ddDiag.Dock = DockStyle.Fill;
            wDialog.Controls.Clear();
            wDialog.Controls.Add(ddDiag);
            wDialog.MinimizeBox = false;
            wDialog.MaximizeBox = false;
            wDialog.CloseBox = true;
            wDialog.StartPosition = FormStartPosition.CenterScreen;
            wDialog.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            bool _IsCancelled = false;
            if (wDialog.ShowDialog() == DialogResult.OK) {
                //Cancel all changes made since last saved (reload all data);
                if (ddDiag.SelectedValue == 1) {
                    //m_OnPressCancel = true;
                    m_AnswerBindingOnProgress = true;
                    this.LoadDialogQuestionnaires();
                    m_AnswerBindingOnProgress = false;
                    int? _ContactId = null;
                    if (SelectedContact != null && SelectedContact.id > 0)
                        _ContactId = SelectedContact.id;

                    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                        _efDbContext.event_log.AddObject(new event_log() {
                            event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.DIALOG_EVENT,
                            user_id = UserSession.CurrentUser.UserId,
                            subcampaign_id = SubCampaignId,
                            account_id = AccountId,
                            contact_id = _ContactId,
                            local_datetime = DateTime.Now,
                            computer_name = UserSession.CurrentUser.ComputerName,
                            param1 = "Cancel Dialog",
                            param2 = "Success",
                            param3 = string.Format("Dialog Id: {0}", m_oDialog.id),
                            param4 = null,
                            param5 = null,
                            param6 = null
                        });

                        _efDbContext.SaveChanges();
                    }
                    msgDialogStatus.Show(this.ParentForm, "Bright Sales", "Dialog successfully cancelled.");
                    _IsCancelled = true;
                    m_EventBus.Notify(new DialogEditorEvents.OnDelete());
                }
                //Delete dialog contact question data (delete all stored data related to contact)
                else if (ddDiag.SelectedValue == 2) {
                    if (SelectedContact == null || SelectedContact.id < 1) {
                        NotificationDialog.Information("Bright Sales", "No contact selected.");
                        return;
                    }

                    WaitDialog.Show(this.ParentForm, "Deleting...");
                    BusinessAnswer.DeleteAnswers(m_oDialog.id, contact_id, CampaignId, AccountId, false);
                    int? _ContactId = null;
                    if (SelectedContact != null && SelectedContact.id > 0)
                        _ContactId = SelectedContact.id;

                    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                        _efDbContext.event_log.AddObject(
                            new event_log() {
                                event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.DIALOG_EVENT,
                                user_id = UserSession.CurrentUser.UserId,
                                subcampaign_id = SubCampaignId,
                                account_id = AccountId,
                                contact_id = _ContactId,
                                local_datetime = DateTime.Now,
                                computer_name = UserSession.CurrentUser.ComputerName,
                                param1 = "Delete Contact Dialog",
                                param2 = "Success",
                                param3 = string.Format("Dialog Id: {0}", m_oDialog.id),
                                param4 = null,
                                param5 = null,
                                param6 = null
                            }
                        );
                        _efDbContext.SaveChanges();
                    }
                    msgDialogStatus.Show(this.ParentForm, "Bright Sales", "Dialog contacts successfully deleted.");
                    _IsCancelled = false;

                    //DisposeGroupControls(layoutControlGroupQuestionnaire);
                    m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
                    m_EventBus.Notify(new DialogEditorEvents.AfterDelete() {
                        IsCancelled = _IsCancelled
                    });

                    WaitDialog.Close();
                    this.LoadDialogQuestionnaires();
                }
                //Delete dialog company question data (delete all stored data related to company)
                else if (ddDiag.SelectedValue == 3) {
                    WaitDialog.Show(this.ParentForm, "Deleting...");
                    BusinessAnswer.DeleteAnswers(m_oDialog.id, contact_id, CampaignId, AccountId, true);
                    int? _ContactId = null;
                    if (SelectedContact != null && SelectedContact.id > 0)
                        _ContactId = SelectedContact.id;

                    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                        _efDbContext.event_log.AddObject(
                            new event_log() {
                                event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.DIALOG_EVENT,
                                user_id = UserSession.CurrentUser.UserId,
                                subcampaign_id = SubCampaignId,
                                account_id = AccountId,
                                contact_id = _ContactId,
                                local_datetime = DateTime.Now,
                                computer_name = UserSession.CurrentUser.ComputerName,
                                param1 = "Delete Account Dialog",
                                param2 = "Success",
                                param3 = string.Format("Dialog Id: {0}", m_oDialog.id),
                                param4 = null,
                                param5 = null,
                                param6 = null
                            }
                        );
                        _efDbContext.SaveChanges();
                    }
                    msgDialogStatus.Show(this.ParentForm, "Bright Sales", "Dialog accounts successfully deleted.");
                    _IsCancelled = false;

                    //DisposeGroupControls(layoutControlGroupQuestionnaire);
                    m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
                    m_EventBus.Notify(new DialogEditorEvents.AfterDelete() {
                        IsCancelled = _IsCancelled
                    });

                    WaitDialog.Close();
                    this.LoadDialogQuestionnaires();
                }

                //if (ddDiag.SelectedValue != 1) {
                //    DisposeGroupControls(layoutControlGroupQuestionnaire);
                //    m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
                //    m_EventBus.Notify(new DialogEditorEvents.AfterDelete() {
                //        IsCancelled = _IsCancelled
                //    });

                //    //DialogEditorAfterDeleteEventNotifier
                //    //if (OnDeleteCompleted != null)
                //    //    OnDeleteCompleted(this, _args);
                
                //    WaitDialog.Close();
                //    this.LoadDialogQuestionnaires();                    
                //}
                //else {
                //    m_EventBus.Notify(new DialogEditorEvents.OnDelete());
                    
                //    //if (OnDeleleteToggleButtons != null)
                //    //    OnDeleleteToggleButtons(this, new EventArgs());
                //}
            }
        }
        private void bbiDialogRequired_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) 
        {
            this.ValidateDialog(true);
        }
        private void oSchedule_ShowCalendarBookingClick(object sender, EventArgs e) 
        {
            Schedule obj = sender as Schedule;
            if (obj == null)
                return;

            objScheduleComponent = obj;
            frmSchedulingPopup = new FrmSchedulingPopup();
            frmSchedulingPopup.SetBreadCrumb(BreadCrumbText);
            frmSchedulingPopup.CurrentScheduleID = obj.CurrentSelectedMeeting;
            
            if (SelectedContact != null)
                frmSchedulingPopup.CurrentContactID = SelectedContact.id;
            
            frmSchedulingPopup.CurrentAccountID = AccountId;
            frmSchedulingPopup.SubCampaignID = SubCampaignId;
            frmSchedulingPopup.ShowDialog();
            if (frmSchedulingPopup.CreatedScheduleID > 0) {
                obj.SetCreatedMeetingSchedule(frmSchedulingPopup.CreatedScheduleID);
                obj.Questionnaire.Form.Settings.DataBindings.schedule_id = frmSchedulingPopup.CreatedScheduleID.ToString();
            }
            obj.RefreshSchedMeetingList();
        }
        private void ricbDialogStatus_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            /**
             * https://brightvision.jira.com/browse/PLATFORM-3223
             * if currently on work mode by other user, do not allow
             * updating of contact status.
             */
            if (m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountLockedByOtherUser()) {
                NotificationDialog.Warning("Bright Sales", "Currently worked by another user.");
                e.Cancel = true;
                return;
            }

            /**
             * contact fields value checking as per defined
             * in the xml configuration per status item.
             */
            XmlUtility.SubCampaignConfig _Status = m_BrightSalesProperty.CampaignBooking.ContactStatus.ContactStatuses.FirstOrDefault(i => i.status == e.NewValue.ToString());
            if (_Status != null && !string.IsNullOrEmpty(_Status.field_checks)) {
                List<string> _lstFieldChecks = _Status.field_checks.Split(',').ToList();
                foreach (string _field in _lstFieldChecks) {
                    if (_field.Equals("first_name") && string.IsNullOrEmpty(m_BrightSalesProperty.CommonProperty.ContactPerson.first_name)) {
                        NotificationDialog.Warning("Bright Sales", "Contact first name is missing. Please kindly set value.");
                        e.Cancel = true;
                    }
                    else if (_field.Equals("last_name") && string.IsNullOrEmpty(m_BrightSalesProperty.CommonProperty.ContactPerson.last_name)) {
                        NotificationDialog.Warning("Bright Sales", "Contact last name is missing. Please kindly set value.");
                        e.Cancel = true;
                    }
                    else if (_field.Equals("email") && string.IsNullOrEmpty(m_BrightSalesProperty.CommonProperty.ContactPerson.email)) {
                        NotificationDialog.Warning("Bright Sales", "Contact email is missing. Please kindly set value.");
                        e.Cancel = true;
                    }
                    else if (_field.Equals("title") && string.IsNullOrEmpty(m_BrightSalesProperty.CommonProperty.ContactPerson.title)) {
                        NotificationDialog.Warning("Bright Sales", "Contact title is missing. Please kindly set value.");
                        e.Cancel = true;
                    }
                }
            }
        }
        private void bbiDialogStatus_EditValueChanged(object sender, EventArgs e)
        {
            if (ContactStatus.ContactStatuses.Count < 1)
                NotificationDialog.Information("Bright Sales", "No values for dropdown defined. Please contact your project manager.");
            
            string _ContactStat = string.Empty;
            if (bbiDialogStatus.EditValue != null)
                _ContactStat = bbiDialogStatus.EditValue.ToString();

            WaitDialog.Show("Saving changes ...");
            DialogEditorEvents.OnSaveCompletedArgs _args = new DialogEditorEvents.OnSaveCompletedArgs() {
                DialogId = m_oDialog.id,
                Status = string.IsNullOrEmpty(_ContactStat) ? "None" : _ContactStat,
                ContactId = SelectedContact != null ? SelectedContact.id : 0
            };

            if (m_CurrentStatus.ToLower() != _ContactStat.ToLower() && !string.IsNullOrEmpty(_ContactStat)) {
                m_CurrentStatus = _ContactStat;
                _args.EventLog = new DialogEditorEvents.EventLog() {
                    EventType = BrightVision.Common.Classes.EventLog.EventTypes.CHANGE_DIALOG_SAVE_EVENT,
                    Parameters = new string[] { 
                        m_oDialog.id.ToString(), 
                        m_CurrentStatus, 
                        string.IsNullOrEmpty(_ContactStat) ? "None" : _ContactStat 
                    }
                };
            }

            m_EventBus.Notify(new DialogEditorEvents.OnSaveCompleted() {
                OnSaveCompletedArgs = _args,
                TransactionSource = DialogEditorEvents.eTransactionSource.OnContactStatusChange
            });
            WaitDialog.Close();
            
            //if (!m_AnswerBindingOnProgress)
            //    m_EventBus.Notify(new DialogEditorEvents.OnAnswerChange());

            //if (bbiDialogStatus_OnEditValueChange != null && m_AllowEditValueChanged)
            //    bbiDialogStatus_OnEditValueChange();
        }
        private void msgDialogStatus_BeforeFormShow(object sender, DevExpress.XtraBars.Alerter.AlertFormEventArgs e)
        {
            DevExpress.Skins.Skin currentSkin;
            currentSkin = DevExpress.Skins.BarSkins.GetSkin(e.AlertForm.LookAndFeel);
            DevExpress.Skins.SkinElement element;
            element = currentSkin["AlertWindow"];
            Graphics g = Graphics.FromImage(element.Image.Image);
            g.FillRectangle(Brushes.PaleGreen, new Rectangle(0, 0, element.Image.Image.Width, element.Image.Image.Height));
        }
        #endregion

        #region Public Methods
        public void SuspendEventDialogStatusChange(bool pSuspendEvent)
        {
            if (pSuspendEvent)
                this.bbiDialogStatus.EditValueChanged -= new System.EventHandler(this.bbiDialogStatus_EditValueChanged);
            else
                this.bbiDialogStatus.EditValueChanged += new System.EventHandler(this.bbiDialogStatus_EditValueChanged);
        }
        public bool Save()
        {
            if (ContactStatus.ContactStatuses.Count < 1)
                NotificationDialog.Error("Bright Sales", "No values for dropdown defined. Please contact your project manager.");

            string _ContactStat = string.Empty;
            if (bbiDialogStatus.EditValue != null)
                _ContactStat = bbiDialogStatus.EditValue.ToString();

            if (!SaveDialogAnswers(_ContactStat, false))
                return false;

            m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
            EnableEditDialogButton = true;
            EnableDialogManagerSaveButton = false;
            EnableDialogManagerDeleteButton = true;
            //bbiDialogStatus.Edit.ReadOnly = true;
            rilueContacts.ReadOnly = false;
            return true;
        }
        public void EditDialog()
        {
            if (HasDialogQuestionnaire)
                this.bbiEditDialog_ItemClick(null, null);
        }
        public void ___BindDialogManagerData()
        {
            rilueContacts.DataSource = null;
            rilueContacts.Columns.Clear();
            if (SubCampaignContactList == null)
                return;

            var ds = SubCampaignContactList.Select(x => new ObjectContactList {
                id = x.id,
                display_name = string.Format("{0} {1}{2}", x.first_name, x.last_name, (!string.IsNullOrEmpty(x.title) ? ", " + x.title : ""))
            }).ToList();

            rilueContacts.ValueMember = "id";
            rilueContacts.DisplayMember = "display_name";
            rilueContacts.NullText = "";
            rilueContacts.ShowHeader = false;
            rilueContacts.DataSource = ds;
            rilueContacts.Columns.Clear();
            rilueContacts.Columns.Add(new LookUpColumnInfo("display_name", "name"));
            rilueContacts.TextEditStyle = TextEditStyles.DisableTextEditor;
            
            if (SelectedContact != null)
                DialogManagerContactSelected = SelectedContact.id;
            else
                bbiContactList.EditValue = null;

            if (ricbDialogStatus.Items != null)
                ricbDialogStatus.Items.Clear();

            ricbDialogStatus.TextEditStyle = TextEditStyles.DisableTextEditor;
            ricbDialogStatus.Items.AddRange(ContactStatus.ContactStatuses.ToArray());
            m_AllowEditValueChanged = false;

            this.SuspendEventDialogStatusChange(true);
            bbiDialogStatus.EditValue = null;
            if (SelectedContact != null && !string.IsNullOrEmpty(SelectedContact.status)) {
                /**
                 * [@jeff 06.21.2012]: https://brightvision.jira.com/browse/PLATFORM-1516
                 * added condition to check for default selected contact dialog status.
                 */
                if ((string.IsNullOrEmpty(SelectedContact.status) || SelectedContact.status.Equals("None")) && ricbDialogStatus.Items.Count > 0)
                    bbiDialogStatus.EditValue = ricbDialogStatus.Items[m_BrightSalesProperty.CampaignBooking.ContactStatus.ContactStatuses.FindIndex(i => i.selected)];
                else if (ContactStatus.ContactStatuses.FindIndex(i => i.status == SelectedContact.status) >= 0)
                    bbiDialogStatus.EditValue = SelectedContact.status;

                //if ((string.IsNullOrEmpty(SelectedContact.status) || SelectedContact.status.Equals("None")) && 
                //    ContactStatus.ContactStatusSelectedIndex >= 0 && 
                //    ricbDialogStatus.Items.Count > 0)
                //        bbiDialogStatus.EditValue = ricbDialogStatus.Items[ContactStatus.ContactStatusSelectedIndex];

                //else if (ContactStatus.ContactStatuses.IndexOf(SelectedContact.status) == -1)
                //    bbiDialogStatus.EditValue = null;

                //else
                //    bbiDialogStatus.EditValue = SelectedContact.status;
            }
            else {
                if (ricbDialogStatus.Items.Count > 0)
                    bbiDialogStatus.EditValue = ricbDialogStatus.Items[m_BrightSalesProperty.CampaignBooking.ContactStatus.ContactStatuses.FindIndex(i => i.selected)];

                //if (ricbDialogStatus.Items.Count > 0)
                //    bbiDialogStatus.EditValue = ricbDialogStatus.Items[ContactStatus.ContactStatusSelectedIndex];
                //else
                //    bbiDialogStatus.EditValue = null;
            }
            this.SuspendEventDialogStatusChange(false);

            //this.SuspendEventDialogStatusChange(false);

            //if (SelectedContact != null && !string.IsNullOrEmpty(SelectedContact.status)) {
            //    /**
            //     * [@jeff 06.21.2012]: https://brightvision.jira.com/browse/PLATFORM-1516
            //     * added condition to check for default selected contact dialog status.
            //     */
            //    if ((string.IsNullOrEmpty(SelectedContact.status) || SelectedContact.status.Equals("None")) && ContactStatus.ContactStatusSelectedIndex >= 0 && ricbDialogStatus.Items.Count > 0)
            //        bbiDialogStatus.EditValue = ricbDialogStatus.Items[ContactStatus.ContactStatusSelectedIndex];

            //    else if (ContactStatus.ContactStatuses.IndexOf(SelectedContact.status) == -1)
            //        bbiDialogStatus.EditValue = null;

            //    else
            //        bbiDialogStatus.EditValue = SelectedContact.status;
            //}
            //else
            //{
            //    if (ricbDialogStatus.Items.Count > 0)
            //        bbiDialogStatus.EditValue = ricbDialogStatus.Items[ContactStatus.ContactStatusSelectedIndex];

            //    else
            //        bbiDialogStatus.EditValue = null;
            //}

            m_AllowEditValueChanged = true;
        }
        public void ___ResetToDefaultState()
        {
            m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
            EnableDialogManagerContactList = true;
            //EnableDialogManagerStatus = false;
            //bbiDialogStatus.Edit.ReadOnly = true;
            bbiDialogComplete.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            bbiDialogRequired.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            this.DisposeGroupControls(layoutControlGroupQuestionnaire);
            rilueContacts.DataSource = null;
            rilueContacts.Columns.Clear();

            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse || !HasDialogQuestionnaire) {
                EnableEditDialogButton = false;
                EnableDialogManagerSaveButton = false;
                EnableDialogManagerDeleteButton = false;
                //EnableEditorControls(false);
            }
            else if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Work) {
                EnableEditDialogButton = true;
                EnableDialogManagerSaveButton = false;
                EnableDialogManagerDeleteButton = true;
                //EnableEditorControls(false);
            }
        }

        public void ResetToDefaultState()
        {
            m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
            EnableDialogManagerContactList = true;
            //EnableDialogManagerStatus = false;
            bbiDialogComplete.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            bbiDialogRequired.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;

            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse || !HasDialogQuestionnaire) {
                EnableEditDialogButton = false;
                EnableDialogManagerSaveButton = false;
                EnableDialogManagerDeleteButton = false;
            }
            else if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Work) {
                EnableEditDialogButton = true;
                EnableDialogManagerSaveButton = false;
                EnableDialogManagerDeleteButton = true;
            }
        }
        public void BindDialogManagerData(bool pReloadContacts = false)
        {
            if (pReloadContacts) {
                rilueContacts.DataSource = null;
                rilueContacts.Columns.Clear();
            }

            if (SubCampaignContactList == null)
                return;

            if (pReloadContacts) {
                var ds = SubCampaignContactList.Select(x => new ObjectContactList {
                    id = x.id,
                    display_name = string.Format("{0} {1}{2}", x.first_name, x.last_name, (!string.IsNullOrEmpty(x.title) ? ", " + x.title : ""))
                }).ToList();

                rilueContacts.ValueMember = "id";
                rilueContacts.DisplayMember = "display_name";
                rilueContacts.NullText = "";
                rilueContacts.ShowHeader = false;
                rilueContacts.DataSource = ds;
                rilueContacts.Columns.Clear();
                rilueContacts.Columns.Add(new LookUpColumnInfo("display_name", "name"));
                rilueContacts.TextEditStyle = TextEditStyles.DisableTextEditor;
            }

            if (SelectedContact != null)
                DialogManagerContactSelected = SelectedContact.id;
            else
                bbiContactList.EditValue = null;

            if (m_BrightSalesProperty.CampaignBooking.Questionnaire.State == SelectionProperty.DialogEditorState.Empty) {
                if (ricbDialogStatus.Items != null)
                    ricbDialogStatus.Items.Clear();

                List<string> _items = new List<string>();
                foreach (XmlUtility.SubCampaignConfig _item in ContactStatus.ContactStatuses)
                    _items.Add(_item.status);

                ricbDialogStatus.TextEditStyle = TextEditStyles.DisableTextEditor;
                ricbDialogStatus.Items.AddRange(_items);
                //ricbDialogStatus.Items.AddRange(ContactStatus.ContactStatuses.ToArray());
            }

            m_AllowEditValueChanged = false;
            this.SuspendEventDialogStatusChange(true);
            bbiDialogStatus.EditValue = null;
            int _selected = m_BrightSalesProperty.CampaignBooking.ContactStatus.ContactStatuses.FindIndex(i => i.selected);
            if (SelectedContact != null && !string.IsNullOrEmpty(SelectedContact.status)) {
                /**
                 * [@jeff 06.21.2012]: https://brightvision.jira.com/browse/PLATFORM-1516
                 * added condition to check for default selected contact dialog status.
                 */

                if ((string.IsNullOrEmpty(SelectedContact.status) || SelectedContact.status.Equals("None")) && ricbDialogStatus.Items.Count > 0)
                    bbiDialogStatus.EditValue = ricbDialogStatus.Items[_selected];
                else if (ContactStatus.ContactStatuses.FindIndex(i => i.status == SelectedContact.status) >= 0)
                    bbiDialogStatus.EditValue = SelectedContact.status;

                //if ((string.IsNullOrEmpty(SelectedContact.status) || SelectedContact.status.Equals("None")) &&
                //    ContactStatus.ContactStatusSelectedIndex >= 0 &&
                //    ricbDialogStatus.Items.Count > 0)
                //    bbiDialogStatus.EditValue = ricbDialogStatus.Items[ContactStatus.ContactStatusSelectedIndex];

                //else if (ContactStatus.ContactStatuses.IndexOf(SelectedContact.status) == -1)
                //    bbiDialogStatus.EditValue = null;
                //else
                //    bbiDialogStatus.EditValue = SelectedContact.status;
            }
            else {
                if (ricbDialogStatus.Items.Count > 0)
                    bbiDialogStatus.EditValue = ricbDialogStatus.Items[_selected];

                //if (ricbDialogStatus.Items.Count > 0)
                //    bbiDialogStatus.EditValue = ricbDialogStatus.Items[ContactStatus.ContactStatusSelectedIndex];
                //else
                //    bbiDialogStatus.EditValue = null;
            }

            this.SuspendEventDialogStatusChange(false);
            m_AllowEditValueChanged = true;
        }

        public void CloseDialogEditor()
        {
            m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode = SelectionProperty.DialogSaveMode.Unspecified;
            SetEditableComponent(false);
            EnableEditorControls(false);
        }
        public void EnableEditorControls(bool state)
        {
            //bbiEditDialog.Enabled = state;
            bbiSaveDialog.Enabled = state;
            //bbiDeleteDialog.Enabled = state;
            //btnArticleCompleted.Enabled = state;
            //btnInterviewCompleted.Enabled = state;
            //btnSaveAsInProgress.Enabled = state;
            //btnDeleteDialog.Enabled = state;  
        }
        public void ___LoadDialogQuestionnaires()
        {
            m_AnswerBindingOnProgress = true;
            m_HasMustSaveDefaultValues = false;
            this.ResetToDefaultState();
            this.BindDialogManagerData();

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                m_oDialog = _efDbContext.dialogs.FirstOrDefault(i => i.subcampaign_id == SubCampaignId && i.is_active == true);
                if (m_oDialog == null)
                {
                    bbiEditDialog.Enabled = false;
                    NotificationDialog.Warning("Bright Sales", "There is no current dialog created for this customer's subcampaign.");
                    return;
                }
                _efDbContext.Detach(m_oDialog);

                /**
                 * Populate each JSON questionnaire from dialog text to list object type.
                 */
                #region Code Logic
                var CQList = new List<CampaignQuestionnaire>();
                CampaignQuestionnaire oQuestionnaire = null;
                List<string> cbdList = new List<string>();
                DataBindings oBindings = null;
                if (!string.IsNullOrEmpty(m_oDialog.dialog_text_json))
                {
                    var jaDiag = JArray.Parse(m_oDialog.dialog_text_json);
                    jaDiag.ForEach(delegate(JToken jt)
                    {
                        /**
                         * [@jeff 06.08.2012]: https://brightvision.jira.com/browse/PLATFORM-1467
                         * added json converter to convert raw json to string, before unescaping.
                         */
                        string _jsonData = ValidationUtility.StripJsonInvalidChars(JsonConvert.ToString(jt.ToString(Formatting.None)).Unescape());
                        oQuestionnaire = CampaignQuestionnaire.InstanciateWith(_jsonData);
                        if (oQuestionnaire != null)
                        {
                            CQList.Add(oQuestionnaire);
                            oBindings = oQuestionnaire.Form.Settings.DataBindings;
                            if (oBindings != null)
                            {
                                if (!string.IsNullOrEmpty(oBindings.questionlayout_id))
                                    cbdList.Add(oBindings.questionlayout_id);
                            }
                        }
                    });
                }
                #endregion

                /**
                 * Populate Answers to each questionnaire.
                 */
                #region Code Logic
                int? campaign_id = CampaignId;
                int? account_id = AccountId;
                int? contact_id = SelectedContact == null || SelectedContact.id <= 0 ? (int?)null : SelectedContact.id;
                int? dialog_id = m_oDialog.id;
                List<int> answerIdList = new List<int>();

                //get all dialog answers based on questionlayout_ids and other params
                var listDialogAnswers = _efDbContext.FIGetDialogAnswers(
                    string.Join(",", cbdList.Distinct().ToArray()),
                    campaign_id,
                    account_id,
                    contact_id,
                    dialog_id

                ).ToList().Clone();


                /*
                 * DAN: FIX for issue:
                 * https://brightvision.jira.com/browse/PLATFORM-2948
                 * https://brightvision.jira.com/browse/PLATFORM-2952
                 */
                //------------------------------------------------------------------------------------------------
                ((System.ComponentModel.ISupportInitialize)(this.groupControlDialog)).BeginInit();
                this.groupControlDialog.SuspendLayout();
                ((System.ComponentModel.ISupportInitialize)(this.layoutControlDialog)).BeginInit();
                this.layoutControlDialog.SuspendLayout();
                ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup3)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup4)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.pnlDialogControls)).BeginInit();
                this.pnlDialogControls.SuspendLayout();
                ((System.ComponentModel.ISupportInitialize)(this.layoutControlMain)).BeginInit();
                this.layoutControlMain.SuspendLayout();
                this.SuspendLayout();
                //------------------------------------------------------------------------------------------------



                //layoutControlGroupQuestionnaire.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                //this.layoutControlQuestionnaire.Visible = false;
                int rowcount = CQList.Count;
                IsInitializingDialogComponents = true;
                IsInitializedComponentsValid = false;

                var a = from oquestion in CQList select oquestion.Form.Settings.DataBindings;
                CTDialogAnswers dlgAnswer = null;
                for (int i = 0; i < rowcount; ++i)
                {
                    oQuestionnaire = CQList[i];
                    oBindings = oQuestionnaire.Form.Settings.DataBindings;
                    if (oBindings.account_level)
                    {
                        dlgAnswer = listDialogAnswers.FirstOrDefault(p =>
                                    p.questionlayout_id == int.Parse(oBindings.questionlayout_id) &&
                                    p.campaign_id == campaign_id &&
                                    p.account_id == account_id &&
                                    (p.account_level == true) &&
                                    p.dialog_id == dialog_id);
                    }
                    else
                    {
                        dlgAnswer = listDialogAnswers.FirstOrDefault(p =>
                                    p.questionlayout_id == int.Parse(oBindings.questionlayout_id) &&
                                    p.campaign_id == campaign_id &&
                                    p.account_id == account_id &&
                                    (p.contact_id == contact_id && p.account_level == false) &&
                                    p.dialog_id == dialog_id);
                    }

                    if (dlgAnswer != null)
                    {
                        oBindings.answer_id = dlgAnswer.id.ToString();
                        oBindings.account_level = dlgAnswer.account_level;
                        oBindings.contact_id = contact_id.ToString();
                        answerIdList.Add(dlgAnswer.id);
                    }
                    else
                    {
                        m_HasMustSaveDefaultValues = true;
                        oBindings.language_code = "SE";
                        oBindings.account_id = account_id.ToString();
                        oBindings.campaign_id = campaign_id.ToString();
                        oBindings.contact_id = contact_id.ToString();
                        oBindings.dialog_id = dialog_id.ToString();
                    }
                }
                //BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
                var oQuestionsAnswers = _efDbContext.answers.Include("sub_answers").Where(ans => answerIdList.Contains(ans.id));
                //if (oQuestionsAnswers != null)
                //    _efDbContext.Detach(oQuestionsAnswers);

                for (int i = 0; i < rowcount; ++i)
                {
                    oQuestionnaire = CQList[i];
                    oBindings = oQuestionnaire.Form.Settings.DataBindings;



                    if (!oBindings.account_level && (contact_id == null || contact_id == 0))
                        continue;

                    dlgAnswer = null;
                    if (oBindings.answer_id != null)
                        if (oBindings.answer_id.Length > 0)
                            dlgAnswer = listDialogAnswers.FirstOrDefault(an => an.id == int.Parse(oBindings.answer_id));

                    switch (oQuestionnaire.Type.ToLower())
                    {
                        #region Drop Box
                        case QuestionTypeConstants.Dropbox:
                            {
                                Dropbox oDropbox = new Dropbox(this.layoutControlQuestionnaire)
                                {
                                    ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                                    DisableSelection = true,
                                    ToolTipController = defaultToolTipController1,
                                    IsInitializing = true,
                                    HasMustSaveDefaultValues = false,
                                    AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId
                                };

                                if (dlgAnswer != null)
                                {
                                    int ansnwerID = int.Parse(oBindings.answer_id.ToString());
                                    var answer = oQuestionsAnswers.FirstOrDefault(oa => oa.id == ansnwerID);
                                    oDropbox.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire, answer);
                                }
                                else
                                {
                                    oDropbox.Questionnaire = oQuestionnaire;
                                    oDropbox.HasMustSaveDefaultValues = true;
                                }

                                oDropbox.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                                oDropbox.BindControls();
                                if (oDropbox.ControlFooter != null && dlgAnswer != null)
                                    oDropbox.ControlFooter.SetSourceTooltip(dlgAnswer.source_contact, dlgAnswer.source_last_modified_by, dlgAnswer.source_last_modified_on.Value.ToString("yyyy-MM-dd"));

                                this.layoutControlGroupQuestionnaire.Add(oDropbox.ControlGroup);
                                controlsInDialog.Add(oDropbox);
                                oDropbox.IsInitializing = false;
                                break;
                            }
                        #endregion
                        #region Multiple Choice
                        case QuestionTypeConstants.MultipleChoice:
                            {
                                Multiplechoice oMultipleChoice = new Multiplechoice(this.layoutControlQuestionnaire)
                                {
                                    ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                                    DisableSelection = true,
                                    ToolTipController = defaultToolTipController1,
                                    IsInitializing = true,
                                    HasMustSaveDefaultValues = false,
                                    AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId
                                };

                                if (dlgAnswer != null)
                                {
                                    int ansnwerID = int.Parse(oBindings.answer_id.ToString());
                                    var answer = oQuestionsAnswers.FirstOrDefault(oa => oa.id == ansnwerID);
                                    oMultipleChoice.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire, answer);
                                }
                                else
                                {
                                    oMultipleChoice.Questionnaire = oQuestionnaire;
                                    oMultipleChoice.HasMustSaveDefaultValues = true;
                                }

                                oMultipleChoice.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                                oMultipleChoice.BindControls();
                                if (oMultipleChoice.ControlFooter != null && dlgAnswer != null)
                                    oMultipleChoice.ControlFooter.SetSourceTooltip(dlgAnswer.source_contact, dlgAnswer.source_last_modified_by, dlgAnswer.source_last_modified_on.Value.ToString("yyyy-MM-dd"));

                                this.layoutControlGroupQuestionnaire.Add(oMultipleChoice.ControlGroup);
                                controlsInDialog.Add(oMultipleChoice);
                                oMultipleChoice.IsInitializing = false;
                                break;
                            }
                        #endregion
                        #region Text Box
                        case QuestionTypeConstants.Textbox:
                            {
                                Textbox oTextbox = new Textbox(this.layoutControlQuestionnaire)
                                {
                                    ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                                    DisableSelection = true,
                                    ToolTipController = defaultToolTipController1,
                                    IsInitializing = true,
                                    HasMustSaveDefaultValues = false,
                                    AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId
                                };

                                if (dlgAnswer != null)
                                {
                                    int ansnwerID = int.Parse(oBindings.answer_id.ToString());
                                    var answer = oQuestionsAnswers.FirstOrDefault(oa => oa.id == ansnwerID);
                                    oTextbox.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire, answer);
                                }
                                else
                                {
                                    oTextbox.Questionnaire = oQuestionnaire;
                                    oTextbox.HasMustSaveDefaultValues = true;
                                }

                                oTextbox.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                                oTextbox.BindControls();
                                if (oTextbox.ControlFooter != null && dlgAnswer != null)
                                    oTextbox.ControlFooter.SetSourceTooltip(dlgAnswer.source_contact, dlgAnswer.source_last_modified_by, dlgAnswer.source_last_modified_on.Value.ToString("yyyy-MM-dd"));

                                this.layoutControlGroupQuestionnaire.Add(oTextbox.ControlGroup);
                                controlsInDialog.Add(oTextbox);
                                oTextbox.IsInitializing = false;
                                break;
                            }
                        #endregion
                        #region Schedule
                        case QuestionTypeConstants.Schedule:
                            {
                                Schedule oSchedule = new Schedule(this.layoutControlQuestionnaire)
                                {
                                    ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                                    IsInitializing = true,
                                    HasMustSaveDefaultValues = false,
                                    AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId
                                };

                                var result = SubCampaignContactList;
                                if (result != null)
                                    oSchedule.ContactList = result;

                                oSchedule.SubcampaignID = SubCampaignId;
                                oSchedule.DisableSelection = true;
                                oSchedule.ToolTipController = defaultToolTipController1;
                                if (SelectedContact != null)
                                    oSchedule.SetCurrentCaller(SelectedContact, AccountId);

                                if (dlgAnswer != null)
                                {
                                    int ansnwerID = int.Parse(oBindings.answer_id.ToString());
                                    var answer = oQuestionsAnswers.FirstOrDefault(oa => oa.id == ansnwerID);
                                    oSchedule.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire, answer);
                                }
                                else
                                {
                                    oSchedule.Questionnaire = oQuestionnaire;
                                    oSchedule.HasMustSaveDefaultValues = true;
                                }

                                oSchedule.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                                oSchedule.BindControls();
                                if (oSchedule.ControlFooter != null && dlgAnswer != null)
                                    oSchedule.ControlFooter.SetSourceTooltip(dlgAnswer.source_contact, dlgAnswer.source_last_modified_by, dlgAnswer.source_last_modified_on.Value.ToString("yyyy-MM-dd"));

                                oSchedule.ShowCalendarBookingClick += new EventHandler(oSchedule_ShowCalendarBookingClick);
                                this.layoutControlGroupQuestionnaire.Add(oSchedule.ControlGroup);
                                controlsInDialog.Add(oSchedule);
                                oSchedule.IsInitializing = false;
                                break;
                            }
                        #endregion
                        #region Smart Text
                        case QuestionTypeConstants.SmartText:
                            {
                                SmartText oSmartText = new SmartText(this.layoutControlQuestionnaire)
                                {
                                    ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                                    DisableSelection = true,
                                    ToolTipController = defaultToolTipController1,
                                    IsInitializing = true,
                                    HasMustSaveDefaultValues = false,
                                    AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId
                                };

                                if (dlgAnswer != null)
                                {
                                    int ansnwerID = int.Parse(oBindings.answer_id.ToString());
                                    var answerD = oQuestionsAnswers.FirstOrDefault(oa => oa.id == ansnwerID);
                                    oSmartText.Questionnaire = BusinessAnswer.BindAnswer(oQuestionnaire, answerD);
                                }
                                else
                                {
                                    oSmartText.Questionnaire = oQuestionnaire;
                                    oSmartText.HasMustSaveDefaultValues = true;
                                }

                                oSmartText.UserId = UserSession.CurrentUser.UserId;
                                oSmartText.UserName = UserSession.CurrentUser.UserFullName;

                                /**
                                 * [jeff 05.16.2012]: https://brightvision.jira.com/browse/PLATFORM-1413
                                 * added checking for null selected contact.
                                 */
                                if (SelectedContact == null)
                                    oSmartText.ContactName = "";
                                else
                                    oSmartText.ContactName = SelectedContact.first_name + ", " + SelectedContact.last_name;

                                if (UserSession.CurrentUser.IsSubCampaignManager || UserSession.CurrentUser.IsCampaignOwner)
                                    oSmartText.CanEditAllRows = true;
                                else
                                    oSmartText.CanEditAllRows = false;

                                oSmartText.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                                oSmartText.BindControls();
                                oSmartText.BindPropertyGrid();

                                if (oSmartText.ControlFooter != null && dlgAnswer != null)
                                    oSmartText.ControlFooter.SetSourceTooltip(dlgAnswer.source_contact, dlgAnswer.source_last_modified_by, dlgAnswer.source_last_modified_on.Value.ToString("yyyy-MM-dd"));

                                this.layoutControlGroupQuestionnaire.Add(oSmartText.ControlGroup);
                                controlsInDialog.Add(oSmartText);
                                oSmartText.IsInitializing = false;
                                break;
                            }
                        #endregion
                    }
                }
                #endregion
            }

            EmptySpaceItem emptySpaceItem1 = new EmptySpaceItem();
            emptySpaceItem1.CustomizationFormText = "emptySpaceItemBottom";
            emptySpaceItem1.Name = "emptySpaceItemBottom";
            emptySpaceItem1.ShowInCustomizationForm = false;
            emptySpaceItem1.Size = new System.Drawing.Size(0, 0);
            emptySpaceItem1.Text = "emptySpaceItemBottom";
            emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);

            this.layoutControlGroupQuestionnaire.AddItem(emptySpaceItem1);

            /*
                 * DAN: FIX for issue:
                 * https://brightvision.jira.com/browse/PLATFORM-2948
                 * https://brightvision.jira.com/browse/PLATFORM-2952
                 */
            //------------------------------------------------------------------------------------------------
            ((System.ComponentModel.ISupportInitialize)(this.groupControlDialog)).EndInit();
            this.groupControlDialog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlDialog)).EndInit();
            this.layoutControlDialog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlDialogControls)).EndInit();
            this.pnlDialogControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlMain)).EndInit();
            this.layoutControlMain.ResumeLayout(false);
            this.ResumeLayout(false);
            //------------------------------------------------------------------------------------------------


            this.SetEditableComponent(false);
            IsInitializedComponentsValid = this.ValidateDialog(false);


            //this.layoutControlGroupQuestionnaire.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            //this.layoutControlQuestionnaire.BestFit();
            //this.layoutControlQuestionnaire.Visible = true;
            //this.layoutControlQuestionnaire.Update();
            //this.layoutControlQuestionnaire.Size = new Size(this.layoutControlQuestionnaire.Size.Width + 1, this.layoutControlQuestionnaire.Size.Height);
            IsInitializingDialogComponents = false;
            m_AnswerBindingOnProgress = false;

            if (bbiDialogStatus.EditValue != null)
                m_CurrentStatus = bbiDialogStatus.EditValue.ToString();

            bbiEditDialog.Enabled = true;
            if (m_BrightSalesProperty.CommonProperty.CompanyLocked)
                bbiDeleteDialog.Enabled = false;
        }
        
        public void LoadDialogQuestionnaires(bool pDisposeQuestionnaire = false)
        {
            this.RunAssync(() => {
                bool _ReloadContacts = false;
                if (pDisposeQuestionnaire) {
                    m_BrightSalesProperty.CampaignBooking.Questionnaire.State = SelectionProperty.DialogEditorState.Empty;
                    this.DisposeGroupControls(layoutControlGroupQuestionnaire);
                    _ReloadContacts = true;
                }

                m_AnswerBindingOnProgress = true;
                m_HasMustSaveDefaultValues = false;
                this.ResetToDefaultState();
                this.BindDialogManagerData(_ReloadContacts); //marker...

                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {

                    /**
                     * get and set bindings.
                     */
                    m_oDialog = _efDbContext.dialogs.FirstOrDefault(i => i.subcampaign_id == SubCampaignId && i.is_active == true);
                    if (m_oDialog == null) {
                        bbiEditDialog.Enabled = false;
                        NotificationDialog.Warning("Bright Sales", "There is no current dialog created for this customer's subcampaign.");
                        return;
                    }
                    _efDbContext.Detach(m_oDialog);

                    /**
                     * Populate each JSON questionnaire from dialog text to list object type.
                     */
                    #region Code Logic
                    m_lstQuestionnaireDialog = new List<CampaignQuestionnaire>();
                    m_lstQuestionLayoutIds = new List<string>();
                    CampaignQuestionnaire _Questionnaire = null;
                    DataBindings _BindingData = null;
                    if (!string.IsNullOrEmpty(m_oDialog.dialog_text_json)) {
                        var jaDiag = JArray.Parse(m_oDialog.dialog_text_json);
                        jaDiag.ForEach(delegate(JToken jt) {
                            /**
                             * [@jeff 06.08.2012]: https://brightvision.jira.com/browse/PLATFORM-1467
                             * added json converter to convert raw json to string, before unescaping.
                             */
                            string _jsonData = ValidationUtility.StripJsonInvalidChars(JsonConvert.ToString(jt.ToString(Formatting.None)).Unescape());
                            _Questionnaire = CampaignQuestionnaire.InstanciateWith(_jsonData);
                            if (_Questionnaire != null) {
                                m_lstQuestionnaireDialog.Add(_Questionnaire);
                                _BindingData = _Questionnaire.Form.Settings.DataBindings;
                                if (_BindingData != null) {
                                    if (!string.IsNullOrEmpty(_BindingData.questionlayout_id))
                                        m_lstQuestionLayoutIds.Add(_BindingData.questionlayout_id);
                                }
                            }
                        });
                    }
                    #endregion

                    int? _CampaignId = CampaignId;
                    int? _AccountId = AccountId;
                    int? _ContactId = SelectedContact == null || SelectedContact.id <= 0 ? (int?)null : SelectedContact.id;
                    int? _DialogId = m_oDialog.id;

                    /**
                     * create dialog if not yet been initialized.
                     * we only create dialog once, for the succeeding loads to be faster.
                     */
                    if (m_BrightSalesProperty.CampaignBooking.Questionnaire.State == SelectionProperty.DialogEditorState.Empty) {
                        this.CreateQuestionnaire();
                        m_BrightSalesProperty.CampaignBooking.Questionnaire.State = SelectionProperty.DialogEditorState.Loaded;
                    }

                    /**
                     * get the list of answers.
                     */
                    #region Code Logic
                    var _lstAnswers = _efDbContext.FIGetDialogAnswers(
                        string.Join(",", m_lstQuestionLayoutIds.Distinct().ToArray()),
                        _CampaignId,
                        _AccountId,
                        _ContactId,
                        _DialogId

                    ).ToList().Clone();
                    IsInitializingDialogComponents = true;
                    IsInitializedComponentsValid = false;

                    List<int> _lstAnswerIds = new List<int>();
                    CTDialogAnswers _dlgAnswer = null;
                    for (int i = 0; i < m_lstQuestionnaireDialog.Count; ++i) {
                        _Questionnaire = m_lstQuestionnaireDialog[i];
                        _BindingData = _Questionnaire.Form.Settings.DataBindings;
                    
                        /**
                         * if questionnaire is contact level and there are no contacts, 
                         * just by pass saving.
                         */
                        if (!_BindingData.account_level && m_BrightSalesProperty.CampaignBooking.ContactCount < 1)
                            continue;

                        if (_BindingData.account_level) {
                            _dlgAnswer = _lstAnswers.FirstOrDefault(p =>
                                p.questionlayout_id == int.Parse(_BindingData.questionlayout_id) &&
                                p.campaign_id == _CampaignId &&
                                p.account_id == _AccountId &&
                                p.account_level == true &&
                                p.dialog_id == _DialogId
                            );
                        }
                        else {
                            _dlgAnswer = _lstAnswers.FirstOrDefault(p =>
                                p.questionlayout_id == int.Parse(_BindingData.questionlayout_id) &&
                                p.campaign_id == _CampaignId &&
                                p.account_id == _AccountId &&
                                p.contact_id == _ContactId &&
                                p.account_level == false &&
                                p.dialog_id == _DialogId
                            );
                        }

                        if (_dlgAnswer != null) {
                            _BindingData.answer_id = _dlgAnswer.id.ToString();
                            _BindingData.account_level = _dlgAnswer.account_level;
                            _BindingData.contact_id = _ContactId.ToString();
                            _lstAnswerIds.Add(_dlgAnswer.id);
                        }
                        else {
                            m_HasMustSaveDefaultValues = true;
                            _BindingData.language_code = "SE";
                            _BindingData.account_id = _AccountId.ToString();
                            _BindingData.campaign_id = _CampaignId.ToString();
                            _BindingData.contact_id = _ContactId.ToString();
                            _BindingData.dialog_id = _DialogId.ToString();
                        }

                        _Questionnaire = null;
                    }
                    #endregion

                    /**
                     * assign eash answers to its respective dialog component.
                     */
                    #region Code Logic
                    int _DialogCount = 0;
                    var _lstDialogAnswers = _efDbContext.answers.Include("sub_answers").Where(i => _lstAnswerIds.Contains(i.id));
                    IQuestionnaire _IQuestionnaire = null;
                    foreach (BaseLayoutItem item in this.layoutControlGroupQuestionnaire.Items) {
                        if (item.IsGroup) {
                            if (item.Tag != null) {
                                /**
                                 * initialize and set defaults.
                                 */
                                _IQuestionnaire = item.Tag as IQuestionnaire;
                                _IQuestionnaire.IsInitializing = false;
                                _IQuestionnaire.InQuestionnaire = false;
                                _IQuestionnaire.DisableSelection = true;
                                _IQuestionnaire.HasMustSaveDefaultValues = false;
                                _IQuestionnaire.AccountId = 0;
                                _IQuestionnaire.ContactId = 0;
                                _IQuestionnaire.ContactPerson = null;
                                _IQuestionnaire.UserId = 0;
                                _IQuestionnaire.UserName = string.Empty;
                                _IQuestionnaire.ContactList = new List<CTScSubCampaignContactList>();
                            
                                /**
                                 * check if questionnaire exists.
                                 */
                                _Questionnaire = m_lstQuestionnaireDialog[_DialogCount];
                                _BindingData = _Questionnaire.Form.Settings.DataBindings;

                                #region Code
                                //if (_IQuestionnaire.DefaultQuestionnaire.Form.Settings.DataBindings.account_level) {
                                //    _Questionnaire = m_lstQuestionnaireDialog.FirstOrDefault(i =>
                                //        i.Form.Settings.DataBindings.questionlayout_id == _IQuestionnaire.DefaultQuestionnaire.Form.Settings.DataBindings.questionlayout_id &&
                                //        i.Form.Settings.DataBindings.campaign_id == _CampaignId.ToString() &&
                                //        i.Form.Settings.DataBindings.account_id == _AccountId.ToString() &&
                                //        i.Form.Settings.DataBindings.account_level == true &&
                                //        i.Form.Settings.DataBindings.dialog_id == _DialogId.ToString()
                                //    );
                                //}
                                //else {
                                //    _Questionnaire = m_lstQuestionnaireDialog.FirstOrDefault(i =>
                                //        i.Form.Settings.DataBindings.questionlayout_id == _IQuestionnaire.DefaultQuestionnaire.Form.Settings.DataBindings.questionlayout_id &&
                                //        i.Form.Settings.DataBindings.campaign_id == _CampaignId.ToString() &&
                                //        i.Form.Settings.DataBindings.account_id == _AccountId.ToString() &&
                                //        i.Form.Settings.DataBindings.contact_id == _ContactId.ToString() &&
                                //        i.Form.Settings.DataBindings.account_level == true &&
                                //        i.Form.Settings.DataBindings.dialog_id == _DialogId.ToString()
                                //    );
                                //}

                                //if (_BindingData.account_level)
                                //{
                                //    _dlgAnswer = _lstAnswers.FirstOrDefault(p =>
                                //        p.questionlayout_id == int.Parse(_BindingData.questionlayout_id) &&
                                //        p.campaign_id == _CampaignId &&
                                //        p.account_id == _AccountId &&
                                //        p.account_level == true &&
                                //        p.dialog_id == _DialogId
                                //    );
                                //}
                                //else
                                //{
                                //    _dlgAnswer = _lstAnswers.FirstOrDefault(p =>
                                //        p.questionlayout_id == int.Parse(_BindingData.questionlayout_id) &&
                                //        p.campaign_id == _CampaignId &&
                                //        p.account_id == _AccountId &&
                                //        p.contact_id == _ContactId &&
                                //        p.account_level == false &&
                                //        p.dialog_id == _DialogId
                                //    );
                                //}

                                //_Questionnaire = m_lstQuestionnaireDialog.FirstOrDefault(i =>
                                //    i.Form.Settings.DataBindings.questionlayout_id == _IQuestionnaire.DefaultQuestionnaire.Form.Settings.DataBindings.questionlayout_id &&
                                //    i.Form.Settings.DataBindings.campaign_id == _CampaignId.ToString() &&
                                //    i.Form.Settings.DataBindings.account_id == _AccountId.ToString() &&
                                //    i.Form.Settings.DataBindings.contact_id == _ContactId.ToString() &&
                                //    i.Form.Settings.DataBindings.dialog_id == _DialogId.ToString()
                                //);

                                //if (_Questionnaire == null) {
                                //    _IQuestionnaire.ControlGroup.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                                //    _DialogCount++;
                                //    continue;
                                //}

                                //_BindingData = _IQuestionnaire.DefaultQuestionnaire.Form.Settings.DataBindings;
                                // = _IQuestionnaire.DefaultQuestionnaire.Form.Settings.DataBindings;
                                //if (!_BindingData.account_level && (_ContactId == null || _ContactId == 0)) {

                                //_BindingData = _Questionnaire.Form.Settings.DataBindings;
                                #endregion

                                if (!_BindingData.account_level && m_BrightSalesProperty.CampaignBooking.ContactCount < 1) {
                                    _IQuestionnaire.ControlGroup.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                                    _DialogCount++;
                                    continue;
                                }

                                _dlgAnswer = null;
                                if (!string.IsNullOrEmpty(_BindingData.answer_id))
                                    _dlgAnswer = _lstAnswers.FirstOrDefault(e => e.id == int.Parse(_BindingData.answer_id));

                                _IQuestionnaire.IsInitializing = true;
                                _IQuestionnaire.InQuestionnaire = true;
                                _IQuestionnaire.DisableSelection = true;
                                _IQuestionnaire.HasMustSaveDefaultValues = false;
                                _IQuestionnaire.AccountId = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId;
                                _IQuestionnaire.ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId;
                                _IQuestionnaire.ContactPerson = _ContactId == null ? null : (_ContactId > 0 ? SelectedContact : null);
                                _IQuestionnaire.UserId = UserSession.CurrentUser.UserId;
                                _IQuestionnaire.UserName = UserSession.CurrentUser.UserFullName;
                                _IQuestionnaire.ContactList = m_BrightSalesProperty.CampaignBooking.ContactList;

                                if (_IQuestionnaire.ControlGroup.Visibility == DevExpress.XtraLayout.Utils.LayoutVisibility.Never)
                                    _IQuestionnaire.ControlGroup.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;

                                if (_dlgAnswer != null) {
                                    int _AnswerId = Convert.ToInt32(_BindingData.answer_id);
                                    var _AnswerData = _lstDialogAnswers.FirstOrDefault(i => i.id == _AnswerId);
                                    _IQuestionnaire.Questionnaire = BusinessAnswer.SetQuestionnaireData(_Questionnaire, _AnswerData);
                                    //_IQuestionnaire.Questionnaire = BusinessAnswer.SetQuestionnaireData(_IQuestionnaire.DefaultQuestionnaire, _AnswerData);
                                }
                                else {
                                    _IQuestionnaire.Questionnaire = _Questionnaire; // _IQuestionnaire.DefaultQuestionnaire;
                                    _IQuestionnaire.HasMustSaveDefaultValues = true;
                                }

                                _IQuestionnaire.Render();
                                if (_IQuestionnaire.ControlFooter != null && _dlgAnswer != null)
                                    _IQuestionnaire.ControlFooter.SetSourceTooltip(_dlgAnswer.source_contact, _dlgAnswer.source_last_modified_by, _dlgAnswer.source_last_modified_on.Value.ToString("yyyy-MM-dd"));

                                _IQuestionnaire.IsInitializing = false;
                                _DialogCount++;
                            }
                        }
                    }
                    #endregion
                }

                this.SetEditableComponent(false);
                IsInitializedComponentsValid = this.ValidateDialog(false);
                IsInitializingDialogComponents = false;
                m_AnswerBindingOnProgress = false;

                if (bbiDialogStatus.EditValue != null)
                    m_CurrentStatus = bbiDialogStatus.EditValue.ToString();

                bbiEditDialog.Enabled = true;
                if (m_BrightSalesProperty.CommonProperty.CompanyLocked)
                    bbiDeleteDialog.Enabled = false;
            });
        }

        public void RefreshContactAttendies()
        {
            Schedule oSchedule = null;
            var objGroupItems = this.layoutControlGroupQuestionnaire.Items;
            foreach (BaseLayoutItem item in objGroupItems) {
                if (item.IsGroup) {
                    if (item.Tag != null) {
                        oSchedule = item.Tag as Schedule;
                        if (oSchedule != null && oSchedule.Questionnaire.Type.ToLower() == QuestionTypeConstants.Schedule)
                            oSchedule.RefreshContactAttendies();
                    }
                }
            }
        }
        public void SetEditableComponent(bool status)
        {
            IQuestionnaire iQuestion = null;
            var objGroupItems = this.layoutControlGroupQuestionnaire.Items;
            foreach (BaseLayoutItem item in objGroupItems) {
                if (item.IsGroup) {
                    if (item.Tag != null) {
                        iQuestion = item.Tag as IQuestionnaire;
                        iQuestion.ReadOnly = !status;
                    }
                }
            }
        }
        public void SetState(bool pState) 
        {
            //bbiContactList.Enabled = pState;
            bbiEditDialog.Enabled = pState;
            bbiSaveDialog.Enabled = !pState;
            bbiDeleteDialog.Enabled = pState;
            bbiDialogStatus.Edit.ReadOnly = !pState;
            bbiContactList.Edit.ReadOnly = !pState;
            //bbiDialogStatus.Enabled = pState;
        }
        #endregion

        #region Private Methods
        private void CreateQuestionnaire()
        {
            CampaignQuestionnaire _Questionnaire = null;
            DataBindings _BindingData = null;

            /**
             * initalize layout.
             */
            #region Code Logic
            /**
             * DAN: FIX for issue:
             * https://brightvision.jira.com/browse/PLATFORM-2948
             * https://brightvision.jira.com/browse/PLATFORM-2952
             */
            ((System.ComponentModel.ISupportInitialize)(this.groupControlDialog)).BeginInit();
            this.groupControlDialog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlDialog)).BeginInit();
            this.layoutControlDialog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlDialogControls)).BeginInit();
            this.pnlDialogControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlMain)).BeginInit();
            this.layoutControlMain.SuspendLayout();
            this.SuspendLayout();
            #endregion

            for (int i = 0; i < m_lstQuestionnaireDialog.Count; ++i) {
                /**
                 * set binding data defaults.
                 */
                #region Code Logic
                _Questionnaire = m_lstQuestionnaireDialog[i];
                _BindingData = _Questionnaire.Form.Settings.DataBindings;

                _BindingData.language_code = "SE";
                _BindingData.contact_id = null;
                _BindingData.same_contact_binding = false;
                _BindingData.account_id = m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId.ToString(); //_AccountId.ToString();
                _BindingData.campaign_id = m_BrightSalesProperty.CommonProperty.CampaignId.ToString(); //_CampaignId.ToString();
                _BindingData.dialog_id = m_oDialog.id.ToString();
                #endregion

                /**
                 * set questionnaire.
                 */
                switch (_Questionnaire.Type.ToLower()) {
                    #region Drop Box
                    case QuestionTypeConstants.Dropbox:
                        Dropbox _Dropbox = new Dropbox(this.layoutControlQuestionnaire) {
                            ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                            DisableSelection = true,
                            ToolTipController = defaultToolTipController1,
                            IsInitializing = true,
                            QuestionId = Convert.ToInt32(_BindingData.question_id),
                            QuestionLayoutId = Convert.ToInt32(_BindingData.questionlayout_id),
                            DefaultQuestionnaire = _Questionnaire,
                            Questionnaire = _Questionnaire,
                            InQuestionnaire = false
                        };

                        _Dropbox.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                        _Dropbox.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(_Dropbox.ControlGroup);
                        controlsInDialog.Add(_Dropbox);
                        _Dropbox.IsInitializing = false;
                        break;
                    #endregion
                    #region Multiple Choice
                    case QuestionTypeConstants.MultipleChoice:
                        Multiplechoice _MultipleChoice = new Multiplechoice(this.layoutControlQuestionnaire) {
                            ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                            DisableSelection = true,
                            ToolTipController = defaultToolTipController1,
                            IsInitializing = true,
                            QuestionId = Convert.ToInt32(_BindingData.question_id),
                            QuestionLayoutId = Convert.ToInt32(_BindingData.questionlayout_id),
                            DefaultQuestionnaire = _Questionnaire,
                            Questionnaire = _Questionnaire,
                            InQuestionnaire = false
                        };

                        _MultipleChoice.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                        _MultipleChoice.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(_MultipleChoice.ControlGroup);
                        controlsInDialog.Add(_MultipleChoice);
                        _MultipleChoice.IsInitializing = false;
                        break;
                    #endregion
                    #region Text Box
                    case QuestionTypeConstants.Textbox:
                        Textbox _Textbox = new Textbox(this.layoutControlQuestionnaire) {
                            ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                            DisableSelection = true,
                            ToolTipController = defaultToolTipController1,
                            IsInitializing = true,
                            QuestionId = Convert.ToInt32(_BindingData.question_id),
                            QuestionLayoutId = Convert.ToInt32(_BindingData.questionlayout_id),
                            DefaultQuestionnaire = _Questionnaire,
                            Questionnaire = _Questionnaire,
                            InQuestionnaire = false
                        };

                        _Textbox.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                        _Textbox.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(_Textbox.ControlGroup);
                        controlsInDialog.Add(_Textbox);
                        _Textbox.IsInitializing = false;
                        break;
                    #endregion
                    #region Schedule
                    case QuestionTypeConstants.Schedule:
                        Schedule _Schedule = new Schedule(this.layoutControlQuestionnaire) {
                            ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                            IsInitializing = true,
                            QuestionId = Convert.ToInt32(_BindingData.question_id),
                            QuestionLayoutId = Convert.ToInt32(_BindingData.questionlayout_id),
                            DefaultQuestionnaire = _Questionnaire,
                            Questionnaire = _Questionnaire,
                            InQuestionnaire = false
                        };

                        _Schedule.SubcampaignID = SubCampaignId;
                        _Schedule.DisableSelection = true;
                        _Schedule.ToolTipController = defaultToolTipController1;
                        _Schedule.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                        _Schedule.ShowCalendarBookingClick += new EventHandler(oSchedule_ShowCalendarBookingClick);
                        _Schedule.BindControls();
                        this.layoutControlGroupQuestionnaire.Add(_Schedule.ControlGroup);
                        controlsInDialog.Add(_Schedule);
                        _Schedule.IsInitializing = false;
                        break;
                    #endregion
                    #region Smart Text
                    case QuestionTypeConstants.SmartText:
                        SmartText _SmartText = new SmartText(this.layoutControlQuestionnaire) {
                            ContactId = m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId,
                            DisableSelection = true,
                            ToolTipController = defaultToolTipController1,
                            IsInitializing = true,
                            QuestionId = Convert.ToInt32(_BindingData.question_id),
                            QuestionLayoutId = Convert.ToInt32(_BindingData.questionlayout_id),
                            DefaultQuestionnaire = _Questionnaire,
                            Questionnaire = _Questionnaire,
                            InQuestionnaire = false
                        };

                        _SmartText.OnComponentNotifyChanged += new ComponentDialogNotifyChangedEventHandler(oComponents_OnComponentNotifyChanged);
                        _SmartText.BindControls();
                        _SmartText.BindPropertyGrid();
                        this.layoutControlGroupQuestionnaire.Add(_SmartText.ControlGroup);
                        controlsInDialog.Add(_SmartText);
                        _SmartText.IsInitializing = false;
                        break;
                    #endregion
                }
            }

            /**
             * resume layout.
             */
            #region Code Logic
            EmptySpaceItem emptySpaceItem1 = new EmptySpaceItem();
            emptySpaceItem1.CustomizationFormText = "emptySpaceItemBottom";
            emptySpaceItem1.Name = "emptySpaceItemBottom";
            emptySpaceItem1.ShowInCustomizationForm = false;
            emptySpaceItem1.Size = new System.Drawing.Size(0, 0);
            emptySpaceItem1.Text = "emptySpaceItemBottom";
            emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlGroupQuestionnaire.AddItem(emptySpaceItem1);
            //this.layoutControlGroupQuestionnaire = m_BrightSalesProperty.CampaignBooking.Questionnaire.QuestionnaireLayout;

            /**
             * DAN: FIX for issue:
             * https://brightvision.jira.com/browse/PLATFORM-2948
             * https://brightvision.jira.com/browse/PLATFORM-2952
             */
            ((System.ComponentModel.ISupportInitialize)(this.groupControlDialog)).EndInit();
            this.groupControlDialog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlDialog)).EndInit();
            this.layoutControlDialog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlDialogControls)).EndInit();
            this.pnlDialogControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlMain)).EndInit();
            this.layoutControlMain.ResumeLayout(false);
            this.ResumeLayout(false);
            #endregion
        }
        private void oComponents_OnComponentNotifyChanged(object sender, ComponentNotifyChangedArgs e) 
        {
            if (!e.IsLoaded || m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode != SelectionProperty.DialogSaveMode.Edit)
                return;

            bool isValid = IsInitializedComponentsValid;
            if (!IsInitializingDialogComponents) {
                isValid = ValidateDialog(false);
            } 

            if (isValid) {
                bbiDialogRequired.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                bbiDialogComplete.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            } 
            else {
                bbiDialogRequired.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                bbiDialogComplete.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }

            if (m_AnswerBindingOnProgress)
                return;

            m_EventBus.Notify(new DialogEditorEvents.OnAnswerChange());
        }
        private bool ValidateDialog(bool focus = false) 
        {
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
                            if (!valid && valid2) {
                                valid2 = false;
                                if(focus)
                                    layoutControlQuestionnaire.FocusHelper.PlaceItemIntoView(item);
                            }
                        }
                    }
                }
            }
            return valid2;
        }
        private void DisposeGroupControls(BaseLayoutItem Item) //Note: CloseDialogEditor is obsoleted. Use this method instead 
        {
            if (Item == null) return;
            var FlatList = new DevExpress.XtraLayout.Helpers.FlatItemsList();
            List<BaseLayoutItem> Items = FlatList.GetItemsList(Item);
            ILayoutControl Layout = Item.Owner;
            if (Layout != null)
            {
                Layout.BeginUpdate();
                BaseLayoutItem li;
                for (int i = Items.Count - 1; i >= 0; --i)
                {
                    li = Items[i];
                    if (!li.Equals(Item) && li.Name != Item.Name)
                    {
                        if (li is LayoutControlItem)
                        {
                            Control TempControl = (li as LayoutControlItem).Control;
                            if (TempControl != null)
                            {
                                TempControl.Dispose();
                            }

                            li.Dispose();
                            Items.RemoveAt(i);
                        }
                        else
                        {
                            li.Dispose();
                            Items.RemoveAt(i);
                        }
                    }
                }
                Layout.EndUpdate();
            }

            
            for (int cnt = 0; cnt < controlsInDialog.Count; cnt++) {
                controlsInDialog[0].Owner = null;
                controlsInDialog[cnt].Dispose();
            }
            controlsInDialog.Clear();
            var lcgItem = Item as LayoutControlGroup;
            lcgItem.Clear();
        }
        private bool SaveDialogAnswers(string dialogStatus, bool pOnPressButtonSave) 
        {
            /**
             * commented as per https://brightvision.jira.com/browse/PLATFORM-1358
             * but will be needed later.
             * this function is for the validation checking of the required fields.
             */
            /** /
            if (!ValidateDialog(true)) {
                MessageBox.Show("Please supply the required fields first before saving. Required fields are indicated in red background.",
                    "Save Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            /**/

            //if ((dialogStatus != null && dialogStatus.ToLower() != "none") ||
            //    (string.IsNullOrEmpty(dialogStatus))) {
            //if (SelectedContact != null && 
            //    ((dialogStatus != null && dialogStatus.ToLower() == "none") || (string.IsNullOrEmpty(dialogStatus))))
            //SavingEventArgs savingArgs = new SavingEventArgs{ Status=dialogStatus, Cancel = false};
            //if (OnSaving != null)
            //    OnSaving(this, ref savingArgs);

            //if (SelectedContact != null && ((dialogStatus != null && dialogStatus.ToLower() == "none")) || savingArgs.Cancel)
            //if (SelectedContact != null && ((dialogStatus != null && dialogStatus.ToLower() == "none"))) {
            //    MessageBox.Show("Please set status in dropdown for contact and company.", "Bright Sales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return false;
            //}

            //WaitDialog.Show(ParentForm, "Saving...");

            IQuestionnaire iQuestion = null;
            var objGroupItems = this.layoutControlGroupQuestionnaire.Items;
            foreach (BaseLayoutItem item in objGroupItems) {
                if (item.IsGroup) {
                    if (item.Tag != null) {
                        iQuestion = item.Tag as IQuestionnaire;

                        /**
                         * if questionnaire is contact level and there are no contacts, 
                         * just by pass saving.
                         */
                        if (!iQuestion.Questionnaire.Form.Settings.DataBindings.account_level && m_BrightSalesProperty.CampaignBooking.ContactCount < 1)
                            continue;
                        
                        iQuestion.Save();
                        if (!iQuestion.Questionnaire.Form.Settings.DataBindings.same_contact_binding && iQuestion.HasChanged)
                            BusinessAnswer.SaveAnswer(iQuestion.Questionnaire);
                        else if (pOnPressButtonSave && iQuestion.HasMustSaveDefaultValues)
                            BusinessAnswer.SaveAnswer(iQuestion.Questionnaire);

                        //if ((!iQuestion.Questionnaire.Form.Settings.DataBindings.same_contact_binding && iQuestion.HasChanged) || iQuestion.HasMustSaveDefaultValues)
                        //    BusinessAnswer.SaveAnswer(iQuestion.Questionnaire);

                        //if (!iQuestion.Questionnaire.Form.Settings.DataBindings.same_contact_binding && iQuestion.HasChanged)
                        //    BusinessAnswer.SaveAnswer(iQuestion.Questionnaire);
                    }
                }
            }

            DialogEditorEvents.OnSaveCompletedArgs _args = new DialogEditorEvents.OnSaveCompletedArgs() {
                DialogId = m_oDialog.id,
                Status = string.IsNullOrEmpty(dialogStatus) ? "None" : dialogStatus,
                ContactId = SelectedContact != null ? SelectedContact.id : 0
            };

            if (m_CurrentStatus.ToLower() != dialogStatus.ToLower() && !string.IsNullOrEmpty(dialogStatus)) {
                m_CurrentStatus = dialogStatus;
                _args.EventLog = new DialogEditorEvents.EventLog() {
                    EventType = BrightVision.Common.Classes.EventLog.EventTypes.CHANGE_DIALOG_SAVE_EVENT,
                    Parameters = new string[] { 
                        m_oDialog.id.ToString(), 
                        m_CurrentStatus, 
                        string.IsNullOrEmpty(dialogStatus) ? "None" : dialogStatus 
                    }
                };
            }

            m_EventBus.Notify(new DialogEditorEvents.OnSaveCompleted() {
                OnSaveCompletedArgs = _args,
                TransactionSource = DialogEditorEvents.eTransactionSource.OnSaveButtonClick
            });

            //this.RaiseSaveCompletedEvent(this, _args);

            //this.RaiseSaveCompletedEvent(this, new SaveCompletedEventArgs() {
            //    DialogID = m_oDialog.id,
            //    Status = dialogStatus,
            //    ContactId = SelectedContact.id
            //});
            //WaitDialog.Close();
            //***NOTE: Move this to the exposed handler OnSaveCompleted
            //string curStatus = string.Empty;
            //if (m_oContactView != null) {
            //    try {
            //        if (m_oContactView.SelectedContact != null)
            //            curStatus = m_oContactView.SelectedContact.status;
            //        m_oContactView.SaveSubCampaignContactAppointmentStatus(
            //            status,
            //            oAppointment.FinalListId,
            //            oAppointment.AccountId,
            //            oAppointment.SubCampaignId);
            //    } catch { }
            //    oMode = SaveMode.Unspecified;
            //    CloseDialogEditor(false);
            //}
            //WaitDialog.Close();

            //if (string.IsNullOrEmpty(m_CurrentStatus)) m_CurrentStatus = "None";
            //string newStatus = "None";
            //switch (status)
            //{
            //    case ContactView.eDialogStatus.InterviewCompleted: newStatus = "Interview Completed"; break;
            //    case ContactView.eDialogStatus.ArticleCompleted: newStatus = "Article Completed"; break;
            //    case ContactView.eDialogStatus.InProgress: newStatus = "In Progress"; break;
            //}
            //Log only if current status has changed to new status. Don't log if both are in the same status
            //if (m_CurrentStatus.ToLower() != dialogStatus.ToLower())
            //{
            //    LogAnEvent(
            //        BrightVision.EventLog.EventTypes.CHANGE_DIALOG_SAVE_EVENT,
            //        m_oDialog.id.ToString(),
            //        m_CurrentStatus,
            //        dialogStatus);
            //}
            return true;
        }
        private void RunAssync(Action pAction)
        {
            if (!IsHandleCreated)
                CreateHandle();

            this.Invoke(pAction);
        }
        #endregion

        #region Overrides
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Enter)) {
                this.bbiSaveDialog_ItemClick(null, null);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
    }
}
