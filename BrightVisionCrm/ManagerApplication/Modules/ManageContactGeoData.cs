
#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using ManagerApplication.Business;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;
using DevExpress.Data.Filtering;
using System.Globalization;
#endregion

namespace ManagerApplication.Modules
{
    public partial class ManageContactGeoData : UserControl
    {
        #region Enumerations
        /// <summary>
        /// Enum for fetch type
        /// </summary>
        private enum eFetchType
        {
            ByAll,
            BySelected
        }
        #endregion

        #region Private Members
        private BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
        private GridView m_objGridView = null;
        private GoogleMapUtility m_objGoogleMapUtility = new GoogleMapUtility();
        private string m_MessageBoxCaption = "Manager Application - Geo Match Companies";
        private CTGeographicalDataContact m_objContact = null;
        private ObjectGeographicalData.GeoDataInstance m_objGeoDataInstance = null;
        private bool m_PerformSearch = true;
        private bool m_ResetAccountPaging = true;
        private bool m_ReloadPage = true;
        #endregion

        #region Constructors
        public ManageContactGeoData()
        {
            this.Visible = false;
            InitializeComponent();
            this.lcManageContactGeoData.AllowCustomizationMenu = false;
            this.PopulateContactView(null, 1);
            this.SetContactViewContextMenu();
            this.Visible = true;
        }
        #endregion

        #region Object Control Events
        private void cbxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Selecting...");
            if (cbxSelectAll.Checked)
                this.SelectAll(true);
            else
                this.SelectAll(false);
           WaitDialog.Close();
        }

        private void cmdGeoMatchAll_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Geo matching contacts...");
            this.SelectAll(true);
            this.GetGeoData(eFetchType.ByAll);
           WaitDialog.Close();
        }

        private void cmdGeoMatchSelected_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(ParentForm, "Geo matching contacts...");
            this.GetGeoData(eFetchType.BySelected);
           WaitDialog.Close();
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Saving...");
            this.SaveGeoData();
            WaitDialog.Close();
        }

        private void miPrintPreview_Click(object sender, EventArgs e)
        {
            gcContact.ShowPrintPreview();
        }

        private void gvContact_ColumnFilterChanged(object sender, EventArgs e)
        {
            // filter to avoid executed twice
            if (m_PerformSearch)
            {
                WaitDialog.Show(ParentForm, "Searching...");
                this.PerformCustomSearch(1);
               WaitDialog.Close();
            }
        }

        private void cboPagingContact_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_ReloadPage)
            {
                WaitDialog.Show(ParentForm, "Loading...");
                m_ResetAccountPaging = false;

                if (!string.IsNullOrEmpty(gvContact.FindFilterText))
                {
                    m_PerformSearch = false;
                    this.PopulateContactView(gvContact.FindFilterText, Convert.ToInt32(cboPagingContact.Text));
                }
                else
                    this.PerformCustomSearch(Convert.ToInt32(cboPagingContact.Text));

               WaitDialog.Close();
            }
        }

        private void gvContact_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Apply pagination
        /// </summary>
        private void SetAccountPagination()
        {
            CTContactGeoDataCount objContactPagination = ObjectContact.GetContactGeoDataPageCount(gvContact.FindFilterText);
            cboPagingContact.Properties.Items.Clear();
            if (objContactPagination.page_count > 1)
            {
                cboPagingContact.Text = "1";
                for (int i = 1; i <= objContactPagination.page_count; i++)
                    cboPagingContact.Properties.Items.Add(i.ToString());
            }
        }

        /// <summary>
        /// Gets the filter criteria and re populate the customer grid view
        /// </summary>
        private void PerformCustomSearch(int PageNo)
        {
            
            m_PerformSearch = false;
            m_ReloadPage = false;
            this.PopulateContactView(gvContact.FindFilterText, PageNo);
            m_PerformSearch = true;
            m_ReloadPage = true;
           WaitDialog.Close();
        }

        /// <summary>
        /// Traverse all rows and save geo data
        /// </summary>
        private void SaveGeoData()
        {
            //SqlConnection objConnection = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString());
            SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection);
            objConnection.Open();

            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcContact.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                m_objContact = null;
                m_objContact = (CTGeographicalDataContact)m_objGridView.GetRow(i);

                if ((bool)m_objContact.include && (m_objContact.geo_latitude != 0 && m_objContact.geo_longitude != 0))
                {
                    m_objGeoDataInstance = null;
                    m_objGeoDataInstance = new ObjectGeographicalData.GeoDataInstance();
                    m_objGeoDataInstance.table_source = (int)ObjectGeographicalData.eTableSource.Contacts;
                    m_objGeoDataInstance.table_id = m_objContact.id;
                    m_objGeoDataInstance.latitude = (decimal)m_objContact.geo_latitude;
                    m_objGeoDataInstance.longitude = (decimal)m_objContact.geo_longitude;
                    ObjectGeographicalData.SaveGeoData(m_objGeoDataInstance, objConnection);
                }
            }

            objConnection.Close();
            objConnection = null;

            //this.PopulateContactView(null, 1);
            MessageBox.Show("Successfully updated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Traverse all rows and get geo data
        /// </summary>
        private void GetGeoData(eFetchType FetchType)
        {
            double Latitude = 0;
            double Longitude = 0;
            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcContact.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                m_objContact = null;
                m_objContact = (CTGeographicalDataContact)m_objGridView.GetRow(i);

                if (FetchType == eFetchType.BySelected && (bool)!m_objContact.include)
                    continue;

                string strAddress = m_objContact.address + ", " + m_objContact.city + " " + m_objContact.zip + ", " + m_objContact.country;
                if (string.IsNullOrEmpty(strAddress) || !GoogleMapUtility.IsValidGeoAddress(strAddress))
                    continue;

                string[] objGeoData = m_objGoogleMapUtility.GetGeographicalData(strAddress).Split(',');

                /**
                 * where: 
                 * objGeoData[2] = latitude
                 * objGeoData[3] = longitude
                 */

                Latitude = 0;
                Longitude = 0;

                if (objGeoData[2] != null)
                    if (ValidationUtility.IsCurrency(objGeoData[2]))
                        Latitude = Convert.ToDouble(objGeoData[2], CultureInfo.InvariantCulture);

                if (objGeoData[3] != null)
                    if (ValidationUtility.IsCurrency(objGeoData[3]))
                        Longitude = Convert.ToDouble(objGeoData[3], CultureInfo.InvariantCulture);

                // display geo data on grid
                m_objGridView.SetRowCellValue(i, "geo_latitude", Latitude.ToString());
                m_objGridView.SetRowCellValue(i, "geo_longitude", Longitude.ToString());
                m_objGridView.SetRowCellValue(i, "geo_status", objGeoData[2].Equals("0") && objGeoData[3].Equals("0") ? "not found" : "found");
            }
        }

        /// <summary>
        /// Traverse all rows and set include checkbox
        /// </summary>
        private void SelectAll(bool IsAll)
        {
            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcContact.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                if (IsAll)
                    m_objGridView.SetRowCellValue(i, "include", true);
                else
                    m_objGridView.SetRowCellValue(i, "include", false);
            }
        }

        /// <summary>
        /// Pupulates the contact grid view contents
        /// </summary>
        private void PopulateContactView(string FilterCriteria, int PageNo)
        {
            try
            {
                gcContact.DataSource = null;
                gcContact.DataSource = ObjectContact.GetGeoMatchedContacts(FilterCriteria, PageNo);

                if (m_ResetAccountPaging)
                    this.SetAccountPagination();

                m_ResetAccountPaging = true;
                m_PerformSearch = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetContactViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miPrintPreview = new MenuItem("Print Preview");
            miPrintPreview.Click += new EventHandler(miPrintPreview_Click);
            objClickMenu.MenuItems.Add(miPrintPreview);
            gcContact.ContextMenu = objClickMenu;
        }
        #endregion
    }
}
