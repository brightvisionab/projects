using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SalesConsultant.Utils {
    /// <summary>
    /// Adds wheel mouse scrolling to a control that does not have focus
    /// </summary>
    internal class FlashWindowMessageFilter : IMessageFilter {

        public const UInt32 WM_SYSTIMER = 0x0118;
        public const UInt32 WM_NCACTIVATE = 0x86;

        // an application can have many windows, only filter for one window at the time
        IntPtr FilteredHwnd = IntPtr.Zero;

        public FlashWindowMessageFilter(IntPtr hwnd) {
            this.FilteredHwnd = hwnd;
        }        

        #region IMessageFilter Members
        public bool PreFilterMessage(ref Message m) {
            if (this.FilteredHwnd == m.HWnd && m.Msg == WM_SYSTIMER)
                return true;     // stop handling the message further
            else
                return false;    // all other msgs: handle them
        }
        #endregion
    }
}
