using System.Data;
using BrightVision.Reporting.ReportDataSetTableAdapters;
using BrightVision.Common.Business;
using BrightVision.Model;
using BrightVision.Common.Utilities;

namespace BrightVision.Reporting {
    public partial class ReportDataSet {
        partial class accountstaticDataTable
        {
        }
    
        public enum eViewType
        {
            AccountsContactsWithDialogData,
            AccountsContactsWithCallAttempts
        }

        public static ReportDataSet GetSampleData()
        {
            ReportDataSet dset = new ReportDataSet();
            //dset.account.AddaccountRow(
            //    1,
            //    "Micron",
            //    "1234",
            //    "sample address",
            //    "sample street address",
            //    "4444",
            //    "Italy",
            //    "Sicily",
            //    "Sample Municipality",
            //    "Sample City",
            //    "223132",
            //    "334433",
            //    "www.micron.com",
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None",
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None",
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None",
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None",
            //    "None", 
            //    "None", 
            //    "None", 
            //    "None",
            //    "None", 
            //    "None", 
            //    "None",
            //    "None",
            //    "None", 
            //    "None", 
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None", 
            //    "None"
            //    );
            //dset.account.AddaccountRow(
            //    2,
            //    "IBM",
            //    "1234",
            //    "sample address",
            //    "sample street address",
            //    "4444",
            //    "france",
            //    "paris",
            //    "Sample Municipality",
            //    "Sample City",
            //    "223132",
            //    "334433",
            //    "www.ibm.com",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None",
            //    "None"
            //    );
            //dset.accountdialog.AddaccountdialogRow(1,dset.account[0], "Textbox", "Question Number 1 Account 1", "Answer Number 1 Account 1", "Allan K",  "02-02-2011");
            //dset.accountdialog.AddaccountdialogRow(2, dset.account[0], "Dropbox", "Question Number 2 Account 1", "Answer Number 2 Account 1", "Dolphy", "02-02-2011");
            //dset.accountdialog.AddaccountdialogRow(3, dset.account[0], "Multiple Choice", "Question Number 3 Account 1", "Answer Number 3 Account 1", "Raymart", "02-02-2011");
            //dset.accountdialog.AddaccountdialogRow(4, dset.account[1], "Textbox", "Question Number 1 Account 2", "Answer Number 1 Account 2", "Claudine", "02-02-2011");
            //dset.accountdialog.AddaccountdialogRow(5, dset.account[1], "Dropbox", "Question Number 2 Account 2", "Answer Number 2 Account 2", "Corona", "02-02-2011");
            //dset.accountdialog.AddaccountdialogRow(6, dset.account[1], "Multiple Choice", "Question Number 3 Account 1", "Answer Number 3 Account 2", "Enrile", "02-02-2011");
            //dset.accountdialog.AddaccountdialogRow(7, dset.account[1], "Multiple Choice", "Question Number 4 Account 1", "Answer Number 3 Account 2", "Justine B", "02-02-2011");
            
            //dset.contact.AddcontactRow(1, dset.account[0], "None", "allan", "firstcontact", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //"None", "None", "None", "None", "None","None");
            //dset.contact.AddcontactRow(2, dset.account[0], "None", "peter", "secondcontact", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //"None", "None", "None", "None", "None", "None");
            //dset.contact.AddcontactRow(3, dset.account[0], "None", "gowing", "secondcontact", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //"None", "None", "None", "None", "None", "None");
            //dset.contact.AddcontactRow(4, dset.account[1], "None", "bernard", "firstcontact", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //"None", "None", "None", "None", "None", "None");
            //dset.contact.AddcontactRow(5, dset.account[1], "None", "lany", "secondcontact", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //    "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None", "None",
            //"None", "None", "None", "None", "None", "None");

            //dset.contactdialog.AddcontactdialogRow(1, dset.contact[0], "Textbox", "Question 1 Contact 1 Account 1", "Answer 1 Contact 1 Account 1", "James P.", "02-04-2011");
            //dset.contactdialog.AddcontactdialogRow(2, dset.contact[0], "Smarttext", "Question 2 Contact 1 Account 1", "Answer 2 Contact 1 Account 1", "James P.", "02-04-2011");
            //dset.contactdialog.AddcontactdialogRow(3, dset.contact[0], "MultipleChoice", "Question 3 Contact 1 Account 1", "Answer 3 Contact 1 Account 1", "James P.", "02-04-2011");
            //dset.contactdialog.AddcontactdialogRow(4, dset.contact[0], "Dropbox", "Question 4 Contact 1 Account 1", "Answer 4 Contact 1 Account 1", "James P.", "02-04-2011");
            
            //dset.contactdialog.AddcontactdialogRow(5, dset.contact[1], "Smarttext", "Question 1 Contact 1 Account 1", "Answer 2 Contact 1 Account 1", "James P.", "02-04-2011");
            //dset.contactdialog.AddcontactdialogRow(6, dset.contact[1], "MultipleChoice", "Question 2 Contact 1 Account 1", "Answer 3 Contact 1 Account 1", "James P.", "02-04-2011");

            //dset.contactdialog.AddcontactdialogRow(7, dset.contact[4], "Textbox", "Question 1 Contact 1 Account 1", "Answer 1 Contact 1 Account 1", "James P.", "02-04-2011");
            //dset.contactdialog.AddcontactdialogRow(8, dset.contact[4], "Smarttext", "Question 2 Contact 1 Account 1", "Answer 2 Contact 1 Account 1", "James P.", "02-04-2011");
            //dset.contactdialog.AddcontactdialogRow(9, dset.contact[4], "MultipleChoice", "Question 3 Contact 1 Account 1", "Answer 3 Contact 1 Account 1", "James P.", "02-04-2011");
            return dset;
        }

        public static ReportDataSet GetReportDataset(int pViewid, int pCustomerId, eViewType pViewType, int pAccountId = 0, string pDatabaseConnection = "") 
        {
            /**
             * reason for this is that web portal calls does not recognize user session instance.
             * so we needed to override the connection string to accept both web portal and application level.
             */
            if (string.IsNullOrEmpty(pDatabaseConnection))
                pDatabaseConnection = UserSession.ProviderConnection;

            else {
                pDatabaseConnection = pDatabaseConnection.Replace("&quot;", "'");
                pDatabaseConnection = pDatabaseConnection.Replace("metadata=res://*/BrightPlatform.csdl|res://*/BrightPlatform.ssdl|res://*/BrightPlatform.msl;provider=System.Data.SqlClient;provider connection string=", "");
                pDatabaseConnection = pDatabaseConnection.Replace("\"", "");
            }

            ReportDataSet adt = new ReportDataSet();
            
            accountTableAdapter ata = new accountTableAdapter();
            contactTableAdapter cta = new contactTableAdapter();
            customersTableAdapter _taCustomer = new customersTableAdapter();

            ata.Connection = new System.Data.SqlClient.SqlConnection(pDatabaseConnection);
            cta.Connection = new System.Data.SqlClient.SqlConnection(pDatabaseConnection);
            _taCustomer.Connection = new System.Data.SqlClient.SqlConnection(pDatabaseConnection);

            if (pViewType == eViewType.AccountsContactsWithDialogData) {
                ata.Fill(adt.account, pViewid, pAccountId);
                cta.Fill(adt.contact, pViewid, pAccountId);
            }
            else if (pViewType == eViewType.AccountsContactsWithCallAttempts) {
                ata.FillByGetReportAccountDataWithCallAttempts(adt.account, pViewid, pAccountId);
                cta.FillByGetReportContactDataWithCallAttempts(adt.contact, pViewid, pAccountId);
            }

            _taCustomer.Fill(adt.customers, pCustomerId);
            return adt;
        }
    }
}
