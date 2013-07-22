
#region How To
/**
 * How to use:
 * 
 * 1. Create an instance of the google map utility located on the common:
 * 
 *      GoogleMapUtility objMapUtility = new GoogleMapUtility();   
 * 
 * 2. Create an instance of the geo location viewer and attach it on a panel or any container:
 * 
 *      GeoLocationViewer objGeoLocationViewer = new GeoLocationViewer();
 *      objGeoLocationViewer.Dock = DockStyle.Fill;
 *      pcGeoMapContact.Controls.Clear();
 *      pcGeoMapContact.Controls.Add(objGeoLocationViewer);
 *      
 * 3. Create an instance of GeoLocationViewer.GeoMapLocation list:
 * 
 *      List<GeoLocationViewer.GeoMapLocation> m_GeoMapList = new List<GeoLocationViewer.GeoMapLocation>();
 * 
 * 4. Create the list of item to locate on the map
 * 
 *      string CompleteAddress = objItemToMap.complete_address;
 *      string[] objGeoData = objMapUtility.GetGeographicalData(CompleteAddress).Split(','); // contains the web service call to google maps api
 *      
 *      double Latitude = Convert.ToDouble(objGeoData[2]);
 *      double Longitude = Convert.ToDouble(objGeoData[3]);
 *      
 *      if (Latitude != 0 || Longitude != 0)
 *      {
 *          GeoLocationViewer.GeoMapLocation Location = new GeoLocationViewer.GeoMapLocation();
 *          Location.Latitude = Latitude;
 *          Location.Longitude = Longitude;
 *          Location.Tooltip = "Place tool tip description here.";
 *          m_GeoMapList.Add(Location); // add to list
 *      }
 *      
 * 5. Lets now put to use the geo location viewer:
 * 
 *      objGeoLocationViewer.SetGeoMapLocation(m_GeoMapList);   // pass the list created      
 *      objGeoLocationViewer.ShowLocations();                   // will zoom focus on an appropriate level that all location markers can be best viewed
 *      
 * 6. To focus on a single geo map location, use this command (we wont to assign GeoLocationViewer.GeoMapLocation.Tooltip since we already did this in step 5):
 *      
 *      GeoLocationViewer.GeoMapLocation Location = new GeoLocationViewer.GeoMapLocation(); 
 *      Location.Latitude = <latitude_value>;
 *      Location.Longitude = <longitude_value>;
 *      objGeoLocationViewer.LocationFocus(Location); // will zoom focus on the specified location
 */
#endregion

#region Namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using GMap.NET;
using GMap.NET.WindowsForms;
#endregion

namespace BrightVision.Common.Modules
{
    public partial class GeoLocationViewer : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Classes
        public class GeoMapLocation
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Tooltip { get; set; }
            public int EntityId { get; set; }
        }
        #endregion

        #region Public Properties
        #endregion
        
        #region Private Members
        private GMap.NET.WindowsForms.Markers.GMapMarkerGoogleRed m_objMarker = null;
        private PointLatLng m_objPosition;
        private GMapOverlay m_objOverlay = null;
        private double m_DefaultLatitude = 60;
        private double m_DefaultLongitude = 15;
        #endregion

        #region Constructor
        public GeoLocationViewer()
        {
            InitializeComponent();
            this.InitializeGmapControl();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Focus on a specific location in zoomed state
        /// </summary>
        /// <param name="Location"></param>
        public void Show(GeoMapLocation Location)
        {
            gMapControl.Position = new PointLatLng(Location.Latitude, Location.Longitude);
            gMapControl.Zoom = 12;
        }

        /// <summary>
        /// Show locations
        /// </summary>
        public void Show()
        {
            gMapControl.Position = new PointLatLng(m_DefaultLatitude, m_DefaultLongitude);
            gMapControl.Zoom = 5;
        }

        /// <summary>
        /// Resets the default coordinates
        /// </summary>
        public void ResetDefaultCoordinates()
        {
            m_DefaultLatitude = 60;
            m_DefaultLongitude = 15;
        }

        /// <summary>
        /// Show the geo map location for a set of items
        /// </summary>
        public void SetGeoMapLocation(List<GeoMapLocation> Locations)
        {
            this.ClearMarkers();
            if (Locations != null)
            {
                int LayerItem = 0;
                foreach (GeoMapLocation Item in Locations)
                    this.SetGeoMapLocation(Item, LayerItem += 1);
            }
            else
                this.ResetDefaultCoordinates();
        }

        /// <summary>
        /// Show the geo map location for a set of items for a specified range from the point of location
        /// </summary>
        /// <param name="Locations"></param>
        /// <param name="PointOfLocation"></param>
        /// <param name="DistanceRange">Distance in kilometers</param>
        public void SetGeoMapLocation(List<GeoMapLocation> Locations, GeoMapLocation PointOfLocation, double DistanceRange)
        {
            this.ClearMarkers();
            if (Locations != null)
            {
                int LayerItem = 0;
                foreach (GeoMapLocation Item in Locations)
                {
                    if (this.GetDistance(PointOfLocation.Latitude, PointOfLocation.Longitude, Item.Latitude, Item.Longitude) <= DistanceRange)
                        this.SetGeoMapLocation(Item, LayerItem += 1);
                }
            }
            else
                this.ResetDefaultCoordinates();
        }

        /// <summary>
        /// Clear all markers on the map
        /// </summary>
        public void ClearMarkers()
        {
            this.ResetDefaultCoordinates();
            gMapControl.Overlays.Clear();
        }

        /// <summary>
        /// Reload map
        /// </summary>
        public void ReloadGmap()
        {
            gMapControl.ReloadMap();
        }

        /// <summary>
        /// Set a specific map location
        /// </summary>
        /// <param name="Location"></param>
        public void SetGeoMapLocation(GeoMapLocation Location)
        {
            this.SetGeoMapLocation(Location, 1);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initialize gmap control to this user control
        /// </summary>
        private void InitializeGmapControl()
        {
          //  gMapControl.MapType = MapType.GoogleMap; //GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gMapControl.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gMapControl.MaxZoom = 20;
            gMapControl.MinZoom = 1;
            gMapControl.Zoom = 5;
            gMapControl.Position = new PointLatLng(m_DefaultLatitude, m_DefaultLongitude); // sweden position
        }

        /// <summary>
        /// Show the geo map location of an item
        /// </summary>
        private void SetGeoMapLocation(GeoMapLocation Location, int LayerItem = 1)
        {
            if (Location.Latitude == 0 && Location.Longitude == 0)
                return;

            //GMapToolTip objToolTip;
            //objToolTip.Font = Font.;
            //objToolTip
            //ToolTip objToolTip = new ToolTip();
            //objToolTip.ToolTipIcon = ToolTipIcon.Info;
            //objToolTip

            m_objPosition = new PointLatLng(Location.Latitude, Location.Longitude);
            m_objMarker = new GMap.NET.WindowsForms.Markers.GMapMarkerGoogleRed(m_objPosition);
            m_objMarker.ToolTipText = Location.Tooltip;
            m_objMarker.ToolTipMode = MarkerTooltipMode.Always;
            //m_objMarker.ToolTip = objToolTip;
            m_objOverlay = new GMapOverlay(gMapControl, "Layer_" + LayerItem.ToString());
            m_objOverlay.Markers.Add(m_objMarker);
            gMapControl.Overlays.Add(m_objOverlay);

            if (LayerItem == 1)
            {
                gMapControl.Position = new PointLatLng(Location.Latitude, Location.Longitude);
                m_DefaultLatitude = Location.Latitude;
                m_DefaultLongitude = Location.Longitude;
            }
        }

        /// <summary>
        /// Get distance between two points in kilometers
        /// </summary>
        /// <param name="LatA"></param>
        /// <param name="LongA"></param>
        /// <param name="LatB"></param>
        /// <param name="LongB"></param>
        /// <returns></returns>
        private double GetDistance(double LatA, double LongA, double LatB, double LongB)
        {
            double Theta = LongA - LongB;
            double Distance = Math.Sin(this.GetRadius(LatA)) * Math.Sin(this.GetRadius(LatB)) + Math.Cos(this.GetRadius(LatA)) * Math.Cos(this.GetRadius(LatB)) * Math.Cos(this.GetRadius(Theta));
            Distance = Math.Acos(Distance);
            Distance = this.GetDegrees(Distance);
            Distance = Distance * 60 * 1.1515;
            Distance = Distance * 1.609344;

            return Distance;
        }

        /// <summary>
        /// Get radius from degrees
        /// </summary>
        /// <param name="Degree"></param>
        /// <returns></returns>
        private double GetRadius(double Degree)
        {
            return (Degree * Math.PI / 180.0);
        }

        /// <summary>
        /// Get degree from radius
        /// </summary>
        /// <param name="Radius"></param>
        /// <returns></returns>
        private double GetDegrees(double Radius)
        {
            return (Radius / Math.PI * 180.0);
        }
        #endregion
    }
}
