
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using DevExpress.XtraEditors.Controls;

using Ozeki.Media.MediaHandlers;
using Ozeki.Media.Audio;

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Common.UI;
using BrightVision.Telephony.Business;
using BrightVision.Common.Utilities;

using SalesConsultant.Business;


namespace SalesConsultant.Modules
{
    public partial class AudioSettings : UserControl
    {
        #region Variable
        List<DeviceInfo> microphoneList;
        List<DeviceInfo> speakerList;
        Speaker speaker;
        Microphone mic;
        #endregion

        public AudioSettings(){
            InitializeComponent();
        }

        private void PhoneSettings_Load(object sender, EventArgs e){
            microphoneList = Microphone.GetDevices();
            speakerList = Speaker.GetDevices();
            lookUpEditSpeaker.Properties.DataSource = speakerList;
            lookUpEditSpeaker.Properties.DisplayMember = "ProductName";
            lookUpEditSpeaker.Properties.ValueMember = "DeviceID";
            lookUpEditSpeaker.Properties.ShowHeader = false;
            lookUpEditSpeaker.Properties.Columns.Add(new LookUpColumnInfo("ProductName"));

            lookUpEditMicrophone.Properties.DataSource = microphoneList;
            lookUpEditMicrophone.Properties.DisplayMember = "ProductName";
            lookUpEditMicrophone.Properties.ValueMember = "DeviceID";
            lookUpEditMicrophone.Properties.Columns.Add(new LookUpColumnInfo("ProductName"));
            lookUpEditMicrophone.Properties.ShowHeader = false;
            SetAudioSettings();
        }

        private void SetAudioSettings()
        {
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            audio_settings audioSetting = objDbModel.audio_settings.FirstOrDefault(e => e.user_id == UserSession.CurrentUser.UserId);

            speaker = AudioSettingUtility.GetDefaultDeviceSpeaker(); //this.GetDefaultDeviceSpeaker(audioSetting);
            if (speaker != null) {
                if (speaker.DeviceInfo.DeviceID != null)
                    lookUpEditSpeaker.EditValue = speaker.DeviceInfo.DeviceID;
            }

            mic = AudioSettingUtility.GetDefaultDeviceMicrophone(); //this.GetDefaultDeviceMicrophone(audioSetting);
            if (mic != null) {
                if (mic.DeviceInfo.DeviceID != null)
                    lookUpEditMicrophone.EditValue = mic.DeviceInfo.DeviceID;
            }
            
            if (AudioSettingUtility.SelectedSpeaker != null)
                speaker = AudioSettingUtility.SelectedSpeaker;
            if (AudioSettingUtility.SelectedMicrophone != null)
                mic = AudioSettingUtility.SelectedMicrophone;

            if (speaker == null)
                NotificationDialog.Error("Bright Sales", "No speaker device found. Please connect a speaker device first.");
            else if (mic == null)
                NotificationDialog.Error("Bright Sales", "No microphone device found. Please connect a microphone device first.");

            if (audioSetting == null) {
                if (speaker != null)
                    zoomTrackBarControlSpeakerVolume.EditValue = speaker.Volume * 10;
                if (mic != null)
                    zoomTrackBarControlMicVolume.EditValue = mic.Volume * 10;
            }
            else {
                comboBoxEditMode.SelectedIndex = audioSetting.mode;
                zoomTrackBarControlSpeakerVolume.EditValue = audioSetting.speaker_volume;
                zoomTrackBarControlMicVolume.EditValue = audioSetting.mic_volume;
                checkEditSpeakerAutoAdjust.EditValue = audioSetting.speaker_auto_adjust;
                checkEditMicAutoAdjust.EditValue = audioSetting.mic_auto_adjust;
            }

            //if (speaker != null) 
            //    lookUpEditSpeaker.EditValue = speaker.DeviceInfo.DeviceID;

            //if (mic != null)
            //    lookUpEditMicrophone.EditValue = mic.DeviceInfo.DeviceID;
        }

        private void simpleButtonSave_Click(object sender, EventArgs e)
        {
            if (mic == null && speaker == null) {
                NotificationDialog.Error("Bright Sales", "Some devices are missing. Please connect a speaker/microphone device first.");
                return;
            }

            WaitDialog.Show(ParentForm, "Saving Audio Settings ...");
            float speakerVolume = float.Parse(zoomTrackBarControlSpeakerVolume.EditValue.ToString());
            float micVolume = float.Parse(zoomTrackBarControlMicVolume.EditValue.ToString());
            bool speakerAutoAdjust = checkEditSpeakerAutoAdjust.Checked;
            bool micAutoAdjust = checkEditMicAutoAdjust.Checked;
            BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
            audio_settings audioSetting = objDbModel.audio_settings.FirstOrDefault(r => r.user_id == UserSession.CurrentUser.UserId);

            int? _mic_device_id = null;
            string _mic_device_name = string.Empty;
            if (lookUpEditMicrophone.EditValue != null) {
                _mic_device_id = Convert.ToInt32(lookUpEditMicrophone.EditValue);
                _mic_device_name = lookUpEditMicrophone.Text;
            }

            int? _speaker_device_id = null;
            string _speaker_device_name = string.Empty;
            if (lookUpEditSpeaker.EditValue != null) {
                _speaker_device_id = Convert.ToInt32(lookUpEditSpeaker.EditValue);
                _speaker_device_name = lookUpEditSpeaker.Text;
            }

            if (audioSetting == null) {
                audioSetting = new audio_settings() {
                    user_id = UserSession.CurrentUser.UserId,
                    mode = comboBoxEditMode.SelectedIndex,
                    mic_volume = micVolume,
                    mic_auto_adjust = micAutoAdjust,
                    speaker_volume = speakerVolume,
                    speaker_auto_adjust = speakerAutoAdjust,
                    mic_device_id = _mic_device_id,
                    mic_device_name = _mic_device_name,
                    speaker_device_id = _speaker_device_id,
                    speaker_device_name = _speaker_device_name
                };
                objDbModel.audio_settings.AddObject(audioSetting);
            }
            else {
                audioSetting.user_id = UserSession.CurrentUser.UserId;
                audioSetting.mode = comboBoxEditMode.SelectedIndex;
                audioSetting.mic_volume = micVolume;
                audioSetting.mic_auto_adjust = micAutoAdjust;
                audioSetting.speaker_volume = speakerVolume;
                audioSetting.speaker_auto_adjust = speakerAutoAdjust;
                audioSetting.mic_device_id = _mic_device_id;
                audioSetting.mic_device_name = _mic_device_name;
                audioSetting.speaker_device_id = _speaker_device_id;
                audioSetting.speaker_device_name = _speaker_device_name;
                objDbModel.audio_settings.ApplyCurrentValues(audioSetting);
            }
            objDbModel.SaveChanges();

            /* https://brightvision.jira.com/browse/PLATFORM-2665
             * DAN: Added to update the calllogbar m_SoftPhone to solve the scenario issue on ticket.
             * It happen because the variable m_SoftPhone.RegisterSuccess is false even thought the user has updated the audio settings.
            */
            //----------------------------------------------------
            CallLogBar clb = CallLogBar.InstanceNoParam;
            if (clb != null)
            {
                if (clb.m_SoftPhone != null)
                    clb.m_SoftPhone.RegisterPhone();
            }
            //----------------------------------------------------

            WaitDialog.Close();
            NotificationDialog.Information("Bright Sales", "Audio setting saved.");
            this.ParentForm.Close();
        }

        private void lookUpEditMicrophone_EditValueChanged(object sender, EventArgs e){
            int deviceId = int.Parse(lookUpEditMicrophone.EditValue.ToString());
            var mic = microphoneList.Where(pr => pr.DeviceID == deviceId).FirstOrDefault();
            Microphone microphoneGetDevice = null;
            if (mic == null)
                microphoneGetDevice = Microphone.GetDefaultDevice();
            else
                microphoneGetDevice = Microphone.GetDevice(mic);

            AudioSettingUtility.SelectedMicrophone = microphoneGetDevice;
        }

        private void lookUpEditSpeaker_EditValueChanged(object sender, EventArgs e){
            int deviceId = int.Parse(lookUpEditSpeaker.EditValue.ToString());
            var speaker = speakerList.Where(pr => pr.DeviceID == deviceId).FirstOrDefault();
            Speaker microphoneGetDevice = null;
            if (speaker == null)
                microphoneGetDevice = Speaker.GetDefaultDevice();
            else
                microphoneGetDevice = Speaker.GetDevice(speaker);
            AudioSettingUtility.SelectedSpeaker = microphoneGetDevice;
        }
    }
}
