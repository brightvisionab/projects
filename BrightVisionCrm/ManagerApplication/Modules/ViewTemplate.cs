using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraVerticalGrid.Rows;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BrightVision.Model;
using BrightVision.Common.Business;
using ManagerApplication.Business;
using BrightVision.Common.Utilities;

namespace ManagerApplication.Modules {
    public partial class ViewTemplate : DevExpress.XtraEditors.XtraUserControl {
        BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
        bool isLoadTemplate = false;
        
        public ViewTemplate(bool IsLoadTemplate) {
            isLoadTemplate = IsLoadTemplate;
            InitializeComponent();
            this.layoutControl1.AllowCustomizationMenu = false;
            SetValidationRules();
            BindGrid();
            if (isLoadTemplate) {
                lciTemplateName.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                lciSaveNewTemplate.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                //btnOverwrite.Text = "Load Template to Columns In View";
                //btnOverwrite.Text = "Load Grid Report Template";
                btnOverwrite.Text = "Load Template to Report";
                lciDelete.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            }
            else {
                btnOverwrite.Text = "Overwrite Selected Template";
                lciDelete.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            }
        }

        public string XMLConfig { get; set; }

        public ViewConfiguration ViewConfigurationModule { get; set; }

        private void BindGrid() {
            var listConfiguration = BPContext.FIGetViewConfigurationTemplate().ToList();
            gridControl1.DataSource = listConfiguration;
        }
        private void SetValidationRules() {
            CustomIsBlankValidationRule isBlankValidationRule = new CustomIsBlankValidationRule();
            isBlankValidationRule.ErrorText = "View name cannot be empty";
            isBlankValidationRule.ErrorType = ErrorType.Critical;
            dxValidationProvider1.SetValidationRule(txtTemplateName, isBlankValidationRule);
        }

        private string FilterXMLTemplate(string xml) {
            if (ViewConfigurationModule.DialogFields == null ||
                ViewConfigurationModule.DialogFields.Count <= 0) return xml;

            var dialogFields = ViewConfigurationModule.DialogFields;
            var comtypes = dialogFields.Select(x=> x.component_type);
            
            var xdoc = XDocument.Parse(xml);
            var xview = xdoc.Element("view");
            if (xview != null && xview.HasElements) {
                var xitems = xview.Elements("item");                
                //int newPos = 1;
                XElement source = null;
                for(int i=0;i<xitems.Count();) {
                    var item = xitems.ElementAtOrDefault(i);
                    if (item != null) {
                        source = item.Element("source");
                        if (source != null) {
                            if (item.Element("source").Value == "Dialog") {
                                var df = dialogFields.FirstOrDefault(x =>
                                            x.component_type == item.Element("component_type").Value &&
                                            x.field_name.ToLower() == item.Element("field_name").Value.ToLower() &&
                                            x.external_value.ToLower() == item.Element("external_value").Value.ToLower() &&
                                            x.field_index == item.Element("field_index").Value);
                                if (df != null) {
                                    //item.SetAttributeValue("position", newPos);
                                    //item.SetAttributeValue("position", i+1);
                                    item.SetAttributeValue("position", Convert.ToInt32(item.Attribute("position").Value));
                                    item.SetAttributeValue("id", df.id);
                                    item.SetElementValue("dialog_id", df.dialog_id);
                                    item.SetElementValue("questionlayout_id", df.questionlayout_id);
                                    i++;
                                    //newPos++;
                                } else {
                                    item.Remove();
                                }
                            } else {
                                var df = dialogFields.FirstOrDefault(x =>
                                           x.source == item.Element("source").Value &&
                                           x.field_name.ToLower() == item.Element("field_name").Value.ToLower());
                                if (df != null) {
                                    //item.SetAttributeValue("position", newPos);
                                    //item.SetAttributeValue("position", i+1);
                                    item.SetAttributeValue("position", Convert.ToInt32(item.Attribute("position").Value));
                                    item.SetAttributeValue("id", df.id);
                                    i++;
                                    //newPos++;
                                } else {
                                    item.Remove();
                                }
                            }
                        } else {
                            i++;
                        }
                    } else i++;
                }
                //todo add filter to dialog fields 
                var finalxml = XElement.Parse("<" + xdoc.Root.Name.LocalName + ">" + string.Concat(xitems) + "</" + xdoc.Root.Name.LocalName + ">");            
                return finalxml.ToString();
            }
            return xml;

            #region Original Code for Filtering XML Data
            /** /
            if (xview != null && xview.HasElements)
            {
                var xitems = xview.Elements("item");
                int newPos = 1;
                XElement source = null;
                for (int i = 0; i < xitems.Count(); )
                {
                    var item = xitems.ElementAtOrDefault(i);
                    if (item != null)
                    {
                        source = item.Element("source");
                        if (source != null)
                        {
                            if (item.Element("source").Value == "Dialog")
                            {
                                var df = dialogFields.FirstOrDefault(x =>
                                            x.component_type == item.Element("component_type").Value &&
                                            x.field_name.ToLower() == item.Element("field_name").Value.ToLower() &&
                                            x.external_value.ToLower() == item.Element("external_value").Value.ToLower() &&
                                            x.field_index == item.Element("field_index").Value);
                                if (df != null)
                                {
                                    item.SetAttributeValue("position", newPos);
                                    item.SetAttributeValue("id", df.id);
                                    item.SetElementValue("dialog_id", df.dialog_id);
                                    item.SetElementValue("questionlayout_id", df.questionlayout_id);
                                    i++;
                                    newPos++;
                                }
                                else
                                {
                                    item.Remove();
                                }
                            }
                            else
                            {
                                var df = dialogFields.FirstOrDefault(x =>
                                           x.source == item.Element("source").Value &&
                                           x.field_name.ToLower() == item.Element("field_name").Value.ToLower());
                                if (df != null)
                                {
                                    item.SetAttributeValue("position", newPos);
                                    item.SetAttributeValue("id", df.id);
                                    i++;
                                    newPos++;
                                }
                                else
                                {
                                    item.Remove();
                                }
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }
                    else i++;
                }
                //todo add filter to dialog fields 
                var finalxml = XElement.Parse("<" + xdoc.Root.Name.LocalName + ">" + string.Concat(xitems) + "</" + xdoc.Root.Name.LocalName + ">");
                return finalxml.ToString();
            }
            /**/
            #endregion
        }

        private void btnSaveNewTemplate_Click(object sender, EventArgs e) {
            if (!dxValidationProvider1.Validate()) return;
            if(string.IsNullOrEmpty(XMLConfig)) {
                if(MessageBox.Show("There are no fields that were added to the current configuration. Do you want to continue saving anyway?",
                    "Save New Template", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    ==  DialogResult.No) return;
            }
            view_configuration_template template = new view_configuration_template() {
                name = txtTemplateName.Text,
                xml_config = XMLConfig,
                created_by = UserSession.CurrentUser.UserId,
                created_date = DateTime.Now
            };
            BPContext.view_configuration_template.AddObject(template);
            BPContext.SaveChanges();
        }

        private void btnOverwrite_Click(object sender, EventArgs e) {
            if (isLoadTemplate) {
                LoadTemplate();
            } else {
                OverwriteTemplate();
            }
        }

        private void OverwriteTemplate() {
            if (!dxValidationProvider1.Validate()) return;
            if (gridView1.SelectedRowsCount <= 0) {
                MessageBox.Show("Please select a template to overwrite first.", "Save Template", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.ParentForm.DialogResult = DialogResult.None;
                return;
            }
            if (MessageBox.Show("Are you sure you want to overwrite the selected existing template?", "Save Template", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.No) {
                    this.ParentForm.DialogResult = DialogResult.None;
                    return;
            }
            var template = gridView1.GetRow(gridView1.FocusedRowHandle) as CTViewConfigurationTemplates;
            var modtemp = BPContext.view_configuration_template.Where(i => i.id == template.id).FirstOrDefault();
            modtemp.name = txtTemplateName.Text;
            modtemp.xml_config = XMLConfig;
            modtemp.modified_by = UserSession.CurrentUser.UserId;
            modtemp.modified_date = DateTime.Now;
            BPContext.SaveChanges();
        }

        private void LoadTemplate() {
            if (gridView1.SelectedRowsCount <= 0) {
                MessageBox.Show("Please select a template to load first.", "Load Template", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var template = gridView1.GetRow(gridView1.FocusedRowHandle) as CTViewConfigurationTemplates;
            var modtemp = BPContext.view_configuration_template.Where(i => i.id == template.id).FirstOrDefault();
            ViewConfigurationModule.ResetColumnsInView();
            var filteredXML = FilterXMLTemplate(modtemp.xml_config);
            ViewConfigurationModule.LoadXML(filteredXML);
            this.ParentForm.DialogResult = DialogResult.OK;
        }

        private void gridView1_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e) {
            if (gridView1.SelectedRowsCount <= 0) {               
                return;
            }
            var template = gridView1.GetRow(gridView1.FocusedRowHandle) as CTViewConfigurationTemplates;
            if (template != null) {
                txtTemplateName.Text = template.name;
                txtTemplateName.SelectAll();
            }
        }

        private void gridView1_DoubleClick(object sender, EventArgs e) {
            if (!isLoadTemplate) return;
            GridView view = (GridView)sender;
            GridHitInfo hitInfo = view.CalcHitInfo((e as MouseEventArgs).Location);
            if (hitInfo.InRow) {
                view.FocusedRowHandle = hitInfo.RowHandle;
                LoadTemplate();                
            }
        }

        private void gridView1_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenu(view, e);
        }

        private void btnDeleteTemplate_Click(object sender, EventArgs e)
        {
            if (gridView1.RowCount < 1)
                return;

            WaitDialog.Show("Deleting template.");
            CTViewConfigurationTemplates _item = gridView1.GetFocusedRow() as CTViewConfigurationTemplates;
            if (_item == null) {
                WaitDialog.Close();
                return;
            }

            view_configuration_template _template = BPContext.view_configuration_template.FirstOrDefault(i => i.id == _item.id);
            if (_template == null) {
                WaitDialog.Close();
                return;
            }

            BPContext.view_configuration_template.DeleteObject(_template);
            BPContext.SaveChanges();
            gridView1.DeleteRow(gridView1.FocusedRowHandle);
            WaitDialog.Close();
        }


    }
}
