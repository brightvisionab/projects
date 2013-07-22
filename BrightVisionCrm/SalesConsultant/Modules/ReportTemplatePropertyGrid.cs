using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SalesConsultant.Business;
using BrightVision.Reporting.Template;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraVerticalGrid;
using DevExpress.Data;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace SalesConsultant.Modules
{
    public partial class ReportTemplatePropertyGrid : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public ReportTemplatePropertyGrid()
        {
            InitializeComponent();

            LoadGraphSize();
            LoadPercentageValueSorting();
        }
        #endregion

        #region Public Properties
        public class StatColumn {
            public int PositionIndex { get; set; }
            public string DisplayName { get; set; }
            public string LabelName { get; set; }
        }

        public TemplateProperty TemplateP { get; set; }
        public List<string> ColumnList {
            get {
                return _ColumnList;
            }
            set {
                _ColumnList = value;
                repositoryItemLookUpEditStatisticColumn.DataSource = _ColumnList;

            }
        }
        #endregion

        #region Private Properties
        private List<string> _ColumnList;
        #endregion

        #region Public
        public void SetAvaiableColumns(List<StatColumn> pColumns)
        {
            this.repositoryItemLookUpEditStatisticColumn.EditValueChanged -= new System.EventHandler(this.repositoryItemLookUpEditStatisticColumn_EditValueChanged);
            repositoryItemLookUpEditStatisticColumn.DataSource = null;
            repositoryItemLookUpEditStatisticColumn.DataSource = pColumns;
            repositoryItemLookUpEditStatisticColumn.DisplayMember = "DisplayName";
            repositoryItemLookUpEditStatisticColumn.ValueMember = "DisplayName";
            repositoryItemLookUpEditStatisticColumn.Columns.Add(new LookUpColumnInfo("DisplayName"));
            this.repositoryItemLookUpEditStatisticColumn.EditValueChanged += new System.EventHandler(this.repositoryItemLookUpEditStatisticColumn_EditValueChanged);
        }
        public void CreateReportLayoutSettings(TemplateProperty templateData)
        {
            vGridControlProperty.Rows.Clear();
            CreateDefaultLayoutSetting(templateData);

            var _TemplateProperty = from property in TemplateP.DynamicProperty orderby property.Order select property;
            foreach (TemplateDynamicData _layoutSetting in _TemplateProperty) {
                if (_layoutSetting.Type == TemplateDynamicType.PageBreak)
                    AddPageBreak(_layoutSetting, false);

                else if (_layoutSetting.Type == TemplateDynamicType.TextField)
                    AddTextField(_layoutSetting, false);

                else if (_layoutSetting.Type == TemplateDynamicType.Statistics)
                    AddStatistics(_layoutSetting, false);
            }
        }
        public void CreateDefaultLayoutSetting(TemplateProperty templateData = null)
        {
            TemplateP = templateData;
            vGridControlProperty.Rows.Clear();
            if (TemplateP == null)
                TemplateP = new TemplateProperty 
                { 
                    IsFooterVisible = true, 
                    IsPageNumberVisible = true, 
                    IsEmptyDynamicValueVisible = false 
                };

            DevExpress.XtraVerticalGrid.Rows.EditorRow row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Value = TemplateP.IsFooterVisible;
            row.Properties.Caption = "Show Footer";
            row.Properties.FieldName = GetPropertyName(() => templateData.IsFooterVisible);
            repositoryItemComboBoxBoolean.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            row.Properties.RowEdit = repositoryItemComboBoxBoolean;
            vGridControlProperty.Rows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Value = TemplateP.IsPageNumberVisible;
            row.Properties.Caption = "Show Page Number";
            row.Properties.FieldName = GetPropertyName(() => TemplateP.IsPageNumberVisible);
            repositoryItemComboBoxBoolean.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            row.Properties.RowEdit = repositoryItemComboBoxBoolean;
            vGridControlProperty.Rows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Value = TemplateP.IsEmptyDynamicValueVisible;
            row.Properties.Caption = "Show Empty Dynamic Content";
            row.Properties.FieldName = GetPropertyName(() => TemplateP.IsEmptyDynamicValueVisible);
            repositoryItemComboBoxBoolean.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            row.Properties.RowEdit = repositoryItemComboBoxBoolean;
            vGridControlProperty.Rows.Add(row);

            if (TemplateP.DynamicProperty == null)
                TemplateP.DynamicProperty = new List<TemplateDynamicData>();
        }
        public void AddStatistics(TemplateDynamicData property, bool AddToInternalCollection = true)
        {
            if (AddToInternalCollection)
                TemplateP.DynamicProperty.Add(property);
            var categoryRow = new DevExpress.XtraVerticalGrid.Rows.CategoryRow();
            categoryRow.Properties.Caption = "Statistics";

            var row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Show";
            row.Properties.RowEdit = repositoryItemComboBoxBoolean;
            row.Properties.Value = property.IsVisible;
            row.Properties.FieldName = GetPropertyName(() => property.IsVisible);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Order";
            row.Properties.RowEdit = repositoryItemTextEditNumberOnly;
            row.Properties.Value = property.Order;
            row.Properties.UnboundType = UnboundColumnType.Integer;
            row.Properties.FieldName = GetPropertyName(() => property.Order);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Column name";
            row.Properties.RowEdit = repositoryItemLookUpEditStatisticColumn;
            //repositoryItemLookUpEditStatisticColumn.Tag = property;
            row.Properties.Value = property.Statistics.ColumnName;
            row.Properties.FieldName = GetPropertyName(() => property.Statistics.ColumnName);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Chart label";
            row.Properties.RowEdit = repositoryItemTextEdit;
            row.Properties.Value = property.Statistics.ChartTitle;
            row.Properties.FieldName = GetPropertyName(() => property.Statistics.ChartTitle);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            #region Size
            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Size";
            //repositoryItemLookUpEditGraphSize.Tag = property;
            row.Properties.RowEdit = repositoryItemLookUpEditGraphSize;
            row.Properties.Value = property.Statistics.GraphSize;
            row.Properties.FieldName = GetPropertyName(() => property.Statistics.GraphSize);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);
            #endregion

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Sort statistics by";
            row.Properties.RowEdit = repositoryItemLookUpEditSorting;
            row.Properties.Value = property.Statistics.PercentageValueSortBy;
            row.Properties.FieldName = GetPropertyName(() => property.Statistics.PercentageValueSortBy);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Show total count";
            row.Properties.RowEdit = repositoryItemComboBoxBoolean;
            row.Properties.Value = property.Statistics.IsTotalCountVisible;
            row.Properties.FieldName = GetPropertyName(() => property.Statistics.IsTotalCountVisible);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Show null category";
            row.Properties.RowEdit = repositoryItemComboBoxBoolean;
            row.Properties.Value = property.Statistics.IsNullCategoryVisible;
            row.Properties.FieldName = GetPropertyName(() => property.Statistics.IsNullCategoryVisible);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            vGridControlProperty.Rows.Add(categoryRow);
        }
        public void AddPageBreak(TemplateDynamicData property, bool AddToInternalCollection = true)
        {
            if (AddToInternalCollection)
                TemplateP.DynamicProperty.Add(property);
            var categoryRow = new DevExpress.XtraVerticalGrid.Rows.CategoryRow();
            categoryRow.Properties.Caption = "Page Break";

            var row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Order";
            row.Properties.RowEdit = repositoryItemTextEditNumberOnly;
            row.Properties.Value = property.Order;
            row.Properties.UnboundType = UnboundColumnType.Integer;
            row.Properties.FieldName = GetPropertyName(() => property.Order);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            vGridControlProperty.Rows.Add(categoryRow);
        }
        public void AddTextField(TemplateDynamicData property, bool AddToInternalCollection = true)
        {
            if (AddToInternalCollection)
                TemplateP.DynamicProperty.Add(property);
            var categoryRow = new DevExpress.XtraVerticalGrid.Rows.CategoryRow();
            categoryRow.Properties.Caption = "Text Field";

            var row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Show";
            row.Properties.RowEdit = repositoryItemComboBoxBoolean;
            row.Properties.Value = property.IsVisible;
            row.Properties.FieldName = GetPropertyName(() => property.IsVisible);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Order";
            row.Properties.RowEdit = repositoryItemTextEditNumberOnly;
            row.Properties.Value = property.Order;
            row.Properties.UnboundType = UnboundColumnType.Integer;
            row.Properties.FieldName = GetPropertyName(() => property.Order);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            if (property.Text == null)
            {
                property.Text = new TextField { Size = TextFieldFontSize.Normal, Style = TextFieldFontStyle.None };
            }

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Value";
            row.Properties.RowEdit = repositoryItemTextEdit;
            row.Properties.Value = property.Text.Text;
            row.Properties.FieldName = GetPropertyName(() => property.Text.Text);
            row.Tag = property;

            categoryRow.ChildRows.Add(row);

            #region Size
            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Size";
            IList lookUpListSize = typeof(TextFieldFontSize).ToEnumList();
            repositoryItemLookUpEditSize.DataSource = lookUpListSize;
            repositoryItemLookUpEditSize.DisplayMember = "Value";
            repositoryItemLookUpEditSize.ValueMember = "Key";
            repositoryItemLookUpEditSize.ShowHeader = false;
            repositoryItemLookUpEditSize.PopulateColumns();
            repositoryItemLookUpEditSize.Columns["Key"].Visible = false;
            repositoryItemLookUpEditSize.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            repositoryItemLookUpEditSize.Tag = property;
            row.Properties.RowEdit = repositoryItemLookUpEditSize;
            row.Properties.Value = property.Text.Size;
            row.Properties.FieldName = GetPropertyName(() => property.Text.Size);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);
            #endregion

            #region Style
            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Style";
            IList lookUpList = typeof(TextFieldFontStyle).ToEnumList();
            repositoryItemLookUpEditStyle.DataSource = lookUpList;
            repositoryItemLookUpEditStyle.DisplayMember = "Value";
            repositoryItemLookUpEditStyle.ValueMember = "Key";
            repositoryItemLookUpEditStyle.ShowHeader = false;
            repositoryItemLookUpEditStyle.PopulateColumns();
            repositoryItemLookUpEditStyle.Columns["Key"].Visible = false;
            repositoryItemLookUpEditStyle.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            repositoryItemLookUpEditStyle.Tag = property;
            row.Properties.RowEdit = repositoryItemLookUpEditStyle;
            row.Properties.Value = property.Text.Style;
            row.Properties.FieldName = GetPropertyName(() => property.Text.Style);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);
            #endregion

            vGridControlProperty.Rows.Add(categoryRow);
        }
        #endregion

        #region Private
        public void SetValueTemplateProperty(object value, string fieldName, ref TemplateProperty propertyOwner)
        {
            PropertyInfo propertyInfo = propertyOwner.GetType().GetProperty(fieldName);
            propertyInfo.SetValue(propertyOwner, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }
        public void SetValueStatisticsProperty(object value, string fieldName, ref StatisticsTemplate propertyOwner)
        {
            PropertyInfo propertyInfo = propertyOwner.GetType().GetProperty(fieldName);
            propertyInfo.SetValue(propertyOwner, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }
        public void SetValueTemplateDynamicData(object value, string fieldName, ref TemplateDynamicData propertyOwner)
        {
            PropertyInfo propertyInfo = propertyOwner.GetType().GetProperty(fieldName);
            propertyInfo.SetValue(propertyOwner, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }
        private string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            return (propertyExpression.Body as MemberExpression).Member.Name;
        }
        public void SetValueTemplateDynamicData(string value, string fieldName, ref TextField textField)
        {
            PropertyInfo propertyInfo = textField.GetType().GetProperty(fieldName);
            propertyInfo.SetValue(textField, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }
        private void LoadGraphSize()
        {
            IList lookUpListSize = typeof(GraphSize).ToEnumList();
            repositoryItemLookUpEditGraphSize.DataSource = lookUpListSize;
            repositoryItemLookUpEditGraphSize.DisplayMember = "Value";
            repositoryItemLookUpEditGraphSize.ValueMember = "Key";
            repositoryItemLookUpEditGraphSize.ShowHeader = false;
            repositoryItemLookUpEditGraphSize.PopulateColumns();
            repositoryItemLookUpEditGraphSize.Columns["Key"].Visible = false;
        }

        private void LoadPercentageValueSorting()
        {
            IList lookUpListSize = typeof(PercentageValueSortingOption).ToEnumList();
            repositoryItemLookUpEditSorting.DataSource = lookUpListSize;
            repositoryItemLookUpEditSorting.DisplayMember = "Value";
            repositoryItemLookUpEditSorting.ValueMember = "Key";
            repositoryItemLookUpEditSorting.ShowHeader = false;
            repositoryItemLookUpEditSorting.PopulateColumns();
            repositoryItemLookUpEditSorting.Columns["Key"].Visible = false;
        }

        #endregion

        #region Events
        private void repositoryItemComboBoxBoolean_EditValueChanged(object sender, EventArgs e)
        {
            var row = vGridControlProperty.FocusedRow;
            bool value = (sender as DevExpress.XtraEditors.ComboBoxEdit).EditValue.ToString() == "True" ? true : false;
            var templateDataTemp = TemplateP;
            if (row.Tag == null)
            {
                string fieldName = row.Properties.FieldName;
                SetValueTemplateProperty(value, fieldName, ref templateDataTemp);
            }
            if (row.Tag is TemplateDynamicData)
            {
                var templdateDynamicData = row.Tag as TemplateDynamicData;
                if (templdateDynamicData.Type == TemplateDynamicType.TextField || templdateDynamicData.Type == TemplateDynamicType.PageBreak)
                {
                    string fieldName = row.Properties.FieldName;
                    SetValueTemplateDynamicData(value, fieldName, ref templdateDynamicData);
                }
                else if (templdateDynamicData.Type == TemplateDynamicType.Statistics)
                {
                    string fieldName = row.Properties.FieldName;
                    if (fieldName == "IsVisible")
                    {
                        SetValueTemplateDynamicData(value, fieldName, ref templdateDynamicData);
                    }
                    else
                    {
                        var stats = templdateDynamicData.Statistics;
                        SetValueStatisticsProperty(value, fieldName, ref stats);
                    }
                }
            }
        }
        private void repositoryItemTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            var row = vGridControlProperty.FocusedRow;
            string value = (sender as DevExpress.XtraEditors.TextEdit).EditValue.ToString();
            if (row.Tag is TemplateDynamicData)
            {
                var templdateDynamicData = row.Tag as TemplateDynamicData;
                if (templdateDynamicData.Type == TemplateDynamicType.TextField)
                {
                    string fieldName = row.Properties.FieldName;
                    var txt = templdateDynamicData.Text;
                    SetValueTemplateDynamicData(value, fieldName, ref txt);
                }
                else if (templdateDynamicData.Type == TemplateDynamicType.Statistics)
                {
                    string fieldName = row.Properties.FieldName;
                    var stats = templdateDynamicData.Statistics;
                    SetValueStatisticsProperty(value, fieldName, ref stats);
                }
            }
        }
        private void repositoryItemTextEditNumberOnly_EditValueChanged(object sender, EventArgs e)
        {
            var row = vGridControlProperty.FocusedRow;
            string value = "0";
            object textboxObj = (sender as DevExpress.XtraEditors.TextEdit).EditValue;
            if (textboxObj != null)
                value = textboxObj.ToString();

            if (row.Tag is TemplateDynamicData)
            {
                var templdateDynamicData = row.Tag as TemplateDynamicData;
                string fieldName = row.Properties.FieldName;
                SetValueTemplateDynamicData(value, fieldName, ref templdateDynamicData);
            }
        }
        private void repositoryItemLookUpEditSize_EditValueChanged(object sender, EventArgs e)
        {
            var le = sender as LookUpEdit;
            var val = le.EditValue;
            var tdd = vGridControlProperty.FocusedRow.Tag as TemplateDynamicData;
            if (tdd.Type == TemplateDynamicType.TextField)
            {
                tdd.Text.Size = (TextFieldFontSize)val;
            }
        }

        private void repositoryItemLookUpEditStyle_EditValueChanged(object sender, EventArgs e)
        {
            var le = sender as LookUpEdit;
            var val = le.EditValue;
            var a = le.Parent;
            var tdd = vGridControlProperty.FocusedRow.Tag as TemplateDynamicData;
            if (tdd.Type == TemplateDynamicType.TextField)
            {
                tdd.Text.Style = (TextFieldFontStyle)val;
            }
        }

        private void repositoryItemLookUpEditGraphSize_EditValueChanged(object sender, EventArgs e)
        {
            var le = sender as LookUpEdit;
            var val = le.EditValue;
            var tdd = vGridControlProperty.FocusedRow.Tag as TemplateDynamicData;
            if (tdd.Type == TemplateDynamicType.Statistics)
            {
                tdd.Statistics.GraphSize = (GraphSize)val;
            }
        }
        private void repositoryItemLookUpEditStatisticColumn_EditValueChanged(object sender, EventArgs e)
        {
            var le = sender as LookUpEdit;
            var val = le.Text;
            var tdd = vGridControlProperty.FocusedRow.Tag as TemplateDynamicData;
            if (tdd.Type == TemplateDynamicType.Statistics)
                tdd.Statistics.ColumnName = val;
        }

        private void repositoryItemLookUpEditSortBy_EditValueChanged(object sender, EventArgs e)
        {
            var le = sender as LookUpEdit;
            var val = le.EditValue;
            var tdd = vGridControlProperty.FocusedRow.Tag as TemplateDynamicData;
            if (tdd.Type == TemplateDynamicType.Statistics)
            {
                tdd.Statistics.PercentageValueSortBy = (PercentageValueSortingOption)val;
            }
        }
        #endregion
    }
}
