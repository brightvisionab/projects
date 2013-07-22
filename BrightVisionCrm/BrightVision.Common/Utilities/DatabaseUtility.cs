
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using BrightVision.Common.Business;

namespace BrightVision.Common.Utilities 
{
    public class DatabaseUtility 
    {
        #region Public Constants
        public static int LargeDatasetFetchLimit = 30;
        #endregion

        #region Constructors
        public DatabaseUtility() {
        }
        #endregion

        #region Public Methods

        #region Execute Command Utilities
        /// <summary>
        /// Gets the document content of a file saved in a blob database table column
        /// </summary>
        /// <param name="objSqlCommand">Contains the sql query that will get the contents of a data</param>
        /// <returns>The content of the document as specified in the sql command</returns>
        public static string GetDocumentContent(SqlCommand objSqlCommand) {
            MemoryStream RtfDocument = null;
            string DocumentContent = string.Empty;

            //using (SqlConnection objConnection = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString()))
            using (SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection)) {
                objConnection.Open();
                objSqlCommand.Connection = objConnection;
                objSqlCommand.CommandTimeout = 100000;

                using (SqlDataReader objReader = objSqlCommand.ExecuteReader()) {
                    try {
                        objReader.Read();
                        if (objReader.HasRows) {
                            if (!objReader.IsDBNull(0)) {
                                Byte[] RawDocument = new Byte[Convert.ToInt32((objReader.GetBytes(0, 0, null, 0, Int32.MaxValue)))];

                                RtfDocument = new MemoryStream();
                                RtfDocument.Write(RawDocument, 0, RawDocument.Length);

                                long BytesReceived = objReader.GetBytes(0, 0, RawDocument, 0, RawDocument.Length);
                                DocumentContent = Encoding.UTF8.GetString(RawDocument, 0, Convert.ToInt32(BytesReceived));
                                //ASCIIEncoding encoding = new ASCIIEncoding();
                                //DocumentContent = encoding.GetString(RawDocument, 0, Convert.ToInt32(BytesReceived));
                            }
                        }
                    } catch (Exception e) {
                        MessageBox.Show(e.Message);
                    }
                }
            }

            return DocumentContent;
        }

        /// <summary>
        /// Execute an sql command query via a pre-defined sql command
        /// </summary>        
        public static void ExecuteSqlCommand(SqlCommand objSqlCommand) {
            //using (SqlConnection objConnection = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString()))
            using (SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection)) {
                objConnection.Open();
                using (SqlTransaction objTransaction = objConnection.BeginTransaction()) {
                    try {
                        objSqlCommand.CommandTimeout = 0;
                        objSqlCommand.Connection = objConnection;
                        objSqlCommand.Transaction = objTransaction;
                        objSqlCommand.ExecuteNonQuery();
                        objTransaction.Commit();
                    } catch (Exception ex) {
                        objTransaction.Rollback();
                    } finally {
                        objSqlCommand.Dispose();
                        objSqlCommand = null;
                    }
                }
            }
        }

        public static void ExecuteNonQuery(string procName, params SqlParameter[] sqlParams) {
            //using (SqlConnection objConnection = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString()))            
            using (SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection)) {
                try {
                    objConnection.Open();
                    using (SqlCommand cmd = new SqlCommand()) {
                        cmd.Connection = objConnection;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = procName;

                        // Handle the parameters 
                        if (sqlParams != null) {
                            foreach (SqlParameter param in sqlParams)
                                cmd.Parameters.Add(param);
                        }
                        cmd.ExecuteNonQuery();
                    }
                } catch { }
            }            
        }

        /// <summary>
        /// Execute an sql stored procedure via a pre-defined sql command
        /// </summary>
        public static DataTable ExecuteSqlQuery(SqlCommand objSqlCommand) {
            DataTable objRecordTable = new DataTable();
            //using (SqlConnection objConnection = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString()))
            using (SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection)) {
                try {
                    objConnection.Open();
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Connection = objConnection;
                    objSqlCommand.CommandTimeout = 0;

                    using (SqlDataAdapter objDataAdapter = new SqlDataAdapter(objSqlCommand)) {
                        objRecordTable.BeginLoadData();
                        objDataAdapter.Fill(objRecordTable);
                        objRecordTable.EndLoadData();
                    }
                } catch { } finally {
                    objSqlCommand.Dispose();
                    objSqlCommand = null;
                }
            }

            return objRecordTable;
        }

        /// <summary>
        /// Execute an sql command query
        /// </summary>
        public static DataTable ExecuteSqlQuery(string sqlQuery) {
            DataTable objRecordTable = new DataTable();
            //using (SqlConnection objConnection = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString()))
            using (SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection)) {
                try {
                    objConnection.Open();
                    using (SqlDataAdapter objDataAdapter = new SqlDataAdapter(sqlQuery, objConnection)) {
                        objRecordTable.BeginLoadData();
                        objDataAdapter.Fill(objRecordTable);
                        objRecordTable.EndLoadData();
                    }
                } catch { }
            }

            return objRecordTable;
        }
        /// <summary>
        /// Execute an sql command query
        /// </summary>
        public static DataTable ExecuteSqlQuery(string sqlQuery, string tableName = "") {
            DataTable objRecordTable = new DataTable(tableName);
            using (SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection)) {
                try {
                    objConnection.Open();
                    using (SqlDataAdapter objDataAdapter = new SqlDataAdapter(sqlQuery, objConnection)) {
                        objRecordTable.BeginLoadData();
                        objDataAdapter.Fill(objRecordTable);
                        objRecordTable.EndLoadData();
                    }
                } catch { }
            }

            return objRecordTable;
        }

        public static DataTable ExecuteStoredProcedure(string procName, string tableName = "", params SqlParameter[] sqlParams) {
            DataTable objRecordTable = new DataTable();
            if (!string.IsNullOrEmpty(tableName))
                objRecordTable = new DataTable(tableName);
            
            using (SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection)) {
                try {
                    objConnection.Open();
                    using (SqlCommand cmd = new SqlCommand()) {
                        cmd.Connection = objConnection;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = procName;
                        cmd.CommandTimeout = 0;

                        // Handle the parameters 
                        if (sqlParams != null) {
                            foreach (SqlParameter param in sqlParams)
                                cmd.Parameters.Add(param);
                        }

                        // Define the data adapter and fill the dataset 
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
                            da.Fill(objRecordTable);
                        }
                    }
                } catch { }
            }

            return objRecordTable;
        }

        /// <summary>
        /// Save using sql bulk processing
        /// </summary>
        public static void ExecuteBulkProcessing(string TableName, DataTable objRecords, SqlConnection objConnection, SqlTransaction objTransaction) {
            using (SqlBulkCopy objBulkProcess = new SqlBulkCopy(objConnection, SqlBulkCopyOptions.Default, objTransaction)) {
                //objBulkProcess.BatchSize = 10;
                objBulkProcess.BulkCopyTimeout = 0;
                objBulkProcess.BatchSize = 5000; // objRecords.Rows.Count;
                objBulkProcess.DestinationTableName = TableName;
                objBulkProcess.WriteToServer(objRecords);
            }
        }

        #endregion

        #region Export View Utility
        //public static IDataReader GetViews(int view_id, ExportView.eExportViewDisplayMode pDisplayMode = ExportView.eExportViewDisplayMode.Default)
        public static Dictionary<string, string> GetViews(int view_id, ExportView.eExportViewDisplayMode pDisplayMode = ExportView.eExportViewDisplayMode.Default)
        {
            return GetPartialViews(view_id, pDisplayMode, null, 0);
        }
        //public static IDataReader GetViews(int view_id, ExportView.eExportViewDisplayMode pDisplayMode = ExportView.eExportViewDisplayMode.Default, string pDatabaseConnection = "")
        public static Dictionary<string, string> GetViews(int view_id, ExportView.eExportViewDisplayMode pDisplayMode = ExportView.eExportViewDisplayMode.Default, string pDatabaseConnection = "", int pAccountId = 0)
        {
            return GetPartialViews(view_id, pDisplayMode, pDatabaseConnection, pAccountId);
        }
        //private static IDataReader GetPartialViews(int view_id, ExportView.eExportViewDisplayMode pDisplayMode = ExportView.eExportViewDisplayMode.Default, string pDatabaseConnection = "")
        private static Dictionary<string, string> GetPartialViews(int view_id, ExportView.eExportViewDisplayMode pDisplayMode = ExportView.eExportViewDisplayMode.Default, string pDatabaseConnection = "", int pAccountId = 0)
        {
            /**
             * reason for this is that web portal calls does not recognize user session instance.
             * so we needed to override the connection string to accept both web portal and application level.
             */
            if (string.IsNullOrEmpty(pDatabaseConnection))
                pDatabaseConnection = UserSession.ProviderConnection;

            else {
                pDatabaseConnection = pDatabaseConnection.Replace("&quot;", "'");
                pDatabaseConnection = pDatabaseConnection.Replace("metadata=res://*/BrightPlatform.csdl|res://*/BrightPlatform.ssdl|res://*/BrightPlatform.msl;provider=System.Data.SqlClient;provider connection string=", "");
                pDatabaseConnection = pDatabaseConnection.Replace("\"", "");
            }

            using (SqlConnection objConnection = new SqlConnection(pDatabaseConnection)) {
                try {
                    IDataReader oReader = null;
                    DataTable _dtData = new DataTable();

                    objConnection.Open();
                    using (SqlCommand cmd = new SqlCommand()) {
                        bool _AccountsAndContactsHavingSubCampaignCallAttemps = false;
                        int? _AccountId = null;

                        if (pDisplayMode == ExportView.eExportViewDisplayMode.AccountsAndContactsHavingSubCampaignCallAttemps)
                            _AccountsAndContactsHavingSubCampaignCallAttemps = true;

                        if (pAccountId > 0)
                            _AccountId = pAccountId;

                        cmd.Connection = objConnection;
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "bvGetViewConfigData_sp";
                        cmd.Parameters.Add(new SqlParameter("view_id", view_id));
                        cmd.Parameters.Add(new SqlParameter("p_accounts_and_contacts_that_have_call_attemps_only", _AccountsAndContactsHavingSubCampaignCallAttemps));
                        cmd.Parameters.Add(new SqlParameter("p_account_id", _AccountId));
                        oReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                        /**
                         * extract the xml data into these separate xml strings.
                         * dialog, schedule, account, contact, and relation data.
                         * build the dictionary containing the xml data.
                         */
                        Dictionary<string, string> _dcXmlData = new Dictionary<string, string>();
                        _dcXmlData.Add("dialogdata", string.Empty);
                        _dcXmlData.Add("scheduledata", string.Empty);
                        _dcXmlData.Add("relationdata", string.Empty);
                        _dcXmlData.Add("accountdata", string.Empty);
                        _dcXmlData.Add("contactdata", string.Empty);

                        if (oReader == null)
                            return null;

                        while (oReader.Read()) {
                            string _data = oReader["dialogdata"].ToString();
                            if (!string.IsNullOrEmpty(_data))
                                _dcXmlData["dialogdata"] = XmlUtility.RemoveInvalidXmlData(_data);
                        }

                        if (oReader.NextResult()) {
                            while (oReader.Read()) {
                                string _data = oReader["scheduledata"].ToString();
                                if (!string.IsNullOrEmpty(_data))
                                    _dcXmlData["scheduledata"] = XmlUtility.RemoveInvalidXmlData(_data);                                   
                            }
                        }

                        if (oReader.NextResult()) {
                            while (oReader.Read()) {
                                string _data = oReader["relationdata"].ToString();
                                if (!string.IsNullOrEmpty(_data))
                                    _dcXmlData["relationdata"] = XmlUtility.RemoveInvalidXmlData(_data);                                    
                            }
                        }

                        if (oReader.NextResult()) {
                            while (oReader.Read()) {
                                string _data = oReader["accountdata"].ToString();
                                if (!string.IsNullOrEmpty(_data))
                                    _dcXmlData["accountdata"] = XmlUtility.RemoveInvalidXmlData(_data);
                            }
                        }

                        if (oReader.NextResult()) {
                            while (oReader.Read()) {
                                string _data = oReader["contactdata"].ToString();
                                if (!string.IsNullOrEmpty(_data))
                                    _dcXmlData["contactdata"] = XmlUtility.RemoveInvalidXmlData(_data);
                            }
                        }

                        return _dcXmlData;
                    }
                }
                catch {
                    if (objConnection.State != ConnectionState.Closed)
                        objConnection.Close();

                    System.Threading.Thread.Sleep(3000);
                    return GetPartialViews(view_id);
                }
            }
        }
        #endregion

        #region Collected Data Utility
        public static IDataReader GetCollectedData(int subcampaign_id, int? account_id, int? contact_id, int? customer_id, bool? custowned, bool? bvowned) {
            return GetPartialCollectedData(subcampaign_id, account_id, contact_id, customer_id, custowned, bvowned);
        }

        private static IDataReader GetPartialCollectedData(int subcampaign_id, int? account_id, int? contact_id, int? customer_id, bool? custowned, bool? bvowned) {
            IDataReader oReader = null;
            SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection);
            try {
                objConnection.Open();
                using (SqlCommand cmd = new SqlCommand()) {
                    cmd.Connection = objConnection;
                    cmd.CommandTimeout = 0;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "bvGetCollectedData_sp";
                    cmd.Parameters.Add(new SqlParameter("@p_subcampaign_id", subcampaign_id));
                    cmd.Parameters.Add(new SqlParameter("@p_account_id", account_id));
                    cmd.Parameters.Add(new SqlParameter("@p_contact_id", contact_id));
                    cmd.Parameters.Add(new SqlParameter("@p_customer_id", customer_id));
                    cmd.Parameters.Add(new SqlParameter("@p_user_id", UserSession.CurrentUser.UserId));
                    cmd.Parameters.Add(new SqlParameter("@p_customer_data", custowned));
                    cmd.Parameters.Add(new SqlParameter("@p_brightvision_data", bvowned));
                    //cmd.CommandText = "bvScGetCollectedData_sp";
                    //cmd.Parameters.Add(new SqlParameter("subcampaign_id", subcampaign_id));
                    //cmd.Parameters.Add(new SqlParameter("account_id", account_id));
                    //cmd.Parameters.Add(new SqlParameter("contact_id", contact_id));
                    //cmd.Parameters.Add(new SqlParameter("customer_id", customer_id));
                    //cmd.Parameters.Add(new SqlParameter("bvowned", bvowned));
                    //cmd.Parameters.Add(new SqlParameter("customerowned", custowned));
                    oReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    return oReader;
                }
            } catch (Exception ex) {
                if (objConnection.State != ConnectionState.Closed) {
                    objConnection.Close();
                }
                System.Threading.Thread.Sleep(3000);
                return GetPartialCollectedData(subcampaign_id, account_id, contact_id, customer_id, custowned, bvowned);
            }
        }
        #endregion

        #region FileMaker DataImport Utitlity
        public static DataTable GetFileMakerCampaigns() {
            DataTable objRecordTable = null;
            string connection = "";
            var curDatabase = UserSession.EntityConnection.StoreConnection.Database;
            var curConnection = UserSession.EntityConnection.StoreConnection.ConnectionString;
            connection = curConnection.Replace(curDatabase, ConfigManager.AppSettings["FileMakerDatabaseName"]);
            using (SqlConnection objConnection = new SqlConnection(connection)) {
                try {
                    objConnection.Open();
                    using (SqlDataAdapter objDataAdapter = new SqlDataAdapter(
                        "select [filemaker_id], [path_name] from [DataImports] group by [filemaker_id], [path_name] order by [path_name]"
                        , objConnection)) {
                        objRecordTable = new DataTable();
                        objRecordTable.BeginLoadData();
                        objDataAdapter.Fill(objRecordTable);
                        objRecordTable.EndLoadData();
                    }
                } catch { }
            }
            return objRecordTable;
        }

        public static DataTable GetFileMakerCampaignFields(long campaign_id) {
            DataTable objRecordTable = null;
            string connection = "";
            var curDatabase = UserSession.EntityConnection.StoreConnection.Database;
            var curConnection = UserSession.EntityConnection.StoreConnection.ConnectionString;
            connection = curConnection.Replace(curDatabase, ConfigManager.AppSettings["FileMakerDatabaseName"]);
            using (SqlConnection objConnection = new SqlConnection(connection)) {
                try {
                    objConnection.Open();
                    using (SqlDataAdapter objDataAdapter = new SqlDataAdapter(
                        "select [column_name], [column_order] = '', [question_id] = '',[question_lookup_desc] = '' "
                        + "from [DataImports] where [filemaker_id] = "
                        + campaign_id.ToString()
                        + " group by [column_name] order by [column_name]"
                        , objConnection)) {
                        objRecordTable = new DataTable();
                        objRecordTable.BeginLoadData();
                        objDataAdapter.Fill(objRecordTable);
                        objRecordTable.EndLoadData();
                    }
                } catch { }
            }
            return objRecordTable;
        }
        public static DataTable GetFileMakerProfilingResults(long campaign_id, string column_names) {
            DataTable objRecordTable = null;
            string connection = "";
            var curDatabase = UserSession.EntityConnection.StoreConnection.Database;
            var curConnection = UserSession.EntityConnection.StoreConnection.ConnectionString;
            connection = curConnection.Replace(curDatabase, ConfigManager.AppSettings["FileMakerDatabaseName"]);
            using (SqlConnection objConnection = new SqlConnection(connection)) {
                try {
                    objConnection.Open();
                    using (SqlCommand cmd = new SqlCommand()) {
                        cmd.CommandTimeout = 0;
                        cmd.Connection = objConnection;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "bvGetProfiledFields_sp";

                        cmd.Parameters.Add(new SqlParameter("filemaker_id", campaign_id));
                        cmd.Parameters.Add(new SqlParameter("column_names", column_names));
                        objRecordTable = new DataTable("FilemakerProfile");
                        // Define the data adapter and fill the dataset 
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
                            da.Fill(objRecordTable);
                        }
                    }
                } catch { }
            }
            return objRecordTable;
        }
        #endregion

        #endregion
    }
}