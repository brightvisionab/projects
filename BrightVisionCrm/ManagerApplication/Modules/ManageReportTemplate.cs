
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Xml;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UserDesigner;

using BrightVision.Model;
using BrightVision.Common.Business;
using BrightVision.Reporting.UI;

namespace ManagerApplication.Modules
{
    public partial class ManageReportTemplate : DevExpress.XtraEditors.XtraUserControl
    {
        #region Variable
        private BrightPlatformEntities BPContext = null;
        ReportUserDesigner reportDesign;
        #endregion

        #region Constructors
        public ManageReportTemplate(int viewid)
        {
            InitializeComponent();
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            reportDesign = new ReportUserDesigner();
            //BindSubcampaignLookup();
        }
        #endregion

        #region Public Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Properties
        private bool HasSelectedCampaign {
            get { 
                //var editval = cboSubCampaign.EditValue;
                //if (editval != null) {
                //    return true;
                //}
                return false;
            }
        }
        private bool HasSelectedTemplate{
            get
            {
                //var editval = gvReportTemplate.GetFocusedRow();
                //if (editval != null)
                //    return true;
                return false;

            }

        }
        #endregion

        #region Private Methods

        private void BindSubcampaignLookup()
        {
            List<CTCustomerCampaignSubcampaign> listCCS = BPContext.FIGetCustomerCampaignSubcampaign(UserSession.CurrentUser.UserId, 2).ToList();
            if (listCCS != null && listCCS.Count > 0)
            {
                //cboSubCampaign.Properties.Columns.Clear();
                //cboSubCampaign.Properties.DataSource = listCCS;
                //cboSubCampaign.Properties.DisplayMember = "title";
                //cboSubCampaign.Properties.ValueMember = "subcamapaign_id";
                //cboSubCampaign.Properties.Columns.Add(new LookUpColumnInfo("title"));
            }
        }


        private void BindTemplateList()
        {
             //var editval = cboSubCampaign.EditValue;
             //if (editval != null) {
             //    int campaignId = (int)editval;
             //    //var result = BPContext.FIGetExportViewReportTemplates(campaignId);
             //    gcReportTemplate.DataSource = result;
             //}
        }

        #endregion

      
        private void btnCreateTemplate_Click(object sender, EventArgs e)
        {
            if (HasSelectedCampaign)
            {
                reportDesign = new ReportUserDesigner();
                reportDesign.SaveReport += new SaveReportEventHandler(reportDesign_SaveReport);
                reportDesign.ChangeReportName += new ChangeNameEventHandler(reportDesign_ChangeReportName);
                reportDesign.ShowDialog();
            }
        }

        private void btnEditTemplate_Click(object sender, EventArgs e)
        {
            if (HasSelectedTemplate) {
                //var obj = (CTExportViewReportTemplate)gvReportTemplate.GetFocusedRow();
                //XtraReportDefault xreport = ReportUserDesigner.StringToXtraReportDefault(obj.layout_config);
                //reportDesign = new ReportUserDesigner(xreport, true);
                //reportDesign.SaveReport+=new SaveReportEventHandler(reportDesign_UpdateReport);
                //reportDesign.ShowDialog();
            }
        }

        private void reportDesign_UpdateReport(object sender, SaveReportEventArgs e)
        {
            //var obj = (CTExportViewReportTemplate)gvReportTemplate.GetFocusedRow();
            //var b =  BPContext.export_view_report_templates.Where(param => param.id == obj.id);
            //if (b.Count() > 0) {
            //    export_view_report_templates templates = b.First();
            //    templates.layout_config = e.ReportTemplate;
            //    templates.modified_by = UserSession.CurrentUser.UserId;
            //    templates.modified_on = DateTime.Now;
            //    BPContext.SaveChanges();
            //}
            //reportDesign.DialogResult = DialogResult.Cancel;
            //BindTemplateList();
        }
        private void  reportDesign_ChangeReportName(object sender, ref ReportNameEventArgs e)
        {
           //string name = e.Name;
           //int subcampaign_id = (int)cboSubCampaign.EditValue;
           // var result = from template in BPContext.export_view_report_templates
           //              where template.sub_campaign_id == subcampaign_id && template.name == name
           //              select template;
           // if (result.Count() > 0)
           //     e.Exist = true;
           // else
           //     e.Exist = false;
        }

        private void reportDesign_SaveReport(object sender, SaveReportEventArgs e)
        {
            //int editval = (int)cboSubCampaign.EditValue;
            //var templates = new export_view_report_templates();
            //templates.sub_campaign_id = editval;
            //templates.layout_config = e.XmlTemplate;
            //templates.name = e.Name;
            //templates.modified_on = DateTime.Now;
            //templates.created_on = DateTime.Now;
            //templates.created_by = UserSession.CurrentUser.UserId;
            //BPContext.AddToexport_view_report_templates(templates);
            //BPContext.SaveChanges();
            //reportDesign.DialogResult = DialogResult.Cancel; 
            //BindTemplateList();
            //MessageBox.Show(this, "Create Template Complete", "Create Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

     




  
    }
}
