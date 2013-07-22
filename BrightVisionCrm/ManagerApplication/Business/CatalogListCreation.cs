using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.Data.Objects;
using System.Data;
using System.Data.SqlClient;

namespace ManagerApplication.Business
{
    public class ObjectListCreation
    {
        #region Classes
        #endregion

        #region Business Methods
        /// <summary>
        /// Save final list
        /// </summary>
        public static int SaveMergeList(int CustomerId, string ListName, int CampaignId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            merge_lists objList = new merge_lists()
            {
                customer_id = CustomerId,
                campaign_id = CampaignId,
                list_name = ListName,
                created_by = UserSession.CurrentUser.UserId,
                created_on = DateTime.Now
            };

            m_objBrightPlatformEntity.merge_lists.AddObject(objList);
            m_objBrightPlatformEntity.SaveChanges();

            return objList.id;
        }

        /// <summary>
        /// Save list
        /// </summary>
        public static int SaveList(int CustomerId, string ListName)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            list objList = new list() {
                customer_id = CustomerId,
                list_name = ListName,
                matched = false,
                created_by = UserSession.CurrentUser.UserId,
                created_on = DateTime.Now
            };

            m_objBrightPlatformEntity.lists.AddObject(objList);
            m_objBrightPlatformEntity.SaveChanges();

            return objList.id;
        }

        /// <summary>
        /// Remove list
        /// </summary>
        public static void RemoveList(list objList)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            m_objBrightPlatformEntity.lists.DeleteObject(objList);
            m_objBrightPlatformEntity.SaveChanges();
        }

        /// <summary>
        /// Get all merged list results
        /// </summary>
        public static DataTable GetMergeListsByListId(int CustomerId, int ListId)
        {
            SqlCommand objSqlCommand = new SqlCommand("bvGetMergeListsByList_sp");
            objSqlCommand.Parameters.Add("@p_customer_id", SqlDbType.Int).Value = CustomerId;
            objSqlCommand.Parameters.Add("@p_list_id", SqlDbType.Int).Value = ListId;
            return DatabaseUtility.ExecuteSqlQuery(objSqlCommand);
        }

        /// <summary>
        /// Get all merged list results
        /// </summary>
        public static DataTable GetMergeLists(int CustomerId)
        {
            SqlCommand objSqlCommand = new SqlCommand("bvGetMergeLists_sp");
            objSqlCommand.Parameters.Add("@p_customer_id", SqlDbType.Int).Value = CustomerId;
            return DatabaseUtility.ExecuteSqlQuery(objSqlCommand);
        }

        /// <summary>
        /// Update list_accounts table
        /// </summary>
        public static void CreateMergeListItems(int MergeListId, string ListAccountIds)
        {
            SqlBuilder objSqlBuilder = new SqlBuilder();
            objSqlBuilder.CreateSelect(MergeListId.ToString(), "merge_list_id", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("id", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateFrom("list_accounts", null);
            objSqlBuilder.CreateWhere("id", ListAccountIds, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.In);
            string sqlListAccounts = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.Select);

            objSqlBuilder.Clear();
            objSqlBuilder.CreateInsertTable("merge_list_list_accounts");
            objSqlBuilder.CreateInsertIntoField("merge_list_id");
            objSqlBuilder.CreateInsertIntoField("list_account_id");
            objSqlBuilder.CreateInsertIntoSelectionValue(sqlListAccounts);
            string sqlInsertIntoMergeListListAccounts = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.InsertSelect);

            SqlCommand objCommand = new SqlCommand(sqlInsertIntoMergeListListAccounts);
            objCommand.CommandType = CommandType.Text;
            DatabaseUtility.ExecuteSqlCommand(objCommand);
        }

        /// <summary>
        /// Remove exported merge lists
        /// </summary>
        public static void RemoveExportedMergeList(string ListAccountIds)
        {
            SqlBuilder objSqlBuilder = new SqlBuilder();
            objSqlBuilder.CreateDeleteTable("list_accounts", null);
            objSqlBuilder.CreateWhere("id", ListAccountIds.ToString(), SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.In);
            string sqlDeleteListAccounts = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.Delete);

            objSqlBuilder = new SqlBuilder();
            objSqlBuilder.CreateDeleteTable("merge_list_list_accounts", null);
            objSqlBuilder.CreateWhere("list_account_id", ListAccountIds.ToString(), SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.In);
            string sqlDeleteMergeListListAccounts = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.Delete);

            // delete the list items in list_accounts table
            SqlCommand objSqlCommand = new SqlCommand(sqlDeleteListAccounts);
            objSqlCommand.CommandType = CommandType.Text;
            DatabaseUtility.ExecuteSqlCommand(objSqlCommand);

            // delete the list items in merge_list_list_accounts table
            objSqlCommand = new SqlCommand(sqlDeleteMergeListListAccounts);
            objSqlCommand.CommandType = CommandType.Text;
            DatabaseUtility.ExecuteSqlCommand(objSqlCommand);

            // delete lists that has no record items
            objSqlCommand = new SqlCommand("bvDeleteEmptyRecordLists_sp");
            objSqlCommand.CommandType = CommandType.StoredProcedure;
            DatabaseUtility.ExecuteSqlCommand(objSqlCommand);
        }

        /// <summary>
        /// Insert to list accounts
        /// </summary>
        public static void SaveListAccounts(int ListId, string AccountIds)
        {
            SqlBuilder objSqlBuilder = new SqlBuilder();
            objSqlBuilder.CreateSelect(ListId.ToString(), "list_id", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("id", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateFrom("accounts", null);
            objSqlBuilder.CreateWhere("id", AccountIds, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.In);
            string sqlAccounts = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.Select);

            objSqlBuilder.Clear();
            objSqlBuilder.CreateInsertTable("list_accounts");
            objSqlBuilder.CreateInsertIntoField("list_id");
            objSqlBuilder.CreateInsertIntoField("account_id");
            objSqlBuilder.CreateInsertIntoSelectionValue(sqlAccounts);
            string sqlInsertIntoListAccounts = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.InsertSelect);

            SqlCommand objCommand = new SqlCommand(sqlInsertIntoListAccounts);
            objCommand.CommandType = CommandType.Text;
            DatabaseUtility.ExecuteSqlCommand(objCommand);
        }

        /// <summary>
        /// Update list_accounts to exported
        /// </summary>
        public static void SaveExportedListAccounts(string ListAccountIds)
        {
            SqlBuilder objSqlBuilder = new SqlBuilder();
            objSqlBuilder.CreateUpdateTable("list_accounts", null);
            objSqlBuilder.CreateUpdateSet("exported", "1");
            objSqlBuilder.CreateWhere("id", ListAccountIds, SqlBuilder.eSqlConditionType.And, SqlBuilder.eSqlValueType.In);
            string sqlUpdateCommand = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.Update);

            SqlCommand objSqlCommand = new SqlCommand(sqlUpdateCommand);
            objSqlCommand.CommandType = CommandType.Text;
            DatabaseUtility.ExecuteSqlCommand(objSqlCommand);
        }
        #endregion
    }
}
