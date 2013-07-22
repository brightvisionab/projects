
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using BrightVision.Common.Business;
using System.Data.Objects;

namespace SalesConsultant.Business
{
    public class SalesScript
    {
        #region Business Methods
        public static IList<IdName> GetUsers(int FinalListId, bool pAddTeamEntry = false)
        {
            List<IdName> _lstData = new List<IdName>();
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var objUsers = (
                    from objFinalList in _efDbContext.final_lists
                    join objSubCampaignUser in _efDbContext.subcampaign_users on objFinalList.sub_campaign_id equals objSubCampaignUser.subcampaign_id
                    join objUser in _efDbContext.users on objSubCampaignUser.user_id equals objUser.id
                    orderby objUser.fullname
                    select new IdName {
                        id = objUser.id,
                        name = objUser.fullname
                    }
                ).Distinct();

                var listResult = objUsers.ToList();
                IdName[] array = new IdName[listResult.Count];
                listResult.CopyTo(array);
                _lstData = array.ToList();
            }

            if (pAddTeamEntry)
                _lstData.Add(new IdName() {
                    id = 0,
                    name = "Team"
                });

            return _lstData; 
        }
        public static int GetDocument(int FinalListId, int UserId)
        {
            sub_campaign_user_sales_scripts _eftUserDocument = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftUserDocument = _efDbContext.sub_campaign_user_sales_scripts.FirstOrDefault(i => i.final_list_id == FinalListId && i.user_id == UserId);
                if (_eftUserDocument != null)
                    _efDbContext.Detach(_eftUserDocument);
            }

            if (_eftUserDocument == null)
                return 0;
            
            return (int)_eftUserDocument.id;
        }
        #endregion
    }
}
