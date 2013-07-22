
#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using ManagerApplication.Business;
using ManagerApplication.Forms;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
#endregion

namespace ManagerApplication.Modules
{
    public partial class ManageSubCampaign : DevExpress.XtraEditors.XtraUserControl
    {
        #region Enumeration
        /// <summary>
        /// Enum for grid view types
        /// </summary>
        private enum eGridViewType
        {
            SubCampaign,
            SubCampaignInternalUser,
            SubCampaignCustomerUser,
            SubCampaignUserRole
        }
        #endregion

        #region Public Properties
        public int SelectedSubCampaignId = 0;
        #endregion

        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private GridView m_objGridView = null;
        private ObjectSubCampaign.SubCampaignInstance m_objSubCampaign = null;
        private CTSubCampaignInternalUsers m_objSubCampaignInternalUser = null;
        private CTSubCampaignCustomerUsers m_objSubCampaignCustomerUser = null;
        private CTSubCampaignUserRole m_objSubCampaignUserRole = null;
        private AddSubCampaign m_objAddSubCampaignForm = null;
        private PopupDialog m_objPopupDialog = null;
        private FrmSchedulingPopup frmSchedulingPopup = null;

        private string m_MessageBoxCaption = "Manager Application - Sub Campaign";
        private bool m_DoneLoading = false;
        private bool m_OnEditMode = false;
        #endregion

        #region Constructors
        public ManageSubCampaign()
        {
            m_DoneLoading = false;
            this.Visible = false;
            InitializeComponent();
            this.lcManageSubCampaign.AllowCustomizationMenu = false;
            this.PopulateSubCampaignView();
            this.SetSubCampaignViewContextMenu();
            m_DoneLoading = true;
            this.ApplyFilters();
            this.Visible = true;
        }
        #endregion

        #region Event Subscriptions
        private void m_objAddSubCampaignInternalUserForm_AfterSave(List<int> pSubCampaignUserIdList)
        {
            this.PopulateSubCampaignInternalUserView(m_objSubCampaign.id);
            //gvInternalUser.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
            //for (int i = 0; i < gvInternalUser.RowCount; i++) {
            //    CTSubCampaignInternalUsers _item = gvInternalUser.GetRow(i) as CTSubCampaignInternalUsers;
            //    if (pSubCampaignUserIdList.Contains(_item.id)) {
            //        //gvInternalUser.FocusedRowHandle = i;
            //        //gvInternalUser.selectrow
            //        //break;
            //    }
            //}
        }
        private void m_objAddSubCampaignCustomerUserForm_AfterSave(int pSubCampaignUserId)
        {
            this.PopulateSubCampaignCustomerUserView(m_objSubCampaign.id);
            for (int i = 0; i < gvCustomerUser.RowCount; i++)
            {
                CTSubCampaignCustomerUsers _item = gvCustomerUser.GetRow(i) as CTSubCampaignCustomerUsers;
                if (_item.id == pSubCampaignUserId)
                {
                    gvCustomerUser.FocusedRowHandle = i;
                    break;
                }
            }

        }
        #endregion

        #region Object Control Events
        private void btnSendEmailTemplate_Click(object sender, EventArgs e)
        {
            if (gvSubCampaign.RowCount < 1)
                return;

            int _SubCampaignId = Convert.ToInt32(gvSubCampaign.GetRowCellValue(gvSubCampaign.FocusedRowHandle, "id"));
            SendEmailTemplate _control = new SendEmailTemplate(_SubCampaignId) {
                Dock = DockStyle.Fill
            };
            PopupDialog _dlg = new PopupDialog() {
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Send Email Template",
                FormBorderStyle = FormBorderStyle.FixedSingle
            };
            _dlg.ClientSize = new Size(_control.Width + 2, _control.Height + 2);
            _dlg.Controls.Add(_control);
            _dlg.ShowDialog(this.ParentForm);
        }
        private void btnConfigureXml_Click(object sender, EventArgs e)
        {
            if (gvSubCampaign.RowCount < 1)
                return;

            WaitDialog.Show("Loading window...", true);
            this.SetFocusedViewInstance(eGridViewType.SubCampaign);
            EditorSubCampaignConfig objForm = new EditorSubCampaignConfig(m_objSubCampaign.id);
            objForm.SetLabel(String.Format("{0} > {1} > {2}", m_objSubCampaign.customer_name, m_objSubCampaign.campaign_name, m_objSubCampaign.sub_campaign_name));
            objForm.Dock = DockStyle.Fill;
            m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Configure Subcampaign Xml";
            m_objPopupDialog.ClientSize = new Size(objForm.Width + 2, objForm.Height + 2);
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.Controls.Add(objForm);
            m_objPopupDialog.ShowDialog(this.ParentForm);
            WaitDialog.Close(true);
        }
        private void cbxAssignRole_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (gvInternalUser.RowCount < 1)
                e.Cancel = true;
        }
        private void cbxAssignRole_CheckedChanged(object sender, EventArgs e)
        {
            if (gvInternalUser.RowCount < 1)
                return;

            this.Cursor = Cursors.WaitCursor;
            this.SetFocusedViewInstance(eGridViewType.SubCampaignUserRole);
            var Item = (CheckEdit) sender;
            if (Item.Checked)
                ObjectSubCampaign.SaveSubCampaignRole(m_objSubCampaignInternalUser.id, m_objSubCampaignUserRole.id);
            else
                ObjectSubCampaign.DeleteSubCampaignRole(m_objSubCampaignInternalUser.id, m_objSubCampaignUserRole.id);
            this.Cursor = Cursors.Default;
        }
        private void riceCampaignOwner_CheckedChanged(object sender, EventArgs e)
        {
            if (gvInternalUser.RowCount < 1)
                return;

            var Item = (CheckEdit)sender;
            if (Item.Checked)
                ObjectSubCampaign.SaveSubCampaignRole(m_objSubCampaignInternalUser.id, 1);
            else
                ObjectSubCampaign.DeleteSubCampaignRole(m_objSubCampaignInternalUser.id, 1);
        }
        private void riceSubCampaignMananger_CheckedChanged(object sender, EventArgs e)
        {
            if (gvInternalUser.RowCount < 1)
                return;

            var Item = (CheckEdit)sender;
            if (Item.Checked)
                ObjectSubCampaign.SaveSubCampaignRole(m_objSubCampaignInternalUser.id, 2);
            else
                ObjectSubCampaign.DeleteSubCampaignRole(m_objSubCampaignInternalUser.id, 2);
        }
        private void riceSubCampaignSales_CheckedChanged(object sender, EventArgs e)
        {
            if (gvInternalUser.RowCount < 1)
                return;

            var Item = (CheckEdit)sender;
            if (Item.Checked)
                ObjectSubCampaign.SaveSubCampaignRole(m_objSubCampaignInternalUser.id, 3);
            else
                ObjectSubCampaign.DeleteSubCampaignRole(m_objSubCampaignInternalUser.id, 3);
        }
        private void riceCustomerUser_CheckedChanged(object sender, EventArgs e)
        {
            if (gvInternalUser.RowCount < 1)
                return;

            var Item = (CheckEdit)sender;
            if (Item.Checked)
                ObjectSubCampaign.SaveSubCampaignRole(m_objSubCampaignInternalUser.id, 4);
            else
                ObjectSubCampaign.DeleteSubCampaignRole(m_objSubCampaignInternalUser.id, 4);
        }
        private void cmdEdit_Click(object sender, EventArgs e)
        {
            if (gvSubCampaign.RowCount < 1)
                return;

            //m_SelectedRow = gvSubCampaign.FocusedRowHandle;
            this.SetFocusedViewInstance(eGridViewType.SubCampaign);
            this.DisplaySubCampaignForm(false);
        }
        private void cmdAddNew_Click(object sender, EventArgs e)
        {
            this.DisplaySubCampaignForm(true);
        }
        private void gvSubCampaign_DoubleClick(object sender, EventArgs e)
        {
            this.SetFocusedViewInstance(eGridViewType.SubCampaign);
            this.DisplaySubCampaignForm(false);
        }
        private void gvSubCampaign_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            //m_SelectedRow = e.FocusedRowHandle;
            this.RefreshData();
        }
        private void gvInternalUser_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            this.SetFocusedViewInstance(eGridViewType.SubCampaignInternalUser);
            this.PopulateUserRoleView();
            this.Cursor = Cursors.Default;
        }
        private void cmdAddInternalUser_Click(object sender, EventArgs e)
        {
            if (gvSubCampaign.RowCount < 1)
                return;

            this.DisplaySubCampaignInternalUserForm();
        }
        private void cmdRemoveInternalUser_Click(object sender, EventArgs e)
        {
            if (gvSubCampaign.RowCount < 1)
                return;

            if (gvInternalUser.RowCount < 1)
                return;

            if (this.ProceedDeleteProcess())
                this.RemoveSubCampaignUser(true);
        }
        private void gvCustomerUser_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.SetFocusedViewInstance(eGridViewType.SubCampaignCustomerUser);
            PopulateResourceSchedule();
        }
        private void cmdRemoveCustomerUser_Click(object sender, EventArgs e)
        {
            if (gvSubCampaign.RowCount < 1)
                return;

            if (gvCustomerUser.RowCount < 1)
                return;

            if (this.ProceedDeleteProcess())
                this.RemoveSubCampaignUser(false);
        }
        private void cmdAddCustomerUser_Click(object sender, EventArgs e)
        {
            if (gvSubCampaign.RowCount < 1)
                return;

            this.DisplaySubCampaignCustomerUserForm();
        }
        private void miPrintPreview_Click(object sender, EventArgs e)
        {
            gcSubCampaign.ShowPrintPreview();
        }
        private void gvSubCampaign_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Equals("start_date") || e.Column.FieldName.Equals("end_date"))
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyy-MM-dd");
        }
        private void cbxShowConfirmed_CheckedChanged(object sender, EventArgs e)
        {
            this.ApplyFilters();
        }
        private void cbxShowInDevelopment_CheckedChanged(object sender, EventArgs e)
        {
            this.ApplyFilters();
        }
        private void cbxShowActive_CheckedChanged(object sender, EventArgs e)
        {
            this.ApplyFilters();
        }
        private void cbxShowOnHold_CheckedChanged(object sender, EventArgs e)
        {
            this.ApplyFilters();
        }
        private void cbxShowArchived_CheckedChanged(object sender, EventArgs e)
        {
            this.ApplyFilters();
        }
        private void cbxShowDeleted_CheckedChanged(object sender, EventArgs e)
        {
            this.ApplyFilters();
        }
        private void btnCreateSchedule_Click(object sender, EventArgs e) 
        {
            if (gvSubCampaign.RowCount < 1 || gvCustomerUser.RowCount < 1)
                return;

            //if (gridViewResourceSchedules.RowCount < 1)
            //    return;

            frmSchedulingPopup = new FrmSchedulingPopup();
            frmSchedulingPopup.SubCampaignID = m_objSubCampaign.id;            
            if (m_objSubCampaign != null) {
                frmSchedulingPopup.SetBreadCrumb(string.Format(
                    "{0}{1}{2}{3}",
                    string.IsNullOrEmpty(m_objSubCampaign.customer_name) ? "" : m_objSubCampaign.customer_name + " > ",
                    string.IsNullOrEmpty(m_objSubCampaign.campaign_name) ? "" : m_objSubCampaign.campaign_name + " > ",
                    string.IsNullOrEmpty(m_objSubCampaign.sub_campaign_name) ? "" : m_objSubCampaign.sub_campaign_name + " > ",
                    string.IsNullOrEmpty(m_objSubCampaign.dialog_name) ? "" : m_objSubCampaign.dialog_name));
            } 
            if (frmSchedulingPopup.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                PopulateResourceSchedule();
            } else {
                PopulateResourceSchedule();
            }
        }
        private void gridViewResourceSchedules_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e) {
            if (e.Column.FieldName.Equals("start_time") || e.Column.FieldName.Equals("end_time"))
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd HH:mm");
        }
        private void gvCustomerUser_DataSourceChanged(object sender, EventArgs e) 
        {
            this.SetFocusedViewInstance(eGridViewType.SubCampaignCustomerUser);
            PopulateResourceSchedule();
        }
        private void gvSubCampaign_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gvInternalUser_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gvCustomerUser_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gvExportTemplate_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gvUserRole_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gridViewResourceSchedules_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void btnConfigureNurtureList_Click(object sender, EventArgs e)
        {
            if (gvSubCampaign.RowCount < 1)
                return;

            int _CustomerId = Convert.ToInt32(gvSubCampaign.GetRowCellValue(gvSubCampaign.FocusedRowHandle, "customer_id"));
            int _CampaignId = Convert.ToInt32(gvSubCampaign.GetRowCellValue(gvSubCampaign.FocusedRowHandle, "campaign_id"));
            int _SubCampaignId = Convert.ToInt32(gvSubCampaign.GetRowCellValue(gvSubCampaign.FocusedRowHandle, "id"));
            EditorNurtureSetting _control = new EditorNurtureSetting(_CustomerId, _CampaignId, _SubCampaignId) {
                Dock = DockStyle.Fill
            };
            PopupDialog _dlg = new PopupDialog() {
                MinimizeBox = false,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Select Sub Campaign Nurture List",
                FormBorderStyle = FormBorderStyle.FixedSingle
            };
            _dlg.ClientSize = new Size(_control.Width + 2, _control.Height + 2);
            _dlg.Controls.Add(_control);
            _dlg.Focus();
            _dlg.ShowDialog(this.ParentForm);
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Pupulates the sub campaign grid view contents
        /// </summary>
        public void PopulateSubCampaignView()
        {
            try
            {
                gcSubCampaign.DataSource = null;

                if (UserSession.CurrentUser.IsManagerAdmin)
                    gcSubCampaign.DataSource = ObjectSubCampaign.GetSubCampaigns(ObjectSubCampaign.eViewType.SubCampaignView_ManagerAdmin, 0, 0).Execute(MergeOption.AppendOnly);
                else if (UserSession.CurrentUser.IsManagerUser)
                    gcSubCampaign.DataSource = ObjectSubCampaign.GetSubCampaigns(ObjectSubCampaign.eViewType.SubCampaignView_ManagerUser, 0, 0).Execute(MergeOption.AppendOnly);

                this.SetDefaultSelectedRow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Pupulates the sub campaign internal user grid view contents
        /// </summary>
        public void PopulateSubCampaignInternalUserView(int SubCampaignId)
        {
            try {
                gcInternalUser.DataSource = null;
                gcInternalUser.DataSource = ObjectSubCampaign.GetSubCampaignUsers(SubCampaignId, ObjectSubCampaign.eUserType.InternalUser);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Pupulates the sub campaign customer user grid view contents
        /// </summary>
        public void PopulateSubCampaignCustomerUserView(int SubCampaignId)
        {
            try
            {
                gcCustomerUser.DataSource = null;
                gcCustomerUser.DataSource = ObjectSubCampaign.GetSubCampaignUsers(SubCampaignId, ObjectSubCampaign.eUserType.CustomerUser);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Set the default selected row
        /// </summary>
        private void SetDefaultSelectedRow()
        {
            if (SelectedSubCampaignId < 1)
                return;

            int _SelectedRow = 0;
            ObjectSubCampaign.SubCampaignInstance _Item = null;

            for (int i = 0; i < gvSubCampaign.RowCount; i++) 
            {
                _Item = gvSubCampaign.GetRow(i) as ObjectSubCampaign.SubCampaignInstance;
                if (_Item.id == SelectedSubCampaignId)
                {
                    _SelectedRow = i;
                    break;
                }
            }

            gvSubCampaign.FocusedRowHandle = _SelectedRow;
        }

        /// <summary>
        /// Populate the sub campaign user roles view
        /// </summary>
        private void PopulateUserRoleView()
        {
            try
            {
                this.SetFocusedViewInstance(eGridViewType.SubCampaignInternalUser);
                int SubCampaignId = m_objSubCampaign == null ? 0 : m_objSubCampaign.id;
                int UserId = m_objSubCampaignInternalUser == null ? 0 : m_objSubCampaignInternalUser.user_id;
                //gcUserRole.DataSource = null;
                //gcUserRole.DataSource = ObjectSubCampaign.GetSubCampaignUserRoles(SubCampaignId, UserId);
            }
            catch { }
        }

        /// <summary>
        /// Refresh display
        /// </summary>
        private void RefreshData()
        {
            if (!m_DoneLoading)
                return;

            gcInternalUser.DataSource = null;
            gcCustomerUser.DataSource = null;
            gridControlResourceSchedules.DataSource = null;
           // gcUserRole.DataSource = null;

            this.SetFocusedViewInstance(eGridViewType.SubCampaign);
            if (m_objSubCampaign == null)
                return;

            this.PopulateSubCampaignInternalUserView(m_objSubCampaign.id);
            this.PopulateSubCampaignCustomerUserView(m_objSubCampaign.id);
            this.PopulateResourceSchedule();
            this.PopulateUserRoleView();
        }

        /// <summary>
        /// Filter function for sub campaign display
        /// </summary>
        private void ApplyFilters()
        {
            m_DoneLoading = false;
            StringBuilder FilterText = new StringBuilder();

            if (cbxShowConfirmed.Checked)
                FilterText.Append("[sub_campaign_status] like 'Confirmed'");

            if (cbxShowInDevelopment.Checked)
                if (FilterText.Length < 1)
                    FilterText.Append("[sub_campaign_status] like 'In Development'");
                else
                    FilterText.Append("or [sub_campaign_status] like 'In Development'");

            if (cbxShowActive.Checked)
                if (FilterText.Length < 1)
                    FilterText.Append("[sub_campaign_status] like 'Active'");
                else
                    FilterText.Append("or [sub_campaign_status] like 'Active'");

            if (cbxShowOnHold.Checked)
                if (FilterText.Length < 1)
                    FilterText.Append("[sub_campaign_status] like 'On Hold'");
                else
                    FilterText.Append("or [sub_campaign_status] like 'On Hold'");

            if (cbxShowArchived.Checked)
                if (FilterText.Length < 1)
                    FilterText.Append("[sub_campaign_status] like 'Archived'");
                else
                    FilterText.Append("or [sub_campaign_status] like 'Archived'");

            if (cbxShowDeleted.Checked)
                if (FilterText.Length < 1)
                    FilterText.Append("[sub_campaign_status] like 'Deleted'");
                else
                    FilterText.Append("or [sub_campaign_status] like 'Deleted'");

            if (FilterText.Length < 1)
                FilterText.Append("[sub_campaign_status] like 'none'");

            gvSubCampaign.ClearColumnsFilter();
            gvSubCampaign.Columns["sub_campaign_status"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo(FilterText.ToString());
            m_DoneLoading = true;
            this.RefreshData();
        }

        /// <summary>
        /// Confirms delete subcampaign users
        /// </summary>
        private bool ProceedDeleteProcess()
        {
            DialogResult objDialogResult = new DialogResult();
            objDialogResult = MessageBox.Show("Are you sure to delete this sub-campaign user?", m_MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (objDialogResult == DialogResult.Yes)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Removes sub campaign user
        /// </summary>
        private void RemoveSubCampaignUser(bool IsInternalUser)
        {
            if (IsInternalUser)
            {
                if (m_objSubCampaignInternalUser == null)
                    return;

                ObjectSubCampaign.RemoveSubCampaignUser(m_objSubCampaignInternalUser.id);
                this.PopulateSubCampaignInternalUserView(m_objSubCampaign.id);
                gvInternalUser.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
                this.SetFocusedViewInstance(eGridViewType.SubCampaignInternalUser);
            }
            else
            {
                if (m_objSubCampaignCustomerUser == null)
                    return;

                ObjectSubCampaign.RemoveSubCampaignUser(m_objSubCampaignCustomerUser.id);
                this.PopulateSubCampaignCustomerUserView(m_objSubCampaign.id);
                gvInternalUser.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
                this.SetFocusedViewInstance(eGridViewType.SubCampaignCustomerUser);
            }

            MessageBox.Show("Successfully updated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Initializes the popup dialog for sub campaign customer user entry and loads the popup dialog
        /// </summary>
        private void DisplaySubCampaignCustomerUserForm()
        {
            AddCustomerUser m_objAddSubCampaignCustomerUserForm = new AddCustomerUser(m_objSubCampaignCustomerUser, m_objSubCampaign.id, m_objSubCampaign.customer_id);
            m_objAddSubCampaignCustomerUserForm.AfterSave += new AddCustomerUser.AfterSaveEventHandler(m_objAddSubCampaignCustomerUserForm_AfterSave);
            m_objAddSubCampaignCustomerUserForm.m_objParentControl = this;
            m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Add Customer User";
            m_objPopupDialog.Controls.Add(m_objAddSubCampaignCustomerUserForm);
            m_objPopupDialog.ClientSize = new Size(m_objAddSubCampaignCustomerUserForm.Width + 2, m_objAddSubCampaignCustomerUserForm.Height + 2);
            m_objPopupDialog.ShowDialog(this.ParentForm);
        }

        /// <summary>
        /// Initializes the popup dialog for sub campaign internal user entry and loads the popup dialog
        /// </summary>
        private void DisplaySubCampaignInternalUserForm()
        {
            AddInternalUser m_objAddSubCampaignInternalUserForm = new AddInternalUser(m_objSubCampaign.id);
            m_objAddSubCampaignInternalUserForm.UsersNotIncluded = GetUserInCampaign();
            m_objAddSubCampaignInternalUserForm.AfterSave += new AddInternalUser.AfterSaveEventHandler(m_objAddSubCampaignInternalUserForm_AfterSave);
            //m_objAddSubCampaignInternalUserForm.m_objParentControl = this;
            m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Add Internal User";
            m_objPopupDialog.Controls.Add(m_objAddSubCampaignInternalUserForm);
            m_objPopupDialog.ClientSize = new Size(m_objAddSubCampaignInternalUserForm.Width + 2, m_objAddSubCampaignInternalUserForm.Height + 2);
            m_objPopupDialog.ShowDialog(this.ParentForm);
        }

        private List<int> GetUserInCampaign()
        {
            List<int> userIdList = new List<int>();
            int count = gvInternalUser.DataRowCount;
            for(int cnt=0; cnt<count;cnt++){
                userIdList.Add((int)gvInternalUser.GetRowCellValue(cnt, "user_id"));    
            }
            return userIdList;
        }

        /// <summary>
        /// Initializes the popup dialog for sub campaign entry and loads the popup dialog
        /// </summary>
        private void DisplaySubCampaignForm(bool IsNew)
        {
            if (IsNew)
            {
                m_objAddSubCampaignForm = new AddSubCampaign();
                m_objAddSubCampaignForm.isNew = true;
            }
            else
            {
                m_objAddSubCampaignForm = new AddSubCampaign(AddSubCampaign.SaveType.SaveTypeEdit, m_objSubCampaign);
                m_objAddSubCampaignForm.isNew = false;
            }

            m_OnEditMode = true;
            m_objAddSubCampaignForm.m_objParentControl = this;
            m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Manager Application - Sub Campaigns";
            m_objPopupDialog.Controls.Add(m_objAddSubCampaignForm);
            m_objPopupDialog.ClientSize = new Size(m_objAddSubCampaignForm.Width + 2, m_objAddSubCampaignForm.Height + 2);
            m_objPopupDialog.ShowDialog(this.ParentForm);
            m_OnEditMode = false;
        }

        /// <summary>
        /// Sets the focused view instance of the grid. Instantiates the objects that can be used for data manipulation.
        /// </summary>
        private void SetFocusedViewInstance(eGridViewType GridViewType)
        {
            m_objGridView = null;
            switch (GridViewType)
            {
                case eGridViewType.SubCampaign:
                {
                    m_objSubCampaign = null;
                    m_objGridView = gcSubCampaign.FocusedView as GridView;
                    m_objSubCampaign = m_objGridView.GetFocusedRow() as ObjectSubCampaign.SubCampaignInstance;
                    if (m_objSubCampaign != null && !m_OnEditMode)
                        SelectedSubCampaignId = m_objSubCampaign.id;
                    break;
                }

                case eGridViewType.SubCampaignInternalUser:
                {
                    m_objSubCampaignInternalUser = null;
                    m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcInternalUser.FocusedView;
                    m_objSubCampaignInternalUser = (CTSubCampaignInternalUsers)m_objGridView.GetFocusedRow();
                    break;
                }

                case eGridViewType.SubCampaignCustomerUser:
                {
                    m_objSubCampaignCustomerUser = null;
                    m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcCustomerUser.FocusedView;
                    m_objSubCampaignCustomerUser = (CTSubCampaignCustomerUsers)m_objGridView.GetFocusedRow();
                    break;
                }

                case eGridViewType.SubCampaignUserRole:
                {
                    m_objSubCampaignUserRole = null;
                    //m_objGridView = (GridView) gcUserRole.FocusedView;
                    //m_objSubCampaignUserRole = (CTSubCampaignUserRole) m_objGridView.GetFocusedRow();
                    break;
                }
            }
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetSubCampaignViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miPrintPreview = new MenuItem("Print Preview");
            miPrintPreview.Click += new EventHandler(miPrintPreview_Click);
            objClickMenu.MenuItems.Add(miPrintPreview);
            gcSubCampaign.ContextMenu = objClickMenu;
        }

        private void PopulateResourceSchedule() {
            
            try {
                gridControlResourceSchedules.DataSource = null;
                repositoryItemLookUpEditScheduleType.DataSource = null;

                WaitDialog.Show(ParentForm, "Loading resource schedules...");
                DataTable dt = new DataTable();
                dt.Columns.Add("name",typeof(String));
                dt.Columns.Add("id",typeof(int));
                DataRow dr = dt.NewRow();
                dr["name"] = "Seminar";
                dr["id"] = 1;
                dt.Rows.Add(dr);
                dr = dt.NewRow();
                dr["name"] = "Webinar";
                dr["id"] = 2;
                dt.Rows.Add(dr);
                dr = dt.NewRow();
                dr["name"] = "Booking";
                dr["id"] = 3;
                dt.Rows.Add(dr);

                repositoryItemLookUpEditScheduleType.DataSource = dt;
                repositoryItemLookUpEditScheduleType.DisplayMember = "name";
                repositoryItemLookUpEditScheduleType.ValueMember = "id";
                repositoryItemLookUpEditScheduleType.Columns.Clear();
                repositoryItemLookUpEditScheduleType.Columns.Add(new LookUpColumnInfo("name"));

                var result = m_objBrightPlatformEntity.schedules
                    .Where(x => x.resource_id == m_objSubCampaignCustomerUser.id)
                    .OrderByDescending(x=>x.start_time);
                gridControlResourceSchedules.DataSource = result.ToList();
                gridViewResourceSchedules.BestFitColumns();
                WaitDialog.Close();
            } catch {
                WaitDialog.Close();
            }
        }
        #endregion
    }
}