using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Windows_Forms_Softphone.Windows
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void LinkLabelWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://www.voip-sip-sdk.com/");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void LinkLabelEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("mailto:" + "info@voip-sip-sdk.com");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
