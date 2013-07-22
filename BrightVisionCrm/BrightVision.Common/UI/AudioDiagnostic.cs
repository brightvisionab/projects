using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using System.Linq;
using System.IO;
using BrightVision.Windows.Azure.Storage.Blob;

namespace BrightVision.Common.UI
{
    public partial class AudioDiagnostic : DevExpress.XtraEditors.XtraForm
    {
        #region CONSTRUCTOR
        public AudioDiagnostic()
        {
            InitializeComponent();
            EmptyValues();
        }
        #endregion

        #region INITIALIZE EMPTY VALUES
        private void EmptyValues()
        {
            lblAudioId.Text = "";
            lblAudioStatus.Text = "";
            lblSubcampaign.Text = "";
            lblAccount.Text = "";
            lblContact.Text = "";
            lblCreatedBy.Text = "";
            lblDateCreated.Text = "";
        }
        #endregion

        #region PUBLIC METHODS
        public void GetAudioDetails(int followUpId)
        {
            try
            {
                using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(Business.UserSession.EntityConnection))
                {
                    var followup = objDbModel.event_followup_log.Where(param => param.id == followUpId).FirstOrDefault();

                    if (followup != null)
                    {
                        //Get Audio Id
                        if (followup.is_azure_blob != null) lblAudioStatus.Text = "New";
                        else lblAudioStatus.Text = "Old";

                        if (followup.azure_blob_audio_id != null) lblAudioId.Text = GetAudioId(IFNull(followup.azure_blob_audio_id, ""));
                        else lblAudioId.Text = GetAudioId(IFNull(followup.audio_id, ""));
                                                
                        lblSubcampaign.Text = followup.subcampaign.title;
                        lblAccount.Text = followup.account.company_name;

                        contact _contact = objDbModel.contacts.FirstOrDefault(i => i.id == followup.contact_id);

                        lblContact.Text = IFNull(_contact.first_name, "") + " " + IFNull(_contact.middle_name, "") + " " + IFNull(_contact.last_name, "");
                        lblDateCreated.Text = followup.date_created.ToString();
                        lblDBIsUploaded.Text = Convert.ToBoolean(IFNull(followup.main_uploaded, "false")).ToString();

                        //int timeDiff = ((TimeSpan)followup.end_time).Minutes - ((TimeSpan)followup.start_time).Minutes;
                        TimeSpan timeDiff = ((TimeSpan)followup.end_time).Subtract((TimeSpan)followup.start_time);
                        lblCallLength.Text = timeDiff.ToString("hh") + ":" + timeDiff.ToString("mm") + ":" + timeDiff.ToString("ss");

                        string _audio = lblAudioId.Text;
                        if (followup.is_azure_blob != null)
                        {
                            int _CustomerId = 0;
                            _CustomerId = (int)objDbModel.campaigns.FirstOrDefault(i => i.id == followup.subcampaign.campaign_id).customer_id;

                            string Environment = Business.ConfigManager.AppSettings["BuildEnvironment"];
                            string Env = "b";
                            switch (Environment)
                            {
                                case "Production Environment": Env = "b"; break;
                                case "Staging Environment": Env = "s"; break;
                                case "Demo Environment": Env = "d"; break;
                            }
                            string AdditionParam = Env + "/" + _CustomerId + "/" + DateTime.Parse(followup.date_created.ToString()).ToString("yyMMdd") + "/";

                            _audio = AdditionParam + lblAudioId.Text.Replace("-","").ToUpper();
                        }

                        CheckIfFileExistOnAzure(followup.id, _audio, Convert.ToBoolean(IFNull(followup.is_azure_blob, "0")));

                        var user = objDbModel.users.Where(param => param.id == followup.created_by).FirstOrDefault();
                        if (user != null)
                        {
                            lblCreatedBy.Text = user.fullname;

                            objDbModel.Detach(user);
                        }

                        objDbModel.Detach(followup);
                    }
                }
            }
            catch { }
        }

        private void CheckIfFileExistOnAzure(int followUpId, string audioId, bool IsNew = true)
        {
            if ((new Utilities.WindowsAzureStorageBlobUtility()).CheckBlobFileExists(followUpId, audioId, IsNew))
            {
                lblExistsOnAzure.Text = "True";
            }

        }
        #endregion
        
        #region PRIVATE METHODS
        private bool SearchMP3(string folder)
        {
            //string[] files = Directory.GetFiles(commonData.ApplicationFolderPath, lblAudioId.Text + "_.mp3");

            bool IsFound = SearchFile(folder, lblAudioId.Text + "_.mp3");

            return IsFound;
        }
        private bool SearchOnTmpWav(string folder)
        {
            bool IsFound = SearchFile(folder, lblAudioId.Text + "_.wav");

            return IsFound;
        }
        private bool SearchFile(string Folder, string audioFile)
        {
            if (Directory.Exists(Folder))
            {
                string[] files = Directory.GetFiles(Folder, audioFile);
                if (files.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }
        private string GetAudioId(string _audioId, bool isNew = true)
        {
            string audioIdtemp = _audioId;
            if (isNew)
            {
                audioIdtemp = _audioId.Substring(_audioId.LastIndexOf("/") + 1, _audioId.Length - (_audioId.LastIndexOf("/") + 1));
            }
            Guid audioId = Guid.Empty;
            Guid.TryParse(audioIdtemp, out audioId);

            return audioId.ToString();
        }
        private string IFNull(object obj, string defaultValue)
        {
            if (obj != null) return obj.ToString();
            else return defaultValue;
        }
        #endregion

        #region EVENTS
        private void btnSearchFile_Click(object sender, EventArgs e)
        {
            CommonApplicationData commonData = new CommonApplicationData("BrightVision", "BrightSales", true);
            bool IsFoundMP3 = SearchMP3(commonData.ApplicationFolderPath);
            bool IsFoundTmpWav = SearchOnTmpWav(commonData.ApplicationFolderPath + "\\tmpwav\\");
            bool IsFoundCacheWav = SearchOnTmpWav(commonData.ApplicationFolderPath + "\\cachewav\\");
            bool IsFoundFailedUploadFolderMP3 = SearchMP3(commonData.ApplicationFolderPath + "\\FailedUpload\\");
            bool IsFoundFailedUploadFolderTmpWav = SearchOnTmpWav(commonData.ApplicationFolderPath + "\\FailedUpload\\");

            string audioFileMP3 = commonData.ApplicationFolderPath + "\\" + lblAudioId.Text + "_.mp3";
            string audioFileWAV = commonData.ApplicationFolderPath + "\\tmpwav\\" + lblAudioId.Text + "_.wav";
            string audioFileCACHE = commonData.ApplicationFolderPath + "\\cachewav\\" + lblAudioId.Text + "_.wav";
            string audioFileFailedUploadMP3 = commonData.ApplicationFolderPath + "\\FailedUpload\\" + lblAudioId.Text + "_.mp3";
            string audioFileFailedUploadWAV = commonData.ApplicationFolderPath + "\\FailedUpload\\" + lblAudioId.Text + "_.wav";

            if (IsFoundTmpWav || IsFoundCacheWav || IsFoundMP3 || IsFoundFailedUploadFolderMP3 || IsFoundFailedUploadFolderTmpWav)
            {
                string audioFile = null;
                if (IsFoundTmpWav) audioFile = audioFileWAV;
                else if (IsFoundCacheWav) audioFile = audioFileCACHE;
                else if (IsFoundMP3) audioFile = audioFileMP3;
                else if (IsFoundFailedUploadFolderMP3) audioFile = audioFileFailedUploadMP3;
                else if (IsFoundFailedUploadFolderTmpWav) audioFile = audioFileFailedUploadWAV;

                if (MessageBox.Show("Found a match. Do you want to re-upload the audio file?", "Match found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Cursor = Cursors.WaitCursor;

                    string _filename = Path.GetFileName(audioFile);
                    if (!IsFoundTmpWav && !IsFoundCacheWav && !IsFoundMP3 && IsFoundFailedUploadFolderMP3)
                        File.Move(audioFile, commonData.ApplicationFolderPath + "\\" + _filename);
                    else if (!IsFoundTmpWav && !IsFoundCacheWav && !IsFoundMP3 && IsFoundFailedUploadFolderTmpWav)
                        File.Move(audioFile, commonData.ApplicationFolderPath + "\\tmpwav" + _filename);

                    if (IsFoundTmpWav && IsFoundMP3) File.Delete(audioFileMP3);
                    (new WindowsAzureStorageBlobUtility()).UploadFile(audioFile);
                    this.Cursor = Cursors.Default;
                }
                else
                {
                    this.Close();
                }
            }
            else
            {

                if (IsFoundMP3)
                {

                }

                MessageBox.Show("No audio file found. Try using the previous computer used by " + lblCreatedBy.Text + ".", "No match found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}