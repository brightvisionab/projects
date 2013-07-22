using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraScheduler;
using DevExpress.XtraScheduler.UI;
using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;

namespace SalesConsultant.Forms {
    public partial class FrmAppointment : XtraForm {
        public enum ScheduleType{
            Unspecified = 0,
            Seminar = 1,
            Webinar = 2,
            Meeting = 3            
        }
        private SchedulerControl control;
        private Appointment appointmentForm;
        private bool openRecurrenceForm = false;
        private int suspendUpdateCount;                
        private BVAppointmentFormController controller;

        protected AppointmentStorage Appointments { get { return control.Storage.Appointments; } }
        protected internal bool IsNewAppointment { get { return controller != null ? controller.IsNewAppointment : true; } }
        protected bool IsUpdateSuspended { get { return suspendUpdateCount > 0; } }
        
        public FrmAppointment(SchedulerControl control, Appointment apt, bool openRecurrenceForm, ScheduleType schedType, List<CTBookingSchedule> listMeetingSchedules) {            
            this.openRecurrenceForm = openRecurrenceForm;
            this.controller = new BVAppointmentFormController(control, apt);
            this.appointmentForm = apt;
            this.control = control;
            this.BookingType = schedType;
            this.ListMeetingSchedules = listMeetingSchedules;
            SuspendUpdate();
            InitializeComponent();
            this.KeyPreview = true;
            btnRecurrence.Enabled = false;
            if (this.controller.IsNewAppointment) {
                btnDelete.Enabled = false;
            } else {
                bool isReadonly = false;
                if (this.control.Tag != null && this.control.Tag.ToString() == "ReadOnly")
                    isReadonly = true;
                //if in editing mode, do this for sales consultant only
                //don't allow delete or edit if webinar and seminar. allow for meeting only

                if (controller.ScheduleType == 2 || controller.ScheduleType == 1) {
                    DisableControls();
                } else if (controller.ScheduleType == 3) {
                    if (ListMeetingSchedules != null) {
                        schedule dbApt = (schedule)apt.GetSourceObject(control.Storage);
                        if (dbApt != null) {
                            var meeting = listMeetingSchedules.FirstOrDefault(x => x.id == dbApt.id);
                            if (meeting == null || isReadonly) {
                                DisableControls();
                            }
                        }
                    }
                }
            }
            ResumeUpdate();            
            UpdateForm();
        }
        
        private void FrmAppointment_Load(object sender, EventArgs e) {
            WaitDialog.Show(this,"Loading components...");
            if (AccountID > 0 && ContactID > 0) {
                System.Text.StringBuilder sbAddress = new System.Text.StringBuilder();
                string address = "";
                BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
                var curContact = BPContext.contacts.SingleOrDefault(x => x.id == ContactID);
                if (curContact != null) {
                    if (!string.IsNullOrEmpty(curContact.address_1)) sbAddress.Append(curContact.address_1);
                    if (!string.IsNullOrEmpty(curContact.address_2)) sbAddress.Append(curContact.address_2);
                    if (!string.IsNullOrEmpty(curContact.zipcode)) sbAddress.Append(curContact.zipcode);
                    if (!string.IsNullOrEmpty(curContact.city)) sbAddress.Append(curContact.city);
                    if (!string.IsNullOrEmpty(sbAddress.ToString())) {
                        if (!string.IsNullOrEmpty(curContact.address_1)) address = curContact.address_1;
                        if (!string.IsNullOrEmpty(curContact.address_2)) address = (address.Length > 0 ? " " : "") + curContact.address_2;
                        textEditAddress.Text = address;
                        textEditZipcode.Text = curContact.zipcode;
                        textEditCity.Text = curContact.city;
                    } else {
                        address = "";
                        var curAccount = BPContext.accounts.SingleOrDefault(x => x.id == AccountID);
                        if (curAccount != null) {
                            if (!string.IsNullOrEmpty(curAccount.box_address)) sbAddress.Append(curAccount.box_address);
                            if (!string.IsNullOrEmpty(curAccount.street_address)) sbAddress.Append(curAccount.street_address);
                            if (!string.IsNullOrEmpty(curAccount.zipcode)) sbAddress.Append(curAccount.zipcode);
                            if (!string.IsNullOrEmpty(curAccount.city)) sbAddress.Append(curAccount.city);
                            if (!string.IsNullOrEmpty(sbAddress.ToString())) {
                                if (!string.IsNullOrEmpty(curAccount.box_address)) address = curAccount.box_address;
                                if (!string.IsNullOrEmpty(curAccount.street_address)) address = (address.Length > 0 ? " " : "") + curAccount.street_address;
                                textEditAddress.Text = address;
                                textEditZipcode.Text = curAccount.zipcode;
                                textEditCity.Text = curAccount.city;
                            }
                        }
                    }
                }
            }
            WaitDialog.Close();
        }
        
        public int AccountID { get; set; }
        
        public int ContactID { get; set; }

        public ScheduleType BookingType { get; set; }
        
        public byte? SchedulerType { get; set; }
        
        public List<CTBookingSchedule> ListMeetingSchedules { get; set; }

        private void DisableControls() {
            textEditAddress.Properties.ReadOnly = true;
            textEditSubject.Properties.ReadOnly = true;
            textEditCity.Properties.ReadOnly = true;
            textEditZipcode.Properties.ReadOnly = true;
            memoEditDescription.Properties.ReadOnly = true;
            resourcesEditResource.Properties.ReadOnly = true;
            labelEditLabel.Properties.ReadOnly = true;
            statusEditStatus.Properties.ReadOnly = true;
            dateEditStart.Properties.ReadOnly = true;
            dateEditEnd.Properties.ReadOnly = true;
            timeEditStart.Properties.ReadOnly = true;
            timeEditEnd.Properties.ReadOnly = true;
            checkAllDay.Properties.ReadOnly = true;
            btnDelete.Enabled = false;
            btnOk.Enabled = false;
        }

        protected void SuspendUpdate() {
			suspendUpdateCount++;
		}
        protected void ResumeUpdate() {
			if (suspendUpdateCount > 0)
				suspendUpdateCount--;
		}
                       
        protected virtual void UpdateIntervalControls() {
            if (IsUpdateSuspended)
                return;

            SuspendUpdate();
            try {
                
                checkAllDay.Checked = controller.AllDay;
                //if (checkAllDay.Checked) {                    
                //    dateEditEnd.EditValue = controller.End.Date;
                //    dateEditStart.EditValue = controller.Start.Date;
                //    timeEditStart.Time = DateTime.MinValue.AddTicks(controller.Start.TimeOfDay.Ticks);
                //    timeEditEnd.Time = DateTime.MinValue.AddTicks(controller.End.TimeOfDay.Ticks);                  
                //} else {                    
                //    dateEditEnd.EditValue = controller.End.Date;
                //    dateEditStart.EditValue = controller.Start.Date;
                //    timeEditStart.EditValue = DateTime.MinValue.AddTicks(controller.Start.TimeOfDay.Ticks);
                //    timeEditEnd.EditValue = DateTime.MinValue.AddTicks(controller.End.TimeOfDay.Ticks);                                   
                //}
                dateEditEnd.EditValue = controller.End.Date;
                dateEditStart.EditValue = controller.Start.Date;
                timeEditStart.EditValue = new DateTime(controller.Start.TimeOfDay.Ticks).ToString("HH:mm");
                timeEditEnd.EditValue = new DateTime(controller.End.TimeOfDay.Ticks).ToString("HH:mm");

                Appointment editedAptCopy = controller.EditedAppointmentCopy;
                bool showControls = IsNewAppointment || editedAptCopy.Type != AppointmentType.Pattern;                
                dateEditStart.Enabled = showControls;
                dateEditEnd.Enabled = showControls;
                if (checkAllDay.Checked) {
                    layoutControlItemTimeStart.BeginInit();                    
                    layoutControlItemTimeStart.Control = null;
                    layoutControlItemTimeStart.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                    timeEditStart.Visible = false;
                    layoutControlItemTimeStart.EndInit();
                    
                    layoutControlItemTimeEnd.BeginInit();                    
                    layoutControlItemTimeEnd.Control= null;
                    layoutControlItemTimeEnd.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                    timeEditEnd.Visible = false;
                    layoutControlItemTimeEnd.EndInit();
                } else {
                    layoutControlItemTimeStart.BeginInit();
                    layoutControlItemTimeStart.Control = timeEditStart;
                    layoutControlItemTimeStart.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                    timeEditStart.Visible = true;
                    layoutControlItemTimeStart.EndInit();

                    layoutControlItemTimeEnd.BeginInit();
                    layoutControlItemTimeEnd.Control = timeEditEnd;
                    layoutControlItemTimeEnd.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                    timeEditEnd.Visible = true;
                    layoutControlItemTimeEnd.EndInit();
                }
                layoutControlAppointment.Refresh();
                checkAllDay.Enabled = showControls;
            } finally {
                ResumeUpdate();
            }
        }

        private void ShowRecurrenceForm() {
            if (!control.SupportsRecurrence)
                return;
            // Prepare to edit appointment's recurrence.
            Appointment editedAptCopy = controller.EditedAppointmentCopy;
            Appointment editedPattern = controller.EditedPattern;
            Appointment patternCopy = controller.PrepareToRecurrenceEdit();

            AppointmentRecurrenceForm dlg = new AppointmentRecurrenceForm(patternCopy, control.OptionsView.FirstDayOfWeek, controller);

            // Required for skins support.
            dlg.LookAndFeel.ParentLookAndFeel = this.LookAndFeel.ParentLookAndFeel;

            DialogResult result = dlg.ShowDialog(this);
            dlg.Dispose();

            if (result == DialogResult.Abort)
                controller.RemoveRecurrence();
            else
                if (result == DialogResult.OK) {
                    controller.ApplyRecurrence(patternCopy);
                    if (controller.EditedAppointmentCopy != editedAptCopy)
                        UpdateForm();
                }
            UpdateIntervalControls();
        }

        private void UpdateForm() {
            SuspendUpdate();
            try {                
                statusEditStatus.Storage = control.Storage;
                labelEditLabel.Storage = control.Storage;
                resourcesEditResource.Storage = control.Storage;
                resourcesEditResource.ResourceId = controller.ResourceId;

                if (IsNewAppointment) {
                    resourcesEditResource.SelectedIndex = -1;
                }
                resourcesEditResource.Properties.Items.RemoveAt(0);
                //resourcesEditResource.SchedulerControl = control;
                //resourcesEditResource.ResourceIds.Clear();
                //resourcesEditResource.ResourceIds.AddRange(controller.ResourceIds);

                textEditSubject.Text = controller.Subject;
                textEditAddress.Text = controller.Location;
                statusEditStatus.Status = Appointments.Statuses[controller.StatusId];
                labelEditLabel.Label = Appointments.Labels[controller.LabelId];
                
                memoEditDescription.Text = controller.Description;
                
                checkAllDay.Checked = controller.AllDay;

                if (IsNewAppointment) {
                    DateTime dtNow = DateTime.Now;
                    controller.Start = dtNow;
                    controller.End = dtNow.AddMinutes(30);
                    controller.AllDay = true;
                }

                //set custom fields
                textEditZipcode.Text = controller.ZipCode;
                textEditCity.Text = controller.City;
                SchedulerType = controller.ScheduleType; 
                if(SchedulerType != null)
                    BookingType = (ScheduleType) Enum.Parse(typeof(ScheduleType), SchedulerType.ToString());

                if (BookingType == ScheduleType.Webinar || BookingType == ScheduleType.Seminar) {
                    layoutControlItemCity.BeginInit();
                    layoutControlItemCity.Control = null;
                    layoutControlItemCity.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                    textEditCity.Visible = false;
                    layoutControlItemCity.EndInit();

                    if (BookingType == ScheduleType.Seminar) {
                        layoutControlItemLocation.BeginInit();
                        layoutControlItemLocation.Text = "Location";
                        layoutControlItemLocation.EndInit();
                        Text = IsNewAppointment ? "Create Seminar" : "View Seminar";
                    } else if (BookingType == ScheduleType.Webinar) {
                        layoutControlItemLocation.BeginInit();
                        layoutControlItemLocation.Control = null;
                        layoutControlItemLocation.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                        textEditAddress.Visible = false;
                        layoutControlItemLocation.EndInit();
                        Text = IsNewAppointment ? "Create Webinar" : "View Webinar";
                    }

                    layoutControlItemZipcode.BeginInit();
                    layoutControlItemZipcode.Control = null;
                    layoutControlItemZipcode.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                    textEditZipcode.Visible = false;
                    layoutControlItemZipcode.EndInit();                    
                } else if (BookingType == ScheduleType.Meeting) {
                    Text = IsNewAppointment ? "Create Meeting" : "View Meeting";
                }
            } finally {
                ResumeUpdate();
            }
            UpdateIntervalControls();
        }

        private bool IsIntervalValid() {
            DateTime start = dateEditStart.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditStart.EditValue.ToString()).Ticks);
            DateTime end = dateEditEnd.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditEnd.EditValue.ToString()).Ticks);
			return end >= start;
		}

        private void UpdateAppointmentStatus() {
			AppointmentStatus currentStatus = statusEditStatus.Status;
			AppointmentStatus newStatus = controller.UpdateAppointmentStatus(currentStatus);
			if (newStatus != currentStatus)
                statusEditStatus.Status = newStatus;
		}

        private bool ValidRequiredFields()
        {
            if (textEditSubject.Text.Length < 1)
            {
                NotificationDialog.Information("Bright Sales", "Please specify a subject.");
                return false;
            }
            else if (Convert.ToInt32(resourcesEditResource.EditValue) < 1 || resourcesEditResource.Text.Length < 1)
            {
                NotificationDialog.Information("Bright Sales", "Please select a resource.");
                return false;
            }
            string start = dateEditStart.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditStart.EditValue.ToString()).Ticks).ToString();
            string end = dateEditEnd.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditEnd.EditValue.ToString()).Ticks).ToString();
            if (!IsValidSqlDateTimeNative(start)) {
                NotificationDialog.Information("Bright Sales", "Start Time is not valid.");
                return false;
            }
            else if (!IsValidSqlDateTimeNative(end))
            {
                NotificationDialog.Information("Bright Sales", "End Time is not valid.");
                return false;
            }
            return true;
        }

        private bool IsValidSqlDateTimeNative(string someval)
        {
            bool valid = false;
            DateTime testDate = DateTime.MinValue;
            System.Data.SqlTypes.SqlDateTime sdt;
            if (DateTime.TryParse(someval, out testDate))
            {
                try
                {
                    // take advantage of the native conversion
                    sdt = new System.Data.SqlTypes.SqlDateTime(testDate);
                    valid = true;
                }
                catch (System.Data.SqlTypes.SqlTypeException ex)
                {

                    // no need to do anything, this is the expected out of range error
                }
            }

            return valid;
        }
        private void btnOk_Click(object sender, EventArgs e) 
        {
            if (!this.ValidRequiredFields()) {
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }
            if (resourcesEditResource.ResourceId == null) {
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                NotificationDialog.Information("Bright Sales", "Please specify a resource for this schedule.");
                return;
            }
            if (!IsIntervalValid()) {
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                NotificationDialog.Information("Bright Sales", "End time should be greater than start time.");
                return;
            }
            controller.Subject = textEditSubject.Text;
            controller.Location = textEditAddress.Text;
            controller.SetStatus(statusEditStatus.Status);
            controller.SetLabel(labelEditLabel.Label);
            controller.AllDay = this.checkAllDay.Checked;
            controller.Start = dateEditStart.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditStart.EditValue.ToString()).Ticks);
            controller.End = dateEditEnd.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditEnd.EditValue.ToString()).Ticks);
            controller.Description = memoEditDescription.Text;
            controller.ZipCode = textEditZipcode.Text;
            controller.City = textEditCity.Text;
            controller.ScheduleType = (byte) BookingType;

            controller.ResourceId = resourcesEditResource.ResourceId;
            //controller.ResourceIds.Clear();
            //controller.ResourceIds.AddRange(resourcesEditResource.ResourceIds);
            
            
            // Required to check appointment's conflicts.            
            if (!controller.IsConflictResolved()) {
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                NotificationDialog.Error("Bright Sales", "The current schedule conflicted with the schedule of selected resource. Please select other schedule or resource.");
                return;
            }
            // Save all changes made to the appointment edited in a form.
            controller.ApplyChanges();
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Are you sure you want to delete this appointment?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == System.Windows.Forms.DialogResult.No) return;

            if (!controller.IsNewAppointment) {
                if (controller.EditedPattern != null) {
                    Appointment apt = control.SelectedAppointments[0];
                    if (apt != null) {
                        apt.RecurrencePattern.Delete();
                    }
                } else {
                    controller.DeleteAppointment();
                }
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void btnRecurrence_Click(object sender, EventArgs e) {
            ShowRecurrenceForm();
        }

        private void FrmAppointment_Activated(object sender, EventArgs e) {
            // Required to show the recurrence form.
            if (openRecurrenceForm) {
                openRecurrenceForm = false;
                ShowRecurrenceForm();
            }
        }

        private void dateEditStart_EditValueChanged(object sender, EventArgs e) {
            if (!IsUpdateSuspended)
                controller.Start = dateEditStart.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditStart.EditValue.ToString()).Ticks);
            UpdateIntervalControls();
        }

        private void timeEditStart_EditValueChanged(object sender, EventArgs e) {
            if (!IsUpdateSuspended)
                controller.Start = dateEditStart.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditStart.EditValue.ToString()).Ticks);
            UpdateIntervalControls();
        }

        private void timeEditEnd_EditValueChanged(object sender, EventArgs e) {
            if (IsUpdateSuspended) return;
            if (IsIntervalValid())
                controller.End = dateEditEnd.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditEnd.EditValue.ToString()).Ticks);
            else
                timeEditEnd.EditValue = controller.End.TimeOfDay;
        }

        private void dateEditEnd_EditValueChanged(object sender, EventArgs e) {
            if (IsUpdateSuspended) return;
            if (IsIntervalValid())
                controller.End = dateEditEnd.DateTime.Date.AddTicks(TimeSpan.Parse(timeEditEnd.EditValue.ToString()).Ticks);
            else
                dateEditEnd.EditValue = controller.End.Date;
        }

        private void checkEditAllday_CheckedChanged(object sender, EventArgs e) {
            controller.AllDay = this.checkAllDay.Checked;
			if (!IsUpdateSuspended)
				UpdateAppointmentStatus();

			UpdateIntervalControls();
        }

        #region Keyboard Shortcuts

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (FormUtility.ShortCutKeysHandled(keyData))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
    }

    /// <summary>
    /// Create class for customer fields in the appointmen form
    /// </summary>
    public class BVAppointmentFormController : AppointmentFormController {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="control">Scheduler control object</param>
        /// <param name="apt">Appointment object</param>
        public BVAppointmentFormController(SchedulerControl control, Appointment apt) 
            : base(control, apt) {
        }       
        /// <summary>
        /// Gets and sets zipcode source
        /// </summary>
        private string SourceZipCode {
            get {
                return (string)SourceAppointment.CustomFields["zipcode"];
            }
            set {
                SourceAppointment.CustomFields["zipcode"] = value;
            }
        }
        /// <summary>
        /// Gets and sets city source
        /// </summary>
        private string SourceCity {
            get {
                return (string)SourceAppointment.CustomFields["city"];
            }
            set {
                SourceAppointment.CustomFields["city"] = value;
            }
        }
        /// <summary>
        /// Gets and sets scheduler type
        /// </summary>
        private byte? SourceScheduleType {
            get {
                return (byte?)SourceAppointment.CustomFields["schedule_type"];
            }
            set {
                SourceAppointment.CustomFields["schedule_type"] = value;
            }
        }       
        /// <summary>
        /// Gets and sets zipcode
        /// </summary>
        public string ZipCode {
            get {
                return (string)EditedAppointmentCopy.CustomFields["zipcode"];
            }
            set {
                EditedAppointmentCopy.CustomFields["zipcode"] = value;
            }
        }
        /// <summary>
        /// Gets and sets city
        /// </summary>
        public string City {
            get {
                return (string)EditedAppointmentCopy.CustomFields["city"];
            }
            set {
                EditedAppointmentCopy.CustomFields["city"] = value;
            }
        }
        /// <summary>
        /// Gets and sets SchedulerType
        /// </summary>
        public byte? ScheduleType {
            get {
                return (byte?)EditedAppointmentCopy.CustomFields["schedule_type"];
            }
            set {
                EditedAppointmentCopy.CustomFields["schedule_type"] = value;
            }
        }
        /// <summary>
        /// Override inherited class
        /// </summary>
        /// <returns>Boolean</returns>
        public override bool IsAppointmentChanged() {
            if (base.IsAppointmentChanged()) return true;
            return SourceZipCode != ZipCode ||
                SourceCity != City || 
                SourceScheduleType != ScheduleType;
        }
        /// <summary>
        /// Override inherited class
        /// </summary>
        protected override void ApplyCustomFieldsValues() {          
            SourceZipCode = ZipCode;
            SourceCity = City;
            SourceScheduleType = ScheduleType;
        }
    }
}