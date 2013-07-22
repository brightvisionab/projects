using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SalesConsultant.Modules
{
    public delegate void MouseUpPhoneKeyboardHandler(int dtmf);
    public delegate void MouseDownPhoneKeyboardHandler(int dtmf);
    public partial class PhoneKeyboard : DevExpress.XtraEditors.XtraUserControl
    {
        #region Event
        public event MouseUpPhoneKeyboardHandler KeyUp;
        public event MouseDownPhoneKeyboardHandler KeyDown;
        #endregion

        #region Constructor
        public PhoneKeyboard()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Method
        /// <summary>
        /// Adds the appropriate keypad character to the dial display.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnKeypad_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
                tbDialNumber.Text += btn.Text;
        }

        private void Keypad_MouseUp(object sender, MouseEventArgs e)
        {
            Button keyPadBtn = sender as Button;
            if (keyPadBtn != null)
            {
                int dtmf = 0;
                if (!(int.TryParse(keyPadBtn.Tag.ToString(), out dtmf)))
                    return;

                if (KeyUp != null)
                    KeyUp(dtmf);
            }
        }

        private void Keypad_MouseDown(object sender, MouseEventArgs e)
        {
            Button keyPadBtn = sender as Button;
            if (keyPadBtn != null)
            {
                int dtmf = 0;
                if (!(int.TryParse(keyPadBtn.Tag.ToString(), out dtmf)))
                    return;

                if (KeyDown != null)
                    KeyDown(dtmf);
            }
        }

        private void btnKeypad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift && e.KeyCode == Keys.D3)
            {
                SetKeyDown(btnKeypadSharp);
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Multiply: SetKeyDown(btnKeypadAsterisk); break;
                    case Keys.NumPad0: SetKeyDown(btnKeypad0); break;
                    case Keys.NumPad1: SetKeyDown(btnKeypad1); break;
                    case Keys.NumPad2: SetKeyDown(btnKeypad2); break;
                    case Keys.NumPad3: SetKeyDown(btnKeypad3); break;
                    case Keys.NumPad4: SetKeyDown(btnKeypad4); break;
                    case Keys.NumPad5: SetKeyDown(btnKeypad5); break;
                    case Keys.NumPad6: SetKeyDown(btnKeypad6); break;
                    case Keys.NumPad7: SetKeyDown(btnKeypad7); break;
                    case Keys.NumPad8: SetKeyDown(btnKeypad8); break;
                    case Keys.NumPad9: SetKeyDown(btnKeypad9); break; 
                }
            }
        }

        private void SetKeyDown(Button btn)
        {
            btn.PerformClick();
            Keypad_MouseDown(btn, null);
            Keypad_MouseUp(btn, null);
            btn.Focus(); 
        }

        #endregion

        
    }
}
