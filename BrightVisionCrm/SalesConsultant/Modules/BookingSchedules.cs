
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraScheduler;
using DevExpress.XtraScheduler.UI;

using BrightVision.Model;
using BrightVision.Common.Business;
using SalesConsultant.Business;
using SalesConsultant.Forms;

namespace SalesConsultant.Modules 
{
    public partial class BookingSchedules : DevExpress.XtraEditors.XtraUserControl 
    {
        #region Constructors
        public BookingSchedules()
        {
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        }
        #endregion

        #region Public Events & Args
        #endregion

        #region Subscribed Events
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private BrightPlatformEntities BPContext;
        private List<CTBookingSchedule> listMeetingSchedules = null;
        private int m_SubCampaignId = 0;
        private int m_PreviousSubCampaignId = 0;
        #endregion

        #region Public Methods
        public void Show(int pSubCampaignId)
        {
            m_PreviousSubCampaignId = m_SubCampaignId;
            m_SubCampaignId = pSubCampaignId;
            if (m_SubCampaignId != m_PreviousSubCampaignId)
                this.LoadData();
        }
        public void Clear()
        {
            schedulerControl1.Storage.Resources.DataSource = null;
            schedulerControl1.Storage.Appointments.DataSource = null;
        }
        #endregion

        #region Private Methods
        private void LoadData()
        {
            if (m_SubCampaignId < 1)
                return;

            //private void InitializeSchedulerControl() {
            listMeetingSchedules = BPContext.FIGetBookingSchedule(3, m_SubCampaignId).ToList();
            //if (listMeetingSchedules == null || listMeetingSchedules.Count < 1)
                //return;

            //create data binding for resources
            List<CTResources> _lstResources = new List<CTResources>();
            _lstResources = BPContext.FIGetResources(m_SubCampaignId).ToList();
            if (_lstResources == null || _lstResources.Count < 1)
                return;

            List<CTResources> resourceList = new List<CTResources>();
            foreach (CTResources _item in _lstResources) {
                if (!resourceList.Exists(i => i.resource_id == _item.resource_id))
                    resourceList.Add(_item);
            }

            //resourceList.AddRange(resourceList.ToArray());
            
            /**
             * what does this part do? it does not make sense.
             */
            // ----
            List<int?> resourceids = new List<int?>();
            resourceList.ForEach(delegate(CTResources r) {
                resourceids.Add(r.resource_id);
            });
            // ---

            ResourceStorage resStorage = schedulerStorage1.Resources;
            resStorage.Mappings.Id = "resource_id";
            resStorage.Mappings.Caption = "resource_name";
            resStorage.DataSource = resourceList;

            //create binding list for schedules
            var result = BPContext.FIGetSubCampaignSchedules(m_SubCampaignId).ToList();
            if (result == null || result.Count < 1)
                return;

            ScheduleList schedList = new ScheduleList();
            schedList.AddRange(result.ToArray());

            //create storage for resource and schedule appointments
            AppointmentStorage aptStorage = schedulerStorage1.Appointments;
            //ResourceStorage resStorage = schedulerStorage1.Resources;

            //set mapping for resources
            //resStorage.Mappings.Id = "resource_id";
            //resStorage.Mappings.Caption = "resource_name";
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
            //resStorage.DataSource = resourceList;
            aptStorage.DataSource = schedList;

            //schedulerControl1.GroupType = SchedulerGroupType.None;
            schedulerControl1.GroupType = SchedulerGroupType.Resource;
            dateNavigator1.DateTime = DateTime.Today.AddMonths(1);
            dateNavigator1.DateTime = DateTime.Today;
        }
        #endregion

        #region Object Events
        private void schedulerControl1_EditAppointmentFormShowing(object sender, AppointmentFormEventArgs e)
        {
            Appointment apt = e.Appointment;
            bool isNewApt = schedulerStorage1.Appointments.IsNewAppointment(apt);

            //disable on double click event to create new appointment for manager application consultant.
            if (isNewApt)
            {
                e.Handled = true;
                return;
            }
            var schedControl = (SchedulerControl)sender;
            schedControl.Tag = "ReadOnly";
            bool openRecurrenceForm = apt.IsRecurring && isNewApt;
            FrmAppointment frmApt = new FrmAppointment(
                schedControl,
                apt,
                openRecurrenceForm,
                FrmAppointment.ScheduleType.Unspecified,
                listMeetingSchedules);
            frmApt.LookAndFeel.ParentLookAndFeel = this.LookAndFeel.ParentLookAndFeel;
            e.DialogResult = frmApt.ShowDialog();
            e.Handled = true;

            if (apt.Type == AppointmentType.Pattern && schedulerControl1.SelectedAppointments.Contains(apt))
                schedulerControl1.SelectedAppointments.Remove(apt);

            schedulerControl1.Refresh();
        }
        private void schedulerControl1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            if (e.Menu.Id == SchedulerMenuItemId.AppointmentMenu)
            {
                var n = sender as SchedulerControl;
                if (n != null)
                {
                    var apt = n.SelectedAppointments[0];
                    if (apt != null)
                    {
                        var customField = apt.CustomFields["schedule_type"];
                        if (customField != null)
                        {
                            if (customField.ToString() == "2" || customField.ToString() == "1")
                            {
                                e.Menu.DisableMenuItem(SchedulerMenuItemId.DeleteAppointment);
                                e.Menu.DisableMenuItem(SchedulerMenuItemId.LabelSubMenu);
                                e.Menu.EnableMenuItem(SchedulerMenuItemId.OpenAppointment);
                                e.Menu.DisableMenuItem(SchedulerMenuItemId.StatusSubMenu);
                                return;
                            }
                            else if (customField.ToString() == "3")
                            {
                                schedule dbApt = (schedule)apt.GetSourceObject(schedulerStorage1);
                                if (dbApt != null)
                                {
                                    var meeting = listMeetingSchedules.FirstOrDefault(x => x.id == dbApt.id);
                                    if (meeting == null)
                                    {
                                        e.Menu.DisableMenuItem(SchedulerMenuItemId.DeleteAppointment);
                                        e.Menu.DisableMenuItem(SchedulerMenuItemId.LabelSubMenu);
                                        e.Menu.EnableMenuItem(SchedulerMenuItemId.OpenAppointment);
                                        e.Menu.DisableMenuItem(SchedulerMenuItemId.StatusSubMenu);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (e.Menu.Id == SchedulerMenuItemId.DefaultMenu)
            {
                e.Menu.RemoveMenuItem(SchedulerMenuItemId.NewRecurringAppointment);
                e.Menu.RemoveMenuItem(SchedulerMenuItemId.NewRecurringEvent);
                e.Menu.RemoveMenuItem(SchedulerMenuItemId.NewAppointment);
                e.Menu.RemoveMenuItem(SchedulerMenuItemId.NewAllDayEvent);
            }
        }
        private void schedulerStorage1_AppointmentsChanged(object sender, PersistentObjectsEventArgs e)
        {
            BPContext.SaveChanges();
        }
        private void resourcesPopupCheckedListBoxControl1_Popup(object sender, EventArgs e)
        {
            //Control c = (sender as DevExpress.Utils.Win.IPopupControl).PopupWindow;
            //PopupContainerControl pControl = c.Controls[2] as PopupContainerControl;
            //ResourcesCheckedListBoxControl rc = pControl.Controls[0] as ResourcesCheckedListBoxControl;
            //rc.CheckOnClick = true;
        }
        #endregion
    }
}
