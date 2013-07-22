using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraLayout;

using BrightVision.DQControl.Business;
using Newtonsoft.Json;
using BrightVision.Model;

namespace BrightVision.DQControl.UI {
    /// <summary>
    /// Represents the Textbox Type of Questionnaire
    /// </summary>
    public class Textbox : LayoutControlGroup, IQuestionnaire {

        #region Member Variables
        private LayoutControlGroup layoutControlGroupQuestion1;
        private MemoEdit memoEdit1;
        private EmptySpaceItem emptySpaceItem1;
        private EmptySpaceItem emptySpaceItem2;     
        private LayoutControlItem layoutControlItem1;
        private Footer ctlFooter;
        private bool isLoaded;
        private bool isResizedEvent;

        private List<DynamicControl> m_DynamicControls = null;
        private class DynamicControl {
            public string DefaultValue { get; set; }
        }
        #endregion

        #region Constructors
        public Textbox(IStyleController styleController)
            : base() {
                InitializeComponent();
                StyleController = styleController;
                this.Disposed += new EventHandler(Textbox_Disposed);
        }

        void Textbox_Disposed(object sender, EventArgs e)
        {
            this.layoutControlGroupQuestion1.Click -= new EventHandler(layoutControlGroupQuestion1_Click);
            this.OnComponentNotifyChanged = null;
            ctlFooter.Parent = null;
            ctlFooter = null;
            this.ControlGroup.Tag = null;
            this.ControlGroup = null;
            this.Parent = null;
        }
       

        public Textbox(IStyleController styleController, string script)
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
                MessageBox.Show("Questionnaire must be bind with JSON first.", "Textbox Component");

            m_DynamicControls = new List<DynamicControl>();

            #region Layout Control Group Initialize
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
            
            #endregion

            #region Footer
            this.layoutControlItem1 = new LayoutControlItem();
            this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();

            //bool isCustomerOwnershipOnly = false;
            //if (oSettings.CustomerOwnership && oSettings.BVOwnership)
            //    isCustomerOwnershipOnly = true;

            //ctlFooter = new Footer() {
            //   IsAccountLevel = oSettings.DataBindings.account_level,
            //   IsCustomerOwnershipOnly = isCustomerOwnershipOnly,
            //   HelpText = oSettings.QuestionHelp,
            //   LanguageCode = oSettings.DataBindings.language_code
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

            #region Answer Option Binding
            IList<AnswerOption> answeroptionList = Questionnaire.Form.Settings.AnswerOptions;
            ITextbox answeroption = null;
            int iAnswerOptions = answeroptionList.Count;
            //System.Drawing.Size newSize;            
            for (int x = 0; x < iAnswerOptions; ++x) {
                answeroption = answeroptionList[x] as ITextbox;
                // layoutControlItem1                                                         
                this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
                //add textEdit1 to layout
                this.memoEdit1 = new MemoEdit();
                this.memoEdit1.Tag = new TextboxData() {
                    PositionIndex = "IndexPosition" + x,
                    ControlContainer = layoutControlItem1,
                    Required = answeroption.Required,
                    HasValue = !string.IsNullOrWhiteSpace(answeroption.DefaultInputValue) ? true : false,
                    AnswerOption = answeroption
                };
                this.memoEdit1.Name = "textEdit" + Guid.NewGuid().ToString();
                this.memoEdit1.Properties.ScrollBars = ScrollBars.None;
                this.memoEdit1.StyleController = this.StyleController;
                //set value 

                string _DefaultValue = string.Empty;
                if (answeroption.DefaultInputValue != null) {
                    _DefaultValue = answeroption.DefaultInputValue.Trim();
                    this.memoEdit1.Text = answeroption.DefaultInputValue.Trim();
                    answeroption.InputValue = answeroption.DefaultInputValue.Trim();
                }

                m_DynamicControls.Add(new DynamicControl() {
                    DefaultValue = _DefaultValue
                });

                memoEdit1.TextChanged += new EventHandler(textEdit1_TextChanged);
                memoEdit1.Resize += new EventHandler(memoEdit1_Resize);
                memoEdit1.Click += new EventHandler(layoutControlGroupQuestion1_Click);

                this.layoutControlItem1.Name = "layoutControlItem" + Guid.NewGuid().ToString();
                this.layoutControlItem1.Control = this.memoEdit1;
                //int width = 1000, height = 1000;
                //if (!string.IsNullOrEmpty(answeroption.TextboxMaxWidth)) {
                //    int.TryParse(answeroption.TextboxMaxWidth, out width);
                //}
                //if (!string.IsNullOrEmpty(answeroption.TextboxMaxHeight)) {
                //    int.TryParse(answeroption.TextboxMaxHeight, out height);
                //}
                if (!string.IsNullOrEmpty(answeroption.TextPrefix)) {
                    this.layoutControlItem1.Text = answeroption.TextPrefix;
                    this.layoutControlItem1.TextVisible = true;
                    this.layoutControlItem1.TextLocation = DevExpress.Utils.Locations.Top;
                    this.layoutControlItem1.MinSize = new Size(0, 41);
                    this.layoutControlItem1.MaxSize = new Size(0, 41);
                } else {
                    this.layoutControlItem1.TextVisible = false;
                    this.layoutControlItem1.MinSize = new Size(0, 24);
                    this.layoutControlItem1.MaxSize = new Size(0, 24);
                }
                //newSize = new System.Drawing.Size(width, height);
                this.layoutControlItem1.SizeConstraintsType = SizeConstraintsType.Custom;
                this.layoutControlItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
                this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1);
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
            //this.emptySpaceItem1.Text = prioText;
            //this.emptySpaceItem1.TextVisible = true;            
            //this.emptySpaceItem1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            //this.emptySpaceItem1.SizeConstraintsType = SizeConstraintsType.Custom;
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
                    TextboxData data;
                    for (int i = Items.Count - 1; i >= 0; --i) {
                        li = Items[i];
                        if (!li.Equals(Item) && li.Name != Item.Name) {
                            if (li is LayoutControlItem) {
                                Control TempControl = (li as LayoutControlItem).Control;
                                if (TempControl != null) {
                                    data = TempControl.Tag as TextboxData;
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
                } else if(!isRequiredAll){
                    ctlFooter.SetQuestionRequired(true, false);
                }
            }

            return isValid;
        }
        public void Render()
        {
            if (Questionnaire == null || Questionnaire.Form.Settings == null) {
                BrightVision.Common.UI.NotificationDialog.Information("Text Box", "Questionnaire must be initialized with Json first.");
                return;
            }

            isLoaded = false;
            AnswerOption _AnswerOption = Questionnaire.Form.Settings.AnswerOptions[0];
            this.SuspendEvents(true);
            
            this.memoEdit1.Text = string.Empty;
            if (_AnswerOption.DefaultInputValue != null) {
                this.memoEdit1.Text = _AnswerOption.DefaultInputValue.Trim();
                _AnswerOption.InputValue = _AnswerOption.DefaultInputValue.Trim();
            }
            else
                this.memoEdit1.Text = m_DynamicControls[0].DefaultValue;

            if (this.memoEdit1 != null)
                BackColor = this.memoEdit1.BackColor;

            isLoaded = true;
            this.SuspendEvents(false);
        }
        #endregion        

        #region Private Methods
        private void SuspendEvents(bool pSuspentEvents)
        {
            if (pSuspentEvents) {
                memoEdit1.TextChanged -= new EventHandler(textEdit1_TextChanged);
            }
            else {
                memoEdit1.TextChanged += new EventHandler(textEdit1_TextChanged);
            }
        }

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
       
        private void textEdit1_TextChanged(object sender, EventArgs e) {
            var objSender = sender as TextEdit;
            var listSubQ = Questionnaire.Form.Settings.AnswerOptions;
            TextboxData data = null;
            if (objSender.Tag != null) {
                data = (TextboxData)objSender.Tag;
                var tag = data.PositionIndex.ToString();
                tag = tag.Replace("IndexPosition", "");
                var oSubQ = listSubQ[int.Parse(tag)];
                oSubQ.InputValue = objSender.Text;
                oSubQ.DefaultInputValue = oSubQ.InputValue;
                if (!string.IsNullOrEmpty(oSubQ.InputValue) && !string.IsNullOrEmpty(oSubQ.InputValue.Trim()))
                    data.HasValue = true;
                else 
                    data.HasValue = false;
            }
            try {
                LayoutControlItem lci = ((LayoutControlItem)((TextboxData)((TextEdit)sender).Tag).ControlContainer);
                var memoEditor = sender as MemoEdit;
                if (memoEditor != null) {                    
                    var vi = memoEditor.GetViewInfo() as MemoEditViewInfo;  
                    TextBoxMaskBox maskbox = memoEditor.MaskBox;
                    int h = maskbox.GetPreferredSize(new Size(vi.MaskBoxRect.Width, int.MaxValue)).Height;
                    if (data != null && data.AnswerOption != null && !string.IsNullOrEmpty(data.AnswerOption.TextPrefix)) {
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

            if(ControlGroup != null)
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
                    ((LayoutControlItem)((TextboxData)((TextEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;
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
        private class TextboxData {
            public string PositionIndex { get; set; }
            public object ControlContainer { get; set; }
            public bool Required { get; set; }
            public bool HasValue { get; set; }
            public ITextbox AnswerOption { get; set; }
        } 
        #endregion

        #region IQuestionnaire Members


        public void Save()
        {
            
        }
        

        #endregion
    }
}
