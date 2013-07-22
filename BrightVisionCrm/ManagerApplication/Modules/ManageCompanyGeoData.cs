
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
using DevExpress.XtraGrid;
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
    public partial class ManageCompanyGeoData : UserControl
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
        private CTGetGeographicalDataCompany m_objCompany = null;
        private ObjectGeographicalData.GeoDataInstance m_objGeoDataInstance = null;
        private GoogleMapUtility m_objGoogleMapUtility = new GoogleMapUtility();
        private string m_MessageBoxCaption = "Manager Application - Geo Match Companies";
        private bool m_PerformSearch = true;
        private bool m_ResetAccountPaging = true;
        private bool m_ReloadPage = true;
        #endregion

        #region Constructors
        public ManageCompanyGeoData()
        {
            this.Visible = false;
            InitializeComponent();
            this.lcManageCompanyGeoData.AllowCustomizationMenu = false;
            this.PopulateCompanyView(null, 1);
            this.SetCompanyViewContextMenu();
            this.Visible = true;
        }
        #endregion

        #region Object Control Events
        private void cmdGeoMatchAllCompanies_Click(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Loading geo data..");
            this.SelectAll(true);
            this.GetGeoData(eFetchType.ByAll);
            WaitDialog.Close();
        }

        private void cmdGeoMatchSelectedCompanies_Click(object sender, EventArgs e)
        {

            WaitDialog.Show(ParentForm, "Loading geo data..");
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
            gcCompany.ShowPrintPreview();
        }

        private void cbxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            
            WaitDialog.Show(ParentForm, "Loading...");
            if (cbxSelectAll.Checked)
                this.SelectAll(true);
            else
                this.SelectAll(false);
            WaitDialog.Close();
        }

        private void gvCompany_ColumnFilterChanged(object sender, EventArgs e)
        {
            // filter to avoid executed twice
            if (m_PerformSearch)
            {
                
                WaitDialog.Show(ParentForm, "Searching...");
                this.PerformCustomSearch(1);
                WaitDialog.Close();
            }
        }

        private void cboPagingAccount_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_ReloadPage)
            {
                
                WaitDialog.Show(ParentForm, "Loading...");
                m_ResetAccountPaging = false;
                cbxSelectAll.Checked = true;

                if (!string.IsNullOrEmpty(gvCompany.FindFilterText))
                {
                    m_PerformSearch = false;
                    this.PopulateCompanyView(gvCompany.FindFilterText, Convert.ToInt32(cboPagingAccount.Text));
                }
                else
                    this.PerformCustomSearch(Convert.ToInt32(cboPagingAccount.Text));

                WaitDialog.Close();
            }
        }

        private void gvCompany_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Gets the filter criteria and re populate the customer grid view
        /// </summary>
        private void PerformCustomSearch(int PageNo)
        {
            m_PerformSearch = false;
            m_ReloadPage = false;
            this.PopulateCompanyView(gvCompany.FindFilterText, PageNo);
            m_PerformSearch = true;
            m_ReloadPage = true;
        }

        /// <summary>
        /// Apply pagination
        /// </summary>
        private void SetAccountPagination()
        {
            CTCompanyGeoDataCount objAccountPagination = ObjectCompany.GetCompanyGeoDataPageCount(gvCompany.FindFilterText);
            cboPagingAccount.Properties.Items.Clear();
            if (objAccountPagination.page_count > 1)
            {
                cboPagingAccount.Text = "1";
                for (int i = 1; i <= objAccountPagination.page_count; i++)
                    cboPagingAccount.Properties.Items.Add(i.ToString());
            }
        }

        /// <summary>
        /// Pupulates the company grid view contents
        /// </summary>
        private void PopulateCompanyView(string FilterCriteria, int PageNo)
        {
            try
            {
                gcCompany.DataSource = null;
                gcCompany.DataSource = ObjectCompany.GetGeoMatchedCompanies(FilterCriteria, PageNo);

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
        /// Traverse all rows and get geo data
        /// </summary>
        private void GetGeoData(eFetchType FetchType)
        {
            double Latitude = 0;
            double Longitude = 0;
            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcCompany.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                m_objCompany = null;
                m_objCompany = (CTGetGeographicalDataCompany)m_objGridView.GetRow(i);

                if (FetchType == eFetchType.BySelected && (bool)!m_objCompany.include)
                    continue;

                string strAddress = m_objCompany.address + ", " + m_objCompany.city + " " + m_objCompany.zip + ", " + m_objCompany.country;
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
        /// Traverse all rows and save geo data
        /// </summary>
        private void SaveGeoData()
        {
            SqlConnection objConnection = new SqlConnection(UserSession.ProviderConnection);
            objConnection.Open();

            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcCompany.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                m_objCompany = null;
                m_objCompany = (CTGetGeographicalDataCompany)m_objGridView.GetRow(i);

                if ((bool)m_objCompany.include && (m_objCompany.geo_latitude != 0 && m_objCompany.geo_longitude != 0))
                {
                    m_objGeoDataInstance = null;
                    m_objGeoDataInstance = new ObjectGeographicalData.GeoDataInstance()
                    {
                        table_source = (int)ObjectGeographicalData.eTableSource.Companies,
                        table_id = m_objCompany.id,
                        latitude = (decimal)m_objCompany.geo_latitude,
                        longitude = (decimal)m_objCompany.geo_longitude
                    };

                    ObjectGeographicalData.SaveGeoData(m_objGeoDataInstance, objConnection);
                }
            }

            objConnection.Close();
            objConnection = null;

            //this.PopulateCompanyView(null, 1);
            MessageBox.Show("Successfully updated!", m_MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Traverse all rows and set include checkbox
        /// </summary>
        private void SelectAll(bool IsAll)
        {
            m_objGridView = (DevExpress.XtraGrid.Views.Grid.GridView)gcCompany.FocusedView;
            for (int i = 0; i < m_objGridView.RowCount; i++)
            {
                if (IsAll)
                    m_objGridView.SetRowCellValue(i, "include", true);
                else
                    m_objGridView.SetRowCellValue(i, "include", false);
            }
        }

        /// <summary>
        /// Set grid right click context menu
        /// </summary>
        private void SetCompanyViewContextMenu()
        {
            ContextMenu objClickMenu = new ContextMenu();
            MenuItem miPrintPreview = new MenuItem("Print Preview");
            miPrintPreview.Click += new EventHandler(miPrintPreview_Click);
            objClickMenu.MenuItems.Add(miPrintPreview);
            gcCompany.ContextMenu = objClickMenu;
        }
        #endregion
    }
}
