
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.Objects;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Columns;
using DevExpress.Data;

using BrightVision.Common.Business;
using BrightVision.Common.UI;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Logging.Enums;
using SalesConsultant.Business;
using BrightVision.Logging;
using System.Data.SqlClient;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using SalesConsultant.Events;
using BrightVision.Common.Events.Core;

namespace SalesConsultant.Modules 
{
    public partial class ContactView : DevExpress.XtraEditors.XtraUserControl 
    {
        #region Enumerations
        /// <summary>
        /// Dialog status selector
        /// </summary>
        //public enum eDialogStatus
        //{
        //    None,
        //    InProgress,
        //    ArticleCompleted,
        //    InterviewCompleted
        //}
        #endregion

        #region Properties
        public int ContactId { get; set; }
        //public CTScSubCampaignContactList SelectedContact { get; set; }        
        public List<CTScSubCampaignContactList> ContactList { get; set; }
        public ManageCampaignBooking ParentModule { get; set; }
        public DialogEditor DialogEditorModule { get; set; }
        
        //public bool HasDialogQuestionnaire { get; set; }
        public int DialogContactId = 0;
        public int? CallLogViewContactId = 0;
        public int ExtraDetailContactId = 0;
        #endregion

        #region Private Members
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        //private SubCampaignAppointment m_objSubCampaignAppointment = null;        
        private bool m_bDataLoaded = false;
        private bool m_bReadonly = false;
        private RepositoryItemBVPopupContainerEdit repositoryItemComboBoxAuto = null;
        //private bool m_DataSourceChangeInitiated = false;
        private int m_ContactForUpdateRowHandle = 0;
        private bool m_OnFilterProcess = false;
        private bool m_HasCampaignListSelectedContact = false;
        private bool m_InGridClick = false;
        #endregion

        #region Instance of the form.
        private static ContactView instance = null;
        public static ContactView Instance(bool bolNew = false)
        {
            //if (bolNew && instance != null)
            //{
            //    instance.Dispose();
            //    instance = null;
            //}

            if (instance == null)
            {
                new ContactView();
            }
            return instance;
        }
        #endregion

        #region Constructors
        public ContactView() 
        {
            this.Visible = false;
            InitializeComponent();
            repositoryItemComboBoxAuto = new RepositoryItemBVPopupContainerEdit();
            //repositoryItemComboBoxAuto.Validating += new CancelEventHandler(repositoryItemComboBoxAuto_Validating);
            this.Visible = true;
            //gvContact.FocusedRowHandle = 1;
            instance = this;

            m_EventBus.GetEvent<DialogEditorEvents.OnContactDropdownChange>().Subscribe(SelectedDialogContactSetFocused);
        }

        //public ContactView(SubCampaignAppointment SubCampaignAppointment) {
        //    this.Visible = false;
        //    InitializeComponent();
        //    repositoryItemComboBoxAuto = new RepositoryItemBVPopupContainerEdit();
        //    repositoryItemComboBoxAuto.Validating += new CancelEventHandler(repositoryItemComboBoxAuto_Validating);
        //    BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        //    //m_objSubCampaignAppointment = SubCampaignAppointment;
        //    //if (SubCampaignAppointment != null) {
        //    //    this.PopulateContactView(m_objSubCampaignAppointment.SubCampaignId, m_objSubCampaignAppointment.AccountId, m_objSubCampaignAppointment.FinalListId);
        //    //}
        //    //btnEditDialog.Enabled = false; 
        //    this.Visible = true;

        //}
        #endregion

        #region Control Events
        private void ContactChanged()
        {
            this.SetFocusedRowInfo();
            if (m_BrightSalesProperty.CommonProperty.ContactPerson != null && !m_HasCampaignListSelectedContact) {
                ContactId = m_BrightSalesProperty.CommonProperty.ContactPerson.id;
                #region Log
                var log = BrightSalesFacade.Logger;
                log.SetLogField(LoggingField.contact_id, ContactId.ToString());
                log.SetLogField(LoggingField.contact_name, (string.Format("{0}, {1}", m_BrightSalesProperty.CommonProperty.ContactPerson.first_name, m_BrightSalesProperty.CommonProperty.ContactPerson.last_name)).LoggerString());
                #endregion
            }
        }

        private void cbxHideInactive_CheckedChanged(object sender, EventArgs e)
        {
            m_OnFilterProcess = true;
            this.ApplyFilters();
        }
        private void cbxHideNotInList_CheckedChanged(object sender, EventArgs e)
        {
            m_OnFilterProcess = true;
            this.ApplyFilters();
        }
        private void gvContact_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e) 
        {
            //if (m_OnFilterProcess)
            //    return;

            //if (m_SuspendEventFocusRowChange || m_OnFilterProcess)
            //    return;

            /**
             * boolean flag to know if this event was triggered thru mouse click event
             * or key up/down.
             */
            if (m_InGridClick)
                WaitDialog.Show("Loading data ...");

            if (DialogEditorModule != null) {
                if (m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit) {
                    NotificationDialog.Warning("Bright Sales", "Dialog on edit mode.");
                    this.gvContact.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
                    this.gvContact.FocusedRowHandle = e.PrevFocusedRowHandle;
                    this.gvContact.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
                    WaitDialog.Close();
                    return;
                }
            }
            
            if (gvContact.RowCount < 1) {
                gvContact.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
                WaitDialog.Close();
                return;
            }

            gvContact.CollapseAllDetails();
            this.ContactChanged();
            this.SetDefaultSelectedItem();
            m_EventBus.Notify(new ContactViewEvents.ContactChanged());

            if (m_InGridClick) {
                m_InGridClick = false;
                WaitDialog.Close();
            }

            //if (m_BrightSalesProperty.CommonProperty.ContactPerson == null) {
            //this.SetFocusedRowInfo();
            //if (m_BrightSalesProperty.CommonProperty.ContactPerson != null && !m_HasCampaignListSelectedContact) {
            //    ContactId = m_BrightSalesProperty.CommonProperty.ContactPerson.id;
            //    #region Log
            //    var log = BrightSalesFacade.Logger;
            //    log.SetLogField(LoggingField.contact_id, ContactId.ToString());
            //    log.SetLogField(LoggingField.contact_name, (string.Format("{0}, {1}", m_BrightSalesProperty.CommonProperty.ContactPerson.first_name, m_BrightSalesProperty.CommonProperty.ContactPerson.last_name)).LoggerString());
            //    #endregion
            //}
            //}

            //if (m_bDataLoaded)
            //    return;
            //this.SetDefaultSelectedItem();
        }
        private void gvContact_DataSourceChanged(object sender, EventArgs e) 
        {
            //if (m_SuspendEventDataSourceChange || m_OnFilterProcess)
            //    return;

            //this.gvContact.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
            this.SuspendEventFocusRowChange(true);
            if (gvContact.RowCount < 1) {
                gvContact.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
                this.gvContact.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
                return;
            }

            gvContact.FocusedRowHandle = 0;
            if (ContactId != null && ContactId > 0)
                this.ContactSetFocus(ContactId);

            this.gvContact_FocusedRowChanged(null, null);
            this.SuspendEventFocusRowChange(false);
            //this.gvContact.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);

            //if (m_OnFilterProcess)
            //    return;
            
            //this.gvContact.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
            //if (gvContact.RowCount < 1) {
            //    gvContact.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
            //    this.gvContact.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
            //    return;
            //}

            //gvContact.FocusedRowHandle = 0;
            //this.ContactChanged();
            //m_EventBus.Notify(new ContactViewEvents.ContactChanged());
            //this.gvContact.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
        }
        private void gvContact_MouseUp(object sender, MouseEventArgs e) {
            //if (m_bReadonly) {
            //    (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;
            //    return;
            //}
            //GridHitInfo _gvHitInfo = gvContact.CalcHitInfo(new Point(e.X, e.Y));
            //if (_gvHitInfo.InRowCell && gvContact.FocusedColumn.FieldName.Equals("active"))
            //{
                //if (Convert.ToBoolean(gvContact.GetRowCellValue(_gvHitInfo.RowHandle, "active")))
                //    gvContact.SetRowCellValue(_gvHitInfo.RowHandle, "active", false);
                //else
                //    gvContact.SetRowCellValue(_gvHitInfo.RowHandle, "active", true);
                //return;
            //}

            //if (GridControlMouseUp != null)
            //    GridControlMouseUp(sender, e);
        }
        private void gvContact_MouseDown(object sender, MouseEventArgs e) {
            //if (m_bReadonly) {
            //    (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;
            //    return;
            //}

            //if (GridControlMouseDown != null)
            //    GridControlMouseDown(sender, e);

            #region Contact De-activation Logic
            //GridHitInfo _gvHitInfo = gvContact.CalcHitInfo(new Point(e.X, e.Y));
            //if (_gvHitInfo.InRowCell && _gvHitInfo.Column.FieldName.Equals("active"))
            //{
            //    if (Convert.ToBoolean(gvContact.GetRowCellValue(_gvHitInfo.RowHandle, "active")))
            //    {
            //        string Message = "This is a global deletion from all campaigns." + Environment.NewLine + "Are you sure you want to de-activate this contact?";
            //        DialogResult _dlg = MessageBox.Show(Message, "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //        if (_dlg == DialogResult.No)
            //            return;

            //        gvContact.SetRowCellValue(_gvHitInfo.RowHandle, "active", false);
            //    }
            //    else
            //        gvContact.SetRowCellValue(_gvHitInfo.RowHandle, "active", true);

            //    m_ContactForUpdateRowHandle = _gvHitInfo.RowHandle;
            //    cbxActive_CheckedChanged(null, null);
            //}
            #endregion
        }
        private void gvContact_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (gvContact.RowCount < 1)
                return;

            CTScSubCampaignContactList _contact = gvContact.GetRow(e.ListSourceRowIndex) as CTScSubCampaignContactList;
            if (e.Column.FieldName == "contact_name")
                e.Value = string.Format("{0} {1}", _contact.first_name, _contact.last_name);

            else if (e.Column.FieldName == "title_role")
            {
                e.Value = string.Format("{0} {1}",
                    _contact.title,
                    string.IsNullOrEmpty(_contact.role_tags) ? string.Empty : string.Format("({0})", _contact.role_tags)
                );
            }
            else if (e.Column.FieldName == "rank") {
                int _RankTotal = 0;
                if (_contact.active)
                    _RankTotal += 10;
                if (Convert.ToBoolean(_contact.in_list))
                    _RankTotal += 1;
                if (!string.IsNullOrEmpty(_contact.status) && !_contact.status.Equals("None"))
                    _RankTotal += 3;
                if (_contact.call > 0)
                    _RankTotal += 2;
                if (!string.IsNullOrEmpty(_contact.last_contact.ToString())) {
                    TimeSpan _DateDifference = DateTime.Now - Convert.ToDateTime(_contact.last_contact);
                    if (_DateDifference.TotalDays > 90)
                        _RankTotal += 1;
                    if (ValidationUtility.NumberInBetween(Convert.ToInt32(_DateDifference.TotalDays), 31, 90))
                        _RankTotal += 2;
                    else if (ValidationUtility.NumberInBetween(Convert.ToInt32(_DateDifference.TotalDays), 8, 30))
                        _RankTotal += 3;
                    else if (ValidationUtility.NumberInBetween(Convert.ToInt32(_DateDifference.TotalDays), 0, 7))
                        _RankTotal += 4;
                }

                e.Value = _RankTotal;
            }
        }
        private void gvContact_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName == "linkedinicon")
            {
                CTScSubCampaignContactList row = gvContact.GetRow(e.RowHandle) as CTScSubCampaignContactList;
                if (row != null)
                {
                    if (ValidationUtility.IFNullString(row.linkedin_url, "") != "")
                    {
                        DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit _repLinkedInButton = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
                        _repLinkedInButton = repLinkedInButton.Clone() as DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit;
                        _repLinkedInButton.Buttons[0].ToolTip = row.linkedin_url;
                        e.RepositoryItem = _repLinkedInButton;
                        //((DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit)e.RepositoryItem).Buttons[0].ToolTip = row.linkedin_url;
                    }
                    else
                    {
                        e.RepositoryItem = repLinkedInButtonHidden;
                    }
                }
            }
        }
        private void cbxActive_CheckedChanged(object sender, EventArgs e)
        {
            CTScSubCampaignContactList _ContactForUpdate = gvContact.GetRow(m_ContactForUpdateRowHandle) as CTScSubCampaignContactList;
            if (sender != null)
            {
                CheckEdit _objCheckBox = sender as CheckEdit;
                if (!_objCheckBox.Checked)
                {
                    ObjectContact.DeActivateContact(_ContactForUpdate.id);
                    if (ActiveChanged != null)
                        ActiveChanged(false);
                }
                else
                {
                    ObjectContact.ActivateContact(_ContactForUpdate.id);
                    if (ActiveChanged != null)
                        ActiveChanged(true);
                }
            }
            //else
            //{
            //    if (!_ContactForUpdate.active)
            //        ObjectContact.DeActivateContact(_ContactForUpdate.id);
            //    else
            //        ObjectContact.ActivateContact(_ContactForUpdate.id);
            //}
        }
        private void cbxActive_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse) {
                e.Cancel = true;
                return;
            }

            m_ContactForUpdateRowHandle = gvContact.FocusedRowHandle;
            CheckEdit _objCheckBox = sender as CheckEdit;
            if (!_objCheckBox.Checked)
                return;

            bool _editable = true;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                final_lists _item = _efDbContext.final_lists.FirstOrDefault(i => i.sub_campaign_id == m_BrightSalesProperty.CommonProperty.SubCampaignId);
                if (_item == null)
                    _editable = false;
                else {
                    _efDbContext.Detach(_item);
                    sub_campaign_account_lists _eftCurrentCompany = _efDbContext.sub_campaign_account_lists.FirstOrDefault(
                        i => i.final_list_id == _item.id && i.account_id == m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId
                    );
                    if (_eftCurrentCompany != null) {
                        if (_eftCurrentCompany.locked_by != UserSession.CurrentUser.UserId)
                            _editable = false;

                        _efDbContext.Detach(_eftCurrentCompany);
                    }
                }
            }

            if (!_editable) {
                NotificationDialog.Information("Bright Sales", "Currently worked by another user.");
                return;
            }

            string Message = String.Format("This is a global deletion from all campaigns.{0}Are you sure you want to de-activate this contact?", Environment.NewLine);
            DialogResult _dlg = MessageBox.Show(Message, "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlg == DialogResult.No)
                e.Cancel = true;
            else
                e.Cancel = false;
        }
        private void gvContact_KeyDown(object sender, KeyEventArgs e) 
        {
            //if (m_bReadonly) {
            //    e.Handled = true;
            //    return;
            //}

            m_InGridClick = true;
            if (GridControlKeyDown != null)
                GridControlKeyDown(sender, e);
        }
        private void gvContact_KeyUp(object sender, KeyEventArgs e) 
        {
            //if (m_bReadonly) {
            //    e.Handled = true;
            //    return;
            //}

            m_InGridClick = true;
            if (GridControlKeyUp != null)
                GridControlKeyUp(sender, e);
        }
        private void gvContact_Click(object sender, EventArgs e)
        {
            GridHitInfo _hitInfo = gvContact.CalcHitInfo((e as MouseEventArgs).Location);
            if (_hitInfo.InRow)
                m_InGridClick = true;
        }
        #endregion

        #region Public Events & Delegates
        public delegate void PopulateContactViewInitiatedEventHandler(object sender, PopulateContactViewArgs e);
        public event PopulateContactViewInitiatedEventHandler PopulateContactView_Initiated;
        public class PopulateContactViewArgs : EventArgs {
            public List<CTScSubCampaignContactList> ContactPersons { get; set; }
            public int SelectedContactPersonIndex { get; set; }
        }

        //public delegate ManageCampaignBooking.eCampaignBookingMode GetCampaignBookingModeHandler();
        public delegate void ActiveChangedEventHander(bool IsActive);
        public delegate void ContactSavedEventHandler(object sender, ContactEventArgs e);
        //public event GetCampaignBookingModeHandler GetCampaignBookingMode;
        public event ActiveChangedEventHander ActiveChanged;
        public event ContactSavedEventHandler OnContactSaved;
        public event MouseEventHandler GridControlMouseUp;
        public event MouseEventHandler GridControlMouseDown;
        public event KeyEventHandler GridControlKeyUp;
        public event KeyEventHandler GridControlKeyDown; 
        #endregion

        #region Public Methods
        public void SuspendEventFocusRowChange(bool pSuspendEvent)
        {
            if (pSuspendEvent)
                this.gvContact.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
            else
                this.gvContact.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);

            //m_SuspendEventFocusRowChange = pSuspendEvent;
        }
        public void SuspendEventDataSourceChange(bool pSuspendEvent)
        {
            if (pSuspendEvent) {
                this.gvContact.DataSourceChanged -= new System.EventHandler(this.gvContact_DataSourceChanged);
                this.gvContact.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
            }
            else {
                this.gvContact.DataSourceChanged += new System.EventHandler(this.gvContact_DataSourceChanged);
                this.gvContact.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvContact_FocusedRowChanged);
                this.gvContact_FocusedRowChanged(null, null);
            }
        }
        /** /
        public void SetAppointmentArgs(SubCampaignAppointment pAppointment)
        {
            m_objSubCampaignAppointment = pAppointment;
        }
        /**/
        public void LoadContactData(bool pHasSelectedContact = false)
        {
            if (m_BrightSalesProperty.CampaignBooking.Appointment != null) {
                if (pHasSelectedContact)
                    m_HasCampaignListSelectedContact = true;

                this.PopulateContacts();
                m_HasCampaignListSelectedContact = false;
            }

            /** /
            //DialogEditorModule.EnableEditDialogButton = false;
            if (m_objSubCampaignAppointment != null) {
                if (pHasSelectedContact)
                    m_HasCampaignListSelectedContact = true;
                this.PopulateContactView(m_objSubCampaignAppointment.SubCampaignId, m_objSubCampaignAppointment.AccountId, m_objSubCampaignAppointment.FinalListId);
                m_HasCampaignListSelectedContact = false;
                //if (ContactList != null && ContactList.Count > 0 && HasDialogQuestionnaire)
                    //DialogEditorModule.EnableEditDialogButton = true;   
            }
            /**/
        }
        public void ResetToDefaulState() 
        {
            if(DialogEditorModule != null)
                DialogEditorModule.EnableEditDialogButton = false;

            gcContact.DataSource = null;
            ContactList = null;
            m_BrightSalesProperty.CommonProperty.ContactPerson = new CTScSubCampaignContactList();
            gvContact.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }
        public void PopulateContacts() 
        {
            List<CTScSubCampaignContactList> _lstContactPersons = new List<CTScSubCampaignContactList>();
            this.ResetToDefaulState();
            ContactList = ObjectSubCampaign.GetSubCampaignContacts(
                m_BrightSalesProperty.CommonProperty.SubCampaignId, 
                m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId, 
                m_BrightSalesProperty.CommonProperty.FinalListId
            );

            m_BrightSalesProperty.CampaignBooking.ContactList = new List<CTScSubCampaignContactList>();
            m_BrightSalesProperty.CampaignBooking.ContactCount = 0;

            if (ContactList.Count() > 0) {
                m_BrightSalesProperty.CampaignBooking.ContactCount = ContactList.Count();
                m_BrightSalesProperty.CampaignBooking.ContactList = ContactList;
            }

            /**
             * generate values for contact_name, title_role, and ranking.
             * the sort by ranking.
             */
            for (int i = 0; i < ContactList.Count(); i++) {
                if (ContactList[i].status.Equals("BrightVision.Common.Utilities.XmlUtility+SubCampaignConfig"))
                    ContactList[i].status = string.Empty;

                ContactList[i].contact_name = string.Format("{0} {1}", ContactList[i].first_name, ContactList[i].last_name);
                ContactList[i].title_role = string.Format("{0} {1}",
                    ContactList[i].title,
                    string.IsNullOrEmpty(ContactList[i].role_tags) ? string.Empty : string.Format("({0})", ContactList[i].role_tags)
                );

                int _RankTotal = 0;
                if (ContactList[i].active)
                    _RankTotal += 10;
                if (Convert.ToBoolean(ContactList[i].in_list))
                    _RankTotal += 1;
                if (!string.IsNullOrEmpty(ContactList[i].status) && !ContactList[i].status.Equals("None"))
                    _RankTotal += 3;
                if (ContactList[i].call > 0)
                    _RankTotal += 2;
                if (!string.IsNullOrEmpty(ContactList[i].last_contact.ToString())) {
                    TimeSpan _DateDifference = DateTime.Now - Convert.ToDateTime(ContactList[i].last_contact);
                    if (_DateDifference.TotalDays > 90)
                        _RankTotal += 1;
                    if (ValidationUtility.NumberInBetween(Convert.ToInt32(_DateDifference.TotalDays), 31, 90))
                        _RankTotal += 2;
                    else if (ValidationUtility.NumberInBetween(Convert.ToInt32(_DateDifference.TotalDays), 8, 30))
                        _RankTotal += 3;
                    else if (ValidationUtility.NumberInBetween(Convert.ToInt32(_DateDifference.TotalDays), 0, 7))
                        _RankTotal += 4;
                }

                ContactList[i].ranking = _RankTotal;
            }

            if (ContactList.Count < 1) {
                if (PopulateContactView_Initiated != null)
                    PopulateContactView_Initiated(this, new PopulateContactViewArgs() {
                        ContactPersons = _lstContactPersons,
                        SelectedContactPersonIndex = 0
                    });
                return;
            }

            if (DialogEditorModule != null)
                if (DialogEditorModule.HasDialogQuestionnaire)
                    DialogEditorModule.EnableEditDialogButton = true;

            gcContact.DataSource = ContactList;
            gvContact.BestFitColumns();
            gvContact.Columns[0].Width = 20;
            gvContact.Columns["title"].Width = 150;
            gvContact.Columns["role_tags"].Width = 150;
            gvContact.Columns["linkedin_url"].Width = 150;

            GridView view = gcContact.FocusedView as GridView;
            gvContact.SortInfo.ClearAndAddRange(new GridColumnSortInfo[] { 
                new GridColumnSortInfo(view.Columns["ranking"], ColumnSortOrder.Descending), 
                new GridColumnSortInfo(view.Columns["contact_name"], ColumnSortOrder.Ascending)
            });

            this.SetDefaultSelectedItem();
            DialogContactId = 0;

            /**
             * get the contact listing.
             */
            for (int i = 0; i < gvContact.RowCount; i++)
                _lstContactPersons.Add(gvContact.GetRow(i) as CTScSubCampaignContactList);

            if (PopulateContactView_Initiated != null)
                PopulateContactView_Initiated(this, new PopulateContactViewArgs() {
                    ContactPersons = _lstContactPersons,
                    SelectedContactPersonIndex = 0
                });
        }
        public void SaveSubCampaignContactAppointmentStatus(string dialogStatus, int FinalListId, int AccountId, int SubCampaignId) 
        {            
            ContactId = m_BrightSalesProperty.CommonProperty.ContactPerson.id;
            ObjectSubCampaign.SaveSubCampaignContactAppointmentStatus(m_BrightSalesProperty.CommonProperty.ContactPerson.id, FinalListId, dialogStatus);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "status", dialogStatus);
            //this.PopulateContactView(SubCampaignId, AccountId, FinalListId);
        }
        public void SetDefaultSelectedItem() 
        {
            if (ContactId < 1)
                return;

            if (ContactList == null || ContactList.Count < 1)
                return;

            m_bDataLoaded = false;
            //ParentModule.LoadingSelectedContact = false;
            var view = gvContact;
            CTScSubCampaignContactList oC;
            var ds = view.GridControl.DataSource as List<CTScSubCampaignContactList>;
            if (ds == null) return;
            if (DialogContactId > 0)
                ContactId = DialogContactId;
            else if (CallLogViewContactId != null)
                if (CallLogViewContactId > 0)
                    ContactId = (int)CallLogViewContactId;
            else if (ExtraDetailContactId > 0)
                ContactId = ExtraDetailContactId;
            var row = ds.FirstOrDefault(p => p.id == ContactId);
            if (row != null) {
                for (int i = 0; i < view.DataRowCount; i++) {
                    oC = view.GetRow(i) as CTScSubCampaignContactList;
                    if (oC != null && row.id == oC.id) {
                        view.FocusedRowHandle = i;
                        m_bDataLoaded = true;
                        break;
                    }
                }
            }

            this.SetFocusedRowInfo();
        }
        public void SetState() {
            m_bReadonly = m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse ? true : false;
            if (!m_bReadonly) {
                if (m_BrightSalesProperty.CommonProperty.ContactPerson != null) {
                    if (DialogEditorModule != null) {
                        if (DialogEditorModule.HasDialogQuestionnaire)
                            DialogEditorModule.EnableEditDialogButton = true;
                    }
                    lvContactEdit.OptionsBehavior.ReadOnly = false;
                }
            } else {
                if(DialogEditorModule != null)
                    DialogEditorModule.EnableEditDialogButton = false;
                lvContactEdit.OptionsBehavior.ReadOnly = true;
            }
        }
        public void DeActivateContact()
        {
            gvContact.SetFocusedRowCellValue("active", false);
        }
        public void SelectedDialogContactSetFocused(DialogEditorEvents.OnContactDropdownChange e)
        {
            if (gvContact.RowCount < 1)
                return;

            for (int i = 0; i < gvContact.RowCount; i++) {
                CTScSubCampaignContactList _item = gvContact.GetRow(i) as CTScSubCampaignContactList;
                if (_item.id == e.ContactId) {
                    gvContact.FocusedRowHandle = i;
                    break;
                }

            }
        }
        public void UpdateContact(CTContactDetails pContactPerson)
        {
            if ((pContactPerson.absence_start != null && pContactPerson.absence_end != null) &&
                (pContactPerson.absence_start <= DateTime.Now && pContactPerson.absence_end >= DateTime.Now))
                pContactPerson.is_absent = true;
            else
                pContactPerson.is_absent = false;

            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "first_name", pContactPerson.first_name);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "last_name", pContactPerson.last_name);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "title", pContactPerson.title);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "direct_phone", pContactPerson.direct_phone);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "mobile", pContactPerson.mobile);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "email", pContactPerson.email);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "has_contact_remarks", pContactPerson.has_contact_remarks);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "complete_address", pContactPerson.complete_address);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "role_tags", pContactPerson.role_tags);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "is_absent", pContactPerson.is_absent);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "country", pContactPerson.country);
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "linkedin_url", pContactPerson.linkedin_url);

            string address = "";
            if (ValidationUtility.IFNullString(pContactPerson.address_1,"") != "") address = pContactPerson.address_1;
            else if (ValidationUtility.IFNullString(pContactPerson.address_2, "") != "") address = pContactPerson.address_2;

            if (ValidationUtility.IFNullString(pContactPerson.city, "") != "") address += ", " + pContactPerson.city;
            if (ValidationUtility.IFNullString(pContactPerson.zipcode, "") != "") address += ", " + pContactPerson.zipcode;
            if (ValidationUtility.IFNullString(pContactPerson.country, "") != "") address += ", " + pContactPerson.country;

            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "complete_address", address);
            

            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "contact_name", string.Format("{0} {1}", pContactPerson.first_name, pContactPerson.last_name));
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "title_role", string.Format("{0} {1}",
                pContactPerson.title,
                string.IsNullOrEmpty(pContactPerson.role_tags) ? string.Empty : string.Format("({0})", pContactPerson.role_tags)
            ));
        }

        public void UpdateContactLinkedInURL(string LinkedInURL)
        {
            if (gvContact.FocusedRowHandle >= 0)
            {
                gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "linkedin_url", LinkedInURL);
            }
        }

        public List<CTScSubCampaignContactList> GetContactList()
        {
            List<CTScSubCampaignContactList> _lstContactPersons = new List<CTScSubCampaignContactList>();
            for (int i = 0; i < gvContact.RowCount; i++)
                _lstContactPersons.Add(gvContact.GetRow(i) as CTScSubCampaignContactList);

            return _lstContactPersons;
        }
        public void ContactSetFocus(int pContactId)
        {
            /**
             * sometimes the row count does not validate.
             * thats why we have to consider re-testing if
             * the grid is accessible or not.
             * if not, then we use the list<string> object
             * to browse thru all the subcampaign contacts.
             */

            if (m_BrightSalesProperty.CommonProperty.CallLogContactId > 0) pContactId = m_BrightSalesProperty.CommonProperty.CallLogContactId;
            if (gvContact.RowCount > 0 && ContactList.Count() > 0) {
                for (int i = 0; i < gvContact.RowCount; i++) {
                    CTScSubCampaignContactList _GridContact = gvContact.GetRow(i) as CTScSubCampaignContactList;
                    if (_GridContact.id == pContactId) {
                        gvContact.FocusedRowHandle = i;
                        break;
                    }
                }
            }
            else {
                for (int i = 0; i < ContactList.Count(); i++) {
                    if (ContactList[i].id == pContactId) {
                        gvContact.FocusedRowHandle = i;
                        break;
                    }
                }
            }

            m_BrightSalesProperty.CommonProperty.CallLogContactId = 0;
        }
        public void UpdateContactCallAttempts()
        {
           
            CTScSubCampaignContactList _item = gvContact.GetFocusedRow() as CTScSubCampaignContactList;
            if (_item == null)
                return;

            DataTable _dtContactStat = DatabaseUtility.ExecuteStoredProcedure(
                "bvGetStatistics_sp",
                "ContactStat",
                new SqlParameter[] {
                    new SqlParameter("contact_id", _item.id),
                    new SqlParameter("p_sub_campaign_id", m_BrightSalesProperty.CommonProperty.SubCampaignId)
                }
            );
            List<object> _row = _dtContactStat.Rows[0].ItemArray.ToList();


            /*
             * https://brightvision.jira.com/browse/PLATFORM-2945
             */
            //string[] _values = _row[2].ToString().Split(' ');
            //if (_values.Count() > 0)
            //    gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "call", Convert.ToInt32(_values[0]));

            string value = _row[1].ToString();
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "call", Convert.ToInt32(value));

        }
        public void UpdateContactLastUser()
        {
            gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "last_user", UserSession.CurrentUser.UserFullName);
        }
        #endregion

        #region Private Methods
        private void SetFocusedRowInfo() 
        {
            m_BrightSalesProperty.CommonProperty.ContactPerson = null;
            //m_BrightSalesProperty.CommonProperty.ContactPerson = (gcContact.DataSource as List<CTScSubCampaignContactList>)[gvContact.FocusedRowHandle];
            m_BrightSalesProperty.CommonProperty.ContactPerson = gvContact.GetFocusedRow() as CTScSubCampaignContactList;
            m_BrightSalesProperty.CommonProperty.CurrentWorkedContactId = m_BrightSalesProperty.CommonProperty.ContactPerson.id;
        }
        private void ApplyFilters()
        {
            //gvContact.ActiveFilterString = "active = " + cbxHideInactive.Checked + " and in_list = '" + (cbxHideNotInList.Checked == true? "Yes": "No") + "'";
            //m_OnFilterProcess = false;
            //this.gvContact_FocusedRowChanged(null, null);
        }
        #endregion

        #region Master Detail Contact
        //private void InitializeAutoTitleField() {
        //    gcContact.RepositoryItems.Add(repositoryItemComboBoxAuto);
        //    lvColumnTitle.ColumnEdit = repositoryItemComboBoxAuto;
        //}
        //private object[,] relations = { { "Contact View" } };
        ////Specifies the maximum number of details 
        //private void gvContact_MasterRowGetRelationCount(object sender, MasterRowGetRelationCountEventArgs e) {
        //    e.RelationCount = 1;
        //}
        ////Specifies whether a specific detail contains data 
        ////The detail should not be displayed if the corresponding relations element is null 
        //private void gvContact_MasterRowEmpty(object sender, MasterRowEmptyEventArgs e) {
        //    e.IsEmpty = IsRelationEmpty(e.RowHandle);
        //}
        //private bool IsRelationEmpty(int rowHandle) {
        //    return rowHandle == GridControl.InvalidRowHandle;
        //}
        //private void gvContact_MasterRowGetChildList(object sender, MasterRowGetChildListEventArgs e) 
        //{
        //    gvContact.CollapseAllDetails();
        //    if (IsRelationEmpty(e.RowHandle)) return;
        //    string s = relations[0, e.RelationIndex].ToString();
        //    if (s == "Contact View") {
        //        var view = sender as GridView;
        //        var obj = view.GetRow(e.RowHandle) as CTScSubCampaignContactList;
        //        if (obj != null) {
        //            using (BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection))
        //            {
                    
        //                var contactList = BPContext.FIGetContactByID(obj.id, null).ToList();
        //                if (contactList != null)
        //                {
        //                    e.ChildList = contactList.Clone().ToList();
        //                    contactList = null;
        //                    InitializeAutoTitleField();
        //                }
        //            }
        //        }
        //    }
        //}
        //private void gvContact_MasterRowGetRelationName(object sender, MasterRowGetRelationNameEventArgs e) {
        //    if (IsRelationEmpty(e.RowHandle)) e.RelationName = "";
        //    else
        //        e.RelationName = relations[0, e.RelationIndex].ToString();
        //}
        //private void gvContact_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        //{
        //    GridView view = sender as GridView;
        //    SalesConsultant.Business.BrightSalesGridUtility.CreateGridContextMenu(view, e);
        //}
        
        //private void lvContactEdit_CustomFieldEditingValueStyle(object sender, DevExpress.XtraGrid.Views.Layout.Events.LayoutViewFieldEditingValueStyleEventArgs e) {
        //    var view = sender as DevExpress.XtraGrid.Views.Layout.LayoutView;
        //    e.Appearance.BackColor = Color.White;
        //    if (e.Column.Caption == "Save" || e.Column.Caption == "Cancel") return;
        //    var val = view.GetRowCellValue(e.RowHandle, e.Column.FieldName);
        //    if (val == null || val.ToString().Trim() == e.Column.Caption) {
        //        view.SetRowCellValue(e.RowHandle, e.Column.FieldName, "");
        //        if (e.Column.FieldName != "title")
        //            e.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
        //        else {
        //            e.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
        //            e.Appearance.BackColor = Color.FromArgb(244, 102, 102);
        //            e.Appearance.BackColor2 = Color.FromArgb(244, 102, 102);
        //        }
        //    } else if (val.ToString().Trim() != "" && e.Column.FieldName == "title") {
        //        var edit = e.Column.ColumnEdit as RepositoryItemBVPopupContainerEdit;
        //        var data = edit.PopupControl.Tag as BVPopupContainerControlData;
        //        bool hasMatch = data.MatchKeyword(val.ToString().Trim());
        //        if (hasMatch) {
        //            e.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
        //            e.Appearance.BackColor = Color.FromArgb(181, 245, 146);//green
        //            e.Appearance.BackColor2 = Color.FromArgb(181, 245, 146);//green
        //        } else {
        //            e.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
        //            e.Appearance.BackColor = Color.FromArgb(244, 102, 102);//red
        //            e.Appearance.BackColor2 = Color.FromArgb(244, 102, 102);//red
        //        }
        //    }
        //}
        //private void lvContactEdit_CustomFieldValueStyle(object sender, DevExpress.XtraGrid.Views.Layout.Events.LayoutViewFieldValueStyleEventArgs e) {
        //    var view = sender as DevExpress.XtraGrid.Views.Layout.LayoutView;
        //    if ((view.FocusedColumn == e.Column) && (view.FocusedRowHandle == e.RowHandle)) {
        //        e.Appearance.BackColor = Color.White;
        //    }
        //    if (e.Column.Caption == "Save" || e.Column.Caption == "Cancel") return;
        //    var val = view.GetRowCellValue(e.RowHandle, e.Column.FieldName);
        //    if (val != null && val.ToString() == e.Column.Caption) {
        //        e.Appearance.ForeColor = Color.FromArgb(215, 215, 215);
        //    } else if (val != null && val.ToString() != "" && e.Column.FieldName == "title") {
        //        var edit = e.Column.ColumnEdit as RepositoryItemBVPopupContainerEdit;
        //        var data = edit.PopupControl.Tag as BVPopupContainerControlData;
        //        bool hasMatch = data.MatchKeyword(val.ToString().Trim());
        //        if (hasMatch) {
        //            e.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
        //            e.Appearance.BackColor = Color.FromArgb(181, 245, 146);//green
        //            e.Appearance.BackColor2 = Color.FromArgb(181, 245, 146);//green
        //        } else {
        //            e.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
        //            e.Appearance.BackColor = Color.FromArgb(244, 102, 102);//red
        //            e.Appearance.BackColor2 = Color.FromArgb(244, 102, 102);//red
        //        }
        //    } else {
        //        e.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
        //    }

        //}
        //private void lvContactEdit_ShownEditor(object sender, EventArgs e) {
        //    var view = sender as DevExpress.XtraGrid.Views.Layout.LayoutView;
        //    if (view != null && view.ActiveEditor != null) {
        //        if (view.ActiveEditor is PopupContainerEdit) {
        //            var comboBoxEdit = view.ActiveEditor as PopupContainerEdit;
        //            if (comboBoxEdit != null) {
        //                comboBoxEdit.Tag = view.FocusedColumn;
        //                comboBoxEdit.IsModified = true;
        //            }
        //        } else if (view.ActiveEditor is TextEdit) {
        //            var textEdit = view.ActiveEditor as TextEdit;
        //            if (textEdit != null) {
        //                textEdit.Tag = view.FocusedColumn;
        //                textEdit.IsModified = true;
        //            }
        //        }
        //    }
        //}
        
        //private void repositoryItemButtonEditSave_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e) 
        //{
        //    //ManageCampaignBooking.eCampaignBookingMode _CampaignBookingMode;
        //    //if (GetCampaignBookingMode != null)
        //    //    _CampaignBookingMode = GetCampaignBookingMode();

        //    //if (GetCampaignBookingMode == null)
        //    //    return;

        //    if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse || 
        //        m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit)
        //        return;
            
        //    //if (ParentModule.CampaignBookingMode == ManageCampaignBooking.eCampaignBookingMode.BrowseMode || 
        //    //    DialogEditorModule.DialogSaveMode == DialogEditor.SaveMode.Edit)
        //    //    return;

        //    bool isvalid = true;
        //    var view = gvContact;
        //    var rowHandle = view.FocusedRowHandle;
        //    var detailView = view.GetDetailView(rowHandle, 0);
        //    var contact = detailView.GetRow(0) as CTContactDetails;

        //    //validate fields first
        //    if (string.IsNullOrEmpty(contact.last_name) && string.IsNullOrEmpty(contact.first_name)) {
        //        isvalid = false;
        //    } else if (string.IsNullOrEmpty(contact.first_name) || contact.first_name.ToLower() == "first name") {
        //        isvalid = false;
        //    } else if (string.IsNullOrEmpty(contact.last_name) || contact.last_name.ToLower() == "last name") {
        //        isvalid = false;
        //    }

        //    if (!isvalid) 
        //    {
        //        NotificationDialog.Warning("Bright Sales", "Firstname and lastname is required.");
        //        return;
        //    }
        //    //else if ( || contact.last_name.ToLower() == "email")
        //    //{
        //    //    MessageBox.Show("Email is required.", "Contact Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    //    return;
        //    //}
        //    else if (!contact.email.Equals("Email") && !string.IsNullOrEmpty(contact.email) && !ValidationUtility.IsEmail(contact.email))
        //    {
        //        NotificationDialog.Warning("Bright Sales", "Invalid email format.");
        //        return;
        //    }

        //    //if (MessageBox.Show("Are you sure you want to save changes you made for this contact?", "Save Contact", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
        //    string firstname = contact.first_name.ToLower() == "first name" ? "" : contact.first_name;
        //    string lastname = contact.last_name.ToLower() == "last name" ? "" : contact.last_name;
        //    string middlename = contact.middle_name.ToLower() == "middle name" ? "" : contact.middle_name;
        //    string title = contact.title.ToLower() == "title" ? "" : contact.title;
        //    string role_tags = contact.role_tags.ToLower() == "roles" ? "" : contact.role_tags;
        //    string email = contact.email.ToLower() == "email" ? "" : contact.email;
        //    string directphone = contact.direct_phone.ToLower() == "direct phone" ? "" : contact.direct_phone;
        //    string mobile = contact.mobile.ToLower() == "mobile phone" ? "" : contact.mobile;
        //    string address1 = contact.address_1.ToLower() == "address 1" ? "" : contact.address_1;
        //    string address2 = contact.address_2.ToLower() == "address 2" ? "" : contact.address_2;
        //    string city = contact.city.ToLower() == "city" ? "" : contact.city;
        //    string zipcode = contact.zipcode.ToLower() == "zip code" ? "" : contact.zipcode;
        //    using (var BPContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
        //        BPContext.FIUpdateContactDetails(
        //            contact.id, firstname, middlename, lastname, directphone,
        //            mobile, contact.title_id, null, role_tags, address1, address2, city,
        //            zipcode, contact.country, contact.remarks, UserSession.CurrentUser.UserId, email, null, null, null, null,
        //            UserSession.CurrentUser.ComputerName,
        //            BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Sales_Contact_View,
        //            contact.absence_start,
        //            contact.absence_end,
        //            m_BrightSalesProperty.CommonProperty.SubCampaignId,
        //            m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId
        //        );
        //    }

        //    /**
        //     * [@jeff 07.09.2012]: https://brightvision.jira.com/browse/PLATFORM-1550
        //     * update column if has contact remarks.
        //     */
        //    if (contact.remarks == null)
        //        gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "has_contact_remarks", false);
        //    else if (contact.remarks.Length > 0)
        //        gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "has_contact_remarks", true);
        //    else
        //        gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "has_contact_remarks", false);

        //    //BPContext.SaveChanges();
        //    //ObjectContact.SaveContactTitle(contact.title);
        //    view.SetRowCellValue(rowHandle, "first_name", firstname);
        //    view.SetRowCellValue(rowHandle, "last_name", lastname);

        //    if (contact.title_id != null && contact.title_id > 0)
        //        view.SetRowCellValue(rowHandle, "title", title);
        //    else
        //        view.SetRowCellValue(rowHandle, "title", contact.org_title);

        //    if (!string.IsNullOrEmpty(contact.role_tags))
        //        view.SetRowCellValue(rowHandle, "role_tags", contact.role_tags);
            
        //    view.SetRowCellValue(rowHandle, "direct_phone", directphone);
        //    view.SetRowCellValue(rowHandle, "mobile", mobile);
        //    view.SetRowCellValue(rowHandle, "email", email);
        //    string complete_address = "";
        //    if (!string.IsNullOrEmpty(address1))
        //        complete_address += address1;
        //    if (!string.IsNullOrEmpty(address2))
        //        complete_address += ", " + address2;
        //    if (!string.IsNullOrEmpty(city))
        //        complete_address += ", " + city;
        //    if (!string.IsNullOrEmpty(zipcode))
        //        complete_address += ", " + zipcode;
        //    if (!string.IsNullOrEmpty(contact.country))
        //        complete_address += ", " + contact.country;
        //    view.SetRowCellValue(rowHandle, "complete_address", complete_address);
        //    view.SetMasterRowExpanded(rowHandle, false);
        //    SetFocusedRowInfo();
        //    //fire custom event by user
        //    if (OnContactSaved != null) {
        //        ContactEventArgs contactArgs = new ContactEventArgs(contact);
        //        OnContactSaved(this, contactArgs);
        //    }
        //    //}

        //    gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "contact_name", string.Format("{0} {1}", contact.first_name, contact.last_name));
        //    gvContact.SetRowCellValue(gvContact.FocusedRowHandle, "title_role", string.Format("{0} {1}",
        //        contact.title,
        //        string.IsNullOrEmpty(contact.role_tags) ? string.Empty : string.Format("({0})", contact.role_tags)
        //    ));
        //}
        //private void repositoryItemButtonEditCancel_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e) 
        //{
        //    //if (GetCampaignBookingMode == null)
        //    //    return;

        //    if (m_BrightSalesProperty.CampaignBooking.Mode == SelectionProperty.CampaignBookingMode.Browse ||
        //        m_BrightSalesProperty.CampaignBooking.Questionnaire.Mode == SelectionProperty.DialogSaveMode.Edit)
        //        return;

        //    //if (ParentModule.CampaignBookingMode == ManageCampaignBooking.eCampaignBookingMode.BrowseMode ||
        //    //    DialogEditorModule.DialogSaveMode == DialogEditor.SaveMode.Edit)
        //    //    return;

        //    var view = gvContact;
        //    var rowHandle = view.FocusedRowHandle;
        //    var detailView = view.GetDetailView(rowHandle, 0);
        //    var contact = detailView.GetRow(0) as CTContactDetails;
        //    try {
        //        using (var BPContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
        //            contact = BPContext.FIGetContactByID(contact.id, null).FirstOrDefault();
        //            BPContext.Detach(contact);
        //        }
        //    }
        //    catch (Exception ex) {
        //        m_EventBus.Notify(new FrmSalesConsultantEvents.Tracer() {
        //            ErrorMessage = ex.Message
        //        });
        //    }
        //    view.SetMasterRowExpanded(rowHandle, false);
        //}
        //private void repositoryItemComboBoxAuto_Validating(object sender, CancelEventArgs e) {
        //    var ctl = sender as PopupContainerEdit;
        //    if (ctl != null) {
        //        var col = ctl.Tag as DevExpress.XtraGrid.Columns.GridColumn;
        //        if (col == null) return;
        //        if (col.Caption == ctl.Text.Trim() || ctl.Text.Trim() == string.Empty) {
        //            ctl.Text = col.Caption;
        //            ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
        //            ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);
        //        } else {
        //            var data = ctl.Properties.PopupControl.Tag as BVPopupContainerControlData;
        //            if (data != null && data.HasMatch) {
        //                ctl.Properties.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
        //                ctl.Properties.Appearance.BackColor = Color.FromArgb(181, 245, 146);
        //            } else {
        //                ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
        //                ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);
        //            }
        //        }
        //    }
        //}
        //private void repositoryItemTextEditField_Validating(object sender, CancelEventArgs e) {
        //    var ctl = sender as TextEdit;
        //    if (ctl != null) {
        //        var col = ctl.Tag as DevExpress.XtraGrid.Columns.GridColumn;
        //        if (col == null) return;
        //        if (col.Caption == ctl.Text.Trim() || ctl.Text.Trim() == string.Empty) {
        //            ctl.Text = col.Caption;
        //            ctl.Properties.Appearance.ForeColor = Color.FromArgb(215, 215, 215);
        //        } else {
        //            ctl.Properties.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
        //        }
        //    }
        //}
        
        //private void textEdit_Leave(object sender, EventArgs e) {
        //    var ctl = sender as TextEdit;
        //    if (ctl != null) {
        //        var col = ctl.Tag as DevExpress.XtraGrid.Columns.GridColumn;
        //        if (col.Caption == ctl.Text.Trim() || ctl.Text.Trim() == string.Empty) {
        //            ctl.Text = col.Caption;
        //            ctl.Properties.Appearance.ForeColor = Color.FromArgb(215, 215, 215);
        //        } else {
        //            ctl.Properties.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
        //        }
        //    }
        //}
        #endregion

        #region Public Objects
        public class ContactEventArgs : EventArgs {
            public ContactEventArgs(CTContactDetails obj) {
                Contact = obj;
            }
            public CTContactDetails Contact { get; set; }
        }
        
        #endregion

    }
}
