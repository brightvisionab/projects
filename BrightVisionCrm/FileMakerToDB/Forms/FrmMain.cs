using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using FileMakerToDB.Model;


namespace FileMakerToDB.Forms {
    public partial class FrmMain : Form {
        public FrmMain() {
            InitializeComponent();
        }
        private FolderBrowserDialog diag = null;        
        private OleDbConnection oleDbCon = null;
        private OleDbDataAdapter oleDbAdapter = null;
        private FileMakerMigratedEntities fileMakerMigratedEntities;
        private string strConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["FileMakerMigrated"].ToString();
        private void FrmMain_Load(object sender, EventArgs e) {
            fileMakerMigratedEntities = new FileMakerMigratedEntities();
            
        }

        private void btnSelectSource_Click(object sender, EventArgs e) {
            diag = new FolderBrowserDialog();
            diag.Description = "Select the source folder of the exported FileMaker.";            
            diag.ShowDialog();
            txtSourcePath.Text = diag.SelectedPath;
        }

        private void btnSelectDestination_Click(object sender, EventArgs e) {
            diag = new FolderBrowserDialog();
            diag.Description = "Select the destination folder of the csv processed files.";                   
            diag.ShowDialog();
            txtDestinationPath.Text = diag.SelectedPath;
        }


        private void btnCancel_Click(object sender, EventArgs e) {
            FrmMain.ActiveForm.Close();
        }

        private void btnProcess_Click(object sender, EventArgs e) {
            if (btnProcess.Text == "Finish") {
                FrmMain.ActiveForm.Close();
            } else {
                btnProcess.Enabled = false;
                backgroundWorker1.RunWorkerAsync(rtbStatus);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            this.Invoke((MethodInvoker)(
                delegate {
                    var ctls = this.Controls;
                    this.Cursor = Cursors.WaitCursor;
                    foreach (Control ctl in ctls)
                        ctl.Cursor = Cursors.WaitCursor;                                        
                }));
            string sourcePath = string.Empty;
            txtSourcePath.Invoke((MethodInvoker)(
                delegate {
                    sourcePath = txtSourcePath.Text;
                }));
            string destPath =string.Empty;
            txtDestinationPath.Invoke((MethodInvoker)(
                delegate {
                    destPath = txtDestinationPath.Text;
                }));

            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);
            FileInfo[] fileInfos = dirInfo.GetFiles("*.csv");
            FileInfo moveFile = null;
            oleDbCon = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0;Data Source="
                + sourcePath + ";Extended Properties='text;HDR=Yes;FMT=Delimited';");
            DataSet oDatasetCsv = null;
            DataTable oTableCsv = null;
            DataSet oDatasetTxt = null;
            DataTable oTableTxt = null;
            //DataImport oDataImport = null;
            DataRow oRow = null;

            DataTable oTableResult = null;
            DataRow oRowResult = null;

            int rowCount = 0, columnCount = 0;
            string[] columnNames = null;
            string columnName = string.Empty, orgColName = string.Empty; ;
            //int columnsIgnored = 0;
            //List<DataImport> listImport = new List<DataImport>();
            int prog= 0;
            int totalcount = 0;
            var rtbStat = e.Argument as RichTextBox;
            string message = string.Empty, oldFilename = string.Empty,
                newFilename = string.Empty,
                rootName = string.Empty;
            Color messageColor = Color.Black;
            var delegateMethod = (MethodInvoker)
                (delegate {
                    Color color = Color.Black;
                    rtbStat.SelectionStart = rtbStat.TextLength;
                    rtbStat.SelectionLength = 0;
                    if (messageColor != color)
                        rtbStat.SelectionColor = messageColor;
                    else
                        rtbStat.SelectionColor = color;
                    messageColor = Color.Black;
                    rtbStat.AppendText(message);
                    rtbStat.SelectionColor = rtbStat.ForeColor;

                    rtbStat.SelectionStart = rtbStat.TextLength;
                    rtbStat.ScrollToCaret();
                    rtbStat.AppendText(Environment.NewLine);                    
                    SetPercentage(prog, totalcount);
                });
            totalcount = fileInfos.Length;
            //try {
                message = "Preparing files from source...";
                rtbStat.Invoke(delegateMethod);
                //foreach (FileInfo fileInfo in fileInfos) {
                //    oldFilename = fileInfo.Name;
                //    newFilename = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(oldFilename);
                //    rootName = Path.GetDirectoryName(fileInfo.FullName) + "\\";
                //    //rename long filename
                //    File.Move(fileInfo.FullName, rootName + newFilename);
                //    oleDbAdapter = new OleDbDataAdapter("SELECT * FROM [" + Path.GetFileName(newFilename) + "]", oleDbCon);                    
                //    oDatasetCsv = new DataSet("csv");
                //    oleDbAdapter.Fill(oDatasetCsv);
                //    //revert back to orig filename
                //    File.Move(rootName + newFilename, rootName + oldFilename);

                //    oTableCsv = oDatasetCsv.Tables[0];
                //    oleDbAdapter.Dispose();
                //    //totalcount = totalcount + oTableCsv.Rows.Count;
                //}
            //} catch (Exception ex) {
            //    MessageBox.Show(ex.Message);
            //    btnProcess.Enabled = true;
            //}
            try {
                message = "Estimated total files to be parsed - (" + totalcount.ToString() + ")";
                rtbStat.Invoke(delegateMethod);
                //totalcount = totalcount + 2; //increment for bulk copy && moving folder to destination
                long filemakerid = 0;
                foreach (FileInfo fileInfo in fileInfos) {
                    try {
                        message = "Reading and parsing file " + fileInfo.Name + "...";
                        rtbStat.Invoke(delegateMethod);

                        oldFilename = fileInfo.Name;
                        newFilename = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(oldFilename);
                        rootName = Path.GetDirectoryName(fileInfo.FullName) + "\\";
                        //rename long filename
                        File.Move(fileInfo.FullName, rootName + newFilename);
                        oleDbAdapter = new OleDbDataAdapter("SELECT * FROM [" + Path.GetFileName(newFilename) + "]", oleDbCon);
                        oDatasetCsv = new DataSet("csv");
                        oleDbAdapter.Fill(oDatasetCsv);
                        //revert back to orig filename
                        File.Move(rootName + newFilename, rootName + oldFilename);

                        oTableCsv = oDatasetCsv.Tables[0];
                        oleDbAdapter.Dispose();

                        oldFilename = fileInfo.Name.Replace(".csv", ".txt");
                        newFilename = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(oldFilename);
                        //rename long filename
                        File.Move(rootName + oldFilename, rootName + newFilename);
                        oleDbAdapter = new OleDbDataAdapter("SELECT * FROM [" + Path.GetFileName(newFilename) + "]", oleDbCon);
                        oDatasetTxt = new DataSet("text");
                        oleDbAdapter.Fill(oDatasetTxt);
                        //revert back to orig filename
                        File.Move(rootName + newFilename, rootName + oldFilename);

                        oTableTxt = oDatasetTxt.Tables[0];
                        oleDbAdapter.Dispose();
                        foreach (DataRow dr in oTableTxt.Rows) {
                            if (oTableCsv.Columns.IndexOf(dr["Field"].ToString()) != -1) {
                                oTableCsv.Columns[dr["Field"].ToString()].ColumnName = dr["Field"].ToString() + ";" + dr["Title"].ToString();
                            }
                        }

                        rowCount = oTableCsv.Rows.Count;
                        columnCount = oTableCsv.Columns.Count;
                        oTableResult = MakeTable();
                        filemakerid = GetFileMakerId();
                        for (int x = 0; x < rowCount; ++x) {
                            for (int i = 0; i < columnCount; ++i) {
                                oRow = oTableCsv.Rows[x];
                                columnNames = oTableCsv.Columns[i].ColumnName.Split(";".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                                orgColName = columnNames[0];
                                if (columnNames.Length > 1) {
                                    columnName = columnNames[1];
                                } else {
                                    columnName = orgColName;
                                }
                                //oDataImport = new DataImport() {
                                //    path_name = Path.GetFileNameWithoutExtension(fileInfo.Name),
                                //    row = x + 1,
                                //    data_context = oRow[i].ToString(),
                                //    column_name = columnName,
                                //    original_column = orgColName
                                //};
                                if (!string.IsNullOrEmpty(oRow[i].ToString())) {
                                    oRowResult = oTableResult.NewRow();
                                    oRowResult["path_name"] = Path.GetFileNameWithoutExtension(fileInfo.Name);
                                    oRowResult["row"] = x + 1;
                                    oRowResult["filemaker_id"] = filemakerid;
                                    oRowResult["data_context"] = oRow[i].ToString();
                                    oRowResult["column_name"] = columnName;
                                    if (columnName == orgColName) {
                                        orgColName = null;
                                    }
                                    oRowResult["original_column"] = orgColName;
                                    oTableResult.Rows.Add(oRowResult);
                                    //listImport.Add(oDataImport);
                                    //fileMakerMigratedEntities.DataImports.AddObject(oDataImport);
                                    //fileMakerMigratedEntities.SaveChanges();
                                } //else {
                                 //   columnsIgnored++;
                                //}
                            }
                            //prog++;
                            //message = "Parsing row " + (x + 1).ToString() + " - " + " empty data fields ignored: " + columnsIgnored.ToString();
                            //rtbStat.Invoke(delegateMethod);
                            //columnsIgnored = 0;
                        }

                        message = "Execute bulk insert to table database server...";
                        rtbStat.Invoke(delegateMethod);
                        string orgFileInfo = fileInfo.FullName;
                        //do sqlbulk insert
                        //Set up the bulk copy object inside the transaction. 
                        using (SqlConnection destinationConnection =
                                   new SqlConnection(strConnectionString)) {
                            destinationConnection.Open();

                            using (SqlTransaction transaction =
                                       destinationConnection.BeginTransaction()) {
                                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(
                                           destinationConnection,
                                           SqlBulkCopyOptions.FireTriggers,
                                           transaction)) {
                                    bulkCopy.BatchSize = 10;
                                    bulkCopy.DestinationTableName = "dbo.vw_DataImports";

                                    // Write from the source to the destination.                       
                                    try {
                                        bulkCopy.WriteToServer(oTableResult);
                                        transaction.Commit();
                                        message = "Bulk insert completed";
                                        prog++;
                                        rtbStat.Invoke(delegateMethod);
                                    } catch (Exception ex) {
                                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        transaction.Rollback();
                                    } finally {
                                    }
                                }
                            }
                        }
                        //move parced file to destination folder 
                        fileInfo.MoveTo(destPath + "\\" + fileInfo.Name);
                        //moveFile = new FileInfo(orgFileInfo.Replace(".csv", ".mer"));
                        //moveFile.MoveTo(destPath + "\\" + fileInfo.Name.Replace(".csv", ".mer"));
                        moveFile = new FileInfo(orgFileInfo.Replace(".csv", ".txt"));
                        moveFile.MoveTo(destPath + "\\" + fileInfo.Name.Replace(".csv", ".txt"));
                        //prog++;
                        message = "Moving file '" + fileInfo.Name + "' and its dependencies to destination folder..."
                            + Environment.NewLine
                            + "Files has been completely moved.";
                        rtbStat.Invoke(delegateMethod);
                    } catch (Exception ex) {
                        totalcount = totalcount - 1;
                        message = ex.Message + " This file will be ignored.";
                        messageColor = Color.Red;
                        rtbStat.Invoke(delegateMethod);
                    }
                }                
                btnProcess.Invoke((MethodInvoker)(
                    delegate {
                        btnProcess.Enabled = true;
                        btnProcess.Text = "Finish";
                    }));
                MessageBox.Show("Files has been successfully imported.", "Complete");
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);                
                this.Invoke((MethodInvoker)(
                delegate {
                    var ctls = this.Controls;
                    this.Cursor = Cursors.Arrow;
                    foreach (Control ctl in ctls)
                        ctl.Cursor = Cursors.Arrow;
                }));
            }
            this.Invoke((MethodInvoker)(
                delegate {
                    var ctls = this.Controls;
                    this.Cursor = Cursors.Arrow;
                    foreach (Control ctl in ctls)
                        ctl.Cursor = Cursors.Arrow;
                }));
        }

        private long GetFileMakerId() {
            using (SqlConnection sqlConnection =
                           new SqlConnection(strConnectionString)) {
                sqlConnection.Open();
                // Perform an initial count on the destination table.
                SqlCommand commandMax = new SqlCommand(
                    "select isnull(max(filemaker_id),0) from dbo.DataImports;", sqlConnection);
                long maxid = System.Convert.ToInt64(commandMax.ExecuteScalar());               
                return maxid + 1;
            }
        }

        private void SetPercentage(int prog, int totalcount) {
            double val = Convert.ToDouble(prog) / Convert.ToDouble(totalcount) * 100;
            if (!double.IsNaN(val))
                backgroundWorker1.ReportProgress(Convert.ToInt32(Math.Round(val)));
            else
                backgroundWorker1.ReportProgress(100);
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            // Change the value of the ProgressBar to the BackgroundWorker progress.
            if (e.ProgressPercentage > 100) {
                progressBar1.Value = 100;
            } else {
                progressBar1.Value = e.ProgressPercentage;
            }
            if(progressBar1.Value==100)
                this.lblStatus.Text = e.ProgressPercentage.ToString() + "% Completed";
            else
                this.lblStatus.Text = e.ProgressPercentage.ToString() + "%";
        }

        private DataTable MakeTable() {
            DataTable oTableResult = new DataTable("NewDataImports");

            DataColumn dtColumn = new DataColumn("filemaker_id");
            dtColumn.DataType = System.Type.GetType("System.Int64");
            oTableResult.Columns.Add(dtColumn);

            dtColumn = new DataColumn("path_name");
            dtColumn.DataType = System.Type.GetType("System.String");
            oTableResult.Columns.Add(dtColumn);

            dtColumn = new DataColumn("row");
            dtColumn.DataType = System.Type.GetType("System.Int64");
            oTableResult.Columns.Add(dtColumn);

            dtColumn = new DataColumn("data_context");
            dtColumn.DataType = System.Type.GetType("System.String");
            oTableResult.Columns.Add(dtColumn);

            dtColumn = new DataColumn("column_name");
            dtColumn.DataType = System.Type.GetType("System.String");
            oTableResult.Columns.Add(dtColumn);

            dtColumn = new DataColumn("original_column");
            dtColumn.DataType = System.Type.GetType("System.String");
            oTableResult.Columns.Add(dtColumn);

            return oTableResult;
        }

        private void txtDestinationPath_TextChanged(object sender, EventArgs e) {
            if (Directory.Exists(txtDestinationPath.Text) && Directory.Exists(txtSourcePath.Text)) {
                btnProcess.Enabled = true;
            }else {
                btnProcess.Enabled = false;
            }
        }

        private void txtSourcePath_TextChanged(object sender, EventArgs e) {
            if (Directory.Exists(txtDestinationPath.Text) && Directory.Exists(txtSourcePath.Text)) {
                btnProcess.Enabled = true;
            }else {
                btnProcess.Enabled = false;
            }
        }
    }
}
