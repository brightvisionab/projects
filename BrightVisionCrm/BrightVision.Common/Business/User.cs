using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using BrightVision.Common.Business;

namespace BrightVision.Common.Business
{
    public class ObjectUser
    {
        #region Classes
        /// <summary>
        /// Class to contain user instance objects
        /// </summary>
        public class UserInstance
        {
            public int id { get; set; }
            public string full_name { get; set; }
            public string title { get; set; }
            public string manager { get; set; }
            public string site { get; set; }
            public bool active { get; set; }
            public string phone { get; set; }
            public string mobile { get; set; }
            public string email { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string comments { get; set; }
            public bool internal_user { get; set; }
            public int customer_id { get; set; }
            public string customer_name { get; set; }
            public int reports_to { get; set; }
            public int? sip_id { get; set; }
        }

        /// <summary>
        /// Class to contain user role instance objects
        /// </summary>
        public class UserRoleInstance
        {
            public int user_role_id { get; set; }
            public int role_id { get; set; }
            public string role_name { get; set; }
            public bool role_enabled { get; set; }
        }

        /// <summary>
        /// Class to contain user customer instance objects
        /// </summary>
        public class UserCustomerInstance
        {
            public int user_customer_id { get; set; }
            public int customer_id { get; set; }
            public string customer_name { get; set; }
            public bool customer_enabled { get; set; }
        }

        /// <summary>
        /// Sip account handler
        /// </summary>
        public class SipAccount {
            public int id { get; set; }
            public string display_name { get; set; }
        }
        #endregion

        #region Business Methods
        /// <summary>
        /// Get sub campaign sales users
        /// </summary>
        /// <param name="SubCampaignId"></param>
        /// <returns></returns>
        public static ObjectQuery GetSubCampaignSalesUsers(int SubCampaignId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objSalesUsers =
                from objSubCampaignUser in objDbModel.subcampaign_users
                join objSubCampaign in objDbModel.subcampaigns on objSubCampaignUser.subcampaign_id equals objSubCampaign.id
                join objUser in objDbModel.users on objSubCampaignUser.user_id equals objUser.id
                join objRoleUser in objDbModel.roles_users on objUser.id equals objRoleUser.user_id
                where objUser.disabled == 0 && objSubCampaign.id == SubCampaignId && objRoleUser.role_id == 3 // load only those bv sales user
                orderby objUser.fullname
                select new
                {
                    id = objUser.id,
                    name = objUser.fullname
                };

            return (ObjectQuery)objSalesUsers;
        }
        
        /// <summary>
        /// Determines if the user is a brightvision sub campaign owner
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="SubcampaignId"></param>
        /// <returns></returns>
		public static bool IsCampaignOwner(int UserId, int SubcampaignId)
        {
            //1 = Campaign Owner
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var iUser =
            (
                from objUserRole in objDbModel.sub_campaign_user_roles
                join objSubCampaignUser in objDbModel.subcampaign_users on objUserRole.sub_campaign_user_id equals objSubCampaignUser.id
                where objSubCampaignUser.user_id == UserId && objUserRole.sub_campaign_role_id == 1 && objSubCampaignUser.subcampaign_id == SubcampaignId
                select objUserRole

            ).FirstOrDefault();

            if (iUser != null)
                return true;
            else
                return false;
        }
        //public static ObjectQuery GetSIPAccounts()
        public static List<SipAccount> GetSIPAccounts()
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntitySIPs =
                from objSIP in m_objBrightPlatformEntity.sip_accounts
                orderby objSIP.display_name
                select new SipAccount
                {
                    id = objSIP.id,
                    display_name = objSIP.display_name
                };

            //return (ObjectQuery)objEntitySIPs;
            return objEntitySIPs.ToList();
        }
        public static bool CanDoCall()
        {
            BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            int _UserId = UserSession.CurrentUser.UserId;
            var userAudio = _efDbContext.audio_settings.FirstOrDefault(i => i.user_id == _UserId);
            if(userAudio == null)
                return false;

            int _CallMode = userAudio.mode;
            if (_CallMode == 0) {
                user _item = _efDbContext.users.FirstOrDefault(i => i.id == UserSession.CurrentUser.UserId);
                if (_item.sip_id == null || Convert.ToInt32(_item.sip_id) < 1)
                    return false;
                else
                    return true;
            }
            return true;

            //int _UserId = UserSession.CurrentUser.UserId;
            //BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);
            //user _efeUser = _efDbContext.users.Where(i => i.id == _UserId).FirstOrDefault();
            //if (_efeUser != null && _efeUser.sip_id != null && _efeUser.sip_id > 0) {
            //    var sipAccount = _efDbContext.sip_accounts.Where(e => e.id == _efeUser.sip_id).FirstOrDefault();
            //    if (sipAccount != null)
            //        return true;
            //}
            //return false;
        }
        public static bool IsSubCampaignManager(int UserId, int SubcampaignId)
        {
            //2 = Sub Campaign Manager
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var iUser =
            (
                from objUserRole in objDbModel.sub_campaign_user_roles
                join objSubCampaignUser in objDbModel.subcampaign_users on objUserRole.sub_campaign_user_id equals objSubCampaignUser.id
                where objSubCampaignUser.user_id == UserId && objUserRole.sub_campaign_role_id == 2 && objSubCampaignUser.subcampaign_id == SubcampaignId
                select objUserRole

            ).FirstOrDefault();

            if (iUser != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determines if the user is a brightvision sub campaign sales user
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="SubcampaignId"></param>
        /// <returns></returns>
        public static bool IsSubCampaignSales(int UserId, int SubcampaignId)
        {
            //3 = Sub Campaign Sales
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var iUser =
            (
                from objUserRole in objDbModel.sub_campaign_user_roles
                join objSubCampaignUser in objDbModel.subcampaign_users on objUserRole.sub_campaign_user_id equals objSubCampaignUser.id
                where objSubCampaignUser.user_id == UserId && objUserRole.sub_campaign_role_id == 3 && objSubCampaignUser.subcampaign_id == SubcampaignId
                select objUserRole

            ).FirstOrDefault();

            if (iUser != null)
                return true;
            else
                return false;
        }


        /// <summary>
        /// Determines if the user is a brightvision customer user
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="SubcampaignId"></param>
        /// <returns></returns>
        public static bool IsCustomerUser(int UserId, int SubcampaignId)
        {
            //3 = Sub Campaign Sales
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var iUser =
            (
                from objUserRole in objDbModel.sub_campaign_user_roles
                join objSubCampaignUser in objDbModel.subcampaign_users on objUserRole.sub_campaign_user_id equals objSubCampaignUser.id
                where objSubCampaignUser.user_id == UserId && objUserRole.sub_campaign_role_id == 4 && objSubCampaignUser.subcampaign_id == SubcampaignId
                select objUserRole

            ).FirstOrDefault();

            if (iUser != null)
            {
                objDbModel.Detach(iUser);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsSalesUser(int UserId)
        {
            //1 = BVSales User from roles table
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var iUser = objDbModel.roles_users.FirstOrDefault(i => i.user_id == UserId && i.role_id == 3);
            if (iUser != null)
                return true;
            else
                return false;
        }
        public static bool IsManagerUser(int UserId)
        {
            //2 = BVManager User from roles table
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var iUser = objDbModel.roles_users.FirstOrDefault(i => i.user_id == UserId && i.role_id == 2);
            if (iUser != null)
                return true;
            else
                return false;
        }
        public static bool IsManagerAdmin(int UserId)
        {
            //1 = BVManager Admin from roles table
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var iUser = objDbModel.roles_users.FirstOrDefault(i => i.user_id == UserId && i.role_id == 1);
            if (iUser != null)
                return true;
            else
                return false;
        }
        public static bool UserExists(bool IsNew, string FullName, bool UserType)
        {
            if (IsNew)
            {
                BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
                var objEntityUser = m_objBrightPlatformEntity.users.Where(i => i.fullname == FullName && i.internal_user == UserType).SingleOrDefault();
                if (objEntityUser != null)
                    return true;
            }

            return false;
        }
        //public static bool ValidateUserLogin(int UserId, string Password)
        public static bool ValidateUserLogin(string pEmail, string pPassword, ref user efoUser)
        {
            user _efoUser;
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.DefaultEntityConnection)) {
                pPassword = HashUtility.GetHashPassword(pPassword);
                _efoUser = m_objBrightPlatformEntity.users.FirstOrDefault(i => i.email == pEmail && i.password.Equals(pPassword) && i.internal_user == true);
                if (_efoUser != null) {
                    efoUser = _efoUser;
                    m_objBrightPlatformEntity.Detach(_efoUser);
                }
            }

            if (_efoUser != null)
                return true;
            else
                return false;
        }

        public static ObjectQuery GetCustomerUsers(int CustomerId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityCustomerUsers =
                from objUser in m_objBrightPlatformEntity.users
                join objUserCustomer in m_objBrightPlatformEntity.user_customers on objUser.id equals objUserCustomer.user_id
                where objUser.disabled == 0 && (objUser.internal_user == false || objUser.internal_user == null) && objUserCustomer.customer_id == CustomerId
                orderby objUser.fullname
                select new
                {
                    id = objUser.id,
                    name = objUser.fullname
                };

            return (ObjectQuery) objEntityCustomerUsers;
        }
        public static ObjectQuery GetSalesConsultantApplicationUsers()
        {
            // display only BVSales User
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            m_objBrightPlatformEntity.users.MergeOption = MergeOption.NoTracking;
            m_objBrightPlatformEntity.roles_users.MergeOption = MergeOption.NoTracking;
            var objUsers =
            (
                from objUser in m_objBrightPlatformEntity.users
                join objUserRole in m_objBrightPlatformEntity.roles_users on objUser.id equals objUserRole.user_id
                where objUser.disabled == 0 && objUser.internal_user == true && objUserRole.role_id == 3 // 3 = BVSales User
                orderby objUser.fullname
                select new
                {
                    id = objUser.id,
                    name = objUser.fullname
                }

            ).Distinct().OrderBy(i => i.name);
            return (ObjectQuery)objUsers;
        }
        public static ObjectQuery GetManagerApplicationUsers()
        {
            // display only BVManager Admin and BVManager User
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objUsers =
            (
                from objUser in m_objBrightPlatformEntity.users
                join objUserRole in m_objBrightPlatformEntity.roles_users on objUser.id equals objUserRole.user_id
                where objUser.disabled == 0 && objUser.internal_user == true && (objUserRole.role_id == 1 || objUserRole.role_id == 2)
                orderby objUser.fullname
                select new
                {
                    id = objUser.id,
                    name = objUser.fullname
                }

            ).Distinct().OrderBy(i => i.name);

            return (ObjectQuery) objUsers;
        }
        public static ObjectQuery GetInternalUsers()
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityInternalUsers =
                from objInternalUser in m_objBrightPlatformEntity.users
                where objInternalUser.internal_user == true && objInternalUser.disabled == 0
                orderby objInternalUser.fullname
                select new
                {
                    id = objInternalUser.id,
                    name = objInternalUser.fullname
                };
            
            return (ObjectQuery) objEntityInternalUsers;
        }
        public static ObjectQuery GetInternalUsers(List<int> idList)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityInternalUsers =
                from objInternalUser in m_objBrightPlatformEntity.users
                where objInternalUser.internal_user == true && 
                      objInternalUser.disabled == 0 &&
                      !idList.Contains(objInternalUser.id)
                orderby objInternalUser.fullname
                select new
                {
                    id = objInternalUser.id,
                    name = objInternalUser.fullname
                };

            return (ObjectQuery)objEntityInternalUsers;
        }
        public static ObjectQuery GetOwnerUsers()
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityOwners = 
                from objOwners in m_objBrightPlatformEntity.users
                where ((objOwners.internal_user == true && objOwners.disabled == 0 && objOwners.id > 0) || objOwners.id == 0)
                orderby objOwners.fullname
                select new
                {
                    id = objOwners.id,
                    owner_name = objOwners.fullname
                };

            return (ObjectQuery) objEntityOwners;
        }
        public static ObjectQuery GetWebAccessUsers(int CustomerId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityWebAccess = 
                from objUser in m_objBrightPlatformEntity.users
                join objRoleUser in m_objBrightPlatformEntity.roles_users on objUser.id equals objRoleUser.user_id
                join objRole in m_objBrightPlatformEntity.roles on objRoleUser.role_id equals objRole.id
                join objUserCustomer in m_objBrightPlatformEntity.user_customers on objUser.id equals objUserCustomer.user_id
                where objUser.internal_user == false && objUser.disabled == 0 && objUserCustomer.customer_id == CustomerId //&& objRole.role_name.Equals("Web Access")
                orderby objUser.fullname
                select new
                {
                    id = objUser.id,
                    web_access_name = objUser.fullname
                };

            return (ObjectQuery) objEntityWebAccess;
        }

        /// <summary>
        /// Gets the manager records
        /// </summary>
        public static ObjectQuery GetManagers()
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityManagers =
                from objManagers in m_objBrightPlatformEntity.users
                orderby objManagers.fullname
                select new
                {
                    id = objManagers.id,
                    manager_name = objManagers.fullname
                };

            return (ObjectQuery)objEntityManagers;
        }
		public static ObjectQuery GetUserCustomers(int UserId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCustomers =
                from objCustomer in m_objBrightPlatformEntity.customers
                join objEntityUserCustomer in m_objBrightPlatformEntity.user_customers
                    on new { customer_id = objCustomer.id, user_id = UserId }
                    equals new { customer_id = objEntityUserCustomer.customer_id, user_id = objEntityUserCustomer.user_id }
                    into objEntityUserCustomer
                from objUserCustomer in objEntityUserCustomer.DefaultIfEmpty()
                where objCustomer.disabled.Equals("0") && objCustomer.customer_name != ""
                orderby objCustomer.customer_name
                select new ObjectUser.UserCustomerInstance
                {
                    user_customer_id = objUserCustomer.id == null ? 0 : objUserCustomer.id,
                    customer_id = objCustomer.id,
                    customer_name = objCustomer.customer_name,
                    customer_enabled = objUserCustomer.id == null ? false : true
                };

            return (ObjectQuery)objCustomers;
        }
        public static ObjectQuery GetUserRoles(int RoleType, int UserId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            m_objBrightPlatformEntity.users.MergeOption = MergeOption.NoTracking;
            m_objBrightPlatformEntity.roles_users.MergeOption = MergeOption.NoTracking;
            var objRoles =

                from objRole in m_objBrightPlatformEntity.roles
                join objEntityUserRole in m_objBrightPlatformEntity.roles_users
                    on new { role_id = objRole.id, user_id = UserId }
                    equals new { role_id = objEntityUserRole.role_id, user_id = objEntityUserRole.user_id }
                    into objEntityUserRole
                from objUserRole in objEntityUserRole.DefaultIfEmpty()
                where objRole.role_type == RoleType
                orderby objRole.role_name
                select new ObjectUser.UserRoleInstance
                {
                    user_role_id = objUserRole.id == null ? 0 : objUserRole.id,
                    role_id = objRole.id,
                    role_name = objRole.role_name,
                    role_enabled = objUserRole.id == null ? false : true
                };
            
            return (ObjectQuery)objRoles;
        }

        public static void SaveUserComment(int UserId, string UserComment)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            bool isNewRecord = false;

            var objEntityUserComment = m_objBrightPlatformEntity.users_comments.Where(i => i.user_id == UserId).SingleOrDefault();
            if (objEntityUserComment == null)
            {
                objEntityUserComment = m_objBrightPlatformEntity.users_comments.CreateObject();
                objEntityUserComment.user_id = UserId;
                isNewRecord = true;
            }

            objEntityUserComment.comments = UserComment;

            if (isNewRecord)
                m_objBrightPlatformEntity.users_comments.AddObject(objEntityUserComment);

            m_objBrightPlatformEntity.SaveChanges();
        }
        public static void DeleteUser(int UserId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityUser = m_objBrightPlatformEntity.users.Where(objField => objField.id == UserId).SingleOrDefault();
            var objEntityUserComment = m_objBrightPlatformEntity.users_comments.Where(i => i.user_id == UserId).SingleOrDefault();

            m_objBrightPlatformEntity.users.DeleteObject(objEntityUser);

            if (objEntityUserComment != null)
                m_objBrightPlatformEntity.users_comments.DeleteObject(objEntityUserComment);

            m_objBrightPlatformEntity.SaveChanges();
        }
        public static void SaveUserCustomer(int UserId, int CustomerId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityUserCustomer = m_objBrightPlatformEntity.user_customers.Where(i => i.user_id == UserId).SingleOrDefault();
            if (objEntityUserCustomer != null)
            {
                m_objBrightPlatformEntity.user_customers.DeleteObject(objEntityUserCustomer);
                m_objBrightPlatformEntity.SaveChanges();
            }

            user_customers objUserCustomer = new user_customers();
            objUserCustomer.user_id = UserId;
            objUserCustomer.customer_id = CustomerId;
            m_objBrightPlatformEntity.user_customers.AddObject(objUserCustomer);
            m_objBrightPlatformEntity.SaveChanges();
        }
        public static void SaveUserCustomer(int UserId, int CustomerId, bool IsEnabled)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityUserCustomer = m_objBrightPlatformEntity.user_customers.Where(i => i.customer_id == CustomerId && i.user_id == UserId).SingleOrDefault();
            switch (IsEnabled)
            {
                case true:
                {
                    if (objEntityUserCustomer == null)
                    {
                        objEntityUserCustomer = m_objBrightPlatformEntity.user_customers.CreateObject();
                        objEntityUserCustomer.customer_id = CustomerId;
                        objEntityUserCustomer.user_id = UserId;
                        m_objBrightPlatformEntity.user_customers.AddObject(objEntityUserCustomer);
                    }

                    break;
                }
                case false:
                {
                    if (objEntityUserCustomer != null)
                        m_objBrightPlatformEntity.user_customers.DeleteObject(objEntityUserCustomer);

                    break;
                }
            }

            m_objBrightPlatformEntity.SaveChanges();
        }
        public static void SaveUserRole(int UserId, int RoleId, bool IsEnabled)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityUserRole = m_objBrightPlatformEntity.roles_users.Where(i => i.role_id == RoleId && i.user_id == UserId).SingleOrDefault();
            switch (IsEnabled)
            {
                case true:
                {
                    if (objEntityUserRole == null)
                    {
                        objEntityUserRole = m_objBrightPlatformEntity.roles_users.CreateObject();
                        objEntityUserRole.role_id = RoleId;
                        objEntityUserRole.user_id = UserId;
                        m_objBrightPlatformEntity.roles_users.AddObject(objEntityUserRole);
                    }

                    break;
                }
                case false:
                {
                    if (objEntityUserRole != null)
                        m_objBrightPlatformEntity.roles_users.DeleteObject(objEntityUserRole);

                    break;
                }
            }

            m_objBrightPlatformEntity.SaveChanges();
        }
        public static void SaveUserType(int UserId, bool IsInternal)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityUser = m_objBrightPlatformEntity.users.Where(i => i.id == UserId).SingleOrDefault();
            objEntityUser.internal_user = IsInternal;
            objEntityUser.modified_date = DateTime.Now;
            objEntityUser.modified_by = UserSession.CurrentUser.UserId;
            m_objBrightPlatformEntity.SaveChanges();
        }
        public static void SaveUserStatus(int UserId, bool IsActivated)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityUser = m_objBrightPlatformEntity.users.Where(i => i.id == UserId).SingleOrDefault();
            //objEntityUser.disabled = Convert.ToByte(IsActivated);
            objEntityUser.disabled = IsActivated == true ? (byte)0 : (byte)1;
            objEntityUser.modified_date = DateTime.Now;
            objEntityUser.modified_by = UserSession.CurrentUser.UserId;
            m_objBrightPlatformEntity.SaveChanges();
        }
        public static bool SaveUserPassword(string pEmail, string pNewPassword)
        {
            var _Connection = UserSession.DefaultEntityConnection;
            if (_Connection == null)
                return false;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(_Connection)) {
                user _eftUser = _efDbContext.users.FirstOrDefault(i => i.email == pEmail && i.internal_user == true);
                if (Convert.ToBoolean(_eftUser.disabled)) {
                    _efDbContext.Detach(_eftUser);
                    return false;
                }

                if (_eftUser != null) {
                    _efDbContext.change_password_logs.AddObject(new change_password_logs() {
                        user_id = _eftUser.id,
                        old_password = _eftUser.password,
                        new_password = pNewPassword,
                        updated_by = _eftUser.id,
                        updated_on = DateTime.Now
                    });
                    _eftUser.password = HashUtility.GetHashPassword(pNewPassword);
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftUser);
                }
                else
                    return false;
            }

            return true;
        }
        public static bool SaveUserPassword(string pEmail, string pNewPassword, ref string pOldPassword)
        {
            var _Connection = UserSession.DefaultEntityConnection;
            if (_Connection == null)
                return false;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(_Connection)) {
                user _eftUser = _efDbContext.users.FirstOrDefault(i => i.email == pEmail && i.internal_user == true);
                if (_eftUser == null) {
                    BrightVision.Common.UI.NotificationDialog.Error("Error", "No match found for the user.\nPlease contact system administrator.");
                    return false;
                }

                if (Convert.ToBoolean(_eftUser.disabled)) {
                    _efDbContext.Detach(_eftUser);
                    return false;
                }

                if (_eftUser != null) {
                    _efDbContext.change_password_logs.AddObject(new change_password_logs() {
                        user_id = _eftUser.id,
                        old_password = _eftUser.password,
                        new_password = pNewPassword,
                        updated_by = _eftUser.id,
                        updated_on = DateTime.Now,
                        machine_name = UserSession.CurrentUser.ComputerName,
                        machine_ip = UserSession.CurrentUser.ComputerIP
                    });

                    pOldPassword = _eftUser.password;
                    _eftUser.password = HashUtility.GetHashPassword(pNewPassword);
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftUser);
                }
                else
                    return false;
            }

            return true;
        }
        public static bool SaveUserOldPassword(string pEmail, string pOldPassword)
        {
            var _Connection = UserSession.DefaultEntityConnection;
            if (_Connection == null)
                return false;

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(_Connection)) {
                user _eftUser = _efDbContext.users.FirstOrDefault(i => i.email == pEmail && i.internal_user == true);
                if (Convert.ToBoolean(_eftUser.disabled)) {
                    _efDbContext.Detach(_eftUser);
                    return false;
                }

                if (_eftUser != null) {
                    _efDbContext.change_password_logs.AddObject(new change_password_logs() {
                        user_id = _eftUser.id,
                        old_password = _eftUser.password,
                        new_password = pOldPassword,
                        updated_by = _eftUser.id,
                        updated_on = DateTime.Now,
                        machine_name = UserSession.CurrentUser.ComputerName,
                        machine_ip = UserSession.CurrentUser.ComputerIP
                    });
                    _eftUser.password = pOldPassword;
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftUser);
                }
                else
                    return false;
            }

            return true;
        }

        public static string GetUserComment(int UserId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityUserComment = m_objBrightPlatformEntity.users_comments.Where(i => i.user_id == UserId).SingleOrDefault();

            return (objEntityUserComment == null? "": objEntityUserComment.comments);
        }
        
        /// <summary>
        /// Gets the user records
        /// </summary>
        //public static ObjectQuery GetUsers(bool InternalUser)
        public static List<UserInstance> GetUsers(bool InternalUser)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            IQueryable<UserInstance> objUsers = null;
            
            switch (InternalUser)
            {
                case true:
                    objUsers =
                        from objUser in m_objBrightPlatformEntity.users
                        join objEntityManager in m_objBrightPlatformEntity.users on objUser.reports_to equals objEntityManager.id into objEntityManager
                        from objManager in objEntityManager.DefaultIfEmpty()
                        where objUser.internal_user == InternalUser
                        orderby objUser.fullname
                        select new UserInstance
                        {
                            id = objUser.id,
                            full_name = objUser.fullname,
                            title = objUser.title,
                            manager = objManager.fullname,
                            site = objUser.site,
                            active = objUser.disabled == null ? false : (objUser.disabled == 0 ? true : false),
                            phone = objUser.phone1,
                            mobile = objUser.mobile_no,
                            email = objUser.email,
                            username = objUser.username,
                            password = objUser.password,
                            internal_user = (bool)(objUser.internal_user == null ? false : objUser.internal_user),
                            reports_to = (int)(objUser.reports_to == null ? 0 : objUser.reports_to),
                            sip_id = objUser.sip_id ?? 0
                        };
                    break;

                case false:
                    objUsers =
                        from objUser in m_objBrightPlatformEntity.users
                        join objEntityManager in m_objBrightPlatformEntity.users on objUser.reports_to equals objEntityManager.id into objEntityManager
                        from objManager in objEntityManager.DefaultIfEmpty()
                        join objUserCustomer in m_objBrightPlatformEntity.user_customers on objUser.id equals objUserCustomer.user_id
                        join objCustomer in m_objBrightPlatformEntity.customers on objUserCustomer.customer_id equals objCustomer.id
                        where objUser.internal_user == InternalUser
                        orderby objUser.fullname
                        select new UserInstance
                        {
                            id = objUser.id,
                            full_name = objUser.fullname,
                            title = objUser.title,
                            manager = objManager.fullname,
                            site = objUser.site,
                            active = objUser.disabled == null ? false : (objUser.disabled == 0 ? true : false),
                            phone = objUser.phone1,
                            mobile = objUser.mobile_no,
                            email = objUser.email,
                            username = objUser.username,
                            password = objUser.password,
                            internal_user = (bool)(objUser.internal_user == null ? false : objUser.internal_user),
                            reports_to = (int)(objUser.reports_to == null ? 0 : objUser.reports_to),
                            customer_id = objUserCustomer.customer_id,
                            customer_name = objCustomer.customer_name
                        };
                    break;
            }

            //return (ObjectQuery) objUsers;
            return objUsers.ToList();
        }

        /// <summary>
        /// Function to save user data
        /// </summary>
        public static int SaveUser(bool isNew, UserInstance objParams, bool hasChangedPassword)
        {
            user objEntityUser = null;
            using (BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection)) {
                objEntityUser = m_objBrightPlatformEntity.users.CreateObject();
                if (!isNew) {
                    objEntityUser = null;
                    objEntityUser = m_objBrightPlatformEntity.users.Where(i => i.id == objParams.id).FirstOrDefault();
                }
                else {
                    objEntityUser.created_by = UserSession.CurrentUser.UserId;
                    objEntityUser.created_date = DateTime.Now;
                }

                string _OldPassword = objEntityUser.password;

                objEntityUser.fullname = objParams.full_name;
                //objEntityUser.last_name = objParams.last_name;
                objEntityUser.title = objParams.title;
                objEntityUser.reports_to = Convert.ToInt16(objParams.reports_to);
                objEntityUser.site = objParams.site;
                objEntityUser.disabled = Convert.ToInt16(objParams.active == true ? 0 : 1);
                objEntityUser.sip_id = objParams.sip_id;
                objEntityUser.phone1 = objParams.phone;
                objEntityUser.mobile_no = objParams.mobile;
                objEntityUser.email = objParams.email;
                objEntityUser.username = objParams.username;
                objEntityUser.internal_user = objParams.internal_user;
                objEntityUser.modified_by = UserSession.CurrentUser.UserId;
                objEntityUser.modified_date = DateTime.Now;

                if (hasChangedPassword)
                    objEntityUser.password = HashUtility.GetHashPassword(objParams.password);

                if (isNew)
                    m_objBrightPlatformEntity.users.AddObject(objEntityUser);

                m_objBrightPlatformEntity.change_password_logs.AddObject(new change_password_logs() {
                    user_id = objParams.id,
                    new_password = objParams.password,
                    old_password = _OldPassword,
                    updated_by = UserSession.CurrentUser.UserId,
                    updated_on = DateTime.Now
                });

                m_objBrightPlatformEntity.SaveChanges();
                m_objBrightPlatformEntity.Detach(objEntityUser);
            }
            return objEntityUser.id;
        }
        #endregion
    }
}
