
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using BrightVision.Common.Business;
using System.Data.Objects;

namespace SalesConsultant.Business
{
    public class StandardQuestion
    {
        #region Business Methods
        public static ObjectResult GetStandardQuestions(int pSubCampaignId, int pAccountId, int pContactId)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                return _efDbContext.FIScGetStandardQuestions(pSubCampaignId, pAccountId, pContactId);
            }
        }
        #endregion
    }
}
