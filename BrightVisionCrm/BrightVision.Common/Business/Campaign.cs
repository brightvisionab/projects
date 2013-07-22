using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using BrightVision.Common.Business;

namespace BrightVision.Common.Business
{
    public class ObjectCampaign
    {
        #region Enumerations
        /// <summary>
        /// View type selector
        /// </summary>
        public enum eViewtype
        {
            //SubCampaignsView,
            ComboListView,
            ComboListViewByCustomerId
        }
        #endregion

        #region Classes
        /// <summary>
        /// Gets or sets campaign instance
        /// </summary>
        public class CampaignInstance
        {
            public int id { get; set; }
            public string campaign_name { get; set; }
            public string owner_name { get; set; }
            public string web_access_name { get; set; }
            public byte priority { get; set; }
            public string status { get; set; }
            public string description { get; set; }
            public int owner_user_id { get; set; }
            public int web_access_user_id { get; set; }
            public int customer_id { get; set; }
        }
        #endregion

        #region Business Methods
        /// <summary>
        /// get active campaigns by specific customer and user
        /// </summary>
        //public static ObjectQuery GetActiveCampaigns(int CustomerId)
        //{
        //    BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        //    var objCampaigns =
        //    (
        //        from objSubCampaign in m_objBrightPlatformEntity.subcampaigns
        //        join objCampaign in m_objBrightPlatformEntity.campaigns on objSubCampaign.campaign_id equals objCampaign.id
        //        join objEntitySubCampaignUser in m_objBrightPlatformEntity.subcampaign_users on objSubCampaign.id equals objEntitySubCampaignUser.subcampaign_id into objEntitySubCampaignUser
        //        from objSubCampaignUser in objEntitySubCampaignUser.DefaultIfEmpty()
        //        join objUserRole in m_objBrightPlatformEntity.sub_campaign_user_roles on objSubCampaignUser.id equals objUserRole.sub_campaign_user_id
        //        where
        //            objSubCampaign.status != "Deleted" &&
        //            objCampaign.customer_id == CustomerId &&
        //            objUserRole.sub_campaign_role_id == 3 && // 3 = Sub Campaign Sales User
        //            objSubCampaignUser.user_id == UserSession.CurrentUser.UserId &&
        //            objSubCampaignUser.internal_user == true

        //        orderby objCampaign.campaign_name
        //        select new
        //        {
        //            id = objCampaign.id,
        //            name = objCampaign.campaign_name
        //        }

        //    ).Distinct();

        //    return (ObjectQuery) objCampaigns;
        //}

        /// <summary>
        /// Gets sub campaign only for a specific customer
        /// </summary>
        public static ObjectQuery GetCustomerContactCampaigns(int CustomerId)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityCampaigns =
                from objCampaign in objBrightPlatformEntity.campaigns
                where objCampaign.customer_id == CustomerId
                orderby objCampaign.campaign_name
                select new
                {
                    id = objCampaign.id,
                    name = objCampaign.campaign_name
                };

            return (ObjectQuery)objEntityCampaigns;
        }

        /// <summary>
        /// Gets sub campaign only for a specific user and customer
        /// </summary>
        public static ObjectQuery GetCampaigns(int CustomerId, int UserId)
        {
            BrightPlatformEntities objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityCampaigns =
            (
                from objCampaign in objBrightPlatformEntity.campaigns
                join objSubCampaign in objBrightPlatformEntity.subcampaigns on objCampaign.id equals objSubCampaign.campaign_id
                join objSubCampaignUser in objBrightPlatformEntity.subcampaign_users on objSubCampaign.id equals objSubCampaignUser.subcampaign_id
                where objCampaign.customer_id == CustomerId && objSubCampaignUser.user_id == UserId && objSubCampaignUser.internal_user == true //&& (objCampaign.assigned_to == UserId || objCampaign.created_by == UserId || objCampaign.owned_by == UserId)
                orderby objCampaign.campaign_name
                select new
                {
                    id = objCampaign.id,
                    name = objCampaign.campaign_name
                }
            ).Distinct();

            return (ObjectQuery) objEntityCampaigns;
        }

        /// <summary>
        /// Gets campaign records returns an object query result set
        /// </summary>
        public static ObjectQuery GetCampaigns(eViewtype ViewType, int CustomerId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            ObjectQuery objCampaigns = null;

            switch (ViewType)
            {
                //case eViewtype.SubCampaignsView:
                //{
                //    var objEntityCampaigns = 
                //        from objCampaign in m_objBrightPlatformEntity.campaigns
                //        join objEntityCustomer in m_objBrightPlatformEntity.customers on objCampaign.customer_nr equals objEntityCustomer.customer_nr into objEntityCustomer
                //        from objCustomer in objEntityCustomer.DefaultIfEmpty()
                //        select new
                //        {
                //            ids = SqlFunctions.StringConvert((double) objCampaign.id).Trim() + ";" + objCustomer.customer_name,
                //            name = objCampaign.campaign_name
                //        };

                //    objCampaigns = (ObjectQuery) objEntityCampaigns;
                //    break;
                //}

                case eViewtype.ComboListView:
                {
                    var objEntityCampaigns =
                        from objCampaign in m_objBrightPlatformEntity.campaigns
                        orderby objCampaign.campaign_name
                        select new
                        {
                            id = objCampaign.id,
                            name = objCampaign.campaign_name
                        };

                    objCampaigns = (ObjectQuery) objEntityCampaigns;
                    break;
                }

                case eViewtype.ComboListViewByCustomerId:
                {
                    var objEntityCampaigns =
                        from objCampaign in m_objBrightPlatformEntity.campaigns
                        where objCampaign.customer_id == CustomerId
                        orderby objCampaign.campaign_name
                        select new
                        {
                            id = objCampaign.id,
                            name = objCampaign.campaign_name
                        };

                    objCampaigns = (ObjectQuery)objEntityCampaigns;
                    break;
                }
            }

            return objCampaigns;
        }

        /// <summary>
        /// Gets campaign records returns an object query result set
        /// </summary>
        public static ObjectQuery GetCampaigns(int CustomerId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCampaigns = 
                from objCampaign in m_objBrightPlatformEntity.campaigns
                join objEntityOwner in m_objBrightPlatformEntity.users on objCampaign.owned_by equals objEntityOwner.id into objEntityOwner
                from objOwner in objEntityOwner.DefaultIfEmpty()
                join objEntityWebAccess in m_objBrightPlatformEntity.users on objCampaign.assigned_to equals objEntityWebAccess.id into objEntityWebAccess
                from objWebAccess in objEntityWebAccess.DefaultIfEmpty()
                where objCampaign.customer_id == CustomerId && objCampaign.campaign_name != ""
                orderby objCampaign.campaign_name
                select new ObjectCampaign.CampaignInstance
                {
                    id = objCampaign.id,
                    campaign_name = objCampaign.campaign_name,
                    owner_name = objOwner.fullname,
                    web_access_name = objWebAccess.fullname,
                    priority = (byte)(objCampaign.priority == null ? 0 : objCampaign.priority),
                    status = objCampaign.status,
                    description = objCampaign.description,
                    //owner_user_id = (int)(objCampaign.created_by == null ? 0 : objCampaign.created_by),
                    owner_user_id = (int)(objCampaign.owned_by == null ? 0 : objCampaign.owned_by),
                    web_access_user_id = (int)(objCampaign.assigned_to == null ? 0 : objCampaign.assigned_to),
                    customer_id = objCampaign.customer_id
                };

            return (ObjectQuery) objCampaigns;
        }

        /// <summary>
        /// Save campaign record
        /// </summary>
        public static void SaveCampaign(bool IsNew, CampaignInstance objParams)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCampaign = m_objBrightPlatformEntity.campaigns.CreateObject();
            if (!IsNew)
            {
                objCampaign = null;
                objCampaign = m_objBrightPlatformEntity.campaigns.Where(i => i.id == objParams.id).SingleOrDefault();
            }
            else
                objCampaign.created_date = DateTime.Now;

            objCampaign.campaign_name = objParams.campaign_name;
            objCampaign.customer_id = objParams.customer_id;
            objCampaign.owned_by = objParams.owner_user_id;
            objCampaign.created_by = UserSession.CurrentUser.UserId;
            objCampaign.description = objParams.description;
            objCampaign.status = objParams.status;
            objCampaign.priority = objParams.priority;
            objCampaign.assigned_to = objParams.web_access_user_id;
            objCampaign.modified_by = UserSession.CurrentUser.UserId;
            objCampaign.modified_date = DateTime.Now;

            if (IsNew)
                m_objBrightPlatformEntity.campaigns.AddObject(objCampaign);

            m_objBrightPlatformEntity.SaveChanges();
        }
        #endregion
    }
}
