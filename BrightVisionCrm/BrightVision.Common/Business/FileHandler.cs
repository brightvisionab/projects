using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.IO;
using LinqToExcel;
using Remotion.Data.Linq;

namespace BrightVision.Common.Business
{
    public class ObjectFileHandler
    {
        #region Private Members
        private OleDbConnection m_objFileConn = null;
        private StringBuilder m_ConnectionString = null;
        private OleDbCommand m_objFileCommand = null;
        private OleDbDataAdapter m_objFileDataAdapter = null;
        private DataTable m_objFileDataTable = null;
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the list of column names from the excel file
        /// </summary>
        /// <returns></returns>
        public List<string> GetImportFileColumnNames(string FileName, string SheetName)
        {
            try
            {
                var excelSheet = new ExcelQueryFactory(FileName);
                return excelSheet.GetColumnNames(SheetName).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets the import file sheets
        /// </summary>
        public List<string> GetImportFileSheetNames(string FileName)
        {
            ExcelQueryFactory _eqfFile = new ExcelQueryFactory(FileName);
            List<string> _lstFileSheets = _eqfFile.GetWorksheetNames().ToList();

            DataTable _dtSheets = m_objFileConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            if (_dtSheets == null || _lstFileSheets.Count < 1)
                return null;

            foreach (DataRow _row in _dtSheets.Rows) {
                if (!_row["TABLE_NAME"].ToString().Contains("$"))
                    _lstFileSheets.Remove(_row["TABLE_NAME"].ToString().TrimStart().TrimEnd());
            }

            return _lstFileSheets;
        }

        /// <summary>
        /// Closes the import file
        /// </summary>
        public void CloseExcelFile()
        {
            m_objFileConn.Close();
            m_objFileConn.Dispose();
            m_objFileConn = null;
        }

        /// <summary>
        /// Opens the import file
        /// </summary>
        public bool OpenExcelFile(string ExcelFile)
        {
            m_ConnectionString = new StringBuilder();
            m_ConnectionString.Append("Provider=Microsoft.ACE.OLEDB.12.0;");
            //m_ConnectionString.Append(string.Format(@"Provider={0}\ACEOLEDB.DLL;", System.Windows.Forms.Application.StartupPath));
            m_ConnectionString.Append(String.Format("Data Source={0};", ExcelFile));
            m_ConnectionString.Append("Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1;\";");
            //m_ConnectionString.Append("Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1;TypeGuessRows=100000;\";");
            m_objFileConn = new OleDbConnection(m_ConnectionString.ToString());

            try {
                m_objFileConn.Open();
                return true;
            }
            catch (Exception ex) {
                BrightVision.Common.UI.NotificationDialog.Information("Open excel file error", ex.Message);
                m_objFileConn = null;
                return false;
            }
        }

        /// <summary>
        /// Get data from the import file
        /// </summary>
        public DataTable GetFileContents(string SheetName)
        {
            //BrightVision.Common.Utilities.RegistryUtility.UpdateTypeGuessRows();
            string strSqlQuery = String.Format("SELECT * FROM [{0}$]", SheetName);
            m_objFileCommand = new OleDbCommand(strSqlQuery, m_objFileConn);
            m_objFileDataAdapter = new OleDbDataAdapter(m_objFileCommand);
            m_objFileDataTable = new DataTable();
            m_objFileDataAdapter.Fill(m_objFileDataTable);

            return m_objFileDataTable;
        }

        /// <summary>
        /// Get data from the import file
        /// </summary>
        public DataTable GetFileContents(string SheetName, string SelectFields, string Condition)
        {
            try
            {
                string strSqlQuery = string.Format("SELECT {0} FROM [{1}]", SelectFields, SheetName);
                if (Condition.Length > 0)
                    strSqlQuery = string.Format("{0} {1}", strSqlQuery, Condition);

                m_objFileDataTable = new DataTable();
                m_objFileCommand = new OleDbCommand(strSqlQuery, m_objFileConn);
                m_objFileDataAdapter = new OleDbDataAdapter(m_objFileCommand);
                m_objFileDataAdapter.Fill(m_objFileDataTable);

                return m_objFileDataTable;
            }
            catch
            {
                return new DataTable();
            }
        }

        /// <summary>
        /// Get data from the import file
        /// </summary>
        public void GetFileContents(string SheetName, string SelectFields, string Condition, ref DataTable PreDefindedTable)
        {
            string strSqlQuery = "SELECT " + SelectFields + " FROM [" + SheetName + "]";
            if (Condition.Length > 0)
                strSqlQuery = strSqlQuery + " " + Condition;

            m_objFileDataTable = new DataTable();
            m_objFileCommand = new OleDbCommand(strSqlQuery, m_objFileConn);
            m_objFileDataAdapter = new OleDbDataAdapter(m_objFileCommand);
            if (PreDefindedTable != null)
            {
                m_objFileDataAdapter.FillSchema(PreDefindedTable, SchemaType.Mapped);
                m_objFileDataAdapter.Fill(PreDefindedTable);
                m_objFileDataTable = PreDefindedTable;
            }
            else
                m_objFileDataAdapter.Fill(m_objFileDataTable);

            //return m_objFileDataTable;
        }

        //void m_objFileDataAdapter_RowUpdating(object sender, OleDbRowUpdatingEventArgs e)
        //{
        //    var x = e.Errors;
        //}

        //void m_objFileDataAdapter_FillError(object sender, FillErrorEventArgs e)
        //{
        //    List<object> _lstObjects = new List<object>();
        //    foreach (object item in e.Values)
        //    {
        //        _lstObjects.Add(item);
        //    }
        //    e.DataTable.Rows.Add(new object[] { e.Values[0], e.Values[1], DBNull.Value }); 
        //}
        #endregion
    }
}
