
#region Namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.Objects;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors;
using SalesConsultant.Business;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
#endregion

namespace SalesConsultant.Modules
{
    public partial class BvStandardQuestion : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructor
        public BvStandardQuestion()
        {
            InitializeComponent();
            gcStandardQuestion.DataSource = null;
        }
        #endregion

        #region Public Properties
        public ObjectResult StandardQuestionData { get; set; }
        #endregion

        #region Private Members
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        #endregion

        #region Public Methods
        public void Show(int pSubCampaignId, int pAccountId, int pContactId)
        {
            try {
                this.Enabled = false;
                StandardQuestionData = null;
                gcStandardQuestion.DataSource = null;
                StandardQuestionData = StandardQuestion.GetStandardQuestions(pSubCampaignId, pAccountId, pContactId);
                if (StandardQuestionData == null)
                    return;

                gcStandardQuestion.DataSource = StandardQuestionData;
                gvStandardQuestion.Columns["question"].Group();
                gvStandardQuestion.Columns["campaign"].Group();
                gvStandardQuestion.Columns["created_date"].Group();
                gvStandardQuestion.ExpandAllGroups();
                this.Enabled = true;
            }
            catch (Exception e) {
                NotificationDialog.Error("Bright Sales", e.InnerException.Message);
            }
        }
        public void SetAsReadOnly(bool pState)
        {
            btnCalculate.Enabled = !pState;
        }
        public void Clear()
        {
            gcStandardQuestion.DataSource = null;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Object Events
        private void gvStandardQuestion_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            SalesConsultant.Business.BrightSalesGridUtility.CreateGridContextMenu(view, e);
        }
        #endregion
    }
}
