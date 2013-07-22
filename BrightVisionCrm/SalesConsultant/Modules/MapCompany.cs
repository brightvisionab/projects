
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Globalization;
using DevExpress.XtraEditors;
using BrightVision.Common.Modules;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Model;

namespace SalesConsultant.Modules
{
    public partial class MapCompany : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public MapCompany()
        {
            InitializeComponent();
            m_oGeoMap = new GeoLocationViewer();
            m_oGeoMap.Dock = DockStyle.Fill;
            pnlMapContainer.Controls.Clear();
            pnlMapContainer.Controls.Add(m_oGeoMap);
        }
        #endregion

        #region Public Event Handlers
        #endregion

        #region Public Event Arguments
        #endregion

        #region Subscribed Events
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private GeoLocationViewer m_oGeoMap = null;
        private List<GeoLocationViewer.GeoMapLocation> m_lstGeoMapLocation = null;
        private int m_FinalListId = 0;
        private int m_PreviousFinalListId = 0;
        #endregion

        #region Public Methods
        public void PrepareLocations()
        {
            /**
             * get locations
             */
            if (!m_FinalListId.Equals(m_PreviousFinalListId))
            {
                m_lstGeoMapLocation = new List<GeoLocationViewer.GeoMapLocation>();
                GoogleMapUtility _oMapUtility = new GoogleMapUtility();
                BrightPlatformEntities _efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
                foreach (CTGeoLocationCompany _Location in _efDbModel.FIGetGeoLocationCompanies(m_FinalListId).ToList())
                {
                    m_lstGeoMapLocation.Add(
                        new GeoLocationViewer.GeoMapLocation()
                        {
                            Tooltip = _Location.company_name,
                            Latitude = Convert.ToDouble(_Location.latitude),
                            Longitude = Convert.ToDouble(_Location.longitude),
                            EntityId = _Location.id
                        }
                    );
                }
            }

            m_PreviousFinalListId = m_FinalListId;
            if (m_lstGeoMapLocation == null)
            {
                m_oGeoMap.ClearMarkers();
                m_oGeoMap.ResetDefaultCoordinates();
                m_oGeoMap.Show();
                return;
            }
            else if (m_lstGeoMapLocation.Count < 1)
            {
                m_oGeoMap.ClearMarkers();
                m_oGeoMap.ResetDefaultCoordinates();
                m_oGeoMap.Show();
                return;
            }

            m_oGeoMap.SetGeoMapLocation(m_lstGeoMapLocation);
            m_oGeoMap.Show();
        }
        public void Show(bool pShowSpecificCompanyMapLocation, int? pAccountId)
        {
            if (m_oGeoMap == null)
                return;

            if (!pShowSpecificCompanyMapLocation)
                m_oGeoMap.Show();
            else
            {
                if (m_lstGeoMapLocation == null)
                {
                    m_oGeoMap.ClearMarkers();
                    m_oGeoMap.ResetDefaultCoordinates();
                    m_oGeoMap.Show();
                    return;
                }

                GeoLocationViewer.GeoMapLocation _item = m_lstGeoMapLocation.Find(i => i.EntityId == pAccountId);
                if (_item != null)
                    m_oGeoMap.Show(_item);
                else
                {
                    m_oGeoMap.ResetDefaultCoordinates();
                    m_oGeoMap.Show();
                }
            }
        }
        public void SetParameters(int pFinalListId)
        {
            m_FinalListId = pFinalListId;
        }
        public void SetAsReadOnly(bool pState)
        {
            btnReload.Enabled = !pState;
            btnShowAll.Enabled = !pState;
        }
        public void Clear()
        {
            m_oGeoMap.ClearMarkers();
        }
        #endregion

        #region Private Methods
        #endregion

        #region Object Events
        private void btnReload_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading map...");
            this.PrepareLocations();
            this.Show(false, null);
            WaitDialog.Close();
        }
        private void btnShowAll_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading map...");
            this.Show(false, null);
            WaitDialog.Close();
        }
        #endregion
    }
}
