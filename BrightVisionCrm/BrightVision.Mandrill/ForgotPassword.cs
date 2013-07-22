using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightVision.Model;
using BrightVision.Common.Utilities;
using BrightVision.Common.Business;
using BrightVision.EventLog;

namespace BrightVision.Mandrill
{
    public partial class ForgotPassword : Form
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);
        private string g_Email = null;
        public ForgotPassword(string email)
        {
            g_Email = email;
            InitializeComponent();
        }

        private void btnSendMail_Click(object sender, EventArgs e)
        {
            string newPassword = GenerateRandomPassword();
            string oldPassword = "";           
            if (!SaveToDBNewPassword(newPassword, ref oldPassword)) {
                //BrightVision.Common.UI.NotificationDialog.Error("Error", "An error has encountered. Please contact system administrator.");
                return;
            }

            string MandrillKey = ConfigManager.AppSettings["MandrillKey"];
            string MandrillAccount = ConfigManager.AppSettings["MandrillAccount"];
            string MandrillAccountName = ConfigManager.AppSettings["MandrillAccountName"];

            BrightVision.Common.Business.WaitDialog.Show(this, "Sending mail...");
            Mandrill mandrill = new Mandrill();
            mandrill.Key = MandrillKey;
            mandrill.From = MandrillAccount;
            mandrill.FromName = MandrillAccountName;
            mandrill.To = g_Email;
            mandrill.ToName = g_Email;
            mandrill.Subject = "New Password";
            mandrill.MessageTEXT = "Your new password: " + newPassword;
            

            if (!mandrill.Send())
            {
                BrightVision.Common.Business.ObjectUser.SaveUserOldPassword(g_Email, oldPassword);
                //MessageBox.Show("Error encountered when trying to send the email. \nPlease contact system administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                BrightVision.Common.UI.NotificationDialog.Error("Error", "Error encountered when trying to send the email. \nPlease contact system administrator.");
                this.Close();
            }
            else
            {
                LogEvent(g_Email, newPassword);
                BrightVision.Common.UI.NotificationDialog.Information("Success", "Email has been successfully sent. Please check your email for the new password.");
            }
            BrightVision.Common.Business.WaitDialog.Close();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool SaveToDBNewPassword(string newPassword, ref string oldPassword)
        {
            if (!BrightVision.Common.Business.ObjectUser.SaveUserPassword(g_Email, newPassword, ref oldPassword)) return false;

            return true;
        }

        private string GenerateRandomPassword()
        {
            // get 1st random string 
            string Rand1 = RandomString(4);
            string Rand2 = RandomString(4);
            // creat full rand string
            string docNum = Rand1 + "-" + Rand2;


            return docNum;
        }

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        private void LogEvent(string pEmail, string pNewPassword)
        {
            string Source = "BrightSales";
            if (Application.ProductName == "ManagerApplication") Source = "BrightManager";

            BrightPlatformEntities m_efDbContext = new BrightPlatformEntities(UserSession.EntityConnection);

            m_efDbContext.event_log.AddObject(
               new event_log()
               {
                   //event_id = (int)BrightVision.EventLog.EventTypes.EMAIL_NEW_PASSWORD,
                   event_id = (int)BrightVision.Common.Classes.EventLog.EventTypes.EMAIL_NEW_PASSWORD,
                   user_id = null,
                   subcampaign_id = null,
                   account_id = null,
                   contact_id = null,
                   local_datetime = DateTime.Now,
                   computer_name = UserSession.CurrentUser.ComputerName,
                   param1 = pNewPassword,
                   param2 = UserSession.CurrentUser.ComputerIP,
                   param3 = null,
                   param4 = Source,
                   param5 = pEmail,
                   param6 = null
               }
           );

            m_efDbContext.SaveChanges();
        }
    }
}
