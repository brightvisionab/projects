
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraLayout;
using BrightVision.DQControl.Business;
using Newtonsoft.Json;
using System.Linq;
using BrightVision.Model;

namespace BrightVision.DQControl.UI 
{
    public class Multiplechoice : LayoutControlGroup, IQuestionnaire 
    {
        #region Member Variables
        private LayoutControlGroup layoutControlGroupQuestion1;
        //private LabelControl labelControl1;
        private MemoEdit memoEdit1;
        private CheckEdit checkEdit1;
        //private SimpleLabelItem simpleLabelItem1;
        private LayoutControlItem layoutControlItem1;
        private SimpleLabelItem simpleLabelItemValidation;
        //private LayoutControlItem layoutControlItem2;
        private Footer ctlFooter;
        private bool isLoaded;
        private bool isResizedEvent;

        private List<DynamicControl> m_DynamicControls = null;
        private class DynamicControl {
            public Control Control { get; set; }
            public string Name { get; set; }
            public bool DefaultValue { get; set; }
            public string DefaultTextValue { get; set; }
            public string TextPrefix { get; set; }
        }
        #endregion

        #region Constructors
        public Multiplechoice(IStyleController styleController)
            : base() {
            InitializeComponent();
            StyleController = styleController;
            this.Disposed += new EventHandler(Multiplechoice_Disposed);
        }

        void Multiplechoice_Disposed(object sender, EventArgs e)
        {
            this.layoutControlGroupQuestion1.Click -= new EventHandler(layoutControlGroupQuestion1_Click);
            this.OnComponentNotifyChanged = null;
            ctlFooter.Parent = null;
            ctlFooter = null;
            this.ControlGroup.Tag = null;
            this.ControlGroup = null;
            this.Parent = null; 
        }

        public Multiplechoice(IStyleController styleController, string script)
            : base() {
            InitializeComponent();
            StyleController = styleController;
            JSONString = script;
            this.Disposed += new EventHandler(Multiplechoice_Disposed);
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
        public List<CTScSubCampaignContactList> ContactList { get; set; }
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
        public void BindControls() 
        {
            if (Questionnaire == null)
                MessageBox.Show("Questionnaire must be bind with JSON first.", "MultipleChoice Component");

            isLoaded = false;
            this.layoutControlGroupQuestion1.Clear();               
            Settings oSettings = Questionnaire.Form.Settings;
            m_DynamicControls = new List<DynamicControl>();

            // layoutControlGroupQuestion1                        
            this.layoutControlGroupQuestion1.Name = "layoutControlGroupQuestion" + Guid.NewGuid().ToString();
            this.layoutControlGroupQuestion1.AppearanceGroup.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.layoutControlGroupQuestion1.AppearanceGroup.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.layoutControlGroupQuestion1.AppearanceGroup.Options.UseFont = true;
            this.layoutControlGroupQuestion1.ExpandButtonVisible = false;
            this.layoutControlGroupQuestion1.GroupBordersVisible = true;
            this.layoutControlGroupQuestion1.TextVisible = false;        
            this.layoutControlGroupQuestion1.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1,1, 1);
            this.layoutControlGroupQuestion1.ShowInCustomizationForm = false;
            //this.layoutControlGroupQuestion1.Size = new System.Drawing.Size(417, 64);
            //this.layoutControlGroupQuestion1.Text = oSettings.Label + " " + oSettings.QuestionText;
            this.layoutControlGroupQuestion1.BeginUpdate();

            #region Footer
            this.layoutControlItem1 = new LayoutControlItem();
            this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();

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
            this.layoutControlItem1.Control = ctlFooter;
            this.layoutControlItem1.ShowInCustomizationForm = false;
            this.layoutControlItem1.TextVisible = false;
            this.layoutControlItem1.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlItem1.MinSize = new Size(0, 24);
            this.layoutControlItem1.MaxSize = new Size(0, 24);
            this.layoutControlItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            this.layoutControlItem1.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1);
            #endregion

            IList<AnswerOption> answeroptionList = Questionnaire.Form.Settings.AnswerOptions;
            IMultipleChoice answeroption = null;
            List<MultipleChoiceValue> mcvalueList = null;
            int iAnswerOptions = answeroptionList.Count;
            int numCols = 1;
            int numRows = 1;
            LayoutControlItem refColItem = null, prevItem = null, lastItem = null;
            List<LayoutControlItem> listItems = null;
            int idx = 0;
            int refColItemWidth = 0;
            for (int x = 0; x < iAnswerOptions; ++x) {                        
                answeroption = answeroptionList[x] as IMultipleChoice;          
                simpleLabelItemValidation = new SimpleLabelItem();
                simpleLabelItemValidation.AppearanceItemCaption.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                simpleLabelItemValidation.Name = Guid.NewGuid().ToString();
                simpleLabelItemValidation.TextAlignMode = TextAlignModeItem.CustomSize;
                simpleLabelItemValidation.TextSize = new Size(212, 15);
                simpleLabelItemValidation.Text = "  Please check at least one item below.";
                simpleLabelItemValidation.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                this.layoutControlGroupQuestion1.AddItem(simpleLabelItemValidation);

                mcvalueList = answeroption.MultipleChoiceValues;
                numCols = answeroption.MultipleChoiceColumns;
                numRows = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(mcvalueList.Count) / Convert.ToDouble(numCols)));
                listItems = new List<LayoutControlItem>();
                for (int i = 1; i <= numCols; ++i) {                  
                    for (int q = 1; q <= numRows && idx < mcvalueList.Count ; ++q) {                       
                        if (i > 1 && q == 1) {
                            lastItem = refColItem;
                        }
                        if (q == 1) {
                            refColItem = new LayoutControlItem();
                            refColItem.Padding = new DevExpress.XtraLayout.Utils.Padding(0);
                            refColItem.Spacing = new DevExpress.XtraLayout.Utils.Padding(0);
                            //add checkedit
                            this.checkEdit1 = new CheckEdit();
                            this.checkEdit1.Tag = new MultiplechoiceData() {
                                ParentPositionIndex = "IndexPosition" + x.ToString(),
                                PositionIndex = "IndexPosition" + idx,
                                ControlContainer = refColItem
                            };
                            this.checkEdit1.Text = mcvalueList[idx].TextPrefix;
                            this.checkEdit1.Checked = mcvalueList[idx].Checked;
                            this.checkEdit1.Name = "checkEdit" + Guid.NewGuid().ToString();
                            //this.checkEdit1.StyleController = this.StyleController;
                            this.checkEdit1.AutoSizeInLayoutControl = true;
                            checkEdit1.CheckedChanged += new EventHandler(checkEdit1_CheckedChanged);

                            m_DynamicControls.Add(new DynamicControl() {
                                Control = this.checkEdit1,
                                Name = this.checkEdit1.Name,
                                DefaultValue = this.checkEdit1.Checked,
                                TextPrefix = this.checkEdit1.Text
                            });

                            refColItem.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                            refColItem.Control = this.checkEdit1;
                            refColItem.TextVisible = false;
                            refColItem.MinSize = new Size(0, 17);
                            refColItem.MaxSize = new Size(0, 17);
                            refColItem.SizeConstraintsType = SizeConstraintsType.Custom;
                            refColItem.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                            refColItem.AllowAutoAlignment = true;

                            if (refColItemWidth < this.checkEdit1.Width) refColItemWidth = this.checkEdit1.Width;

                            if (i > 1) {
                                layoutControlGroupQuestion1.AddItem(refColItem, lastItem, DevExpress.XtraLayout.Utils.InsertType.Right);
                            } else {
                                layoutControlGroupQuestion1.AddItem(refColItem);
                            }
                            listItems.Add(refColItem);                            
                        } 
                        else {
                            this.layoutControlItem1 = new LayoutControlItem();
                            this.layoutControlItem1.Padding = new DevExpress.XtraLayout.Utils.Padding(0);
                            this.layoutControlItem1.Spacing = new DevExpress.XtraLayout.Utils.Padding(0);
                            //add checkedit
                            this.checkEdit1 = new CheckEdit();
                            this.checkEdit1.Tag = new MultiplechoiceData() {
                                ParentPositionIndex = "IndexPosition" + x.ToString(),
                                PositionIndex = "IndexPosition" + idx,
                                ControlContainer = layoutControlItem1
                            };
                            
                            this.checkEdit1.Text = mcvalueList[idx].TextPrefix;
                            this.checkEdit1.Checked = mcvalueList[idx].Checked;
                            this.checkEdit1.Name = "checkEdit" + Guid.NewGuid().ToString();
                            this.checkEdit1.StyleController = this.StyleController;
                            checkEdit1.CheckedChanged += new EventHandler(checkEdit1_CheckedChanged);

                            m_DynamicControls.Add(new DynamicControl() {
                                Control = this.checkEdit1,
                                Name = this.checkEdit1.Name,
                                DefaultValue = this.checkEdit1.Checked,
                                TextPrefix = this.checkEdit1.Text
                            });

                            this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();                            
                            this.layoutControlItem1.Control = this.checkEdit1;
                            this.layoutControlItem1.TextVisible = false;                            
                            this.layoutControlItem1.MinSize = new Size(0, 17);
                            this.layoutControlItem1.MaxSize = new Size(0, 17);
                            this.layoutControlItem1.SizeConstraintsType = SizeConstraintsType.Custom;
                            this.layoutControlItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                            
                            if (q > 1 && i==1) {
                                this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1);
                            } else {                                
                                if ((idx-numRows) >= 0) {
                                    this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1, listItems[idx - numRows], DevExpress.XtraLayout.Utils.InsertType.Right);                                   
                                }                                
                            }
                            listItems.Add(this.layoutControlItem1);
                            prevItem = this.layoutControlItem1;
                        }
                        idx++;
                    }                   
                }
                idx = 0;

                //EmptySpaceItem esitem = new EmptySpaceItem();
                //esitem.Size = new Size(100,20);
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
                        //    this.simpleLabelItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                        //    this.layoutControlGroupQuestion1.AddItem(this.simpleLabelItem1);
                        //} else {
                        //    this.simpleLabelItem1 = null;
                        //}
                        // layoutControlItem1                                                         
                        this.layoutControlItem1 = new LayoutControlItem();
                        //add textEdit1 to layout
                        this.memoEdit1 = new MemoEdit();
                        this.memoEdit1.Tag = new MultiplechoiceData() {
                            ParentPositionIndex = "IndexPosition" + x.ToString(),
                            PositionIndex = "IndexPosition" + idx.ToString(),
                            ControlContainer = layoutControlItem1,
                            Required = oChoice.Required,
                            HasValue = !string.IsNullOrWhiteSpace(oChoice.DefaultInputValue) ? true : false,
                            ChoiceOption = oChoice
                        };
                        this.memoEdit1.Name = "textEdit" + Guid.NewGuid().ToString();
                        this.memoEdit1.Properties.ScrollBars = ScrollBars.None;
                        this.memoEdit1.StyleController = this.StyleController;
                        //set value
                        string _DefaultValue = string.Empty;
                        if (oChoice.DefaultInputValue != null) {
                            _DefaultValue = oChoice.DefaultInputValue.Trim();
                            this.memoEdit1.Text = oChoice.DefaultInputValue.Trim();
                            oChoice.InputValue = oChoice.DefaultInputValue.Trim();                            
                        }

                        m_DynamicControls.Add(new DynamicControl() {
                            Control = this.memoEdit1,
                            Name = this.memoEdit1.Name,
                            DefaultTextValue = _DefaultValue,
                            TextPrefix = oChoice.TextPrefix
                        });

                        memoEdit1.TextChanged += new EventHandler(textEdit1_TextChanged);
                        memoEdit1.Resize += new EventHandler(memoEdit1_Resize);
                        memoEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);

                        this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                        this.layoutControlItem1.Control = this.memoEdit1;
                        if (!string.IsNullOrEmpty(oChoice.TextPrefix)) {
                            this.layoutControlItem1.Text = oChoice.TextPrefix;
                            this.layoutControlItem1.TextVisible = true;
                            this.layoutControlItem1.TextLocation = DevExpress.Utils.Locations.Top;
                            this.layoutControlItem1.MaxSize = new Size(0, 41);
                            this.layoutControlItem1.MinSize = new Size(0, 41);
                        } else {
                            this.layoutControlItem1.TextVisible = false;
                            this.layoutControlItem1.MaxSize = new Size(0, 24);
                            this.layoutControlItem1.MinSize = new Size(0, 24);
                        }                        
                        this.layoutControlItem1.SizeConstraintsType = SizeConstraintsType.Custom;
                        this.layoutControlItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                        this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1);
                        idx++;
                    }
                }
				
            }

           

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

            // simpleLabelItem1
            //this.simpleLabelItem1 = new DevExpress.XtraLayout.SimpleLabelItem();
            //this.simpleLabelItem1.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
            //this.simpleLabelItem1.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            //this.simpleLabelItem1.AppearanceItemCaption.Options.UseTextOptions = true;
            //this.simpleLabelItem1.ShowInCustomizationForm = false;
            //this.simpleLabelItem1.Text = prioText;
            //this.simpleLabelItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            //this.layoutControlGroupQuestion1.AddItem(this.simpleLabelItem1);

            //if (!string.IsNullOrEmpty(oSettings.QuestionHelp)) {
            //    // LabelControl1 help
            //    this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            //    this.labelControl1.Name = "labelControl" + Guid.NewGuid().ToString();
            //    this.labelControl1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            //    this.labelControl1.Appearance.Options.UseTextOptions = true;
            //    this.labelControl1.Text = "Help";
            //    this.labelControl1.ToolTip = oSettings.QuestionHelp.Trim();

            //    //apply tooltip controller from parent
            //    if (this.ToolTipController != null && this.ToolTipController.DefaultController != null) {
            //        this.labelControl1.ToolTipController = ToolTipController.DefaultController;
            //    }
            //    this.labelControl1.Click += new EventHandler(layoutControlGroupQuestion1_Click);

            //    //layoutControlItem1                                                         
            //    this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            //    this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();
            //    this.layoutControlItem1.Control = this.labelControl1;
            //    this.layoutControlItem1.TextVisible = false;
            //    this.layoutControlItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            //    this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1, this.simpleLabelItem1, DevExpress.XtraLayout.Utils.InsertType.Right);
            //}

            this.layoutControlGroupQuestion1.Click += new EventHandler(layoutControlGroupQuestion1_Click);

            /*
             * https://brightvision.jira.com/browse/PLATFORM-2440
             */
            for (int i = 0; i < this.layoutControlGroupQuestion1.Items.Count; i++)
            {
                ((LayoutControlItem)this.layoutControlGroupQuestion1.Items[i]).ControlMinSize = new Size(refColItemWidth, 17);
            }

            this.layoutControlGroupQuestion1.EndUpdate();                       

            Image bg = BGImage(this);
            if (bg != null) {
                this.layoutControlGroupQuestion1.BackgroundImage = bg;
                this.layoutControlGroupQuestion1.BackgroundImageVisible = true;
            }
            if (this.memoEdit1 != null)
                this.BackColor = this.memoEdit1.BackColor;

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
                    MultiplechoiceData data;
                    for (int i = Items.Count - 1; i >= 0; --i) {
                        li = Items[i];
                        if (!li.Equals(Item) && li.Name != Item.Name) {
                            if (li is LayoutControlItem) {
                                Control TempControl = (li as LayoutControlItem).Control;
                                if (TempControl != null) {
                                    data = TempControl.Tag as MultiplechoiceData;
                                    //special case for checkedit's because it is in group of selection
                                    if (TempControl is CheckEdit) {
                                        //do nothing. use below loop for validation of all checkedit
                                    } else {
                                        if (data != null) {
                                            if (data.Required) if (!isRequiredAll) isRequiredAll = true;
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
                    }
                    var opt = Questionnaire.Form.Settings.AnswerOptions[0];
                    if (opt != null) {
                        if (opt.MultipleChoiceRequired) { if (!isRequiredAll) isRequiredAll = true; }
                        if (opt.MultipleChoiceRequired && opt.MultipleChoiceValues != null) {
                            var vals = opt.MultipleChoiceValues;
                            int count = 0;
                            foreach (MultipleChoiceValue val in vals) {
                                if (!val.Checked) { count++; }
                            }
                            if (vals.Count == count) {
                                isValid = false;
                                //prompt message to require selection of checkboxes.
                                if (simpleLabelItemValidation != null) {
                                    simpleLabelItemValidation.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
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
        public void Render()
        {
            if (Questionnaire == null || Questionnaire.Form.Settings == null) {
                BrightVision.Common.UI.NotificationDialog.Information("Multiple Choice", "Questionnaire must be initialized with Json first.");
                return;
            }

            isLoaded = false;
            IList<AnswerOption> _lstAnswerOptions = Questionnaire.Form.Settings.AnswerOptions;
            IMultipleChoice _AnswerData = null;
            DynamicControl _DynamicControl = null;
            List<MultipleChoiceValue> _lstMultipleChoices = null;
            for (int i = 0; i < _lstAnswerOptions.Count; i++) {
                _AnswerData = _lstAnswerOptions[i] as IMultipleChoice;
                _lstMultipleChoices = _AnswerData.MultipleChoiceValues;
                foreach (MultipleChoiceValue _Value in _lstMultipleChoices) {
                    _DynamicControl = m_DynamicControls.FirstOrDefault(e => e.TextPrefix == _Value.TextPrefix);
                    if (_DynamicControl.Control.GetType().Name == "CheckEdit") {
                        CheckEdit _control = _DynamicControl.Control as CheckEdit;
                        _control.Checked = _Value.Checked;
                    }
                }
                foreach (OtherChoice _OtherChoice in _AnswerData.OtherChoices) {
                    if (_OtherChoice.Enabled) {
                        _DynamicControl = m_DynamicControls.FirstOrDefault(e => e.TextPrefix == _OtherChoice.TextPrefix);
                        if (_DynamicControl.Control.GetType().Name == "MemoEdit") {
                            MemoEdit _control = _DynamicControl.Control as MemoEdit;
                            if (_OtherChoice.DefaultInputValue != null) {
                                _control.Text = _OtherChoice.DefaultInputValue.Trim();
                                _OtherChoice.InputValue = _OtherChoice.DefaultInputValue.Trim();
                            }
                        }
                    }
                }

            }

            if (this.memoEdit1 != null)
                this.BackColor = this.memoEdit1.BackColor;

            isLoaded = true;
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
                            if (TempControl is BaseEdit) {
                                (TempControl as BaseEdit).Properties.ReadOnly = !isEditable;
                                (TempControl as BaseEdit).Properties.AllowFocused = isEditable;
                            }                       
                        }
                    }
                }
                Layout.EndUpdate();
            }    
        }

        private void checkEdit1_CheckedChanged(object sender, EventArgs e) {
            var objSender = sender as CheckEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            if (objSender.Tag != null) {
                var tag = ((MultiplechoiceData)objSender.Tag).PositionIndex;
                var parentTag = ((MultiplechoiceData)objSender.Tag).ParentPositionIndex;
                tag = tag.Replace("IndexPosition", "");
                parentTag = parentTag.Replace("IndexPosition", "");
                var oSubQ = listSubQ[int.Parse(parentTag)].MultipleChoiceValues[int.Parse(tag)];
                oSubQ.Checked = objSender.Checked;
                oSubQ.TextPrefix = objSender.Text;
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

        private void textEdit1_TextChanged(object sender, EventArgs e) {
            var objSender = sender as TextEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            MultiplechoiceData data = null;
            if (objSender.Tag != null) {
                data = (MultiplechoiceData)objSender.Tag;
                var tag = data.PositionIndex;
                var parentTag = ((MultiplechoiceData)objSender.Tag).ParentPositionIndex;
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
                LayoutControlItem lci = ((LayoutControlItem)((MultiplechoiceData)((TextEdit)sender).Tag).ControlContainer);
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
            textEdit1_TextChanged(sender, e);
            isResizedEvent = false;
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
                    ((LayoutControlItem)((MultiplechoiceData)((TextEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;
            } else if (sender is CheckEdit) {
                objSender =
                    ((LayoutControlItem)((MultiplechoiceData)((CheckEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;
            }

            if (objSender != null) {
                LayoutGroup objRootGroup = objSender.Parent;
                if (objRootGroup != null) {
                    LayoutControlGroup objGroup;
                    IQuestionnaire iQuestion;
                    Image bgImage = null;
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
        private class MultiplechoiceData {
            public string ParentPositionIndex { get; set; }
            public string PositionIndex { get; set; }
            public object ControlContainer { get; set; }
            public bool Required { get; set; }
            public bool HasValue { get; set; }
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
