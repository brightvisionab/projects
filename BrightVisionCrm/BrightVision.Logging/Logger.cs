using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Logging.Enums;
using System.Diagnostics;
using System.Configuration;
using BrightVision.Common.Business;
using System.Web;

namespace BrightVision.Logging
{
    public class Logger
    {
        private bool TurnOffLogging = true;
        //Here is the once-per-class call to initialize the log object
        private log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<LoggingField, string> logFields = new Dictionary<LoggingField, string>();
        public ConnectionType ConnectionToDB { get; set; }
        public BrightVisionApplication Application { get; set; }
        public BrightVisionEnvironment Environment { get; set; }
        public string SoftwareVersion { get; set; }
        //public int UserId { get; set; }
        //public int? CustomerId { get; set; }
        //public int? CampaignId { get; set; }
        //public int? SubCampaignId { get; set; }
        //public int? DialogId { get; set; }
        //public int? AccountId { get; set; }
        //public int? ContactId { get; set; }
        public string ComputerName { get { return System.Environment.MachineName; } }
        public long AppMemory { get { return (Process.GetCurrentProcess().PrivateMemorySize64 / 1024) / 1024; } }
        //public string TimeFromStart { get; set; }
        //public string Param1 { get; set; }
        //public string Param2 { get; set; }
        //public string Param3 { get; set; }
        public Logger(BrightVisionApplication application)
        {
            if (TurnOffLogging) return;


            string buildEnv = ConfigManager.AppSettings["BuildEnvironment"];
            if (buildEnv == "Production Environment")
            {
                this.Environment = BrightVisionEnvironment.Production;
            }
            else if (buildEnv == "Staging Environment")
            {
                this.Environment = BrightVisionEnvironment.Staging;
            }
            this.Application = application;
        }

        public void SetLogField(LoggingField field, string value) {
            if (logFields.ContainsKey(field))
            {
                logFields[field] = value;
            }
            else {
                logFields.Add(field, value);
            }
        }

        public void Info(string message)
        {
            if (TurnOffLogging) return;

            SetGlobalContext();
            log.Info(message.LoggerString());
            ClearVal();
        }
        public void SendInfo(string label, string message)
        {
            if (TurnOffLogging) return;

            log4net.GlobalContext.Properties["event_label"] = label.LoggerString();
            SetGlobalContext();
            log.Info(message.LoggerString());
            ClearVal();
        }

        private void ClearVal()
        {
            //log4net.GlobalContext.Properties["par1"] = null;
            //log4net.GlobalContext.Properties["par2"] = null;
            //log4net.GlobalContext.Properties["par3"] = null;
        }

        public void Error(string label, string message, Exception exception)
        {
            if (TurnOffLogging) return;

            log4net.GlobalContext.Properties["event_label"] = label;
            SetGlobalContext();
            log.Error(message, exception);
        }

        public void Debug(string message, Exception exception)
        {
            if (TurnOffLogging) return;

            SetGlobalContext();
            log.Debug(message, exception);
        }

        private void SetGlobalContext() {
            if (UserSession.CurrentUser.ServerName == BrightVisionServers.Gothenburg)
                ConnectionToDB = ConnectionType.Gothernburg;
            else if (UserSession.CurrentUser.ServerName == BrightVisionServers.Hamachi)
                ConnectionToDB = ConnectionType.Hamachi;
            else if (UserSession.CurrentUser.ServerName == BrightVisionServers.DemoEnv)
                ConnectionToDB = ConnectionType.DemoEnv;

            foreach (var field in logFields) {
                log4net.GlobalContext.Properties[field.Key.GetEnumDescription()] = field.Value;
            }

            log4net.GlobalContext.Properties["application"] = Application.GetEnumDescription();
            log4net.GlobalContext.Properties["environment"] = Environment.GetEnumDescription();
            log4net.GlobalContext.Properties["software_version"] = SoftwareVersion;
            log4net.GlobalContext.Properties["user_id"] = UserSession.CurrentUser.UserId;
            log4net.GlobalContext.Properties["username"] = UserSession.CurrentUser.UserFullName;
            log4net.GlobalContext.Properties["computer_name"] = ComputerName.LoggerString();
            log4net.GlobalContext.Properties["app_memory"] = AppMemory;
            log4net.GlobalContext.Properties["connection_to_db"] = ConnectionToDB.GetEnumDescription();
        }
        
    }


}
