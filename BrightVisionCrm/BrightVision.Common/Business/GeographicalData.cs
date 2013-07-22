using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using System.Data.Objects;
using BrightVision.Common.Utilities;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace BrightVision.Common.Business
{
    public class ObjectGeographicalData
    {
        /**
         * [@jeff 05.04.2011]
         * 
         * where:
         * table_source = is the table name source (e.g. accounts, contacts)
         * table_id = is the table record id of the source
         * 
         * constant values for table source:
         * 0 = accounts
         * 1 = contacts
         */

        #region Enums
        /// <summary>
        /// Enum for table sources
        /// </summary>
        public enum eTableSource
        {
            Companies = 0,
            Contacts = 1       
        }
        #endregion

        #region Classes
        /// <summary>
        /// Gets or sets the accounts geographical data instance
        /// </summary>
        public class GeoDataInstance
        {
            public int id { get; set; }
            public int table_source { get; set; }
            public int table_id { get; set; }
            public decimal latitude { get; set; }
            public decimal longitude { get; set; }
            public string status { get; set; }
        }
        #endregion

        #region Business Methods
        /// <summary>
        /// Execute stored procedure to save geo data
        /// </summary>
        private static void ExecuteSaveProcedure(GeoDataInstance objParams, SqlConnection SqlConn)
        {
            SqlCommand objCommand = new SqlCommand("bvSaveGeographicalData_sp", SqlConn);
            objCommand.CommandType = CommandType.StoredProcedure;
            objCommand.Parameters.Add("@p_table_source", SqlDbType.Int).Value = objParams.table_source;
            objCommand.Parameters.Add("@p_table_id", SqlDbType.Int).Value = objParams.table_id;
            objCommand.Parameters.Add("@p_latitude", SqlDbType.Decimal).Value = objParams.latitude;
            objCommand.Parameters.Add("@p_longitude", SqlDbType.Decimal).Value = objParams.longitude;
            objCommand.Parameters.Add("@p_user_id", SqlDbType.Int).Value = UserSession.CurrentUser.UserId;
            objCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Save geographical data
        /// </summary>
        public static void SaveGeoData(GeoDataInstance objParams, SqlConnection SqlConn)
        {
            ExecuteSaveProcedure(objParams, SqlConn);
        }

        /// <summary>
        /// Get sub campaign accounts geo data
        /// </summary>
        public static ObjectResult GetSubCampaignAccountGeoData(int SubCampaignId)
        {
            BrightPlatformEntities objDatabaseModel = new BrightPlatformEntities(UserSession.EntityConnection);
            return objDatabaseModel.FIScGetSubCampaignCompanyListGeoMapViewer(SubCampaignId);
        }
        #endregion
    }
}
