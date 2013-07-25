using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;

using BrightVision.Model;
using System.Data.Objects;
using System.Data.Common;
using System.Collections;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;

namespace BrightVision.Common.Utilities 
{
    public class DataImportUtility
    {
        #region Classes
        /// <summary>
        /// Gets or sets import file instance
        /// </summary>
        //public class ImportFileInstance
        //{
        //    public int id { get; set; }
        //    public string list_name { get; set; }
        //    public int customer_id { get; set; }
        //    public DateTime created_date { get; set; }
        //    public int created_by_id { get; set; }
        //}

        /// <summary>
        /// Gets or sets import file header id instance
        /// </summary>
        public class ImportFileHeaderIdInstance
        {
            public int id { get; set; }
            public string column_name { get; set; }
        }

        /// <summary>
        /// Gets or sets import file row instance
        /// </summary>
        public class ImportFileRowInstance
        {
            public int row_order { get; set; }
            public string row_value { get; set; }
        }

        public enum eMatchBy
        {
            AccountId,
            OrgNo,
            ContactId,
            Email
        }
        #endregion

        #region Business Methods
        public static DataTable CreateDataImportAccountTable()
        {
            using (SqlCommand objSqlCommand = new SqlCommand("bvGetDataImportAccountBulkInsertStructure_sp")) {
                return DatabaseUtility.ExecuteSqlQuery(objSqlCommand);
            }
        }
        public static DataTable CreateAccountTable()
        {
            using (SqlCommand objSqlCommand = new SqlCommand("bvGetAccountBulkInsertStructure_sp")) {
                return DatabaseUtility.ExecuteSqlQuery(objSqlCommand);
            }
        }
        public static DataTable CreateContactTable()
        {
            using (SqlCommand objSqlCommand = new SqlCommand("bvGetContactBulkInsertStructure_sp")) {
                return DatabaseUtility.ExecuteSqlQuery(objSqlCommand);
            }
        }
        public static DataTable GetLargeCompanyImportMatchedContacts(eMatchBy pMatchBy, DataTable ImportFileMatchColumn, int CountryId, List<string> ColumnNames)
        {
            using (SqlCommand objCommand = new SqlCommand("bvGetImportFileMatchedContacts_sp")) {
                objCommand.Parameters.Add("p_country_id", SqlDbType.Int).Value = CountryId;
                objCommand.Parameters.Add("p_select_columns", SqlDbType.NVarChar).Value = string.Join(",", ColumnNames.ToArray());
                objCommand.Parameters.Add("p_match_column", SqlDbType.Structured).Value = ImportFileMatchColumn;

                if (pMatchBy == eMatchBy.Email)
                    objCommand.Parameters.Add("p_match_by", SqlDbType.NVarChar).Value = "email";
                else if (pMatchBy == eMatchBy.ContactId)
                    objCommand.Parameters.Add("p_match_by", SqlDbType.NVarChar).Value = "contact_id";

                objCommand.CommandTimeout = 0;
                return DatabaseUtility.ExecuteSqlQuery(objCommand);
            }
        }

        /// <summary>
        /// Get large company import file matched account items
        /// </summary>
        /// <param name="MergeListIds"></param>
        /// <param name="BlockedMergeListIds"></param>
        /// <returns></returns>
        public static DataTable GetLargeCompanyImportMatchedAccounts(eMatchBy pMatchBy, DataTable ImportFileMatchColumn, int CountryId, List<string> ColumnNames)
        {
            using (SqlCommand objCommand = new SqlCommand("bvGetImportFileMatchedAccounts_sp")) {
                objCommand.Parameters.Add("p_country_id", SqlDbType.Int).Value = CountryId;
                objCommand.Parameters.Add("p_select_columns", SqlDbType.NVarChar).Value = string.Join(",", ColumnNames.ToArray());
                objCommand.Parameters.Add("p_match_column", SqlDbType.Structured).Value = ImportFileMatchColumn;

                if (pMatchBy == eMatchBy.OrgNo)
                    objCommand.Parameters.Add("p_match_by", SqlDbType.NVarChar).Value = "org_no";
                else if (pMatchBy == eMatchBy.AccountId)
                    objCommand.Parameters.Add("p_match_by", SqlDbType.NVarChar).Value = "account_id";

                objCommand.CommandTimeout = 0;
                return DatabaseUtility.ExecuteSqlQuery(objCommand);
            }
        }

        /// <summary>
        /// Create a data table for the large company import
        /// </summary>
        /// <returns></returns>
        public static DataTable CreateLargeContactImportColumnMatching()
        {
            DataTable objTable = new DataTable("large_contact_import_column_matches");
            DataColumn objDataCol = null;

            objDataCol = new DataColumn("master_data_fields");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("matched_column_name");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("matched_column_no");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            return objTable;
        }

        /// <summary>
        /// Create a data table for the large company import
        /// </summary>
        /// <returns></returns>
        public static DataTable CreateLargeCompanyImportColumnMatching()
        {
            DataTable objTable = new DataTable("large_company_import_column_matches");
            DataColumn objDataCol = null;

            objDataCol = new DataColumn("master_data_fields");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("matched_column_name");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("matched_column_no");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            return objTable;
        }

        /// <summary>
        /// Save and create collected data out of the imported file
        /// </summary>
        /// <param name="SubCampaignId"></param>
        /// <param name="QuestionId"></param>
        public static void CreateImportListCollectedData(int ImportFileId, bool CustomerOwnedData, bool BvOwnedData, int CustomerId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.CommandTimeout = 0;
                objDbModel.FICreateImportFileCollectedData(ImportFileId, CustomerOwnedData, BvOwnedData, CustomerId);
            }
        }

        /// <summary>
        /// create fuzzy lookup accounts call list and generate contacts matching list
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="CustomerId"></param>
        /// <param name="CallListName"></param>
        public static void CreateCallListAndContactMatchList(int ImportFileId, int CustomerId, string CallListName, bool DoCreateContactMatchList)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                objDbModel.FICreateFuzzyLookupAccountsCallList(ImportFileId, CallListName, CustomerId, UserSession.CurrentUser.UserId, DoCreateContactMatchList);
            }
        }

        /// <summary>
        /// Check if fuzzy company macthing is done 
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="FuzzyLookupAccountIds"></param>
        /// <returns></returns>
        public static CTProcessedFuzzyLookupAccount CheckProcessedFuzzyMatchesCompanies(int ImportFileId)
        {
            CTProcessedFuzzyLookupAccount _item = null;
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _item = objDbModel.FICheckProcessedFuzzyLookupAccounts(ImportFileId).FirstOrDefault();
                if (_item != null) {
                    if (_item.items_processed == _item.total_items)
                        objDbModel.FIUpdateFuzzyMatchCompanyReferences(ImportFileId);
                }
            }

            return _item;
        }
        
        /// <summary>
        /// Get existing profiled data records
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <returns></returns>
        public static DataTable GetProfiledData(int ImportFileId)
        {
            using (SqlCommand objCommand = new SqlCommand("bvGetProfiledData_sp")) {
                objCommand.Parameters.Add("@p_import_file_id", SqlDbType.Int).Value = ImportFileId;
                return DatabaseUtility.ExecuteSqlQuery(objCommand);
            }
        }

        /// <summary>
        /// Get profiled data column mappings
        /// </summary>
        public static ObjectQuery GetProfiledDataColumnMaps(int ImportFileId, BrightPlatformEntities objBrightPlatformEntity)
        {
            profiled_data_column_maps item = objBrightPlatformEntity.profiled_data_column_maps.FirstOrDefault(i => i.import_file_id == ImportFileId);
            if (item == null) {
                profiled_data_column_maps pdcm = new profiled_data_column_maps() { import_file_id = ImportFileId };
                objBrightPlatformEntity.profiled_data_column_maps.AddObject(pdcm);
                objBrightPlatformEntity.SaveChanges();
            }

            var objProfiledDataColumnMaps =
                from objItem in objBrightPlatformEntity.profiled_data_column_maps
                where objItem.import_file_id == ImportFileId
                select objItem;

            return (ObjectQuery) objProfiledDataColumnMaps;
        }

        /// <summary>
        /// Save the selected companies to master data
        /// </summary>
        /// <param name="FuzzyLookupAccountIds"></param>
        public static void SaveFuzzyLookupAccountToMasterTable(int CustomerId, string ListName, int ImportFileId, List<string> FuzzyLookupAccountIds, string pLastModifiedSource)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FISaveFuzzyLookupAccountsToMasterTable(
                    CustomerId,
                    ListName,
                    UserSession.CurrentUser.UserId,
                    ImportFileId,
                    string.Join(",", FuzzyLookupAccountIds.ToArray()),
                    UserSession.CurrentUser.ComputerName,
                    pLastModifiedSource);
                //BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Import_List);
            }
        }

        /// <summary>
        /// Get fuzzy lookup accounts statistics
        /// </summary>
        /// <param name="ImportFileId"></param>
        public static CTFuzzyLookupAccountMatchStatistics GetFuzzyLookupAccountMatchStatistics(int ImportFileId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                return objDbModel.FIGetFuzzyLookupAccountMatchStatistics(ImportFileId).ToList()[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="FuzzyLookupAccountIds"></param>
        public static void UpdateFuzzyLookupAccountsByFuzzyCompanyMatches(int ImportFileId, List<string> FuzzyLookupAccountIds)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FIUpdateFuzzyLookupAccountMatchesByFuzzyCompanyName(ImportFileId, string.Join(",", FuzzyLookupAccountIds.ToArray()), UserSession.CurrentUser.UserId);
            }
        }

        /// <summary>
        /// get country code by id
        /// </summary>
        /// <param name="CountryId"></param>
        /// <returns></returns>
        public static string GetCountryCode(int CountryId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection))  {
                var item = objDbModel.countries.FirstOrDefault(i => i.id == CountryId);
                if (item != null)
                    return item.code;
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Update fuzzy lookup accounts
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="ProfiledDataIds"></param>
        public static void UpdateFuzzyLookupAccountsByExactOrgNo(int ImportFileId, List<string> FuzzyLookupAccountIds, short pMatchByAccountType)
        {
            /**
             *  pMatchByAccountType:    
             *  0 = match both validated and unvalidated accounts
             *  1 = match by validated accounts only
             *  2 = match by unvalidated accounts only
             */
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.CommandTimeout = 0;
                objDbModel.FIUpdateFuzzyLookupAccountMatchesByExactOrganizationNo(ImportFileId, string.Join(",", FuzzyLookupAccountIds.ToArray()), UserSession.CurrentUser.UserId, pMatchByAccountType);
            }
        }

        /// <summary>
        /// Update fuzzy lookup accounts
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="FuzzyLookupAccountIds"></param>
        public static void UpdateFuzzyLookupAccountsByExactCompanyNameWithCity(int ImportFileId, List<string> FuzzyLookupAccountIds, short pMatchByAccountType)
        {
            /**
             *  pMatchByAccountType:    
             *  0 = match both validated and unvalidated accounts
             *  1 = match by validated accounts only
             *  2 = match by unvalidated accounts only
             */
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FIUpdateFuzzyLookupAccountMatchesByExactCompanyNameAndCity(ImportFileId, string.Join(",", FuzzyLookupAccountIds.ToArray()), UserSession.CurrentUser.UserId, pMatchByAccountType);
            }
        }

        /// <summary>
        /// Update fuzzy lookup accounts
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="ProfiledDataIds"></param>
        public static void UpdateFuzzyLookupAccountsByExactCompanyName(int ImportFileId, List<string> FuzzyLookupAccountIds, short pMatchByAccountType)
        {
            /**
             *  pMatchByAccountType:    
             *  0 = match both validated and unvalidated accounts
             *  1 = match by validated accounts only
             *  2 = match by unvalidated accounts only
             */
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FIUpdateFuzzyLookupAccountMatchesByExactCompanyName(ImportFileId, string.Join(",", FuzzyLookupAccountIds.ToArray()), UserSession.CurrentUser.UserId, pMatchByAccountType);
            }
        }

        /// <summary>
        /// Clear company match list
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="FuzzyLookupCompanyIds"></param>
        public static void ClearFuzzyLookupAccountMatches(int ImportFileId, List<string> FuzzyLookupCompanyIds, bool PrepareForFuzzyMatching)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FIClearFuzzyLookupAccountMatches(ImportFileId, string.Join(",", FuzzyLookupCompanyIds.ToArray()), UserSession.CurrentUser.UserId, PrepareForFuzzyMatching);
            }
        }

        /// <summary>
        /// Get account matching list
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <returns></returns>
        public static List<CTFuzzyLookupAccount> GetFuzzyLookupAccountList(int ImportFileId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                return objDbModel.FIGetFuzzyLookupAccounts(ImportFileId).ToList();
            }
        }

        /// <summary>
        /// Create the fuzzy data records for use in accounts and contacts matching
        /// </summary>
        /// <param name="ImportFileId"></param>
        public static void CreateFuzzyData(int ImportFileId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FICreateFuzzyData(ImportFileId, UserSession.CurrentUser.UserId);
            }
        }

        /// <summary>
        /// Save the selected fuzzy lookup contacts to the contacts master table
        /// </summary>
        /// <param name="FuzzyLookupContactIds"></param>
        public static void SaveFuzzyLookupContactToMasterTable(List<string> FuzzyLookupContactIds, string pLastModifiedSource)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FISaveFuzzyLookupContactsToMasterTable(
                    string.Join(",", FuzzyLookupContactIds.ToArray()),
                    UserSession.CurrentUser.UserId,
                    UserSession.CurrentUser.ComputerName,
                    pLastModifiedSource);
                //BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Import_List);
            }
        }

        /// <summary>
        /// Get statistics
        /// </summary>
        /// <param name="ImportFileId"></param>
        public static CTFuzzyLookupContactStatistics GetFuzzyLookupContactStatistics(int ImportFileId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                return objDbModel.FIGetFuzzyLookupContactStatistics(ImportFileId).ToList()[0];
            }
        }

        /// <summary>
        /// Clear profiled data contact matches
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="FuzzyLookupContactIds"></param>
        public static void ClearFuzzyLookupContactMatches(int ImportFileId, List<string> FuzzyLookupContactIds)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FIClearFuzzyLookupContactMatches(ImportFileId, string.Join(",", FuzzyLookupContactIds.ToArray()));
            }
        }

        /// <summary>
        /// Update fuzzy lookup contact matches by exact email
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="FuzzyLookupContactIds"></param>
        public static void UpdateFuzzyLookupContactsByExactContactEmail(int ImportFileId, List<string> FuzzyLookupContactIds)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FIUpdateFuzzyLookupContactMatchesByExactEmail(ImportFileId, string.Join(",", FuzzyLookupContactIds.ToArray()));
            }
        }

        /// <summary>
        /// Update fuzzy lookup contacts by exact contact name
        /// </summary>
        /// <param name="ImportFileId"></param>
        /// <param name="FuzzyLookupContactIds"></param>
        public static void UpdateFuzzyLookupContactsByExactContactName(int ImportFileId, List<string> FuzzyLookupContactIds)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objDbModel.FIUpdateFuzzyLookupContactMatchesByExactContactName(ImportFileId, string.Join(",", FuzzyLookupContactIds.ToArray()));
            }
        }

        /// <summary>
        /// Get profiled data contacts
        /// </summary>
        /// <returns></returns>
        public static List<CTFuzzyLookupContact> GetFuzzyLookupContacts(int ImportFileId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                return objDbModel.FIGetFuzzyLookupContacts(ImportFileId).ToList();
            }
        }

        /// <summary>
        /// Build data table for profiled data
        /// </summary>
        public static DataTable CreateProfiledDataTable()
        {
            DataTable objTable = new DataTable("vw_profiled_data");
            DataColumn objDataCol = null;

            objDataCol = new DataColumn("import_file_id");
            objDataCol.DataType = System.Type.GetType("System.Int64");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("row_order");
            objDataCol.DataType = System.Type.GetType("System.Int64");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_company_name");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_org_no");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_box_address");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_street_address");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_zipcode");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_city");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_country");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_telephone");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_telefax");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_www");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_year_established");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_parent_company");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_activity_code");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            //objDataCol = new DataColumn("account_activity_code_description");
            //objDataCol.DataType = System.Type.GetType("System.String");
            //objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_activity_code_2");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            //objDataCol = new DataColumn("account_activity_code_2_description");
            //objDataCol.DataType = System.Type.GetType("System.String");
            //objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_currency");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            
            objDataCol = new DataColumn("account_turnover");
            objDataCol.DataType = System.Type.GetType("System.Double");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_result");
            objDataCol.DataType = System.Type.GetType("System.Double");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_employees_abroad");
            objDataCol.DataType = System.Type.GetType("System.Int64");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_employees_total");
            objDataCol.DataType = System.Type.GetType("System.Int64");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("assigned_to");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_first_name");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_middle_name");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_last_name");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_direct_phone");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_mobile");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_email");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_title");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_address_1");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_address_2");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_city");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_zipcode");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_country");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("bv_account_id");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("bv_contact_id");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("account_priority");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("contact_priority");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_list_comment");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_1");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_2");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_3");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_4");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_5");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_6");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_7");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_8");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_9");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("customer_field_10");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("collected_data_field_1");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            objDataCol = new DataColumn("collected_data_field_2");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            objDataCol = new DataColumn("collected_data_field_3");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            objDataCol = new DataColumn("collected_data_field_4");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            objDataCol = new DataColumn("collected_data_field_5");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            objDataCol = new DataColumn("collected_data_field_6");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            objDataCol = new DataColumn("collected_data_field_7");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            objDataCol = new DataColumn("collected_data_field_8");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            objDataCol = new DataColumn("collected_data_field_9");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);
            objDataCol = new DataColumn("collected_data_field_10");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("collected_data_date");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            return objTable;
        }

        /// <summary>
        /// Build data table for imported file detail table
        /// </summary>
        public static DataTable CreateImportFileDetailTable()
        {
            DataTable objTable = new DataTable("vw_import_file_details");
            DataColumn objDataCol = null;

            objDataCol = new DataColumn("imported_file_header_id");
            objDataCol.DataType = System.Type.GetType("System.Int64");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("row_order");
            objDataCol.DataType = System.Type.GetType("System.Int64");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("column_value");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            return objTable;
        }

        /// <summary>
        /// Build data table for imported file header table
        /// </summary>
        public static DataTable CreateImportFileHeaderTable()
        {
            DataTable objTable = new DataTable("vw_import_file_headers");
            DataColumn objDataCol = null;

            objDataCol = new DataColumn("imported_file_id");
            objDataCol.DataType = System.Type.GetType("System.Int32");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("column_order");
            objDataCol.DataType = System.Type.GetType("System.Int32");
            objTable.Columns.Add(objDataCol);

            objDataCol = new DataColumn("column_name");
            objDataCol.DataType = System.Type.GetType("System.String");
            objTable.Columns.Add(objDataCol);

            return objTable;
        }

        /// <summary>
        /// Get import file rows
        /// </summary>
        public static List<ImportFileRowInstance> GetImportFileRows(int ImportFileId)
        {
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                var objImportFileRows = from objImportFileRow in m_objBrightPlatformEntity.FIGetImportFileRowItems(ImportFileId)
                    select new ImportFileRowInstance {
                        row_order = objImportFileRow.row_order,
                        row_value = objImportFileRow.row_values
                    };

                return objImportFileRows.ToList<ImportFileRowInstance>();
            }
        }

        /// <summary>
        /// Get import file columns
        /// </summary>
        public static ObjectQuery GetImportFileColumns(int ImportFileId, bool ForProfiling)
        {
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                if (ForProfiling) {
                    var objImportFileHeaders = (
                            from objImportFileHeader in m_objBrightPlatformEntity.imported_file_headers
                            where objImportFileHeader.imported_file_id == ImportFileId
                            orderby objImportFileHeader.column_order
                            select new ImportFileHeaderIdInstance {
                                id = 0,
                                column_name = string.Empty
                            }
                        )
                        .Skip(0).Take(1).Union(
                            from objImportFileHeader in m_objBrightPlatformEntity.imported_file_headers
                            where objImportFileHeader.imported_file_id == ImportFileId
                            orderby objImportFileHeader.column_order
                            select new ImportFileHeaderIdInstance {
                                id = objImportFileHeader.id,
                                column_name = objImportFileHeader.column_name
                            }
                        );

                    return (ObjectQuery)objImportFileHeaders;
                }
                else {
                    var objImportFileHeaders =
                       from objImportFileHeader in m_objBrightPlatformEntity.imported_file_headers
                       where objImportFileHeader.imported_file_id == ImportFileId
                       orderby objImportFileHeader.column_order
                       select new ImportFileHeaderIdInstance {
                           id = objImportFileHeader.id,
                           column_name = objImportFileHeader.column_name
                       };

                    return (ObjectQuery)objImportFileHeaders;
                }
            }
        }

        /// <summary>
        /// Get import files
        /// </summary>
        //public static List<CTImportList> GetImportLists()
        //{
        //    List<CTImportList> _items = null;
        //    using (BrightPlatformEntities _objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
        //        _objBrightPlatformEntity.CommandTimeout = 0;
        //        _items = _objBrightPlatformEntity.FIGetImportLists().ToList();
        //    }
            
        //    return _items;
        //    //m_objBrightPlatformEntity.CommandTimeout = 0;
        //    //var objImportLists =
        //    //    from objImportList in m_objBrightPlatformEntity.imported_files
        //    //    join objCustomer in m_objBrightPlatformEntity.customers on objImportList.customer_id equals objCustomer.id
        //    //    join objUser in m_objBrightPlatformEntity.users on objImportList.created_by equals objUser.id
        //    //    join objCampaign in m_objBrightPlatformEntity.campaigns on objImportList.campaign_id equals objCampaign.id
        //    //    join objTCountry in m_objBrightPlatformEntity.countries on objImportList.country_id equals objTCountry.id into objTCountry
        //    //    from objCountry in objTCountry.DefaultIfEmpty()
        //    //    orderby objImportList.active descending, objImportList.id descending
        //    //    select new ImportListInstance
        //    //    {
        //    //        id = objImportList.id,
        //    //        active = (objImportList.active == null? false: (bool) objImportList.active),
        //    //        profiled = (objImportList.profiled == null? false: (bool) objImportList.profiled),
        //    //        matched_by_account = (objImportList.matched_by_account == null? false: (bool) objImportList.matched_by_account),
        //    //        matched_by_contact = (objImportList.matched_by_contact == null? false: (bool) objImportList.matched_by_contact),
        //    //        improved_master_data = (objImportList.improved_master_data == null? false: (bool) objImportList.improved_master_data),
        //    //        country = "",
        //    //        import_list_name = objImportList.list_name,
        //    //        date_and_time = (DateTime) objImportList.created_date,
        //    //        customer_name = objCustomer.customer_name,
        //    //        imported_by = objUser.fullname,
        //    //        customer_id = objCustomer.id,
        //    //        campaign_id = objImportList.campaign_id,
        //    //        campaign_name = objCampaign.campaign_name,
        //    //        country_id = objImportList.country_id == null? 0: (int) objImportList.country_id,
        //    //        country_name = objCountry.name,
        //    //        source_file_name = objImportList.source_file_name
        //    //    };

        //    ////m_objBrightPlatformEntity.Connection.Close();
        //    //return objImportLists.ToList();
        //}

        /// <summary>
        /// Save and get import file header ids
        /// </summary>
        public static List<ImportFileHeaderIdInstance> SaveImportFileHeaders(int ImportFileId, DataTable objTable, SqlConnection objConnection, SqlTransaction objTransaction)
        {
            SqlCommand objCommand = null;
            SqlDataReader objReader = null;
            IAsyncResult objResult = null;
            List<ImportFileHeaderIdInstance> objList = new List<ImportFileHeaderIdInstance>();
            ImportFileHeaderIdInstance objFileHeader = null;

            foreach (DataRow Item in objTable.Rows) {
                using (objCommand = new SqlCommand("bvSaveImportFileHeaders_sp", objConnection, objTransaction)) {
                    objCommand.CommandType = CommandType.StoredProcedure;
                    objCommand.Parameters.Add("@p_import_file_id", SqlDbType.Int).Value = Item["imported_file_id"];
                    objCommand.Parameters.Add("@p_column_order", SqlDbType.Int).Value = Item["column_order"];
                    objCommand.Parameters.Add("@p_column_name", SqlDbType.NVarChar).Value = Item["column_name"];

                    objReader = null;
                    objResult = null;

                    objResult = objCommand.BeginExecuteReader(CommandBehavior.SingleRow);
                    objReader = objCommand.EndExecuteReader(objResult);
                    objReader.Read();

                    objFileHeader = null;
                    objFileHeader = new ImportFileHeaderIdInstance();
                    objFileHeader.id = (int)objReader[0];
                    objFileHeader.column_name = Item["column_name"].ToString();

                    objReader.Close();
                    objList.Add(objFileHeader);
                }
            }

            return objList;
        }

        /// <summary>
        /// Save using sql bulk processing
        /// </summary>
        public static void ExecuteBulkProcessing(string TableName, DataTable objRecords, SqlConnection objConnection, SqlTransaction objTransaction)
        {
            DatabaseUtility.ExecuteBulkProcessing(TableName, objRecords, objConnection, objTransaction);
            //SqlBulkCopy objBulkProcess = new SqlBulkCopy(objConnection, SqlBulkCopyOptions.Default, objTransaction);
            //objBulkProcess.BatchSize = 10;
            //objBulkProcess.DestinationTableName = TableName;
            //objBulkProcess.WriteToServer(objRecords);
        }

        /// <summary>
        /// Save import file record
        /// </summary>
        public static int SaveImportFile(string SourceFileName,string ListName, int CustomerId, int CampaignId, int CountryId, SqlConnection objConnection, SqlTransaction objTransaction)
        {
            using (SqlCommand objCommand = new SqlCommand("bvSaveImportFile_sp", objConnection, objTransaction)) {
                objCommand.CommandType = CommandType.StoredProcedure;
                objCommand.Parameters.Add("@p_source_file_name", SqlDbType.NVarChar).Value = Path.GetFileName(SourceFileName);
                objCommand.Parameters.Add("@p_list_name", SqlDbType.NVarChar).Value = ListName;
                objCommand.Parameters.Add("@p_customer_id", SqlDbType.Int).Value = CustomerId;
                objCommand.Parameters.Add("@p_campaign_id", SqlDbType.Int).Value = CampaignId;
                objCommand.Parameters.Add("@p_country_id", SqlDbType.Int).Value = CountryId;
                objCommand.Parameters.Add("@p_created_by_id", SqlDbType.Int).Value = UserSession.CurrentUser.UserId;

                IAsyncResult objResult = objCommand.BeginExecuteReader(CommandBehavior.SingleRow);
                SqlDataReader objReader = objCommand.EndExecuteReader(objResult);
                objReader.Read();
                int ImportFileId = Convert.ToInt32(objReader[0]);
                objReader.Close();

                return ImportFileId;
            }
        }

        /// <summary>
        /// Deletes profiled data
        /// </summary>
        public static void DeleteProfiledData(int ImportFileId)
        {
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_objBrightPlatformEntity.FIDeleteProfiledData(ImportFileId);
            }
        }

        /// <summary>
        /// Saves import file profiled status
        /// </summary>
        public static void SetImportFileProfiledStatus(int ImportFileId)
        {
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var objEntityImportFile = m_objBrightPlatformEntity.imported_files.Where(objEntity => objEntity.id == ImportFileId).SingleOrDefault();
                objEntityImportFile.profiled = true;
                m_objBrightPlatformEntity.SaveChanges();
            }
        }

        /// <summary>
        /// Saves import file status
        /// </summary>
        public static void SetImportFileStatus(int ImportFileId, bool IsEnabled)
        {
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var objEntityImportFile = m_objBrightPlatformEntity.imported_files.Where(objEntity => objEntity.id == ImportFileId).SingleOrDefault();
                objEntityImportFile.active = IsEnabled;
                m_objBrightPlatformEntity.SaveChanges();
            }
        }

        /// <summary>
        /// Saves import file status
        /// </summary>
        public static void DeleteImportFile(int ImportFileId)
        {
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                m_objBrightPlatformEntity.FIDeleteImportFile(ImportFileId);
            }
        }

        /// <summary>
        /// Save import file as matched by account
        /// </summary>
        /// <param name="ImportedFileId"></param>
        public static void MatchedByAccount(int ImportedFileId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var Item = objDbModel.imported_files.FirstOrDefault(i => i.id == ImportedFileId);
                Item.matched_by_account = true;
                objDbModel.SaveChanges();
            }
        }

        /// <summary>
        /// Save import file as matched by contact
        /// </summary>
        /// <param name="ImportedFileId"></param>
        public static void MatchedByContact(int ImportedFileId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var Item = objDbModel.imported_files.FirstOrDefault(i => i.id == ImportedFileId);
                Item.matched_by_contact = true;
                objDbModel.SaveChanges();
            }
        }

        /// <summary>
        /// Save import file as improved master data
        /// </summary>
        /// <param name="ImportedFileId"></param>
        public static void ImprovedMasterData(int ImportedFileId)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var Item = objDbModel.imported_files.FirstOrDefault(i => i.id == ImportedFileId);
                Item.improved_master_data = true;
                objDbModel.SaveChanges();
            }
        }
        #endregion
    }
}
