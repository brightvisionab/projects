
#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using SalesConsultant.Business;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using SalesConsultant.Forms;
#endregion

namespace SalesConsultant.Modules
{
    public partial class ManageCustomerCampaign : DevExpress.XtraEditors.XtraUserControl
    {
        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private GridView m_objGridView = null;
        private ObjectCustomer.CustomerInstance m_objCustomer = null;
        private ObjectCampaign.CampaignInstance m_objCampaign = null;
        private AddCustomer m_objAddCustomerForm = null;
        private AddCampaign m_objAddCampaignForm = null;
        private PopupDialog m_objPopupDialog = null;
        private string m_MessageBoxCaption = "Manager Application - Customers & Campaigns";
        #endregion

        #region Constructors
        public ManageCustomerCampaign()
        {
            this.Visible = false;
            InitializeComponent();
            this.lcCustomerCampaign.AllowCustomizationMenu = false;
            this.PopulateCustomerView();
            this.SetCustomerViewContextMenu();
            this.SetCampaignViewContextMenu();
            this.Visible = true;
        }
        #endregion

        #region Object Control Events
        private void gvCustomer_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            this.SetFocusedViewInstance();
            this.PopulateCampaignView(m_objCustomer.id);
        }

        private void riChkCustomerActive_CheckStateChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckEdit objCheckBox = sender as DevExpress.XtraEditors.CheckEdit;
            if (objCheckBox.Checked)
            {
                this.SetCustomerStatus(true);
                MessageBox.Show("Customer " + m_objCustomer.customer_name + " activated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.SetCustomerStatus(false);
                MessageBox.Show("Customer " + m_objCustomer.customer_name + " de-activated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cmdAddCustomer_Click(object sender, EventArgs e)
        {
            this.DisplayCustomerForm(true);
        }

        private void gvCustomer_DoubleClick(object sender, EventArgs e)
        {
            this.SetFocusedViewInstance();
            this.DisplayCustomerForm(false);
        }

        private void gvCustomerCampaign_DoubleClick(object sender, EventArgs e)
        {
            this.SetFocusedViewInstance();
            this.DisplayCampaignForm(false);
        }

        private void cmdAddCampaign_Click(object sender, EventArgs e)
        {
            this.DisplayCampaignForm(true);
        }

        private void miCustomerPrintPreview_Click(object sender, EventArgs e)
        {
            gcCustomer.ShowPrintPreview();
        }

        private void miCampaignPrintPreview_Click(object sender, EventArgs e)
        {
            gcCustomerCampaign.ShowPrintPreview();
        }

        private void gvCustomer_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void gvCustomerCampaign_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Pupulates the customer grid view contents
        /// </summary>
        public void PopulateCustomerView()
        {
            try
            {
                gcCustomer.DataSource = null;

                if (UserSession.CurrentUser.IsManagerAdmin)
                    gcCustomer.DataSource = ObjectCustomer.GetCustomers(ObjectCustomer.eViewType.CustomerCampaign_ManagerAdmin).Execute(MergeOption.AppendOnly);
                else if (UserSession.CurrentUser.IsManagerUser)
                    gcCustomer.DataSource = ObjectCustomer.GetCustomers(ObjectCustomer.eViewType.CustomerCampaign_ManagerUser).Execute(MergeOption.AppendOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Pupulates the campaign grid view contents
        /// </summary>
        public void PopulateCampaignView(int CustomerId)
        {
            try
            {
                gcCustomerCampaign.DataSource = null;
                gcCustomerCampaign.DataSource = ObjectCampaign.GetCampaigns(CustomerId).Execute(MergeOption.AppendOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Sets the focused view instance of the grid. Instantiates the objects that can be used for data manipulation.
        /// Where gcCustomer, is the current selected object in the grid view
        /// </summary>
        private void SetFocusedViewInstance()
        {
            m_objGridView = null;
            m_objCustomer = null;
            m_objCampaign = null;

            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcCustomer.FocusedView;
            m_objCustomer = (ObjectCustomer.CustomerInstance) m_objGridView.GetFocusedRow();

            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView) gcCustomerCampaign.FocusedView;
            m_objCampaign = (ObjectCampaign.CampaignInstance) m_objGridView.GetFocusedRow();
        }

        /// <summary>
        /// Sets the customer's status (e.g. active, inactive)
        /// </summary>
        private void SetCustomerStatus(bool IsActivated)
        {
            ObjectCustomer.SetCustomerStatus(m_objCustomer.id, IsActivated);
        }

        /// <summary>
        /// Initializes the popup dialog for customer entry and loads the popup dialog
        /// </summary>
        private void DisplayCustomerForm(bool IsNew)
        {
            //if (IsNew)
            //{
            //    m_objAddCustomerForm = new AddCustomer();
            //    m_objAddCustomerForm.isNew = true;
            //}
            //else
            //{
            //    m_objAddCustomerForm = new AddCustomer(AddCustomer.SaveType.SaveTypeEdit, m_objCustomer);
            //    m_objAddCustomerForm.isNew = false;
            //}

            //m_objAddCustomerForm.m_objParentControl = this;
            //m_objPopupDialog = new PopupDialog();
            //m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            //m_objPopupDialog.MinimizeBox = false;
            //m_objPopupDialog.MaximizeBox = false;
            //m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            //m_objPopupDialog.Text = "Manager Application - Customers & Campaigns";
            //m_objPopupDialog.Controls.Add(m_objAddCustomerForm);
            //m_objPopupDialog.ClientSize = new Size(m_objAddCustomerForm.Width + 2, m_objAddCustomerForm.Height + 2);
            //m_objPopupDialog.ShowDialog(this.ParentForm);
        }

        /// <summary>
        /// Initializes the popup dialog for campaign entry and loads the popup dialog
        /// </summary>
        private void DisplayCampaignForm(bool IsNew)
        {
            //if (IsNew)
            //{
            //    m_objAddCampaignForm = new AddCampaign();
            //    m_objAddCampaignForm.isNew = true;
            //}
            //else
            //{
            //    m_objAddCampaignForm = new AddCampaign(AddCampaign.SaveType.SaveTypeEdit, m_objCampaign);
            //    m_objAddCampaignForm.isNew = false;
            //}

            m_objAddCampaignForm = new AddCampaign(AddCampaign.SaveType.SaveTypeEdit, m_objCampaign);
            m_objAddCampaignForm.IsNew = false;
            m_objAddCampaignForm.CustomerId = m_objCustomer.id;
            m_objAddCampaignForm.m_objParentControl = this;
            m_objPopupDialog = new PopupDialog();
            m_objPopupDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            m_objPopupDialog.MinimizeBox = false;
            m_objPopupDialog.MaximizeBox = false;
            m_objPopupDialog.StartPosition = FormStartPosition.CenterScreen;
            m_objPopupDialog.Text = "Manager Application - Customers & Campaigns";
            m_objPopupDialog.Controls.Add(m_objAddCampaignForm);
            m_objPopupDialog.ClientSize = new Size(m_objAddCampaignForm.Width + 2, m_objAddCampaignForm.Height + 2);
            m_objPopupDialog.ShowDialog(this.ParentForm);
        }

        /// <summary>
        /// Set grid right click context menu for customers
        /// </summary>
        private void SetCustomerViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miCustomerPrintPreview = new MenuItem("Print Preview");
            miCustomerPrintPreview.Click += new EventHandler(miCustomerPrintPreview_Click);
            objClickMenu.MenuItems.Add(miCustomerPrintPreview);
            gcCustomer.ContextMenu = objClickMenu;
        }

        /// <summary>
        /// Set grid right click context menu for campaigns
        /// </summary>
        private void SetCampaignViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miCampaignPrintPreview = new MenuItem("Print Preview");
            miCampaignPrintPreview.Click += new EventHandler(miCampaignPrintPreview_Click);
            objClickMenu.MenuItems.Add(miCampaignPrintPreview);
            gcCustomerCampaign.ContextMenu = objClickMenu;
        }
        #endregion
    }
}
