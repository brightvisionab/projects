using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightVision.Common.Utilities
{
    public class SqlBuilder
    {
        #region Enumerations
        /// <summary>
        /// Sql select aggregate function type
        /// </summary>
        public enum eSqlSelectAggregateType
        {
            None,
            Distinct
        }

        /// <summary>
        /// Sql join type enumerator
        /// </summary>
        public enum eSqlJoinType
        {
            LeftJoin,
            InnerJoin
        }

        /// <summary>
        /// Sql where type enumerator
        /// </summary>
        public enum eSqlConditionType
        {
            And,
            Or
        }

        /// <summary>
        /// Sql value type enumerator
        /// </summary>
        public enum eSqlValueType
        {
            Int,
            String,
            In
        }

        /// <summary>
        /// Sql transaction type
        /// </summary>
        public enum eSqlTransactionType
        {
            Select,
            Update,
            Delete,
            InsertSelect
        }
        #endregion

        #region Constructors
        public SqlBuilder()
        {
            this.Clear();
        }
        #endregion

        #region Private Members
        private StringBuilder m_sqlInsertTable = null;
        private StringBuilder m_sqlInsertField = null;
        private StringBuilder m_sqlInsertSelectionValue = null;
        private StringBuilder m_sqlDelete = null;
        private StringBuilder m_sqlDeleteTable = null;
        private StringBuilder m_sqlUpdate = null;
        private StringBuilder m_sqlUpdateTable = null;
        private StringBuilder m_sqlUpdateSet = null;
        private StringBuilder m_sqlSelect = null;
        private StringBuilder m_sqlFrom = null;
        private StringBuilder m_sqlJoin = null;
        private StringBuilder m_sqlWhere = null;
        private StringBuilder m_sqlOrderBy = null;
        private StringBuilder m_sqlGroupBy = null;
        private StringBuilder m_sqlCommand = null;
        #endregion

        #region Business Methods
        /// <summary>
        /// Clear sql command
        /// </summary>
        public void Clear()
        {
            m_sqlInsertTable = new StringBuilder();
            m_sqlInsertField = new StringBuilder();
            m_sqlInsertSelectionValue = new StringBuilder();
            m_sqlDelete = new StringBuilder();
            m_sqlDeleteTable = new StringBuilder();
            m_sqlUpdate = new StringBuilder();
            m_sqlUpdateTable = new StringBuilder();
            m_sqlUpdateSet = new StringBuilder();
            m_sqlSelect = new StringBuilder();
            m_sqlFrom = new StringBuilder();
            m_sqlJoin = new StringBuilder();
            m_sqlWhere = new StringBuilder();
            m_sqlOrderBy = new StringBuilder();
            m_sqlCommand = new StringBuilder();
            m_sqlGroupBy = new StringBuilder();
        }

        /// <summary>
        /// Build sql query
        /// </summary>
        public string BuildSqlQuery(eSqlTransactionType TransactionType)
        {
            switch (TransactionType)
            {
                case eSqlTransactionType.Select:
                {
                    m_sqlCommand = new StringBuilder();
                    m_sqlCommand.AppendFormat("SELECT {0} ", m_sqlSelect);
                    m_sqlCommand.AppendFormat("FROM {0} ", m_sqlFrom);

                    if (m_sqlJoin.Length > 0) 
                        m_sqlCommand.AppendFormat("{0} ", m_sqlJoin);

                    if (m_sqlWhere.Length > 0)
                        m_sqlCommand.AppendFormat("{0} ", m_sqlWhere);

                    m_sqlCommand.Append(m_sqlOrderBy);
                    break;
                }

                case eSqlTransactionType.Update:
                {
                    m_sqlCommand = new StringBuilder();
                    m_sqlCommand.AppendFormat("UPDATE {0} ", m_sqlUpdateTable);
                    m_sqlCommand.AppendFormat("SET {0} ", m_sqlUpdateSet);

                    if (m_sqlJoin.Length > 0)
                        m_sqlCommand.AppendFormat("{0} ", m_sqlJoin);

                    if (m_sqlWhere.Length > 0)
                        m_sqlCommand.AppendFormat("{0}", m_sqlWhere);

                    break;
                }

                case eSqlTransactionType.Delete:
                {
                    m_sqlCommand = new StringBuilder();
                    m_sqlCommand.AppendFormat("DELETE FROM {0} ", m_sqlDeleteTable);

                    if (m_sqlJoin.Length > 0)
                        m_sqlCommand.AppendFormat("{0} ", m_sqlJoin);

                    if (m_sqlWhere.Length > 0)
                        m_sqlCommand.AppendFormat("{0}", m_sqlWhere);

                    break;
                }

                case eSqlTransactionType.InsertSelect:
                {
                    m_sqlCommand = new StringBuilder();
                    m_sqlCommand.AppendFormat("INSERT INTO {0}({1}) {2}", m_sqlInsertTable, m_sqlInsertField, m_sqlInsertSelectionValue);
                    break;
                }
            }

            return m_sqlCommand.ToString();
        }

        /// <summary>
        /// Create SET sql line
        /// </summary>
        public void CreateUpdateSet(string sqlFieldName, string sqlFieldValue)
        {
            if (m_sqlUpdateSet.Length > 0)
                m_sqlUpdateSet.Append(", ");

            m_sqlUpdateSet.AppendFormat("{0} = {1}", sqlFieldName, sqlFieldValue);
        }

        /// <summary>
        /// Create INSERT INTO table
        /// </summary>
        public void CreateInsertTable(string sqlTableName)
        {
            m_sqlInsertTable.Append(sqlTableName);
        }

        /// <summary>
        /// Create INSERT INTO field sets
        /// </summary>
        public void CreateInsertIntoField(string sqlInsertFieldName)
        {
            m_sqlInsertField.AppendFormat("{0}", m_sqlInsertField.Length > 0? "," + sqlInsertFieldName: sqlInsertFieldName);
        }

        /// <summary>
        /// Create INSERT INTO value sets, using a pre-defined sql select statement
        /// </summary>
        public void CreateInsertIntoSelectionValue(string sqlStatement)
        {
            m_sqlInsertSelectionValue.Append(sqlStatement);
        }

        /// <summary>
        /// Create UPDATE table source 
        /// </summary>
        public void CreateDeleteTable(string sqlTableName, string sqlAlias)
        {
            m_sqlDeleteTable.Append(sqlTableName);

            if (sqlAlias != null && sqlAlias.Length > 0)
                m_sqlDeleteTable.Append(" AS " + sqlAlias);
        }

        /// <summary>
        /// Create UPDATE table source 
        /// </summary>
        public void CreateUpdateTable(string sqlTableName, string sqlAlias)
        {
            m_sqlUpdateTable.Append(sqlTableName);

            if (sqlAlias != null && sqlAlias.Length > 0)
                m_sqlUpdateTable.Append(" AS " + sqlAlias);
        }

        /// <summary>
        /// Create SELECT query line
        /// </summary>
        public void CreateSelect(string sqlFieldName, string sqlAlias, eSqlSelectAggregateType AggregateType)
        {
            if (m_sqlSelect.Length > 0)
                m_sqlSelect.Append(", ");

            if (sqlAlias != null && sqlAlias.Length > 0)
                m_sqlSelect.Append(sqlAlias + " = ");

            switch (AggregateType)
            {
                case eSqlSelectAggregateType.None:
                {
                    m_sqlSelect.Append(sqlFieldName);
                    break;
                }

                case eSqlSelectAggregateType.Distinct:
                {
                    m_sqlSelect.Append("DISTINCT " + sqlFieldName);
                    break;
                }
            }
        }

        /// <summary>
        /// Create FROM line
        /// </summary>
        public void CreateFrom(string sqlTableName, string sqlAlias)
        {
            m_sqlFrom.Append(sqlTableName);

            if (sqlAlias != null && sqlAlias.Length > 0)
                m_sqlFrom.Append(" AS " + sqlAlias);
        }

        /// <summary>
        /// Create JOIN line
        /// </summary>
        public void CreateJoin(string sqlTableName, string sqlTableAlias, string sqlTableCondition, eSqlJoinType sqlJoinType)
        {
            if (m_sqlJoin.Length > 0)
                m_sqlJoin.Append(" ");

            switch (sqlJoinType)
            {
                case eSqlJoinType.InnerJoin:
                {
                    m_sqlJoin.Append("INNER JOIN " + sqlTableName + (sqlTableAlias == null ? " ON " : " " + sqlTableAlias + " ON ") + sqlTableCondition);
                    break;
                }

                case eSqlJoinType.LeftJoin:
                {
                    m_sqlJoin.Append("LEFT JOIN " + sqlTableName + (sqlTableAlias == null ? " ON " : " " + sqlTableAlias + " ON ") + sqlTableCondition);
                    break;
                }
            }
        }

        /// <summary>
        /// Create JOIN line
        /// </summary>
        public void CreateDerivedTableJoin(string sqlDerivedTable, string sqlTableAlias, string sqlTableCondition, eSqlJoinType sqlJoinType)
        {
            if (m_sqlJoin.Length > 0)
                m_sqlJoin.Append(" ");

            switch (sqlJoinType)
            {
                case eSqlJoinType.InnerJoin:
                    {
                        m_sqlJoin.Append("INNER JOIN (" + sqlDerivedTable + ")" +  (sqlTableAlias == null ? " ON " : " " + sqlTableAlias + " ON ") + sqlTableCondition);
                        break;
                    }

                case eSqlJoinType.LeftJoin:
                    {
                        m_sqlJoin.Append("LEFT JOIN (" + sqlDerivedTable + ")" + (sqlTableAlias == null ? " ON " : " " + sqlTableAlias + " ON ") + sqlTableCondition);
                        break;
                    }
            }
        }

        /// <summary>
        /// Create WHERE line
        /// </summary>
        public void CreateWhere(string sqlStatement, eSqlConditionType ConditionType)
        {
            if (m_sqlWhere.Length > 0)
                m_sqlWhere.Append(" ");

            switch (ConditionType)
            {
                case eSqlConditionType.And:
                {
                    if (m_sqlWhere.Length < 1)
                        m_sqlWhere.Append("WHERE " + sqlStatement);
                    else
                        m_sqlWhere.Append("AND " + sqlStatement);

                    break;
                }

                case eSqlConditionType.Or:
                {
                    if (m_sqlWhere.Length < 1)
                        m_sqlWhere.Append("WHERE " + sqlStatement);
                    else
                        m_sqlWhere.Append("OR " + sqlStatement);

                    break;
                }
            }
        }

        /// <summary>
        /// Create WHERE line
        /// </summary>
        public void CreateWhere(string sqlFieldName, string sqlFieldValue, eSqlConditionType ConditionType, eSqlValueType ValueType)
        {
            if (m_sqlWhere.Length > 0)
                m_sqlWhere.Append(" ");
            
            switch (ConditionType)
            {
                case eSqlConditionType.And:
                {
                    if (m_sqlWhere.Length < 1)
                        m_sqlWhere.Append("WHERE " + sqlFieldName);
                    else
                        m_sqlWhere.Append("AND " + sqlFieldName);

                    break;
                }

                case eSqlConditionType.Or:
                {
                    if (m_sqlWhere.Length < 1)
                        m_sqlWhere.Append("WHERE " + sqlFieldName);
                    else
                        m_sqlWhere.Append("OR " + sqlFieldName);

                    break;
                }
            }

            if (sqlFieldValue.Length > 0)
            {
                if (ValueType == eSqlValueType.String)
                    m_sqlWhere.Append(" LIKE '%" + sqlFieldValue + "%'");

                else if (ValueType == eSqlValueType.Int)
                    m_sqlWhere.Append(" = " + sqlFieldValue);

                else if (ValueType == eSqlValueType.In)
                    m_sqlWhere.Append(" IN (" + sqlFieldValue + ")");
            }
        }

        /// <summary>
        /// Create WHERE line
        /// </summary>
        public void CreateWhere(string sqlFieldName, string MinValue, string MaxValue, eSqlConditionType ConditionType)
        {
            if (m_sqlWhere.Length > 0)
                m_sqlWhere.Append(" ");

            string sqlCondition = string.Empty;
            if (Convert.ToInt32(MaxValue) == 0)
                sqlCondition = sqlFieldName + " >= " + MinValue;
            else
                sqlCondition = "(" + sqlFieldName + " BETWEEN " + MinValue + " AND " + MaxValue + ")";

            switch (ConditionType)
            {
                case eSqlConditionType.And:
                {
                    if (m_sqlWhere.Length < 1)
                        m_sqlWhere.Append("WHERE " + sqlCondition);
                    else
                        m_sqlWhere.Append("AND " + sqlCondition);

                    break;
                }

                case eSqlConditionType.Or:
                {
                    if (m_sqlWhere.Length < 1)
                        m_sqlWhere.Append("WHERE " + sqlCondition);
                    else
                        m_sqlWhere.Append("OR " + sqlCondition);

                    break;
                }
            }
        }

        /// <summary>
        /// Create ORDER BY line
        /// </summary>
        public void CreateOrderBy(string sqlFieldToSort)
        {
            if (m_sqlOrderBy.Length > 0)
                m_sqlOrderBy.Append(", ");
            else
                m_sqlOrderBy.Append("ORDER BY ");

            m_sqlOrderBy.Append(sqlFieldToSort);
        }

        /// <summary>
        /// Create GROUP BY line
        /// </summary>
        public void CreateGroupBy(string sqlField)
        {
            if (m_sqlGroupBy.Length > 0)
                m_sqlGroupBy.Append(", ");
            else
                m_sqlGroupBy.Append("GROUP BY ");

            m_sqlGroupBy.Append(sqlField);
        }
        #endregion
    }
}
