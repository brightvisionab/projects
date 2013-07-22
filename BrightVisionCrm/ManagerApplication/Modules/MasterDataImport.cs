
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Transactions;

using BrightVision.Common.Business;
using System.IO;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using ManagerApplication.Business;
using BrightVision.Model;
using System.Data.SqlClient;
using BrightVision.Common.Utilities;
using System.Globalization;
using System.Linq;

namespace ManagerApplication.Modules
{
    public partial class MasterDataImport : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Members & Properties
        #endregion

        #region Private Members & Properties
        #region Company
        private ObjectFileHandler m_objFileHandler = null;
        private string m_sFileName = string.Empty;
        private DataTable m_dtColumnMatches = null;
        private BackgroundWorker m_bwColumnMatching = null;
        private BackgroundWorker m_bwImportToGrid = null;
        private BackgroundWorker m_bwUpdateMasterData = null;
        private List<CTAccountColumnName> m_lstAccountColumnNames = new List<CTAccountColumnName>();
        private DataTable m_dtImportFileMatchedAccounts = null;
        private int m_NoOfColumnsMatched = 0;
        #endregion
        #region Contact
        private ObjectFileHandler m_objFileHandlerContact = null;
        private string m_sFileNameContact = string.Empty;
        private DataTable m_dtColumnMatchesContact = null;
        private BackgroundWorker m_bwColumnMatchingContact = null;
        private BackgroundWorker m_bwImportToGridContact = null;
        private BackgroundWorker m_bwUpdateMasterDataContact = null;
        private List<CTContactColumnName> m_lstContactColumnNames = new List<CTContactColumnName>();
        private DataTable m_dtImportFileMatchedContacts = null;
        private int m_NoOfColumnsMatchedContact = 0;
        #endregion
        #endregion

        #region Constructor
        public MasterDataImport()
        {
            this.Visible = false;
            InitializeComponent();
            cmdUpdateToGrid.Enabled = false;
            cmdUpdateToMasterDataTable.Enabled = false;
            btnUpdateToGridContact.Enabled = false;
            btnUpdateToMasterDataTableContact.Enabled = false;
            this.GetCountries();
            this.Visible = true;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void GetCountries()
        {
            try
            {
                cboCountry.Properties.DataSource = null;
                cboCountry.Properties.DataSource = ObjectCountry.GetCountries().Execute(System.Data.Objects.MergeOption.AppendOnly);
                cboCountry.Properties.DisplayMember = "name";
                cboCountry.Properties.ValueMember = "id";
                cboCountry.Properties.Columns.Add(new LookUpColumnInfo("name"));
                cboCountry.EditValue = 211; // sweden as default

                cboCountryContact.Properties.DataSource = null;
                cboCountryContact.Properties.DataSource = ObjectCountry.GetCountries().Execute(System.Data.Objects.MergeOption.AppendOnly);
                cboCountryContact.Properties.DisplayMember = "name";
                cboCountryContact.Properties.ValueMember = "id";
                cboCountryContact.Properties.Columns.Add(new LookUpColumnInfo("name"));
                cboCountryContact.EditValue = 211; // sweden as default
            }
            catch { }
        }
        private void SetEditableControlsCompany(bool ControlState)
        {
            cmdSelectImportFile.Enabled = ControlState;
            cmdOpenLargeCompanyFile.Enabled = ControlState;
            cmdUpdateToGrid.Enabled = ControlState;
            cmdUpdateToMasterDataTable.Enabled = ControlState;
            gvImportFileData.OptionsBehavior.Editable = ControlState;
        }
        private void SetEditableControlsContact(bool ControlState)
        {
            btnBrowseFileContact.Enabled = ControlState;
            btnOpenLargeContactFile.Enabled = ControlState;
            btnUpdateToGridContact.Enabled = ControlState;
            btnUpdateToMasterDataTableContact.Enabled = ControlState;
            gvImportFileDataContact.OptionsBehavior.Editable = ControlState;
        }
        private int? TryParseInt(string sInput)
        {
            sInput = sInput.Replace(",", "");
            if (ValidationUtility.IsCurrency(sInput))
                return Convert.ToInt32(sInput, CultureInfo.InvariantCulture);
            else
                return 0;
        }
        private decimal TryParseDecimal(string sInput)
        {
            sInput = sInput.Replace(",", "");
            if (ValidationUtility.IsCurrency(sInput))
                return Convert.ToDecimal(sInput, CultureInfo.InvariantCulture);
            else
                return 0;
        }
        private byte TryParseByte(string sInput)
        {
            if (ValidationUtility.IsCurrency(sInput))
                return Convert.ToByte(sInput, CultureInfo.InvariantCulture);
            else
                return 0;
        }
        #endregion

        #region Object Events
        #region Company
        private void cmdSelectImportFile_Click(object sender, EventArgs e)
        {
            try {
                // browse excel import file
                OpenFileDialog objOpenFile = new OpenFileDialog();
                objOpenFile.Title = "Select an import excel file";
                objOpenFile.InitialDirectory = @"c:\";
                objOpenFile.Filter = "Excel files (*.xls;*.xlsx)|*.xls;*.xlsx";
                objOpenFile.FilterIndex = 2;
                objOpenFile.RestoreDirectory = true;

                if (objOpenFile.ShowDialog() == DialogResult.OK)
                    txtImportFile.Text = objOpenFile.FileName;
                else
                    return;

                m_objFileHandler = null;
                m_objFileHandler = new ObjectFileHandler();
                List<string> ExcelSheetNames = new List<string>();

                gcMatchedColumn.DataSource = null;
                cboSheetName.Properties.Items.Clear();
                lblRecordStatistic.Text = "          Number of Records: 0";

                
                WaitDialog.Show(ParentForm, "Pulling import file...");

                // create temporary copy for importing
                if (File.Exists(m_sFileName) && m_sFileName.Length > 0)
                    File.Delete(m_sFileName);

                string _TempFileName = Path.GetFileName(txtImportFile.Text);
                string _TempFilePath = txtImportFile.Text.Replace(_TempFileName, "");
                m_sFileName = String.Format("{0}tmp_{1}", _TempFilePath, _TempFileName);
                if (File.Exists(m_sFileName))
                    File.Delete(m_sFileName);

                File.Copy(txtImportFile.Text, m_sFileName);

                // populate sheet name combo box
                if (m_objFileHandler.OpenExcelFile(m_sFileName)) {
                    ExcelSheetNames = m_objFileHandler.GetImportFileSheetNames(m_sFileName);
                    m_objFileHandler.CloseExcelFile();
                }
                else {
                    txtImportFile.Text = "";
                    WaitDialog.Close();
                    MessageBox.Show("Not a valid excel file", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (string SheetName in ExcelSheetNames)
                    cboSheetName.Properties.Items.Add(SheetName.Replace("$", ""));

                cboSheetName.SelectedIndex = 0;
                WaitDialog.Close();
                gcMatchedColumn.DataSource = null;
                gcImportFileData.DataSource = null;
                cmdUpdateToGrid.Enabled = false;
                cmdUpdateToMasterDataTable.Enabled = false;
            }
            catch { }
        }
        private void cmdOpenLargeCompanyFile_Click(object sender, EventArgs e)
        {
            if (!File.Exists(m_sFileName) || txtImportFile.Text.Length < 1) {
                MessageBox.Show("Please select an import file.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (cboSheetName.Text.Length < 1) {
                MessageBox.Show("Please select an import sheet name.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (Convert.ToInt32(cboCountry.EditValue) < 1) {
                MessageBox.Show("Please select a country.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try {
                /**
                 * start column matching process
                 * note: if cancelling of matching process is needed later, just use m_bwImport.CancelAsync();
                 */

                m_bwColumnMatching = null;
                m_bwColumnMatching = new BackgroundWorker { 
                    WorkerSupportsCancellation = true 
                };
                m_bwColumnMatching.DoWork += new DoWorkEventHandler(m_bwColumnMatching_DoWork);
                m_bwColumnMatching.RunWorkerAsync();
            }
            catch { }
        }
        private void m_bwColumnMatching_DoWork(object sender, DoWorkEventArgs e)
        {
            m_objFileHandler = new ObjectFileHandler();
            m_dtColumnMatches = new DataTable();
            m_dtColumnMatches = DataImportUtility.CreateLargeCompanyImportColumnMatching();

            this.Invoke(new MethodInvoker(delegate  {
                WaitDialog.Show(ParentForm, "Matching import file...");
                cmdUpdateToGrid.Enabled = false;
                cmdUpdateToMasterDataTable.Enabled = false;

                gcMatchedColumn.DataSource = null;
                if (!m_objFileHandler.OpenExcelFile(m_sFileName)) {
                    cmdUpdateToGrid.Enabled = true;
                    WaitDialog.Close();
                    e.Cancel = true;
                    return;
                }

                List<string> _lstImportFileColumns = m_objFileHandler.GetImportFileColumnNames(m_sFileName, cboSheetName.Text);
                if (_lstImportFileColumns.Count < 1) {
                    cmdUpdateToGrid.Enabled = true;
                    MessageBox.Show("Selected import file has no columns.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    WaitDialog.Close();
                    e.Cancel = true;
                    return;
                }

                m_NoOfColumnsMatched = 0;
                int _iMatchedColumnIndex = 0;
                string _sMasterDataFieldName = string.Empty;
                layoutControlItem3.Text = "Identical Column Matching         now processing import file column matching ...";

                m_lstAccountColumnNames = null;
                m_lstAccountColumnNames = new List<CTAccountColumnName>();
                m_lstAccountColumnNames = ObjectCompany.GetAccountColumnNames();
                for (int i = 0; i < m_lstAccountColumnNames.Count; i++) {
                    _sMasterDataFieldName = m_lstAccountColumnNames[i].master_data_field;
                    _iMatchedColumnIndex = _lstImportFileColumns.IndexOf(_sMasterDataFieldName);
                    if (_iMatchedColumnIndex >= 0) {
                        m_lstAccountColumnNames[i].matched_column_no = _iMatchedColumnIndex.ToString();
                        m_lstAccountColumnNames[i].matched_column_name = _sMasterDataFieldName;
                        m_NoOfColumnsMatched ++;
                    }
                }

                gcMatchedColumn.DataSource = m_lstAccountColumnNames;
                gvMatchedColumn.BestFitColumns();
                layoutControlItem3.Text = "Identical Column Matching";
                m_objFileHandler.CloseExcelFile();
                cmdUpdateToGrid.Enabled = true;
                WaitDialog.Close();

            }));

            e.Cancel = true;
        }
        private void cmdUpdateToGrid_Click(object sender, EventArgs e)
        {
            if (!File.Exists(m_sFileName) || txtImportFile.Text.Length < 1) {
                MessageBox.Show("Please select an import file.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (cboSheetName.Text.Length < 1) {
                MessageBox.Show("Please select an import sheet name.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (Convert.ToInt32(cboCountry.EditValue) < 1) {
                MessageBox.Show("Please select a country.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (m_NoOfColumnsMatched < 1) {
                MessageBox.Show("No matched columns.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (gvMatchedColumn == null) {
                MessageBox.Show("No matched columns.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (gvMatchedColumn.RowCount < 1) {
                MessageBox.Show("No matched columns.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try {
                m_bwImportToGrid = null;
                m_bwImportToGrid = new BackgroundWorker { WorkerSupportsCancellation = true };
                m_bwImportToGrid.DoWork += new DoWorkEventHandler(m_bwImportToGrid_DoWork);
                m_bwImportToGrid.RunWorkerAsync();
            }
            catch { }
        }
        private void m_bwImportToGrid_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate  {
                WaitDialog.Show(ParentForm, "Processing import file...");
                this.SetEditableControlsCompany(false);

                /**
                 * read excel file first, if no records, cancel matching process.
                 */
                gcImportFileData.DataSource = null;
                lblRecordStatistic.Text = "          Number of Records: 0";
                if (!m_objFileHandler.OpenExcelFile(m_sFileName)) {
                    MessageBox.Show("Cannot open or invalid import file.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.SetEditableControlsCompany(true);
                    cmdUpdateToMasterDataTable.Enabled = false;
                    WaitDialog.Close();
                    e.Cancel = true;
                    return;
                }

                /**
                 * get the org_no's/account id's first, so we can query this against the master data,
                 * and get initial results.
                 */
                DataTable _dtImportFileItems = null;
                if (cboMatchBy.Text.Equals("Account Id"))
                    _dtImportFileItems = m_objFileHandler.GetFileContents(cboSheetName.Text + "$", "[id]", "WHERE len([id]) > 0");
                else if (cboMatchBy.Text.Equals("Org No"))
                    _dtImportFileItems = m_objFileHandler.GetFileContents(cboSheetName.Text + "$", "[org_no]", "WHERE len([org_no]) > 0");

                if (_dtImportFileItems.Rows.Count < 1) {
                    MessageBox.Show("Import file has no data.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.SetEditableControlsCompany(true);
                    cmdUpdateToMasterDataTable.Enabled = false;
                    WaitDialog.Close();
                    e.Cancel = true;
                    return;
                }

                /**
                 * we add another data column to handle conversion from different data type to string,
                 * since we will need to pass the org_no's as string parameter for the stored procedure.
                 */
                DataColumn _dcAdditionalColumn = new DataColumn("str_matched_item") { 
                    DataType = System.Type.GetType("System.String"), 
                    Expression = cboMatchBy.Text.Equals("Account Id") ? "Convert(id, 'System.String')" : "Convert(org_no, 'System.String')" 
                };
                
                _dtImportFileItems.Columns.Add(_dcAdditionalColumn);
                var _dvImportFileItems = _dtImportFileItems.DefaultView;
                var _dtToMatchItems = _dvImportFileItems.ToTable(true, "str_matched_item");

                /**
                 * get only matched columns for use in sql select field, no need to select all columns.
                 */
                List<string> _SelectColumns = new List<string>();
                List<string> _MasterDataSelectColumns = new List<string>();
                foreach (CTAccountColumnName _ColumnItem in m_lstAccountColumnNames) {
                    if (_ColumnItem.matched_column_no.Length < 1)
                        continue;

                    _SelectColumns.Add(_ColumnItem.master_data_field);
                    if (_ColumnItem.master_data_field.Equals("org_no") || _ColumnItem.master_data_field.Equals("id"))
                        continue;

                    _MasterDataSelectColumns.Add(String.Format("{0} = Convert(nvarchar(255), a.{0})", _ColumnItem.master_data_field));
                }

                /**
                 * we get our initial results for the master data and the import file.
                 * we will match the data from the master data against the import file row/row and cell/cell,
                 * except for those columns defined to be skipped though.
                 */
                #region Code Logic
                m_dtImportFileMatchedAccounts = null;
                DataTable _dtImportFileData = null;
                if (cboMatchBy.Text.Equals("Account Id")) {
                    m_dtImportFileMatchedAccounts = DataImportUtility.GetLargeCompanyImportMatchedAccounts(DataImportUtility.eMatchBy.AccountId, _dtToMatchItems, Convert.ToInt32(cboCountry.EditValue), _MasterDataSelectColumns);
                    _dtImportFileData = m_objFileHandler.GetFileContents(cboSheetName.Text + "$", string.Join(",", _SelectColumns.ToArray()), "WHERE len([id]) > 0 ORDER BY [id]");
                }
                else if (cboMatchBy.Text.Equals("Org No")) {
                    m_dtImportFileMatchedAccounts = DataImportUtility.GetLargeCompanyImportMatchedAccounts(DataImportUtility.eMatchBy.OrgNo, _dtToMatchItems, Convert.ToInt32(cboCountry.EditValue), _MasterDataSelectColumns);
                    _dtImportFileData = m_objFileHandler.GetFileContents(cboSheetName.Text + "$", string.Join(",", _SelectColumns.ToArray()), "WHERE len([org_no]) > 0 ORDER BY [org_no]");
                }

                DataTable _dtAccountStructure = DataImportUtility.CreateAccountTable();
                DataRow[] _drMatchedRows = null;
                for (int i = 0; i < m_dtImportFileMatchedAccounts.Rows.Count; i++) {
                    /**
                     * if no match was found against the import file, just continue to the next record.
                     * well always use index = 0 for the matched rows, we will assume that the import file 
                     * always have a distinct org_no entries
                     */
                    _drMatchedRows = null;
                    if (cboMatchBy.Text.Equals("Org No"))
                        _drMatchedRows = _dtImportFileData.Select(String.Format("org_no = '{0}'", m_dtImportFileMatchedAccounts.Rows[i]["org_no"]));
                    else if (cboMatchBy.Text.Equals("Account Id"))
                        _drMatchedRows = _dtImportFileData.Select(String.Format("id = '{0}'", m_dtImportFileMatchedAccounts.Rows[i]["id"]));

                    if (_drMatchedRows.Length < 1)
                        continue;

                    foreach (DataColumn _dcItem in m_dtImportFileMatchedAccounts.Columns) {
                        
                        /**
                         * we bypass non needed columns
                         */
                        if (_dcItem.ColumnName.Equals("action_type") || _dcItem.ColumnName.Equals("list_id") || _dcItem.ColumnName.Equals("id"))
                            continue;
                        
                        try {
                            /**
                             * if new record, just udpate directly and continue to the next record, no need to compare
                             */
                            if (m_dtImportFileMatchedAccounts.Rows[i]["action_type"].Equals("New Record")) {
                                m_dtImportFileMatchedAccounts.Rows[i][_dcItem.ColumnName] = _drMatchedRows[0][_dcItem.ColumnName].ToString().Replace("NULL", "");
                                continue;
                            }

                            /**
                             * we will need to strip off the ".00" from the money and number figures,
                             * so when comparing as string, we will have an exact comparison.
                             */
                            string _sMasterDataValue = m_dtImportFileMatchedAccounts.Rows[i][_dcItem.ColumnName].ToString();
                            if (_sMasterDataValue.EndsWith(".00"))
                                _sMasterDataValue = _sMasterDataValue.Remove(_sMasterDataValue.Length - 3, 3);

                            string _sImportFileValue = _drMatchedRows[0][_dcItem.ColumnName].ToString();
                            if (_sImportFileValue.EndsWith(".00"))
                                _sImportFileValue = _sImportFileValue.Remove(_sImportFileValue.Length - 3, 3);

                            _sMasterDataValue = _sMasterDataValue.Replace("NULL", "");
                            _sImportFileValue = _sImportFileValue.Replace("NULL", "");

                            /**
                             * assign the correct default value to have a more precise matching.
                             * like for example, if the cell value is supposedly to be an integer, and
                             * it has a null or no value, it will be assigned as 0.
                             */
                            if (_dtAccountStructure.Columns.Contains(_dcItem.ColumnName)) {
                                if (_dtAccountStructure.Columns[_dcItem.ColumnName].DataType.FullName.Equals("System.Decimal") ||
                                    _dtAccountStructure.Columns[_dcItem.ColumnName].DataType.FullName.Equals("System.Int32") ||
                                    _dtAccountStructure.Columns[_dcItem.ColumnName].DataType.FullName.Equals("System.Byte"))
                                {
                                    if (string.IsNullOrEmpty(_sMasterDataValue))
                                        _sMasterDataValue = "0";

                                    if (string.IsNullOrEmpty(_sImportFileValue))
                                        _sImportFileValue = "0";
                                }

                                else if (_dtAccountStructure.Columns[_dcItem.ColumnName].DataType.FullName.Equals("System.String")) {
                                    if (string.IsNullOrEmpty(_sMasterDataValue))
                                        _sMasterDataValue = "";

                                    if (string.IsNullOrEmpty(_sImportFileValue))
                                        _sImportFileValue = "";
                                }
                            }

                            /**
                             * now we will compare the master data column value against the import file column value.
                             * if they are not the same, we will write it as "New Value/Old Value" on the cell,
                             * and flag it out as "For Update"
                             */
                            if (_sMasterDataValue != _sImportFileValue) {
                                if (_sMasterDataValue.Length > 0 && _sImportFileValue.Length < 1) {
                                    m_dtImportFileMatchedAccounts.Rows[i][_dcItem.ColumnName] = _sMasterDataValue;
                                    continue;
                                }

                                else if (_sMasterDataValue.Length < 1 && _sImportFileValue.Length > 0)
                                    m_dtImportFileMatchedAccounts.Rows[i][_dcItem.ColumnName] = _sImportFileValue + "¶";

                                else if (_sMasterDataValue.Length > 0 && _sImportFileValue.Length > 0)
                                    m_dtImportFileMatchedAccounts.Rows[i][_dcItem.ColumnName] = _sImportFileValue + "[«]" + _sMasterDataValue;

                                else
                                    m_dtImportFileMatchedAccounts.Rows[i][_dcItem.ColumnName] = "";

                                m_dtImportFileMatchedAccounts.Rows[i]["action_type"] = "For Update";
                            }
                        }
                        catch {
                            /**
                             * just write a default value when encountered an error.
                             */
                            m_dtImportFileMatchedAccounts.Rows[i][_dcItem.ColumnName] = DBNull.Value;
                        }
                    }
                }
                #endregion

                /**
                 * we display our matched results and close the excel file
                 */
                DevExpress.XtraEditors.Repository.RepositoryItemComboBox cboActionType = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
                cboActionType.TextEditStyle = TextEditStyles.DisableTextEditor;
                cboActionType.Items.Add("For Update");
                cboActionType.Items.Add("New Record");
                cboActionType.Items.Add("No Changes");

                gcImportFileData.DataSource = null;
                lblRecordStatistic.Text = "          Number of Records: 0";
                if (m_dtImportFileMatchedAccounts != null && m_dtImportFileMatchedAccounts.Rows.Count > 0) {
                    gcImportFileData.DataSource = m_dtImportFileMatchedAccounts;
                    gvImportFileData.Columns["list_id"].OptionsColumn.AllowEdit = false;
                    gvImportFileData.Columns["list_id"].OptionsColumn.AllowFocus = false;
                    gvImportFileData.Columns["id"].Caption = "account_id";
                    gvImportFileData.Columns["id"].OptionsColumn.AllowEdit = false;
                    gvImportFileData.Columns["id"].OptionsColumn.AllowFocus = false;
                    if (cboMatchBy.Text.Equals("Org No")) {
                        gvImportFileData.Columns["org_no"].OptionsColumn.AllowEdit = false;
                        gvImportFileData.Columns["org_no"].OptionsColumn.AllowFocus = false;
                    }
                    else {
                        gvImportFileData.Columns["org_no"].OptionsColumn.AllowEdit = true;
                        gvImportFileData.Columns["org_no"].OptionsColumn.AllowFocus = true;
                    }
                    gvImportFileData.BestFitColumns();
                    gvImportFileData.Columns["action_type"].ColumnEdit = cboActionType;
                    gvImportFileData.Columns["action_type"].Width = 100;
                    lblRecordStatistic.Text = "          Number of Records: " + m_dtImportFileMatchedAccounts.Rows.Count.ToString();
                }

                m_objFileHandler.CloseExcelFile();
                this.SetEditableControlsCompany(true);
                WaitDialog.Close();

            }));

            e.Cancel = true;
        }
        private void cmdUpdateToMasterDataTable_Click(object sender, EventArgs e)
        {
            if (m_dtImportFileMatchedAccounts.Rows.Count < 1 || m_dtImportFileMatchedAccounts.Select("action_type <> 'No Changes'").Length < 1) {
                MessageBox.Show("No records to process.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int _iForUpdate = m_dtImportFileMatchedAccounts.Select("action_type = 'For Update'").Length;
            int _iNewRecord = m_dtImportFileMatchedAccounts.Select("action_type = 'New Record'").Length;
            int _iNoChanges = m_dtImportFileMatchedAccounts.Select("action_type = 'No Changes'").Length;

            string _sMessage = "You are about to update the accounts master data." + Environment.NewLine + Environment.NewLine
                             + "New records: " + _iNewRecord.ToString() + Environment.NewLine
                             + "For update: " + _iForUpdate.ToString() + Environment.NewLine
                             + "No Changes: " + _iNoChanges.ToString() + Environment.NewLine + Environment.NewLine
                             + "Are you sure to continue?";

            DialogResult _dlgrDialog = MessageBox.Show(_sMessage, "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlgrDialog == DialogResult.No)
                return;

            try
            {
                m_bwUpdateMasterData = null;
                m_bwUpdateMasterData = new BackgroundWorker();
                m_bwUpdateMasterData.WorkerSupportsCancellation = true;
                m_bwUpdateMasterData.DoWork += new DoWorkEventHandler(m_bwUpdateMasterData_DoWork);
                m_bwUpdateMasterData.RunWorkerAsync();
            }
            catch { }
        }
        private void m_bwUpdateMasterData_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate 
            {
                //BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
                WaitDialog.Show(ParentForm, "Processing import file...");
                this.SetEditableControlsCompany(false);
                string[] _sRowValues = null;
                DataTable _dtAccounts = m_dtImportFileMatchedAccounts.Copy();

                /**
                 * clean records to be updated first. get all the new values from all cells and re-update each cell values
                 * with the new values from the import data as from the format (New Value/Old Value)
                 * _dtAccounts will now have the cleaned records
                 */
                #region Code Logic
                for (int i = 0; i < _dtAccounts.Rows.Count; i++)
                {
                    foreach (DataColumn _dcItem in _dtAccounts.Columns)
                    {
                        // get off inverted p on column values 
                        _dtAccounts.Rows[i][_dcItem.ColumnName] = _dtAccounts.Rows[i][_dcItem.ColumnName].ToString().Replace("¶", "");

                        // we bypass non needed columns
                        //if (_dcItem.ColumnName.Equals("org_no") || _dcItem.ColumnName.Equals("action_type") || _dcItem.ColumnName.Equals("list_id") || _dcItem.ColumnName.Equals("account_idid"))
                        if (_dcItem.ColumnName.Equals("action_type") || _dcItem.ColumnName.Equals("list_id") || _dcItem.ColumnName.Equals("id"))
                            continue;

                        // we bypass "No Changes" rows
                        else if (_dtAccounts.Rows[i]["action_type"].Equals("No Changes"))
                            continue;

                        // we bypass null/empty cell values
                        else if (string.IsNullOrEmpty(_dtAccounts.Rows[i][_dcItem.ColumnName].ToString()))
                            continue;

                        // we bypass rows with no new values
                        // respectively, we dont update cell values that include separator "/" at index 0
                        else if (_dtAccounts.Rows[i][_dcItem.ColumnName].ToString().IndexOf("[«]") < 1)
                            continue;

                        try
                        {
                            _sRowValues = null;
                            _sRowValues = _dtAccounts.Rows[i][_dcItem.ColumnName].ToString().Split(new string[] { "[«]" }, StringSplitOptions.None);
                            if (_sRowValues.Length > 1)
                                _dtAccounts.Rows[i][_dcItem.ColumnName] = _sRowValues[0].ToString();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.SetEditableControlsCompany(true);
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                #endregion

                /**
                 * gather to be updated account records.
                 */
                #region Code Logic
                Guid _TransactionId = Guid.NewGuid();
                DataTable _dtToUpdateAccounts = DataImportUtility.CreateDataImportAccountTable();
                foreach (DataRow _drRow in _dtAccounts.Select("action_type = 'For Update'")) {

                    DataRow _drAccount = _dtToUpdateAccounts.NewRow();
                    foreach (DataColumn _dcAccount in _dtToUpdateAccounts.Columns) {
                        if (!_dtAccounts.Columns.Contains(_dcAccount.ColumnName))
                            continue;

                        string _DataRowValue = _drRow[_dcAccount.ColumnName].ToString();
                        if (!string.IsNullOrEmpty(_DataRowValue)) {

                            if (_dcAccount.DataType.FullName.Equals("System.String"))
                                _drAccount[_dcAccount.ColumnName] = _DataRowValue;

                            else if (_dcAccount.DataType.FullName.Equals("System.Decimal"))
                                _drAccount[_dcAccount.ColumnName] = this.TryParseDecimal(_DataRowValue);

                            else if (_dcAccount.DataType.FullName.Equals("System.Int32"))
                                _drAccount[_dcAccount.ColumnName] = this.TryParseInt(_DataRowValue);

                            else if (_dcAccount.DataType.FullName.Equals("System.Byte"))
                                _drAccount[_dcAccount.ColumnName] = this.TryParseByte(_DataRowValue);
                        }
                        else
                            _drAccount[_dcAccount.ColumnName] = System.DBNull.Value;
                    }

                    //_drAccount["org_no"] = string.IsNullOrEmpty(_drAccount["org_no"].ToString()) ? null : _drAccount["org_no"].ToString();
                    //_drAccount["company_name"] = string.IsNullOrEmpty(_drAccount["company_name"].ToString()) ? null : _drAccount["company_name"].ToString();
                    //_drAccount["box_address"] = string.IsNullOrEmpty(_drAccount["box_address"].ToString()) ? null : _drAccount["box_address"].ToString();
                    //_drAccount["street_address"] = string.IsNullOrEmpty(_drAccount["street_address"].ToString()) ? null : _drAccount["street_address"].ToString();
                    //_drAccount["zipcode"] = string.IsNullOrEmpty(_drAccount["zipcode"].ToString()) ? null : _drAccount["zipcode"].ToString();
                    //_drAccount["country"] = string.IsNullOrEmpty(_drAccount["country"].ToString()) ? null : _drAccount["country"].ToString();
                    //_drAccount["county"] = string.IsNullOrEmpty(_drAccount["county"].ToString()) ? null : _drAccount["county"].ToString();
                    //_drAccount["municipality"] = string.IsNullOrEmpty(_drAccount["municipality"].ToString()) ? null : _drAccount["municipality"].ToString();
                    //_drAccount["city"] = string.IsNullOrEmpty(_drAccount["city"].ToString()) ? null : _drAccount["city"].ToString();
                    //_drAccount["telephone"] = string.IsNullOrEmpty(_drAccount["telephone"].ToString()) ? null : _drAccount["telephone"].ToString();
                    //_drAccount["telefax"] = string.IsNullOrEmpty(_drAccount["telefax"].ToString()) ? null : _drAccount["telefax"].ToString();
                    //_drAccount["www"] = string.IsNullOrEmpty(_drAccount["www"].ToString()) ? null : _drAccount["www"].ToString();
                    //_drAccount["parent_company"] = string.IsNullOrEmpty(_drAccount["parent_company"].ToString()) ? null : _drAccount["parent_company"].ToString();
                    //_drAccount["year_established"] = string.IsNullOrEmpty(_drAccount["year_established"].ToString()) ? null : _drAccount["year_established"].ToString();
                    //_drAccount["activity_code"] = string.IsNullOrEmpty(_drAccount["activity_code"].ToString()) ? null : _drAccount["activity_code"].ToString();
                    //_drAccount["activity_code_2"] = string.IsNullOrEmpty(_drAccount["activity_code_2"].ToString()) ? null : _drAccount["activity_code_2"].ToString();
                    //_drAccount["currency"] = string.IsNullOrEmpty(_drAccount["currency"].ToString()) ? null : _drAccount["currency"].ToString();
                    //_drAccount["fiscal"] = string.IsNullOrEmpty(_drAccount["fiscal"].ToString()) ? null : _drAccount["fiscal"].ToString();
                    //_drAccount["turnover"] = string.IsNullOrEmpty(_drAccount["turnover"].ToString()) ? null : this.TryParseDecimal(_drAccount["turnover"].ToString());
                    //_drAccount["export"] = string.IsNullOrEmpty(_drAccount["export"].ToString()) ? null : this.TryParseDecimal(_drAccount["export"].ToString());
                    //_drAccount["result"] = string.IsNullOrEmpty(_drAccount["result"].ToString()) ? null : this.TryParseDecimal(_drAccount["result"].ToString());
                    //_drAccount["sales_abroad"] = string.IsNullOrEmpty(_drAccount["sales_abroad"].ToString()) ? null : this.TryParseDecimal(_drAccount["sales_abroad"].ToString());
                    //_drAccount["employees_total"] = string.IsNullOrEmpty(_drAccount["employees_total"].ToString()) ? null : this.TryParseInt(_drAccount["employees_total"].ToString());
                    //_drAccount["employees_abroad"] = string.IsNullOrEmpty(_drAccount["employees_abroad"].ToString()) ? 0 : this.TryParseInt(_drAccount["employees_abroad"].ToString());
                    //_drAccount["fiscal_2"] = string.IsNullOrEmpty(_drAccount["fiscal_2"].ToString()) ? string.Empty : _drAccount["fiscal_2"].ToString();
                    //_drAccount["turnover_2"] = string.IsNullOrEmpty(_drAccount["turnover_2"].ToString()) ? 0 : this.TryParseDecimal(_drAccount["turnover_2"].ToString());
                    //_drAccount["export_2"] = string.IsNullOrEmpty(_drAccount["export_2"].ToString()) ? 0 : this.TryParseDecimal(_drAccount["export_2"].ToString());
                    //_drAccount["result_2"] = string.IsNullOrEmpty(_drAccount["result_2"].ToString()) ? 0 : this.TryParseDecimal(_drAccount["result_2"].ToString());
                    //_drAccount["sales_abroad_2"] = string.IsNullOrEmpty(_drAccount["sales_abroad_2"].ToString()) ? 0 : this.TryParseDecimal(_drAccount["sales_abroad_2"].ToString());
                    //_drAccount["employees_total_2"] = string.IsNullOrEmpty(_drAccount["employees_total_2"].ToString()) ? 0 : this.TryParseInt(_drAccount["employees_total_2"].ToString());
                    //_drAccount["employees_abroad_2"] = string.IsNullOrEmpty(_drAccount["employees_abroad_2"].ToString()) ? 0 : this.TryParseInt(_drAccount["employees_abroad_2"].ToString());
                    //_drAccount["turnover_2"] = string.IsNullOrEmpty(_drAccount["fiscal_3"].ToString()) ? 0 : this.TryParseDecimal(_drAccount["fiscal_3"].ToString());
                    //_drAccount["turnover_3"] = string.IsNullOrEmpty(_drAccount["turnover_3"].ToString()) ? 0 : this.TryParseDecimal(_drAccount["turnover_3"].ToString());
                    //_drAccount["export_3"] = string.IsNullOrEmpty(_drAccount["export_3"].ToString()) ? 0 : this.TryParseDecimal(_drAccount["export_3"].ToString());
                    //_drAccount["result_3"] = string.IsNullOrEmpty(_drAccount["result_3"].ToString()) ? 0 : this.TryParseDecimal(_drAccount["result_3"].ToString());
                    //_drAccount["sales_abroad_3"] = string.IsNullOrEmpty(_drAccount["sales_abroad_3"].ToString()) ? 0 : this.TryParseDecimal(_drAccount["sales_abroad_3"].ToString());
                    //_drAccount["employees_total_3"] = string.IsNullOrEmpty(_drAccount["employees_total_3"].ToString()) ? 0 : this.TryParseInt(_drAccount["employees_total_3"].ToString());
                    //_drAccount["employees_abroad_3"] = string.IsNullOrEmpty(_drAccount["employees_abroad_3"].ToString()) ? 0 : this.TryParseInt(_drAccount["employees_abroad_3"].ToString());
                    //_drAccount["source_cred"] = string.IsNullOrEmpty(_drAccount["source_cred"].ToString()) ? (byte)0 : this.TryParseByte(_drAccount["source_cred"].ToString());
                    //_drAccount["category"] = string.IsNullOrEmpty(_drAccount["category"].ToString()) ? string.Empty : _drAccount["category"].ToString();
                    //_drAccount["bv_source"] = string.IsNullOrEmpty(_drAccount["bv_source"].ToString()) ? string.Empty : _drAccount["bv_source"].ToString();
                    //_drAccount["regions"] = string.IsNullOrEmpty(_drAccount["regions"].ToString()) ? string.Empty : _drAccount["regions"].ToString();
                    //_drAccount["remarks"] = string.IsNullOrEmpty(_drAccount["remarks"].ToString()) ? string.Empty : _drAccount["remarks"].ToString();
                    //_drAccount["profile_data"] = string.IsNullOrEmpty(_drAccount["profile_data"].ToString()) ? string.Empty : _drAccount["profile_data"].ToString();
                    //_drAccount["active"] = string.IsNullOrEmpty(_drAccount["active"].ToString()) ? false : Convert.ToBoolean(_drAccount["active"].ToString());

                    //if (string.IsNullOrEmpty(_drAccount["active"].ToString()))
                    //    _drAccount["active"] = 0;

                    _drAccount["transaction_id"] = _TransactionId.ToString();
                    _drAccount["account_id"] = Convert.ToInt32(_drRow["id"]);
                    _drAccount["last_modified_machine"] = UserSession.CurrentUser.ComputerName;
                    _drAccount["last_modified_source"] = BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Master_Data_Import;

                    if (_drAccount != null)
                        _dtToUpdateAccounts.Rows.Add(_drAccount);
                }

                /**
                 * previous logic using EF. very slow when updating bulky records.
                 * not applicable to implement, takes too long to finish.
                 */
                /** /
                int _AcctId = 0;
                account _efAccount = new account();
                List<account> _lstUpdatedAccounts = new List<account>();
                foreach (DataRow _drRow in _dtAccounts.Select("action_type = 'For Update'")) 
                {
                    _efAccount = null;
                    _AcctId = Convert.ToInt32(_drRow["id"]);
                    _efAccount = _efDbModel.accounts.FirstOrDefault(i => i.id == _AcctId);

                    if (_dtAccounts.Columns.Contains("org_no"))
                        _efAccount.org_no = string.IsNullOrEmpty(_drRow["org_no"].ToString()) ? string.Empty : _drRow["org_no"].ToString();
                    if (_dtAccounts.Columns.Contains("company_name"))
                        _efAccount.company_name = string.IsNullOrEmpty(_drRow["company_name"].ToString()) ? string.Empty : _drRow["company_name"].ToString();
                    if (_dtAccounts.Columns.Contains("box_address"))
                        _efAccount.box_address = string.IsNullOrEmpty(_drRow["box_address"].ToString()) ? string.Empty : _drRow["box_address"].ToString();
                    if (_dtAccounts.Columns.Contains("street_address"))
                        _efAccount.street_address = string.IsNullOrEmpty(_drRow["street_address"].ToString()) ? string.Empty : _drRow["street_address"].ToString();
                    if (_dtAccounts.Columns.Contains("zipcode"))
                        _efAccount.zipcode = string.IsNullOrEmpty(_drRow["zipcode"].ToString()) ? string.Empty : _drRow["zipcode"].ToString();
                    if (_dtAccounts.Columns.Contains("country"))
                        _efAccount.country = string.IsNullOrEmpty(_drRow["country"].ToString()) ? string.Empty : _drRow["country"].ToString();
                    if (_dtAccounts.Columns.Contains("county"))
                        _efAccount.county = string.IsNullOrEmpty(_drRow["county"].ToString()) ? string.Empty : _drRow["county"].ToString();
                    if (_dtAccounts.Columns.Contains("municipality"))
                        _efAccount.municipality = string.IsNullOrEmpty(_drRow["municipality"].ToString()) ? string.Empty : _drRow["municipality"].ToString();
                    if (_dtAccounts.Columns.Contains("city"))
                        _efAccount.city = string.IsNullOrEmpty(_drRow["city"].ToString()) ? string.Empty : _drRow["city"].ToString();
                    if (_dtAccounts.Columns.Contains("telephone"))
                        _efAccount.telephone = string.IsNullOrEmpty(_drRow["telephone"].ToString()) ? string.Empty : _drRow["telephone"].ToString();
                    if (_dtAccounts.Columns.Contains("telefax"))
                        _efAccount.telefax = string.IsNullOrEmpty(_drRow["telefax"].ToString()) ? string.Empty : _drRow["telefax"].ToString();
                    if (_dtAccounts.Columns.Contains("www"))
                        _efAccount.www = string.IsNullOrEmpty(_drRow["www"].ToString()) ? string.Empty : _drRow["www"].ToString();
                    if (_dtAccounts.Columns.Contains("parent_company"))
                        _efAccount.parent_company = string.IsNullOrEmpty(_drRow["parent_company"].ToString()) ? string.Empty : _drRow["parent_company"].ToString();
                    if (_dtAccounts.Columns.Contains("year_established"))
                        _efAccount.year_established = string.IsNullOrEmpty(_drRow["year_established"].ToString()) ? string.Empty : _drRow["year_established"].ToString();
                    if (_dtAccounts.Columns.Contains("activity_code"))
                        _efAccount.activity_code = string.IsNullOrEmpty(_drRow["activity_code"].ToString()) ? string.Empty : _drRow["activity_code"].ToString();
                    if (_dtAccounts.Columns.Contains("activity_code_2"))
                        _efAccount.activity_code_2 = string.IsNullOrEmpty(_drRow["activity_code_2"].ToString()) ? string.Empty : _drRow["activity_code_2"].ToString();
                    if (_dtAccounts.Columns.Contains("currency"))
                        _efAccount.currency = string.IsNullOrEmpty(_drRow["currency"].ToString()) ? string.Empty : _drRow["currency"].ToString();
                    if (_dtAccounts.Columns.Contains("fiscal"))
                        _efAccount.fiscal = string.IsNullOrEmpty(_drRow["fiscal"].ToString()) ? string.Empty : _drRow["fiscal"].ToString();
                    if (_dtAccounts.Columns.Contains("turnover"))
                        _efAccount.turnover = string.IsNullOrEmpty(_drRow["turnover"].ToString()) ? 0 : this.TryParseDecimal(_drRow["turnover"].ToString());
                    if (_dtAccounts.Columns.Contains("export"))
                        _efAccount.export = string.IsNullOrEmpty(_drRow["export"].ToString()) ? 0 : this.TryParseDecimal(_drRow["export"].ToString());
                    if (_dtAccounts.Columns.Contains("result"))
                        _efAccount.result = string.IsNullOrEmpty(_drRow["result"].ToString()) ? 0 : this.TryParseDecimal(_drRow["result"].ToString());
                    if (_dtAccounts.Columns.Contains("sales_abroad"))
                        _efAccount.sales_abroad = string.IsNullOrEmpty(_drRow["sales_abroad"].ToString()) ? 0 : this.TryParseDecimal(_drRow["sales_abroad"].ToString());
                    if (_dtAccounts.Columns.Contains("employees_total"))
                        _efAccount.employees_total = string.IsNullOrEmpty(_drRow["employees_total"].ToString()) ? 0 : this.TryParseInt(_drRow["employees_total"].ToString());
                    if (_dtAccounts.Columns.Contains("employees_abroad"))
                        _efAccount.employees_abroad = string.IsNullOrEmpty(_drRow["employees_abroad"].ToString()) ? 0 : this.TryParseInt(_drRow["employees_abroad"].ToString());
                    if (_dtAccounts.Columns.Contains("fiscal_2"))
                        _efAccount.fiscal_2 = string.IsNullOrEmpty(_drRow["fiscal_2"].ToString()) ? string.Empty : _drRow["fiscal_2"].ToString();
                    if (_dtAccounts.Columns.Contains("turnover_2"))
                        _efAccount.turnover_2 = string.IsNullOrEmpty(_drRow["turnover_2"].ToString()) ? 0 : this.TryParseDecimal(_drRow["turnover_2"].ToString());
                    if (_dtAccounts.Columns.Contains("export_2"))
                        _efAccount.export_2 = string.IsNullOrEmpty(_drRow["export_2"].ToString()) ? 0 : this.TryParseDecimal(_drRow["export_2"].ToString());
                    if (_dtAccounts.Columns.Contains("result_2"))
                        _efAccount.result_2 = string.IsNullOrEmpty(_drRow["result_2"].ToString()) ? 0 : this.TryParseDecimal(_drRow["result_2"].ToString());
                    if (_dtAccounts.Columns.Contains("sales_abroad_2"))
                        _efAccount.sales_abroad_2 = string.IsNullOrEmpty(_drRow["sales_abroad_2"].ToString()) ? 0 : this.TryParseDecimal(_drRow["sales_abroad_2"].ToString());
                    if (_dtAccounts.Columns.Contains("employees_total_2"))
                        _efAccount.employees_total_2 = string.IsNullOrEmpty(_drRow["employees_total_2"].ToString()) ? 0 : this.TryParseInt(_drRow["employees_total_2"].ToString());
                    if (_dtAccounts.Columns.Contains("employees_abroad_2"))
                        _efAccount.employees_abroad_2 = string.IsNullOrEmpty(_drRow["employees_abroad_2"].ToString()) ? 0 : this.TryParseInt(_drRow["employees_abroad_2"].ToString());
                    if (_dtAccounts.Columns.Contains("fiscal_3"))
                        _efAccount.turnover_2 = string.IsNullOrEmpty(_drRow["fiscal_3"].ToString()) ? 0 : this.TryParseDecimal(_drRow["fiscal_3"].ToString());
                    if (_dtAccounts.Columns.Contains("turnover_3"))
                        _efAccount.turnover_3 = string.IsNullOrEmpty(_drRow["turnover_3"].ToString()) ? 0 : this.TryParseDecimal(_drRow["turnover_3"].ToString());
                    if (_dtAccounts.Columns.Contains("export_3"))
                        _efAccount.export_3 = string.IsNullOrEmpty(_drRow["export_3"].ToString()) ? 0 : this.TryParseDecimal(_drRow["export_3"].ToString());
                    if (_dtAccounts.Columns.Contains("result_3"))
                        _efAccount.result_3 = string.IsNullOrEmpty(_drRow["result_3"].ToString()) ? 0 : this.TryParseDecimal(_drRow["result_3"].ToString());
                    if (_dtAccounts.Columns.Contains("sales_abroad_3"))
                        _efAccount.sales_abroad_3 = string.IsNullOrEmpty(_drRow["sales_abroad_3"].ToString()) ? 0 : this.TryParseDecimal(_drRow["sales_abroad_3"].ToString());
                    if (_dtAccounts.Columns.Contains("employees_total_3"))
                        _efAccount.employees_total_3 = string.IsNullOrEmpty(_drRow["employees_total_3"].ToString()) ? 0 : this.TryParseInt(_drRow["employees_total_3"].ToString());
                    if (_dtAccounts.Columns.Contains("employees_abroad_3"))
                        _efAccount.employees_abroad_3 = string.IsNullOrEmpty(_drRow["employees_abroad_3"].ToString()) ? 0 : this.TryParseInt(_drRow["employees_abroad_3"].ToString());
                    if (_dtAccounts.Columns.Contains("source_cred"))
                        _efAccount.source_cred = string.IsNullOrEmpty(_drRow["source_cred"].ToString()) ? (byte)0 : this.TryParseByte(_drRow["source_cred"].ToString());
                    if (_dtAccounts.Columns.Contains("category"))
                        _efAccount.category = string.IsNullOrEmpty(_drRow["category"].ToString()) ? string.Empty : _drRow["category"].ToString();
                    if (_dtAccounts.Columns.Contains("bv_source"))
                        _efAccount.bv_source = string.IsNullOrEmpty(_drRow["bv_source"].ToString()) ? string.Empty : _drRow["bv_source"].ToString();
                    if (_dtAccounts.Columns.Contains("regions"))
                        _efAccount.regions = string.IsNullOrEmpty(_drRow["regions"].ToString()) ? string.Empty : _drRow["regions"].ToString();
                    if (_dtAccounts.Columns.Contains("remarks"))
                        _efAccount.remarks = string.IsNullOrEmpty(_drRow["remarks"].ToString()) ? string.Empty : _drRow["remarks"].ToString();
                    if (_dtAccounts.Columns.Contains("profile_data"))
                        _efAccount.profile_data = string.IsNullOrEmpty(_drRow["profile_data"].ToString()) ? string.Empty : _drRow["profile_data"].ToString();
                    if (_dtAccounts.Columns.Contains("active"))
                        _efAccount.active = string.IsNullOrEmpty(_drRow["active"].ToString()) ? false : Convert.ToBoolean(_drRow["active"].ToString());

                    _efAccount.modified_by = UserSession.CurrentUser.UserId;
                    _efAccount.modified_date = DateTime.Now;
                    _efAccount.last_modified_machine = UserSession.CurrentUser.ComputerName;
                    _efAccount.last_modified_source = BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Master_Data_Import;
                    
                    _lstUpdatedAccounts.Add(_efAccount);
                }
                /**/
                #endregion

                /**
                 * gather to be added account records.
                 */
                #region Code Logic
                DataTable _dtNewAccounts = DataImportUtility.CreateAccountTable();
                foreach (DataRow _drRow in _dtAccounts.Select("action_type = 'New Record'"))
                {
                    DataRow _drNewAccount = _dtNewAccounts.NewRow();
                    foreach (DataColumn _dcAccount in _dtNewAccounts.Columns)
                    {
                        if (!_dtAccounts.Columns.Contains(_dcAccount.ColumnName))
                            continue;

                        if (_dcAccount.DataType.FullName.Equals("System.String"))
                            _drNewAccount[_dcAccount.ColumnName] = _drRow[_dcAccount.ColumnName].ToString();

                        else if (_dcAccount.DataType.FullName.Equals("System.Decimal"))
                            _drNewAccount[_dcAccount.ColumnName] = this.TryParseDecimal(_drRow[_dcAccount.ColumnName].ToString());

                        else if (_dcAccount.DataType.FullName.Equals("System.Int32"))
                            _drNewAccount[_dcAccount.ColumnName] = this.TryParseInt(_drRow[_dcAccount.ColumnName].ToString());

                        else if (_dcAccount.DataType.FullName.Equals("System.Byte"))
                            _drNewAccount[_dcAccount.ColumnName] = this.TryParseByte(_drRow[_dcAccount.ColumnName].ToString());
                    }

                    _drNewAccount["created_by"] = UserSession.CurrentUser.UserId;
                    _drNewAccount["created_date"] = DateTime.Now;
                    _drNewAccount["modified_by"] = UserSession.CurrentUser.UserId;
                    _drNewAccount["modified_date"] = DateTime.Now;
                    _drNewAccount["last_modified_machine"] = UserSession.CurrentUser.ComputerName;
                    _drNewAccount["last_modified_source"] = BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Master_Data_Import;

                    if (_drNewAccount != null)
                        _dtNewAccounts.Rows.Add(_drNewAccount);
                }
                #endregion

                /**
                 * start the insert/update transaction.
                 * updates will be first processed over new data.
                 */
                #region Code Logic
                SqlConnection _sqlConnection = new SqlConnection(UserSession.ProviderConnection);
                _sqlConnection.Open();
                SqlTransaction _sqlTransaction = _sqlConnection.BeginTransaction();
                using (TransactionScope _efDbTransaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() {
                    Timeout = TimeSpan.FromMinutes(120),
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                })) {
                    try {
                        

                        /**
                         * process bulk insert of to update account data.
                         */
                        if (_dtToUpdateAccounts.Rows.Count > 0) {
                            DatabaseUtility.ExecuteBulkProcessing("vw_data_import_accounts", _dtToUpdateAccounts, _sqlConnection, _sqlTransaction);
                            SqlCommand _SqlCommand = new SqlCommand("bvUpdateAccounts_sp") {
                                Connection = _sqlConnection,
                                Transaction = _sqlTransaction,
                                CommandType = CommandType.StoredProcedure
                            };
                            _SqlCommand.Parameters.Add("@p_transaction_id", SqlDbType.NVarChar).Value = _TransactionId.ToString();
                            _SqlCommand.Parameters.Add("@p_user_id", SqlDbType.Int).Value = UserSession.CurrentUser.UserId;
                            _SqlCommand.ExecuteNonQuery();
                        }

                        /** /
                        if (_lstUpdatedAccounts.Count > 0) {
                            account _item = null;
                            int _id = 0;
                            for (int x = 0; x < _lstUpdatedAccounts.Count; x++) {
                                _id = _lstUpdatedAccounts[x].id;
                                _item = _efDbModel.accounts.FirstOrDefault(i => i.id == _id);
                                _efDbModel.accounts.ApplyCurrentValues(_lstUpdatedAccounts[x]);
                            }
                            _efDbModel.SaveChanges();
                        }
                        /**/
                        //ObjectCompany.SaveAccounts(_lstUpdatedAccounts, _efDbModel);

                        /**
                         * process new records using bulk insert.
                         * commit and accept all changes to the database.
                         * to be updated records will be committed on _efDbModel.AcceptAllChanges() call.
                         */
                        if (_dtNewAccounts.Rows.Count > 0)
                            DatabaseUtility.ExecuteBulkProcessing("vw_accounts", _dtNewAccounts, _sqlConnection, _sqlTransaction);

                        /**
                         * commit only if records to insert/update.
                         */
                        if (_dtNewAccounts.Rows.Count > 0 || _dtToUpdateAccounts.Rows.Count > 0)
                            _sqlTransaction.Commit();

                        //if (_lstUpdatedAccounts.Count > 0) {
                        //    _efDbModel.AcceptAllChanges();
                        //    _efDbTransaction.Complete();
                        //}

                        txtImportFile.Text = "";
                        cboSheetName.Properties.Items.Clear();
                        cboSheetName.Text = "";
                        gcMatchedColumn.DataSource = null;
                        gcImportFileData.DataSource = null;
                        lblRecordStatistic.Text = "          Number of Records: 0";
                        WaitDialog.Close();
                        MessageBox.Show("Successfully saved items to accounts master data.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        WaitDialog.Show("Initializing ...");
                    }
                    catch (Exception Ex) {
                        WaitDialog.Close();
                        MessageBox.Show(string.Format("Transaction rolled backed due to the ff:{0}{0}{1}", Environment.NewLine, Ex.Message), "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        WaitDialog.Show("Initializing ...");
                        _efDbTransaction.Dispose();
                        _sqlTransaction.Rollback();
                        //_efDbModel = null;
                        _sqlConnection = null;
                        this.SetEditableControlsCompany(true);
                        e.Cancel = true;
                        return;
                    }
                    finally {
                        _efDbTransaction.Dispose();
                        //_efDbModel = null;
                        _sqlConnection = null;                        
                    }
                }
                #endregion

                /**
                 * delete the temporary excel file created during the matching
                 */
                if (File.Exists(m_sFileName))
                    File.Delete(m_sFileName);
                
                this.SetEditableControlsCompany(true);
                cmdUpdateToGrid.Enabled = false;
                cmdUpdateToMasterDataTable.Enabled = false;
                WaitDialog.Close();

            }));

            e.Cancel = true;
        }
        private void gvImportFileData_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName.Equals("action_type"))
                return;

            if (gvImportFileData.GetFocusedRowCellDisplayText("action_type").Equals("New Record") || gvImportFileData.GetFocusedRowCellDisplayText("id").Length < 1)
                return;

            gvImportFileData.SetRowCellValue(e.RowHandle, "action_type", "For Update");
            //m_UpdatedGridCol = e.Column;
            //gvImportFileData.SelectCell(e.RowHandle, e.Column).
        }
        private void gvImportFileData_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            //if (e.Column.FieldName.Equals("org_no") || e.Column.FieldName.Equals("list_id") || e.Column.FieldName.Equals("account_id"))
            if (e.Column.FieldName.Equals("list_id") || e.Column.FieldName.Equals("id"))
                return;

            if (e.CellValue == null)
                return;

            //if (m_UpdatedGridCol == e.Column)
            //    e.Appearance.BackColor = Color.GreenYellow;
            
            if (e.CellValue.ToString().Contains("[«]"))
                e.Appearance.BackColor = Color.GreenYellow;

            else if ((e.CellValue.ToString().Contains("¶")))
                e.Appearance.BackColor = Color.GreenYellow;
                //string _NewValue = gvImportFileData.GetRowCellValue(e.RowHandle, e.Column).ToString().Replace("¶", "");
                //gvImportFileData.SetRowCellValue(e.RowHandle, e.Column, _NewValue);
        }
        private void gvImportFileData_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Value == null)
                return;

            if ((e.Value.ToString().Contains("¶")))
                e.DisplayText = e.Value.ToString().Replace("¶", "");
        }
        private void gvMatchedColumn_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gvImportFileData_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gvImportFileData_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView _gvData = sender as GridView;
            string _cellValue = _gvData.GetRowCellValue(gvImportFileData.FocusedRowHandle, "action_type").ToString();
            if (_cellValue.Equals("New Record") && gvImportFileData.FocusedColumn.ToString().Equals("action_type"))
                e.Cancel = true;
        }
        #endregion
        #region Contact
        private void btnBrowseFileContact_Click(object sender, EventArgs e)
        {
            try
            {
                // browse excel import file
                OpenFileDialog objOpenFile = new OpenFileDialog();
                objOpenFile.Title = "Select an import excel file";
                objOpenFile.InitialDirectory = @"c:\";
                objOpenFile.Filter = "Excel files (*.xls;*.xlsx)|*.xls;*.xlsx";
                objOpenFile.FilterIndex = 2;
                objOpenFile.RestoreDirectory = true;

                if (objOpenFile.ShowDialog() == DialogResult.OK)
                    tbxImportFileContact.Text = objOpenFile.FileName;
                else
                    return;

                m_objFileHandlerContact = null;
                m_objFileHandlerContact = new ObjectFileHandler();
                List<string> ExcelSheetNames = new List<string>();

                gcMatchedColumnContact.DataSource = null;
                cboSheetNameContact.Properties.Items.Clear();
                lblRecordStatisticContact.Text = "          Number of Records: 0";
                WaitDialog.Show("Pulling import file...");

                // create temporary copy for importing
                if (File.Exists(m_sFileNameContact) && m_sFileNameContact.Length > 0)
                    File.Delete(m_sFileNameContact);

                string _TempFileName = Path.GetFileName(tbxImportFileContact.Text);
                string _TempFilePath = tbxImportFileContact.Text.Replace(_TempFileName, "");
                m_sFileNameContact = _TempFilePath + "tmp_" + _TempFileName;
                if (File.Exists(m_sFileNameContact))
                    File.Delete(m_sFileNameContact);

                File.Copy(tbxImportFileContact.Text, m_sFileNameContact);

                // populate sheet name combo box
                if (m_objFileHandlerContact.OpenExcelFile(m_sFileNameContact))
                {
                    ExcelSheetNames = m_objFileHandlerContact.GetImportFileSheetNames(m_sFileNameContact);
                    m_objFileHandlerContact.CloseExcelFile();
                }
                else
                {
                    tbxImportFileContact.Text = "";
                    WaitDialog.Close();
                    MessageBox.Show("Not a valid excel file", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (string SheetName in ExcelSheetNames)
                    cboSheetNameContact.Properties.Items.Add(SheetName.Replace("$", ""));

                cboSheetNameContact.SelectedIndex = 0;
                WaitDialog.Close();
                gcMatchedColumnContact.DataSource = null;
                gcImportFileDataContact.DataSource = null;
                btnUpdateToGridContact.Enabled = false;
                btnUpdateToMasterDataTableContact.Enabled = false;
            }
            catch { }
        }
        private void btnOpenLargeContactFile_Click(object sender, EventArgs e)
        {
            if (!File.Exists(m_sFileNameContact) || tbxImportFileContact.Text.Length < 1)
            {
                MessageBox.Show("Please select an import file.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (cboSheetNameContact.Text.Length < 1)
            {
                MessageBox.Show("Please select an import sheet name.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (Convert.ToInt32(cboCountryContact.EditValue) < 1)
            {
                MessageBox.Show("Please select a country.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                /**
                 * start column matching process
                 * note: if cancelling of matching process is needed later, just use m_bwImport.CancelAsync();
                 */

                m_bwColumnMatchingContact = null;
                m_bwColumnMatchingContact = new BackgroundWorker();
                m_bwColumnMatchingContact.WorkerSupportsCancellation = true;
                m_bwColumnMatchingContact.DoWork += new DoWorkEventHandler(m_bwColumnMatchingContact_DoWork);
                m_bwColumnMatchingContact.RunWorkerAsync();
            }
            catch { }
        }
        private void m_bwColumnMatchingContact_DoWork(object sender, DoWorkEventArgs e)
        {
            m_objFileHandlerContact = new ObjectFileHandler();
            m_dtColumnMatchesContact = new DataTable();
            m_dtColumnMatchesContact = DataImportUtility.CreateLargeContactImportColumnMatching();

            this.Invoke(new MethodInvoker(delegate
            {
                WaitDialog.Show("Matching import file...");
                btnUpdateToGridContact.Enabled = false;
                btnUpdateToMasterDataTableContact.Enabled = false;

                gcMatchedColumnContact.DataSource = null;
                if (!m_objFileHandlerContact.OpenExcelFile(m_sFileNameContact))
                {
                    btnUpdateToGridContact.Enabled = true;
                    WaitDialog.Close();
                    e.Cancel = true;
                    return;
                }

                List<string> _lstImportFileColumns = m_objFileHandlerContact.GetImportFileColumnNames(m_sFileNameContact, cboSheetNameContact.Text);
                if (_lstImportFileColumns.Count < 1)
                {
                    btnUpdateToGridContact.Enabled = true;
                    MessageBox.Show("Selected import file has no columns.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    WaitDialog.Close();
                    e.Cancel = true;
                    return;
                }

                m_NoOfColumnsMatchedContact = 0;
                int _iMatchedColumnIndex = 0;
                string _sMasterDataFieldName = string.Empty;
                layoutControlItem13.Text = "Identical Column Matching         now processing import file column matching ...";

                m_lstContactColumnNames = null;
                m_lstContactColumnNames = new List<CTContactColumnName>();
                m_lstContactColumnNames = ObjectContact.GetContactColumnNames().ToList();
                for (int i = 0; i < m_lstContactColumnNames.Count; i++) {
                    _sMasterDataFieldName = m_lstContactColumnNames[i].master_data_field;
                    _iMatchedColumnIndex = _lstImportFileColumns.IndexOf(_sMasterDataFieldName);
                    if (_iMatchedColumnIndex >= 0)
                    {
                        m_lstContactColumnNames[i].matched_column_no = _iMatchedColumnIndex.ToString();
                        m_lstContactColumnNames[i].matched_column_name = _sMasterDataFieldName;
                        m_NoOfColumnsMatchedContact++;
                    }
                }

                gcMatchedColumnContact.DataSource = m_lstContactColumnNames;
                gvMatchedColumnContact.BestFitColumns();
                layoutControlItem13.Text = "Identical Column Matching";
                m_objFileHandlerContact.CloseExcelFile();
                btnUpdateToGridContact.Enabled = true;
                WaitDialog.Close();

            }));

            e.Cancel = true;
        }
        private void btnUpdateToGridContact_Click(object sender, EventArgs e)
        {
            if (!File.Exists(m_sFileNameContact) || tbxImportFileContact.Text.Length < 1)
            {
                MessageBox.Show("Please select an import file.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (cboSheetNameContact.Text.Length < 1)
            {
                MessageBox.Show("Please select an import sheet name.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (Convert.ToInt32(cboCountryContact.EditValue) < 1)
            {
                MessageBox.Show("Please select a country.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (m_NoOfColumnsMatchedContact < 1)
            {
                MessageBox.Show("No matched columns.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (gvMatchedColumnContact == null)
            {
                MessageBox.Show("No matched columns.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (gvMatchedColumnContact.RowCount < 1)
            {
                MessageBox.Show("No matched columns.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                m_bwImportToGridContact = null;
                m_bwImportToGridContact = new BackgroundWorker();
                m_bwImportToGridContact.WorkerSupportsCancellation = true;
                m_bwImportToGridContact.DoWork += new DoWorkEventHandler(m_bwImportToGridContact_DoWork);
                m_bwImportToGridContact.RunWorkerAsync();
            }
            catch { }
        }
        private void m_bwImportToGridContact_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                WaitDialog.Show("Processing import file...");
                this.SetEditableControlsContact(false);

                /**
                 * read excel file first, if no records, cancel matching process.
                 */
                gcImportFileDataContact.DataSource = null;
                lblRecordStatisticContact.Text = "          Number of Records: 0";
                if (!m_objFileHandlerContact.OpenExcelFile(m_sFileNameContact))
                {
                    MessageBox.Show("Cannot open or invalid import file.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.SetEditableControlsContact(true);
                    btnUpdateToMasterDataTableContact.Enabled = false;
                    WaitDialog.Close();
                    e.Cancel = true;
                    return;
                }

                /**
                 * get the emails/contact id's first, so we can query this against the master data,
                 * and get initial results.
                 */
                DataTable _dtImportFileItems = null;
                if (cboMatchByContact.Text.Equals("Contact Id"))
                    _dtImportFileItems = m_objFileHandlerContact.GetFileContents(cboSheetNameContact.Text + "$", "[id]", "WHERE len([id]) > 0");
                else if (cboMatchByContact.Text.Equals("Email"))
                    _dtImportFileItems = m_objFileHandlerContact.GetFileContents(cboSheetNameContact.Text + "$", "[email]", "WHERE len([email]) > 0");

                if (_dtImportFileItems.Rows.Count < 1)
                {
                    MessageBox.Show("Import file has no data.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.SetEditableControlsContact(true);
                    btnUpdateToMasterDataTableContact.Enabled = false;
                    WaitDialog.Close();
                    e.Cancel = true;
                    return;
                }

                /**
                 * we add another data column to handle conversion from different data type to string,
                 * since we will need to pass the email/contact id as string parameter for the stored procedure.
                 */
                DataColumn _dcAdditionalColumn = null;
                _dcAdditionalColumn = new DataColumn("str_matched_item");
                _dcAdditionalColumn.DataType = System.Type.GetType("System.String");
                _dcAdditionalColumn.Expression = cboMatchByContact.Text.Equals("Contact Id") ? "Convert(id, 'System.String')" : "Convert(email, 'System.String')";
                _dtImportFileItems.Columns.Add(_dcAdditionalColumn);

                DataView _dvImportFileItems = _dtImportFileItems.DefaultView;
                DataTable _dtToMatchItems = _dvImportFileItems.ToTable(true, "str_matched_item");

                /**
                 * get only matched columns for use in sql select field, no need to select all columns.
                 */
                List<string> _SelectColumns = new List<string>();
                List<string> _MasterDataSelectColumns = new List<string>();
                foreach (CTContactColumnName _ColumnItem in m_lstContactColumnNames)
                {
                    if (_ColumnItem.matched_column_no.Length < 1)
                        continue;

                    _SelectColumns.Add(_ColumnItem.master_data_field);
                    if (_ColumnItem.master_data_field.Equals("email") || _ColumnItem.master_data_field.Equals("id"))
                        continue;

                    _MasterDataSelectColumns.Add(String.Format("{0} = Convert(nvarchar(255), a.{0})", _ColumnItem.master_data_field));
                }

                /**
                 * we get our initial results for the master data and the import file.
                 * we will match the data from the master data against the import file row/row and cell/cell,
                 * except for those columns defined to be skipped though.
                 */
                #region Code Logic
                m_dtImportFileMatchedContacts = null;
                DataTable _dtImportFileData = null;
                if (cboMatchByContact.Text.Equals("Contact Id"))
                {
                    m_dtImportFileMatchedContacts = DataImportUtility.GetLargeCompanyImportMatchedContacts(DataImportUtility.eMatchBy.ContactId, _dtToMatchItems, Convert.ToInt32(cboCountryContact.EditValue), _MasterDataSelectColumns);
                    _dtImportFileData = m_objFileHandlerContact.GetFileContents(cboSheetNameContact.Text + "$", string.Join(",", _SelectColumns.ToArray()), "WHERE len([id]) > 0 ORDER BY [id]");
                }
                else if (cboMatchByContact.Text.Equals("Email"))
                {
                    m_dtImportFileMatchedContacts = DataImportUtility.GetLargeCompanyImportMatchedContacts(DataImportUtility.eMatchBy.Email, _dtToMatchItems, Convert.ToInt32(cboCountryContact.EditValue), _MasterDataSelectColumns);
                    _dtImportFileData = m_objFileHandlerContact.GetFileContents(cboSheetNameContact.Text + "$", string.Join(",", _SelectColumns.ToArray()), "WHERE len([email]) > 0 ORDER BY [email]");
                }

                DataTable _dtContactStructure = DataImportUtility.CreateContactTable();
                DataRow[] _drMatchedRows = null;
                for (int i = 0; i < m_dtImportFileMatchedContacts.Rows.Count; i++)
                {
                    /**
                     * if no match was found against the import file, just continue to the next record.
                     * well always use index = 0 for the matched rows, we will assume that the import file 
                     * always have a distinct org_no entries
                     */
                    _drMatchedRows = null;
                    if (cboMatchByContact.Text.Equals("Email"))
                        _drMatchedRows = _dtImportFileData.Select(String.Format("email = '{0}'", m_dtImportFileMatchedContacts.Rows[i]["email"]));
                    else if (cboMatchByContact.Text.Equals("Contact Id"))
                        _drMatchedRows = _dtImportFileData.Select(String.Format("id = '{0}'", m_dtImportFileMatchedContacts.Rows[i]["id"]));

                    if (_drMatchedRows.Length < 1)
                        continue;

                    foreach (DataColumn _dcItem in m_dtImportFileMatchedContacts.Columns)
                    {
                        /**
                         * we bypass non needed columns
                         */
                        //if (_dcItem.ColumnName.Equals("org_no") || _dcItem.ColumnName.Equals("action_type") || _dcItem.ColumnName.Equals("list_id") || _dcItem.ColumnName.Equals("id"))
                        if (_dcItem.ColumnName.Equals("action_type") || _dcItem.ColumnName.Equals("list_id") || _dcItem.ColumnName.Equals("id"))
                            continue;

                        try
                        {
                            /**
                             * if new record, just udpate directly and continue to the next record, no need to compare
                             */
                            if (m_dtImportFileMatchedContacts.Rows[i]["action_type"].Equals("New Record"))
                            {
                                m_dtImportFileMatchedContacts.Rows[i][_dcItem.ColumnName] = _drMatchedRows[0][_dcItem.ColumnName].ToString().Replace("NULL", "");
                                continue;
                            }

                            /**
                             * we will need to strip off the ".00" from the money and number figures,
                             * so when comparing as string, we will have an exact comparison.
                             */
                            string _sMasterDataValue = m_dtImportFileMatchedContacts.Rows[i][_dcItem.ColumnName].ToString();
                            if (_sMasterDataValue.EndsWith(".00"))
                                _sMasterDataValue = _sMasterDataValue.Remove(_sMasterDataValue.Length - 3, 3);

                            string _sImportFileValue = _drMatchedRows[0][_dcItem.ColumnName].ToString();
                            if (_sImportFileValue.EndsWith(".00"))
                                _sImportFileValue = _sImportFileValue.Remove(_sImportFileValue.Length - 3, 3);

                            _sMasterDataValue = _sMasterDataValue.Replace("NULL", "");
                            _sImportFileValue = _sImportFileValue.Replace("NULL", "");

                            /**
                             * assign the correct default value to have a more precise matching.
                             * like for example, if the cell value is supposedly to be an integer, and
                             * it has a null or no value, it will be assigned as 0.
                             */
                            if (_dtContactStructure.Columns.Contains(_dcItem.ColumnName))
                            {
                                if (_dtContactStructure.Columns[_dcItem.ColumnName].DataType.FullName.Equals("System.Decimal") ||
                                    _dtContactStructure.Columns[_dcItem.ColumnName].DataType.FullName.Equals("System.Int32") ||
                                    _dtContactStructure.Columns[_dcItem.ColumnName].DataType.FullName.Equals("System.Byte"))
                                {
                                    if (string.IsNullOrEmpty(_sMasterDataValue))
                                        _sMasterDataValue = "0";

                                    if (string.IsNullOrEmpty(_sImportFileValue))
                                        _sImportFileValue = "0";
                                }

                                else if (_dtContactStructure.Columns[_dcItem.ColumnName].DataType.FullName.Equals("System.String"))
                                {
                                    if (string.IsNullOrEmpty(_sMasterDataValue))
                                        _sMasterDataValue = "";

                                    if (string.IsNullOrEmpty(_sImportFileValue))
                                        _sImportFileValue = "";
                                }
                            }

                            /**
                             * now we will compare the master data column value against the import file column value.
                             * if they are not the same, we will write it as "New Value/Old Value" on the cell,
                             * and flag it out as "For Update"
                             */
                            if (_sMasterDataValue != _sImportFileValue)
                            {
                                if (_sMasterDataValue.Length > 0 && _sImportFileValue.Length < 1)
                                {
                                    m_dtImportFileMatchedContacts.Rows[i][_dcItem.ColumnName] = _sMasterDataValue;
                                    continue;
                                }

                                else if (_sMasterDataValue.Length < 1 && _sImportFileValue.Length > 0)
                                    m_dtImportFileMatchedContacts.Rows[i][_dcItem.ColumnName] = _sImportFileValue + "¶";

                                else if (_sMasterDataValue.Length > 0 && _sImportFileValue.Length > 0)
                                    m_dtImportFileMatchedContacts.Rows[i][_dcItem.ColumnName] = _sImportFileValue + "[«]" + _sMasterDataValue;

                                else
                                    m_dtImportFileMatchedContacts.Rows[i][_dcItem.ColumnName] = "";

                                m_dtImportFileMatchedContacts.Rows[i]["action_type"] = "For Update";
                            }
                        }
                        catch
                        {
                            /**
                             * just write a default value when encountered an error.
                             */
                            m_dtImportFileMatchedContacts.Rows[i][_dcItem.ColumnName] = DBNull.Value;
                        }
                    }
                }
                #endregion

                /**
                 * we display our matched results and close the excel file
                 */
                DevExpress.XtraEditors.Repository.RepositoryItemComboBox cboActionType = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
                cboActionType.TextEditStyle = TextEditStyles.DisableTextEditor;
                cboActionType.Items.Add("For Update");
                //cboActionType.Items.Add("New Record");
                cboActionType.Items.Add("No Changes");

                gcImportFileDataContact.DataSource = null;
                lblRecordStatisticContact.Text = "          Number of Records: 0";
                if (m_dtImportFileMatchedContacts != null && m_dtImportFileMatchedContacts.Rows.Count > 0)
                {
                    gcImportFileDataContact.DataSource = m_dtImportFileMatchedContacts;
                    gvImportFileDataContact.Columns["list_id"].OptionsColumn.AllowEdit = false;
                    gvImportFileDataContact.Columns["list_id"].OptionsColumn.AllowFocus = false;
                    gvImportFileDataContact.Columns["id"].Caption = "contact_id";
                    gvImportFileDataContact.Columns["id"].OptionsColumn.AllowEdit = false;
                    gvImportFileDataContact.Columns["id"].OptionsColumn.AllowFocus = false;
                    if (cboMatchByContact.Text.Equals("Email"))
                    {
                        gvImportFileDataContact.Columns["email"].OptionsColumn.AllowEdit = false;
                        gvImportFileDataContact.Columns["email"].OptionsColumn.AllowFocus = false;
                    }
                    else
                    {
                        gvImportFileDataContact.Columns["email"].OptionsColumn.AllowEdit = true;
                        gvImportFileDataContact.Columns["email"].OptionsColumn.AllowFocus = true;
                    }
                    gvImportFileDataContact.BestFitColumns();
                    gvImportFileDataContact.Columns["action_type"].ColumnEdit = cboActionType;
                    gvImportFileDataContact.Columns["action_type"].Width = 100;
                    lblRecordStatisticContact.Text = "          Number of Records: " + m_dtImportFileMatchedContacts.Rows.Count.ToString();
                }

                m_objFileHandlerContact.CloseExcelFile();
                this.SetEditableControlsContact(true);
                WaitDialog.Close();

            }));

            e.Cancel = true;
        }
        private void btnUpdateToMasterDataTableContact_Click(object sender, EventArgs e)
        {
            if (m_dtImportFileMatchedContacts.Rows.Count < 1 || m_dtImportFileMatchedContacts.Select("action_type <> 'No Changes'").Length < 1)
            {
                MessageBox.Show("No records to process.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int _iForUpdate = m_dtImportFileMatchedContacts.Select("action_type = 'For Update'").Length;
            int _iNewRecord = m_dtImportFileMatchedContacts.Select("action_type = 'New Record'").Length;
            int _iNoChanges = m_dtImportFileMatchedContacts.Select("action_type = 'No Changes'").Length;

            string _sMessage = "You are about to update the contacts master data." + Environment.NewLine + Environment.NewLine
                             + "New records: " + _iNewRecord.ToString() + Environment.NewLine
                             + "For update: " + _iForUpdate.ToString() + Environment.NewLine
                             + "No Changes: " + _iNoChanges.ToString() + Environment.NewLine + Environment.NewLine
                             + "Are you sure to continue?";

            DialogResult _dlgrDialog = MessageBox.Show(_sMessage, "Bright Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlgrDialog == DialogResult.No)
                return;

            try
            {
                m_bwUpdateMasterDataContact = null;
                m_bwUpdateMasterDataContact = new BackgroundWorker();
                m_bwUpdateMasterDataContact.WorkerSupportsCancellation = true;
                m_bwUpdateMasterDataContact.DoWork += new DoWorkEventHandler(m_bwUpdateMasterDataContact_DoWork);
                m_bwUpdateMasterDataContact.RunWorkerAsync();
            }
            catch { }
        }
        private void m_bwUpdateMasterDataContact_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate
            {

                BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
                WaitDialog.Show("Processing import file...");
                this.SetEditableControlsContact(false);
                string[] _sRowValues = null;
                DataTable _dtContacts = m_dtImportFileMatchedContacts.Copy();

                /**
                 * clean records to be updated first. get all the new values from all cells and re-update each cell values
                 * with the new values from the import data as from the format (New Value/Old Value)
                 * _dtAccounts will now have the cleaned records
                 */
                #region Code Logic
                for (int i = 0; i < _dtContacts.Rows.Count; i++)
                {
                    foreach (DataColumn _dcItem in _dtContacts.Columns)
                    {
                        // get off inverted p on column values 
                        _dtContacts.Rows[i][_dcItem.ColumnName] = _dtContacts.Rows[i][_dcItem.ColumnName].ToString().Replace("¶", "");

                        // we bypass non needed columns
                        //if (_dcItem.ColumnName.Equals("org_no") || _dcItem.ColumnName.Equals("action_type") || _dcItem.ColumnName.Equals("list_id") || _dcItem.ColumnName.Equals("account_idid"))
                        if (_dcItem.ColumnName.Equals("action_type") || _dcItem.ColumnName.Equals("list_id") || _dcItem.ColumnName.Equals("id"))
                            continue;

                        // we bypass "No Changes" rows
                        else if (_dtContacts.Rows[i]["action_type"].Equals("No Changes"))
                            continue;

                        // we bypass null/empty cell values
                        else if (string.IsNullOrEmpty(_dtContacts.Rows[i][_dcItem.ColumnName].ToString()))
                            continue;

                        // we bypass rows with no new values
                        // respectively, we dont update cell values that include separator "/" at index 0
                        else if (_dtContacts.Rows[i][_dcItem.ColumnName].ToString().IndexOf("[«]") < 1)
                            continue;

                        try
                        {
                            _sRowValues = null;
                            _sRowValues = _dtContacts.Rows[i][_dcItem.ColumnName].ToString().Split(new string[] { "[«]" }, StringSplitOptions.None);
                            if (_sRowValues.Length > 1)
                                _dtContacts.Rows[i][_dcItem.ColumnName] = _sRowValues[0].ToString();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.SetEditableControlsContact(true);
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                #endregion

                /**
                 * gather to be updated account records.
                 */
                #region Code Logic
                int _ContactId = 0;
                contact _efeContact= new contact();
                List<contact> _lstUpdatedContacts = new List<contact>();
                foreach (DataRow _drRow in _dtContacts.Select("action_type = 'For Update'"))
                {
                    _efeContact = null;
                    _ContactId = Convert.ToInt32(_drRow["id"]);
                    _efeContact = _efDbModel.contacts.FirstOrDefault(i => i.id == _ContactId);

                    if (_dtContacts.Columns.Contains("first_name"))
                        _efeContact.first_name = string.IsNullOrEmpty(_drRow["first_name"].ToString())? string.Empty : _drRow["first_name"].ToString();
                    if (_dtContacts.Columns.Contains("middle_name"))
                        _efeContact.middle_name = string.IsNullOrEmpty(_drRow["middle_name"].ToString()) ? string.Empty : _drRow["middle_name"].ToString();
                    if (_dtContacts.Columns.Contains("last_name"))
                        _efeContact.last_name = string.IsNullOrEmpty(_drRow["last_name"].ToString()) ? string.Empty : _drRow["last_name"].ToString();
                    if (_dtContacts.Columns.Contains("direct_phone"))
                        _efeContact.direct_phone = string.IsNullOrEmpty(_drRow["direct_phone"].ToString()) ? string.Empty : _drRow["direct_phone"].ToString();
                    if (_dtContacts.Columns.Contains("mobile"))
                        _efeContact.mobile = string.IsNullOrEmpty(_drRow["mobile"].ToString()) ? string.Empty : _drRow["mobile"].ToString();
                    if (_dtContacts.Columns.Contains("email"))
                        _efeContact.email = string.IsNullOrEmpty(_drRow["email"].ToString()) ? string.Empty : _drRow["email"].ToString();
                    if (_dtContacts.Columns.Contains("title_id"))
                        _efeContact.title_id = string.IsNullOrEmpty(_drRow["title_id"].ToString()) ? null : this.TryParseInt(_drRow["title_id"].ToString());
                    if (_dtContacts.Columns.Contains("title"))
                        _efeContact.title = string.IsNullOrEmpty(_drRow["title"].ToString()) ? string.Empty : _drRow["title"].ToString();
                    if (_dtContacts.Columns.Contains("role_tag_ids"))
                        _efeContact.role_tag_ids = string.IsNullOrEmpty(_drRow["role_tag_ids"].ToString()) ? null : _drRow["role_tag_ids"].ToString();
                    if (_dtContacts.Columns.Contains("address_1"))
                        _efeContact.address_1 = string.IsNullOrEmpty(_drRow["address_1"].ToString()) ? string.Empty : _drRow["address_1"].ToString();
                    if (_dtContacts.Columns.Contains("address_2"))
                        _efeContact.address_2 = string.IsNullOrEmpty(_drRow["address_2"].ToString()) ? string.Empty : _drRow["address_2"].ToString();
                    if (_dtContacts.Columns.Contains("city"))
                        _efeContact.city = string.IsNullOrEmpty(_drRow["city"].ToString()) ? string.Empty : _drRow["city"].ToString();
                    if (_dtContacts.Columns.Contains("zipcode"))
                        _efeContact.zipcode = string.IsNullOrEmpty(_drRow["zipcode"].ToString()) ? string.Empty : _drRow["zipcode"].ToString();
                    if (_dtContacts.Columns.Contains("country"))
                        _efeContact.country = string.IsNullOrEmpty(_drRow["country"].ToString()) ? string.Empty : _drRow["country"].ToString();
                    if (_dtContacts.Columns.Contains("remarks"))
                        _efeContact.remarks = string.IsNullOrEmpty(_drRow["remarks"].ToString()) ? string.Empty : _drRow["first_remarksname"].ToString();
                    if (_dtContacts.Columns.Contains("role"))
                        _efeContact.role = string.IsNullOrEmpty(_drRow["role"].ToString()) ? string.Empty : _drRow["role"].ToString();
                    if (_dtContacts.Columns.Contains("active"))
                        _efeContact.active = string.IsNullOrEmpty(_drRow["active"].ToString()) ? false : Convert.ToBoolean(this.TryParseByte(_drRow["active"].ToString()));

                    _efeContact.modified_by = UserSession.CurrentUser.UserId;
                    _efeContact.modified_date = DateTime.Now;
                    _efeContact.last_modified_machine = UserSession.CurrentUser.ComputerName;
                    _efeContact.last_modified_source = BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Master_Data_Import;

                    _lstUpdatedContacts.Add(_efeContact);
                }
                #endregion

                /**
                 * gather to be added account records.
                 */
                #region Code Logic
                DataTable _dtNewContacts = DataImportUtility.CreateContactTable();
                foreach (DataRow _drRow in _dtContacts.Select("action_type = 'New Record'"))
                {
                    DataRow _drNewContact = _dtContacts.NewRow();
                    foreach (DataColumn _dcContact in _dtContacts.Columns)
                    {
                        if (!_dtContacts.Columns.Contains(_dcContact.ColumnName))
                            continue;

                        if (_dcContact.DataType.FullName.Equals("System.String"))
                            _drNewContact[_dcContact.ColumnName] = _drRow[_dcContact.ColumnName].ToString();

                        else if (_dcContact.DataType.FullName.Equals("System.Decimal"))
                            _drNewContact[_dcContact.ColumnName] = this.TryParseDecimal(_drRow[_dcContact.ColumnName].ToString());

                        else if (_dcContact.DataType.FullName.Equals("System.Int32"))
                            _drNewContact[_dcContact.ColumnName] = this.TryParseInt(_drRow[_dcContact.ColumnName].ToString());

                        else if (_dcContact.DataType.FullName.Equals("System.Byte"))
                            _drNewContact[_dcContact.ColumnName] = this.TryParseByte(_drRow[_dcContact.ColumnName].ToString());
                    }

                    _drNewContact["created_by"] = UserSession.CurrentUser.UserId;
                    _drNewContact["created_date"] = DateTime.Now;
                    _drNewContact["modified_by"] = UserSession.CurrentUser.UserId;
                    _drNewContact["modified_date"] = DateTime.Now;
                    _drNewContact["last_modified_machine"] = UserSession.CurrentUser.ComputerName;
                    _drNewContact["last_modified_source"] = BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Master_Data_Import;

                    if (_drNewContact != null)
                        _dtNewContacts.Rows.Add(_drNewContact);
                }
                #endregion

                /**
                 * start the insert/update transaction.
                 * updates will be first processed over new data.
                 */
                #region Code Logic
                SqlConnection _sqlConnection = new SqlConnection(UserSession.ProviderConnection);
                _sqlConnection.Open();
                SqlTransaction _sqlTransaction = _sqlConnection.BeginTransaction();
                using (TransactionScope _efDbTransaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() {
                    Timeout = TimeSpan.FromMinutes(120),
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                })) {
                    try {
                        /**
                         * start updating records.
                         * transaction will not be saved until _efDbModel.AcceptAllChanges() is called.
                         */
                        if (_lstUpdatedContacts.Count > 0) {
                            contact _item = null;
                            int _id = 0;
                            for (int x = 0; x < _lstUpdatedContacts.Count; x++) {
                                _id = _lstUpdatedContacts[x].id;
                                _item = _efDbModel.contacts.FirstOrDefault(i => i.id == _id);
                                _efDbModel.contacts.ApplyCurrentValues(_lstUpdatedContacts[x]);
                            }
                            _efDbModel.SaveChanges();
                        }
                        //ObjectContact.SaveContacts(_lstUpdatedContacts, _efDbModel);

                        /**
                         * process new records using bulk insert.
                         * commit and accept all changes to the database.
                         * to be updated records will be committed on _efDbModel.AcceptAllChanges() call.
                         */
                        if (_dtNewContacts.Rows.Count > 0)
                            DatabaseUtility.ExecuteBulkProcessing("vw_contacts", _dtNewContacts, _sqlConnection, _sqlTransaction);

                        /**
                         * commit only if has records to insert/update.
                         */
                        if (_dtNewContacts.Rows.Count > 0)
                            _sqlTransaction.Commit();

                        if (_lstUpdatedContacts.Count > 0) {
                            _efDbModel.AcceptAllChanges();
                            _efDbTransaction.Complete();
                        }

                        tbxImportFileContact.Text = "";
                        cboSheetNameContact.Properties.Items.Clear();
                        cboSheetNameContact.Text = "";
                        gcMatchedColumnContact.DataSource = null;
                        gcImportFileDataContact.DataSource = null;
                        lblRecordStatisticContact.Text = "          Number of Records: 0";
                        WaitDialog.Close();
                        MessageBox.Show("Successfully saved items to contacts master data.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        WaitDialog.Show("Initializing ...");
                    }
                    catch (Exception Ex) {
                        WaitDialog.Close();
                        MessageBox.Show(string.Format("Transaction rolled backed due to the ff:{0}{0}{1}", Environment.NewLine, Ex.Message), "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        WaitDialog.Show("Initializing ...");
                        _efDbTransaction.Dispose();
                        _sqlTransaction.Rollback();
                        _efDbModel = null;
                        _sqlConnection = null;
                        this.SetEditableControlsContact(true);
                        e.Cancel = true;
                        return;
                    }
                    finally {
                        _efDbTransaction.Dispose();
                        _efDbModel = null;
                        _sqlConnection = null;
                    }

                    WaitDialog.Close();
                }
                #endregion

                /**
                 * delete the temporary excel file created during the matching
                 */
                if (File.Exists(m_sFileNameContact))
                    File.Delete(m_sFileNameContact);

                this.SetEditableControlsContact(true);
                btnUpdateToGridContact.Enabled = false;
                btnUpdateToMasterDataTableContact.Enabled = false;
                WaitDialog.Close();

            }));

            e.Cancel = true;
        }
        private void gvImportFileDataContact_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName.Equals("action_type"))
                return;

            if (gvImportFileDataContact.GetFocusedRowCellDisplayText("action_type").Equals("New Record") || gvImportFileDataContact.GetFocusedRowCellDisplayText("id").Length < 1)
                return;

            gvImportFileDataContact.SetRowCellValue(e.RowHandle, "action_type", "For Update");
        }
        private void gvImportFileDataContact_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName.Equals("list_id") || e.Column.FieldName.Equals("id"))
                return;

            if (e.CellValue == null)
                return;

            if (e.CellValue.ToString().Contains("[«]"))
                e.Appearance.BackColor = Color.GreenYellow;

            else if ((e.CellValue.ToString().Contains("¶")))
                e.Appearance.BackColor = Color.GreenYellow;
        }
        private void gvImportFileDataContact_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Value == null)
                return;

            if ((e.Value.ToString().Contains("¶")))
                e.DisplayText = e.Value.ToString().Replace("¶", "");
        }
        private void gvImportFileDataContact_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        private void gvImportFileDataContact_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView _gvData = sender as GridView;
            string _cellValue = _gvData.GetRowCellValue(gvImportFileDataContact.FocusedRowHandle, "action_type").ToString();
            if (_cellValue.Equals("New Record") && gvImportFileDataContact.FocusedColumn.ToString().Equals("action_type"))
                e.Cancel = true;
        }
        #endregion
        #endregion
    }
}
