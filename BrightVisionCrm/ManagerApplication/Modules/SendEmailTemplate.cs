
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using BrightVision.Common.UI;
using BrightVision.Model;
using BrightVision.Common.Business;

using DevExpress.XtraEditors;

namespace ManagerApplication.Modules
{
    public partial class SendEmailTemplate : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public SendEmailTemplate()
        {
            InitializeComponent();
        }
        public SendEmailTemplate(int pSubCampaignId)
        {
            InitializeComponent();
            m_SubCampaignId = pSubCampaignId;
            this.GetDefaultConfig();
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
        private void GetDefaultConfig()
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                subcampaign _eftSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == m_SubCampaignId);
                if (_eftSubCampaign != null) {
                    //if (!string.IsNullOrEmpty(_eftSubCampaign.send_email_config)) {
                    //    string[] _data = _eftSubCampaign.send_email_config.Split(new string[] { "[sep]" }, StringSplitOptions.None);
                    //    tbxSubject.Text = _data[0];
                    //    tbxMessage.Text = _data[1];
                    //}
                    _efDbContext.Detach(_eftSubCampaign);
                }
            }
        }
        #endregion

        #region Control Events
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbxSubject.Text)) {
                NotificationDialog.Information("Bright Manager", "Please specify a default send email suject.");
                return;
            }
            else if (string.IsNullOrEmpty(tbxMessage.Text)) {
                NotificationDialog.Information("Bright Manager", "Please specify a default send email message.");
                return;
            }

            /**
             * build the subject and message into one text only,
             * separated by a separator text "[sep]".
             */
            string _SendEmailConfig = string.Format("{0}[sep]{1}", 
                tbxSubject.Text,
                tbxMessage.Text
            );

            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                subcampaign _eftSubCampaign = _efDbContext.subcampaigns.FirstOrDefault(i => i.id == m_SubCampaignId);
                if (_eftSubCampaign != null) {
                    //_eftSubCampaign.send_email_config = _SendEmailConfig;
                    _efDbContext.subcampaigns.ApplyCurrentValues(_eftSubCampaign);
                    _efDbContext.SaveChanges();
                    _efDbContext.Detach(_eftSubCampaign);
                }
            }

            NotificationDialog.Information("Bright Manager", "Saved send email config.");
            this.ParentForm.Close();
        }
        #endregion
    }
}
