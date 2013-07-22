
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BrightVision.Model;
using BrightVision.Common.Business;
using System.Linq;
using DevExpress.Data.Linq;
using DevExpress.Data.PLinq;
using DevExpress.Data.Filtering;
using System.Threading;

namespace ManagerApplication.Forms
{
    public partial class DemoWindow : DevExpress.XtraEditors.XtraForm
    {
        public DemoWindow()
        {
            InitializeComponent();
        }

        //FrmProgressBar _frm;
        private void btnShowProgressBar_Click(object sender, EventArgs e)
        {
            //_frm = new FrmProgressBar() {
            //    CurrentPosition = 0,
            //    ProcessTotal = 10000
            //};

            //BackgroundWorker _bwDisplay = new BackgroundWorker()
            //{
            //    WorkerSupportsCancellation = true
            //};
            //_bwDisplay.DoWork += _bwDisplay_DoWork;
            //_bwDisplay.RunWorkerAsync();
            //System.Threading.Thread.Sleep(1000);
            //Thread _worker = new Thread( new ThreadStart(() => {
            //    _frm.Show();
            //    for (int i = 0; i <= _frm.ProcessTotal; i++)
            //        _frm.CurrentPosition = i;
            //}));
            //Thread _worker = new Thread(new ThreadStart(Test));
            //_worker.Start();
            
            //BackgroundWorker _bw = new BackgroundWorker() {
            //    WorkerSupportsCancellation = true
            //};
            //_bw.DoWork += _bw_DoWork;
            //_bw.RunWorkerAsync();
            

            //pbProgressTotal.Position = 0;
            //pbProgressTotal.Properties.Maximum = 100;
            //for (int i = 0; i <= pbProgressTotal.Properties.Maximum; i += 5)
            //{
            //    pbProgressTotal.Position = i;
            //    pbProgressTotal.Refresh();
            //    System.Threading.Thread.Sleep(500);
            //}

            //BackgroundWorker _bw = new BackgroundWorker() {
            //    WorkerSupportsCancellation = true
            //};
            //_bw.DoWork += _bw_DoWork;
            //_bw.RunWorkerAsync();
        }

        //void Test()
        //{
        //    _frm.Show();
        //    for (int i = 0; i <= _frm.ProcessTotal; i++)
        //        _frm.CurrentPosition = i;
        //}

        //void _bwDisplay_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    this.Invoke(new MethodInvoker(delegate { _frm.Show(); }));
        //}

        //void _bw_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    this.Invoke(new MethodInvoker(delegate {

                
        //        for (int i = 0; i <= _frm.ProcessTotal; i++)
        //        {
        //            _frm.CurrentPosition = i;
        //            //System.Threading.Thread.Sleep(500);
        //        }
        //        _frm.Close();
        //        //pbProgressTotal.Position = 0;
        //        //pbProgressTotal.Properties.Maximum = 100;
        //        //for (int i = 0; i <= pbProgressTotal.Properties.Maximum; i += 5) {
        //        //    pbProgressTotal.Position = i;
        //        //    pbProgressTotal.Refresh();
        //        //    System.Threading.Thread.Sleep(500);
        //        //}
        //    }));
        //}

        
    }
}