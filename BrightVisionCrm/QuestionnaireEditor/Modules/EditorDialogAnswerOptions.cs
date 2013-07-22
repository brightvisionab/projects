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
using QuestionnaireEditor.Business;

namespace QuestionnaireEditor.Modules {
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
                    oRow = new EditorRow("DefaultSelectionValueIfOther");
                    oRow.Properties.Caption = "DefaultSelectionValueIfOther";
                    oRow.Properties.ReadOnly = false;
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
                    EditorRow oRow = new EditorRow("CalendarButtonLabel");
                    oRow.Properties.Caption = "CalendarButtonLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("UseContactButtonLabel");
                    oRow.Properties.Caption = "UseContactButtonLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);
                    oRow = new EditorRow("CalendarOption");
                    oRow.Properties.Caption = "CalendarOption";
                    oRow.Properties.ReadOnly = true;

                    EditorRow oChildRow = new EditorRow("CalendarOption.TextPrefix");
                    oChildRow.Properties.Caption = "TextPrefix";
                    oChildRow.Properties.ReadOnly = false;
                    oRow.ChildRows.Add(oChildRow);
                    oChildRow = new EditorRow("CalendarOption.CalendarSelectedValue");
                    oChildRow.Properties.Caption = "CalendarSelectedValue";
                    oChildRow.Properties.ReadOnly = false;                    
                    oRow.ChildRows.Add(oChildRow);
                    //oChildRow = new EditorRow("CalendarValues");
                    //oChildRow.Properties.Caption = "CalendarValues";
                    //oChildRow.Properties.ReadOnly = false;
                    //oRow.ChildRows.Add(oChildRow);
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("ScheduleDate");
                    oRow.Properties.Caption = "ScheduleDate";
                    oRow.Properties.ReadOnly = false;

                    oChildRow = new EditorRow("ScheduleDate.TextPrefix");
                    oChildRow.Properties.Caption = "TextPrefix";
                    oChildRow.Properties.ReadOnly = false;
                    oRow.ChildRows.Add(oChildRow);
                    oChildRow = new EditorRow("ScheduleDate.DefaultInputValue");
                    oChildRow.Properties.Caption = "DefaultInputValue";
                    oChildRow.Properties.ReadOnly = false;
                    oRow.ChildRows.Add(oChildRow);
                    oChildRow = new EditorRow("ScheduleDate.Enabled");
                    oChildRow.Properties.Caption = "Enabled";
                    oChildRow.Properties.ReadOnly = false;
                    RepositoryItemComboBox repositoryItemComboBoxEnabled = new RepositoryItemComboBox();
                    repositoryItemComboBoxEnabled.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxEnabled.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oChildRow.Properties.RowEdit = repositoryItemComboBoxEnabled;
                    oRow.ChildRows.Add(oChildRow);
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("StartTime");
                    oRow.Properties.Caption = "StartTime";
                    oRow.Properties.ReadOnly = false;

                    oChildRow = new EditorRow("StartTime.TextPrefix");
                    oChildRow.Properties.Caption = "TextPrefix";
                    oChildRow.Properties.ReadOnly = false;
                    oRow.ChildRows.Add(oChildRow);
                    oChildRow = new EditorRow("StartTime.DefaultInputValue");
                    oChildRow.Properties.Caption = "DefaultInputValue";
                    oChildRow.Properties.ReadOnly = false;
                    oRow.ChildRows.Add(oChildRow);
                    oChildRow = new EditorRow("StartTime.Enabled");
                    oChildRow.Properties.Caption = "Enabled";
                    oChildRow.Properties.ReadOnly = false;
                    repositoryItemComboBoxEnabled = new RepositoryItemComboBox();
                    repositoryItemComboBoxEnabled.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxEnabled.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oChildRow.Properties.RowEdit = repositoryItemComboBoxEnabled;
                    oRow.ChildRows.Add(oChildRow);
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("EndTime");
                    oRow.Properties.Caption = "EndTime";
                    oRow.Properties.ReadOnly = false;

                    oChildRow = new EditorRow("EndTime.TextPrefix");
                    oChildRow.Properties.Caption = "TextPrefix";
                    oChildRow.Properties.ReadOnly = false;
                    oRow.ChildRows.Add(oChildRow);
                    oChildRow = new EditorRow("EndTime.DefaultInputValue");
                    oChildRow.Properties.Caption = "DefaultInputValue";
                    oChildRow.Properties.ReadOnly = false;
                    oRow.ChildRows.Add(oChildRow);
                    oChildRow = new EditorRow("EndTime.Enabled");
                    oChildRow.Properties.Caption = "Enabled";
                    oChildRow.Properties.ReadOnly = false;
                    repositoryItemComboBoxEnabled = new RepositoryItemComboBox();
                    repositoryItemComboBoxEnabled.Items.AddRange(new object[] { "True", "False" });
                    repositoryItemComboBoxEnabled.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    oChildRow.Properties.RowEdit = repositoryItemComboBoxEnabled;
                    oRow.ChildRows.Add(oChildRow);
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("AttendiesLabel");
                    oRow.Properties.Caption = "AttendiesLabel";
                    oRow.Properties.ReadOnly = false;
                    propertyGridControl1.Rows.Add(oRow);

                    oRow = new EditorRow("AddContactButtonLabel");
                    oRow.Properties.Caption = "AddContactButtonLabel";
                    oRow.Properties.ReadOnly = false;
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

                    //oRow = new EditorRow("MeetingPlaceDetails");
                    //oRow.Properties.Caption = "MeetingPlaceDetails";
                    //oRow.Properties.ReadOnly = false;
                    //RepositoryItemButtonEdit repositoryItemButtonEditMeetingPlaceDetails = new RepositoryItemButtonEdit();
                    //repositoryItemButtonEditMeetingPlaceDetails.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                    //repositoryItemButtonEditMeetingPlaceDetails.Click += new EventHandler(repositoryItemButtonEditMeetingPlaceDetails_Click);
                    //repositoryItemButtonEditMeetingPlaceDetails.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(repositoryItemButtonEditMeetingPlaceDetails_CustomDisplayText);
                    //oRow.Properties.RowEdit = repositoryItemButtonEditMeetingPlaceDetails;
                    //propertyGridControl1.Rows.Add(oRow);
                    //propertyGridControl1.DefaultEditors.Add(typeof(List<MeetingPlaceDetail>), repositoryItemButtonEditMeetingPlaceDetails);

                    listBoxControlMembers.DataSource = answerOptionList;
                    listBoxControlMembers.DisplayMember = "Attendies";
                    simpleButtonAdd.Enabled = false;
                    simpleButtonRemove.Enabled = false;
                    simpleButtonMoveUp.Enabled = false;
                    simpleButtonMoveDown.Enabled = false;

                } 
                //else if (campaignQuestionnaire.Type.ToLower() == QuestionTypeConstants.Seminar) {
                //    EditorRow oRow = new EditorRow("SeminarSchedule");
                //    oRow.Properties.Caption = "SeminarSchedule";
                //    oRow.Properties.ReadOnly = false;
                    
                //    EditorRow oChildRow = new EditorRow("SeminarSchedule.TextPrefix");
                //    oChildRow.Properties.Caption = "TextPrefix";
                //    oChildRow.Properties.ReadOnly = false;
                //    oRow.ChildRows.Add(oChildRow);
                //    oChildRow = new EditorRow("SeminarSchedule.SelectedValue");
                //    oChildRow.Properties.Caption = "SelectedValue";
                //    oChildRow.Properties.ReadOnly = false;
                //    oRow.ChildRows.Add(oChildRow);
                //    propertyGridControl1.Rows.Add(oRow);
                //    oRow = new EditorRow("AttendiesLabel");
                //    oRow.Properties.Caption = "AttendiesLabel";
                //    oRow.Properties.ReadOnly = false;
                //    propertyGridControl1.Rows.Add(oRow);
                //    oRow = new EditorRow("AddContactButtonLabel");
                //    oRow.Properties.Caption = "AddContactButtonLabel";
                //    oRow.Properties.ReadOnly = false;
                //    propertyGridControl1.Rows.Add(oRow);
                    
                //    listBoxControlMembers.DataSource = answerOptionList;
                //    listBoxControlMembers.DisplayMember = "Attendies";
                //    simpleButtonAdd.Enabled = false;
                //    simpleButtonRemove.Enabled = false;
                //    simpleButtonMoveUp.Enabled = false;
                //    simpleButtonMoveDown.Enabled = false;
                //}
            }
        }

        private void repositoryItemButtonEditMeetingPlaceDetails_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e) {
            e.DisplayText = "(MeetingPlaceDetailsCollection)";
        }

        private void repositoryItemButtonEditMeetingPlaceDetails_Click(object sender, EventArgs e) {                        
            campaignQuestionnaire.Form.Settings.AnswerOptions[SelectedIndex] = propertyGridControl1.SelectedObject as AnswerOption;
            editorOtherChoice = new EditorDialogOtherChoices(campaignQuestionnaire, propertyGridControl1, OtherChoiceOptions.MeetingPlaceDetails);
            editorOtherChoice.SelectedIndex = listBoxControlMembers.SelectedIndex;
            editorOtherChoice.StartPosition = FormStartPosition.CenterParent;
            editorOtherChoice.ShowDialog();
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
            //if (editorOtherChoice == null) {
                campaignQuestionnaire.Form.Settings.AnswerOptions[SelectedIndex] = propertyGridControl1.SelectedObject as AnswerOption;
                editorOtherChoice = new EditorDialogOtherChoices(campaignQuestionnaire, propertyGridControl1);
                editorOtherChoice.SelectedIndex = listBoxControlMembers.SelectedIndex;
                editorOtherChoice.StartPosition = FormStartPosition.CenterParent;
                editorOtherChoice.ShowDialog();
            //} else {
            //    editorOtherChoice.ShowDialog();
            //}
        }

        private void repositoryItemButtonEditMultipleChoiceValues_ButtonClick(object sender, EventArgs e) {
            if (editorMultipleChoice == null) {
                campaignQuestionnaire.Form.Settings.AnswerOptions[SelectedIndex] = propertyGridControl1.SelectedObject as AnswerOption;
                editorMultipleChoice = new EditorDialogMultipleChoiceValues(campaignQuestionnaire, propertyGridControl1);
                editorMultipleChoice.SelectedIndex = listBoxControlMembers.SelectedIndex;
                editorMultipleChoice.StartPosition = FormStartPosition.CenterParent;
                editorMultipleChoice.ShowDialog();
            } else {
                editorMultipleChoice.ShowDialog();
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
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void simpleButtonCancel_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void simpleButtonAdd_Click(object sender, EventArgs e) {
            PropertyInfo pi = answerOptionList.GetType().GetProperty("Item");
            object instance = Activator.CreateInstance(pi.PropertyType);
            answerOptionList.Add(instance as AnswerOption);
            listBoxControlMembers.Refresh();
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

    }
}