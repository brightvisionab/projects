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
    public class ObjectCallList
    {
        #region Classes
        #endregion

        #region Business Methods
        /// <summary>
        /// Add new contacts to an existing sub-campaign
        /// </summary>
        /// <param name="lstContactIds"></param>
        /// <param name="SubCampaignId"></param>
        public static void AddContactsToSubCampaign(List<int> lstContactIds, int SubCampaignId)
        {
            BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            int? _FinalListId = _efDbModel.final_lists.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId).id;
            if (_FinalListId > 0)
            {
                foreach (int _ContactId in lstContactIds)
                {
                    sub_campaign_contact_lists _item = new sub_campaign_contact_lists()
                    {
                        final_list_id = (int)_FinalListId,
                        contact_id = _ContactId,
                        created_by = UserSession.CurrentUser.UserId,
                        created_on = DateTime.Now,
                        active = true,
                        modified = false,
                        priority = null,
                        in_list = false
                    };

                    _efDbModel.sub_campaign_contact_lists.AddObject(_item);
                }
                _efDbModel.SaveChanges();
            }
        }

        /// <summary>
        /// Add new companies to an existing sub-campaign
        /// </summary>
        /// <param name="lstAcctIds"></param>
        /// <param name="SubCampaignId"></param>
        public static void AddCompaniesToSubCampaign(List<int> lstAcctIds, int SubCampaignId, string ListSource = "")
        {
            BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            int? _FinalListId = _efDbModel.final_lists.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId).id;
            if (_FinalListId > 0)
            {
                foreach (int _AcctId in lstAcctIds)
                {
                    sub_campaign_account_lists _item = new sub_campaign_account_lists()
                    {
                        final_list_id = (int)_FinalListId,
                        account_id = _AcctId,
                        created_by = UserSession.CurrentUser.UserId,
                        created_on = DateTime.Now,
                        modified = false,
                        active = true,
                        priority = null,
                        locked = false,
                        locked_by = null,
                        locked_timestamp = null,
                        assigned_to = null,
                        list_source = ListSource,
                        modified_by = UserSession.CurrentUser.UserId,
                        modified_on = DateTime.Now
                    };

                    _efDbModel.sub_campaign_account_lists.AddObject(_item);
                }
                _efDbModel.SaveChanges();
            }
        }

        /// <summary>
        /// To release account locks of a specified sub campaign
        /// </summary>
        /// <param name="SubCampaignAccountListId"></param>
        public static void ReleaseAccountLocks(List<int> SubCampaignAccountListIds)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            foreach (int SubCampaignAccountListId in SubCampaignAccountListIds)
            {
                var item = objDbModel.sub_campaign_account_lists.FirstOrDefault(p => p.id == SubCampaignAccountListId);
                if (item == null)
                    continue;

                item.locked = false;
                item.locked_by = null;
                item.locked_timestamp = null;
                objDbModel.sub_campaign_account_lists.ApplyCurrentValues(item);
            }
            
            objDbModel.SaveChanges();
        }

        /// <summary>
        /// Get customer exported to bin call list
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        public static ObjectQuery GetCustomerCallList(int CustomerId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCallLists =
                from objCallList in objDbModel.lists
                where objCallList.customer_id == CustomerId && objCallList.matched == false
                select new
                {
                    id = objCallList.id,
                    list_name = objCallList.list_name
                };

            return (ObjectQuery) objCallLists;
        }

        /// <summary>
        /// Get the final call list contacts
        /// </summary>
        /// <param name="SubCampaignId"></param>
        /// <returns></returns>
        public static List<CTFinalCallListContact> GetFinalCallListContacts(int SubCampaignId)
        {
            List<CTFinalCallListContact> _lstData = new List<CTFinalCallListContact>();
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                _lstData = objDbModel.FIGetFinalCallListContacts(SubCampaignId).ToList();
            }
            return _lstData;
        }

        /// <summary>
        /// Get the final call list accounts
        /// </summary>
        /// <param name="SubCampaignId"></param>
        /// <returns></returns>
        public static List<CTFinalCallListAccount> GetFinalCallListAccounts(int SubCampaignId)
        {
            List<CTFinalCallListAccount> _lstData = new List<CTFinalCallListAccount>();
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                _lstData = objDbModel.FIGetFinalCallListAccounts(SubCampaignId).ToList();
            }
            return _lstData;
        }

        /// <summary>
        /// Save updated subcampaign contact lists
        /// </summary>
        public static void SaveFinalCallListContacts(List<CTFinalCallListContact> Items, int SubCampaignId)
        {
            bool _HasUpdates = false;
            sub_campaign_contact_lists ItemToupdate = new sub_campaign_contact_lists();
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            foreach (CTFinalCallListContact item in Items)
            {
                ItemToupdate = null;
                ItemToupdate = objDbModel.sub_campaign_contact_lists.FirstOrDefault(i => i.id == item.id);
                if (ItemToupdate == null)
                    continue;

                ItemToupdate.priority = item.priority;
                ItemToupdate.modified = true;
                objDbModel.sub_campaign_contact_lists.ApplyCurrentValues(ItemToupdate);
                _HasUpdates = true;
            }

            if (_HasUpdates)
            {
                var item = objDbModel.final_lists.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId);
                if (item != null)
                {
                    item.modified_by = UserSession.CurrentUser.UserId;
                    item.modified_on = DateTime.Now;
                }

                objDbModel.SaveChanges();
            }
        }

        /// <summary>
        /// Save updated subcampaign account lists
        /// </summary>
        public static void SaveFinalCallListAccounts(List<CTFinalCallListAccount> Items, int SubCampaignId)
        {
            bool _HasUpdates = false;
            sub_campaign_account_lists ItemToupdate = new sub_campaign_account_lists();
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            foreach (CTFinalCallListAccount item in Items) {
                ItemToupdate = null;
                ItemToupdate = objDbModel.sub_campaign_account_lists.FirstOrDefault(i => i.id == item.id);
                if (ItemToupdate == null)
                    continue;

                ItemToupdate.active = item.active.Equals("Yes")? true: false;
                ItemToupdate.modified = item.modified.Equals("Yes") ? true : false;
                ItemToupdate.priority = item.priority;
                ItemToupdate.assigned_to = item.assigned_to;
                ItemToupdate.modified = true;
                ItemToupdate.modified_on = DateTime.Now;
                ItemToupdate.modified_by = UserSession.CurrentUser.UserId;
                objDbModel.sub_campaign_account_lists.ApplyCurrentValues(ItemToupdate);
                _HasUpdates = true;
            }

            if (_HasUpdates) {
                var item = objDbModel.final_lists.FirstOrDefault(i => i.sub_campaign_id == SubCampaignId);
                if (item != null) {
                    item.modified_by = UserSession.CurrentUser.UserId;
                    item.modified_on = DateTime.Now;
                }

                objDbModel.SaveChanges();
            }
        }

        /// <summary>
        /// Save subcampaign account list records
        /// </summary>
        public static void UpdateMergeLists(string MergeListIds, string BlockedMergeListIds)
        {
            BrightPlatformEntities objBrightPlatformEntities = new BrightPlatformEntities(UserSession.EntityConnection);
            //objBrightPlatformEntities.CommandTimeout = 0;
            objBrightPlatformEntities.FIUpdateMergeList(MergeListIds, BlockedMergeListIds);
        }

        /// <summary>
        /// Save subcampaign account list records
        /// </summary>
        public static void SaveSubCampaignContactLists(string MergeListIds, string BlockedMergeListIds, int FinalListId)
        {
            using (BrightPlatformEntities objBrightPlatformEntities = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                objBrightPlatformEntities.FISaveSubCampaignContactList(MergeListIds, BlockedMergeListIds, FinalListId, UserSession.CurrentUser.UserId);
            }
        }

        /// <summary>
        /// Save subcampaign account list records
        /// </summary>
        public static void SaveSubCampaignAccountLists(string MergeListIds, string BlockedMergeListIds, int FinalListId)
        {
            BrightPlatformEntities objBrightPlatformEntities = new BrightPlatformEntities(UserSession.EntityConnection);
            objBrightPlatformEntities.CommandTimeout = 0;
            objBrightPlatformEntities.FISaveSubCampaignAccountList(MergeListIds, BlockedMergeListIds, FinalListId, UserSession.CurrentUser.UserId);
        }

        /// <summary>
        /// Save final call list
        /// </summary>
        public static int SaveFinalCallList(string FinalCallListName, int CustomerId, int CampaignId, int SubCampaignId)
        {
            int FinalCallListId = 0;
            BrightPlatformEntities objBrightPlatformEntities = new BrightPlatformEntities(UserSession.EntityConnection);
            var objItem = objBrightPlatformEntities.final_lists.Where(i => i.sub_campaign_id == SubCampaignId).FirstOrDefault();
            if (objItem != null)
            {
                objItem.modified_by = UserSession.CurrentUser.UserId;
                objItem.modified_on = System.DateTime.Now;
                objBrightPlatformEntities.SaveChanges();
                FinalCallListId = objItem.id;
            }
            else
            {
                final_lists objFinalCallList = new final_lists()
                {
                    list_name = FinalCallListName + " (final)",
                    customer_id = CustomerId,
                    campaign_id = CampaignId,
                    sub_campaign_id = SubCampaignId,
                    created_by = UserSession.CurrentUser.UserId,
                    created_on = System.DateTime.Now,
                    modified_by = UserSession.CurrentUser.UserId,
                    modified_on = System.DateTime.Now
                };

                objBrightPlatformEntities.final_lists.AddObject(objFinalCallList);
                objBrightPlatformEntities.SaveChanges();
                FinalCallListId = objFinalCallList.id;
            }

            return FinalCallListId;
        }

        /// <summary>
        /// Get subcampaign accounts and contacts records
        /// </summary>
        public static DataTable GetSubCampaignAccountsAndContacts(string MergeListIds, string BlockedMergeListIds)
        {
            SqlCommand objCommand = new SqlCommand("bvGetSubCampaignAccountAndContacts_sp");
            objCommand.Parameters.Add("@p_merge_list_ids", SqlDbType.NVarChar).Value = MergeListIds;
            objCommand.Parameters.Add("@p_merge_list_block_list_ids", SqlDbType.NVarChar).Value = BlockedMergeListIds;
            objCommand.CommandTimeout = 0;
            return DatabaseUtility.ExecuteSqlQuery(objCommand);
        }

        /// <summary>
        /// Get call list records
        /// </summary>
        public static List<CTCallList> GetCallLists(int CustomerId, int CampaignId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            return objDbModel.FIGetCallLists(CustomerId, CampaignId).ToList();
        }
        #endregion
    }
}
