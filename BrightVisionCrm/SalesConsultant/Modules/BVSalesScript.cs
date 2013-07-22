
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

using DevExpress.XtraEditors;
using DevExpress.XtraRichEdit;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using BrightVision.Common.UI;

namespace SalesConsultant.Modules
{
    public partial class BvSalesScript : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public BvSalesScript()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Properties
        public int FinalListId { get; set; }
        public bool AllowSaving { get; set; }
        #endregion

        #region Private Properties
        private int m_FinalListId = 0;
        #endregion

        #region Public Methods
        public void Show(int pFinalListId)
        {
            if (pFinalListId == m_FinalListId)
                return;

            m_FinalListId = pFinalListId;
            this.Show();
        }
        public void SetAsReadOnly(bool pState)
        {
            cmdSave.Enabled = !pState;
            docSalesScript.Enabled = !pState;
        }
        public void Clear()
        {
            docSalesScript = new RichEditControl();
        }
        #endregion

        #region Private Methods
        private void Show()
        {
            docSalesScript.CreateNewDocument();
            StringBuilder _sbCommandQuery = new StringBuilder();
            _sbCommandQuery.AppendFormat("SELECT data_content FROM sub_campaign_sales_scripts WHERE final_list_id = {0}", m_FinalListId.ToString());
            SqlCommand _sqlCommand = new SqlCommand(_sbCommandQuery.ToString());
            string DocContent = DatabaseUtility.GetDocumentContent(_sqlCommand);
            if (!string.IsNullOrEmpty(DocContent))
                docSalesScript.WordMLText = DocContent;
        }
        private byte[] ReadFile(MemoryStream RtfDocument)
        {
            long StartPosition = RtfDocument.Position;
            RtfDocument.Position = 0;

            try {
                byte[] RtfDocBuffer = new byte[RtfDocument.Length];
                int TotalBytesRead = 0;
                int BytesRead = 0;

                while ((BytesRead = RtfDocument.Read(RtfDocBuffer, TotalBytesRead, RtfDocBuffer.Length - TotalBytesRead)) > 0) {
                    TotalBytesRead += BytesRead;
                    if (TotalBytesRead == RtfDocBuffer.Length) {
                        int NextByte = RtfDocument.ReadByte();
                        if (NextByte != -1) {
                            byte[] TempByte = new byte[RtfDocBuffer.Length * 2];
                            Buffer.BlockCopy(RtfDocBuffer, 0, TempByte, 0, RtfDocBuffer.Length);
                            Buffer.SetByte(TempByte, TotalBytesRead, (byte)NextByte);
                            RtfDocBuffer = TempByte;
                            TotalBytesRead++;
                        }
                    }
                }

                byte[] ReturnBuffer = RtfDocBuffer;
                if (RtfDocBuffer.Length != TotalBytesRead) {
                    ReturnBuffer = new byte[TotalBytesRead];
                    Buffer.BlockCopy(RtfDocBuffer, 0, ReturnBuffer, 0, TotalBytesRead);
                }

                return ReturnBuffer;
            }
            finally {
                RtfDocument.Position = StartPosition;
            }
        }
        #endregion

        #region Object Events
        private void btnPrint_Click(object sender, EventArgs e)
        {
            docSalesScript.ShowPrintPreview();
        }
        private void cmdSave_Click(object sender, EventArgs e)
        {
            if (!AllowSaving)
                return;

            DialogResult _dlgResult = MessageBox.Show("Are you sure to save this document?", "Bright Sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (_dlgResult == DialogResult.No)
                return;

            WaitDialog.Show(ParentForm, "Saving...");
            SqlCommand _sqlCommand = null;
            try {
                MemoryStream objSalesScript = new MemoryStream();
                docSalesScript.SaveDocument(objSalesScript, DocumentFormat.WordML);
                byte[] WordDocFile = this.ReadFile(objSalesScript);

                _sqlCommand = new SqlCommand("bvSaveBvSalesScript_sp");
                _sqlCommand.CommandType = CommandType.StoredProcedure;
                _sqlCommand.Parameters.Add("@p_final_list_id", SqlDbType.BigInt).Value = m_FinalListId;// FinalListId;
                _sqlCommand.Parameters.Add("@p_data_content", SqlDbType.VarBinary).Value = (object)WordDocFile;
                _sqlCommand.Parameters.Add("@p_user_id", SqlDbType.Int).Value = UserSession.CurrentUser.UserId;
                DatabaseUtility.ExecuteSqlCommand(_sqlCommand);
            }
            catch (Exception ex) {
                NotificationDialog.Error("Bright Sales", ex.InnerException.Message);
            }
            finally {
                _sqlCommand.Dispose();
                _sqlCommand = null;
                WaitDialog.Close();
            }
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Reloading data ...");
            this.Show();
            WaitDialog.Close();
        }
        #endregion
    }
}
