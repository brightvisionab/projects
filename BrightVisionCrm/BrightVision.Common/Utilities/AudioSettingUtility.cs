
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ozeki.Media.MediaHandlers;
using Ozeki.Media.Audio;

using BrightVision.Model;
using BrightVision.Common.Business;

namespace BrightVision.Common.Utilities
{
    public static class AudioSettingUtility
    {
        public static Speaker SelectedSpeaker {get;set;}
        public static Microphone SelectedMicrophone { get; set; }
        public static void SetLicense()
        {
            if (Ozeki.VoIP.SDK.Protection.LicenseManager.Instance.LicenseType != Ozeki.VoIP.SDK.Protection.LicenseType.Activated)
                Ozeki.VoIP.SDK.Protection.LicenseManager.Instance.SetLicense("OZSDK-BRI2CALL-120712-9D26513C", "TUNDOjIsTVBMOjIsRzcyOTp0cnVlLE1TTEM6MixNRkM6MixVUDoyMDEzLjA3LjEyLFA6MjE5OS4wMS4wMXx4OVJpbjdaOTg5MTZRWGVHRm82STVqcVJkU0psNVl1WWVPcDA1Mm5RV1k0UUVub29HWlIyTDZoakMwcnk3bk9HdWJvK09xdHhFU0trWCsvZkJwVzNSZz09");
        }
        public static void SetDefaultAudio() {
            var speaker = Speaker.GetDefaultDevice();
            var mic = Microphone.GetDefaultDevice();
            SelectedSpeaker = speaker;
            SelectedMicrophone = mic;
        }
        public static audio_settings GetUserAudioSetting()
        {
            var speaker = Speaker.GetDefaultDevice();
            if (speaker == null)
                return null;

            var mic = Microphone.GetDefaultDevice();
            if (mic == null)
                return null;

            if (SelectedSpeaker == null)
                SelectedSpeaker = speaker;
            if (SelectedMicrophone == null)
                SelectedMicrophone = mic;
            audio_settings audioSetting;
            using (BrightPlatformEntities objDbModel = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                audioSetting = objDbModel.audio_settings.Where(r => r.user_id == UserSession.CurrentUser.UserId).FirstOrDefault();
                if (audioSetting == null)
                {
                    audioSetting = new audio_settings();
                    audioSetting.user_id = UserSession.CurrentUser.UserId;
                    audioSetting.mode = 0;
                    audioSetting.mic_volume = mic.Volume * 10;
                    audioSetting.mic_auto_adjust = true;
                    audioSetting.mic_device_id = mic.DeviceInfo.DeviceID;
                    audioSetting.mic_device_name = mic.DeviceInfo.ProductName;
                    audioSetting.speaker_volume = speaker.Volume * 10;
                    audioSetting.speaker_auto_adjust = true;
                    audioSetting.speaker_device_id = speaker.DeviceInfo.DeviceID;
                    audioSetting.speaker_device_name = speaker.DeviceInfo.ProductName;
                    objDbModel.audio_settings.AddObject(audioSetting);
                    objDbModel.SaveChanges();
                }
                objDbModel.Detach(audioSetting);
             }
             return audioSetting;

        }

        public static Speaker GetDefaultDeviceSpeaker()
        {
            audio_settings _efeAudioSetting = GetUserAudioSetting();
            List<DeviceInfo> _lstDevices = Speaker.GetDevices();
            if (_efeAudioSetting != null) {
                if (_efeAudioSetting.speaker_device_id != null) {
                    DeviceInfo _DeviceInfo = _lstDevices.Find(i =>
                        i.DeviceID == _efeAudioSetting.speaker_device_id &&
                        i.ProductName == _efeAudioSetting.speaker_device_name
                    );
                    if (_DeviceInfo != null)
                        return Speaker.GetDevice(_DeviceInfo);
                }
            }

            return Speaker.GetDefaultDevice();
        }
        public static Microphone GetDefaultDeviceMicrophone()
        {
            audio_settings _efeAudioSetting = GetUserAudioSetting();
            List<DeviceInfo> _lstDevices = Microphone.GetDevices();
            if (_efeAudioSetting != null) {
                if (_efeAudioSetting.mic_device_id != null) {
                    DeviceInfo _DeviceInfo = _lstDevices.Find(i =>
                        i.DeviceID == _efeAudioSetting.mic_device_id &&
                        i.ProductName == _efeAudioSetting.mic_device_name
                    );
                    if (_DeviceInfo != null)
                        return Microphone.GetDevice(_DeviceInfo);
                }
            }
            
            return Microphone.GetDefaultDevice();
        }
    }
}
