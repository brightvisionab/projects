
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using DevExpress.Data;
using BrightVision.Reporting.Template;
using DevExpress.XtraEditors.Repository;
using  DevExpress.XtraVerticalGrid;
namespace ManagerApplication.Business
{
    public static class ReportLayoutSettingsUtility
    {
        public static void CreateReportLayoutSettings(TemplateProperty templateData, RepositoryItemComboBox repositoryItemComboBoxBoolean,
                                                        RepositoryItemComboBox repositoryItemComboBoxSize, RepositoryItemComboBox repositoryItemComboBoxStyle, 
                                                        RepositoryItemTextEdit repositoryItemTextEditNumberOnly, RepositoryItemMemoEdit repositoryItemTextEdit,
                                                        VGridControl vGridControlProperty)
        {
            CreateDefaultLayoutSetting(repositoryItemComboBoxBoolean
            ,vGridControlProperty, templateData);

            var _TemplateProperty = from property in templateData.DynamicProperty orderby property.Order select property;
            foreach (TemplateDynamicData _layoutSetting in _TemplateProperty)
            {
                if (_layoutSetting.Type == TemplateDynamicType.PageBreak)
                    AddPageBreak(_layoutSetting, repositoryItemTextEditNumberOnly, vGridControlProperty);

                else if (_layoutSetting.Type == TemplateDynamicType.TextField)
                    AddTextField(_layoutSetting,repositoryItemComboBoxBoolean, repositoryItemComboBoxSize, repositoryItemComboBoxStyle, 
                        repositoryItemTextEditNumberOnly,repositoryItemTextEdit,vGridControlProperty);

                else if (_layoutSetting.Type == TemplateDynamicType.Statistics)
                    AddStatistics(_layoutSetting, repositoryItemComboBoxBoolean,repositoryItemComboBoxSize,
                                   repositoryItemTextEditNumberOnly,  repositoryItemTextEdit,  vGridControlProperty);
            }
        }

        public static void CreateDefaultLayoutSetting(RepositoryItemComboBox repositoryItemComboBoxBoolean
            ,VGridControl vGridControlProperty,TemplateProperty templateData = null)
        {
            if (templateData == null)
                templateData = new TemplateProperty { IsFooterVisible = true, IsPageNumberVisible = true };

            DevExpress.XtraVerticalGrid.Rows.EditorRow row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Value = templateData.IsFooterVisible;
            row.Properties.Caption = "Show Footer";
            row.Properties.FieldName = GetPropertyName(() => templateData.IsFooterVisible);
            repositoryItemComboBoxBoolean.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            row.Properties.RowEdit = repositoryItemComboBoxBoolean;
            vGridControlProperty.Rows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Value = templateData.IsPageNumberVisible;
            row.Properties.Caption = "Show Page Number";
            row.Properties.FieldName = GetPropertyName(() => templateData.IsPageNumberVisible);
            repositoryItemComboBoxBoolean.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            row.Properties.RowEdit = repositoryItemComboBoxBoolean;
            vGridControlProperty.Rows.Add(row);

            if (templateData.DynamicProperty == null)
                templateData.DynamicProperty = new List<TemplateDynamicData>();
        }
        public static void AddStatistics(TemplateDynamicData property, RepositoryItemComboBox repositoryItemComboBoxBoolean, RepositoryItemComboBox repositoryItemComboBoxSize,
                                  RepositoryItemTextEdit repositoryItemTextEditNumberOnly, RepositoryItemMemoEdit repositoryItemTextEdit,
                                  VGridControl vGridControlProperty)
        {
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
            row.Properties.RowEdit = repositoryItemTextEdit;
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

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Size";
            row.Properties.RowEdit = repositoryItemComboBoxSize;
            row.Properties.Value = property.Statistics.GraphSize;
            row.Properties.FieldName = GetPropertyName(() => property.Statistics.GraphSize);
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
        public static void AddPageBreak(TemplateDynamicData property, RepositoryItemTextEdit repositoryItemTextEditNumberOnly,
            VGridControl vGridControlProperty)
        {
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
        public static void AddTextField(TemplateDynamicData property, RepositoryItemComboBox repositoryItemComboBoxBoolean,
                                        RepositoryItemComboBox repositoryItemComboBoxSize, RepositoryItemComboBox repositoryItemComboBoxStyle, 
                                        RepositoryItemTextEdit repositoryItemTextEditNumberOnly, 
                                        RepositoryItemMemoEdit repositoryItemTextEdit, VGridControl vGridControlProperty)
        {
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

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Value";
            row.Properties.RowEdit = repositoryItemTextEdit;
            row.Properties.Value = property.TextField;
            row.Properties.FieldName = GetPropertyName(() => property.Text.Text);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            /**
             * https://brightvision.jira.com/browse/PLATFORM-1887
             * fixed null object exception for property.Text
             */
            if (property.Text == null)
                property.Text = new TextField() {
                    Size = TextFieldFontSize.Small, 
                    Style = TextFieldFontStyle.None, 
                    Text = string.Empty  
                };

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Size";
            row.Properties.RowEdit = repositoryItemComboBoxSize;
            repositoryItemComboBoxSize.Items.AddRange(System.Enum.GetValues(typeof(TextFieldFontStyle)));
            row.Properties.Value = property.Text.Style;
            row.Properties.FieldName = GetPropertyName(() => property.Text.Size);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            row = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            row.Properties.Caption = "Style";
            row.Properties.RowEdit = repositoryItemComboBoxStyle;
            row.Properties.Value = property.Text.Style;
            row.Properties.FieldName = GetPropertyName(() => property.Text.Style);
            row.Tag = property;
            categoryRow.ChildRows.Add(row);

            vGridControlProperty.Rows.Add(categoryRow);
        }
        public static void SetValueTemplateProperty(object value, string fieldName, ref TemplateProperty propertyOwner)
        {
            PropertyInfo propertyInfo = propertyOwner.GetType().GetProperty(fieldName);
            propertyInfo.SetValue(propertyOwner, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }
        public static void SetValueStatisticsProperty(object value, string fieldName, ref StatisticsTemplate propertyOwner)
        {
            PropertyInfo propertyInfo = propertyOwner.GetType().GetProperty(fieldName);
            propertyInfo.SetValue(propertyOwner, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }
        public static void SetValueTemplateDynamicData(object value, string fieldName, ref TemplateDynamicData propertyOwner)
        {
            PropertyInfo propertyInfo = propertyOwner.GetType().GetProperty(fieldName);
            propertyInfo.SetValue(propertyOwner, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }
        private static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            return (propertyExpression.Body as MemberExpression).Member.Name;
        }
    }
}
