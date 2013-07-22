
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
namespace BrightVision.Common.Utilities
{
    public class MediaUtility
    {
        public enum ePhoneCallSoundType {
            Ring,
            HangUp,
            Busy
        }

        private SoundPlayer m_SoundPlayer = null;

        public void Start(ePhoneCallSoundType pSoundType)
        {
            this.Stop();
            
            if (pSoundType == ePhoneCallSoundType.Ring) {
                m_SoundPlayer = new SoundPlayer(Resources.call_ring);
                m_SoundPlayer.PlayLooping();
            }
                
            else if (pSoundType == ePhoneCallSoundType.HangUp) {
                m_SoundPlayer = new SoundPlayer(Resources.call_done);
                m_SoundPlayer.Play();
                System.Threading.Thread.Sleep(500);
            }

            else if (pSoundType == ePhoneCallSoundType.Busy) {
                m_SoundPlayer = new SoundPlayer(Resources.call_busy);
                m_SoundPlayer.PlayLooping();
            }
        }
        public void Stop()
        {
            if (m_SoundPlayer != null) {
                m_SoundPlayer.Stop();
                m_SoundPlayer = null;
            }
        }
    }
}
