
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
using System.Data;
using DevExpress.Data;
using System.ComponentModel;
using System.Linq;
using DevExpress.XtraEditors.Repository;
using BrightVision.Common.Business;
using BrightVision.Model;

namespace BrightVision.DQControl.UI
{
    public class SmartText : LayoutControlGroup, IQuestionnaire
    {
        #region Private Components
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraEditors.MemoEdit memoEdit1;
        private DevExpress.XtraEditors.SimpleButton simpleButtonCancel;
        private DevExpress.XtraGrid.GridControl gridControlHistory;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewHistory;
        private DevExpress.XtraEditors.SimpleButton simpleButtonSave;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroupQuestion1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnId;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnComment;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnCreationDate;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnUser;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnCustomerContact;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repositoryItemButtonEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repositoryItemButtonEdit2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnDelete;
        private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit2;
        private DevExpress.XtraLayout.LayoutControl layoutControl2;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private Label lprefix;
        private LayoutControlItem layoutControlItemHeader;
        private LayoutControlItem layoutControlItemPrefix;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        #endregion

        #region Private Properties
        private Footer ctlFooter;
        private BindingList<SmartTextValue> gridValues;
        private bool isSampleData = false;
        private bool isLoaded = true;
        private bool isEditable;
        private int lastline = 0;
        #endregion
        
        #region Public Properties
        public event ComponentDialogNotifyChangedEventHandler OnComponentNotifyChanged;

        private bool m_bReadOnly;
        public bool ReadOnly
        {
            get {
                return m_bReadOnly;
            }
            set {
                m_bReadOnly = value;
                SetEditableGroupControls(this.layoutControlGroupQuestion1, !m_bReadOnly);
            }
        }

        public string JSONString 
        {
            get;
            set;
        }
        public string ContactName 
        { 
            get; 
            set; 
        }
        
        public bool HasMissingFields 
        {
            get;
            set;
        }
        public bool Focused 
        {
            get;
            set;
        }
        public bool DisableSelection 
        {
            get;
            set;
        }
        public bool HasChanged 
        {
            get;
            set;
        }
        public bool CanEditAllRows 
        { 
            get; 
            set; 
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

        public IStyleController StyleController
        {
            get;
            set;
        }
        public DefaultToolTipController ToolTipController
        {
            get;
            set;
        }
        public CampaignQuestionnaire Questionnaire
        {
            get;
            set;
        }
        public CampaignQuestionnaire DefaultQuestionnaire 
        { 
            get; 
            set; 
        }
        public CTScSubCampaignContactList ContactPerson 
        { 
            get; 
            set; 
        }
        public List<CTScSubCampaignContactList> ContactList 
        { 
            get; 
            set; 
        }
        public int UserId { get; set; }
        public string UserName { get; set; }

        public LayoutControlGroup ControlGroup
        {
            get;
            set;
        }
        public Footer ControlFooter
        {
            get;
            set;
        }
        private Color BackColor 
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
        public int AccountId 
        { 
            get; 
            set; 
        }
        public int ContactId { get; set; } //remove...
        #endregion

        #region Constructor
        public SmartText(IStyleController styleController): base() 
        {
            StyleController = styleController;
            this.Disposed += new EventHandler(SmartText_Disposed);
        }
        #endregion

        #region Public Methods
        public void Save()
        {
            this.SaveTextboxToGrid();
        }
        public void Render()
        {
            if (Questionnaire == null || Questionnaire.Form.Settings == null) {
                BrightVision.Common.UI.NotificationDialog.Information("Smart text", "Questionnaire must be initialized with Json first.");
                return;
            }

            if (layoutControlGroupQuestion1 == null)
                this.BindControls();

            if (this.memoEdit1 != null)
                BackColor = this.memoEdit1.BackColor;

            isLoaded = false;
            AnswerOption _AnswerOption = Questionnaire.Form.Settings.AnswerOptions[0];

            /**
             * reset controls.
             */
            this.memoEdit1.Text = string.Empty;
            gridControlHistory.DataSource = null;

            /**
             * [jeff 05.16.2012]: https://brightvision.jira.com/browse/PLATFORM-1413
             * added checking for null selected contact.
             */
            ContactName = string.Empty;
            if (ContactPerson != null)
                ContactName = string.Format("{0} {1}", ContactPerson.first_name, ContactPerson.last_name);;

            CanEditAllRows = false;
            if (UserSession.CurrentUser.IsSubCampaignManager || UserSession.CurrentUser.IsCampaignOwner)
                CanEditAllRows = true;

            /**
             * text value.
             */
            this.memoEdit1.Text = _AnswerOption.DefaultInputValue;
            
            /**
             * header text.
             */
            if (_AnswerOption.HeaderCommentText.Length > 0)
                gridColumnComment.Caption = _AnswerOption.HeaderCommentText;
            if (_AnswerOption.HeaderCreationDateText.Length > 0)
                gridColumnCreationDate.Caption = _AnswerOption.HeaderCreationDateText;
            if (_AnswerOption.HeaderUserText.Length > 0)
                gridColumnUser.Caption = _AnswerOption.HeaderUserText;
            if (_AnswerOption.HeaderCustomerContactText.Length > 0)
                gridColumnCustomerContact.Caption = _AnswerOption.HeaderCustomerContactText;
            
            /**
             * order setting.
             */
            string ordercolumn = "";
            switch (_AnswerOption.OrderBy) {
                case "Comment": ordercolumn = "Comment"; break;
                case "Creation Date": ordercolumn = "CreationDate"; break;
                case "User": ordercolumn = "User"; break;
                case "Customer Contact": ordercolumn = "CustomerContact"; break;
            }
            if (_AnswerOption.OrderDirection.Length > 0)
                gridViewHistory.Columns[ordercolumn].SortOrder = _AnswerOption.OrderDirection == "Ascending" ? ColumnSortOrder.Ascending : ColumnSortOrder.Descending;

            /**
             * grid control.
             */
            gridValues = this.ConvertToBindingList(Questionnaire.Form.Settings.AnswerOptions[0].SmartTextValues);
            if (gridValues != null && gridValues.Count > 0)
                gridControlHistory.DataSource = gridValues;

            if (!Questionnaire.Form.Settings.DataBindings.account_level) {
                gridColumnCustomerContact.Visible = false;
                gridColumnComment.Width = 270;
            }

            isLoaded = true;
        }
        public void BindControls()
        {
            InitializeComponent();
        }
        public void BindSampleDataTable()
        {
            gridValues = new BindingList<SmartTextValue>();
            gridValues.Add(
                new SmartTextValue
                {
                    Comment = "sample long text, sample long text sample long text sample long text sample long text",
                    CreationDate = "4/1/2012",
                    User = "User User",
                    UserId = -1,
                    CustomerContact = "Alfred Ango",
                    CustomerContactId = -1
                });
            gridValues.Add(
                new SmartTextValue
                {
                    Comment = "sample long text, sample long text sample long text sample long text sample long text",
                    CreationDate = "4/2/2012",
                    User = "Allan Batnam",
                    UserId = -1,
                    CustomerContact = "Tiron namo",
                    CustomerContactId = -1
                });
            gridValues.Add(
                new SmartTextValue
                {
                    Comment = "sample long text sample long text",
                    CreationDate = "4/2/2012",
                    User = "Spidi Man",
                    UserId = -1,
                    CustomerContact = "Jamie Lanister",
                    CustomerContactId = -1
                });
            gridValues.Add(
                new SmartTextValue
                {
                    Comment = "sample long text",
                    CreationDate = "3/2/2012",
                    User = "Spidi Man",
                    UserId = -1,
                    CustomerContact = "Jamie Lanister",
                    CustomerContactId = -1
                });
            //System.Data.DataTable tb = new System.Data.DataTable();
            //tb.Columns.Add("Comment");
            //tb.Columns.Add("CreationDate");
            //tb.Columns.Add("User");
            //tb.Columns.Add("CustomerContact");
            //tb.Rows.Add(new string[] { , "4/1/2012", "User User", "Alfred Ango" });
            //tb.Rows.Add(new string[] { "sample long text sample long text sample long text sample long text", "4/2/2012", "Allan Batnam", "Tiron nam" });
            //tb.Rows.Add(new string[] { "sample long text sample long text", "4/2/2012", "Spidi Man", "Jamie Lanister" });
            //tb.Rows.Add(new string[] { "sample long text", "4/2/2012", "Spidi Man", "Jamie Lanister" });
            gridControlHistory.DataSource = gridValues;
            simpleButtonSave.Enabled = false;
            simpleButtonCancel.Enabled = false;
            isSampleData = true;

        }
        public void BindQuestionnaire()
        {
            try
            {
                CampaignQuestionnaire objQuestion = JsonConvert.DeserializeObject<CampaignQuestionnaire>(JSONString,
                    new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                Questionnaire = objQuestion;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void BindPropertyGrid()
        {
            if (layoutControlGroupQuestion1 == null)
                BindControls();
            Image bg = BGImage(this);
            if (bg != null)
            {
                this.layoutControlGroupQuestion1.BackgroundImage = bg;
                this.layoutControlGroupQuestion1.BackgroundImageVisible = true;
            }
            if (this.memoEdit1 != null)
                BackColor = this.memoEdit1.BackColor;
            this.ControlGroup = this.layoutControlGroupQuestion1;
            this.ControlGroup.Tag = this;

            if (this.Questionnaire != null && this.Questionnaire.Form.Settings != null)
            {
                var answerOption = this.Questionnaire.Form.Settings.AnswerOptions[0];

                #region Prefix
                string txtPrefix = answerOption.TextPrefix;
                if (txtPrefix.Length > 0)
                {
                    this.layoutControlItemPrefix.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                    this.lprefix.Text = txtPrefix;
                }
                else
                    this.layoutControlItemPrefix.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                #endregion

                #region Text
                string txtDefaultText = answerOption.DefaultInputValue;
                this.memoEdit1.Text = txtDefaultText;
                #endregion

                #region Header Text
                if (answerOption.HeaderCommentText.Length > 0)
                    gridColumnComment.Caption = answerOption.HeaderCommentText;
                if (answerOption.HeaderCreationDateText.Length > 0)
                    gridColumnCreationDate.Caption = answerOption.HeaderCreationDateText;
                if (answerOption.HeaderUserText.Length > 0)
                    gridColumnUser.Caption = answerOption.HeaderUserText;
                if (answerOption.HeaderCustomerContactText.Length > 0)
                    gridColumnCustomerContact.Caption = answerOption.HeaderCustomerContactText;
                #endregion

                #region Order Setting
                string ordercolumn = "";
                switch (answerOption.OrderBy)
                {
                    case "Comment": ordercolumn = "Comment"; break;
                    case "Creation Date": ordercolumn = "CreationDate"; break;
                    case "User": ordercolumn = "User"; break;
                    case "Customer Contact": ordercolumn = "CustomerContact"; break;
                }
                if (answerOption.OrderDirection.Length > 0)
                    gridViewHistory.Columns[ordercolumn].SortOrder = answerOption.OrderDirection == "Ascending" ? ColumnSortOrder.Ascending : ColumnSortOrder.Descending;
                #endregion

                #region Footer
                ctlFooter.IsAccountLevel = this.Questionnaire.Form.Settings.DataBindings.account_level;
                ctlFooter.IsCustomerOwned = this.Questionnaire.Form.Settings.CustomerOwnership;
                ctlFooter.IsBrightvisionOwned = this.Questionnaire.Form.Settings.BVOwnership;
                ctlFooter.HelpText = this.Questionnaire.Form.Settings.QuestionHelp;
                ctlFooter.LanguageCode = this.Questionnaire.Form.Settings.DataBindings.language_code;
                ctlFooter.QuestionText = this.Questionnaire.Form.Settings.Label + " " + this.Questionnaire.Form.Settings.QuestionText;
                ctlFooter.InitializeFooter();
                #endregion

                #region Grid
                gridValues = ConvertToBindingList(this.Questionnaire.Form.Settings.AnswerOptions[0].SmartTextValues);
                // gridValues.ListChanged += new ListChangedEventHandler(gridValues_ListChanged);
                if (gridValues != null && gridValues.Count > 0)
                {
                    gridControlHistory.DataSource = gridValues;
                }
                #endregion

                if (!this.Questionnaire.Form.Settings.DataBindings.account_level)
                {
                    gridColumnCustomerContact.Visible = false;
                    gridColumnComment.Width = 270;
                }
            }
        }
        public bool Validate()
        {
            bool isValid = true;
            bool isRequired = Questionnaire.Form.Settings.AnswerOptions[0].Required;
            bool hasValue = gridValues.Count > 0 ? true : false;
            try
            {
                var Item = layoutControlGroupQuestion1;
                if (Item == null) return false;
                var FlatList = new DevExpress.XtraLayout.Helpers.FlatItemsList();
                List<BaseLayoutItem> Items = FlatList.GetItemsList(Item);
                ILayoutControl Layout = Item.Owner;
                if (Layout != null)
                {
                    Layout.BeginUpdate();
                    BaseLayoutItem li;

                    if (isRequired && !hasValue)
                    {
                        gridViewHistory.Appearance.Empty.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                        if (isValid) isValid = false;
                    }
                    else
                    {
                        gridViewHistory.Appearance.Empty.BackColor = this.BackColor;
                    }
                    Layout.EndUpdate();
                }

            }
            catch
            {
            }

            if (ctlFooter != null)
            {
                if (!isValid && isRequired)
                {
                    ctlFooter.SetQuestionRequired(false, true);
                }
                else if (isValid && isRequired)
                {
                    ctlFooter.SetQuestionRequired(true, true);
                }
                else if (!isRequired)
                {
                    ctlFooter.SetQuestionRequired(true, false);
                }
            }

            return isValid;
        }
        #endregion

        #region Private Methods
        private BindingList<SmartTextValue> ConvertToBindingList(List<SmartTextValue> list)
        {
            var bindingList = new BindingList<SmartTextValue>();
            if (list == null)
                return bindingList;

            foreach (var smartValue in list)
                bindingList.Add(smartValue);

            return bindingList;
        }
        private Image BGImage(IQuestionnaire iQuestion)
        {
            Image imgBGImage = null;
            if (iQuestion.Questionnaire != null && iQuestion.Questionnaire.Form.Settings != null)
            {
                Settings qSettings = iQuestion.Questionnaire.Form.Settings;
                string bgColor = qSettings.BackgroundColor.ToLower().Trim();
                if (!string.IsNullOrEmpty(bgColor))
                {
                    switch (bgColor)
                    {
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
       
        private void SetEditableGroupControls(BaseLayoutItem Item, bool isEditable)
        {
            if (Item == null) return;
            var FlatList = new DevExpress.XtraLayout.Helpers.FlatItemsList();
            List<BaseLayoutItem> Items = FlatList.GetItemsList(Item);
            ILayoutControl Layout = Item.Owner;
            if (Layout != null)
            {
                Layout.BeginUpdate();
                BaseLayoutItem li;
                for (int i = Items.Count - 1; i >= 0; --i)
                {
                    li = Items[i];
                    if (!li.Equals(Item) && li.Name != Item.Name)
                    {
                        if (li is LayoutControlItem)
                        {
                            Control TempControl = (li as LayoutControlItem).Control;
                            if (TempControl is BaseEdit)
                            {
                                (TempControl as BaseEdit).Properties.ReadOnly = !isEditable;
                                (TempControl as BaseEdit).Properties.AllowFocused = isEditable;
                            }
                           
                        }
                    }
                }
                simpleButtonCancel.Enabled = isEditable;
                simpleButtonSave.Enabled = isEditable;
                this.isEditable = isEditable;
                /**
                 * [@jeff 05.17.2012]: https://brightvision.jira.com/browse/PLATFORM-1415
                 * added read-only setting for the grid view.
                 */
                gridViewHistory.OptionsBehavior.ReadOnly = !isEditable;
                gridControlHistory.RefreshDataSource();
                Layout.EndUpdate();

                if (isEditable) {
                    HasChanged = false;
                }
            }
        }
        private void InitializeComponent()
        {
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject1 = new DevExpress.Utils.SerializableAppearanceObject();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.simpleButtonCancel = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButtonSave = new DevExpress.XtraEditors.SimpleButton();
            this.gridControlHistory = new DevExpress.XtraGrid.GridControl();
            this.gridViewHistory = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumnId = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnComment = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemMemoEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            this.repositoryItemMemoEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            this.gridColumnCreationDate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnUser = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnCustomerContact = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnDelete = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemButtonEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repositoryItemButtonEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.memoEdit1 = new DevExpress.XtraEditors.MemoEdit();
            this.layoutControlGroupQuestion1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItemHeader = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItemHeader.Name = "layoutControlItemHeader"+Guid.NewGuid().ToString();
            this.layoutControlItemPrefix = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();

            // 
            // layoutControlGroupQuestion1
            // 
            this.layoutControlGroupQuestion1.CustomizationFormText = "Root";
            this.layoutControlGroupQuestion1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
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

            #region Prefix
            lprefix = new Label();
            lprefix.Name = "lprefix";
            lprefix.Text = "";
            lprefix.Visible = false;
            this.layoutControlItemPrefix.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItemPrefix.Name = "layoutControlItemPrefix1";
            this.layoutControlItemPrefix.Control = lprefix;
            this.layoutControlItemPrefix.ShowInCustomizationForm = false;
            this.layoutControlItemPrefix.TextVisible = false;
            this.layoutControlItemPrefix.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlItemPrefix.MinSize = new Size(0, 10);
            this.layoutControlItemPrefix.MaxSize = new Size(0, 10);
            this.layoutControlItemPrefix.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutControlItemPrefix.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            #endregion

            #region Footer
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
            Settings oSettings = Questionnaire.Form.Settings;
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
            this.layoutControlItemHeader.Control = ctlFooter;
            this.layoutControlItemHeader.ShowInCustomizationForm = false;
            this.layoutControlItemHeader.TextVisible = false;
            this.layoutControlItemHeader.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlItemHeader.MinSize = new Size(0, 24);
            this.layoutControlItemHeader.MaxSize = new Size(0, 24);
            this.layoutControlItemHeader.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            this.layoutControlItemHeader.SizeConstraintsType = SizeConstraintsType.Default;
           #endregion
             // 
            // layoutControl1
            // 

            this.layoutControl1.Controls.Add(this.ctlFooter);
            this.layoutControl1.Controls.Add(this.simpleButtonCancel);
            this.layoutControl1.Controls.Add(this.simpleButtonSave);
            this.layoutControl1.Controls.Add(this.gridControlHistory);
            this.layoutControl1.Controls.Add(this.memoEdit1);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.layoutControlGroupQuestion1;
            //this.layoutControl1.Size = new System.Drawing.Size(539, 216);
            this.layoutControl1.TabIndex = 2;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // simpleButtonCancel
            // 
            this.simpleButtonCancel.Image = global::BrightVision.DQControl.Properties.Resources.cancel_16x16;
            this.simpleButtonCancel.Location = new System.Drawing.Point(498, 41);
            this.simpleButtonCancel.Margin = new System.Windows.Forms.Padding(1);
            this.simpleButtonCancel.Name = "simpleButtonCancel";
            this.simpleButtonCancel.MinimumSize = new System.Drawing.Size(22, 22);
            this.simpleButtonCancel.MaximumSize = new System.Drawing.Size(22, 22);
            this.simpleButtonCancel.ImageLocation = ImageLocation.MiddleCenter;
            this.simpleButtonCancel.StyleController = this.layoutControl1;
            this.simpleButtonCancel.TabIndex = 7;
            this.simpleButtonCancel.ToolTip = "Cancel";
            this.simpleButtonCancel.Click += new EventHandler(simpleButtonCancel_Click);
            // 
            // simpleButtonSave
            // 
            this.simpleButtonSave.Image = global::BrightVision.DQControl.Properties.Resources.save_16x16;
            this.simpleButtonSave.Location = new System.Drawing.Point(498, 7);
            this.simpleButtonSave.Margin = new System.Windows.Forms.Padding(1);
            this.simpleButtonSave.Name = "simpleButtonSave";
            this.simpleButtonSave.MinimumSize = new System.Drawing.Size(22, 22);
            this.simpleButtonSave.MaximumSize = new System.Drawing.Size(22, 22);
            this.simpleButtonSave.ImageLocation = ImageLocation.MiddleCenter;
            this.simpleButtonSave.StyleController = this.layoutControl1;
             
            this.simpleButtonSave.TabIndex = 6;
            this.simpleButtonSave.Click += new EventHandler(simpleButtonSave_Click);
            this.simpleButtonSave.ToolTip = "Save";
            // 
            // gridControl1
            // 
            this.gridControlHistory.Location = new System.Drawing.Point(7, 75);
            this.gridControlHistory.MainView = this.gridViewHistory;
            this.gridControlHistory.Name = "gridControlHistory";
            this.gridControlHistory.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemButtonEdit1,
            this.repositoryItemMemoEdit1,
            this.repositoryItemMemoEdit2,
            this.repositoryItemButtonEdit2});
            this.gridControlHistory.MinimumSize = new System.Drawing.Size(0, 150);
            this.gridControlHistory.MaximumSize = new System.Drawing.Size(0, 150);
            this.gridControlHistory.Size = new Size(0, 150);
            
            this.gridControlHistory.TabIndex = 4;
            this.gridControlHistory.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewHistory});
           
            // 
            // gridView1
            // 
            this.gridViewHistory.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumnId,
            this.gridColumnComment,
            this.gridColumnCreationDate,
            this.gridColumnUser,
            this.gridColumnCustomerContact,
            this.gridColumnDelete});
            this.gridViewHistory.GridControl = this.gridControlHistory;
            this.gridViewHistory.Name = "gridViewHistory";
            this.gridViewHistory.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gridViewHistory.OptionsView.RowAutoHeight = true;
            this.gridViewHistory.OptionsView.ShowGroupPanel = false;
            this.gridViewHistory.OptionsBehavior.ReadOnly = true;
           
            //this.gridViewHistory.VertScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Always;
            this.gridViewHistory.CustomRowCellEdit += new DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventHandler(gridViewHistory_CustomRowCellEdit);
            //
            //gridcolumn id
            //
            this.gridColumnId.Caption = "Id";
            this.gridColumnId.FieldName = "id";
            this.gridColumnId.Name = "gridColumnId";
            this.gridColumnId.Visible = false;

            // 
            // gridColumn1
            // 
            this.gridColumnComment.Caption = "Comment";
            this.gridColumnComment.ColumnEdit = this.repositoryItemMemoEdit1;
            this.gridColumnComment.FieldName = "Comment";
            this.gridColumnComment.Name = "gridColumnComment";
            this.gridColumnComment.Visible = true;
            this.gridColumnComment.VisibleIndex = 0;
            this.gridColumnComment.Width = 200;
            this.gridColumnComment.MaxWidth = 0;
            this.gridColumnComment.MinWidth = 0;
            this.gridColumnComment.ColumnEdit.EditValueChanged += new EventHandler(ColumnEdit_EditValueChanged);
            // 
            // repositoryItemMemoEdit1
            // 
            this.repositoryItemMemoEdit1.Name = "repositoryItemMemoEdit1";
            this.repositoryItemMemoEdit1.ReadOnly = false;
            this.repositoryItemMemoEdit2.Name = "repositoryItemMemoEdit2";
            this.repositoryItemMemoEdit2.ReadOnly = true;
            // 
            // gridColumn2
            // 
            this.gridColumnCreationDate.Caption = "Date";
            this.gridColumnCreationDate.FieldName = "CreationDate";
            this.gridColumnCreationDate.Name = "gridColumnCreationDate";
            this.gridColumnCreationDate.Visible = true;
            this.gridColumnCreationDate.VisibleIndex = 1;
            this.gridColumnCreationDate.Width = 50;
            this.gridColumnCreationDate.MaxWidth = 0;
            this.gridColumnCreationDate.MinWidth = 0;
            this.gridColumnCreationDate.DisplayFormat.FormatType = FormatType.Custom;
            this.gridColumnCreationDate.DisplayFormat.Format = new DateCustomFormatter();
            this.gridColumnCreationDate.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm";
            this.gridColumnCreationDate.OptionsColumn.ReadOnly = true;
            // 
            // gridColumn3
            // 
            this.gridColumnUser.Caption = "User";
            this.gridColumnUser.FieldName = "User";
            this.gridColumnUser.Name = "gridColumnUser";
            this.gridColumnUser.Visible = true;
            this.gridColumnUser.VisibleIndex = 2;
            this.gridColumnUser.Width = 70;
            this.gridColumnUser.MaxWidth = 0;
            this.gridColumnUser.MinWidth = 0;
            this.gridColumnUser.OptionsColumn.ReadOnly = true;
            // 
            // gridColumn4
            // 
            this.gridColumnCustomerContact.Caption = "Contact";
            this.gridColumnCustomerContact.FieldName = "CustomerContact";
            this.gridColumnCustomerContact.Name = "gridColumnCustomerContact";
            this.gridColumnCustomerContact.Visible = true;
            this.gridColumnCustomerContact.VisibleIndex = 3;
            this.gridColumnCustomerContact.Width = 70;
            this.gridColumnCustomerContact.MaxWidth = 0;
            this.gridColumnCustomerContact.MinWidth = 0;
         
            this.gridColumnCustomerContact.OptionsColumn.ReadOnly = true;
            // 
            // gridColumn5
            // 
            this.gridColumnDelete.Caption = "";
            this.gridColumnDelete.ColumnEdit = this.repositoryItemButtonEdit1;
            this.gridColumnDelete.FieldName = "";
            this.gridColumnDelete.MaxWidth = 30;
            this.gridColumnDelete.Name = "gridColumnDelete";
            this.gridColumnDelete.Visible = true;
            this.gridColumnDelete.VisibleIndex = 4;
            this.gridColumnDelete.Width = 30;
        
            this.gridColumnDelete.OptionsColumn.ReadOnly = true;
            // 
            // repositoryItemButtonEdit1
            // 
            Image img = ((System.Drawing.Image)(global::BrightVision.DQControl.Properties.Resources.cancel_16x16));
            var btn = new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, img, null, null, "Delete");
            
            this.repositoryItemButtonEdit1.Buttons.Add(btn);
            this.repositoryItemButtonEdit1.Name = "repositoryItemButtonEdit1";
            this.repositoryItemButtonEdit1.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
            //this.repositoryItemButtonEdit1.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(repositoryItemButtonDelete_ButtonClick);
            this.repositoryItemButtonEdit1.Click += new EventHandler(repositoryItemButtonDelete_Click);
            //btn.Appearance.Image = img;

            this.repositoryItemButtonEdit2.Buttons.Add(btn);
            this.repositoryItemButtonEdit2.Name = "repositoryItemButtonEdit1";
            this.repositoryItemButtonEdit2.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
            this.repositoryItemButtonEdit2.ReadOnly = true;
            //
            // memoEdit1
            //
            this.memoEdit1.Location = new System.Drawing.Point(7, 7);
            this.memoEdit1.Name = "memoEdit1";
            //this.memoEdit1.MinimumSize = new Size(100, 52);
            //this.memoEdit1.MaximumSize = new Size(0,0);
            this.memoEdit1.StyleController = this.layoutControl1;
            this.memoEdit1.TabIndex = 8;
            this.memoEdit1.Properties.ScrollBars = ScrollBars.None;
            this.memoEdit1.TextChanged += new EventHandler(memoEdit1_TextChanged);
            
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.CustomizationFormText = "emptySpaceItem1";
            this.emptySpaceItem1.Location = new System.Drawing.Point(383, 52);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(22, 212);
            this.emptySpaceItem1.Text = "emptySpaceItem1";
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            //
            //gridControl
            //
            this.layoutControlItem1.Control = this.gridControlHistory;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 68);
            //this.layoutControlItem1.MinSize = new System.Drawing.Size(0, 100);
            //this.layoutControlItem1.MaxSize = new System.Drawing.Size(0, 200);
            this.Size = new Size(0, 150);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.SupportHorzAlignment;
            this.layoutControlItem1.ControlAlignment = ContentAlignment.BottomCenter;
            //this.layoutControlItem1.Size = new Size(0, 150);
            this.layoutControlItem1.Text = "layoutControlItem1";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextToControlDistance = 0;
            this.layoutControlItem1.TextVisible = false;
             // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.simpleButtonSave;
            this.layoutControlItem3.CustomizationFormText = "layoutControlItem3";
            this.layoutControlItem3.Location = new System.Drawing.Point(491, 0);
            this.layoutControlItem3.MaxSize = new System.Drawing.Size(22, 22);
            this.layoutControlItem3.MinSize = new System.Drawing.Size(22, 22);
            this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(1);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Default;
            this.layoutControlItem3.Text = "layoutControlItem3";
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextToControlDistance = 0;
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.simpleButtonCancel;
            this.layoutControlItem4.CustomizationFormText = "layoutControlItem4";
            this.layoutControlItem4.Location = new System.Drawing.Point(491, 34);
            this.layoutControlItem4.MaxSize = new System.Drawing.Size(22, 102);
            this.layoutControlItem4.MinSize = new System.Drawing.Size(22, 22);
            this.layoutControlItem4.Size = new Size(22, 102);
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(1);
 
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Default;
            this.layoutControlItem4.Text = "layoutControlItem4";
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextToControlDistance = 0;
            this.layoutControlItem4.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.memoEdit1;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Text = "layoutControlItem2";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.MinSize = new Size(0, 24);
            this.layoutControlItem1.MaxSize = new Size(0, 24);
            this.layoutControlItem2.TextToControlDistance = 0;
            this.layoutControlItem2.TextVisible = false;
            this.layoutControlItem2.SizeConstraintsType = SizeConstraintsType.SupportHorzAlignment;
            // 
            // testinglayout
            // 
            this.layoutControlGroupQuestion1.AddItem(this.layoutControlItemHeader);
            this.layoutControlGroupQuestion1.AddItem(this.layoutControlItemPrefix);
            this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem2);
            this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem3, this.layoutControlItem2, DevExpress.XtraLayout.Utils.InsertType.Right);
            this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem4, this.layoutControlItem3, DevExpress.XtraLayout.Utils.InsertType.Bottom);
            this.layoutControlGroupQuestion1.AddItem(this.layoutControlItem1);
 
            this.layoutControlGroupQuestion1.Click += new EventHandler(layoutControlGroupQuestion1_Click);
            
            this.layoutControlGroupQuestion1.EndUpdate();
        }
        private void SaveTextboxToGrid()
        {
            if (isSampleData)
                return;

            if (gridValues == null)
                gridValues = new BindingList<SmartTextValue>();

            string txt = this.memoEdit1.Text;
            if (txt.Length > 0) {
                SmartTextValue val = new SmartTextValue();
                if (gridValues.Count == 0)
                    val.id = 1;
                else
                    val.id = gridValues.Max(param => param.id) + 1;

                val.Comment = txt;
                val.CreationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                Questionnaire.Form.Settings.DataBindings.contact_id = null;
                if (ContactPerson != null)
                    Questionnaire.Form.Settings.DataBindings.contact_id = ContactPerson.id.ToString();

                val.CustomerContactId = 0;
                val.CustomerContact = string.Empty;

                if (Questionnaire.Form.Settings.DataBindings.account_level) {
                    if (string.IsNullOrEmpty(Questionnaire.Form.Settings.DataBindings.contact_id) || Convert.ToInt32(Questionnaire.Form.Settings.DataBindings.contact_id) < 1)
                        Questionnaire.Form.Settings.DataBindings.contact_id = null;

                    if (!string.IsNullOrEmpty(Questionnaire.Form.Settings.DataBindings.contact_id)) {
                        val.CustomerContactId = int.Parse(Questionnaire.Form.Settings.DataBindings.contact_id);
                        if (ContactPerson != null)
                            val.CustomerContact = string.Format("{0} {1}", ContactPerson.first_name, ContactPerson.last_name);
                    }
                }

                val.User = UserName;
                val.UserId = UserId;
                gridValues.Add(val);

                if (gridControlHistory.DataSource == null)
                    gridControlHistory.DataSource = gridValues;

                /**
                 * [@jeff 05.16.2012]: https://brightvision.jira.com/browse/PLATFORM-1411
                 * added logic to set focus on the newly added comment.
                 */
                for (int i = 0; i < gridViewHistory.RowCount; i++) {
                    if (Convert.ToInt32(gridViewHistory.GetRowCellValue(i, "id")) == val.id) {
                        gridViewHistory.FocusedRowHandle = i;
                        break;
                    }
                }

                this.memoEdit1.Text = "";
                this.SaveToAnswerForm();

                //if (!IsInitializing) {
                //    if (OnComponentNotifyChanged != null)
                //        OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(true));

                //    this.HasChanged = true;
                //}

                //if (ControlGroup != null)
                //    ControlGroup.Tag = this;

            }
        }
        private void SaveToAnswerForm() 
        {
            Questionnaire.Form.Settings.AnswerOptions[0].SmartTextValues = gridValues.ToArray().ToList();

            if (!IsInitializing) {
                if (OnComponentNotifyChanged != null)
                    OnComponentNotifyChanged(this, new ComponentNotifyChangedArgs(true));
                this.HasChanged = true;
            }

            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }
        private void layoutControlGroupQuestion1_Click(object sender, EventArgs e)
        {
            LayoutControlGroup objSender = null;
            if (sender is LayoutControlGroup)
            {
                objSender = sender as LayoutControlGroup;
                objSender.Owner.FocusHelper.FocusElement(objSender, false);
            }
            else if (sender is SimpleLabelItem)
            {
                objSender = ((SimpleLabelItem)sender).Parent as LayoutControlGroup;
                objSender.Owner.FocusHelper.FocusElement(objSender, false);
            }
            else if (sender is LayoutControlItem)
            {
                objSender = ((LayoutControlItem)sender).Parent as LayoutControlGroup;
                objSender.Owner.FocusHelper.FocusElement(objSender, false);
            }
            //else if (sender is TextEdit)
            //{
            //    objSender =
            //        ((LayoutControlItem)((DropboxData)((TextEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;
            //}
            //else if (sender is ComboBoxEdit)
            //{
            //    objSender =
            //        ((LayoutControlItem)((DropboxData)((ComboBoxEdit)sender).Tag).ControlContainer).Parent as LayoutControlGroup;
            //}
            if (objSender != null)
            {
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
                if (!DisableSelection)
                {
                    this.layoutControlGroupQuestion1.BackgroundImage = Properties.Resources.bg_selector;
                    this.layoutControlGroupQuestion1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                }
                objSender.BackgroundImageVisible = true;
            }
            if (ControlGroup != null)
                ControlGroup.Tag = this;
        }
        #endregion

        #region Control Events
        private void gridValues_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (isSampleData)
                return;

            this.SaveToAnswerForm();
        }
        private void repositoryItemButtonDelete_Click(object sender, EventArgs e)
        {
            if (isSampleData || !this.isEditable)
                return;

            if (gridViewHistory.RowCount > 0) {
                int id = int.Parse(gridViewHistory.GetRowCellValue(gridViewHistory.FocusedRowHandle, "id").ToString());
                var val = gridValues.Where(param => param.id == id).FirstOrDefault();
                if (val == null)
                    return;

                DialogResult diagResult = MessageBox.Show("Want to remove this item?", "Remove", MessageBoxButtons.OKCancel);
                if (diagResult == DialogResult.OK) {
                    int index = gridValues.IndexOf(val);
                    gridValues.RemoveAt(index);
                    SaveToAnswerForm();
                }
            }
        }
        private void ColumnEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (isSampleData)
                return;

            this.SaveToAnswerForm();
        }
        private void gridViewHistory_CustomRowCellEdit(object sender, DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventArgs e)
        {
            if (!this.isEditable || this.isSampleData) {
                if (e.Column.FieldName == "Comment")
                    e.RepositoryItem = repositoryItemMemoEdit2;
                else if (e.Column.Name == "gridColumnDelete")
                    e.RepositoryItem = repositoryItemButtonEdit2;

                return;
            }

            if (gridValues.Count < 1)
                return;

            var accountLevel = Questionnaire.Form.Settings.DataBindings.account_level;
            int rowHandle = e.RowHandle;
            int id = int.Parse(gridViewHistory.GetRowCellValue(rowHandle, "id").ToString());
            int rowUserId = gridValues.Where(param => param.id == id).First().UserId;

            if (e.Column.FieldName == "Comment") {
                if (CanEditAllRows)
                    e.RepositoryItem = repositoryItemMemoEdit1;
                else if (rowUserId == UserId)
                    e.RepositoryItem = repositoryItemMemoEdit1;
                else
                    e.RepositoryItem = repositoryItemMemoEdit2;
            }
            else if (e.Column.Name == "gridColumnDelete") {
                if (CanEditAllRows)
                    e.RepositoryItem = repositoryItemButtonEdit1;
                else if (rowUserId == UserId)
                    e.RepositoryItem = repositoryItemButtonEdit1;
                else
                    e.RepositoryItem = repositoryItemButtonEdit2;
            }
        }
        private void memoEdit1_TextChanged(object sender, EventArgs e)
        {
            try {
                if (lastline != memoEdit1.Lines.Length)
                    lastline = memoEdit1.Lines.Length;

                else if (memoEdit1.Lines[memoEdit1.Lines.Length - 1].Length > 110) {
                    // put codes here ...
                }
                else
                    return;

                LayoutControlItem lci = layoutControlItem2;
                var memoEditor = sender as MemoEdit;
                if (memoEditor != null) {
                    var vi = memoEditor.GetViewInfo() as MemoEditViewInfo;
                    TextBoxMaskBox maskbox = memoEditor.MaskBox;
                    int h = maskbox.GetPreferredSize(new Size(vi.MaskBoxRect.Width, int.MaxValue)).Height;
                    if (memoEditor.Text.Length > 0) {
                        memoEditor.MinimumSize = new Size(0, h + 9);
                        lci.MinSize = new Size(lci.MinSize.Width, h + 10);
                        lci.Update();
                    }
                }

            }
            catch { 
            }
        }
        private void repositoryItemButtonDelete_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (isSampleData || !this.isEditable)
                return;

            if (gridViewHistory.RowCount > 0) {
                int id = int.Parse(gridViewHistory.GetRowCellValue(gridViewHistory.FocusedRowHandle, "id").ToString());
                var val = gridValues.Where(param => param.id == id).FirstOrDefault();
                if (val == null)
                    return;

                DialogResult diagResult = MessageBox.Show("Want to remove this item?", "Remove", MessageBoxButtons.OKCancel);
                if (diagResult == DialogResult.OK) {
                    int index = gridValues.IndexOf(val);
                    gridValues.RemoveAt(index);
                    SaveToAnswerForm();
                }
            }
        }
        private void simpleButtonCancel_Click(object sender, EventArgs e)
        {
            this.memoEdit1.Text = "";
        }
        private void simpleButtonSave_Click(object sender, EventArgs e)
        {
            this.SaveTextboxToGrid();
        }
        private void SmartText_Disposed(object sender, EventArgs e)
        {
            this.layoutControlItemHeader.Click -= new EventHandler(layoutControlGroupQuestion1_Click);
            this.layoutControlGroupQuestion1.Click -= new EventHandler(layoutControlGroupQuestion1_Click);
            this.OnComponentNotifyChanged = null;
            ctlFooter.Parent = null;
            ctlFooter = null;
            this.ControlGroup.Tag = null;
            this.ControlGroup = null;
            this.Parent = null;
        }
        #endregion
    }

    #region Custom Formatter Class
    class DateCustomFormatter : IFormatProvider, ICustomFormatter
    {
        // The GetFormat method of the IFormatProvider interface.
        // This must return an object that provides formatting services for the specified type.
        public object GetFormat(System.Type type)
        {
            return this;
        }
        // The Format method of the ICustomFormatter interface.
        // This must format the specified value according to the specified format settings.
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            string formatValue = arg.ToString();
            try {
                return DateTime.Parse(formatValue).ToString(format);
            }
            catch {
                /**
                 * temporary fix for date formats with this kind of signature: 16-04-2013 09:07:25
                 * in this sample, the format was in DMY which caused the date parsing error.
                 * we will try to reformat this date to YMD.
                 */
                string[] _Unformatted = formatValue.Split(' ');
                string[] _DateParts = _Unformatted[0].Split('-');
                string _Formatted = string.Format("{0}-{1}-{2} {3}", _DateParts[2], _DateParts[1], _DateParts[0], _Unformatted[1]);

                return DateTime.Parse(_Formatted).ToString(format);
            }
        }
    }
    #endregion
}
