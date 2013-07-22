using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.Utils;


namespace ManagerApplication.Business {
    //[@jeff 07.07.2011]: trasnferred to common WaitDialog.cs
    //public static class WaitDialog {
    //    private static WaitDialogForm diag = null;
    //    private static Cursor currentCursor;
    //    public static void CreateWaitDialog(string Caption = "", string Title = "") {
    //        if(Title == "")
    //            diag = new WaitDialogForm(Caption);
    //        else 
    //            diag = new WaitDialogForm(Caption, Title);
    //        currentCursor = Cursor.Current;
    //        Cursor.Current = Cursors.WaitCursor;
    //    }
    //    public static void CloseWaitDialog() {
    //        if(diag != null) diag.Close();
    //        Cursor.Current = currentCursor;
    //    }
    //    public static void SetWaitDialogCaption(string fCaption) {
    //        if(diag != null)
    //            diag.Caption = fCaption;
    //    }
    //}

    public class ProgressBackgroundWork {
        private System.ComponentModel.BackgroundWorker Worker = null;
        private bool workDone = false;

        public ProgressBackgroundWork(BarEditItem progresBarItem, BarStaticItem statusNotifier) {
            ProgressBarItem = progresBarItem;
            StatusNotifier = statusNotifier;
            Worker = new System.ComponentModel.BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.DoWork += new System.ComponentModel.DoWorkEventHandler(Worker_DoWork);
            Worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(Worker_ProgressChanged);
        }

        public BarEditItem ProgressBarItem { get; set; }

        public BarStaticItem StatusNotifier { get; set; }

        private void Worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e) {
            if (e.ProgressPercentage >= 100) {
                if (ProgressBarItem != null && StatusNotifier != null) {
                    ProgressBarItem.Visibility = BarItemVisibility.Never;
                    StatusNotifier.Caption = string.Empty;
                }
            }
        }
        
        private void Worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {          
            if (ProgressBarItem != null && StatusNotifier != null) {         
                ProgressBarItem.Visibility = BarItemVisibility.Always;
                StatusNotifier.Caption = "Loading " + e.Argument.ToString().ToLower() + "...";                
                while (true) {
                    if (!InDoneState()) {
                        System.Threading.Thread.Sleep(1000);
                    } else {
                        Worker.ReportProgress(100);
                        break;
                    }
                }                
            }
        }

        private bool InDoneState() {
            return workDone;
        }

        public void StartWork(string strMessage) {            
            workDone = false;
            Worker.RunWorkerAsync(strMessage);
        }
        public void StopWork() {
            workDone = true;            
        }
        
   }   
}
