
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesConsultant.Events
{
    class CallLogBarEvents
    {
    }

    public enum CallLogBarVoidEvent
    {
        PhoneCallStarted,
        CallBoardInitiated,
        CallDirectInitiated,
        CallMobileInitiated,
        CallAnonymousInitiated,
        EndCallInitiated,
        UseContact,
        CallNotConnected
    }
    
    /// <summary>
    /// Phone call save
    /// </summary>
    public class PhoneCallSaveEventNotifier
    {
        public int EventLogId { get; set; }
    }

    /// <summary>
    /// Phone call end
    /// </summary>
    public class PhoneCallEndEventNotifier
    {
        public int? ContactId { get; set; }
        public TimeSpan CallStart { get; set; }
        public TimeSpan CallEnd { get; set; }
    }

    /// <summary>
    /// Phone call register success
    /// </summary>
    public class PhoneRegisterSuccessEventNotifier
    {
        public string NotificationMessage { get; set; }
    }
}
