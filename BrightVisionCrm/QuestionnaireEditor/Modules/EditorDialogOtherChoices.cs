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
    public partial class EditorDialogOtherChoices : DevExpress.XtraEditors.XtraForm {        
        private IList<AnswerOption> answerOptionList = null;
        private PropertyGridControl sourcePG = null;
        private CampaignQuestionnaire campaignQuestionnaire = null;
        private OtherChoiceOptions oChoiceOption = OtherChoiceOptions.OtherChoice;
        
        public EditorDialogOtherChoices(CampaignQuestionnaire oQuestionnaire, 
            PropertyGridControl sourcePropertyGrid, 
            OtherChoiceOptions options = OtherChoiceOptions.OtherChoice) {            
            InitializeComponent();
            sourcePG = sourcePropertyGrid;
            campaignQuestionnaire = oQuestionnaire;
            answerOptionList = campaignQuestionnaire.Form.Settings.AnswerOptions;
            oChoiceOption = options;
            CreateEditorRows();            
        }

        public int SelectedIndex { get; set; }

        private void CreateEditorRows() {
            if (campaignQuestionnaire != null) {
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
                oRow = new EditorRow("Enabled");
                oRow.Properties.Caption = "Enabled";
                oRow.Properties.ReadOnly = false;
                RepositoryItemComboBox repositoryItemComboBoxEnabled = new RepositoryItemComboBox();
                repositoryItemComboBoxEnabled.Items.AddRange(new object[] { "True", "False" });
                repositoryItemComboBoxEnabled.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                oRow.Properties.RowEdit = repositoryItemComboBoxEnabled;
                propertyGridControl1.Rows.Add(oRow);
                if(oChoiceOption == OtherChoiceOptions.OtherChoice)
                    listBoxControlMembers.DataSource = answerOptionList[SelectedIndex].OtherChoices;
              
                listBoxControlMembers.DisplayMember = "TextPrefix";
            }
        }

        private void simpleButtonOk_Click(object sender, EventArgs e) {           
            listBoxControlMembers.Refresh();
            var datasource = listBoxControlMembers.DataSource as List<OtherChoice>;
            campaignQuestionnaire.Form.Settings.AnswerOptions[SelectedIndex].OtherChoices = datasource;
            sourcePG.SelectedObject = campaignQuestionnaire.Form.Settings.AnswerOptions[SelectedIndex];
            sourcePG.Refresh();
            this.Close();
        }

        private void listBoxControlMembers_SelectedValueChanged(object sender, EventArgs e) {
            propertyGridControl1.SelectedObject = listBoxControlMembers.SelectedValue;
        }

        private void simpleButtonCancel_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void simpleButtonAdd_Click(object sender, EventArgs e) {
            PropertyInfo pi = answerOptionList[SelectedIndex].OtherChoices.GetType().GetProperty("Item");
            var instance = Activator.CreateInstance(pi.PropertyType) as OtherChoice;
            instance.TextPrefix = "Text Prefix";
            answerOptionList[SelectedIndex].OtherChoices.Add(instance);
            listBoxControlMembers.Refresh();
            listBoxControlMembers.SelectedIndex = answerOptionList[SelectedIndex].OtherChoices.Count - 1;
        }

        private void simpleButtonRemove_Click(object sender, EventArgs e) {
            var option = listBoxControlMembers.SelectedItem as OtherChoice;
            if (option != null) {
                answerOptionList[SelectedIndex].OtherChoices.Remove(option);
            }                
            listBoxControlMembers.Refresh();
            listBoxControlMembers_SelectedValueChanged(null, null);
        }

        private void simpleButtonMoveUp_Click(object sender, EventArgs e) {
            if (listBoxControlMembers.SelectedItems.Count > 0) {
                var selected = listBoxControlMembers.SelectedItem as OtherChoice;
                int indx = answerOptionList[SelectedIndex].OtherChoices.IndexOf(selected);
                int totl = answerOptionList[SelectedIndex].OtherChoices.Count;
                if (indx == 0) {
                    answerOptionList[SelectedIndex].OtherChoices.Remove(selected);
                    answerOptionList[SelectedIndex].OtherChoices.Insert(totl - 1, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(totl - 1, true);
                } else {
                    answerOptionList[SelectedIndex].OtherChoices.Remove(selected);
                    answerOptionList[SelectedIndex].OtherChoices.Insert(indx - 1, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(indx - 1, true);
                }
            }
        }

        private void simpleButtonMoveDown_Click(object sender, EventArgs e) {
            if (listBoxControlMembers.SelectedItems.Count > 0) {
                var selected = listBoxControlMembers.SelectedItem as OtherChoice;
                int indx = answerOptionList[SelectedIndex].OtherChoices.IndexOf(selected);
                int totl = answerOptionList[SelectedIndex].OtherChoices.Count;
                
                if (indx == totl - 1) {   
                    answerOptionList[SelectedIndex].OtherChoices.Remove(selected);
                    answerOptionList[SelectedIndex].OtherChoices.Insert(0, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(0, true);
                } else {
                    answerOptionList[SelectedIndex].OtherChoices.Remove(selected);
                    answerOptionList[SelectedIndex].OtherChoices.Insert(indx + 1, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(indx + 1, true);
                }
            }
        }

        private void propertyGridControl1_EditorKeyDown(object sender, KeyEventArgs e) {
            listBoxControlMembers.Refresh();
        }

        private void propertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e) {
            listBoxControlMembers.Refresh();
        }
    }
}