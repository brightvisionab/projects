using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using System.Data.Objects;
using System.Collections;
using System.Data;

namespace ManagerApplication.Business
{
    public class ObjectProfiledData
    {
        #region Classes
        /// <summary>
        /// Gets or sets profile data instance
        /// </summary>
        public class ProfileDataInstance
        {
            public int id { get; set; }
            public int import_file_id { get; set; }
            public int row_order { get; set; }
            public string account_company_name { get; set; }
            public string account_org_no { get; set; }
            public string account_box_address { get; set; }
            public string account_street_address { get; set; }
            public string account_zipcode { get; set; }
            public string account_city { get; set; }
            public string account_country { get; set; }
            public string account_telephone { get; set; }
            public string account_telefax { get; set; }
            public string account_www { get; set; }
            public string account_year_established { get; set; }
            public string account_activity_code { get; set; }
            //public string account_activity_code_description { get; set; }
            public string account_activity_code_2 { get; set; }
            //public string account_activity_code_2_description { get; set; }
            public decimal account_currency { get; set; }
            public decimal account_turnover { get; set; }
            public decimal account_result { get; set; }
            public int account_employees_abroad { get; set; }
            public int account_employees_total { get; set; }
            public string contact_first_name { get; set; }
            public string contact_middle_name { get; set; }
            public string contact_last_name { get; set; }
            public string contact_direct_phone { get; set; }
            public string contact_mobile { get; set; }
            public string contact_email { get; set; }
            public string contact_title { get; set; }
            public string contact_address_1 { get; set; }
            public string contact_address_2 { get; set; }
            public string contact_city { get; set; }
            public string contact_zipcode { get; set; }
            public string contact_country { get; set; }
        }
        #endregion

        #region Business Methods
        /// <summary>
        /// Save profiled data record
        /// </summary>
        //public static void SaveProfiledData(DataRowView objParams, BrightPlatformEntities m_objBrightPlatformEntity)
        //{
        //    profiled_data objProfiledData = new profiled_data()
        //    {
        //        import_file_id = Convert.ToInt32(objParams.Row[0]),
        //        row_order = Convert.ToInt32(objParams.Row[1]),
        //        account_company_name = Convert.ToString(objParams.Row[2]),
        //        account_org_no = Convert.ToString(objParams.Row[3]),
        //        account_box_address = Convert.ToString(objParams.Row[4]),
        //        account_street_address = Convert.ToString(objParams.Row[5]),
        //        account_zipcode = Convert.ToString(objParams.Row[6]),
        //        account_city = Convert.ToString(objParams.Row[7]),
        //        account_country = Convert.ToString(objParams.Row[8]),
        //        account_telephone = Convert.ToString(objParams.Row[9]),
        //        account_telefax = Convert.ToString(objParams.Row[10]),
        //        account_www = Convert.ToString(objParams.Row[11]),
        //        account_year_established = Convert.ToString(objParams.Row[12]),
        //        account_parent_company = Convert.ToString(objParams.Row[13]),
        //        account_activity_code = Convert.ToString(objParams.Row[14]),
        //        account_activity_code_description = Convert.ToString(objParams.Row[15]),
        //        account_activity_code_2 = Convert.ToString(objParams.Row[16]),
        //        account_activity_code_2_description = Convert.ToString(objParams.Row[17]),
        //        account_currency = objParams.Row[18].ToString() == string.Empty? 0: Convert.ToDecimal(objParams.Row[18]),
        //        account_revenue = objParams.Row[19].ToString() == string.Empty? 0: Convert.ToDecimal(objParams.Row[19]),
        //        account_result = objParams.Row[20].ToString() == string.Empty? 0: Convert.ToDecimal(objParams.Row[20]),
        //        account_employees_abroad = objParams.Row[21].ToString() == string.Empty? 0: Convert.ToInt32(objParams.Row[21]),
        //        account_employees_total = objParams.Row[22].ToString() == string.Empty? 0: Convert.ToInt32(objParams.Row[22]),
        //        contact_first_name = Convert.ToString(objParams.Row[23]),
        //        contact_middle_name = Convert.ToString(objParams.Row[24]),
        //        contact_last_name = Convert.ToString(objParams.Row[25]),
        //        contact_direct_phone = Convert.ToString(objParams.Row[26]),
        //        contact_mobile = Convert.ToString(objParams.Row[27]),
        //        contact_email = Convert.ToString(objParams.Row[28]),
        //        contact_title = Convert.ToString(objParams.Row[29]),
        //        contact_address_1 = Convert.ToString(objParams.Row[30]),
        //        contact_address_2 = Convert.ToString(objParams.Row[31]),
        //        contact_city = Convert.ToString(objParams.Row[32]),
        //        contact_zipcode = Convert.ToString(objParams.Row[33]),
        //        contact_country = Convert.ToString(objParams.Row[34])
        //    };

        //    m_objBrightPlatformEntity.profiled_data.AddObject(objProfiledData);
        //}
        #endregion
    }
}
