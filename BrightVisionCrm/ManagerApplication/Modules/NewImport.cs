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
using System.Data.SqlClient;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using ManagerApplication.Business;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using DevExpress.XtraEditors.Controls;

namespace ManagerApplication.Modules
{
    public partial class NewImport : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Members
        public ManageImportList m_objUserControl = null; //object to handle access on the user parent form controls and functions
        public ImportWorker objImportWorker = null;
        public Thread objWorkThread = null;
        #endregion

        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private ObjectFileHandler m_objFileHandler = null;
        private DataTable m_objFileDataTable = null;
        private List<DataImportUtility.ImportFileHeaderIdInstance> m_objFileHeaderIdList = null;
        private string m_MessageBoxCaption = "Manager Application - Import List";
        private DataTable m_objImportFileHeaderTable = null;
        private DataTable m_objImportFileDetailTable = null;
        private DataRow m_objTableRow = null;
        private SqlConnection m_objConnection = null;
        private SqlTransaction m_objTransaction = null;
        private int m_ImportFileId = 0;
        private string m_FileName = string.Empty;
        #endregion

        #region Contructors
        public NewImport()
        {
            InitializeComponent();
        }
        #endregion

        #region Object Control Events
        private void cmdDone_Click(object sender, EventArgs e)
        {
            this.ParentForm.Close();
        }

        private void cmdBrowseFile_Click(object sender, EventArgs e)
        {
            this.GetImportFile();
            this.GetImportFileSheetNames();
        }

        private void cmdImportNow_Click(object sender, EventArgs e)
        {
            if (cboCustomer.EditValue == null || cboCampaign.EditValue == null || cboCustomer.Text.Length < 1 || cboCampaign.Text.Length < 1)
                return;

            imported_files _item;
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                int _CustId = Convert.ToInt32(cboCustomer.EditValue);
                int _CampaignId = Convert.ToInt32(cboCampaign.EditValue);
                _item = _efDbModel.imported_files.FirstOrDefault(
                    i => i.list_name == txtImportListName.Text
                    && i.customer_id == _CustId
                    && i.campaign_id == _CampaignId);
            }
            
            if (_item != null)
            {
                MessageBox.Show("List name already exist.", "Import File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(m_FileName))
            {
                MessageBox.Show("Import file not found.", "Import File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this.ValidateImportFileRequiredFields())
            {
                cmdCancel.Enabled = true;
                this.StartImportProcess();
                cmdCancel.Enabled = false;
            }
        }

        private void NewImport_Load(object sender, EventArgs e)
        {
            this.PopulateCustomers();
            this.PopulateCountries();
            cmdCancel.Enabled = false;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            objImportWorker.CancelImportProcess();
        }

        private void cboCustomer_EditValueChanged(object sender, EventArgs e)
        {
            if (cboCustomer.EditValue == null || Convert.ToInt32(cboCustomer.EditValue) < 1)
                return;

            this.Cursor = Cursors.WaitCursor;
            this.PopulateCampaigns();
            this.Cursor = Cursors.Default;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Open the import file and read contents
        /// </summary>
        public void GetFileContents()
        {
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate { this.WriteLog("Opening file: " + m_FileName); }));
                this.Invoke(new MethodInvoker(delegate { this.WriteLog("Reading and getting file contents ..."); }));

                m_objFileDataTable = null;
                m_objFileDataTable = new DataTable();
                m_objFileHandler = new ObjectFileHandler();

                if (m_objFileHandler.OpenExcelFile(m_FileName))
                {
                    this.Invoke(new MethodInvoker(delegate { m_objFileDataTable = m_objFileHandler.GetFileContents(cboSheetName.Text); }));
                    this.Invoke(new MethodInvoker(delegate { m_objFileHandler.CloseExcelFile(); }));
                }
                else
                    MessageBox.Show("Not a valid excel file", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                this.Invoke(new MethodInvoker(delegate { this.WriteLog("Done ..."); }));
                this.Invoke(new MethodInvoker(delegate { this.WriteLog("Getting total number of records on file ..."); }));
                this.Invoke(new MethodInvoker(delegate { this.WriteLog("Records found: " + m_objFileDataTable.Rows.Count.ToString()); }));
                this.Invoke(new MethodInvoker(delegate { this.WriteLog("Done ..."); }));
            }
        }

        /// <summary>
        /// Traverse excel data table and save data
        /// </summary>
        public void SaveExcelData()
        {
            if (InvokeRequired)
            {
                // do not proceed if has not importable data
                if (m_objFileDataTable.Rows.Count < 1)
                {
                    MessageBox.Show("No records to import", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int TotalRecordCount = m_objFileDataTable.Rows.Count;
                int RowIndexer = 0;
                int ColumnIndexer = 0;
                string LogRecorder = string.Empty;

                prbImport.Minimum = 0;
                this.Invoke(new MethodInvoker(delegate { prbImport.Maximum = 8; })); // represents the blocks traversed, not the actual # of records
                this.Invoke(new MethodInvoker(delegate { prbImport.Value = 0; }));
                
                // begin transaction
                //m_objConnection = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString());
                m_objConnection = new SqlConnection(UserSession.ProviderConnection);
                m_objConnection.Open();

                this.Invoke(new MethodInvoker(delegate { this.WriteLog("Opening and beginning the import transaction ..."); }));
                m_objTransaction = m_objConnection.BeginTransaction();
                this.Invoke(new MethodInvoker(delegate { this.WriteLog("Done ..."); }));
                this.Invoke(new MethodInvoker(delegate { prbImport.Value = 1; }));
                
                try
                {
                    // import file
                    this.Invoke(new MethodInvoker(delegate { prbImport.Value = 2; }));
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Inserting import file information. Please kindly wait ..."); }));
                    this.Invoke(new MethodInvoker(delegate {
                        m_ImportFileId = DataImportUtility.SaveImportFile(txtImportFile.Text, txtImportListName.Text, (int)cboCustomer.EditValue, (int)cboCampaign.EditValue, (int)cboCountry.EditValue, m_objConnection, m_objTransaction);
                    }));
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Done ..."); }));
                    this.Invoke(new MethodInvoker(delegate { prbImport.Value = 3; }));
                                        
                    // import file headers
                    m_objFileHeaderIdList = null;
                    m_objImportFileHeaderTable = null;
                    m_objImportFileHeaderTable = DataImportUtility.CreateImportFileHeaderTable();
                    m_objFileHeaderIdList = new List<DataImportUtility.ImportFileHeaderIdInstance>();

                    ColumnIndexer = 1;
                    LogRecorder = string.Empty;
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Queueing import file headers. Please kindly wait ..."); }));
                    
                    foreach (DataColumn Item in m_objFileDataTable.Columns)
                    {
                        m_objTableRow = null;
                        m_objTableRow = m_objImportFileHeaderTable.NewRow();
                        m_objTableRow["imported_file_id"] = m_ImportFileId;
                        m_objTableRow["column_order"] = ColumnIndexer;
                        m_objTableRow["column_name"] = Item.ColumnName;
                        m_objImportFileHeaderTable.Rows.Add(m_objTableRow);

                        ColumnIndexer ++;
                        LogRecorder = LogRecorder + "Queueing column header: " + Item.ColumnName + Environment.NewLine;
                    }

                    this.Invoke(new MethodInvoker(delegate { this.WriteLog(LogRecorder); }));
                    this.Invoke(new MethodInvoker(delegate { prbImport.Value = 4; }));
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Batch inserting import file column headers. Please kindly wait ..."); }));
                    m_objFileHeaderIdList = DataImportUtility.SaveImportFileHeaders(m_ImportFileId, m_objImportFileHeaderTable, m_objConnection, m_objTransaction);
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Done ..."); }));
                    this.Invoke(new MethodInvoker(delegate { prbImport.Value = 5; }));
                    
                    // import file details
                    RowIndexer = 1;
                    m_objImportFileDetailTable = null;
                    m_objImportFileDetailTable = DataImportUtility.CreateImportFileDetailTable();
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Queueing import file records. Please kindly wait ..."); }));
                    
                    //DateTime start = DateTime.Now;
                    foreach (DataRow Item in m_objFileDataTable.Rows)
                    {
                        if (Item == null)
                            continue;

                        ColumnIndexer = 1;
                        string ItemDetail = string.Empty;
                        LogRecorder = string.Empty;

                        for (int i = 0; i < Item.ItemArray.Count(); i++)
                        {
                            ItemDetail = string.Empty;
                            ItemDetail = Item.ItemArray[i].ToString();

                            if (ItemDetail.Length < 1 || ItemDetail == null)
                                ItemDetail = "";

                            //{
                            //    //ColumnIndexer++;
                            //    //continue;
                            //}

                            m_objTableRow = null;
                            m_objTableRow = m_objImportFileDetailTable.NewRow();
                            m_objTableRow["imported_file_header_id"] = (int)m_objFileHeaderIdList[ColumnIndexer - 1].id;
                            m_objTableRow["row_order"] = (int)RowIndexer;
                            m_objTableRow["column_value"] = ItemDetail;
                            m_objImportFileDetailTable.Rows.Add(m_objTableRow);

                            ColumnIndexer ++;
                        }

                        LogRecorder = "Queueing record " + RowIndexer.ToString() + " of " + TotalRecordCount.ToString();
                        this.Invoke(new MethodInvoker(delegate { this.WriteLog(LogRecorder); }));
                        RowIndexer++;
                    }
                    //DateTime endtime = DateTime.Now;
                    //TimeSpan elapsedtime = endtime - start;
                    //MessageBox.Show("total time: " + elapsedtime.ToString());
                    
                    //this.Invoke(new MethodInvoker(delegate { this.WriteLog(LogRecorder); }));
                    this.Invoke(new MethodInvoker(delegate { prbImport.Value = 6; }));
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Batch inserting import file row items. Please kindly wait ..."); }));
                    DataImportUtility.ExecuteBulkProcessing("vw_import_file_details", m_objImportFileDetailTable, m_objConnection, m_objTransaction);
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Done ..."); }));
                    this.Invoke(new MethodInvoker(delegate { prbImport.Value = 7; }));
                    
                    // commit transaction
                    this.Invoke(new MethodInvoker(delegate { cmdCancel.Enabled = false; }));
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Committing transaction ..."); }));
                    this.Invoke(new MethodInvoker(delegate {
                        if (File.Exists(m_FileName))
                            File.Delete(m_FileName); 
                    }));
                    m_objTransaction.Commit();
                    this.Invoke(new MethodInvoker(delegate { prbImport.Value = 8; }));
                    MessageBox.Show("Importing successful!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Done ..."); }));
                    this.Invoke(new MethodInvoker(delegate { this.ReloadImportListView(); }));
                }
                catch (Exception ex)
                {
                    // rollback transaction
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Rolling back transaction ..."); }));
                    m_objTransaction.Rollback();
                    MessageBox.Show("Transaction rolled back due to the following: " + ex.Message, m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Exception message:\r\n" + ex.Message); })); 
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Done ..."); }));
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Transaction cancelled ..."); }));
                }
                finally
                {
                    this.Invoke(new MethodInvoker(delegate { this.WriteLog("Disposing transaction object and closing connection ..."); }));

                    m_objTransaction.Dispose();
                    m_objTransaction = null;
                    m_objConnection.Close();
                    m_objConnection.Dispose();
                    m_objConnection = null;

                    this.Invoke(new MethodInvoker(delegate { this.CreateImportLogFile(); }));                    
                    this.Invoke(new MethodInvoker(delegate { this.ParentForm.Close(); }));
                }
            }
        }

        /// <summary>
        /// Save to text file the import logs
        /// </summary>
        public void CreateImportLogFile()
        {
            try
            {
                string FileName = "import_log_" + m_ImportFileId.ToString() + "[" + System.DateTime.Now.ToString().Trim() + "]";
                FileName = FileName.Replace("/", ".");
                FileName = FileName.Replace(":", ".");
                FileName = FileName.Replace(" ", "-");
                FileName = Application.StartupPath + @"\import_logs\" + FileName + ".txt";

                if (!System.IO.Directory.Exists(Application.StartupPath + @"\import_logs"))
                {
                    System.IO.Directory.CreateDirectory(Application.StartupPath + @"\import_logs");
                }

                using (StreamWriter objTextFile = new StreamWriter(FileName))
                    objTextFile.Write(txtImportLogs.Text);

                MessageBox.Show("Import log file created to " + FileName, m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Re-populate import list view grid on parent form
        /// </summary>
        public void ReloadImportListView()
        {
            this.Invoke(new MethodInvoker(delegate { m_objUserControl.PopulateImportListView(m_ImportFileId); }));
            //this.Invoke(new MethodInvoker(delegate { m_objUserControl.PopulateImportProfilingView(); }));
        }

        /// <summary>
        /// Re-populate import list view grid on parent form
        /// </summary>
        public void SetFormControlObjectAcess(bool IsEnabled)
        {
            this.Invoke(new MethodInvoker(delegate { txtImportListName.Enabled = IsEnabled; }));
            this.Invoke(new MethodInvoker(delegate { cboCustomer.Enabled = IsEnabled; }));
            this.Invoke(new MethodInvoker(delegate { txtImportFile.Enabled = IsEnabled; }));
            this.Invoke(new MethodInvoker(delegate { cmdBrowseFile.Enabled = IsEnabled; }));
            this.Invoke(new MethodInvoker(delegate { cboSheetName.Enabled = IsEnabled; }));
            this.Invoke(new MethodInvoker(delegate { cmdImportNow.Enabled = IsEnabled; }));
            this.Invoke(new MethodInvoker(delegate { cmdDone.Enabled = IsEnabled; }));
            this.Invoke(new MethodInvoker(delegate { cboCampaign.Enabled = IsEnabled; }));
            this.Invoke(new MethodInvoker(delegate { cboCountry.Enabled = IsEnabled; }));

            if (IsEnabled)
                this.Invoke(new MethodInvoker(delegate { cmdCancel.Enabled = false; }));
            else
                this.Invoke(new MethodInvoker(delegate { cmdCancel.Enabled = true; }));

            //this.Invoke(new MethodInvoker(delegate { cmdCancel.Enabled = IsEnabled == true? false: true; }));
        }

        /// <summary>
        /// Empty the log text box
        /// </summary>
        public void EmptyLogText()
        {
            this.Invoke(new MethodInvoker(delegate { txtImportLogs.Text = ""; }));
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Thread the import process and start importing
        /// </summary>
        private void StartImportProcess()
        {
            objImportWorker = new ImportWorker(this);
            objWorkThread = new Thread(objImportWorker.ProcessImporting);
            objWorkThread.Start();
        }

        /// <summary>
        /// Validate required fields
        /// </summary>
        private bool ValidateImportFileRequiredFields()
        {
            if (txtImportListName.Text.Length < 1)
            {
                MessageBox.Show("invalid list name", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else if (txtImportFile.Text.Length < 1)
            {
                MessageBox.Show("invalid import file name", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else if (cboSheetName.Text.Length < 1)
            {
                MessageBox.Show("invalid import file sheet name", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else if (Convert.ToInt32(cboCountry.EditValue) < 1 || cboCountry.Text.Length < 1)
            {
                MessageBox.Show("invalid country", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write log to log import text box
        /// </summary>
        private void WriteLog(string LogMessage)
        {
            txtImportLogs.Text = txtImportLogs.Text + Environment.NewLine + LogMessage;
            txtImportLogs.Select(txtImportLogs.Text.Length, 1);
            txtImportLogs.ScrollToCaret();
            //Thread.Sleep(100);
        }

        /// <summary>
        /// Populates the customer combo box on the new import form
        /// </summary>
        private void PopulateCustomers()
        {
            cboCustomer.Properties.DataSource = null;
            cboCustomer.Properties.DataSource = ObjectCustomer.GetCustomers(ObjectCustomer.eViewType.ComboListView).Execute(MergeOption.AppendOnly);
            cboCustomer.Properties.DisplayMember = "customer_name";
            cboCustomer.Properties.ValueMember = "id";
            cboCustomer.Properties.Columns.Add(new LookUpColumnInfo("customer_name"));
            cboCustomer.ItemIndex = 0;
        }

        /// <summary>
        /// Populates the campaign combo box on the new import form
        /// </summary>
        private void PopulateCampaigns()
        {
            cboCampaign.Properties.DataSource = null;
            cboCampaign.Properties.Columns.Clear();
            if (Convert.ToInt32(cboCustomer.EditValue) < 1)
                return;

            cboCampaign.Properties.DataSource = ObjectCampaign.GetCustomerContactCampaigns((int)cboCustomer.EditValue).Execute(MergeOption.AppendOnly);
            cboCampaign.Properties.DisplayMember = "name";
            cboCampaign.Properties.ValueMember = "id";
            cboCampaign.Properties.Columns.Add(new LookUpColumnInfo("name"));
            cboCampaign.ItemIndex = 0;
        }

        /// <summary>
        /// Populates the country combo list
        /// </summary>
        private void PopulateCountries()
        {
            cboCountry.Properties.DataSource = null;
            cboCountry.Properties.DataSource = ObjectCountry.GetCountries().Execute(MergeOption.AppendOnly);
            cboCountry.Properties.DisplayMember = "name";
            cboCountry.Properties.ValueMember = "id";
            cboCountry.Properties.Columns.Add(new LookUpColumnInfo("name"));
            //cboCountry.ItemIndex = 0;
            int idx = cboCountry.Properties.GetDataSourceRowIndex("name", "Sweden");
            cboCountry.EditValue = cboCountry.Properties.GetDataSourceValue("id", idx);
        }

        /// <summary>
        /// Get import file sheet names
        /// </summary>
        private void GetImportFileSheetNames()
        {
            WaitDialog.Show("Reading file information ...");
            List<string> ExcelSheetNames = new List<string>();
            m_objFileHandler = new ObjectFileHandler();
            
            // create a temporary copy of the import file
            try {
                // delete existing temporary file
                if (File.Exists(m_FileName))
                    File.Delete(m_FileName);

                string _TempFileName = Path.GetFileName(txtImportFile.Text);
                string _TempFilePath = txtImportFile.Text.Replace(_TempFileName, "");  

                m_FileName = string.Empty;
                m_FileName = String.Format("{0}tmp_{1}", _TempFilePath, _TempFileName);
                if (File.Exists(m_FileName))
                    File.Delete(m_FileName);

                File.Copy(txtImportFile.Text, m_FileName);
            }
            catch (Exception Ex) {
                m_FileName = string.Empty;
                MessageBox.Show("Was not able to create / delete the temporary file. Please check your path access rights to that file / folder.", "Import File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                WaitDialog.Close();
                return;
            }

            if (m_objFileHandler.OpenExcelFile(m_FileName)) {
                ExcelSheetNames = m_objFileHandler.GetImportFileSheetNames(m_FileName);
                m_objFileHandler.CloseExcelFile();
            }
            else {
                MessageBox.Show("Not a valid excel file", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                WaitDialog.Close();
                return;
            }

            cboSheetName.Items.Clear();
            foreach (string SheetName in ExcelSheetNames)
                cboSheetName.Items.Add(SheetName);

            WaitDialog.Close();
        }

        /// <summary>
        /// Browse for the import file
        /// </summary>
        private void GetImportFile()
        {
            OpenFileDialog objOpenFile = new OpenFileDialog();
            objOpenFile.Title = "Select an import excel file";
            objOpenFile.InitialDirectory = @"c:\";
            //objOpenFile.Filter = "Excel files (*.xls)|*.xls|Excel files (*.xlsx)|*.xlsx";
            objOpenFile.Filter = "Excel files (*.xls;*.xlsx)|*.xls;*.xlsx";
            objOpenFile.FilterIndex = 2;
            objOpenFile.RestoreDirectory = true;

            if (objOpenFile.ShowDialog() == DialogResult.OK)
                txtImportFile.Text = objOpenFile.FileName;
        }
        #endregion
    }

    #region Import Thread Handler
    /// <summary>
    /// Import work threader
    /// </summary>
    public class ImportWorker
    {
        private NewImport m_objNewImportClass = null;

        public ImportWorker(NewImport objNewImport)
        {
            m_objNewImportClass = objNewImport;
        }

        public void ProcessImporting()
        {
            m_objNewImportClass.SetFormControlObjectAcess(false);
            m_objNewImportClass.EmptyLogText();
            m_objNewImportClass.GetFileContents();
            m_objNewImportClass.SaveExcelData();
            m_objNewImportClass.SetFormControlObjectAcess(true);
        }

        public void CancelImportProcess()
        {
            m_objNewImportClass.objWorkThread.Abort();
        }
    }
    #endregion
}
