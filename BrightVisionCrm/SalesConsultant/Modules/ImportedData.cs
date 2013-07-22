
#region Namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.Objects;

using DevExpress.XtraEditors;
using BrightVision.Common.Business;
using BrightVision.Model;
#endregion

namespace SalesConsultant.Modules
{
    public partial class ImportedData : DevExpress.XtraEditors.XtraUserControl
    {
        #region Private Members       
        private BrightPlatformEntities m_objDatabaseModel = null;
        #endregion

        #region Constructor
        public ImportedData()
        {
            InitializeComponent();
        }
        #endregion

        #region Object Events
        #endregion

        #region Public Methods
        /// <summary>
        /// Load company information
        /// </summary>
        /// <param name="AccountId"></param>
        public void LoadImportedData(int accountId, int campaign_id)
        {
            if (m_objDatabaseModel == null)
                m_objDatabaseModel = new BrightPlatformEntities(UserSession.EntityConnection);

            var m_objImportedData = m_objDatabaseModel.FIGetImportedMatchedData(accountId, campaign_id);
            if (m_objImportedData != null)
            {
                vgcCompanyInfo.DataSource = null;
                vgcCompanyInfo.DataSource = m_objImportedData;                
            }
        }

        /// <summary>
        /// Clear company information
        /// </summary>
        public void ClearImportedData()
        {
            vgcCompanyInfo.DataSource = null;
        }

        /// <summary>
        /// Disable editing
        /// </summary>
        public void DisableEditing()
        {
            vgcCompanyInfo.OptionsBehavior.Editable = false;
        }

        
        #endregion
    }
}