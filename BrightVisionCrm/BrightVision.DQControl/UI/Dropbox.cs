
using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.Utils;
using BrightVision.DQControl.Business;
using Newtonsoft.Json;
using BrightVision.Model;
using System.Linq;

namespace BrightVision.DQControl.UI 
{
    public class Dropbox : LayoutControlGroup, IQuestionnaire 
    {
        #region Private Properties
        private LayoutControlGroup layoutControlGroupQuestion1;  
        private TextEdit textEdit1;
        private ComboBoxEdit comboBoxEdit1;
        private SimpleLabelItem simpleLabelItem1;
        private LayoutControlItem layoutControlItem1;
        private LayoutControlItem layoutControlItem2;
        private Footer ctlFooter;
        private bool isLoaded;
        private Color BackColor { get; set; }

        private class DropboxData {
            public string PositionIndex { get; set; }
            public object ControlContainer { get; set; }
            public bool Required { get; set; }
            public bool HasValue { get; set; }
        } 

        private List<DynamicControl> m_DynamicControls = null;
        private class DynamicControl {
            public Control Control { get; set; }
            public string Name { get; set; }
            public string DefaultValue { get; set; }
            public string TextPrefix { get; set; }
        }
        #endregion

        #region Constructors
        public Dropbox(IStyleController styleController): base() 
        {
            InitializeComponent();
            StyleController = styleController;
            this.Disposed += new EventHandler(Dropbox_Disposed);
        }
        public Dropbox(IStyleController styleController, string script) : base() 
        {
            InitializeComponent();
            StyleController = styleController;
            JSONString = script;
            this.Disposed += new EventHandler(Dropbox_Disposed);
        }
        #endregion

        #region Properties
        public IStyleController StyleController { 
            get; 
            set; 
        }
        public DefaultToolTipController ToolTipController {
            get;
            set;
        }
        public CampaignQuestionnaire Questionnaire {
            get;
            set;
        }
        public CampaignQuestionnaire DefaultQuestionnaire {
            get;
            set;
        }
        public CTScSubCampaignContactList ContactPerson {
            get;
            set;
        }
        public List<CTScSubCampaignContactList> ContactList {
            get;
            set;
        }
        public LayoutControlGroup ControlGroup {
            get;
            set;
        }
        public Footer ControlFooter {
            get { 
                return ctlFooter; 
            }
        }

        public int UserId {
            get;
            set;
        }
        public string UserName {
            get;
            set;
        }
        public string JSONString {
            get;
            set;
        }
        
        public bool Focused 
        {  
            get;  
            set;  
        }
        public bool DisableSelection {
            get;
            set;
        }
        public bool HasMissingFields {
            get;
            set;
        }

        private bool m_bReadOnly;
        public bool ReadOnly {
            get { 
                return m_bReadOnly; 
            }
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

        public bool HasMustSaveDefaultValues {
            get;
            set;
        }
        public bool IsInitializing {
            get;
            set;
        }
        public bool InQuestionnaire {
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
        public int AccountId {
            get;
            set;
        }
        public int ContactId {
            get;
            set;
        } //remove...
        #endregion
        
        #region Public Methods
        public void Save()
        {
        }
        public void InitializeComponent()
        {
            layoutControlGroupQuestion1 = new LayoutControlGroup();
        }
        public void BindQuestionnaire() 
        {
            try {
                CampaignQuestionnaire objQuestion = JsonConvert.DeserializeObject<CampaignQuestionnaire>(
                    JSONString,
                    new JsonSerializerSettings {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    }
                );
                Questionnaire = objQuestion;
            } 
            catch (Exception ex) {
                throw ex;
            }
        }
        public void BindControls() 
        {
            if (Questionnaire == null) {
                BrightVision.Common.UI.NotificationDialog.Information("Drop Box", "Questionnaire must be initialized with Json first.");
                return;
            }

            isLoaded = false;
            this.layoutControlGroupQuestion1.Clear();
            Settings oSettings = Questionnaire.Form.Settings;
            m_DynamicControls = new List<DynamicControl>();

            this.layoutControlGroupQuestion1.Name = "layoutControlGroupQuestion" + Guid.NewGuid().ToString();
            this.layoutControlGroupQuestion1.AppearanceGroup.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.layoutControlGroupQuestion1.AppearanceGroup.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.layoutControlGroupQuestion1.AppearanceGroup.Options.UseFont = true;
            this.layoutControlGroupQuestion1.ExpandButtonVisible = false;
            this.layoutControlGroupQuestion1.GroupBordersVisible = true;
            this.layoutControlGroupQuestion1.TextVisible = false;
            //this.layoutControlGroupQuestion1.Location = new System.Drawing.Point(0, 0);            
            this.layoutControlGroupQuestion1.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlGroupQuestion1.ShowInCustomizationForm = false;
            //this.layoutControlGroupQuestion1.Text = oSettings.Label + " " + oSettings.QuestionText;
            this.layoutControlGroupQuestion1.BeginUpdate();

            #region Footer
            this.layoutControlItem1 = new LayoutControlItem();
            this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();

            //bool isCustomerOwned = false;
            //bool isBrightvisionOwned = false;
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
                QuestionText = String.Format("{0} {1}", oSettings.Label, oSettings.QuestionText)
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
            IDropbox answeroption = null;
            int iAnswerOptions = answeroptionList.Count;
            System.Drawing.Size newSize;
            int selectedIndex = 0;
            for (int x = 0; x < iAnswerOptions; ++x) {
                answeroption = answeroptionList[x] as IDropbox;
                // simpleLabelItem1
                this.simpleLabelItem1 = new DevExpress.XtraLayout.SimpleLabelItem();
                this.simpleLabelItem1.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
                this.simpleLabelItem1.ShowInCustomizationForm = false;
                this.simpleLabelItem1.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                this.simpleLabelItem1.Text = answeroption.TextPrefix;
                //this.simpleLabelItem1.SizeConstraintsType = SizeConstraintsType.Custom;
                //this.simpleLabelItem1.MaxSize = new System.Drawing.Size(0, 10);
                //this.simpleLabelItem1.MinSize = new System.Drawing.Size(5, 10);
                //this.simpleLabelItem1.Size = new System.Drawing.Size(5, 10);       
                this.simpleLabelItem1.AppearanceItemCaption.BackColor = System.Drawing.Color.Transparent;
                this.simpleLabelItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.simpleLabelItem1);
                
                // layoutControlItem1                                                         
                this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
                // comboBox1
                this.comboBoxEdit1 = new ComboBoxEdit();
                this.comboBoxEdit1.Tag = new DropboxData() {
                    PositionIndex = "IndexPosition" + x,
                    ControlContainer = layoutControlItem1,
                    Required = answeroption.SelectionValueRequired,
                    HasValue = !string.IsNullOrWhiteSpace(answeroption.DefaultSelectionValue) ? true : false
                };
                this.comboBoxEdit1.Name = "comboBox" + Guid.NewGuid().ToString();
                this.comboBoxEdit1.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                this.comboBoxEdit1.SelectedIndexChanged += new EventHandler(comboBoxEdit1_SelectedIndexChanged);
                this.comboBoxEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);            
                List<string> dropboxvalues = answeroption.DropBoxValues;
                var colItems = this.comboBoxEdit1.Properties.Items;
                dropboxvalues.ForEach(delegate(string value) {
                    colItems.Add(value);
                });
                
                if (answeroption.DefaultSelectionValue != null) {
                    selectedIndex = dropboxvalues.IndexOf(answeroption.DefaultSelectionValue.Trim());
                    answeroption.SelectionValue = answeroption.DefaultSelectionValue.Trim();
                }

                m_DynamicControls.Add(new DynamicControl() {
                    Control = this.comboBoxEdit1,
                    Name = this.comboBoxEdit1.Name,
                    DefaultValue = selectedIndex.ToString(),
                    TextPrefix = answeroption.TextPrefix
                });

                this.comboBoxEdit1.SelectedIndex = selectedIndex;
                this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();  
                this.layoutControlItem1.Control = this.comboBoxEdit1;                
                //this.layoutControlItem1.MinSize = new Size(150, 24);
                //this.layoutControlItem1.MaxSize = new Size(250, 24);
                
                //if (!string.IsNullOrEmpty(answeroption.DropboxMaxWidth)) {
                //    newSize = new System.Drawing.Size(int.Parse(answeroption.DropboxMaxWidth), 24);
                //    this.layoutControlItem1.Size = newSize;                                    
                //} else {
                //    this.layoutControlItem1.Size = new Size(80, 24);
                //}
                if (answeroption.SelectionValueIfOtherVisible) {
                    this.layoutControlItem1.SizeConstraintsType = SizeConstraintsType.Custom;
                    this.layoutControlItem1.MaxSize = new Size(150, 24);
                    this.layoutControlItem1.MinSize = new Size(80, 24);
                }
                //else {
                //    this.layoutControlItem1.MaxSize = new Size(0, 24);
                //    this.layoutControlItem1.MinSize = new Size(0, 24);                    
                //}
                //if (!string.IsNullOrEmpty(answeroption.TextPrefix)) {
                //    this.layoutControlItem1.Text = answeroption.TextPrefix;
                //    this.layoutControlItem1.TextVisible = true;
                //    this.layoutControlItem1.AppearanceItemCaption.TextOptions.HAlignment = HorzAlignment.Far;                    
                //} else {
                //    this.layoutControlItem1.TextVisible = false;
                //}
                this.layoutControlItem1.TextVisible = false;
                this.layoutControlItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1, this.simpleLabelItem1, DevExpress.XtraLayout.Utils.InsertType.Right);

                if (answeroption.SelectionValueIfOtherVisible) {
                    // layoutControlItem2
                    this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
                    //add textEdit1 to layout
                    this.textEdit1 = new TextEdit();
                    this.textEdit1.Tag = new DropboxData() {
                        PositionIndex = "IndexPosition" + x,
                        ControlContainer = layoutControlItem2,
                        Required = answeroption.SelectionValueIfOtherRequired,
                        HasValue = (!string.IsNullOrWhiteSpace(answeroption.DefaultSelectionValueIfOther) ? true : false)
                    };
                    this.textEdit1.Name = "textEdit" + Guid.NewGuid().ToString();
                    this.textEdit1.StyleController = this.StyleController;
                    textEdit1.TextChanged += new EventHandler(textEdit1_TextChanged);
                    textEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                    //if (!string.IsNullOrEmpty(answeroption.DefaultSelectionValueIfOther)) {
                    //    this.textEdit1.Text = answeroption.DefaultSelectionValueIfOther.Trim();
                    //}else if (!string.IsNullOrEmpty(answeroption.SelectionValueIfOther)) {
                    //    this.textEdit1.Text = answeroption.SelectionValueIfOther.Trim();
                    //}

                    string _DefaultValue = string.Empty;
                    if (answeroption.DefaultSelectionValueIfOther != null) {
                        _DefaultValue = answeroption.DefaultSelectionValueIfOther.Trim();
                        this.textEdit1.Text = answeroption.DefaultSelectionValueIfOther.Trim();
                        answeroption.SelectionValueIfOther = this.textEdit1.Text;
                    }

                    m_DynamicControls.Add(new DynamicControl() {
                        Control = this.textEdit1,
                        Name = this.textEdit1.Name,
                        DefaultValue = _DefaultValue,
                        TextPrefix = answeroption.TextPrefix
                    });

                    this.layoutControlItem2.SizeConstraintsType = SizeConstraintsType.Custom;
                    newSize = new System.Drawing.Size(350, 24);
                    this.layoutControlItem2.Size = newSize;
                    this.layoutControlItem2.MaxSize = new Size(350, 24);
                    this.layoutControlItem2.MinSize = new Size(150, 24);

                    //this.layoutControlItem2.SizeConstraintsType = SizeConstraintsType.Custom;
                    //this.layoutControlItem2.MaxSize = new Size(250, 24);
                    this.layoutControlItem2.Control = this.textEdit1;
                    this.layoutControlItem2.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                    this.layoutControlItem2.TextToControlDistance = 0;
                    this.layoutControlItem2.TextVisible = false;
                    this.layoutControlItem2.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                    this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem2, this.layoutControlItem1, DevExpress.XtraLayout.Utils.InsertType.Right);
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

            //// simpleLabelItem1 status
            //this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            //this.emptySpaceItem1.Name = "simpleLabelItem" + Guid.NewGuid().ToString();
            //this.emptySpaceItem1.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            //this.emptySpaceItem1.AppearanceItemCaption.Options.UseTextOptions = true;
            //this.emptySpaceItem1.ShowInCustomizationForm = false;
            //this.emptySpaceItem1.SizeConstraintsType = SizeConstraintsType.Custom;
            //this.emptySpaceItem1.Text = prioText;
            //this.emptySpaceItem1.TextVisible = true;
            //this.emptySpaceItem1.Size = new Size(10, 20);
            //this.emptySpaceItem1.MaxSize = new Size(0, 20);
            //this.emptySpaceItem1.MinSize = new Size(0, 20);
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
            //    this.emptySpaceItem2.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            //    this.emptySpaceItem2.AppearanceItemCaption.TextOptions.HAlignment = HorzAlignment.Far;
            //    this.layoutControlGroupQuestion1.AddItem(this.emptySpaceItem2, this.emptySpaceItem1, DevExpress.XtraLayout.Utils.InsertType.Right);
            //}
            
            this.layoutControlGroupQuestion1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            this.layoutControlGroupQuestion1.EndUpdate();
            Image bg = BGImage(this);
            if (bg != null) {
                this.layoutControlGroupQuestion1.BackgroundImage = bg;
                this.layoutControlGroupQuestion1.BackgroundImageVisible = true;
            }
            if (this.textEdit1 != null)
                BackColor = this.textEdit1.BackColor;
            
            this.ControlGroup = this.layoutControlGroupQuestion1;
            this.ControlGroup.Tag = this;
            isLoaded = true;
        }
        public bool Validate() 
        {
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
                    DropboxData data;
                    for (int i = Items.Count - 1; i >= 0; --i) {
                        li = Items[i];
                        if (!li.Equals(Item) && li.Name != Item.Name) {
                            if (li is LayoutControlItem) {
                                Control TempControl = (li as LayoutControlItem).Control;
                                if (TempControl != null) {
                                    data = TempControl.Tag as DropboxData;
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
                BrightVision.Common.UI.NotificationDialog.Information("Drop Box", "Questionnaire must be initialized with Json first.");
                return;
            }

            isLoaded = false;
            IList<AnswerOption> _lstAnswerOptions = Questionnaire.Form.Settings.AnswerOptions;
            IDropbox _AnswerData = null;
            DynamicControl _DynamicControl = null;
            List<string> _lstDropBoxOptions = null;

            for (int i = 0; i < _lstAnswerOptions.Count; i++) {
                _AnswerData = _lstAnswerOptions[i] as IDropbox;
                _DynamicControl = m_DynamicControls.FirstOrDefault(e => e.TextPrefix == _AnswerData.TextPrefix);
                if (_DynamicControl.Control.GetType().Name == "ComboBoxEdit") {
                    _lstDropBoxOptions = new List<string>();
                    _lstDropBoxOptions = _AnswerData.DropBoxValues;
                    ComboBoxEdit _control = _DynamicControl.Control as ComboBoxEdit;
                    if (_AnswerData.DefaultSelectionValue != null) {
                        _control.SelectedIndex = _lstDropBoxOptions.IndexOf(_AnswerData.DefaultSelectionValue.Trim());
                        _AnswerData.SelectionValue = _AnswerData.DefaultSelectionValue.Trim();
                    }
                    else
                        _control.SelectedIndex = Convert.ToInt32(_DynamicControl.DefaultValue);
                }
                if (_AnswerData.SelectionValueIfOtherVisible) {
                    _DynamicControl = m_DynamicControls.FirstOrDefault(e => e.TextPrefix == _AnswerData.TextPrefix && e.Control.GetType().Name == "TextEdit");
                    if (_DynamicControl != null) {
                        TextEdit _control = _DynamicControl.Control as TextEdit;
                        if (_AnswerData.DefaultSelectionValueIfOther != null) {
                            _control.Text = _AnswerData.DefaultSelectionValueIfOther.Trim();
                            _AnswerData.SelectionValueIfOther = this.textEdit1.Text;
                        }
                        else
                            _control.Text = _DynamicControl.DefaultValue;
                    }
                }

                //else if (_DynamicControl.Control.GetType().Name == "TextEdit") {
                //    TextEdit _control = _DynamicControl.Control as TextEdit;
                //    if (_AnswerData.DefaultSelectionValueIfOther != null) {
                //        _control.Text = _AnswerData.DefaultSelectionValueIfOther.Trim();
                //        _AnswerData.SelectionValueIfOther = this.textEdit1.Text;
                //    }
                //    else
                //        _control.Text = _DynamicControl.DefaultValue;
                //}
            }

            if (this.textEdit1 != null)
                BackColor = this.textEdit1.BackColor;

            isLoaded = true;
        }
        #endregion

        #region Private Methods
        private Image BGImage(IQuestionnaire iQuestion) 
        {            
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
        private void Reset()
        {
            foreach (DynamicControl _Control in m_DynamicControls) {
                if (_Control.GetType().Name == "ComboBoxEdit") {
                    ComboBoxEdit _control = _Control.Control as ComboBoxEdit;
                    _control.SelectedIndex = Convert.ToInt32(_Control.DefaultValue);
                }
                else if (_Control.GetType().Name == "TextEdit") {
                    TextEdit _control = _Control.Control as TextEdit;
                    _control.Text = _Control.DefaultValue;
                }
            }
        }
        #endregion

        #region Control Events
        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var objSender = sender as ComboBoxEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            if (objSender.Tag != null) {
                var data = ((DropboxData)objSender.Tag);
                var tag = data.PositionIndex.ToString();
                tag = tag.Replace("IndexPosition", "");
                var oSubQ = listSubQ[int.Parse(tag)];
                data.HasValue = false;
                if (objSender != null && objSender.SelectedItem != null) {
                    if (objSender.SelectedItem.ToString() != oSubQ.SelectionValue) {
                        oSubQ.SelectionValue = objSender.SelectedItem.ToString();
                        oSubQ.DefaultSelectionValue = oSubQ.SelectionValue;
                        if (!string.IsNullOrEmpty(oSubQ.SelectionValue) && !string.IsNullOrEmpty(oSubQ.SelectionValue.Trim()))
                            data.HasValue = true;
                    }
                }
                //if (objSender.SelectedItem.ToString() != oSubQ.SelectionValue) {
                //    oSubQ.SelectionValue = objSender.SelectedItem.ToString();
                //    oSubQ.DefaultSelectionValue = oSubQ.SelectionValue;
                //    if(!string.IsNullOrEmpty(oSubQ.SelectionValue) &&
                //        !string.IsNullOrEmpty(oSubQ.SelectionValue.Trim()))
                //        data.HasValue = true;
                //else 
                //    data.HasValue = false;
                //}
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
        private void textEdit1_TextChanged(object sender, EventArgs e)
        {
            var objSender = sender as TextEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            if (objSender.Tag != null) {
                var data = ((DropboxData)objSender.Tag);
                var tag = data.PositionIndex.ToString();
                tag = tag.Replace("IndexPosition", "");
                var oSubQ = listSubQ[int.Parse(tag)];
                oSubQ.SelectionValueIfOther = objSender.Text;
                oSubQ.DefaultSelectionValueIfOther = objSender.Text;
                data.HasValue = false;
                if (!string.IsNullOrEmpty(oSubQ.SelectionValueIfOther) && !string.IsNullOrEmpty(oSubQ.SelectionValueIfOther.Trim()))
                    data.HasValue = true;
            }
            //if (objSender.Tag != null) {
            //    var data = ((DropboxData)objSender.Tag);
            //    var tag = data.PositionIndex.ToString();
            //    tag = tag.Replace("IndexPosition", "");
            //    var oSubQ = listSubQ[int.Parse(tag)];
            //    oSubQ.SelectionValueIfOther = objSender.Text;
            //    oSubQ.DefaultSelectionValueIfOther = objSender.Text;
            //    if (!string.IsNullOrEmpty(oSubQ.SelectionValueIfOther) &&
            //        !string.IsNullOrEmpty(oSubQ.SelectionValueIfOther.Trim()))
            //        data.HasValue = true;
            //    else
            //        data.HasValue = false;
            //}

            if (!IsInitializing) {
                if (isLoaded)
                    m_bHasChanged = true;

                if (OnComponentNotifyChanged != null)
                    OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(isLoaded));
            }

            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }
        private void layoutControlGroupQuestion1_Click(object sender, EventArgs e)
        {
            LayoutControlGroup objSender = null;
            if (sender is LayoutControlGroup) {
                objSender = sender as LayoutControlGroup;
                objSender.Owner.FocusHelper.FocusElement(objSender, false);
            }
            else if (sender is SimpleLabelItem) {
                objSender = ((SimpleLabelItem)sender).Parent as LayoutControlGroup;
                objSender.Owner.FocusHelper.FocusElement(objSender, false);
            }
            else if (sender is LayoutControlItem) {
                objSender = ((LayoutControlItem)sender).Parent as LayoutControlGroup;
                objSender.Owner.FocusHelper.FocusElement(objSender, false);
            }
            else if (sender is TextEdit)
                objSender = ((LayoutControlItem)((DropboxData)((TextEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;
            else if (sender is ComboBoxEdit)
                objSender = ((LayoutControlItem)((DropboxData)((ComboBoxEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;

            if (objSender != null) {
                LayoutGroup objRootGroup = objSender.Parent;
                LayoutControlGroup objGroup;
                IQuestionnaire iQuestion;
                Image bgImage = null;
                if (objRootGroup != null)
                {
                    foreach (BaseLayoutItem item in objRootGroup.Items)
                    {
                        if (item.IsGroup)
                        {
                            objGroup = item as LayoutControlGroup;
                            iQuestion = objGroup.Tag as IQuestionnaire;
                            bgImage = BGImage(iQuestion);
                            if (!objGroup.Equals(objSender))
                            {
                                objGroup.BackgroundImage = bgImage;
                                if (bgImage != null)
                                    objGroup.BackgroundImageVisible = true;
                                else
                                    objGroup.BackgroundImageVisible = false;
                                iQuestion.Focused = false;
                            }
                            else
                            {
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
        private void Dropbox_Disposed(object sender, EventArgs e)
        {
            this.layoutControlGroupQuestion1.Click -= new EventHandler(layoutControlGroupQuestion1_Click);
            this.OnComponentNotifyChanged = null;
            ctlFooter.Parent = null;
            ctlFooter = null;
            this.ControlGroup.Tag = null;
            this.ControlGroup = null;
            this.Parent = null;
        }
        #endregion

        #region Public Event and Delegates
        public event ComponentDialogNotifyChangedEventHandler OnComponentNotifyChanged;
        #endregion
    }
}
