using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SalesConsultant.Modules {
    public partial class DeleteDialog : DevExpress.XtraEditors.XtraUserControl {
        public DeleteDialog() {
            InitializeComponent();
            SelectedValue = 1;
        }

        public byte SelectedValue { get; set; }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e) {
            var edit = sender as RadioGroup;
            if(edit.EditValue != null && edit.EditValue.ToString() != string.Empty)
                SelectedValue = (byte) edit.EditValue;
        }

    }
}
