
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using DevExpress.XtraEditors;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules
{
    public partial class EditorNurtureSetting : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public EditorNurtureSetting(int pCustomerId, int pCampaignId, int pSubCampaignId)
        {
            InitializeComponent();
            this.GetData(pCustomerId, pCampaignId, pSubCampaignId);
        }
        public EditorNurtureSetting()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Events & Args
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private int m_SubCampaignId = 0;
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void GetData(int pCustomerId, int pCampaignId, int pSubCampaignId)
        {
            m_SubCampaignId = pSubCampaignId;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                gcNurtureSubCampaign.DataSource = null;
                gcNurtureSubCampaign.DataSource = _efDbContext.FIGetSubCampaignNurtureList(pCustomerId, pCampaignId, pSubCampaignId).ToList();
            }
        }
        #endregion

        #region Control Events
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (gvNurtureSubCampaign.RowCount < 1)
                return;

            WaitDialog.Show("Saving data.");
            List<string> _lstIds = new List<string>();
            for (int i = 0; i < gvNurtureSubCampaign.RowCount; i++) {
                CTSubCampaignNurtureItem _item = gvNurtureSubCampaign.GetRow(i) as CTSubCampaignNurtureItem;
                if (!Convert.ToBoolean(_item.selected))
                    continue;

                _lstIds.Add(string.Format("<item id=\"{0}\" />", _item.sub_campaign_id));
            }

            if (_lstIds.Count > 0) {
                string _xml = string.Format("<sub_campaign_config><nurture_sub_campaign>{0}</nurture_sub_campaign></sub_campaign_config>", string.Join("", _lstIds.ToArray()));
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                    subcampaign _efSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == m_SubCampaignId);
                    if (_efSubCampaign != null) {
                        _efSubCampaign.xml_nurture_setting = _xml;
                        _efDbContext.subcampaigns.ApplyCurrentValues(_efSubCampaign);
                        _efDbContext.SaveChanges();
                        _efDbContext.Detach(_efSubCampaign);
                    }
                }
            }

            WaitDialog.Close();
            this.ParentForm.Close();
        }
        #endregion
    }
}
