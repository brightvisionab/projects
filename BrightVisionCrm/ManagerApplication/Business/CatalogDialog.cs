using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.Data.Objects;

namespace ManagerApplication.Business
{
    public class ObjectDialog
    {
        #region Classes
        #endregion

        #region Business Methods
        /// <summary>
        /// Gets the dialog records
        /// </summary>
        public static ObjectQuery GetDialogs()
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objDialogs = 
                from objDialog in m_objBrightPlatformEntity.dialogs
                orderby objDialog.name
                select new
                {
                    id = objDialog.id,
                    name = objDialog.name
                };

            return (ObjectQuery) objDialogs;
        }

        /// <summary>
        /// Check if already exists, must implement 1 dialog = 1 sub campaign relationship
        /// </summary>
        /// <param name="DialogName"></param>
        /// <param name="SubCampaignId"></param>
        /// <returns></returns>
        public static bool Exists(string DialogName, int SubCampaignId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var Item = objDbModel.dialogs.FirstOrDefault(i => i.name.Equals(DialogName.Trim()) && i.subcampaign_id == SubCampaignId && i.is_active == true);
            if (Item == null)
                return false;
            else
                return true;
        }

        public static bool CanAddDialog(int SubCampaignId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var Item = objDbModel.dialogs.FirstOrDefault(i => i.subcampaign_id == SubCampaignId && i.is_active == true);
            if (Item == null)
                return true;
            else
                return false;
        }

        public static bool Exists(string DialogName, int SubCampaignId, int dialogId)
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            var Item = objDbModel.dialogs.FirstOrDefault(i => i.name.Equals(DialogName.Trim()) && i.subcampaign_id == SubCampaignId && i.is_active == true);
            if (Item != null)
            {
                if (Item.id == dialogId)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }
        #endregion
    }
}
