using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraLayout;

using BrightVision.DQControl.UI;
using BrightVision.DQControl.Business;
using BrightVision.Model;
using BrightVision.Common.Business;

using QuestionnaireEditor.Business;

namespace QuestionnaireEditor.Forms {    
    public partial class FrmMain : DevExpress.XtraEditors.XtraForm {
        public FrmMain() {
            InitializeComponent();
        }

        private BaseLayoutItem lastitem = null;
        private BrightPlatformEntities BPContext = null;

        private void FrmMain_Load(object sender, EventArgs e) {
            //dummy values for question_id
            //int dropboxId = 1, textboxId = 2, multipleChoiceId = 3;
            BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            //var resourceList = BPContext.FIGetResources(3).ToList();
            //List<int?> resourceids = new List<int?>();
            //resourceList.ForEach(delegate(CTResources r) {
            //    resourceids.Add(r.resource_id);
            //});
            //var schedList = BPContext.schedules.Where(x => resourceids.Contains(x.resource_id)).ToList();

            this.layoutControlGroupQuestionnaire.BeginUpdate();

            ////add dropbox
            //Dropbox oDropbox = new Dropbox(this.layoutControlQuestionnaire);
            //oDropbox.ToolTipController = defaultToolTipController1;
            //oDropbox.Questionnaire = CampaignQuestionnaire.Instanciate(QuestionTypeConstants.Dropbox);
            ////oDropbox.Questionnaire = Business.BusinessAnswer.BindAnswer(oDropbox.Questionnaire, BPContext, dropboxId);
            //oDropbox.BindControls();
            //this.layoutControlGroupQuestionnaire.Add(oDropbox.ControlGroup);

            ////add textbox            
            //Textbox oTextbox = new Textbox(this.layoutControlQuestionnaire);
            //oTextbox.Questionnaire = CampaignQuestionnaire.Instanciate(QuestionTypeConstants.Textbox);
            ////oTextbox.Questionnaire = Business.BusinessAnswer.BindAnswer(oTextbox.Questionnaire, BPContext, textboxId);
            //oTextbox.BindControls();
            //this.layoutControlGroupQuestionnaire.Add(oTextbox.ControlGroup);

            ////add multiplechoice           
            Multiplechoice oMultipleChoice = new Multiplechoice(this.layoutControlQuestionnaire);
            oMultipleChoice.Questionnaire = CampaignQuestionnaire.Instanciate(QuestionTypeConstants.MultipleChoice);
            //oMultipleChoice.Questionnaire = Business.BusinessAnswer.BindAnswer(oMultipleChoice.Questionnaire, BPContext, multipleChoiceId);
            oMultipleChoice.BindControls();
            this.layoutControlGroupQuestionnaire.Add(oMultipleChoice.ControlGroup);

            //add schedule
            Schedule oSchedule = new Schedule(this.layoutControlQuestionnaire);
            oSchedule.Questionnaire = CampaignQuestionnaire.Instanciate(QuestionTypeConstants.Schedule);
            oSchedule.Questionnaire.Form.Settings.DataBindings.account_id = "1";
            //contact callerContact = BPContext.contacts.FirstOrDefault(x=>x.id == 35758);
            //if (callerContact != null)
            //    oSchedule.SetCurrentCaller(callerContact, 1);
            //oSchedule.ShowCalendarBookingClick += new EventHandler(oSchedule_ShowCalendarBookingClick);
            //oSchedule.Questionnaire = Business.BusinessAnswer.BindAnswer(oSchedule.Questionnaire, BPContext, multipleChoiceId); 
            oSchedule.BindControls();
            this.layoutControlGroupQuestionnaire.Add(oSchedule.ControlGroup);

            //add seminar
            //Seminar oSeminar = new Seminar(this.layoutControlQuestionnaire);
            //oSeminar.Questionnaire = CampaignQuestionnaire.Instanciate(QuestionTypeConstants.Seminar);
            //oSeminar.Questionnaire.Form.Settings.DataBindings.account_id = "1";
            ////oSeminar.Questionnaire = Business.BusinessAnswer.BindAnswer(oSchedule.Questionnaire, BPContext, multipleChoiceId);
            //oSeminar.BindControls();
            //this.layoutControlGroupQuestionnaire.Add(oSeminar.ControlGroup);

            //add bottom spacer
            this.layoutControlGroupQuestionnaire.AddItem(emptySpaceItem1);            
            this.layoutControlGroupQuestionnaire.EndUpdate();  
        }

     

        private void simpleButtonEdit_Click(object sender, EventArgs e) {
            BrightVision.DQControl.Business.IQuestionnaire iQuestion;
            var objGroupItems = this.layoutControlGroupQuestionnaire.Items;            
            foreach (BaseLayoutItem item in objGroupItems) {
                if (item.IsGroup) {
                    if (item.Tag != null) {
                        iQuestion = item.Tag as BrightVision.DQControl.Business.IQuestionnaire;
                        if (iQuestion.ControlGroup.BackgroundImageVisible
                            && iQuestion.Focused) {
                                lastitem = item;
                                memoEditJSON.Text = iQuestion.Questionnaire.ToJSONString(true);
                            break;
                        }
                    }
                }
            }
        }

        private void simpleButtonPreview_Click(object sender, EventArgs e) {
            try {
                if (lastitem != null) {
                    BrightVision.DQControl.Business.IQuestionnaire iQuestion =
                    lastitem.Tag as BrightVision.DQControl.Business.IQuestionnaire;
                    iQuestion.JSONString = memoEditJSON.Text;
                    iQuestion.BindQuestionnaire();
                    iQuestion.BindControls();
                }

                //var objGroupItems = this.layoutControlGroupQuestionnaire.Items;
                //foreach (BaseLayoutItem item in objGroupItems) {
                //    if (item.IsGroup) {
                //        if (item.Tag != null && lastitem != null && item.Name == lastitem.Name) {
                //            iQuestion = item.Tag as BrightVision.DQControl.Business.IQuestionnaire;
                //            if (iQuestion.ControlGroup.BackgroundImageVisible
                //            && iQuestion.Focused) {
                //                iQuestion.JSONString = memoEditJSON.Text;
                //                iQuestion.BindControls();
                //                break;
                //            }
                //        }
                //    }
                //}
            } catch (Exception ex) {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e) {
            BrightVision.DQControl.Business.IQuestionnaire iQuestion;
            //dummy values for question_id
            //int dropboxId = 1, textboxId = 2, multipleChoiceId = 3, scheduleId = 4, seminarId = 5;
            CampaignQuestionnaire oQues;
            
            var objGroupItems = this.layoutControlGroupQuestionnaire.Items;
            foreach (BaseLayoutItem item in objGroupItems) {
                if (item.IsGroup) {
                    if (item.Tag != null) {
                        iQuestion = item.Tag as IQuestionnaire;                        
                        oQues = iQuestion.Questionnaire;
                        MessageBox.Show(oQues.ToJSONString(true));
                        if (oQues.Type.ToLower() == QuestionTypeConstants.Textbox) {                            
                            //Business.BusinessAnswer.SaveAnswer(oQues, BPContext, textboxId);
                        } else if (oQues.Type.ToLower() == QuestionTypeConstants.MultipleChoice) {
                            //Business.BusinessAnswer.SaveAnswer(oQues, BPContext, multipleChoiceId);
                        } else if (oQues.Type.ToLower() == QuestionTypeConstants.Dropbox) {
                            //Business.BusinessAnswer.SaveAnswer(oQues, BPContext, dropboxId);
                        } else if (oQues.Type.ToLower() == QuestionTypeConstants.Schedule) {
                            //BusinessAnswer.SaveAnswer(oQues, BPContext, 5);
                        } 
                        //else if (oQues.Type.ToLower() == QuestionTypeConstants.Seminar) {
                        //    //BusinessAnswer.SaveAnswer(oQues, BPContext, 6);
                        //}
                    }
                }
            }
        }

    }
}
