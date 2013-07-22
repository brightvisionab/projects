using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using DevExpress.XtraGrid.Views.Grid;
using System.Linq;

using BrightVision.Model;
using BrightVision.Common.Business;

using BrightVision.Reporting;
using BrightVision.Reporting.UI;
using BrightVision.Common.UI;
using BrightVision.Common.Utilities;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;

namespace SalesConsultant.Modules
{
    public partial class Release : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public Release()
        {
            InitializeComponent();
        }
        #endregion

        #region Private Properties
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        private class ViewCofigData {
            public int id { get; set; }
            public int subcampaign_id { get; set; }
            public string name { get; set; }
        }
        #endregion

        #region Control Events
        private void Release_Load(object sender, EventArgs e)
        {
            this.LoadContacts();
        }
        #endregion

        #region Private Methods
        private void LoadContacts()
        {
            gcContact.DataSource = null;
            using (BrightPlatformEntities _efDbCOntext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                gcContact.DataSource = _efDbCOntext.FIGetSubCampaignEmailContacts(
                    m_BrightSalesProperty.CommonProperty.SubCampaignId,
                    m_BrightSalesProperty.CommonProperty.CurrentWorkedAccountId,
                    m_BrightSalesProperty.CommonProperty.FinalListId
                );
                gvContact.BestFitColumns();
            }

            //ContactView cv = ContactView.Instance();
            //gcContact.DataSource = cv.gcContact.DataSource;
        }
        #endregion
    }
}
