
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using System.Data.Objects;

namespace BrightVision.Common.Business
{
    public class ExportView
    {
        public enum eExportViewDisplayMode
        {
            Default, //AccountsAndContactsHavingDialogData,
            AccountsAndContactsHavingSubCampaignCallAttemps,
        }

        #region Business Methods
        public static List<CTAdditionalDataReportTemplate> GetAdditionalDataReportTemplates()
        {
            List<CTAdditionalDataReportTemplate> list = new List<CTAdditionalDataReportTemplate>();
            using (BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection) { CommandTimeout = 0 }) {
                list = _efDbModel.FIGetAdditionalDataReportTemplates().ToList();
            }
            return list;
        }
        #endregion
    }
}
