
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using SalesConsultant.Business;
using SalesConsultant.Forms;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using System.IO;
using BrightVision.Common.UI;
using System.Threading;
using DevExpress.Data.Linq;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using SalesConsultant.Facade;
using SalesConsultant.PublicProperties;

namespace SalesConsultant.Modules
{
    public partial class ViewCallLogs : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public ViewCallLogs()
        {
            InitializeComponent();
            //LoadCalls();
            this.ParentChanged += new EventHandler(ViewCallLogs_ParentChanged);
            gridColumnCallLength.DisplayFormat.Format = new CustomTimeSpanFormatter();
            gridColumnCallLength.SortMode = DevExpress.XtraGrid.ColumnSortMode.Custom;
            gvCallLog.CustomColumnSort += gvCallLog_CustomColumnSort;
        }
        #endregion

        #region Public Events & Args
        #endregion

        #region Subscribed Events
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private BrightPlatformEntities m_efDbContext = null;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        //private LinqServerModeSource m_lsmsCallLogData = null;
        private CallLogPlayer _player;
        private sub_campaign_account_lists m_CurrentAccount = null;

        //private CTMyFollowUps m_objFollowUp = null;
        //private event_followup_log m_EventFollowUpLog = null;
        private MyFollowUps m_objMyFollowUps = null;
        #endregion

        #region Public Methods
        public void GetCallLogs(string search = "")
        {
            gcCallLog.DataSource = null;
            //m_lsmsCallLogData = null;
            m_efDbContext = null;

            m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)
            {
                CommandTimeout = 0
            };


            try
            {
                /*
                m_efDbContext.FIPopulateCallLogs(UserSession.CurrentUser.UserId);
                //m_efDbContext.vw_call_logs.Where("");
                m_lsmsCallLogData = new LinqServerModeSource()
                {
                    KeyExpression = "uid",
                    QueryableSource = from CallLog in m_efDbContext.vw_call_logs
                                      where CallLog.user_id == UserSession.CurrentUser.UserId
                                      select CallLog
                };
                m_lsmsCallLogData.ExceptionThrown += new LinqServerModeExceptionThrownEventHandler(m_lsmsCallLogData_ExceptionThrown);
                m_lsmsCallLogData.InconsistencyDetected += new LinqServerModeInconsistencyDetectedEventHandler(m_lsmsCallLogData_InconsistencyDetected);
                gcCallLog.DataSource = m_lsmsCallLogData;
                gvCallLog.Columns["id"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
                gvCallLog.BestFitColumns();
                */
            }
            catch
            {
            }

            //using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            //{
            //    gcCallLog.DataSource = null;
            //    _lstData = _efDbContext.FIGetCallLogs().ToList();
            //}
            //gcCallLog.DataSource = _lstData;
            //gvCallLog.BestFitColumns();
        }

        private void m_lsmsCallLogData_InconsistencyDetected(object sender, LinqServerModeInconsistencyDetectedEventArgs e)
        {
            //this.BeginInvoke(new MethodInvoker(delegate {
            //    WaitDialog.Show("Reloading data ...");
            //    ((LinqServerModeSource)sender).Reload();
            //    //gcCallLog.DataSource = m_lsmsCallLogData;
            //    WaitDialog.Close();
            //}));
        }
        private void m_lsmsCallLogData_ExceptionThrown(object sender, LinqServerModeExceptionThrownEventArgs e)
        {
            //this.BeginInvoke(new MethodInvoker(delegate {
            //    WaitDialog.Show("Reloading data ...");
            //    ((LinqServerModeSource)sender).Reload();
            //    //gcCallLog.DataSource = m_lsmsCallLogData;
            //    WaitDialog.Close();
            //}));
        }
        #endregion

        #region Private Methods
        //private void LoadLatestCalls() {
        //    if (collection.Count > 0) {
                
        //        CTGetViewCallLog callLog = collection[0];
        //        m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)
        //        {
        //            CommandTimeout = 0
        //        };

        //        var result = m_efDbContext.FIGetLastestViewCallLog(callLog.date_of_transaction_datetime);
        //        foreach (CTGetLastestViewCallLog latest in result) {
        //            CTGetViewCallLog newCallLog = new CTGetViewCallLog { 
        //                assigned_user = latest.assigned_user,
        //                audio_file = latest.audio_file,
        //                call_length = latest.call_length,
        //                campaign_name = latest.campaign_name,
        //                company_name = latest.company_name,
        //                contact_name = latest.contact_name,
        //                contact_no = latest.contact_no,
        //                contact_title = latest.contact_title,
        //                customer_name = latest.customer_name,
        //                date_of_transaction = latest.date_of_transaction,
        //                date_of_transaction_datetime = latest.date_of_transaction_datetime,
        //                event_status = latest.event_status,
        //                id = latest.id,
        //                main_uploaded = latest.main_uploaded,
        //                row_num = latest.row_num,
        //                short_message = latest.short_message,
        //                sub_campaign_name = latest.sub_campaign_name,
        //                uid = Guid.Empty,
        //                user_id = latest.user_id
        //            };
        //            this.Invoke(new Action(delegate() {
        //                collection.Insert(0, newCallLog);
        //            }));
                    
        //        }

        //    }
        //}
        private void LoadCalls()
        {
            //gcCallLog.DataSource = collection;
            //BackgroundWorker work = new BackgroundWorker();
            //work.DoWork += new DoWorkEventHandler(work_DoWork);
            //work.RunWorkerAsync();

            try
            {
                int SubCampaignId = m_BrightSalesProperty.CommonProperty.SubCampaignId;
                if (SubCampaignId <= 0) return;

                WaitDialog.Show(ParentForm, "Loading call logs...");
                cmdRefreshData.Enabled = false;
                //SqlCommand _scCommand = new SqlCommand("bvGetCallLogs_sp");
                //_scCommand.Parameters.Add("@p_user_id", SqlDbType.Int).Value = Convert.ToInt32(UserSession.CurrentUser.UserId);
                //_scCommand.Parameters.Add("@p_top_rows", SqlDbType.Int).Value = Convert.ToInt32(tbxRecordsToShow.Text);
                //DataTable _dtCallLogs = DatabaseUtility.ExecuteSqlQuery(_scCommand);
                m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)
                {
                    CommandTimeout = 0
                };

                List<CTCallLog> _lstData = new List<CTCallLog>();

                bool ShowOnlyMyCalls = cbxShowOnlyMyCalls.Checked;
                _lstData = m_efDbContext.FIGetCallLogs(UserSession.CurrentUser.UserId, Convert.ToInt32(tbxRecordsToShow.Text), ShowOnlyMyCalls, SubCampaignId).ToList();

                gcCallLog.DataSource = null;
                if (_lstData.Count > 0)
                {
                    gcCallLog.DataSource = _lstData;
                    gvCallLog.BestFitColumns();
                    WaitDialog.Close();
                }
                cmdRefreshData.Enabled = true;
                WaitDialog.Close();
            }
            catch
            {
                WaitDialog.Close();
                cmdRefreshData.Enabled = true;
            }
        }
        public void SetLoadedText(int numberOfLoadedItem, int totalNumber)
        {
            //labelLoaded.Text = string.Format("Loaded {0}/{1}", numberOfLoadedItem, totalNumber);
        }
        private void PlayAudio(string url)
        {
            FileInfo info = new FileInfo(url);
            if (info.Length == 0)
                MessageBox.Show("Can't play audio. Audio file don't have content.", "Can't play audio", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                _player = new CallLogPlayer(url);
                if (!_player.IsDisposed)
                {
                    //_player.PlayAudio();
                    _player.Show(this);
                }
                //_player.Stop();
            }
        }
        #endregion

        #region Control Events
        private void gvCallLog_CustomColumnSort(object sender, CustomColumnSortEventArgs e)
        {
            if (e.Column.FieldName == "call_length")
            {

                if (e.SortOrder == DevExpress.Data.ColumnSortOrder.Ascending)
                {

                    TimeSpan val1 = TimeSpan.Parse(e.Value1.ToString());
                    TimeSpan val2 = TimeSpan.Parse(e.Value2.ToString());
                    if (val1 == val2)
                    {
                        e.Result = 0;
                    }
                    else if (val1 < val2)
                    {
                        e.Result = -1;
                    }
                    else
                    {
                        e.Result = 1;
                    }
                }
                else if (e.SortOrder == DevExpress.Data.ColumnSortOrder.Descending)
                {
                    TimeSpan val1 = TimeSpan.Parse(e.Value1.ToString());
                    TimeSpan val2 = TimeSpan.Parse(e.Value2.ToString());
                    if (val1 == val2)
                    {
                        e.Result = 0;
                    }
                    else if (val1 < val2)
                    {
                        e.Result = 1;
                    }
                    else
                    {
                        e.Result = -1;
                    }
                }
            }
        }
        private void gvCallLog_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            if (gvCallLog.DataRowCount == 0) return;

            CTCallLog _row = gvCallLog.GetFocusedRow() as CTCallLog;
            event_followup_log _eftCallLog = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                _eftCallLog = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _row.id);
                if (_eftCallLog != null)
                    _efDbContext.Detach(_eftCallLog);
            }           

            GridView view = sender as GridView;
            GridUtility.CreateGridContextMenuCallLog(view, e, _eftCallLog.audio_id, this);
        }
        private void ViewCallLogs_ParentChanged(object sender, EventArgs e)
        {
            if (this.ParentForm == null)
                this.Dispose();
        }

        private void work_DoWork(object sender, DoWorkEventArgs e)
        {
            /*
            m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)
            {
                CommandTimeout = 0
            };

            m_efDbContext.FIPopulateCallLogs(UserSession.CurrentUser.UserId);
            var totalCount = (from CallLog in m_efDbContext.vw_call_logs
                              where CallLog.user_id == UserSession.CurrentUser.UserId
                              select CallLog.uid).Count();
            int numberPerPage = 50;
            decimal num = decimal.Divide(totalCount, numberPerPage);
            num = Math.Ceiling(num);
            int totalLoaded = 0;
            for (int cnt = 1; cnt <= num; cnt++)
            {
                var result = m_efDbContext.FIGetViewCallLog(numberPerPage, cnt, UserSession.CurrentUser.UserId);
                var arrayCalls = result.ToArray();
                foreach (CTGetViewCallLog callLog in arrayCalls)
                {
                    totalLoaded++;
                    this.Invoke(new Action(delegate()
                    {
                        collection.Add(callLog);
                        SetLoadedText(totalLoaded, totalCount);
                    }));
                    if (this.IsDisposed) break;

                }
                if (this.IsDisposed) break;
                Thread.Sleep(500);
            }

            e.Cancel = true;
            */
        }
        private void gvCallLog_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Value == null)
                return;

            CTCallLog _item = gvCallLog.GetRow(e.RowHandle) as CTCallLog;
           
            if (e.Column.FieldName.Equals("date_of_transaction"))
                e.DisplayText = Convert.ToDateTime(e.Value).ToString("yyyy-MM-dd HH:mm");

            //if (e.Column.FieldName.Equals("call_length")) {
            //    if (_item == null)
            //    {
            //        e.DisplayText = "00:00";
            //        return;
            //    }

            //    if (e.Value.ToString().Equals("00:00:00") || _item.audio_file.ToString().Equals("00000000-0000-0000-0000-000000000000"))
            //        e.DisplayText = "00:00";
            //    else {
            //        TimeSpan _time = (TimeSpan)e.Value;
            //        e.DisplayText = string.Format("{0:mm\\:ss}", _time); 
            //        //e.DisplayText = string.Format("{0}:{1}", _time.Minutes, _time.Seconds);
            //    }
            //}
        }
        private void gvCallLog_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e == null || e.Column == null || e.CellValue == null)
                return;

            if (e.Column.FieldName == "audio_file") {
                CTCallLog _Row = gvCallLog.GetRow(e.RowHandle) as CTCallLog;
                
                if (_Row == null) {
                    e.RepositoryItem = ribePlayHidden;
                    return;
                }

                if (_Row.call_length.ToString() == "00:00:00")
                {
                    e.RepositoryItem = ribePlayHidden;
                    return;
                }

                /**
                 * if azure blob.
                 */
                else if (Convert.ToBoolean(_Row.is_azure_blob))
                {
                    //if (_Row.azure_blob_audio_id != null)
                        e.RepositoryItem = ribePlay;
                    //else
                    //    e.RepositoryItem = ribePlayHidden;
                }

                /**
                 * if old audio.
                 */
                else {
                    if (_Row.audio_file == null) {
                        e.RepositoryItem = ribePlayHidden;
                        return;
                    }

                    if (string.IsNullOrEmpty(_Row.audio_file.ToString())) {
                        e.RepositoryItem = ribePlayHidden;
                        return;
                    }

                    Guid _AudioId = (Guid)_Row.audio_file;
                    if (_AudioId == Guid.Empty) {
                        e.RepositoryItem = ribePlayHidden;
                        return;
                    }

                    e.RepositoryItem = ribePlay;
                }

            }

            #region Old Code
            //if (e == null || e.Column == null || e.CellValue == null)
            //    return;
                
            ////if (e.Column.FieldName == "Play") {
            //if (e.Column.FieldName == "audio_file")
            //{
            //    if (e.CellValue == null)
            //    {
            //        e.RepositoryItem = ribePlayHidden;
            //        return;
            //    }
            //    var audioId = (Guid)e.CellValue;
            //    if (audioId == Guid.Empty)
            //    {
            //        e.RepositoryItem = ribePlayHidden;
            //        return;
            //    }

            //    //If audio exist in the tmpwav show play
            //    CommonApplicationData commonFolder = new CommonApplicationData("BrightVision", "BrightSales");
            //    string filePathCachWav = String.Format(@"{0}\cachewav\{1}_.wav", commonFolder.ApplicationFolderPath, audioId);
            //    if (File.Exists(filePathCachWav))
            //    {
            //        e.RepositoryItem = ribePlay;
            //        return;
            //    }

            //    var source = gcCallLog.DataSource as List<vw_call_logs>;
            //    if (source == null)
            //        return;

            //    var followup = source.FirstOrDefault(param => param.audio_file == audioId);
            //    if (followup != null)
            //    {
            //        if (followup.main_uploaded.HasValue && followup.main_uploaded.Value)
            //        {
            //            e.RepositoryItem = ribePlay;

            //        }
            //        else
            //        {
            //            e.RepositoryItem = ribePlayHidden;
            //        }
            //    }
            //    else
            //    {
            //        e.RepositoryItem = ribePlayHidden;
            //    }
            //}
            #endregion
        }
        private void ribePlay_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            /**
             * if audio_id is not null, then its an old audio file.
             * else, load the azure_blob_audio_id.
             */

            string _FileUrl = string.Empty;

            CTCallLog _row = gvCallLog.GetFocusedRow() as CTCallLog;
            event_followup_log _eftCallLog = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftCallLog = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == _row.id);
                if (_eftCallLog != null)
                    _efDbContext.Detach(_eftCallLog);
            }

            if (_eftCallLog.is_azure_blob != null && _eftCallLog.is_azure_blob == true)
            {
                if (_eftCallLog.main_uploaded == null)
                {
                    NotificationDialog.Error("Bright Manager", "File is not yet uploaded\nPlease try again in a minute.", _eftCallLog.id);
                    return;
                }
            }

            string audioId = "";
            bool IsNew = true;
            if (!string.IsNullOrEmpty(_eftCallLog.azure_blob_audio_id))
            {
                _FileUrl = string.Format("{0}/{1}.mp3", ConfigManager.AppSettings["AzureBlobStorageNewAudioUrl"].ToString(), _eftCallLog.azure_blob_audio_id.ToString());
                _row.azure_blob_audio_id = _eftCallLog.azure_blob_audio_id;
                audioId = _eftCallLog.azure_blob_audio_id;
            }
            else
            {
                _FileUrl = string.Format("{0}/{1}_.mp3", ConfigManager.AppSettings["AzureBlobStorageOldAudioUrl"].ToString(), _eftCallLog.audio_id.ToString());
                audioId = _eftCallLog.audio_id.ToString();
                IsNew = false;
            }

            WaitDialog.Show("Loading audio stream ...");
            CallLogPlayer _Player = new CallLogPlayer(_eftCallLog.id, audioId, _FileUrl, true, IsNew);
            if (!_Player.IsDisposed && _Player.CanBePlayed) {
                WaitDialog.Close();
                _Player.Show(this);
            }
            else
                WaitDialog.Close();
            

            #region Old Code
            //Guid audioId = Guid.Parse(gvCallLog.GetFocusedRowCellValue("audio_file").ToString());
            //var commonFolder = new CommonApplicationData("BrightVision", "BrightSales");
            ////string filePathTmpWav = String.Format(@"{0}\tmpwav\{1}_.wav", commonFolder.ApplicationFolderPath, audioId);
            //string filePathCachWav = String.Format(@"{0}\cachewav\{1}_.wav", commonFolder.ApplicationFolderPath, audioId);
            //string filePathCachWav2 = String.Format(@"{0}\cachewav\{1}_.mp3", commonFolder.ApplicationFolderPath, audioId);
            //object objMainUploaded = gvCallLog.GetFocusedRowCellValue("main_uploaded");
            //bool mainUploaded = false;
            ////bool isTmWavExist = File.Exists(filePathTmpWav);
            //bool isCacheWavExist = File.Exists(filePathCachWav);
            //bool isCacheWav2Exist = File.Exists(filePathCachWav2);
            //if (objMainUploaded != null)
            //    mainUploaded = bool.Parse(objMainUploaded.ToString());


            //if (isCacheWavExist)
            //    PlayAudio(filePathCachWav);
            //else if (isCacheWav2Exist)
            //    PlayAudio(filePathCachWav2);
            //else if (mainUploaded)
            //{
            //    if (_player != null)
            //    {

            //        _player = null;
            //    }

            //    WaitDialog.Show("Downloading audio files....");
            //    Guid audioID = Guid.Parse(gvCallLog.GetFocusedRowCellValue("audio_file").ToString());
            //    string fileServerUrl = FileManagerUtility.GetServerUrl(audioID);
            //    /*if(!ManagerApplicationFacade.WebDavFile.IsFileExist(fileServerUrl, audioID.ToString()))
            //    {
            //        MessageBox.Show("Audio file does not exist. Please contact administrator.", "Download audio failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }*/

            //    string m_AudioFileUrl = ManagerApplicationFacade.WebDavFile.AudioToCacheFolder(fileServerUrl);
            //    if (string.IsNullOrEmpty(m_AudioFileUrl))
            //    {
            //        MessageBox.Show("Can't download audio file. Please contact administrator.", "Download audio failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }
            //    Thread.Sleep(2000);
            //    WaitDialog.Close();
            //    PlayAudio(m_AudioFileUrl);
            //}
            #endregion
        }
        private void simpleButtonFind_Click(object sender, EventArgs e)
        {
            WaitDialog.Show(this.ParentForm, "Loading....");
            string search = textEditSearch.Text;
            gvCallLog.FindFilterText = textEditSearch.Text;
            WaitDialog.Close();
        }
        private void simpleButtonClear_Click(object sender, EventArgs e)
        {
            textEditSearch.Text = "";
            gvCallLog.FindFilterText = textEditSearch.Text;
        }
        private void textEditSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            WaitDialog.Show(this.ParentForm, "Loading....");
            string search = textEditSearch.Text;
            gvCallLog.FindFilterText = textEditSearch.Text;
            WaitDialog.Close();
        }
        private void gvCallLog_TopRowChanged(object sender, EventArgs e)
        {
            gvCallLog.FocusedRowHandle = gvCallLog.TopRowIndex;
            gvCallLog.SelectRow(gvCallLog.TopRowIndex);
        }

        private void gcCallLog_Load(object sender, EventArgs e)
        {
            LoadCalls();
        }

        private void cmdRefreshData_Click(object sender, EventArgs e)
        {
            this.LoadCalls();
        }

        private void cbxShowOnlyMyCalls_EditValueChanged(object sender, EventArgs e)
        {
            this.LoadCalls();
        }

        public void LoadCompanyView()
        {
            if (gvCallLog.DataRowCount < 1)
                return;

            CTCallLog objCallLog = gvCallLog.GetFocusedRow() as CTCallLog;
            if (objCallLog != null)
            {
                bool AccountActive = true;
                using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection))
                {
                    event_followup_log objEventFollowUpLog = _efDbContext.event_followup_log.FirstOrDefault(i => i.id == objCallLog.id);
                    if (objEventFollowUpLog != null)
                    {
                        sub_campaign_account_lists objscal = _efDbContext.sub_campaign_account_lists.FirstOrDefault(i => i.account_id == objEventFollowUpLog.account_id);
                        if (objscal != null)
                        {
                            if (!Convert.ToBoolean(ValidationUtility.IFNullString(objscal.active, "0")))
                            {
                                NotificationDialog.Warning("Bright Sales", "The account you are trying to load is already de-activated.");
                                AccountActive = false;
                            }
                        }

                        m_BrightSalesProperty.CommonProperty.CallLogContactId = ValidationUtility.TryParseInt(objEventFollowUpLog.contact_id);
                        _efDbContext.Detach(objEventFollowUpLog);
                    }
                }

                if (AccountActive)
                {
                    m_objMyFollowUps = new MyFollowUps();
                    pnlMyFollowups.Controls.Clear();
                    pnlMyFollowups.Controls.Add(m_objMyFollowUps);

                    m_objMyFollowUps.m_FollowUpBar_FollowUpId = objCallLog.id;
                    m_objMyFollowUps.LoadEventsCallLogCaller(m_BrightSalesProperty.CommonProperty.SubCampaignId);
                    m_objMyFollowUps.SetFocusRow();
                    m_objMyFollowUps.cmdWorkWithCompany_Click(null, null);
                    m_objMyFollowUps.Dispose();
                    m_objMyFollowUps = null;
                }
            }

        }
        #endregion

    }
}
