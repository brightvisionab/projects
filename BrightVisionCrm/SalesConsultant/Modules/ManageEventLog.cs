
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

namespace SalesConsultant.Modules
{
    public partial class ManageEventLog : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public ManageEventLog()
        {
            this.Visible = false;
            InitializeComponent();
            this.Visible = true;
        }
        #endregion

        #region Private Properties
        private BackgroundWorker m_bwLoadEventLogs = null;
        #endregion

        #region Private Methods
        private void LoadEventLogs()
        {
            if (Convert.ToInt32(tbxRecordsToShow.Text) < 1) {
                MessageBox.Show("Please set a valid no of records to show.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            m_bwLoadEventLogs = new BackgroundWorker();
            m_bwLoadEventLogs.WorkerSupportsCancellation = true;
            m_bwLoadEventLogs.DoWork += new DoWorkEventHandler(m_bwLoadEventLogs_DoWork);
            m_bwLoadEventLogs.RunWorkerAsync();
        }
        #endregion

        #region Object Events
        private void ManageEventLog_Load(object sender, EventArgs e)
        {
            this.LoadEventLogs();
        }
        private void cmdRefreshData_Click(object sender, EventArgs e)
        {
            this.LoadEventLogs();
        }
        private void m_bwLoadEventLogs_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate {                
                try {
                    WaitDialog.Show(ParentForm, "Loading event logs...");
                    cmdRefreshData.Enabled = false;
                    SqlCommand _scCommand = new SqlCommand("bvGetEventLogs_sp");
                    _scCommand.Parameters.Add("@p_top_rows", SqlDbType.Int).Value = Convert.ToInt32(tbxRecordsToShow.Text);
                    DataTable _dtEventLogs = DatabaseUtility.ExecuteSqlQuery(_scCommand);
                    gcEventLog.DataSource = null;
                    if (_dtEventLogs.Rows.Count > 0) {
                        gcEventLog.DataSource = _dtEventLogs;
                        gvEventLog.BestFitColumns();
                        WaitDialog.Close();
                    }
                    cmdRefreshData.Enabled = true;
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
