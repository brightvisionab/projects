
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Linq;
using System.Data.Objects;
using System.Data.Common;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Configuration;
using MSDTS = Microsoft.SqlServer.Dts.Runtime;

using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using SalesConsultant.Business;
using SalesConsultant.Forms;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using BrightVision.Storage.Queue;
using BrightVision.Storage.Models;
using BrightVision.Storage.Repositories;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.SqlServer.Dts.Runtime;
using BrightVision.Common.Events.Core;
using SalesConsultant.Facade;
using SalesConsultant.PublicProperties;
using SalesConsultant.Events;

namespace SalesConsultant.Modules
{
    public partial class ManageImportList : XtraUserControl
    {
        #region Enums
        //private enum eViewSource
        //{
        //    ImportList,
        //    ImportProfileList
        //}

        private enum eContactMatchType
        {
            ByName,
            ByEmail,
            ByManual
        }
        #endregion

        #region Private Members
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;

        private NewImport m_objFrmNewImport = null;
        private PopupDialog m_objPopupDialog = null;
        private GridView m_objGridView = null;
        private CTImportList m_objImportList = null;
        //private ObjectImport.ImportListInstance m_objImportProfileList = null;
        private List<DataImportUtility.ImportFileRowInstance> m_objImportFileRowList = null;
        private DataTable m_objImportFileRecord = new DataTable();
        private DataTable m_objProfiledDataList = new DataTable();
        private ObjectQuery m_objImportFileHeaders = null;
        private string m_MessageBoxCaption = "Manager Application - Import List";
        private Dictionary<string, int> objProfilingList = null;
        //private List<string> m_objMasterDataList = null;
        private List<int> m_objProfiledDataHeaderIndexes = null;
        //private DbTransaction m_objDbTransaction = null;
        private BrightPlatformEntities m_objBrightPlatformEntity = null;
        private SqlConnection m_objConnection = null;
        private SqlTransaction m_objTransaction = null;

        private SSISPackageQueue m_oPackageQueue = null;
        private UserTextNotificationRepository m_oRepository = null;

        private int m_SelectedImportFileId = 0;
        private bool m_IsLoading = false;
        private BrightPlatformEntities m_objDbModelProfiling = new BrightPlatformEntities(UserSession.EntityConnection);
        private int m_SelectedImportFileRowHandle = 0;

        /**
         *  m_MatchByAccountType:    
         *  0 = match both validated and unvalidated accounts   non-valid
         *  1 = match by validated accounts only                valid
         *  2 = match by unvalidated accounts only              both
         */
        private short m_MatchByAccountType = 1;
        #endregion

        #region Contructors
        public ManageImportList()
        {
            this.Visible = false;
            InitializeComponent();
            tbxMaxRow.Text = "10";
            this.lcContents.AllowCustomizationMenu = false;
            this.PopulateImportListView();
            //this.PopulateImportProfilingView();
            this.SetImportProfilingControls(false);
            this.SetProfileDataControls(false);
            this.SetImportListViewContextMenu();
            this.SetImportRecordViewContextMenu();
            this.Visible = true;

            m_oPackageQueue = new SSISPackageQueue();
            m_oRepository = new UserTextNotificationRepository();
            this.RegisterEvents();
        }
        #endregion

        #region Object Control Events
        private void ManageImportList_Load(object sender, EventArgs e)
        {
            trackBarControlConfidence.EditValue = 0;
            trackBarControlSimilarity.EditValue = 0;
            comboBoxEditConfidence.SelectedIndex = 0;
            comboBoxEditSimilarity.SelectedIndex = 0;

            //trackBarControlConfidenceContacts.EditValue = 0;
            //trackBarControlSimilarityContacts.EditValue = 0;
            //comboBoxEditConfidenceContacts.SelectedIndex = 0;
            //comboBoxEditSimilarityContacts.SelectedIndex = 0;
        }

        private void cmdAccountMatchingHelp_Click(object sender, EventArgs e)
        {
            string Message = "Each match includes a SIMILIARITY score and a CONFIDENCE score." + Environment.NewLine + Environment.NewLine
                           + "The SIMILARITY score is a mathematical measure of the textural similarity between the input record and the record that Fuzzy Lookup transformation returns from the reference table." + Environment.NewLine + Environment.NewLine
                           + "The CONFIDENCE score is a measure of how likely it is that a particular value is the best match among the matches found in the reference table." + Environment.NewLine + Environment.NewLine
                           + "The confidence score assigned to a record depends on the other matching records that are returned. For example, matching St. and Saint returns a low similarity score regardless of other matches." + Environment.NewLine + Environment.NewLine
                           + "If Saint is the only match returned, the confidence score is high. If both Saint and St. appear in the reference table, the confidence in St. is high and the confidence in Saint is low."
                           + "However, high similarity may not mean high confidence. For example, if you are looking up the value Chapter 4, the returned results Chapter 1, Chapter 2, and Chapter 3 have a high similarity score." + Environment.NewLine + Environment.NewLine
                           + "But a low confidence score because it is unclear which of the results is the best match.";

            MessageBox.Show(Message, "Account Matching Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cmdNewImport_Click(object sender, EventArgs e)
        {
            this.DisplayNewImportForm();
        }

        private void cmdLoadData_Click(object sender, EventArgs e)
        {
            if (gvImportFile.RowCount < 1)
                return;
            
            WaitDialog.Show(ParentForm, "Loading import data...");
            this.SetFocusedViewInstance();
            this.PopulateImportRecordView(m_objImportList.id);
            lblImportFileTotalRecords.Text = "Total Records: " + gvImportRecord.RowCount.ToString();
           WaitDialog.Close();
        }

        private void gvImportFile_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.InitImportFileItemView();
            this.InitFuzzyLookupAccountMatchingView();
            this.InitFuzzyLookupContactMatchingView();
            this.ClearProfilingHeaderDropdown();
            this.InitProfiledDataView();
            this.SetImportProfilingControls(false);
            this.SetProfileDataControls(false);
            
            if (gvImportFile.RowCount < 1)
            {
                m_SelectedImportFileId = 0;
                return;
            }

            this.SetFocusedViewInstance();                                   
            //if (tcManageImport.SelectedTabPage.Name.Equals(tpProfiling.Name))
            //{
            //    this.ClearProfilingHeaderDropdown();
            //    this.InitProfiledDataView();
            //    this.SetImportProfilingControls(false);
            //    this.SetProfileDataControls(false);
            //}

            m_SelectedImportFileRowHandle = e.FocusedRowHandle;
        }

        private void cmdEditExport_Click(object sender, EventArgs e)
        {
            this.DisplayEditImportFileForm();
        }

        private void cmdDeleteImportFile_Click(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(m_objImportList.active))
            {
                DialogResult objResult = new DialogResult();
                string Message = "Are you sure to delete this import file? This will also delete all profiled, account matching, and contact matching data for this import file.";
                objResult = MessageBox.Show(Message, m_MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (objResult == DialogResult.No)
                    return;

                this.DeleteImportFile(m_objImportList.id);
                this.PopulateImportListView();
                MessageBox.Show("Successfully deleted import file!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("Delete not allowed for active import file", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void cmdEditProfiling_Click(object sender, EventArgs e)
        {
            
            DialogResult objResult = new DialogResult();
            if (Convert.ToBoolean(m_objImportList.profiled))
            {
                objResult = MessageBox.Show
                (
                    "This import file has already been profiled. Are you sure to edit this record? Upon saving, this will overwrite all existing profiled data related to this import file.",
                    m_MessageBoxCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (objResult == DialogResult.Yes)
                {
                    WaitDialog.Show(ParentForm, "Loading column maps...");
                    this.SetImportProfilingControls(true);
                    this.GetImportFileHeaders(m_objImportList.id);
                    this.LoadProfileMapping();
                    //this.LoadProfileAndData();
                    cmdGenerate.Enabled = true;
                    vgcProfilingDetail.OptionsBehavior.Editable = true;
                    WaitDialog.Close();
                }
            }
            else
            {
                WaitDialog.Show(ParentForm, "Loading column maps...");
                this.SetImportProfilingControls(true);
                this.GetImportFileHeaders(m_objImportList.id);
                this.LoadProfileMapping();
                //this.LoadProfileAndData();
                cmdGenerate.Enabled = true;
                vgcProfilingDetail.OptionsBehavior.Editable = true;
                WaitDialog.Close();
            }
        }

        private void cmdLoadProfileAndData_Click(object sender, EventArgs e)
        {
            if (gvImportFile.RowCount < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Loading profiled data...");
            this.SetImportProfilingControls(true);
            this.GetImportFileHeaders(m_objImportList.id);
            this.LoadProfileAndData();

            // force user to press edit first
            cmdGenerate.Enabled = false;
            vgcProfilingDetail.OptionsBehavior.Editable = false;
            gcProfiledData.Enabled = true;
            lblProfilingTotalRecords.Text = "Total Records: " + gvProfiledData.RowCount.ToString();
            WaitDialog.Close();
        }

        private void cmdGenerate_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Processing profiled data...");
            this.GetProfiledDataList();
            this.PopulateProfiledDataView(m_objImportList.id);
            this.SetProfileDataControls(true);
            WaitDialog.Close();
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            if (gvProfiledData.RowCount < 1)
            {
                MessageBox.Show("No records to profile.", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult objChoice = MessageBox.Show("Are you sure to save this profiled data?", "Import Profiling", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objChoice == DialogResult.No)
                return;

            WaitDialog.Show(ParentForm, "Saving...");
            m_objDbModelProfiling.SaveChanges(); // save profiled data column mappings
            this.SaveProfiledData();
            this.CreateFuzzyData();
            this.SetImportProfilingControls(false);
            this.SetProfileDataControls(false);
            this.SetImportFileProfiledFlag(m_objImportList.id);
            this.InitProfiledDataView();
            //this.PopulateImportListView();
            WaitDialog.Close();
        }

        private void miImportListPrintPreview_Click(object sender, EventArgs e)
        {
            gcImportFile.ShowPrintPreview();
        }

        private void miImportRecordPrintPreview_Click(object sender, EventArgs e)
        {
            gcImportRecord.ShowPrintPreview();
        }
        
        private void tcManageImport_SelectedPageChanging(object sender, DevExpress.XtraTab.TabPageChangingEventArgs e) {
            if (e.Page.Name == "tpMatchAccounts" && e.PrevPage.Name != "tpMatchContacts") {
                this.SetFocusedViewInstance();
                //m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcImportFile.FocusedView;
                //m_objImportProfileList = (ObjectImport.ImportListInstance)m_objGridView.GetFocusedRow();

                if (m_objImportList != null)
                {
                    if (m_objImportList.id != current_profile_list_id)
                    {
                        if (m_SelectedImportFileRowHandle != gvImportFile.FocusedRowHandle)
                            gcMatchingCompany.DataSource = null;

                        //gcMatchingCompany.DataSource = null;
                        //gridControlSimilarity.DataSource = null;
                        //gridControlSimilarityContacts.DataSource = null;
                        //gridControlConfidenceContacts.DataSource = null;
                    }
                }
            }
        }

        private void cbxActive_CheckedChanged(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Updating...");
            DevExpress.XtraEditors.CheckEdit objCheckBox = sender as DevExpress.XtraEditors.CheckEdit;
            this.SetFocusedViewInstance();

            if (objCheckBox.Checked)
                this.SetImportFileStatus(true);
            else
                this.SetImportFileStatus(false);

            WaitDialog.Close();
            MessageBox.Show("Successfully updated import file!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRefreshGrid_Click(object sender, EventArgs e)
        {
            if (gvImportFile.RowCount < 1)
                return;

            WaitDialog.Show("Reloading list ...");
            CTImportList _item = gvImportFile.GetFocusedRow() as CTImportList;
            this.gvImportFile.FocusedRowChanged -= new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvImportFile_FocusedRowChanged);
            this.PopulateImportListView(_item.id);
            this.gvImportFile.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gvImportFile_FocusedRowChanged);
            alertControlSSISPackage.Show(this.ParentForm, "Bright Manager", "Import list reloaded.");
            WaitDialog.Close();
        }
        private void btnCheckItemFuzzyMatchStatus_Click(object sender, EventArgs e)
        {
            if (gvImportFile.RowCount < 1)
                return;

        }
        private void btnCancelItemFuzzyMatch_Click(object sender, EventArgs e)
        {
            if (gvImportFile.RowCount < 1)
                return;

            WaitDialog.Show("Cancelling process ...");
            CTImportList _item = gvImportFile.GetFocusedRow() as CTImportList;
            if (_item.fuzzy_match_status.Equals("Done")) {
                WaitDialog.Close();
                return;
            }
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efDbContext.FICancelFuzzyMatch(_item.id);
            }
            gvImportFile.SetRowCellValue(gvImportFile.FocusedRowHandle, "fuzzy_match_status", string.Empty);
            alertControlSSISPackage.Show(this.ParentForm, "Bright Manager", "Fuzzy match cancelled.");
            WaitDialog.Close();
        }
        private void alertControlSSISPackage_BeforeFormShow(object sender, DevExpress.XtraBars.Alerter.AlertFormEventArgs e)
        {
            DevExpress.Skins.Skin currentSkin;
            currentSkin = DevExpress.Skins.BarSkins.GetSkin(e.AlertForm.LookAndFeel);
            DevExpress.Skins.SkinElement element;
            element = currentSkin["AlertWindow"];
            Graphics g = Graphics.FromImage(element.Image.Image);
            g.FillRectangle(Brushes.PaleGreen, new Rectangle(0, 0, element.Image.Image.Width, element.Image.Image.Height));
        }
        #endregion

        #region Public Functions
        public void PopulateImportListView(int ImportFileId = 0)
        {
            if (Convert.ToInt32(tbxMaxRow.Text) < 1) {
                NotificationDialog.Warning("Bright Sales", "Invalid max row setting.");
                return;
            }

            try {
                if (ImportFileId > 0)
                    m_SelectedImportFileId = ImportFileId;

                m_IsLoading = true;
                lblImportFileTotalRecords.Text = "Total Records: 0";
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                    gcImportRecord.BeginUpdate();
                    gcImportFile.BeginUpdate();
                    gcImportRecord.DataSource = null;
                    gcImportFile.DataSource = null;
                    gcImportFile.DataSource = _efDbContext.FIGetImportLists(Convert.ToInt32(tbxMaxRow.Text)).ToList();
                    gvImportFile.BestFitColumns();
                    gcImportRecord.EndUpdate();
                    gcImportFile.EndUpdate();
                }
               
                //gcImportFile.DataSource = DataImportUtility.GetImportLists();
                //lblImportFileTotalRecords.Text = "Total Records: " + gvImportFile.RowCount.ToString();
                //gvImportFile.BestFitColumns();                
                this.SetDefaultSelectedRow();
                m_IsLoading = false;
            }
            catch (Exception ex) {
                MessageBox.Show("Error: " + ex.InnerException.Message, "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Private Functions
        private void RegisterEvents()
        {
            m_EventBus.GetEvent<FrmManualMatchAccountEvents.OnClose>().Subscribe(FrmManualMatchingAccount_OnClose);
        }
        private void FrmManualMatchingAccount_OnClose(FrmManualMatchAccountEvents.OnClose e)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                for (int i = 0; i < gvMatchingCompany.RowCount; i++) {

                    /**
                     * process only selected items.
                     */
                    if (!Convert.ToBoolean(gvMatchingCompany.GetRowCellValue(i, "selected")))
                        continue;

                    /**
                     * if no match found from the matching list,
                     * just continue to next one.
                     */
                    CTFuzzyLookupAccount _cxFuzzyAccount = gvMatchingCompany.GetRow(i) as CTFuzzyLookupAccount;
                    ClassesProperty.ManualMatchAccount _ManualMatchAccount = e.lstMatchAccounts.FirstOrDefault(p => p.fuzzy_lookup_account_id == _cxFuzzyAccount.id);
                    if (_ManualMatchAccount == null)
                        continue;

                    /**
                     * if selected item already had a match with the same company,
                     * then just continue to next record and let it as is.
                     */
                    fuzzy_lookup_accounts _eftFuzzyAccount = _efDbContext.fuzzy_lookup_accounts.FirstOrDefault(p => p.id == _ManualMatchAccount.fuzzy_lookup_account_id);
                    if (_ManualMatchAccount.is_match && _eftFuzzyAccount.account_id == _ManualMatchAccount.match_account_id)
                        continue;

                    /**
                     * we update fuzzy_lookup_accounts record and
                     * set default values for the grid columns.
                     */
                    _eftFuzzyAccount.account_id = _ManualMatchAccount.is_match? _ManualMatchAccount.match_account_id: 0;
                    _eftFuzzyAccount.matched = _ManualMatchAccount.is_match? true: false;
                    _eftFuzzyAccount.match_method = _ManualMatchAccount.is_match? "Manual Match": string.Empty;
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftFuzzyAccount);

                    gvMatchingCompany.SetRowCellValue(i, "account_id", 0);
                    gvMatchingCompany.SetRowCellValue(i, "company_name", string.Empty);
                    gvMatchingCompany.SetRowCellValue(i, "org_no", string.Empty);
                    gvMatchingCompany.SetRowCellValue(i, "validated", false);
                    gvMatchingCompany.SetRowCellValue(i, "matched", false);
                    gvMatchingCompany.SetRowCellValue(i, "match_method", string.Empty);
                    
                    /**
                     * theres no point continuing below if its not a match.
                     */
                    if (!_ManualMatchAccount.is_match)
                        continue;

                    /**
                     * we update grid columns with the manually matched company
                     * data and values.
                     */
                    account _eftAccount = _efDbContext.accounts.FirstOrDefault(p => p.id == _ManualMatchAccount.match_account_id);
                    gvMatchingCompany.SetRowCellValue(i, "account_id", _eftAccount.id);
                    gvMatchingCompany.SetRowCellValue(i, "company_name", _eftAccount.company_name);
                    gvMatchingCompany.SetRowCellValue(i, "org_no", _eftAccount.org_no);
                    gvMatchingCompany.SetRowCellValue(i, "validated", _eftAccount.validated);
                    gvMatchingCompany.SetRowCellValue(i, "matched", true);
                    gvMatchingCompany.SetRowCellValue(i, "match_method", "Manual Match");
                    //gvMatchingCompany.SetRowCellValue(i, "date_created", _eftAccount.created_date);
                    //gvMatchingCompany.SetRowCellValue(i, "date_modified", _eftAccount.modified_date);
                    //gvMatchingCompany.SetRowCellValue(i, "created_by", _eftAccount.crea);
                    _efDbContext.Detach(_eftAccount);
                }
            }
        }

        /// <summary>
        /// Create fuzzy data records
        /// </summary>
        private void CreateFuzzyData()
        {
            if (m_objImportList == null)
                return;

            DataImportUtility.CreateFuzzyData(m_objImportList.id);
        }

        /// <summary>
        /// Display edit import file form
        /// </summary>
        private void DisplayEditImportFileForm()
        {
            this.Cursor = Cursors.WaitCursor;
            this.SetFocusedViewInstance();
            EditImportFile objFrmImportFile = new EditImportFile(this.m_objImportList);
            objFrmImportFile.objParentControl = this;
            m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = m_MessageBoxCaption;
            m_objPopupDialog.Controls.Add(objFrmImportFile);
            m_objPopupDialog.ClientSize = new Size(objFrmImportFile.Width + 2, objFrmImportFile.Height + 2);
            m_objPopupDialog.ShowDialog(this.ParentForm);
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Display new import form
        /// </summary>
        private void DisplayNewImportForm()
        {
            m_objFrmNewImport = new NewImport();
            m_objFrmNewImport.m_objUserControl = this;
            m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = m_MessageBoxCaption;
            m_objPopupDialog.Controls.Add(m_objFrmNewImport);
            m_objPopupDialog.ClientSize = new Size(m_objFrmNewImport.Width + 2, m_objFrmNewImport.Height + 2);
            m_objPopupDialog.ShowDialog(this.ParentForm);
        }
        
        /// <summary>
        /// Save the profiled data list
        /// </summary>
        private void SaveProfiledData()
        {
            DataImportUtility.DeleteProfiledData(m_objImportList.id);
            m_objBrightPlatformEntity = null;
            m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            
            try
            {
                m_objConnection = new SqlConnection(UserSession.ProviderConnection);
                m_objConnection.Open();
                m_objTransaction = m_objConnection.BeginTransaction();
                DataImportUtility.ExecuteBulkProcessing("vw_profiled_data", m_objProfiledDataList, m_objConnection, m_objTransaction);
                m_objTransaction.Commit();
                //MessageBox.Show("Successfully saved profiled data to database", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                m_objTransaction.Rollback();
                MessageBox.Show("Transaction rolled back due to the ff:" + Environment.NewLine + ex.Message, m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            m_objTransaction.Dispose();
            m_objTransaction = null;
            m_objConnection.Close();
            m_objConnection.Dispose();
            m_objConnection = null;
        }

        /// <summary>
        /// Update the list import as profiled
        /// </summary>
        private void SetImportFileProfiledFlag(int ImportFileId)
        {
            DataImportUtility.SetImportFileProfiledStatus(ImportFileId);
            gvImportFile.SetFocusedRowCellValue("matched_by_account", true);
            gvImportFile.SetFocusedRowCellValue("profiled", true);
        }

        /// <summary>
        /// Process the profiled data list
        /// </summary>
        private void PopulateProfiledDataView(int ImportFileId)
        {
            this.InitProfiledDataView();
            m_objProfiledDataHeaderIndexes = null;
            m_objProfiledDataHeaderIndexes = new List<int>();
            List<int> objImportFileColumnsIndexes = new List<int>();

            // set data table for profiled data
            m_objProfiledDataList = null;
            m_objProfiledDataList = DataImportUtility.CreateProfiledDataTable();

            // get import file column headers, and set indexes for value processing
            ObjectQuery objImportFileColumns = DataImportUtility.GetImportFileColumns(ImportFileId, false);
            foreach (DataImportUtility.ImportFileHeaderIdInstance Item in objImportFileColumns)
                objImportFileColumnsIndexes.Add(Item.id);
            
            foreach (var Item in objProfilingList)
                m_objProfiledDataHeaderIndexes.Add(objImportFileColumnsIndexes.IndexOf(Item.Value));
            
            // process data table row values / records
            object[] ItemArray = null;
            DataRow ItemRow = null;
            m_objImportFileRowList = DataImportUtility.GetImportFileRows(ImportFileId);
            string ItemValue = string.Empty;

            foreach (DataImportUtility.ImportFileRowInstance Item in m_objImportFileRowList) {
                if (string.IsNullOrEmpty(Item.row_value.Trim()))
                    continue;

                ItemArray = null;
                ItemRow = m_objProfiledDataList.NewRow();
                ItemArray = Item.row_value.Split(new string[] { "[sep]" }, StringSplitOptions.None);

                ItemRow.SetField(0, ImportFileId);
                ItemRow.SetField(1, Item.row_order);

                for (int i = 0; i < m_objProfiledDataHeaderIndexes.Count; i++) {
                    if (m_objProfiledDataHeaderIndexes[i] >= 0 && m_objProfiledDataHeaderIndexes[i] < ItemArray.Count()) {
                        ItemValue = string.Empty;
                        ItemValue = ItemArray[m_objProfiledDataHeaderIndexes[i]].ToString();

                        if (i == 15 || i == 16) {
                            if (ValidationUtility.IsCurrency(ItemValue))
                                ItemRow.SetField(i + 2, Convert.ToDouble(ItemValue));
                            else
                                ItemRow.SetField(i + 2, 0);
                        }
                        else if (i == 17 || i == 18) {
                            if (ValidationUtility.IsCurrency(ItemValue))
                                ItemRow.SetField(i + 2, Convert.ToInt32(ItemValue));
                            else
                                ItemRow.SetField(i + 2, 0);
                        }
                        else {
                            /**
                             * [@jeff 07.02.2012]: https://brightvision.jira.com/browse/PLATFORM-1538
                             * impement validation to check for row values more than 255 chars.
                             * warn the user to check the profiling fields.
                             */
                            if (i < 47 && ItemValue.Length > 255) {
                                MessageBox.Show(
                                    string.Format(
                                        "A row in {0} contains more than 255 characters.{1}Please kindly check.",
                                        m_objProfiledDataList.Columns[i].ColumnName,
                                        Environment.NewLine
                                    ), 
                                    "Bright Manager", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Information
                                );
                                return;
                            }
                            ItemRow.SetField(i + 2, ItemValue);
                        }
                    }
                    else
                    {
                        // 15 - 18 are the money figures, must be set to 0
                        if (i == 15 || i == 16 || i == 17 || i == 18)
                            ItemRow.SetField(i + 2, 0);
                        else
                            ItemRow.SetField(i + 2, "");
                    }
                }

                m_objProfiledDataList.Rows.Add(ItemRow);
            }

            lblProfilingTotalRecords.Text = "Total Records: 0";
            gcProfiledData.DataSource = m_objProfiledDataList;
            lblProfilingTotalRecords.Text = "Total Records: " + m_objProfiledDataList.Rows.Count.ToString();
            gvProfiledData.Columns[0].Visible = false; // hide import file id
            gvProfiledData.Columns[1].Visible = false; // hide row id
            gvProfiledData.BestFitColumns();
        }

        /// <summary>
        /// Load profiled column mappings and records
        /// </summary>
        private void LoadProfileAndData()
        {
            if (m_objImportList == null)
                return;

            this.LoadProfileMapping();
            this.LoadProfiledDataRecords();
        }

        /// <summary>
        /// Load the existing profiled data records if has any
        /// </summary>
        private void LoadProfiledDataRecords()
        {
            if (m_objImportList == null)
                return;

            gcProfiledData.DataSource = null;
            DataTable ObjResult = DataImportUtility.GetProfiledData(m_objImportList.id);
            if (ObjResult.Rows.Count > 0)
            {
                gcProfiledData.DataSource = ObjResult;
                gvProfiledData.BestFitColumns();
            }
        }

        /// <summary>
        /// Load profiled data column maps to view
        /// </summary>
        private void LoadProfileMapping()
        {
            vgcProfilingDetail.Enabled = true;
            vgcProfilingDetail.DataSource = null;
            vgcProfilingDetail.DataSource = DataImportUtility.GetProfiledDataColumnMaps(m_objImportList.id, m_objDbModelProfiling).Execute(MergeOption.AppendOnly);
        }

        /** /
        /// <summary>
        /// Set profiling master data list
        /// </summary>
        private void SetMasterDataList()
        {
            m_objMasterDataList = null;
            m_objMasterDataList = new List<string>();
            m_objMasterDataList.Add("Company Name");
            m_objMasterDataList.Add("Company Organization #");
            m_objMasterDataList.Add("Company Box Address");
            m_objMasterDataList.Add("Company Street Address");
            m_objMasterDataList.Add("Company Zipcode");
            m_objMasterDataList.Add("Company City");
            m_objMasterDataList.Add("Company Country");
            m_objMasterDataList.Add("Company Telephone");
            m_objMasterDataList.Add("Company Telefax");
            m_objMasterDataList.Add("Company Site");
            m_objMasterDataList.Add("Company Year Established");
            m_objMasterDataList.Add("Parent Company");
            m_objMasterDataList.Add("Company Activity Code");
            m_objMasterDataList.Add("Company Activity Code Description");
            m_objMasterDataList.Add("Company Activity Code 2");
            m_objMasterDataList.Add("Company Activity Code 2 Description");
            m_objMasterDataList.Add("Company Currency");
            m_objMasterDataList.Add("Company Revenue"); //double
            m_objMasterDataList.Add("Company Result"); //double
            m_objMasterDataList.Add("Company Employees Abroad"); //int
            m_objMasterDataList.Add("Company Employees Total"); //int
            m_objMasterDataList.Add("Contact Firstname");
            m_objMasterDataList.Add("Contact Middlename");
            m_objMasterDataList.Add("Contact Lastname");
            m_objMasterDataList.Add("Contact Direct Phone");
            m_objMasterDataList.Add("Contact Mobile");
            m_objMasterDataList.Add("Contact Email");
            m_objMasterDataList.Add("Contact Title");
            m_objMasterDataList.Add("Contact Address 1");
            m_objMasterDataList.Add("Contact Address 2");
            m_objMasterDataList.Add("Contact City");
            m_objMasterDataList.Add("Contact Zipcode");
            m_objMasterDataList.Add("Contact Country");
            m_objMasterDataList.Add("BV Account Id");
            m_objMasterDataList.Add("BV Contact Id");
            m_objMasterDataList.Add("Customer List Priority");
            m_objMasterDataList.Add("Customer List Comment");
            m_objMasterDataList.Add("Customer Field 1");
            m_objMasterDataList.Add("Customer Field 2");
            m_objMasterDataList.Add("Customer Field 3");
            m_objMasterDataList.Add("Customer Field 4");
            m_objMasterDataList.Add("Customer Field 5");
            m_objMasterDataList.Add("Customer Field 6");
            m_objMasterDataList.Add("Customer Field 7");
            m_objMasterDataList.Add("Customer Field 8");
            m_objMasterDataList.Add("Customer Field 9");
            m_objMasterDataList.Add("Customer Field 10");
        }
        /**/

        /// <summary>
        /// Get profiled data list
        /// </summary>
        private void GetProfiledDataList()
        {
            //this.SetMasterDataList();
            objProfilingList = null;
            objProfilingList = new Dictionary<string, int>();
            int ImportFileHeaderId = 0;

            for (int i = 0; i < vgcProfilingDetail.Rows.Count; i++) {
                for (int j = 0; j < vgcProfilingDetail.Rows[i].ChildRows.Count; j++) {
                    ImportFileHeaderId = 0;
                    ImportFileHeaderId = vgcProfilingDetail.GetCellValue(vgcProfilingDetail.Rows[i].ChildRows[j], 0) == null ? 0 : Convert.ToInt32(vgcProfilingDetail.GetCellValue(vgcProfilingDetail.Rows[i].ChildRows[j], 0));
                    objProfilingList.Add(string.Format("{0}|{1}", i, j), ImportFileHeaderId);
                }
            }
        }

        /// <summary>
        /// Clear import file header dropdown for profiling
        /// </summary>
        private void ClearProfilingHeaderDropdown()
        {
            riImportFileHeader.Columns.Clear();
            riImportFileHeader.DataSource = null;
        }

        /// <summary>
        /// Get import file headers for profiling
        /// </summary>
        private void GetImportFileHeaders(int ImportFileId)
        {
            m_objImportFileHeaders = null;
            m_objImportFileHeaders = DataImportUtility.GetImportFileColumns(ImportFileId, true);

            this.ClearProfilingHeaderDropdown();
            riImportFileHeader.DataSource = m_objImportFileHeaders.Execute(MergeOption.AppendOnly);
            riImportFileHeader.DisplayMember = "column_name";
            riImportFileHeader.ValueMember = "id";
            riImportFileHeader.Columns.Add(new LookUpColumnInfo("column_name"));
            riImportFileHeader.ShowHeader = false;
            riImportFileHeader.ShowFooter = false;
        }

        /// <summary>
        /// Enables / disables controls for the profiled data view and button
        /// </summary>
        private void SetProfileDataControls(bool IsEnabled)
        {
            gcProfiledData.Enabled = IsEnabled;
            cmdSave.Enabled = IsEnabled;
        }

        /// <summary>
        /// Enables / disables controls for use in import profiling
        /// </summary>
        private void SetImportProfilingControls(bool IsEnabled)
        {
            vgcProfilingDetail.Enabled = IsEnabled;
            cmdGenerate.Enabled = IsEnabled;
        }

        /// <summary>
        /// Set the default selected row of the import file
        /// </summary>
        private void SetDefaultSelectedRow()
        {
            if (m_SelectedImportFileId == 0)
                return;

            int RowNo = 0;
            CTImportList ImportFile = null;
            GridView objGridView = (GridView) gcImportFile.FocusedView;
            for (int i = 0; i < objGridView.RowCount; i++)
            {
                ImportFile = objGridView.GetRow(i) as CTImportList;
                if (ImportFile.id == m_SelectedImportFileId)
                {
                    RowNo = i;
                    break;
                }
            }

            gvImportFile.FocusedRowHandle = RowNo;
        }

        /// <summary>
        /// Sets the focused view instance of the grid. Instantiates the objects that can be used for data manipulation.
        /// </summary>
        private void SetFocusedViewInstance()
        {
            m_objGridView = null;
            m_objImportList = null;
            m_objGridView = gcImportFile.FocusedView as GridView;
            m_objImportList = m_objGridView.GetFocusedRow() as CTImportList;

            if (!m_IsLoading)
                m_SelectedImportFileId = m_objImportList.id;
            
            //switch (ViewSource)
            //{
            //    case eViewSource.ImportList:
            //    {
            //        m_objImportList = null;
            //        m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcImportFile.FocusedView;
            //        m_objImportList = (ObjectImport.ImportListInstance) m_objGridView.GetFocusedRow();
            //        break;
            //    }

            //    case eViewSource.ImportProfileList:
            //    {
            //        m_objImportProfileList = null;
            //        m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcImportFile.FocusedView;
            //        m_objImportProfileList = (DataImportUtility.ImportListInstance) m_objGridView.GetFocusedRow();
            //        break;
            //    }
            //}
        }

        /// <summary>
        /// Clear the import file record items view
        /// </summary>
        private void InitProfiledDataView()
        {
            m_objProfiledDataList.Clear();
            m_objProfiledDataList.Columns.Clear();
            m_objProfiledDataList.Rows.Clear();
            gcProfiledData.DataSource = null;
            gcProfiledData.Refresh();
            gvProfiledData.Columns.Clear();
        }

        /// <summary>
        /// Clear the import file record items view
        /// </summary>
        private void InitImportFileItemView()
        {
            lblImportFileTotalRecords.Text = "Records: 0";
            m_objImportFileRecord.Clear();
            m_objImportFileRecord.Columns.Clear();
            m_objImportFileRecord.Rows.Clear();
            gcImportRecord.DataSource = null;
            gcImportRecord.Refresh();
            gvImportRecord.Columns.Clear();
        }

        /// <summary>
        /// Pupulates the import headers view
        /// </summary>
        private void PopulateImportRecordView(int ImportFileId)
        {
            this.InitImportFileItemView();
            
            // process data table column headers
            ObjectQuery objImportFileColumns = DataImportUtility.GetImportFileColumns(ImportFileId, false);
            foreach (DataImportUtility.ImportFileHeaderIdInstance Item in objImportFileColumns)
                m_objImportFileRecord.Columns.Add(Item.column_name);

            // process data table row values / records
            object[] ItemArray = null;
            m_objImportFileRowList = DataImportUtility.GetImportFileRows(ImportFileId);
            foreach (DataImportUtility.ImportFileRowInstance Item in m_objImportFileRowList)
            {
                ItemArray = null;
                //ItemArray = Regex.Split(Item.row_value, "*");
                ItemArray = Item.row_value.Split(new string[] {"[sep]"}, StringSplitOptions.None);
                m_objImportFileRecord.Rows.Add(ItemArray);
            }

            gcImportRecord.DataSource = m_objImportFileRecord;
            gvImportRecord.BestFitColumns();
        }

        /// <summary>
        /// Sets the import file's status (e.g. active, inactive)
        /// </summary>
        private void SetImportFileStatus(bool IsEnabled)
        {
            DataImportUtility.SetImportFileStatus(m_objImportList.id, IsEnabled);
            this.PopulateImportListView();            
        }

        /// <summary>
        /// Delete the import file (those inactive only)
        /// </summary>
        private void DeleteImportFile(int ImportFileId)
        {
            DataImportUtility.DeleteImportFile(ImportFileId);
            gcImportRecord.DataSource = null;
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetImportListViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miImportListPrintPreview = new MenuItem("Print Preview");
            miImportListPrintPreview.Click += new EventHandler(miImportListPrintPreview_Click);
            objClickMenu.MenuItems.Add(miImportListPrintPreview);
            gcImportFile.ContextMenu = objClickMenu;
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetImportRecordViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miImportRecordPrintPreview = new MenuItem("Print Preview");
            miImportRecordPrintPreview.Click += new EventHandler(miImportRecordPrintPreview_Click);
            objClickMenu.MenuItems.Add(miImportRecordPrintPreview);
            gcImportRecord.ContextMenu = objClickMenu;
        }
        #endregion

        #region Account Matching
        private int current_profile_list_id = 0;

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {

            #region This snippet is used if the implementation is when using Azure Message Queue or Azure Storage API
            //try {
            //    SetMessage("Loading SSIS account package...", true);
            //    var objArg = e.Argument as ObjectArguments;
            //    var packageMsg = new SSISPackageMessage();

            //    packageMsg.PackageID = Guid.NewGuid().ToString();
            //    packageMsg.PackageType = "Account";
            //    packageMsg.UserID = UserSession.CurrentUser.UserId;
            //    packageMsg.ImportFileID = objArg.import_file_id;
            //    packageMsg.Similarity = objArg.similarity;
            //    packageMsg.Confidence = objArg.confidence;
            //    packageMsg.SimilarityOperator = objArg.similarity_operator;
            //    packageMsg.ConfidenceOperator = objArg.confidence_operator;

            //    SetMessage("Enqueuing account package to message queue...",true);
            //    m_oPackageQueue.AddMessage(packageMsg);
            //    SetMessage("Account package enqueued successfully.",true);
            //    SetMessage("Please wait while the account package is being processed...", true);

            //    while (true) {
            //        try {
            //            System.Threading.Thread.Sleep(5000);
            //            //retrieve notifications
            //            UserTextNotification[] userToasts = m_oRepository.GetNotificationsForUser(UserSession.CurrentUser.UserId.ToString());
            //            if (userToasts != null && userToasts.Length > 0) {
            //                var data = (from UserTextNotification toast in userToasts
            //                            where toast.Title.ToLower() == "accountnotification"
            //                            orderby toast.Timestamp descending
            //                            select toast).ToArray();
            //                if (data != null) {
            //                    if (data[0].MessageText.ToLower() == "success") {
            //                        //delete notification after use
            //                        m_oRepository.DeleteNotification(data[0]);
            //                        SetMessage("Loading results to grids...", true);

            //                        m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            //                        gcMatchingCompany.DataSource = m_objBrightPlatformEntity.FIGetFuzzyConfidenceAccounts(UserSession.CurrentUser.UserId).ToList();
            //                        //gridControlSimilarity.DataSource = m_objBrightPlatformEntity.FIGetFuzzySimilarityAccounts(UserSession.CurrentUser.UserId).ToList();
                                    
            //                        System.Threading.Thread thread = new System.Threading.Thread(ShowAccountAlerter);
            //                        thread.Start();

            //                        //simpleButtonStartMatching.Invoke((MethodInvoker)delegate {
            //                        //        simpleButtonStartMatching.Enabled = true;
            //                        //    });
            //                        //simpleButtonSave.Invoke((MethodInvoker)delegate {
            //                        //    simpleButtonSave.Enabled = true;
            //                        //});
            //                        break;
            //                    }
            //                }
            //            }
            //        } catch {
            //        }
            //    }

            //} catch (Exception ex) {
            //    //simpleButtonStartMatching.Invoke((MethodInvoker)delegate {
            //    //    simpleButtonStartMatching.Enabled = true;
            //    //});
            //    //simpleButtonSave.Invoke((MethodInvoker)delegate {
            //    //    simpleButtonSave.Enabled = true;
            //    //});
            //}
           
            //SetMessage("Package executed successfully.", true);
            //SetMessage("All completed.", false);
            #endregion

            #region This snippet is used for local area network communication using .Net Remoting TCP
            /*
            SetMessage("Loading SSIS package...", true);
            System.Runtime.Remoting.Channels.IChannel[] chns = System.Runtime.Remoting.Channels.ChannelServices.RegisteredChannels;
            var clist = chns.ToList();
            if (clist.Count == 0 || (!clist.Exists(x => x.ChannelName == "BrightVision.SSIS.PackageExecution"))) {
                Dictionary<string, string> props = new Dictionary<string, string>() {
                    {"name","BrightVision.SSIS.PackageExecution"}
                };
                System.Runtime.Remoting.Channels.Tcp.TcpChannel channel
                    = new System.Runtime.Remoting.Channels.Tcp.TcpChannel(props, null, null);
                System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(channel, false);
            }
            try {
                SetMessage("Start executing package...", true);
                string TCPServerAddress = ConfigurationManager.AppSettings["TCPServerAddress"].ToString();
                BrightVision.RemotableObject.TaskCaller taskCaller =
                    (BrightVision.RemotableObject.TaskCaller)Activator.GetObject(
                        typeof(BrightVision.RemotableObject.TaskCaller), TCPServerAddress);
                ObjectArguments objArg = e.Argument as ObjectArguments;
                taskCaller.MakeCall(
                    objArg.import_file_id,
                    objArg.confidence,
                    objArg.confidence_operator,
                    objArg.similarity,
                    objArg.similarity_operator,
                    1);

            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }

            SetMessage("Package executed successfully.", true);
            System.Threading.Thread.Sleep(5000);
            SetMessage("Loading results to grids", true);
            m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            gridControlConfidence.Invoke((MethodInvoker)delegate {
                gridControlConfidence.DataSource = m_objBrightPlatformEntity.FIGetFuzzyConfidenceAccounts(UserSession.CurrentUser.UserId);
            });
            gridControlSimilarity.Invoke((MethodInvoker)delegate {
                gridControlSimilarity.DataSource = m_objBrightPlatformEntity.FIGetFuzzySimilarityAccounts(UserSession.CurrentUser.UserId);
            });
            SetMessage("All completed.", false);
            */
            #endregion

            #region This snippet is used for standalone client only
            /*
            //************************************************
            // This snippet is used for standalone client only
            //************************************************
            SetMessage("Loading SSIS package...",true);
            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            ObjectArguments objArg = e.Argument as ObjectArguments;
            MSDTS.Application app = new MSDTS.Application();
            //
            // Load package from file system
            //
            MSDTS.Package package = app.LoadPackage(Application.StartupPath + "\\SSIS Package\\ProfileMatchAccounts.dtsx", null);
            package.ImportConfigurationFile(Application.StartupPath + "\\SSIS Package\\dtProfileMatchAccounts.dtsConfig");
            var pkgVars = package.Variables;

            //Console.Write("Enter the value of firstname filter: ");
            SetMessage("Reading parameters...",true);

            package.VariableDispenser.LockOneForWrite("user_id", ref pkgVars);
            pkgVars["user_id"].Value = objArg.user_id;
            pkgVars.Unlock();

            package.VariableDispenser.LockOneForWrite("import_file_id", ref pkgVars);
            pkgVars["import_file_id"].Value = objArg.import_file_id;
            pkgVars.Unlock();

            //Console.Write("Enter the value of lastname filter: ");
            package.VariableDispenser.LockOneForWrite("confidence", ref pkgVars);
            pkgVars["confidence"].Value = objArg.confidence;
            pkgVars.Unlock();

            //Console.Write("Enter the value of location filter: ");
            package.VariableDispenser.LockOneForWrite("similarity", ref pkgVars);
            pkgVars["similarity"].Value = objArg.similarity;
            pkgVars.Unlock();
            
            //Console.Write("Enter the value of location filter: ");
            package.VariableDispenser.LockOneForWrite("similarity_operator", ref pkgVars);
            pkgVars["similarity_operator"].Value = objArg.similarity_operator;
            pkgVars.Unlock();

            //Console.Write("Enter the value of location filter: ");
            package.VariableDispenser.LockOneForWrite("confidence_operator", ref pkgVars);
            pkgVars["confidence_operator"].Value = objArg.confidence_operator;
            pkgVars.Unlock();

            //System.Diagnostics.Trace.WriteLine("Executing package... Please wait...");
            SetMessage("Start executing package...",true);
            try {
                MSDTS.DTSExecResult result = package.Execute();
                SetMessage("Package executed successfully.",true);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            package.Dispose();

            m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            SetMessage("Loading results to grids",true);
            gridControlConfidence.Invoke((MethodInvoker) delegate {                
                gridControlConfidence.DataSource = m_objBrightPlatformEntity.FIGetFuzzyConfidenceAccounts(UserSession.CurrentUser.UserId);    
            });            
            gridControlSimilarity.Invoke((MethodInvoker)delegate {
                gridControlSimilarity.DataSource = m_objBrightPlatformEntity.FIGetFuzzySimilarityAccounts(UserSession.CurrentUser.UserId);
            });
            SetMessage("All completed.",false);
            Cursor.Current = currentCursor;
            */
            #endregion

        }

        private void ShowContactAlerter() {
            if (InvokeRequired) {
                BeginInvoke(new MethodInvoker(ShowContactAlerter));
            } else { 
                alertControlSSISPackage.Show(this.ParentForm, "Profile Matching", "Contact package has been successfully processed.");
            }
        }

        private void ShowAccountAlerter()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(ShowAccountAlerter));
            }
            else
            {
                alertControlSSISPackage.Show(this.ParentForm, "Profile Matching", "Account package has been successfully processed.");
            }
        }

        private void SetMessage(string msg, bool newLine) {            
            //richTextBoxMessageLog.Invoke((MethodInvoker)delegate() {
            //    richTextBoxMessageLog.SelectionStart = richTextBoxMessageLog.TextLength;
            //    richTextBoxMessageLog.SelectionLength = 0;                
            //    richTextBoxMessageLog.AppendText(msg + (newLine ? Environment.NewLine: ""));
            //    richTextBoxMessageLog.SelectionStart = richTextBoxMessageLog.TextLength;
            //    richTextBoxMessageLog.ScrollToCaret();
            //});
        }

        private void trackBarControlConfidence_BeforeShowValueToolTip(object sender, TrackBarValueToolTipEventArgs e) {
            e.ShowArgs.ToolTip = string.Format("{0} Percent", trackBarControlConfidence.Value);
            simpleLabelItemConfidence.Text = string.Format("{0} %", trackBarControlConfidence.Value);
        }

        private void trackBarControlSimilarity_BeforeShowValueToolTip(object sender, TrackBarValueToolTipEventArgs e) {
            e.ShowArgs.ToolTip = string.Format("{0} Percent ", trackBarControlSimilarity.Value);
            simpleLabelItemSimilarity.Text = string.Format("{0} %", trackBarControlSimilarity.Value);
        }
                
        private void checkEditIncludeAll_CheckedChanged(object sender, EventArgs e) {
            //bool state = checkEditIncludeAll.Checked;
            //int count = gvMatchingCompany.RowCount;
            //GridColumn gridCol = gvMatchingCompany.Columns["include_insert"];
            //for(int x = 0; x < count; ++x) {
            //    gvMatchingCompany.SetRowCellValue(x, gridCol, state);
            //}
        }

        //private void checkEditIncludeAll2_CheckedChanged(object sender, EventArgs e) {
        //    bool state = checkEditIncludeAll2.Checked;
        //    int count = gridViewSimilarity.RowCount;
        //    GridColumn gridCol = gridViewSimilarity.Columns["include_insert"];
        //    for (int x = 0; x < count; ++x) {
        //        gridViewSimilarity.SetRowCellValue(x, gridCol, state);
        //    }
        //}

        //======================= TO LINK
        //private void simpleButtonSave_Click(object sender, EventArgs e) 
        //{
        //    if (m_objImportList == null)
        //        return;

        //    if (m_objImportList.id < 1)
        //        return;

        //    //simpleButtonStartMatching.Enabled = false;
        //    //simpleButtonSave.Enabled = false;

        //    Cursor curCursor = Cursor.Current;
        //    Cursor.Current = Cursors.WaitCursor;

        //    List<string> listIncludeInsert = new List<string>();
        //    List<string> ValidatedRecordIds = new List<string>();
        //    List<string> NonValidatedRecordIds = new List<string>();
        //    int count = gvMatchingCompany.RowCount;                        
        //    //CTFuzzyConfidenceAccounts fuzzConAcct = null;
        //    CTFuzzyAccount fuzzConAcct = null;
        //    for (int x = 0; x < count; ++x) {
        //        //fuzzConAcct = gridViewConfidence.GetRow(x) as CTFuzzyConfidenceAccounts;
        //        fuzzConAcct = gvMatchingCompany.GetRow(x) as CTFuzzyAccount;
        //        if (fuzzConAcct.include_insert != null && fuzzConAcct.include_insert == true) {
        //            listIncludeInsert.Add(fuzzConAcct.record_id.ToString());

        //            // get list of validated and non-validated included accounts
        //            if (Convert.ToBoolean(fuzzConAcct.validated))
        //                ValidatedRecordIds.Add(fuzzConAcct.record_id.ToString());
        //            else
        //                NonValidatedRecordIds.Add(fuzzConAcct.record_id.ToString());
        //        }
        //    }

        //    if (listIncludeInsert.Count <= 0) 
        //    {
        //        MessageBox.Show("Please check at least one include items first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        //simpleButtonStartMatching.Enabled = true;
        //        //simpleButtonSave.Enabled = true;
        //        return;
        //    }

        //    if (MessageBox.Show("Are you sure you want to save all checked 'include' rows to accounts master table?",
        //        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        //        != DialogResult.Yes)
        //    {
        //        //simpleButtonStartMatching.Enabled = true;
        //        //simpleButtonSave.Enabled = true;
        //        return;
        //    }

        //    string ids = string.Join(",", listIncludeInsert.ToArray());
        //    m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        //    //--> do for accounts confidence
        //    //get all "include_insert = true" rows and mark them in the reference table.
        //    /********************************************
        //    @match_type   : 0 - confidence
        //                    1 - similarity
        //    @table_match  : 0 - accounts
        //                    1 - contacts
        //    @state        : 0 - don't include during insert
        //                    1 - include during insert
        //    ********************************************/
        //    m_objBrightPlatformEntity.FIUpdateFuzzyLookupIncludeInsert(UserSession.CurrentUser.UserId, false, ids, true, m_objImportList.id);

        //    // update validated fields for selected matched accounts
        //    m_objBrightPlatformEntity.FIValidateFuzzyAccounts(string.Join(",", ValidatedRecordIds.ToArray()), string.Join(",", NonValidatedRecordIds.ToArray()), m_objImportList.id);

        //    //--> do for accounts similarity
        //    //listIncludeInsert = new List<string>();
        //    //CTFuzzySimilarityAccounts fuzzSimAcct = null;
        //    //count = gridViewSimilarity.RowCount;
        //    //for (int x = 0; x < count; ++x) {
        //    //    fuzzSimAcct = gridViewSimilarity.GetRow(x) as CTFuzzySimilarityAccounts;
        //    //    if (fuzzSimAcct.include_insert != null && fuzzSimAcct.include_insert == true) {
        //    //        listIncludeInsert.Add(fuzzSimAcct.record_id.ToString());
        //    //    }
        //    //}

        //    //ids = string.Join(",", listIncludeInsert.ToArray());
        //    //m_objBrightPlatformEntity.FIUpdateFuzzyLookupIncludeInsert(UserSession.CurrentUser.UserId, true, false, ids, true);            

        //    // save to master account table whose "include_insert" field has been marked as true.
        //    List<CTMatchedAccountId> items = m_objBrightPlatformEntity.FISaveFuzzyLookupAccountsMasterTable(m_objImportList.customer_id, m_objImportList.import_list_name, UserSession.CurrentUser.UserId, m_objImportList.id).ToList();
            
        //    // get matched account ids
        //    m_MatchedAccountIds = null;
        //    m_MatchedAccountIds = new List<string>();
        //    if (items.Count > 0)
        //        foreach (CTMatchedAccountId item in items)
        //            m_MatchedAccountIds.Add(item.account_id.ToString());

        //    // save profiled data contacts
        //    m_objBrightPlatformEntity.FISaveProfiledDataContacts(m_objImportList.id, string.Join(",", m_MatchedAccountIds.ToArray()));

        //    // save import list as matched
        //    DataImportUtility.MatchedByAccount(m_objImportList.id);
        //    this.PopulateImportListView();
        //    this.SetContactMatchingModule(true);

        //    //string Message = string.Empty;
        //    //if (txtListName.Text.Length > 0)
        //    //    Message = "Rows has been successfully saved." + Environment.NewLine + "A new imported list has been created for " + lblCustomer.ToString().ToUpper() + " -> " + txtListName.Text.ToUpper() + ".";
        //    //else
        //    //    Message = "Rows has been successfully saved.";

        //    string Message = "Rows has been successfully saved." + Environment.NewLine + "A new imported list has been created for " + m_objImportList.customer_name.ToUpper() + " -> " + m_objImportList.import_list_name.ToUpper() + ".";
        //    MessageBox.Show(Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    Cursor.Current = curCursor;
        //    //simpleButtonStartMatching.Enabled = true;
        //    //simpleButtonSave.Enabled = true;
        //}
        #endregion
        
        #region Object Argument Background Thread
        class ObjectArguments {
            public int user_id;
            public int import_file_id;
            public string country;
            public string fuzzy_ids;
            public int confidence;
            public int similarity;
            public string confidence_operator;
            public string similarity_operator;
            public byte validated;
        }
        #endregion     

        #region ===== Contact Matching Block =====
        #region Enumerations
        private enum eFuzzyLookupContactMatchType
        {
            FuzzyContactName,
            ExactContactName,
            ExactContactEmail
        }
        #endregion

        #region Private Members
        private List<CTFuzzyLookupContact> m_FuzzyLookupContacts = null;
        private List<string> m_FuzzyLookupContactIds = null;
        private int m_SelectedContacts = 0;
        private int m_ExistingContacts = 0;
        #endregion

        #region Object Events
        private void cmdCreateCallListContact_Click(object sender, EventArgs e)
        {
            if (gvMatchingContact.RowCount < 1)
                return;

            DialogResult objChoice = MessageBox.Show("Are you sure to generate call list for this selected import file?", "Import File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objChoice == DialogResult.No)
                return;

            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Creating call list...");
            DataImportUtility.CreateCallListAndContactMatchList(m_objImportList.id, m_objImportList.customer_id, m_objImportList.import_list_name, false);
            this.MatchImportListByContact();
            WaitDialog.Close();
            MessageBox.Show("Call list created.", "Import List", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cmdLoadImportFileContact_Click(object sender, EventArgs e)
        {
            this.SetFocusedViewInstance();
            if (m_objImportList == null)
                return;

            //if (!m_objImportList.matched_by_account)
            //{
            //    MessageBox.Show("Selected import file is not yet matched by companies.", "Contact Matching", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            
            WaitDialog.Show("Loading contacts ...");
            this.PopulateFuzzyLookupContactList();
            WaitDialog.Close();
        }

        private void cmdSelectAll_Click(object sender, EventArgs e)
        {
            GridColumn objGridCol = gvMatchingContact.Columns["selected"];
            for (int i = 0; i < gvMatchingContact.RowCount; i++)
                gvMatchingContact.SetRowCellValue(i, objGridCol, true);
        }

        private void cmdSelectNonMatched_Click(object sender, EventArgs e)
        {
            bool _IsMatched = false;
            GridColumn objGridCol = gvMatchingContact.Columns["selected"];
            for (int i = 0; i < gvMatchingContact.RowCount; i++)
            {
                gvMatchingContact.SetRowCellValue(i, objGridCol, false);
                _IsMatched = Convert.ToBoolean(gvMatchingContact.GetRowCellValue(i, gvMatchingContact.Columns["matched"]));
                //if (!Convert.ToBoolean(m_FuzzyLookupContacts[i].matched))
                if (!_IsMatched)
                    gvMatchingContact.SetRowCellValue(i, objGridCol, true);
            }
        }

        private void cmdClearSelection_Click(object sender, EventArgs e)
        {
            GridColumn objGridCol = gvMatchingContact.Columns["selected"];
            for (int i = 0; i < gvMatchingContact.RowCount; i++)
                gvMatchingContact.SetRowCellValue(i, objGridCol, false);
        }

        private void cmdExecuteContactMatch_Click(object sender, EventArgs e)
        {
            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            this.GetFuzzyLookupContactIds();
            if (m_FuzzyLookupContacts == null || m_FuzzyLookupContactIds == null)
                return;
            else if (m_FuzzyLookupContacts.Count < 1 || m_FuzzyLookupContactIds.Count < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Loading...");

            if (cboMatchCriteriaContact.Text.Equals("Contact Name") && cboMatchTypeContact.Text.Equals("Fuzzy"))
                this.ExecuteFuzzyLookupContactMatching(eFuzzyLookupContactMatchType.FuzzyContactName);
            else if (cboMatchCriteriaContact.Text.Equals("Contact Name") && cboMatchTypeContact.Text.Equals("Exact"))
                this.ExecuteFuzzyLookupContactMatching(eFuzzyLookupContactMatchType.ExactContactName);
            else if (cboMatchCriteriaContact.Text.Equals("Contact Email") && cboMatchTypeContact.Text.Equals("Exact"))
                this.ExecuteFuzzyLookupContactMatching(eFuzzyLookupContactMatchType.ExactContactEmail);

            this.MatchImportListByContact();
            WaitDialog.Close();
        }

        private void cmdRemoveSelectedMatch_Click(object sender, EventArgs e)
        {
            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            this.GetFuzzyLookupContactIds();

            if (m_FuzzyLookupContacts == null || m_FuzzyLookupContactIds == null)
                return;
            else if (m_FuzzyLookupContacts.Count < 1 || m_FuzzyLookupContactIds.Count < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Clearing...");
            DataImportUtility.ClearFuzzyLookupContactMatches(m_objImportList.id, m_FuzzyLookupContactIds);
            this.PopulateFuzzyLookupContactList();
            WaitDialog.Close();
        }

        private void cmdSaveToMasterData_Click(object sender, EventArgs e)
        {
            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            this.GetFuzzyLookupContactIds();
            if (m_FuzzyLookupContacts == null || m_FuzzyLookupContactIds == null)
                return;
            else if (m_FuzzyLookupContacts.Count < 1 || m_FuzzyLookupContactIds.Count < 1)
                return;

            string Message = string.Empty;
            if (m_ExistingContacts > 0)
                Message = "There are " + m_SelectedContacts.ToString() + " selected contacts." + Environment.NewLine + Environment.NewLine
                        + m_ExistingContacts.ToString() + " already have a match. Please revoke those first." + Environment.NewLine + Environment.NewLine
                        + "Be Understood that these contacts will be added to master data table as non-validated contacts for the respective companies." + Environment.NewLine + Environment.NewLine
                        + "You want to continue?";
            else
                Message = "There are " + m_SelectedContacts.ToString() + " selected contacts." + Environment.NewLine + Environment.NewLine
                        + "Be understood that these contacts will be added to master data table as non-validated contacts for the respective companies." + Environment.NewLine + Environment.NewLine
                        + "You want to continue?";

            DialogResult objDialog = MessageBox.Show(Message, "Add Import File Contacts", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objDialog == DialogResult.No)
                return;

            WaitDialog.Show(ParentForm, "Saving import file...");
            DataImportUtility.SaveFuzzyLookupContactToMasterTable(m_FuzzyLookupContactIds, BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Import_List);
            DataImportUtility.MatchedByContact(m_objImportList.id);
            gvImportFile.SetRowCellValue(m_SelectedImportFileRowHandle, "matched_by_contact", true);
            //this.MatchImportListByContact();
            this.PopulateFuzzyLookupContactList();
            WaitDialog.Close();
        }

        private void cbxShowOnlyValidatedContact_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterFuzzyLookupContactsView();
        }

        private void cbxShowOnlyNonMatchedContact_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterFuzzyLookupContactsView();
        }

        private void cboMatchCriteriaContact_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetContactMatchingRequiredControls();
        }

        private void cboMatchTypeContact_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetContactMatchingRequiredControls();
        }

        private void cmdSelectMatchedContact_Click(object sender, EventArgs e)
        {
            bool _IsMatched = false;
            GridColumn objGridCol = gvMatchingContact.Columns["selected"];
            for (int i = 0; i < gvMatchingContact.RowCount; i++)
            {
                gvMatchingContact.SetRowCellValue(i, objGridCol, false);
                _IsMatched = Convert.ToBoolean(gvMatchingContact.GetRowCellValue(i, gvMatchingContact.Columns["matched"]));
                //if (Convert.ToBoolean(m_FuzzyLookupContacts[i].matched))
                if (_IsMatched)
                    gvMatchingContact.SetRowCellValue(i, objGridCol, true);
            }
        }

        private void cmdMarkAsMatchedContact_Click(object sender, EventArgs e)
        {
            //this.PopulateImportListView();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update the import list view matched by contact check box item
        /// </summary>
        private void MatchImportListByContact()
        {
            GridColumn objGridCol = gvImportFile.Columns["matched_by_contact"];
            gvImportFile.SetRowCellValue(m_SelectedImportFileRowHandle, objGridCol, true);
        }

        /// <summary>
        /// Set the required controls for company matching
        /// </summary>
        private void SetContactMatchingRequiredControls()
        {
            if (cboMatchCriteriaContact.Text.Equals("Contact Name") && cboMatchTypeContact.Text.Equals("Fuzzy"))
            {
                cmdExecuteMatchContact.Enabled = false;
                lcgThresholdContact.Enabled = false;
            }
            else if (cboMatchCriteriaContact.Text.Equals("Contact Name") && cboMatchTypeContact.Text.Equals("Exact"))
            {
                cmdExecuteMatchContact.Enabled = true;
                lcgThresholdContact.Enabled = false;
            }
            else if (cboMatchCriteriaContact.Text.Equals("Contact Email") && cboMatchTypeContact.Text.Equals("Exact"))
            {
                cmdExecuteMatchContact.Enabled = true;
                lcgThresholdContact.Enabled = false;
            }
            else
            {
                cmdExecuteMatchContact.Enabled = false;
                lcgThresholdContact.Enabled = false;
            }
        }

        /// <summary>
        /// Apply filter
        /// </summary>
        private void FilterFuzzyLookupContactsView()
        {
            gvMatchingContact.ClearColumnsFilter();
            string FilterString = string.Empty;

            if (cbxShowOnlyNonMatchedContact.Checked)
                FilterString = "[matched] = 0";

            if (cbxShowOnlyValidatedContact.Checked)
                FilterString = FilterString.Length > 0 ? FilterString + " AND [validated] = 1" : "[validated] = 1";

            if (FilterString.Length > 0)
                gvMatchingContact.ActiveFilterString = FilterString;
        }

        /// <summary>
        /// Execute contact matching process
        /// </summary>
        /// <param name="FuzzyLookupContactMatchType"></param>
        private void ExecuteFuzzyLookupContactMatching(eFuzzyLookupContactMatchType FuzzyLookupContactMatchType)
        {
            switch (FuzzyLookupContactMatchType)
            {
                case eFuzzyLookupContactMatchType.FuzzyContactName:
                    // reserved for future implementation
                    break;

                case eFuzzyLookupContactMatchType.ExactContactName:
                    DataImportUtility.UpdateFuzzyLookupContactsByExactContactName(m_objImportList.id, m_FuzzyLookupContactIds);
                    this.PopulateFuzzyLookupContactList();
                    break;

                case eFuzzyLookupContactMatchType.ExactContactEmail:
                    DataImportUtility.UpdateFuzzyLookupContactsByExactContactEmail(m_objImportList.id, m_FuzzyLookupContactIds);
                    this.PopulateFuzzyLookupContactList();
                    break;
            }
        }

        /// <summary>
        /// Get the selected profiled data contact ids for matching
        /// </summary>
        private void GetFuzzyLookupContactIds()
        {
            m_FuzzyLookupContactIds = null;
            m_FuzzyLookupContactIds = new List<string>();
            m_SelectedContacts = 0;
            m_ExistingContacts = 0;
            bool _IsSelected = false;
            bool _IsMatched = false;
            string _FuzzyLookupContactId = "";

            //gvMatchingCompany.GetRowCellValue(i, gvMatchingCompany.Columns["matched"])

            //for (int i = 0; i < m_FuzzyLookupContacts.Count; i++)
            for (int i = 0; i < gvMatchingContact.RowCount; i++)
            {
                _IsSelected = Convert.ToBoolean(gvMatchingContact.GetRowCellValue(i, gvMatchingContact.Columns["selected"]));
                //if (Convert.ToBoolean(m_FuzzyLookupContacts[i].selected))
                if (_IsSelected)
                {
                    _FuzzyLookupContactId = gvMatchingContact.GetRowCellValue(i, gvMatchingContact.Columns["id"]).ToString();
                    m_FuzzyLookupContactIds.Add(_FuzzyLookupContactId);
                    //m_FuzzyLookupContactIds.Add(m_FuzzyLookupContacts[i].id.ToString());
                    m_SelectedContacts++;
                    _IsMatched = Convert.ToBoolean(gvMatchingContact.GetRowCellValue(i, gvMatchingContact.Columns["matched"]));
                    //if (m_FuzzyLookupContacts[i].matched)
                    if (_IsMatched)
                        m_ExistingContacts++;
                }
            }
        }

        /// <summary>
        /// Clear the statistics
        /// </summary>
        private void ClearFuzzyLookupContactStatistics()
        {
            lblNoOfContact.Text = "No. Of Contacts: 0";
            lblExactMatchContactName.Text = "Matched By Name: 0";
            lblExactMatchContactEmail.Text = "Matched By Email: 0";
            lblNonMatchedContact.Text = "Non Matched: 0";
        }

        /// <summary>
        /// Set statictics
        /// </summary>
        /// <param name="objStatistic"></param>
        private void SetFuzzyLookupContactMatchStatistics(CTFuzzyLookupContactStatistics objStatistic)
        {
            lblNoOfContact.Text = "No. Of Contacts: " + objStatistic.total_records.ToString();
            lblExactMatchContactName.Text = "Matched By Name: " + objStatistic.matched_by_name.ToString();
            lblExactMatchContactEmail.Text = "Matched By Email: " + objStatistic.matched_by_email.ToString();
            lblNonMatchedContact.Text = "Non Matched: " + objStatistic.non_matched.ToString();
            lblNewlyAddedContact.Text = "Newly Added: " + objStatistic.newly_added.ToString();
        }

        /// <summary>
        /// Populate the matched contacts view
        /// </summary>
        private void PopulateFuzzyLookupContactList()
        {
            this.ClearFuzzyLookupContactStatistics();
            gcMatchingContact.DataSource = null;
            m_FuzzyLookupContacts = null;
            if (m_objImportList == null)
                return;

            if (m_FuzzyLookupContacts == null)
                m_FuzzyLookupContacts = new List<CTFuzzyLookupContact>();

            m_FuzzyLookupContacts = DataImportUtility.GetFuzzyLookupContacts(m_objImportList.id);
            if (m_FuzzyLookupContacts.Count > 0) {
                gcMatchingContact.DataSource = m_FuzzyLookupContacts;
                CTFuzzyLookupContactStatistics objStatistics = DataImportUtility.GetFuzzyLookupContactStatistics(m_objImportList.id);
                this.SetFuzzyLookupContactMatchStatistics(objStatistics);
                this.SetFuzzyLookupContactMatchingViewControls(true);
            }
            else
                this.SetFuzzyLookupContactMatchingViewControls(false);
        }

        /// <summary>
        /// Enable / disable controls
        /// </summary>
        private void SetFuzzyLookupContactMatchingViewControls(bool ControlStatus)
        {
            cboMatchCriteriaContact.Enabled = ControlStatus;
            cboMatchTypeContact.Enabled = ControlStatus;
            cmdExecuteMatchContact.Enabled = ControlStatus;
            cmdManualMatch.Enabled = false; // ControlStatus;
            lcgThresholdContact.Enabled = false; //ControlStatus;
            cbxShowOnlyValidatedContact.Enabled = ControlStatus;
            cbxShowOnlyNonMatchedContact.Enabled = ControlStatus;
            cmdSelectAllContact.Enabled = ControlStatus;
            cmdSelectNonMatchedContact.Enabled = ControlStatus;
            cmdClearSelectionContact.Enabled = ControlStatus;
            cmdRemoveSelectedMatchContact.Enabled = ControlStatus;
            cmdMarkAsMatchedContact.Enabled = false; // ControlStatus;
            cmdSaveToMasterDataContact.Enabled = ControlStatus;
            gcMatchingContact.Enabled = ControlStatus;
            cmdSelectMatchedContact.Enabled = ControlStatus;
            cmdCreateCallListContact.Enabled = ControlStatus;
            cmdCreateCollectedData.Enabled = ControlStatus;
        }

        /// <summary>
        /// Initialize the module
        /// </summary>
        private void InitFuzzyLookupContactMatchingView()
        {
            gcMatchingContact.DataSource = null;
            lblNoOfContact.Text = "No. Of Contacts: 0";
            //lblFuzzyMatchCompanyName.Text = "Fuzzy Company Name: 0";
            lblExactMatchContactName.Text = "Exact Contact Name: 0";
            lblExactMatchContactEmail.Text = "Exact Contact Email: 0";
            lblNonMatchedContact.Text = "Non-Matched: 0";
            lblNewlyAddedContact.Text = "Newly Added: 0";
            m_FuzzyLookupContacts = null;
            this.SetFuzzyLookupContactMatchingViewControls(false);
        }
        #endregion

        #region Commented Codes
        //private void simpleButtonExecuteMatchContacts_Click(object sender, EventArgs e) {
        //    gridControlConfidenceContacts.DataSource = null;
        //    gridControlSimilarityContacts.DataSource = null;
        //    richTextBoxOutputMessageContacts.Text = "";
        //    this.SetFocusedViewInstance();
        //    //m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcImportFile.FocusedView;
        //    //m_objImportProfileList = (ObjectImport.ImportListInstance)m_objGridView.GetFocusedRow();
        //    if (m_objImportList == null) {
        //        //tcManageImport.SelectedTabPage = tpProfiling;
        //        return;
        //    }

        //    int _confidence = 0, _similarity = 0;
        //    int.TryParse(trackBarControlConfidenceContacts.EditValue.ToString(), out _confidence);
        //    int.TryParse(trackBarControlSimilarityContacts.EditValue.ToString(), out _similarity);

        //    string _confidence_operator = comboBoxEditConfidenceContacts.EditValue.ToString();
        //    string _similarity_operator = comboBoxEditSimilarityContacts.EditValue.ToString();

        //    ObjectArguments objArg = new ObjectArguments() {
        //        user_id = UserSession.CurrentUser.UserId,
        //        import_file_id = m_objImportList.id,
        //        confidence = _confidence,
        //        similarity = _similarity,
        //        confidence_operator = _confidence_operator,
        //        similarity_operator = _similarity_operator
        //    };
        //    simpleButtonExecuteMatchContacts.Enabled = false;
        //    simpleButtonSaveAllContacts.Enabled = false;
        //    //backgroundWorker2.RunWorkerAsync(objArg);

        //    #region This snippet is used if the implementation is when using Azure Message Queue or Azure Storage API
        //    try {
        //        SetMessageContacts("Loading SSIS contact package...", true);              
        //        var packageMsg = new SSISPackageMessage();

        //        packageMsg.PackageID = Guid.NewGuid().ToString();
        //        packageMsg.PackageType = "contact";
        //        packageMsg.UserID = UserSession.CurrentUser.UserId;
        //        packageMsg.ImportFileID = objArg.import_file_id;
        //        packageMsg.Similarity = objArg.similarity;
        //        packageMsg.Confidence = objArg.confidence;
        //        packageMsg.SimilarityOperator = objArg.similarity_operator;
        //        packageMsg.ConfidenceOperator = objArg.confidence_operator;

        //        SetMessageContacts("Enqueuing contact package to message queue...", true);
        //        m_oPackageQueue.AddMessage(packageMsg);
        //        SetMessageContacts("Contact package enqueued successfully.", true);
        //        SetMessageContacts("Please wait while the contact package is being processed...", true);

        //        while (true) {
        //            try {
        //                System.Threading.Thread.Sleep(5000);
        //                //retrieve notifications
        //                UserTextNotification[] userToasts = m_oRepository.GetNotificationsForUser(UserSession.CurrentUser.UserId.ToString());
        //                if (userToasts != null && userToasts.Length > 0) {
        //                    var data = (from UserTextNotification toast in userToasts
        //                                where toast.Title.ToLower() == "contactnotification"
        //                                orderby toast.Timestamp descending
        //                                select toast).ToArray();
        //                    if (data != null) {
        //                        if (data[0].MessageText.ToLower() == "success") {
        //                            //delete notification after use
        //                            m_oRepository.DeleteNotification(data[0]);

        //                            SetMessageContacts("Loading results to grids...", true);
        //                            m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
                                        
        //                            gridControlConfidenceContacts.DataSource = m_objBrightPlatformEntity.FIGetFuzzyConfidenceContacts(UserSession.CurrentUser.UserId);                                 
        //                            gridControlSimilarityContacts.DataSource = m_objBrightPlatformEntity.FIGetFuzzySimilarityContacts(UserSession.CurrentUser.UserId);                                  

        //                            System.Threading.Thread thread = new System.Threading.Thread(ShowContactAlerter);
        //                            thread.Start();

        //                            simpleButtonExecuteMatchContacts.Invoke((MethodInvoker)delegate {
        //                                simpleButtonExecuteMatchContacts.Enabled = true;
        //                            });
        //                            simpleButtonSaveAllContacts.Invoke((MethodInvoker)delegate {
        //                                simpleButtonSaveAllContacts.Enabled = true;
        //                            });
        //                            break;
        //                        }
        //                    }
        //                }
        //            } catch {
        //            }
        //        }
        //    } catch (Exception ex) {
        //        MessageBox.Show(ex.Message);
        //        simpleButtonStartMatching.Invoke((MethodInvoker)delegate {
        //            simpleButtonStartMatching.Enabled = true;
        //        });
        //        simpleButtonSave.Invoke((MethodInvoker)delegate {
        //            simpleButtonSave.Enabled = true;
        //        });
        //    }
            
        //    SetMessageContacts("Package executed successfully.", true);
        //    SetMessageContacts("All completed.", false);
        //    #endregion
        //}

        //private void simpleButtonSaveAllContacts_Click(object sender, EventArgs e) {
            
        //    simpleButtonExecuteMatchContacts.Enabled = false;
        //    simpleButtonSaveAllContacts.Enabled = false;

        //    Cursor curCursor = Cursor.Current;
        //    Cursor.Current = Cursors.WaitCursor;

        //    List<string> listIncludeInsert = new List<string>();
        //    int count = gridViewConfidenceContacts.RowCount;
        //    CTFuzzyConfidenceContacts fuzzConfiCont = null;
        //    for (int x = 0; x < count; ++x) {
        //        fuzzConfiCont = gridViewConfidenceContacts.GetRow(x) as CTFuzzyConfidenceContacts;
        //        if (fuzzConfiCont.include_insert != null && fuzzConfiCont.include_insert == true) {
        //            listIncludeInsert.Add(fuzzConfiCont.record_id.ToString());
        //        }
        //    }
            
        //    if (listIncludeInsert.Count <= 0) {
        //        MessageBox.Show("Please check at least one include items first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        simpleButtonExecuteMatchContacts.Enabled = true;
        //        simpleButtonSaveAllContacts.Enabled = true;
        //        return;
        //    }

        //    if (MessageBox.Show("Are you sure you want to save all checked 'include' rows to contacts master table?",
        //        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        //        != DialogResult.Yes) return;

        //    string ids = string.Join(",", listIncludeInsert.ToArray());
        //    m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        //    //--> do for accounts confidence
        //    //get all "include_insert = true" rows and mark them in the reference table.
        //    /********************************************
        //    @match_type   : 0 - confidence
        //                    1 - similarity
        //    @table_match  : 0 - accounts
        //                    1 - contacts
        //    @state        : 0 - don't include during insert
        //                    1 - include during insert
        //    ********************************************/
        //    m_objBrightPlatformEntity.FIUpdateFuzzyLookupIncludeInsert(UserSession.CurrentUser.UserId, true, ids, true, m_objImportList.id);

        //    //--> do for accounts similarity
        //    //listIncludeInsert = new List<string>();
        //    //CTFuzzySimilarityAccounts fuzzSimAcct = null;
        //    //count = gridViewSimilarity.RowCount;
        //    //for (int x = 0; x < count; ++x) {
        //    //    fuzzSimAcct = gridViewSimilarity.GetRow(x) as CTFuzzySimilarityAccounts;
        //    //    if (fuzzSimAcct.include_insert != null && fuzzSimAcct.include_insert == true) {
        //    //        listIncludeInsert.Add(fuzzSimAcct.record_id.ToString());
        //    //    }
        //    //}
        //    //ids = string.Join(",", listIncludeInsert.ToArray());
        //    //m_objBrightPlatformEntity.FIUpdateFuzzyLookupIncludeInsert(UserSession.CurrentUser.UserId, true, true, ids, true);

        //    //save to master account table whose "include_insert" field has been marked as true.
        //    m_objBrightPlatformEntity.FISaveFuzzyLookupContactsMasterTable(UserSession.CurrentUser.UserId);
        //    MessageBox.Show("Rows has been saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    Cursor.Current = curCursor;
        //    simpleButtonExecuteMatchContacts.Enabled = true;
        //    simpleButtonSaveAllContacts.Enabled = true;
        //}

        //private void checkEditConfidenceContacts_CheckedChanged(object sender, EventArgs e) {
        //    bool state = checkEditConfidenceContacts.Checked;
        //    int count = gridViewConfidenceContacts.RowCount;
        //    GridColumn gridCol = gridViewConfidenceContacts.Columns["include_insert"];
        //    for (int x = 0; x < count; ++x) {
        //        gridViewConfidenceContacts.SetRowCellValue(x, gridCol, state);
        //    }
        //}

        //private void checkEditSimilarityContacts_CheckedChanged(object sender, EventArgs e) {
        //    bool state = checkEditSimilarityContacts.Checked;
        //    int count = gridViewSimilarityContacts.RowCount;
        //    GridColumn gridCol = gridViewSimilarityContacts.Columns["include_insert"];
        //    for (int x = 0; x < count; ++x) {
        //        gridViewSimilarityContacts.SetRowCellValue(x, gridCol, state);
        //    }
        //}

        //private void trackBarControlConfidenceContacts_BeforeShowValueToolTip(object sender, TrackBarValueToolTipEventArgs e) {
        //    e.ShowArgs.ToolTip = string.Format("{0} Percent", trackBarControlConfidenceContacts.Value);
        //    simpleLabelItemConfidenceContacts.Text = string.Format("{0} %", trackBarControlConfidenceContacts.Value);
        //}

        //private void trackBarControlSimilarityContacts_BeforeShowValueToolTip(object sender, TrackBarValueToolTipEventArgs e) {
        //    e.ShowArgs.ToolTip = string.Format("{0} Percent ", trackBarControlSimilarityContacts.Value);
        //    simpleLabelItemSimilarityContacts.Text = string.Format("{0} %", trackBarControlSimilarityContacts.Value);
        //}

        //private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e) {

        //    #region This snippet is used if the implementation is when using Azure Message Queue or Azure Storage API
        //    try {
        //        SetMessageContacts("Loading SSIS contact package...", true);
        //        var objArg = e.Argument as ObjectArguments;
        //        var packageMsg = new SSISPackageMessage();

        //        packageMsg.PackageID = Guid.NewGuid().ToString();
        //        packageMsg.PackageType = "contact";
        //        packageMsg.UserID = UserSession.CurrentUser.UserId;
        //        packageMsg.ImportFileID = objArg.import_file_id;
        //        packageMsg.Similarity = objArg.similarity;
        //        packageMsg.Confidence = objArg.confidence;
        //        packageMsg.SimilarityOperator = objArg.similarity_operator;
        //        packageMsg.ConfidenceOperator = objArg.confidence_operator;

        //        SetMessageContacts("Enqueuing contact package to message queue...", true);
        //        m_oPackageQueue.AddMessage(packageMsg);
        //        SetMessageContacts("Contact package enqueued successfully.", true);
        //        SetMessageContacts("Please wait while the contact package is being processed...", true);
                
        //        while (true) {
        //            try {
        //                System.Threading.Thread.Sleep(5000);
        //                //retrieve notifications
        //                UserTextNotification[] userToasts = m_oRepository.GetNotificationsForUser(UserSession.CurrentUser.UserId.ToString());
        //                if (userToasts != null && userToasts.Length > 0) {
        //                    var data = (from UserTextNotification toast in userToasts
        //                                where toast.Title.ToLower() == "contactnotification"
        //                                orderby toast.Timestamp descending
        //                                select toast).ToArray();
        //                    if (data != null) {
        //                        if (data[0].MessageText.ToLower() == "success") {
        //                            //delete notification after use
        //                            m_oRepository.DeleteNotification(data[0]);

        //                            System.Threading.Thread thread = new System.Threading.Thread(ShowAccountAlerter);
        //                            thread.Start();

        //                            //simpleButtonExecuteMatchContacts.Invoke((MethodInvoker)delegate {
        //                            //    simpleButtonExecuteMatchContacts.Enabled = true;
        //                            //});
        //                            //simpleButtonSaveAllContacts.Invoke((MethodInvoker)delegate {
        //                            //    simpleButtonSaveAllContacts.Enabled = true;
        //                            //});
        //                            break;
        //                        }
        //                    }
        //                }
        //            } catch {
        //            }
        //        }
        //    } catch (Exception ex){
        //        MessageBox.Show(ex.Message);
        //        simpleButtonStartMatching.Invoke((MethodInvoker)delegate {
        //            simpleButtonStartMatching.Enabled = true;
        //        });
        //        simpleButtonSave.Invoke((MethodInvoker)delegate {
        //            simpleButtonSave.Enabled = true;
        //        });
        //    }

            //SetMessageContacts("Loading results to grids...", true);
            //m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            //gridControlConfidenceContacts.Invoke((MethodInvoker)delegate {
            //    gridControlConfidenceContacts.DataSource = m_objBrightPlatformEntity.FIGetFuzzyConfidenceContacts(UserSession.CurrentUser.UserId);
            //});
            //gridControlSimilarityContacts.Invoke((MethodInvoker)delegate {
            //    gridControlSimilarityContacts.DataSource = m_objBrightPlatformEntity.FIGetFuzzySimilarityContacts(UserSession.CurrentUser.UserId);
            //});
            //SetMessageContacts("Package executed successfully.", true);
            //SetMessageContacts("All completed.", false);
        //    #endregion

        //    #region This snippet is used for local area network communication using .Net Remoting TCP
        //    /*
        //    SetMessageContacts("Loading SSIS package...", true);
        //    System.Runtime.Remoting.Channels.IChannel[] chns = System.Runtime.Remoting.Channels.ChannelServices.RegisteredChannels;
        //    var clist = chns.ToList();
        //    if (clist.Count == 0 || (!clist.Exists(x => x.ChannelName == "BrightVision.SSIS.PackageExecution"))) {
        //        Dictionary<string, string> props = new Dictionary<string, string>() {
        //            {"name","BrightVision.SSIS.PackageExecution"}
        //        };               
        //        System.Runtime.Remoting.Channels.Tcp.TcpChannel channel
        //            = new System.Runtime.Remoting.Channels.Tcp.TcpChannel(props, null, null);
        //        System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(channel, false);
        //    }
        //    try {
        //        SetMessageContacts("Start executing package...", true);
        //        string TCPServerAddress = ConfigurationManager.AppSettings["TCPServerAddress"].ToString();
        //        BrightVision.RemotableObject.TaskCaller taskCaller = 
        //            (BrightVision.RemotableObject.TaskCaller) Activator.GetObject(
        //                typeof(BrightVision.RemotableObject.TaskCaller), TCPServerAddress);
        //        ObjectArguments objArg = e.Argument as ObjectArguments;
        //        taskCaller.MakeCall(
        //            objArg.import_file_id, 
        //            objArg.confidence, 
        //            objArg.confidence_operator, 
        //            objArg.similarity, 
        //            objArg.similarity_operator,
        //            2);

        //    } catch (Exception ex) {
        //        MessageBox.Show(ex.Message);
        //    }
        //    SetMessageContacts("Package executed successfully.", true);            
        //    System.Threading.Thread.Sleep(5000);
        //    SetMessageContacts("Loading results to grids", true);
        //    m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        //    gridControlConfidenceContacts.Invoke((MethodInvoker)delegate {
        //        gridControlConfidenceContacts.DataSource = m_objBrightPlatformEntity.FIGetFuzzyConfidenceContacts();
        //    });
        //    gridControlSimilarityContacts.Invoke((MethodInvoker)delegate {
        //        gridControlSimilarityContacts.DataSource = m_objBrightPlatformEntity.FIGetFuzzySimilarityContacts();
        //    });
        //    SetMessageContacts("All completed.", false);
        //    */
        //    #endregion

        //    #region  This snippet is used for standalone client only
        //    /*
        //    SetMessageContacts("Loading SSIS package...", true);
        //    Cursor currentCursor = Cursor.Current;
        //    Cursor.Current = Cursors.WaitCursor;
        //    ObjectArguments objArg = e.Argument as ObjectArguments;
        //    MSDTS.Application app = new MSDTS.Application();
        //    //
        //    // Load package from file system
        //    //
        //    MSDTS.Package package = app.LoadPackage(Application.StartupPath + "\\SSIS Package\\ProfileMatchContacts.dtsx", null);
        //    package.ImportConfigurationFile(Application.StartupPath + "\\SSIS Package\\dtProfileMatchContacts.dtsConfig");
        //    var pkgVars = package.Variables;

        //    //Console.Write("Enter the value of firstname filter: ");
        //    SetMessageContacts("Reading parameters...", true);
        //    package.VariableDispenser.LockOneForWrite("user_id", ref pkgVars);
        //    pkgVars["user_id"].Value = objArg.user_id;
        //    pkgVars.Unlock();

        //    package.VariableDispenser.LockOneForWrite("import_file_id", ref pkgVars);
        //    pkgVars["import_file_id"].Value = objArg.import_file_id;
        //    pkgVars.Unlock();

        //    //Console.Write("Enter the value of lastname filter: ");
        //    package.VariableDispenser.LockOneForWrite("confidence", ref pkgVars);
        //    pkgVars["confidence"].Value = objArg.confidence;
        //    pkgVars.Unlock();

        //    //Console.Write("Enter the value of location filter: ");
        //    package.VariableDispenser.LockOneForWrite("similarity", ref pkgVars);
        //    pkgVars["similarity"].Value = objArg.similarity;
        //    pkgVars.Unlock();

        //    //Console.Write("Enter the value of location filter: ");
        //    package.VariableDispenser.LockOneForWrite("similarity_operator", ref pkgVars);
        //    pkgVars["similarity_operator"].Value = objArg.similarity_operator;
        //    pkgVars.Unlock();

        //    //Console.Write("Enter the value of location filter: ");
        //    package.VariableDispenser.LockOneForWrite("confidence_operator", ref pkgVars);
        //    pkgVars["confidence_operator"].Value = objArg.confidence_operator;
        //    pkgVars.Unlock();

        //    //System.Diagnostics.Trace.WriteLine("Executing package... Please wait...");
        //    SetMessageContacts("Start executing package...", true);
        //    try {
        //        MSDTS.DTSExecResult result = package.Execute();
        //        SetMessageContacts("Package executed successfully.", true);
        //    } catch (Exception ex) {
        //        MessageBox.Show(ex.Message);
        //    }
        //    package.Dispose();

        //    m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        //    SetMessageContacts("Loading results to grids", true);
        //    gridControlConfidenceContacts.Invoke((MethodInvoker)delegate {
        //        gridControlConfidenceContacts.DataSource = m_objBrightPlatformEntity.FIGetFuzzyConfidenceContacts(UserSession.CurrentUser.UserId);
        //    });
        //    gridControlSimilarityContacts.Invoke((MethodInvoker)delegate {
        //        gridControlSimilarityContacts.DataSource = m_objBrightPlatformEntity.FIGetFuzzySimilarityContacts(UserSession.CurrentUser.UserId);
        //    });
        //    SetMessageContacts("All completed.", false);
        //    Cursor.Current = currentCursor;
        //    */
        //    #endregion
        //}

        //private void SetMessageContacts(string msg, bool newLine) {
        //    richTextBoxOutputMessageContacts.Invoke((MethodInvoker)delegate() {
        //        richTextBoxOutputMessageContacts.SelectionStart = richTextBoxOutputMessageContacts.TextLength;
        //        richTextBoxOutputMessageContacts.SelectionLength = 0;
        //        richTextBoxOutputMessageContacts.AppendText(msg + (newLine ? Environment.NewLine : ""));
        //        richTextBoxOutputMessageContacts.SelectionStart = richTextBoxOutputMessageContacts.TextLength;
        //        richTextBoxOutputMessageContacts.ScrollToCaret();
        //    });
        //}
        #endregion
        #endregion

        #region ===== Company Matching Block =====
        #region Enumerations
        private enum eFuzzyLookupAccountMatchType
        {
            FuzzyCompanyName,
            ExactCompanyName,
            ExactOrganizationNo,
            ExactCompanyNameWithCity
        }
        #endregion

        #region Private Members
        private List<CTFuzzyLookupAccount> m_FuzzyLookupAccountList = null;
        private List<string> m_FuzzyLookupAccountIds = null;
        //private List<string> m_ProcessedAccountIds = null;
        private int m_SelectedCompanies = 0;
        private int m_ExistingCompanies = 0;
        #endregion

        #region Object Events
        private void cmdLoadImportFileCompany_Click(object sender, EventArgs e)
        {
            this.PopulateFuzzyLookupAccountList();
        }
        private void cmdSelectAllCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            GridColumn objGridCol = gvMatchingCompany.Columns["selected"];
            for (int i = 0; i < gvMatchingCompany.RowCount; i++)
                gvMatchingCompany.SetRowCellValue(i, objGridCol, true);
        }
        private void cmdSelectNonMatchedCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            bool HasMatch = false;
            GridColumn objGridCol = gvMatchingCompany.Columns["selected"];
            for (int i = 0; i < gvMatchingCompany.RowCount; i++)
            {
                gvMatchingCompany.SetRowCellValue(i, objGridCol, false);
                HasMatch = Convert.ToBoolean(gvMatchingCompany.GetRowCellValue(i, gvMatchingCompany.Columns["matched"]));
                //if (!Convert.ToBoolean(m_FuzzyLookupAccountList[i].matched))
                if (!HasMatch)
                    gvMatchingCompany.SetRowCellValue(i, objGridCol, true);
            }
        }
        private void cmdClearSelectionCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            GridColumn objGridCol = gvMatchingCompany.Columns["selected"];
            for (int i = 0; i < gvMatchingCompany.RowCount; i++)
                gvMatchingCompany.SetRowCellValue(i, objGridCol, false);
        }
        private void cmdRemoveMatchCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            this.GetFuzzyLookupAccountIds();
            if (m_FuzzyLookupAccountList == null || m_FuzzyLookupAccountIds == null)
                return;
            else if (m_FuzzyLookupAccountList.Count < 1 || m_FuzzyLookupAccountIds.Count < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Clearing...");
            DataImportUtility.ClearFuzzyLookupAccountMatches(m_objImportList.id, m_FuzzyLookupAccountIds, false);
            WaitDialog.Close();
            this.PopulateFuzzyLookupAccountList();
        }
        private void cmdExecuteMatchCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            this.GetFuzzyLookupAccountIds();
            if (m_FuzzyLookupAccountList == null || m_FuzzyLookupAccountIds == null)
                return;
            else if (m_FuzzyLookupAccountList.Count < 1 || m_FuzzyLookupAccountIds.Count < 1)
                return;

            if (cbxValidatedCompany.CheckState == CheckState.Indeterminate)
                m_MatchByAccountType = 2;
            else if (cbxValidatedCompany.CheckState == CheckState.Checked)
                m_MatchByAccountType = 1;
            else if (cbxValidatedCompany.CheckState == CheckState.Unchecked)
                m_MatchByAccountType = 0;

            if (cboMatchCriteriaCompany.Text.Equals("Company Name") && cboMatchTypeCompany.Text.Equals("Fuzzy")) {
                this.ExecuteFuzzyLookupAccountMatching(eFuzzyLookupAccountMatchType.FuzzyCompanyName);
                this.MatchImportListByAccount();
            }
            else if (cboMatchCriteriaCompany.Text.Equals("Company Name") && cboMatchTypeCompany.Text.Equals("Exact")) {
                WaitDialog.Show(ParentForm, "Loading...");
                this.ExecuteFuzzyLookupAccountMatching(eFuzzyLookupAccountMatchType.ExactCompanyName);
                this.MatchImportListByAccount();
                WaitDialog.Close();
            }
            else if (cboMatchCriteriaCompany.Text.Equals("Organization No") && cboMatchTypeCompany.Text.Equals("Exact")) {
                WaitDialog.Show(ParentForm, "Loading...");
                this.ExecuteFuzzyLookupAccountMatching(eFuzzyLookupAccountMatchType.ExactOrganizationNo);
                this.MatchImportListByAccount();
                WaitDialog.Close();
            }
            else if (cboMatchCriteriaCompany.Text.Equals("Company Name + City") && cboMatchTypeCompany.Text.Equals("Exact")) {
                WaitDialog.Show(ParentForm, "Loading...");
                this.ExecuteFuzzyLookupAccountMatching(eFuzzyLookupAccountMatchType.ExactCompanyNameWithCity);
                this.MatchImportListByAccount();
                WaitDialog.Close();
            }
        }
        private void cboMatchTypeCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetAccountMatchingRequiredControls();
        }
        private void cboMatchCriteriaCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetAccountMatchingRequiredControls();
        }
        private void cmdMarkAsMatchedCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            this.GetFuzzyLookupAccountIds();
            if (m_FuzzyLookupAccountList == null || m_FuzzyLookupAccountIds == null)
                return;
            else if (m_FuzzyLookupAccountList.Count < 1 || m_FuzzyLookupAccountIds.Count < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Updating...");
            DataImportUtility.UpdateFuzzyLookupAccountsByFuzzyCompanyMatches(m_objImportList.id, m_FuzzyLookupAccountIds);
            WaitDialog.Close();
            this.MatchImportListByAccount();
            this.PopulateFuzzyLookupAccountList();
        }
        private void cmdSaveMatchCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            this.GetFuzzyLookupAccountIds();
            if (m_FuzzyLookupAccountList == null || m_FuzzyLookupAccountIds == null)
                return;
            else if (m_FuzzyLookupAccountList.Count < 1 || m_FuzzyLookupAccountIds.Count < 1)
                return;
            
            this.SaveFuzzyLookupAccountsToMasterTable();
        }
        private void cbxShowOnlyNonMatchedCompany_CheckedChanged(object sender, EventArgs e)
        {
                this.FilterFuzzyLookupAccountsView();
        }
        private void cbxShowOnlyValidatedCompany_CheckedChanged(object sender, EventArgs e)
        {
                this.FilterFuzzyLookupAccountsView();
        }
        private void cmdCreateCallListCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            DialogResult objChoice = MessageBox.Show("Are you sure to generate call list and contact matching list for this selected import file?", "Import File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objChoice == DialogResult.No)
                return;

            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            
            WaitDialog.Show(ParentForm, "Creating call list...");
            DataImportUtility.CreateCallListAndContactMatchList(m_objImportList.id, m_objImportList.customer_id, m_objImportList.import_list_name, true);
            this.MatchImportListByAccount();
            WaitDialog.Close();
            MessageBox.Show("Call list created.", "Import List", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void cmdSelectMatchedCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            bool HasMatch = false;
            GridColumn objGridCol = gvMatchingCompany.Columns["selected"];
            for (int i = 0; i < gvMatchingCompany.RowCount; i++)
            {
                gvMatchingCompany.SetRowCellValue(i, objGridCol, false);
                HasMatch = Convert.ToBoolean(gvMatchingCompany.GetRowCellValue(i, gvMatchingCompany.Columns["matched"]));
                //if (Convert.ToBoolean(m_FuzzyLookupAccountList[i].matched))
                if (HasMatch)
                    gvMatchingCompany.SetRowCellValue(i, objGridCol, true);
            }
        }
        private void cmdCreateCollectedData_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            DialogResult objChoice = MessageBox.Show("Are you sure to generate collected data records for this selected import file?", "Import File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objChoice == DialogResult.No)
                return;

            if (m_objImportList == null)
                return;
            else if (m_objImportList.id < 1)
                return;

            //
            //WaitDialog.Show(ParentForm, "Creating collected data.");
            //DataImportUtility.CreateImportListCollectedData(m_objImportList.id);
            ImportCollectedData objImportCollectedData = new ImportCollectedData();
            objImportCollectedData.ImportFileId = m_objImportList.id;
            objImportCollectedData.CustomerId = m_objImportList.customer_id == null ? 0 : m_objImportList.customer_id;
            m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Import File Collected Data";
            m_objPopupDialog.Controls.Add(objImportCollectedData);
            m_objPopupDialog.ClientSize = new Size(objImportCollectedData.Width + 2, objImportCollectedData.Height + 2);
            m_objPopupDialog.ShowDialog(this.ParentForm);
            //WaitDialog.Close();
        }
        private void cmdManualMatchCompany_Click(object sender, EventArgs e)
        {
            if (gvMatchingCompany.RowCount < 1)
                return;

            WaitDialog.Show("Loading manual matching ...");
            List<ClassesProperty.ManualMatchAccount> _lstMatchAccounts = new List<ClassesProperty.ManualMatchAccount>();
            List<CTFuzzyLookupAccount> _lstAccounts = m_FuzzyLookupAccountList.Where(i => i.selected == true).ToList();
            if (_lstAccounts.Count < 1) {
                NotificationDialog.Information("Bright Sales", "No items selected.");
                WaitDialog.Close();
                return;
            }

            foreach (CTFuzzyLookupAccount _item in _lstAccounts)
                _lstMatchAccounts.Add(new ClassesProperty.ManualMatchAccount() {
                    // match properties goes here ...
                    fuzzy_lookup_account_id = _item.id,
                    match_account_id = (string.IsNullOrEmpty(_item.account_id.ToString()) || _item.account_id < 1) ? 0 : _item.account_id,
                    import_data_company_name = _item.import_company_name,
                    master_data_company_name = _item.company_name,
                    is_match = (string.IsNullOrEmpty(_item.account_id.ToString()) || _item.account_id < 1)? false: true,

                    // column field values goes here ...
                    org_no = _item.import_org_no,
                    address = string.Format("{0} {1}, {2}, {3} {4}", 
                        _item.import_box_address,
                        _item.import_street_address,
                        _item.import_city,
                        _item.import_country,
                        _item.import_zipcode
                    ),
                    city = _item.import_city,
                    zip_code = _item.import_zipcode,
                    country = _item.import_country,
                    telephone = _item.import_telephone,
                    validated = (string.IsNullOrEmpty(_item.validated.ToString()) || _item.validated < 1)? false: true
                });
            
            FrmManualMatchAccount _form = new FrmManualMatchAccount() {
                StartPosition = FormStartPosition.CenterParent,
                lstMatchAccounts = _lstMatchAccounts
            };
            _form.RenderMatchAccounts();
            WaitDialog.Close();
            _form.ShowDialog(this.ParentForm);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update the import list view matched by account check box item
        /// </summary>
        private void MatchImportListByAccount()
        {
            GridColumn objGridCol = gvImportFile.Columns["matched_by_account"];
            gvImportFile.SetRowCellValue(m_SelectedImportFileRowHandle, objGridCol, true);
        }

        /// <summary>
        /// Set the required controls for company matching
        /// </summary>
        private void SetAccountMatchingRequiredControls()
        {
            if (cboMatchCriteriaCompany.Text.Equals("Company Name") && cboMatchTypeCompany.Text.Equals("Fuzzy"))
            {
                cmdExecuteMatchCompany.Enabled = true;
                lcgThreshold.Enabled = true;
            }
            else if (cboMatchCriteriaCompany.Text.Equals("Company Name") && cboMatchTypeCompany.Text.Equals("Exact"))
            {
                cmdExecuteMatchCompany.Enabled = true;
                lcgThreshold.Enabled = false;
            }
            else if (cboMatchCriteriaCompany.Text.Equals("Company Name + City") && cboMatchTypeCompany.Text.Equals("Exact"))
            {
                cmdExecuteMatchCompany.Enabled = true;
                lcgThreshold.Enabled = false;
            }
            else if (cboMatchCriteriaCompany.Text.Equals("Organization No") && cboMatchTypeCompany.Text.Equals("Exact"))
            {
                cmdExecuteMatchCompany.Enabled = true;
                lcgThreshold.Enabled = false;
            }
            else
            {
                cmdExecuteMatchCompany.Enabled = false;
                lcgThreshold.Enabled = false;
            }
        }

        /// <summary>
        /// Apply filter
        /// </summary>
        private void FilterFuzzyLookupAccountsView()
        {
            gvMatchingCompany.ClearColumnsFilter();
            string FilterString = string.Empty;

            if (cbxShowOnlyNonMatchedCompany.Checked)
                FilterString = "[matched] = 0";

            if (cbxShowOnlyValidatedCompany.Checked)
                FilterString = FilterString.Length > 0? FilterString + " AND [validated] = 1": "[validated] = 1";

            if (FilterString.Length > 0)
                gvMatchingCompany.ActiveFilterString = FilterString;
        }

        /// <summary>
        /// Save the selected fuzzy lookup accounts to the accounts master table
        /// </summary>
        private void SaveFuzzyLookupAccountsToMasterTable()
        {
            //if (m_ExistingCompanies > 0)
            //    Message = "There are " + m_SelectedCompanies.ToString() + " selected companies." + Environment.NewLine + Environment.NewLine
            //            + m_ExistingCompanies.ToString() + " of these already exist on the master data. Please revoke those first." + Environment.NewLine + Environment.NewLine
            //            + "Be understood that the non-existing companies will be added to master data as non-validated companies." + Environment.NewLine + Environment.NewLine
            //            + "Contact matching list will be generated for the selected companies." + Environment.NewLine + Environment.NewLine
            //            + "You want to continue?";
            //else
            //    Message = "There are " + m_SelectedCompanies.ToString() + " selected companies." + Environment.NewLine + Environment.NewLine
            //            + "Be understood that the non-existing companies will be added to master data as non-validated companies." + Environment.NewLine + Environment.NewLine
            //            + "Contact matching list will be generated for the selected companies." + Environment.NewLine + Environment.NewLine
            //            + "You want to continue?";

            /**
             * get existing accounts with m_FuzzyLookupAccountIds
             */
            BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            int _iExistingAccounts = Convert.ToInt32(_efDbModel.FICheckExistingAccounts(string.Join(",", m_FuzzyLookupAccountIds.ToArray())).SingleOrDefault());
            string Message = string.Empty;
            if (_iExistingAccounts > 0)
                Message = "There are " + m_SelectedCompanies.ToString() + " selected companies." + Environment.NewLine + Environment.NewLine
                        + _iExistingAccounts.ToString() + " of these selected companies are already existing. Please check for validated/unvalidated records." + Environment.NewLine + Environment.NewLine
                        + "Be understood that the non-existing companies will be added to master data as non-validated companies." + Environment.NewLine + Environment.NewLine
                        + "You want to continue?";
            else
                Message = "There are " + m_SelectedCompanies.ToString() + " selected companies." + Environment.NewLine + Environment.NewLine
                        + "Be understood that the non-existing companies will be added to master data as non-validated companies." + Environment.NewLine + Environment.NewLine
                        + "You want to continue?";

            DialogResult objDialog = MessageBox.Show(Message, "Add Import File Companies", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (objDialog == DialogResult.No)
                return;

            WaitDialog.Show(ParentForm,"Saving import file...");

            DataImportUtility.SaveFuzzyLookupAccountToMasterTable(m_objImportList.customer_id, m_objImportList.import_list_name, m_objImportList.id, m_FuzzyLookupAccountIds, BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Import_List);

            // save import list as matched
            DataImportUtility.MatchedByAccount(m_objImportList.id);
            gvImportFile.SetRowCellValue(m_SelectedImportFileRowHandle, "matched_by_account", true);
            //this.PopulateImportListView();
            //this.SetContactMatchingModule(true);
            this.PopulateFuzzyLookupAccountList();

            //Message = "Selected companies has been successfully saved to master data." + Environment.NewLine + Environment.NewLine 
            //        + "A new imported list has been created for " + m_objImportList.customer_name.ToUpper() 
            //        + " -> " + m_objImportList.import_list_name.ToUpper() + ".";

            //MessageBox.Show(Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            WaitDialog.Close();
        }

        /// <summary>
        /// Execute fuzzy match
        /// </summary>
        private void ExecuteFuzzyMatch()
        {
            //gcma.DataSource = null;
            //gridControlSimilarity.DataSource = null;
            //lblConfidenceRowCount.Text = "Total Rows: 0";
            //lblSimilarityRowCount.Text = "Total Rows: 0";
            //richTextBoxMessageLog.Text = "";
            //this.SetFocusedViewInstance();
            //m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcImportFile.FocusedView;
            //m_objImportProfileList = (ObjectImport.ImportListInstance)m_objGridView.GetFocusedRow();

            if (m_objImportList == null) {
                tcManageImport.SelectedTabPage = tpProfiling;
                return;
            }

            WaitDialog.Show("Queueing process ...");
            current_profile_list_id = m_objImportList.id;
            int _confidence = 0, _similarity = 0;
            int.TryParse(trackBarControlConfidence.EditValue.ToString(), out _confidence);
            int.TryParse(trackBarControlSimilarity.EditValue.ToString(), out _similarity);

            string _confidence_operator = comboBoxEditConfidence.EditValue.ToString();
            string _similarity_operator = comboBoxEditSimilarity.EditValue.ToString();
            byte _validated = (byte)m_MatchByAccountType;

            //string _fuzzy_ids = "'" + string.Join(",", m_FuzzyLookupAccountIds.ToArray()) + "'"; //todo: add value to this member; this is the selected ids to match.
            string _country = DataImportUtility.GetCountryCode(Convert.ToInt32(m_objImportList.country_id)); //todo: add value to this member; this is the country of search results
            string _fuzzy_match_field = SSISPackageMessage.Fuzzy_Company_Name; //check if combobox is match by fuzzy_company_name

            //Forms.FrmFuzzyMatching _frmFuzzyMatching = new Forms.FrmFuzzyMatching() {
            //    FuzzyMatchArgs = new Forms.FrmFuzzyMatching.FuzzyMatchArguments() {
            //        user_id = UserSession.CurrentUser.UserId,
            //        import_file_id = m_objImportList.id,
            //        country = _country,
            //        confidence = _confidence,
            //        similarity = _similarity,
            //        confidence_operator = _confidence_operator,
            //        similarity_operator = _similarity_operator,
            //        validated = _validated
            //    },
            //    ImportListId = m_objImportList.id,
            //    AccountIds = m_FuzzyLookupAccountIds,
            //    FuzzyMatchField = _fuzzy_match_field,
            //    StartPosition = FormStartPosition.CenterParent
            //};
            //_frmFuzzyMatching.ShowDialog(this.ParentForm);

            DataImportUtility.ClearFuzzyLookupAccountMatches(m_objImportList.id, m_FuzzyLookupAccountIds, true);
            ObjectArguments objArg = new ObjectArguments() {
                user_id = UserSession.CurrentUser.UserId,
                import_file_id = m_objImportList.id,
                country = _country,
                //fuzzy_ids = _fuzzy_ids,
                confidence = _confidence,
                similarity = _similarity,
                confidence_operator = _confidence_operator,
                similarity_operator = _similarity_operator,
                validated = _validated
            };
            //simpleButtonStartMatching.Enabled = false;
            //simpleButtonSave.Enabled = false;

            // This snippet is used if the implementation is when using Azure Message Queue or Azure Storage API
            try
            {
                //SetMessage("Loading SSIS account package...", true);
                var packageMsg = new SSISPackageMessage();

                packageMsg.PackageID = Guid.NewGuid().ToString();
                packageMsg.PackageType = "Account";
                packageMsg.Fuzzy_Match_Field = _fuzzy_match_field;
                packageMsg.UserID = UserSession.CurrentUser.UserId;
                packageMsg.ImportFileID = objArg.import_file_id;
                packageMsg.Country = objArg.country;                
                packageMsg.Similarity = objArg.similarity;
                packageMsg.Confidence = objArg.confidence;
                packageMsg.SimilarityOperator = objArg.similarity_operator;
                packageMsg.ConfidenceOperator = objArg.confidence_operator;
                packageMsg.Validated = objArg.validated;

                /** /
                string _SSISPackageName = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SSISPackageName"]);
                string _SSISPackageServer = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SSISPackageServer"]);
                string _SSISPackageServerUsername = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SSISPackageServerUsername"]);
                string _SSISPackageServerPassword = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SSISPackageServerPassword"]);
                Microsoft.SqlServer.Dts.Runtime.Application _app = new Microsoft.SqlServer.Dts.Runtime.Application();
                //Microsoft.SqlServer.Dts.Runtime.Package pkg = _app.LoadFromDtsServer(_SSISPackageName, _SSISPackageServer, null); //new Microsoft.SqlServer.Dts.Runtime.Package();
                Microsoft.SqlServer.Dts.Runtime.Package pkg = _app.LoadFromSqlServer(_SSISPackageName, _SSISPackageServer, _SSISPackageServerUsername, _SSISPackageServerPassword, null);

                pkg.Variables["import_file_id"].Value = objArg.import_file_id;
                pkg.Variables["country"].Value = objArg.country;
                pkg.Variables["similarity"].Value = objArg.similarity;
                pkg.Variables["confidence"].Value = objArg.confidence;
                pkg.Variables["similarity_operator"].Value = objArg.similarity_operator;
                pkg.Variables["confidence_operator"].Value = objArg.confidence_operator;
                pkg.Variables["validated"].Value = objArg.validated;
                DTSExecResult _result = pkg.Execute();
                /**/
                
                //SetMessage("Enqueuing account package to message queue...", true);
                m_oPackageQueue.AddMessage(packageMsg);
                gvImportFile.SetRowCellValue(gvImportFile.FocusedRowHandle, "fuzzy_match_status", "On Progress");
                alertControlSSISPackage.Show(this.ParentForm, "Bright Manager", string.Format("Fuzzy matching now processed at background.{0}You can now proceed with other task.", Environment.NewLine));
                WaitDialog.Close();
                return;

                /**
                 * temporarily commented so we can test the new implementation logic.
                 * we will manually need to clear the UserTextNotification table on azure storage weekly.
                 */
                #region Commented
                /** /
                //SetMessage("Account package enqueued successfully.", true);
                //SetMessage("Please wait while the account package is being processed...", true);
                string notificationTitle = string.Empty;
                if (packageMsg.Fuzzy_Match_Field == SSISPackageMessage.Fuzzy_Company_Name)
                {
                    WaitDialog.Show(ParentForm, "Processing...");
                    notificationTitle = "AccountNotification_FuzzyCompanyName";
                }
                while (true)
                {
                    
                    try
                    {
                        System.Threading.Thread.Sleep(3000);
                        //retrieve notifications
                        UserTextNotification[] userToasts = m_oRepository.GetNotificationsForUser(UserSession.CurrentUser.UserId.ToString());
                        if (userToasts != null && userToasts.Length > 0)
                        {
                            var data = (from UserTextNotification toast in userToasts
                                        where toast.Title == notificationTitle
                                        orderby toast.Timestamp descending
                                        select toast).ToArray();
                            if (data != null)
                            {
                                int _MaxChecking = 0;
                                if (data[0].MessageText.ToLower() == "success")
                                {
                                    while (true)
                                    {
                                        System.Threading.Thread.Sleep(2000);
                                        CTProcessedFuzzyLookupAccount _item = ObjectImport.CheckProcessedFuzzyMatchesCompanies(m_objImportList.id);
                                        if (_item.items_processed == _item.total_items)
                                            break;

                                        //if (ObjectImport.DoneFuzzyMatchingCompanies(m_objImportList.id))
                                        //    break;

                                        _MaxChecking++;
                                        if (_MaxChecking == 10)
                                            break;
                                    }

                                    //delete notification after use
                                    m_oRepository.DeleteNotification(data[0]);

                                    //SetMessage("Loading results to grids...", true);
                                    //System.Threading.Thread.Sleep(10000);
                                    m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
                                    this.PopulateFuzzyLookupAccountList();
                                    //gcMatchingCompany.DataSource = m_objBrightPlatformEntity.FIGetFuzzyAccounts(UserSession.CurrentUser.UserId, m_objImportList.id).ToList();
                                    //gridControlConfidence.DataSource = m_objBrightPlatformEntity.FIGetFuzzyConfidenceAccounts(UserSession.CurrentUser.UserId).ToList();
                                    //gridControlSimilarity.DataSource = m_objBrightPlatformEntity.FIGetFuzzySimilarityAccounts(UserSession.CurrentUser.UserId).ToList();
                                    //lblConfidenceRowCount.Text = "Total Rows: " + gvMatchingCompany.RowCount.ToString("N0");
                                    //lblSimilarityRowCount.Text = "Total Rows: " + gridViewSimilarity.RowCount.ToString("N0");
                                    System.Threading.Thread thread = new System.Threading.Thread(ShowAccountAlerter);
                                    thread.Start();
                                   WaitDialog.Close();
                                    //simpleButtonStartMatching.Invoke((MethodInvoker)delegate
                                    //{
                                    //    simpleButtonStartMatching.Enabled = true;
                                    //});
                                    //simpleButtonSave.Invoke((MethodInvoker)delegate
                                    //{
                                    //    simpleButtonSave.Enabled = true;
                                    //});
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                }
                /**/
                #endregion
            }
            catch {
                // do nothing ...
            }

            #region Commented
            //catch //(Exception ex)
            //{
                //simpleButtonStartMatching.Invoke((MethodInvoker)delegate
                //{
                //    simpleButtonStartMatching.Enabled = true;
                //});
                //simpleButtonSave.Invoke((MethodInvoker)delegate
                //{
                //    simpleButtonSave.Enabled = true;
                //});
            //}
            //[@jeff 09.28.2011]: http://brightvision.jira.com/browse/PLATFORM-562
            //finally
            //{

                //simpleButtonStartMatching.Invoke((MethodInvoker)delegate
                //{
                //    simpleButtonStartMatching.Enabled = true;
                //});
                //simpleButtonSave.Invoke((MethodInvoker)delegate
                //{
                //    simpleButtonSave.Enabled = true;
                //});
            //}

            //SetMessage("Package executed successfully.", true);
            //SetMessage("All completed.", false);
            //backgroundWorker1.RunWorkerAsync(objArg);
            #endregion
        }

        /// <summary>
        /// Execute fuzzy lookup account matching
        /// </summary>
        /// <param name="FuzzyLookupAccountMatchType"></param>
        private void ExecuteFuzzyLookupAccountMatching(eFuzzyLookupAccountMatchType FuzzyLookupAccountMatchType)
        {
            switch (FuzzyLookupAccountMatchType)
            {
                case eFuzzyLookupAccountMatchType.FuzzyCompanyName:
                    //this.PopulateFuzzyLookupAccountList();
                    this.ExecuteFuzzyMatch();
                    break;

                case eFuzzyLookupAccountMatchType.ExactCompanyName:
                    DataImportUtility.UpdateFuzzyLookupAccountsByExactCompanyName(m_objImportList.id, m_FuzzyLookupAccountIds, m_MatchByAccountType);
                    this.PopulateFuzzyLookupAccountList();
                    break;

                case eFuzzyLookupAccountMatchType.ExactOrganizationNo:
                    DataImportUtility.UpdateFuzzyLookupAccountsByExactOrgNo(m_objImportList.id, m_FuzzyLookupAccountIds, m_MatchByAccountType);
                    this.PopulateFuzzyLookupAccountList();
                    break;

                case eFuzzyLookupAccountMatchType.ExactCompanyNameWithCity:
                    DataImportUtility.UpdateFuzzyLookupAccountsByExactCompanyNameWithCity(m_objImportList.id, m_FuzzyLookupAccountIds, m_MatchByAccountType);
                    this.PopulateFuzzyLookupAccountList();
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PopulateFuzzyLookupAccountList()
        {
            m_FuzzyLookupAccountList = null;
            gcMatchingCompany.DataSource = null;
            if (m_objImportList == null)
                return;

            WaitDialog.Show(ParentForm, "Loading...");
            m_FuzzyLookupAccountList = DataImportUtility.GetFuzzyLookupAccountList(m_objImportList.id);
            if (m_FuzzyLookupAccountList.Count > 0) {
                gcMatchingCompany.BeginUpdate();
                gcMatchingCompany.DataSource = m_FuzzyLookupAccountList;
                gcMatchingCompany.EndUpdate();
                this.SetFuzzyLookupAccountMatchStatistics(DataImportUtility.GetFuzzyLookupAccountMatchStatistics(m_objImportList.id));
                this.SetFuzzyLookupAccountMatchingViewControls(true);
            }
            else
                this.SetFuzzyLookupAccountMatchingViewControls(false);
            WaitDialog.Close();
        }

        /// <summary>
        /// Get the match list account ids
        /// </summary>
        private void GetFuzzyLookupAccountIds()
        {
            m_FuzzyLookupAccountIds = null;
            m_FuzzyLookupAccountIds = new List<string>();
            m_SelectedCompanies = 0;
            m_ExistingCompanies = 0;
            bool _IsSelected = false;
            bool _IsMatched = false;
            string _FuzzyLookupId = "";

            //for (int i = 0; i < m_FuzzyLookupAccountList.Count; i++)
            for (int i = 0; i < gvMatchingCompany.RowCount; i++)
            {
                _IsSelected = Convert.ToBoolean(gvMatchingCompany.GetRowCellValue(i, gvMatchingCompany.Columns["selected"]));
                //if (Convert.ToBoolean(m_FuzzyLookupAccountList[i].selected))
                if (_IsSelected)
                {
                    //m_FuzzyLookupAccountIds.Add(m_FuzzyLookupAccountList[i].id.ToString());
                    _FuzzyLookupId = gvMatchingCompany.GetRowCellValue(i, gvMatchingCompany.Columns["id"]).ToString();
                    m_FuzzyLookupAccountIds.Add(_FuzzyLookupId);
                    m_SelectedCompanies ++;

                    _IsMatched = Convert.ToBoolean(gvMatchingCompany.GetRowCellValue(i, gvMatchingCompany.Columns["matched"]));
                    //if (m_FuzzyLookupAccountList[i].matched)
                    if (_IsMatched)
                        m_ExistingCompanies ++;
                }
            }
        }

        /// <summary>
        /// Set statictics
        /// </summary>
        /// <param name="objStatistic"></param>
        private void SetFuzzyLookupAccountMatchStatistics(CTFuzzyLookupAccountMatchStatistics objStatistic)
        {
            lblNoOfCompany.Text = "No. Of Companies: " + objStatistic.total_records.ToString();
            lblFuzzyMatchCompanyName.Text = "Fuzzy Company Name: " + objStatistic.matched_by_fuzzy_company_name.ToString();
            lblExactMatchCompanyName.Text = "Exact Company Name: " + objStatistic.matched_by_precise_company_name.ToString();
            lblExactMatchCompanyOrgNo.Text = "Exact Org No: " + objStatistic.matched_by_precise_org_no.ToString();
            lblNonMatchedCompany.Text = "Non-Matched: " + objStatistic.non_matched.ToString();
            lblNewlyAddedCompany.Text = "Newly Added: " + objStatistic.newly_added.ToString();
        }

        /// <summary>
        /// Initialize the module
        /// </summary>
        private void InitFuzzyLookupAccountMatchingView()
        {
            gcMatchingCompany.DataSource = null;
            lblNoOfCompany.Text = "No. Of Companies: 0";
            lblFuzzyMatchCompanyName.Text = "Fuzzy Company Name: 0";
            lblExactMatchCompanyName.Text = "Exact Company Name: 0";
            lblExactMatchCompanyOrgNo.Text = "Exact Org No: 0";
            lblNonMatchedCompany.Text = "Non-Matched: 0";
            lblNewlyAddedCompany.Text = "Newly Added: 0";
            m_FuzzyLookupAccountList = null;
            this.SetFuzzyLookupAccountMatchingViewControls(false);
        }

        /// <summary>
        /// Enable / disable controls
        /// </summary>
        private void SetFuzzyLookupAccountMatchingViewControls(bool ControlStatus)
        {
            cboMatchCriteriaCompany.Enabled = ControlStatus;
            cboMatchTypeCompany.Enabled = ControlStatus;
            cmdExecuteMatchCompany.Enabled = ControlStatus;
            cmdManualMatchCompany.Enabled = ControlStatus;
            cbxValidatedCompany.Enabled = ControlStatus;
            cbxShowOnlyValidatedCompany.Enabled = ControlStatus;
            cbxShowOnlyNonMatchedCompany.Enabled = ControlStatus;
            cmdSelectAllCompany.Enabled = ControlStatus;
            cmdSelectNonMatchedCompany.Enabled = ControlStatus;
            cmdClearSelectionCompany.Enabled = ControlStatus;
            cmdRemoveMatchCompany.Enabled = ControlStatus;
            cmdMarkAsMatchedCompany.Enabled = ControlStatus;
            cmdSaveMatchCompany.Enabled = ControlStatus;
            gcMatchingCompany.Enabled = ControlStatus;
            cmdAccountMatchingHelp.Enabled = ControlStatus;
            cmdSelectMatchedCompany.Enabled = ControlStatus;
            cmdCreateCallListCompany.Enabled = ControlStatus;
            cmdCreateCollectedData.Enabled = ControlStatus;

            if (cboMatchTypeCompany.Text.Equals("Exact"))
                lcgThreshold.Enabled = false;
            else
                lcgThreshold.Enabled = true;
        }
        #endregion
        #endregion

        #region Grids Context Menu
        private void gvImportFile_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvImportRecord_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvProfiledData_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvMatchingCompany_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvMatchingContact_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion
    }
}
