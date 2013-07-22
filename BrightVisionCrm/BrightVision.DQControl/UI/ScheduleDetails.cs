using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace BrightVision.DQControl.UI {
    public partial class ScheduleDetails : System.Windows.Forms.Form {
        private BrightPlatformEntities BPContext;
        
        public ScheduleDetails(int schedule_id) {
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            DataBind(schedule_id);
        }
        public void DataBind(int schedule_id) {
            var details = BPContext.FIGetScheduleDetail(schedule_id);
            if (details != null) {
                CTScheduleDetail detail = details.FirstOrDefault();
                if (detail != null) {
                    lblScheduleType.Text = detail.schedule_type;
                    lblSubject.Text = detail.subject;
                    lblLocation.Text = detail.location;
                    lblDescription.Text = detail.description;
                    lblStartTime.Text = detail.start_time;
                    lblEndTime.Text = detail.end_time;
                    lblAllDay.Text = detail.all_day;
                    lblSalesPerson.Text = detail.resource_name;
                }
            }
            
        }
    }
}