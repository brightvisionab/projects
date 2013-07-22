
/**
 * Shedule Types:
 * Seminar => 1
 * Webinar => 2
 * Meeting => 3
 */

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraLayout;
using Newtonsoft.Json;

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.DQControl.Business;


namespace BrightVision.DQControl.UI {
    /// <summary>
    /// Represents the Schedule Type of Questionnaire
    /// </summary>
    public class Schedule : LayoutControlGroup, IQuestionnaire {

        #region Member Variables
        private LayoutControlGroup layoutControlGroupQuestion1;                
        private MemoEdit memoEdit1;
        private LookUpEdit lookUpEdit1;
        private LookUpEdit lookUpEdit2;
        private ComboBoxEdit comboBoxEdit1;
        private Label lblScheduleDetails;
        
        private SimpleButton simpleButton1;
        private SimpleButton simpleButton2;
        private SimpleButton simpleButton3;
        private SimpleButton PreviewBooking;
        private SimpleButton CreateMeeting;
        private SimpleButton DeleteMeeting;
        private EmptySpaceItem emptySpaceItem1;      
        //private EmptySpaceItem emptySpaceItem2;
        private LayoutControlItem lciFooter;
        private LayoutControlItem lciSalesPerson;
        private LayoutControlItem lciScheduleType;
        private LayoutControlItem lciListOfScheduleDropDown;
        private LayoutControlItem lciListOfSchedulePreviewButton;
        private LayoutControlItem lciListOfScheduleCreateEditButton;
        private LayoutControlItem lciListOfScheduleDeleteButton;
        private LayoutControlItem lciGridControl;
        private LayoutControlItem lciAddCaller;
        private LayoutControlItem lciAddAdditional;
        private LayoutControlItem lciDeleteCaller;
        private LayoutControlItem lciOtherChoice;
        //private SimpleLabelItem simpleLabelItem1;
        private SimpleLabelItem simpleLabelItemValidation;

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;        
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;        
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;        
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn6;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn7;
        private ContactAttendie m_Caller;
        private Footer ctlFooter;
        private bool isLoaded;
        private bool isResizedEvent;
        private bool isGridDataSourceLoaded;
        private bool m_HasSchedules = false;
        private List<CTBookingSchedule> listBookingSchedules = null;
        private List<CTBookingSchedule> listBookingSalesPerson = null;
        #endregion

        #region Constructors
        public Schedule(IStyleController styleController)
            : base() {
                InitializeComponent();
                StyleController = styleController;
                this.Disposed += new EventHandler(Schedule_Disposed);
        }

        void Schedule_Disposed(object sender, EventArgs e)
        {
            this.layoutControlGroupQuestion1.Click -= new EventHandler(layoutControlGroupQuestion1_Click);
            this.OnComponentNotifyChanged = null;
            ctlFooter.Parent = null;
            ctlFooter = null;
            this.ControlGroup.Tag = null;
            this.ControlGroup = null;
            this.Parent = null;
        }

        public Schedule(IStyleController styleController, string script)
            : base() {
                InitializeComponent();
                StyleController = styleController;
                JSONString = script;
                this.Disposed += new EventHandler(Schedule_Disposed);
        }

        public void InitializeComponent() {
            layoutControlGroupQuestion1 = new LayoutControlGroup();
        }
        #endregion

        #region Properties
        public string JSONString {
            get;
            set;
        }

        public DefaultToolTipController ToolTipController {
            get;
            set;
        }

        public IStyleController StyleController {
            get;
            set;
        }

        public CampaignQuestionnaire Questionnaire {
            get;
            set;
        }
        public CampaignQuestionnaire DefaultQuestionnaire { get; set; }
        public CTScSubCampaignContactList ContactPerson { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        
        public LayoutControlGroup ControlGroup {
            get;
            set;
        }

        public Footer ControlFooter {
            get { return ctlFooter; }
        }

        public bool Focused { 
            get; 
            set; 
        }

        public bool DisableSelection { 
            get; 
            set; 
        }
        
        public bool HasMissingFields { get; set; }

        public List<CTScSubCampaignContactList> ContactList { get; set; }

        public int SubcampaignID { get; set; }

        private bool m_bReadOnly;
        public bool ReadOnly {
            get { return m_bReadOnly; }
            set {
                m_bReadOnly = value;
                SetEditableGroupControls(this.layoutControlGroupQuestion1, !m_bReadOnly);
            }
        }

        private bool m_bHasChanged = false;
        public bool HasChanged {
            get { return m_bHasChanged; }
            set { m_bHasChanged = value; }
        }

        public bool HasMustSaveDefaultValues
        {
            get;
            set;
        }
        public bool IsInitializing
        {
            get;
            set;
        }
        public bool InQuestionnaire
        {
            get;
            set;
        }
        public int QuestionLayoutId {
            get;
            set;
        }
        public int QuestionId
        {
            get;
            set;
        }
        public int AccountId { get; set; }
        public int ContactId { get; set; } //remove...
        #endregion
        
        #region Public Methods
        public event EventHandler ShowCalendarBookingClick;
        /// <summary>
        /// Bind Questionnaire from JSON
        /// </summary>
        public void BindQuestionnaire() {
            try {
                CampaignQuestionnaire objQuestion = JsonConvert.DeserializeObject<CampaignQuestionnaire>(JSONString,
                    new JsonSerializerSettings {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                Questionnaire = objQuestion;
            } catch (Exception ex) {
                throw ex;
            }
        }
        /// <summary>
        /// Bind dynamic controls inside layout control
        /// </summary>
        public void __BindControls() {

            #region Initialization
            if (Questionnaire == null)
                MessageBox.Show("Questionnaire must be bind with JSON first.", "Schedule Component");
            
            var answerOpt = Questionnaire.Form.Settings.AnswerOptions[0] as ISchedule;

            //if (CalendarDataSource == null) {                
            //    if (answerOpt != null) {
            //        if (answerOpt.CalendarOption != null && answerOpt.CalendarOption.CalendarValues.Count > 0)
            //            CalendarDataSource = answerOpt.CalendarOption.CalendarValues;
            //        else {
            //            MessageBox.Show("\"CalendarDataSource\" property must be set first.","Schedule Component");
            //            return;
            //        }
            //    } else {
            //        MessageBox.Show("\"CalendarDataSource\" property must be set first.", "Schedule Component");
            //        return;
            //    }
            //}
            
            #endregion

            isLoaded = false;
            this.layoutControlGroupQuestion1.Clear();
            Settings oSettings = Questionnaire.Form.Settings;
            
            // layoutControlGroupQuestion1                        
            this.layoutControlGroupQuestion1.Name = "layoutControlGroupQuestion" + Guid.NewGuid().ToString();
            this.layoutControlGroupQuestion1.AppearanceGroup.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.layoutControlGroupQuestion1.AppearanceGroup.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.layoutControlGroupQuestion1.AppearanceGroup.Options.UseFont = true;
            this.layoutControlGroupQuestion1.ExpandButtonVisible = false;
            this.layoutControlGroupQuestion1.GroupBordersVisible = true;
            this.layoutControlGroupQuestion1.TextVisible = false;
            this.layoutControlGroupQuestion1.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlGroupQuestion1.ShowInCustomizationForm = false;
            //this.layoutControlGroupQuestion1.Text = oSettings.Label + " " + oSettings.QuestionText;
            this.layoutControlGroupQuestion1.BeginUpdate();

            #region Footer
            this.lciFooter = new LayoutControlItem();
            this.lciFooter.Name = "layoutControlItem" + Guid.NewGuid().ToString();

            //bool isCustomerOwnershipOnly = false;
            //if (oSettings.CustomerOwnership && oSettings.BVOwnership)
            //    isCustomerOwnershipOnly = true;

            //ctlFooter = new Footer() {
            //    IsAccountLevel = oSettings.DataBindings.account_level,
            //    IsCustomerOwnershipOnly = isCustomerOwnershipOnly,
            //    HelpText = oSettings.QuestionHelp,
            //    LanguageCode = oSettings.DataBindings.language_code
            //};

            ctlFooter = new Footer() {
                IsAccountLevel = oSettings.DataBindings.account_level,
                IsCustomerOwned = oSettings.CustomerOwnership,
                IsBrightvisionOwned = oSettings.BVOwnership,
                HelpText = oSettings.QuestionHelp,
                LanguageCode = oSettings.DataBindings.language_code,
                QuestionText = oSettings.Label + " " + oSettings.QuestionText
            };

            ctlFooter.InitializeFooter();
            ctlFooter.Dock = DockStyle.Fill;
            ctlFooter.Height = 20;
            this.lciFooter.Control = ctlFooter;
            this.lciFooter.ShowInCustomizationForm = false;
            this.lciFooter.TextVisible = false;
            this.lciFooter.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.lciFooter.MinSize = new Size(0, 24);
            this.lciFooter.MaxSize = new Size(0, 24);
            this.lciFooter.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            this.lciFooter.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutControlGroupQuestion1.AddItem(this.lciFooter);
            #endregion

            IList<AnswerOption> answeroptionList = Questionnaire.Form.Settings.AnswerOptions;
            ISchedule answeroption = null;
            int iAnswerOptions = answeroptionList.Count;
            //System.Drawing.Size newSize;
            int idx = 0; 
            ScheduleSalesPerson oSalesPerson;
            for (int x = 0; x < iAnswerOptions; ++x) {
                answeroption = answeroptionList[x] as ISchedule;

                #region Sales Person
                // layoutControlItem1                                                         
                this.lciSalesPerson = new LayoutControlItem();
                //lookUpEdit1
                this.lookUpEdit1 = new LookUpEdit();
                this.lookUpEdit1.Name = "SalesPerson_lookUpEdit" + Guid.NewGuid().ToString();
                this.lookUpEdit1.Properties.NullText = "";
                //this.lookUpEdit1.Properties.DisplayMember = "Name";
                //this.lookUpEdit1.Properties.ValueMember = "Id";   
                this.lookUpEdit1.Properties.DisplayMember = "resource_name";
                this.lookUpEdit1.Properties.ValueMember = "resource_id";             
                //this.lookUpEdit1.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name", "SalesPerson"));
                this.lookUpEdit1.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("resource_name", "SalesPerson"));
                this.lookUpEdit1.Properties.ShowFooter = false;
                this.lookUpEdit1.Properties.ShowHeader = false;
                this.lookUpEdit1.Properties.ReadOnly = true;
                this.lookUpEdit1.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

                oSalesPerson = answeroption.ScheduleSalesPerson;
                if(oSalesPerson != null) {
                    if(oSalesPerson.SalesPersonSelectedValue != null) {
                        this.lookUpEdit1.Properties.DataSource = new List<SalesPerson>{oSalesPerson.SalesPersonSelectedValue};
                        this.lookUpEdit1.EditValue = oSalesPerson.SalesPersonSelectedValue.Id;
                    }
                }

                this.lookUpEdit1.Tag = new ScheduleData() {
                    Name = "SalesPerson",
                    PositionIndex = "IndexPosition" + x,
                    ControlContainer = lciSalesPerson
                };
                this.lookUpEdit1.Size = new System.Drawing.Size(155, 20);
                this.lookUpEdit1.StyleController = this.StyleController;
                this.lookUpEdit1.EditValueChanged += new EventHandler(lookUpEdit1_SelectedIndexChanged);
                this.lookUpEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);

                // lciSalesPerson
                this.lciSalesPerson.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciSalesPerson.Control = this.lookUpEdit1;
                if (oSalesPerson != null && !string.IsNullOrEmpty(oSalesPerson.TextPrefix)) {
                    this.lciSalesPerson.Text = oSalesPerson.TextPrefix.Trim();
                    
                } else {
                    this.lciSalesPerson.Text = "Sales Person:";
                }
                this.lciSalesPerson.TextLocation = DevExpress.Utils.Locations.Top;
                this.lciSalesPerson.TextSize = new System.Drawing.Size(108, 13);
                this.lciSalesPerson.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciSalesPerson.MaxSize = new System.Drawing.Size(160, 20);
                this.lciSalesPerson.MinSize = new System.Drawing.Size(160, 20);
                this.lciSalesPerson.Size = new System.Drawing.Size(50, 20);
                this.lciSalesPerson.ShowInCustomizationForm = false;
                this.lciSalesPerson.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                //this.layoutControlGroupQuestion1.AddItem(this.lciSalesPerson);

                #endregion

                #region Schedule Type
                // emptySpaceItem1
                this.emptySpaceItem1 = new EmptySpaceItem();
                this.emptySpaceItem1.Name = "emptySpaceItem" + Guid.NewGuid().ToString();                
                this.emptySpaceItem1.Size = new System.Drawing.Size(25, 20);
                this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
                this.emptySpaceItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.MinSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.ShowInCustomizationForm = false;
                this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                
                // 
                // comboBoxEdit1
                this.lciScheduleType = new DevExpress.XtraLayout.LayoutControlItem();
                this.comboBoxEdit1 = new ComboBoxEdit();
                this.comboBoxEdit1.Tag = new ScheduleData() {
                    Name = "ScheduleType",
                    PositionIndex = "IndexPosition" + x,
                    ControlContainer = lciScheduleType                    
                };

                this.comboBoxEdit1.Size = new System.Drawing.Size(50, 20);
                this.comboBoxEdit1.Name = "ScheduleType_comboBoxEdit" + Guid.NewGuid().ToString();
                this.comboBoxEdit1.Properties.ReadOnly = true;
                this.comboBoxEdit1.StyleController = this.StyleController;
                this.comboBoxEdit1.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                this.comboBoxEdit1.SelectedIndexChanged += new EventHandler(comboBoxEdit1_SelectedIndexChanged);
                this.comboBoxEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                if (answeroption.ScheduleType == null) {
                    answeroption.ScheduleType = new ScheduleType();                    
                }
                if (answeroption.ScheduleType.ScheduleTypeSelectedValue == null) {
                    answeroption.ScheduleType.ScheduleTypeSelectedValue = "";
                }
                answeroption.ScheduleType.ScheduleTypeValues = new List<string> { "Seminar", "Webinar", "Meeting" };

                answeroption.ScheduleType.ScheduleTypeValues.ForEach(delegate(string strValue) {
                    this.comboBoxEdit1.Properties.Items.Add(strValue);
                });


                // lciScheduleType
                this.lciScheduleType.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                //this.layoutControlItem2.AppearanceItemCaption.Options.UseTextOptions = true;
                //this.layoutControlItem2.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                this.lciScheduleType.Control = this.comboBoxEdit1;
                this.lciScheduleType.TextLocation = DevExpress.Utils.Locations.Top;
                this.lciScheduleType.TextSize = new System.Drawing.Size(60, 13);
                if (!string.IsNullOrEmpty(answeroption.ScheduleType.TextPrefix)) {
                    this.lciScheduleType.Text = answeroption.ScheduleType.TextPrefix;                    
                } else {
                    this.lciScheduleType.Text = "Schedule Type:";
                }

                this.lciScheduleType.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciScheduleType.MaxSize = new System.Drawing.Size(160, 24);
                this.lciScheduleType.MinSize = new System.Drawing.Size(160, 24);
                this.lciScheduleType.Size = new System.Drawing.Size(160, 24);
                this.lciScheduleType.ShowInCustomizationForm = false;
                this.lciScheduleType.Click += new EventHandler(layoutControlGroupQuestion1_Click);

                this.layoutControlGroupQuestion1.AddItem(this.lciScheduleType);
                this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1, this.lciScheduleType, DevExpress.XtraLayout.Utils.InsertType.Right);
                this.layoutControlGroupQuestion1.AddItem(this.lciSalesPerson, this.emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Right);

                // emptySpaceItem1
                this.emptySpaceItem1 = new EmptySpaceItem();
                this.emptySpaceItem1.Name = "emptySpaceItem" + Guid.NewGuid().ToString();
                this.emptySpaceItem1.Size = new System.Drawing.Size(1, 15);
                this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
                this.emptySpaceItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 15);
                this.emptySpaceItem1.MinSize = new System.Drawing.Size(0, 15);
                this.emptySpaceItem1.ShowInCustomizationForm = false;
                this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1);
                #endregion

                #region List of Schedules Available
                // lciListOfSchedule                                               
                this.lciListOfScheduleDropDown = new DevExpress.XtraLayout.LayoutControlItem();
                //lookUpEdit1
                this.lookUpEdit2 = new LookUpEdit();
                this.lookUpEdit2.Name = "lookUpEdit" + Guid.NewGuid().ToString();
                this.lookUpEdit2.Properties.NullText = "";
                this.lookUpEdit2.Properties.DisplayMember = "title";
                this.lookUpEdit2.Properties.ValueMember = "id";
                this.lookUpEdit2.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("title", "List of Bookings"));
                comboBoxEdit1_SelectedIndexChanged(this.comboBoxEdit1, null);
                this.lookUpEdit2.Properties.ShowFooter = true;
                this.lookUpEdit2.Properties.ShowHeader = false;
                this.lookUpEdit2.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;                

                this.lookUpEdit2.Tag = new ScheduleData() {
                    Name = "ListOfBookingsAvailable",
                    PositionIndex = "IndexPosition" + x,
                    ControlContainer = lciListOfScheduleDropDown,
                    Required = answeroption.ListOfBookingsAvailableRequired,
                    HasValue = answeroption.ScheduleValue != null  && !string.IsNullOrEmpty(answeroption.ScheduleValue.ScheduleId) ? true : false
                };
                this.lookUpEdit2.Size = new System.Drawing.Size(155, 20);
                this.lookUpEdit2.StyleController = this.StyleController;
                //load answer values
                if (answeroption.ScheduleType != null) {                    
                    if (answeroption.ScheduleValue != null) {
                        if (!string.IsNullOrEmpty(answeroption.ScheduleValue.ScheduleId)) {
                            int schedid = int.Parse(answeroption.ScheduleValue.ScheduleId);
                           
                            lookUpEdit2.EditValue = schedid;
                            //if (answeroption.ScheduleType.ScheduleTypeSelectedValue.ToLower() == "meeting") {
                            //    CreateMeeting.Enabled = true;
                            //}
                        }
                    }
                }
                this.lookUpEdit2.EditValueChanged += new EventHandler(lookUpEdit2_EditValueChanged);
                this.lookUpEdit2.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                
                this.comboBoxEdit1.EditValue = answeroption.ScheduleType.ScheduleTypeSelectedValue;

                // layoutControlItem1
                this.lciListOfScheduleDropDown.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciListOfScheduleDropDown.Control = this.lookUpEdit2;
               
                if (!string.IsNullOrEmpty(answeroption.ListOfBookingsAvailableLabel)) {
                    this.lciListOfScheduleDropDown.Text = answeroption.ListOfBookingsAvailableLabel.Trim();
                } else {
                    this.lciListOfScheduleDropDown.Text = "List of Schedules Available:";
                }
                this.lciListOfScheduleDropDown.TextLocation = DevExpress.Utils.Locations.Top;
                this.lciListOfScheduleDropDown.ShowInCustomizationForm = false;
                this.lciListOfScheduleDropDown.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciListOfScheduleDropDown);
                // 
                // simpleButton1
                //                 
                this.PreviewBooking = new SimpleButton();
                this.PreviewBooking.Name = "PreviewDetails_simpleButton" + Guid.NewGuid().ToString();
                this.PreviewBooking.Size = new System.Drawing.Size(130, 22);
                this.PreviewBooking.StyleController = this.StyleController;
                if(!string.IsNullOrEmpty(answeroption.ViewDetailSummaryButtonLabel))
                    this.PreviewBooking.Text = answeroption.ViewDetailSummaryButtonLabel;
                else
                    this.PreviewBooking.Text = "Preview Details";

                this.PreviewBooking.Click += new EventHandler(viewDetailSummary_Click);
                this.PreviewBooking.Enabled = false;

                // lciListOfSchedulePreviewButton
                this.lciListOfSchedulePreviewButton = new LayoutControlItem();
                this.lciListOfSchedulePreviewButton.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciListOfSchedulePreviewButton.Control = this.PreviewBooking;
                this.lciListOfSchedulePreviewButton.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciListOfSchedulePreviewButton.MaxSize = new System.Drawing.Size(100, 24);
                this.lciListOfSchedulePreviewButton.MinSize = new System.Drawing.Size(80, 24);
                this.lciListOfSchedulePreviewButton.Size = new System.Drawing.Size(80, 24);
                this.lciListOfSchedulePreviewButton.TextVisible = false;
                this.lciListOfSchedulePreviewButton.ShowInCustomizationForm = false;
                this.lciListOfSchedulePreviewButton.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciListOfSchedulePreviewButton);
                               
                // 
                // simpleButton1
                //                 
                this.CreateMeeting = new SimpleButton();                
                this.CreateMeeting.Name = "CreateMeeting_simpleButton" + Guid.NewGuid().ToString();
                this.CreateMeeting.Size = new System.Drawing.Size(130, 22);
                this.CreateMeeting.StyleController = this.StyleController;
                if (!string.IsNullOrEmpty(answeroption.CreateMeetingButtonLabel)) {
                    if (answeroption.CreateMeetingButtonLabel.Trim().ToLower() == "create meeting") {
                        this.CreateMeeting.Text = "Create/Edit Meeting";
                    } else {
                        this.CreateMeeting.Text = answeroption.CreateMeetingButtonLabel;
                    }
                } else {
                    this.CreateMeeting.Text = "Create/Edit Meeting";
                }
                this.CreateMeeting.Click += new EventHandler(createMeeting_Click);                
                this.CreateMeeting.Enabled = false;                

                // layoutControlItem1
                this.lciListOfScheduleCreateEditButton = new LayoutControlItem();
                if (answeroption.ScheduleType != null && answeroption.ScheduleType.ScheduleTypeSelectedValue == "Meeting") {
                    //lookUpEdit2.Properties.ReadOnly = true;
                    this.CreateMeeting.Tag = "HasMeeting";
                } else
                    this.lciListOfScheduleCreateEditButton.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                this.lciListOfScheduleCreateEditButton.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciListOfScheduleCreateEditButton.Control = this.CreateMeeting;
                this.lciListOfScheduleCreateEditButton.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciListOfScheduleCreateEditButton.MaxSize = new System.Drawing.Size(115, 24);
                this.lciListOfScheduleCreateEditButton.MinSize = new System.Drawing.Size(115, 24);
                this.lciListOfScheduleCreateEditButton.Size = new System.Drawing.Size(115, 24);
                this.lciListOfScheduleCreateEditButton.TextVisible = false;
                this.lciListOfScheduleCreateEditButton.ShowInCustomizationForm = false;
                this.lciListOfScheduleCreateEditButton.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciListOfScheduleCreateEditButton, this.lciListOfSchedulePreviewButton, DevExpress.XtraLayout.Utils.InsertType.Right);
                // 
                // simpleButton1
                //                 
                this.DeleteMeeting = new SimpleButton();
                this.DeleteMeeting.Name = "DeleteMeeting_simpleButton" + Guid.NewGuid().ToString();
                this.DeleteMeeting.Size = new System.Drawing.Size(130, 22);
                this.DeleteMeeting.StyleController = this.StyleController;
                this.DeleteMeeting.Text = "Delete Meeting";
                this.DeleteMeeting.Click += new EventHandler(deleteMeeting_Click);
                this.DeleteMeeting.Enabled = false;

                // lciListOfScheduleDeleteButton
                this.lciListOfScheduleDeleteButton = new LayoutControlItem();
                if (answeroption.ScheduleType != null && answeroption.ScheduleType.ScheduleTypeSelectedValue == "Meeting") {
                    this.DeleteMeeting.Tag = "HasMeeting";
                    //lookUpEdit2.Properties.ReadOnly = true;
                    var lueData = lookUpEdit2.Tag as ScheduleData;
                    lueData.IsMeeting = true;
                } else
                    this.lciListOfScheduleDeleteButton.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                this.lciListOfScheduleDeleteButton.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciListOfScheduleDeleteButton.Control = this.DeleteMeeting;
                this.lciListOfScheduleDeleteButton.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciListOfScheduleDeleteButton.MaxSize = new System.Drawing.Size(100, 24);
                this.lciListOfScheduleDeleteButton.MinSize = new System.Drawing.Size(80, 24);
                this.lciListOfScheduleDeleteButton.Size = new System.Drawing.Size(80, 24);
                this.lciListOfScheduleDeleteButton.TextVisible = false;
                this.lciListOfScheduleDeleteButton.ShowInCustomizationForm = false;
                this.lciListOfScheduleDeleteButton.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciListOfScheduleDeleteButton, this.lciListOfScheduleCreateEditButton, DevExpress.XtraLayout.Utils.InsertType.Right);

                ////load answer values
                //if (answeroption.ScheduleType != null) {                    
                //    if (answeroption.ScheduleValue != null) {
                //        if (!string.IsNullOrEmpty(answeroption.ScheduleValue.ScheduleId)) {
                //            int schedid = int.Parse(answeroption.ScheduleValue.ScheduleId);
                           
                //            lookUpEdit2.EditValue = schedid;
                //            //if (answeroption.ScheduleType.ScheduleTypeSelectedValue.ToLower() == "meeting") {
                //            //    CreateMeeting.Enabled = true;
                //            //}
                //        }
                //    }
                //}

                // emptySpaceItem1
                this.emptySpaceItem1 = new EmptySpaceItem();
                this.emptySpaceItem1.Name = "emptySpaceItem" + Guid.NewGuid().ToString();
                this.emptySpaceItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.MinSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.Size = new System.Drawing.Size(200, 20);
                this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
                this.emptySpaceItem1.ShowInCustomizationForm = false;
                this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1, this.lciListOfScheduleDeleteButton, DevExpress.XtraLayout.Utils.InsertType.Right);
                #endregion
                
                #region Customer Attendies

                #region Grid
                simpleLabelItemValidation = new SimpleLabelItem();
                simpleLabelItemValidation.AppearanceItemCaption.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                simpleLabelItemValidation.Name = Guid.NewGuid().ToString();
                simpleLabelItemValidation.TextAlignMode = TextAlignModeItem.CustomSize;
                simpleLabelItemValidation.TextSize = new Size(212, 20);
                simpleLabelItemValidation.Text = "  Please add at least one contact attendie.";
                simpleLabelItemValidation.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                this.layoutControlGroupQuestion1.AddItem(simpleLabelItemValidation);

                //create our gridcontrol
                // 
                // gridColumn1
                // 
                this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn1.Caption = "Name";
                this.gridColumn1.FieldName = "Name";
                this.gridColumn1.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn1.Visible = true;
                this.gridColumn1.OptionsColumn.AllowEdit = false;
                this.gridColumn1.VisibleIndex = 0;
                this.gridColumn1.Width = 66;
                // 
                // gridColumn2
                // 
                this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn2.Caption = "Address";
                this.gridColumn2.FieldName = "Address";
                this.gridColumn2.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn2.Visible = true;
                this.gridColumn2.OptionsColumn.AllowEdit = false;
                this.gridColumn2.VisibleIndex = 1;
                this.gridColumn2.Width = 66;
                // 
                // gridColumn3
                // 
                //this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
                //this.gridColumn3.Caption = "City";
                //this.gridColumn3.FieldName = "City";
                //this.gridColumn3.Name = "gridColumn" + Guid.NewGuid().ToString();
                //this.gridColumn3.Visible = true;
                //this.gridColumn3.OptionsColumn.AllowEdit = false;
                //this.gridColumn3.VisibleIndex = 2;
                //this.gridColumn3.Width = 66;
                // 
                // gridColumn4
                // 
                this.gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn4.Caption = "Telephone";
                this.gridColumn4.FieldName = "Telephone";
                this.gridColumn4.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn4.Visible = true;
                this.gridColumn4.OptionsColumn.AllowEdit = false;
                this.gridColumn4.VisibleIndex = 3;
                this.gridColumn4.Width = 66;
                // 
                // gridColumn5
                // 
                this.gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn5.Caption = "Email";
                this.gridColumn5.FieldName = "Email";
                this.gridColumn5.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn5.Visible = true;
                this.gridColumn5.OptionsColumn.AllowEdit = false;
                this.gridColumn5.VisibleIndex = 4;
                this.gridColumn5.Width = 70;
                // 
                // gridColumn6
                // 
                this.gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn6.Caption = "AccountID";
                this.gridColumn6.FieldName = "AccountID";
                this.gridColumn6.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn6.Visible = false;
                // 
                // gridColumn7
                // 
                this.gridColumn7 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn7.Caption = "ContactID";
                this.gridColumn7.FieldName = "ContactID";
                this.gridColumn7.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn7.Visible = false;
                                               
                // 
                // gridView1
                // 
                this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
                this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
                this.gridColumn1,
                this.gridColumn2,
                //this.gridColumn3,
                this.gridColumn4,
                this.gridColumn5,
                this.gridColumn6,
                this.gridColumn7});
                this.gridView1.GridControl = this.gridControl1;
                this.gridView1.Name = "gridView" + Guid.NewGuid().ToString();
                this.gridView1.OptionsFind.AlwaysVisible = false;
                this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
                this.gridView1.OptionsSelection.MultiSelect = true;
                this.gridView1.OptionsView.ShowGroupPanel = false;
                this.gridView1.OptionsView.ColumnAutoWidth = true;
                this.gridView1.DataSourceChanged += new EventHandler(gridView1_DataSourceChanged);                
                // 
                // gridControl1
                // 
                this.gridControl1 = new DevExpress.XtraGrid.GridControl();
                this.gridControl1.LookAndFeel.UseDefaultLookAndFeel = false;
                this.gridControl1.MainView = this.gridView1;
                this.gridControl1.Name = "gridControl" + Guid.NewGuid().ToString();
                this.gridControl1.Size = new System.Drawing.Size(150, 74);
                this.gridControl1.TabIndex = 11;
                this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {                    
                this.gridView1});
                SetCurrentAttendies();
                var schedData = new ScheduleData() {
                    Required = answeroption.AttendiesRequired                   
                };
                if (gridView1.GridControl.DataSource != null) {
                    List<ContactAttendie> gvDs = gridView1.GridControl.DataSource as List<ContactAttendie>;
                    if (gvDs != null && gvDs.Count > 0)
                        schedData.HasValue = true;
                }
                this.gridView1.Tag = schedData;
                // 
                // lciGridControl
                // 
                this.lciGridControl = new LayoutControlItem();
                this.lciGridControl.Control = this.gridControl1;
                this.lciGridControl.Name = "layoutControlItem" + Guid.NewGuid().ToString();                
                if(!string.IsNullOrEmpty(answeroption.AttendiesLabel))
                    this.lciGridControl.Text = answeroption.AttendiesLabel;
                else
                    this.lciGridControl.Text = "Attendies:";
                this.lciGridControl.TextLocation = DevExpress.Utils.Locations.Top;
                this.lciGridControl.TextSize = new System.Drawing.Size(108, 13);
                this.lciGridControl.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciGridControl.MaxSize = new System.Drawing.Size(0, 90);
                this.lciGridControl.MinSize = new System.Drawing.Size(0, 90);
                this.lciGridControl.Size = new System.Drawing.Size(200, 90);
                this.lciGridControl.ShowInCustomizationForm = false;
                this.layoutControlGroupQuestion1.AddItem(this.lciGridControl);
                #endregion

                // 
                // simpleButton1
                //                 
                this.simpleButton1 = new SimpleButton();
                this.simpleButton1.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.simpleButton1.Size = new System.Drawing.Size(130, 22);
                this.simpleButton1.StyleController = this.StyleController;
                this.simpleButton1.Enabled = false;
                if(!string.IsNullOrEmpty(answeroption.AddCallerButtonLabel))
                    this.simpleButton1.Text = answeroption.AddCallerButtonLabel;
                else
                    this.simpleButton1.Text = "Add Caller";
                this.simpleButton1.Click += new EventHandler(AddCaller_Click);

                // lciAddCaller
                this.lciAddCaller = new LayoutControlItem();
                this.lciAddCaller.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciAddCaller.Control = this.simpleButton1;
                this.lciAddCaller.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciAddCaller.MaxSize = new System.Drawing.Size(80, 24);
                this.lciAddCaller.MinSize = new System.Drawing.Size(80, 24);
                this.lciAddCaller.Size = new System.Drawing.Size(80, 24);
                this.lciAddCaller.TextVisible = false;
                this.lciAddCaller.ShowInCustomizationForm = false;
                this.lciAddCaller.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciAddCaller);

                // 
                // simpleButton2
                //                 
                this.simpleButton2 = new SimpleButton();
                this.simpleButton2.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.simpleButton2.Size = new System.Drawing.Size(130, 22);
                this.simpleButton2.StyleController = this.StyleController;
                this.simpleButton2.Enabled = false;
                if(!string.IsNullOrEmpty(answeroption.AddAdditionalAttendieButtonLabel))
                    this.simpleButton2.Text = answeroption.AddAdditionalAttendieButtonLabel;
                else
                    this.simpleButton2.Text = "Add Additional";
                this.simpleButton2.Click += new EventHandler(AddAdditional_Click);

                // layoutControlItem1
                this.lciAddAdditional = new LayoutControlItem();
                this.lciAddAdditional.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciAddAdditional.Control = this.simpleButton2;
                this.lciAddAdditional.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciAddAdditional.MaxSize = new System.Drawing.Size(100, 24);
                this.lciAddAdditional.MinSize = new System.Drawing.Size(80, 24);
                this.lciAddAdditional.Size = new System.Drawing.Size(80, 24);
                this.lciAddAdditional.TextVisible = false;
                this.lciAddAdditional.ShowInCustomizationForm = false;
                this.lciAddAdditional.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciAddAdditional, this.lciAddCaller, DevExpress.XtraLayout.Utils.InsertType.Right);

                // 
                // simpleButton3
                //                 
                this.simpleButton3 = new SimpleButton();
                this.simpleButton3.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.simpleButton3.Size = new System.Drawing.Size(130, 22);
                this.simpleButton3.StyleController = this.StyleController;
                this.simpleButton3.Enabled = false;
                if(!string.IsNullOrEmpty(answeroption.DeleteAttendieButtonLabel))
                    this.simpleButton3.Text = answeroption.DeleteAttendieButtonLabel;
                else
                    this.simpleButton3.Text = "Delete";
                this.simpleButton3.Click += new EventHandler(DeleteAttendie_Click);

                // layoutControlItem2
                this.lciDeleteCaller = new LayoutControlItem();
                this.lciDeleteCaller.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciDeleteCaller.Control = this.simpleButton3;
                this.lciDeleteCaller.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciDeleteCaller.MaxSize = new System.Drawing.Size(70, 24);
                this.lciDeleteCaller.MinSize = new System.Drawing.Size(70, 24);
                this.lciDeleteCaller.Size = new System.Drawing.Size(70, 24);
                this.lciDeleteCaller.TextVisible = false;
                this.lciDeleteCaller.ShowInCustomizationForm = false;
                this.lciDeleteCaller.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciDeleteCaller, this.lciAddAdditional, DevExpress.XtraLayout.Utils.InsertType.Right);

                // emptySpaceItem1
                this.emptySpaceItem1 = new EmptySpaceItem();
                this.emptySpaceItem1.Name = "emptySpaceItem" + Guid.NewGuid().ToString();
                this.emptySpaceItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;                
                this.emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.MinSize = new System.Drawing.Size(0, 20);                
                this.emptySpaceItem1.Size = new System.Drawing.Size(200, 20);
                this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
                this.emptySpaceItem1.ShowInCustomizationForm = false;
                this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1, this.lciDeleteCaller, DevExpress.XtraLayout.Utils.InsertType.Right);

                #endregion

                #region Other Choices
                idx = 0;
                //EmptySpaceItem esitem = new EmptySpaceItem();
                //esitem.Size = new Size(100, 20);
                //this.layoutControlGroupQuestion1.AddItem(esitem);
                foreach (OtherChoice oChoice in answeroption.OtherChoices) {
                    if (oChoice.Enabled) {
                        //if (!string.IsNullOrEmpty(oChoice.TextPrefix)) {
                        //    // simpleLabelItem1                
                        //    this.simpleLabelItem1 = new DevExpress.XtraLayout.SimpleLabelItem();

                        //    this.simpleLabelItem1.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
                        //    //this.simpleLabelItem1.ShowInCustomizationForm = false;
                        //    //this.simpleLabelItem1.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                        //    this.simpleLabelItem1.Text = oChoice.TextPrefix;
                        //    //newSize = new System.Drawing.Size(100, 20);
                        //    newSize = new System.Drawing.Size(20, 20);
                        //    this.simpleLabelItem1.SizeConstraintsType = SizeConstraintsType.Custom;
                        //    this.simpleLabelItem1.MaxSize = newSize;
                        //    this.simpleLabelItem1.MinSize = newSize;
                        //    this.simpleLabelItem1.Size = newSize;
                        //    this.simpleLabelItem1.AppearanceItemCaption.BackColor = System.Drawing.Color.Transparent;
                        //    this.simpleLabelItem1.ShowInCustomizationForm = false;
                        //    this.simpleLabelItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                        //    this.layoutControlGroupQuestion1.AddItem(this.simpleLabelItem1);
                        //} else {
                        //    this.simpleLabelItem1 = null;
                        //}
                        // layoutControlItem1                                                         
                        this.lciOtherChoice = new LayoutControlItem();
                        //add textEdit1 to layout
                        this.memoEdit1 = new MemoEdit();
                        this.memoEdit1.Tag = new ScheduleData() {
                            ParentPositionIndex = "IndexPosition" + x.ToString(),
                            PositionIndex = "IndexPosition" + idx.ToString(),
                            ControlContainer = lciOtherChoice,
                            Required = oChoice.Required,
                            HasValue = !string.IsNullOrWhiteSpace(oChoice.DefaultInputValue) ? true : false,
                            ChoiceOption = oChoice
                        };
                        this.memoEdit1.Name = "textEdit" + Guid.NewGuid().ToString();
                        this.memoEdit1.Properties.ScrollBars = System.Windows.Forms.ScrollBars.None;
                        this.memoEdit1.StyleController = this.StyleController;
                        if (oChoice.DefaultInputValue != null) {
                            this.memoEdit1.Text = oChoice.DefaultInputValue.Trim();
                            oChoice.InputValue = oChoice.DefaultInputValue.Trim();
                        }
                        memoEdit1.TextChanged += new EventHandler(memoEdit1_TextChanged);
                        memoEdit1.Resize += new EventHandler(memoEdit1_Resize);
                        memoEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);

                        this.lciOtherChoice.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                        this.lciOtherChoice.Control = this.memoEdit1;

                        if (!string.IsNullOrEmpty(oChoice.TextPrefix)) {
                            this.lciOtherChoice.Text = oChoice.TextPrefix;
                            this.lciOtherChoice.TextVisible = true;
                            this.lciOtherChoice.TextLocation = DevExpress.Utils.Locations.Top;
                            this.lciOtherChoice.MaxSize = new Size(0, 41);
                            this.lciOtherChoice.MinSize = new Size(0, 41);
                        } else {
                            this.lciOtherChoice.TextVisible = false;
                            this.lciOtherChoice.MaxSize = new Size(0, 24);
                            this.lciOtherChoice.MinSize = new Size(0, 24);
                        }
                        this.lciOtherChoice.SizeConstraintsType = SizeConstraintsType.Custom;
                        this.lciOtherChoice.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                        this.lciOtherChoice.ShowInCustomizationForm = false;
                        this.layoutControlGroupQuestion1.AddItem(this.lciOtherChoice);                                                
                        idx++;
                    }
                }
                #endregion

            }

            #region Footer
            //string prioText = oSettings.Priority;
            //if (oSettings.CustomerOwnership && oSettings.BVOwnership) {
            //    prioText += "(Cust+BV)";
            //} else if (oSettings.CustomerOwnership) {
            //    prioText += "(Cust)";
            //} else if (oSettings.BVOwnership) {
            //    prioText += "(BV)";
            //}
            //if (oSettings.PlotDoneStatus.Trim().ToLower() == "done") {
            //    prioText += " Done";
            //}

            //// simpleLabelItem1 status
            //this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            //this.emptySpaceItem1.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
            //this.emptySpaceItem1.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            //this.emptySpaceItem1.AppearanceItemCaption.Options.UseTextOptions = true;
            //this.emptySpaceItem1.ShowInCustomizationForm = false;
            //this.emptySpaceItem1.SizeConstraintsType = SizeConstraintsType.Custom;
            //this.emptySpaceItem1.Text = prioText;
            //this.emptySpaceItem1.TextVisible = true;
            //this.emptySpaceItem1.ShowInCustomizationForm = false;
            //this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            //this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1);

            //if (!string.IsNullOrEmpty(oSettings.QuestionHelp)) {
            //    this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            //    this.emptySpaceItem2.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
            //    this.emptySpaceItem2.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            //    this.emptySpaceItem2.AppearanceItemCaption.Options.UseTextOptions = true;
            //    this.emptySpaceItem2.ShowInCustomizationForm = false;
            //    this.emptySpaceItem2.Text = "Help";
            //    this.emptySpaceItem2.OptionsToolTip.ToolTip = oSettings.QuestionHelp.Trim();

            //    //apply tooltip controller from parent
            //    //if (this.ToolTipController != null && this.ToolTipController.DefaultController != null) {
            //    //    this.emptySpaceItem2.OptionsToolTip.ToolTipController = ToolTipController.DefaultController;
            //    //}

            //    this.emptySpaceItem2.TextVisible = true;
            //    this.emptySpaceItem2.Size = new Size(50, 20);
            //    this.emptySpaceItem2.MaxSize = new Size(50, 20);
            //    this.emptySpaceItem2.MinSize = new Size(50, 20);
            //    this.emptySpaceItem2.SizeConstraintsType = SizeConstraintsType.Custom;
            //    this.emptySpaceItem2.ShowInCustomizationForm = false;
            //    this.emptySpaceItem2.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            //    this.emptySpaceItem2.AppearanceItemCaption.TextOptions.HAlignment = HorzAlignment.Far;
            //    this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem2, this.emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Right);
            //}
            #endregion

            this.layoutControlGroupQuestion1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            this.layoutControlGroupQuestion1.EndUpdate();            
            Image bg = BGImage(this);
            if (bg != null) {
                this.layoutControlGroupQuestion1.BackgroundImage = bg;
                this.layoutControlGroupQuestion1.BackgroundImageVisible = true;
            }
            if (this.memoEdit1 != null)
                BackColor = this.memoEdit1.BackColor;

            //lookUpEdit2.EditValue = null;
            //lookUpEdit2.ItemIndex = 0;

            this.ControlGroup = this.layoutControlGroupQuestion1;
            this.ControlGroup.Tag = this;
            isLoaded = true;
        }

        /*
         * https://brightvision.jira.com/browse/PLATFORM-3015
         */
        public void BindControls()
        {

            #region Initialization
            if (Questionnaire == null)
                MessageBox.Show("Questionnaire must be bind with JSON first.", "Schedule Component");

            var answerOpt = Questionnaire.Form.Settings.AnswerOptions[0] as ISchedule;

            //if (CalendarDataSource == null) {                
            //    if (answerOpt != null) {
            //        if (answerOpt.CalendarOption != null && answerOpt.CalendarOption.CalendarValues.Count > 0)
            //            CalendarDataSource = answerOpt.CalendarOption.CalendarValues;
            //        else {
            //            MessageBox.Show("\"CalendarDataSource\" property must be set first.","Schedule Component");
            //            return;
            //        }
            //    } else {
            //        MessageBox.Show("\"CalendarDataSource\" property must be set first.", "Schedule Component");
            //        return;
            //    }
            //}

            #endregion

            isLoaded = false;
            this.layoutControlGroupQuestion1.Clear();
            Settings oSettings = Questionnaire.Form.Settings;

            // layoutControlGroupQuestion1                        
            this.layoutControlGroupQuestion1.Name = "layoutControlGroupQuestion" + Guid.NewGuid().ToString();
            this.layoutControlGroupQuestion1.AppearanceGroup.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.layoutControlGroupQuestion1.AppearanceGroup.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.layoutControlGroupQuestion1.AppearanceGroup.Options.UseFont = true;
            this.layoutControlGroupQuestion1.ExpandButtonVisible = false;
            this.layoutControlGroupQuestion1.GroupBordersVisible = true;
            this.layoutControlGroupQuestion1.TextVisible = false;
            this.layoutControlGroupQuestion1.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlGroupQuestion1.ShowInCustomizationForm = false;
            //this.layoutControlGroupQuestion1.Text = oSettings.Label + " " + oSettings.QuestionText;
            this.layoutControlGroupQuestion1.BeginUpdate();

            #region Footer
            this.lciFooter = new LayoutControlItem();
            this.lciFooter.Name = "layoutControlItem" + Guid.NewGuid().ToString();

            //bool isCustomerOwnershipOnly = false;
            //if (oSettings.CustomerOwnership && oSettings.BVOwnership)
            //    isCustomerOwnershipOnly = true;

            //ctlFooter = new Footer() {
            //    IsAccountLevel = oSettings.DataBindings.account_level,
            //    IsCustomerOwnershipOnly = isCustomerOwnershipOnly,
            //    HelpText = oSettings.QuestionHelp,
            //    LanguageCode = oSettings.DataBindings.language_code
            //};

            ctlFooter = new Footer()
            {
                IsAccountLevel = oSettings.DataBindings.account_level,
                IsCustomerOwned = oSettings.CustomerOwnership,
                IsBrightvisionOwned = oSettings.BVOwnership,
                HelpText = oSettings.QuestionHelp,
                LanguageCode = oSettings.DataBindings.language_code,
                QuestionText = oSettings.Label + " " + oSettings.QuestionText
            };

            ctlFooter.InitializeFooter();
            ctlFooter.Dock = DockStyle.Fill;
            ctlFooter.Height = 20;
            this.lciFooter.Control = ctlFooter;
            this.lciFooter.ShowInCustomizationForm = false;
            this.lciFooter.TextVisible = false;
            this.lciFooter.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.lciFooter.MinSize = new Size(0, 24);
            this.lciFooter.MaxSize = new Size(0, 24);
            this.lciFooter.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            this.lciFooter.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutControlGroupQuestion1.AddItem(this.lciFooter);
            #endregion

            IList<AnswerOption> answeroptionList = Questionnaire.Form.Settings.AnswerOptions;
            ISchedule answeroption = null;
            int iAnswerOptions = answeroptionList.Count;
            //System.Drawing.Size newSize;
            int idx = 0;
            ScheduleSalesPerson oSalesPerson;
            for (int x = 0; x < iAnswerOptions; ++x)
            {
                answeroption = answeroptionList[x] as ISchedule;

                LayoutControlItem lciScheduleDetails = new LayoutControlItem();
                this.lblScheduleDetails = new Label();
                this.lblScheduleDetails.Text = "";
                lciScheduleDetails.Control = this.lblScheduleDetails;
                lciScheduleDetails.TextVisible = false;
                this.layoutControlGroupQuestion1.AddItem(lciScheduleDetails);

                #region Sales Person
                // layoutControlItem1                                                         
                this.lciSalesPerson = new LayoutControlItem();
                //lookUpEdit1
                this.lookUpEdit1 = new LookUpEdit();
                this.lookUpEdit1.Name = "SalesPerson_lookUpEdit" + Guid.NewGuid().ToString();
                this.lookUpEdit1.Properties.NullText = "";
                //this.lookUpEdit1.Properties.DisplayMember = "Name";
                //this.lookUpEdit1.Properties.ValueMember = "Id";   
                this.lookUpEdit1.Properties.DisplayMember = "resource_name";
                this.lookUpEdit1.Properties.ValueMember = "resource_id";
                //this.lookUpEdit1.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name", "SalesPerson"));
                this.lookUpEdit1.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("resource_name", "SalesPerson"));
                this.lookUpEdit1.Properties.ShowFooter = false;
                this.lookUpEdit1.Properties.ShowHeader = false;
                this.lookUpEdit1.Properties.ReadOnly = true;
                this.lookUpEdit1.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

                oSalesPerson = answeroption.ScheduleSalesPerson;
                if (oSalesPerson != null)
                {
                    if (oSalesPerson.SalesPersonSelectedValue != null)
                    {
                        this.lookUpEdit1.Properties.DataSource = new List<SalesPerson> { oSalesPerson.SalesPersonSelectedValue };
                        this.lookUpEdit1.EditValue = oSalesPerson.SalesPersonSelectedValue.Id;
                    }
                }

                this.lookUpEdit1.Tag = new ScheduleData()
                {
                    Name = "SalesPerson",
                    PositionIndex = "IndexPosition" + x,
                    ControlContainer = lciSalesPerson
                };
                this.lookUpEdit1.Size = new System.Drawing.Size(155, 20);
                this.lookUpEdit1.StyleController = this.StyleController;
                //this.lookUpEdit1.EditValueChanged += new EventHandler(lookUpEdit1_SelectedIndexChanged);
                this.lookUpEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);

                // lciSalesPerson
                this.lciSalesPerson.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciSalesPerson.Control = this.lookUpEdit1;
                if (oSalesPerson != null && !string.IsNullOrEmpty(oSalesPerson.TextPrefix))
                {
                    this.lciSalesPerson.Text = oSalesPerson.TextPrefix.Trim();

                }
                else
                {
                    this.lciSalesPerson.Text = "Sales Person:";
                }
                this.lciSalesPerson.TextLocation = DevExpress.Utils.Locations.Top;
                this.lciSalesPerson.TextSize = new System.Drawing.Size(108, 13);
                this.lciSalesPerson.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciSalesPerson.MaxSize = new System.Drawing.Size(160, 20);
                this.lciSalesPerson.MinSize = new System.Drawing.Size(160, 20);
                this.lciSalesPerson.Size = new System.Drawing.Size(50, 20);
                this.lciSalesPerson.ShowInCustomizationForm = false;
                this.lciSalesPerson.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                //this.layoutControlGroupQuestion1.AddItem(this.lciSalesPerson);

                #endregion

                #region Schedule Type
                // emptySpaceItem1
                this.emptySpaceItem1 = new EmptySpaceItem();
                this.emptySpaceItem1.Name = "emptySpaceItem" + Guid.NewGuid().ToString();
                this.emptySpaceItem1.Size = new System.Drawing.Size(25, 20);
                this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
                this.emptySpaceItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.MinSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.ShowInCustomizationForm = false;
                this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);

                // 
                // comboBoxEdit1
                this.lciScheduleType = new DevExpress.XtraLayout.LayoutControlItem();
                this.comboBoxEdit1 = new ComboBoxEdit();
                this.comboBoxEdit1.Tag = new ScheduleData()
                {
                    Name = "ScheduleType",
                    PositionIndex = "IndexPosition" + x,
                    ControlContainer = lciScheduleType
                };

                this.comboBoxEdit1.Size = new System.Drawing.Size(50, 20);
                this.comboBoxEdit1.Name = "ScheduleType_comboBoxEdit" + Guid.NewGuid().ToString();
                this.comboBoxEdit1.Properties.ReadOnly = true;
                this.comboBoxEdit1.StyleController = this.StyleController;
                this.comboBoxEdit1.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                this.comboBoxEdit1.SelectedIndexChanged += new EventHandler(comboBoxEdit1_SelectedIndexChanged);
                this.comboBoxEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                if (answeroption.ScheduleType == null)
                {
                    answeroption.ScheduleType = new ScheduleType();
                }
                if (answeroption.ScheduleType.ScheduleTypeSelectedValue == null)
                {
                    answeroption.ScheduleType.ScheduleTypeSelectedValue = "";
                }
                answeroption.ScheduleType.ScheduleTypeValues = new List<string> { "Seminar", "Webinar", "Meeting" };

                answeroption.ScheduleType.ScheduleTypeValues.ForEach(delegate(string strValue)
                {
                    this.comboBoxEdit1.Properties.Items.Add(strValue);
                });


                // lciScheduleType
                this.lciScheduleType.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                //this.layoutControlItem2.AppearanceItemCaption.Options.UseTextOptions = true;
                //this.layoutControlItem2.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                this.lciScheduleType.Control = this.comboBoxEdit1;
                this.lciScheduleType.TextLocation = DevExpress.Utils.Locations.Top;
                this.lciScheduleType.TextSize = new System.Drawing.Size(60, 13);
                if (!string.IsNullOrEmpty(answeroption.ScheduleType.TextPrefix))
                {
                    this.lciScheduleType.Text = answeroption.ScheduleType.TextPrefix;
                }
                else
                {
                    this.lciScheduleType.Text = "Schedule Type:";
                }

                this.lciScheduleType.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciScheduleType.MaxSize = new System.Drawing.Size(160, 24);
                this.lciScheduleType.MinSize = new System.Drawing.Size(160, 24);
                this.lciScheduleType.Size = new System.Drawing.Size(160, 24);
                this.lciScheduleType.ShowInCustomizationForm = false;
                this.lciScheduleType.Click += new EventHandler(layoutControlGroupQuestion1_Click);


                this.lciSalesPerson.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.OnlyInCustomization;
                this.lciScheduleType.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.OnlyInCustomization;
                this.emptySpaceItem1.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.OnlyInCustomization;

                this.layoutControlGroupQuestion1.AddItem(this.lciScheduleType);
                this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1, this.lciScheduleType, DevExpress.XtraLayout.Utils.InsertType.Right);
                this.layoutControlGroupQuestion1.AddItem(this.lciSalesPerson, this.emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Right);

                // emptySpaceItem1
                this.emptySpaceItem1 = new EmptySpaceItem();
                this.emptySpaceItem1.Name = "emptySpaceItem" + Guid.NewGuid().ToString();
                this.emptySpaceItem1.Size = new System.Drawing.Size(1, 15);
                this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
                this.emptySpaceItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 15);
                this.emptySpaceItem1.MinSize = new System.Drawing.Size(0, 15);
                this.emptySpaceItem1.ShowInCustomizationForm = false;
                this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                //this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1);
                #endregion

                #region List of Schedules Available
                // lciListOfSchedule                                               
                this.lciListOfScheduleDropDown = new DevExpress.XtraLayout.LayoutControlItem();
                //lookUpEdit1
                this.lookUpEdit2 = new LookUpEdit();
                this.lookUpEdit2.Name = "lookUpEdit" + Guid.NewGuid().ToString();
                this.lookUpEdit2.Properties.NullText = "";
                this.lookUpEdit2.Properties.DisplayMember = "title";
                this.lookUpEdit2.Properties.ValueMember = "id";
                this.lookUpEdit2.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("title", "List of Bookings"));
                comboBoxEdit1_SelectedIndexChanged(this.comboBoxEdit1, null);
                this.lookUpEdit2.Properties.ShowFooter = true;
                this.lookUpEdit2.Properties.ShowHeader = false;
                this.lookUpEdit2.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

                this.lookUpEdit2.Tag = new ScheduleData()
                {
                    Name = "ListOfBookingsAvailable",
                    PositionIndex = "IndexPosition" + x,
                    ControlContainer = lciListOfScheduleDropDown,
                    Required = answeroption.ListOfBookingsAvailableRequired,
                    HasValue = answeroption.ScheduleValue != null && !string.IsNullOrEmpty(answeroption.ScheduleValue.ScheduleId) ? true : false
                };
                this.lookUpEdit2.Size = new System.Drawing.Size(155, 20);
                this.lookUpEdit2.StyleController = this.StyleController;
                //load answer values
                if (answeroption.ScheduleType != null)
                {
                    if (answeroption.ScheduleValue != null)
                    {
                        if (!string.IsNullOrEmpty(answeroption.ScheduleValue.ScheduleId))
                        {
                            int schedid = int.Parse(answeroption.ScheduleValue.ScheduleId);

                            lookUpEdit2.EditValue = schedid;
                            //if (answeroption.ScheduleType.ScheduleTypeSelectedValue.ToLower() == "meeting") {
                            //    CreateMeeting.Enabled = true;
                            //}
                        }
                    }
                }
                //this.lookUpEdit2.EditValueChanged += new EventHandler(lookUpEdit2_EditValueChanged);
                this.lookUpEdit2.Click += new EventHandler(layoutControlGroupQuestion1_Click);

                this.comboBoxEdit1.EditValue = answeroption.ScheduleType.ScheduleTypeSelectedValue;

                // layoutControlItem1
                this.lciListOfScheduleDropDown.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciListOfScheduleDropDown.Control = this.lookUpEdit2;

                if (!string.IsNullOrEmpty(answeroption.ListOfBookingsAvailableLabel))
                {
                    this.lciListOfScheduleDropDown.Text = answeroption.ListOfBookingsAvailableLabel.Trim();
                }
                else
                {
                    this.lciListOfScheduleDropDown.Text = "List of Schedules Available:";
                }
                this.lciListOfScheduleDropDown.TextLocation = DevExpress.Utils.Locations.Top;
                this.lciListOfScheduleDropDown.ShowInCustomizationForm = false;
                this.lciListOfScheduleDropDown.Click += new EventHandler(layoutControlGroupQuestion1_Click);


                this.lciListOfScheduleDropDown.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.OnlyInCustomization;
                this.layoutControlGroupQuestion1.AddItem(this.lciListOfScheduleDropDown);
                // 
                // simpleButton1
                //                 
                this.PreviewBooking = new SimpleButton();
                //this.PreviewBooking.Name = "PreviewDetails_simpleButton" + Guid.NewGuid().ToString();
                this.PreviewBooking.Name = "PreviewBooking";
                this.PreviewBooking.Size = new System.Drawing.Size(130, 22);
                this.PreviewBooking.StyleController = this.StyleController;
                if (!string.IsNullOrEmpty(answeroption.ViewDetailSummaryButtonLabel))
                    this.PreviewBooking.Text = answeroption.ViewDetailSummaryButtonLabel;
                else
                    this.PreviewBooking.Text = "Preview Details";

                this.PreviewBooking.Click += new EventHandler(viewDetailSummary_Click);
                this.PreviewBooking.Enabled = false;

                // lciListOfSchedulePreviewButton
                this.lciListOfSchedulePreviewButton = new LayoutControlItem();
                this.lciListOfSchedulePreviewButton.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciListOfSchedulePreviewButton.Control = this.PreviewBooking;
                this.lciListOfSchedulePreviewButton.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciListOfSchedulePreviewButton.MaxSize = new System.Drawing.Size(100, 24);
                this.lciListOfSchedulePreviewButton.MinSize = new System.Drawing.Size(80, 24);
                this.lciListOfSchedulePreviewButton.Size = new System.Drawing.Size(80, 24);
                this.lciListOfSchedulePreviewButton.TextVisible = false;
                this.lciListOfSchedulePreviewButton.ShowInCustomizationForm = false;
                this.lciListOfSchedulePreviewButton.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciListOfSchedulePreviewButton);

                // 
                // simpleButton1
                //                 
                this.CreateMeeting = new SimpleButton();
                //this.CreateMeeting.Name = "CreateMeeting_simpleButton" + Guid.NewGuid().ToString();
                this.CreateMeeting.Name = "CreateMeeting";
                this.CreateMeeting.Size = new System.Drawing.Size(130, 22);
                this.CreateMeeting.StyleController = this.StyleController;
                if (!string.IsNullOrEmpty(answeroption.CreateMeetingButtonLabel))
                {
                    if (answeroption.CreateMeetingButtonLabel.Trim().ToLower() == "create meeting")
                    {
                        this.CreateMeeting.Text = "Create/Edit Meeting";
                    }
                    else
                    {
                        this.CreateMeeting.Text = answeroption.CreateMeetingButtonLabel;
                    }
                }
                else
                {
                    this.CreateMeeting.Text = "Create/Edit Meeting";
                }
                this.CreateMeeting.Click += new EventHandler(createMeeting_Click);
                this.CreateMeeting.Enabled = false;

                // layoutControlItem1
                this.lciListOfScheduleCreateEditButton = new LayoutControlItem();
                if (answeroption.ScheduleType != null && answeroption.ScheduleType.ScheduleTypeSelectedValue == "Meeting")
                {
                    //lookUpEdit2.Properties.ReadOnly = true;
                    this.CreateMeeting.Tag = "HasMeeting";
                }
                else
                    this.lciListOfScheduleCreateEditButton.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                //this.lciListOfScheduleCreateEditButton.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciListOfScheduleCreateEditButton.Name = "lciListOfScheduleCreateEditButton";
                this.lciListOfScheduleCreateEditButton.Control = this.CreateMeeting;
                this.lciListOfScheduleCreateEditButton.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciListOfScheduleCreateEditButton.MaxSize = new System.Drawing.Size(115, 24);
                this.lciListOfScheduleCreateEditButton.MinSize = new System.Drawing.Size(115, 24);
                this.lciListOfScheduleCreateEditButton.Size = new System.Drawing.Size(115, 24);
                this.lciListOfScheduleCreateEditButton.TextVisible = false;
                this.lciListOfScheduleCreateEditButton.ShowInCustomizationForm = false;
                this.lciListOfScheduleCreateEditButton.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciListOfScheduleCreateEditButton, this.lciListOfSchedulePreviewButton, DevExpress.XtraLayout.Utils.InsertType.Right);
                // 
                // simpleButton1
                //                 
                this.DeleteMeeting = new SimpleButton();
                //this.DeleteMeeting.Name = "DeleteMeeting_simpleButton" + Guid.NewGuid().ToString();
                this.DeleteMeeting.Name = "DeleteMeeting";
                this.DeleteMeeting.Size = new System.Drawing.Size(130, 22);
                this.DeleteMeeting.StyleController = this.StyleController;
                this.DeleteMeeting.Text = "Delete Meeting";
                this.DeleteMeeting.Click += new EventHandler(deleteMeeting_Click);
                this.DeleteMeeting.Enabled = false;

                // lciListOfScheduleDeleteButton
                this.lciListOfScheduleDeleteButton = new LayoutControlItem();
                if (answeroption.ScheduleType != null && answeroption.ScheduleType.ScheduleTypeSelectedValue == "Meeting")
                {
                    this.DeleteMeeting.Tag = "HasMeeting";
                    //lookUpEdit2.Properties.ReadOnly = true;
                    var lueData = lookUpEdit2.Tag as ScheduleData;
                    lueData.IsMeeting = true;
                }
                else
                    this.lciListOfScheduleDeleteButton.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                this.lciListOfScheduleDeleteButton.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciListOfScheduleDeleteButton.Control = this.DeleteMeeting;
                this.lciListOfScheduleDeleteButton.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciListOfScheduleDeleteButton.MaxSize = new System.Drawing.Size(100, 24);
                this.lciListOfScheduleDeleteButton.MinSize = new System.Drawing.Size(80, 24);
                this.lciListOfScheduleDeleteButton.Size = new System.Drawing.Size(80, 24);
                this.lciListOfScheduleDeleteButton.TextVisible = false;
                this.lciListOfScheduleDeleteButton.ShowInCustomizationForm = false;
                this.lciListOfScheduleDeleteButton.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciListOfScheduleDeleteButton, this.lciListOfScheduleCreateEditButton, DevExpress.XtraLayout.Utils.InsertType.Right);

                ////load answer values
                //if (answeroption.ScheduleType != null) {                    
                //    if (answeroption.ScheduleValue != null) {
                //        if (!string.IsNullOrEmpty(answeroption.ScheduleValue.ScheduleId)) {
                //            int schedid = int.Parse(answeroption.ScheduleValue.ScheduleId);

                //            lookUpEdit2.EditValue = schedid;
                //            //if (answeroption.ScheduleType.ScheduleTypeSelectedValue.ToLower() == "meeting") {
                //            //    CreateMeeting.Enabled = true;
                //            //}
                //        }
                //    }
                //}

                // emptySpaceItem1
                this.emptySpaceItem1 = new EmptySpaceItem();
                this.emptySpaceItem1.Name = "emptySpaceItem" + Guid.NewGuid().ToString();
                this.emptySpaceItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.MinSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.Size = new System.Drawing.Size(200, 20);
                this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
                this.emptySpaceItem1.ShowInCustomizationForm = false;
                this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1, this.lciListOfScheduleDeleteButton, DevExpress.XtraLayout.Utils.InsertType.Right);
                #endregion

                #region Customer Attendies

                #region Grid
                simpleLabelItemValidation = new SimpleLabelItem();
                simpleLabelItemValidation.AppearanceItemCaption.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                simpleLabelItemValidation.Name = Guid.NewGuid().ToString();
                simpleLabelItemValidation.TextAlignMode = TextAlignModeItem.CustomSize;
                simpleLabelItemValidation.TextSize = new Size(212, 20);
                simpleLabelItemValidation.Text = "  Please add at least one contact attendie.";
                simpleLabelItemValidation.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                this.layoutControlGroupQuestion1.AddItem(simpleLabelItemValidation);

                //create our gridcontrol
                // 
                // gridColumn1
                // 
                this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn1.Caption = "Name";
                this.gridColumn1.FieldName = "Name";
                this.gridColumn1.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn1.Visible = true;
                this.gridColumn1.OptionsColumn.AllowEdit = false;
                this.gridColumn1.VisibleIndex = 0;
                this.gridColumn1.Width = 66;
                // 
                // gridColumn2
                // 
                this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn2.Caption = "Address";
                this.gridColumn2.FieldName = "Address";
                this.gridColumn2.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn2.Visible = true;
                this.gridColumn2.OptionsColumn.AllowEdit = false;
                this.gridColumn2.VisibleIndex = 1;
                this.gridColumn2.Width = 66;
                // 
                // gridColumn3
                // 
                //this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
                //this.gridColumn3.Caption = "City";
                //this.gridColumn3.FieldName = "City";
                //this.gridColumn3.Name = "gridColumn" + Guid.NewGuid().ToString();
                //this.gridColumn3.Visible = true;
                //this.gridColumn3.OptionsColumn.AllowEdit = false;
                //this.gridColumn3.VisibleIndex = 2;
                //this.gridColumn3.Width = 66;
                // 
                // gridColumn4
                // 
                this.gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn4.Caption = "Telephone";
                this.gridColumn4.FieldName = "Telephone";
                this.gridColumn4.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn4.Visible = true;
                this.gridColumn4.OptionsColumn.AllowEdit = false;
                this.gridColumn4.VisibleIndex = 3;
                this.gridColumn4.Width = 66;
                // 
                // gridColumn5
                // 
                this.gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn5.Caption = "Email";
                this.gridColumn5.FieldName = "Email";
                this.gridColumn5.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn5.Visible = true;
                this.gridColumn5.OptionsColumn.AllowEdit = false;
                this.gridColumn5.VisibleIndex = 4;
                this.gridColumn5.Width = 70;
                // 
                // gridColumn6
                // 
                this.gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn6.Caption = "AccountID";
                this.gridColumn6.FieldName = "AccountID";
                this.gridColumn6.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn6.Visible = false;
                // 
                // gridColumn7
                // 
                this.gridColumn7 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn7.Caption = "ContactID";
                this.gridColumn7.FieldName = "ContactID";
                this.gridColumn7.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn7.Visible = false;

                // 
                // gridView1
                // 
                this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
                this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
                this.gridColumn1,
                this.gridColumn2,
                //this.gridColumn3,
                this.gridColumn4,
                this.gridColumn5,
                this.gridColumn6,
                this.gridColumn7});
                this.gridView1.GridControl = this.gridControl1;
                this.gridView1.Name = "gridView" + Guid.NewGuid().ToString();
                this.gridView1.OptionsFind.AlwaysVisible = false;
                this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
                this.gridView1.OptionsSelection.MultiSelect = true;
                this.gridView1.OptionsView.ShowGroupPanel = false;
                this.gridView1.OptionsView.ColumnAutoWidth = true;
                this.gridView1.DataSourceChanged += new EventHandler(gridView1_DataSourceChanged);
                // 
                // gridControl1
                // 
                this.gridControl1 = new DevExpress.XtraGrid.GridControl();
                this.gridControl1.LookAndFeel.UseDefaultLookAndFeel = false;
                this.gridControl1.MainView = this.gridView1;
                this.gridControl1.Name = "gridControl" + Guid.NewGuid().ToString();
                this.gridControl1.Size = new System.Drawing.Size(150, 74);
                this.gridControl1.TabIndex = 11;
                this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {                    
                this.gridView1});
                SetCurrentAttendies();
                var schedData = new ScheduleData()
                {
                    Required = answeroption.AttendiesRequired
                };
                if (gridView1.GridControl.DataSource != null)
                {
                    List<ContactAttendie> gvDs = gridView1.GridControl.DataSource as List<ContactAttendie>;
                    if (gvDs != null && gvDs.Count > 0)
                        schedData.HasValue = true;
                }
                this.gridView1.Tag = schedData;
                // 
                // lciGridControl
                // 
                this.lciGridControl = new LayoutControlItem();
                this.lciGridControl.Control = this.gridControl1;
                this.lciGridControl.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                if (!string.IsNullOrEmpty(answeroption.AttendiesLabel))
                    this.lciGridControl.Text = answeroption.AttendiesLabel;
                else
                    this.lciGridControl.Text = "Attendies:";
                this.lciGridControl.TextLocation = DevExpress.Utils.Locations.Top;
                this.lciGridControl.TextSize = new System.Drawing.Size(108, 13);
                this.lciGridControl.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciGridControl.MaxSize = new System.Drawing.Size(0, 90);
                this.lciGridControl.MinSize = new System.Drawing.Size(0, 90);
                this.lciGridControl.Size = new System.Drawing.Size(200, 90);
                this.lciGridControl.ShowInCustomizationForm = false;
                this.layoutControlGroupQuestion1.AddItem(this.lciGridControl);
                #endregion

                // 
                // simpleButton1
                //                 
                this.simpleButton1 = new SimpleButton();
                this.simpleButton1.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.simpleButton1.Size = new System.Drawing.Size(130, 22);
                this.simpleButton1.StyleController = this.StyleController;
                this.simpleButton1.Enabled = false;
                if (!string.IsNullOrEmpty(answeroption.AddCallerButtonLabel))
                    this.simpleButton1.Text = answeroption.AddCallerButtonLabel;
                else
                    this.simpleButton1.Text = "Add Caller";
                this.simpleButton1.Click += new EventHandler(AddCaller_Click);

                // lciAddCaller
                this.lciAddCaller = new LayoutControlItem();
                this.lciAddCaller.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciAddCaller.Control = this.simpleButton1;
                this.lciAddCaller.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciAddCaller.MaxSize = new System.Drawing.Size(80, 24);
                this.lciAddCaller.MinSize = new System.Drawing.Size(80, 24);
                this.lciAddCaller.Size = new System.Drawing.Size(80, 24);
                this.lciAddCaller.TextVisible = false;
                this.lciAddCaller.ShowInCustomizationForm = false;
                this.lciAddCaller.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciAddCaller);

                // 
                // simpleButton2
                //                 
                this.simpleButton2 = new SimpleButton();
                this.simpleButton2.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.simpleButton2.Size = new System.Drawing.Size(130, 22);
                this.simpleButton2.StyleController = this.StyleController;
                this.simpleButton2.Enabled = false;
                if (!string.IsNullOrEmpty(answeroption.AddAdditionalAttendieButtonLabel))
                    this.simpleButton2.Text = answeroption.AddAdditionalAttendieButtonLabel;
                else
                    this.simpleButton2.Text = "Add Additional";
                this.simpleButton2.Click += new EventHandler(AddAdditional_Click);

                // layoutControlItem1
                this.lciAddAdditional = new LayoutControlItem();
                this.lciAddAdditional.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciAddAdditional.Control = this.simpleButton2;
                this.lciAddAdditional.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciAddAdditional.MaxSize = new System.Drawing.Size(100, 24);
                this.lciAddAdditional.MinSize = new System.Drawing.Size(80, 24);
                this.lciAddAdditional.Size = new System.Drawing.Size(80, 24);
                this.lciAddAdditional.TextVisible = false;
                this.lciAddAdditional.ShowInCustomizationForm = false;
                this.lciAddAdditional.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciAddAdditional, this.lciAddCaller, DevExpress.XtraLayout.Utils.InsertType.Right);

                // 
                // simpleButton3
                //                 
                this.simpleButton3 = new SimpleButton();
                this.simpleButton3.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.simpleButton3.Size = new System.Drawing.Size(130, 22);
                this.simpleButton3.StyleController = this.StyleController;
                this.simpleButton3.Enabled = false;
                if (!string.IsNullOrEmpty(answeroption.DeleteAttendieButtonLabel))
                    this.simpleButton3.Text = answeroption.DeleteAttendieButtonLabel;
                else
                    this.simpleButton3.Text = "Delete";
                this.simpleButton3.Click += new EventHandler(DeleteAttendie_Click);

                // layoutControlItem2
                this.lciDeleteCaller = new LayoutControlItem();
                this.lciDeleteCaller.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.lciDeleteCaller.Control = this.simpleButton3;
                this.lciDeleteCaller.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.lciDeleteCaller.MaxSize = new System.Drawing.Size(70, 24);
                this.lciDeleteCaller.MinSize = new System.Drawing.Size(70, 24);
                this.lciDeleteCaller.Size = new System.Drawing.Size(70, 24);
                this.lciDeleteCaller.TextVisible = false;
                this.lciDeleteCaller.ShowInCustomizationForm = false;
                this.lciDeleteCaller.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.lciDeleteCaller, this.lciAddAdditional, DevExpress.XtraLayout.Utils.InsertType.Right);

                // emptySpaceItem1
                this.emptySpaceItem1 = new EmptySpaceItem();
                this.emptySpaceItem1.Name = "emptySpaceItem" + Guid.NewGuid().ToString();
                this.emptySpaceItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.MinSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.Size = new System.Drawing.Size(200, 20);
                this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
                this.emptySpaceItem1.ShowInCustomizationForm = false;
                this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1, this.lciDeleteCaller, DevExpress.XtraLayout.Utils.InsertType.Right);

                #endregion

                #region Other Choices
                idx = 0;
                //EmptySpaceItem esitem = new EmptySpaceItem();
                //esitem.Size = new Size(100, 20);
                //this.layoutControlGroupQuestion1.AddItem(esitem);
                foreach (OtherChoice oChoice in answeroption.OtherChoices)
                {
                    if (oChoice.Enabled)
                    {
                        //if (!string.IsNullOrEmpty(oChoice.TextPrefix)) {
                        //    // simpleLabelItem1                
                        //    this.simpleLabelItem1 = new DevExpress.XtraLayout.SimpleLabelItem();

                        //    this.simpleLabelItem1.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
                        //    //this.simpleLabelItem1.ShowInCustomizationForm = false;
                        //    //this.simpleLabelItem1.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                        //    this.simpleLabelItem1.Text = oChoice.TextPrefix;
                        //    //newSize = new System.Drawing.Size(100, 20);
                        //    newSize = new System.Drawing.Size(20, 20);
                        //    this.simpleLabelItem1.SizeConstraintsType = SizeConstraintsType.Custom;
                        //    this.simpleLabelItem1.MaxSize = newSize;
                        //    this.simpleLabelItem1.MinSize = newSize;
                        //    this.simpleLabelItem1.Size = newSize;
                        //    this.simpleLabelItem1.AppearanceItemCaption.BackColor = System.Drawing.Color.Transparent;
                        //    this.simpleLabelItem1.ShowInCustomizationForm = false;
                        //    this.simpleLabelItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                        //    this.layoutControlGroupQuestion1.AddItem(this.simpleLabelItem1);
                        //} else {
                        //    this.simpleLabelItem1 = null;
                        //}
                        // layoutControlItem1                                                         
                        this.lciOtherChoice = new LayoutControlItem();
                        //add textEdit1 to layout
                        this.memoEdit1 = new MemoEdit();
                        this.memoEdit1.Tag = new ScheduleData()
                        {
                            ParentPositionIndex = "IndexPosition" + x.ToString(),
                            PositionIndex = "IndexPosition" + idx.ToString(),
                            ControlContainer = lciOtherChoice,
                            Required = oChoice.Required,
                            HasValue = !string.IsNullOrWhiteSpace(oChoice.DefaultInputValue) ? true : false,
                            ChoiceOption = oChoice
                        };
                        this.memoEdit1.Name = "textEdit" + Guid.NewGuid().ToString();
                        this.memoEdit1.Properties.ScrollBars = System.Windows.Forms.ScrollBars.None;
                        this.memoEdit1.StyleController = this.StyleController;
                        if (oChoice.DefaultInputValue != null)
                        {
                            this.memoEdit1.Text = oChoice.DefaultInputValue.Trim();
                            oChoice.InputValue = oChoice.DefaultInputValue.Trim();
                        }
                        memoEdit1.TextChanged += new EventHandler(memoEdit1_TextChanged);
                        memoEdit1.Resize += new EventHandler(memoEdit1_Resize);
                        memoEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);

                        this.lciOtherChoice.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                        this.lciOtherChoice.Control = this.memoEdit1;

                        if (!string.IsNullOrEmpty(oChoice.TextPrefix))
                        {
                            this.lciOtherChoice.Text = oChoice.TextPrefix;
                            this.lciOtherChoice.TextVisible = true;
                            this.lciOtherChoice.TextLocation = DevExpress.Utils.Locations.Top;
                            this.lciOtherChoice.MaxSize = new Size(0, 41);
                            this.lciOtherChoice.MinSize = new Size(0, 41);
                        }
                        else
                        {
                            this.lciOtherChoice.TextVisible = false;
                            this.lciOtherChoice.MaxSize = new Size(0, 24);
                            this.lciOtherChoice.MinSize = new Size(0, 24);
                        }
                        this.lciOtherChoice.SizeConstraintsType = SizeConstraintsType.Custom;
                        this.lciOtherChoice.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                        this.lciOtherChoice.ShowInCustomizationForm = false;
                        this.layoutControlGroupQuestion1.AddItem(this.lciOtherChoice);
                        idx++;
                    }
                }
                #endregion

            }

            #region Footer
            //string prioText = oSettings.Priority;
            //if (oSettings.CustomerOwnership && oSettings.BVOwnership) {
            //    prioText += "(Cust+BV)";
            //} else if (oSettings.CustomerOwnership) {
            //    prioText += "(Cust)";
            //} else if (oSettings.BVOwnership) {
            //    prioText += "(BV)";
            //}
            //if (oSettings.PlotDoneStatus.Trim().ToLower() == "done") {
            //    prioText += " Done";
            //}

            //// simpleLabelItem1 status
            //this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            //this.emptySpaceItem1.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
            //this.emptySpaceItem1.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            //this.emptySpaceItem1.AppearanceItemCaption.Options.UseTextOptions = true;
            //this.emptySpaceItem1.ShowInCustomizationForm = false;
            //this.emptySpaceItem1.SizeConstraintsType = SizeConstraintsType.Custom;
            //this.emptySpaceItem1.Text = prioText;
            //this.emptySpaceItem1.TextVisible = true;
            //this.emptySpaceItem1.ShowInCustomizationForm = false;
            //this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            //this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1);

            //if (!string.IsNullOrEmpty(oSettings.QuestionHelp)) {
            //    this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            //    this.emptySpaceItem2.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
            //    this.emptySpaceItem2.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            //    this.emptySpaceItem2.AppearanceItemCaption.Options.UseTextOptions = true;
            //    this.emptySpaceItem2.ShowInCustomizationForm = false;
            //    this.emptySpaceItem2.Text = "Help";
            //    this.emptySpaceItem2.OptionsToolTip.ToolTip = oSettings.QuestionHelp.Trim();

            //    //apply tooltip controller from parent
            //    //if (this.ToolTipController != null && this.ToolTipController.DefaultController != null) {
            //    //    this.emptySpaceItem2.OptionsToolTip.ToolTipController = ToolTipController.DefaultController;
            //    //}

            //    this.emptySpaceItem2.TextVisible = true;
            //    this.emptySpaceItem2.Size = new Size(50, 20);
            //    this.emptySpaceItem2.MaxSize = new Size(50, 20);
            //    this.emptySpaceItem2.MinSize = new Size(50, 20);
            //    this.emptySpaceItem2.SizeConstraintsType = SizeConstraintsType.Custom;
            //    this.emptySpaceItem2.ShowInCustomizationForm = false;
            //    this.emptySpaceItem2.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            //    this.emptySpaceItem2.AppearanceItemCaption.TextOptions.HAlignment = HorzAlignment.Far;
            //    this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem2, this.emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Right);
            //}
            #endregion

            this.layoutControlGroupQuestion1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            this.layoutControlGroupQuestion1.EndUpdate();
            Image bg = BGImage(this);
            if (bg != null)
            {
                this.layoutControlGroupQuestion1.BackgroundImage = bg;
                this.layoutControlGroupQuestion1.BackgroundImageVisible = true;
            }
            if (this.memoEdit1 != null)
                BackColor = this.memoEdit1.BackColor;

            //lookUpEdit2.EditValue = null;
            //lookUpEdit2.ItemIndex = 0;

            this.ControlGroup = this.layoutControlGroupQuestion1;
            this.ControlGroup.Tag = this;
            isLoaded = true;
        }
        public bool Validate() {
            bool isValid = true;
            bool isRequiredAll = false;
            try {
                var Item = layoutControlGroupQuestion1;
                if (Item == null) return false;
                var FlatList = new DevExpress.XtraLayout.Helpers.FlatItemsList();
                List<BaseLayoutItem> Items = FlatList.GetItemsList(Item);
                ILayoutControl Layout = Item.Owner;
                if (Layout != null) {
                    Layout.BeginUpdate();
                    BaseLayoutItem li;
                    ScheduleData data;
                    for (int i = Items.Count - 1; i >= 0; --i) {
                        li = Items[i];
                        if (!li.Equals(Item) && li.Name != Item.Name) {
                            if (li is LayoutControlItem) {
                                Control TempControl = (li as LayoutControlItem).Control;
                                if (TempControl != null) {
                                    data = TempControl.Tag as ScheduleData;
                                    if (data != null) {
                                        if (data.Required) { if (!isRequiredAll) isRequiredAll = true; }
                                        if (data.Required && !data.HasValue) {
                                            TempControl.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                                            if (isValid) isValid = false;
                                        } else {
                                            TempControl.BackColor = this.BackColor;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    var opt = Questionnaire.Form.Settings.AnswerOptions[0];
                    if (opt != null) {
                        if (opt.AttendiesRequired)  {
                           if (!isRequiredAll) isRequiredAll = true;
                        }
                        if (opt.AttendiesRequired && opt.Attendies != null) {
                            var vals = opt.Attendies;
                            if(vals.Count <= 0) {
                                //prompt message to require to add at least one attendie
                                if (simpleLabelItemValidation != null) {
                                    simpleLabelItemValidation.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                                    if(isValid) isValid = false;                                    
                                }
                            } else {
                                if (simpleLabelItemValidation != null) {
                                    simpleLabelItemValidation.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                                }
                            }
                        }
                    }
                    Layout.EndUpdate();
                }

            } catch {
            }

            if (ctlFooter != null) {
                if (!isValid && isRequiredAll) {
                    ctlFooter.SetQuestionRequired(false, true);
                } else if (isValid && isRequiredAll) {
                    ctlFooter.SetQuestionRequired(true, true);
                } else if (!isRequiredAll) {
                    ctlFooter.SetQuestionRequired(true, false);
                }
            }

            return isValid;
        }

        public void SetCurrentCaller(CTScSubCampaignContactList ca, int account_id) 
        {
            m_Caller = new ContactAttendie() {
                AccountID = account_id,
                ContactID = ca.id,
                Name = ca.first_name + (ca.last_name.Length > 0 ? " " + ca.last_name : ""),
                Address = ca.complete_address,
                City = "",
                Email = ca.email,
                Telephone = ca.direct_phone,
                Attending = true
            };
        }

        public void SetCreatedMeetingSchedule(int schedule_id) {
            var listSched = lookUpEdit2.Properties.DataSource as List<CTBookingSchedule>;
            CTBookingSchedule sched =null;
            if (listSched != null && listSched.Count > 0) {
                sched = listSched.FirstOrDefault(x => x.id == schedule_id);
            }
            
            //BindBookingSchedules(3, schedule_id);
            //lookUpEdit2.EditValue = schedule_id;

            if (sched == null) {
                BindBookingSchedules(3, schedule_id);
                lookUpEdit2.EditValue = schedule_id;
            }
            else {
                lookUpEdit2.EditValue = schedule_id;
            }
            PreviewBooking.Enabled = true;
            DeleteMeeting.Enabled = true;
        }

        public void RemoveCurrentMeetingSchedule() {            
            BindBookingSchedules(3);
            lookUpEdit2.EditValue = null;            
            PreviewBooking.Enabled = false;
            DeleteMeeting.Enabled = false;
        }

        public int CurrentSelectedMeeting {
            get {
                if(lookUpEdit2.EditValue != null) {
                    int val = int.Parse(lookUpEdit2.EditValue.ToString());
                    return val;
                }
                return 0;
            }
        }

        public void RefreshContactAttendies() {
            SetCurrentAttendies();
        }
                
        public List<ContactAttendie> ContactAttendies {
            get;
            set;
        }

        public void Render()
        {
            if (Questionnaire == null || Questionnaire.Form.Settings == null) {
                BrightVision.Common.UI.NotificationDialog.Information("Scheduling / Booking", "Questionnaire must be initialized with Json first.");
                return;
            }
            
            //var answerOpt = Questionnaire.Form.Settings.AnswerOptions[0] as ISchedule;
            //caller = new ContactAttendie() {
            //    AccountID = account_id,
            //    ContactID = ca.id,
            //    Name = ca.first_name + (ca.last_name.Length > 0 ? " " + ca.last_name : ""),
            //    Address = ca.complete_address,
            //    City = "",
            //    Email = ca.email,
            //    Telephone = ca.direct_phone,
            //    Attending = true
            //};

            //if (CalendarDataSource == null) {
            //    if (answerOpt != null) {
            //        if (answerOpt.CalendarOption != null && answerOpt.CalendarOption.CalendarValues.Count > 0)
            //            CalendarDataSource = answerOpt.CalendarOption.CalendarValues;
            //        else {
            //            MessageBox.Show("\"CalendarDataSource\" property must be set first.","Schedule Component");
            //            return;
            //        }
            //    } else {
            //        MessageBox.Show("\"CalendarDataSource\" property must be set first.", "Schedule Component");
            //        return;
            //    }
            //}
            
            //#endregion

            isLoaded = false;
            IList<AnswerOption> _lstAnswerOptions = Questionnaire.Form.Settings.AnswerOptions;
            ISchedule _AnswerData = null;

            /**
             * set caller.
             */
            m_Caller = null;
            if (ContactPerson != null) {
                m_Caller = new ContactAttendie() {
                    AccountID = AccountId,
                    ContactID = ContactPerson.id,
                    Name = ContactPerson.first_name + (ContactPerson.last_name.Length > 0 ? " " + ContactPerson.last_name : ""),
                    Address = ContactPerson.complete_address,
                    City = "",
                    Email = ContactPerson.email,
                    Telephone = ContactPerson.direct_phone,
                    Attending = true
                };
            }

            for (int i = 0; i < _lstAnswerOptions.Count; i++) {
                _AnswerData = _lstAnswerOptions[i] as ISchedule;

                /*
                 * https://brightvision.jira.com/browse/PLATFORM-3015
                 */
                if (_AnswerData.ScheduleType != null && _AnswerData.ScheduleValue != null)
                {
                    if (!string.IsNullOrEmpty(_AnswerData.ScheduleValue.ScheduleId) && this.lblScheduleDetails != null)
                    {
                        this.lblScheduleDetails.Text = _AnswerData.ScheduleValue.Description;
                        if (_AnswerData.ScheduleSalesPerson.SalesPersonSelectedValue.Name != null && _AnswerData.ScheduleSalesPerson.SalesPersonSelectedValue.Name != "")
                            this.lblScheduleDetails.Text += " | " + _AnswerData.ScheduleSalesPerson.SalesPersonSelectedValue.Name;
                    }
                    if (string.IsNullOrEmpty(_AnswerData.ScheduleValue.ScheduleId) && this.lblScheduleDetails != null)
                    {
                        this.lblScheduleDetails.Text = "";
                        this.DeleteMeeting.Enabled = false;
                        this.PreviewBooking.Enabled = false;
                    }
                }

                /**
                 * sales person.
                 */
                if (_AnswerData.ScheduleSalesPerson == null || _AnswerData.ScheduleSalesPerson.SalesPersonSelectedValue == null)
                    this.lookUpEdit1.EditValue = null;
                else
                    this.lookUpEdit1.EditValue = _AnswerData.ScheduleSalesPerson.SalesPersonSelectedValue.Id;

                /**
                 * list of bookings selected schedule id.
                 */
                if (_AnswerData.ScheduleType != null && _AnswerData.ScheduleValue != null) {
                    if (!string.IsNullOrEmpty(_AnswerData.ScheduleValue.ScheduleId))
                        lookUpEdit2.EditValue = int.Parse(_AnswerData.ScheduleValue.ScheduleId);
                    else
                        lookUpEdit2.EditValue = null;
                }

                /**
                 * list of bookings label value.
                 */
                this.lciListOfScheduleDropDown.Text = "List of Available Schedules";
                if (!string.IsNullOrEmpty(_AnswerData.ListOfBookingsAvailableLabel))
                    this.lciListOfScheduleDropDown.Text = _AnswerData.ListOfBookingsAvailableLabel.Trim();

                /**
                 * preview booking text value.
                 */
                this.PreviewBooking.Text = "Preview Details";
                if (!string.IsNullOrEmpty(_AnswerData.ViewDetailSummaryButtonLabel))
                    this.PreviewBooking.Text = _AnswerData.ViewDetailSummaryButtonLabel;
                    
                /**
                 * create button text value.
                 */
                this.CreateMeeting.Text = "Create/Edit Meeting";
                if (!string.IsNullOrEmpty(_AnswerData.CreateMeetingButtonLabel))
                    if (_AnswerData.CreateMeetingButtonLabel.Trim().ToLower() != "create meeting")
                        this.CreateMeeting.Text = _AnswerData.CreateMeetingButtonLabel;

                if (_AnswerData.ScheduleType != null && _AnswerData.ScheduleType.ScheduleTypeSelectedValue == "Meeting") {
                    this.lciListOfScheduleCreateEditButton.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                    this.CreateMeeting.Tag = "HasMeeting";
                }
                else
                    this.lciListOfScheduleCreateEditButton.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;

                /**
                 * delete button text value.
                 */
                if (_AnswerData.ScheduleType != null && _AnswerData.ScheduleType.ScheduleTypeSelectedValue == "Meeting") {
                    this.DeleteMeeting.Tag = "HasMeeting";
                    this.lciListOfScheduleDeleteButton.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                }
                else
                    this.lciListOfScheduleDeleteButton.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;

                /**
                 * set attendies grid.
                 */
                this.SetCurrentAttendies();
                ScheduleData _ScheduleData = new ScheduleData() {
                    Required = _AnswerData.AttendiesRequired
                };
                if (gridView1.GridControl.DataSource != null) {
                    List<ContactAttendie> _lstAttendies = gridView1.GridControl.DataSource as List<ContactAttendie>;
                    if (_lstAttendies != null && _lstAttendies.Count > 0)
                        _ScheduleData.HasValue = true;
                }
                this.gridView1.Tag = _ScheduleData;

                /**
                 * set labels.
                 */
                this.lciGridControl.Text = "Attendies";
                if (!string.IsNullOrEmpty(_AnswerData.AttendiesLabel))
                    this.lciGridControl.Text = _AnswerData.AttendiesLabel;

                this.simpleButton1.Text = "Add Caller";
                if (!string.IsNullOrEmpty(_AnswerData.AddCallerButtonLabel))
                    this.simpleButton1.Text = _AnswerData.AddCallerButtonLabel;

                this.simpleButton2.Text = "Add Additional";
                if (!string.IsNullOrEmpty(_AnswerData.AddAdditionalAttendieButtonLabel))
                    this.simpleButton2.Text = _AnswerData.AddAdditionalAttendieButtonLabel;

                this.simpleButton3.Text = "Delete";
                if (!string.IsNullOrEmpty(_AnswerData.DeleteAttendieButtonLabel))
                    this.simpleButton3.Text = _AnswerData.DeleteAttendieButtonLabel;

                /**
                 * set other choices.
                 */
                foreach (OtherChoice _OtherChoice in _AnswerData.OtherChoices) {
                    if (_OtherChoice.Enabled) {
                        if (_OtherChoice.DefaultInputValue != null) {
                            this.memoEdit1.Text = _OtherChoice.DefaultInputValue.Trim();
                            _OtherChoice.InputValue = _OtherChoice.DefaultInputValue.Trim();
                        }
                    }
                }
            }

            if (this.memoEdit1 != null)
                BackColor = this.memoEdit1.BackColor;

            isLoaded = true;
            //this.SetEditableGroupControls(this.layoutControlGroupQuestion1, false);
        }
        
        #endregion

        #region Private Methods
        private Color BackColor { get; set; }

        private Image BGImage(IQuestionnaire iQuestion) {            
            Image imgBGImage = null;
            if (iQuestion.Questionnaire != null && iQuestion.Questionnaire.Form.Settings != null) {
                Settings qSettings = iQuestion.Questionnaire.Form.Settings;
                string bgColor = qSettings.BackgroundColor.ToLower().Trim();
                if (!string.IsNullOrEmpty(bgColor)) {                        
                    switch (bgColor) {
                        case BackgroundConstants.Cyan:
                            imgBGImage = Properties.Resources.bg_cyan;
                            break;
                        case BackgroundConstants.Pink:
                            imgBGImage = Properties.Resources.bg_pink;
                            break;
                        case BackgroundConstants.Yellow:
                            imgBGImage = Properties.Resources.bg_yellow;
                            break;                            
                    }                        
                }
            }                
            return imgBGImage;
        }

        private void SetEditableGroupControls(BaseLayoutItem Item, bool isEditable) {
            if (Item == null) return;
            var FlatList = new DevExpress.XtraLayout.Helpers.FlatItemsList();
            List<BaseLayoutItem> Items = FlatList.GetItemsList(Item);
            ILayoutControl Layout = Item.Owner;
            if (Layout != null) {
                Layout.BeginUpdate();
                BaseLayoutItem li;
                for (int i = Items.Count - 1; i >= 0; --i) {
                    li = Items[i];
                    if (!li.Equals(Item) && li.Name != Item.Name) {
                        if (li is LayoutControlItem) {
                            Control TempControl = (li as LayoutControlItem).Control;
                            if (TempControl is SimpleButton) {
                                if (TempControl.Name.Equals("PreviewBooking") || 
                                    TempControl.Name.Equals("DeleteMeeting") ||
                                    TempControl.Text.Equals("Add Caller") ||
                                    TempControl.Text.Equals("Add Additional") ||
                                    TempControl.Text.Equals("Delete"))
                                {
                                    if (this.lblScheduleDetails != null && this.lblScheduleDetails.Text != "" && isEditable)
                                        TempControl.Enabled = true;
                                    else
                                        TempControl.Enabled = false;
                                    continue;
                                }

                                if (TempControl.Enabled != isEditable)
                                    TempControl.Enabled = isEditable;

                                //if (TempControl.Name.IndexOf("PreviewDetails_simpleButton") != -1 ||
                                //    TempControl.Name.IndexOf("CreateMeeting_simpleButton") != -1 ||
                                //    TempControl.Name.IndexOf("DeleteMeeting_simpleButton") != -1) {
                                //        if (TempControl.Tag != null && TempControl.Tag.ToString() == "HasMeeting")
                                //        {
                                //            if (TempControl.Name.IndexOf("DeleteMeeting_simpleButton") != -1)
                                //            {
                                //                if (lookUpEdit2.EditValue != null)
                                //                    TempControl.Enabled = isEditable;
                                //                else
                                //                    TempControl.Enabled = false;
                                //            }
                                //            else
                                //            {
                                //                TempControl.Enabled = isEditable;
                                //            }
                                //        }
                                //        else if(TempControl.Name.IndexOf("PreviewDetails_simpleButton") != -1) {
                                //            TempControl.Enabled = isEditable;
                                //        }
                                //    //Do Nothing
                                //} else {
                                //    TempControl.Enabled = isEditable;
                                //}
                            } 
                            else if (TempControl is BaseEdit) {
                                //
                                var tagData = TempControl.Tag as ScheduleData;                                
                                if (TempControl.Name.IndexOf("SalesPerson_lookUpEdit") != -1 ||
                                    TempControl.Name.IndexOf("ScheduleType_comboBoxEdit") != -1 ||
                                    (tagData != null && tagData.IsMeeting)) {
                                        if (tagData.Name.Equals("ListOfBookingsAvailable"))
                                        {
                                            if (m_HasSchedules) {
                                                (TempControl as BaseEdit).Properties.ReadOnly = false;
                                                (TempControl as BaseEdit).Properties.AllowFocused = true;
                                            }
                                            else {
                                                (TempControl as BaseEdit).Properties.ReadOnly = true;
                                                (TempControl as BaseEdit).Properties.AllowFocused = false;
                                            }
                                        }
                                        else {
                                            (TempControl as BaseEdit).Properties.ReadOnly = true;
                                            (TempControl as BaseEdit).Properties.AllowFocused = false;
                                        }

                                    //(TempControl as BaseEdit).Properties.ReadOnly = true;
                                    //(TempControl as BaseEdit).Properties.AllowFocused = false;
                                }
                                else if (TempControl.Tag.Equals("ListOfBookingsAvailable")) {
                                    if (m_HasSchedules) {
                                        (TempControl as BaseEdit).Properties.ReadOnly = false;
                                        (TempControl as BaseEdit).Properties.AllowFocused = true;
                                    }
                                    else {
                                        (TempControl as BaseEdit).Properties.ReadOnly = true;
                                        (TempControl as BaseEdit).Properties.AllowFocused = false;
                                    }
                                }
                                else {
                                    (TempControl as BaseEdit).Properties.ReadOnly = !isEditable;
                                    (TempControl as BaseEdit).Properties.AllowFocused = isEditable;
                                }
                            }
                        }
                    }
                }
                Layout.EndUpdate();
            }

            //this.CreateMeeting.Enabled = isEditable;
            //this.DeleteMeeting.Enabled = isEditable;
            //this.simpleButton1.Enabled = isEditable;
        }

        //Schedule Type
        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e) {            
            var cbox = sender as ComboBoxEdit;
           if (cbox != null && cbox.EditValue != null) {
                string scheduletype = cbox.EditValue.ToString().ToLower();
                if (scheduletype == "seminar") {
                    BindBookingSchedules(1);
                     if (CreateMeeting != null) 
                        CreateMeeting.Enabled = false;
                } else if (scheduletype == "webinar") {
                    BindBookingSchedules(2);
                    if (CreateMeeting != null)
                        CreateMeeting.Enabled = false;
                } else if (scheduletype == "meeting") {
                    BindBookingSchedules(3);
                    if (CreateMeeting != null)
                        CreateMeeting.Enabled = true;
                    if (DeleteMeeting != null)
                        DeleteMeeting.Enabled = true;
                } else {
                    if (lookUpEdit1 != null && lookUpEdit2 != null) {
                        lookUpEdit2.Properties.DataSource = null;
                        lookUpEdit1.Properties.DataSource = null;
                    }
                    if (CreateMeeting != null)
                        CreateMeeting.Enabled = false;
                    if (DeleteMeeting != null)
                        DeleteMeeting.Enabled = false;
                }
                if (PreviewBooking != null) {
                    var listSched = lookUpEdit2.Properties.DataSource as List<CTBookingSchedule>;
                    if (listSched == null || listSched.Count <= 0) {
                        PreviewBooking.Enabled = false;                        
                    }
                    if (lookUpEdit2.EditValue != null &&
                        listSched != null &&
                        listSched.Count > 0) {
                        lookUpEdit2_EditValueChanged(lookUpEdit2, null);
                        PreviewBooking.Enabled = true;                        
                    }
                }
                var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
                if (cbox.Tag != null) {
                    var data = (ScheduleData)cbox.Tag;
                    var tag = data.PositionIndex.ToString();
                    tag = tag.Replace("IndexPosition", "");
                    var oSubQ = listSubQ[int.Parse(tag)];
                    oSubQ.ScheduleType.ScheduleTypeSelectedValue = cbox.EditValue.ToString();

                    if(!string.IsNullOrEmpty(oSubQ.ScheduleType.ScheduleTypeSelectedValue) &&
                        !string.IsNullOrEmpty(oSubQ.ScheduleType.ScheduleTypeSelectedValue.Trim()))
                        data.HasValue = true;
                    else 
                        data.HasValue = false;
                }
            }else {
                BindBookingSchedules(3);
                 if (PreviewBooking != null) 
                     PreviewBooking.Enabled = false;
                 if (DeleteMeeting != null)
                     DeleteMeeting.Enabled = false;
                 if (CreateMeeting != null)
                        CreateMeeting.Enabled  = false;
            }

           if (!IsInitializing) {
               if (isLoaded)
                   m_bHasChanged = true;

               if (OnComponentNotifyChanged != null)
                   OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(isLoaded));
           }

            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }
        //Sales Person
        private void lookUpEdit1_SelectedIndexChanged(object sender, EventArgs e) {
            var objSender = sender as LookUpEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            if (objSender.Tag != null) {
                var data =(ScheduleData)objSender.Tag;
                var tag = data.PositionIndex.ToString();
                tag = tag.Replace("IndexPosition", "");
                var oSubQ = listSubQ[int.Parse(tag)];
                var person = objSender.GetSelectedDataRow() as SalesPerson;
                if (oSubQ.ScheduleSalesPerson == null)
                    oSubQ.ScheduleSalesPerson = new ScheduleSalesPerson();
                oSubQ.ScheduleSalesPerson.SalesPersonSelectedValue = person;
                data.HasValue = person != null;
            }

            if (!IsInitializing) {
                if (isLoaded)
                    m_bHasChanged = true;

                if (OnComponentNotifyChanged != null)
                    OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(isLoaded));
            }

            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }
        private void lookUpEdit1SelectedIndexChanged(SalesPerson person)
        {
            var objSender = lookUpEdit1;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            if (objSender.Tag != null)
            {
                var data = (ScheduleData)objSender.Tag;
                var tag = data.PositionIndex.ToString();
                tag = tag.Replace("IndexPosition", "");
                var oSubQ = listSubQ[int.Parse(tag)];
                //var person = objSender.GetSelectedDataRow() as SalesPerson;
                if (oSubQ.ScheduleSalesPerson == null)
                    oSubQ.ScheduleSalesPerson = new ScheduleSalesPerson();
                oSubQ.ScheduleSalesPerson.SalesPersonSelectedValue = person;
                data.HasValue = person != null;
            }

            if (!IsInitializing)
            {
                if (isLoaded)
                    m_bHasChanged = true;

                if (OnComponentNotifyChanged != null)
                    OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(isLoaded));
            }

            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }
        //List of available bookings
        private void lookUpEdit2_EditValueChanged(object sender, EventArgs e) 
        {
            this.CreateMeeting.Enabled = true;
            var obj = sender as LookUpEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            if (obj != null) {
                if (obj.EditValue != null && !string.IsNullOrEmpty(obj.EditValue.ToString())) {
                    PreviewBooking.Enabled = true;
                    //DeleteMeeting.Enabled = true;
                    var row = obj.GetSelectedDataRow() as CTBookingSchedule;
                    if (row != null) {
                        SalesPerson person = new SalesPerson() {
                            Id = row.resource_id,
                            Name = row.resource_name
                        };
                        //lookUpEdit1.Properties.DataSource = new List<SalesPerson> { person };
                        lookUpEdit1.Properties.DataSource = listBookingSalesPerson;
                        lookUpEdit1.EditValue = person.Id;
                        //listBookingSchedules
                        lookUpEdit1_SelectedIndexChanged(lookUpEdit1, null);

                        if (obj.Tag != null) {
                            var data = (ScheduleData)obj.Tag;
                            var tag = data.PositionIndex.ToString();
                            tag = tag.Replace("IndexPosition", "");
                            var oSubQ = listSubQ[int.Parse(tag)];                            
                            oSubQ.ScheduleValue = new ScheduleValue() {
                                ScheduleId = row.id.ToString(),
                                Description = row.title
                            };
                            Questionnaire.Form.Settings.DataBindings.schedule_id = row.id.ToString();
                            data.HasValue = row.id > 0;

                            /**
                             * if not from the same company,
                             * dont allow edit.
                             */
                            int _id = Convert.ToInt32(obj.EditValue);
                            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                                answer _data = _efDbContext.answers.FirstOrDefault(i => i.schedule_id == _id);
                                if (_data != null) {
                                    _efDbContext.Detach(_data);
                                    if (_data.accounts_id != AccountId)
                                        this.CreateMeeting.Enabled = false;
                                }
                                else
                                    this.CreateMeeting.Enabled = false;
                            }
                        }
                    }
                } else {
                    PreviewBooking.Enabled = false;
                    //DeleteMeeting.Enabled = false;
                }
                
            }

            if (!IsInitializing) {
                if (isLoaded)
                    m_bHasChanged = true;

                if (OnComponentNotifyChanged != null)
                    OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(isLoaded));
            }

            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }
        private void lookUpEdit2EditValueChanged(CTBookingSchedule obj)
        {
            this.CreateMeeting.Enabled = true;
            //var obj = sender as LookUpEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            if (obj != null)
            {
                    PreviewBooking.Enabled = true;
                    DeleteMeeting.Enabled = true;
                    simpleButton1.Enabled = true;
                    simpleButton2.Enabled = true;
                    simpleButton3.Enabled = true;
                
                    //var row = obj.GetSelectedDataRow() as CTBookingSchedule;
                    //if (row != null)
                    //{
                        SalesPerson person = new SalesPerson()
                        {
                            Id = obj.resource_id,
                            Name = obj.resource_name
                        };
                        //lookUpEdit1.Properties.DataSource = new List<SalesPerson> { person };
                        lookUpEdit1.Properties.DataSource = listBookingSalesPerson;
                        lookUpEdit1.EditValue = person.Id;
                        //listBookingSchedules
                        lookUpEdit1SelectedIndexChanged(person);

                        if (lookUpEdit2.Tag != null)
                        {
                            var data = (ScheduleData)lookUpEdit2.Tag;
                            var tag = data.PositionIndex.ToString();
                            tag = tag.Replace("IndexPosition", "");
                            var oSubQ = listSubQ[int.Parse(tag)];
                            oSubQ.ScheduleValue = new ScheduleValue()
                            {
                                ScheduleId = obj.id.ToString(),
                                Description = obj.title
                            };
                            Questionnaire.Form.Settings.DataBindings.schedule_id = obj.id.ToString();
                            data.HasValue = obj.id > 0;

                            /**
                             * if not from the same company,
                             * dont allow edit.
                             */
                            int _id = Convert.ToInt32(obj.id);
                            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
                            {
                                answer _data = _efDbContext.answers.FirstOrDefault(i => i.schedule_id == _id);
                                if (_data != null)
                                {
                                    _efDbContext.Detach(_data);
                                    if (_data.accounts_id != AccountId)
                                        this.CreateMeeting.Enabled = false;
                                }
                                else
                                    this.CreateMeeting.Enabled = false;
                            }
                        }
                    //}
                //}
                //else
                //{
                //    PreviewBooking.Enabled = false;
                //    //DeleteMeeting.Enabled = false;
                //}

            }

            if (!IsInitializing)
            {
                if (isLoaded)
                    m_bHasChanged = true;

                if (OnComponentNotifyChanged != null)
                    OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(isLoaded));
            }

            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }
        //comment
        private void memoEdit1_TextChanged(object sender, EventArgs e) {
            var objSender = sender as TextEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            ScheduleData data = null;
            if (objSender.Tag != null) {
                data = (ScheduleData)objSender.Tag;
                var tag = data.PositionIndex;
                var parentTag = ((ScheduleData)objSender.Tag).ParentPositionIndex;
                tag = tag.Replace("IndexPosition", "");
                parentTag = parentTag.Replace("IndexPosition", "");
                var oSubQ = listSubQ[int.Parse(parentTag)].OtherChoices[int.Parse(tag)];
                oSubQ.InputValue = objSender.Text;
                oSubQ.DefaultInputValue = objSender.Text;
                if(!string.IsNullOrEmpty(oSubQ.InputValue) &&
                    !string.IsNullOrEmpty(oSubQ.InputValue.Trim()))
                    data.HasValue = true;
                else
                    data.HasValue = false;
            }

            try {
                LayoutControlItem lci = ((LayoutControlItem)((ScheduleData)((TextEdit)sender).Tag).ControlContainer);
                var memoEditor = sender as MemoEdit;
                if (memoEditor != null) {
                    var vi = memoEditor.GetViewInfo() as MemoEditViewInfo;
                    TextBoxMaskBox maskbox = memoEditor.MaskBox;
                    int h = maskbox.GetPreferredSize(new Size(vi.MaskBoxRect.Width, int.MaxValue)).Height;
                    if (data != null && data.ChoiceOption != null && !string.IsNullOrEmpty(data.ChoiceOption.TextPrefix)) {
                        if (h > 41) {
                            //lci.MaxSize = new Size(0, h + 10);
                            lci.MinSize = new Size(lci.MinSize.Width, h + 24);
                        } else {
                            lci.MinSize = new Size(lci.MinSize.Width, 41);
                        }
                    } else {
                        if (h > 24) {
                            //lci.MaxSize = new Size(0, h + 10);
                            lci.MinSize = new Size(lci.MinSize.Width, h + 10);
                        } else {
                            lci.MinSize = new Size(lci.MinSize.Width, 24);
                        }
                    }
                }
                //var vi = objSender.GetViewInfo() as MemoEditViewInfo;
                //var cache = new GraphicsCache(objSender.CreateGraphics());
                //int h = ((IHeightAdaptable)vi).CalcHeight(cache, lci.Width);
                //var args = new ObjectInfoArgs();
                //args.Bounds = new Rectangle(0, 0, lci.Width, h);
                //var rect = vi.BorderPainter.CalcBoundsByClientRectangle(args);
                //cache.Dispose();
                //if (rect.Height > 24) {
                //    //lci.MinSize = new Size(lci.MinSize.Width, rect.Height);
                //    lci.Size = new Size(lci.MinSize.Width, rect.Height);
                //    objSender.Size = new Size(lci.Width - 5, lci.Height - 5);
                //} else {
                //    //lci.MinSize = new Size(lci.MinSize.Width, 24);
                //    lci.Size = new Size(lci.MinSize.Width, 24);
                //    objSender.Size = new Size(lci.Width - 5, lci.Height - 5);
                //}
            } catch { }

            if (!IsInitializing) {
                if (isLoaded && !isResizedEvent)
                    m_bHasChanged = true;

                if (OnComponentNotifyChanged != null)
                    OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(isLoaded));
            }

            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }

        private void memoEdit1_Resize(object sender, EventArgs e) {
            isResizedEvent = true;
            memoEdit1_TextChanged(sender, e);
            isResizedEvent = false;
        }

        private void gridView1_DataSourceChanged(object sender, EventArgs e) {
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            List<ContactAttendie> datasource = view.GridControl.DataSource as List<ContactAttendie>;
            if (datasource != null) {                
                var listSubQ = Questionnaire.Form.Settings.AnswerOptions[0];
                if (listSubQ == null) listSubQ = new AnswerOption();
                else listSubQ.Attendies.Clear();
                datasource.ForEach(delegate(ContactAttendie contact) {
                    listSubQ.Attendies.Add(new Attendie() { Id = contact.ContactID, Name = contact.Name });
                });
                var data = view.Tag as ScheduleData;
                if (data != null) {
                    data.HasValue =  listSubQ.Attendies != null && listSubQ.Attendies.Count > 0;
                }
            }

            if (!IsInitializing) {
                if (isLoaded && isGridDataSourceLoaded) {
                    m_bHasChanged = true;
                    if (OnComponentNotifyChanged != null)
                        OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(isLoaded));
                }
            }

            if (!isGridDataSourceLoaded)
                isGridDataSourceLoaded = true;

            if (ControlGroup != null)
                ControlGroup.Tag = this;
            
        }
        
        private void viewDetailSummary_Click(object sender, EventArgs e) {
            if (lookUpEdit2 != null && lookUpEdit2.EditValue != null) {
                int schedule_id = (int)lookUpEdit2.EditValue;
                if (schedule_id > 0) {
                    ScheduleDetails details = new ScheduleDetails(schedule_id);
                    details.ShowDialog();
                }
            }
        }

        private void createMeeting_Click(object sender, EventArgs e) 
        {
            if (ContactList == null || SubcampaignID <= 0) return;
            if (ShowCalendarBookingClick != null)
                ShowCalendarBookingClick(this, e);
        }

        private void deleteMeeting_Click(object sender, EventArgs e) {
            DialogResult _dlgResult = MessageBox.Show("Are you sure to delete this meeting?", "Bright Manager/Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlgResult == DialogResult.No)
                return;

            if (lookUpEdit2.EditValue != null) {
                Cursor current = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                int schedId = (int)lookUpEdit2.EditValue;
                var dsSchedules = lookUpEdit2.Properties.DataSource as List<CTBookingSchedule>;
                if (dsSchedules != null) {
                    BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
                    var selectedSchedule = dsSchedules.FirstOrDefault(x => x.id == schedId);                    
                    var sched = BPContext.schedules.FirstOrDefault(x => x.id == schedId);
                    if (sched != null) {
                        BPContext.schedules.DeleteObject(sched);
                        try {
                            BPContext.SaveChanges();
                            //remove from datasource
                            dsSchedules.RemoveAll(x => x.id == schedId);
                            var objSender = sender as SimpleButton;
                            if (objSender != null) {
                                objSender.Enabled = false;
                                PreviewBooking.Enabled = false;
                            }
                            lookUpEdit2.EditValue = null;
                        } catch { }
                    }
                }
                Cursor.Current = current;
                try {
                    lookUpEdit2.ItemIndex = -1;
                    //lookUpEdit2.ItemIndex = 0;
                    this.lblScheduleDetails.Text = "";
                }
                catch { }
            }
            //if (lookUpEdit2.EditValue == null)
            if (lookUpEdit2.ItemIndex < 1)
                this.gridControl1.DataSource = null;
        }

        private void AddCaller_Click(object sender, EventArgs e) 
        {
            if (m_Caller != null) {
                ContactAttendie res = null;
                if (ContactAttendies != null) {
                    res = ContactAttendies.FirstOrDefault(
                        x => x.AccountID == m_Caller.AccountID &&
                            x.ContactID == m_Caller.ContactID &&
                            x.Name == m_Caller.Name);                    
                }
                if (res == null) {
                    using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                        contact _eftContact = _efDbContext.contacts.FirstOrDefault(i => i.id == m_Caller.ContactID);
                        if (_eftContact != null) {
                            m_Caller.Email = _eftContact.email;
                            m_Caller.Address = string.Format("{0}{1}{2}{3}",
                                string.IsNullOrEmpty(_eftContact.address_1) ? "" : _eftContact.address_1,
                                string.IsNullOrEmpty(_eftContact.address_2) ? "" : " " + _eftContact.address_2,
                                string.IsNullOrEmpty(_eftContact.city) ? "" : ", " + _eftContact.city,
                                string.IsNullOrEmpty(_eftContact.country) ? "" : ", " + _eftContact.country
                            );
                            m_Caller.Telephone = _eftContact.direct_phone;
                            _efDbContext.Detach(_eftContact);
                        }
                    }
                    ContactAttendies.Add(m_Caller);
                    gridControl1.DataSource = null;
                    gridControl1.DataSource = ContactAttendies;
                }
            }
        }

        private void AddAdditional_Click(object sender, EventArgs e) 
        {
            if (ContactList == null || SubcampaignID <= 0) return;
            if (ContactAttendies != null && ContactAttendies.Count == ContactList.Count) {
                MessageBox.Show("No more contact attendees to add.", "System Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AddAttendies frmAddAttendies = new AddAttendies(this, ContactList);
            if (frmAddAttendies.ShowDialog() == DialogResult.OK) {
                gridControl1.DataSource = null;
                gridControl1.DataSource = ContactAttendies;
            }
        }

        private void DeleteAttendie_Click(object sender, EventArgs e) {
            if (ContactList == null || SubcampaignID <= 0) return;
            var atts = ContactAttendies.Clone();
            int[] rowHandles = gridView1.GetSelectedRows();
            if (rowHandles.Length > 0) {
                if (atts == null)
                    atts = new List<ContactAttendie>();
                for (int x = 0; x < rowHandles.Length; ++x) {
                    ContactAttendie att = gridView1.GetRow(rowHandles[x]) as ContactAttendie;                    
                    if (att != null) {
                        att.Attending = false;
                        atts.Remove(att);
                    }
                    //gridView1.DeleteRow(rowHandles[x]);
                }
                ContactAttendies = atts.ToList();
                gridControl1.DataSource = null;
                gridControl1.DataSource = ContactAttendies;
                
            }
        }
        public void RefreshSchedMeetingList() {
            try
            {

                BindBookingSchedules(3, CurrentSelectedMeeting);
                lookUpEdit2_EditValueChanged(lookUpEdit2, null);
            }
            catch { 
            }
        }
        private void BindBookingSchedules(byte scheduleType, int pScheduleId = 0) 
        {
           
            if (SubcampaignID <= 0) return;
            BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            listBookingSchedules = null;
            switch (scheduleType) {
                //seminar
                case 1:
                    listBookingSchedules = BPContext.FIGetBookingSchedule(1, SubcampaignID).ToList();
                    break; 
                //webinar
                case 2:
                    listBookingSchedules = BPContext.FIGetBookingSchedule(2, SubcampaignID).ToList();
                    break; 
                //meeting
                case 3:
                    listBookingSchedules = BPContext.FIGetBookingSchedule(3, SubcampaignID).ToList();
                    break; 
            }

            //lookUpEdit2: schedules
            lookUpEdit1.Properties.ReadOnly = false; //sales person is always read only
            listBookingSalesPerson = new List<CTBookingSchedule>();

            if (listBookingSchedules.Count() > 0) {
                foreach (CTBookingSchedule _item in listBookingSchedules) {
                    if (listBookingSalesPerson.Exists(i => i.resource_id == _item.resource_id))
                        continue;

                    listBookingSalesPerson.Add(_item);
                }
                m_HasSchedules = true;
                //lookUpEdit1.Properties.ReadOnly = false;
                lookUpEdit2.Properties.ReadOnly = false;
                lookUpEdit2.Properties.AllowFocused = true;
                lookUpEdit1.Properties.DataSource = listBookingSalesPerson;
                lookUpEdit2.Properties.DataSource = listBookingSchedules;
                if (pScheduleId > 0)
                {
                    lookUpEdit2.EditValue = pScheduleId;
                    if (this.lblScheduleDetails != null)
                    {
                        CTBookingSchedule _item = listBookingSchedules.Where(i => i.id == pScheduleId).FirstOrDefault();

                        if (_item != null)
                        {
                            this.lblScheduleDetails.Text = _item.title;

                            lookUpEdit2EditValueChanged(_item);
                        }

                        if (_item.resource_name != "")
                        {
                            this.lblScheduleDetails.Text += " | " + _item.resource_name;
                        }
                    }
                }
            }
            else {
                m_HasSchedules = false;
                //lookUpEdit1.Properties.ReadOnly = true;
                lookUpEdit2.Properties.ReadOnly = true;
                lookUpEdit2.Properties.AllowFocused = false;
                lookUpEdit1.Properties.DataSource = null;
                lookUpEdit2.Properties.DataSource = null;
            }
        }

        private void SetCurrentAttendies() 
        {
            gridControl1.DataSource = null;
            if (ContactList == null || ContactList.Count <= 0) 
                return;

            int accountid = int.Parse(Questionnaire.Form.Settings.DataBindings.account_id);
            var AllAttendies = new List<ContactAttendie>();
            ContactList.ForEach(delegate(CTScSubCampaignContactList ca)
            {
                AllAttendies.Add(new ContactAttendie() {
                    AccountID = accountid,
                    ContactID = ca.id,
                    Name = ca.first_name + (ca.last_name.Length > 0 ? " " + ca.last_name : ""),
                    Address = ca.complete_address,
                    City = "",
                    Email = ca.email,
                    Telephone = ca.direct_phone,
                    Attending = false
                });
            });
            
            var calValues = Questionnaire.Form.Settings.AnswerOptions[0].Attendies;
            var setValues = from ac in AllAttendies where calValues.Any(x => x.Id == ac.ContactID && x.Name == ac.Name) select ac;

            ContactAttendies = new List<ContactAttendie>();            
            setValues.ForEach(delegate(ContactAttendie contact) {                
                contact.Attending = true;
            });
            ContactAttendies.AddRange(setValues);
            gridControl1.DataSource = ContactAttendies;
        }

        private void layoutControlGroupQuestion1_Click(object sender, EventArgs e) {
            LayoutControlGroup objSender = null;
            if (sender is LayoutControlGroup) {
                objSender = sender as LayoutControlGroup;
                objSender.Owner.FocusHelper.FocusElement(objSender, false);
            } else if (sender is SimpleLabelItem) {
                objSender = ((SimpleLabelItem)sender).Parent as LayoutControlGroup;
                objSender.Owner.FocusHelper.FocusElement(objSender, false);
            } else if (sender is LayoutControlItem) {
                objSender = ((LayoutControlItem)sender).Parent as LayoutControlGroup;
                objSender.Owner.FocusHelper.FocusElement(objSender, false);
            } else if (sender is LookUpEdit) {
                objSender =
                    ((LayoutControlItem)((ScheduleData)((LookUpEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;                
            } else if (sender is TextEdit) {
                objSender =
                    ((LayoutControlItem)((ScheduleData)((TextEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;
            }
            if (objSender != null) {
                LayoutGroup objRootGroup = objSender.Parent;
                LayoutControlGroup objGroup;
                IQuestionnaire iQuestion;
                Image bgImage = null;
                if (objRootGroup != null) {
                    foreach (BaseLayoutItem item in objRootGroup.Items) {
                        if (item.IsGroup) {
                            objGroup = item as LayoutControlGroup;
                            iQuestion = objGroup.Tag as IQuestionnaire;
                            bgImage = BGImage(iQuestion);
                            if (!objGroup.Equals(objSender)) {
                                objGroup.BackgroundImage = bgImage;
                                if (bgImage != null)
                                    objGroup.BackgroundImageVisible = true;
                                else
                                    objGroup.BackgroundImageVisible = false;
                                iQuestion.Focused = false;
                            } else {
                                iQuestion.Focused = true;
                            }
                        }
                    }
                }
                if (!DisableSelection) {
                    this.layoutControlGroupQuestion1.BackgroundImage = Properties.Resources.bg_selector;
                    this.layoutControlGroupQuestion1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                }
                objSender.BackgroundImageVisible = true;
            }
            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }
                
        #endregion

        #region Public Event and Delegates
        public event ComponentDialogNotifyChangedEventHandler OnComponentNotifyChanged;
        #endregion
        
        #region Private Class
        private class ScheduleData {
            public string Name { get; set; }
            public string ParentPositionIndex { get; set; }
            public string PositionIndex { get; set; }
            public object ControlContainer { get; set; }
            public bool Required { get; set; }
            public bool HasValue { get; set; }
            public bool IsMeeting { get; set; }
            public OtherChoice ChoiceOption { get; set; }
        }
        #endregion

        #region IQuestionnaire Members


        public void Save()
        {
           
        }
        
        #endregion
    }

}
