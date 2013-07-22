using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SalesConsultant.Forms;
using System.Diagnostics;
using SalesConsultant.Business;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using System.Globalization;

namespace SalesConsultant {
    static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {

            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.PriorityClass = ProcessPriorityClass.RealTime;
            //System.Threading.Thread.CurrentThread.CurrentCulture =
            //    new System.Globalization.CultureInfo("sv-SE");

            //System.Threading.Thread.CurrentThread.CurrentUICulture =
            //    new System.Globalization.CultureInfo("sv-SE");



            //AppDomain currentDomain = default(AppDomain);
            //currentDomain = AppDomain.CurrentDomain;
            //// Handler for unhandled exceptions.
            //currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            //currentDomain.FirstChanceException += currentDomain_FirstChanceException;
            //currentDomain.ProcessExit += currentDomain_ProcessExit;
            //// Handler for exceptions in threads behind forms.
            //System.Windows.Forms.Application.ThreadException += GlobalThreadExceptionHandler;




            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new FrmDemo());

            // uncomment to apply skin
            //DevExpress.UserSkins.BonusSkins.Register();
            //DevExpress.UserSkins.OfficeSkins.Register();
            //DevExpress.Skins.SkinManager.EnableFormSkins();
            //Application.Run(new Forms.FrmCampaignBooking(null,null));

            //check for updates
            //System.Timers.Timer aTimer = new System.Timers.Timer();
            //aTimer.Elapsed += new System.Timers.ElapsedEventHandler(aTimer_Elapsed);
            //aTimer.Interval = 5000; // 300000; //5 minutes
            //----------
            try
            {
                BrightSalesFacade.Logger.SoftwareVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                BrightSalesFacade.Logger.SendInfo("startup_bs", "starting brightsales");
                FrmAppUpdater updater = new FrmAppUpdater();
                if (updater.HasNewUpdate)
                {
                    //aTimer.Enabled = false;
                    updater.ShowDialog();
                }
                else
                {
                    //aTimer.Enabled = true;
                    var controller = new FrmSalesConsultant();
                    EventQueue.Instance.SetController(controller);
                    Application.Run(controller);
                }
                BrightSalesFacade.Logger.SendInfo("close_bs", "starting brightsales");
            }
            catch (Exception ex)
            {
                //BrightSalesFacade.Logger.Error("app_exception", "", ex);
                (new BrightVision.Common.Utilities.RaygunClientLogger()).Send(ex);
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            //------------
        }

        static void currentDomain_ProcessExit(object sender, EventArgs e)
        {
            //MessageBox.Show(e.ToString());
        }

        static void currentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message + "\n" + e.Exception.StackTrace);
            //(new BrightVision.Common.Utilities.RaygunClientLogger()).Send(e.Exception);
        }

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = default(Exception);
            ex = (Exception)e.ExceptionObject;
            MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
        }

        private static void GlobalThreadExceptionHandler(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = default(Exception);
            ex = e.Exception;
            MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
        }

        //static void aTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
        //    var t = sender as System.Timers.Timer;
        //    var updater = new FrmAppUpdater();
        //    if (updater.HasNewUpdate) {
        //        t.Enabled = false;
        //        updater.ShowDialog();
        //    }
        //}
    }
    //Singleton Object to Process all event to queue.
    public sealed class EventQueue {
        private static volatile EventQueue instance;
        private static readonly object syncRoot = new object();
        
        private EventQueue() { }

        public static EventQueue Instance {
            get {
                if (instance == null) {
                    lock (syncRoot) {
                        if(instance == null)
                        instance = new EventQueue();
                    }
                }
                return instance;
            }
        }

        public void SetController(object objForm) {
            Controller = (FrmSalesConsultant) objForm;
        }

        private FrmSalesConsultant Controller { get; set; }

        public void AddWorkToQueue(BrightVision.EventLog.EventMessage eventMessage) {
            if (Controller != null) {
                Controller.AddWorkToQueue(eventMessage);
            }
        }
    }
}
