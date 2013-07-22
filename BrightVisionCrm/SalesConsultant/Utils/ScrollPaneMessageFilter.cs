using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SalesConsultant.Utils {
    /// <summary>
    /// Adds wheel mouse scrolling to a control that does not have focus
    /// </summary>
    internal class ScrollPanelMessageFilter : IMessageFilter {
        int WM_MOUSEWHEEL = 0x20A;
        Control m_control;
        bool controlHasFocus = false;

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);
        [DllImport("User32.dll")]
        static extern Int32 SendMessage(int hWnd, int Msg, int wParam, int lParam);

        public ScrollPanelMessageFilter(Control control) {
            this.m_control = control;
            //Go through each control on the panel and add an event handler.
            //We need to know if a control on the panel has focus to prevent sending
            //the scroll message a second time
            AddFocusEvent(control);
        }

        private void AddFocusEvent(Control parentControl) {
            foreach (Control aControl in parentControl.Controls) {
                if (aControl.Controls.Count == 0) {
                    aControl.GotFocus += new EventHandler(control_GotFocus);
                    aControl.LostFocus += new EventHandler(control_LostFocus);                    
                } else {
                    AddFocusEvent(aControl);
                }
            }
        }

        void control_GotFocus(object sender, EventArgs e) {
            controlHasFocus = true;
        }

        void control_LostFocus(object sender, EventArgs e) {
            controlHasFocus = false;
        }

        #region IMessageFilter Members

        public bool PreFilterMessage(ref Message m) {
            //filter out all other messages except than mousewheel
            //also only proceed with processing if the panel is focusable, no controls on the panel have focus 
            //and the vertical scroll bar is visible
            if (m.Msg == WM_MOUSEWHEEL && m_control.CanFocus && !controlHasFocus) {
                if ((m_control is ScrollableControl) || (m_control is DataGridView)) {
                    // If this is some other type of control, 
                    // true -> sends the mouse wheel messages anyway
                    // false -> don't send mouse wheel messages to any control other than a ScrollableControl
                    bool scrollbarsvisible = true;

                    // If we are dealing with a scrollable control, we'll check to make sure the scrollbar is visible before sending messages
                    if (m_control is ScrollableControl) scrollbarsvisible = (m_control as ScrollableControl).VerticalScroll.Visible;

                    if (scrollbarsvisible) {
                        //is mouse cordinates over the panel display rectangle?
                        Rectangle rect = m_control.RectangleToScreen(m_control.ClientRectangle);
                        Point cursorPoint = new Point();
                        GetCursorPos(ref cursorPoint);
                        if ((cursorPoint.X > rect.X && cursorPoint.X < rect.X + rect.Width) &&
                            (cursorPoint.Y > rect.Y && cursorPoint.Y < rect.Y + rect.Height)) {
                            //send the mouse wheel message to the panel.
                            SendMessage((int)m_control.Handle, m.Msg, (Int32)m.WParam, (Int32)m.LParam);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
