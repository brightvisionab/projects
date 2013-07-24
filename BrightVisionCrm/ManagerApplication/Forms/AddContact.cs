using System;
using System.Drawing;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Controls;

using BrightVision.Model;
using BrightVision.DQControl.Business;
using BrightVision.DQControl.UI;
using BrightVision.Common.Business;
using BrightVision.Common.Utilities;
using BrightVision.Common.UI;


namespace ManagerApplication.Forms {
    public partial class AddContact : XtraForm {
        #region Private Members
        private int m_accountID;
        private int m_ContactId;
        private bool m_Edit = false;
        private BrightPlatformEntities BPContext = null;
        private string m_Firstname { get; set; }
        private string m_Lastname { get; set; }

        private BVPopupContainerEdit ComboBoxAuto = null;

        #endregion

        #region Constructors
        public AddContact(int accountID) {
            InitializeComponent();
            this.KeyPreview = true;
            m_accountID = accountID;
            ComboBoxAuto = new BVPopupContainerEdit();
            ComboBoxAuto.BackColor = Color.Transparent;
            ComboBoxAuto.Properties.AppearanceFocused.BorderColor = Color.Transparent;
            ComboBoxAuto.Properties.AppearanceFocused.Options.UseBackColor = true;
            ComboBoxAuto.Properties.AppearanceFocused.Options.UseBorderColor = true;
            ComboBoxAuto.BorderStyle = BorderStyles.Default;
            ComboBoxAuto.Validating += new CancelEventHandler(ComboBoxAuto_Validating);
            this.layoutControl1.Controls.Add(ComboBoxAuto);
            lciTitle.Control = ComboBoxAuto;
        }

        public AddContact(int accountID, int ContactId) {
            InitializeComponent();
            this.KeyPreview = true;
            m_accountID = accountID;
            m_ContactId = ContactId;
            m_Edit = true; 
            ComboBoxAuto = new BVPopupContainerEdit(true);            
            //ComboBoxAuto.BackColor = Color.Transparent;
            //ComboBoxAuto.Properties.AppearanceFocused.BorderColor = Color.Transparent;
            //ComboBoxAuto.Properties.AppearanceFocused.Options.UseBackColor = true;
            //ComboBoxAuto.Properties.AppearanceFocused.Options.UseBorderColor = true;
            //ComboBoxAuto.BorderStyle = BorderStyles.Default;            
            ComboBoxAuto.Validating += new CancelEventHandler(ComboBoxAuto_Validating);
            this.layoutControl1.Controls.Add(ComboBoxAuto);
            lciTitle.Control = ComboBoxAuto;
        }

        

        #endregion

        #region Public Properties
        public object OwnerUI { get; set; }
        public int ContactId { get; set; }
        #endregion

        #region Controllers
        private void ComboBoxAuto_Validating(object sender, CancelEventArgs e) {
            var ctl = sender as PopupContainerEdit;
            if (ctl != null) {
                var col = ctl.Tag as object[];
                if (col == null) {
                    ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                    ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);
                    ctl.Properties.AppearanceFocused.ForeColor = Color.FromArgb(255, 255, 255);
                    ctl.Properties.AppearanceFocused.BackColor = Color.FromArgb(244, 102, 102);
                    ctl.ForeColor = Color.FromArgb(255, 255, 255);
                    ctl.BackColor = Color.FromArgb(244, 102, 102);                    
                    return;
                }
                if (col[1].ToString() != ctl.Text.Trim() || ctl.Text.Trim() == string.Empty) {                    
                    ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                    ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);
                    ctl.Properties.AppearanceFocused.ForeColor = Color.FromArgb(255, 255, 255);
                    ctl.Properties.AppearanceFocused.BackColor = Color.FromArgb(244, 102, 102);
                    ctl.ForeColor = Color.FromArgb(255, 255, 255);
                    ctl.BackColor = Color.FromArgb(244, 102, 102);                    
                } else {
                    var data = ctl.Properties.PopupControl.Tag as BVPopupContainerControlData;
                    if (data != null && data.HasMatch) {
                        ctl.Properties.Appearance.ForeColor = Color.FromArgb(0, 0, 0);
                        ctl.Properties.Appearance.BackColor = Color.FromArgb(181, 245, 146);
                        ctl.Properties.AppearanceFocused.ForeColor = Color.FromArgb(0, 0, 0);
                        ctl.Properties.AppearanceFocused.BackColor = Color.FromArgb(181, 245, 146);
                        ctl.ForeColor = Color.FromArgb(0, 0, 0);
                        ctl.BackColor = Color.FromArgb(181, 245, 146);               
                    } else {
                        ctl.Properties.Appearance.ForeColor = Color.FromArgb(255, 255, 255);
                        ctl.Properties.Appearance.BackColor = Color.FromArgb(244, 102, 102);
                        ctl.Properties.AppearanceFocused.ForeColor = Color.FromArgb(255, 255, 255);
                        ctl.Properties.AppearanceFocused.BackColor = Color.FromArgb(244, 102, 102);
                        ctl.ForeColor = Color.FromArgb(255, 255, 255);
                        ctl.BackColor = Color.FromArgb(244, 102, 102);                        
                    }
                }
            }
        }

        private void simpleButtonSave_Click(object sender, EventArgs e) {
            if (textEditFirstname.Text.Length < 1 || textEditLastname.Text.Length < 1) {
                MessageBox.Show("Firstname / Lastname is required.", "Add Contact", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }
            bool IsActivated = false;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);            

            contact objContact = ObjectContact.GetContact(textEditFirstname.Text, textEditLastname.Text, m_accountID);
            if (objContact != null) {
                if (!m_Edit && !objContact.active) {
                    string Message = "This contact is currently de-activated." + Environment.NewLine + "Would you like to re-activate this contact?";
                    DialogResult objResult = MessageBox.Show(Message, "Add Contact", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (objResult == DialogResult.Yes) {                        
                        var objC = BPContext.contacts.FirstOrDefault(x=>x.id == objContact.id);
                        objC.active = true;
                        BPContext.SaveChanges();
                        DialogResult = System.Windows.Forms.DialogResult.OK;
                        return;
                    } else {
                        DialogResult = System.Windows.Forms.DialogResult.None;
                        return;
                    }
                } else if (m_Edit && !objContact.active) {
                    string Message = "Contact already exist and is currently de-activated, please enter another contact name.";
                    MessageBox.Show(Message, "Add Contact", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    DialogResult = System.Windows.Forms.DialogResult.None;
                    return;
                }else {
                    MessageBox.Show("Contact already exist, please enter another contact name.", "Contacts", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    DialogResult = System.Windows.Forms.DialogResult.None;
                    return;
                }
            }
            
         

            var titleval = ComboBoxAuto.Tag as object[];
            int? title_id = null;
            /**
             * [@jeff 05.24.2012]: https://brightvision.jira.com/browse/PLATFORM-1433
             * added validation for null titleval
             */
            if (titleval != null)
                if (titleval.Length > 0)
                    title_id = titleval[0] != null ? (int?) int.Parse(titleval[0].ToString()) : null;

            int? contact_id;

            if (m_Edit) {
                BPContext.FIUpdateContactDetails(
                    objContact.id, textEditFirstname.Text.Trim(), textEditMiddlename.Text.Trim(), textEditLastname.Text.Trim(),
                    textEditDirectphone.Text.Trim(), textEditMobile.Text.Trim(), title_id, null, textEditRoles.Text.Trim(),
                    textEditAddress1.Text.Trim(), textEditAddress2.Text.Trim(), textEditCity.Text.Trim(),
                    textEditZip.Text.Trim(), textEditCountry.Text.Trim(), "", UserSession.CurrentUser.UserId,
                    textEditEmail.Text.Trim(), "", 0, IsActivated, m_accountID, null, 
                    UserSession.CurrentUser.ComputerName, 
                    BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Add_Contact,
                    null,
                    null,
                    null,
                    null
                );
            } else {
                contact_id = BPContext.FIUpdateContactDetails(
                        null, textEditFirstname.Text.Trim(), textEditMiddlename.Text.Trim(), textEditLastname.Text.Trim(),
                        textEditDirectphone.Text.Trim(), textEditMobile.Text.Trim(), title_id, null, textEditRoles.Text.Trim(),
                        textEditAddress1.Text.Trim(), textEditAddress2.Text.Trim(), textEditCity.Text.Trim(),
                        textEditZip.Text.Trim(), textEditCountry.Text.Trim(), "", UserSession.CurrentUser.UserId,
                        textEditEmail.Text.Trim(), "", 0, true , m_accountID, null,
                        UserSession.CurrentUser.ComputerName,
                        BrightVision.EventLog.Business.FacadeEventLog.Source_Bright_Manager_Add_Contact,
                        null,
                        null,
                        null,
                        null
                ).FirstOrDefault();

                ContactId = contact_id ?? 0;
            }
        }

        private void simpleButtonCancel_Click(object sender, EventArgs e) {
            this.Close();
        }

        private string GetCompanyName(int AccountId) {
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCompany = BPContext.accounts.Where(objRow => objRow.id == AccountId).SingleOrDefault();

            return objCompany.company_name;
        }

        private void LoadContactInformation(int ContactId) {
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            var objContact = BPContext.contacts.Where(i => i.id == ContactId).SingleOrDefault();
            textEditFirstname.Text = objContact.first_name;
            textEditMiddlename.Text = objContact.middle_name;
            textEditLastname.Text = objContact.last_name;
            textEditDirectphone.Text = objContact.direct_phone;
            textEditMobile.Text = objContact.mobile;
            textEditEmail.Text = objContact.email;
            //cboTitle.Text = objContact.title;
            textEditAddress1.Text = objContact.address_1;
            textEditAddress2.Text = objContact.address_2;
            textEditCity.Text = objContact.city;
            textEditZip.Text = objContact.zipcode;
            textEditCountry.Text = objContact.country;
            m_Firstname = objContact.first_name;
            m_Lastname = objContact.last_name;
        }

        private void AddContact_Load(object sender, EventArgs e) {
            textEditCompanyname.Text = this.GetCompanyName(m_accountID);
            //this.InitTitles();

            if (m_Edit)
                this.LoadContactInformation(m_ContactId);
        }

        //private void InitTitles() {
        //    cboTitle.Properties.Buttons.Clear();
        //    cboTitle.Properties.TextEditStyle = TextEditStyles.Standard;
        //    cboTitle.Properties.NullText = "";
        //    cboTitle.Properties.AutoComplete = true;
        //    cboTitle.Properties.ImmediatePopup = false;
        //    cboTitle.Properties.CaseSensitiveSearch = false;
        //}

        //private void BindComboBoxEdit() {
        //    cboTitle.Properties.Items.Clear();
        //    var arr = ObjectContact.GetTitles(cboTitle.Text);
        //    if (arr != null && arr.Count > 0) {
        //        cboTitle.Properties.Items.AddRange(arr.ToArray());
        //        cboTitle.ShowPopup();
        //    } else {
        //        cboTitle.ClosePopup();
        //    }
        //}

        //private void cboTitle_TextChanged(object sender, EventArgs e) {
        //    BindComboBoxEdit();
        //} 
        #endregion

        #region Keyboard Shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (FormUtility.ShortCutKeysHandled(keyData))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
    }
}
