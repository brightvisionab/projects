using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Configuration;
using System.ServiceProcess;
//using System.Runtime.Remoting;
//using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Tcp;
//using BrightVision.RemotableObject;

using System.Threading;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

using BrightVision.Storage.Repositories;
using BrightVision.Storage.Models;
using BrightVision.Storage.Queue;

using NSpring.Logging;
using NSpring.Logging.Loggers;
using MSDTS = Microsoft.SqlServer.Dts.Runtime;
using BrightVision.Common.Business;

namespace BrightVision.SSIS.Agent {

    #region This snippet is used for .Net Remoting TCP implementation
    /*
    partial class PackageService : ServiceBase, IObserver {
        public PackageService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            //the port on which server channel listens
            string port = ConfigurationManager.AppSettings["TCPChannelPort"].ToString();
            string logDir = ConfigurationManager.AppSettings["LogDirectory"].ToString();
            
            string logFileName = logDir + "ServiceAgentLog_" + DateTime.Now.ToString("MMMMddyyyyhhmmss") + ".xml";
            FileStream fs = new FileStream(logFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamLogger logStream = new StreamLogger(fs, true);
            logStream.IsIDGenerationEnabled = true;
            logStream.IsLineBreakingEnabled = true;
            logStream.IsPreambleEnabled = true;
            logStream.IsExceptionAutoFlushEnabled = true;
            logStream.IsContextEnabled = true;
            logStream.Open();
            try {
                try {                    
                    //************************************* TCP *************************************
                    // using TCP protocol                    
                    TcpChannel channel = new TcpChannel(Convert.ToInt32(port));
                    logStream.Log("TCP Channel port " + port + " has been established.");
                    ChannelServices.RegisterChannel(channel, false);
                    logStream.Log("TCP Channel at port " + port + " is registered.");
                    RemotingConfiguration.RegisterWellKnownServiceType(typeof(TaskCaller), "BrightVisionSSISRemoteCall", WellKnownObjectMode.Singleton);
                    logStream.Log("WellKnowService Type \"TaskCaller\" in mode Singleton  was registered.");
                    //************************************* TCP *************************************
                    //Attach our observer service to the remotable object
                    BrightVision.RemotableObject.Cache.Attach(this);

                } catch (Exception tex) {
                    logStream.Log("SSISRemotePackageExecution - TCP Bind Error");
                    logStream.Log(String.Format("Error in starting service: {0}", tex.Message + Environment.NewLine + tex.StackTrace));
                    if (tex.InnerException != null) {
                        logStream.Log("Inner Exception: " + tex.InnerException.Message.ToString());
                    }
                }            
            } catch (Exception ex) {
                logStream.Log(ex);
            } finally {
                logStream.Close();
            }
        }

        protected override void OnStop() {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }

        public void Notify(int _import_file_id, int _confidence, string _confidence_operator, int _similarity, string _similarity_operator, short _execution_type) {
            SSISPackage ssisPackage = new SSISPackage(_import_file_id, _confidence,_confidence_operator, _similarity, _similarity_operator);
            if (_execution_type == 1) { //execute account package
                Thread ssisPackageThread = new Thread(new ThreadStart(ssisPackage.RunDtsAccountPackage));
                ssisPackageThread.IsBackground = true;
                ssisPackageThread.Start();
            } else if (_execution_type == 2) { //execute contact package
                Thread ssisPackageThread = new Thread(new ThreadStart(ssisPackage.RunDtsContactPackage));
                ssisPackageThread.IsBackground = true;
                ssisPackageThread.Start();
            }                     
        }
    }
    */
    #endregion

    #region This snippet is used for Azure storage implementation
    partial class PackageService : ServiceBase {
       
        public PackageService() {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args) {
            string logDir = ConfigManager.AppSettings["LogDirectory"].ToString();

            string logFileName = logDir + "ServiceAgentLog_" + DateTime.Now.ToString("MMMMddyyyyhhmmss") + ".xml";
            FileStream fs = new FileStream(logFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamLogger logStream = new StreamLogger(fs, true);
            logStream.IsIDGenerationEnabled = true;
            logStream.IsLineBreakingEnabled = true;
            logStream.IsPreambleEnabled = true;
            logStream.IsExceptionAutoFlushEnabled = true;
            logStream.IsContextEnabled = true;
            logStream.Open();
            try {
                try {
                    logStream.Log("Service started successfully...");
                    //retry the action if it fails to start the thread every 30 seconds.
                    //suspected to throw exception during starting the service if the internet is not yet ready.
                    RetryAction(() => Start(), 1000 * 30);
                } catch (Exception tex) {
                    logStream.Log("SSISPackageExecution using Windows Azure Message Queue");
                    logStream.Log(String.Format("Error in starting service: {0}", tex.Message + Environment.NewLine + tex.StackTrace));
                    if (tex.InnerException != null) {
                        logStream.Log("Inner Exception: " + tex.InnerException.Message.ToString());
                    }
                }
            } catch (Exception ex) {
                logStream.Log(ex);
            } finally {
                logStream.Close();
            }
        }

        protected override void OnStop() {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }

        private void Start() {
            SSISPackage ssisPackage = new SSISPackage();
            Thread ssisPackageThread = new Thread(new ThreadStart(ssisPackage.ProcessSSISPackage));
            ssisPackageThread.IsBackground = true;
            ssisPackageThread.Start();   
        }

        private void RetryAction(Action action, int retryTimeout) {
            do {
                try {
                    action();
                    break;
                } catch {
                    System.Threading.Thread.Sleep(retryTimeout);
                }
            } while (true);
        }

    }
    #endregion

    public class SSISPackage {
        private SSISPackageQueue packageQueue;        
        private UserTextNotificationRepository packageRepository;


        public SSISPackage() {
            packageRepository = new UserTextNotificationRepository();
            packageQueue = new SSISPackageQueue();
        }

        public void ProcessSSISPackage() {

            int interval = int.Parse(ConfigManager.AppSettings["TimeInterval"]);
            while (true) {
                try {
                    Thread.Sleep(interval);         
                    RunDtsPackage();
                } catch {
                }
            }
        }

        private void RunDtsPackage() {
            bool isAccount = false;
            try {
                CloudQueueMessage cqm = packageQueue.GetMessage();

                while (cqm != null) {
                    SSISPackageMessage packageMessage = QueueMessageBase.FromMessage<SSISPackageMessage>(cqm);

                    isAccount = packageMessage.PackageType.ToLower() == "account";
                    MSDTS.Application app = new MSDTS.Application();
                    //
                    // Load package from file system
                    //
                    string packageLocation = System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;
                    string configLocation = System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;
                    MSDTS.Package package = null;
                    if (isAccount) {
                        if (packageMessage.Fuzzy_Match_Field == SSISPackageMessage.Fuzzy_Company_Name) {
                            package = app.LoadPackage(packageLocation + "\\SSIS Package\\account\\fuzzy_company_name.dtsx", null);
                            package.ImportConfigurationFile(configLocation + "\\SSIS Package\\account\\fuzzy_company_name.dtsConfig");
                        }
                        //TO DO: Add other fuzzy field name here for account fuzzy lookup match.
                    } else {
                        //TO DO: Add other fuzzy field name here for contact fuzzy lookup match.
                        //package = app.LoadPackage(packageLocation + "\\SSIS Package\\contact\\ProfileMatchContacts.dtsx", null);
                        //package.ImportConfigurationFile(configLocation + "\\SSIS Package\\contact\\dtProfileMatchContacts.dtsConfig");
                    }
                    var pkgVars = package.Variables;
                  
                    //SetMessageContacts("Reading parameters...", true);
                    package.VariableDispenser.LockOneForWrite("import_file_id", ref pkgVars);
                    pkgVars["import_file_id"].Value = packageMessage.ImportFileID;
                    pkgVars.Unlock();                    
                                     
                    package.VariableDispenser.LockOneForWrite("country", ref pkgVars);
                    pkgVars["country"].Value = packageMessage.Country;
                    pkgVars.Unlock();
                                     
                    package.VariableDispenser.LockOneForWrite("confidence", ref pkgVars);
                    pkgVars["confidence"].Value = packageMessage.Confidence;
                    pkgVars.Unlock();

                    package.VariableDispenser.LockOneForWrite("similarity", ref pkgVars);
                    pkgVars["similarity"].Value = packageMessage.Similarity;
                    pkgVars.Unlock();

                    package.VariableDispenser.LockOneForWrite("similarity_operator", ref pkgVars);
                    pkgVars["similarity_operator"].Value = packageMessage.SimilarityOperator;
                    pkgVars.Unlock();

                    package.VariableDispenser.LockOneForWrite("confidence_operator", ref pkgVars);
                    pkgVars["confidence_operator"].Value = packageMessage.ConfidenceOperator;
                    pkgVars.Unlock();

                    package.VariableDispenser.LockOneForWrite("validated", ref pkgVars);
                    pkgVars["validated"].Value = packageMessage.Validated;
                    pkgVars.Unlock();

                    MSDTS.DTSExecResult result = package.Execute();
                    
                    package.Dispose();

                    string title = string.Empty;
                    if (isAccount) {
                        if (packageMessage.Fuzzy_Match_Field == SSISPackageMessage.Fuzzy_Company_Name) {
                            title = "AccountNotification_FuzzyCompanyName";
                        }
                        //TO DO: add other fuzzy fields for account notification
                    } else {
                        //TO DO: add other fuzzy fields for contact notification
                    }
                    packageRepository.AddNotification(new UserTextNotification() {
                        Title = title,
                        MessageText = "Success",                        
                        MessageDate = DateTime.Now,
                        TargetUserName = packageMessage.UserID.ToString()
                    });

                    packageQueue.DeleteMessage(cqm);
                    cqm = packageQueue.GetMessage();
                }

               
            } catch {

            }
        }
    }
}
