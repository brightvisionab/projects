using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.DXErrorProvider;

using BrightVision.Model;
using SalesConsultant.Business;

namespace SalesConsultant.Modules
{
    public partial class AddTag : DevExpress.XtraEditors.XtraUserControl 
    {
        #region Member Variables
        private BrightPlatformEntities BPContext = null;
        private int editID;
        private string m_QuestionTag = string.Empty;
        #endregion

        #region Constructor
        public AddTag(BrightPlatformEntities context) {
            InitializeComponent();
            BPContext = context;
        }
        public AddTag(GridView oTagsGrid, BrightPlatformEntities context) {
            InitializeComponent();
            BPContext = context;
            TagsGrid = oTagsGrid;
        } 
        #endregion

        #region Methods
        protected override void OnLoad(EventArgs e) {
            if (EditMode) {
                if (TagsGrid != null) {
                    var gridview = TagsGrid;
                    var obj = gridview.GetFocusedRow();
                    if (obj != null) {
                        questiontag qtag = (questiontag)obj;
                        if (qtag != null) {
                            editID = qtag.id;
                            textEditTagname.Text = qtag.title;
                            textEditDescription.Text = qtag.description;
                        }
                    }
                }
            }
            m_QuestionTag = textEditTagname.Text;
            SetValidationRules();
            base.OnLoad(e);
        }
        private bool m_bEditMode;
        public bool EditMode {
            get { return m_bEditMode; }
            set {
                m_bEditMode = value;
                if (value) {
                    simpleButtonAdd.Text = "Update";
                }
            }
        }
        public GridView TagsGrid { get; set; }
        private void simpleButtonCancel_Click(object sender, EventArgs e) {
            ParentForm.Close();
        }
        private bool TagExists(string QuestionTag)
        {
            var iTag = BPContext.questiontags.FirstOrDefault(i => i.title.Equals(QuestionTag));
            if (iTag != null)
                return true;
            else
                return false;
        }
        private void simpleButtonAdd_Click(object sender, EventArgs e) 
        {
            if (!dxValidationProvider1.Validate()) 
                return;

            questiontag _efeQuestionTag = null;

            if (!this.EditMode) {
                if (this.TagExists(textEditTagname.Text.Trim()))
                {
                    MessageBox.Show("Tag already exist. Please enter another tag.", "Tags", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    ParentForm.DialogResult = DialogResult.None;
                    return;
                }
                _efeQuestionTag = new questiontag() {
                    title = textEditTagname.Text,
                    description = textEditDescription.Text
                };
                BPContext.questiontags.AddObject(_efeQuestionTag);
                //BPContext.questiontags.AddObject(new questiontag() {
                //    title = textEditTagname.Text,
                //    description = textEditDescription.Text
                //});
            } else {             
                //var obj = BPContext.questiontags.FirstOrDefault(p => p.id == editID);
                _efeQuestionTag = BPContext.questiontags.FirstOrDefault(p => p.id == editID);
                _efeQuestionTag.title = textEditTagname.Text;
                _efeQuestionTag.description = textEditDescription.Text;
            }
            BPContext.SaveChanges();
            var datasource = BPContext.questiontags.ToList();
            TagsGrid.GridControl.DataSource = datasource;
            this.textEditTagname.Text = string.Empty;
            this.textEditDescription.Text = string.Empty;

            var view = TagsGrid;
            questiontag oC;            
            if (datasource == null) return;
            var row = datasource.FirstOrDefault(p => p.id == editID);
            if (row != null) {
                for (int i = 0; i < view.DataRowCount; i++) {
                    oC = view.GetRow(i) as questiontag;
                    if (oC != null && row.id == oC.id) {
                        view.FocusedRowHandle = i;                        
                        break;
                    }
                }
            }

            if (AfterSave != null)
                AfterSave(this, new AddTagArgs() { QuestionTagId = _efeQuestionTag.id });

            ParentForm.Close();
        }
        private void SetValidationRules() {
            CustomIsBlankValidationRule isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "The tag name field cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(textEditTagname, isBlankValidationRule);
        }
        private void textEditTagname_Validating(object sender, CancelEventArgs e) {
            if (!dxValidationProvider1.Validate()) e.Cancel = true;
        } 
        #endregion

        #region Public Events & Args
        public delegate void AfterSaveEventHandler(object sender, AddTagArgs e);
        public event AfterSaveEventHandler AfterSave;
        public class AddTagArgs : EventArgs
        {
            public int QuestionTagId { get; set; }
        }
        #endregion
    }
}
