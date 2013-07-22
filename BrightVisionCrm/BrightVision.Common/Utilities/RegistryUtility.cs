
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BrightVision.Common.Utilities
{
    public class RegistryUtility
    {
        /**
         * pertains to the registry entry:
         * HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Jet\4.0\Engines\Excel (32 bit)
         * HKLM\SOFTWARE\wow6432node\microsoft\jet\4.0\engines\excel (64 bit)
         */
        public static void UpdateTypeGuessRows()
        {
            try {
                RegistryKey _reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Jet\\4.0\\Engines\\Excel", true);
                string _val = _reg.GetValue("TypeGuessRows").ToString();
                if (Convert.ToInt32(_val) > 0)
                    _reg.SetValue("TypeGuessRows", 0, RegistryValueKind.DWord);

                //MessageBox.Show("Successfull ...");
                //BrightVision.Common.UI.NotificationDialog.Information("Bright Sales", "Suuccess ...");
            }
            catch (Exception e) {
                //BrightVision.Common.UI.NotificationDialog.Information("Bright Sales", e.Message.ToString());
                //MessageBox.Show("Not successfull ...");
            }
        }
    }
}
