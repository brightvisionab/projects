using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Data;
using BrightVision.Common.Utilities;
using System.Data.Objects.SqlClient;

namespace BrightVision.Common.Business
{
    public class AccountStatus
    {
        public List<string> AccountLeadStatuses { get; set; }
        public List<string> AccountStatuses { get; set; }
        public int AccountLeadStatusSelectedIndex { get; set; }
        public int AccountStatusSelectedIndex { get; set; }
        public int AccountLeadStatusNotQualifiedIndex { get; set; }
        public int AccountStatusNotQualifiedIndex { get; set; }
    }

    public class ContactStatus
    {
        public List<string> ContactStatuses { get; set; }
        public int ContactStatusSelectedIndex { get; set; }
        public int ContactStatusNotQualifiedIndex { get; set; }
    }

    public class SubCampaignAppointment
    {
        #region Constructor
        public SubCampaignAppointment(int customer_id, int campaign_id, int sub_campaign_id)
        {
            CustomerId = customer_id;
            CampaignId = campaign_id;
            SubCampaignId = sub_campaign_id;
            AccountId = 0;
            FinalListId = 0;
            CompanyWebsite = null;
            CompanyAppointmentStatus = null;
            CompanyBoardNumber = null;
        }
        #endregion

        #region Getters & Setters
        public int CustomerId { get; set; }
        public int CampaignId { get; set; }
        public int SubCampaignId { get; set; }
        public int AccountId { get; set; }
        public int FinalListId { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyAppointmentStatus { get; set; }
        public string CompanyBoardNumber { get; set; }
        public string CompanyAppointmentLeadStatus { get; set; }
        //public List<string> AccountLeadStatuses { get; set; }
        //public List<string> AccountStatuses { get; set; }
        //public List<string> ContactStatuses { get; set; }
        //public int AccountLeadStatusSelectedIndex { get; set; }
        //public int AccountStatusSelectedIndex { get; set; }
        //public int ContactStatusSelectedIndex { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public ContactStatus ContactStatus { get; set; }
        #endregion
    }

    public class ObjectSubCampaign
    {
        #region Enums
        public enum eUserType
        {
            InternalUser,
            CustomerUser
        }

        public enum eViewType
        {
            SubCampaignView_ManagerAdmin,
            SubCampaignView_ManagerUser,
            ComboListView
        }

        public enum eSalesAppCompanyViewMode
        {
            CompaniesOnly,
            CompaniesAndContacts
        }
        #endregion

        #region Classes
        /// <summary>
        /// Gets or sets the sub campaign instance
        /// </summary>
        public class SubCampaignInstance
        {
            public int id { get; set; }
            public string customer_name { get; set; }
            public string campaign_name { get; set; }
            public string sub_campaign_name { get; set; }
            public string campaign_status { get; set; }
            public string sub_campaign_status { get; set; }
            public string dialog_name { get; set; }
            public string list_name { get; set; }
            public string campaign_owner_name { get; set; }
            public byte campaign_priority { get; set; }
            public int sub_campaign_priority { get; set; }
            public DateTime start_date { get; set; }
            public DateTime end_date { get; set; }
            public int campaign_id { get; set; }
            public int customer_id { get; set; }
            public int owner_id { get; set; }
            public int list_id { get; set; }
            public string description { get; set; }
        }

        public class SubCampaignModificationInfo
        {
            public string creation_info { get; set; }
            public string modification_info { get; set; }
        }
        #endregion

        #region Business Methods
        /// <summary>
        /// Get sub campaign appointment last updated information
        /// </summary>
        /// <param name="FinalListId"></param>
        /// <param name="AccountId"></param>
        /// <returns></returns>
        public static string GetSubCampaignAppointmentLastUpdtedInfo(int FinalListId, int AccountId)
        {
            string _LastUpdatedInfo = string.Empty;
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var _item = objDbModel.sub_campaign_account_appointments.FirstOrDefault(i => i.final_list_id == FinalListId && i.account_id == AccountId) as sub_campaign_account_appointments;
            if (_item != null)
            {
                var _user = objDbModel.users.FirstOrDefault(i => i.id == _item.last_user) as user;
                if (_user != null)
                    _LastUpdatedInfo = string.Format("{0:yyyy-MM-dd} :: {1}",
                        _item.last_updated,
                        _user.fullname
                    );
                //_LastUpdatedInfo = _item.last_updated.ToString() + " by " + _user.fullname;
            }

            return _LastUpdatedInfo;
        }

        /// <summary>
        /// Get the sub campaign information
        /// </summary>
        /// <param name="SubCampaignId"></param>
        /// <returns></returns>
        public static SubCampaignModificationInfo GetSubCampaignModificationInfo(int SubCampaignId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSubCampaignInfo =
            (
                from objSubCampaign in objDbModel.subcampaigns
                join objFinalList in objDbModel.final_lists on objSubCampaign.id equals objFinalList.sub_campaign_id
                join tmpCreatedBy in objDbModel.users on objFinalList.created_by equals tmpCreatedBy.id into tmpCreatedBy
                from objCreatedBy in tmpCreatedBy.DefaultIfEmpty()
                join tmpModifiedBy in objDbModel.users on objFinalList.modified_by equals tmpModifiedBy.id into tmpModifiedBy
                from objModifiedBy in tmpModifiedBy.DefaultIfEmpty()
                where objSubCampaign.id == SubCampaignId
                select new
                {
                    created_by = objCreatedBy.fullname,
                    created_on = objFinalList.created_on,
                    modified_by = objModifiedBy.fullname,
                    modified_on = objFinalList.modified_on
                }

            ).FirstOrDefault();

            if (objSubCampaignInfo == null)
                return null;

            SubCampaignModificationInfo item = new SubCampaignModificationInfo()
            {
                creation_info = string.IsNullOrEmpty(objSubCampaignInfo.created_by) ? "" : objSubCampaignInfo.created_by + "/" + ((DateTime)objSubCampaignInfo.created_on).ToShortDateString(),
                modification_info = string.IsNullOrEmpty(objSubCampaignInfo.modified_by) ? "" : objSubCampaignInfo.modified_by + "/" + ((DateTime)objSubCampaignInfo.modified_on).ToShortDateString()
            };

            return item;
        }

        /// <summary>
        /// Delete a sub campaign user role
        /// </summary>
        /// <param name="SubCampaignUserId"></param>
        /// <param name="RoleId"></param>
        public static void DeleteSubCampaignRole(int SubCampaignUserId, int RoleId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var UserRole = objDbModel.sub_campaign_user_roles.FirstOrDefault(i => i.sub_campaign_user_id == SubCampaignUserId && i.sub_campaign_role_id == RoleId);
            if (UserRole == null)
                return;
            
            objDbModel.sub_campaign_user_roles.DeleteObject(UserRole);
            objDbModel.SaveChanges();
        }

        /// <summary>
        /// Save a sub campaign user role
        /// </summary>
        /// <param name="SubCampaignUserId"></param>
        /// <param name="RoleId"></param>
        public static void SaveSubCampaignRole(int SubCampaignUserId, int RoleId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            sub_campaign_user_roles UserRole = new sub_campaign_user_roles()
            {
                sub_campaign_user_id = SubCampaignUserId,
                sub_campaign_role_id = RoleId
            };

            objDbModel.sub_campaign_user_roles.AddObject(UserRole);
            objDbModel.SaveChanges();
        }

        /// <summary>
        /// Get the subcampaign user's roles
        /// </summary>
        /// <param name="SubCampaignId"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static List<CTSubCampaignUserRole> GetSubCampaignUserRoles(int SubCampaignId, int UserId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            return objDbModel.FIGetSubCampaignUserRoles(SubCampaignId, UserId).ToList();
        }

        /// <summary>
        /// Save company subcampaign appointment call
        /// </summary>
        public static sub_campaign_account_appointments SaveSubCampaignCompanyAppointmentStatus(int AccountId, int FinalListId, string AccountStatus, string LeadStatus = null)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var objCompanyAppointment = _efDbContext.sub_campaign_account_appointments.Where(i => i.account_id == AccountId && i.final_list_id == FinalListId).FirstOrDefault();
                if (objCompanyAppointment == null) {
                    //objBrightPlatformEntity.sub_campaign_account_appointments.AddObject(
                    objCompanyAppointment = new sub_campaign_account_appointments() {
                        account_id = AccountId,
                        final_list_id = FinalListId,
                        last_contact = DateTime.Now,
                        last_user = UserSession.CurrentUser.UserId,
                        status = AccountStatus,
                        lead_status = LeadStatus,
                        last_updated = DateTime.Now
                    };
                    _efDbContext.sub_campaign_account_appointments.AddObject(objCompanyAppointment);
                    //);
                }
                else {
                    objCompanyAppointment.last_contact = DateTime.Now;
                    objCompanyAppointment.last_user = UserSession.CurrentUser.UserId;
                    objCompanyAppointment.status = AccountStatus;
                    if (objCompanyAppointment.status != AccountStatus)
                        objCompanyAppointment.last_updated = DateTime.Now;
                    if (!string.IsNullOrEmpty(LeadStatus))
                        objCompanyAppointment.lead_status = LeadStatus;

                    _efDbContext.sub_campaign_account_appointments.ApplyCurrentValues(objCompanyAppointment);
                }

                _efDbContext.SaveChanges();
                return objCompanyAppointment;
            }
        }

        /// <summary>
        /// Save contact subcampaign appointment status
        /// </summary>
        public static void SaveSubCampaignContactAppointmentStatus(int ContactId, int FinalListId, string DialogStatus)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objContactAppointment = objBrightPlatformEntity.sub_campaign_contact_appointments.Where(i => i.contact_id == ContactId && i.final_list_id == FinalListId).SingleOrDefault();
            if (objContactAppointment == null)
            {
                sub_campaign_contact_appointments objItem = new sub_campaign_contact_appointments()
                {
                    contact_id = ContactId,
                    final_list_id = FinalListId,
                    status = DialogStatus,
                    last_user = UserSession.CurrentUser.UserId,
                    last_contact = DateTime.Now,
                    last_updated = DateTime.Now
                };
                objBrightPlatformEntity.sub_campaign_contact_appointments.AddObject(objItem);
            }
            else
            {
                objContactAppointment.status = DialogStatus;
                objContactAppointment.last_user = UserSession.CurrentUser.UserId;
                objContactAppointment.last_contact = DateTime.Now;
                if (objContactAppointment.status != DialogStatus)
                    objContactAppointment.last_updated = DateTime.Now;
            }

            objBrightPlatformEntity.SaveChanges();
        }

        /// <summary>
        /// Get subcampaign contacts for sales consultant
        /// </summary>
        public static List<CTScSubCampaignContactList> GetSubCampaignContacts(int SubCampaignId, int AccountId, int FinalListId)
        {
            List<CTScSubCampaignContactList> _items = new List<CTScSubCampaignContactList>();
            using (BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _items = objBrightPlatformEntity.FIScGetSubCampaignContactList(SubCampaignId, AccountId, FinalListId).ToList();
            }

            return _items;
        }

        /// <summary>
        /// Get subcampaign accounts for sales consultant
        /// </summary>
        public static List<CTScSubCampaignCompanyAndContact> GetSubCampaignAccounts(int SubCampaignId, eSalesAppCompanyViewMode SalesAppCompanyViewMode)
        {
            List<CTScSubCampaignCompanyAndContact> items = null;
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            objBrightPlatformEntity.CommandTimeout = 0;
            if (SalesAppCompanyViewMode == eSalesAppCompanyViewMode.CompaniesOnly)
                items = objBrightPlatformEntity.FIScGetSubCampaignCompanyList(SubCampaignId).ToList();
            else if (SalesAppCompanyViewMode == eSalesAppCompanyViewMode.CompaniesAndContacts)
            {
                var _objFinalListId = objBrightPlatformEntity.final_lists.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId);
                items = objBrightPlatformEntity.FIScGetSubCampaignCompaniesAndContactsList(SubCampaignId, _objFinalListId.id).ToList();
            }

            return items;
        }

        /// <summary>
        /// Save sub campaign customer user record
        /// </summary>
        public static int SaveSubCampaignCustomerUser(int SubCampaignId, int UserId, string Description)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSubCampaignUser = m_objBrightPlatformEntity.subcampaign_users.CreateObject();

            objSubCampaignUser.subcampaign_id = SubCampaignId;
            objSubCampaignUser.user_id = UserId;
            objSubCampaignUser.internal_user = false;
            objSubCampaignUser.description = Description;
            //objSubCampaignUser.calendar; //? still not finalized

            m_objBrightPlatformEntity.subcampaign_users.AddObject(objSubCampaignUser);
            m_objBrightPlatformEntity.SaveChanges();
            return objSubCampaignUser.id;
        }

        /// <summary>
        /// Removes sub campaign internal user
        /// </summary>
        public static void RemoveSubCampaignUser(int SubCampaignUserId)
        {

            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            objDbModel.FIDeleteSubCampaignUser(SubCampaignUserId);

            //var objEntityUser = m_objBrightPlatformEntity.subcampaign_users.Where(objSubCampaignUser => objSubCampaignUser.id == SubCampaignUserId).SingleOrDefault();
            //m_objBrightPlatformEntity.subcampaign_users.DeleteObject(objEntityUser);
            //m_objBrightPlatformEntity.SaveChanges();
        }

        /// <summary>
        /// Save sub campaign internal user record
        /// </summary>
        public static int SaveSubCampaignInternalUser(int SubCampaignId, int UserId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSubCampaignUser = m_objBrightPlatformEntity.subcampaign_users.CreateObject();

            objSubCampaignUser.subcampaign_id = SubCampaignId;
            objSubCampaignUser.user_id = UserId;
            objSubCampaignUser.internal_user = true;
            objSubCampaignUser.description = ""; //todo: implement later if approved for sbcampaign internal users
            //objSubCampaignUser.calendar; //? still not finalized

            m_objBrightPlatformEntity.subcampaign_users.AddObject(objSubCampaignUser);
            m_objBrightPlatformEntity.SaveChanges();
            return objSubCampaignUser.id;
        }

        /// <summary>
        /// Validate sub-campaign internal user if exists
        /// </summary>
        public static bool ValidateSubCampaignUserExists(int SubCampaignId, string Name, bool IsInternalUser)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityUser = 
            (
                from objSubCampaignUser in m_objBrightPlatformEntity.subcampaign_users
                join objUser in m_objBrightPlatformEntity.users on objSubCampaignUser.user_id equals objUser.id
                where objSubCampaignUser.subcampaign_id == SubCampaignId && objUser.fullname.Equals(Name) && objSubCampaignUser.internal_user == IsInternalUser
                select objSubCampaignUser

             ).FirstOrDefault();

            if (objEntityUser != null)
                return true;

            return false;
        }

        /// <summary>
        /// Validate sub-campaign internal user if exists
        /// </summary>
        public static bool ValidateSubCampaignUserExists(int SubCampaignId, int UserId, bool IsInternalUser)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityUser = m_objBrightPlatformEntity.subcampaign_users
                .Where(objUser => objUser.subcampaign_id == SubCampaignId && objUser.user_id == UserId && objUser.internal_user == IsInternalUser).SingleOrDefault();

            if (objEntityUser != null)
                return true;

            return false;
        }

        /// <summary>
        /// Get sub-campaign internal user records
        /// </summary>
        public static ObjectResult GetSubCampaignUsers(int SubCampaignId, eUserType UserType)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            ObjectResult objSubCampaignUsers = null;

            switch (UserType)
            {
                case eUserType.InternalUser:
                {
                    objSubCampaignUsers = m_objBrightPlatformEntity.FIGetSubCampaignInternalUsers(SubCampaignId);
                    break;
                }
                case eUserType.CustomerUser:
                {
                    objSubCampaignUsers = m_objBrightPlatformEntity.FIGetSubCampaignCustomerUsers(SubCampaignId);
                    break;
                }
            }

            return objSubCampaignUsers;
        }

        /// <summary>
        /// Get sub-campaign records for a specific campaign, customer, and user only
        /// </summary>
        //public static ObjectQuery GetActiveSubCampaigns(int CampaignId, int CustomerId, int UserId)
        public static List<CTScSubCampaignList> GetActiveSubCampaigns(int UserId, List<string> Statuses)
        {
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection)) {
                if (Statuses.Count < 1)
                    return objDbModel.FIScGetSubCampaignList(UserId, "").ToList();
                else
                    return objDbModel.FIScGetSubCampaignList(UserId, string.Join(",", Statuses.ToArray())).ToList();
            }

            //var objSubCampaigns =
            //(
            //    from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
            //    join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
            //    join objEntitySubCampaignUser in m_objBrightPlatformEntity.subcampaign_users on objSubCampaign.id equals objEntitySubCampaignUser.subcampaign_id into objEntitySubCampaignUser
            //    from objSubCampaignUser in objEntitySubCampaignUser.DefaultIfEmpty()
            //    join objUserRole in m_objBrightPlatformEntity.sub_campaign_user_roles on objSubCampaignUser.id equals objUserRole.sub_campaign_user_id
            //    where
            //        objSubCampaign.status != "Deleted" &&
            //        objSubCampaign.campaign_id == CampaignId &&
            //        objCampaign.customer_id == CustomerId &&
            //        objUserRole.sub_campaign_role_id == 3 && // 3 = Sub Campaign Sales User
            //        objSubCampaignUser.user_id == UserId
            //    //(objSubCampaign.campaign_manager_user_id == UserId || objSubCampaignUser.user_id == UserId)

            //    orderby objSubCampaign.priority descending, objSubCampaign.title
            //    select new
            //    {
            //        id = objSubCampaign.id,
            //        title = objSubCampaign.title
            //    }

            //).Distinct();

            //return (ObjectQuery)objSubCampaigns;
        }

        /// <summary>
        /// Get sub-campaign records for a specific campaign, customer, tied to final lists
        /// </summary>
        public static ObjectQuery GetFinalCallListSubCampaigns(int CampaignId, int CustomerId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSubCampaigns =
            (
                from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
                join objFinalList in m_objBrightPlatformEntity.final_lists on objSubCampaign.id equals objFinalList.sub_campaign_id
                where objSubCampaign.campaign_id == CampaignId && objCampaign.customer_id == CustomerId && objSubCampaign.status != "Deleted"
                orderby objSubCampaign.title
                select new
                {
                    id = objSubCampaign.id,
                    title = objSubCampaign.title
                }

            ).Distinct();

            return (ObjectQuery)objSubCampaigns;
        }

        /// <summary>
        /// Get sub-campaign records for a specific campaign, customer
        /// </summary>
        public static ObjectQuery GetSubCampaigns(int CampaignId, int CustomerId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSubCampaigns =
            (
                from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
                where objSubCampaign.campaign_id == CampaignId && objCampaign.customer_id == CustomerId && objSubCampaign.status != "Deleted"
                orderby objSubCampaign.title
                select new
                {
                    id = objSubCampaign.id,
                    title = objSubCampaign.title
                }

            ).Distinct();

            return (ObjectQuery)objSubCampaigns;
        }

        /// <summary>
        /// Get sub-campaign records for a specific campaign, customer, and user only
        /// </summary>
        public static ObjectQuery GetSubCampaigns(int CampaignId, int CustomerId, int UserId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSubCampaigns =
            (
                from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
                join objEntitySubCampaignUser in m_objBrightPlatformEntity.subcampaign_users on objSubCampaign.id equals objEntitySubCampaignUser.subcampaign_id into objEntitySubCampaignUser
                from objSubCampaignUser in objEntitySubCampaignUser.DefaultIfEmpty()
                where
                    objSubCampaign.campaign_id == CampaignId &&
                    objCampaign.customer_id == CustomerId &&
                    (objSubCampaign.campaign_manager_user_id == UserId || objSubCampaignUser.user_id == UserId)

                orderby objSubCampaign.title
                select new
                {
                    id = objSubCampaign.id,
                    title = objSubCampaign.title
                }

            ).Distinct();

            return (ObjectQuery) objSubCampaigns;
        }

        /// <summary>
        /// Get sub-campaign records
        /// </summary>
        public static List<SubCampaignInstance> GetSubCampaignList(eViewType ViewType, int CampaignId, int CustomerId)
        {
            List<SubCampaignInstance> objSubCampaignList = new List<SubCampaignInstance>();
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                switch (ViewType) {
                    case eViewType.SubCampaignView_ManagerAdmin:
                        {
                            var objSubCampaigns =
                            from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                            join objEntityCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objEntityCampaign.id into objEntityCampaign
                            from objCampaign in objEntityCampaign.DefaultIfEmpty()
                            join objEntityCustomer in m_objBrightPlatformEntity.customers on objCampaign.customer_id equals objEntityCustomer.id into objEntityCustomer
                            from objCustomer in objEntityCustomer.DefaultIfEmpty()
                            join objEntityOwner in m_objBrightPlatformEntity.users on objSubCampaign.campaign_manager_user_id equals objEntityOwner.id into objEntityOwner
                            from objOwner in objEntityOwner.DefaultIfEmpty()
                            join objEntityDialog in m_objBrightPlatformEntity.dialogs
                                on new { sid = objSubCampaign.id, ac = true }
                                equals new { sid = objEntityDialog.subcampaign_id, ac = objEntityDialog.is_active }
                                into objEntityDialog
                            from objDialog in objEntityDialog.DefaultIfEmpty()
                            orderby objCustomer.customer_name, objCampaign.campaign_name, objSubCampaign.title
                            select new SubCampaignInstance
                            {
                                id = objSubCampaign.id,
                                customer_name = objCustomer.customer_name,
                                campaign_name = objCampaign.campaign_name,
                                sub_campaign_name = objSubCampaign.title,
                                campaign_status = objCampaign.status,
                                sub_campaign_status = objSubCampaign.status,
                                dialog_name = objDialog.name,
                                list_name = objSubCampaign.list_name,
                                campaign_owner_name = objOwner.fullname,
                                campaign_priority = (byte)(objCampaign.priority == null ? 0 : objCampaign.priority),
                                sub_campaign_priority = (int)(objSubCampaign.priority == null ? 0 : objSubCampaign.priority),
                                start_date = (DateTime)objSubCampaign.start_date,
                                end_date = (DateTime)objSubCampaign.end_date,
                                campaign_id = (int)(objCampaign.id == null ? 0 : objCampaign.id),
                                customer_id = (int)(objCustomer.id == null ? 0 : objCustomer.id),
                                owner_id = (int)(objOwner.id == null ? 0 : objOwner.id),
                                list_id = 0, //todo: implement later of finalized
                                description = objSubCampaign.description
                            };

                            objSubCampaignList = objSubCampaigns.ToList();
                            break;
                        }

                    case eViewType.SubCampaignView_ManagerUser:
                        {
                            var objSubCampaigns =
                            from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                            join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
                            join objUserCustomer in m_objBrightPlatformEntity.user_customers
                                on new { user_id = UserSession.CurrentUser.UserId, customer_id = objCampaign.customer_id }
                                equals new { user_id = objUserCustomer.user_id, customer_id = objUserCustomer.customer_id }
                            //join objEntityCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objEntityCampaign.id into objEntityCampaign
                            //from objCampaign in objEntityCampaign.DefaultIfEmpty()
                            join objEntityCustomer in m_objBrightPlatformEntity.customers on objCampaign.customer_id equals objEntityCustomer.id into objEntityCustomer
                            from objCustomer in objEntityCustomer.DefaultIfEmpty()
                            join objEntityOwner in m_objBrightPlatformEntity.users on objSubCampaign.campaign_manager_user_id equals objEntityOwner.id into objEntityOwner
                            from objOwner in objEntityOwner.DefaultIfEmpty()
                            join objEntityDialog in m_objBrightPlatformEntity.dialogs
                                on new { sid = objSubCampaign.id, ac = true }
                                equals new { sid = objEntityDialog.subcampaign_id, ac = objEntityDialog.is_active }
                                into objEntityDialog
                            from objDialog in objEntityDialog.DefaultIfEmpty()
                            //where objCampaign.customer_id == CustomerId
                            orderby objCustomer.customer_name, objCampaign.campaign_name, objSubCampaign.title
                            select new SubCampaignInstance
                            {
                                id = objSubCampaign.id,
                                customer_name = objCustomer.customer_name,
                                campaign_name = objCampaign.campaign_name,
                                sub_campaign_name = objSubCampaign.title,
                                campaign_status = objCampaign.status,
                                sub_campaign_status = objSubCampaign.status,
                                dialog_name = objDialog.name,
                                list_name = objSubCampaign.list_name,
                                campaign_owner_name = objOwner.fullname,
                                campaign_priority = (byte)(objCampaign.priority == null ? 0 : objCampaign.priority),
                                sub_campaign_priority = (int)(objSubCampaign.priority == null ? 0 : objSubCampaign.priority),
                                start_date = (DateTime)objSubCampaign.start_date,
                                end_date = (DateTime)objSubCampaign.end_date,
                                campaign_id = (int)(objCampaign.id == null ? 0 : objCampaign.id),
                                customer_id = (int)(objCustomer.id == null ? 0 : objCustomer.id),
                                owner_id = (int)(objOwner.id == null ? 0 : objOwner.id),
                                list_id = 0, //todo: implement later of finalized
                                description = objSubCampaign.description
                            };

                            objSubCampaignList = objSubCampaigns.ToList();
                            break;
                        }
                }
            }

            return objSubCampaignList;
        }
        public static ObjectQuery GetSubCampaigns(eViewType ViewType, int CampaignId, int CustomerId)
        {
            ObjectQuery objSubCampaignList = null;

            /*
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                switch (ViewType) {
                    case eViewType.SubCampaignView_ManagerAdmin:
                        {
                            var objSubCampaigns =
                            from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                            join objEntityCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objEntityCampaign.id into objEntityCampaign
                            from objCampaign in objEntityCampaign.DefaultIfEmpty()
                            join objEntityCustomer in m_objBrightPlatformEntity.customers on objCampaign.customer_id equals objEntityCustomer.id into objEntityCustomer
                            from objCustomer in objEntityCustomer.DefaultIfEmpty()
                            join objEntityOwner in m_objBrightPlatformEntity.users on objSubCampaign.campaign_manager_user_id equals objEntityOwner.id into objEntityOwner
                            from objOwner in objEntityOwner.DefaultIfEmpty()
                            join objEntityDialog in m_objBrightPlatformEntity.dialogs
                                on new { sid = objSubCampaign.id, ac = true }
                                equals new { sid = objEntityDialog.subcampaign_id, ac = objEntityDialog.is_active }
                                into objEntityDialog
                            from objDialog in objEntityDialog.DefaultIfEmpty()
                            orderby objCustomer.customer_name, objCampaign.campaign_name, objSubCampaign.title
                            select new SubCampaignInstance
                            {
                                id = objSubCampaign.id,
                                customer_name = objCustomer.customer_name,
                                campaign_name = objCampaign.campaign_name,
                                sub_campaign_name = objSubCampaign.title,
                                campaign_status = objCampaign.status,
                                sub_campaign_status = objSubCampaign.status,
                                dialog_name = objDialog.name,
                                list_name = objSubCampaign.list_name,
                                campaign_owner_name = objOwner.fullname,
                                campaign_priority = (byte)(objCampaign.priority == null ? 0 : objCampaign.priority),
                                sub_campaign_priority = (int)(objSubCampaign.priority == null ? 0 : objSubCampaign.priority),
                                start_date = (DateTime)objSubCampaign.start_date,
                                end_date = (DateTime)objSubCampaign.end_date,
                                campaign_id = (int)(objCampaign.id == null ? 0 : objCampaign.id),
                                customer_id = (int)(objCustomer.id == null ? 0 : objCustomer.id),
                                owner_id = (int)(objOwner.id == null ? 0 : objOwner.id),
                                list_id = 0, //todo: implement later of finalized
                                description = objSubCampaign.description
                            };

                            objSubCampaignList = (ObjectQuery)objSubCampaigns;
                            break;
                        }

                    case eViewType.SubCampaignView_ManagerUser:
                        {
                            var objSubCampaigns =
                            from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                            join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
                            join objUserCustomer in m_objBrightPlatformEntity.user_customers
                                on new { user_id = UserSession.CurrentUser.UserId, customer_id = objCampaign.customer_id }
                                equals new { user_id = objUserCustomer.user_id, customer_id = objUserCustomer.customer_id }
                            //join objEntityCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objEntityCampaign.id into objEntityCampaign
                            //from objCampaign in objEntityCampaign.DefaultIfEmpty()
                            join objEntityCustomer in m_objBrightPlatformEntity.customers on objCampaign.customer_id equals objEntityCustomer.id into objEntityCustomer
                            from objCustomer in objEntityCustomer.DefaultIfEmpty()
                            join objEntityOwner in m_objBrightPlatformEntity.users on objSubCampaign.campaign_manager_user_id equals objEntityOwner.id into objEntityOwner
                            from objOwner in objEntityOwner.DefaultIfEmpty()
                            join objEntityDialog in m_objBrightPlatformEntity.dialogs
                                on new { sid = objSubCampaign.id, ac = true }
                                equals new { sid = objEntityDialog.subcampaign_id, ac = objEntityDialog.is_active }
                                into objEntityDialog
                            from objDialog in objEntityDialog.DefaultIfEmpty()
                            //where objCampaign.customer_id == CustomerId
                            orderby objCustomer.customer_name, objCampaign.campaign_name, objSubCampaign.title
                            select new SubCampaignInstance
                            {
                                id = objSubCampaign.id,
                                customer_name = objCustomer.customer_name,
                                campaign_name = objCampaign.campaign_name,
                                sub_campaign_name = objSubCampaign.title,
                                campaign_status = objCampaign.status,
                                sub_campaign_status = objSubCampaign.status,
                                dialog_name = objDialog.name,
                                list_name = objSubCampaign.list_name,
                                campaign_owner_name = objOwner.fullname,
                                campaign_priority = (byte)(objCampaign.priority == null ? 0 : objCampaign.priority),
                                sub_campaign_priority = (int)(objSubCampaign.priority == null ? 0 : objSubCampaign.priority),
                                start_date = (DateTime)objSubCampaign.start_date,
                                end_date = (DateTime)objSubCampaign.end_date,
                                campaign_id = (int)(objCampaign.id == null ? 0 : objCampaign.id),
                                customer_id = (int)(objCustomer.id == null ? 0 : objCustomer.id),
                                owner_id = (int)(objOwner.id == null ? 0 : objOwner.id),
                                list_id = 0, //todo: implement later of finalized
                                description = objSubCampaign.description
                            };

                            objSubCampaignList = (ObjectQuery)objSubCampaigns;
                            break;
                        }

                    case eViewType.ComboListView:
                        {
                            var objSubCampaigns =
                                from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                                join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
                                where objSubCampaign.campaign_id == CampaignId
                                    && objCampaign.customer_id == CustomerId
                                    && objSubCampaign.status != "Deleted"
                                select new
                                {
                                    id = objSubCampaign.id,
                                    title = objSubCampaign.title
                                };

                            objSubCampaignList = (ObjectQuery)objSubCampaigns;
                            break;
                        }
                }
            }
            */

            /*
             * https://brightvision.jira.com/browse/PLATFORM-3169
             * DAN: In causes the error so remodified the code
             */

            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            
            switch (ViewType)
            {
                case eViewType.SubCampaignView_ManagerAdmin:
                    {
                        var objSubCampaigns =
                        from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                        join objEntityCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objEntityCampaign.id into objEntityCampaign
                        from objCampaign in objEntityCampaign.DefaultIfEmpty()
                        join objEntityCustomer in m_objBrightPlatformEntity.customers on objCampaign.customer_id equals objEntityCustomer.id into objEntityCustomer
                        from objCustomer in objEntityCustomer.DefaultIfEmpty()
                        join objEntityOwner in m_objBrightPlatformEntity.users on objSubCampaign.campaign_manager_user_id equals objEntityOwner.id into objEntityOwner
                        from objOwner in objEntityOwner.DefaultIfEmpty()
                        join objEntityDialog in m_objBrightPlatformEntity.dialogs
                            on new { sid = objSubCampaign.id, ac = true }
                            equals new { sid = objEntityDialog.subcampaign_id, ac = objEntityDialog.is_active }
                            into objEntityDialog
                        from objDialog in objEntityDialog.DefaultIfEmpty()
                        orderby objCustomer.customer_name, objCampaign.campaign_name, objSubCampaign.title
                        select new SubCampaignInstance
                        {
                            id = objSubCampaign.id,
                            customer_name = objCustomer.customer_name,
                            campaign_name = objCampaign.campaign_name,
                            sub_campaign_name = objSubCampaign.title,
                            campaign_status = objCampaign.status,
                            sub_campaign_status = objSubCampaign.status,
                            dialog_name = objDialog.name,
                            list_name = objSubCampaign.list_name,
                            campaign_owner_name = objOwner.fullname,
                            campaign_priority = (byte)(objCampaign.priority == null ? 0 : objCampaign.priority),
                            sub_campaign_priority = (int)(objSubCampaign.priority == null ? 0 : objSubCampaign.priority),
                            start_date = (DateTime)objSubCampaign.start_date,
                            end_date = (DateTime)objSubCampaign.end_date,
                            campaign_id = (int)(objCampaign.id == null ? 0 : objCampaign.id),
                            customer_id = (int)(objCustomer.id == null ? 0 : objCustomer.id),
                            owner_id = (int)(objOwner.id == null ? 0 : objOwner.id),
                            list_id = 0, //todo: implement later of finalized
                            description = objSubCampaign.description
                        };

                        objSubCampaignList = (ObjectQuery)objSubCampaigns;
                        break;
                    }

                case eViewType.SubCampaignView_ManagerUser:
                    {
                        var objSubCampaigns =
                        from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                        join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
                        join objUserCustomer in m_objBrightPlatformEntity.user_customers
                            on new { user_id = UserSession.CurrentUser.UserId, customer_id = objCampaign.customer_id }
                            equals new { user_id = objUserCustomer.user_id, customer_id = objUserCustomer.customer_id }
                        //join objEntityCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objEntityCampaign.id into objEntityCampaign
                        //from objCampaign in objEntityCampaign.DefaultIfEmpty()
                        join objEntityCustomer in m_objBrightPlatformEntity.customers on objCampaign.customer_id equals objEntityCustomer.id into objEntityCustomer
                        from objCustomer in objEntityCustomer.DefaultIfEmpty()
                        join objEntityOwner in m_objBrightPlatformEntity.users on objSubCampaign.campaign_manager_user_id equals objEntityOwner.id into objEntityOwner
                        from objOwner in objEntityOwner.DefaultIfEmpty()
                        join objEntityDialog in m_objBrightPlatformEntity.dialogs
                            on new { sid = objSubCampaign.id, ac = true }
                            equals new { sid = objEntityDialog.subcampaign_id, ac = objEntityDialog.is_active }
                            into objEntityDialog
                        from objDialog in objEntityDialog.DefaultIfEmpty()
                        //where objCampaign.customer_id == CustomerId
                        orderby objCustomer.customer_name, objCampaign.campaign_name, objSubCampaign.title
                        select new SubCampaignInstance
                        {
                            id = objSubCampaign.id,
                            customer_name = objCustomer.customer_name,
                            campaign_name = objCampaign.campaign_name,
                            sub_campaign_name = objSubCampaign.title,
                            campaign_status = objCampaign.status,
                            sub_campaign_status = objSubCampaign.status,
                            dialog_name = objDialog.name,
                            list_name = objSubCampaign.list_name,
                            campaign_owner_name = objOwner.fullname,
                            campaign_priority = (byte)(objCampaign.priority == null ? 0 : objCampaign.priority),
                            sub_campaign_priority = (int)(objSubCampaign.priority == null ? 0 : objSubCampaign.priority),
                            start_date = (DateTime)objSubCampaign.start_date,
                            end_date = (DateTime)objSubCampaign.end_date,
                            campaign_id = (int)(objCampaign.id == null ? 0 : objCampaign.id),
                            customer_id = (int)(objCustomer.id == null ? 0 : objCustomer.id),
                            owner_id = (int)(objOwner.id == null ? 0 : objOwner.id),
                            list_id = 0, //todo: implement later of finalized
                            description = objSubCampaign.description
                        };

                        objSubCampaignList = (ObjectQuery)objSubCampaigns;
                        break;
                    }

                case eViewType.ComboListView:
                    {
                        var objSubCampaigns =
                            from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
                            join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
                            where objSubCampaign.campaign_id == CampaignId
                                && objCampaign.customer_id == CustomerId
                                && objSubCampaign.status != "Deleted"
                            select new
                            {
                                id = objSubCampaign.id,
                                title = objSubCampaign.title
                            };

                        objSubCampaignList = (ObjectQuery)objSubCampaigns;
                        break;
                    }
            }


            return objSubCampaignList;
        }

        /// <summary>
        /// Save sub campaign record
        /// </summary>
        public static int SaveSubCampaign(bool IsNew, SubCampaignInstance objParams)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSubCampaign = m_objBrightPlatformEntity.subcampaigns.CreateObject();
            if (!IsNew)
            {
                objSubCampaign = null;
                objSubCampaign = m_objBrightPlatformEntity.subcampaigns.Where(objField => objField.id == objParams.id).SingleOrDefault();
            }

            objSubCampaign.title = objParams.sub_campaign_name;
            objSubCampaign.subprocess_id = 1; //todo: imeplement later if finalized
            objSubCampaign.list_id = objParams.list_id;
            objSubCampaign.list_name = objParams.list_name;
            objSubCampaign.start_date = objParams.start_date;
            objSubCampaign.end_date = objParams.end_date;
            objSubCampaign.campaign_manager_user_id = objParams.owner_id;
            objSubCampaign.status = objParams.sub_campaign_status;
            objSubCampaign.priority = (short) objParams.sub_campaign_priority;
            objSubCampaign.campaign_id = objParams.campaign_id;
            objSubCampaign.description = objParams.description;
            
            if (IsNew)
                m_objBrightPlatformEntity.subcampaigns.AddObject(objSubCampaign);

            m_objBrightPlatformEntity.SaveChanges();
            return objSubCampaign.id;
        }
        #endregion
    }
}
