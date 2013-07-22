using System;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;


namespace BrightVision.Common.UI {
    //The attribute that points to the registration method
    [UserRepositoryItem("RegisterBVPopupContainerEdit")]
    public class RepositoryItemBVPopupContainerEdit : RepositoryItemPopupContainerEdit {

        private System.Collections.Hashtable cacheSize;
        private DataTable dtSourceObject = null;

        //The static constructor which calls the registration method
        static RepositoryItemBVPopupContainerEdit() { RegisterBVPopupContainerEdit(); }

        //Initialize new properties
        public RepositoryItemBVPopupContainerEdit() : base() {
            this.cacheSize = null;
            this.cacheSize = new System.Collections.Hashtable();
            this.Buttons.Clear();
            this.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            this.NullText = "";
            this.AppearanceFocused.BackColor = Color.White;
            this.AppearanceFocused.BorderColor = Color.Silver;
            this.AppearanceFocused.Options.UseBackColor = true;
            this.AppearanceFocused.Options.UseBorderColor = true;
            this.BorderStyle = BorderStyles.Simple;
            this.ShowPopupCloseButton = false;
            this.ValidateOnEnterKey = true;            
            this.PopupControl = PopupContainerControl;            
            this.PopupControl.Show();
            this.PopupControl.Hide();
            this.EditValueChanged += (sender, e) => {
                var edit = sender as BVPopupContainerEdit;
                if (edit.IsPopupOpen) {
                    var cancelArgs = new CancelEventArgs();
                    FilterData(sender, cancelArgs);
                    if (cancelArgs.Cancel) edit.ClosePopup();
                } else {
                    edit.ShowPopup();                    
                }
                edit.Focus();
            };
            
            this.QueryPopUp += (sender, e) => {
                FilterData(sender, e);
            };
            this.Enter += (sender, e) => {
                var edit = sender as BVPopupContainerEdit;
                if (edit != null) {
                    if(edit.IsPopupOpen)
                        edit.ClosePopup();
                }
            };         
            this.QueryResultValue += (sender, e) => {
                var edit = sender as BVPopupContainerEdit;
                if (edit != null) {                    
                    var data = edit.Properties.PopupControl.Tag as BVPopupContainerControlData;
                    if (data != null && data.View != null && data.HasSelection) {
                        //set editor backcolor to green                        
                        edit.Properties.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                        edit.Properties.Appearance.BackColor = Color.FromArgb(181, 245, 146);//green
                        edit.Properties.Appearance.Options.UseBackColor = true;
                        edit.Properties.Appearance.Options.UseForeColor = true;
                        data.HasSelection = false;
                        var datarow = data.View.GetDataRow(data.View.FocusedRowHandle) as DataRow;
                        if (datarow != null) {
                            if (e.Value == datarow["name"]) return;
                            e.Value = datarow["name"];
                            edit.SelectionStart = edit.Text.Length;
                            var grid = edit.Parent as GridControl;
                            if (grid != null) {
                                var view = grid.FocusedView as GridView;
                                if (view != null) {
                                    view.SetRowCellValue(view.FocusedRowHandle, "title_id", datarow["id"]);
                                    view.SetRowCellValue(view.FocusedRowHandle, "title", datarow["name"]);
                                } else {
                                    var lview = grid.FocusedView as DevExpress.XtraGrid.Views.Layout.LayoutView;
                                    if (lview != null) {
                                        lview.SetRowCellValue(lview.FocusedRowHandle, "title_id", datarow["id"]);
                                        lview.SetRowCellValue(lview.FocusedRowHandle, "title", datarow["name"]);
                                    }
                                }
                            } else {
                                var vgrid = edit.Parent as DevExpress.XtraVerticalGrid.VGridControl;
                                if (vgrid != null) {
                                    vgrid.SetCellValue(vgrid.GetRowByFieldName("title_id"), vgrid.FocusedRecord, datarow["id"]);
                                    vgrid.SetCellValue(vgrid.GetRowByFieldName("title"), vgrid.FocusedRecord, datarow["name"]);
                                } else {
                                    //layout control
                                    //note use this to get the edit value of the popupcontaineredit control
                                    edit.Tag = new object[] { datarow["id"], datarow["name"] };
                                    edit.IsModified = true;
                                }
                            }
                        }
                    }
                }
            };           
            
            this.KeyDown += (sender, e) => {
                var edit = sender as PopupContainerEdit;
                if (edit != null) {
                    var data = edit.Properties.PopupControl.Tag as BVPopupContainerControlData;
                    if (data != null && data.View != null) {
                        var view = data.View;
                        if (e.KeyCode == Keys.Down) {
                            if (!edit.IsPopupOpen)
                                edit.ShowPopup();                                
                            else 
                                view.MoveNext();
                            edit.SelectionStart = edit.Text.Length;
                        } else if (e.KeyCode == Keys.Up) {
                            view.MovePrev();
                            edit.SelectionStart = edit.Text.Length;
                        } else if (e.KeyCode == Keys.PageUp) {
                            view.MoveFirst();
                            edit.SelectionStart = edit.Text.Length;
                        } else if (e.KeyCode == Keys.PageDown) {
                            view.MoveLast();
                            edit.SelectionStart = edit.Text.Length;
                        } else if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter) {                            
                            data.HasSelection = true;
                            edit.ClosePopup();                            
                            if (edit.Parent != null) {
                                var gControl = edit.Parent as DevExpress.XtraGrid.GridControl;
                                if (gControl != null) {
                                    gControl.FocusedView.CloseEditor();
                                } else {
                                    var vControl = edit.Parent as DevExpress.XtraVerticalGrid.VGridControl;
                                    if (vControl != null) {
                                        vControl.CloseEditor();
                                    }
                                }

                            }
                        }
                    }
                }
            };
        }

        protected override void OnLoaded() {
            base.OnLoaded();
            if (IsDesignMode) return;
        }

        //The unique name for the custom editor
        internal const string BVPopupContainerEditName = "BVPopupContainerEdit";

        //Return the unique name
        public override string EditorTypeName { get { return BVPopupContainerEditName; } }

        [Browsable(false)]
        public new BVPopupContainerEdit OwnerEdit {
            get {
                return base.OwnerEdit as BVPopupContainerEdit;
            }
        }

        public System.Collections.Hashtable CacheSize {
            get {
                return cacheSize;
            }
        }

        //Register the editor
        public static void RegisterBVPopupContainerEdit() {
            //Icon representing the editor within a container editor's Designer
            Image img = null;
            EditorRegistrationInfo.Default.Editors.Add(new EditorClassInfo(BVPopupContainerEditName,
              typeof(BVPopupContainerEdit), typeof(RepositoryItemBVPopupContainerEdit),
              typeof(PopupContainerEditViewInfo), new ButtonEditPainter(), true, img));
        }

        //Override the Assign method
        public override void Assign(RepositoryItem item) {
            BeginUpdate();
            try {
                base.Assign(item);
                RepositoryItemBVPopupContainerEdit source = item as RepositoryItemBVPopupContainerEdit;
                if (source == null) return;
                this.cacheSize = source.cacheSize;
                UseDefaultMode = source.UseDefaultMode;
            } finally {
                EndUpdate();
            }
        }

        public void FilterData(object sender, CancelEventArgs e) {
            var ctl = sender as PopupContainerEdit;
            if (ctl != null) {
                ctl.IsModified = true;
                var popCon = ctl.Properties.PopupControl;
                var popUpdata = popCon.Tag as BVPopupContainerControlData;
                int results = BindPopupGridDataSource(popUpdata, ctl.Text);
                if (results < 1) e.Cancel = true;

                bool hasMatch = popUpdata.MatchKeyword(ctl.Text.Trim());
                if (hasMatch) {
                    ctl.Properties.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                    ctl.Properties.Appearance.BackColor = Color.FromArgb(181, 245, 146);//green
                    ctl.Properties.Appearance.Options.UseBackColor = true;
                    ctl.Properties.Appearance.Options.UseForeColor = true;
                } else {
                    ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                    ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);//red
                    ctl.Properties.Appearance.Options.UseBackColor = true;
                    ctl.Properties.Appearance.Options.UseForeColor = true;
                }
                ctl.Focus();
            }
        }

        public PopupContainerControl PopupContainerControl {
            get {
                var popCon = new PopupContainerControl();
                var lc = new LayoutControl();
                var lcg = new LayoutControlGroup();
                var lci1 = new LayoutControlItem();
                var lci2 = new LayoutControlItem();

                var rdg = new RadioGroup();
                rdg.Properties.Appearance.BackColor = Color.Transparent;
                rdg.Properties.Appearance.Options.UseBackColor = true;
                rdg.Properties.Items.AddRange(new RadioGroupItem[] {
                    new RadioGroupItem(((byte)(0)),"Popularity", true),
                    new RadioGroupItem(((byte)(1)),"Alphabetical order")
                });
                rdg.SelectedIndex = 0;
                rdg.Size = new Size(252, 240);
                rdg.StyleController = lc;
                rdg.SelectedIndexChanged += (sender, e) => {
                    string filter = "";
                    if (popCon.OwnerEdit != null)
                        filter = popCon.OwnerEdit.EditValue.ToString();
                    BindPopupGridDataSource(popCon.Tag as BVPopupContainerControlData, filter);
                };

                var pcgCol1 = new GridColumn(); //id
                pcgCol1.Caption = "ID";
                pcgCol1.FieldName = "id";
                pcgCol1.Visible = false;

                var pcgCol2 = new GridColumn(); //ssyk
                pcgCol2.Caption = "SSYK";
                pcgCol2.FieldName = "ssyk";
                pcgCol2.Visible = true;
                pcgCol2.OptionsColumn.AllowEdit = false;
                pcgCol2.VisibleIndex = 1;
                pcgCol2.Width = 70;

                var pcgCol3 = new GridColumn(); //name
                pcgCol3.Caption = "Name";
                pcgCol3.FieldName = "name";
                pcgCol3.Visible = true;
                pcgCol3.OptionsColumn.AllowEdit = false;
                pcgCol3.VisibleIndex = 0;
                pcgCol3.Width = 170;

                var pcgControl = new GridControl();

                var pcgView = new GridView();
                pcgView.Columns.AddRange(new GridColumn[] { pcgCol1, pcgCol2, pcgCol3 });
                pcgView.GridControl = pcgControl;
                pcgView.OptionsFind.AlwaysVisible = false;
                pcgView.OptionsSelection.EnableAppearanceFocusedCell = false;
                pcgView.OptionsSelection.MultiSelect = false;
                pcgView.OptionsView.ShowGroupPanel = false;
                pcgView.OptionsView.ColumnAutoWidth = true;
                pcgView.OptionsView.ShowColumnHeaders = false;
                pcgView.RowClick += (sender, e) => {
                    var view = sender as GridView;
                    var popControl = view.GridControl.Parent.Parent as PopupContainerControl;
                    var data = popControl.Tag as BVPopupContainerControlData;
                    data.HasSelection = true;
                    var edit = popControl.OwnerEdit;
                    if (edit != null) {
                        edit.ClosePopup();
                        edit.SelectionStart = edit.Text.Length;
                    }
                    view.CloseEditor();

                    if (edit.Parent != null) {
                        var gControl = edit.Parent as DevExpress.XtraGrid.GridControl;
                        if (gControl != null) {
                            gControl.FocusedView.CloseEditor();
                        } else {
                            var vControl = edit.Parent as DevExpress.XtraVerticalGrid.VGridControl;
                            if (vControl != null) {
                                vControl.CloseEditor();
                            }
                        }

                    }
                };
                pcgView.KeyUp += (sender, e) => {
                    var view = sender as GridView;
                    if (e.KeyCode != Keys.Enter || view.RowCount < 1)
                        return;
                    var popControl = view.GridControl.Parent.Parent as PopupContainerControl;
                    var data = popControl.Tag as BVPopupContainerControlData;
                    data.HasSelection = true;
                    var edit = popControl.OwnerEdit;
                    if (edit != null) {
                        edit.ClosePopup();
                        edit.SelectionStart = edit.Text.Length;
                    }
                    view.CloseEditor();       
                   
                    if(edit.Parent != null) {
                        var gControl = edit.Parent as DevExpress.XtraGrid.GridControl;
                        if (gControl != null) {
                            gControl.FocusedView.CloseEditor();
                        } else {
                            var vControl = edit.Parent as DevExpress.XtraVerticalGrid.VGridControl;
                            if (vControl != null) {
                                vControl.CloseEditor();
                            }
                        }

                    }
                };

                pcgControl.MainView = pcgView;
                pcgControl.ViewCollection.AddRange(new BaseView[] { pcgView });

                lci1.Control = pcgControl;
                lci1.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
                lci2.SizeConstraintsType = SizeConstraintsType.Custom;
                lci1.Size = new Size(300, 180);
                lci2.MaxSize = new Size(0, 180);
                lci1.TextVisible = false;

                lci2.Control = rdg;
                lci2.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 1, 0);
                lci2.SizeConstraintsType = SizeConstraintsType.Custom;
                lci2.Size = new Size(252, 30);
                lci2.MaxSize = new Size(0, 30);
                lci2.TextVisible = false;

                lc.Controls.Add(pcgControl);
                lc.Controls.Add(rdg);
                lc.Dock = DockStyle.Fill;
                lc.Root = lcg;
                lc.Padding = new Padding(0, 0, 0, 0);

                lcg.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
                lcg.GroupBordersVisible = false;
                lcg.AddItem(lci1);
                lcg.AddItem(lci2);
                lcg.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);

                popCon.Controls.Add(lc);
                popCon.Size = new Size(270, 180);
                popCon.BackColor = Color.White;
                popCon.BorderStyle = BorderStyles.NoBorder;

                var popUpContainerData = new BVPopupContainerControlData() { View = pcgView, Group = rdg };
                popCon.Tag = popUpContainerData;
                popCon.Padding = new Padding(1, 1, 1, 0);

                pcgControl.DataSource = DataSourceObject;
                pcgControl.ForceInitialize();
                
                return popCon;
            }
        }

        public int BindPopupGridDataSource(BVPopupContainerControlData controlData, string filterExpression) {
            DataTable dt = null;
            byte order = 0;
            if (controlData.Group != null && controlData.Group.EditValue != null)
                order = Convert.ToByte(controlData.Group.EditValue);
            var sortOrder = order == 0 ? "occurences DESC" : "name ASC";
            string filter = "", exactFilter = "";
            if (!string.IsNullOrEmpty(filterExpression)) {
                filter = "name LIKE '%" + filterExpression.Replace("'", "''") + "%'";
                exactFilter = "name = '" + filterExpression.Replace("'", "''") + "'";
            }
            var view = controlData.View;            
            DataRow[] drMatch = null;
            if (view != null && view.GridControl != null) {
                view.GridControl.DataSource = null;
                dt = DataSourceObject.Copy();
                dt.DefaultView.Sort = sortOrder;
                drMatch = dt.Select(exactFilter);
                if(drMatch != null && drMatch.Length > 0) {
                    view.GridControl.DataSource = dt;
                    view.GridControl.ForceInitialize();                    
                    for (int x = 0; x < view.DataRowCount; ++x) {
                        if (drMatch[0].Equals(view.GetDataRow(x))) {
                            view.FocusedRowHandle = x;
                            view.MakeRowVisible(x);
                            break;
                        }
                    }
                    
                } else {
                    dt = DataSourceObject.Clone();
                    drMatch = DataSourceObject.Select(filter, sortOrder);
                    drMatch.CopyToDataTable(dt, LoadOption.PreserveChanges);
                    view.GridControl.DataSource = dt;
                    view.FocusedRowHandle = -1;
                }                
            }
            return drMatch != null ? drMatch.Length : 0;
        }

        public DataTable DataSourceObject {
            get {
                if (dtSourceObject == null) {
                    dtSourceObject = (DataTable) BrightVision.Common.Business.UserSession.CurrentUser.TitleList;
                    if (dtSourceObject == null) {
                        dtSourceObject = Utilities.DatabaseUtility.ExecuteStoredProcedure("bvGetTitles_sp", null);
                    }
                }
                return dtSourceObject;
            }
        }

        //A custom property 
        private bool useDefaultMode;

        public bool UseDefaultMode {
            get { return useDefaultMode; }
            set {
                if (useDefaultMode != value) {
                    useDefaultMode = value;
                    OnPropertiesChanged();
                }
            }
        }
    }


    public class BVPopupContainerEdit : PopupContainerEdit {

        //The static constructor which calls the registration method
        static BVPopupContainerEdit() {
            RepositoryItemBVPopupContainerEdit.RegisterBVPopupContainerEdit();
        }

        //Initialize the new instance
        public BVPopupContainerEdit(bool UseDefautMode) : base() {
            this.Properties.UseDefaultMode = UseDefautMode;            
            this.Properties.AppearanceFocused.BackColor = Color.Transparent;
            this.Properties.AppearanceFocused.BorderColor = Color.Transparent;
            this.Properties.AppearanceFocused.Options.UseBackColor = true;
            this.Properties.AppearanceFocused.Options.UseBorderColor = true;
            this.Properties.BorderStyle = BorderStyles.Default;
            
            this.Properties.ValidateOnEnterKey = true;
            this.Enter += (sender, e) => {
                var edit = sender as BVPopupContainerEdit;
                if (edit != null) {
                    if (edit.IsPopupOpen)
                        edit.ClosePopup();
                }
            };

            this.EditValueChanged += (sender, e) => {
                var edit = sender as PopupContainerEdit;
                edit.DoValidate();
            };

            this.KeyDown += (sender, e) => {
                var edit = sender as PopupContainerEdit; 
                if (edit != null) {
                    var data = edit.Properties.PopupControl.Tag as BVPopupContainerControlData;
                    if (data != null && data.View != null) {
                        var view = data.View;
                        if (e.KeyCode == Keys.Down) {
                            if (!edit.IsPopupOpen)
                                edit.ShowPopup();
                            else
                                view.MoveNext();
                            edit.SelectionStart = edit.Text.Length;
                        } else if (e.KeyCode == Keys.Up) {
                            view.MovePrev();
                            edit.SelectionStart = edit.Text.Length;
                        } else if (e.KeyCode == Keys.PageUp) {
                            view.MoveFirst();
                            edit.SelectionStart = edit.Text.Length;
                        } else if (e.KeyCode == Keys.PageDown) {
                            view.MoveLast();
                            edit.SelectionStart = edit.Text.Length;
                        } else if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter) {
                            data.HasSelection = true;
                            edit.ClosePopup();
                            edit.SelectionStart = edit.Text.Length;
                            if (edit.Parent != null) {
                                var gControl = edit.Parent as DevExpress.XtraGrid.GridControl;
                                if (gControl != null) {
                                    gControl.FocusedView.CloseEditor();
                                } else {
                                    var vControl = edit.Parent as DevExpress.XtraVerticalGrid.VGridControl;
                                    if (vControl != null) {
                                        vControl.CloseEditor();
                                    }
                                }

                            }
                        }
                    }
                }
            };
            
        }

        public BVPopupContainerEdit() : this(false) {                         
        }

        //Return the unique name
        public override string EditorTypeName {
            get {
                return
                    RepositoryItemBVPopupContainerEdit.BVPopupContainerEditName;
            }
        }

        //Override the Properties property
        //Simply type-cast the object to the custom repository item type
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new RepositoryItemBVPopupContainerEdit Properties {
            get { return base.Properties as RepositoryItemBVPopupContainerEdit; }
        }
        
    }

    public class BVPopupContainerControlData {
        public GridView View { get; set; }
        public RadioGroup Group { get; set; }
        public bool HasSelection { get; set; }
        public bool HasMatch { get; set; }
        public bool MatchKeyword(string keyword) {
            DataTable dt = (DataTable)BrightVision.Common.Business.UserSession.CurrentUser.TitleList;
            if (dt == null) return false;
            DataRow[] drs = dt.Select("name='" + keyword.Replace("'", "''") + "'");
            if (drs != null && drs.Length > 0) {
                HasMatch = true;
                return true;                
            }
            HasMatch = false;
            return false;
        }
    }
}
