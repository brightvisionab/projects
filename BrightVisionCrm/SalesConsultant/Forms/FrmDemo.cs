using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightVision.Common.UI;
namespace SalesConsultant.Forms
{
    public partial class FrmDemo : Form
    {
        CallLogPlayer _player = null;
        public FrmDemo()
        {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            string _path = string.Format(@"{0}\test.mp3", Application.StartupPath);
            CallLogPlayer _player = new CallLogPlayer(_path);
            //_player.PlayAudio();
            _player.ShowDialog(this);
        }
    }
}
