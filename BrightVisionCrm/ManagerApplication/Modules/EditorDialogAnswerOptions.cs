using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraVerticalGrid;
using DevExpress.XtraVerticalGrid.Rows;

using BrightVision.DQControl.Business;
using BrightVision.Common.Utilities;
using ManagerApplication.Business;

namespace ManagerApplication.Modules {
    public partial class EditorDialogAnswerOptions : DevExpress.XtraEditors.XtraForm {

        #region Member Variables
        private IList<AnswerOption> answerOptionList = null;
        private PropertyGridControl sourcePG = null;
        private CampaignQuestionnaire campaignQuestionnaire = null;
        private EditorDialogOtherChoices editorOtherChoice = null;
        private EditorDialogMultipleChoiceValues editorMultipleChoice = null; 
        #endregion
        
        #region Constructor
        public EditorDialogAnswerOptions(CampaignQuestionnaire oQuestionnaire, PropertyGridControl sourcePropertyGrid) {
            InitializeComponent();
            this.KeyPreview = true;
            sourcePG = sourcePropertyGrid;
            campaignQuestionnaire = oQuestionnaire;
            answerOptionList = campaignQuestionnaire.Form.Settings.AnswerOptions;
            CreateEditorRows();
        } 
        #endregion

        #region Properties
        
        public int SelectedIndex { get; set; }

        public NewQuestions Sender { get; set; } 
        #endregion

        #region Methods
        private void CreateEditorRows() {
            if (campaignQuestionnaire != null) {
                if (campaignQuestionnaire.Type.ToLower() == QuestionTypeConstants.Dropbox) {
                    EditorRow oRow = new EditorRow("TextPrefix");
                    oRow.Properties.Caption = "TextPrefix";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("DropBoxValues");
                    oRow.Properties.Caption = "DropBoxValues";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("DefaultSelectionValue");
                    oRow.Properties.Caption = "DefaultSelectionValue";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("SelectionValueRequired");
                    oRow.Properties.Caption = "SelectionValueRequired";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxSelectionValueRequired = new RepositoryItemComboBox();
                    repositoryItemComboBoxSelectionValueRequired.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxSelectionValueRequired.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxSelectionValueRequired;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("DefaultSelectionValueIfOther");
                    oRow.Properties.Caption = "DefaultSelectionValueIfOther";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("SelectionValueIfOtherRequired");
                    oRow.Properties.Caption = "SelectionValueIfOtherRequired";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxSelectionValueIfOtherRequired = new RepositoryItemComboBox();
                    repositoryItemComboBoxSelectionValueIfOtherRequired.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxSelectionValueIfOtherRequired.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxSelectionValueIfOtherRequired;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("SelectionValueIfOtherVisible");
                    oRow.Properties.Caption = "SelectionValueIfOtherVisible";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxSelectionValueVisible = new RepositoryItemComboBox();
                    repositoryItemComboBoxSelectionValueVisible.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxSelectionValueVisible.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxSelectionValueVisible;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("DropboxMaxWidth");
                    oRow.Properties.Caption = "DropboxMaxWidth";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    listBoxControlMembers.DataSource = answerOptionList;
                    listBoxControlMembers.DisplayMember = "TextPrefix";
                } else if (campaignQuestionnaire.Type.ToLower() == QuestionTypeConstants.MultipleChoice) {
                    EditorRow oRow = new EditorRow("MultipleChoiceColumns");
                    oRow.Properties.Caption = "MultipleChoiceColumns";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("MultipleChoiceValues");
                    oRow.Properties.Caption = "MultipleChoiceValues";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemButtonEdit repositoryItemButtonEditMultipleChoiceValues = new RepositoryItemButtonEdit();
                    repositoryItemButtonEditMultipleChoiceValues.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    repositoryItemButtonEditMultipleChoiceValues.Click += new EventHandler(repositoryItemButtonEditMultipleChoiceValues_ButtonClick);
                    repositoryItemButtonEditMultipleChoiceValues.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(repositoryItemButtonEditMultipleChoiceValues_CustomDisplayText);
                    oRow.Properties.RowEdit = repositoryItemButtonEditMultipleChoiceValues;
                    propertyGridControl1.Rows.Add(oRow);
                    propertyGridControl1.DefaultEditors.Add(typeof(List<MultipleChoiceValue>), repositoryItemButtonEditMultipleChoiceValues);


                    oRow = new EditorRow("MultipleChoiceRequired");
                    oRow.Properties.Caption = "MultipleChoiceRequired";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxMultipleChoiceRequired = new RepositoryItemComboBox();
                    repositoryItemComboBoxMultipleChoiceRequired.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxMultipleChoiceRequired.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxMultipleChoiceRequired;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("OtherChoices");
                    oRow.Properties.Caption = "OtherChoices";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemButtonEdit repositoryItemButtonEditOtherChoices = new RepositoryItemButtonEdit();
                    repositoryItemButtonEditOtherChoices.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    repositoryItemButtonEditOtherChoices.Click += new EventHandler(repositoryItemButtonEditOtherChoices_ButtonClick);
                    repositoryItemButtonEditOtherChoices.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(repositoryItemButtonEditOtherChoices_CustomDisplayText);
                    oRow.Properties.RowEdit = repositoryItemButtonEditOtherChoices;
                    propertyGridControl1.Rows.Add(oRow);
                    propertyGridControl1.DefaultEditors.Add(typeof(List<OtherChoice>), repositoryItemButtonEditOtherChoices);
                    listBoxControlMembers.DataSource = answerOptionList;
                    listBoxControlMembers.DisplayMember = "MultipleChoiceValues";
                    simpleButtonAdd.Enabled = false;
                    simpleButtonRemove.Enabled = false;
                    simpleButtonClear.Enabled = false;
                    simpleButtonMoveUp.Enabled = false;
                    simpleButtonMoveDown.Enabled = false;

                } else if (campaignQuestionnaire.Type.ToLower() == QuestionTypeConstants.Textbox) {
                    EditorRow oRow = new EditorRow("TextPrefix");
                    oRow.Properties.Caption = "TextPrefix";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("DefaultInputValue");
                    oRow.Properties.Caption = "DefaultInputValue";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("Required");
                    oRow.Properties.Caption = "Required";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxTextboxRequired = new RepositoryItemComboBox();
                    repositoryItemComboBoxTextboxRequired.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxTextboxRequired.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxTextboxRequired;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("TextboxMaxWidth");
                    oRow.Properties.Caption = "TextboxMaxWidth";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("TextboxMaxHeight");
                    oRow.Properties.Caption = "TextboxMaxHeight";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    listBoxControlMembers.DataSource = answerOptionList;
                    listBoxControlMembers.DisplayMember = "TextPrefix";

                } else if (campaignQuestionnaire.Type.ToLower() == QuestionTypeConstants.Schedule) {
                    EditorRow oRow = new EditorRow("ViewDetailSummaryButtonLabel");
                    oRow.Properties.Caption = "ViewDetailSummaryButtonLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("CreateMeetingButtonLabel");
                    oRow.Properties.Caption = "CreateMeetingButtonLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("ScheduleType");
                    oRow.Properties.Caption = "ScheduleType";
                    oRow.Properties.ReadOnly = true;

                    EditorRow oChildRow = new EditorRow("ScheduleType.TextPrefix");
                    oChildRow.Properties.Caption = "TextPrefix";
                    oChildRow.Properties.ReadOnly = false;
                    oRow.ChildRows.Add(oChildRow);
                    oChildRow = new EditorRow("ScheduleType.ScheduleTypeSelectedValue");
                    oChildRow.Properties.Caption = "ScheduleTypeSelectedValue";
                    oChildRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxSelValues = new RepositoryItemComboBox();
                    repositoryItemComboBoxSelValues.Items.AddRange(new object[] {"Seminar", "Webinar", "Meeting" });
                    repositoryItemComboBoxSelValues.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oChildRow.Properties.RowEdit = repositoryItemComboBoxSelValues;
                    oRow.ChildRows.Add(oChildRow);                    
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("ScheduleSalesPerson");
                    oRow.Properties.Caption = "ScheduleSalesPerson";
                    oRow.Properties.ReadOnly = true;
                    oChildRow = new EditorRow("ScheduleSalesPerson.TextPrefix");
                    oChildRow.Properties.Caption = "TextPrefix";
                    oChildRow.Properties.ReadOnly = false;
                    oRow.ChildRows.Add(oChildRow);
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("ListOfBookingsAvailableLabel");
                    oRow.Properties.Caption = "ListOfBookingsAvailableLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("ListOfBookingsAvailableRequired");
                    oRow.Properties.Caption = "ListOfBookingsAvailableRequired";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxListOfBookingsAvailableRequired = new RepositoryItemComboBox();
                    repositoryItemComboBoxListOfBookingsAvailableRequired.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxListOfBookingsAvailableRequired.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxListOfBookingsAvailableRequired;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("AddCallerButtonLabel");
                    oRow.Properties.Caption = "AddCallerButtonLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("AddAdditionalAttendieButtonLabel");
                    oRow.Properties.Caption = "AddAdditionalAttendieButtonLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("DeleteAttendieButtonLabel");
                    oRow.Properties.Caption = "DeleteAttendieButtonLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("AttendiesLabel");
                    oRow.Properties.Caption = "AttendiesLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("AttendiesRequired");
                    oRow.Properties.Caption = "AttendiesRequired";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxAttendiesRequired = new RepositoryItemComboBox();
                    repositoryItemComboBoxAttendiesRequired.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxAttendiesRequired.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxAttendiesRequired;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("OtherChoices");
                    oRow.Properties.Caption = "OtherChoices";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemButtonEdit repositoryItemButtonEditOtherChoices = new RepositoryItemButtonEdit();
                    repositoryItemButtonEditOtherChoices.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    repositoryItemButtonEditOtherChoices.Click += new EventHandler(repositoryItemButtonEditOtherChoices_ButtonClick);
                    repositoryItemButtonEditOtherChoices.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(repositoryItemButtonEditOtherChoices_CustomDisplayText);
                    oRow.Properties.RowEdit = repositoryItemButtonEditOtherChoices;
                    propertyGridControl1.Rows.Add(oRow);
                    propertyGridControl1.DefaultEditors.Add(typeof(List<OtherChoice>), repositoryItemButtonEditOtherChoices);

                    listBoxControlMembers.DataSource = answerOptionList;
                    listBoxControlMembers.DisplayMember = "Attendies";
                    simpleButtonAdd.Enabled = false;
                    simpleButtonRemove.Enabled = false;
                    simpleButtonClear.Enabled = false;
                    simpleButtonMoveUp.Enabled = false;
                    simpleButtonMoveDown.Enabled = false;

                }else if (campaignQuestionnaire.Type.ToLower() == QuestionTypeConstants.SmartText){
                    EditorRow oRow = new EditorRow("TextPrefix");
                    oRow.Properties.Caption = "TextPrefix";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("DefaultInputValue");
                    oRow.Properties.Caption = "DefaultInputValue";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("Required");
                    oRow.Properties.Caption = "Required";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxTextboxRequired = new RepositoryItemComboBox();
                    repositoryItemComboBoxTextboxRequired.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxTextboxRequired.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxTextboxRequired;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("HeaderCommentText");
                    oRow.Properties.Caption = "HeaderCommentText";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("HeaderCreationDateText");
                    oRow.Properties.Caption = "HeaderCreationDateText";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("HeaderUserText");
                    oRow.Properties.Caption = "HeaderUserText";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("HeaderCustomerContactText");
                    oRow.Properties.Caption = "HeaderCustomerContactText";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

 
                    oRow = new EditorRow("OrderBy");
                    oRow.Properties.Caption = "OrderBy";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxTextboxOrderBy = new RepositoryItemComboBox();
                    repositoryItemComboBoxTextboxOrderBy.Items.AddRange(new object[] { "Comment", "Creation Date", "User", "Customer Contact" });
                    repositoryItemComboBoxTextboxOrderBy.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxTextboxOrderBy;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("OrderDirection");
                    oRow.Properties.Caption = "OrderDirection";
                    oRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxTextboxOrderDirection = new RepositoryItemComboBox();
                    repositoryItemComboBoxTextboxOrderDirection.Items.AddRange(new object[] { "Ascending", "Descending" });
                    repositoryItemComboBoxTextboxOrderDirection.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oRow.Properties.RowEdit = repositoryItemComboBoxTextboxOrderDirection;
                    propertyGridControl1.Rows.Add(oRow);

                    listBoxControlMembers.DataSource = answerOptionList;
                    listBoxControlMembers.DisplayMember = "TextPrefix";

                    simpleButtonAdd.Enabled = false;
                    simpleButtonRemove.Enabled = false;
                    simpleButtonClear.Enabled = false;
                    simpleButtonMoveUp.Enabled = false;
                    simpleButtonMoveDown.Enabled = false;
                }
            }
        }

        private void propertyGridControl1_EditorKeyDown(object sender, KeyEventArgs e) {
            listBoxControlMembers.Refresh();
        }

        private void propertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e) {
            listBoxControlMembers.Refresh();
            var datasource = listBoxControlMembers.DataSource as IList<AnswerOption>;
            campaignQuestionnaire.Form.Settings.AnswerOptions = datasource;
            sourcePG.SelectedObject = null;
            sourcePG.SelectedObject = campaignQuestionnaire;
            sourcePG.Refresh();
            if (Sender != null) {
                Sender.ForceCellValueChange();
            }
        }

        private void repositoryItemButtonEditOtherChoices_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e) {
            e.DisplayText = "(OtherChoiceCollection)";
        }

        private void repositoryItemButtonEditMultipleChoiceValues_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e) {
            e.DisplayText = "(MultipleChoiceValueCollection)";
        }

        private void repositoryItemButtonEditOtherChoices_ButtonClick(object sender, EventArgs e) {
            if (editorOtherChoice != null) {
                editorOtherChoice = null;
                return;
            }
            campaignQuestionnaire.Form.Settings.AnswerOptions[SelectedIndex] = propertyGridControl1.SelectedObject as AnswerOption;
            editorOtherChoice = new EditorDialogOtherChoices(campaignQuestionnaire, propertyGridControl1);
            editorOtherChoice.SelectedIndex = listBoxControlMembers.SelectedIndex;
            editorOtherChoice.StartPosition = FormStartPosition.CenterParent;
            if (editorOtherChoice.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                propertyGridControl1.CloseEditor();
                editorOtherChoice = null;
            } else {
                propertyGridControl1.CloseEditor();
                editorOtherChoice.Close();
                editorOtherChoice = null;
            }
        }

        private void repositoryItemButtonEditMultipleChoiceValues_ButtonClick(object sender, EventArgs e) {
            if (editorMultipleChoice != null) {
                editorMultipleChoice = null;
                return;
            }
            campaignQuestionnaire.Form.Settings.AnswerOptions[SelectedIndex] = propertyGridControl1.SelectedObject as AnswerOption;
            editorMultipleChoice = new EditorDialogMultipleChoiceValues(campaignQuestionnaire, propertyGridControl1);
            editorMultipleChoice.SelectedIndex = listBoxControlMembers.SelectedIndex;
            editorMultipleChoice.StartPosition = FormStartPosition.CenterParent;
            if (editorMultipleChoice.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                propertyGridControl1.CloseEditor();
                editorMultipleChoice = null;
            } else {
                propertyGridControl1.CloseEditor();
                editorMultipleChoice.Close();
                editorMultipleChoice = null;
            }
        }

        private void listBoxControlMembers_SelectedValueChanged(object sender, EventArgs e) {
            propertyGridControl1.SelectedObject = listBoxControlMembers.SelectedValue;
        }

        private void simpleButtonOk_Click(object sender, EventArgs e) {
            listBoxControlMembers.Refresh();
            var datasource = listBoxControlMembers.DataSource as IList<AnswerOption>;
            campaignQuestionnaire.Form.Settings.AnswerOptions = datasource;
            sourcePG.SelectedObject = null;
            sourcePG.SelectedObject = campaignQuestionnaire;
            sourcePG.Refresh(); 
            if (Sender != null) {
                Sender.ForceCellValueChange();
            }
        }

        private void simpleButtonAdd_Click(object sender, EventArgs e) {
            PropertyInfo pi = answerOptionList.GetType().GetProperty("Item");
            object instance = Activator.CreateInstance(pi.PropertyType);
            answerOptionList.Add(instance as AnswerOption);
            listBoxControlMembers.Refresh();
            listBoxControlMembers_SelectedValueChanged(null, null);
            listBoxControlMembers.SelectedIndex = answerOptionList.Count - 1;
        }

        private void simpleButtonRemove_Click(object sender, EventArgs e) {
            var option = listBoxControlMembers.SelectedItem as AnswerOption;
            if (option != null) {
                answerOptionList.Remove(option);
            }
            listBoxControlMembers.Refresh();
            listBoxControlMembers_SelectedValueChanged(null, null);
        }

        private void simpleButtonClear_Click(object sender, EventArgs e) {
            if (listBoxControlMembers != null && listBoxControlMembers.Items != null) {
                listBoxControlMembers.Items.Clear();                
                answerOptionList.Clear();
                listBoxControlMembers.Refresh();
                listBoxControlMembers_SelectedValueChanged(null, null);
            }
        }

        private void simpleButtonMoveUp_Click(object sender, EventArgs e) {
            if (listBoxControlMembers.SelectedItems.Count > 0) {
                var selected = listBoxControlMembers.SelectedItem as AnswerOption;

                int indx = answerOptionList.IndexOf(selected);
                int totl = answerOptionList.Count;

                if (indx == 0) {
                    answerOptionList.Remove(selected);
                    answerOptionList.Insert(totl - 1, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(totl - 1, true);
                } else {
                    answerOptionList.Remove(selected);
                    answerOptionList.Insert(indx - 1, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(indx - 1, true);
                }
            }
        }

        private void simpleButtonMoveDown_Click(object sender, EventArgs e) {
            if (listBoxControlMembers.SelectedItems.Count > 0) {
                var selected = listBoxControlMembers.SelectedItem as AnswerOption;
                int indx = answerOptionList.IndexOf(selected);
                int totl = answerOptionList.Count;

                if (indx == totl - 1) {
                    answerOptionList.Remove(selected);
                    answerOptionList.Insert(0, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(0, true);
                } else {
                    answerOptionList.Remove(selected);
                    answerOptionList.Insert(indx + 1, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(indx + 1, true);
                }
            }
        }

        
        #endregion

        #region Keyboard Shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (FormUtility.ShortCutKeysHandled(keyData))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

    }
}