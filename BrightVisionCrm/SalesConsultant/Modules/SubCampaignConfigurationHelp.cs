
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SalesConsultant.Modules
{
    public partial class SubCampaignConfigurationHelp : DevExpress.XtraEditors.XtraUserControl
    {
        public SubCampaignConfigurationHelp()
        {
            InitializeComponent();
            tbxHelp.Text = SalesConsultant.Properties.Resources.sub_campaign_config_settings_help;
        }
    }
}
