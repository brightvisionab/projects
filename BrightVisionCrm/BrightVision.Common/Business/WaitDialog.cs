using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightVision.Common.Utilities;
using DevExpress.Utils;
using DevExpress.XtraSplashScreen;

namespace BrightVision.Common.Business
{
    public static class WaitDialog
    {
        private static Cursor currentCursor;
        private static int windowOpenCount = 0;
        
        static WaitDialog() { }

        public static void Show(Form parentForm, string Caption = "", string Description = "") {
            windowOpenCount++;
            if (SplashScreenManager.Default != null && SplashScreenManager.Default.IsSplashFormVisible) {
                return;
            }
            currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SplashScreenManager.ShowForm(parentForm, typeof(WaitForm1), false, false, false);
            if (Caption != "")
                SplashScreenManager.Default.SetWaitFormCaption(Caption);
            if (Description != "")
                SplashScreenManager.Default.SetWaitFormDescription(Description);            
        }

        public static void Show(Form parentForm, string Description = "") {
            windowOpenCount++;
            if (SplashScreenManager.Default != null && SplashScreenManager.Default.IsSplashFormVisible) {                
                return;
            }
            currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SplashScreenManager.ShowForm(parentForm, typeof(WaitForm1), false, false, false);            
            if (Description != "")
                SplashScreenManager.Default.SetWaitFormDescription(Description);
        }

        public static void Show( string Description = "", bool ForceOpen = false) {
            if (!ForceOpen)
            {
                windowOpenCount++;
                if (SplashScreenManager.Default != null && SplashScreenManager.Default.IsSplashFormVisible)
                {
                    return;
                }
            }
            currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SplashScreenManager.ShowForm(null, typeof(WaitForm1), false, false, false);
            if (Description != "")
                SplashScreenManager.Default.SetWaitFormDescription(Description);
        }

        public static void Close(bool ForceClose = false)
        {
            if (ForceClose)
            {
                windowOpenCount = 0;
                Cursor.Current = currentCursor;
                SplashScreenManager.CloseForm(false);
                return;
            }

            if (windowOpenCount == 1) {
                windowOpenCount--;
                Cursor.Current = currentCursor;
                SplashScreenManager.CloseForm(false);
            }

            if (windowOpenCount > 1) {              
                    windowOpenCount--;
            }

        }


    }
}
