using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.DQControl.Business;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace BrightVision.DQControl.UI {
    public partial class AddAttendies : System.Windows.Forms.Form {
        private Schedule m_oSchedule;
        private List<CTScSubCampaignContactList> m_oSubCampaignContactList;
        public AddAttendies(Schedule objSchedule, List<CTScSubCampaignContactList> campaignList) {
            InitializeComponent();
            this.m_oSchedule = objSchedule;
            this.m_oSubCampaignContactList = campaignList;
            BindGrid();
        }

        private void BindGrid() {
            var schedContactAttendies = m_oSchedule.ContactAttendies;
            CampaignQuestionnaire Questionnaire = m_oSchedule.Questionnaire;
            int accountid = int.Parse(Questionnaire.Form.Settings.DataBindings.account_id);
            if (m_oSubCampaignContactList == null) return;

            var AllAttendies = new List<ContactAttendie>();            
            m_oSubCampaignContactList.ForEach(delegate(CTScSubCampaignContactList ca) {
                AllAttendies.Add(new ContactAttendie() {
                    AccountID = accountid,
                    ContactID = ca.id,
                    Name = ca.first_name + (ca.last_name.Length > 0 ? " " + ca.last_name : ""),
                    Address = ca.complete_address,
                    City = "",
                    Email = ca.email,
                    Telephone = ca.direct_phone,
                    Attending = false
                });
            });

            var contactAttendies = AllAttendies;
            
            //filter contacts for remaining list            
            var remList = contactAttendies.Except(schedContactAttendies.AsEnumerable());
            gridControl1.DataSource = remList.ToList();
        }
        private void btnAdd_Click(object sender, EventArgs e) {
            var atts = m_oSchedule.ContactAttendies;            
            int[] rowHandles = gridView1.GetSelectedRows();
            if (rowHandles.Length > 0) {
                if (atts == null)
                    atts = new List<ContactAttendie>();
                for (int x = 0; x < rowHandles.Length; ++x) {
                    ContactAttendie att = gridView1.GetRow(rowHandles[x]) as ContactAttendie;
                    if (att != null) {
                        using (BrightPlatformEntities _efDbContext = new BrightPlatformEntities(UserSession.EntityConnection)) {
                            contact _eftContact = _efDbContext.contacts.FirstOrDefault(i => i.id == att.ContactID);
                            if (_eftContact != null) {
                                att.Email = _eftContact.email;
                                att.Address = string.Format("{0}{1}{2}{3}",
                                    string.IsNullOrEmpty(_eftContact.address_1) ? "" : _eftContact.address_1,
                                    string.IsNullOrEmpty(_eftContact.address_2) ? "" : " " + _eftContact.address_2,
                                    string.IsNullOrEmpty(_eftContact.city) ? "" : ", " + _eftContact.city,
                                    string.IsNullOrEmpty(_eftContact.country) ? "" : ", " + _eftContact.country
                                );
                                att.Telephone = _eftContact.direct_phone;
                                _efDbContext.Detach(_eftContact);
                            }
                        }

                        att.Attending = true;
                        atts.Add(att);
                    }
                }
            }
        }
    }
}