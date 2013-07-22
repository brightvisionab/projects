
#region Namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.Objects;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.Import;
using DevExpress.Utils.Commands;
using SalesConsultant.Business;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
#endregion

namespace SalesConsultant.Modules
{
    public partial class MySalesScript : DevExpress.XtraEditors.XtraUserControl
    {
        #region Public Properties
        public bool AllowSaving { get; set; }
        #endregion

        #region Private Properties
        private int m_UserId = 0;
        private bool m_DoneLoadingUsers = false;
        private int m_FinalListId = 0;
        #endregion

        #region Constructor
        public MySalesScript()
        {
            InitializeComponent();
            richEditBarController1.RichEditControl = null;
            m_UserId = UserSession.CurrentUser.UserId;
            m_DoneLoadingUsers = false;
            this.LoadUsers();
            m_DoneLoadingUsers = true;
            //docSalesScript.BeforeImport += new BeforeImportEventHandler(docSalesScript_BeforeImport);            
        }

        //public MySalesScript(int pFinalListId)
        //{
        //    InitializeComponent();
        //    richEditBarController1.RichEditControl = null;
        //    //m_FinalListId = pFinalListId;
        //    //m_UserId = UserSession.CurrentUser.UserId;
        //    //m_DoneLoadingUsers = false;
        //    //this.PopulateSubCampaignUserComboList();
        //    //m_DoneLoadingUsers = true;
        //}
        #endregion

        #region Public Methods
        public void Show(int pFinalListId)
        {
            if (m_UserId == Convert.ToInt32(cboUser.EditValue) &&
                m_FinalListId == pFinalListId)
                return;

            m_UserId = Convert.ToInt32(cboUser.EditValue);
            m_FinalListId = pFinalListId;
            docSalesScript.CreateNewDocument();
            StringBuilder sqlCommandQuery = new StringBuilder();
            sqlCommandQuery.AppendFormat("SELECT data_content FROM sub_campaign_user_sales_scripts WHERE user_id = {0} AND final_list_id = {1}", cboUser.EditValue.ToString(), m_FinalListId.ToString());
            SqlCommand objCommand = new SqlCommand(sqlCommandQuery.ToString());
            string DocumentContent = DatabaseUtility.GetDocumentContent(objCommand);
            if (!string.IsNullOrEmpty(DocumentContent))
                docSalesScript.WordMLText = DocumentContent;
        }
        public void SetAsReadOnly(bool pState)
        {
            cboUser.Properties.ReadOnly = pState;
            cmdSave.Enabled = !pState;
            cmdClear.Enabled = !pState;
            cmdLoadBvSalesScript.Enabled = !pState;
            docSalesScript.Enabled = !pState;
        }
        public void Clear()
        {
            docSalesScript = new RichEditControl();
        }
        #endregion

        #region Private Methods
        private void LoadUsers()
        {
            try
            {
                cboUser.Properties.DataSource = null;
                cboUser.Properties.DataSource = SalesScript.GetUsers(m_FinalListId);
                cboUser.Properties.DisplayMember = "name";
                cboUser.Properties.ValueMember = "id";
                cboUser.Properties.Columns.Clear();
                cboUser.Properties.Columns.Add(new LookUpColumnInfo("name"));
                cboUser.EditValue = UserSession.CurrentUser.UserId;
            }
            catch { }
        }
        private byte[] ReadFile(MemoryStream RtfDocument)
        {
            long StartPosition = RtfDocument.Position;
            RtfDocument.Position = 0;

            try
            {
                byte[] RtfDocBuffer = new byte[RtfDocument.Length];
                int TotalBytesRead = 0;
                int BytesRead = 0;

                while ((BytesRead = RtfDocument.Read(RtfDocBuffer, TotalBytesRead, RtfDocBuffer.Length - TotalBytesRead)) > 0)
                {
                    TotalBytesRead += BytesRead;
                    if (TotalBytesRead == RtfDocBuffer.Length)
                    {
                        int NextByte = RtfDocument.ReadByte();
                        if (NextByte != -1)
                        {
                            byte[] TempByte = new byte[RtfDocBuffer.Length * 2];
                            Buffer.BlockCopy(RtfDocBuffer, 0, TempByte, 0, RtfDocBuffer.Length);
                            Buffer.SetByte(TempByte, TotalBytesRead, (byte)NextByte);
                            RtfDocBuffer = TempByte;
                            TotalBytesRead++;
                        }
                    }
                }

                byte[] ReturnBuffer = RtfDocBuffer;
                if (RtfDocBuffer.Length != TotalBytesRead)
                {
                    ReturnBuffer = new byte[TotalBytesRead];
                    Buffer.BlockCopy(RtfDocBuffer, 0, ReturnBuffer, 0, TotalBytesRead);
                }

                return ReturnBuffer;
            }
            finally
            {
                RtfDocument.Position = StartPosition;
            }
        }
        private void SaveSalesScriptDocument()
        {
            string sqlCommandQuery = string.Empty;
            SqlCommand objCommand = null;

            try
            {
                MemoryStream objSalesScript = new MemoryStream();
                docSalesScript.SaveDocument(objSalesScript, DocumentFormat.WordML);
                byte[] WordDocFile = this.ReadFile(objSalesScript);

                int UserDocumentId = SalesScript.GetDocument(m_FinalListId, m_UserId);
                if (UserDocumentId == 0)
                {
                    sqlCommandQuery = "INSERT INTO sub_campaign_user_sales_scripts(data_content, final_list_id, user_id) values(@data_content, @final_list_id, @user_id)";
                    objCommand = new SqlCommand(sqlCommandQuery);
                    objCommand.Parameters.Add("@data_content", SqlDbType.VarBinary).Value = (object) WordDocFile;
                    objCommand.Parameters.Add("@final_list_id", SqlDbType.BigInt).Value = m_FinalListId;
                    objCommand.Parameters.Add("@user_id", SqlDbType.BigInt).Value = m_UserId;
                }
                else
                {
                    sqlCommandQuery = "UPDATE sub_campaign_user_sales_scripts SET data_content = @data_content WHERE id = @id";
                    objCommand = new SqlCommand(sqlCommandQuery);
                    objCommand.Parameters.Add("@data_content", SqlDbType.VarBinary).Value = (object) WordDocFile;
                    objCommand.Parameters.Add("@id", SqlDbType.BigInt).Value = UserDocumentId;                    
                }
                
                DatabaseUtility.ExecuteSqlCommand(objCommand);
            }
            catch (Exception ex)
            {
                NotificationDialog.Error("Bright Sales", ex.InnerException.Message);
            }
            finally
            {
                objCommand.Dispose();
                objCommand = null;
            }            
        }        
        #endregion

        #region Object Events
        #region Buttons
        private void btnPrint_Click(object sender, EventArgs e)
        {
            docSalesScript.ShowPrintPreview();
        }
        private void cmdSave_Click(object sender, EventArgs e)
        {
            if (!AllowSaving)
                return;

            WaitDialog.Show(ParentForm, "Saving...");
            this.SaveSalesScriptDocument();
            WaitDialog.Close();
        }
        private void cmdClear_Click(object sender, EventArgs e)
        {
            if (!AllowSaving)
                return;

            WaitDialog.Show(ParentForm, "Clearing...");
            docSalesScript.CreateNewDocument();
            WaitDialog.Close();
        }
        private void cmdLoadBvSalesScript_Click(object sender, EventArgs e)
        {
            if (!AllowSaving)
                return;

            WaitDialog.Show(ParentForm, "Loading...");
            docSalesScript.CreateNewDocument();
            StringBuilder _sbCommandQuery = new StringBuilder();
            _sbCommandQuery.AppendFormat("SELECT data_content FROM sub_campaign_sales_scripts WHERE final_list_id = {0}", m_FinalListId.ToString());
            SqlCommand _sqlCommand = new SqlCommand(_sbCommandQuery.ToString());
            string DocContent = DatabaseUtility.GetDocumentContent(_sqlCommand);
            if (!string.IsNullOrEmpty(DocContent))
                docSalesScript.WordMLText = DocContent;

            WaitDialog.Close();
        }
        #endregion

        #region Combo Box
        private void cboUser_EditValueChanged(object sender, EventArgs e)
        {
            if (m_DoneLoadingUsers)
                this.Show(Convert.ToInt32(cboUser.EditValue));
        }
        #endregion

        #region Rich Text Editor
        private void docSalesScript_BeforeImport(object sender, BeforeImportEventArgs e)
        {
            if (e.DocumentFormat == DocumentFormat.WordML)
            {
                ((PlainTextDocumentImporterOptions)e.Options).Encoding = Encoding.UTF8;
            }
        }
        //fix for ctrl + v or pasting content focus control.
        private void docSalesScript_Leave(object sender, EventArgs e)
        {
            richEditBarController1.RichEditControl = null;
        }
        //fix for ctrl + v or pasting content focus control.
        private void docSalesScript_Enter(object sender, EventArgs e)
        {
            richEditBarController1.RichEditControl = docSalesScript;
        }
        #endregion
        #endregion
    }
}
