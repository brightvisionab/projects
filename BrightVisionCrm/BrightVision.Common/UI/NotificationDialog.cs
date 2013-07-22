
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraBars.Alerter;

namespace BrightVision.Common.UI
{
    public static class NotificationDialog
    {
        static NotificationDialog()
        {
        }

        private static AlertControl m_AlertDialogInformation = null; // green colored
        private static AlertControl m_AlertDialogWarning = null; // yellow colored
        private static AlertControl m_AlertDialogError = null; // red colored

        public static void Information(string pCaption, string pText)
        {
            m_AlertDialogInformation = new AlertControl() {
                AutoFormDelay = 3000
            };
            m_AlertDialogInformation.BeforeFormShow += m_AlertDialogInformation_BeforeFormShow;
            m_AlertDialogInformation.Show(new Form(), pCaption, pText);
        }
        public static void Warning(string pCaption, string pText)
        {
            m_AlertDialogWarning = new AlertControl() {
                AutoFormDelay = 10000
            };
            m_AlertDialogWarning.BeforeFormShow += m_AlertDialogWarning_BeforeFormShow;
            m_AlertDialogWarning.Show(new Form(), pCaption, pText);
        }
        public static void Error(string pCaption, string pText)
        {
            m_AlertDialogError = new AlertControl() {
                AutoFormDelay = 10000
            };
            m_AlertDialogError.BeforeFormShow += m_AlertDialogError_BeforeFormShow;
            m_AlertDialogError.Show(new Form(), pCaption, pText);
        }
        public static void Error(string pCaption, string pText, int followUpId)
        {
            m_AlertDialogError = new AlertControl()
            {
                AutoFormDelay = 10000
            };
            
            AlertButton btnDiagnose = new AlertButton(Common.Resources.magnifying_glass);
            btnDiagnose.Hint = "Diagnose audio to see more details";
            m_AlertDialogError.Buttons.Add(btnDiagnose);
            m_AlertDialogError.ButtonClick += new AlertButtonClickEventHandler((sender, e) => AlertButton_Click(sender, e, followUpId));

            m_AlertDialogError.BeforeFormShow += m_AlertDialogError_BeforeFormShow;
            m_AlertDialogError.Show(new Form(), pCaption, pText);
        }
        private static void AlertButton_Click(object sender, AlertButtonClickEventArgs e, int followUpId)
        {
            //_control.GetType().GetMethod("Check").Invoke(_control, null);
            AudioDiagnostic _AudioDiagnostic = new AudioDiagnostic();
            _AudioDiagnostic.GetAudioDetails(followUpId);
            _AudioDiagnostic.ShowDialog();
        }

        private static void m_AlertDialogError_BeforeFormShow(object sender, AlertFormEventArgs e)
        {
            DevExpress.Skins.Skin currentSkin;
            currentSkin = DevExpress.Skins.BarSkins.GetSkin(e.AlertForm.LookAndFeel);
            DevExpress.Skins.SkinElement element;
            element = currentSkin["AlertWindow"];
            Graphics g = Graphics.FromImage(element.Image.Image);
            g.FillRectangle(Brushes.Red, new Rectangle(0, 0, element.Image.Image.Width, element.Image.Image.Height));
        }
        private static void m_AlertDialogWarning_BeforeFormShow(object sender, AlertFormEventArgs e)
        {
            DevExpress.Skins.Skin currentSkin;
            currentSkin = DevExpress.Skins.BarSkins.GetSkin(e.AlertForm.LookAndFeel);
            DevExpress.Skins.SkinElement element;
            element = currentSkin["AlertWindow"];
            Graphics g = Graphics.FromImage(element.Image.Image);
            g.FillRectangle(Brushes.Red, new Rectangle(0, 0, element.Image.Image.Width, element.Image.Image.Height));
            //g.FillRectangle(Brushes.Yellow, new Rectangle(0, 0, element.Image.Image.Width, element.Image.Image.Height));
        }
        private static void m_AlertDialogInformation_BeforeFormShow(object sender, AlertFormEventArgs e)
        {
            DevExpress.Skins.Skin currentSkin;
            currentSkin = DevExpress.Skins.BarSkins.GetSkin(e.AlertForm.LookAndFeel);
            DevExpress.Skins.SkinElement element;
            element = currentSkin["AlertWindow"];
            Graphics g = Graphics.FromImage(element.Image.Image);
            g.FillRectangle(Brushes.PaleGreen, new Rectangle(0, 0, element.Image.Image.Width, element.Image.Image.Height));
        }
    }
}
