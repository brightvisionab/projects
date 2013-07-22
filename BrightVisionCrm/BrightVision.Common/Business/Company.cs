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

namespace BrightVision.Common.Business
{
    public class ObjectCompany
    {
        #region Enums
        /// <summary>
        /// Enum for view type use
        /// </summary>
        public enum eViewType
        {
            GeoMatchCompany
        }
        #endregion

        #region Classes
        /// <summary>
        /// Gets or sets company instance
        /// </summary>
        public class CompanyInstance
        {
            public int id { get; set; }
            public string company_name { get; set; }
            public string org_no { get; set; }
            public string box_address { get; set; }
            public string street_address { get; set; }
            public string zip_code { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string telephone { get; set; }
            public string telefax { get; set; }
            public string site { get; set; }
            public string year_established { get; set; }
            public string parent_company { get; set; }
            public string activity_code { get; set; }
            public string activity_code_2 { get; set; }
            public string currency { get; set; }
            public string fiscal { get; set; }
            //public decimal consolidated_figures { get; set; }
            public decimal turnover { get; set; }
            public decimal export { get; set; }
            public decimal result { get; set; }
            public decimal sales_abroad { get; set; }
            public int employees_abroad { get; set; }
            public int employees_total { get; set; }
            public bool validated { get; set; }
        }
        #endregion

        #region Business Methods
        /// <summary>
        /// Get company listing for adding new companies to an existing sub campaign
        /// </summary>
        /// <param name="KeyWord"></param>
        /// <param name="LimitNo"></param>
        /// <returns></returns>
        public static List<CTCompany> GetCompanyListing(string KeyWord, int LimitNo)
        {
            BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            return _efDbModel.FIGetCompanies(KeyWord, LimitNo).ToList();
        }

        /// <summary>
        /// Save the list of accounts from large company import
        /// </summary>
        /// <param name="lstAccounts"></param>
        /// <param name="_efDbModel"></param>
        //public static void SaveAccounts(List<account> lstAccounts, BrightPlatformEntities _efDbModel)
        //{
        //    account _efAccount = null;
        //    for (int x = 0; x < lstAccounts.Count; x++)
        //    {
        //        int _iAcctId = lstAccounts[x].id;
        //        _efAccount = null;
        //        _efAccount = _efDbModel.accounts.FirstOrDefault(i => i.id == _iAcctId);
        //        _efDbModel.accounts.ApplyCurrentValues(lstAccounts[x]);
        //    }

        //    _efDbModel.SaveChanges(false);
        //}

        /// <summary>
        /// Get account table column names
        /// </summary>
        /// <returns></returns>
        public static List<CTAccountColumnName> GetAccountColumnNames()
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            return objDbModel.FIGetAccountColumnNames().ToList();
        }

        /// <summary>
        /// Method to save company remarks
        /// </summary>
        /// <param name="SubCampignId"></param>
        /// <param name="CompanyId"></param>
        /// <param name="Remarks"></param>
        public static void SaveCompanyRemarks(int SubCampignId, int CompanyId, string Remarks)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            sub_campaign_account_remarks objCompanyRemark = objDbModel.sub_campaign_account_remarks.FirstOrDefault(i => i.sub_campaign_id == SubCampignId && i.account_id == CompanyId);

            if (objCompanyRemark != null)
            {
                objCompanyRemark.remarks = Remarks;
                objDbModel.SaveChanges();
            }
            else
            {
                if (!string.IsNullOrEmpty(Remarks))
                {
                    objCompanyRemark = null;
                    objCompanyRemark = new sub_campaign_account_remarks()
                    {
                        sub_campaign_id = SubCampignId,
                        account_id = CompanyId,
                        remarks = Remarks
                    };

                    objDbModel.sub_campaign_account_remarks.AddObject(objCompanyRemark);
                    objDbModel.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Method to get the company remark
        /// </summary>
        /// <param name="SubCampignId"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public static string GetCompanyRemarks(int SubCampignId, int CompanyId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            sub_campaign_account_remarks objCompanyRemark = objDbModel.sub_campaign_account_remarks.FirstOrDefault(i => i.sub_campaign_id == SubCampignId && i.account_id == CompanyId);
            if (objCompanyRemark != null)
                return objCompanyRemark.remarks;
            else
                return null;
        }

        /// <summary>
        /// Save company contact to table
        /// </summary>
        /// <param name="AccountId"></param>
        /// <param name="ContactId"></param>
        public static void SaveCompanyContact(int AccountId, int ContactId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            objDbModel.account_contacts.AddObject(new account_contacts() { account_id = AccountId, contact_id = ContactId });
            objDbModel.SaveChanges();

            //var objDuplicate = objDbModel.account_contacts.FirstOrDefault(i => i.account_id == AccountId && i.contact_id == ContactId);
            //if (objDuplicate == null)
            //{
            //    objDbModel.account_contacts.AddObject(new account_contacts() { account_id = AccountId, contact_id = ContactId });
            //    objDbModel.SaveChanges();
            //}
        }

        /// <summary>
        /// Get the company name via company id
        /// </summary>
        /// <returns>The company name as per company id parameter</returns>
        public static string GetCompanyName(int CompanyId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCompany = objDbModel.accounts.Where(i => i.id == CompanyId).SingleOrDefault();
            if (objCompany != null)
                return objCompany.company_name;
            else
                return string.Empty;
        }

        /// <summary>
        /// Get company details
        /// </summary>
        public static ObjectQuery GetCompanyDetails(int AccountId, BrightPlatformEntities objBrightPlatformEntity)
        {
            var objCompany = 
                from objItem in objBrightPlatformEntity.accounts
                where objItem.id == AccountId
                select objItem;

            return (ObjectQuery) objCompany;
        }

        /// <summary>
        /// De-activate a company
        /// </summary>
        public static void DeActivateCompany(int AccountId, string Reason)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCompany = objBrightPlatformEntity.accounts.FirstOrDefault(i => i.id == AccountId) as account;
            if (objCompany != null) {
                //objCompany.active = false;
                objCompany.de_activate_reason = Reason;
                objCompany.modified_by = UserSession.CurrentUser.UserId;
                objCompany.modified_date = DateTime.Now;
                objCompany.de_activate_by = UserSession.CurrentUser.UserId; ;
                objCompany.de_activate_date = DateTime.Now;
                objBrightPlatformEntity.SaveChanges();
            }
        }

        /// <summary>
        /// Remove sub campaign account
        /// </summary>
        public static void RemoveAccountFromSubCampaign(int AccountId)
        {
            using (BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objBrightPlatformEntity.FIDeActivateSubCampaignAccounts(AccountId);
            }
            //var objSubCampaignCompany = objBrightPlatformEntity.sub_campaign_account_lists.Where(i => i.final_list_id == FinalListId && i.account_id == AccountId).SingleOrDefault();
            //if (objSubCampaignCompany != null) {
            //    objBrightPlatformEntity.sub_campaign_account_lists.DeleteObject(objSubCampaignCompany);
            //    objBrightPlatformEntity.SaveChanges();
            //}
        }

        /// <summary>
        /// Set sql command setting for account contacts
        /// </summary>
        public static DataTable GetAccountContacts(int AccountId)
        {
            SqlCommand objCommand = new SqlCommand("bvScGetAccountContactList_sp");
            objCommand.Parameters.Add("@p_account_id", SqlDbType.Int).Value = AccountId;
            return DatabaseUtility.ExecuteSqlQuery(objCommand);
        }

        /// <summary>
        /// Get page count and total no of records
        /// </summary>
        public static CTCompanyGeoDataCount GetCompanyGeoDataPageCount(string sqlFilterText)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            return objBrightPlatformEntity.FIGetCompanyGeoDataCount(DatabaseUtility.LargeDatasetFetchLimit, sqlFilterText).FirstOrDefault();
        }

        /// <summary>
        /// Get page count and total no of records
        /// </summary>
        public static CTCompanyCount GetCompanyPageCount(string sqlFilterText)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            return objBrightPlatformEntity.FIGetCompanyCount(DatabaseUtility.LargeDatasetFetchLimit, sqlFilterText).FirstOrDefault();
        }

        /// <summary>
        /// Get company categories
        /// </summary>
        public static ObjectQuery GetCompanyCategories()
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCategories =
                (
                    from objCategory in m_objBrightPlatformEntity.accounts
                    orderby objCategory.category
                    select new 
                    {
                        category = objCategory.category
                    }
                ).Distinct();

            return (ObjectQuery) objCategories;
        }

        /// <summary>
        /// Save companies record
        /// </summary>
        public static void SaveCompanies(List<CTCompanyList> aCompanyList, string pLastModifiedSource)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            foreach (CTCompanyList iCompany in aCompanyList)
            {
                if (iCompany == null)
                    return;

                var objCompany = m_objBrightPlatformEntity.accounts.Where(objRow => objRow.id == iCompany.id).SingleOrDefault();
                objCompany.company_name = iCompany.company_name;
                objCompany.org_no = iCompany.org_no;
                objCompany.box_address = iCompany.box_address;
                objCompany.street_address = iCompany.street_address;
                objCompany.zipcode = iCompany.zip_code;
                objCompany.city = iCompany.city;
                objCompany.country = iCompany.country;
                objCompany.telephone = iCompany.telephone;
                objCompany.telefax = iCompany.telefax;
                objCompany.www = iCompany.site;
                objCompany.year_established = iCompany.year_established;
                objCompany.parent_company = iCompany.parent_company;
                objCompany.activity_code = iCompany.activity_code;
                //objCompany.activity_code_description = iCompany.activity_code_description;
                objCompany.activity_code_2 = iCompany.activity_code_2;
                //objCompany.activity_code2_description = iCompany.activity_code_2_description;
                objCompany.currency = iCompany.currency;
                objCompany.fiscal = iCompany.fiscal;
                //objCompany.consolidated_figures = iCompany.consolidated_figures;
                objCompany.turnover = iCompany.turnover;
                objCompany.export = iCompany.export;
                objCompany.result = iCompany.result;
                objCompany.sales_abroad = iCompany.sales_abroad;
                objCompany.employees_abroad = iCompany.employees_abroad;
                objCompany.employees_total = iCompany.employees_total;
                //objCompany.net_interest_incom = iCompany.net_interest_income;
                //objCompany.gross_premiums = iCompany.gross_premiums;
                objCompany.modified_by = UserSession.CurrentUser.UserId;
                objCompany.modified_date = DateTime.Now;
                objCompany.validated = true;
                objCompany.active = iCompany.active;
                objCompany.last_modified_machine = UserSession.CurrentUser.ComputerName;
                objCompany.last_modified_source = pLastModifiedSource;
                m_objBrightPlatformEntity.SaveChanges();

                //if (iCompany.activity_code.Length > 0)
                //    SaveActivityCode(iCompany.activity_code, iCompany.activity_code_description);
            }
        }

        /// <summary>
        /// Saves the company records
        /// </summary>
        public static void SaveCompany(CompanyInstance objParams)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            account objCompany = new account()
            {
                company_name = objParams.company_name,
                org_no = objParams.org_no,
                box_address = objParams.box_address,
                street_address = objParams.street_address,
                zipcode = objParams.zip_code,
                city = objParams.city,
                country = objParams.country,
                telephone = objParams.telephone,
                telefax = objParams.telefax,
                www = objParams.site,
                year_established = objParams.year_established,
                parent_company = objParams.parent_company,
                activity_code = objParams.activity_code,
                //activity_code_description = objParams.activity_code_description,
                activity_code_2 = objParams.activity_code_2,
                //activity_code2_description = objParams.activity_code_2_description,
                currency = objParams.currency,
                fiscal = objParams.fiscal,
                //consolidated_figures = objParams.consolidated_figures,
                turnover = objParams.turnover,
                export = objParams.export,
                result = objParams.result,
                sales_abroad = objParams.sales_abroad,
                employees_abroad = objParams.employees_abroad,
                employees_total = objParams.employees_total,
                //net_interest_incom = objParams.net_interest_income,
                //gross_premiums = objParams.gross_premiums,
                validated = true,
                created_date = DateTime.Now,
                created_by = UserSession.CurrentUser.UserId
            };

            m_objBrightPlatformEntity.accounts.AddObject(objCompany);
            m_objBrightPlatformEntity.SaveChanges();

            //if (objParams.activity_code.Length > 0)
            //    SaveActivityCode(objParams.activity_code, objParams.activity_code_description);
        }

        /// <summary>
        /// Gets the company records
        /// </summary>
        public static ObjectResult GetCompanies(int FetchLimit, string FilterCriteria, int PageNo)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            m_objBrightPlatformEntity.CommandTimeout = 0;
            return m_objBrightPlatformEntity.FIGetCompanyLists(FetchLimit, FilterCriteria, PageNo);
        }

        /// <summary>
        /// Gets the geo matched company records
        /// </summary>
        public static ObjectResult GetGeoMatchedCompanies(string FilterCriteria, int PageNo)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            return m_objBrightPlatformEntity.FIGetGeographicalDataCompany(DatabaseUtility.LargeDatasetFetchLimit, FilterCriteria, PageNo);
        }

        /// <summary>
        /// Get activity codes
        /// </summary>
        public static DataTable GetActivityCodes()
        {
            SqlBuilder objSqlBuilder = new SqlBuilder();
            objSqlBuilder.CreateSelect("code", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("description", null, SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("CAST(0 AS bit)", "use_item", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateWhere("LEN(code) > 0", SqlBuilder.eSqlConditionType.And);
            objSqlBuilder.CreateFrom("activity_codes", null);
            objSqlBuilder.CreateOrderBy("code");
            string sqlSelectCommand = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.Select);

            return DatabaseUtility.ExecuteSqlQuery(sqlSelectCommand);
        }

        /// <summary>
        /// Save activity code
        /// </summary>
        public static void SaveActivityCode(string Code, string Description)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objActivityCode = m_objBrightPlatformEntity.activity_codes.Where(objEntity => objEntity.code == Code).SingleOrDefault();

            if (objActivityCode == null)
            {
                activity_codes objItem = new activity_codes()
                {
                    code = Code,
                    description = Description
                };

                m_objBrightPlatformEntity.activity_codes.AddObject(objItem);
                m_objBrightPlatformEntity.SaveChanges();
            }
        }
        #endregion
    }
}
