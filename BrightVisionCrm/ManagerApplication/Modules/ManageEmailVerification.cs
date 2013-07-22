
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Model;


namespace ManagerApplication.Modules
{
    public partial class ManageEmailVerification : DevExpress.XtraEditors.XtraUserControl
    {
        #region Constructors
        public ManageEmailVerification()
        {
            this.Visible = false;
            InitializeComponent();
            this.LoadContactEmails();
            DateTime _date = Convert.ToDateTime(m_efDbModel.contacts.Max(i => i.email_last_verified_on));
            if (_date == null)
                dteLastVerificationDate.EditValue = DateTime.Now;
            else
                dteLastVerificationDate.EditValue = m_efDbModel.contacts.Max(i => i.email_last_verified_on);
            this.Visible = true;
        }
        #endregion

        #region Public Properties
        #endregion

        #region Private Properties
        private List<Email.ContactEmail> m_lstContactEmails = null;
        private BackgroundWorker m_bgwEmailVerification = null;
        private BrightPlatformEntities m_efDbModel = new BrightPlatformEntities(UserSession.EntityConnection);
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void LoadContactEmails()
        {
            try
            {
                gcContactEmail.DataSource = null;
                if (m_lstContactEmails.Count > 0)
                {
                    gcContactEmail.DataSource = m_lstContactEmails;
                    gvContactEmail.BestFitColumns();
                }
            }
            catch { }
        }
        #endregion

        #region Object Events
        private void cmdVerifySelected_Click(object sender, EventArgs e)
        {
            try
            {
                m_bgwEmailVerification = null;
                m_bgwEmailVerification = new BackgroundWorker();
                m_bgwEmailVerification.WorkerSupportsCancellation = true;
                m_bgwEmailVerification.DoWork += new DoWorkEventHandler(m_bgwEmailVerification_DoWork);
                m_bgwEmailVerification.RunWorkerAsync();
            }
            catch { }
        }
        private void cmdVerifyAll_Click(object sender, EventArgs e)
        {
            try
            {
                m_bgwEmailVerification = null;
                m_bgwEmailVerification = new BackgroundWorker();
                m_bgwEmailVerification.WorkerSupportsCancellation = true;
                m_bgwEmailVerification.DoWork += new DoWorkEventHandler(m_bgwEmailVerification_DoWork);
                m_bgwEmailVerification.RunWorkerAsync();
            }
            catch { }
        }
        private void m_bgwEmailVerification_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate 
            {
                WaitDialog.Show("Verifying emails...");
                for (int i = 0; i < gvContactEmail.RowCount; i++)
                {
                    Email.ContactEmail _item = gvContactEmail.GetRow(i) as Email.ContactEmail;
                    contact _contact = m_efDbModel.contacts.FirstOrDefault(obj => obj.id == _item.contact_id);
                    if (_contact == null)
                        continue;

                    /**
                     * validate first if to proceed with the email verification.
                     * depending if the item satisfies the set parameters by the user on the gui.
                     */
                    #region Code Logic
                    if (!_item.selected)
                        continue;

                    else if (_item.last_verified_date != null)
                    {
                        if (_item.last_verified_date <= Convert.ToDateTime(dteLastVerificationDate.EditValue))
                            continue;
                    }

                    else if (cboVerificationStatus.Text.Equals("Not Tested Only"))
                    {
                        if (_item.verify_attempt_1 != 0 && _item.verify_attempt_2 != 0 && _item.verify_attempt_3 != 0)
                            continue;
                    }

                    else if (cboVerificationStatus.Text.Equals("Failed Only"))
                    {
                        if (_item.verify_attempt_1 != 1 && _item.verify_attempt_2 != 1 && _item.verify_attempt_3 != 1)
                            continue;
                    }

                    else if (cboVerificationStatus.Text.Equals("Verified Only"))
                    {
                        if (_item.verify_attempt_1 != 2 && _item.verify_attempt_2 != 2 && _item.verify_attempt_3 != 2)
                            continue;
                    }
                    #endregion

                    /**
                     * contact email verification
                     */
                    #region Code Logic
                    bool _IsValidEmail = false;
                    int _VerificationRetries = Convert.ToInt16(cboNoOfRetry.Text);
                    gvContactEmail.SetRowCellValue(i, "last_verified_date", DateTime.Now);
                    _contact.email_last_verified_by = UserSession.CurrentUser.UserId;
                    _contact.email_last_verified_on = DateTime.Now;
                    _contact.email_verified = false;
                    for (int x = 0; x < _VerificationRetries; x++)
                    {
                        _IsValidEmail = false;
                        if (SmtpValidator.Validate(_item.email))
                            _IsValidEmail = true;

                        if (x == 0)
                        {
                            _item.verify_attempt_1 = _IsValidEmail ? (short)2 : (short)1;
                            gvContactEmail.SetRowCellValue(i, "s_verify_attempt_1", Email.GetVerifyCodeValue((short)_item.verify_attempt_1));
                            _contact.email_verify_attempt_1 = (byte)_item.verify_attempt_1;
                        }
                        else if (x == 1)
                        {
                            _item.verify_attempt_2 = _IsValidEmail ? (short)2 : (short)1;
                            gvContactEmail.SetRowCellValue(i, "s_verify_attempt_2", Email.GetVerifyCodeValue((short)_item.verify_attempt_2));
                            _contact.email_verify_attempt_2 = (byte)_item.verify_attempt_2;
                        }
                        else if (x == 2)
                        {
                            _item.verify_attempt_3 = _IsValidEmail ? (short)2 : (short)1;
                            gvContactEmail.SetRowCellValue(i, "s_verify_attempt_3", Email.GetVerifyCodeValue((short)_item.verify_attempt_3));
                            _contact.email_verify_attempt_3 = (byte)_item.verify_attempt_3;
                        }

                        gvContactEmail.SetRowCellValue(i, "verify_no", _item.verify_attempt_1.ToString() + _item.verify_attempt_2.ToString() + _item.verify_attempt_3.ToString());
                        _contact.email_verified = _IsValidEmail;
                        m_efDbModel.contacts.ApplyCurrentValues(_contact);

                        /**
                         * we wont need to re-verify again if its already verified
                         */
                        if (_IsValidEmail)
                            break;
                    }
                    m_efDbModel.SaveChanges();
                    #endregion
                }
                WaitDialog.Close();

            }));
            e.Cancel = true;
        }
        private void cmdCancelVerification_Click(object sender, EventArgs e)
        {
            m_bgwEmailVerification.CancelAsync();
        }
        private void cmdAddContactsToVerify_Click(object sender, EventArgs e)
        {
            WaitDialog.Show("Loading form...");
            PopupDialog _pdlg = new PopupDialog();
            AddEmailVerifyContact _moAddContactEmail = new AddEmailVerifyContact();
            _moAddContactEmail.btnAddToQueue_OnClick += new AddEmailVerifyContact.btnAddToQueueOnClickEventHandler(_moAddContactEmail_btnAddToQueue_OnClick);
            _moAddContactEmail.Dock = DockStyle.Fill;
            _pdlg.FormBorderStyle = FormBorderStyle.FixedSingle;
            _pdlg.MinimizeBox = false;
            _pdlg.MaximizeBox = false;
            _pdlg.StartPosition = FormStartPosition.CenterScreen;
            _pdlg.Text = "Add Contact Emails for Verification";
            _pdlg.Size = new Size(_moAddContactEmail.Width + 2, _moAddContactEmail.Height + 2);
            _pdlg.Controls.Add(_moAddContactEmail);
            _pdlg.Show();
            WaitDialog.Close();
        }
        private void cbxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < gvContactEmail.RowCount; i++)
                gvContactEmail.SetRowCellValue(i, "selected", cbxSelectAll.Checked);
        }
        #endregion

        #region Subscribed Events
        private void _moAddContactEmail_btnAddToQueue_OnClick(object sender, AddEmailVerifyContact.ContactEmailArgs e)
        {
            //m_lstContactEmailQueuer
            BrightPlatformEntities efDbModel = new BrightVision.Model.BrightPlatformEntities(UserSession.EntityConnection);
            foreach (CTEmailVerifyContact _item in e.lstEmailVerifyContacts)
            {
                if (m_lstContactEmails == null)
                    m_lstContactEmails = new List<Email.ContactEmail>();

                else if (m_lstContactEmails.Find(x => x.contact_id == _item.contact_id) != null)
                    continue;

                _item.email_verify_attempt_1 = _item.email_verify_attempt_1 == null ? 0 : _item.email_verify_attempt_1;
                _item.email_verify_attempt_2 = _item.email_verify_attempt_2 == null ? 0 : _item.email_verify_attempt_2;
                _item.email_verify_attempt_3 = _item.email_verify_attempt_3 == null ? 0 : _item.email_verify_attempt_3;

                Email.ContactEmail _ContactEmail = new Email.ContactEmail();
                _ContactEmail.company_name = _item.company_name;
                _ContactEmail.contact_id = _item.contact_id;
                _ContactEmail.email = _item.email;
                _ContactEmail.last_verified_by = _item.email_last_verified_by == null ? 0 : (int)_item.email_last_verified_by;
                _ContactEmail.last_verified_date = _item.email_last_verified_on;
                _ContactEmail.name = _item.first_name + " " + _item.last_name;
                _ContactEmail.s_verify_attempt_1 = Email.GetVerifyCodeValue((short)_item.email_verify_attempt_1);
                _ContactEmail.s_verify_attempt_2 = Email.GetVerifyCodeValue((short)_item.email_verify_attempt_2);
                _ContactEmail.s_verify_attempt_3 = Email.GetVerifyCodeValue((short)_item.email_verify_attempt_3);
                _ContactEmail.selected = true;
                _ContactEmail.verify_attempt_1 = (short)_item.email_verify_attempt_1;
                _ContactEmail.verify_attempt_2 = (short)_item.email_verify_attempt_2;
                _ContactEmail.verify_attempt_3 = (short)_item.email_verify_attempt_3;
                _ContactEmail.verify_no = _item.email_verify_attempt_1.ToString() + _item.email_verify_attempt_2.ToString() + _item.email_verify_attempt_3.ToString();
                m_lstContactEmails.Add(_ContactEmail);
            }

            this.LoadContactEmails();
        }
        #endregion
    }
}
