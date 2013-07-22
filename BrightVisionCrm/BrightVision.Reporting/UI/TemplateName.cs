
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BrightVision.Reporting.UI
{
    public partial class TemplateName : Form
    {
        #region Constructors
        public TemplateName()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Event & Arguments
        public delegate void AfterSaveEventHandler(object sender, TempalteFormArgs e);
        public event AfterSaveEventHandler AfterSave;

        public class TempalteFormArgs : EventArgs
        {
            public string ReportTemplateName;
        }
        #endregion

        #region Public Properties
        public string ReportTemplateName
        {
            set
            {
                _templateName = value;
                tbxTemplateName.Text = _templateName;
            }
            get
            {
                return _templateName;
            }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Properties
        private string _templateName = string.Empty;
        #endregion

        #region Private Methods
        #endregion

        #region Object Events
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (AfterSave != null)
                AfterSave(this, new TempalteFormArgs() { 
                    ReportTemplateName = tbxTemplateName.Text 
                });

            ReportTemplateName = tbxTemplateName.Text;
        }
        #endregion
    }
}
