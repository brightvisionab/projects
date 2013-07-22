
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
using SalesConsultant.Facade;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Modules
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
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
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
                try
                {
                    int SubCampaignId = m_BrightSalesProperty.CommonProperty.SubCampaignId;
                    if (SubCampaignId <= 0) return;

                    WaitDialog.Show(ParentForm, "Loading message logs...");

                    cmdRefreshData.Enabled = false;
                    SqlCommand _scCommand = new SqlCommand("bvGetMessageLogs_sp");
                    _scCommand.Parameters.Add("@p_top_rows", SqlDbType.Int).Value = Convert.ToInt32(tbxRecordsToShow.Text);
                    _scCommand.Parameters.Add("@p_user_id", SqlDbType.Int).Value = UserSession.CurrentUser.UserId;
                    _scCommand.Parameters.Add("@p_show_only_user", SqlDbType.Bit).Value = cbxShowOnlyMyMessages.Checked;
                    _scCommand.Parameters.Add("@p_sub_campaign_id", SqlDbType.Int).Value = SubCampaignId;
                    DataTable _dtMessageLog = DatabaseUtility.ExecuteSqlQuery(_scCommand);
                    gcMessageLog.DataSource = null;
                    if (_dtMessageLog.Rows.Count > 0) {
                        gcMessageLog.DataSource = _dtMessageLog;
                        gvMessageLog.BestFitColumns();
                        WaitDialog.Close();
                    }
                    cmdRefreshData.Enabled = true;

                    gvMessageLog.Columns["Id"].Visible = false;
                    gvMessageLog.Columns["Sent Time Stamp"].Visible = false;
                    gvMessageLog.Columns["Mail thred Id"].Visible = false;
                    gvMessageLog.Columns["Sub Campaign"].Visible = false;
                    

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
        private void cbxShowOnlyMyMessages_EditValueChanged(object sender, EventArgs e)
        {
            this.LoadMessageLog();
        }

        private void simpleButtonFind_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(this.ParentForm, "Loading....");
            string search = textEditSearch.Text;
            gvMessageLog.FindFilterText = textEditSearch.Text;
            WaitDialog.Close();
        }
        private void simpleButtonClear_Click(object sender, EventArgs e)
        {
            textEditSearch.Text = "";
            gvMessageLog.FindFilterText = textEditSearch.Text;
        }
        private void textEditSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            WaitDialog.Show(this.ParentForm, "Loading....");
            string search = textEditSearch.Text;
            gvMessageLog.FindFilterText = textEditSearch.Text;
            WaitDialog.Close();
        }
        #endregion
    }
}
