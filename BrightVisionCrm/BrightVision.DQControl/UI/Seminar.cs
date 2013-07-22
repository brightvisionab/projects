using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.Utils;
using Newtonsoft.Json;

using BrightVision.Model;
using BrightVision.DQControl.Business;

namespace BrightVision.DQControl.UI {
    /// <summary>
    /// Represents the Seminar Type of Questionnaire
    /// </summary>
    public class Seminar : LayoutControlGroup, IQuestionnaire {

        #region Member Variables
        private LayoutControlGroup layoutControlGroupQuestion1;        
        private MemoEdit memoEdit1;
        private LookUpEdit lookUpEdit1;
        private SimpleButton simpleButton1;
        private EmptySpaceItem emptySpaceItem1;      
        private EmptySpaceItem emptySpaceItem2;
        private LayoutControlItem layoutControlItem1;
        private LayoutControlItem layoutControlItem2;
        private SimpleLabelItem simpleLabelItem1;

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;        
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn6;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn7;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn8;

        #endregion

        #region Constructors
        public Seminar(IStyleController styleController)
            : base() {
                InitializeComponent();
                StyleController = styleController;
        }

        public Seminar(IStyleController styleController, string script)
            : base() {
                InitializeComponent();
                StyleController = styleController;
                JSONString = script;
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
        public object SeminarScheduleDataSource {
            get;
            set;
        }

        public LayoutControlGroup ControlGroup {
            get;
            set;
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
        #endregion
        
        #region Public Methods
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
        public void BindControls() {
            #region Initialize
            if (Questionnaire == null)
                MessageBox.Show("Questionnaire must be bind with JSON first.", "Seminar Component");
            var answerOpt = Questionnaire.Form.Settings.AnswerOptions[0] as ISeminar;
            if (SeminarScheduleDataSource == null) {                
                if (answerOpt != null) {
                    if (answerOpt.SeminarSchedule != null && answerOpt.SeminarSchedule.ScheduleValues.Count > 0)
                        SeminarScheduleDataSource = answerOpt.SeminarSchedule.ScheduleValues;
                    else {
                        MessageBox.Show("\"SeminarScheduleDataSource\" property must be set first.", "Seminar Component");
                        return;
                    }
                } else {
                    MessageBox.Show("\"SeminarScheduleDataSource\" property must be set first.", "Seminar Component");
                    return;
                }
                
            }
            #endregion

            this.layoutControlGroupQuestion1.Clear();
            Settings oSettings = Questionnaire.Form.Settings;

            // layoutControlGroupQuestion1                        
            this.layoutControlGroupQuestion1.Name = "layoutControlGroupQuestion" + Guid.NewGuid().ToString();
            this.layoutControlGroupQuestion1.AppearanceGroup.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.layoutControlGroupQuestion1.AppearanceGroup.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.layoutControlGroupQuestion1.AppearanceGroup.Options.UseFont = true;
            this.layoutControlGroupQuestion1.ExpandButtonVisible = true;
            //this.layoutControlGroupQuestion1.Location = new System.Drawing.Point(0, 0);            
            this.layoutControlGroupQuestion1.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutControlGroupQuestion1.ShowInCustomizationForm = false;
            this.layoutControlGroupQuestion1.Text = oSettings.Label + " " + oSettings.QuestionText;
            this.layoutControlGroupQuestion1.BeginUpdate();

            IList<AnswerOption> answeroptionList = Questionnaire.Form.Settings.AnswerOptions;
            ISeminar answeroption = null;
            int iAnswerOptions = answeroptionList.Count;
            System.Drawing.Size newSize;
            int selectedIndex = 0;
            int idx = 0; 
            for (int x = 0; x < iAnswerOptions; ++x) {
                answeroption = answeroptionList[x] as ISeminar;

                #region Seminar List
                // layoutControlItem1                                                         
                this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
                //lookUpEdit1
                this.lookUpEdit1 = new LookUpEdit();
                this.lookUpEdit1.Name = "lookUpEdit" + Guid.NewGuid().ToString();                
                this.lookUpEdit1.Properties.NullText = "";
                this.lookUpEdit1.Properties.DisplayMember = "Name";
                this.lookUpEdit1.Properties.ValueMember = "Id";
                this.lookUpEdit1.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name", "Seminars"));
                this.lookUpEdit1.Properties.ShowFooter = false;
                this.lookUpEdit1.Properties.ShowHeader = false;
                this.lookUpEdit1.ItemIndex = -1;
                this.lookUpEdit1.Tag = new SeminarData() {
                    PositionIndex = "IndexPosition" + x,
                    ControlContainer = layoutControlItem1
                };
                this.lookUpEdit1.Size = new System.Drawing.Size(155, 20);
                this.lookUpEdit1.StyleController = this.StyleController;
                this.lookUpEdit1.EditValueChanged += new EventHandler(lookUpEdit1_SelectedIndexChanged);
                this.lookUpEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.lookUpEdit1.Properties.DataSource = SeminarScheduleDataSource;

                if (answeroption.SeminarSchedule != null && !string.IsNullOrEmpty(answeroption.SeminarSchedule.SelectedValue)) {
                    this.lookUpEdit1.EditValue = answeroption.SeminarSchedule.SelectedValue;
                } else {
                    selectedIndex = -1;
                }
                this.lookUpEdit1.ItemIndex = selectedIndex;
                                
                // layoutControlItem1
                this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();  
                this.layoutControlItem1.Control = this.lookUpEdit1;
                int hgt = 28;
                if (answeroption.SeminarSchedule != null && !string.IsNullOrEmpty(answeroption.SeminarSchedule.TextPrefix)) {
                    this.layoutControlItem1.Text = answeroption.SeminarSchedule.TextPrefix;
                    hgt += 20;
                } else {
                    this.layoutControlItem1.TextVisible = false;
                }
                this.layoutControlItem1.TextLocation = DevExpress.Utils.Locations.Top;
                this.layoutControlItem1.TextSize = new System.Drawing.Size(108, 13);
                this.layoutControlItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.layoutControlItem1.MaxSize = new System.Drawing.Size(0, hgt);
                this.layoutControlItem1.MinSize = new System.Drawing.Size(0, hgt);
                this.layoutControlItem1.Size = new System.Drawing.Size(50, 28);
                this.layoutControlItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1);

                #endregion

                #region Customer Attendies
                //create our gridcontrol
                // 
                // gridColumn1
                // 
                this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn1.Caption = "Attending";
                this.gridColumn1.ColumnEdit = this.repositoryItemCheckEdit1;
                this.gridColumn1.FieldName = "Attending";
                this.gridColumn1.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn1.Visible = true;
                this.gridColumn1.VisibleIndex = 0;
                this.gridColumn1.Width = 57;
                // 
                // repositoryItemCheckEdit1
                // 
                this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
                this.repositoryItemCheckEdit1.AutoHeight = false;
                this.repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit" + Guid.NewGuid().ToString();
                // 
                // gridColumn2
                // 
                this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn2.Caption = "Name";
                this.gridColumn2.FieldName = "Name";
                this.gridColumn2.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn2.Visible = true;
                this.gridColumn2.OptionsColumn.AllowEdit = false;
                this.gridColumn2.VisibleIndex = 1;
                this.gridColumn2.Width = 66;
                // 
                // gridColumn3
                // 
                this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn3.Caption = "Address";
                this.gridColumn3.FieldName = "Address";
                this.gridColumn3.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn3.Visible = true;
                this.gridColumn3.OptionsColumn.AllowEdit = false;
                this.gridColumn3.VisibleIndex = 2;
                this.gridColumn3.Width = 66;
                // 
                // gridColumn4
                // 
                this.gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn4.Caption = "City";
                this.gridColumn4.FieldName = "City";
                this.gridColumn4.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn4.Visible = true;
                this.gridColumn4.OptionsColumn.AllowEdit = false;
                this.gridColumn4.VisibleIndex = 3;
                this.gridColumn4.Width = 66;
                // 
                // gridColumn5
                // 
                this.gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn5.Caption = "Telephone";
                this.gridColumn5.FieldName = "Telephone";
                this.gridColumn5.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn5.Visible = true;
                this.gridColumn5.OptionsColumn.AllowEdit = false;
                this.gridColumn5.VisibleIndex = 4;
                this.gridColumn5.Width = 66;
                // 
                // gridColumn6
                // 
                this.gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn6.Caption = "Email";
                this.gridColumn6.FieldName = "Email";
                this.gridColumn6.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn6.Visible = true;
                this.gridColumn6.OptionsColumn.AllowEdit = false;
                this.gridColumn6.VisibleIndex = 5;
                this.gridColumn6.Width = 70;

                // 
                // gridColumn7
                // 
                this.gridColumn7 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn7.Caption = "AccountID";
                this.gridColumn7.FieldName = "AccountID";
                this.gridColumn7.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn7.Visible = false;
                // 
                // gridColumn8
                // 
                this.gridColumn8 = new DevExpress.XtraGrid.Columns.GridColumn();
                this.gridColumn8.Caption = "ContactID";
                this.gridColumn8.FieldName = "ContactID";
                this.gridColumn8.Name = "gridColumn" + Guid.NewGuid().ToString();
                this.gridColumn8.Visible = false;
                                               
                // 
                // gridView1
                // 
                this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
                this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
                this.gridColumn1,
                this.gridColumn2,
                this.gridColumn3,
                this.gridColumn4,
                this.gridColumn5,
                this.gridColumn6,
                this.gridColumn7,
                this.gridColumn8});
                this.gridView1.GridControl = this.gridControl1;
                this.gridView1.Name = "gridView" + Guid.NewGuid().ToString();
                this.gridView1.OptionsFind.AlwaysVisible = true;
                this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
                this.gridView1.OptionsView.ShowGroupPanel = false;
                this.gridView1.OptionsView.ColumnAutoWidth = false;
                this.gridView1.CellValueChanging += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gridView1_CellValueChanging);
                // 
                // gridControl1
                // 
                this.gridControl1 = new DevExpress.XtraGrid.GridControl();
                this.gridControl1.LookAndFeel.UseDefaultLookAndFeel = false;
                this.gridControl1.MainView = this.gridView1;
                this.gridControl1.Name = "gridControl" + Guid.NewGuid().ToString();
                this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
                this.repositoryItemCheckEdit1});
                this.gridControl1.Size = new System.Drawing.Size(412, 74);
                this.gridControl1.TabIndex = 11;
                this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
                this.gridView1});
                BindAttendiesDatasource();
                // 
                // layoutControlItem2
                // 
                this.layoutControlItem2 = new LayoutControlItem();
                this.layoutControlItem2.Control = this.gridControl1;
                this.layoutControlItem2.Name = "layoutControlItem" + Guid.NewGuid().ToString();                
                this.layoutControlItem2.Text = answeroption.AttendiesLabel;
                this.layoutControlItem2.TextLocation = DevExpress.Utils.Locations.Top;
                this.layoutControlItem2.TextSize = new System.Drawing.Size(108, 13);
                this.layoutControlItem2.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.layoutControlItem2.MaxSize = new System.Drawing.Size(0, 200);
                this.layoutControlItem2.MinSize = new System.Drawing.Size(0, 200);
                this.layoutControlItem2.Size = new System.Drawing.Size(200, 200);
                this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem2);
                // 
                // simpleButton1
                //                 
                this.simpleButton1 = new SimpleButton();
                this.simpleButton1.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.simpleButton1.Size = new System.Drawing.Size(130, 22);
                this.simpleButton1.StyleController = this.StyleController;
                this.simpleButton1.Text = answeroption.AddContactButtonLabel; 
                this.simpleButton1.Click += new EventHandler(simpleButton1_Click);

                // layoutControlItem2
                this.layoutControlItem2 = new LayoutControlItem();
                this.layoutControlItem2.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.layoutControlItem2.Control = this.simpleButton1;                              
                this.layoutControlItem2.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.layoutControlItem2.MaxSize = new System.Drawing.Size(150, 24);
                this.layoutControlItem2.MinSize = new System.Drawing.Size(80, 24);
                this.layoutControlItem2.Size = new System.Drawing.Size(80, 24);
                this.layoutControlItem2.TextVisible = false;                
                this.layoutControlItem2.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem2);
                
                // 
                // simpleButton1
                //                 
                this.simpleButton1 = new SimpleButton();
                this.simpleButton1.Name = "simpleButton" + Guid.NewGuid().ToString();
                this.simpleButton1.Size = new System.Drawing.Size(130, 22);
                this.simpleButton1.StyleController = this.StyleController;
                this.simpleButton1.Text = "Refresh";
                this.simpleButton1.Click += new EventHandler(simpleButton1Refresh_Click);

                // layoutControlItem1
                this.layoutControlItem1 = new LayoutControlItem();
                this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.layoutControlItem1.Control = this.simpleButton1;
                this.layoutControlItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
                this.layoutControlItem1.MaxSize = new System.Drawing.Size(80, 24);
                this.layoutControlItem1.MinSize = new System.Drawing.Size(80, 24);
                this.layoutControlItem1.Size = new System.Drawing.Size(80, 24);
                this.layoutControlItem1.TextVisible = false;
                this.layoutControlItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1, this.layoutControlItem2, DevExpress.XtraLayout.Utils.InsertType.Right);
                                
                // emptySpaceItem1
                this.emptySpaceItem1 = new EmptySpaceItem();
                this.emptySpaceItem1.Name = "emptySpaceItem" + Guid.NewGuid().ToString();
                this.emptySpaceItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;                
                this.emptySpaceItem1.MaxSize = new System.Drawing.Size(0, 20);
                this.emptySpaceItem1.MinSize = new System.Drawing.Size(0, 20);                
                this.emptySpaceItem1.Size = new System.Drawing.Size(200, 20);
                this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
                this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1, this.layoutControlItem1, DevExpress.XtraLayout.Utils.InsertType.Right);
                #endregion

                #region Other Choices
                idx = 0;

                EmptySpaceItem esitem = new EmptySpaceItem();
                esitem.Size = new Size(100, 20);
                this.layoutControlGroupQuestion1.AddItem(esitem);
                foreach (OtherChoice oChoice in answeroption.OtherChoices) {
                    if (oChoice.Enabled) {
                        if (!string.IsNullOrEmpty(oChoice.TextPrefix)) {
                            // simpleLabelItem1                
                            this.simpleLabelItem1 = new DevExpress.XtraLayout.SimpleLabelItem();

                            this.simpleLabelItem1.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
                            //this.simpleLabelItem1.ShowInCustomizationForm = false;
                            //this.simpleLabelItem1.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                            this.simpleLabelItem1.Text = oChoice.TextPrefix;
                            //newSize = new System.Drawing.Size(100, 20);
                            newSize = new System.Drawing.Size(20, 20);
                            this.simpleLabelItem1.SizeConstraintsType = SizeConstraintsType.Custom;
                            this.simpleLabelItem1.MaxSize = new Size(0, 0);
                            this.simpleLabelItem1.MinSize = newSize;
                            this.simpleLabelItem1.Size = newSize;
                            this.simpleLabelItem1.AppearanceItemCaption.BackColor = System.Drawing.Color.Transparent;
                            this.simpleLabelItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                            this.layoutControlGroupQuestion1.AddItem(this.simpleLabelItem1);
                        } else {
                            this.simpleLabelItem1 = null;
                        }
                        // layoutControlItem1                                                         
                        this.layoutControlItem1 = new LayoutControlItem();
                        //add textEdit1 to layout
                        this.memoEdit1 = new MemoEdit();
                        this.memoEdit1.Tag = new SeminarData() {
                            ParentPositionIndex = "IndexPosition" + x.ToString(),
                            PositionIndex = "IndexPosition" + idx.ToString(),
                            ControlContainer = layoutControlItem1
                        };
                        this.memoEdit1.Name = "textEdit" + Guid.NewGuid().ToString();
                        this.memoEdit1.Properties.ScrollBars = System.Windows.Forms.ScrollBars.None;
                        this.memoEdit1.StyleController = this.StyleController;

                        memoEdit1.TextChanged += new EventHandler(memoEdit1_TextChanged);
                        memoEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                        if (string.IsNullOrEmpty(oChoice.InputValue)) {
                            if (!string.IsNullOrEmpty(oChoice.DefaultInputValue)) {
                                this.memoEdit1.Text = oChoice.DefaultInputValue.Trim();
                                memoEdit1_TextChanged(this.memoEdit1, null);
                            }
                        } else {
                            this.memoEdit1.Text = oChoice.InputValue.Trim();
                            memoEdit1_TextChanged(this.memoEdit1, null);
                        }
                        this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                        this.layoutControlItem1.Control = this.memoEdit1;
                        //int width = 0, height = 100;
                        //if (!string.IsNullOrEmpty(oChoice.TextboxMaxWidth)) {
                        //    int.TryParse(oChoice.TextboxMaxWidth, out width);
                        //}
                        //if (!string.IsNullOrEmpty(oChoice.TextboxMaxHeight)) {
                        //    int.TryParse(oChoice.TextboxMaxHeight, out height);
                        //}

                        //newSize = new System.Drawing.Size(width, height);
                        //this.layoutControlItem1.Size = new Size(200, 50);
                        //this.layoutControlItem1.MaxSize = new Size(400, 50);
                        //this.layoutControlItem1.MinSize = new Size(300, 50);
                        this.layoutControlItem1.SizeConstraintsType = SizeConstraintsType.Custom;
                        this.layoutControlItem1.MinSize = new Size(0, 60);
                        //this.layoutControlItem1.Text = oChoice.TextPrefix;
                        this.layoutControlItem1.TextVisible = false;
                        this.layoutControlItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                        //if (this.simpleLabelItem1 != null) {
                        //    this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1,this.simpleLabelItem1, DevExpress.XtraLayout.Utils.InsertType.Right);
                        //} else {
                        this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1);
                        //}
                        idx++;
                    }
                }
                #endregion
            }

            #region Footer
            string prioText = oSettings.Priority;
            if (oSettings.CustomerOwnership && oSettings.BVOwnership) {
                prioText += "(Cust+BV)";
            } else if (oSettings.CustomerOwnership) {
                prioText += "(Cust)";
            } else if (oSettings.BVOwnership) {
                prioText += "(BV)";
            }
            if (oSettings.PlotDoneStatus.Trim().ToLower() == "done") {
                prioText += " Done";
            }

            // simpleLabelItem1 status
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem1.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
            this.emptySpaceItem1.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.emptySpaceItem1.AppearanceItemCaption.Options.UseTextOptions = true;
            this.emptySpaceItem1.ShowInCustomizationForm = false;
            this.emptySpaceItem1.SizeConstraintsType = SizeConstraintsType.Custom;
            this.emptySpaceItem1.Text = prioText;
            this.emptySpaceItem1.TextVisible = true;            
            this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem1);

            if (!string.IsNullOrEmpty(oSettings.QuestionHelp)) {
                this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
                this.emptySpaceItem2.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
                this.emptySpaceItem2.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
                this.emptySpaceItem2.AppearanceItemCaption.Options.UseTextOptions = true;
                this.emptySpaceItem2.ShowInCustomizationForm = false;
                this.emptySpaceItem2.Text = "Help";
                this.emptySpaceItem2.OptionsToolTip.ToolTip = oSettings.QuestionHelp.Trim();

                //apply tooltip controller from parent
                //if (this.ToolTipController != null && this.ToolTipController.DefaultController != null) {
                //    this.emptySpaceItem2.OptionsToolTip.ToolTipController = ToolTipController.DefaultController;
                //}

                this.emptySpaceItem2.TextVisible = true;
                this.emptySpaceItem2.Size = new Size(50, 20);
                this.emptySpaceItem2.MaxSize = new Size(50, 20);
                this.emptySpaceItem2.MinSize = new Size(50, 20);
                this.emptySpaceItem2.SizeConstraintsType = SizeConstraintsType.Custom;
                this.emptySpaceItem2.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.emptySpaceItem2.AppearanceItemCaption.TextOptions.HAlignment = HorzAlignment.Far;
                this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem2, this.emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Right);
            }
            #endregion

            this.layoutControlGroupQuestion1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            this.layoutControlGroupQuestion1.EndUpdate();
            Image bg = BGImage(this);
            if (bg != null) {
                this.layoutControlGroupQuestion1.BackgroundImage = bg;
                this.layoutControlGroupQuestion1.BackgroundImageVisible = true;
            }
            this.ControlGroup = this.layoutControlGroupQuestion1;
            this.ControlGroup.Tag = this;
        }
        public void AddContactToAttendiesDatasource(contact contact, int account_id) {
            if (gridControl1 == null && gridControl1.DataSource == null) return;
            var AttendiesDataSource = gridControl1.DataSource;
            if (AttendiesDataSource == null) {
                AttendiesDataSource = new List<ContactAttendie>();
            }
            var AttendiesDS = AttendiesDataSource as List<ContactAttendie>;
            AttendiesDS.Add(new ContactAttendie() {
                Address = contact.address_1 + " " + contact.address_2,
                Name = contact.first_name + " " + contact.last_name,
                Attending = false,
                City = contact.city,
                ContactID = contact.id,
                Email = contact.email,
                Telephone = contact.direct_phone,
            });
            gridControl1.DataSource = AttendiesDataSource;
        }

        public void BindAttendiesDatasource() {   
            if(gridControl1 == null) return;

            int accountid = int.Parse(Questionnaire.Form.Settings.DataBindings.account_id);
            var contactAttendies = CustomDataContext.GetContactAttendies(accountid);

            var objSrc = contactAttendies as List<ContactAttendie>;
            var calValues = Questionnaire.Form.Settings.AnswerOptions[0].Attendies;
            var setValues = from ac in objSrc where calValues.Any(x => x.Id == ac.ContactID && x.Name == ac.Name) select ac;
            setValues.ForEach(delegate(ContactAttendie contact) {
                contact.Attending = true;
            });
            gridControl1.DataSource = contactAttendies;
        }

        #endregion

        #region Private Methods
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

        private void lookUpEdit1_SelectedIndexChanged(object sender, EventArgs e) {
            var objSender = sender as LookUpEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            if (objSender.Tag != null) {
                var tag = ((SeminarData)objSender.Tag).PositionIndex.ToString();
                tag = tag.Replace("IndexPosition", "");
                var oSubQ = listSubQ[int.Parse(tag)];
                if (objSender.EditValue.ToString() != oSubQ.SeminarSchedule.SelectedValue)
                    oSubQ.SeminarSchedule.SelectedValue = objSender.EditValue.ToString();
            }
            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }

        //private void textEdit1_TextChanged(object sender, EventArgs e) {
        //    var objSender = sender as TextEdit;
        //    var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
        //    if (objSender.Tag != null) {
        //        var tag = ((SeminarData)objSender.Tag).PositionIndex.ToString();
        //        tag = tag.Replace("IndexPosition", "");
        //        var oSubQ = listSubQ[int.Parse(tag)];
        //        oSubQ.SelectionValueIfOther = objSender.Text;
        //    }
        //    if(ControlGroup != null)
        //        ControlGroup.Tag = this;
        //}

        private void memoEdit1_TextChanged(object sender, EventArgs e) {
            var objSender = sender as TextEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            if (objSender.Tag != null) {
                var tag = ((SeminarData)objSender.Tag).PositionIndex;
                var parentTag = ((SeminarData)objSender.Tag).ParentPositionIndex;
                tag = tag.Replace("IndexPosition", "");
                parentTag = parentTag.Replace("IndexPosition", "");
                var oSubQ = listSubQ[int.Parse(parentTag)].OtherChoices[int.Parse(tag)];
                oSubQ.InputValue = objSender.Text;
            }

            try {
                LayoutControlItem lci = ((LayoutControlItem)((SeminarData)((TextEdit)sender).Tag).ControlContainer);
                //if (lci.MinSize.Height <= 50) {
                //auto height textbox
                Size sz = new Size(objSender.ClientSize.Width, int.MaxValue);
                TextFormatFlags flags = TextFormatFlags.WordBreak;
                int padding = 3;
                int borders = objSender.Height - objSender.ClientSize.Height;
                sz = TextRenderer.MeasureText(objSender.Text, objSender.Font, sz, flags);
                int h = sz.Height + borders + padding;
                //if (objSender.Top + h > lci.Height - 10) {
                //    h = lci.Height - 10 - objSender.Top;
                //}
                if(lci.MinSize.Height > 59 && (h +10) > 59)
                    lci.MinSize = new Size(lci.MinSize.Width, h + 10);
                // }
            } catch { }

            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e) {
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            var contactAttendie = view.GetRow(e.RowHandle) as ContactAttendie;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            bool exists = false;
            exists = listSubQ[0].Attendies.Exists(x => x.Id == contactAttendie.ContactID && x.Name == contactAttendie.Name);
            var attendie = new Attendie() {
                Id = contactAttendie.ContactID,
                Name = contactAttendie.Name
            };
            if (Convert.ToBoolean(e.Value)) {
                if (!exists) {
                    listSubQ[0].Attendies.Add(attendie);
                }
            } else {
                if (exists) {
                    listSubQ[0].Attendies.Remove(attendie);
                }
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e) {
            var databinding = Questionnaire.Form.Settings.DataBindings;
            if (string.IsNullOrEmpty(databinding.account_id)) return;
            AddContact contactForm = new AddContact(int.Parse(databinding.account_id));
            contactForm.OwnerUI = this;
            contactForm.StartPosition = FormStartPosition.CenterScreen;
            contactForm.ShowDialog();
        }

        private void simpleButton1Refresh_Click(object sender, EventArgs e) {
            BindAttendiesDatasource();
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
            } else if (sender is TextEdit) {
                objSender =
                    ((LayoutControlItem)((SeminarData)((TextEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;
            } else if (sender is LookUpEdit) {
                objSender =
                    ((LayoutControlItem)((SeminarData)((LookUpEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;
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
        
        #region Private Class
        private class SeminarData {
            public string ParentPositionIndex { get; set; }
            public string PositionIndex { get; set; }
            public object ControlContainer { get; set; }
        } 
        #endregion
    }
}
