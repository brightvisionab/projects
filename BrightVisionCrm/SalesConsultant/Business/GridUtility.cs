
using System;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Menu;
using DevExpress.Utils.Menu;
using BrightVision.Common.Utilities;
using BrightVision.Model;
using BrightVision.Common.Business;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BrightVision.Common.UI;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using BrightVision.Windows.Azure.Storage.Blob;

namespace SalesConsultant.Business 
{
    public static class BrightSalesGridUtility
    {
        public class AdditionalMenuItems {
            public bool LoadNurtureEvent = false;
            public bool EditEvent = false;
            public bool SelectContact = false;
            public bool RefreshGrid = false;
        }

        public delegate void SelectContactOnClickEventHandler();
        public static event SelectContactOnClickEventHandler SelectContactOnClick;

        public delegate void EditEventOnClickEventHandler();
        public static event EditEventOnClickEventHandler EditEventOnClick;

        public delegate void LoadNurtureEventOnClickEventHandler();
        public static event LoadNurtureEventOnClickEventHandler LoadNurtureEventOnClick;

        public delegate void RefreshClickEventHandler();
        public static event RefreshClickEventHandler RefreshOnClick;

        public static void CreateGridContextMenu(GridView view, PopupMenuShowingEventArgs e, bool pDisplayNurtureEventMenus = false, AdditionalMenuItems pItems = null) 
        {
            // Check whether there are data in the grid.
            if (view.Columns.Count <= 0 && view.RowCount <= 0) return;
            // Check whether a row or user is right-clicked.
            if (e.MenuType == GridMenuType.Row || e.MenuType == GridMenuType.User) {
                int rowHandle = e.HitInfo.RowHandle;
                if (e.MenuType == GridMenuType.User) {
                    if (e.Menu == null) {
                        e.Menu = new GridViewMenu(view);
                    }
                }
                e.Menu.Items.Clear();

                /**
                 * menus for nurture event feature must come first before other menus.
                 */
                if (pDisplayNurtureEventMenus) {
                    if (pItems != null) {
                        if (pItems.SelectContact)
                            e.Menu.Items.Add(CreateSelectContact());
                        if (pItems.LoadNurtureEvent)
                            e.Menu.Items.Add(CreateLoadNurtureEvent());
                        if (pItems.EditEvent)
                            e.Menu.Items.Add(CreateEditEvent());
                    }
                }

                // Add a submenu with a single menu item.
                e.Menu.Items.Add(CreateCopyText(view, rowHandle));
                e.Menu.Items.Add(CreateExportMenu(view, rowHandle));
                e.Menu.Items.Add(CreatePrintingMenu(view, rowHandle));                

                if (view.Name == "gvEventLog") {
                    DXMenuItem saveAudioToLocationMenu = SaveAudioToLocation(view, rowHandle);
                    if (saveAudioToLocationMenu != null)
                        e.Menu.Items.Add(saveAudioToLocationMenu);
                }
                
                if(pItems != null)
                    if (pItems.RefreshGrid)
                        e.Menu.Items.Add(CreateRefreshMenu(view, rowHandle));

                e.Menu.CloseUp += Menu_CloseUp;
            }
        }

        private static void Menu_CloseUp(object sender, EventArgs e)
        {
            if (SelectContactOnClick != null) SelectContactOnClick = null;
            if (EditEventOnClick != null) EditEventOnClick = null;
            if (LoadNurtureEventOnClick != null) LoadNurtureEventOnClick = null;
            if (RefreshOnClick != null) RefreshOnClick = null;
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
        private static DXMenuItem SaveAudioToLocationOld(GridView view, int rowHandle)
        {
            Guid audioId = Guid.Empty;
            object objAudioId = view.GetFocusedRowCellValue("audio_id");
            if (objAudioId == null)
                return null;

            bool IsGuid = Guid.TryParse(objAudioId.ToString(), out audioId);
            if (IsGuid)
            {
                if (!AudioFileExist(view, audioId))
                {
                    var objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
                    var followup = objDbModel.event_followup_log.Where(param => param.audio_id == audioId).FirstOrDefault();
                    if (followup == null ||
                       !followup.main_uploaded == null ||
                       !(followup.main_uploaded.HasValue && followup.main_uploaded.Value))
                    {
                        return null;
                    }
                }
            }

            //Create Save Audio To Location MenuItem with Click Event
            var menuitem = new DXMenuItem { Caption = "Save Audio To Location", Shortcut = Shortcut.CtrlS };
            menuitem.Tag = new RowInfo(view, rowHandle);
            menuitem.Click += delegate(object sender, EventArgs e)
            {
                var commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
                string audioCachePath = String.Format("{0}\\cachewav\\{1}", commonData.ApplicationFolderPath, audioId + "_.mp3");
                string audioCachePath2 = String.Format("{0}\\cachewav\\{1}", commonData.ApplicationFolderPath, audioId + "_.wav");
                string audioTmpPath = String.Format("{0}\\tmpwav\\{1}", commonData.ApplicationFolderPath, audioId + "_.wav");

                if (File.Exists(audioCachePath))
                {
                    try
                    {
                        FileManagerUtility.SaveToLocationAudio(audioCachePath);
                    }
                    catch
                    {
                        NotificationDialog.Information("Cannot Play", "Cannot play audio. The audio is still in the process of downloading. Try again later.");
                    }
                }
                else if (File.Exists(audioCachePath2))
                {
                    try
                    {
                        FileManagerUtility.SaveToLocationAudio(audioCachePath2);
                    }
                    catch
                    {
                        NotificationDialog.Information("Cannot Play", "Cannot play audio. The audio is still in the process of downloading. Try again later.");
                    }
                }
                else if (File.Exists(audioTmpPath))
                {
                    try
                    {
                        File.Copy(audioTmpPath, audioTmpPath.Replace("tmpwav","cachewav"));
                        FileManagerUtility.SaveToLocationAudio(audioCachePath2);
                    }
                    catch
                    {
                        NotificationDialog.Information("Cannot Play", "Cannot play audio. The audio is being converted to mp3. Try again later.");
                    }
                }
                else
                {
                    audioId = Guid.Parse(objAudioId.ToString());
                    WaitDialog.Show("Downloading audio files....");
                    string fileUrl = FileManagerUtility.GetServerUrl(audioId);
                    string fileDir =  BrightSalesFacade.WebDavFile.AudioToCacheFolder(fileUrl);
                    Thread.Sleep(2000);
                    WaitDialog.Close();
                    if (fileUrl == null)
                        return;
                    try
                    {
                        FileManagerUtility.SaveToLocationAudio(fileDir);
                    }
                    catch
                    {
                        NotificationDialog.Information("Cannot Play", "Cannot play audio. The audio is being converted to mp3. Try again later.");
                    }
                }
            };
            return menuitem;
        }
        private static DXMenuItem SaveAudioToLocation(GridView view, int rowHandle)
        {

            object objOldAudioId = view.GetFocusedRowCellValue("audio_id");
            object objAudioId = view.GetFocusedRowCellValue("azure_blob_audio_id");
            if (objAudioId == null && objOldAudioId == null)
                return null;
            


            var menuitem = new DXMenuItem { Caption = "Save Audio To Location", Shortcut = Shortcut.CtrlS };
            //Create Save Audio To Location MenuItem with Click Event                
            menuitem.Tag = new RowInfo(view, rowHandle);
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

            string txt = info.View.GetFocusedDisplayText();
            if (!string.IsNullOrEmpty(txt))
            {
                Clipboard.SetText(txt);
            }
        }

        private static DXMenuItem CreatePrintingMenu(GridView view, int rowHandle)
        {
            DXSubMenuItem subMenu = new DXSubMenuItem("Print");
            DXMenuItem menuItemPrinting = new DXMenuItem("&Preview", new EventHandler(onPrintingClick), Properties.Resources.printer);
            menuItemPrinting.Tag = new RowInfo(view, rowHandle);
            subMenu.Items.Add(menuItemPrinting);
            return subMenu;
        }

        private static DXMenuItem CreateRefreshMenu(GridView view, int rowHandle)
        {
            var menuitem = new DXMenuItem { Caption = "Refresh" };
            menuitem.Click += new EventHandler(Refresh_Click);
            return menuitem;
        }
        private static void Refresh_Click(object sender, EventArgs e)
        {
            if (RefreshOnClick != null)
                RefreshOnClick();
        }

        private static DXMenuItem CreateExportMenu(GridView view, int rowHandle) {
            DXSubMenuItem subMenu = new DXSubMenuItem("Export");
            DXMenuItem menuItemExport = new DXMenuItem("&Excel Workbook (*.xls)", new EventHandler(OnExportToExcelClick), Properties.Resources.export_xls);
            menuItemExport.Tag = new RowInfo(view, rowHandle, ViewExportType.Excel2003);
            subMenu.Items.Add(menuItemExport);
            menuItemExport = new DXMenuItem("E&xcel Workbook (*.xlsx)", new EventHandler(OnExportToExcelClick), Properties.Resources.export_excel);
            menuItemExport.Tag = new RowInfo(view, rowHandle, ViewExportType.Excel2007);
            subMenu.Items.Add(menuItemExport);
            menuItemExport = new DXMenuItem("&CSV (Comma Delimited) (*.csv)", new EventHandler(OnExportToExcelClick), Properties.Resources.export_csv);
            menuItemExport.Tag = new RowInfo(view, rowHandle, ViewExportType.CSV);
            subMenu.Items.Add(menuItemExport);
            return subMenu;
        }

        private static void onPrintingClick(object sender, EventArgs e) {
            DXMenuItem item = sender as DXMenuItem;
            RowInfo info = item.Tag as RowInfo;
            if (!info.View.GridControl.IsPrintingAvailable) {
                NotificationDialog.Error("Printing", "The 'DevExpress.XtraPrinting' Library is not found.");
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

        /*
         * DAN: Commented code as it gets ambiguous with the BrightVision.Common.GridUtility
         */
        //public static void SelectRow(this GridView gv, string column, object value)
        //{
        //    for (int cnt = 0; cnt < gv.DataRowCount; cnt++)
        //    {
        //        object obj = gv.GetRowCellValue(cnt, column);
        //        if (obj.ToString() == value.ToString())
        //        {
        //            gv.FocusedRowHandle = cnt;
        //            gv.SelectRow(cnt);
        //            return;
        //        }
        //    }
        //}

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
            var menuitem = new DXMenuItem { Caption = "Load Nurture Task" };
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

    //enum ViewExportType {
    //    Unspecified = 0,
    //    Excel2003 = 1,
    //    Excel2007 = 2,
    //    CSV = 3
    //}
}
