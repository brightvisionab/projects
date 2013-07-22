
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.PublicProperties
{
    public class SelectionProperty
    {
        /**
         * global enums.
         * common enums for general use.
         */
        public enum eWriteMode {
            New,
            Edit,
            None
        }

        /**
         * sales consultant enums
         */
        public enum CallingEnvironment {
            CampaignList,
            CampaignBooking,
            None
        }
        public enum WorkingEnvironment {
            Demo,
            None
        }

        /**
         * forms enums
         */
        public enum CurrentTab {
            CampaignList,
            CampaignBooking,
            Events,
            Help,
            MillTime,
            Report,
            InternalUsers,
            CustomerUsers,
            CustomersAndCampaigns,
            SubCampaigns,
            MasterDataTools,
            CompaniesAndContacts,
            ImportAndViewList,
            ListCreation,
            CallListCreation,
            FinalCallList,
            ManageQuestions,
            ManageDialogs,
            SIPAccounts,
            ReportConfiguration,
            ViewEventLogs,
            CallLogs,
            ViewMessageLogs,
            Dashboard,
            MasterDataImport,
            TitleSettings,
            LanguagesSettings
        }

        /**
         * campaign list enums
         */
        public enum CampaignListMode {
            CompaniesOnly,
            CampaniesAndContacts,
            None
        }

        /**
         * campaign booking enums
         */
        public enum CallMethod {
            Call_Board,
            Call_Direct,
            Call_Mobile,
            Call_Anonymous,
            None
        }
        public enum DialogSaveMode {
            Unspecified = 0,
            Edit = 1,
            Add = 2,
            Delete = 3
        }
        public enum DialogEditorState {
            Empty = 0,
            Loaded = 1
        }
        public enum CampaignBookingMode {
            Work,
            Browse,
            None
        }
        public enum ParentView {
            CampaignList,
            MyFollowUp,
            None
        }
    }
}
