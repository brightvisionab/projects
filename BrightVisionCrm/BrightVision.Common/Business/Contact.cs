using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.Data.Objects;
using System.Data;

namespace BrightVision.Common.Business
{
    public class ObjectContact
    {
        #region Classes
        /// <summary>
        /// Gets or sets contact instance
        /// </summary>
        public class ContactInstance
        {
            public int id { get; set; }
            public string first_name { get; set; }
            public string middle_name { get; set; }
            public string last_name { get; set; }
            public string direct_phone { get; set; }
            public string mobile { get; set; }
            public string email { get; set; }
            public string title { get; set; }
            public string company_name { get; set; }
            public string address_1 { get; set; }
            public string address_2 { get; set; }
            public string city { get; set; }
            public string zip_code { get; set; }
            public string country { get; set; }
            public bool active { get; set; }
            public DateTime date_created { get; set; }
            public DateTime date_modified { get; set; }
        }
        #endregion

        #region Business Methods
        public static IList<CTContactColumnName> GetContactColumnNames()
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            return objDbModel.FIGetContactColumnNames().ToList();
        }

        /// <summary>
        /// Save non-existing contact title
        /// </summary>
        /// <param name="ContactTitle"></param>
        public static void SaveContactTitle(string ContactTitle)
        {
            //BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            //var _ContactTitle = objDbModel.titles.FirstOrDefault(i => i.code.Trim().Equals(ContactTitle)) as titles;
            //if (_ContactTitle == null)
            //{
            //    titles item = new titles()
            //    {
            //        language_id = 1, // swedish as default language
            //        code = ContactTitle,
            //        description = ContactTitle
            //    };

            //    objDbModel.titles.AddObject(item);
            //    objDbModel.SaveChanges();
            //}
        }

        /// <summary>
        /// Get the contact data object
        /// </summary>
        /// <param name="Firstname"></param>
        /// <param name="Lastname"></param>
        /// <param name="AccountId"></param>
        /// <returns></returns>
        public static contact GetContact(string Firstname, string Lastname, int AccountId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objContactPerson = (
                from objContact in objDbModel.contacts
                join objAccountContact in objDbModel.account_contacts on objContact.id equals objAccountContact.contact_id
                where objContact.first_name.Equals(Firstname) && objContact.last_name.Equals(Lastname) && objAccountContact.account_id == AccountId
                select objContact
            ).FirstOrDefault();

            if (objContactPerson != null)
                objDbModel.Detach(objContactPerson);

            return objContactPerson;
        }

        /// <summary>
        /// Get contact id of a particular contact
        /// </summary>
        /// <param name="Firstname"></param>
        /// <param name="Lastname"></param>
        /// <param name="AccountId"></param>
        /// <returns></returns>
        public static int GetContactId(string Firstname, string Lastname, int AccountId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objDuplicates =
            (
                from objContact in objDbModel.contacts
                join objAccountContact in objDbModel.account_contacts on objContact.id equals objAccountContact.contact_id
                where objContact.first_name.Equals(Firstname) && objContact.last_name.Equals(Lastname) && objAccountContact.account_id == AccountId
                select objContact

            ).FirstOrDefault();

            if (objDuplicates == null)
                return 0;
            else
                return objDuplicates.id;
        }

        //public static void SaveContacts(List<contact> lstContacts, BrightPlatformEntities _efDbModel)
        //{
        //    contact _efContact= null;
        //    for (int x = 0; x < lstContacts.Count; x++)
        //    {
        //        int _iContactId = lstContacts[x].id;
        //        _efContact = null;
        //        _efContact = _efDbModel.contacts.FirstOrDefault(i => i.id == _iContactId);
        //        _efDbModel.contacts.ApplyCurrentValues(lstContacts[x]);
        //    }

        //    _efDbModel.SaveChanges(false);
        //}

        /// <summary>
        /// Save contact to table
        /// </summary>
        /// <param name="ContactPerson"></param>
        /// <returns></returns>
        public static int SaveContact(contact ContactPerson, bool IsEditMode, int ContactId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            if (!IsEditMode)
            {
                if (ContactPerson.id < 1)
                {
                    objDbModel.contacts.AddObject(ContactPerson);
                    objDbModel.SaveChanges();
                }

                return ContactPerson.id;
            }
            else
            {
                if (ContactPerson.id > 0)
                    ContactId = ContactPerson.id;
                else
                    ContactPerson.id = ContactId;

                objDbModel.contacts.Single(i => i.id == ContactId);
                objDbModel.contacts.ApplyCurrentValues(ContactPerson);
                objDbModel.SaveChanges();
                return ContactId;
            }
        }

        /// <summary>
        /// To check if a contact already exists
        /// </summary>
        /// <param name="Firstname"></param>
        /// <param name="Lastname"></param>
        /// <param name="AccountId"></param>
        /// <returns></returns>
        public static bool Exists(string Firstname, string Lastname, int AccountId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objDuplicates =
            (
                from objContact in objDbModel.contacts
                join objAccountContact in objDbModel.account_contacts on objContact.id equals objAccountContact.contact_id
                where objContact.first_name.Equals(Firstname) && objContact.last_name.Equals(Lastname) && objAccountContact.account_id == AccountId
                select objContact

            ).FirstOrDefault();

            if (objDuplicates == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Get the contact name via contact id
        /// </summary>
        /// <returns>The contact name as per contact id parameter</returns>
        public static string GetContactName(int ContactId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objContact = objDbModel.contacts.Where(i => i.id == ContactId).SingleOrDefault();
            if (objContact != null)
                return objContact.first_name + " " + objContact.last_name;
            else
                return string.Empty;
        }

        /// <summary>
        /// Get contact details
        /// </summary>
        public static ObjectQuery GetContactDetails(int ContactId, BrightPlatformEntities _efDbModel)
        {
            //BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objContact =
                from objItem in _efDbModel.contacts
                where objItem.id == ContactId
                select objItem;

            return (ObjectQuery) objContact;
        }

        /// <summary>
        /// De-activate a contact
        /// </summary>
        public static void DeActivateContact(int ContactId)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objContact = objBrightPlatformEntity.contacts.Where(i => i.id == ContactId).Single();
            objContact.active = false;
            objContact.modified_date = DateTime.Now;
            objContact.modified_by = UserSession.CurrentUser.UserId;
            objBrightPlatformEntity.SaveChanges();
        }

        /// <summary>
        /// Re-activate a contact
        /// </summary>
        public static void ActivateContact(int ContactId)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objContact = objBrightPlatformEntity.contacts.Where(i => i.id == ContactId).Single();
            objContact.active = true;
            objContact.modified_date = DateTime.Now;
            objContact.modified_by = UserSession.CurrentUser.UserId;
            objBrightPlatformEntity.SaveChanges();
        }

        /// <summary>
        /// Remove sub campaign account
        /// </summary>
        public static void RemoveContactFromSubCampaign(int ContactId, int FinalListId)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSubCampaignContact = objBrightPlatformEntity.sub_campaign_contact_lists.Where(i => i.final_list_id == FinalListId && i.contact_id == ContactId).SingleOrDefault();
            if (objSubCampaignContact != null)
            {
                objBrightPlatformEntity.sub_campaign_contact_lists.DeleteObject(objSubCampaignContact);
                objBrightPlatformEntity.SaveChanges();
            }
        }

        /// <summary>
        /// Get page count and total no of records
        /// </summary>
        public static CTContactGeoDataCount GetContactGeoDataPageCount(string sqlFilterText)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            return objBrightPlatformEntity.FIGetContactGeoDataCount(DatabaseUtility.LargeDatasetFetchLimit, sqlFilterText).FirstOrDefault();
        }

        /// <summary>
        /// Save contact record
        /// </summary>
        public static void SaveContacts(List<CTContact> aContactList, int pContactId, string pModifiedSource)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            foreach (CTContact iContact in aContactList) {
                if (iContact == null)
                    continue;

                m_objBrightPlatformEntity.FIUpdateContactDetails (
                    iContact.id,
                    iContact.first_name,
                    iContact.middle_name,
                    iContact.last_name,
                    iContact.direct_phone,
                    iContact.mobile,
                    null,
                    iContact.title,
                    iContact.role_tags,
                    iContact.address_1,
                    iContact.address_2,
                    iContact.city,
                    iContact.zipcode,
                    iContact.country,
                    "", 
                    UserSession.CurrentUser.UserId,
                    iContact.email,
                    iContact.linkedin_url,
                    null, 
                    true,
                    pContactId, 
                    null,
                    UserSession.CurrentUser.ComputerName,
                    pModifiedSource,
                    iContact.absence_start,
                    iContact.absence_end,
                    null,
                    null
                );

                //var objContact = m_objBrightPlatformEntity.contacts.Where(objRow => objRow.id == iContact.id).SingleOrDefault();
                //objContact.first_name = iContact.first_name;
                //objContact.middle_name = iContact.middle_name;
                //objContact.last_name = iContact.last_name;
                //objContact.direct_phone = iContact.direct_phone;
                //objContact.mobile = iContact.mobile;
                //objContact.email = iContact.email;
                //objContact.title = iContact.title;
                ////objContact.company_name = iContact.company_name;
                //objContact.address_1 = iContact.address_1;
                //objContact.address_2 = iContact.address_2;
                //objContact.city = iContact.city;
                //objContact.zipcode = iContact.zipcode;
                //objContact.country = iContact.country;
                //objContact.active = iContact.active;
                //m_objBrightPlatformEntity.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the contact records
        /// </summary>
        public static List<CTContact> GetContacts(int AccountId)
        {
            BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            return _efDbModel.FIGetContacts(AccountId).ToList();

            //var objContacts =
            //    from objContact in m_objBrightPlatformEntity.contacts
            //    join objAccountContact in m_objBrightPlatformEntity.account_contacts on objContact.id equals objAccountContact.contact_id
            //    join objAccount in m_objBrightPlatformEntity.accounts on objAccountContact.account_id equals objAccount.id
            //    //where objContact.account_id == AccountId
            //    where objAccountContact.account_id == AccountId
            //    orderby objContact.first_name, objContact.last_name
            //    select new ContactInstance
            //    {
            //        id = objContact.id,
            //        first_name = objContact.first_name,
            //        middle_name = objContact.middle_name,
            //        last_name = objContact.last_name,
            //        direct_phone = objContact.direct_phone,
            //        mobile = objContact.mobile,
            //        email = objContact.email,
            //        title = objContact.title,
            //        company_name = objAccount.company_name,
            //        address_1 = objContact.address_1,
            //        address_2 = objContact.address_2,
            //        city = objContact.city,
            //        zip_code = objContact.zipcode,
            //        country = objContact.country,
            //        active = objContact.active,
            //        date_created = objContact.created_date == null? DateTime.Now :(DateTime)objContact.created_date,
            //        date_modified = objContact.modified_date == null? DateTime.Now : (DateTime)objContact.modified_date
            //    };

            //return (ObjectQuery) objContacts;
        }

        /// <summary>
        /// Gets the geo matched contact records
        /// </summary>
        public static ObjectResult GetGeoMatchedContacts(string FilterCriteria, int PageNo)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            ObjectResult objContacts = null;
            objContacts = m_objBrightPlatformEntity.FIGetGeographicalDataContact(DatabaseUtility.LargeDatasetFetchLimit, FilterCriteria, PageNo);

            return objContacts;
        }

        /// <summary>
        /// Gets the the contact information for use in list creation filter conditionals
        /// </summary>
        public static DataTable GetContactDetails()
        {
            SqlBuilder objSqlBuilder = new SqlBuilder();
            objSqlBuilder.CreateSelect("title", null, SqlBuilder.eSqlSelectAggregateType.Distinct);
            objSqlBuilder.CreateSelect("CAST(0 AS bit)", "use_item", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("CAST(0 AS bit)", "with_email", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("CAST(0 AS bit)", "with_direct_phone", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateSelect("CAST(0 AS bit)", "with_mobile", SqlBuilder.eSqlSelectAggregateType.None);
            objSqlBuilder.CreateFrom("contacts", null);
            objSqlBuilder.CreateWhere("LEN(title) > 0", SqlBuilder.eSqlConditionType.And);
            objSqlBuilder.CreateOrderBy("title");
            string sqlSelectCommand = objSqlBuilder.BuildSqlQuery(SqlBuilder.eSqlTransactionType.Select);

            return DatabaseUtility.ExecuteSqlQuery(sqlSelectCommand);

            //BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            //var objContactDetails =
            //(
            //    from objContactDetail in m_objBrightPlatformEntity.Contacts
            //    where objContactDetail.title != null && objContactDetail.title != ""
            //    orderby objContactDetail.title
            //    select new ContactInformationInstance
            //    {
            //        use_item = true,
            //        title = objContactDetail.title,
            //        with_email = false,
            //        with_direct_phone = false,
            //        with_mobile = false
            //    }
            //).Distinct();

            //return (ObjectQuery) objContactDetails;
        }

        /// <summary>
        /// Get titles for adding contact
        /// </summary>
        public static List<string> GetTitles(string text) {
            if (string.IsNullOrEmpty(text)) return null;
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSource = objDbModel.titles
                .Where(x => x.name.Contains(text))
                .OrderByDescending(x => x.name)
                .Select(x => x.name).ToList();
            return objSource;
        }
        #endregion
    }
}
