using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Model;

namespace ManagerApplication.Modules {
    public partial class AutoTitleCorrection : DevExpress.XtraEditors.XtraUserControl {

        #region Member Variables
        private BrightPlatformEntities BPContext = null;
        private OleDbConnection oleDbCon = null;
        private OleDbDataAdapter oleDbAdapter = null;

        private DataTable dtMatches = null;
        private DataTable udtTitles = null;

        private string importFileName = "";
        private string importPath = "";
        private string destFilePath = "";
        private string newFileName = "";
        private string orgFileName = "";
        
        #endregion
        
        #region Constructor
        public AutoTitleCorrection() {
            InitializeComponent();
            btnCommit.Enabled = false;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);

        } 
        #endregion

        #region Private Methods
        private void PopulateGrid() {
            btnCommit.Enabled = false;
            btnMatchImportFile.Enabled = false;
            txtFilename.Enabled = false;
            gcImport.DataSource = null;
            
            WaitDialog.Show(ParentForm, "Processing import file...");
            try {
                importFileName = Path.GetFileName(txtFilename.Text);
                importPath = txtFilename.Text.Replace(importFileName, "");
                File.Copy(importPath + importFileName, importPath + "~" + importFileName, true);
                orgFileName = "~" + importFileName;
                newFileName = Guid.NewGuid().ToString().Substring(0, 10)
                    + Path.GetExtension(orgFileName.Replace(".csv", ".txt"));
                destFilePath = importPath + newFileName;

                File.Move(importPath + orgFileName, destFilePath);
                oleDbCon = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0;Data Source="
                + importPath + ";Extended Properties='text;HDR=Yes;FMT=Delimited';");
                oleDbAdapter = new OleDbDataAdapter("SELECT * FROM [" + Path.GetFileName(newFileName) + "]", oleDbCon);
                var oDatasetTxt = new DataSet("text");
                oleDbAdapter.Fill(oDatasetTxt);
                var oTableTxt = oDatasetTxt.Tables[0];
                oleDbAdapter.Dispose();
                DataColumn dtExp = new DataColumn("title_id", typeof(int));
                dtExp.Expression = "Convert(" + oTableTxt.Columns[1].ColumnName + ",'System.Int32')";
                oTableTxt.Columns.Add(dtExp);

                udtTitles = oTableTxt.AsDataView().ToTable(true, oTableTxt.Columns[0].ColumnName, "title_id");
                SqlCommand objCommand = new SqlCommand("bvGetTitleMatches_sp");
                objCommand.Parameters.Add("tableToMatch", SqlDbType.Structured).Value = udtTitles;
                dtMatches = DatabaseUtility.ExecuteSqlQuery(objCommand);
                gcImport.DataSource = dtMatches;

                if (dtMatches != null && dtMatches.Rows.Count > 0) {
                    lblTotalRecords.Text = "Total Records: " + dtMatches.Rows.Count.ToString();
                    btnCommit.Enabled = true;
                    var cols = gvImport.Columns;
                    foreach (DevExpress.XtraGrid.Columns.GridColumn col in cols) {
                        col.OptionsColumn.AllowEdit = false;
                        col.OptionsColumn.AllowFocus = false;
                        col.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
                    }
                } else {
                    lblTotalRecords.Text = "Total Records: 0";
                }
                WaitDialog.Close();
            } catch {
                WaitDialog.Close();
                MessageBox.Show("An error occured while reading the import file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);                
            }

            //remove the copied new file
            File.Delete(destFilePath);
            btnMatchImportFile.Enabled = true;
            txtFilename.Enabled = true;
            btnMatchImportFile.Enabled = true;
            txtFilename.Enabled = true;                
        }
               
        #endregion

        #region Controllers

        private void txtFilename_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e) {
            OpenFileDialog objOpenFile = new OpenFileDialog();
            objOpenFile.Title = "Select an import csv file";
            objOpenFile.InitialDirectory = @"c:\";
            objOpenFile.Filter = "CSV (Comma Delimited) (*.csv)|*.csv";
            objOpenFile.FilterIndex = 2;
            objOpenFile.RestoreDirectory = true;

            if (objOpenFile.ShowDialog() == DialogResult.OK) {
                txtFilename.Text = objOpenFile.FileName;
            }
        }

        private void btnMatchImportFile_Click(object sender, EventArgs e) {
            gcImport.DataSource = null;
            if (File.Exists(txtFilename.Text)) {
                if (Path.GetExtension(txtFilename.Text).ToLower() == ".csv") {
                    PopulateGrid();
                    return;
                } else {
                    MessageBox.Show("Import file is not a valid CSV. Please select a valid CSV file.",
                        "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            } else if (!string.IsNullOrEmpty(txtFilename.Text)) {
                MessageBox.Show("Import file does not exists. Please make sure that the import file existed.",
                    "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            } else if(string.IsNullOrEmpty(txtFilename.Text)) {
                MessageBox.Show("Please select or enter import file csv first.",
                    "System Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnCommit_Click(object sender, EventArgs e) {
            if (udtTitles == null) return;
            if (MessageBox.Show("Are you sure you want to commit these title changes to contacts db table?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            
            WaitDialog.Show(ParentForm, "Updating...");
            btnCommit.Enabled = false;
            btnMatchImportFile.Enabled = false;
            txtFilename.Enabled = false;
            try {
                SqlParameter sqlParam = new SqlParameter("tableToMatch", SqlDbType.Structured);
                sqlParam.Value = udtTitles;
                DatabaseUtility.ExecuteNonQuery("bvUpdateTitleMatches_sp", sqlParam);
                WaitDialog.Close();
                MessageBox.Show("Data changes has been committed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch {
                WaitDialog.Close();
            }
            btnCommit.Enabled = true;
            btnMatchImportFile.Enabled = true;
            txtFilename.Enabled = true;
        }

        private void gvImport_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        #endregion
                
    }
}
