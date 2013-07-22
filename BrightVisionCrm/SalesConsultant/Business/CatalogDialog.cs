using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.Data.Objects;

namespace SalesConsultant.Business
{
    public class ObjectDialog
    {
        #region Business Methods
        public static ObjectQuery GetDialogs()
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                var objDialogs =
                    from objDialog in _efDbContext.dialogs
                    orderby objDialog.name
                    select new {
                        id = objDialog.id,
                        name = objDialog.name
                    };

                return (ObjectQuery)objDialogs;
            }
        }
        public static bool Exists(string pDialogName, int pSubCampaignId)
        {
            dialog _eftDialog = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftDialog = _efDbContext.dialogs.FirstOrDefault(i => i.name.Equals(pDialogName.Trim()) && i.subcampaign_id == pSubCampaignId && i.is_active == true);
                if (_eftDialog != null) {
                    _efDbContext.Detach(_eftDialog);
                    return true;
                }
            }

            return false;
        }
        public static bool CanAddDialog(int pSubCampaignId)
        {
            dialog _eftDialog = null;
            using (BrightPlatformEntities _eftDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftDialog = _eftDbContext.dialogs.FirstOrDefault(i => i.subcampaign_id == pSubCampaignId && i.is_active == true);
                if (_eftDialog != null) {
                    _eftDbContext.Detach(_eftDialog);
                    return true;
                }
            }
            return false;
        }
        public static bool Exists(string pDialogName, int pSubCampaignId, int pDialogId)
        {
            dialog _eftDialog = null;
            using (BrightPlatformEntities _eftDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftDialog = _eftDbContext.dialogs.FirstOrDefault(i => i.name.Equals(pDialogName.Trim()) && i.subcampaign_id == pSubCampaignId && i.is_active == true);
                if (_eftDialog != null) {
                    _eftDbContext.Detach(_eftDialog);
                    if (_eftDialog.id == pDialogId)
                        return false;

                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
