
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SalesConsultant.Modules;
using BrightVision.EventLog;

namespace SalesConsultant.Events
{
    public class DialogEditorEvents
    {
        public class OnSaveCompletedArgs {
            public string Status { get; set; }
            public int DialogId { get; set; }
            public int ContactId { get; set; }
            public EventLog EventLog { get; set; }
        }
        public class EventLog {
            public BrightVision.Common.Classes.EventLog.EventTypes EventType { get; set; }
            public string[] Parameters { get; set; }
        }
        public class OnAnswerChange
        {
        }
        public class OnContactDropdownChange
        {
            public int ContactId { get; set; }
        }
        public class OnDelete
        {
        }
        public class AfterDelete
        {
            public bool IsCancelled { get; set; }
        }
        public class OnEditDialog
        {
        }
        public class OnSaveCompleted
        {
            public OnSaveCompletedArgs OnSaveCompletedArgs { get; set; }
            public eTransactionSource TransactionSource = eTransactionSource.None;
        }
        public class OnChangeDialogStatus
        {
            public OnSaveCompletedArgs OnSaveCompletedArgs { get; set; }
            public eTransactionSource TransactionSource = eTransactionSource.None;
        }
        public enum eTransactionSource {
            OnContactStatusChange,
            OnSaveButtonClick,
            None
        }
    }
}
