
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


namespace ManagerApplication.Modules
{
    public partial class AddSubCampaignConfigTemplate : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public AddSubCampaignConfigTemplate()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Events & Args
        public delegate void AfterSaveEventHandler(int pConfigTemplateId);
        public event AfterSaveEventHandler AfterSave;
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private BrightPlatformEntities m_efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        #region Object Events
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (tbxName.Text.Length < 1)
                return;

            if (m_efDbModel.sub_campaign_configuration_templates.FirstOrDefault(i => i.name == tbxName.Text) != null) {
                BrightVision.Common.UI.NotificationDialog.Information("Bright Manager", "Config name already exist.");
                return;
            }
            else if (string.IsNullOrEmpty(tbxXmlData.Text)) {
                BrightVision.Common.UI.NotificationDialog.Information("Bright Manager", "Please kindly specify your config data.");
                return;
            }

            WaitDialog.Show("Saving config template...");
            sub_campaign_configuration_templates _item = new sub_campaign_configuration_templates() {
                name = tbxName.Text,
                type = cboType.Text,
                template = tbxXmlData.Text,
                is_default = false,
                created_by = UserSession.CurrentUser.UserId,
                created_on = DateTime.Now
            };
            m_efDbModel.sub_campaign_configuration_templates.AddObject(_item);
            m_efDbModel.SaveChanges();

            if (AfterSave != null)
                AfterSave(_item.id);

            WaitDialog.Close();
            this.ParentForm.Close();
        }
        #endregion
    }
}
