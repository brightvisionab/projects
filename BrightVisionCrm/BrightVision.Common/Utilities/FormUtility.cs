
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using DevExpress.XtraEditors;
using BrightVision.Common.Business;

namespace BrightVision.Common.Utilities {
    public class FormUtility {
        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        public static bool ShortCutKeysHandled(Keys keyData) {
            Control ctl = Control.FromHandle(GetFocus());
            if (keyData == (Keys.Control | Keys.OemMinus)) {
                if (ctl is TextEdit) {
                    if (((TextEdit)ctl).Enabled && !((TextEdit)ctl).Properties.ReadOnly) {
                        string insertText = DateTime.Now.ToString("yyyy-MM-dd");
                        int selectionIndex = ((TextEdit)ctl).SelectionStart;
                        ctl.Text = ctl.Text.Insert(selectionIndex, insertText);
                        ((TextEdit)ctl).SelectionStart = selectionIndex + insertText.Length;
                        return true;
                    }
                } else if (ctl is TextBoxMaskBox) {
                    if (((TextBoxMaskBox)ctl).Enabled && !((TextBoxMaskBox)ctl).ReadOnly) {
                        string insertText = DateTime.Now.ToString("yyyy-MM-dd");
                        int selectionIndex = ((TextBoxMaskBox)ctl).MaskBoxSelectionStart;
                        ctl.Text = ctl.Text.Insert(selectionIndex, insertText);
                        ((TextBoxMaskBox)ctl).MaskBoxSelectionStart = selectionIndex + insertText.Length;
                        return true;
                    }
                }
            } else if (keyData == (Keys.Shift | Keys.Down)) {
                if (ctl is TextBoxMaskBox) {
                    var edit = ctl as TextBoxMaskBox;

                    int selectionLength = edit.MaskBoxSelectionLength;
                    int remTextLength = edit.MaskBoxText.Length - edit.MaskBoxSelectionLength;
                    string remText = edit.MaskBoxText.Substring(selectionLength, remTextLength);

                    if (remText.IndexOf(Environment.NewLine) == -1) {
                        edit.MaskBoxSelectionLength = edit.MaskBoxText.Length;
                        return true;
                    }
                }
            } else if (keyData == Keys.Enter) {
                if (ctl is ButtonEdit) {
                    try
                    {
                        var edit = ctl as ButtonEdit;
                        edit.PerformClick(edit.Properties.Buttons[0]);
                    }
                    catch { }
                }
            }

            return false;
        }

        public static Control FindFocusedControl(Control control) {
            var container = control as ContainerControl;
            while (container != null) {
                control = container.ActiveControl;
                container = control as ContainerControl;
            }
            return control;
        }

        public static string GetConfigSetting(string key) {
            string val = ConfigManager.AppSettings[key];
            if (null == val) return string.Empty;
            return val;
        }
    }
}
