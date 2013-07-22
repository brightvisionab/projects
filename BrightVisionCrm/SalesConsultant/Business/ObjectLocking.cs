
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace SalesConsultant.Business 
{
    public class ObjectLocking 
    {
        public static void ReleaseUserLock()
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efDbContext.FIReleaseCampaignLocks(UserSession.CurrentUser.UserId, null, null);
            }
        }
        public static void ReleaseLock(int pFinalListId)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efDbContext.FIReleaseCampaignLocks(UserSession.CurrentUser.UserId, pFinalListId, null);
            }
        }
        public static sub_campaign_account_lists ReleaseLock(int pFinalListId, int pAccountId)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _efDbContext.FIReleaseCampaignLocks(UserSession.CurrentUser.UserId, pFinalListId, pAccountId);
            }

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                return _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.final_list_id == pFinalListId && i.account_id == pAccountId);
            }
        }
    }
}
