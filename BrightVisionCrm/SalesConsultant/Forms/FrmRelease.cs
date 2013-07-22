using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System.Linq;

using BrightVision.Model;
using BrightVision.Common.Business;

using BrightVision.Reporting;
using BrightVision.Reporting.UI;
using BrightVision.Common.UI;
using BrightVision.Common.Utilities;
using SalesConsultant.Modules;
using SalesConsultant.PublicProperties;
using SalesConsultant.Facade;
using BrightVision.Common.Events.Core;
using SalesConsultant.Events;

namespace SalesConsultant.Forms
{
    public partial class FrmRelease : DevExpress.XtraEditors.XtraForm
    {
        #region Constructors
        public FrmRelease()
        {
            WaitDialog.Show("Loading send email ...");
            InitializeComponent();

            modRelease = new Release();
            modReleaseSendMail = Modules.ReleaseSendMail.Instance();
            
            InitializeEvents();
            SetComboBoxReports();
            WaitDialog.Close();
        }
        #endregion

        #region Public Properties
        public class ComboboxItem {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() {
                return Text;
            }
        }
        //public bool p_IsCancelClick = false;
        #endregion

        #region Private Properties
        private class ViewCofigData {
            public int id { get; set; }
            public int subcampaign_id { get; set; }
            public string name { get; set; }
        }
        private Modules.ViewDisplay view;
        //private static int p_SubCampaignId;
        //private static string p_CompanyName = null;
        private static List<int> p_subcampaign_ids = new List<int>();
        private static Release modRelease = null;
        private static ReleaseSendMail modReleaseSendMail = null;
        private BrightSalesProperty m_BrightSalesProperty = BrightSalesFacade.Property;
        private IEventAggregator m_EventBus = BrightSalesFacade.EventBus;
        #endregion

        #region Public Methods
        public void SetComboBoxReports()
        {
            p_subcampaign_ids.Add(m_BrightSalesProperty.CommonProperty.SubCampaignId);
            List<ViewCofigData> m_oListViewConfig = this.GetViewConfigInfo(p_subcampaign_ids.ToArray());
            string subcampaign_title = string.Empty;
            for (int x = 0; x < m_oListViewConfig.Count; ++x)
                cboReports.Properties.Items.Add(new ComboboxItem() { Text = m_oListViewConfig[x].name, Value = m_oListViewConfig[x].id });
        }
        #endregion

        #region Private Methods
        private List<ViewCofigData> GetViewConfigInfo(int[] subcampaign_ids)
        {
            List<ViewCofigData> listViewConfig = null;
            using (BrightPlatformEntities context = new BrightPlatformEntities(UserSession.EntityConnection)) {
                if (subcampaign_ids.Length <= 0) return null;
                    listViewConfig = context.view_configuration.Where(x => 
                        subcampaign_ids.Contains(x.subcampaign_id) && 
                        x.MGC == false && x.report_layout_config != null
                    ).Select(x => 
                        new ViewCofigData { id = x.id, name = x.name }
                    ).ToList();
            }
            return listViewConfig;
        }
        private void Print()
        {
            view = new Modules.ViewDisplay();
            //view.PrintPreview((cboReports.SelectedItem as ComboboxItem).Value);
            view.EventComponentsLoaded += new EventHandler(this.EVENT_COMPONENTS_LOADED);
            view.LoadViews(p_subcampaign_ids.ToArray(), (cboReports.SelectedItem as ComboboxItem).Value);
            view.PopulateOtherTabDatasources();
        }
        private void CreatePDF()
        {
            view = new Modules.ViewDisplay();
            view.EventComponentsLoaded += new EventHandler(this.EVENT_COMPONENTS_LOADED_PDF);
            view.LoadViews(p_subcampaign_ids.ToArray(), (cboReports.SelectedItem as ComboboxItem).Value);
            view.PopulateOtherTabDatasources();
        }
        #endregion

        #region Instance of the form.
        //private static FrmRelease instance = null;
        //public static FrmRelease Instance(string CompanyName, int SubCampaignId)
        //{
        //    //if (instance == null || p_CompanyName != CompanyName) {
        //        p_CompanyName = CompanyName;
        //        p_SubCampaignId = SubCampaignId;
        //        p_subcampaign_ids.Clear();
        //        p_subcampaign_ids.Add(p_SubCampaignId);
        //        modRelease = new Release();
        //        modReleaseSendMail = Modules.ReleaseSendMail.Instance();
        //        //modReleaseSendMail = Modules.ReleaseSendMail.Instance(p_SubCampaignId);
        //        instance = new FrmRelease();
        //    //}

        //    return instance;
        //}
        #endregion

        #region InitializeEvents
        private void InitializeEvents()
        {
            this.FormClosed += new FormClosedEventHandler(this.EVENT_FORM_CLOSED);
            this.FormClosing += new FormClosingEventHandler(this.EVENT_FORM_CLOSING);
            this.Load += new EventHandler(this.EVENT_FORM_LOAD);
            this.Resize += new EventHandler(this.EVENT_RESIZE);

            btnCancel.Click += new EventHandler(this.EVENT_CLICK_CANCEL);
            btnPreviewReport.Click += new EventHandler(this.EVENT_PRINT_PREVIEW);
            btnRelease.Click += new EventHandler(this.EVENT_CLICK_RELEASE);
        }
        #endregion

        #region InitializeReleaseModule
        private void InitializeReleaseModule()
        {
            modRelease.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);

            int iWidth = modRelease.Size.Width;
            int iHeight = modRelease.Size.Height;

            this.Size = new Size(iWidth + 43 , iHeight + 90);
            panContainer.Controls.Clear();
            panContainer.Controls.Add(modRelease);

            btnRelease.Text = "&Release";
            this.Text = "Release";
        }
        #endregion

        #region Control Events
        private void EVENT_COMPONENTS_LOADED(object sender, EventArgs e)
        {
            view.PrintPreview(-1);
        }
        private void EVENT_FORM_LOAD(object sender, EventArgs e)
        {
            InitializeReleaseModule();
            //WaitDialog.Close();
        }
        private void EVENT_FORM_CLOSING(object sender, FormClosingEventArgs e)
        {
            //if (e.CloseReason == CloseReason.UserClosing)
            //{
            //    p_IsCancelClick = true;
            //}

            if (btnRelease.Text == "Send")
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }        
        private void EVENT_FORM_CLOSED(object ender, FormClosedEventArgs e)
        {
        }
        private void EVENT_CLICK_CANCEL(object sender, EventArgs e)
        {
            if (btnRelease.Text == "Send") {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }
        private void EVENT_RESIZE(object sender, EventArgs e)
        {
            //panContainer.Controls[0].Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom);
            //panContainer.Controls[0].Refresh();
        }
        private void EVENT_PRINT_PREVIEW(object sender, EventArgs e)
        {
            if (cboReports.SelectedIndex < 0)
            {
                NotificationDialog.Information("Please select reports to preview.", "Print");
                return;
            }

            WaitDialog.Show("Loading print preview ...");
            this.Print();
            WaitDialog.Close();
        }
        private void EVENT_CLICK_RELEASE(object sender, EventArgs e)
        {
            if (btnRelease.Text == "Send") {
                if (modReleaseSendMail.CountRecipients() < 1)
                    BrightVision.Common.UI.NotificationDialog.Information("Bright Sales", "No emails selected. Will proceed without sending email.");

                else {
                    WaitDialog.Show("Creating attachment ...");
                    if (cboReports.SelectedIndex >= 0)
                        CreatePDF();

                    else {
                        modReleaseSendMail.ProcessSending();
                        this.DialogResult = System.Windows.Forms.DialogResult.OK;
                        //this.FormClosing -= new FormClosingEventHandler(this.EVENT_FORM_CLOSING);
                        //p_IsCancelClick = false;
                        //instance = null;
                        this.Close();
                    }
                    WaitDialog.Close();
                }
            }

            // release button
            else {
                WaitDialog.Show("Saving data and loading ...");
                m_EventBus.Notify(new FrmReleaseSendEmailSaveWorkEventNotifier());
                WaitDialog.Close();

                WaitDialog.Show("Saving data ...");
                modReleaseSendMail.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
                int iWidth = modReleaseSendMail.Size.Width;
                int iHeight = modReleaseSendMail.Size.Height;
                this.Size = new Size(iWidth + 43, iHeight + 90);
                panContainer.Controls.Clear();
                panContainer.Controls.Add(modReleaseSendMail);

                btnRelease.Text = "Send";
                this.Text = "New mail to Customer";
                WaitDialog.Close();
            }
        }
        private void EVENT_COMPONENTS_LOADED_PDF(object sender, EventArgs e)
        {
            string filename = view.CreatePDF();
            modReleaseSendMail.ProcessSending(filename);

            //modReleaseSendMail.ProcessSending(filename);
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            //this.FormClosing -= new FormClosingEventHandler(this.EVENT_FORM_CLOSING);
            //p_IsCancelClick = false;
            //instance = null;
            this.Close();

            //WaitDialog.Close();
        }
        #endregion
    }
}