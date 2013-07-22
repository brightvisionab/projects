using BrightVision.Common.Business;
using BrightVision.Common.UI;
using BrightVision.Model;
using DevExpress.Utils.Drawing;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Menu;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BrightVision.Common.Utilities
{
    public static class GridUtility 
    {
        public class AdditionalMenuItems {
            public bool LoadNurtureEvent = false;
            public bool EditEvent = false;
            public bool SelectContact = false;
        }

        public delegate void SelectContactOnClickEventHandler();
        public static event SelectContactOnClickEventHandler SelectContactOnClick;

        public delegate void EditEventOnClickEventHandler();
        public static event EditEventOnClickEventHandler EditEventOnClick;

        public delegate void LoadNurtureEventOnClickEventHandler();
        public static event LoadNurtureEventOnClickEventHandler LoadNurtureEventOnClick;

        public static void CreateGridContextMenu(GridView view, PopupMenuShowingEventArgs e)
        {
            // Check whether there are data in the grid.
            if (view.Columns.Count <= 0 && view.RowCount <= 0) return;
            // Check whether a row or user is right-clicked.
            if (e.MenuType == GridMenuType.Row || e.MenuType == GridMenuType.User)
            {
                int rowHandle = e.HitInfo.RowHandle;
                if (e.MenuType == GridMenuType.User)
                {
                    if (e.Menu == null)
                    {
                        e.Menu = new GridViewMenu(view);
                    }
                }
                e.Menu.Items.Clear();
                // Add a submenu with a single menu item.
                e.Menu.Items.Add(CreateExportMenu(view, rowHandle));
                e.Menu.Items.Add(CreatePrintingMenu(view, rowHandle));
                e.Menu.Items.Add(CreateCopyText(view, rowHandle));
            }
        }

        public static void CreateGridContextMenuCallLog(GridView view, PopupMenuShowingEventArgs e, object objOldAudioId, DevExpress.XtraEditors.XtraUserControl caller)
        {
            // Check whether there are data in the grid.
            if (view.Columns.Count <= 0 && view.RowCount <= 0) return;
            // Check whether a row or user is right-clicked.
            if (e.MenuType == GridMenuType.Row || e.MenuType == GridMenuType.User)
            {
                int rowHandle = e.HitInfo.RowHandle;
                if (e.MenuType == GridMenuType.User)
                {
                    if (e.Menu == null)
                    {
                        e.Menu = new GridViewMenu(view);
                    }
                }
                e.Menu.Items.Clear();
                // Add a submenu with a single menu item.
                //e.Menu.Items.Add(CreateExportMenu(view, rowHandle));
                //e.Menu.Items.Add(CreatePrintingMenu(view, rowHandle));
                e.Menu.Items.Add(CreateLoadCompanyView(view, rowHandle, caller));
                DXMenuItem saveAudioToLocationMenu = SaveAudioToLocation(view, rowHandle, objOldAudioId);
                if (saveAudioToLocationMenu != null)
                    e.Menu.Items.Add(saveAudioToLocationMenu);
            }
        }

        //public static void CreateGridContextMenu(GridView view, PopupMenuShowingEventArgs e, bool pDisplayNurtureEventMenus = false, AdditionalMenuItems pItems = null)
        //{
        //    // Check whether there are data in the grid.
        //    if (view.Columns.Count <= 0 && view.RowCount <= 0) return;
        //    // Check whether a row or user is right-clicked.
        //    if (e.MenuType == GridMenuType.Row || e.MenuType == GridMenuType.User)
        //    {
        //        int rowHandle = e.HitInfo.RowHandle;
        //        if (e.MenuType == GridMenuType.User)
        //        {
        //            if (e.Menu == null)
        //            {
        //                e.Menu = new GridViewMenu(view);
        //            }
        //        }
        //        e.Menu.Items.Clear();

        //        /**
        //         * menus for nurture event feature must come first before other menus.
        //         */
        //        if (pDisplayNurtureEventMenus)
        //        {
        //            if (pItems != null)
        //            {
        //                if (pItems.SelectContact)
        //                    e.Menu.Items.Add(CreateSelectContact());
        //                if (pItems.LoadNurtureEvent)
        //                    e.Menu.Items.Add(CreateLoadNurtureEvent());
        //                if (pItems.EditEvent)
        //                    e.Menu.Items.Add(CreateEditEvent());
        //            }
        //        }

        //        // Add a submenu with a single menu item.
        //        e.Menu.Items.Add(CreateCopyText(view, rowHandle));
        //        e.Menu.Items.Add(CreateExportMenu(view, rowHandle));
        //        e.Menu.Items.Add(CreatePrintingMenu(view, rowHandle));

        //        //if (view.Name == "gvEventLog")
        //        //{
        //        //    DXMenuItem saveAudioToLocationMenu = SaveAudioToLocation(view, rowHandle);
        //        //    if (saveAudioToLocationMenu != null)
        //        //        e.Menu.Items.Add(saveAudioToLocationMenu);
        //        //}
        //    }
        //}

        private static DXMenuItem CreateLoadCompanyView(GridView view, int rowHandle, DevExpress.XtraEditors.XtraUserControl caller)
        {
            var menuitem = new DXMenuItem { Caption = "Load Company View", Shortcut = Shortcut.CtrlC };
            menuitem.Tag = new RowInfo(view, rowHandle);
            menuitem.Click += new EventHandler((sender, args) => LoadCompanyView(sender, null, caller));
            return menuitem;
        }

        static void LoadCompanyView(object sender, EventArgs e, DevExpress.XtraEditors.XtraUserControl caller)
        {
            System.Reflection.MethodInfo mi = caller.GetType().GetMethod("LoadCompanyView");
            mi.Invoke(caller, null);
        }

        private static DXMenuItem SaveAudioToLocation(GridView view, int rowHandle, object objOldAudioId)
        {

            //object objOldAudioId = view.GetFocusedRowCellValue("audio_id");
            object objAudioId = view.GetFocusedRowCellValue("azure_blob_audio_id");
            if (objAudioId == null && objOldAudioId == null)
                return null;



            var menuitem = new DXMenuItem { Caption = "Save Audio To Location", Shortcut = Shortcut.CtrlS };
            //Create Save Audio To Location MenuItem with Click Event                
            menuitem.Tag = new RowInfo(view, rowHandle);
            menuitem.Image = Resources.save17;
            menuitem.Click += delegate(object sender, EventArgs e)
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    //dialog.ShowNewFolderButton = false;
                    //dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                    dialog.Filter = "MP3 file|*.mp3";
                    dialog.Title = "Save as";
                    dialog.FileName = "*.mp3";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        WaitDialog.Show("Saving audio files....");

                        BrightVision.Windows.Azure.Storage.Blob.WindowsAzureStorageBlob wab = new BrightVision.Windows.Azure.Storage.Blob.WindowsAzureStorageBlob();

                        string WindowsAzureStorageBlobAccountName = ConfigManager.AppSettings["WindowsAzureStorageBlobAccountName"].ToString();
                        string WindowsAzureStorageBlobAccountKey = ConfigManager.AppSettings["WindowsAzureStorageBlobAccountKey"].ToString();
                        string WindowsAzureStorageBlobContainerName = ConfigManager.AppSettings["WindowsAzureStorageBlobContainerName"].ToString();
                        string strAudioId = "";

                        if (objOldAudioId != null && objAudioId == null)
                        {
                            strAudioId = objOldAudioId + "_.mp3";

                            WindowsAzureStorageBlobContainerName = ConfigManager.AppSettings["WindowsAzureStorageBlobContainerNameOld"].ToString();
                        }
                        else
                        {
                            strAudioId = objAudioId + ".mp3";
                        }


                        if (wab.InitialzeWindowsAzureStorage(
                                WindowsAzureStorageBlobAccountName,
                                WindowsAzureStorageBlobAccountKey,
                                WindowsAzureStorageBlobContainerName
                            )
                        )
                        {
                            string msg = "";
                            if (wab.ProcessDownload(strAudioId, dialog.FileName, ref msg))
                            {
                                NotificationDialog.Information("Success", "Successfully save audio file.");

                                string argument = @"/select, " + dialog.FileName;
                                System.Diagnostics.Process.Start("explorer.exe", argument);
                            }
                            else
                            {
                                if (!wab.IsBlobFileExist(strAudioId))
                                {
                                    NotificationDialog.Error("Error", "Blob file does not exist.\nPlease consult system administrator.");
                                }
                                else
                                {
                                    NotificationDialog.Error("Error", "Unable save audio file.\nERROR: " + msg + "\nPlease consult system administrator.");
                                }
                            }
                        }
                        wab = null;

                        WaitDialog.Close();
                    }
                }
            };

            return menuitem;
        }


        private static DXMenuItem CreatePrintingMenu(GridView view, int rowHandle) {
            DXSubMenuItem subMenu = new DXSubMenuItem("Print");
            DXMenuItem menuItemPrinting = new DXMenuItem("&Preview", new EventHandler(onPrintingClick), BrightVision.Common.Resources.printer);
            menuItemPrinting.Tag = new RowInfo(view, rowHandle);
            subMenu.Items.Add(menuItemPrinting);
            return subMenu;
        }

        private static DXMenuItem CreateExportMenu(GridView view, int rowHandle) {
            DXSubMenuItem subMenu = new DXSubMenuItem("Export");
            DXMenuItem menuItemExport = new DXMenuItem("&Excel Workbook (*.xls)", new EventHandler(OnExportToExcelClick), BrightVision.Common.Resources.export_xls);
            menuItemExport.Tag = new RowInfo(view, rowHandle, ViewExportType.Excel2003);
            subMenu.Items.Add(menuItemExport);
            menuItemExport = new DXMenuItem("E&xcel Workbook (*.xlsx)", new EventHandler(OnExportToExcelClick), BrightVision.Common.Resources.export_excel);
            menuItemExport.Tag = new RowInfo(view, rowHandle, ViewExportType.Excel2007);
            subMenu.Items.Add(menuItemExport);
            menuItemExport = new DXMenuItem("&CSV (Comma Delimited) (*.csv)", new EventHandler(OnExportToExcelClick), BrightVision.Common.Resources.export_csv);
            menuItemExport.Tag = new RowInfo(view, rowHandle, ViewExportType.CSV);
            subMenu.Items.Add(menuItemExport);
            return subMenu;
        }

        private static DXMenuItem CreateCopyText(GridView view, int rowHandle)
        {
            var menuitem = new DXMenuItem { Caption = "Copy Text", Shortcut = Shortcut.CtrlC };
            menuitem.Tag = new RowInfo(view, rowHandle);
            menuitem.Click += new EventHandler(copyText_Click);
            return menuitem;
        }

        static void copyText_Click(object sender, EventArgs e)
        {
            DXMenuItem item = sender as DXMenuItem;
            RowInfo info = item.Tag as RowInfo;
            if (info == null || info.View == null)
                return;

            try {
                string txt = info.View.GetFocusedDisplayText();
                if (!string.IsNullOrEmpty(txt)) {
                    Clipboard.SetText(txt);
                }
            }
            catch {
                return;
            }
        }

        private static void onPrintingClick(object sender, EventArgs e) {
            DXMenuItem item = sender as DXMenuItem;
            RowInfo info = item.Tag as RowInfo;
            if (!info.View.GridControl.IsPrintingAvailable) {
                MessageBox.Show("The 'DevExpress.XtraPrinting' Library is not found", "Error");
                return;
            }
            info.View.GridControl.ShowPrintPreview();
        }

        private static void OnExportToExcelClick(object sender, EventArgs e) {
            DXMenuItem item = sender as DXMenuItem;
            RowInfo info = item.Tag as RowInfo;
            var exportType = info.ExportType;
            SaveFileDialog dialog1 = new SaveFileDialog();
            if (exportType == ViewExportType.Excel2003) {
                dialog1.Filter = "Excel Workbook (*.xls)|*.xls";
            } else if (exportType == ViewExportType.Excel2007) {
                dialog1.Filter = "Excel Workbook (*.xslx)|*.xlsx";
            } else if (exportType == ViewExportType.CSV) {
                dialog1.Filter = "CSV (Comma Delimited) (*.csv)|*.csv";
            }

            dialog1.Title = "Save As";
            dialog1.CheckPathExists = true;
            dialog1.CheckFileExists = false;
            if (dialog1.ShowDialog() == DialogResult.OK) {
                if (dialog1.FileName != "") {
                    if (dialog1.FilterIndex == 1) {
                        info.View.OptionsPrint.AutoWidth = false;
                        info.View.BestFitColumns();

                        FileStream fs = (FileStream)dialog1.OpenFile();
                        if (exportType == ViewExportType.CSV) {
                            DevExpress.XtraPrinting.CsvExportOptions opts = new DevExpress.XtraPrinting.CsvExportOptions();
                            info.View.Export(DevExpress.XtraPrinting.ExportTarget.Csv, fs, opts);
                        } else if (exportType == ViewExportType.Excel2007) {
                            DevExpress.XtraPrinting.XlsxExportOptions opts = new DevExpress.XtraPrinting.XlsxExportOptions();
                            opts.ExportMode = DevExpress.XtraPrinting.XlsxExportMode.SingleFile;
                            opts.SheetName = "Sheet1";
                            info.View.GridControl.ExportToXlsx(fs, opts);
                        } else if (exportType == ViewExportType.Excel2003) {
                            DevExpress.XtraPrinting.XlsExportOptions opts = new DevExpress.XtraPrinting.XlsExportOptions();
                            opts.ExportMode = DevExpress.XtraPrinting.XlsExportMode.SingleFile;
                            opts.SheetName = "Sheet1";
                            info.View.GridControl.ExportToXls(fs, opts);
                        }
                        fs.Close();
                    }

                }
            }
        }

        public static void SelectRow(this GridView gv, string fieldName, object value) {
            for (int cnt = 0; cnt < gv.DataRowCount; cnt++) {
                object obj = gv.GetRowCellValue(cnt, fieldName);
                if (obj.ToString() == value.ToString()) {
                     gv.FocusedRowHandle = cnt;
                     gv.SelectRow(cnt);
                     return;
                }
            }
        }

        private static DXMenuItem CreateSelectContact()
        {
            var menuitem = new DXMenuItem { Caption = "Select Contact" };
            menuitem.Click += new EventHandler(SelectContact_Click);
            return menuitem;
        }
        private static void SelectContact_Click(object sender, EventArgs e)
        {
            /**
             * just raise an event, and let the caller execute its logic.
             */
            if (SelectContactOnClick != null)
                SelectContactOnClick();
        }
        private static DXMenuItem CreateEditEvent()
        {
            var menuitem = new DXMenuItem { Caption = "Edit Task" };
            menuitem.Click += new EventHandler(CreateEditEvent_Click);
            return menuitem;
        }
        private static void CreateEditEvent_Click(object sender, EventArgs e)
        {
            /**
             * just raise an event, and let the caller execute its logic.
             */
            if (EditEventOnClick != null)
                EditEventOnClick();
        }
        private static DXMenuItem CreateLoadNurtureEvent()
        {
            var menuitem = new DXMenuItem { Caption = "Load Nurture Event" };
            menuitem.Click += new EventHandler(CreateLoadNurtureEvent_Click);
            return menuitem;
        }
        private static void CreateLoadNurtureEvent_Click(object sender, EventArgs e)
        {
            /**
             * just raise an event, and let the caller execute its logic.
             */
            if (LoadNurtureEventOnClick != null)
                LoadNurtureEventOnClick();
        }

        public static void DisableGridContextMenu(GridView view, PopupMenuShowingEventArgs e)
        {
            if (view.DataRowCount == 0)
            {
                for (int i = 0; i < e.Menu.Items.Count; i++)
                {
                    e.Menu.Items[i].Enabled = false;
                }
            }
        }

        private static bool AudioFileExist(GridView view, Guid id)
        {
            Guid audioId = Guid.Empty;
            string strAudioId = view.GetFocusedRowCellValue("audio_id").ToString();
            var commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
            string audioCachePath = String.Format("{0}\\cachewav\\{1}", commonData.ApplicationFolderPath, strAudioId + "_.mp3");
            string audioTmpPath = String.Format("{0}\\tmpwav\\{1}", commonData.ApplicationFolderPath, strAudioId + "_.wav");

            if (File.Exists(audioCachePath) || File.Exists(audioTmpPath))
            {
                return true;
            }
            return false;
        }
        //private static DXMenuItem SaveAudioToLocation(GridView view, int rowHandle)
        //{
        //    Guid audioId = Guid.Empty;
        //    object objAudioId = view.GetFocusedRowCellValue("audio_id");
        //    if (objAudioId == null)
        //        return null;

        //    bool IsGuid = Guid.TryParse(objAudioId.ToString(), out audioId);
        //    if (IsGuid)
        //    {
        //        if (!AudioFileExist(view, audioId))
        //        {
        //            var objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        //            var followup = objDbModel.event_followup_log.Where(param => param.audio_id == audioId).FirstOrDefault();
        //            if (followup == null ||
        //               !followup.main_uploaded == null ||
        //               !(followup.main_uploaded.HasValue && followup.main_uploaded.Value))
        //            {
        //                return null;
        //            }
        //        }
        //    }

        //    //Create Save Audio To Location MenuItem with Click Event
        //    var menuitem = new DXMenuItem { Caption = "Save Audio To Location", Shortcut = Shortcut.CtrlS };
        //    menuitem.Tag = new RowInfo(view, rowHandle);
        //    menuitem.Click += delegate(object sender, EventArgs e)
        //    {
        //        var commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
        //        string audioCachePath = String.Format("{0}\\cachewav\\{1}", commonData.ApplicationFolderPath, audioId + "_.mp3");
        //        string audioCachePath2 = String.Format("{0}\\cachewav\\{1}", commonData.ApplicationFolderPath, audioId + "_.wav");
        //        string audioTmpPath = String.Format("{0}\\tmpwav\\{1}", commonData.ApplicationFolderPath, audioId + "_.wav");

        //        if (File.Exists(audioCachePath))
        //        {
        //            try
        //            {
        //                FileManagerUtility.SaveToLocationAudio(audioCachePath);
        //            }
        //            catch
        //            {
        //                NotificationDialog.Information("Cannot Play", "Cannot play audio. The audio is still in the process of downloading. Try again later.");
        //            }
        //        }
        //        else if (File.Exists(audioCachePath2))
        //        {
        //            try
        //            {
        //                FileManagerUtility.SaveToLocationAudio(audioCachePath2);
        //            }
        //            catch
        //            {
        //                NotificationDialog.Information("Cannot Play", "Cannot play audio. The audio is still in the process of downloading. Try again later.");
        //            }
        //        }
        //        else if (File.Exists(audioTmpPath))
        //        {
        //            try
        //            {
        //                File.Copy(audioTmpPath, audioTmpPath.Replace("tmpwav", "cachewav"));
        //                FileManagerUtility.SaveToLocationAudio(audioCachePath2);
        //            }
        //            catch
        //            {
        //                NotificationDialog.Information("Cannot Play", "Cannot play audio. The audio is being converted to mp3. Try again later.");
        //            }
        //        }
        //        else
        //        {
        //            audioId = Guid.Parse(objAudioId.ToString());
        //            WaitDialog.Show("Downloading audio files....");
        //            string fileUrl = FileManagerUtility.GetServerUrl(audioId);
        //            string fileDir = BrightVision. BrightSalesFacade.WebDavFile.AudioToCacheFolder(fileUrl);
        //            //string fileDir = BrightSalesFacade.WebDavFile.AudioToCacheFolder(fileUrl);
        //            //Thread.Sleep(2000);
        //            WaitDialog.Close();
        //            if (fileUrl == null)
        //                return;
        //            try
        //            {
        //                //FileManagerUtility.SaveToLocationAudio(fileDir);
        //            }
        //            catch
        //            {
        //                NotificationDialog.Information("Cannot Play", "Cannot play audio. The audio is being converted to mp3. Try again later.");
        //            }
        //        }
        //    };
        //    return menuitem;
        //}
        private static bool IsFileLock(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                if (stream != null)
                    stream.Close();
                return true;

            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        static void SaveAudioToLocation_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        //DXMenuItem CreateRowSubMenu(GridView view, int rowHandle) {
        //    DXSubMenuItem subMenu = new DXSubMenuItem("Export");
        //    DXMenuItem menuItemExport = new DXMenuItem("&To Excel", new EventHandler(OnExportToXcelClick), Properties.Resources.info);
        //    menuItemExport.Tag = new RowInfo(view, rowHandle);
        //    subMenu.Items.Add(menuItemExport);

        //    DXMenuItem menuItemExport = new DXMenuItem("&To CSV", new EventHandler(OnExportClick), Properties.Resources.info);
        //    menuItemExport.Tag = new RowInfo(view, rowHandle);
        //    subMenu.Items.Add(menuItemExport);

        //    return subMenu;
        //}

        //DXMenuCheckItem CreateCellMergeMenuItem(GridView view, int rowHandle, bool beginGroup) {
        //    DXMenuCheckItem checkItem = new DXMenuCheckItem("&Merging Enabled",
        //      view.OptionsView.AllowCellMerge, null, new EventHandler(OnCellMergeClick));
        //    checkItem.Tag = new RowInfo(view, rowHandle);
        //    return checkItem;
        //}



        //void OnExportToCSVClick(object sender, EventArgs e) {
        //    DXMenuItem item = sender as DXMenuItem;
        //    RowInfo info = item.Tag as RowInfo;
        //    //info.View.DeleteRow(info.RowHandle);
        //}


        //void OnPrintClick(object sender, EventArgs e) {
        //    DXMenuCheckItem item = sender as DXMenuCheckItem;
        //    RowInfo info = item.Tag as RowInfo;
        //    //info.View.OptionsView.AllowCellMerge = item.Checked;
        //} 
    }

    class RowInfo {
        public RowInfo(GridView view, int rowHandle, ViewExportType exportType) {
            this.RowHandle = rowHandle;
            this.View = view;
            this.ExportType = exportType;
        }
        public RowInfo(GridView view, int rowHandle) {
            this.RowHandle = rowHandle;
            this.View = view;
        } 
        public GridView View;
        public int RowHandle;
        public ViewExportType ExportType;
    }

    public enum ViewExportType {
        Unspecified = 0,
        Excel2003 = 1,
        Excel2007 = 2,
        CSV = 3
    }

    public class GridCheckMarksSelection {
        protected GridView _view;
        protected ArrayList selection;
        GridColumn column;
        RepositoryItemCheckEdit edit;
        const int CheckboxIndent = 4;

        public GridCheckMarksSelection() {
            selection = new ArrayList();
        }

        public GridCheckMarksSelection(GridView view)
            : this() {
            View = view;
        }
        public GridView View {
            get { return _view; }
            set {
                if (_view != value) {
                    Detach();
                    Attach(value);
                }
            }
        }
        public GridColumn CheckMarkColumn { get { return column; } }

        public int SelectedCount { get { return selection.Count; } }
        public object GetSelectedRow(int index) {
            return selection[index];
        }
        public ArrayList GetSelectedRows { get { return selection; } }
        public int GetSelectedIndex(object row) {
            return selection.IndexOf(row);
        }
        public void ClearSelection() {
            selection.Clear();
            Invalidate();
        }
        public void SelectAll() {
            selection.Clear();
            // fast (won't work if the grid is filtered)
            //if(_view.DataSource is ICollection)
            //	selection.AddRange(((ICollection)_view.DataSource));
            //else
            // slow:
            for (int i = 0; i < _view.DataRowCount; i++)
                selection.Add(_view.GetRow(i));
            Invalidate();
        }
        public void SelectGroup(int rowHandle, bool select) {
            if (IsGroupRowSelected(rowHandle) && select) return;
            for (int i = 0; i < _view.GetChildRowCount(rowHandle); i++) {
                int childRowHandle = _view.GetChildRowHandle(rowHandle, i);
                if (_view.IsGroupRow(childRowHandle))
                    SelectGroup(childRowHandle, select);
                else
                    SelectRow(childRowHandle, select, false);
            }
            Invalidate();
        }
        public void SelectRow(int rowHandle, bool select) {
            SelectRow(rowHandle, select, true);
        }
        public void InvertRowSelection(int rowHandle) {
            if (View.IsDataRow(rowHandle)) {
                SelectRow(rowHandle, !IsRowSelected(rowHandle));
            }
            if (View.IsGroupRow(rowHandle)) {
                SelectGroup(rowHandle, !IsGroupRowSelected(rowHandle));
            }
        }
        public bool IsGroupRowSelected(int rowHandle) {
            for (int i = 0; i < _view.GetChildRowCount(rowHandle); i++) {
                int row = _view.GetChildRowHandle(rowHandle, i);
                if (_view.IsGroupRow(row)) {
                    if (!IsGroupRowSelected(row)) return false;
                } else
                    if (!IsRowSelected(row)) return false;
            }
            return true;
        }
        public bool IsRowSelected(int rowHandle) {
            if (_view.IsGroupRow(rowHandle))
                return IsGroupRowSelected(rowHandle);

            object row = _view.GetRow(rowHandle);
            return GetSelectedIndex(row) != -1;
        }

        protected virtual void Attach(GridView view) {
            if (view == null) return;
            selection.Clear();
            this._view = view;
            view.BeginUpdate();
            try {
                edit = view.GridControl.RepositoryItems.Add("CheckEdit") as RepositoryItemCheckEdit;

                column = view.Columns.Add();
                column.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
                column.Visible = true;
                column.VisibleIndex = 0;
                column.FieldName = "CheckMarkSelection";
                column.Caption = "Mark";
                column.OptionsColumn.ShowCaption = false;
                column.OptionsColumn.AllowEdit = false;
                column.OptionsColumn.AllowSize = false;
                column.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                column.Width = GetCheckBoxWidth();
                column.ColumnEdit = edit;

                view.Click += new EventHandler(View_Click);
                view.CustomDrawColumnHeader += new ColumnHeaderCustomDrawEventHandler(View_CustomDrawColumnHeader);
                view.CustomDrawGroupRow += new RowObjectCustomDrawEventHandler(View_CustomDrawGroupRow);
                view.CustomUnboundColumnData += new CustomColumnDataEventHandler(view_CustomUnboundColumnData);
                view.KeyDown += new KeyEventHandler(view_KeyDown);
                view.RowStyle += new RowStyleEventHandler(view_RowStyle);
            } finally {
                view.EndUpdate();
            }
        }
        protected virtual void Detach() {
            if (_view == null) return;
            if (column != null)
                column.Dispose();
            if (edit != null) {
                _view.GridControl.RepositoryItems.Remove(edit);
                edit.Dispose();
            }

            _view.Click -= new EventHandler(View_Click);
            _view.CustomDrawColumnHeader -= new ColumnHeaderCustomDrawEventHandler(View_CustomDrawColumnHeader);
            _view.CustomDrawGroupRow -= new RowObjectCustomDrawEventHandler(View_CustomDrawGroupRow);
            _view.CustomUnboundColumnData -= new CustomColumnDataEventHandler(view_CustomUnboundColumnData);
            _view.KeyDown -= new KeyEventHandler(view_KeyDown);
            _view.RowStyle -= new RowStyleEventHandler(view_RowStyle);

            _view = null;
        }
        protected int GetCheckBoxWidth() {
            DevExpress.XtraEditors.ViewInfo.CheckEditViewInfo info = edit.CreateViewInfo() as DevExpress.XtraEditors.ViewInfo.CheckEditViewInfo;
            int width = 0;
            GraphicsInfo.Default.AddGraphics(null);
            try {
                width = info.CalcBestFit(GraphicsInfo.Default.Graphics).Width;
            } finally {
                GraphicsInfo.Default.ReleaseGraphics();
            }
            return width + CheckboxIndent * 2;
        }
        protected void DrawCheckBox(Graphics g, Rectangle r, bool Checked) {
            DevExpress.XtraEditors.ViewInfo.CheckEditViewInfo info;
            DevExpress.XtraEditors.Drawing.CheckEditPainter painter;
            DevExpress.XtraEditors.Drawing.ControlGraphicsInfoArgs args;
            info = edit.CreateViewInfo() as DevExpress.XtraEditors.ViewInfo.CheckEditViewInfo;
            painter = edit.CreatePainter() as DevExpress.XtraEditors.Drawing.CheckEditPainter;
            info.EditValue = Checked;
            info.Bounds = r;
            info.CalcViewInfo(g);
            args = new DevExpress.XtraEditors.Drawing.ControlGraphicsInfoArgs(info, new DevExpress.Utils.Drawing.GraphicsCache(g), r);
            painter.Draw(args);
            args.Cache.Dispose();
        }
        void Invalidate() {
            _view.CloseEditor();
            _view.BeginUpdate();
            _view.EndUpdate();
        }
        void SelectRow(int rowHandle, bool select, bool invalidate) {
            if (IsRowSelected(rowHandle) == select) return;
            object row = _view.GetRow(rowHandle);
            if (select)
                selection.Add(row);
            else
                selection.Remove(row);
            if (invalidate) {
                Invalidate();
            }
        }
        void view_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e) {
            if (e.Column == CheckMarkColumn) {
                if (e.IsGetData)
                    e.Value = IsRowSelected(View.GetRowHandle(e.ListSourceRowIndex));
                else
                    SelectRow(View.GetRowHandle(e.ListSourceRowIndex), (bool)e.Value);
            }
        }
        void view_KeyDown(object sender, KeyEventArgs e) {
            if (View.FocusedColumn != column || e.KeyCode != Keys.Space) return;
            InvertRowSelection(View.FocusedRowHandle);
        }
        void View_Click(object sender, EventArgs e) {
            GridHitInfo info;
            Point pt = _view.GridControl.PointToClient(Control.MousePosition);
            info = _view.CalcHitInfo(pt);
            if (info.Column == column) {
                if (info.InColumn) {
                    if (SelectedCount == _view.DataRowCount)
                        ClearSelection();
                    else
                        SelectAll();
                }
                if (info.InRowCell) {
                    InvertRowSelection(info.RowHandle);
                }
            }
            if (info.InRow && _view.IsGroupRow(info.RowHandle) && info.HitTest != GridHitTest.RowGroupButton) {
                InvertRowSelection(info.RowHandle);
            }
        }
        void View_CustomDrawColumnHeader(object sender, ColumnHeaderCustomDrawEventArgs e) {
            if (e.Column == column) {
                e.Info.InnerElements.Clear();
                e.Painter.DrawObject(e.Info);
                DrawCheckBox(e.Graphics, e.Bounds, SelectedCount == _view.DataRowCount);
                e.Handled = true;
            }
        }
        void View_CustomDrawGroupRow(object sender, RowObjectCustomDrawEventArgs e) {
            DevExpress.XtraGrid.Views.Grid.ViewInfo.GridGroupRowInfo info;
            info = e.Info as DevExpress.XtraGrid.Views.Grid.ViewInfo.GridGroupRowInfo;

            info.GroupText = "         " + info.GroupText.TrimStart();
            e.Info.Paint.FillRectangle(e.Graphics, e.Appearance.GetBackBrush(e.Cache), e.Bounds);
            e.Painter.DrawObject(e.Info);

            Rectangle r = info.ButtonBounds;
            r.Offset(r.Width + CheckboxIndent * 2 - 1, 0);
            DrawCheckBox(e.Graphics, r, IsGroupRowSelected(e.RowHandle));
            e.Handled = true;
        }
        void view_RowStyle(object sender, RowStyleEventArgs e) {
            if (IsRowSelected(e.RowHandle)) {
                e.Appearance.BackColor = SystemColors.Highlight;
                e.Appearance.ForeColor = SystemColors.HighlightText;
            }
        }

    }
}
