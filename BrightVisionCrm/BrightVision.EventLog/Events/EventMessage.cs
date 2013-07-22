using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;

using BrightVision.Common.Business;
using BrightVision.Model;
using BrightVision.Threading;

namespace BrightVision.EventLog {

    [Serializable]
    public class EventMessage : WorkItem {
                
        public EventMessage() { }

        public int EventID { get; set; }
        
        public int UserID { get; set; }

        public int SubCampaignID { get; set; }

        public int AccountID { get; set; }

        public int ContactID { get; set; }
        
        public DateTime LocalDateTime { get; set; }

        public string ComputerName { get; set; }

        public string Param1 { get; set; }

        public string Param2 { get; set; }

        public string Param3 { get; set; }

        public string Param4 { get; set; }

        public string Param5 { get; set; }

        public string Param6 { get; set; }

        public bool IsLoadTitle { get; set; }

        public override void Perform() {
            //Add this event message to the event log table db.
            //try {
            /**
             * @jeff 05.30.2012: https://brightvision.jira.com/browse/PLATFORM-1445
             * - if no contact id is defined, just let it be as null, to be accepted by the foreign key constraint.
             */
            int? _contactId = null;
                if (ContactID != null)
                    if (ContactID > 0)
                        _contactId = ContactID;

                var BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
                if (!IsLoadTitle) {
                    var eventLog = new event_log() {
                        event_id = EventID,
                        user_id = UserID,
                        subcampaign_id = SubCampaignID,
                        account_id = AccountID,
                        contact_id = _contactId,
                        local_datetime = DateTime.Now,
                        computer_name = ComputerName,
                        param1 = Param1,
                        param2 = Param2,
                        param3 = Param3,
                        param4 = Param4,
                        param5 = Param5,
                        param6 = Param6
                    };
                    BPContext.event_log.AddObject(eventLog);
                    BPContext.SaveChanges();
                } else {
                    //load global titles                     
                    UserSession.CurrentUser.TitleList =
                        BrightVision.Common.Utilities.DatabaseUtility.ExecuteStoredProcedure("bvGetTitles_sp", null);
                }
            //} catch(Exception ex) {
                //throw (ex);
            //}
            //System.Diagnostics.Debug.WriteLine("new Log is written: " + eventLog.GetHashCode().ToString());
        }

    }
}
