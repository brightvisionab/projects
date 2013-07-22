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
    public partial class EditorDialogMultipleChoiceValues : DevExpress.XtraEditors.XtraForm {
        private IList<AnswerOption> answerOptionList = null;
        private PropertyGridControl sourcePG = null;
        private CampaignQuestionnaire campaignQuestionnaire = null;
       
        public EditorDialogMultipleChoiceValues(CampaignQuestionnaire oQuestionnaire, PropertyGridControl sourcePropertyGrid) {            
            InitializeComponent();
            sourcePG = sourcePropertyGrid;
            campaignQuestionnaire = oQuestionnaire;
            answerOptionList = campaignQuestionnaire.Form.Settings.AnswerOptions;
            CreateEditorRows();            
        }

        public int SelectedIndex { get; set; }

        private void CreateEditorRows() {
            if (campaignQuestionnaire != null) {
                EditorRow oRow = new EditorRow("TextPrefix");
                oRow.Properties.Caption = "TextPrefix";
                oRow.Properties.ReadOnly = false;
                propertyGridControl1.Rows.Add(oRow);
                oRow = new EditorRow("Checked");
                oRow.Properties.Caption = "Checked";
                oRow.Properties.ReadOnly = false;
                RepositoryItemComboBox repositoryItemComboBoxChecked = new RepositoryItemComboBox();
                repositoryItemComboBoxChecked.Items.AddRange(new object[] { "True", "False" });
                repositoryItemComboBoxChecked.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                oRow.Properties.RowEdit = repositoryItemComboBoxChecked;
                propertyGridControl1.Rows.Add(oRow);
                listBoxControlMembers.DataSource = answerOptionList[SelectedIndex].MultipleChoiceValues;
                listBoxControlMembers.DisplayMember = "TextPrefix";
            }
        }
        private void simpleButtonOk_Click(object sender, EventArgs e) {
            listBoxControlMembers.Refresh();
            var datasource = listBoxControlMembers.DataSource as List<MultipleChoiceValue>;
            campaignQuestionnaire.Form.Settings.AnswerOptions[SelectedIndex].MultipleChoiceValues = datasource;
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
            PropertyInfo pi = answerOptionList[SelectedIndex].MultipleChoiceValues.GetType().GetProperty("Item");
            var instance = Activator.CreateInstance(pi.PropertyType) as MultipleChoiceValue;
            instance.TextPrefix = "Text Prefix";
            answerOptionList[SelectedIndex].MultipleChoiceValues.Add(instance);
            listBoxControlMembers.Refresh();
            listBoxControlMembers.SelectedIndex = answerOptionList[SelectedIndex].MultipleChoiceValues.Count - 1;
        }

        private void simpleButtonRemove_Click(object sender, EventArgs e) {
            var option = listBoxControlMembers.SelectedItem as MultipleChoiceValue;
            if (option != null) {
                answerOptionList[SelectedIndex].MultipleChoiceValues.Remove(option);
            }
            listBoxControlMembers.Refresh();
            listBoxControlMembers_SelectedValueChanged(null, null);
        }

        private void simpleButtonMoveUp_Click(object sender, EventArgs e) {
            if (listBoxControlMembers.SelectedItems.Count > 0) {
                var selected = listBoxControlMembers.SelectedItem as MultipleChoiceValue;

                int indx = answerOptionList[SelectedIndex].MultipleChoiceValues.IndexOf(selected);
                int totl = answerOptionList[SelectedIndex].MultipleChoiceValues.Count;


                if (indx == 0) {
                    answerOptionList[SelectedIndex].MultipleChoiceValues.Remove(selected);
                    answerOptionList[SelectedIndex].MultipleChoiceValues.Insert(totl - 1, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(totl - 1, true);
                } else {
                    answerOptionList[SelectedIndex].MultipleChoiceValues.Remove(selected);
                    answerOptionList[SelectedIndex].MultipleChoiceValues.Insert(indx - 1, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(indx - 1, true);
                }
            }
        }

        private void simpleButtonMoveDown_Click(object sender, EventArgs e) {
            if (listBoxControlMembers.SelectedItems.Count > 0) {
                var selected = listBoxControlMembers.SelectedItem as MultipleChoiceValue;
                
                int indx = answerOptionList[SelectedIndex].MultipleChoiceValues.IndexOf(selected);
                int totl = answerOptionList[SelectedIndex].MultipleChoiceValues.Count;
                
                if (indx == totl - 1) {
                    answerOptionList[SelectedIndex].MultipleChoiceValues.Remove(selected);
                    answerOptionList[SelectedIndex].MultipleChoiceValues.Insert(0, selected);
                    listBoxControlMembers.Refresh();
                    listBoxControlMembers.SetSelected(0, true);
                } else {
                    answerOptionList[SelectedIndex].MultipleChoiceValues.Remove(selected);
                    answerOptionList[SelectedIndex].MultipleChoiceValues.Insert(indx + 1, selected);
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