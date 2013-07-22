
using System;
using System.Linq;
using System.Windows.Forms;

using BrightVision.Model;
using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace SalesConsultant.Forms 
{
    public partial class AddContact : XtraForm 
    {
        #region Constructors
        public AddContact(int pAccountId) 
        {
            InitializeComponent();
            this.KeyPreview = true;
            m_AccountID = pAccountId;
        }
        public AddContact(int pAccountId, int pContactId) 
        {
            InitializeComponent();
            this.KeyPreview = true;
            m_AccountID = pAccountId;
            m_ContactId = pContactId;
            m_Edit = true;
        }
        #endregion

        #region Private Properties
        private int m_AccountID;
        private int m_ContactId;
        private bool m_Edit = false;
        //private BrightPlatformEntities BPContext = null;
        private string m_Firstname { get; set; }
        private string m_Lastname { get; set; }
        #endregion

        #region Public Properties
        public object OwnerUI { get; set; }
        public int ContactId { get; set; }
        #endregion

        #region Controller
        private void simpleButtonSave_Click(object sender, EventArgs e) 
        {
            if (textEditFirstname.Text.Length < 1 || textEditLastname.Text.Length < 1) {
                NotificationDialog.Information("Bright Sales", "Firstname / Lastname is required.");
                DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            bool IsActivated = false;
            contact objContact = ObjectContact.GetContact(textEditFirstname.Text, textEditLastname.Text, m_AccountID);
            if (objContact != null) {
                if (!m_Edit && !objContact.active) {
                    string Message = String.Format("This contact is currently de-activated.{0}Would you like to re-activate this contact?", Environment.NewLine);
                    DialogResult objResult = MessageBox.Show(Message, "Add Contact", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (objResult == DialogResult.Yes) {
                        objContact.active = true;
                        m_Edit = true;
                        IsActivated = true;
                    } else {
                        DialogResult = System.Windows.Forms.DialogResult.None;
                        return;
                    }
                } 
                else if (m_Edit && !objContact.active) {
                    NotificationDialog.Information("Bright Sales", "Contact already exist and is currently de-activated. Please enter another contact name.");
                    DialogResult = System.Windows.Forms.DialogResult.None;
                    return;
                } 
                else if (m_Edit && objContact.first_name.Equals(m_Firstname) && objContact.last_name.Equals(m_Lastname)) {
                    // just bypass here
                } 
                else {
                    NotificationDialog.Information("Bright Sales", "Contact already exist. Please enter another contact name.");
                    DialogResult = System.Windows.Forms.DialogResult.None;
                    return;
                }
            }

            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            //BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            if (objContact == null) {
                objContact = new contact();
                objContact.validated = 0;
                objContact.active = true;
                objContact.created_by = UserSession.CurrentUser.UserId;
                objContact.created_date = DateTime.Now;
            } 
            else if (m_Edit) {
                if (!IsActivated) {
                    objContact.modified_date = DateTime.Now;
                    objContact.modified_by = UserSession.CurrentUser.UserId;
                    objContact.active = true;
                }
            }

            objContact.address_1 = textEditAddress1.Text;
            objContact.address_2 = textEditAddress2.Text;
            objContact.city = textEditCity.Text;
            objContact.country = textEditCountry.Text;
            objContact.direct_phone = textEditDirectphone.Text;
            objContact.email = textEditEmail.Text;
            objContact.linkedin_url = textEditLinkedIn.Text;
            objContact.first_name = textEditFirstname.Text;
            objContact.last_name = textEditLastname.Text;
            objContact.middle_name = textEditMiddlename.Text;
            objContact.mobile = textEditMobile.Text;
            objContact.title = cboTitle.Text;
            objContact.zipcode = textEditZip.Text;

            ContactId = ObjectContact.SaveContact(objContact, m_Edit, m_ContactId);
            if (objContact.id < 1)
                objContact.id = ContactId;

            if (!m_Edit)
                ObjectCompany.SaveCompanyContact(m_AccountID, objContact.id);

            ObjectContact.SaveContactTitle(cboTitle.Text);
            Cursor.Current = currentCursor;
            this.Close();
        }
        private void simpleButtonCancel_Click(object sender, EventArgs e) 
        {
            this.Close();
        }
        private void AddContact_Load(object sender, EventArgs e) 
        {
            textEditCompanyname.Text = this.GetCompanyName(m_AccountID);
            this.InitTitles();

            if (m_Edit)
                this.LoadContactInformation(m_ContactId);
        }
        private void cboTitle_TextChanged(object sender, EventArgs e) 
        {
            this.BindComboBoxEdit();
        } 
        #endregion

        #region Private Methods
        private void LoadContactInformation(int pContactId)
        {
            contact _eftContact = null;
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                _eftContact = _efDbContext.contacts.FirstOrDefault(i => i.id == pContactId);
                _efDbContext.Detach(_eftContact);
            }

            textEditFirstname.Text = _eftContact.first_name;
            textEditMiddlename.Text = _eftContact.middle_name;
            textEditLastname.Text = _eftContact.last_name;
            textEditDirectphone.Text = _eftContact.direct_phone;
            textEditMobile.Text = _eftContact.mobile;
            textEditEmail.Text = _eftContact.email;
            textEditLinkedIn.Text = _eftContact.linkedin_url;
            cboTitle.Text = _eftContact.title;
            textEditAddress1.Text = _eftContact.address_1;
            textEditAddress2.Text = _eftContact.address_2;
            textEditCity.Text = _eftContact.city;
            textEditZip.Text = _eftContact.zipcode;
            textEditCountry.Text = _eftContact.country;

            m_Firstname = _eftContact.first_name;
            m_Lastname = _eftContact.last_name;
        }
        private string GetCompanyName(int pAccountId)
        {
            using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                account _eftAccount = _efDbContext.accounts.Where(i => i.id == pAccountId).FirstOrDefault();
                _efDbContext.Detach(_eftAccount);
                return _eftAccount.company_name;
            }
        }
        private void InitTitles()
        {
            cboTitle.Properties.BeginUpdate();
            cboTitle.Properties.Buttons.Clear();
            cboTitle.Properties.TextEditStyle = TextEditStyles.Standard;
            cboTitle.Properties.NullText = "";
            cboTitle.Properties.AutoComplete = true;
            cboTitle.Properties.ImmediatePopup = false;
            cboTitle.Properties.CaseSensitiveSearch = false;
            cboTitle.Properties.EndUpdate();
        }
        private void BindComboBoxEdit()
        {
            cboTitle.Properties.Items.Clear();
            var arr = ObjectContact.GetTitles(cboTitle.Text);
            if (arr != null && arr.Count > 0) {
                cboTitle.Properties.Items.AddRange(arr.ToArray());
                cboTitle.ShowPopup();
            }
            else 
                cboTitle.ClosePopup();
        }
        #endregion

        #region Keyboard Shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) 
        {
            if (FormUtility.ShortCutKeysHandled(keyData))
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
    }
}
