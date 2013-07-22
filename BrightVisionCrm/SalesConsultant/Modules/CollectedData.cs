
#region Namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.Objects;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using SalesConsultant.Business;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using System.Threading;
#endregion

namespace SalesConsultant.Modules
{
    public partial class CollectedData : DevExpress.XtraEditors.XtraUserControl
    {
        //delegate void CallFilterCollectedData();
        //private Thread m_ThreadFilterCollectedData = null;
        private BackgroundWorker m_bwAnswerData = null;
        //private GridControl _gcDummy = new GridControl();
        
        #region Constructor
        public CollectedData()
        {
            InitializeComponent();
            CollectedDataFilterByContact = false;
            //cmdShowAllData.Enabled = false;
            //cmdShowSelectedData.Enabled = true;
            //backWorkerFilterCollectedData = new BackgroundWorker();
            //backWorkerFilterCollectedData.WorkerSupportsCancellation = true;
            //backWorkerFilterCollectedData.DoWork += new DoWorkEventHandler(backWorkerFilterCollectedData_DoWork);
        }


        private void gvCollectedData_CalcRowHeight(object sender, DevExpress.XtraGrid.Views.Grid.RowHeightEventArgs e)
        {
           //http://www.devexpress.com/Support/Center/p/CQ18285.aspx
            GridViewInfo vi = gvCollectedData.GetViewInfo() as GridViewInfo; 
                                                                                                          
            Text = vi.ViewRects.Rows.ToString(); 
            if (e.RowHeight > vi.ViewRects.Rows.Height - 10) 
                e.RowHeight = vi.ViewRects.Rows.Height - 10; 
        }
        private void backWorkerFilterCollectedData_DoWork(object sender, DoWorkEventArgs e)
        {
            GetCollectedData();
        }

        #endregion

        #region Public Properties
        public ObjectResult CollectedDataResult { get; set; }
        public bool CollectedDataFilterByContact { get; set; }
        public int? ContactId { get; set; }
        public int SubCampaignId { get; set; }
        public int AccountId { get; set; }
        public int CampaignId { get; set; }
        public int CustomerId { get; set; }
        #endregion

        #region Private Properties
        private int m_PreviousSubCampaignId = 0;
        private int m_PreviousAccountId = 0;
        private int m_PreviousContactId = 0;
        private int m_PreviousCustomerId = 0;
        BackgroundWorker backWorkerFilterCollectedData;
        #endregion

        #region Private Methods
        private void RunAssync(Action pAction)
        {
            if (!IsHandleCreated)
                CreateHandle();

            this.Invoke(pAction);
        }
        private void ___GetCollectedData()
        {
            if (m_bwAnswerData != null) {
                m_bwAnswerData.CancelAsync();
                m_bwAnswerData.Dispose();
                m_bwAnswerData = null;
                GC.Collect();
            }

            try {
                btnReload.Enabled = false;
                gcCollectedData.Enabled = false;
                gcCollectedData.BeginUpdate();
                gvCollectedData.Columns.Clear();
                gcCollectedData.DataSource = null;

                m_bwAnswerData = new BackgroundWorker {
                    WorkerSupportsCancellation = true
                };

                m_bwAnswerData.DoWork += m_bwAnswerData_DoWork;
                m_bwAnswerData.RunWorkerCompleted += m_bwAnswerData_RunWorkerCompleted;
                m_bwAnswerData.RunWorkerAsync();
            }
            catch (Exception ex) {
                NotificationDialog.Error("Bright Sales", ex.InnerException.Message);
            }

            //try {
            //    bool _CustomerOwned = true;
            //    bool _BrightvisionOwned = true;
            //    int _CustomerId = CustomerId;

            //    if (UserSession.CurrentUser.IsSubCampaignSales && UserSession.CurrentUser.IsCustomerUser)
            //        _BrightvisionOwned = false;

            //    gcCollectedData.BeginUpdate();
            //    gvCollectedData.Columns.Clear();
            //    gcCollectedData.DataSource = null;
            //    gcCollectedData.DataSource = ObjectCollectedData.GetCollectedData(SubCampaignId, AccountId, null, _CustomerId, _CustomerOwned, _BrightvisionOwned);
                
            //    if (gvCollectedData.Columns.Count > 0) {
            //        GridColumn _gc;
            //        for (int x = 0; x < gvCollectedData.Columns.Count; ++x) {
            //            _gc = gvCollectedData.Columns[x];
            //            if (_gc.FieldName == "AccountID" || _gc.FieldName == "ContactID")
            //                _gc.Visible = false;

            //            _gc.OptionsColumn.AllowEdit = true;
            //            _gc.OptionsColumn.AllowFocus = true;
            //            if (_gc.FieldName == "Answer") {
            //                var memEdit = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            //                _gc.ColumnEdit = memEdit;
            //                _gc.Width = 230;
            //            }
            //            if (_gc.FieldName == "Question")
            //                _gc.Width = 180;
            //        }

            //        if (gvCollectedData.Columns.Count > 0)
            //            gvCollectedData.Columns["Date"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;

            //        gvCollectedData.OptionsFind.AllowFindPanel = true;
            //        gvCollectedData.OptionsFind.AlwaysVisible = true;
            //        gvCollectedData.OptionsView.ShowGroupPanel = false;
            //        gvCollectedData.OptionsView.RowAutoHeight = true;
            //        gvCollectedData.OptionsView.ColumnAutoWidth = false;
            //        gvCollectedData.OptionsSelection.MultiSelect = true;
            //        gvCollectedData.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CellSelect;
            //        gvCollectedData.LeftCoord = 0;
            //    }
            //}
            //catch (Exception e) {
            //    NotificationDialog.Error("Bright Sales", e.InnerException.Message);
            //}

            //gvCollectedData.ClearColumnsFilter();
            //gvCollectedData.BestFitColumns();
            //if (gvCollectedData.Columns.Count > 0)
            //    gvCollectedData.Columns["Answer"].Width = 400;

            //gcCollectedData.EndUpdate();
        }
        private void GetCollectedData()
        {
            this.RunAssync(() => {
                try {
                    bool _CustomerOwned = true;
                    bool _BrightvisionOwned = true;
                    int _CustomerId = CustomerId;

                    if (UserSession.CurrentUser.IsSubCampaignSales && UserSession.CurrentUser.IsCustomerUser)
                        _BrightvisionOwned = false;

                    btnReload.Enabled = false;
                    gcCollectedData.Enabled = false;
                    gcCollectedData.BeginUpdate();
                    gvCollectedData.Columns.Clear();
                    gcCollectedData.DataSource = null;
                    gcCollectedData.DataSource = ObjectCollectedData.GetCollectedData(SubCampaignId, AccountId, null, _CustomerId, _CustomerOwned, _BrightvisionOwned);

                    if (gvCollectedData.Columns.Count > 0) {
                        GridColumn _gc;
                        for (int x = 0; x < gvCollectedData.Columns.Count; ++x) {
                            _gc = gvCollectedData.Columns[x];
                            if (_gc.FieldName == "AccountID" || _gc.FieldName == "ContactID")
                                _gc.Visible = false;

                            _gc.OptionsColumn.AllowEdit = true;
                            _gc.OptionsColumn.AllowFocus = true;
                            if (_gc.FieldName == "Answer") {
                                var memEdit = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
                                _gc.ColumnEdit = memEdit;
                                _gc.Width = 230;
                            }
                            if (_gc.FieldName == "Question")
                                _gc.Width = 180;
                        }

                        if (gvCollectedData.Columns.Count > 0)
                            gvCollectedData.Columns["Date"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;

                        gvCollectedData.OptionsFind.AllowFindPanel = true;
                        gvCollectedData.OptionsFind.AlwaysVisible = true;
                        gvCollectedData.OptionsView.ShowGroupPanel = false;
                        gvCollectedData.OptionsView.RowAutoHeight = true;
                        gvCollectedData.OptionsView.ColumnAutoWidth = false;
                        gvCollectedData.OptionsSelection.MultiSelect = true;
                        gvCollectedData.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CellSelect;
                        gvCollectedData.LeftCoord = 0;
                    }
                }
                catch (Exception e) {
                    NotificationDialog.Error("Bright Sales", e.InnerException.Message);
                }

                gvCollectedData.ClearColumnsFilter();
                gvCollectedData.BestFitColumns();
                if (gvCollectedData.Columns.Count > 0)
                    gvCollectedData.Columns["Answer"].Width = 400;

                gcCollectedData.EndUpdate();
                btnReload.Enabled = true;
                gcCollectedData.Enabled = true;
            });
        }

        private void m_bwAnswerData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            gcCollectedData.EndUpdate();
            gcCollectedData.Enabled = true;
            btnReload.Enabled = true;
        }
        private void m_bwAnswerData_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate  {
                bool _CustomerOwned = true;
                bool _BrightvisionOwned = true;
                int _CustomerId = CustomerId;

                if (UserSession.CurrentUser.IsSubCampaignSales && UserSession.CurrentUser.IsCustomerUser)
                    _BrightvisionOwned = false;

                gcCollectedData.DataSource = ObjectCollectedData.GetCollectedData(SubCampaignId, AccountId, null, _CustomerId, _CustomerOwned, _BrightvisionOwned);
                if (gvCollectedData.Columns.Count > 0) {
                    GridColumn _gc;
                    for (int x = 0; x < gvCollectedData.Columns.Count; ++x) {
                        _gc = gvCollectedData.Columns[x];
                        if (_gc.FieldName == "AccountID" || _gc.FieldName == "ContactID")
                            _gc.Visible = false;

                        _gc.OptionsColumn.AllowEdit = true;
                        _gc.OptionsColumn.AllowFocus = true;
                        if (_gc.FieldName == "Answer")
                        {
                            var memEdit = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
                            _gc.ColumnEdit = memEdit;
                            _gc.Width = 230;
                        }
                        if (_gc.FieldName == "Question")
                            _gc.Width = 180;
                    }

                    if (gvCollectedData.Columns.Count > 0)
                        gvCollectedData.Columns["Date"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;

                    gvCollectedData.OptionsFind.AllowFindPanel = true;
                    gvCollectedData.OptionsFind.AlwaysVisible = true;
                    gvCollectedData.OptionsView.ShowGroupPanel = false;
                    gvCollectedData.OptionsView.RowAutoHeight = true;
                    gvCollectedData.OptionsView.ColumnAutoWidth = false;
                    gvCollectedData.OptionsSelection.MultiSelect = true;
                    gvCollectedData.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CellSelect;
                    gvCollectedData.LeftCoord = 0;

                    gvCollectedData.ClearColumnsFilter();
                    gvCollectedData.BestFitColumns();
                    if (gvCollectedData.Columns.Count > 0)
                        gvCollectedData.Columns["Answer"].Width = 400;
                }
            }));
        }

        //private void FilterCollectedData()
        //{
        //    WaitDialog.Show(ParentForm, "Loading collected data...");
        //    bool? customerowned = null;
        //    bool? bvowned = null;
        //    int custid = 0;
        //    try
        //    {
        //        custid = CustomerId;
        //        //if (cboCustomerFilter.SelectedIndex == 0) // Customer (Yes)
        //        //    customerowned = true;
        //        //else if (cboCustomerFilter.SelectedIndex == 1) // Customer (No)
        //        //    customerowned = false;

        //        //if (cboPublicFilter.SelectedIndex == 0) // Public (Yes)
        //        //    bvowned = true;
        //        //else if (cboPublicFilter.SelectedIndex == 1) // Public (No)
        //        //    bvowned = false;

        //        //if (cbxCustomerOwned.Checked)
        //        //    customerowned = true;

        //        //if (cbxBrightvisionOwned.Checked)
        //        //    bvowned = true;

        //        /**
        //         * continue next increment.
        //         */
                
        //        customerowned = true;
        //        bvowned = true;
        //        /**/
        //        if (UserSession.CurrentUser.IsSubCampaignSales && UserSession.CurrentUser.IsCustomerUser)
        //            bvowned = false;
        //        /**/
                

        //        //CollectedDataResult = CollectedData.GetCollectedData(SubCampaignId, AccountId);
        //        DataTable dt = null;
        //        //if (CollectedDataFilterByContact)
        //        //    dt = ObjectCollectedData.GetCollectedData(SubCampaignId, AccountId, ContactId, custid, customerowned, bvowned);
        //        //else
        //        dt = ObjectCollectedData.GetCollectedData(SubCampaignId, AccountId, null, custid, customerowned, bvowned);
        //        //if (CollectedDataFilterByContact)
        //        //    dt = ObjectCollectedData.GetCollectedData(CampaignId, AccountId, ContactId, custid, customerowned, bvowned);
        //        //else
        //        //    dt = ObjectCollectedData.GetCollectedData(CampaignId, AccountId, null, custid, customerowned, bvowned);

        //            //gvCollectedData.Columns.Clear();
        //            //gcCollectedData.DataSource = null;
        //            //gcCollectedData.DataSource = dt;

        //        try {
        //            gcCollectedData.BeginUpdate();
        //            gvCollectedData.Columns.Clear();
        //            gcCollectedData.DataSource = null;
        //            gcCollectedData.DataSource = dt;
        //        }
        //        finally {
        //            gcCollectedData.EndUpdate();
        //        }

        //        if (gvCollectedData.Columns.Count > 0) {
        //            GridColumn gc;
        //            for (int x = 0; x < gvCollectedData.Columns.Count; ++x) {
        //                gc = gvCollectedData.Columns[x];
        //                if (gc.FieldName == "AccountID" || gc.FieldName == "ContactID")
        //                    gc.Visible = false;

        //                gc.OptionsColumn.AllowEdit = true;
        //                gc.OptionsColumn.AllowFocus = true;
        //                if (gc.FieldName == "Answer") {
        //                    var memEdit = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
        //                    gc.ColumnEdit = memEdit;
        //                    gc.Width = 230;
        //                }
        //                if (gc.FieldName == "Question") 
        //                    gc.Width = 180;

        //                //gc.OptionsColumn.FixedWidth = false;
        //                //gc.BestFit();                        
        //            }

        //            if (gvCollectedData.Columns.Count > 0)
        //                gvCollectedData.Columns["Date"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;

        //            gvCollectedData.OptionsFind.AllowFindPanel = true;
        //            gvCollectedData.OptionsFind.AlwaysVisible = true;
        //            gvCollectedData.OptionsView.ShowGroupPanel = false;
        //            gvCollectedData.OptionsView.RowAutoHeight = true;
        //            gvCollectedData.OptionsView.ColumnAutoWidth = false;
        //            gvCollectedData.OptionsSelection.MultiSelect = true;
        //            gvCollectedData.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CellSelect;
        //            //gvCollectedData.Columns["Date"].Group();
        //            //gvCollectedData.Columns["Contact Name"].Group();
        //            //gvCollectedData.Columns["Contact Title"].Group();
        //            //gvCollectedData.Columns["Contact Roles"].Group();
        //            //gvCollectedData.Columns["Customer>Campaign>SubCampaign"].Group();
        //            //gvCollectedData.ExpandAllGroups();
        //            gvCollectedData.LeftCoord = 0;

        //        }
        //    }
        //    catch (Exception e) {
        //        NotificationDialog.Error("Bright Sales", e.InnerException.Message);
        //    }

        //    string sColumnFilter = string.Empty;
        //    gvCollectedData.ClearColumnsFilter();

        //    //if (cbxCustomerOwned.Checked && cbxBrightvisionOwned.Checked)
        //    //    sColumnFilter = "[CustomerOwned] = " + true + " OR [Public] = " + true;

        //    //else if (cbxCustomerOwned.Checked && !cbxBrightvisionOwned.Checked)
        //    //    sColumnFilter = "[CustomerOwned] = " + true;

        //    //else if (!cbxCustomerOwned.Checked && cbxBrightvisionOwned.Checked)
        //    //    sColumnFilter = "[Public] = " + true;

        //    //else
        //    //    sColumnFilter = "[CustomerOwned] = " + false + " AND [Public] = " + false;
        //    //sColumnFilter = "[Public] = " + false;

        //    /** /
        //    // no need since we already filter from the query.
            
        //    if (cbxBrightvisionOwned.Checked) {
        //        sColumnFilter = "[Public] = " + cbxBrightvisionOwned.Checked;
        //        gvCollectedData.ActiveFilterString = sColumnFilter;
        //    }
        //    else
        //        gvCollectedData.ActiveFilterString = string.Empty;
        //    /**/

        //    gvCollectedData.BestFitColumns();
        //    if (gvCollectedData.Columns.Count > 0)
        //        gvCollectedData.Columns["Answer"].Width = 400;

        //    WaitDialog.Close();

        //    //if (cbxCustomerOwned.Checked)
        //    //    gvCollectedData.Columns["CustomerOwned"].FilterInfo = new ColumnFilterInfo("[CustomerOwned] = " + true);

        //    //if (cbxBrightvisionOwned.Checked)
        //    //    gvCollectedData.Columns["BVOwned"].FilterInfo = new ColumnFilterInfo("[BVOwned] = " + true);

        //    //gvCompanyAndContact.Columns["last_user"].FilterInfo = new ColumnFilterInfo("[last_user] like '" + UserSession.CurrentUser.UserFullName + "'");

        //    //if (CollectedDataFilterByContact)
        //    //    gvCollectedData.Columns["contact_id"].FilterInfo = new ColumnFilterInfo("[contact_id] = " + ContactId);

        //    //gvCollectedData.ExpandAllGroups();
        //}
        #endregion

        #region Public Methods
        public void Activate()
        {
            this.Enabled = true;
        }
        public void Clear()
        {
            gcCollectedData.DataSource = null;
            this.Enabled = false;
        }
        public void SetContactCollectedDataButton(bool ControlStatus)
        {
            cmdShowSelectedData.Enabled = ControlStatus;
        }
        //public void LoadCollectedData(int SubCampaignId, int? AccountId, int? ContactId)
        //public void LoadCollectedData()
        //{
        //    try
        //    {
        //        //CollectedDataResult = CollectedData.GetCollectedData(SubCampaignId, AccountId);
        //        gcCollectedData.DataSource = null;
        //        DataTable dt = null;
        //        if (CollectedDataFilterByContact)
        //            dt = ObjectCollectedData.GetCollectedData(SubCampaignId, AccountId, ContactId, CustomerId, true, true);
        //        else
        //            dt = ObjectCollectedData.GetCollectedData(SubCampaignId, AccountId, null, CustomerId, true, true);
        //        gcCollectedData.DataSource = dt;
        //        if (gvCollectedData.Columns.Count > 0)
        //        {
        //            GridColumn gc;
        //            for (int x = 0; x < gvCollectedData.Columns.Count; ++x)
        //            {
        //                gc = gvCollectedData.Columns[x];
        //                gc.OptionsColumn.AllowEdit = true;
        //                gc.OptionsColumn.AllowFocus = true;
        //                gc.OptionsColumn.ReadOnly = true;
                        
        //                if (gc.FieldName == "AccountID" || gc.FieldName == "ContactID")
        //                    gc.Visible = false;

        //                if (gc.FieldName == "Answer")
        //                {
        //                    var memEdit = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
        //                    gc.ColumnEdit = memEdit;
        //                    gc.Width = 660;
        //                }
        //                if (gc.FieldName == "Question") gc.Width = 220;
        //                else
        //                    gc.BestFit();
        //                //gc.OptionsColumn.FixedWidth = false;
        //                //gc.BestFit();                        
        //            }
        //            gvCollectedData.OptionsFind.AllowFindPanel = true;
        //            gvCollectedData.OptionsFind.AlwaysVisible = true;
        //            gvCollectedData.OptionsView.ShowGroupPanel = false;
        //            gvCollectedData.OptionsView.RowAutoHeight = true;
        //            gvCollectedData.OptionsView.ColumnAutoWidth = false;
        //            gvCollectedData.OptionsSelection.MultiSelect = true;
        //            gvCollectedData.OptionsSelection.EnableAppearanceFocusedCell = true;
        //            gvCollectedData.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CellSelect;

        //            gvCollectedData.Columns["Date"].Group();
        //            gvCollectedData.Columns["Contact Name"].Group();
        //            gvCollectedData.Columns["Contact Title"].Group();
        //            gvCollectedData.Columns["Contact Roles"].Group();
        //            gvCollectedData.Columns["Customer>Campaign>SubCampaign"].Group();
        //            gvCollectedData.Columns["Dialog-ID"].Group();

        //            gvCollectedData.ExpandAllGroups();
        //            gvCollectedData.LeftCoord = 0;
        //        }


        //        //this.FilterCollectedData();
        //    }
        //    catch (Exception e)
        //    {
        //        NotificationDialog.Error("Bright Sales", e.InnerException.Message);
        //    }
        //}
        public void Show()
        {
            bool _LoadData = false;
            if (m_PreviousSubCampaignId != SubCampaignId)
                _LoadData = true;
            else if (m_PreviousAccountId != AccountId)
                _LoadData = true;
            else if (m_PreviousCustomerId != CustomerId)
                _LoadData = true;
            else if (CollectedDataFilterByContact && m_PreviousContactId != ContactId)
                _LoadData = true;

            m_PreviousSubCampaignId = SubCampaignId;
            m_PreviousAccountId = AccountId;
            m_PreviousCustomerId = CustomerId;
            if (!_LoadData)
                return;

            if (CollectedDataFilterByContact && ContactId != null)
                m_PreviousContactId = (int)ContactId;
            //else if (ContactId == null) return;
            else
                m_PreviousContactId = 0;

            if (AccountId > 0)
                this.GetCollectedData();

            //backWorkerFilterCollectedData.RunWorkerAsync();
            //this.m_ThreadFilterCollectedData = new Thread(new ThreadStart(this.LoadFilterCollectedData));
            //this.m_ThreadFilterCollectedData.Start();
        }
        //private void LoadFilterCollectedData()
        //{
        //    if (gcCollectedData.InvokeRequired)
        //    {
        //        CallFilterCollectedData d = new CallFilterCollectedData(LoadFilterCollectedData);
        //        this.Invoke(d);
        //    }
        //    else
        //    {
        //        this.GetCollectedData();
        //    }
        //}
        public void SetAsReadOnly(bool pState)
        {
            //cmdShowSelectedData.Enabled = !pState;
            //cmdShowAllData.Enabled = !pState;
            btnReload.Enabled = !pState;
            //cbxCustomerOwned.Enabled = !pState;
            cbxBrightvisionOwned.Enabled = !pState;
        }
        public void Reload()
        {
            this.GetCollectedData();
        }
        #endregion

        #region Object Events
        private void cbxBrightvisionOwned_CheckedChanged(object sender, EventArgs e)
        {
            this.GetCollectedData();
        }
        private void cbxCustomerOwned_CheckedChanged(object sender, EventArgs e)
        {
            this.GetCollectedData();
        }
        private void cmdShowSelectedData_Click(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Loading collected data...");
            CollectedDataFilterByContact = true;
            //cmdShowSelectedData.Enabled = false;
            //cmdShowAllData.Enabled = true;
            //this.LoadCollectedData();
            this.GetCollectedData();
            WaitDialog.Close();
            //this.FilterCollectedData();
        }
        private void cmdShowAllData_Click(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Loading collected data...");
            CollectedDataFilterByContact = false;
            //cbxCustomerOwned.Checked = true;
            cbxBrightvisionOwned.Checked = true;
            //cmdShowSelectedData.Enabled = true;
            //cmdShowAllData.Enabled = false;
            //this.LoadCollectedData();
            this.GetCollectedData();
            WaitDialog.Close();
        }
        private void btnReload_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Reloading data.");
            this.GetCollectedData();
            WaitDialog.Close();
        }
        
        private void gvCollectedData_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            SalesConsultant.Business.BrightSalesGridUtility.CreateGridContextMenu(view, e);
        }
        private void gvCollectedData_EndGrouping(object sender, EventArgs e) {
            var view = sender as GridView;
            if (view != null) {
                view.ExpandAllGroups();
                var gc = view.Columns.ColumnByFieldName("Answer");
                gc.ColumnEdit.AllowFocused = true;
                
                if (gc != null) gc.MinWidth = 660;
                gc = view.Columns.ColumnByFieldName("Question");
                if (gc != null) gc.MinWidth = 220;
            }
        }
        private void gvCollectedData_CellMerge(object sender, CellMergeEventArgs e)
        {
            if (e.CellValue1.ToString() == e.CellValue2.ToString())
                e.Merge = true;
            else
                e.Merge = false;

            e.Handled = true;
        }
        #endregion
    }
}
