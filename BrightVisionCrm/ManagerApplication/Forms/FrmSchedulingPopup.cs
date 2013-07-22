
#region Namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Globalization;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraScheduler;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraScheduler.UI;

using BrightVision.Model;
using BrightVision.DQControl.UI;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using BrightVision.Common.Modules;
using ManagerApplication.Business;
using ManagerApplication.Modules;

#endregion

namespace ManagerApplication.Forms
{
    public partial class FrmSchedulingPopup : DevExpress.XtraEditors.XtraForm 
    {
        #region Classes
        private enum eGeoMapView
        {
            DefaultView,
            RangedView
        }
        #endregion

        #region Member Variables
        private BrightPlatformEntities BPContext;
        private bool m_DoneLoadingBookings = false;
        private GeoLocationViewer m_objGeoLocationViewer = null;
        private List<GeoLocationViewer.GeoMapLocation> m_GeoMapList = null;
        private List<GeoLocationViewer.GeoMapLocation> m_GeoMapListByResource = null;
        private GoogleMapUtility m_objGoogleMapUtility = new GoogleMapUtility();
        private eGeoMapView CurrentView { get; set; }
        private int SelectedResourceRow { get; set; }
        BaseCheckedListBoxControl.CheckedItemCollection m_objCheckedResources = null;
        List<CTBookingSchedule> listMeetingSchedules = null;
        //private object m_objLastGridObjectSender = null;
		private event EventHandler OnAssignAsSelectedBooking = null;
        private bool isactivated = false;
        private GeoLocationViewer.GeoMapLocation m_SelectedLocation = new GeoLocationViewer.GeoMapLocation();
        #endregion

        #region Constructors
        public FrmSchedulingPopup() 
        {
            m_DoneLoadingBookings = false; // flag as not done loading yet
            InitializeComponent();
            this.KeyPreview = true;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        }
        
        public int CreatedScheduleID { get; set; }
        public int SubCampaignID { get; set; }
        public int CurrentScheduleID { get; set; }
        public Schedule ControlSchedule { get; set; }
        public int CurrentAccountID { get; set; }
        public int CurrentContactID { get; set; }
        #endregion

        #region Private Methods
        /// <summary>
        /// Show distant bookings within the range selected on the form
        /// </summary>
        private void ShowDistantBookings()
        {
            if (m_objCheckedResources == null)
                return;

            if (m_objCheckedResources.Count < 1)
                return;

            //object objScheduleItem = ((GridView)m_objLastGridObjectSender).GetFocusedRow();
            //object objScheduleItem = gridViewResources.GetFocusedRow();
            //GeoLocationViewer.GeoMapLocation Location = this.GetLocationDetail(objScheduleItem);
            if (m_SelectedLocation != null)
            {
                m_objGeoLocationViewer.SetGeoMapLocation(m_GeoMapList, m_SelectedLocation, Convert.ToDouble(cboDistance.EditValue));
                m_objGeoLocationViewer.Show();
                CurrentView = eGeoMapView.RangedView;
            }
            else
                m_objGeoLocationViewer.Show();
        }

        /// <summary>
        /// Get a specific location detail for use in gmap
        /// </summary>
        /// <param name="RowNo"></param>
        /// <returns></returns>
        private GeoLocationViewer.GeoMapLocation GetLocationDetail(DataRowView objScheduleItem)
        {
            double Latitude = 0;
            double Longitude = 0;
            GeoLocationViewer.GeoMapLocation Location = null;
            //GridView objGridView = null;

            // init geo location viewer object
            Location = new GeoLocationViewer.GeoMapLocation();
            Location.Latitude = 0;
            Location.Longitude = 0;
            Location.Tooltip = "";

            if (objScheduleItem == null)
                return null;

            if (gridControlBookings.Views == null || gridControlBookings.Views.Count <= 0)
                return Location;

            //if (SelectedResourceRow > 0 && gridControlBookings.Views.Count < 2)
            //if (SelectedResourceRow > 0 && SelectedResourceRow >= gridControlBookings.Views.Count)
            //    return Location;

            //if (SelectedResourceRow < 1)
            //    SelectedResourceRow = 1;

            //if (SelectedResourceRow >= gridControlBookings.Views.Count)
            //    return Location;

            //objGridView = (GridView)gridControlBookings.Views[SelectedResourceRow];
            //if (objGridView == null)
            //    return Location;

            //if (SelectedResourceRow > 0)
            //    objGridView = (GridView)gridControlBookings.Views[SelectedResourceRow];
            //else {
            //    if(gridControlBookings.Views.Count > 1)
            //        objGridView = (GridView)gridControlBookings.Views[1];
            //}
            
            //object objRow = objGridView.GetRow(SelectedRow);

            //DataRowView objRowView = objScheduleItem;
            if (objScheduleItem.Row.ItemArray.Count() <= 2)
                return Location;

            string BookingTitle = objScheduleItem.Row[8].ToString()
                                + Environment.NewLine + objScheduleItem.Row[4].ToString()
                                + Environment.NewLine + "From: " + objScheduleItem.Row[0].ToString()
                                + Environment.NewLine + "To: " + objScheduleItem.Row[1].ToString();

            string strAddress = objScheduleItem.Row[5].ToString() + ", " + objScheduleItem.Row[6].ToString() + ", " + objScheduleItem.Row[7].ToString();
            if (string.IsNullOrEmpty(strAddress) || !GoogleMapUtility.IsValidGeoAddress(strAddress))
                return Location;

            string[] objGeoData = m_objGoogleMapUtility.GetGeographicalData(strAddress).Split(',');

            if (objGeoData[2] != null)
                if (ValidationUtility.IsCurrency(objGeoData[2]))
                    Latitude = Convert.ToDouble(objGeoData[2], CultureInfo.InvariantCulture);

            if (objGeoData[3] != null)
                if (ValidationUtility.IsCurrency(objGeoData[3]))
                    Longitude = Convert.ToDouble(objGeoData[3], CultureInfo.InvariantCulture);

            if (Latitude != 0 || Longitude != 0)
            {
                Location = new GeoLocationViewer.GeoMapLocation();
                Location.Latitude = Latitude;
                Location.Longitude = Longitude;
                Location.Tooltip = BookingTitle;
            }

            return Location;
        }

        /// <summary>
        /// Show a particular geo map of a booking
        /// </summary>
        private void ShowSelectedBooking(DataRowView objScheduleItem)
        {
            if (CurrentView == eGeoMapView.RangedView)
            {
                if (m_GeoMapList != null)
                    m_objGeoLocationViewer.SetGeoMapLocation(m_GeoMapList);

                CurrentView = eGeoMapView.DefaultView;
            }

            GeoLocationViewer.GeoMapLocation Location = this.GetLocationDetail(objScheduleItem);
            m_SelectedLocation = Location;

            if (Location != null)
            {
                m_objGeoLocationViewer.ClearMarkers();
                m_objGeoLocationViewer.SetGeoMapLocation(Location);
                m_objGeoLocationViewer.Show(Location);
            }
            else
                m_objGeoLocationViewer.Show();
        }

        /// <summary>
        /// Load geo map locations
        /// </summary>
        private void GetGeoMapLocationsByResource()
        {
            if ((gridControlBookings == null) || (gridControlBookings.Views != null && gridControlBookings.Views.Count <= 0)) 
                return;

            if (gridControlBookings.Views.Count == 1)
                return;

            DataRow[] drResourceSchedules = gridViewResources.GetDataRow(SelectedResourceRow).GetChildRows("ResourceSchedules");
            
            //int viewCount = gridControlBookings.Views.Count;
            //if (SelectedResourceRow > viewCount) 
            //    return;

            //var view = gridControlBookings.Views[SelectedResourceRow];
            //if (view == null) 
            //    return;

            double Latitude = 0;
            double Longitude = 0;
            m_GeoMapListByResource = null;
            m_GeoMapListByResource = new List<GeoLocationViewer.GeoMapLocation>();
            //DataRowView objRowView = null;
            GeoLocationViewer.GeoMapLocation Location = null;
            
            //GridView objGridView = (GridView) view;
            //for (int i = 0; i < objGridView.RowCount; i++)
            foreach (DataRow item in drResourceSchedules)
            {             
                //objRowView = null;
                //objRowView = drResourceSchedules
                //objRowView = (DataRowView) objGridView.GetRow(i);
                if (item.ItemArray.Count() == 2)
                    continue;

                string BookingTitle = item[8].ToString()
                                    + Environment.NewLine + item[4].ToString()
                                    + Environment.NewLine + "From: " + item[0].ToString()
                                    + Environment.NewLine + "To: " + item[1].ToString();

                string strAddress = item[5].ToString() + ", " + item[6].ToString() + ", " + item[7].ToString();
                if (string.IsNullOrEmpty(strAddress) || !GoogleMapUtility.IsValidGeoAddress(strAddress))
                    continue;

                string[] objGeoData = m_objGoogleMapUtility.GetGeographicalData(strAddress).Split(',');

                if (objGeoData[2] != null)
                    if (ValidationUtility.IsCurrency(objGeoData[2]))
                        Latitude = Convert.ToDouble(objGeoData[2], CultureInfo.InvariantCulture);

                if (objGeoData[3] != null)
                    if (ValidationUtility.IsCurrency(objGeoData[3]))
                        Longitude = Convert.ToDouble(objGeoData[3], CultureInfo.InvariantCulture);

                if (Latitude != 0 || Longitude != 0)
                {
                    Location = null;
                    Location = new GeoLocationViewer.GeoMapLocation();
                    Location.Latitude = Latitude;
                    Location.Longitude = Longitude;
                    Location.Tooltip = BookingTitle;
                    m_GeoMapListByResource.Add(Location);
                }
            }
        }

        /// <summary>
        /// Load geo map locations
        /// </summary>
        private void GetGeoMapLocations()
        {
            double Latitude = 0;
            double Longitude = 0;
            int ResourceId = 0;
            m_GeoMapList = null;
            m_GeoMapList = new List<GeoLocationViewer.GeoMapLocation>();
            ResourceCheckedListBoxItem objCheckedItem = null;
            GridView objGridView = null;
            DataRowView objRowView = null; 
            GeoLocationViewer.GeoMapLocation Location = null;
            m_objGeoLocationViewer.ClearMarkers();

            for (int x = 0; x < gridControlBookings.Views.Count; x++)
            {
                objGridView = null;
                objGridView = (GridView) gridControlBookings.Views[x];
                if (objGridView == null || ((DataRowView)objGridView.GetRow(0)) == null)
                    continue;

                if (((DataRowView)objGridView.GetRow(0)).Row.ItemArray.Count() <= 2)
                    continue;

                ResourceId = (int)((DataRowView)objGridView.GetRow(0)).Row.ItemArray[10];
                for (int i = 0; i < m_objCheckedResources.Count; i++)
                {
                    objCheckedItem = null;
                    objCheckedItem = (ResourceCheckedListBoxItem)m_objCheckedResources[i];
                    if (Convert.ToInt32(objCheckedItem.Resource.Id) != Convert.ToInt32(ResourceId))
                        continue;

                    for (int j = 0; j < objGridView.RowCount; j++)
                    {
                        objRowView = null;
                        objRowView = (DataRowView)objGridView.GetRow(j);
                        if (objRowView == null || objRowView.Row.ItemArray.Count() <= 2)
                            break;

                        string BookingTitle = objRowView.Row[8].ToString()
                                            + Environment.NewLine + objRowView.Row[4].ToString()
                                            + Environment.NewLine + "From: " + objRowView.Row[0].ToString()
                                            + Environment.NewLine + "To: " + objRowView.Row[1].ToString();

                        string strAddress = objRowView.Row[5].ToString() + ", " + objRowView.Row[6].ToString() + ", " + objRowView.Row[7].ToString();
                        if (string.IsNullOrEmpty(strAddress) || !GoogleMapUtility.IsValidGeoAddress(strAddress))
                            continue;

                        string[] objGeoData = m_objGoogleMapUtility.GetGeographicalData(strAddress).Split(',');

                        if (objGeoData[2] != null)
                            if (ValidationUtility.IsCurrency(objGeoData[2]))
                                Latitude = Convert.ToDouble(objGeoData[2], CultureInfo.InvariantCulture);

                        if (objGeoData[3] != null)
                            if (ValidationUtility.IsCurrency(objGeoData[3]))
                                Longitude = Convert.ToDouble(objGeoData[3], CultureInfo.InvariantCulture);

                        if (Latitude != 0 || Longitude != 0)
                        {
                            Location = null;
                            Location = new GeoLocationViewer.GeoMapLocation();
                            Location.Latitude = Latitude;
                            Location.Longitude = Longitude;
                            Location.Tooltip = BookingTitle;
                            m_GeoMapList.Add(Location);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize the geo map viewer
        /// </summary>
        private void InitializeGeoMapViewer()
        {
            m_objGeoLocationViewer = new GeoLocationViewer();
            m_objGeoLocationViewer.Dock = DockStyle.Fill;
            pcGeoMap.Controls.Clear();
            pcGeoMap.Controls.Add(m_objGeoLocationViewer);
            this.GetGeoMapLocations();

            if (m_GeoMapList != null)
                m_objGeoLocationViewer.SetGeoMapLocation(m_GeoMapList);

            if (m_GeoMapList.Count > 0)
                m_SelectedLocation = m_GeoMapList[0];

            m_objGeoLocationViewer.Show();
            CurrentView = eGeoMapView.DefaultView;
        }

        private void InitializeSchedulerControl() {
            OnAssignAsSelectedBooking += new EventHandler(FrmSchedulingPopup_OnAssignAsSelectedBooking);
            listMeetingSchedules = BPContext.FIGetBookingSchedule(3, SubCampaignID).ToList();

            //enable restriction sales consultant app only
            schedulerControl1.OptionsCustomization.AllowInplaceEditor = UsedAppointmentType.None;
            //schedulerControl1.OptionsCustomization.AllowAppointmentDelete = UsedAppointmentType.None;
            schedulerControl1.OptionsCustomization.AllowAppointmentCopy = UsedAppointmentType.None;
            schedulerControl1.OptionsCustomization.AllowAppointmentDrag = UsedAppointmentType.None;
            schedulerControl1.OptionsCustomization.AllowAppointmentDragBetweenResources = UsedAppointmentType.None;
            //schedulerControl1.OptionsCustomization.AllowAppointmentEdit = UsedAppointmentType.None;
            schedulerControl1.OptionsCustomization.AllowAppointmentMultiSelect = false;
            //schedulerControl1.OptionsCustomization.AllowAppointmentResize = UsedAppointmentType.None;

            //create data binding for resources
            List<CTResources> resourceList = new List<CTResources>();
            resourceList.AddRange(BPContext.FIGetResources(SubCampaignID).ToList().ToArray());
          
            List<int?> resourceids = new List<int?>();
            resourceList.ForEach(delegate(CTResources r) {
                resourceids.Add(r.resource_id);
            });

            //create binding list for schedules
            ScheduleList schedList = new ScheduleList();
            var result = BPContext.FIGetSubCampaignSchedules(SubCampaignID);
            schedList.AddRange(result.ToList().ToArray());

            //create storage for resource and schedule appointments
            AppointmentStorage aptStorage = schedulerStorage1.Appointments;
            ResourceStorage resStorage = schedulerStorage1.Resources;
            
            //set mapping for resources
            resStorage.Mappings.Id = "resource_id";
            resStorage.Mappings.Caption = "resource_name";
            //resStorage.Mappings.Image = "picture";

            //set mapping for schedules              
            aptStorage.Mappings.ResourceId = "resource_id";
            aptStorage.Mappings.Status = "status";
            aptStorage.Mappings.Subject = "subject";
            aptStorage.Mappings.Location = "address";            
            aptStorage.Mappings.Description = "description";
            aptStorage.Mappings.Label = "label";
            aptStorage.Mappings.Start = "start_time";
            aptStorage.Mappings.End = "end_time";
            aptStorage.Mappings.AllDay = "all_day"; 
            aptStorage.Mappings.Type = "event_type";                        
            aptStorage.Mappings.RecurrenceInfo = "recurrence_info";
            aptStorage.Mappings.ReminderInfo = "reminder_info";            
            
            //set custom fields mapping
            aptStorage.CustomFieldMappings.Add(new AppointmentCustomFieldMapping("zipcode", "zipcode"));
            aptStorage.CustomFieldMappings.Add(new AppointmentCustomFieldMapping("city", "city"));
            aptStorage.CustomFieldMappings.Add(new AppointmentCustomFieldMapping("schedule_type", "schedule_type"));

            //set the data bindings as datasources            
            resStorage.DataSource = resourceList;
            aptStorage.DataSource = schedList;

            repositoryItemComboBoxGroupBy.SelectedIndexChanged += 
                new EventHandler(repositoryItemComboBoxGroupBy_SelectedIndexChanged);
            repositoryItemImageComboBoxLabels.SelectedIndexChanged += 
                new EventHandler(repositoryItemImageComboBoxLabels_SelectedIndexChanged);
            repositoryItemTrackBarHeaderHeight.EditValueChanged += 
                new EventHandler(repositoryItemTrackBarHeaderHeight_EditValueChanged);
            repositoryItemCheckEditRotateHeader.CheckedChanged += 
                new EventHandler(repositoryItemCheckEditRotateHeader_CheckedChanged);

            schedulerControl1.GroupType = SchedulerGroupType.Resource;
            dateNavigator1.DateTime = DateTime.Today.AddMonths(1);
            dateNavigator1.DateTime = DateTime.Today;
            barEditItemGroupBy.EditValue = schedulerControl1.GroupType;


            FillFilterLabels();
            barEditItemLabels.EditValue = -1;    
            
        }
                        
        private void FillFilterLabels() {
            AppointmentLabelCollection labCol = schedulerStorage1.Appointments.Labels;
            ImageCollection labelImages = new ImageCollection();            
            //labelImages.AddImage( new AppointmentLabelBase(Color.DarkGray,"AllLabels").CreateBitmap(16,16));
            labCol.ForEach(delegate(AppointmentLabelBase labelBase) {
                //labelImages.AddImage(labelBase.CreateBitmap(16, 16));
            });

            this.repositoryItemImageComboBoxLabels.SmallImages = labelImages;
            this.repositoryItemImageComboBoxLabels.LargeImages = labelImages;
            this.repositoryItemImageComboBoxLabels.Items.Clear();
            this.repositoryItemImageComboBoxLabels.Items.Add(new ImageComboBoxItem("All Labels", -1, 0));            
            for (int i = 0; i < labCol.Count; i++) {
                this.repositoryItemImageComboBoxLabels.Items.Add(new ImageComboBoxItem(labCol[i].DisplayName, i, i+1));              
            }
            barEditItemLabels.EditValue = 0;
        }

        private void AdjustResourceHeaders() {
            int height = 0;
            SchedulerGroupType groupType = schedulerControl1.GroupType;
            if ((schedulerControl1.ActiveView is WeekView && groupType == SchedulerGroupType.Date)
                || (schedulerControl1.ActiveView is TimelineView && groupType != SchedulerGroupType.None)) {
                    height = Convert.ToInt32(barEditItemTrackHeight.EditValue);
            }
            schedulerControl1.OptionsView.ResourceHeaders.Height = height;            
        }

        private void PopulateListofBookings() 
        {
            if (SubCampaignID <= 0) return;
            //Create Data objects for representing database's tables 
            DataSet dataSetResourceSchedule = new DataSet();      
            SqlParameter sqlParam1 = new SqlParameter("subcampaign_id", SubCampaignID);
            DataTable dataTable = DatabaseUtility.ExecuteStoredProcedure("[bvGetSubCampaignGridSchedules_sp]", "Schedules", sqlParam1);            
            dataSetResourceSchedule.Tables.Add(dataTable);
            SqlParameter sqlParam2 = new SqlParameter("subcampaign_id", SubCampaignID);
            dataTable = DatabaseUtility.ExecuteStoredProcedure("bvGetResources_sp", "Resources", sqlParam2);
            dataSetResourceSchedule.Tables.Add(dataTable);

            //Set up a master-detail relationship between the DataTables 
            DataColumn keyColumn = dataSetResourceSchedule.Tables["Resources"].Columns["resource_id"];
            DataColumn foreignKeyColumn = dataSetResourceSchedule.Tables["Schedules"].Columns["resource_id"];
            dataSetResourceSchedule.Relations.Add("ResourceSchedules", keyColumn, foreignKeyColumn);

            //Bind the grid control to the data source 
            gridControlBookings.DataSource = dataSetResourceSchedule.Tables["Resources"];
            gridControlBookings.ForceInitialize();
            
            //Hide some of the resource columns
            gridViewResources.Columns["resource_id"].VisibleIndex = -1;
            //Assign GridView Child to the relationship
            //Create columns for the pattern view 
            gridViewSchedules.PopulateColumns(dataSetResourceSchedule.Tables["Schedules"]);            
            //Hide some of the schedule column for the pattern view 
            gridViewSchedules.Columns["resource_name"].VisibleIndex = -1;
            gridViewSchedules.Columns["schedule_id"].VisibleIndex = -1;
            gridViewSchedules.Columns["resource_id"].VisibleIndex = -1;
            gridViewSchedules.Columns["account_id"].VisibleIndex = -1;
            gridViewSchedules.Columns["contact_id"].VisibleIndex = -1;
            
            //expand all details
            gridViewResources.BeginUpdate();
            try {
                int dataRowCount = gridViewResources.DataRowCount;
                for (int rHandle = 0; rHandle < dataRowCount; ++rHandle) {
                    gridViewResources.SetMasterRowExpanded(rHandle, true);
                }
            } finally {
                gridViewResources.EndUpdate();
            }
        }

        private void FilterGridBooking() {
            var selectedItems = resourcesCheckedListBoxControl1.CheckedItems;
            var itemcount = resourcesCheckedListBoxControl1.ItemCount;
            
            if (selectedItems.Count > 0) {
                DevExpress.XtraScheduler.UI.ResourceCheckedListBoxItem clbitem;
                List<string> listResids = new List<string>();
                string filter = string.Empty;
                for (int x = 0; x < selectedItems.Count; ++x) {
                    clbitem = selectedItems[x] as DevExpress.XtraScheduler.UI.ResourceCheckedListBoxItem;
                    if (clbitem != null) {
                        filter = "[resource_id] = " + clbitem.Resource.Id.ToString();
                        listResids.Add(filter);
                    }
                }
                if (listResids.Count > 0) {
                    gridViewResources.Columns["resource_id"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo(string.Join(" OR ", listResids.ToArray()));
                }
            } 
            else
                gridViewResources.Columns["resource_id"].FilterInfo = new DevExpress.XtraGrid.Columns.ColumnFilterInfo("[resource_id] = -1");

            gridViewResources.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
        }

        private void InitViewsListChecker()
        {
            m_objCheckedResources = null;
            m_objCheckedResources = resourcesCheckedListBoxControl1.CheckedItems;
        }

        private void EditAppointment() {
            if (isactivated) return;
                        
            if (CurrentScheduleID > 0) {
                isactivated = true;
                schedule aptSched = null;
                Appointment apt = null;                
                AppointmentCollection aptCol = schedulerStorage1.Appointments.Items;
                for(int x=0; x < aptCol.Count; ++x) {
                    aptSched = (schedule) aptCol[x].GetSourceObject(schedulerStorage1);
                    if(aptSched.id == CurrentScheduleID) {
                        apt = aptCol[x];
                        break;
                    }
                }
                if (apt != null) {
                    schedulerControl1.Refresh();
                    schedulerControl1.ActiveView.SelectAppointment(apt);
                    FrmAppointment frmApt = new FrmAppointment(schedulerControl1, apt, false, FrmAppointment.ScheduleType.Unspecified, listMeetingSchedules);
                    frmApt.LookAndFeel.ParentLookAndFeel = this.LookAndFeel.ParentLookAndFeel;
                    frmApt.ShowDialog();
                }
            }
        }

        public void SetBreadCrumb(string text) {
            simpleLabelItemBreadCrumb.Text = text;
        }
        #endregion

        #region Controllers
        private void FrmSchedulingPopup_Load(object sender, EventArgs e) {
            
            WaitDialog.Show(this, "Loading components...");
            this.PopulateListofBookings();
            m_DoneLoadingBookings = true;     
            this.InitializeSchedulerControl();
            this.InitViewsListChecker();
            this.InitializeGeoMapViewer();
            WaitDialog.Close();
            //WaitDialog.CloseWaitDialog();
        }

        private void repositoryItemCheckEditRotateHeader_CheckedChanged(object sender, EventArgs e) {
            CheckEdit checkEdit = sender as CheckEdit;
            if (checkEdit == null) return;
            bool rotate = (bool) checkEdit.EditValue;
            schedulerControl1.OptionsView.ResourceHeaders.RotateCaption = rotate;
        }

        private void repositoryItemTrackBarHeaderHeight_EditValueChanged(object sender, EventArgs e) {
            TrackBarControl trackBar = sender as TrackBarControl;
            if (trackBar == null) return;
            schedulerControl1.OptionsView.ResourceHeaders.Height = (int) trackBar.EditValue;
        }
        
        private void repositoryItemComboBoxGroupBy_SelectedIndexChanged(object sender, EventArgs e) {
            schedulerControl1.BeginUpdate();
            try {
                ComboBoxEdit comboBoxEdit = sender as ComboBoxEdit;
                if (comboBoxEdit == null) return;
                schedulerControl1.GroupType = (SchedulerGroupType) Enum.Parse(typeof(SchedulerGroupType), comboBoxEdit.EditValue.ToString());
                AdjustResourceHeaders();
            } finally {
                schedulerControl1.EndUpdate();
            }
        }
        
        private void schedulerStorage1_AppointmentsInserted(object sender, PersistentObjectsEventArgs e) {
            foreach (Appointment apt in e.Objects) {
                schedule dbApt = (schedule)apt.GetSourceObject(schedulerStorage1);
                if (dbApt.start_time == null || dbApt.end_time == null) {
                    MessageBox.Show("Time start/end required.", "Bright Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                BPContext.schedules.AddObject(dbApt);
                BPContext.SaveChanges();
                PopulateListofBookings();
                CreatedScheduleID = dbApt.id;
                //this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void schedulerStorage1_AppointmentDeleting(object sender, PersistentObjectCancelEventArgs e) {
            Appointment apt = (Appointment)e.Object;
            schedule dbApt = (schedule)apt.GetSourceObject(schedulerStorage1);
            BPContext.schedules.DeleteObject(dbApt);
            try {
                BPContext.SaveChanges();
                //Note: Doing deletion of schedule has two ways, inside the schedulepopup or within the meeting schedule component button "delete"
                //check if current meeting schedule is being edited
                if (ControlSchedule != null) {
                    //if deleted schedule equals to the current selected schedule in the meeting component                    
                    if (dbApt.id == ControlSchedule.CurrentSelectedMeeting) {
                        //then refresh the available booking list and set edit value to null by calling
                        ControlSchedule.RemoveCurrentMeetingSchedule();
                    }
                }
            } catch(Exception ex) {
                
            }
            PopulateListofBookings();
        }

        private void schedulerStorage1_AppointmentsChanged(object sender, PersistentObjectsEventArgs e) {
            BPContext.SaveChanges();
            PopulateListofBookings();
        }

        private void repositoryItemImageComboBoxLabels_SelectedIndexChanged(object sender, EventArgs e) {
            ComboBoxEdit comboBoxEdit = sender as ComboBoxEdit;
            if (comboBoxEdit == null) return;
            barEditItemLabels.EditValue = comboBoxEdit.EditValue;
            schedulerStorage1.RefreshData();
        }

        private void schedulerStorage1_FilterAppointment(object sender, PersistentObjectCancelEventArgs e) {            
            int labelId = Convert.ToInt32(barEditItemLabels.EditValue);
            if (labelId < 0)
                return;
            Appointment apt = (Appointment)e.Object;
            e.Cancel = apt.LabelId != labelId;
        }

        private void schedulerControl1_EditAppointmentFormShowing(object sender, AppointmentFormEventArgs e) {
            Appointment apt = e.Appointment;
            bool isNewApt =schedulerStorage1.Appointments.IsNewAppointment(apt);

            //disable on double click event to create new appointment for manager application consultant.
            if (isNewApt) {
                e.Handled = true;
                return;
            }

            bool openRecurrenceForm = apt.IsRecurring && isNewApt;
            FrmAppointment frmApt = new FrmAppointment((SchedulerControl)sender, apt, openRecurrenceForm, FrmAppointment.ScheduleType.Unspecified, listMeetingSchedules);
            frmApt.LookAndFeel.ParentLookAndFeel = this.LookAndFeel.ParentLookAndFeel;
            e.DialogResult = frmApt.ShowDialog();
            e.Handled = true;

            if (apt.Type == AppointmentType.Pattern && schedulerControl1.SelectedAppointments.Contains(apt))
                schedulerControl1.SelectedAppointments.Remove(apt);

            schedulerControl1.Refresh();
        }

        private void resourcesCheckedListBoxControl1_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
        {
            m_objCheckedResources = null;
            m_objCheckedResources = resourcesCheckedListBoxControl1.CheckedItems;
            this.FilterGridBooking();
        }

        private void gridViewResources_ColumnFilterChanged(object sender, EventArgs e)
        {
            if (m_DoneLoadingBookings)
            {
                // if there's no current checked resource in the list
                if (m_objCheckedResources.Count < 1)
                {
                    m_objGeoLocationViewer.ClearMarkers();
                    m_objGeoLocationViewer.ReloadGmap();
                    m_objGeoLocationViewer.Show();
                    return;
                }

                this.Cursor = Cursors.WaitCursor;
                this.GetGeoMapLocations();
                if (m_GeoMapList != null && m_GeoMapList.Count > 0)
                    m_objGeoLocationViewer.SetGeoMapLocation(m_GeoMapList);

                m_objGeoLocationViewer.Show();
                CurrentView = eGeoMapView.DefaultView;
                this.Cursor = Cursors.Default;
            }
        }

        private void gridViewSchedules_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (m_DoneLoadingBookings)
            {
                DataRowView item = gridControlBookings.FocusedView.GetRow(e.FocusedRowHandle) as DataRowView;
                //DataRow[] items = gridViewResources.GetDataRow(SelectedResourceRow).GetChildRows("ResourceSchedules");
                //if (e.FocusedRowHandle > (items.Count() - 1))
                //    return;
                //[e.FocusedRowHandle];
                this.ShowSelectedBooking(item);
            }
        }
        
        private void gridViewSchedules_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Equals("start_time") || e.Column.FieldName.Equals("end_time"))
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd HH:mm");
        }

        private void btnShowDistantBookings_Click(object sender, EventArgs e)
        {
            this.ShowDistantBookings();
        }

        private void gridViewResources_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (!m_DoneLoadingBookings || e.FocusedRowHandle < 0)
                return;

            //if (e.FocusedRowHandle == 0)
            //    SelectedResourceRow = e.FocusedRowHandle + 1; // we add 1 since 0 is the main grid view, so we can get the selected resource item row
            //else

            SelectedResourceRow = e.FocusedRowHandle;

            this.GetGeoMapLocationsByResource();
            if (m_GeoMapListByResource == null || m_GeoMapListByResource.Count <= 0)
                return;

            m_SelectedLocation = m_GeoMapListByResource[0];
            if (m_GeoMapListByResource.Count > 0)
            {
                m_objGeoLocationViewer.SetGeoMapLocation(m_GeoMapListByResource);
                m_objGeoLocationViewer.Show();
            }
        }

        private void cmdViewAll_Click(object sender, EventArgs e)
        {
            this.GetGeoMapLocations();
            if (m_GeoMapList == null || m_GeoMapList.Count <= 0)
                return;

            m_objGeoLocationViewer.SetGeoMapLocation(m_GeoMapList);
            m_objGeoLocationViewer.Show();
        }

        private void barBtnCreateMeeting_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            Appointment apt = schedulerStorage1.CreateAppointment(AppointmentType.Normal);

            FrmAppointment frmApt = new FrmAppointment(schedulerControl1, apt, false, FrmAppointment.ScheduleType.Meeting, null);
            frmApt.Text = "Create Meeting";
            frmApt.BookingType = FrmAppointment.ScheduleType.Meeting;
            frmApt.LookAndFeel.ParentLookAndFeel = this.LookAndFeel.ParentLookAndFeel;
            //frmApt.ContactID = CurrentContactID;
            //frmApt.AccountID = CurrentAccountID;
            frmApt.ShowDialog();
        }

        private void barBtnCreateSeminar_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            Appointment apt = schedulerStorage1.CreateAppointment(AppointmentType.Normal);

            FrmAppointment frmApt = new FrmAppointment(schedulerControl1, apt, false, FrmAppointment.ScheduleType.Seminar, null);
            frmApt.Text = "Create Seminar";
            frmApt.BookingType = FrmAppointment.ScheduleType.Seminar;
            frmApt.LookAndFeel.ParentLookAndFeel = this.LookAndFeel.ParentLookAndFeel;
            frmApt.ShowDialog();
        }

        private void barBtnCreateWebinar_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {            
            Appointment apt = schedulerStorage1.CreateAppointment(AppointmentType.Normal);            
            FrmAppointment frmApt = new FrmAppointment(schedulerControl1, apt, false, FrmAppointment.ScheduleType.Webinar, null);
            frmApt.Text = "Create Webinar";
            frmApt.BookingType = FrmAppointment.ScheduleType.Webinar;
            frmApt.LookAndFeel.ParentLookAndFeel = this.LookAndFeel.ParentLookAndFeel;
            frmApt.ShowDialog();
        }
        
        private void schedulerControl1_PopupMenuShowing(object sender, DevExpress.XtraScheduler.PopupMenuShowingEventArgs e) {
            if (e.Menu.Id == SchedulerMenuItemId.AppointmentMenu) {
                var n = sender as SchedulerControl;
                if (n != null) {
                    var apt = n.SelectedAppointments[0];
                    if (apt != null) {
                        var customField = apt.CustomFields["schedule_type"];
                        if (customField != null) {
                            if (customField.ToString() == "2" || customField.ToString() == "1") {
                                //DAN: New implementation base on johan's comment on ticket: https://brightvision.jira.com/browse/PLATFORM-2482
                                //Just commented the code below to make it editable...

                                /*e.Menu.DisableMenuItem(SchedulerMenuItemId.DeleteAppointment);
                                e.Menu.DisableMenuItem(SchedulerMenuItemId.LabelSubMenu);
                                e.Menu.EnableMenuItem(SchedulerMenuItemId.OpenAppointment);
                                e.Menu.DisableMenuItem(SchedulerMenuItemId.StatusSubMenu);*/
                                return;
                            } else if (customField.ToString() == "3") {
                                schedule dbApt = (schedule)apt.GetSourceObject(schedulerStorage1);
                                if (dbApt != null) {
                                    var meeting = listMeetingSchedules.FirstOrDefault(x => x.id == dbApt.id);
                                    if (meeting == null) {
                                        e.Menu.DisableMenuItem(SchedulerMenuItemId.DeleteAppointment);
                                        e.Menu.DisableMenuItem(SchedulerMenuItemId.LabelSubMenu);
                                        e.Menu.EnableMenuItem(SchedulerMenuItemId.OpenAppointment);
                                        e.Menu.DisableMenuItem(SchedulerMenuItemId.StatusSubMenu);
                                        return;
                                    } else {                                        
                                        e.Menu.Items.Add(
                                            new DevExpress.Utils.Menu.DXMenuItem("Assign As Selected Booking", OnAssignAsSelectedBooking));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (e.Menu.Id == SchedulerMenuItemId.DefaultMenu) {
                e.Menu.RemoveMenuItem(SchedulerMenuItemId.NewRecurringAppointment);
                e.Menu.RemoveMenuItem(SchedulerMenuItemId.NewRecurringEvent);
                e.Menu.RemoveMenuItem(SchedulerMenuItemId.NewAppointment);
                e.Menu.RemoveMenuItem(SchedulerMenuItemId.NewAllDayEvent);
            }
        }

        private void FrmSchedulingPopup_OnAssignAsSelectedBooking(object sender, EventArgs e) {
            var apt = schedulerControl1.SelectedAppointments[0];
            if (apt != null) {
                schedule dbApt = (schedule)apt.GetSourceObject(schedulerStorage1);                
                CreatedScheduleID = dbApt.id;
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }
        private void FrmSchedulingPopup_Activated(object sender, EventArgs e) {
            EditAppointment();
        }
        #endregion

        #region Keyboard Shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (FormUtility.ShortCutKeysHandled(keyData))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Commented Codes
        //private void gridViewBookings_CellMerge(object sender, DevExpress.XtraGrid.Views.Grid.CellMergeEventArgs e) {
        //    if(e.Column == gridViewBookings.Columns["resource_name"]) return;
        //    string value1 = gridViewBookings.GetRowCellDisplayText(e.RowHandle1, e.Column);
        //    string value2 = gridViewBookings.GetRowCellDisplayText(e.RowHandle2, e.Column);
        //    if(value1 == value2){
        //        e.Merge = true;
        //        e.Handled = true;
        //    }
        //}
        #endregion
    }
}