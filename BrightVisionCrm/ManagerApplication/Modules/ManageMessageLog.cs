
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Data.SqlClient;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;

namespace ManagerApplication.Modules
{
    public partial class ManageMessageLog : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public ManageMessageLog()
        {
            this.Visible = false;
            InitializeComponent();
            this.Visible = true;
        }
        #endregion

        #region Private Properties
        private BackgroundWorker m_bwLoadMessageLog = null;
        #endregion

        #region Private Methods
        private void LoadMessageLog()
        {
            if (Convert.ToInt32(tbxRecordsToShow.Text) < 1) {
                MessageBox.Show("Please set a valid no of records to show.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            m_bwLoadMessageLog = new BackgroundWorker();
            m_bwLoadMessageLog.WorkerSupportsCancellation = true;
            m_bwLoadMessageLog.DoWork += new DoWorkEventHandler(m_bwLoadMessageLog_DoWork);
            m_bwLoadMessageLog.RunWorkerAsync();
        }
        #endregion

        #region Object Events
        private void ManageMessageLog_Load(object sender, EventArgs e)
        {
            this.LoadMessageLog();
        }
        private void cmdRefreshData_Click(object sender, EventArgs e)
        {
            this.LoadMessageLog();
        }
        private void m_bwLoadMessageLog_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate {                
                try {
                    WaitDialog.Show(ParentForm, "Loading event logs...");
                    cmdRefreshData.Enabled = false;
                    SqlCommand _scCommand = new SqlCommand("bvGetMessageLogs_sp");
                    _scCommand.Parameters.Add("@p_top_rows", SqlDbType.Int).Value = Convert.ToInt32(tbxRecordsToShow.Text);
                    DataTable _dtMessageLog = DatabaseUtility.ExecuteSqlQuery(_scCommand);
                    gcMessageLog.DataSource = null;
                    if (_dtMessageLog.Rows.Count > 0) {
                        gcMessageLog.DataSource = _dtMessageLog;
                        gvMessageLog.BestFitColumns();
                        WaitDialog.Close();
                    }
                    cmdRefreshData.Enabled = true;

                    gvMessageLog.Columns["Id"].Visible = false;

                    WaitDialog.Close();
                }
                catch {
                    WaitDialog.Close();
                    cmdRefreshData.Enabled = true;
                    e.Cancel = true;
                }
            }));

            e.Cancel = true;
        }
        #endregion
    }
}
